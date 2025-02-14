using System;
using System.CodeDom;
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
            if (value == null)
                return sql.Append("NULL");
            else if (value is string sValue)
                return sql.Append('\'').Append(sValue).Append('\'');
            else if (value is bool bValue)
                return sql.Append(bValue ? '1' : '0');
            else if (value is DateTime dtValue)
                return sql.Append('\'').Append(dtValue.ToString("s")).Append('\'');
            else if (value is char cValue)
                return sql.Append('\'').Append(cValue).Append('\'');
            else
                return sql.Append(value);

            /*
            var type = typeof(TValue);
            if (value == null)
                return sql.Append("NULL");
            else if (type == typeof(string))
                return sql.Append('\'').Append(value).Append('\'');
            else if (type == typeof(bool) || type == typeof(bool?))
                return sql.Append(value.ToString()[0] == bool.TrueString[0] ? '1' : '0');
            else if (value is DateTime dt) 
                return sql.Append('\'').Append(dt.ToString("s")).Append('\'');
            else
                return sql.Append(value);
            */
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
