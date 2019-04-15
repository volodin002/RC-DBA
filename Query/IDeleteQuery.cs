using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.DBA.Query
{
    public interface IDeleteQuery<T> : IJoin<T>
    {
        SqlQuery<T> Compile();
    }
}
