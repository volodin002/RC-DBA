using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RC.DBA.Collections;
using RC.DBA.Metamodel;
using static System.Net.Mime.MediaTypeNames;

namespace RC.DBA.Query.Impl
{
    class SelectQueryImpl<T> : QueryImpl<T>, ISelectQuery<T>
    {
        private Parameter<int> _limit;
        private Parameter<int> _offset;
        private int _top;
        

        public SelectQueryImpl(QueryContext ctx, IAliasExpression alias) : base(ctx, alias)
        {
        }

        public override StringBuilder CompileToSQL(StringBuilder sql)
        {
            sql.Append("SELECT");
            if ((_offset == null || _offset.Value == 0) && _limit?.Value > 0)
            {
                sql.Append(" TOP(@").Append(_limit.Name).Append(") ");
            }

            if(_top > 0)
            {
                sql.Append(" TOP(").Append(_top).Append(") ");
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

            if (_offset != null && _offset.Value > 0 && _limit?.Value > 0)
            {
                sql.AppendLine()
                .Append("OFFSET @").Append(_offset.Name)
                    .Append(" ROWS FETCH NEXT @").Append(_limit.Name)
                    .Append(" ROWS ONLY");
            }

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
            _limit = Parameter.Create(SqlQuery.Limit_Parameter_Name, limit);
            return this;
        }

        public ISelectQuery<T> Offset(int offset)
        {
            if (offset > 0)
                _offset = Parameter.Create(SqlQuery.Offset_Parameter_Name, offset);
            else
                _offset = null;

            return this;
        }

        public ISelectQuery<T> Top(int top)
        {
            _top = top;
            return this;
        }

        public ISelectQuery<T> Count()
        {
            Select().Count();
            return this;
        }

        public ISelectQuery<T> CountBig()
        {
            Select().CountBig();
            return this;
        }

        public ISelectQuery<T> SelectSubQuery<TOther, TProp>(ISelectQuery<TOther> subQuery, Expression<Func<T, TProp>> expression)
        {
            if (_subQueriesPredicates == null) _subQueriesPredicates = new ResizableArray<Predicate>();
            _subQueriesPredicates.Add(subQuery.Predicate);

            var memberInfo = Helper.Member(expression);
            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(memberInfo.Name);

            var select = Select();
            select.Select(new SelectSubQueryExpressionImpl<TOther>(Alias, entityAttr, Path, subQuery));
            

            return this;
        }

        protected override IEnumerable<Parameter> AdditionalParameters()
        {
            if (_limit != null)
                yield return _limit;

            if (_offset != null)
                yield return _offset;

        }

        internal IEnumerable<IJoin> Joins() => _Joins;

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

        protected ResizableArray<Predicate> _subQueriesPredicates;

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

        public virtual SqlQuery<T, T1> Compile<T1>(string alias1)
        {
            var sql = CompileToSQL(new StringBuilder());

            return new SqlQuery<T, T1>(sql.ToString(), alias1, GetParameters().ToArray());
        }

        public virtual SqlQuery<T, T1, T2> Compile<T1, T2>(string alias1, string alias2)
        {
            var sql = CompileToSQL(new StringBuilder());

            return new SqlQuery<T, T1, T2>(sql.ToString(), alias1, alias2, GetParameters().ToArray());
        }

        public IEnumerable<Parameter> GetParameters()
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

            if(_subQueriesPredicates != null) foreach (var subQueriesPredicate in _subQueriesPredicates)
            {
                parameters = subQueriesPredicate.Parameters;
                if (parameters == null) continue;

                foreach (var p in parameters)
                {
                    yield return p;
                }
            }

            foreach(var p in AdditionalParameters())
                yield return p;

        }

        protected virtual IEnumerable<Parameter> AdditionalParameters() => Enumerable.Empty<Parameter>();


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
