using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace RC.DBA.Emit
{
    /*
           var mappedObject = new SqlDataReaderMapper<DTOObject>(reader)
                .NameTransformers("_", "")
                .ForMember<int>("CurrencyId")
                .ForMember("CurrencyCode", "Code")
                .ForMember<string>("CreatedByUser", "User").Trim()
                .ForMemberManual("CountryCode", val => val.ToString().Substring(0, 10))
                .ForMemberManual("ZipCode", val => val.ToString().Substring(0, 5), "ZIP")
                .Build();
            */

    public class SqlDataReaderMapper<T>
    {
        private static int _dynamicMethodNumber;

        private Dictionary<string, MemberInfo> _columsMap;

        private Dictionary<string, bool> _columsNullCheck;

        private Dictionary<string, Type> _columsDbType;

        private Dictionary<string, MethodInfo> _columsEnumFromString;

        private Dictionary<string, MethodInfo> _columsConvertes;

        private HashSet<string> _nullCheck;

        public static void InitJsonDeserializeMethod(MethodInfo methodInfo)
        {
            ReflectionData._jsonDeserializeMethod = methodInfo;
        }

        public SqlDataReaderMapper<T> ForMember<TVal>(Expression<Func<T, TVal>> member, string sqlColumnName = null, bool? checkNull = null)
        {
            var memberInfo = Helper.Member<T, TVal>(member);
            if (sqlColumnName != null && sqlColumnName != memberInfo.Name)
            {
                if (_columsMap == null)
                    _columsMap = new Dictionary<string, MemberInfo>();

                _columsMap[sqlColumnName] = memberInfo;
            }

            if (checkNull != null)
            {
                if (_columsNullCheck == null)
                    _columsNullCheck = new Dictionary<string, bool>();

                if (sqlColumnName == null)
                    sqlColumnName = memberInfo.Name;

                _columsNullCheck.Add(sqlColumnName, checkNull.Value);
            }

            return this;
        }

        public SqlDataReaderMapper<T> ForMemberManual<TVal>(Expression<Func<T, TVal>> member, Expression<Func<TVal, TVal>> convert, string sqlColumnName = null)
        {
            var convertmethodInfo = Helper.Method(convert);

            if (sqlColumnName == null) sqlColumnName = Helper.Member<T, TVal>(member).Name;
            if (_columsConvertes == null) _columsConvertes = new Dictionary<string, MethodInfo>();

            _columsConvertes[sqlColumnName] = convertmethodInfo;

            return this;
        }

        public SqlDataReaderMapper<T> UseDbTypeForMember<TVal>(Expression<Func<T, TVal>> member, Type dbType, string sqlColumnName = null)
        {
            if (sqlColumnName != null)
                ForMember(member, sqlColumnName);
            else
                sqlColumnName = Helper.Member<T, TVal>(member).Name;

            if (_columsDbType == null)
                _columsDbType = new Dictionary<string, Type>();

            _columsDbType[sqlColumnName] = dbType;

            return this;
        }

        public SqlDataReaderMapper<T> UseStringForEnumMember<TEnum>(Expression<Func<T, TEnum>> member, string sqlColumnName = null)
        {
            var enumType = Helper.GetNonNullableType(typeof(TEnum));
            if (!enumType.IsEnum)
                throw new ArgumentException($"Member type ${enumType.FullName} is not Enum type!");

            UseDbTypeForMember(member, typeof(string), sqlColumnName);

            if (sqlColumnName == null)
                sqlColumnName = Helper.Member<T, TEnum>(member).Name;

            var parseMethodInfo = typeof(ReflectionData).GetMethod("ParseEnum", new Type[] { typeof(string) }); 
            parseMethodInfo = parseMethodInfo.MakeGenericMethod(enumType);

            if (_columsEnumFromString == null)
                _columsEnumFromString = new Dictionary<string, MethodInfo>();

            _columsEnumFromString[sqlColumnName] = parseMethodInfo;

            return this;
        }

        //public SqlDataReaderMapper<T> UseConverterForMember<TVal, TDbVal>(Expression<Func<T, TVal>> member, Func<TDbVal, TVal> converter, string sqlColumnName = null)
        //{
        //    UseDbTyForMember(member, typeof(TDbVal), sqlColumnName);

        //    if (sqlColumnName == null)
        //        sqlColumnName = Helper.Member<T, TVal>(member).Name;

        //    if (_columsConvertes == null)
        //        _columsConvertes = new Dictionary<string, object>();

        //    _columsConvertes[sqlColumnName] = converter;

        //    return this;
        //}

        public Func<IDataReader, T> Build(IDataReader reader)
        {
            var dynamicMethodNumber = System.Threading.Interlocked.Increment(ref _dynamicMethodNumber);
            var type = typeof(T);
            var method = new DynamicMethod("_$SqlDataReaderMapper_" + typeof(T).Name + dynamicMethodNumber.ToString(),
                type, new[] { typeof(IDataReader) }, true);

            var gen = method.GetILGenerator();

            var notNullableType = Helper.GetNonNullableType(type);
            if (Type.GetTypeCode(notNullableType) != TypeCode.Object)
                return BuildForValue(gen, reader, method, type);

            EmitItem(reader, type, gen, null); // [] => [item]

            gen.Emit(OpCodes.Ret);

            return method.CreateDelegate(typeof(Func<IDataReader, T>)) as Func<IDataReader, T>;
        }

        public Func<IDataReader, ValueTuple<T, T1>> Build<T1>(IDataReader reader, string alias1)
        {
            var dynamicMethodNumber = System.Threading.Interlocked.Increment(ref _dynamicMethodNumber);
            
            var method = new DynamicMethod("_$SqlDataReaderMapper_" + typeof(T).Name + dynamicMethodNumber.ToString(),
                typeof(ValueTuple<T, T1>), new[] { typeof(IDataReader) }, true);

            var gen = method.GetILGenerator();

            Type itemType = typeof(T);
            EmitItem(reader, itemType, gen, null); // [] => [item]

            Type itemType1 = typeof(T1);
            EmitItem(reader, itemType1, gen, alias1); // [item] => [item, item1]

            var tupleType = typeof(ValueTuple<T, T1>);
            gen.Emit(OpCodes.Newobj, tupleType.GetConstructor(new[] { itemType, itemType1 })); // [ (item, item1) ]

            gen.Emit(OpCodes.Ret);

            return method.CreateDelegate(typeof(Func<IDataReader, ValueTuple<T, T1>>)) as Func<IDataReader, ValueTuple<T, T1>>;
        }

        public Func<IDataReader, ValueTuple<T, T1, T2>> Build<T1, T2>(IDataReader reader, string alias1, string alias2)
        {
            var dynamicMethodNumber = System.Threading.Interlocked.Increment(ref _dynamicMethodNumber);

            var method = new DynamicMethod("_$SqlDataReaderMapper_" + typeof(T).Name + dynamicMethodNumber.ToString(),
                typeof(ValueTuple<T, T1>), new[] { typeof(IDataReader) }, true);

            var gen = method.GetILGenerator();

            Type itemType = typeof(T);
            EmitItem(reader, itemType, gen, null); // [] => [item]

            Type itemType1 = typeof(T1);
            EmitItem(reader, itemType1, gen, alias1); // [item] => [item, item1]

            Type itemType2 = typeof(T2);
            EmitItem(reader, itemType2, gen, alias2); // [item] => [item, item1, item2]

            var tupleType = typeof(ValueTuple<T, T1, T2>);
            gen.Emit(OpCodes.Newobj, tupleType.GetConstructor(new[] { itemType, itemType1, itemType2 })); // [ (item, item1, item2) ]

            gen.Emit(OpCodes.Ret);

            return method.CreateDelegate(typeof(Func<IDataReader, ValueTuple<T, T1, T2>>)) as Func<IDataReader, ValueTuple<T, T1, T2>>;
        }

        private void EmitItem(IDataReader reader, Type itemType, ILGenerator gen, string alias) // [...] => [..., item]
        {
            var cnt = reader.FieldCount;

            EmitHelper.EmitNewWithDefaultCtor(gen, itemType); // [...] => [..., item]

            for (int ordinal = 0; ordinal < cnt; ordinal++)
            {
                var fieldName = reader.GetName(ordinal);

                if (fieldName.IndexOf('.') < 0)
                {
                    if (alias == null)
                        EmitForSimpleProp(gen, itemType, fieldName, ordinal);

                    continue;
                }

                var fieldNames = fieldName.Split('.');
                if (alias == null)
                    EmitForComplexProp(gen, itemType, fieldNames, 0, ordinal);
                else if(alias == fieldNames[0])
                {
                    if (fieldNames.Length == 2)
                        EmitForSimpleProp(gen, itemType, fieldNames[1], ordinal);
                    else
                        EmitForComplexProp(gen, itemType, fieldNames, 1, ordinal);
                }
            }

            if (itemType.IsValueType)
                gen.Emit(OpCodes.Ldobj, itemType); // [item &] => [item]
        }

        private void EmitForComplexProp(ILGenerator gen, Type type, string[] fieldNames, int indx, int ordinal)
        {
            var fieldName = fieldNames[indx++];
            var memberInfo = GetMemberInfo(type, fieldName);
            if (memberInfo == null) return;

            if (indx == fieldNames.Length)
            {
                EmitForSimpleProp(gen, type, fieldName, ordinal);
                return;
            }

            var attrName = String.Join(".", fieldNames, 0, indx);
            if (_nullCheck == null) _nullCheck = new HashSet<string>();

            gen.Emit(OpCodes.Dup);     // [item] => [item, item]

            if (memberInfo is PropertyInfo propertyInfo)
            {
                type = propertyInfo.PropertyType;

                if (_nullCheck.Add(attrName))
                {
                    gen.Emit(OpCodes.Dup); // [item, item] => [item, item, item];
                    gen.Emit(OpCodes.Callvirt, propertyInfo.GetGetMethod()); // [item, item, item] => [item, item, prop_Value];

                    var lblNotNullProp = gen.DefineLabel();
                    gen.Emit(OpCodes.Brtrue_S, lblNotNullProp); // [item, item, prop_Value] => [item, item]  if(prop_Value != null) goto lblNotNull
                    EmitHelper.EmitNewWithDefaultCtor(gen, propertyInfo.PropertyType); // [item, item] => [item, item, prop_Value]
                    gen.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod()); // [item, item, prop_Value] => [item]
                    gen.Emit(OpCodes.Dup); // [item] => [item, item];
                    gen.MarkLabel(lblNotNullProp);
                    gen.Emit(OpCodes.Callvirt, propertyInfo.GetGetMethod()); //[item, item] => [item, prop_Value];

                }
                else
                {
                    gen.Emit(OpCodes.Callvirt, propertyInfo.GetGetMethod()); //[item, item] => [item, prop_Value];
                }

            }
            else if (memberInfo is FieldInfo fieldInfo)
            {
                type = fieldInfo.FieldType;

                if (_nullCheck.Add(attrName))
                {
                    gen.Emit(OpCodes.Dup); // [item, item] => [item, item, item];
                    gen.Emit(OpCodes.Ldfld, fieldInfo); // [item, item, item] => [item, item, field_Value];

                    var lblNotNullField = gen.DefineLabel();
                    gen.Emit(OpCodes.Brtrue_S, lblNotNullField); // [item, item, field_Value] => [item, item]  if(field_Value != null) goto lblNotNull
                    EmitHelper.EmitNewWithDefaultCtor(gen, type); // [item, item] => [item, item, field_Value]
                    gen.Emit(OpCodes.Stfld, fieldInfo); // [item, item, field_Value] => [item]
                    gen.Emit(OpCodes.Dup); // [item] => [item, item];
                    gen.MarkLabel(lblNotNullField);
                    gen.Emit(OpCodes.Ldfld, fieldInfo); //[item, item] => [item, field_Value];

                }
                else
                {
                    gen.Emit(OpCodes.Ldfld, fieldInfo); //[item, item] => [item, field_Value];
                }
            }
            else
                throw new ApplicationException();

            EmitForComplexProp(gen, type, fieldNames, indx, ordinal);

            gen.Emit(OpCodes.Pop); //  [item, prop_Value || field_Value] => [item]

        }

        private void EmitForSimpleProp(ILGenerator gen, Type type, string fieldName, int ordinal)
        {
            var memberInfo = GetMemberInfo(type, fieldName);
            if (memberInfo == null) return;


            var memberType = GetMemberType(memberInfo);

            if (_columsNullCheck == null || !_columsNullCheck.TryGetValue(fieldName, out var checkNull))
            {
                checkNull = Helper.IsTypeCanBeNull(memberType);
            }

            if (checkNull)
                EmitReadAndSetFieldOrPropValueThatCanBeNULL(gen, memberInfo, fieldName, ordinal);
            else
                EmitReadAndSetFieldOrPropValue(gen, memberInfo, fieldName, ordinal);
        }

        private Func<IDataReader, T> BuildForValue(ILGenerator gen, IDataReader reader, DynamicMethod method, Type type, bool? checkNull = null)
        {
            if (checkNull == null)
                checkNull = Helper.IsTypeCanBeNull(type);

            Label lblDbNull = new Label();
            if (checkNull == true)
                lblDbNull = EmitReadIsDbNULL(gen, 0);


            gen.Emit(OpCodes.Ldarg_0); // [] => [reader]
            gen.Emit(OpCodes.Ldc_I4_0); // [reader] => [reader, 0 (ordinal)]
            var readerMethod = ReaderMethodFromType(Helper.GetNonNullableType(type));
            gen.Emit(OpCodes.Callvirt, readerMethod); // [reader, 0] => [value]
            EmitHelper.EmitNullableCtor(gen, type);

            if (checkNull == true)
            {
                var lblEnd = gen.DefineLabel();
                gen.Emit(OpCodes.Br, lblEnd);
                gen.MarkLabel(lblDbNull);

                EmitDefaultValue(gen, type);

                gen.MarkLabel(lblEnd);
            }

            gen.Emit(OpCodes.Ret);

            return method.CreateDelegate(typeof(Func<IDataReader, T>)) as Func<IDataReader, T>;
        }

        /*
        private static void EmitNewWithDefaultCtor(ILGenerator gen, Type type)
        {
            if (type.IsValueType)
            {
                var local = gen.DeclareLocal(type);
                gen.Emit(OpCodes.Ldloca_S, local);
                gen.Emit(OpCodes.Initobj, type);
                gen.Emit(OpCodes.Ldloca_S, local);
            }
            else
            {
                gen.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
            }
        }
        */

        private static void EmitDefaultValue(ILGenerator gen, Type type)
        {
            // Emit default(T)
            var notNullableType = Helper.GetNonNullableType(type);
            switch (Type.GetTypeCode(notNullableType))
            {
                case TypeCode.String:
                    gen.Emit(OpCodes.Ldnull);
                    break;

                case TypeCode.Int32:
                case TypeCode.Boolean:
                    gen.Emit(OpCodes.Ldc_I4_0);
                    break;

                case TypeCode.Int64:
                    gen.Emit(OpCodes.Ldc_I8, 0L);
                    break;

                case TypeCode.Double:
                    gen.Emit(OpCodes.Ldc_R8, 0);
                    break;

                case TypeCode.DateTime:
                case TypeCode.Decimal:
                    var dtLocal = gen.DeclareLocal(notNullableType);

                    gen.Emit(OpCodes.Ldloca_S, dtLocal);
                    gen.Emit(OpCodes.Initobj, notNullableType);
                    gen.Emit(OpCodes.Ldloc_S, dtLocal);

                    break;

                default:
                    throw new NotSupportedException("default(T): unsupported type " + type.FullName);
            }

            EmitHelper.EmitNullableCtor(gen, type);

        }

        private void EmitReadAndSetFieldOrPropValueThatCanBeNULL(ILGenerator gen, MemberInfo memberInfo, string fieldName, int ordinal)
        {
            var lblDbNull = EmitReadIsDbNULL(gen, ordinal);

            EmitReadAndSetFieldOrPropValue(gen, memberInfo, fieldName, ordinal);

            gen.MarkLabel(lblDbNull);
        }

        private static Label EmitReadIsDbNULL(ILGenerator gen, int ordinal)
        {
            var isDbNullMethod = typeof(IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) }); ;

            gen.Emit(OpCodes.Ldarg_0);      // [...] => [..., reader]             
            gen.Emit(OpCodes.Ldc_I4, ordinal); // [..., reader] => [..., reader, ordinal] 
            gen.Emit(OpCodes.Callvirt, isDbNullMethod); // [..., reader, ordinal] => [..., reader.IsDbNull(ordinal)] 

            //if value is DbNull skip property setter
            var lblDbNull = gen.DefineLabel();
            gen.Emit(OpCodes.Brtrue, lblDbNull); // if(reader.IsDbNull(ordinal)) => goto lblDbNull 

            return lblDbNull;
        }

        private void EmitReadAndSetFieldOrPropValue(ILGenerator gen, MemberInfo memberInfo, string fieldName, int ordinal)
        {
            gen.Emit(OpCodes.Dup);     // [item] => [item, item]
            gen.Emit(OpCodes.Ldarg_0); // [item, item] => [item, item, reader]

            MethodInfo propSetter = null; FieldInfo fieldInfo = null;
            Type valueType;
            if (memberInfo is PropertyInfo propertyInfo)
            {
                valueType = propertyInfo.PropertyType;
                propSetter = propertyInfo.GetSetMethod();
            }
            else
            {
                fieldInfo = (FieldInfo)memberInfo;
                valueType = fieldInfo.FieldType;
            }

            if (_columsDbType == null || !_columsDbType.TryGetValue(fieldName, out var dbValueType))
                dbValueType = valueType;

            var readerMethod = ReaderMethodFromType(Helper.GetNonNullableType(dbValueType));

            gen.Emit(OpCodes.Ldc_I4, ordinal); // [item, item, reader] => [item, item, reader, ordinal]

            gen.Emit(OpCodes.Callvirt, readerMethod); // [item, item, reader, ordinal] => [item, item, value]

            if (_columsEnumFromString != null && _columsEnumFromString.TryGetValue(fieldName, out var enumParseMethodInfo))
            {
                //var nonNullableValueType = Helper.GetNonNullableType(valueType);
                //TEnum Enum.Parse<TEnum>(string value)
                gen.Emit(OpCodes.Call, enumParseMethodInfo); // [item, item, value] => [item, item, Enum.Parse<TEnum>(value)]
            }

            EmitHelper.EmitNullableCtor(gen, valueType); // [item, item, value] => [item, item, Nullable<value>]

            //if (_columsConvertes == null || !_columsConvertes.TryGetValue(fieldName, out var converter))

            //else
            //{
            //    gen.Emit(OpCodes.Callvirt, converter.GetType().GetMethod("Invoke"));
            //}

            if (TypeIsCollection(valueType))
            {
                var deserializeMethod = ReflectionData.GetJsonDeserializeMethod(valueType);
                gen.Emit(OpCodes.Call, deserializeMethod); // [item, item, value] => [item, item, deserialize_value]
            }

            if (propSetter != null)
                gen.Emit(OpCodes.Callvirt, propSetter); // [item, item, Nullable<value>] => [item]
            else
                gen.Emit(OpCodes.Stfld, fieldInfo);    // [item, item, Nullable<value>] => [item]
        }

        /*
        private static void EmitNullableCtor(ILGenerator gen, Type valueType)
        {
            if (Helper.IsNullableType(valueType)) // If propType is Nullable<?>
            {
                // [..., valueType]

                var type = Helper.GetNonNullableType(valueType);
                var ctor = typeof(Nullable<>).MakeGenericType(type).GetConstructor(new Type[] { type });
                gen.Emit(OpCodes.Newobj, ctor);

                // [..., new Nullable<propType>(propValue)]
            }
        }
        */
        private static Type GetMemberType(MemberInfo member)
        {
            if (member is PropertyInfo propertyInfo)
                return propertyInfo.PropertyType;
            else
                return ((FieldInfo)member).FieldType;
        }

        private MemberInfo GetMemberInfo(Type type, string fieldName)
        {
            if (_columsMap == null || !_columsMap.TryGetValue(fieldName, out var memberInfo))
            {
                var prop = type.GetProperty(fieldName);
                if (prop != null)
                {
                    memberInfo = prop;
                }
                else
                {
                    var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field == null)
                        return null;

                    memberInfo = field;
                }

            }

            return memberInfo;
        }

        private static MethodInfo ReaderMethodFromType(Type valueType)
        {
            if (!ReflectionData.ReaderMethodsCache.TryGetValue(valueType, out var method))
            {
                if (TypeIsCollection(valueType))
                {
                    return ReflectionData.ReaderMethodsCache[typeof(string)];
                }
                throw new NotSupportedException("ReaderMethodFromType: unsupported type " + valueType.FullName);
            }

            return method;
        }

        private static bool TypeIsCollection(Type type) =>
            typeof(string) != type && typeof(System.Collections.IEnumerable).IsAssignableFrom(type);

    }

    static class ReflectionData
    {
        public static readonly Dictionary<Type, MethodInfo> ReaderMethodsCache = new Dictionary<Type, MethodInfo>()
        {
            { typeof(string), typeof(IDataRecord).GetMethod("GetString", new Type[] { typeof(int) }) },
            { typeof(int), typeof(IDataRecord).GetMethod("GetInt32", new Type[] { typeof(int) }) },
            { typeof(long), typeof(IDataRecord).GetMethod("GetInt64", new Type[] { typeof(int) }) },
            { typeof(DateTime), typeof(IDataRecord).GetMethod("GetDateTime", new Type[] { typeof(int) }) },
            { typeof(decimal), typeof(IDataRecord).GetMethod("GetDecimal", new Type[] { typeof(int) }) },
            { typeof(double), typeof(IDataRecord).GetMethod("GetDouble", new Type[] { typeof(int) }) },
            { typeof(bool), typeof(IDataRecord).GetMethod("GetBoolean", new Type[] { typeof(int) }) },
            { typeof(byte), typeof(IDataRecord).GetMethod("GetByte", new Type[] { typeof(int) }) },
            { typeof(Guid), typeof(IDataRecord).GetMethod("GetGuid", new Type[] { typeof(int) }) },
            { typeof(char), typeof(IDataRecord).GetMethod("GetChar", new Type[] { typeof(int) }) },
            { typeof(float), typeof(IDataRecord).GetMethod("GetFloat", new Type[] { typeof(int) }) },
            { typeof(short), typeof(IDataRecord).GetMethod("GetInt16", new Type[] { typeof(int) }) },

        };

        public static MethodInfo _jsonDeserializeMethod;

        public static MethodInfo GetJsonDeserializeMethod(Type type) => _jsonDeserializeMethod.MakeGenericMethod(type);

        //private static MethodInfo GetJsonDeserializeGenericMethod()
        //{
        //    return
        //        typeof(Newtonsoft.Json.JsonConvert).GetMethods(BindingFlags.Public | BindingFlags.Static)
        //           .Where(m => m.Name == "DeserializeObject" && m.IsGenericMethod)
        //           .Select(m => (method: m, args: m.GetParameters()))
        //           .Where(x => x.args.Length == 1 && x.args[0].ParameterType == typeof(string))
        //           .Select(x => x.method)
        //           .Single();
        //}

        public static TEnum ParseEnum<TEnum>(string value) where TEnum : struct
        {
            if (Enum.TryParse<TEnum>(value, out var result))
                return result;

            return default(TEnum);
        }
    }
}
