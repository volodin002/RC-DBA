using System;
using System.Collections.Generic;
using System.Linq;

namespace RC.DBA.Query
{

    public interface IPopertyExpression<T, TProp> : IPopertyExpression { }

    public interface IPopertyExpression : IExpression { }

}
