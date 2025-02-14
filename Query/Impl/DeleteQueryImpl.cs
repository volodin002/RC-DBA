using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.DBA.Query.Impl
{
    class DeleteQueryImpl<T> : QueryImpl<T>, IDeleteQuery<T>
    {
        public DeleteQueryImpl(QueryContext ctx, IAliasExpression alias) : base(ctx, alias)
        { }

        public override StringBuilder CompileToSQL(StringBuilder sql)
        {
            sql.Append("DELETE ");
            _From.Table.CompileAlias(sql, _alias);
           
            sql.AppendLine()
                .Append("FROM ")
                .Append(_From.Table.TableName)
                .Append(' ');
            _From.Table.CompileAlias(sql, _alias);

            CompileJoinsToSQL(sql);
            CompileFilterToSQL(sql);

            return sql;
        }
    }
}
