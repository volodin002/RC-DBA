using RC.DBA.Metamodel;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace RC.DBA
{
    /*
    class ContextManagerImpl : IContextManager
    {
        private Dictionary<Type, object> _typeFactortyCache = new Dictionary<Type, object>();
        private Dictionary<Type, object> _valueFactortyCache = new Dictionary<Type, object>();
        private Dictionary<Type, object> _readValueFactortyCache = new Dictionary<Type, object>();
        private IModelManager _ModelManager = new Metamodel.Impl.ModelManager();

        public IModelManager ModelManager => _ModelManager;

        public Func<DbDataReader, IList<T>> GetResultListFactory<T>(DbDataReader reader, object cacheKey)
        {
            object val;
            Dictionary<object, Func<DbDataReader, IList<T>>> typeFactoryCache;
            if (_typeFactortyCache.TryGetValue(typeof(T), out val))
            {
                typeFactoryCache = (Dictionary<object, Func<DbDataReader, IList<T>>>)val;
            }
            else
            {
                _typeFactortyCache[typeof(T)] = typeFactoryCache =
                    new Dictionary<object, Func<DbDataReader, IList<T>>>();
            }

            Func<DbDataReader, IList<T>> factory;
            if (!typeFactoryCache.TryGetValue(cacheKey, out factory))
            {
                factory = EmitResultListFactory<T>(reader, cacheKey, _ModelManager);
                typeFactoryCache.Add(cacheKey, factory);
            }

            return factory;
        }

        static Func<DbDataReader, IList<T>> EmitResultListFactory<T>(DbDataReader reader, object cacheKey, IModelManager modelManager)
        {
            return Emit.DbContextFactoryEmiter.EmitResultListFactory<T>(reader, cacheKey, modelManager);
        }

        static Func<DbDataReader, T> EmitValueFactory<T>(string prefix, bool emitRead)
        {
            var method = new DynamicMethod(prefix + typeof(T).Name,
               typeof(T), new[] { typeof(DbDataReader) }, true);
            var readerMethod = Emit.DbContextFactoryEmiter.ReaderMethodFromType(Helper.GetNonNullableType(typeof(T)),
                Emit.DbContextFactoryEmiter.ReaderArgTypes);

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
            gen.Emit(OpCodes.Callvirt, readerMethod); // [value]

            gen.Emit(OpCodes.Ret);

            gen.MarkLabel(retDefaultLabel);

            if (!typeof(T).IsValueType)
            {
                gen.Emit(OpCodes.Ldnull);
            }
            else
            {
                var typeCode = Type.GetTypeCode(typeof(T));
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
                    default:
                        throw new NotSupportedException("Not supported: " + typeof(T).Name);
                }

                Emit.DbContextFactoryEmiter.EmitNullableCtor(gen, typeof(T));
            }

            gen.Emit(OpCodes.Ret);

            return method.CreateDelegate(typeof(Func<DbDataReader, T>)) as Func<DbDataReader, T>;
        }

        public Func<DbDataReader, T> GetReadValueFactory<T>()
        {
            object f;
            Func<DbDataReader, T> factory;
            if (!_readValueFactortyCache.TryGetValue(typeof(T), out f))
            {
                factory = EmitValueFactory<T>("_$ReadValueFactory_", true);
                _readValueFactortyCache.Add(typeof(T), factory);
            }
            else
                factory = (Func<DbDataReader, T>)f;

            return factory;

        }

        public Func<DbDataReader, T> GetValueFactory<T>()
        {
            object f;
            Func<DbDataReader, T> factory;
            if (!_valueFactortyCache.TryGetValue(typeof(T), out f))
            {
                factory = EmitValueFactory<T>("_$ValueFactory_", false);
                _valueFactortyCache.Add(typeof(T), factory);
            }
            else
                factory = (Func<DbDataReader, T>)f;

            return factory;

        }
    }
    */
}
