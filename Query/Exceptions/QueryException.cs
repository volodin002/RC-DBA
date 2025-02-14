using System;
using System.Collections.Generic;


namespace RC.DBA.Query
{
    public class QueryException : Exception
    {
        public QueryException(string message) : base(message) { }
    }
}
