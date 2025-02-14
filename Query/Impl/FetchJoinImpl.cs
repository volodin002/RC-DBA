using RC.DBA.Metamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RC.DBA.Query.Impl
{
    class FetchJoinImpl<T> : JoinImpl<T>
    {
        //PropertyInfo _FetchProp;
        public FetchJoinImpl(QueryContext ctx, IAliasExpression alias, IPathExpression parentPath, IEntityAttribute attribute, List<IJoin> joins) : 
            base(ctx, alias, new PathExpressionImpl(parentPath, attribute.Name), joins)
        {
            //_FetchProp = attribute.Member;
            var filter = Filter();
            var manager = ctx.ModelManager;
            var entityType = manager.Entity<T>();
            IEntityAttribute foreignKey;
            Predicate onPredicate;

            if (attribute.IsCollection)
            {
                foreignKey = attribute.GetCollectionForeignKey();

                onPredicate = new PredicateImpl(x =>
                {
                    foreignKey.EntityType.Table.CompileAlias(x, alias)
                    .Append('.').Append(foreignKey.Column)
                    .Append('=');
                    if (alias.Parent != null)
                    {
                        attribute.EntityType.Table.CompileAlias(x, alias.Parent)
                        .Append('.');
                    }
                    x.Append(attribute.EntityType.Key.Column);
                }, 
                new HashSet<EntityTable>() {
                    foreignKey.EntityType.Table,
                    attribute.EntityType.Table });
            }
            else
            {
                foreignKey = attribute.ForeignKey;

                onPredicate = new PredicateImpl(x =>
                {
                    entityType.Table.CompileAlias(x, alias)
                    .Append('.').Append(entityType.Key.Column)
                    .Append('=');
                    if (alias.Parent != null)
                    {
                        foreignKey.EntityType.Table.CompileAlias(x, alias.Parent)
                        .Append('.');
                    }
                    x.Append(foreignKey.Column);
                },
                new HashSet<EntityTable>() {
                    entityType.Table,
                    entityType.Key.EntityType.Table,
                    foreignKey.EntityType.Table });
            }

            filter.Predicate(onPredicate);

            
            if (_JoinType == JoinType.UNDEFINED && (attribute.IsCollection || !foreignKey.IsRequired))
                _JoinType = JoinType.LEFT;
            
        }

       
    }
}
