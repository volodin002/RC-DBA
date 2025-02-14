using RC.DBA.Metamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RC.DBA.Query
{
    public abstract class Filter<T> : Predicate
    {
        public abstract Predicate FromExpression(Expression<Func<T, bool>> expression);

        public abstract PopertyFilterExpression<T, TProp> Prop<TProp>(IEntityAttribute entityAttribute);
        public abstract PopertyFilterExpression<T, TProp> Prop<TProp>(string field);

        public abstract PopertyFilterExpression<T, TProp> Prop<TProp>(Expression<Func<T, TProp>> expression);

        public abstract PopertyFilterExpression<T, TProp> PropOfType<TProp>(Expression<Func<T, TProp>> expression);

        public abstract Filter<T> Predicate(Func<Filter<T>, Predicate> filterFactory);
        public abstract void Predicate(Predicate predicate);

        public abstract Predicate FromSql(string sql, IEnumerable<Parameter> parameters);

        public abstract bool IsEmpty();

        public abstract void SetEmpty();

        public abstract Predicate Exists<TOther>(ISelectQuery<TOther> query);

        public abstract Predicate NotExists<TOther>(ISelectQuery<TOther> query);

        public abstract Predicate OfType<T1>() where T1 : T;

        public abstract Predicate OfType(Type type);

    }

    public abstract class Predicate : IExpression
    {
        protected bool _isNot;

        public abstract Predicate And(params Predicate[] predicates);

        public abstract Predicate Or(params Predicate[] predicates);

        public abstract Predicate And(IEnumerable<Predicate> predicates);

        public abstract Predicate Or(IEnumerable<Predicate> predicates);

        public Predicate Not()
        {
            _isNot = !_isNot;
            return this;
        }

        public abstract StringBuilder CompileToSQL(StringBuilder sql);
        

        public abstract IEnumerable<Parameter> Parameters { get; }

        public abstract IEnumerable<EntityTable> Tables { get; }
    }

    public static class PredicateOp
    {
        public static Predicate And(params Predicate[] predicates)
        {
            return new Impl.CompositePredicateImpl(false, predicates);
        }

        public static Predicate And(IEnumerable<Predicate> predicates)
        {
            return new Impl.CompositePredicateImpl(false, predicates.ToArray());
        }

        public static Predicate Or(params Predicate[] predicates)
        {
            return new Impl.CompositePredicateImpl(true, predicates);
        }

        public static Predicate Or(IEnumerable<Predicate> predicates)
        {
            return new Impl.CompositePredicateImpl(true, predicates.ToArray());
        }
    }
}
