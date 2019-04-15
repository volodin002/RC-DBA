using RC.DBA.Metamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace RC.DBA.Query.Impl
{
    class QueryBuilderImpl : IQueryBuilder
    {
        int alias_number;
        QueryContext _ctx;
        public QueryBuilderImpl(IModelManager modelManager)
        {
            _ctx = new QueryContext(modelManager);
        }

        public ISelectQuery<T> From<T>()
        {
            var alias = "a" + (alias_number++).ToString();
            return From<T>(alias);
        }

        public ISelectQuery<T> From<T>(string alias)
        {
            return From<T>(new AliasExpressionImpl(alias));
        }

        public ISelectQuery<T> From<T>(IAliasExpression alias)
        {
            return new SelectQueryImpl<T>(_ctx, alias);
        }

        public Filter<T> Filter<T>(IAliasExpression alias)
        {
            return new FilterImpl<T>(_ctx, alias);
        }

        public IOrderBy<T> OrderBy<T>(IAliasExpression alias)
        {
            return new OrderByImpl<T>(_ctx, alias);
        }

        public IUpdateQuery<T> Update<T>()
        {
            var alias = "a" + (alias_number++).ToString();
            return new UpdateQueryImpl<T>(_ctx, new AliasExpressionImpl(alias));
        }

        public IInsertValuesQuery<T> InsertValues<T>()
        {
            return new InsertValuesQueryImpl<T>(_ctx.ModelManager);
        }

        public IInsertFromQuery<T, TFrom> InsertFrom<T, TFrom>(ISelectQuery<TFrom> from)
        {
            return new InsertFromQueryImpl<T, TFrom>(_ctx.ModelManager, from);
        }

        public IDeleteQuery<T> Delete<T>()
        {
            var alias = "a" + (alias_number++).ToString();
            return new DeleteQueryImpl<T>(_ctx, new AliasExpressionImpl(alias));
        }

        public UpdateObjectCompiledQuery<T> UpdateObject<T>()
        {
            var manager = _ctx.ModelManager;
            var entity = manager.Entity<T>();
            var keyAttr = entity.Key;

            var tableStack = GetEntityTableStack(entity);

            StringBuilder sql = new StringBuilder();
            for (int k = tableStack.Count - 1; k >= 0; k--)
            {
                var x = tableStack[k];
                var table = x.table;
                var attributes = x.attributes
                    .Where(a => a.DbGeneratedOption == DatabaseGeneratedOption.None && a != keyAttr)
                    .ToList();

                if (attributes.Count == 0) continue;

                sql.Append("UPDATE ").Append(table.TableName).AppendLine(" SET ");
                var attr = attributes[0];
                sql.Append(attr.Column).Append('=').Append('@').Append(attr.Name);
                for (int i = 1; i < attributes.Count; i++)
                {
                    attr = attributes[i];
                    sql.Append(',');
                    sql.AppendLine();
                    sql.Append(attr.Column).Append('=').Append('@').Append(attr.Name);
                }
                sql.AppendLine();
                sql.Append("WHERE ")
                    .Append(keyAttr.Column)
                    .Append('=')
                    .Append('@').Append(keyAttr.Name);

                sql.AppendLine();
            }


            var factory = Emit.EntityParametersEmitter.EmitUpdateFactory<T>(manager);
            return new UpdateObjectCompiledQuery<T>(sql.ToString(), factory);
        }

        public UpdateObjectCompiledQuery<T> InsertObject<T>()
        {
            var manager = _ctx.ModelManager;
            var entity = manager.Entity<T>();

            var tableStack = GetEntityTableStack(entity);

            StringBuilder sql = new StringBuilder();

            InsertObjectSql(sql, null, tableStack);

            var factory = Emit.EntityParametersEmitter.EmitInsertFactory<T>(manager);
            return new UpdateObjectCompiledQuery<T>(sql.ToString(), factory);
        }

        public BatchUpdateCompiledQuery<T> BatchInsertObject<T>()
        {
            var manager = _ctx.ModelManager;
            var entity = manager.Entity<T>();

            var tableStack = GetEntityTableStack(entity);

            string sqlFactory(T[] items)
            {
                StringBuilder sql = new StringBuilder();
                for (int i = 0; i < items.Length; i++)
                {
                    InsertObjectSql(sql, i.ToString(), tableStack);
                }
                return sql.ToString();
            };

            var factory = Emit.EntityParametersEmitter.EmitInsertFactory<T>(manager);
            Func<T[], IEnumerable<Parameter>> paramFactory = items =>
                items
                .SelectMany((item, i) => factory(item).Select(p => p.AddPrefix(i.ToString())));
            
            return new BatchUpdateCompiledQuery<T>(sqlFactory, paramFactory);
        }

        private static void InsertObjectSql(
            StringBuilder sql,
            string prefix,
            IReadOnlyList<(EntityTable table, List<IEntityAttribute> attributes, IEntityAttribute keyAttr)> tableStack)
        {

            for (int k = tableStack.Count - 1; k >= 0; k--)
            {
                var x = tableStack[k];
                var table = x.table;
                var attributes = x.attributes;
                var keyAttr = x.keyAttr;

                if (attributes.Count == 0) continue;

                sql.Append("INSERT INTO ").Append(table.TableName).Append('(');
                sql.Append(attributes[0].Column);
                for (int i = 1; i < attributes.Count; i++)
                {
                    sql.Append(',');
                    sql.Append(attributes[i].Column);
                }
                sql.AppendLine(")");

                sql.Append("VALUES (");
                RenderInsertAttrParam(sql, table, attributes[0], prefix);
                for (int i = 1; i < attributes.Count; i++)
                {
                    sql.Append(',');
                    RenderInsertAttrParam(sql, table, attributes[i], prefix);
                }
                sql.AppendLine(")");

                if (keyAttr != null && keyAttr.DbGeneratedOption == DatabaseGeneratedOption.Identity)
                {
                    sql.Append("SELECT ").Append('@').Append(keyAttr.Name);
                    if (!String.IsNullOrEmpty(prefix))
                        sql.Append(prefix);
                    sql.Append('=').AppendLine("SCOPE_IDENTITY()");
                }

                var generated = attributes.Where(a => !a.IsKey && a.DbGeneratedOption != DatabaseGeneratedOption.None).ToList();
                if (generated.Count > 0)
                {
                    sql.Append("SELECT ");

                    var attr = attributes[0];

                    sql.Append(attr.Column).Append('=').Append('@').Append(attr.Name);
                    if (!String.IsNullOrEmpty(prefix))
                        sql.Append(prefix);

                    for (int i = 1; i < generated.Count; i++)
                    {
                        attr = attributes[i];
                        sql.Append(',');
                        sql.Append(attr.Column).Append('=').Append('@').Append(attr.Name);
                        if (!String.IsNullOrEmpty(prefix))
                            sql.Append(prefix);
                    }

                    sql.AppendLine()
                        .Append("FROM ").Append(table.TableName).AppendLine()
                        .Append("WHERE ").Append(keyAttr.Column).Append('=').Append('@').Append(keyAttr.Name);

                    if (!String.IsNullOrEmpty(prefix))
                        sql.Append(prefix);
                }

                sql.AppendLine();
            }

        }

        public UpdateObjectCompiledQuery<T> DeleteObject<T>()
        {
            var manager = _ctx.ModelManager;
            var entity = manager.Entity<T>();
            var keyAttr = entity.Key;

            var tableList = new List<EntityTable>();
            while (entity != null)
            {
                int cnt = tableList.Count;
                if (cnt > 0 && tableList[cnt - 1] == entity.Table)
                    continue;

                tableList.Add(entity.Table);

                entity = entity.BaseEntityType;
            }

            StringBuilder sql = new StringBuilder();
            //if (tableStack.Count > 1) // reverse table stack
            //{
            //    var temp = new Stack<EntityTable>();
            //    while (tableStack.Count != 0)
            //        temp.Push(tableStack.Pop());
            //    tableStack = temp;
            //}

            DeleteObjectSql(sql, null, keyAttr, tableList);

            var factory = Emit.EntityParametersEmitter.EmitDeleteFactory<T>(manager);
            return new UpdateObjectCompiledQuery<T>(sql.ToString(), factory);
        }

        public BatchUpdateCompiledQuery<T> BatchDeleteObject<T>()
        {
            var manager = _ctx.ModelManager;
            var entity = manager.Entity<T>();
            var keyAttr = entity.Key;

            var tableList = new List<EntityTable>();
            while (entity != null)
            {
                int cnt = tableList.Count;
                if (cnt > 0 && tableList[cnt - 1] == entity.Table)
                    continue;

                tableList.Add(entity.Table);

                entity = entity.BaseEntityType;
            }

            Func<T[], string> sqlFactory = items =>
            {
                StringBuilder sql = new StringBuilder();
                for (int i = 0; i < items.Length; i++)
                {
                    DeleteObjectSql(sql, i.ToString(), keyAttr, tableList);
                }
                return sql.ToString();
            };


            var factory = Emit.EntityParametersEmitter.EmitInsertFactory<T>(manager);
            Func<T[], IEnumerable<Parameter>> paramFactory = items =>
                items
                .SelectMany((item, i) => factory(item).Select(p => p.AddPrefix(i.ToString())));

            return new BatchUpdateCompiledQuery<T>(sqlFactory, paramFactory);
        }

        private static void DeleteObjectSql(StringBuilder sql, string prefix, IEntityAttribute keyAttr, IReadOnlyList<EntityTable> tableList)
        {
            //while (tableStack.Count > 0)
            for (int k = 0; k < tableList.Count; k++)
            {
                var table = tableList[k];

                sql.Append("DELETE FROM ").Append(table.TableName).AppendLine();
                sql.Append("WHERE ")
                    .Append(keyAttr.Column)
                    .Append('=')
                    .Append('@').Append(keyAttr.Name);

                if (!String.IsNullOrEmpty(prefix))
                    sql.Append(prefix);

                sql.AppendLine();
            }
        }

        private static IReadOnlyList<(EntityTable table, List<IEntityAttribute> attributes, IEntityAttribute keyAttr)> GetEntityTableStack(IEntityType entity)
        {
            var tableStack = new List<(EntityTable table, List<IEntityAttribute> attributes, IEntityAttribute keyAttr)>();
            
            while (entity != null)
            {
                int cnt = tableStack.Count;
                if (cnt > 0 && tableStack[cnt - 1].table == entity.Table)
                {
                    tableStack[cnt - 1].attributes.AddRange(
                        entity.Attributes.Where(a => !a.IsNotMapped && !a.IsAssociation && a.DbGeneratedOption == DatabaseGeneratedOption.None)
                        .ToList());
                }
                else
                {
                    tableStack.Add((table: entity.Table, 
                        attributes: entity.Attributes
                            .Where(a => !a.IsNotMapped && !a.IsAssociation && a.DbGeneratedOption == DatabaseGeneratedOption.None)
                            .ToList(),
                        keyAttr: entity.Key));
                }

                entity = entity.BaseEntityType;
            }

            return tableStack;
        }

        private static void RenderInsertAttrParam(StringBuilder sql, EntityTable table, IEntityAttribute attribute, string prefix)
        {
            if (attribute.IsKey)
            {
                if (attribute.EntityType.Table != table || attribute.DbGeneratedOption == DatabaseGeneratedOption.None)
                {
                    sql.Append('@').Append(attribute.Name);
                }
            }
            else
            {
                sql.Append('@').Append(attribute.Name);
            }

            if (!String.IsNullOrEmpty(prefix))
                sql.Append(prefix);
        }
    }
}
