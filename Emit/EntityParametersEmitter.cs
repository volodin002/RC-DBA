using RC.DBA.Metamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace RC.DBA.Emit
{
    class EntityParametersEmitter
    {
        private static int _DynamicMethodNumber;

        public static Func<T, Parameter[]> EmitUpdateFactory<T>(IModelManager modelManager)
        {
            return EmitFactory<T>(modelManager, false);
        }

        public static Func<T, Parameter[]> EmitInsertFactory<T>(IModelManager modelManager)
        {
            return EmitFactory<T>(modelManager, true);
        }

        public static Func<T, Parameter[]> EmitDeleteFactory<T>(IModelManager modelManager)
        {
            var entity = modelManager.Entity<T>();
            var attr = entity.Key;

            System.Threading.Interlocked.Increment(ref _DynamicMethodNumber);
            var method = new DynamicMethod("_$DeleteParametersFactory_" + typeof(T).Name + _DynamicMethodNumber.ToString(),
               typeof(Parameter[]), new[] { typeof(T) }, true);
            var gen = method.GetILGenerator();

            int createMethodParamCount = attr.MaxLength > 0 ? 3 : 2;

            var createMethod = typeof(Parameter)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.Name == "Create" && m.IsGenericMethod && m.GetParameters().Length == createMethodParamCount)
                .Single();


            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Newarr, typeof(Parameter)); // new Parameter[count] => [array]
                
            gen.Emit(OpCodes.Dup);             // [array, array]
            gen.Emit(OpCodes.Ldc_I4_0);        // [array, array, indx]

            gen.Emit(OpCodes.Ldstr, attr.Name); // [array, array, indx, name]
            gen.Emit(OpCodes.Ldarg_0);          // [array, array, indx, name, T]
            if (attr.IsProperty)
                gen.Emit(OpCodes.Callvirt, attr.GetGetter());    // [array, array, indx, name, value]
            else
                gen.Emit(OpCodes.Ldfld, (FieldInfo)attr.Member); // [array, array, indx, name, value]

            if (attr.MaxLength > 0)
                gen.Emit(OpCodes.Ldc_I4, attr.MaxLength);        // [array, array, indx, name, size]

            gen.Emit(OpCodes.Call, createMethod.MakeGenericMethod(attr.MemberType)); // [array, array, indx, parameter];

            gen.Emit(OpCodes.Stelem, typeof(Parameter)); // [array]
            //gen.Emit(OpCodes.Stelem_Ref); // [array]
            
            gen.Emit(OpCodes.Ret);

            return method.CreateDelegate(typeof(Func<T, Parameter[]>)) as Func<T, Parameter[]>;
        }
        public static Func<T, Parameter[]> EmitFactory<T>(IModelManager modelManager, bool isInsert)
        {
            var entity = modelManager.Entity<T>();

            System.Threading.Interlocked.Increment(ref _DynamicMethodNumber);
            var method = new DynamicMethod((isInsert ? "_$InsertParametersFactory_" : "_$UpdateParametersFactory_") + typeof(T).Name + _DynamicMethodNumber.ToString(),
               typeof(Parameter[]), new[] { typeof(T) }, true);
            var gen = method.GetILGenerator();

            var attributes = entity.AllAttributes
                .Where(a => !a.IsNotMapped && !a.IsAssociation && (!a.IsKey || (a.IsKey && a.EntityType.BaseEntityType == null)))
                .Distinct(new AttributeNameComparer())
                .ToList();

            var createMethods = typeof(Parameter)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.Name == "Create" && m.IsGenericMethod)
                .ToList();

            var createMethod2 = createMethods
                .Where(m => m.GetParameters().Length == 2)
                .Single();

            var createMethod3 = createMethods
                .Where(m => m.GetParameters().Length == 3)
                .Single();

            var paramDirectionField = typeof(Parameter).GetField("Direction");


            gen.Emit(OpCodes.Ldc_I4, attributes.Count);
            gen.Emit(OpCodes.Newarr, typeof(Parameter)); // new Parameter[count] => [array]
            
            for (int i = 0; i < attributes.Count; i++)
            {
                var attr = attributes[i];

                gen.Emit(OpCodes.Dup);              // [array, array]
                gen.Emit(OpCodes.Ldc_I4, i);        // [array, array, indx]

                gen.Emit(OpCodes.Ldstr, attr.Name); // [array, array, indx, name]
                gen.Emit(OpCodes.Ldarg_0);          // [array, array, indx, name, T]
                if (attr.IsProperty)
                    gen.Emit(OpCodes.Callvirt, attr.GetGetter());    // [array, array, indx, name, value]
                else
                    gen.Emit(OpCodes.Ldfld, (FieldInfo)attr.Member); // [array, array, indx, name, value]

                if (attr.MaxLength > 0)
                {
                    gen.Emit(OpCodes.Ldc_I4, attr.MaxLength);         // [array, array, indx, name, value, size]
                    gen.Emit(OpCodes.Call, createMethod3.MakeGenericMethod(attr.MemberType)); // [array, array, indx, parameter];
                }
                else
                    gen.Emit(OpCodes.Call, createMethod2.MakeGenericMethod(attr.MemberType)); // [array, array, indx, parameter];

                if ((isInsert && attr.DbGeneratedOption != DatabaseGeneratedOption.None) ||
                    (!isInsert && attr.DbGeneratedOption != DatabaseGeneratedOption.None && !attr.IsKey) ||
                    (isInsert && attr.IsKey && !string.IsNullOrEmpty(attr.KeySequenceName)) ||
                    (attr.IsKey && attr.DbGeneratedOption == DatabaseGeneratedOption.Identity)
                    )
                {
                    gen.Emit(OpCodes.Dup);                                         // [array, array, indx, parameter, parameter];
                    gen.Emit(OpCodes.Ldc_I4, (int)ParameterDirection.InputOutput); // [array, array, indx, parameter, parameter, ParameterDirection.InputOutput];
                    gen.Emit(OpCodes.Stfld, paramDirectionField);                  // [array, array, indx, parameter];
                }


                gen.Emit(OpCodes.Stelem, typeof(Parameter)); // [array]
                //gen.Emit(OpCodes.Stelem_Ref); // [array]
            }

            gen.Emit(OpCodes.Ret);

            return method.CreateDelegate(typeof(Func<T, Parameter[]>)) as Func<T, Parameter[]>;
        }

        
        class AttributeNameComparer : IEqualityComparer<IEntityAttribute>
        {
            public bool Equals(IEntityAttribute x, IEntityAttribute y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(IEntityAttribute obj)
            {
                return obj.Name.GetHashCode();
            }
        }

    }
}
