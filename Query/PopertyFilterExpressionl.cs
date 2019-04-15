using RC.DBA.Metamodel;
using RC.DBA.Query.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.DBA.Query
{
    #pragma warning disable CS0660 // Warning CS0660  'PopertyFilterExpression<T, TProp>' defines operator == or operator != but does not override Object.Equals(object o) 
    #pragma warning disable CS0661 // Warning CS0661  'PopertyFilterExpression<T, TProp>' defines operator == or operator != but does not override Object.GetHashCode() 

    public class PopertyFilterExpression<T, TProp> : PopertyExpression, IPopertyFilterExpression<T, TProp>
    {
        public PopertyFilterExpression(IAliasExpression alias, IEntityAttribute entityAttribute) : base(alias, entityAttribute)
        {   
        }

        #region IPopertyFilterExpression
        public Predicate Eq(TProp value)
        {
            return Operator(value, "=");
        }

        public Predicate Eq(Parameter<TProp> parameter)
        {
            if (_entityAttribute.MaxLength > 0)
                parameter.Size = _entityAttribute.MaxLength;

            return Operator(parameter, "=");
        }

        public Predicate Eq<TOther>(IPopertyFilterExpression<TOther, TProp> prop)
        {
            return Operator(prop, "=");
        }

        public Predicate NotEq(TProp value)
        {
            return Operator(value, "<>");
        }

        public Predicate NotEq(Parameter<TProp> prop)
        {
            return Operator(prop, "<>");
        }

        public Predicate NotEq<TOther>(IPopertyFilterExpression<TOther, TProp> prop)
        {
            return Operator(prop, "<>");
        }


        public Predicate greaterThan(TProp value)
        {
            return Operator(value, ">");
        }

        public Predicate greaterThan(Parameter<TProp> prop)
        {
            return Operator(prop, ">");
        }

        public Predicate greaterThan<TOther>(IPopertyFilterExpression<TOther, TProp> prop)
        {
            return Operator(prop, ">");
        }

        public Predicate greaterThanOrEq(TProp value)
        {
            return Operator(value, ">=");
        }

        public Predicate greaterThanOrEq(Parameter<TProp> prop)
        {
            return Operator(prop, ">=");
        }

        public Predicate greaterThanOrEq<TOther>(IPopertyFilterExpression<TOther, TProp> prop)
        {
            return Operator(prop, ">=");
        }

        public Predicate In(params TProp[] values)
        {
            if (values == null || values.Length == 0) return null;

            return new PredicateImpl(sql =>
            {
                CompileToSQL(sql)
                .Append(" IN (");

                ValueExpressionImpl.Value(sql, values[0]);
                for (int i = 1; i < values.Length; i++)
                {
                    ValueExpressionImpl.Value(sql.Append(','), values[i]);
                }

                sql.Append(')');
            }, null);
        }

        public Predicate In(params Parameter<TProp>[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return null;

            return new PredicateImpl(sql =>
            {
                CompileToSQL(sql)
                .Append(" IN (");

                sql.Append('@').Append(parameters[0].Name);
                for (int i = 1; i < parameters.Length; i++)
                {
                    sql.Append(",@").Append(parameters[i].Name);
                }

                sql.Append(')');

            }, parameters, null);
        }

        public Predicate In(string SQL, params Parameter<TProp>[] parameters)
        {
            return new PredicateImpl(sql =>
            {
                CompileToSQL(sql).Append(" IN (").Append(SQL).Append(')');

            }, parameters, null);
        }

        public Predicate IsNotNull()
        {
            return new PredicateImpl(sql =>
            {
                CompileToSQL(sql).Append(" is not NULL");
            }, null);
        }

        public Predicate IsNull()
        {
            return new PredicateImpl(sql =>
            {
                CompileToSQL(sql).Append(" is NULL");
            }, null);
        }

        public Predicate lessThan(TProp value)
        {
            return Operator(value, "<");
        }

        public Predicate lessThan(Parameter<TProp> prop)
        {
            return Operator(prop, "<");
        }

        public Predicate lessThan<TOther>(IPopertyFilterExpression<TOther, TProp> prop)
        {
            return Operator(prop, "<");
        }

        public Predicate lessThanOrEq(TProp value)
        {
            return Operator(value, "<=");
        }

        public Predicate lessThanOrEq(Parameter<TProp> prop)
        {
            return Operator(prop, "<=");
        }

        public Predicate lessThanOrEq<TOther>(IPopertyFilterExpression<TOther, TProp> prop)
        {
            return Operator(prop, "<=");
        }

        #endregion // IPopertyFilterExpression

        private Predicate Operator(TProp value, string op)
        {
            return new PredicateImpl(sql =>
            {
                CompileToSQL(sql)
                .Append(op);

                ValueExpressionImpl.Value(sql, value);
            }, new EntityTable[] { _entityAttribute.EntityType.Table });
        }
        private Predicate Operator(bool value, string op)
        {
            return new PredicateImpl(sql =>
            {
                CompileToSQL(sql)
                .Append(op);

                ValueExpressionImpl.Value(sql, value);
            }, new EntityTable[] { _entityAttribute.EntityType.Table });
        }
        private Predicate Operator(Parameter<TProp> param, string op)
        {
            return new PredicateImpl(sql =>
            {
                CompileToSQL(sql)
                .Append(op)
                .Append('@')
                .Append(param.Name);

            }, param, new EntityTable[] { _entityAttribute.EntityType.Table });
        }

        private Predicate Operator<TOther>(IPopertyFilterExpression<TOther, TProp> prop, string op)
        {
            return new PredicateImpl(sql =>
            {
                CompileToSQL(sql)
                .Append(op);
                prop.CompileToSQL(sql);
            }, new EntityTable[] { _entityAttribute.EntityType.Table });
        }

        #region Operators overload

        public static Predicate operator ==(PopertyFilterExpression<T, TProp> prop, Parameter<TProp> parameter)
        {
            return prop.Eq(parameter);
        }

        public static Predicate operator !=(PopertyFilterExpression<T, TProp> prop, Parameter<TProp> parameter)
        {
            return prop.NotEq(parameter);
        }

        #endregion // Operators overload

    }
}
