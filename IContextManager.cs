using RC.DBA.Metamodel;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace RC.DBA
{
    public interface IContextManager
    {
        Func<DbDataReader, IList<T>> GetResultListFactory<T>(DbDataReader reader, object cacheKey);
        Func<DbDataReader, T> GetValueFactory<T>();
        Func<DbDataReader, T> GetReadValueFactory<T>();

        IModelManager ModelManager { get; }
    }
}
