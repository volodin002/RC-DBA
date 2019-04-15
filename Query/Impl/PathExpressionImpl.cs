using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.DBA.Query.Impl
{
    class PathExpressionImpl : IPathExpression
    {
        string _alias;
        IPathExpression _parent;
        public PathExpressionImpl(IPathExpression parent, string alias) : this(alias)
        {
            _parent = parent;
        }

        public PathExpressionImpl(string alias)
        {
            _alias = alias;
        }

        public IPathExpression Parent => _parent;

        public StringBuilder CompileToSQL(StringBuilder sql)
        {
            if (_parent != null)
                _parent.CompileToSQL(sql).Append('.');

            return sql.Append(_alias);
        }
    }
}
