using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.DBA.Metamodel;

namespace RC.DBA.Query.Impl
{
    class QueryContext
    {
        private IModelManager _modelManager;
        //private List<ISelectExpression> _select;
        //private List<IOrderBy> _orderBy;

        public IModelManager ModelManager  => _modelManager;

        public QueryContext(IModelManager modelManager)
        {
            _modelManager = modelManager;
        }

        //public void AddSelect(ISelectExpression expression)
        //{
        //    if (_select == null)
        //        _select = new List<ISelectExpression>();

        //    _select.Add(expression);
        //}

        //public void AddOrderBy(IOrderBy expression)
        //{
        //    if (_orderBy == null)
        //        _orderBy = new List<IOrderBy>();

        //    _orderBy.Add(expression);
        //}

    }
}
