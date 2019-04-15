using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.DBA.Query.Impl
{
    class AliasExpressionImpl : IAliasExpression
    {
        string _name;
        IAliasExpression _parent;
        public AliasExpressionImpl(IAliasExpression parent, string name) : this(name)
        {
            _parent = parent;
        }

        public AliasExpressionImpl(string name)
        {
            _name = name;
        }

        public IAliasExpression Parent => _parent;

        public StringBuilder CompileToSQL(StringBuilder sql)
        {
            if (_parent != null)
                _parent.CompileToSQL(sql);

            return sql.Append(_name);
        }
    }
}
