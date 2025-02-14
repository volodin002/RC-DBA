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

        public PathExpressionImpl(IPathExpression parent, IPathExpression path) : this(path.Alias)
        {
            var localParent = path.Parent;
            if (localParent == null)
            {
                _parent = parent;
                return;
            }

            var stack = new Stack<IPathExpression>();
            
            while (localParent != null)
            {
                stack.Push(localParent);
                localParent = localParent.Parent;
            }

            do
            {
                localParent = stack.Pop();
                parent = new PathExpressionImpl(parent, localParent.Alias);
            }
            while (stack.Count > 0);

            _parent = parent;
        }

        public PathExpressionImpl(string alias)
        {
            _alias = alias;
        }

        public IPathExpression Parent => _parent;

        public string Alias => _alias;

        public StringBuilder CompileToSQL(StringBuilder sql)
        {
            if (_parent != null)
                _parent.CompileToSQL(sql).Append('.');

            return sql.Append(_alias);
        }
    }
}
