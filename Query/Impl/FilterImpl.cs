using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using RC.DBA.Metamodel;

namespace RC.DBA.Query.Impl
{
    class FilterImpl<T> : Filter<T> // PredicateBaseImpl,
    {
        QueryContext _ctx;
        IAliasExpression _alias;
        Predicate _predicate;

        public IAliasExpression Alias => _alias;

        public FilterImpl(QueryContext ctx, IAliasExpression alias) 
        {
            _ctx = ctx;
            _alias = alias;
        }

        public override Predicate FromExpression(Expression<Func<T, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public override PopertyFilterExpression<T, TProp> Prop<TProp>(IEntityAttribute entityAttr)
        {   
            return new PopertyFilterExpression<T, TProp>(_alias, entityAttr);
        }

        public override PopertyFilterExpression<T, TProp> Prop<TProp>(string field)
        {
            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(field);
            
            return new PopertyFilterExpression<T, TProp>(_alias, entityAttr);
        }

        public override PopertyFilterExpression<T, TProp> Prop<TProp>(Expression<Func<T, TProp>> expression)
        {
            var memberInfo = Helper.Member(expression);
            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(memberInfo.Name);

            return new PopertyFilterExpression<T, TProp>(_alias, entityAttr);
        }

        public override PopertyFilterExpression<T, TProp> PropOfType<TProp>(Expression<Func<T, TProp>> expression)
        {
            var memberInfo = Helper.MemberOfTypeCall(expression);
            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(memberInfo.Name);

            return new PopertyFilterExpression<T, TProp>(_alias, entityAttr);
        }


        public override IEnumerable<Parameter> Parameters => _predicate?.Parameters;

        public override IEnumerable<EntityTable> Tables => _predicate?.Tables;

        public override StringBuilder CompileToSQL(StringBuilder sql)
        {
            if (_predicate != null)
            {
                if (_isNot) sql.Append("NOT (");
                _predicate.CompileToSQL(sql);
                if (_isNot) sql.Append(')');
            }

            return sql;
        }

        public override bool IsEmpty()
        {
            return _predicate == null;
        }

        public override void SetEmpty()
        {
            _predicate = null;
        }

        public override Filter<T> Predicate(Func<Filter<T>, Predicate> filterFactory)
        {
            Predicate(filterFactory(this));
            return this;
        }

        public override void Predicate(Predicate predicate)
        {
            _predicate = predicate;
        }

        public override Predicate Exists<TOther>(ISelectQuery<TOther> query)
        {
            return new PredicateImpl(sql => {
                sql.Append(" EXISTS(");
                query.CompileToSQL(sql);
                sql.Append(')');
            }, query.GetParameters(), null);
        }

        public override Predicate NotExists<TOther>(ISelectQuery<TOther> query)
        {
            return new PredicateImpl(sql => {
                sql.Append(" NOT EXISTS(");
                query.CompileToSQL(sql);
                sql.Append(')');
            }, query.GetParameters(), null);
        }

        public override Predicate FromSql(string predicateSql, IEnumerable<Parameter> parameters)
        {
            return new PredicateImpl(sql => {
                sql.Append(predicateSql);
            }, parameters, null);
        }

        public override Predicate OfType<T1>() //where T1 : T
        {
            return OfType(typeof(T1));
        }

        public override Predicate OfType(Type type)
        {
            var entityType = _ctx.ModelManager.Entity(type);

            return new PredicateImpl(
                x => {
                    entityType.Table.CompileAlias(x, _alias);
                    x.Append('.')
                    .Append(entityType.Key.Column).Append(" is not NULL");
                },
                new[] { entityType.Table });
        }

        public override Predicate And(params Predicate[] predicates)
        {
            int length = predicates.Length;
            Predicate predicate;

            if (_predicate != null && length > 0)
            {
                var newPredicates = new Predicate[length + 1];
                newPredicates[0] = _predicate;
                Array.Copy(predicates, 0, newPredicates, 1, length);

                predicate = new CompositePredicateImpl(false, newPredicates);
            }
            else if (_predicate == null && length > 0)
            {
                if (predicates.Length > 1)
                    predicate = new CompositePredicateImpl(false, predicates);
                else
                    predicate = predicates[0];
            }
            else
                predicate = _predicate;

            return predicate;
        }

        public override Predicate Or(params Predicate[] predicates)
        {
            int length = predicates.Length;
            Predicate predicate;

            if (_predicate != null && length > 0)
            {
                var newPredicates = new Predicate[length + 1];
                newPredicates[0] = _predicate;
                Array.Copy(predicates, 0, newPredicates, 1, length);

                predicate = new CompositePredicateImpl(true, newPredicates);
            }
            else if (_predicate == null && length > 0)
            {
                if (predicates.Length > 1)
                    predicate = new CompositePredicateImpl(true, predicates);
                else
                    predicate = predicates[0];
            }
            else
                predicate = _predicate;

            return predicate;
        }

        public override Predicate And(IEnumerable<Predicate> predicates)
        {
            return this.And(predicates.ToArray());
        }

        public override Predicate Or(IEnumerable<Predicate> predicates)
        {
            return this.Or(predicates.ToArray());
        }
    }

    class CompositePredicateImpl : PredicateBaseImpl
    {
        private bool _isOR;
        protected List<Predicate> _predicates;
        public override IEnumerable<Parameter> Parameters
        {
            get
            {
                if (_predicates == null || _predicates.Count == 0)
                    return null;

                return _predicates
                    .Where(p => p.Parameters != null)
                    .SelectMany(p => p.Parameters);
            }
        }

        public override IEnumerable<EntityTable> Tables
        {
            get
            {
                if (_predicates == null) return null;
                return _predicates.Where(p => p.Tables != null).SelectMany(p => p.Tables);
            }
        }

        public CompositePredicateImpl(bool isOR, Predicate[] predicates)
        {
            _predicates = new List<Predicate>(predicates);
            _isOR = isOR;
        }


        public override StringBuilder CompileToSQL(StringBuilder sql)
        {
            if (_predicates == null || _predicates.Count == 0) return sql;

            if (_isNot) sql.Append("NOT (");
            else sql.Append(" (");

            _predicates[0].CompileToSQL(sql);
            var sOp = _isOR ? " OR " : " AND ";
            for (int i = 1; i < _predicates.Count; i++)
            {
                _predicates[i].CompileToSQL(sql.Append(sOp));
            }

            return sql.Append(')');
        }
    }

    class PredicateImpl : PredicateBaseImpl
    {
        
        IEnumerable<Parameter> _param;
        Action<StringBuilder> _impl;
        IEnumerable<EntityTable> _tables;

        public PredicateImpl(Action<StringBuilder> imp, IEnumerable<Parameter> param, IEnumerable<EntityTable> tables) : this(imp, tables)
        {
            _param = param;
        }

        public PredicateImpl(Action<StringBuilder> imp, Parameter param, IEnumerable<EntityTable> tables) : this(imp, tables)
        {
            _param = new Parameter[] { param };
        }

        public PredicateImpl(Action<StringBuilder> imp, IEnumerable<EntityTable> tables)
        {
            _impl = imp;
            _tables = tables;
        }

        public override IEnumerable<Parameter> Parameters => _param;

        public override IEnumerable<EntityTable> Tables => _tables;

        public override StringBuilder CompileToSQL(StringBuilder sql)
        {
            if (_isNot) sql.Append("NOT (");
            _impl(sql);
            if (_isNot) sql.Append(")");
            return sql;
        }
    }

    abstract class PredicateBaseImpl : Predicate
    {
        public override Predicate And(params Predicate[] predicates)
        {
            return new CompositePredicateImpl(false, predicates);
        }

        public override Predicate And(IEnumerable<Predicate> predicates)
        {
            return new CompositePredicateImpl(false, predicates.ToArray());
        }

        public override Predicate Or(params Predicate[] predicates)
        {
            return new CompositePredicateImpl(true, predicates);
        }

        public override Predicate Or(IEnumerable<Predicate> predicates)
        {
            return new CompositePredicateImpl(true, predicates.ToArray());
        }
        
    }
}
