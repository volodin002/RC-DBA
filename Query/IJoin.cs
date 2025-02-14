using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;


namespace RC.DBA.Query
{
    public interface IJoin<T> : IJoin
    {
        ISelect<T> Select();

        //ISelect<TProjection> Select<TProjection>();

        ISelect<T> Select(IPathExpression mapping);

        IPopertyFilterExpression<T, TProp> FromProp<TProp>(Expression<Func<T, TProp>> expression);

        IPopertyFilterExpression<T, TProp> FromPropOfType<TProp>(Expression<Func<T, TProp>> expression);

        IJoin<TOther> Join<TOther>(string alias);

        IJoin<TOther> LeftJoin<TOther>(string alias);

        IJoin<TProp> FetchJoin<TProp>(Expression<Func<T, TProp>> expression);

        IJoin<T> FetchJoin<TProp>(Expression<Func<T, TProp>> expression, out IJoin<TProp> alias);

        IJoin<TProp> FetchLeftJoin<TProp>(Expression<Func<T, TProp>> expression);

        IJoin<T> FetchLeftJoin<TProp>(Expression<Func<T, TProp>> expression, out IJoin<TProp> alias);

        IJoin<TProp> Join<TProp>(Expression<Func<T, TProp>> expression);

        IJoin<TProp> LeftJoin<TProp>(Expression<Func<T, TProp>> expression);

        IJoin<TProp> FetchJoin<TProp>(Expression<Func<T, TProp>> expression, IPathExpression mapping);

        IJoin<T> FetchJoin<TProp>(Expression<Func<T, TProp>> expression, out IJoin<TProp> alias, IPathExpression mapping);

        IJoin<T> FetchLeftJoin<TProp>(Expression<Func<T, TProp>> expression, out IJoin<TProp> alias, IPathExpression mapping);

        IJoin<TProp> FetchCollectionJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression);

        IJoin<TProp> FetchCollectionLeftJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression);

        IJoin<T> FetchCollectionJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression, out IJoin<TProp> alias);

        IJoin<T> FetchCollectionLeftJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression, out IJoin<TProp> alias);

        IJoin<TProp> CollectionJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression);

        IJoin<TProp> FetchCollectionJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression, IPathExpression mapping);

        IJoin<TProp> FetchCollectionLeftJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression, IPathExpression mapping);

        IJoin<T> FetchCollectionJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression, out IJoin<TProp> alias, IPathExpression mapping);

        IJoin<T> FetchCollectionLeftJoin<TProp>(Expression<Func<T, IEnumerable<TProp>>> expression, out IJoin<TProp> alias, IPathExpression mapping);

        IJoin<TOther> OuterApply<TOther>(ISelectQuery<TOther> query, string alias);

        IJoin<TOther> OuterApplyEx<TOther>(ISelectQuery<TOther> query, string alias);

        IJoin<TOther> CrossApply<TOther>(ISelectQuery<TOther> query, string alias);

        IJoin<TOther> CrossApplyEx<TOther>(ISelectQuery<TOther> query, string alias);

        Filter<T> Filter();

        Filter<T> Filter(Func<Filter<T>, Predicate> filterFactory);

        IOrderBy<T> OrderBy();

        ISelect<T> OfType(IEnumerable<Type> types);

        ISelect<T> OfType<T1, T2>() where T1 : T where T2 : T;

        ISelect<T> OfType<T1, T2, T3>() where T1 : T where T2 : T where T3 : T;
    }

    public interface IJoin : IExpression
    {
        IAliasExpression Alias { get; }

        IPathExpression Path { get; }

        ISelectExpression SelectExpression();

        JoinType JoinType { get; set; }

        Predicate Predicate { get;  }

        void ClearSelect();
    }
}
