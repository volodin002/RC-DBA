using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.DBA.Query
{
    public interface IProperySetExpression<T, TProp> : IPopertyExpression<T, TProp>
    {
        IUpdateSetExpression Value(TProp val);

        IUpdateSetExpression Parameter(Parameter<TProp> param);
    }

    public interface IUpdateSetExpression : IExpression
    {
        IEnumerable<Parameter> Parameters { get; }
    }
}
