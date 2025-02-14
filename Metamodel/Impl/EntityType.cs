using RC.DBA.Attributes;
using RC.DBA.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace RC.DBA.Metamodel.Impl
{
    class EntityType : IEntityType
    {
        protected ModelManager _manager;
        protected IEntityAttribute _key;
        protected IDictionary<string, IEntityAttribute> _attributes;

        protected HashMap<string, MethodInfo> _attributeConverters;
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

        protected EntityType(Type type, ModelManager modelManager, ref int tableIndx)
        {
            Init(type, modelManager, ref tableIndx);
        }

        protected EntityType() { }

        protected void Init(Type type, ModelManager modelManager, ref int tableIndx)
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

        public static EntityType Create(Type type, ModelManager modelManager, ref int tableIndx)
        {
            var entityTypeType = typeof(EntityType<>).MakeGenericType(type);
            var entityType = (EntityType)Activator.CreateInstance(entityTypeType, true);
            entityType.Init(type, modelManager, ref tableIndx);

            return entityType;
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

            _table = new EntityTable(name, schema, this,
                System.Threading.Interlocked.Increment(ref _manager.GetTableIndex()));

        }

        public void SetDescriminator(int value)
        {
            _descriminator = value;
        }

        public string GetPropertyColumn(string property)
        {
            return GetAttribute(property)?.Column;
        }

        public void SetPropertyColumn(string property, string column)
        {
            var attr = GetAttribute(property);
            attr.Column = column;
        }

        public string GetInverseProperty(string property)
        {
            throw new NotImplementedException();
        }

        public void SetInverseProperty(string property, string inverseProperty)
        {
            throw new NotImplementedException();
        }

        public IEntityAttribute GetForeignKey(string property)
        {
            return GetAttribute(property)?.ForeignKey;
        }

        public void SetForeignKey(string property, string foreignKey)
        {
            var attr = GetAttribute(property);
            if (!attr.IsAssociation) return;

            if (attr.IsCollection)
            {
                var foreignKeyEntity = Manager.Entity(attr.MemberType);
                attr.ForeignKey = foreignKeyEntity.GetAttribute(foreignKey);
            }
            else
            {
                attr.ForeignKey = GetAttribute(foreignKey);
            }
        }

        public void SetForeignKey(string property, IEntityAttribute foreignKey)
        {
            var attr = GetAttribute(property);
            attr.ForeignKey = foreignKey;
        }

        public void SetKey(string property)
        {
            var newKeyAttr = GetAttribute(property);
            if (newKeyAttr == null)
                throw new ArgumentException($" Entiy {Name} SetKey: Cannot find attribute name {property}");

            newKeyAttr.IsKey = true;
            if (_key != null) _key.IsKey = false;
            _key = newKeyAttr;
        }

        public bool IsKey(string property)
        {
            return Key?.Name == property;
        }

        public void SetNotMapped(string property)
        {
            GetAttribute(property).IsNotMapped = true;
        }

        public bool IsNotMapped(string property)
        {
            return GetAttribute(property)?.IsNotMapped ?? true;
        }

        public DatabaseGeneratedOption GetDbGeneratedOption(string property)
        {
            return GetAttribute(property).DbGeneratedOption;
        }

        public void SetDbGeneratedOption(string property, DatabaseGeneratedOption opt)
        {
            GetAttribute(property).DbGeneratedOption = opt;
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

        public MethodInfo GetAttributeConvertMethod(string attributeName)
        {
            MethodInfo method = null;
            if (_attributeConverters != null)
                _attributeConverters.TryGetValue(attributeName, ref method);

            return method;
        }

        public void SetPropConvertMethod(string property, MethodInfo method)
        {
            if (_attributeConverters == null)
                _attributeConverters = new HashMap<string, MethodInfo>();

            if (!method.IsStatic)
                throw new ArgumentException($"Parameter methodExpression must be a static function call expression. Method [{method.Name}] is not static.");


            _attributeConverters.Add(property, method);
        }
    }

    class EntityType<T> : EntityType, IEntityType<T>
    {
        protected EntityType() { }

        internal EntityType(ModelManager modelManager, ref int tableIndx) : base(typeof(T), modelManager, ref tableIndx) { }

        public DatabaseGeneratedOption GetDbGeneratedOption<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property)
        {
            var memberInfo = Helper.Member(property);
            return GetDbGeneratedOption(memberInfo.Name);
        }

        public IEntityAttribute GetForeignKey<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property) 
        {
            var memberInfo = Helper.Member(property);
            return GetForeignKey(memberInfo.Name);
        }

        public string GetInverseProperty<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property)
        {
            throw new NotImplementedException();
        }

        public string GetPropertyColumn<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property)
        {
            var memberInfo = Helper.Member(property);
            return GetAttribute(memberInfo.Name).Column;
        }

        public bool IsKey<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property)
        {
            var memberInfo = Helper.Member(property);
            return GetAttribute(memberInfo.Name).IsKey;
        }

        public bool IsNotMapped<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property)
        {
            var memberInfo = Helper.Member(property);
            return GetAttribute(memberInfo.Name).IsNotMapped;
        }

        public IEntityType<T> SetDbGeneratedOption<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property, DatabaseGeneratedOption opt)
        {
            var memberInfo = Helper.Member(property);
            SetDbGeneratedOption(memberInfo.Name, opt);

            return this;
        }

        public IEntityType<T> SetForeignKey<TProp, TFKProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property, System.Linq.Expressions.Expression<Func<T, TFKProp>> foreignKey)
        {
            var memberInfo = Helper.Member(property);
            SetForeignKey(memberInfo.Name, Helper.Member(foreignKey).Name);

            return this;
        }

        public IEntityType<T> SetCollectionForeignKey<TProp, TFKProp>(Expression<Func<T, IEnumerable<TProp>>> property, Expression<Func<TProp, TFKProp>> foreignKey)
        {
            var memberInfo = Helper.Member(property);
            SetForeignKey(memberInfo.Name, Helper.Member(foreignKey).Name);

            return this;
        }

        public IEntityType<T> SetInverseProperty<TProp, TInvProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property, System.Linq.Expressions.Expression<Func<TProp, TInvProp>> inverseProperty)
        {
            throw new NotImplementedException();
        }

        public IEntityType<T> SetKey<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property)
        {
            var memberInfo = Helper.Member(property);
            SetKey(memberInfo.Name);

            return this;
        }

        public IEntityType<T> SetKey<TProp>(Expression<Func<T, TProp>> property, string keySequenceName)
        {
            var memberInfo = Helper.Member(property);
            var newKeyAttr = GetAttribute(memberInfo.Name);
            if (newKeyAttr == null)
                throw new ArgumentException($" Entiy {Name} SetKey: Cannot find attribute name {property}");

            newKeyAttr.IsKey = true;
            if (_key != null) _key.IsKey = false;
            _key = newKeyAttr;

            if (keySequenceName != "")
                newKeyAttr.KeySequenceName = keySequenceName;

            return this;
        }

        public IEntityType<T> SetNotMapped<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property)
        {
            var memberInfo = Helper.Member(property);
            SetNotMapped(memberInfo.Name);

            return this;
        }

        public IEntityType<T> SetPropertyColumn<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property, string column)
        {
            var memberInfo = Helper.Member(property);
            SetPropertyColumn(memberInfo.Name, column);

            return this;
        }

        public IEntityType<T> SetMaxLength<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property, int maxLength)
        {
            var memberInfo = Helper.Member(property);
            var attr = GetAttribute(memberInfo.Name);
            attr.MaxLength = maxLength;

            return this;
        }

        public int GetMaxLength<TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property)
        {
            var memberInfo = Helper.Member(property);
            var attr = GetAttribute(memberInfo.Name);
            return attr.MaxLength;
        }

        public IEntityType<T> SetPropConvertMethod<TDb, TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property, Expression<Func<TDb, TProp>> methodExpression)
        {
            var memberInfo = Helper.Member(property);
            string attrName = memberInfo.Name;

            if (_attributeConverters == null)
                _attributeConverters = new HashMap<string, MethodInfo>();

            var methodInfo = Helper.Method<TDb, TProp>(methodExpression);
            if (methodInfo == null)
                throw new ArgumentException($"Parameter methodExpression must be a static function call expression. Canntod find method call in expression: {methodExpression}");
            if (!methodInfo.IsStatic)
                throw new ArgumentException($"Parameter methodExpression must be a static function call expression. Method [{methodInfo.Name}] is not static.");


            _attributeConverters.Add(attrName, methodInfo);

            return this;
        }


        public IEntityType<T> SetTableForType(string name, string schema)
        {
            SetTable(name, schema);

            return this;
        }
    }
}
