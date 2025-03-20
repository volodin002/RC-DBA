using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RC.DBA.Query
{
    public interface IOrderBy<T> : IOrderBy
    {
        IOrderBy<T> OrderBy<TProp>(Expression<Func<T, TProp>> expression);

        IOrderBy<T> OrderBy<TProp>(Expression<Func<T, TProp>> expression, bool desc);

        IOrderBy<T> OrderBy<TProp>(string alias, Expression<Func<T, TProp>> expression);

        IOrderBy<T> OrderBy<TProp>(string alias, Expression<Func<T, TProp>> expression, bool desc);

        IOrderBy<T> OrderBy(string field);

        IOrderBy<T> OrderBy(string field, bool desc);

        IOrderBy<T> Clear();
    }

    public interface IOrderBy : IExpression
    {
        IList<IOrderByExpression> Expressions();
        IOrderBy OrderBy(params IOrderBy[] orderBy);

        IOrderBy OrderBy(IEnumerable<IOrderBy> orderBy);

        IOrderBy Add(IOrderByExpression expression);



    }

    public interface IOrderByExpression : IExpression
    {
        string Name { get; }
        bool IsDesc { get; set; }

        IOrderByExpression Asc();
        IOrderByExpression Desc();
    }


}
