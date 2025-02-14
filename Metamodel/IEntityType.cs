using RC.DBA.Metamodel.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace RC.DBA.Metamodel
{
    public interface IEntityType<T> : IEntityType
    {
        IEntityType<T> SetPropertyColumn<TProp>(Expression<Func<T, TProp>> property, string column);
        string GetPropertyColumn<TProp>(Expression<Func<T, TProp>> property);

        IEntityType<T> SetInverseProperty<TProp, TInvProp>(Expression<Func<T, TProp>> property, Expression<Func<TProp, TInvProp>> inverseProperty);
        string GetInverseProperty<TProp>(Expression<Func<T, TProp>> property);
        IEntityType<T> SetForeignKey<TProp, TFKProp>(Expression<Func<T, TProp>> property, Expression<Func<T, TFKProp>> foreignKey);

        IEntityType<T> SetCollectionForeignKey<TProp, TFKProp>(Expression<Func<T, IEnumerable<TProp>>> property, Expression<Func<TProp, TFKProp>> foreignKey);
        IEntityAttribute GetForeignKey<TProp>(Expression<Func<T, TProp>> property);

        IEntityType<T> SetKey<TProp>(Expression<Func<T, TProp>> property);

        IEntityType<T> SetKey<TProp>(Expression<Func<T, TProp>> property, string keySequenceName);
        bool IsKey<TProp>(Expression<Func<T, TProp>> property);

        IEntityType<T> SetNotMapped<TProp>(Expression<Func<T, TProp>> property);
        bool IsNotMapped<TProp>(Expression<Func<T, TProp>> property);

        IEntityType<T> SetPropConvertMethod<TDb, TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> property, Expression<Func<TDb, TProp>> methodExpression);

        DatabaseGeneratedOption GetDbGeneratedOption<TProp>(Expression<Func<T, TProp>> property);

        IEntityType<T> SetMaxLength<TProp>(Expression<Func<T, TProp>> property, int maxLength);
        int GetMaxLength<TProp>(Expression<Func<T, TProp>> property);

        IEntityType<T> SetDbGeneratedOption<TProp>(Expression<Func<T, TProp>> property, DatabaseGeneratedOption opt);

        IEntityType<T> SetTableForType(string name, string schema);

    }
    public interface IEntityType 
    {
        Type ClassType { get; }

        string Name { get; }

        IModelManager Manager { get; }

        IEnumerable<IEntityAttribute> Attributes { get; }

        /// <summary>
        /// Own entity table
        /// </summary>
        EntityTable Table { get; }

        /// <summary>
        /// Entity table in entity hierarchy.
        /// </summary>
        EntityTable EnityTable { get; }

        EntityTable ParentTable { get; }

        void SetTable(string name, string schema);

        bool IsAbstract { get; }

        IEntityType BaseEntityType { get; }

        int Descriminator { get; }

        IEntityAttribute GetAttribute(string name);
        IEnumerable<IEntityAttribute> GetAttributes(string name);

        IEnumerable<IEntityType> Implementations { get; }

        IEnumerable<IEntityAttribute> AllAttributes { get;  }

        IEntityAttribute Key { get; }


        void SetDescriminator(int value);

        string GetPropertyColumn(string property);

        void SetPropertyColumn(string property, string column);
        string GetInverseProperty(string property);

        void SetInverseProperty(string property, string inverseProperty);

        IEntityAttribute GetForeignKey(string property);
        void SetForeignKey(string property, string foreignKey);

        void SetForeignKey(string property, IEntityAttribute foreignKey);

        void SetKey(string property);

        bool IsKey(string property);

        void SetNotMapped(string property);

        bool IsNotMapped(string property);

        DatabaseGeneratedOption GetDbGeneratedOption(string property);

        void SetDbGeneratedOption(string property, DatabaseGeneratedOption opt);

        MethodInfo GetAttributeConvertMethod(string attributeName);

        void SetPropConvertMethod(string property, MethodInfo method);
    }

    public class EntityTable
    {
        public readonly string Schema;
        public readonly string Name;

        public string TableName;

        //public readonly EntityTable Parent;
        public readonly IEntityType Entity;

        public readonly int Index;

        public EntityTable Parent => Entity.ParentTable;

        public EntityTable(string name, string schema, IEntityType entity, int index)
        {
            Schema = schema;
            Name = name;

            TableName = string.IsNullOrEmpty(schema) ? name : schema + "." + name;

            Entity = entity;

            Index = index;
        }

        public StringBuilder CompileAlias(StringBuilder sql, Query.IAliasExpression alias)
        {
            return alias.CompileToSQL(sql).Append('t').Append(Index);
        }

    }

}
