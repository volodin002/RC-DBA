using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace RC.DBA.Metamodel
{
    public interface IModelManager
    {
        IEntityType<T> Entity<T>();

        IEntityType Entity(Type type);

        void ResetEntity(Type type);

        void ResetEntity<T>();

        //IEntityConfiguration<T> EntityConfig<T>();

        //IEntityConfiguration EntityConfig(Type type);

        IEnumerable<IEntityType> GetImplementations(Type type);

        IEnumerable<IEntityType> GetImplementations<T>();

        Func<DbDataReader, T> GetReadValueFactory<T>();


        Func<DbDataReader, T> GetValueFactory<T>();

        ref int GetTableIndex();

    }

    /*
    public interface IEntityConfiguration<T> : IEntityConfiguration
    {
        IEntityConfiguration<T> SetPropertyColumn<TProp>(Expression<Func<T, TProp>> property, string column);
        string GetPropertyColumn<TProp>(Expression<Func<T, TProp>> property);

        IEntityConfiguration<T> SetInverseProperty<TProp, TInvProp>(Expression<Func<T, TProp>> property, Expression<Func<TProp, TInvProp>> inverseProperty);
        string GetInverseProperty<TProp>(Expression<Func<T, TProp>> property);
        IEntityConfiguration<T> SetForeignKey<TProp,TFKProp>(Expression<Func<T, TProp>> property, Expression<Func<T, TFKProp>> foreignKey);
        string GetForeignKey<TProp>(Expression<Func<T, TProp>> property);

        IEntityConfiguration<T> SetKey<TProp>(Expression<Func<T, TProp>> property);
        bool IsKey<TProp>(Expression<Func<T, TProp>> property);

        IEntityConfiguration<T> SetNotMapped<TProp>(Expression<Func<T, TProp>> property);
        bool IsNotMapped<TProp>(Expression<Func<T, TProp>> property);

        DatabaseGeneratedOption GetDbGeneratedOption<TProp>(Expression<Func<T, TProp>> property);

        void SetDbGeneratedOption<TProp>(Expression<Func<T, TProp>> property, DatabaseGeneratedOption opt);
    }

    public interface IEntityConfiguration
    {
        Type Type { get; }
        string TableName { get; }
        string TableSchema { get; }

        void Table(string name);
        void Table(string name, string schema);

        int Descriminator { get; }

        void SetDescriminator(int value);

        string GetPropertyColumn(string property);

        void SetPropertyColumn(string property, string column);
        string GetInverseProperty(string property);

        void SetInverseProperty(string property, string inverseProperty);

        string GetForeignKey(string property);
        void SetForeignKey(string property, string foreignKey);

        void SetKey(string property);

        bool IsKey(string property);

        void SetNotMapped(string property);

        bool IsNotMapped(string property);

        DatabaseGeneratedOption GetDbGeneratedOption(string property);

        void SetDbGeneratedOption(string property, DatabaseGeneratedOption opt);
    }
    */
}
