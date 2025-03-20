using RC.DBA.Metamodel;
using RC.DBA.Metamodel.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RC.DBA.Query.Impl
{
    class OrderByImpl<T> : IOrderBy<T>
    {
        QueryContext _ctx;
        IAliasExpression _alias;
        List<IOrderByExpression> _expressions;

        public OrderByImpl(QueryContext ctx, IAliasExpression alias)
        {
            _ctx = ctx;
            _alias = alias;
        }

        public StringBuilder CompileToSQL(StringBuilder sql)
        {
            if (_expressions == null || _expressions.Count == 0) return sql;

            sql.AppendLine().Append("ORDER BY ");
            _expressions[0].CompileToSQL(sql);
            for (int i = 1; i < _expressions.Count; i++)
            {
                _expressions[i].CompileToSQL(sql.Append(','));
            }

            return sql;
        }

        public IOrderBy Add(IOrderByExpression expression)
        {
            Expressions().Add(expression);
            return this;
        }

        public IList<IOrderByExpression> Expressions()
        {
            return _expressions ?? (_expressions = new List<IOrderByExpression>());
        }

        public IOrderBy<T> OrderBy<TProp>(Expression<Func<T, TProp>> expression)
        {
            return OrderBy(expression, false);
        }

        public IOrderBy<T> OrderBy<TProp>(string alias, Expression<Func<T, TProp>> expression)
        {
            return OrderBy(alias, expression, false);
        }

        public IOrderBy<T> OrderBy(string field)
        {
            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(field);
            if (entityAttr == null)
                throw new ArgumentException($"Cannot find attribute {field} of type {typeof(T)}");

            if (_expressions == null) _expressions = new List<IOrderByExpression>();
            _expressions.Add(new OrderByExpressionImpl(_alias, entityAttr));

            return this;
        }

        public IOrderBy<T> OrderBy<TProp>(Expression<Func<T, TProp>> expression, bool desc)
        {
            var memberInfo = Helper.Member(expression);
            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(memberInfo.Name);

            if (_expressions == null) _expressions = new List<IOrderByExpression>();
            _expressions.Add(new OrderByExpressionImpl(_alias, entityAttr) { IsDesc = desc });

            return this;
        }

        public IOrderBy<T> OrderBy<TProp>(string alias, Expression<Func<T, TProp>> expression, bool desc)
        {
            var memberInfo = Helper.Member(expression);
            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(memberInfo.Name);

            if (_expressions == null) _expressions = new List<IOrderByExpression>();
            _expressions.Add(new OrderByExpressionWithAliasImpl(new AliasExpressionImpl(alias), entityAttr) { IsDesc = desc });

            return this;
        }

        public IOrderBy<T> OrderBy(string field, bool desc)
        {
            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(field);
            if (entityAttr == null)
                throw new ArgumentException($"Cannot find attribute {field} of type {typeof(T)}");

            if (_expressions == null) _expressions = new List<IOrderByExpression>();
            _expressions.Add(new OrderByExpressionImpl(_alias, entityAttr) { IsDesc = desc });

            return this;
        }

        public IOrderBy<T> Clear()
        {
            _expressions?.Clear();
            return this;
        }

        public IOrderBy OrderBy(params IOrderBy[] orderBy)
        {
            if (_expressions == null) _expressions = new List<IOrderByExpression>();

            for (int i = 0; i < orderBy.Length; i++)
            {
                _expressions.AddRange(orderBy[i].Expressions());
            }

            return this;
        }

        public IOrderBy OrderBy(IEnumerable<IOrderBy> orderBy)
        {
            foreach (var x in orderBy)
            {
                _expressions.AddRange(x.Expressions());
            }

            return this;
        }

    }

    class OrderByExpressionImpl : IOrderByExpression
    {
        protected IAliasExpression _alias;
        protected IEntityAttribute _entityAttribute;
        protected bool _isDesc;
        public string Name => _entityAttribute.Name;

        public bool IsDesc { get => _isDesc; set => _isDesc = value; }

        public OrderByExpressionImpl(IAliasExpression alias, IEntityAttribute entityAttribute)
        {
            _alias = alias;
            _entityAttribute = entityAttribute;
        }
        public IOrderByExpression Asc()
        {
            _isDesc = false;
            return this;
        }

        public IOrderByExpression Desc()
        {
            _isDesc = true;
            return this; 
        }

        public virtual StringBuilder CompileToSQL(StringBuilder sql)
        {
            _alias.CompileToSQL(sql)
                .Append('t').Append(_entityAttribute.EntityType.Table.Index)
                .Append('.').Append(_entityAttribute.Column);
            if (_isDesc)
                sql.Append(" DESC");

            return sql;
        }
        
    }

    class OrderByExpressionWithAliasImpl : OrderByExpressionImpl
    {
        public OrderByExpressionWithAliasImpl(IAliasExpression alias, IEntityAttribute entityAttribute) : base(alias, entityAttribute)
        {
        }

        public override StringBuilder CompileToSQL(StringBuilder sql)
        {
            _alias.CompileToSQL(sql)
                .Append('.').Append(_entityAttribute.Column); ;
               
            if (_isDesc)
                sql.Append(" DESC");

            return sql;
        }
    }
}
