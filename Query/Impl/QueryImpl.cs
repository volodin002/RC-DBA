using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using RC.DBA.Metamodel;

namespace RC.DBA.Query.Impl
{
    class SelectQueryImpl<T> : QueryImpl<T>, ISelectQuery<T>
    {
        private int _limit;
        private int _offset;
        public SelectQueryImpl(QueryContext ctx, IAliasExpression alias) : base(ctx, alias)
        {
        }

        public override StringBuilder CompileToSQL(StringBuilder sql)
        {
            sql.Append("SELECT");
            if(_offset==0 && _limit > 0)
            {
                sql.Append(" TOP(").Append(_limit).Append(") ");
            }

            var select = Select();
            {
                var selectExpr = select.Expressions();
                if (selectExpr == null || !selectExpr.Any())
                {
                    select.Select(new SelectExpressionImpl(Alias, _From.Key, Path));
                }
            }
            select.CompileToSQL(sql);
            if (_Joins != null && _Joins.Count > 0)
            {
                
                foreach (var join in _Joins)
                {
                    var selectExpr = join.SelectExpression();
                    var expr = selectExpr.Expressions();
                    if (expr == null || !expr.Any())
                        continue;

                    sql.Append(',');
                    selectExpr.CompileToSQL(sql);
                }
            }

            sql.AppendLine()
                .Append("FROM ")
                .Append(_From.Table.TableName)
                .Append(' ');
            _From.Table.CompileAlias(sql, _alias);

            AddJoinForSelectedTables(sql, _From);

            CompileJoinsToSQL(sql);

            CompileFilterToSQL(sql);

            if (_OrderBy != null)
                _OrderBy.CompileToSQL(sql);

            return sql;
        }

       
        public IProjection<TRes, T> Projection<TRes>()
        {
            throw new NotImplementedException();
        }

        public IPathExpression Projection<TRes>(Expression<Func<T, TRes>> expression)
        {
            throw new NotImplementedException();
            //var propInfo = Helper.Property(expression);
            //return new PathExpressionImpl(propInfo.Name);
        }

        public ISelectQuery<T> Limit(int limit)
        {
            _limit = limit;
            return this;
        }

        public ISelectQuery<T> Offset(int offset)
        {
            _offset = offset;
            return this;
        }

        /*
        protected bool _distinct;
        protected IEnumerable<IOrder> _orderBy;
        public ICriteriaQuery<T> distinct(bool distinct)
        {
            _distinct = distinct;
            return this;
        }

        public IRoot<X> from<X>()
        {
            throw new NotImplementedException();
        }

        public IRoot<X> from<X>(IEntityType<X> entity)
        {
            throw new NotImplementedException();
        }

        public List<IExpression<X>> getGroupList<X>()
        {
            throw new NotImplementedException();
        }

        public Predicate getGroupRestriction()
        {
            throw new NotImplementedException();
        }

        public List<IOrder> getOrderList()
        {
            throw new NotImplementedException();
        }

        public ISet<IParameterExpression<X>> getParameters<X>()
        {
            throw new NotImplementedException();
        }

        public Predicate getRestriction()
        {
            throw new NotImplementedException();
        }

        public Type getResultType()
        {
            return typeof(T);
        }

        public ISet<IRoot<X>> getRoots<X>()
        {
            throw new NotImplementedException();
        }

        public ISelection<T> getSelection()
        {
            throw new NotImplementedException();
        }

        public ICriteriaQuery<T> groupBy<X>(IExpression<X>[] grouping)
        {
            throw new NotImplementedException();
        }

        public ICriteriaQuery<T> groupBy<X>(IEnumerable<IExpression<X>> grouping)
        {
            throw new NotImplementedException();
        }

        public ICriteriaQuery<T> having(IExpression<bool> restriction)
        {
            throw new NotImplementedException();
        }

        public ICriteriaQuery<T> having(Predicate[] restrictions)
        {
            throw new NotImplementedException();
        }

        public bool isDistinct()
        {
            return _distinct;
        }

        public ICriteriaQuery<T> multiselect<X>(ISelection<X>[] selections)
        {
            throw new NotImplementedException();
        }

        public ICriteriaQuery<T> multiselect<X>(IEnumerable<ISelection<X>> selectionList)
        {
            throw new NotImplementedException();
        }

        public ICriteriaQuery<T> orderBy(params IOrder[] o)
        {
            _orderBy = o;
            return this;
        }

        public ICriteriaQuery<T> orderBy(IEnumerable<IOrder> o)
        {
            _orderBy = o;
            return this;
        }

        public ICriteriaQuery<T> select<X>(ISelection<X> selection) where X : T
        {
            throw new NotImplementedException();
        }

        public ISubquery<T1> subquery<T1>()
        {
            throw new NotImplementedException();
        }

        public ICriteriaQuery<T> where(IExpression<bool> restriction)
        {
            throw new NotImplementedException();
        }

        public ICriteriaQuery<T> where(Predicate[] restrictions)
        {
            throw new NotImplementedException();
        }

        IAbstractQuery<T> IAbstractQuery<T>.distinct(bool distinct)
        {
            return this.distinct(distinct);
        }

        IAbstractQuery<T> IAbstractQuery<T>.groupBy<X>(IExpression<X>[] grouping)
        {
            throw new NotImplementedException();
        }

        IAbstractQuery<T> IAbstractQuery<T>.groupBy<X>(IEnumerable<IExpression<X>> grouping)
        {
            throw new NotImplementedException();
        }

        IAbstractQuery<T> IAbstractQuery<T>.having(IExpression<bool> restriction)
        {
            throw new NotImplementedException();
        }

        IAbstractQuery<T> IAbstractQuery<T>.having(Predicate[] restrictions)
        {
            throw new NotImplementedException();
        }

        IAbstractQuery<T> IAbstractQuery<T>.where(IExpression<bool> restriction)
        {
            throw new NotImplementedException();
        }

        IAbstractQuery<T> IAbstractQuery<T>.where(Predicate[] restrictions)
        {
            throw new NotImplementedException();
        }
        */

    }

    class QueryImpl<T> : JoinImpl<T>
    {
        protected IEntityType _From;
        
        public QueryImpl(QueryContext ctx, IAliasExpression alias) : base(ctx, alias, null, null)
        {
            _From = ctx.ModelManager.Entity<T>();
        }

        public virtual SqlQuery<T> Compile()
        {
            var sql = CompileToSQL(new StringBuilder());

            return new SqlQuery<T>(sql.ToString(), GetParameters().ToArray());

            //if (Filter().Parameters != null)
            //    return new SqlQuery<T>(sql.ToString(), Filter().Parameters.ToArray());
            //else
            //    return new SqlQuery<T>(sql.ToString());
        }

        protected IEnumerable<Parameter> GetParameters()
        {
            var parameters = Filter().Parameters;
            if (parameters != null) foreach (var p in parameters)
            {
                yield return p;
            }

            if (_Joins != null) foreach (var j in _Joins.Where(j => j.Predicate != null))
            {
                parameters = j.Predicate.Parameters;
                if (parameters == null) continue;

                foreach (var p in parameters)
                {
                    yield return p;
                }
            }

        }

        protected void CompileJoinsToSQL(StringBuilder sql)
        {
            if (_Joins != null && _Joins.Count > 0)
            {
                foreach (var join in _Joins)
                {
                    join.CompileToSQL(sql);
                }
            }
        }

        protected void CompileFilterToSQL(StringBuilder sql)
        {
            if (_Filter != null && !_Filter.IsEmpty())
            {
                sql.AppendLine().Append("WHERE ");
                _Filter.CompileToSQL(sql);
            }
        }

    }



}
