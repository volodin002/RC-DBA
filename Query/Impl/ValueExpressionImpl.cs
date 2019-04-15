using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.DBA.Metamodel;

namespace RC.DBA.Query.Impl
{
    class ValueExpressionImpl<TValue> : IExpression
    {
        TValue _val;

        public ValueExpressionImpl(TValue val)
        {
            _val = val;
        }

        public StringBuilder CompileToSQL(StringBuilder sql)
        {
            return ValueExpressionImpl.Value(sql, _val);
        }
    }

    class ValueExpressionImpl
    {
        public static StringBuilder Value<TValue>(StringBuilder sql, TValue value)
        {
            if (typeof(TValue) == typeof(string))
                return sql.Append('\'').Append(value).Append('\'');
            else if (typeof(TValue) == typeof(bool))
                return sql.Append(value.ToString()[0] == bool.TrueString[0] ? '1' : '0');
            else
                return sql.Append(value);
        }
    }

    class ParameterExpressionImpl<TValue> : IParameterExpression<TValue>
    {
        Parameter<TValue> _param;

        public ParameterExpressionImpl(Parameter<TValue> param)
        {
            _param = param;
        }

        public Parameter Parameter => _param;

        public StringBuilder CompileToSQL(StringBuilder sql)
        {
            return sql.Append('@').Append(_param.Name);
        }

        public Parameter<TValue> GetParameter()
        {
            return _param;
        }
    }
}
