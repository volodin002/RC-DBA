using System;
using System.Collections.Generic;
using System.Linq;


namespace RC.DBA.Query
{
    public interface IPopertyFilterExpression<T, TProp> : IPopertyExpression<T, TProp>
    {
        Predicate Eq(TProp value);
        Predicate Eq(Parameter<TProp> prop);

        Predicate Eq<TOther>(IPopertyFilterExpression<TOther, TProp> prop);

        Predicate NotEq(TProp value);
        Predicate NotEq(Parameter<TProp> prop);

        Predicate NotEq<TOther>(IPopertyFilterExpression<TOther, TProp> prop);

        /// <summary>
        /// Greate than
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Predicate greaterThan(TProp value);
        Predicate greaterThan(Parameter<TProp> prop);
        Predicate greaterThan<TOther>(IPopertyFilterExpression<TOther, TProp> prop);

        /// <summary>
        /// Greate than or equal to ...
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Predicate greaterThanOrEq(TProp value);
        Predicate greaterThanOrEq(Parameter<TProp> prop);
        Predicate greaterThanOrEq<TOther>(IPopertyFilterExpression<TOther, TProp> prop);

        /// <summary>
        /// Less than 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Predicate lessThan(TProp value);
        Predicate lessThan(Parameter<TProp> prop);
        Predicate lessThan<TOther>(IPopertyFilterExpression<TOther, TProp> prop);

        /// <summary>
        /// Less than or equal to ...
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Predicate lessThanOrEq(TProp value);
        Predicate lessThanOrEq(Parameter<TProp> prop);
        Predicate lessThanOrEq<TOther>(IPopertyFilterExpression<TOther, TProp> prop);

        Predicate IsNull();

        Predicate IsNotNull();

        Predicate In(params TProp[] values);

        Predicate NotIn(params TProp[] values);

        Predicate In(params Parameter<TProp>[] parameters);

        Predicate In(string SQL, params Parameter<TProp>[] parameters);

        Predicate In<TOther>(ISelectQuery<TOther> query);

        Predicate NotIn<TOther>(ISelectQuery<TOther> query);

        Predicate like(string value);
        Predicate like(Parameter<TProp> prop, Func<string, string> likeExpressionFromParamName);
        Predicate like<TOther>(IPopertyFilterExpression<TOther, TProp> prop, Func<string, string> likeExpressionFromPropName);

        Predicate Operator(TProp value, string op);
    }
}
