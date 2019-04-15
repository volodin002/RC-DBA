using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace RC.DBA
{

    public interface  IDbObjectFactory<T> 
    {
        bool OnFirst(DbDataReader reader);
        bool OnNext(DbDataReader reader);
        IEnumerable<T> OnComplete();
    }


    public interface IDbRecordReader<T>
    {
        T ReadRecord(DbDataReader reader);
    }
}
