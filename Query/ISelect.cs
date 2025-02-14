using RC.DBA.Metamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

//using iExpr = System.Func<System.Text.StringBuilder, System.Text.StringBuilder>;

namespace RC.DBA.Query
{
    public interface ISelect<T> : ISelectExpression
    {
        ISelect<T> SelectAll();

        ISelect<T> SelectAllFromType<Tx>() where Tx : T;

        ISelect<T> SelectFromType<Tx, TProp>(Tp<Tx> tx, Expression<Func<Tx, TProp>> expression) where Tx : T;

        ISelect<T> Select<TProp>(Expression<Func<T, TProp>> expression);

        //ISelect<T> Select<TProp>(Action<StringBuilder, iExpr> compile, Expression<Func<T, TProp>> expression);

        ISelect<T> Select<TProp>(ISelectExpression<T, TProp> expression);

        ISelectExpression Select(ISelectExpression expression);

        ISelect<T> Count();
        ISelect<T> CountBig();

        ISelect<T> SelectDescriminator();

        ISelect<T> Clear();
    }

    public class Tp<T> { }
    //public static class Tp
    //{
    //    public static Tp<T> ForType<T>() { return new Tp<T>(); }
    //}

    


    

    public interface ISelectExpression<T, TProp> : ISelectExpression { }

    public interface ISelectExpression : IExpression
    {
        IEnumerable<ISelectExpression> Expressions();

        IEnumerable<EntityTable> Tables();

        IEntityAttribute EntityAttribute { get; }

    }
}
