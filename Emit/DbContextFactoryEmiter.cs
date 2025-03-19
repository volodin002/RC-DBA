using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RC.DBA.Attributes;
using RC.DBA.Collections;
using RC.DBA.Metamodel;

namespace RC.DBA.Emit
{
    class DbContextFactoryEmiter
    {
        static Type[] readerArgTypes = new Type[] { typeof(int) };

        internal static Type[] ReaderArgTypes = readerArgTypes;

        private static int _DynamicMethodNumber;

        public static Func<DbDataReader, IList<T>> EmitResultListFactory<T>(DbDataReader reader, IModelManager modelManager)
        {
            int fieldCount = reader.FieldCount;

            type_meta meta = new type_meta(typeof(T), modelManager);

            for (int i = 0; i < fieldCount; i++)
            {
                var name = reader.GetName(i);
                var names = name.Split('.');

                meta.set_meta_prop(name, 0, names, i);
            }

           var cacheEmmiter = new EntityCacheEmitter();

           System.Threading.Interlocked.Increment(ref _DynamicMethodNumber);
           var method = new DynamicMethod("_$ResultListFactory_" + typeof(T).Name + _DynamicMethodNumber.ToString(),
                typeof(IList<T>), new[] { typeof(DbDataReader) }, true);
            var gen = method.GetILGenerator();


            cacheEmmiter.EmitCacheInit(gen, meta, false);
            Type resultType = cacheEmmiter.HasCache(meta)
                ? typeof(Collections.HashSetList<>).MakeGenericType(meta.type)
                : typeof(List<>).MakeGenericType(meta.type);
            
            var resultCtor = resultType.GetConstructor(Type.EmptyTypes);
            var resultAddMethod = resultType.GetMethod("Add");


            gen.Emit(OpCodes.Newobj, resultCtor); // [ items ]


            var whileStartLabel = gen.DefineLabel();
            var whileEndLabel = gen.DefineLabel();
            
            gen.MarkLabel(whileStartLabel); // while {

            gen.Emit(OpCodes.Ldarg_0); // [ items, reader ]

            gen.Emit(OpCodes.Callvirt, typeof(DbDataReader).GetMethod("Read", Type.EmptyTypes)); // [ items, reader.Read() ]

            gen.Emit(OpCodes.Brfalse, whileEndLabel); // goto to the end of while(){} loop => [items]


            gen.Emit(OpCodes.Dup); // [ items, items ]
            

            cacheEmmiter.EmitNewObject(gen, meta); // [ items, items, item ]

            if (meta.id_prop != null) EmitSimpleProp(gen, meta.id_prop);
            meta.props?.Apply(p => EmitSimpleProp(gen, p));
            meta.obj_props?.Apply(p => EmitEntityProp(gen, p, cacheEmmiter));
            meta.collection_props?.Apply(p => EmitEntityCollectionProp(gen, p, cacheEmmiter));

            cacheEmmiter.EmitSetImplementationProp(gen, meta);


            gen.Emit(OpCodes.Call, resultAddMethod);   // items.Add(item) => [items]            

            gen.Emit(OpCodes.Br, whileStartLabel); // goto to the start of while(){} loop => [items]
            gen.MarkLabel(whileEndLabel); // } end while
            
            gen.Emit(OpCodes.Ret); // return [items]

            return method.CreateDelegate(typeof(Func<DbDataReader, IList<T>>)) as Func<DbDataReader, IList<T>>;
        }

        public static Func<DbDataReader, IList<ValueTuple<T, T1>>> EmitResultListFactory<T, T1>(DbDataReader reader, IModelManager modelManager, string alias1)
        {
            int fieldCount = reader.FieldCount;

            type_meta meta = new type_meta(typeof(T), modelManager);
            type_meta meta1 = new type_meta(typeof(T1), modelManager);

            for (int i = 0; i < fieldCount; i++)
            {
                var name = reader.GetName(i);
                var names = name.Split('.');


                if (names.Length > 1 && names[0] == alias1)
                {
                    var names1 = new string[names.Length - 1];
                    Array.Copy(names, 1, names1, 0, names1.Length);

                    meta1.set_meta_prop(name, 0, names1, i);
                }
                else
                    meta.set_meta_prop(name, 0, names, i);
            }

            var cacheEmmiter = new EntityCacheEmitter();
            var cacheEmmiter1 = new EntityCacheEmitter();

            System.Threading.Interlocked.Increment(ref _DynamicMethodNumber);
            var method = new DynamicMethod("_$ResultListFactory_" + typeof(T).Name + _DynamicMethodNumber.ToString(),
                 typeof(IList<ValueTuple<T, T1>>), new[] { typeof(DbDataReader) }, true);
            var gen = method.GetILGenerator();

            Type tupleType = typeof(ValueTuple<T, T1>);

            cacheEmmiter.EmitCacheInit(gen, meta, false);
            cacheEmmiter1.EmitCacheInit(gen, meta1, cacheEmmiter.HasCache(meta));
            Type resultType = cacheEmmiter.HasCache(meta) || cacheEmmiter1.HasCache(meta1)
                ? typeof(Collections.HashSetList<>).MakeGenericType(tupleType)
                : typeof(List<>).MakeGenericType(tupleType);

            var resultCtor = resultType.GetConstructor(Type.EmptyTypes);
            var resultAddMethod = resultType.GetMethod("Add");


            gen.Emit(OpCodes.Newobj, resultCtor); // [ items ]


            var whileStartLabel = gen.DefineLabel();
            var whileEndLabel = gen.DefineLabel();

            gen.MarkLabel(whileStartLabel); // while {

            gen.Emit(OpCodes.Ldarg_0); // [ items, reader ]

            gen.Emit(OpCodes.Callvirt, typeof(DbDataReader).GetMethod("Read", Type.EmptyTypes)); // [ items, reader.Read() ]

            gen.Emit(OpCodes.Brfalse, whileEndLabel); // goto to the end of while(){} loop => [items]


            gen.Emit(OpCodes.Dup); // [ items, items ]


            cacheEmmiter.EmitNewObject(gen, meta); // [ items, items, item ]

            if (meta.id_prop != null) EmitSimpleProp(gen, meta.id_prop);
            meta.props?.Apply(p => EmitSimpleProp(gen, p));
            meta.obj_props?.Apply(p => EmitEntityProp(gen, p, cacheEmmiter));
            meta.collection_props?.Apply(p => EmitEntityCollectionProp(gen, p, cacheEmmiter));

            cacheEmmiter.EmitSetImplementationProp(gen, meta);

            {
                var isDbNullMethod = typeof(DbDataReader).GetMethod("IsDBNull", readerArgTypes); ;

                gen.Emit(OpCodes.Ldarg_0);                  // [ items, items, item, reader ]
                gen.Emit(OpCodes.Ldc_I4, meta1.id_prop.ordinal); // [items, items, item, reader, i_ordinal]
                gen.Emit(OpCodes.Callvirt, isDbNullMethod); // [items, items, item, isDbNull]

                //if item1.ID is NULL skip creation of item1
                var lblItem1IsNull = gen.DefineLabel();
                var lblItem1End = gen.DefineLabel();
                gen.Emit(OpCodes.Brtrue, lblItem1IsNull);        // [items, items, item] if(item.ID == null) goto lblItem1IsNull;

                cacheEmmiter1.EmitNewObject(gen, meta1); // [ items, items, item, item1 ]

                if (meta1.id_prop != null) EmitSimpleProp(gen, meta1.id_prop);
                meta1.props?.Apply(p => EmitSimpleProp(gen, p));
                meta1.obj_props?.Apply(p => EmitEntityProp(gen, p, cacheEmmiter1));
                meta1.collection_props?.Apply(p => EmitEntityCollectionProp(gen, p, cacheEmmiter1));

                cacheEmmiter1.EmitSetImplementationProp(gen, meta1); // [ items, items, item, item1 ]

                gen.Emit(OpCodes.Br, lblItem1End);

                gen.MarkLabel(lblItem1IsNull);
                gen.Emit(OpCodes.Ldnull); // [ items, items, item, (T1)null ]

                gen.MarkLabel(lblItem1End);
            }

            gen.Emit(OpCodes.Newobj, tupleType.GetConstructor(new[] { typeof(T), typeof(T1) })); // [ items, items, (item, item1) ]

            gen.Emit(OpCodes.Callvirt, resultAddMethod);   // items.Add((item, item1)) => [items]            

            gen.Emit(OpCodes.Br, whileStartLabel); // goto to the start of while(){} loop => [items]
            gen.MarkLabel(whileEndLabel); // } end while

            gen.Emit(OpCodes.Ret); // return [items]

            return method.CreateDelegate(typeof(Func<DbDataReader, IList<ValueTuple<T, T1>>>)) as Func<DbDataReader, IList<ValueTuple<T, T1>>>;
        }

        public static Func<DbDataReader, IList<ValueTuple<T, T1, T2>>> EmitResultListFactory<T, T1, T2>(DbDataReader reader, IModelManager modelManager, string alias1, string alias2)
        {
            int fieldCount = reader.FieldCount;

            type_meta meta = new type_meta(typeof(T), modelManager);
            type_meta meta1 = new type_meta(typeof(T1), modelManager);
            type_meta meta2 = new type_meta(typeof(T2), modelManager);

            for (int i = 0; i < fieldCount; i++)
            {
                var name = reader.GetName(i);
                var names = name.Split('.');


                if (names.Length > 1 && names[0] == alias1)
                {
                    var names1 = new string[names.Length - 1];
                    Array.Copy(names, 1, names1, 0, names1.Length);

                    meta1.set_meta_prop(name, 0, names1, i);
                }
                else if (names.Length > 1 && names[0] == alias2)
                {
                    var names2 = new string[names.Length - 1];
                    Array.Copy(names, 1, names2, 0, names2.Length);

                    meta2.set_meta_prop(name, 0, names2, i);
                }
                else
                    meta.set_meta_prop(name, 0, names, i);
            }

            var cacheEmmiter = new EntityCacheEmitter();
            var cacheEmmiter1 = new EntityCacheEmitter();
            var cacheEmmiter2 = new EntityCacheEmitter();

            System.Threading.Interlocked.Increment(ref _DynamicMethodNumber);
            var method = new DynamicMethod("_$ResultListFactory_" + typeof(T).Name + _DynamicMethodNumber.ToString(),
                 typeof(IList<ValueTuple<T, T1, T2>>), new[] { typeof(DbDataReader) }, true);
            var gen = method.GetILGenerator();

            Type tupleType = typeof(ValueTuple<T, T1, T2>);

            cacheEmmiter.EmitCacheInit(gen, meta, false);
            cacheEmmiter1.EmitCacheInit(gen, meta1, cacheEmmiter.HasCache(meta));
            cacheEmmiter2.EmitCacheInit(gen, meta2, cacheEmmiter.HasCache(meta));
            Type resultType = cacheEmmiter.HasCache(meta) || cacheEmmiter1.HasCache(meta1) || cacheEmmiter2.HasCache(meta2)
                ? typeof(Collections.HashSetList<>).MakeGenericType(tupleType)
                : typeof(List<>).MakeGenericType(tupleType);

            var resultCtor = resultType.GetConstructor(Type.EmptyTypes);
            var resultAddMethod = resultType.GetMethod("Add");


            gen.Emit(OpCodes.Newobj, resultCtor); // [ items ]


            var whileStartLabel = gen.DefineLabel();
            var whileEndLabel = gen.DefineLabel();

            gen.MarkLabel(whileStartLabel); // while {

            gen.Emit(OpCodes.Ldarg_0); // [ items, reader ]

            gen.Emit(OpCodes.Callvirt, typeof(DbDataReader).GetMethod("Read", Type.EmptyTypes)); // [ items, reader.Read() ]

            gen.Emit(OpCodes.Brfalse, whileEndLabel); // goto to the end of while(){} loop => [items]


            gen.Emit(OpCodes.Dup); // [ items, items ]


            cacheEmmiter.EmitNewObject(gen, meta); // [ items, items, item ]

            if (meta.id_prop != null) EmitSimpleProp(gen, meta.id_prop);
            meta.props?.Apply(p => EmitSimpleProp(gen, p));
            meta.obj_props?.Apply(p => EmitEntityProp(gen, p, cacheEmmiter));
            meta.collection_props?.Apply(p => EmitEntityCollectionProp(gen, p, cacheEmmiter));

            cacheEmmiter.EmitSetImplementationProp(gen, meta);

            {
                var isDbNullMethod = typeof(DbDataReader).GetMethod("IsDBNull", readerArgTypes); ;

                gen.Emit(OpCodes.Ldarg_0);                  // [ items, items, item, reader ]
                gen.Emit(OpCodes.Ldc_I4, meta1.id_prop.ordinal); // [items, items, item, reader, i_ordinal]
                gen.Emit(OpCodes.Callvirt, isDbNullMethod); // [items, items, item, isDbNull]

                //if item1.ID is NULL skip creation of item1
                var lblItem1IsNull = gen.DefineLabel();
                var lblItem1End = gen.DefineLabel();
                gen.Emit(OpCodes.Brtrue, lblItem1IsNull);        // [items, items, item] if(item1.ID == null) goto lblItem1IsNull;

                cacheEmmiter1.EmitNewObject(gen, meta1); // [ items, items, item, item1 ]

                if (meta1.id_prop != null) EmitSimpleProp(gen, meta1.id_prop);
                meta1.props?.Apply(p => EmitSimpleProp(gen, p));
                meta1.obj_props?.Apply(p => EmitEntityProp(gen, p, cacheEmmiter1));
                meta1.collection_props?.Apply(p => EmitEntityCollectionProp(gen, p, cacheEmmiter1));

                cacheEmmiter1.EmitSetImplementationProp(gen, meta1); // [ items, items, item, item1 ]

                gen.Emit(OpCodes.Br, lblItem1End);

                gen.MarkLabel(lblItem1IsNull);
                gen.Emit(OpCodes.Ldnull); // [ items, items, item, (T1)null ]

                gen.MarkLabel(lblItem1End);
            }

            {
                var isDbNullMethod = typeof(DbDataReader).GetMethod("IsDBNull", readerArgTypes); ;

                gen.Emit(OpCodes.Ldarg_0);                  // [ items, items, item, item1, reader ]
                gen.Emit(OpCodes.Ldc_I4, meta2.id_prop.ordinal); // [items, items, item, item1, reader, i_ordinal]
                gen.Emit(OpCodes.Callvirt, isDbNullMethod); // [items, items, item, item1, isDbNull]

                //if item1.ID is NULL skip creation of item1
                var lblItem2IsNull = gen.DefineLabel();
                var lblItem2End = gen.DefineLabel();
                gen.Emit(OpCodes.Brtrue, lblItem2IsNull);        // [items, items, item, item1] if(item2.ID == null) goto lblItem2IsNull;

                cacheEmmiter2.EmitNewObject(gen, meta2); // [ items, items, item, item1, item2 ]

                if (meta2.id_prop != null) EmitSimpleProp(gen, meta2.id_prop);
                meta2.props?.Apply(p => EmitSimpleProp(gen, p));
                meta2.obj_props?.Apply(p => EmitEntityProp(gen, p, cacheEmmiter2));
                meta2.collection_props?.Apply(p => EmitEntityCollectionProp(gen, p, cacheEmmiter2));

                cacheEmmiter2.EmitSetImplementationProp(gen, meta2); // [ items, items, item, item1, item2 ]

                gen.Emit(OpCodes.Br, lblItem2End);

                gen.MarkLabel(lblItem2IsNull);
                gen.Emit(OpCodes.Ldnull); // [ items, items, item, item1, (T2)null ]

                gen.MarkLabel(lblItem2End);
            }

            gen.Emit(OpCodes.Newobj, tupleType.GetConstructor(new[] { typeof(T), typeof(T1), typeof(T2) })); // [ items, items, (item, item1, item2) ]

            gen.Emit(OpCodes.Callvirt, resultAddMethod);   // items.Add((item, item1, item2)) => [items]            

            gen.Emit(OpCodes.Br, whileStartLabel); // goto to the start of while(){} loop => [items]
            gen.MarkLabel(whileEndLabel); // } end while

            gen.Emit(OpCodes.Ret); // return [items]

            return method.CreateDelegate(typeof(Func<DbDataReader, IList<ValueTuple<T, T1, T2>>>)) as Func<DbDataReader, IList<ValueTuple<T, T1, T2>>>;
        }

        static void EmitSimpleProp(ILGenerator gen, prop_meta metaProp)
        {
            // [ items, items, item ]

            var prop = metaProp.prop;
            Type propType = GetMemberType(prop);

            if ((Helper.IsTypeCanBeNull(propType) && !IsRequiredProp(prop)))
                EmitSimplePropThatCanBeNULL(gen, metaProp);
            else
                EmitReadAndSetSimpleProp(gen, metaProp);
            

            // [items, items, item]
        }

        static Label EmitChildIdProp(ILGenerator gen, obj_prop_meta childObj, EntityCacheEmitter entityEmitter, bool isCollection)
        {
            // [items, items, item]
            if (childObj.id_prop == null)
                throw new EmitterException("Child Object property do not have ID field!");
            var lblDbNull = EmitReadIsDbNULL(gen, childObj.id_prop);

            // [items, items, item]


            gen.Emit(OpCodes.Dup); // [items, items, item, item]
            if (isCollection)
            {
                MemberInfo prop = childObj.prop;
                MethodInfo propGetter = null;
                MethodInfo propSetter = null;
                FieldInfo fieldInfo = null;
                Type propType;
                if (prop is PropertyInfo propertyInfo)
                {
                    propGetter = propertyInfo.GetGetMethod();
                    propType = propertyInfo.PropertyType;
                    propSetter = propertyInfo.GetSetMethod();
                }
                else
                {
                    fieldInfo = (FieldInfo)prop;
                    propType = fieldInfo.FieldType;
                }

                if (propGetter != null)
                    gen.Emit(OpCodes.Callvirt, propGetter); // load collection to stack
                else
                    gen.Emit(OpCodes.Ldfld, fieldInfo);

                // [items, items, item, collection ]

                var lblCollNotNull = gen.DefineLabel();
                gen.Emit(OpCodes.Brtrue_S, lblCollNotNull); // if(collection !=null) => goto lblCollNotNull;
                // [items, items, item ]

                // Init collection property => item.collection = new List<?>();
                // [..., item]
                gen.Emit(OpCodes.Dup); // [..., item, item]

                var addMethodInfo = GetCollectionPropAddMethod(propType); //propType.GetMethod("Add");
                var listType = (addMethodInfo.ReturnType == typeof(bool) 
                    ? typeof(HashSet<>) 
                    : (entityEmitter.HasCache(childObj) ? typeof(Collections.HashSetList<>) : typeof(List<>))
                    )
                    .MakeGenericType(childObj.type);

                gen.Emit(OpCodes.Newobj, listType.GetConstructor(Type.EmptyTypes)); // [..., item, item, collection]
                if (propSetter != null)
                    gen.Emit(OpCodes.Callvirt, propSetter);
                else
                    gen.Emit(OpCodes.Stfld, fieldInfo);
                // [..., item]

                gen.MarkLabel(lblCollNotNull);

                gen.Emit(OpCodes.Dup); // [items, items, item, item]
                // load collection to stack
                if (propGetter != null)
                    gen.Emit(OpCodes.Callvirt, propGetter);
                else
                    gen.Emit(OpCodes.Ldfld, fieldInfo);
                // [items, items, item, collection ]
            }
            
            entityEmitter.EmitNewObject(gen, childObj);
            
            EmitReadAndSetSimpleProp(gen, childObj.id_prop);
            
            return lblDbNull;
        }

        private static void EmitSimplePropThatCanBeNULL(ILGenerator gen, prop_meta metaProp)
        {
            var lblDbNull = EmitReadIsDbNULL(gen, metaProp);

            EmitReadAndSetSimpleProp(gen, metaProp);

            gen.MarkLabel(lblDbNull);
        }

        private static Label EmitReadIsDbNULL(ILGenerator gen, prop_meta metaProp)
        {
            var isDbNullMethod = typeof(DbDataReader).GetMethod("IsDBNull", readerArgTypes); ;

            gen.Emit(OpCodes.Ldarg_0);                  // [items, items, item, reader]
            gen.Emit(OpCodes.Ldc_I4, metaProp.ordinal); // [items, items, item, reader, i_ordinal]
            gen.Emit(OpCodes.Callvirt, isDbNullMethod); // [items, items, item, isDbNull]

            //if value is DbNull skip property setter
            var lblDbNull = gen.DefineLabel();
            gen.Emit(OpCodes.Brtrue, lblDbNull);        // [items, items, item]

            return lblDbNull;
        }

        private static void EmitReadAndSetSimpleProp(ILGenerator gen, prop_meta metaProp)
        {
            gen.Emit(OpCodes.Dup);     // [ items, items, item, item ]
            gen.Emit(OpCodes.Ldarg_0); // [ items, items, item, item, reader ]

            var prop = metaProp.prop;

            MethodInfo propSetter = null; FieldInfo fieldInfo = null;
            Type propType;
            if (prop is PropertyInfo propertyInfo)
            {            
                propType = propertyInfo.PropertyType;
                propSetter = propertyInfo.GetSetMethod();
            }
            else
            {
                fieldInfo = (FieldInfo)prop;
                propType = fieldInfo.FieldType;
            }

            Type dbType;

            if (metaProp.convertMethod != null)
            {
                var arg = metaProp.convertMethod.GetParameters()[0];
                dbType = arg.ParameterType;
            }
            else
                dbType = propType;
            
           
            gen.Emit(OpCodes.Ldc_I4, metaProp.ordinal);// [items, items, item, item, reader, i_ordinal]
            var readerMethod = ReaderMethodFromType(Helper.GetNonNullableType(dbType));
            gen.Emit(readerMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, readerMethod); // [items, items, item, item, propValue]

            EmitHelper.EmitNullableCtor(gen, dbType);

            if (metaProp.convertMethod != null)
            {
                gen.Emit(OpCodes.Call, metaProp.convertMethod);
            }

            // [items, items, item]
            if (propSetter != null)
                gen.Emit(OpCodes.Callvirt, propSetter);
            else
                gen.Emit(OpCodes.Stfld, fieldInfo);
        }

        /*
        internal static void EmitNullableCtor(ILGenerator gen, Type propType)
        {
            if (Helper.IsNullableType(propType)) // If propType is Nullable<?>
            {
                // [..., propValue]

                var type = Helper.GetNonNullableType(propType);
                var ctor = typeof(Nullable<>).MakeGenericType(type).GetConstructor(new Type[] { type });
                gen.Emit(OpCodes.Newobj, ctor);

                // [..., new Nullable<propType>(propValue)]
            }
        }
        */

        static void EmitEntityProp(ILGenerator gen, obj_prop_meta metaProp, EntityCacheEmitter entityEmitter)
        {
            var prop = metaProp.prop;

            MethodInfo propSetter = null; FieldInfo fieldInfo = null;
            Type propType;
            if (prop is PropertyInfo propertyInfo)
            {
                propType = propertyInfo.PropertyType;
                propSetter = propertyInfo.GetSetMethod();
            }
            else
            {
                fieldInfo = (FieldInfo)prop;
                propType = fieldInfo.FieldType;
            }

            var childIsNull = EmitChildIdProp(gen, metaProp, entityEmitter, false);

            metaProp.props?.Apply(p => EmitSimpleProp(gen, p));
            metaProp.obj_props?.Apply(p => EmitEntityProp(gen, p, entityEmitter));
            metaProp.collection_props?.Apply(p => EmitEntityCollectionProp(gen, p, entityEmitter));

            entityEmitter.EmitSetImplementationProp(gen, metaProp);

            if (propSetter != null)
                gen.Emit(OpCodes.Callvirt, propSetter);
            else
                gen.Emit(OpCodes.Stfld, fieldInfo);

            gen.MarkLabel(childIsNull);

        }

        static void EmitEntityCollectionProp(ILGenerator gen, obj_prop_meta metaProp, EntityCacheEmitter entityEmitter)
        {
            Type propType = GetMemberType(metaProp.prop);

            var addMethodInfo = GetCollectionPropAddMethod(propType); //propType.GetMethod("Add");

            var childIsNull = EmitChildIdProp(gen, metaProp, entityEmitter, true);

            metaProp.props?.Apply(p => EmitSimpleProp(gen, p));
            metaProp.obj_props?.Apply(p => EmitEntityProp(gen, p, entityEmitter));
            metaProp.collection_props?.Apply(p => EmitEntityCollectionProp(gen, p, entityEmitter));


            entityEmitter.EmitSetImplementationProp(gen, metaProp);

            gen.Emit(OpCodes.Callvirt, addMethodInfo);
            if (addMethodInfo.ReturnType == typeof(bool))
                gen.Emit(OpCodes.Pop);

            gen.MarkLabel(childIsNull);

        }


        static Dictionary<Type, MethodInfo> _ReaderMethodsCache = new Dictionary<Type, MethodInfo>()
        {
            { typeof(string), typeof(DbDataReader).GetMethod("GetString", readerArgTypes) },
            { typeof(int), typeof(DbDataReader).GetMethod("GetInt32", readerArgTypes) },
            { typeof(long), typeof(DbDataReader).GetMethod("GetInt64", readerArgTypes) },
            { typeof(DateTime), typeof(DbDataReader).GetMethod("GetDateTime", readerArgTypes) },
            { typeof(decimal), typeof(DbDataReader).GetMethod("GetDecimal", readerArgTypes) },
            { typeof(double), typeof(DbDataReader).GetMethod("GetDouble", readerArgTypes) },
            { typeof(bool), typeof(DbDataReader).GetMethod("GetBoolean", readerArgTypes) },
            { typeof(byte), typeof(DbDataReader).GetMethod("GetByte", readerArgTypes) },
            { typeof(Guid), typeof(DbDataReader).GetMethod("GetGuid", readerArgTypes) },
            { typeof(char), typeof(DbDataReader).GetMethod("GetChar", readerArgTypes) },
            { typeof(float), typeof(DbDataReader).GetMethod("GetFloat", readerArgTypes) },
            { typeof(short), typeof(DbDataReader).GetMethod("GetInt16", readerArgTypes) },
            { typeof(byte[]), typeof(Helper).GetMethod(nameof(Helper.GetBytesBufferFromReader), new[] {typeof(DbDataReader), typeof(int) }) },

        };

        internal static MethodInfo ReaderMethodFromType(Type propType)
        {
            MethodInfo method;
            if(!_ReaderMethodsCache.TryGetValue(propType, out method))
                throw new NotSupportedException("ReaderMethodFromType: unsupported type " + propType.FullName);

            return method;
        }

        internal static bool IsRequiredProp(MemberInfo prop)
        {
            return prop.GetCustomAttribute<RequiredAttribute>() != null || prop.GetCustomAttribute<KeyAttribute>() != null;
        }

        class EntityCacheEmitter
        {
            Dictionary<Type, Locals> _locals_cache;
            //ConstructorInfo _resultCtor;
            //MethodInfo _resultAddMethod;

            //public ConstructorInfo ResultCtor => _resultCtor;
            //public MethodInfo ResultAddMethod => _resultAddMethod;

            public void EmitCacheInit(ILGenerator gen, type_meta meta, bool forceRootType)
            {
                var types = meta.get_types_with_cache(forceRootType);
                if (types.Count > 0) // we have caches
                {
                    _locals_cache = new Dictionary<Type, Locals>(types.Count);
                    foreach (var m in types)
                    {
                        if (m.id_prop == null)
                        {
                            continue;
                            /*
                            throw new EmitterException("Need ID property for type [" + m.type.FullName + "] in query result.\n" +
                                "Types that have collection property need ID in query result to have key in Entity cache.");
                            */
                        }
                            

                        var idType = GetMemberType(m.id_prop.prop);
                        var cacheType = typeof(Collections.HashMap<,>) //typeof(Dictionary<,>)
                           .MakeGenericType(new Type[] { idType, m.type });

                        var locals = new Locals() { cache = gen.DeclareLocal(cacheType), obj = gen.DeclareLocal(m.type) };
                        _locals_cache.Add(m.type, locals);

                        // => [...]

                        gen.Emit(OpCodes.Newobj, cacheType.GetConstructor(Type.EmptyTypes)); // => [..., cache]
                        gen.Emit(OpCodes.Stloc, locals.cache);

                        // => [...]
                    }

                    /*
                    // find base types
                    var baseTypes = new List<Type>();
                    var otherTypes = new List<Type>();
                    foreach (var m in types)
                    {
                        if (!types.Any(t => Helper.IsBaseType(m.type, t.type)))
                        {
                            var cacheType = typeof(Dictionary<,>)
                                .MakeGenericType(new Type[] { m.id_prop.prop.PropertyType, m.type });

                            var cache = _locals_cache[m.type].cache = gen.DeclareLocal(cacheType);

                            baseTypes.Add(m.type);

                            // => [...]

                            gen.Emit(OpCodes.Newobj, cacheType.GetConstructor(Type.EmptyTypes)); // => [..., cache]
                            gen.Emit(OpCodes.Stloc, cache);

                            // => [...]
                        }
                        else
                        {
                            otherTypes.Add(m.type);
                        }
                    }
                    foreach(var t in otherTypes)
                    {
                        var baseType = baseTypes.First(bt => Helper.IsBaseType(t, bt));
                        _locals_cache[t].cache = _locals_cache[baseType].cache;
                    }*/

                }

                /*
                Type resultType;
                if (HasCache(meta))
                {
                    resultType = typeof(Collections.HashSetList<>).MakeGenericType(meta.type);
                }
                else
                {
                    resultType = typeof(List<>).MakeGenericType(meta.type);
                }  
                _resultCtor = resultType.GetConstructor(Type.EmptyTypes);
                _resultAddMethod = resultType.GetMethod("Add");
                */
            }

            

            public static void EmitNew(ILGenerator gen, type_meta meta)
            {
                gen.Emit(OpCodes.Newobj, meta.type.GetConstructor(Type.EmptyTypes));
            }

            public static void EmitNewOrThrow(ILGenerator gen, type_meta meta)
            {
                if (meta.type.IsAbstract)
                {
                    var exType = typeof(NotSupportedException);
                    gen.Emit(OpCodes.Ldstr, "Wrong descriminator value");
                    gen.Emit(OpCodes.Newobj, exType.GetConstructor(new Type[] { typeof(string) }));
                    gen.ThrowException(exType);
                }
                else
                {
                    gen.Emit(OpCodes.Newobj, meta.type.GetConstructor(Type.EmptyTypes));
                    //if (meta.collection_props != null)
                    //{
                    //    foreach (var p in meta.collection_props)
                    //        EmitInitCollectionProp(gen, p);
                    //}
                }
            }

            public static void EmitNew(ILGenerator gen, type_meta meta, type_meta_base impl)
            {

                if (impl != null)
                {
                    gen.Emit(OpCodes.Newobj, impl.type.GetConstructor(Type.EmptyTypes)); // [ items, items, item ]
                    //if (impl.collection_props != null)
                    //{
                    //    foreach (var p in impl.collection_props)
                    //        EmitInitCollectionProp(gen, p);
                    //}
                }
                else
                {
                    EmitNewOrThrow(gen, meta);
                }
            }

            public void EmitNewObject(ILGenerator gen, type_meta meta)
            {
                //var listAddMethod = typeof(List<>).MakeGenericType(new Type[] { meta.type }).GetMethod("Add");
                
                if (meta.descriminator_prop != null)
                {
                    gen.Emit(OpCodes.Ldarg_0); // [ items, items, reader ]
                    gen.Emit(OpCodes.Ldc_I4, meta.descriminator_prop.ordinal);// [items, items, reader, i_ordinal]
                    var readerMethod = ReaderMethodFromType(typeof(int));
                    gen.Emit(readerMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, readerMethod); // [items, items, descriminatorValue]

                    LocalBuilder descriminatorLocal = meta.descriminator_prop.loc = gen.DeclareLocal(typeof(int));
                    gen.Emit(OpCodes.Stloc, descriminatorLocal); // [items, items]
                    gen.Emit(OpCodes.Ldloc, descriminatorLocal); // [items, items, descriminatorValue]


                    int types_count = meta.implementations.Length;
                    var switchTable = new Label[types_count];
                    for (int i = 0; i < types_count; i++)
                        switchTable[i] = gen.DefineLabel();

                    Label endNewObjLabel = gen.DefineLabel();

                    gen.Emit(OpCodes.Switch, switchTable); // [items, items]

                    // Default switch case
                    EmitNewOrGetFromCache(gen, meta, null);
                    gen.Emit(OpCodes.Br, endNewObjLabel);

                    for (int i = 0; i < types_count; i++)
                    {
                        // case: =>
                        gen.MarkLabel(switchTable[i]);
                        var impl = meta.implementations[i];

                        EmitNewOrGetFromCache(gen, meta, impl); // [ items, items, item ]
                        gen.Emit(OpCodes.Br, endNewObjLabel);
                    }

                    gen.MarkLabel(endNewObjLabel);
                }
                else
                {
                    EmitNewOrGetFromCache(gen, meta, null); // [ items, items, item ]
                }
            }

            public void EmitSetImplementationProp(ILGenerator gen, type_meta meta)
            {
                if (meta.implementations == null || meta.descriminator_prop == null) return;

                var descriminatorLocal = meta.descriminator_prop.loc;

                int types_count = meta.implementations.Length;
                var switchTable = new Label[types_count];
                for (int i = 0; i < types_count; i++)
                    switchTable[i] = gen.DefineLabel();

                Label endNewObjLabel = gen.DefineLabel();

                gen.Emit(OpCodes.Ldloc, descriminatorLocal); // [items, items, item, descriminatorValue]
                gen.Emit(OpCodes.Switch, switchTable);       // switch(descriminatorValue) { [items, items, item]
                gen.Emit(OpCodes.Br, endNewObjLabel); // default
                for (int i = 0; i < types_count; i++)
                {
                    gen.MarkLabel(switchTable[i]); // case [descriminator] :
                    
                    var impl = meta.implementations[i];
                    if (impl != null)
                    {
                        gen.Emit(OpCodes.Castclass, impl.type);

                        impl.props?.Apply(p => EmitSimpleProp(gen, p));
                        impl.obj_props?.Apply(p => EmitEntityProp(gen, p, this));
                    }

                    gen.Emit(OpCodes.Br, endNewObjLabel); // break;
                }

                gen.MarkLabel(endNewObjLabel); // } switch end

            }

            public void EmitNewOrGetFromCache(ILGenerator gen, type_meta meta, type_meta_base impl) // [...] => [..., item]
            {
                if (!HasCache(meta))
                {
                    EmitNew(gen, meta, impl); // => [..., item]
                    return;
                }

                Locals locals = _locals_cache[meta.type];
                {
                    var cacheType = locals.cache.LocalType;
                    var idType = GetMemberType(meta.id_prop.prop);
                    // There is the Cache for this type => try get object from cache
                    gen.Emit(OpCodes.Ldloc, locals.cache); // => [..., cache]
                    gen.Emit(OpCodes.Ldarg_0); // => [ ..., cache, reader ]
                    gen.Emit(OpCodes.Ldc_I4, meta.id_prop.ordinal); // => [ ..., cache, reader, i_ordinal]
                    var readerMethod = ReaderMethodFromType(idType);
                    gen.Emit(readerMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, readerMethod); // => [ ..., cache, id]
                    gen.Emit(OpCodes.Ldloca, locals.obj); // => [ ..., cache, id, item]
                    gen.Emit(OpCodes.Callvirt, cacheType.GetMethod("TryGetValue")); // => [ ..., is_item_in_cache(bool)]
                    
                    var isInCache = gen.DefineLabel();
                    gen.Emit(OpCodes.Brtrue, isInCache); // if in cache => skip object creation
                    
                    EmitNew(gen, meta, impl); // create new object => [..., item]
                    
                    // Add created object to cache
                    gen.Emit(OpCodes.Stloc, locals.obj); // => [...]
                    EmitAddToCache(gen, locals, meta.id_prop);
                    // => [...]
                    


                    gen.MarkLabel(isInCache);
                    gen.Emit(OpCodes.Ldloc, locals.obj); // => [..., item]
                }

            }

            static void EmitAddToCache(ILGenerator gen, Locals locals, prop_meta id_prop)
            {
                // => [ ... ]

                gen.Emit(OpCodes.Ldloc, locals.cache); // => [..., cache]

                gen.Emit(OpCodes.Ldarg_0); // => [ ..., cache, reader ]
                gen.Emit(OpCodes.Ldc_I4, id_prop.ordinal); // => [ ..., cache, reader, i_ordinal]
                var readerMethod = ReaderMethodFromType(GetMemberType(id_prop.prop));
                gen.Emit(readerMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, readerMethod); // => [ ..., cache, id]
                gen.Emit(OpCodes.Ldloc, locals.obj); // => [ ..., cache, id, item]
                gen.Emit(OpCodes.Callvirt, locals.cache.LocalType.GetMethod("Add")); // => [ ... ]
            }

            //internal static void EmitInitCollectionProp(ILGenerator gen, obj_prop_meta prop)
            //{
            //    // [..., item]

            //    gen.Emit(OpCodes.Dup); // [..., item, item]

            //    var listType = typeof(List<>).MakeGenericType(new Type[] { prop.type });

            //    gen.Emit(OpCodes.Newobj, listType.GetConstructor(Type.EmptyTypes)); // [..., item, item, collection]
            //    gen.Emit(OpCodes.Callvirt, prop.prop.GetSetMethod());

            //    // [..., item]
            //}

            public bool HasCache(type_meta_base meta)
            {
                if (_locals_cache == null) return false;
                return _locals_cache.ContainsKey(meta.type);
            }

#if TRACE_DB_EMIT
            public void EmitTrace(ILGenerator gen)
            {
                if (_locals_cache == null) return;

                gen.EmitWriteLine("---------------------");
                foreach(var type in _locals_cache.Keys)
                {
                    gen.EmitWriteLine(type.FullName);
                    gen.EmitWriteLine(_locals_cache[type].obj);
                    gen.EmitWriteLine(_locals_cache[type].cache);
                }
                gen.EmitWriteLine("---------------------");
            }

            Stack<string> _stackTrace = new Stack<string>();
            public void StackTracePush(string value, string comment = null)
            {
                _stackTrace.Push(value);
                PrintStackTrace(comment);
            }
            public void StackTracePop(int count, bool print = false, string comment = null)
            {
                while (count-- > 0) _stackTrace.Pop();
                if(print) PrintStackTrace(comment);
            }

            public void StackTraceDup(string comment = null)
            {
                _stackTrace.Push(_stackTrace.Peek());
                PrintStackTrace(comment);
            }

            private void PrintStackTrace(string comment)
            {
                var trace = "[" + String.Join(",", _stackTrace.Select(s => s).Reverse()) + "]" + (comment != null ? (" // " + comment) : "");
                System.Diagnostics.Trace.TraceInformation(trace);
            }

#endif
            class Locals
            {
                public LocalBuilder cache;
                public LocalBuilder obj;
            }
        }

        static Type GetMemberType(MemberInfo member)
        {
            if (member is PropertyInfo propertyInfo)
                return propertyInfo.PropertyType;
            else
                return ((FieldInfo)member).FieldType;
        }

        public static MethodInfo GetCollectionPropAddMethod(Type propType)
        {
            var addMethodInfo = propType.GetMethod("Add");

            if(addMethodInfo == null && propType.IsInterface)
            {
                var baseInterfaces = propType.GetInterfaces();
                foreach (var interfaceType in baseInterfaces)
                {
                    addMethodInfo = interfaceType.GetMethod("Add");
                    if (addMethodInfo != null)
                        break;
                }
            }

            if (addMethodInfo == null)
                throw new NotSupportedException($"{propType} type cannot be used for collection property. Cannot find Add(T item) method.");

            return addMethodInfo;
        }

        public static Func<DbDataReader, T> EmitValueFactory<T>(string prefix, bool emitRead)
        {
            var method = EmitValueFactory(typeof(T), prefix, emitRead);
            return method.CreateDelegate(typeof(Func<DbDataReader, T>)) as Func<DbDataReader, T>;
        }

        public static DynamicMethod EmitValueFactory(Type type, string prefix, bool emitRead) 
        {
            var method = new DynamicMethod(prefix + type.Name,
               type, new[] { typeof(DbDataReader) }, true);

            var readerMethod = Emit.DbContextFactoryEmiter.ReaderMethodFromType(Helper.GetNonNullableType(type));

            var gen = method.GetILGenerator();

            var retDefaultLabel = gen.DefineLabel();
            var isDbNullMethod = typeof(DbDataReader).GetMethod("IsDBNull", Emit.DbContextFactoryEmiter.ReaderArgTypes);

            if (emitRead)
            {
                gen.Emit(OpCodes.Ldarg_0); // [ reader ]
                gen.Emit(OpCodes.Callvirt, typeof(DbDataReader).GetMethod("Read", Type.EmptyTypes)); // [ reader.Read() ]
                gen.Emit(OpCodes.Brfalse, retDefaultLabel); // [if(!) => ]
            }

            gen.Emit(OpCodes.Ldarg_0);                  // [reader]
            gen.Emit(OpCodes.Ldc_I4_0);                 // [reader, ordinal: 0]
            gen.Emit(OpCodes.Callvirt, isDbNullMethod); // [reader.isDbNull(0)]
            gen.Emit(OpCodes.Brtrue, retDefaultLabel);  // []

            gen.Emit(OpCodes.Ldarg_0);  // [ reader ]
            gen.Emit(OpCodes.Ldc_I4_0); // [ reader, ordinal:0]
            gen.Emit(readerMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, readerMethod); // [value]

            Type originalType = type;
            if (type.IsValueType)
            {
                type = Helper.GetNonNullableType(type);
                Emit.EmitHelper.EmitNullableCtor(gen, originalType); // [value] => [new Nullable<T>(value)]
            }

            gen.Emit(OpCodes.Ret);

            gen.MarkLabel(retDefaultLabel);

            EmitHelper.EmitDefault(gen, originalType);

            /*
            if (!type.IsValueType)
            {
                gen.Emit(OpCodes.Ldnull);
            }
            else
            {   
                var typeCode = Type.GetTypeCode(type);
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                    case TypeCode.Int32:
                        gen.Emit(OpCodes.Ldc_I4_0);
                        break;
                    case TypeCode.Int16:
                        gen.Emit(OpCodes.Ldc_I4_0);
                        gen.Emit(OpCodes.Conv_I2);
                        break;
                    case TypeCode.Int64:
                        gen.Emit(OpCodes.Ldc_I8, 0);
                        break;
                    case TypeCode.Single:
                        gen.Emit(OpCodes.Ldc_R4, 0);
                        break;
                    case TypeCode.Double:
                        gen.Emit(OpCodes.Ldc_R8, 0);
                        break;
                    case TypeCode.Object:
                        if(type == typeof(Guid))
                        {
                            var emptyFld = (typeof(Guid).GetField("Empty", BindingFlags.Public | BindingFlags.Static));
                            gen.Emit(OpCodes.Ldsfld, emptyFld);
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Not supported: {type.Name}");
                }

                Emit.DbContextFactoryEmiter.EmitNullableCtor(gen, originalType); // [default(T)] => [new Nallable<T>(default(T))]
            }
            */

            gen.Emit(OpCodes.Ret);

            return method;
        }

    }

    /*
    [System.Diagnostics.DebuggerDisplay("Count = {Count}")]
    class ListSet<T> : IList<T>
    {
        private List<T> _list = new List<T>();
        private HashSet<T> _set = new HashSet<T>();

        public ListSet()
        {
            List<T> _list = new List<T>();
        }

        public T this[int index] { get => _list[index]; set => _list[index] = value; }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            if (_set.Add(item))
                _list.Add(item);

            //if (typeof(T).Name == "ServAgr")
            //{
            //    Console.Write("List.Add:" + item.ToString());
            //    Console.WriteLine("[" + String.Join(",", _list.Select(x => x.ToString())) + "]");
            //}
        }

        public void Clear()
        {
            _list.Clear();
            _set.Clear();
        }

        public bool Contains(T item)
        {
            return _set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
        }

        public bool Remove(T item)
        {
            if (_set.Remove(item))
                return _list.Remove(item);

            return false;
        }

        public void RemoveAt(int index)
        {
            var item = _list[index];
            _list.RemoveAt(index);
            _set.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }*/
}
