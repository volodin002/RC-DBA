using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RC.DBA.Query.Impl
{
    class UpdateQueryImpl<T> : QueryImpl<T>, IUpdateQuery<T>
    {
        private List<IUpdateSetExpression> _sets;
        public UpdateQueryImpl(QueryContext ctx, IAliasExpression alias) : base(ctx, alias)
        {
            _sets = new List<IUpdateSetExpression>();
        }

        public IUpdateQuery<T> Set<TProp>(Expression<Func<T, TProp>> expression, TProp value)
        {
            
            var memberInfo = Helper.Member(expression);
            if (memberInfo == null)
                memberInfo = Helper.MemberOfTypeCall(expression);

            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(memberInfo.Name);

            var prop = new PopertyExpression(_alias, entityAttr);

            _sets.Add(new UpdateSetExpressionImpl(prop, new ValueExpressionImpl<TProp>(value)));

            return this;
        }

        public IUpdateQuery<T> Set<TProp>(Expression<Func<T, TProp>> expression, Parameter<TProp> param)
        {
            var memberInfo = Helper.Member(expression);
            if (memberInfo == null)
                memberInfo = Helper.MemberOfTypeCall(expression);

            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(memberInfo.Name);

            var prop = new PopertyExpression(_alias, entityAttr);

            _sets.Add(new UpdateSetExpressionImpl(prop, new ParameterExpressionImpl<TProp>(param)));

            return this;
        }

        public IUpdateQuery<T> Set<TProp>(Expression<Func<T, TProp>> expression, IPopertyExpression<T, TProp> propertyExpression)
        {
            var memberInfo = Helper.Member(expression);
            var entityAttr = _ctx.ModelManager.Entity<T>().GetAttribute(memberInfo.Name);

            var prop = new PopertyExpression(_alias, entityAttr);

            _sets.Add(new UpdateSetExpressionImpl(prop, propertyExpression));

            return this;
        }

        public override StringBuilder CompileToSQL(StringBuilder sql)
        {
            sql.Append("UPDATE ");
            _From.Table.CompileAlias(sql, _alias)
                .AppendLine(" SET");

            _sets[0].CompileToSQL(sql);
            for (int i = 1; i < _sets.Count; i++)
            {
                _sets[i].CompileToSQL(sql.AppendLine(","));
            }

            sql.AppendLine()
                .Append("FROM ")
                .Append(_From.Table.TableName)
                .Append(' ');
            _From.Table.CompileAlias(sql, _alias);

            CompileJoinsToSQL(sql);
            CompileFilterToSQL(sql);

            return sql;
        }

        public override SqlQuery<T> Compile()
        {
            var sql = CompileToSQL(new StringBuilder());

            var filterParameters = Filter().Parameters;
            var setsParameters = _sets.Where(s => s.Parameters != null).SelectMany(s => s.Parameters);

            Parameter[] parameters;
            if (filterParameters == null)
                parameters = setsParameters.ToArray();
            else
                parameters = Enumerable.Concat(filterParameters, setsParameters).ToArray();

            return new SqlQuery<T>(sql.ToString(), parameters);
        }
    }

    class UpdateSetExpressionImpl : IUpdateSetExpression
    {
        Parameter[] _param;
        IPopertyExpression _propExpression;
        IExpression _valueExpression;

        public UpdateSetExpressionImpl(IPopertyExpression propExpression, IExpression valueExpression)
        {
            _propExpression = propExpression;
            _valueExpression = valueExpression;
            var paramExpression = valueExpression as IParameterExpression;
            if (paramExpression != null && paramExpression.Parameter != null)
            {
                _param = new Parameter[] { paramExpression.Parameter };
            }
        }

        public UpdateSetExpressionImpl(IPopertyExpression propExpression, IExpression valueExpression, params Parameter[] parameters)
            :this(propExpression, valueExpression)
        {
            if (_param != null && parameters.Length > 0)
            {
                var newParams = new Parameter[_param.Length + parameters.Length];
                _param.CopyTo(newParams, 0);
                parameters.CopyTo(newParams, _param.Length);
                _param = newParams;
            }
            else
                _param = parameters;
        }

        public IEnumerable<Parameter> Parameters => _param;


        public StringBuilder CompileToSQL(StringBuilder sql)
        {
            _propExpression.CompileToSQL(sql).Append("=");
            _valueExpression.CompileToSQL(sql);
            return sql;
        }

    }
}
