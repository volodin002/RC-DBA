using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.DBA.Query
{
    public interface IExpression
    {
        StringBuilder CompileToSQL(StringBuilder sql);
    }

    public interface IPathExpression : IExpression
    {
        IPathExpression Parent { get; }
        string Alias { get; }
    }

    public interface IAliasExpression : IExpression
    {
        IAliasExpression Parent { get; }
    }


}
