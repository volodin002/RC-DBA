using RC.DBA.Metamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RC.DBA.Query.Impl
{
    class InsertValuesQueryImpl<T> : IInsertValuesQuery<T>
    {
        protected IModelManager _modelManager;

        protected List<Tuple<IExpression, IExpression>> _values;
        protected List<Parameter> _parameters;

        protected bool _ReturnIdentity;

        public InsertValuesQueryImpl(IModelManager modelManager)
        {
            _modelManager = modelManager;
            _values = new List<Tuple<IExpression, IExpression>>();
            _parameters = new List<Parameter>();
        }
        public virtual SqlQuery<T> Compile()
        {
            var sql = CompileToSQL(new StringBuilder());

            return new SqlQuery<T>(sql.ToString(), _parameters.ToArray());
        }

        public virtual StringBuilder CompileToSQL(StringBuilder sql)
        {
            var entity = _modelManager.Entity<T>();
            sql.Append("INSERT INTO ").Append(entity.Table.TableName).Append("(");

            _values[0].Item1.CompileToSQL(sql);
            for (int i = 1; i < _values.Count; i++)
            {
                sql.Append(',');
                _values[i].Item1.CompileToSQL(sql);
            }
            sql.Append(')');

            sql.Append(" VALUES (");
            _values[0].Item2.CompileToSQL(sql);
            for (int i = 1; i < _values.Count; i++)
            {
                sql.Append(',');
                _values[i].Item2.CompileToSQL(sql);
            }

            sql.Append(')');

            if(_ReturnIdentity)
            {
                sql.AppendLine().Append("SELECT SCOPE_IDENTITY()");
            }

            return sql;
        }

        public IInsertValuesQuery<T> ReturnIdentity<TProp>()
        {
            _ReturnIdentity = true;
            return this;
        }

        public IInsertValuesQuery<T> Value<TProp>(Expression<Func<T, TProp>> expression, TProp value)
        {
            var memberInfo = Helper.Member(expression);
            var entityAttr = _modelManager.Entity<T>().GetAttribute(memberInfo.Name);

            _values.Add(Tuple.Create<IExpression, IExpression>(new NameExpressionImpl(entityAttr.Column), new ValueExpressionImpl<TProp>(value)));

            return this;
        }

        public IInsertValuesQuery<T> Value<TProp>(Expression<Func<T, TProp>> expression, Parameter<TProp> param)
        {
            var memberInfo = Helper.Member(expression);
            var entityAttr = _modelManager.Entity<T>().GetAttribute(memberInfo.Name);

            _values.Add(Tuple.Create<IExpression, IExpression>(new NameExpressionImpl(entityAttr.Column), new ParameterExpressionImpl<TProp>(param)));
            _parameters.Add(param);

            return this;
        }
    }

    class InsertFromQueryImpl<T, TFrom> : InsertValuesQueryImpl<T>, IInsertFromQuery<T, TFrom>
    {
        ISelectQuery<TFrom> _from;
        public InsertFromQueryImpl(IModelManager modelManager, ISelectQuery<TFrom> from) : base(modelManager)
        {
            _from = from;
        }

        public override SqlQuery<T> Compile()
        {
            var sql = CompileToSQL(new StringBuilder());

            var fromParameters = _from.Filter().Parameters ?? Enumerable.Empty<Parameter>();

            return new SqlQuery<T>(sql.ToString(), fromParameters.Union(_parameters).ToArray());
        }

        public override StringBuilder CompileToSQL(StringBuilder sql)
        {
            var returnIdentity = _ReturnIdentity;
            if (returnIdentity) _ReturnIdentity = false;

            base.CompileToSQL(sql).AppendLine();

            _from.CompileToSQL(sql);

            if (returnIdentity)
            {
                sql.AppendLine().Append("SELECT SCOPE_IDENTITY()");
            }

            return sql;
        }

        public IInsertFromQuery<T, TFrom> Value<TProp>(Expression<Func<T, TProp>> expression, IPopertyExpression<TFrom, TProp> propertyExpression)
        {
            var memberInfo = Helper.Member(expression);
            var entityAttr = _modelManager.Entity<T>().GetAttribute(memberInfo.Name);

            _values.Add(Tuple.Create<IExpression, IExpression>(new NameExpressionImpl(entityAttr.Column), propertyExpression));

            return this;
        }
    }
}
