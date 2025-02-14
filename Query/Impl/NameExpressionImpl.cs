using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.DBA.Query.Impl
{
    class NameExpressionImpl : IExpression
    {
        string _name;
        public NameExpressionImpl(string name)
        {
            _name = name;
        }

        public StringBuilder CompileToSQL(StringBuilder sql)
        {
            return sql.Append(_name);
        }
    }
}
