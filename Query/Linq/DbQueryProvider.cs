using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RC.DBA.Query.Linq
{
    public class DbQueryProvider : QueryProviderImpl
    {
        QueryTranslator _translator;
        public DbQueryProvider()
        {
            _translator = new QueryTranslator();
        }

        public override object Execute(Expression expression)
        {
            var sql = _translator.Translate(expression);

            throw new NotImplementedException();
        }

        public override string GetQueryText(Expression expression)
        {
            return _translator.Translate(expression);
        }
    }
}
