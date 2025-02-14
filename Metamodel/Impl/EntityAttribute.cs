using RC.DBA.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RC.DBA.Metamodel.Impl
{
    class EntityAttribute : IEntityAttribute
    {
        private readonly MemberInfo _member;
        private readonly Type _memberType;
        private readonly IEntityType _entityType;
        private string _column;
        private IEntityAttribute _foreignKey;
        
        private DatabaseGeneratedOption _dbGeneratedOption;
        private string _keySequenceName;
        private int _maxLength;

        protected int _flags;
        #region CONSTS
        private const Int32 _isRequired = 0x001;
        private const Int32 _isNullableType = 0x002;
        private const Int32 _isAssociation = 0x004;
        private const Int32 _isCollection = 0x008;
        private const Int32 _isNotMapped = 0x010;
        private const Int32 _isProperty = 0x020;
        private const Int32 _isKey = 0x080;
        #endregion // CONSTS

        public EntityAttribute(MemberInfo member, IEntityType entityType)
        {
            _member = member;

            Type type;
            var propInfo = (member as PropertyInfo);
            if (propInfo != null)
            {
                IsProperty = true;
                type = propInfo.PropertyType;
            }
            else
                type = ((FieldInfo)member).FieldType;

            IsCollection =
                type != typeof(string) && type != typeof(byte[]) &&
                typeof(System.Collections.IEnumerable).IsAssignableFrom(type);


            if (IsCollection)
            {
                var types = type.GetGenericArguments();
                if (types.Length != 1)
                    throw new NotSupportedException(type.FullName + " is not supported as entity property type");
                _memberType = types[0];

                IsAssociation = true;
            }
            else
            {
                _memberType = type;
            }

            if (!IsAssociation)
            {
                IsAssociation = Helper.IsAssociationPropertyType(type);
            }

            if (!IsAssociation)
            {
                _column = member.GetCustomAttribute<ColumnAttribute>()?.Name  ?? _member.Name;
                var cuastomConvertAttr = member.GetCustomAttribute<CovertMethodAttribute>();
                if (cuastomConvertAttr != null)
                {
                    entityType.SetPropConvertMethod(_member.Name, cuastomConvertAttr.ConvertMethod);
                }

                IsRequired =
                    Emit.DbContextFactoryEmiter.IsRequiredProp(member) ||
                    !Helper.IsTypeCanBeNull(type);

                IsKey = member.GetCustomAttribute<KeyAttribute>() != null;

                var dbGenAttr = member.GetCustomAttribute<DatabaseGeneratedAttribute>();
                _dbGeneratedOption = dbGenAttr != null
                    ? dbGenAttr.DatabaseGeneratedOption
                    : DatabaseGeneratedOption.None;

                if(IsKey)
                {
                    var dbSequenceAttr = member.GetCustomAttribute<SequenceAttribute>();
                    if (dbSequenceAttr != null)
                        _keySequenceName = dbSequenceAttr.SequenceName;
                }
            }

            IsNotMapped = member.GetCustomAttribute<NotMappedAttribute>() != null;

            var maxLengthAttr = member.GetCustomAttribute<MaxLengthAttribute>();
            if (maxLengthAttr != null)
                _maxLength = maxLengthAttr.Length;

            _entityType = entityType;
        }

        protected IEntityAttribute GetForeignKey()
        {
            if (!IsAssociation || IsCollection) return null;

            string name = _member.GetCustomAttribute<ForeignKeyAttribute>()?.Name ?? (Name + "Id");

            return _entityType.GetAttribute(name);
        }

        public IEntityAttribute GetCollectionForeignKey()
        {
            if (!IsAssociation || !IsCollection) return null;

            if (_foreignKey != null) return _foreignKey;

            var foreignKeyEntity = _entityType.Manager.Entity(MemberType);

            string name;
            var inversePropAttr = _member.GetCustomAttribute<InversePropertyAttribute>();
            if (inversePropAttr != null)
            {
                name = inversePropAttr.Property ?? Name;
                _foreignKey = foreignKeyEntity.GetAttribute(name).ForeignKey;
            }
            else
            {
                var foreignKeyAttr = _member.GetCustomAttribute<ForeignKeyAttribute>();
                if (foreignKeyAttr == null)
                    throw new Query.QueryException("Cannot find collection Foreign Key");

                name = foreignKeyAttr.Name;
                _foreignKey = foreignKeyEntity.GetAttribute(name);
            }

            return _foreignKey;
            
        }

        public MethodInfo GetGetter()
        {
            return ((PropertyInfo)_member).GetGetMethod();
        }

        public IEntityAttribute ForeignKey { get => _foreignKey ?? (_foreignKey = GetForeignKey()); set => _foreignKey = value; }

        public string Name => _member.Name;
        public MemberInfo Member => _member;

        public Type MemberType => _memberType;

        public bool IsAssociation
        {
            get => (_flags & _isAssociation) > 0;
            private set => _flags = value ? (_flags | _isAssociation) : (_flags & ~_isAssociation);
        }
        public bool IsCollection
        {
            get => (_flags & _isCollection) > 0;
            private set => _flags = value ? (_flags | _isCollection) : (_flags & ~_isCollection);
        }

        public string Column { get => _column; set => _column = value; }

        
        public bool IsRequired
        {
            get => (_flags & _isRequired) > 0;
            set => _flags = value ? (_flags | _isRequired) : (_flags & ~_isRequired);
        }

        public bool IsKey
        {
            get => (_flags & _isKey) > 0;
            set => _flags = value ? (_flags | _isKey) : (_flags & ~_isKey);
        }

        public bool IsNotMapped
        {
            get => (_flags & _isNotMapped) > 0;
            set => _flags = value ? (_flags | _isNotMapped) : (_flags & ~_isNotMapped);
        }

        public IEntityType EntityType => _entityType;

        public DatabaseGeneratedOption DbGeneratedOption { get => _dbGeneratedOption; set => _dbGeneratedOption = value; }

        public string KeySequenceName { get => _keySequenceName; set => _keySequenceName = value; }

        public bool IsProperty
        {
            get => (_flags & _isProperty) > 0;
            private set => _flags = value ? (_flags | _isProperty) : (_flags & ~_isProperty);
        }
        public int MaxLength { get => _maxLength; set => _maxLength = value; }

    }
}
