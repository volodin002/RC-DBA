using RC.DBA.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace RC.DBA.Metamodel.Impl
{
    public class ModelManager : IModelManager
    {
        private ConcurrentDictionary<Type, IEntityType> _typeEntityCache = new ConcurrentDictionary<Type, IEntityType>();
        private ConcurrentDictionary<Type, Delegate> _valueFactortyCache = new ConcurrentDictionary<Type, Delegate>();
        private ConcurrentDictionary<Type, Delegate> _readValueFactortyCache = new ConcurrentDictionary<Type, Delegate>();

        int tableIndx;

        public ModelManager() { }
        public ModelManager(IEnumerable<Type> types)
        {
            foreach (var type in types)
                Entity(type);
        }

        public ModelManager(params Type[] types)
        {
            foreach (var type in types)
                Entity(type);
        }

        public ref int GetTableIndex() => ref tableIndx;

        public IEntityType<T> Entity<T>()
        {
            //return Entity(typeof(T));

            return (IEntityType<T>)_typeEntityCache.GetOrAdd(typeof(T),
                t => new EntityType<T>(this, ref tableIndx));
        }

        public IEntityType Entity(Type type)
        {
            //IEntityType res;
            //if (!_typeEntityCache.TryGetValue(type, out res))
            //{
            //    _typeEntityCache[type] = res = new EntityType(type, this, tableIndx++);
            //}
            //return res;

            // Thread-safe way to get (or create if not exists) EntityType from cache
            return _typeEntityCache.GetOrAdd(type, 
                t => EntityType.Create(t, this, ref tableIndx));
        }

        //public IEntityConfiguration<T> EntityConfig<T>()
        //{
        //    return (IEntityConfiguration<T>)(EntityConfig(typeof(T)));
        //}

        //public IEntityConfiguration EntityConfig(Type type)
        //{
        //    return Entity(type).EntityConfig;
        //}

        public IEnumerable<IEntityType> GetImplementations(Type type)
        {
            return GetDerivedEntities(type, true)
                .Where(e => e.Descriminator >= 0)
                .OrderBy(e => e.Descriminator);
                
            /*
            return type.Assembly.GetTypes()
                .Where(t =>
                    t.IsClass && //&& !t.IsAbstract
                    type.IsAssignableFrom(t) &&
                    EntityConfig(t).Descriminator >= 0)
                .Select(t => Entity(t))
                .OrderBy(e => e.Descriminator)
                .ToArray(); throw new NotImplementedException();
                */
        }


        private IEnumerable<IEntityType> GetDerivedEntities(Type type, bool withSelf)
        {
            var entity = Entity(type);
            if (withSelf) yield return entity;

            // Level-order traversal
            var queue = new Queue<IEntityType>();
            while (true)
            {
                foreach (var e in _typeEntityCache.Values.Where(e => e.BaseEntityType == entity))
                    queue.Enqueue(e);

                if (queue.Count == 0) break;
                entity = queue.Dequeue();

                yield return entity;
            }

            /*
            var stack = new Stack<IEntityType>(_entityCash.Values.Where(e => e.BaseEntity == entity));
           
            while (stack.TryPop(out var derivedEntity))
            {
                yield return derivedEntity;

                entity = derivedEntity;
                foreach(var e in _entityCash.Values.Where(e => e.BaseEntity == entity))
                    stack.Push(e);
            }
            */
        }

        public IEnumerable<IEntityType> GetImplementations<T>()
        {
            return GetImplementations(typeof(T));
        }

        public void ResetEntity(Type type)
        {
            _typeEntityCache.TryRemove(type, out var entityType);
        }

        public void ResetEntity<T>()
        {
            ResetEntity(typeof(T));
        }

        public Func<DbDataReader, T> GetReadValueFactory<T>()
        {

            return _readValueFactortyCache.GetOrAdd(typeof(T), t =>
                 Emit.DbContextFactoryEmiter
                     .EmitValueFactory(t, "_$ReadValueFactory_", true)
                     .CreateDelegate(typeof(Func<DbDataReader, T>)) 
             ) 
             as Func<DbDataReader, T>;
        }

        public Func<DbDataReader, T> GetValueFactory<T>()
        {
            return _valueFactortyCache.GetOrAdd(typeof(T), t =>
                 Emit.DbContextFactoryEmiter
                     .EmitValueFactory(t, "_$ValueFactory_", false)
                     .CreateDelegate(typeof(Func<DbDataReader, T>))
             )
             as Func<DbDataReader, T>;
        }
    }

    /*
    class EntityConfigurationImpl<T> : IEntityConfiguration<T>
    {
        private string _TableName;
        private string _TableSchema;
        private int _Descriminator;
        
        private ConcurrentDictionary<string, PropAttributes> _PropCache;

        private class PropAttributes
        {
            public string ForeignKey;
            public string InverseProp;
            public string Column;
            public bool IsKey;
            public bool IsNotMapped;
            public DatabaseGeneratedOption DbGeneratedOption;
        }

        public Type Type => typeof(T);

        public string TableName => _TableName;

        public string TableSchema => _TableSchema;

        public int Descriminator => _Descriminator;

        public EntityConfigurationImpl()
        {
            var tableAttr = typeof(T).GetCustomAttribute<TableAttribute>(false);
            if(tableAttr != null)
            {
                _TableName = tableAttr.Name;
                _TableSchema = tableAttr.Schema;
            }
            var descrAttr = typeof(T).GetCustomAttribute<DescriminatorValueAttribute>();
            _Descriminator = descrAttr != null ? descrAttr.Value : -1;

            _PropCache = new ConcurrentDictionary<string, PropAttributes>();
        }

        public void Table(string name)
        {
            _TableName = name;
        }

        public void Table(string name, string schema)
        {
            _TableName = name;
            _TableSchema = schema;
        }

        private void SetPropertyAttrValue(string property, Action<PropAttributes> setter)
        {
            //PropAttributes propAttr;
            //if (!_PropCache.TryGetValue(property, out propAttr))
            //{
            //    _PropCache[property] = propAttr = new PropAttributes();
            //}

            // Thread-safe way to get (or create if not exists) attribute config from cache
            var propAttr = _PropCache.GetOrAdd(property, x => new PropAttributes());

            setter(propAttr);
        }

        private TVal GetPropertyAttrValue<TVal>(string property, Func<PropAttributes, TVal> getter)
        {
            //PropAttributes propAttr;
            //if (!_PropCache.TryGetValue(property, out propAttr))
            //{
            //    _PropCache[property] = propAttr = new PropAttributes();
            //    InitDefaultPropAttributes(property, propAttr);
            //}
            var propAttr = _PropCache.GetOrAdd(property, 
                x => InitDefaultPropAttributes(x, new PropAttributes()));

            return getter(propAttr);
        }

        private static PropAttributes InitDefaultPropAttributes(string property, PropAttributes p)
        {
            var prop = typeof(T).GetProperty(property);

            p.IsKey = prop.GetCustomAttribute<KeyAttribute>() != null;

            var dbGenAttr = prop.GetCustomAttribute<DatabaseGeneratedAttribute>();
            p.DbGeneratedOption = dbGenAttr != null
                ? dbGenAttr.DatabaseGeneratedOption
                : DatabaseGeneratedOption.None;
            {
                var attr = prop.GetCustomAttribute<ColumnAttribute>();
                if (attr != null) p.Column = attr.Name;
            }
            
            p.IsNotMapped = prop.GetCustomAttribute<NotMappedAttribute>() != null;

            {
                var attr = prop.GetCustomAttribute<ForeignKeyAttribute>();
                if (attr != null) p.ForeignKey = attr.Name;
            }

            {
                var attr = prop.GetCustomAttribute<InversePropertyAttribute>();
                if (attr != null) p.InverseProp = attr.Property;
            }

            return p;
        }

        #region Column
        public IEntityConfiguration<T> SetPropertyColumn<TProp>(Expression<Func<T, TProp>> property, string column)
        {
            var propInfo = Helper.Property(property);
            SetPropertyColumn(propInfo.Name, column);

            return this;
        }

        public string GetPropertyColumn<TProp>(Expression<Func<T, TProp>> property)
        {
            var propInfo = Helper.Property(property);
            return GetPropertyColumn(propInfo.Name);
        }

        public string GetPropertyColumn(string property)
        {
            return GetPropertyAttrValue(property, p => p.Column);
        }

        public void SetPropertyColumn(string property, string column)
        {
            SetPropertyAttrValue(property, p => p.Column = column);
        }

        #endregion // Column

        #region Foreign Key
        public string GetForeignKey<TProp>(Expression<Func<T, TProp>> property)
        {
            var propInfo = Helper.Property(property);
            return GetForeignKey(propInfo.Name);
        }

        public string GetForeignKey(string property)
        {
            return GetPropertyAttrValue(property, p => p.ForeignKey);
        }

        public IEntityConfiguration<T> SetForeignKey<TProp, TFKProp>(Expression<Func<T, TProp>> property, Expression<Func<T, TFKProp>> foreignKey)
        {
            var propInfo = Helper.Property(property);
            var fkPropInfo = Helper.Property(foreignKey);
            SetForeignKey(propInfo.Name, fkPropInfo.Name);

            return this;
        }

        public void SetForeignKey(string property, string foreignKey)
        {
            SetPropertyAttrValue(property, p => p.ForeignKey = foreignKey);
        }

        #endregion // Foreign Key


        #region Inverse Property
        public string GetInverseProperty<TProp>(Expression<Func<T, TProp>> property)
        {
            var propInfo = Helper.Property(property);
            return GetInverseProperty(propInfo.Name);
        }

        public string GetInverseProperty(string property)
        {
            return GetPropertyAttrValue(property, p => p.InverseProp);
        }

        public IEntityConfiguration<T> SetInverseProperty<TProp, TInvProp>(Expression<Func<T, TProp>> property, Expression<Func<TProp, TInvProp>> inverseProperty)
        {
            var propInfo = Helper.Property(property);
            var invPropInfo = Helper.Property(inverseProperty);
            SetInverseProperty(propInfo.Name, invPropInfo.Name);

            return this;
        }

        public void SetInverseProperty(string property, string inverseProperty)
        {
            SetPropertyAttrValue(property, p => p.InverseProp = inverseProperty);
        }

        #endregion // Inverse Property

        #region Key
        public bool IsKey(string property)
        {
            return GetPropertyAttrValue(property, p => p.IsKey);
        }

        public void SetDescriminator(int value)
        {
            _Descriminator = value;
        }


        public IEntityConfiguration<T> SetKey<TProp>(Expression<Func<T, TProp>> property)
        {
            var propInfo = Helper.Property(property);
            SetKey(propInfo.Name);
            return this;
        }

        public void SetKey(string property)
        {
            SetPropertyAttrValue(property, p => p.IsKey = true);
        }

        #endregion // Key

        #region  Database Generated Option
        public DatabaseGeneratedOption GetDbGeneratedOption<TProp>(Expression<Func<T, TProp>> property)
        {
            var propInfo = Helper.Property(property);
            return GetDbGeneratedOption(propInfo.Name);
        }

        public void SetDbGeneratedOption<TProp>(Expression<Func<T, TProp>> property, DatabaseGeneratedOption opt)
        {
            var propInfo = Helper.Property(property);
            SetDbGeneratedOption(propInfo.Name, opt);
        }

        public DatabaseGeneratedOption GetDbGeneratedOption(string property)
        {
            return GetPropertyAttrValue(property, p => p.DbGeneratedOption);
        }

        public void SetDbGeneratedOption(string property, DatabaseGeneratedOption opt)
        {
            SetPropertyAttrValue(property, p => p.DbGeneratedOption = opt);
        }

        public bool IsKey<TProp>(Expression<Func<T, TProp>> property)
        {
            throw new NotImplementedException();
        }

        #endregion  //Database Generated Option

        #region NotMapped
        public IEntityConfiguration<T> SetNotMapped<TProp>(Expression<Func<T, TProp>> property)
        {
            var propInfo = Helper.Property(property);
            SetNotMapped(propInfo.Name);
            return this;
        }

        public bool IsNotMapped<TProp>(Expression<Func<T, TProp>> property)
        {
            var propInfo = Helper.Property(property);
            return IsNotMapped(propInfo.Name);
        }

        public void SetNotMapped(string property)
        {
            SetPropertyAttrValue(property, p => p.IsNotMapped = true);
        }

        public bool IsNotMapped(string property)
        {
            return GetPropertyAttrValue(property, p => p.IsNotMapped);
        }

        #endregion // NotMapped
    }
    */
}
