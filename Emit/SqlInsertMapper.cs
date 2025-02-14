using RC.DBA.Collections;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace RC.DBA.Emit
{
    public class SqlInsertMapper<T>
    {
        private static int _dynamicMethodNumber;
        private HashMap<string, string> columnMapping = new HashMap<string, string>();
        private HashSetList<string> columnNotMapped = new HashSetList<string>();

        private string tableName = typeof(T).Name;

        private string compiledSql;
        private Func<T, IList<Parameter>> parametersFactory;


        public SqlInsertMapper<T> SetTable(string table, string schema)
        {
            tableName = schema != null ? $"{schema}.{table}" : table;

            return this;
        }

        public SqlInsertMapper<T> SetColumn<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property, string column)
        {
            var memberInfo = Helper.Member(property);
            columnMapping.Add(memberInfo.Name, column);

            return this;
        }

        public SqlInsertMapper<T> SetNotMapped<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property)
        {
            var memberInfo = Helper.Member(property);
            columnNotMapped.Add(memberInfo.Name);

            return this;
        }

        public bool Skip(string propName) => columnNotMapped.Contains(propName);

        public string GetColumName(string propName)
        {
            int index = columnMapping.IndexOf(propName);
            if (index < 0) return propName;

            return columnMapping.GetValueByIndex(index);
        }

        public string TableName() => tableName;

        public (string sql, Func<T, IList<Parameter>> factory) Build()
        {
            if (compiledSql != null && parametersFactory != null) 
                return (sql: compiledSql, factory: parametersFactory);

            var sql = new StringBuilder();
            var type = typeof(T);

            var dynamicMethodNumber = System.Threading.Interlocked.Increment(ref _dynamicMethodNumber);
            var method = new DynamicMethod("_$SqlInsertMapper_" + typeof(T).Name + dynamicMethodNumber.ToString(),
                typeof(IList<Parameter>), new[] { type }, true);

            var parameterCreateMethodInfoGeneric = typeof(Parameter).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m =>
                {
                    if (m.Name != "Create") return false;
                    var methodParameters = m.GetParameters();
                    if (methodParameters.Length != 2) return false;
                    if (methodParameters[0].ParameterType != typeof(string)) return false;
                    if (!methodParameters[1].ParameterType.IsGenericParameter) return false;

                    return true;
                })
                .Single();

            var resizableArrayAddMethodInfo = typeof(ResizableArray<Parameter>).GetMethod("Add",
                BindingFlags.Instance | BindingFlags.Public);

            var gen = method.GetILGenerator();

            EmitHelper.EmitNewWithDefaultCtor(gen, typeof(ResizableArray<Parameter>)); //[] => [ResizableArray<Parameter> parameters]
            

            sql.Append("insert into ").Append(TableName()).Append('(');

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => !Helper.IsAssociationPropertyType(p.PropertyType) && !Skip(p.Name)).
                ToList();

            foreach (var prop in properties)
            {
                //if (Skip(prop.Name)) continue;

                sql.Append(GetColumName(prop.Name)).Append(',');
                

                var propGetter = prop.GetGetMethod();
                var parameterCreateMethodInfo = parameterCreateMethodInfoGeneric.MakeGenericMethod(prop.PropertyType);

                gen.Emit(OpCodes.Dup); // [parameters] => [parameters, parameters]
                gen.Emit(OpCodes.Ldstr, prop.Name); // [parameters, parameters] => [parameters, parameters, paramName]
                gen.Emit(OpCodes.Ldarg_0); // [parameters, parameters, paramName] => [parameters, parameters, paramName, T item]
                gen.Emit(OpCodes.Callvirt, propGetter); // [parameters, parameters, paramName, item] => [parameters, parameters, paramName, value]
                gen.Emit(OpCodes.Call, parameterCreateMethodInfo); // [parameters, parameters, paramName, value] => [parameters, parameters, Parameter<T> parameter]
                gen.Emit(OpCodes.Callvirt, resizableArrayAddMethodInfo); // [parameters, parameters, parameter] => [parameters]
            }
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(f => !Helper.IsAssociationPropertyType(f.FieldType) && !Skip(f.Name))
                .ToList();

            foreach (var field in fields)
            {
                //if (Skip(field.Name)) continue;

                sql.Append(GetColumName(field.Name)).Append(',');

                var parameterCreateMethodInfo = parameterCreateMethodInfoGeneric.MakeGenericMethod(field.FieldType);

                gen.Emit(OpCodes.Dup); // [parameters] => [parameters, parameters]
                gen.Emit(OpCodes.Ldstr, field.Name); // [parameters, parameters] => [parameters, parameters, paramName]
                gen.Emit(OpCodes.Ldarg_0); // [parameters, parameters, paramName] => [parameters, parameters, paramName, T item]
                gen.Emit(OpCodes.Ldfld, field);  // [parameters, parameters, paramName, item] => [parameters, parameters, paramName, value]
                gen.Emit(OpCodes.Call, parameterCreateMethodInfo); // [parameters, parameters, paramName, value] => [parameters, parameters, Parameter<T> parameter]
                gen.Emit(OpCodes.Callvirt, resizableArrayAddMethodInfo); // [parameters, parameters, parameter] => [parameters]
            }

            sql.Remove(sql.Length - 1, 1).Append(") values (");

            gen.Emit(OpCodes.Ret); // [parameters] => []

            parametersFactory = method.CreateDelegate(typeof(Func<T, IList<Parameter>>)) as Func<T, IList<Parameter>>;

            var parameters = new List<Parameter>();
            foreach (var prop in properties)
            {
                //if (Skip(prop.Name)) continue;

                sql.Append('@').Append(prop.Name).Append(',');
            }
            foreach (var field in fields)
            {
                //if (Skip(field.Name)) continue;

                sql.Append('@').Append(field.Name).Append(',');
            }
            sql.Remove(sql.Length - 1, 1).Append(')');

            compiledSql = sql.ToString();

            return (sql: compiledSql, factory: parametersFactory);
        }

    }
}
