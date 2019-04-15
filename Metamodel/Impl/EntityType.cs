using RC.DBA.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RC.DBA.Metamodel.Impl
{
    class EntityType : IEntityType
    {
        protected ModelManager _manager;
        protected IEntityAttribute _key;
        protected IDictionary<string, IEntityAttribute> _attributes;

        //IEntityConfiguration _EntityConfig;

        protected EntityTable _table;
        protected IEntityType _baseEntityType;

        protected Type _classType;

        protected int _descriminator;

        public Type ClassType => _classType;

        //public IEntityConfiguration EntityConfig => _EntityConfig;

        public IEnumerable<IEntityAttribute> Attributes => _attributes.Values;

        public EntityTable Table => _table;

        public EntityTable EnityTable => _table ?? ParentTable;
        public bool IsAbstract => _classType.IsAbstract;

        public IEntityType BaseEntityType => _baseEntityType;

        public int Descriminator => _descriminator;

        public IModelManager Manager => _manager;

        public EntityType(Type type, ModelManager modelManager, ref int tableIndx)
        {
            _classType = type;
            _manager = modelManager;

            //var config = modelManager.EntityConfig(type);
            //var configType = typeof(EntityConfigurationImpl<>).MakeGenericType(type);
            //var config = _EntityConfig = (IEntityConfiguration)Activator.CreateInstance(configType);

            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            _attributes = new Dictionary<string, IEntityAttribute>();
            for (int i = 0; i < props.Length; i++)
            {
                var attr = new EntityAttribute(props[i], this);
                _attributes.Add(attr.Name, attr);
            }
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            for (int i = 0; i < fields.Length; i++)
            {
                var attr = new EntityAttribute(fields[i], this);
                _attributes.Add(attr.Name, attr);
            }

            //EntityTable parentTable;
            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                _baseEntityType = modelManager.Entity(type.BaseType);
                //parentTable = _baseEntityType.Table;
            }
            //else
            //    parentTable = null;

            var tableAttr = type.GetCustomAttribute<TableAttribute>(false);
            if (tableAttr != null)
            {
                _table = new EntityTable(tableAttr.Name ?? type.Name, 
                    tableAttr.Schema, this, 
                    System.Threading.Interlocked.Increment(ref tableIndx));
            }
            //else if(parentTable != null)
            //{
            //    _table = parentTable;
            //}

            var descAttr = type.GetCustomAttribute<DescriminatorValueAttribute>();
            _descriminator = descAttr != null ? descAttr.Value : -1;
            
            // There are no Table mapping in base Entity => base class is not Entity!
            if (_baseEntityType != null && _baseEntityType.EnityTable == null)
            {
                // move all attributes from base Entity to the current
                foreach (var a in _baseEntityType.AllAttributes)
                {
                    var attr = new EntityAttribute(a.Member, this);
                    _attributes.Add(attr.Name, attr);
                }
                _baseEntityType = null; // reset base Entity
            }
            

            //_Implementations = type.Assembly.GetTypes()
            //    .Where(t => t.IsClass && !t.IsAbstract && type.IsAssignableFrom(t) && t.GetCustomAttribute<DescriminatorValueAttribute>() != null)
            //    .Select(t => modelManager.Entity(t))
            //    .ToArray();

            var key = AllAttributes.FirstOrDefault(x => x.IsKey);
            if (key != null && key.EntityType != this)
            {
                key = new EntityAttribute(key.Member, this);
                key.DbGeneratedOption = DatabaseGeneratedOption.None; // It's depended from parent table key field !!!
                
                _attributes.Add(key.Name, key);
            }
            _key = key;

        }

        public IEntityAttribute GetAttribute(string name)
        {
            IEntityAttribute attr;
            if (_attributes.TryGetValue(name, out attr))
                return attr;
            else if (_baseEntityType != null)
                return _baseEntityType.GetAttribute(name);

            return null;
        }

        public IEnumerable<IEntityAttribute> GetAttributes(string name)
        {
            IEntityAttribute attr;
            if (_attributes.TryGetValue(name, out attr))
                yield return attr;
            if (_baseEntityType == null) yield break;

            foreach (var a in _baseEntityType.GetAttributes(name))
                yield return a;
        }

        public void SetTable(string name, string schema)
        {
            throw new NotImplementedException();
        }

        public void SetDescriminator(int value)
        {
            _descriminator = value;
        }

        public string GetPropertyColumn(string property)
        {
            throw new NotImplementedException();
        }

        public void SetPropertyColumn(string property, string column)
        {
            throw new NotImplementedException();
        }

        public string GetInverseProperty(string property)
        {
            throw new NotImplementedException();
        }

        public void SetInverseProperty(string property, string inverseProperty)
        {
            throw new NotImplementedException();
        }

        public string GetForeignKey(string property)
        {
            throw new NotImplementedException();
        }

        public void SetForeignKey(string property, string foreignKey)
        {
            throw new NotImplementedException();
        }

        public void SetKey(string property)
        {
            var newKeyAttr = GetAttribute(property);
            if (newKeyAttr == null)
                throw new ArgumentException($" Entiy {Name} SetKey: Cannot find attribute name {property}");

            newKeyAttr.IsKey = true;
            _key.IsKey = false;
            _key = newKeyAttr;
        }

        public bool IsKey(string property)
        {
            return Key.Name == property;
        }

        public void SetNotMapped(string property)
        {
            throw new NotImplementedException();
        }

        public bool IsNotMapped(string property)
        {
            throw new NotImplementedException();
        }

        public DatabaseGeneratedOption GetDbGeneratedOption(string property)
        {
            throw new NotImplementedException();
        }

        public void SetDbGeneratedOption(string property, DatabaseGeneratedOption opt)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IEntityAttribute> AllAttributes
        {
            get
            {
                foreach (var a in _attributes.Values)
                    yield return a;


                var baseType = _baseEntityType;
                while (baseType != null)
                {
                    foreach (var a in baseType.AllAttributes)
                        yield return a;

                    baseType = baseType.BaseEntityType;
                }
            }
        }

        public IEntityAttribute Key => _key;

        public EntityTable ParentTable
        {
            get
            {
                EntityTable table;
                IEntityType entity = _baseEntityType;
                while (entity != null)
                {
                    table = entity.Table;
                    if (table != null) return table;
                    entity = entity.BaseEntityType;
                }

                return null;
            }
        }

        public IEnumerable<IEntityType> Implementations => _manager.GetImplementations(_classType);

        public string Name => _classType.Name;
    }

    class EntityType<T> : EntityType, IEntityType<T>
    {
        internal EntityType(ModelManager modelManager, ref int tableIndx) : base(typeof(T), modelManager, ref tableIndx) { }

        public DatabaseGeneratedOption GetDbGeneratedOption<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property)
        {
            throw new NotImplementedException();
        }

        public string GetForeignKey<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property)
        {
            throw new NotImplementedException();
        }

        public string GetInverseProperty<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property)
        {
            throw new NotImplementedException();
        }

        public string GetPropertyColumn<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property)
        {
            throw new NotImplementedException();
        }

        public bool IsKey<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property)
        {
            throw new NotImplementedException();
        }

        public bool IsNotMapped<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property)
        {
            throw new NotImplementedException();
        }

        public void SetDbGeneratedOption<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property, DatabaseGeneratedOption opt)
        {
            throw new NotImplementedException();
        }

        public IEntityType<T> SetForeignKey<TProp, TFKProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property, System.Linq.Expressions.Expression<Func<T, TFKProp>> foreignKey)
        {
            throw new NotImplementedException();
        }

        public IEntityType<T> SetInverseProperty<TProp, TInvProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property, System.Linq.Expressions.Expression<Func<TProp, TInvProp>> inverseProperty)
        {
            throw new NotImplementedException();
        }

        public IEntityType<T> SetKey<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property)
        {
            throw new NotImplementedException();
        }

        public IEntityType<T> SetNotMapped<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property)
        {
            throw new NotImplementedException();
        }

        public IEntityType<T> SetPropertyColumn<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property, string column)
        {
            throw new NotImplementedException();
        }
    }
}
