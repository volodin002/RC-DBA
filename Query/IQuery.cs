using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RC.DBA.Query
{
    public enum JoinType
    {
        UNDEFINED = 0, INNER = 1, LEFT = 2, RIGHT = 3
    }

    public interface ISelectQuery<T> : IJoin<T>
    {
        IProjection<TRes, T> Projection<TRes>();

        IPathExpression Projection<TRes>(Expression<Func<T, TRes>> expression);

        SqlQuery<T> Compile();

        SqlQuery<T, T1> Compile<T1>(string alias1);

        SqlQuery<T, T1, T2> Compile<T1, T2>(string alias1, string alias2);

        ISelectQuery<T> Limit(int limit);

        ISelectQuery<T> Offset(int offset);

        ISelectQuery<T> Top(int top);

        ISelectQuery<T> Count();

        ISelectQuery<T> CountBig();

        ISelectQuery<T> SelectSubQuery<TOther, TProp>(ISelectQuery<TOther> subQuery, Expression<Func<T, TProp>> expression);

        IEnumerable<Parameter> GetParameters();

        ///// <summary>
        ///// Specify the item that is to be returned in the query result. Replaces the previously specified selection(s), if any.
        ///// Note: Applications using the string-based API may need to specify the type of the select item when it results from a get or join operation and the query result type is specified.
        ///// For example:
        ///// CriteriaQuery<String> q = cb.createQuery(String.class);
        ///// Root<Order> order = q.from(Order.class);
        ///// q.select(order.get("shippingAddress").<String>get("state"));
        ///// CriteriaQuery<Product> q2 = cb.createQuery(Product.class);
        ///// q2.select(q2.from(Order.class).join("items").<Item,Product>join("product"));
        ///// </summary>
        ///// <typeparam name="X"></typeparam>
        ///// <param name="selection">selection - selection specifying the item that is to be returned in the query result</param>
        ///// <returns>the modified query</returns>
        //ICriteriaQuery<T> select<X>(ISelection<X> selection) where X : T;

        //ICriteriaQuery<T> multiselect<X>(ISelection<X>[] selections);

        //ICriteriaQuery<T> multiselect<X>(IEnumerable<ISelection<X>> selectionList);

        //new ICriteriaQuery<T> where(IExpression<bool> restriction);

        //new ICriteriaQuery<T> where(Predicate[] restrictions);

        //new ICriteriaQuery<T> groupBy<X>(IExpression<X>[] grouping);

        //new ICriteriaQuery<T> groupBy<X>(IEnumerable<IExpression<X>> grouping);

        //new ICriteriaQuery<T> having(IExpression<bool> restriction);

        //new ICriteriaQuery<T> having(Predicate[] restrictions);

        ///// <summary>
        ///// Specify the ordering expressions that are used to order the query results. 
        ///// Replaces the previous ordering expressions, if any. 
        ///// If no ordering expressions are specified, the previous ordering, if any, is simply removed, and results will be returned in no particular order. 
        ///// The left-to-right sequence of the ordering expressions determines the precedence, whereby the leftmost has highest precedence.
        ///// </summary>
        ///// <param name="o">zero or more ordering expressions</param>
        ///// <returns>the modified query</returns>
        //ICriteriaQuery<T> orderBy(params IOrder[] o);

        ///// <summary>
        ///// Specify the ordering expressions that are used to order the query results. 
        ///// Replaces the previous ordering expressions, if any. 
        ///// If no ordering expressions are specified, the previous ordering, if any, is simply removed, and results will be returned in no particular order. 
        ///// The left-to-right sequence of the ordering expressions determines the precedence, whereby the leftmost has highest precedence.
        ///// </summary>
        ///// <param name="o">list of zero or more ordering expressions</param>
        ///// <returns>the modified query</returns>
        //ICriteriaQuery<T> orderBy(IEnumerable<IOrder> o);

        ///// <summary>
        ///// Specify whether duplicate query results will be eliminated. 
        ///// A true value will cause duplicates to be eliminated. A false value will cause duplicates to be retained. 
        ///// If distinct has not been specified, duplicate results must be retained. 
        ///// This method only overrides the return type of the corresponding AbstractQuery method.
        ///// </summary>
        ///// <param name="distinct">boolean value specifying whether duplicate results must be eliminated from the query result or whether they must be retained</param>
        ///// <returns>the modified query.</returns>
        //new ICriteriaQuery<T> distinct(bool distinct);


        //List<IOrder> getOrderList();

        //ISet<IParameter> getParameters();
    }

    public interface IUpdateQuery<T> : IJoin<T>
    {

        SqlQuery<T> Compile();

        IUpdateQuery<T> Set<TProp>(Expression<Func<T, TProp>> expression, TProp value);
        IUpdateQuery<T> Set<TProp>(Expression<Func<T, TProp>> expression, Parameter<TProp> param);

        IUpdateQuery<T> Set<TProp>(Expression<Func<T, TProp>> expression, IPopertyExpression<T, TProp> propertyExpression);

    }

    public interface IInsertValuesQuery<T> : IExpression
    {
        SqlQuery<T> Compile();

        IInsertValuesQuery<T> Value<TProp>(Expression<Func<T, TProp>> expression, TProp value);

        IInsertValuesQuery<T> Value<TProp>(Expression<Func<T, TProp>> expression, Parameter<TProp> param);

        IInsertValuesQuery<T> ReturnIdentity<TProp>();
    }

    public interface IInsertFromQuery<T, TFrom> : IInsertValuesQuery<T>
    {
        IInsertFromQuery<T, TFrom> Value<TProp>(Expression<Func<T, TProp>> expression, IPopertyExpression<TFrom, TProp> propertyExpression);

    }

    public interface IUpdateObjectQuery<T> 
    {
        UpdateObjectCompiledQuery<T> Compile();
    }

    public interface IProjection<TRes, T>
    {
    }

}
