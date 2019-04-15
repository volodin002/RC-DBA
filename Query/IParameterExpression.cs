using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.DBA.Query
{
    public interface IParameterExpression<T> : IParameterExpression
    {
        Parameter<T> GetParameter();
    }

    public interface IParameterExpression : IExpression
    {
        Parameter Parameter { get; }
    }
}
