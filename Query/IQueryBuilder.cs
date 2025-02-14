using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.DBA.Query
{
    public interface IQueryBuilder
    {

        ISelectQuery<T> From<T>();

        ISelectQuery<T> From<T>(string alias);

        ISelectQuery<T> From<T>(IAliasExpression alias);

        Filter<T> Filter<T>(IAliasExpression alias);

        IOrderBy<T> OrderBy<T>(IAliasExpression alias);

        IUpdateQuery<T> Update<T>();

        IInsertValuesQuery<T> InsertValues<T>();

        IInsertFromQuery<T, TFrom> InsertFrom<T, TFrom>(ISelectQuery<TFrom> from);

        IDeleteQuery<T> Delete<T>();

        UpdateObjectCompiledQuery<T> UpdateObject<T>();
        UpdateObjectCompiledQuery<T> InsertObject<T>(bool saveId = false);
        BatchUpdateCompiledQuery<T> BatchInsertObject<T>();

        UpdateObjectCompiledQuery<T> DeleteObject<T>();

        BatchUpdateCompiledQuery<T> BatchDeleteObject<T>();

        ///// <summary>
        ///// Create a CriteriaQuery object.
        ///// </summary>
        ///// <returns></returns>
        //ICriteriaQuery<Object> createQuery();
        /// <summary>
        ///// Create a CriteriaQuery object with the specified result type.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //ICriteriaQuery<T> createQuery<T>();

        /*
        /// <summary>
        /// Create an ordering by the ascending value of the expression.
        /// </summary>
        /// <param name="x">expression used to define the ordering</param>
        /// <returns>ascending ordering corresponding to the expression</returns>
        IOrder asc(IExpression x);

        IOrder desc(IExpression x);

        IExpression<long> count(IExpression x);

        IExpression<long> countDistinct(IExpression x);

        

        /// <summary>
        /// Create a predicate testing the existence of a subquery result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subquery"></param>
        /// <returns></returns>
        Predicate exists<T>(ISubquery<T> subquery);

        Predicate and(IExpression<bool> x, IExpression<bool> y);

        Predicate and(Predicate[] restrictions);

        Predicate or(IExpression<bool> x, IExpression<bool> y);

        Predicate or(Predicate[] restrictions);

        Predicate not(IExpression<bool> restriction);

        Predicate isTrue(IExpression<bool> x);

        Predicate isFalse(IExpression<bool> x);

        Predicate isNull(IExpression x);

        Predicate isNotNull(IExpression x);

        Predicate like(IExpression<String> x, String pattern);

        Predicate notLike(IExpression<String> x, String pattern);

        IExpression<String> lower(IExpression<String> x);

        IExpression<String> upper(IExpression<String> x);

        IExpression<int> length(IExpression<String> x);


        /*

    public CriteriaQuery<Tuple> createTupleQuery();

    public <T extends Object> CriteriaUpdate<T> createCriteriaUpdate(Class<T> targetEntity);

    public <T extends Object> CriteriaDelete<T> createCriteriaDelete(Class<T> targetEntity);

    public <Y extends Object> CompoundSelection<Y> construct(Class<Y> resultClass, Selection<?>[] selections);

    public CompoundSelection<Tuple> tuple(Selection<?>[] selections);

    public CompoundSelection<Object[]> array(Selection<?>[] selections);


    public <N extends Number> Expression<Double> avg(Expression<N> x);

    public <N extends Number> Expression<N> sum(Expression<N> x);

    public Expression<Long> sumAsLong(Expression<Integer> x);

    public Expression<Double> sumAsDouble(Expression<Float> x);

    public <N extends Number> Expression<N> max(Expression<N> x);

    public <N extends Number> Expression<N> min(Expression<N> x);

    public <X extends Comparable<? super X>> Expression<X> greatest(Expression<X> x);

    public <X extends Comparable<? super X>> Expression<X> least(Expression<X> x);

    

    public <Y extends Object> Expression<Y> all(Subquery<Y> subquery);

    public <Y extends Object> Expression<Y> some(Subquery<Y> subquery);

    public <Y extends Object> Expression<Y> any(Subquery<Y> subquery);

   

    public Predicate conjunction();

    public Predicate disjunction();

    

    public Predicate equal(Expression<?> x, Expression<?> y);

    public Predicate equal(Expression<?> x, Object y);

    public Predicate notEqual(Expression<?> x, Expression<?> y);

    public Predicate notEqual(Expression<?> x, Object y);

    public <Y extends Comparable<? super Y>> Predicate greaterThan(Expression<? extends Y> x, Expression<? extends Y> y);

    public <Y extends Comparable<? super Y>> Predicate greaterThan(Expression<? extends Y> x, Y y);

    public <Y extends Comparable<? super Y>> Predicate greaterThanOrEqualTo(Expression<? extends Y> x, Expression<? extends Y> y);

    public <Y extends Comparable<? super Y>> Predicate greaterThanOrEqualTo(Expression<? extends Y> x, Y y);

    public <Y extends Comparable<? super Y>> Predicate lessThan(Expression<? extends Y> x, Expression<? extends Y> y);

    public <Y extends Comparable<? super Y>> Predicate lessThan(Expression<? extends Y> x, Y y);

    public <Y extends Comparable<? super Y>> Predicate lessThanOrEqualTo(Expression<? extends Y> x, Expression<? extends Y> y);

    public <Y extends Comparable<? super Y>> Predicate lessThanOrEqualTo(Expression<? extends Y> x, Y y);

    public <Y extends Comparable<? super Y>> Predicate between(Expression<? extends Y> v, Expression<? extends Y> x, Expression<? extends Y> y);

    public <Y extends Comparable<? super Y>> Predicate between(Expression<? extends Y> v, Y x, Y y);

    public Predicate gt(Expression<? extends Number> x, Expression<? extends Number> y);

    public Predicate gt(Expression<? extends Number> x, Number y);

    public Predicate ge(Expression<? extends Number> x, Expression<? extends Number> y);

    public Predicate ge(Expression<? extends Number> x, Number y);

    public Predicate lt(Expression<? extends Number> x, Expression<? extends Number> y);

    public Predicate lt(Expression<? extends Number> x, Number y);

    public Predicate le(Expression<? extends Number> x, Expression<? extends Number> y);

    public Predicate le(Expression<? extends Number> x, Number y);

    public <N extends Number> Expression<N> neg(Expression<N> x);

    public <N extends Number> Expression<N> abs(Expression<N> x);

    public <N extends Number> Expression<N> sum(Expression<? extends N> x, Expression<? extends N> y);

    public <N extends Number> Expression<N> sum(Expression<? extends N> x, N y);

    public <N extends Number> Expression<N> sum(N x, Expression<? extends N> y);

    public <N extends Number> Expression<N> prod(Expression<? extends N> x, Expression<? extends N> y);

    public <N extends Number> Expression<N> prod(Expression<? extends N> x, N y);

    public <N extends Number> Expression<N> prod(N x, Expression<? extends N> y);

    public <N extends Number> Expression<N> diff(Expression<? extends N> x, Expression<? extends N> y);

    public <N extends Number> Expression<N> diff(Expression<? extends N> x, N y);

    public <N extends Number> Expression<N> diff(N x, Expression<? extends N> y);

    public Expression<Number> quot(Expression<? extends Number> x, Expression<? extends Number> y);

    public Expression<Number> quot(Expression<? extends Number> x, Number y);

    public Expression<Number> quot(Number x, Expression<? extends Number> y);

    public Expression<Integer> mod(Expression<Integer> x, Expression<Integer> y);

    public Expression<Integer> mod(Expression<Integer> x, Integer y);

    public Expression<Integer> mod(Integer x, Expression<Integer> y);

    public Expression<Double> sqrt(Expression<? extends Number> x);

    public Expression<Long> toLong(Expression<? extends Number> number);

    public Expression<Integer> toInteger(Expression<? extends Number> number);

    public Expression<Float> toFloat(Expression<? extends Number> number);

    public Expression<Double> toDouble(Expression<? extends Number> number);

    public Expression<BigDecimal> toBigDecimal(Expression<? extends Number> number);

    public Expression<BigInteger> toBigInteger(Expression<? extends Number> number);

    public Expression<String> toString(Expression<Character> character);

    public <T extends Object> Expression<T> literal(T value);

    public <T extends Object> Expression<T> nullLiteral(Class<T> resultClass);

    public <T extends Object> ParameterExpression<T> parameter(Class<T> paramClass);

    public <T extends Object> ParameterExpression<T> parameter(Class<T> paramClass, String name);

    public <C extends Collection<?>> Predicate isEmpty(Expression<C> collection);

    public <C extends Collection<?>> Predicate isNotEmpty(Expression<C> collection);

    public <C extends Collection<?>> Expression<Integer> size(Expression<C> collection);

    public <C extends Collection<?>> Expression<Integer> size(C collection);

    public <E extends Object, C extends Collection<E>> Predicate isMember(Expression<E> elem, Expression<C> collection);

    public <E extends Object, C extends Collection<E>> Predicate isMember(E elem, Expression<C> collection);

    public <E extends Object, C extends Collection<E>> Predicate isNotMember(Expression<E> elem, Expression<C> collection);

    public <E extends Object, C extends Collection<E>> Predicate isNotMember(E elem, Expression<C> collection);

    public <V extends Object, M extends Map<?, V>> Expression<Collection<V>> values(M map);

    public <K extends Object, M extends Map<K, ?>> Expression<Set<K>> keys(M map);

    public Predicate like(Expression<String> x, Expression<String> pattern);

    

    public Predicate like(Expression<String> x, Expression<String> pattern, Expression<Character> escapeChar);

    public Predicate like(Expression<String> x, Expression<String> pattern, char escapeChar);

    public Predicate like(Expression<String> x, String pattern, Expression<Character> escapeChar);

    public Predicate like(Expression<String> x, String pattern, char escapeChar);

    public Predicate notLike(Expression<String> x, Expression<String> pattern);

    

    public Predicate notLike(Expression<String> x, Expression<String> pattern, Expression<Character> escapeChar);

    public Predicate notLike(Expression<String> x, Expression<String> pattern, char escapeChar);

    public Predicate notLike(Expression<String> x, String pattern, Expression<Character> escapeChar);

    public Predicate notLike(Expression<String> x, String pattern, char escapeChar);

    public Expression<String> concat(Expression<String> x, Expression<String> y);

    public Expression<String> concat(Expression<String> x, String y);

    public Expression<String> concat(String x, Expression<String> y);

    public Expression<String> substring(Expression<String> x, Expression<Integer> from);

    public Expression<String> substring(Expression<String> x, int from);

    public Expression<String> substring(Expression<String> x, Expression<Integer> from, Expression<Integer> len);

    public Expression<String> substring(Expression<String> x, int from, int len);

    public Expression<String> trim(Expression<String> x);

    public Expression<String> trim(Trimspec ts, Expression<String> x);

    public Expression<String> trim(Expression<Character> t, Expression<String> x);

    public Expression<String> trim(Trimspec ts, Expression<Character> t, Expression<String> x);

    public Expression<String> trim(char t, Expression<String> x);

    public Expression<String> trim(Trimspec ts, char t, Expression<String> x);

    

    public Expression<Integer> locate(Expression<String> x, Expression<String> pattern);

    public Expression<Integer> locate(Expression<String> x, String pattern);

    public Expression<Integer> locate(Expression<String> x, Expression<String> pattern, Expression<Integer> from);

    public Expression<Integer> locate(Expression<String> x, String pattern, int from);

    public Expression<Date> currentDate();

    public Expression<Timestamp> currentTimestamp();

    public Expression<Time> currentTime();

    public <T extends Object> In<T> in(Expression<? extends T> expression);

    public <Y extends Object> Expression<Y> coalesce(Expression<? extends Y> x, Expression<? extends Y> y);

    public <Y extends Object> Expression<Y> coalesce(Expression<? extends Y> x, Y y);

    public <Y extends Object> Expression<Y> nullif(Expression<Y> x, Expression<?> y);

    public <Y extends Object> Expression<Y> nullif(Expression<Y> x, Y y);

    public <T extends Object> Coalesce<T> coalesce();

    public <C extends Object, R extends Object> SimpleCase<C, R> selectCase(Expression<? extends C> expression);

    public <R extends Object> Case<R> selectCase();

    public <T extends Object> Expression<T> function(String name, Class<T> type, Expression<?>[] args);

    public <X extends Object, T extends Object, V extends T> Join<X, V> treat(Join<X, T> join, Class<V> type);

    public <X extends Object, T extends Object, E extends T> CollectionJoin<X, E> treat(CollectionJoin<X, T> join, Class<E> type);

    public <X extends Object, T extends Object, E extends T> SetJoin<X, E> treat(SetJoin<X, T> join, Class<E> type);

    public <X extends Object, T extends Object, E extends T> ListJoin<X, E> treat(ListJoin<X, T> join, Class<E> type);

    public <X extends Object, K extends Object, T extends Object, V extends T> MapJoin<X, K, V> treat(MapJoin<X, K, T> join, Class<V> type);

    public <X extends Object, T extends X> Path<T> treat(Path<X> path, Class<T> type);

    public <X extends Object, T extends X> Root<T> treat(Root<X> root, Class<T> type);
         */

        /*
        <N extends Number>
Expression<N>	abs(Expression<N> x)
Create an expression that returns the absolute value of its argument.
<Y> Expression<Y>	all(Subquery<Y> subquery)
Create an all expression over the subquery results.
Predicate	and(Expression<Boolean> x, Expression<Boolean> y)
Create a conjunction of the given boolean expressions.
Predicate	and(Predicate... restrictions)
Create a conjunction of the given restriction predicates.
<Y> Expression<Y>	any(Subquery<Y> subquery)
Create an any expression over the subquery results.
CompoundSelection<Object[]>	array(Selection<?>... selections)
Create an array-valued selection item.

<N extends Number>
Expression<Double>	avg(Expression<N> x)
Create an aggregate expression applying the avg operation.
<Y extends Comparable<? super Y>>
Predicate	between(Expression<? extends Y> v, Expression<? extends Y> x, Expression<? extends Y> y)
Create a predicate for testing whether the first argument is between the second and third arguments in value.
<Y extends Comparable<? super Y>>
Predicate	between(Expression<? extends Y> v, Y x, Y y)
Create a predicate for testing whether the first argument is between the second and third arguments in value.
<T> CriteriaBuilder.Coalesce<T>	coalesce()
Create a coalesce expression.
<Y> Expression<Y>	coalesce(Expression<? extends Y> x, Expression<? extends Y> y)
Create an expression that returns null if all its arguments evaluate to null, and the value of the first non-null argument otherwise.
<Y> Expression<Y>	coalesce(Expression<? extends Y> x, Y y)
Create an expression that returns null if all its arguments evaluate to null, and the value of the first non-null argument otherwise.
Expression<String>	concat(Expression<String> x, Expression<String> y)
Create an expression for string concatenation.
Expression<String>	concat(Expression<String> x, String y)
Create an expression for string concatenation.
Expression<String>	concat(String x, Expression<String> y)
Create an expression for string concatenation.
Predicate	conjunction()
Create a conjunction (with zero conjuncts).
<Y> CompoundSelection<Y>	construct(Class<Y> resultClass, Selection<?>... selections)
Create a selection item corresponding to a constructor.
Expression<Long>	count(Expression<?> x)
Create an aggregate expression applying the count operation.
Expression<Long>	countDistinct(Expression<?> x)
Create an aggregate expression applying the count distinct operation.
<T> CriteriaDelete<T>	createCriteriaDelete(Class<T> targetEntity)
Create a CriteriaDelete query object to perform a bulk delete operation.
<T> CriteriaUpdate<T>	createCriteriaUpdate(Class<T> targetEntity)
Create a CriteriaUpdate query object to perform a bulk update operation.

CriteriaQuery<Tuple>	createTupleQuery()
Create a CriteriaQuery object that returns a tuple of objects as its result.
Expression<Date>	currentDate()
Create expression to return current date.
Expression<Time>	currentTime()
Create expression to return current time.
Expression<Timestamp>	currentTimestamp()
Create expression to return current timestamp.
Order	desc(Expression<?> x)
Create an ordering by the descending value of the expression.
<N extends Number>
Expression<N>	diff(Expression<? extends N> x, Expression<? extends N> y)
Create an expression that returns the difference between its arguments.
<N extends Number>
Expression<N>	diff(Expression<? extends N> x, N y)
Create an expression that returns the difference between its arguments.
<N extends Number>
Expression<N>	diff(N x, Expression<? extends N> y)
Create an expression that returns the difference between its arguments.
Predicate	disjunction()
Create a disjunction (with zero disjuncts).
Predicate	equal(Expression<?> x, Expression<?> y)
Create a predicate for testing the arguments for equality.
Predicate	equal(Expression<?> x, Object y)
Create a predicate for testing the arguments for equality.

<T> Expression<T>	function(String name, Class<T> type, Expression<?>... args)
Create an expression for the execution of a database function.
Predicate	ge(Expression<? extends Number> x, Expression<? extends Number> y)
Create a predicate for testing whether the first argument is greater than or equal to the second.
Predicate	ge(Expression<? extends Number> x, Number y)
Create a predicate for testing whether the first argument is greater than or equal to the second.
<Y extends Comparable<? super Y>>
Predicate	greaterThan(Expression<? extends Y> x, Expression<? extends Y> y)
Create a predicate for testing whether the first argument is greater than the second.
<Y extends Comparable<? super Y>>
Predicate	greaterThan(Expression<? extends Y> x, Y y)
Create a predicate for testing whether the first argument is greater than the second.
<Y extends Comparable<? super Y>>
Predicate	greaterThanOrEqualTo(Expression<? extends Y> x, Expression<? extends Y> y)
Create a predicate for testing whether the first argument is greater than or equal to the second.
<Y extends Comparable<? super Y>>
Predicate	greaterThanOrEqualTo(Expression<? extends Y> x, Y y)
Create a predicate for testing whether the first argument is greater than or equal to the second.
<X extends Comparable<? super X>>
Expression<X>	greatest(Expression<X> x)
Create an aggregate expression for finding the greatest of the values (strings, dates, etc).
Predicate	gt(Expression<? extends Number> x, Expression<? extends Number> y)
Create a predicate for testing whether the first argument is greater than the second.
Predicate	gt(Expression<? extends Number> x, Number y)
Create a predicate for testing whether the first argument is greater than the second.
<T> CriteriaBuilder.In<T>	in(Expression<? extends T> expression)
Create predicate to test whether given expression is contained in a list of values.
<C extends Collection<?>>
Predicate	isEmpty(Expression<C> collection)
Create a predicate that tests whether a collection is empty.
Predicate	isFalse(Expression<Boolean> x)
Create a predicate testing for a false value.
<E,C extends Collection<E>>
Predicate	isMember(E elem, Expression<C> collection)
Create a predicate that tests whether an element is a member of a collection.
<E,C extends Collection<E>>
Predicate	isMember(Expression<E> elem, Expression<C> collection)
Create a predicate that tests whether an element is a member of a collection.
<C extends Collection<?>>
Predicate	isNotEmpty(Expression<C> collection)
Create a predicate that tests whether a collection is not empty.
<E,C extends Collection<E>>
Predicate	isNotMember(E elem, Expression<C> collection)
Create a predicate that tests whether an element is not a member of a collection.
<E,C extends Collection<E>>
Predicate	isNotMember(Expression<E> elem, Expression<C> collection)
Create a predicate that tests whether an element is not a member of a collection.
Predicate	isNotNull(Expression<?> x)
Create a predicate to test whether the expression is not null.
Predicate	isNull(Expression<?> x)
Create a predicate to test whether the expression is null.
Predicate	isTrue(Expression<Boolean> x)
Create a predicate testing for a true value.
<K,M extends Map<K,?>>
Expression<Set<K>>	keys(M map)
Create an expression that returns the keys of a map.
Predicate	le(Expression<? extends Number> x, Expression<? extends Number> y)
Create a predicate for testing whether the first argument is less than or equal to the second.
Predicate	le(Expression<? extends Number> x, Number y)
Create a predicate for testing whether the first argument is less than or equal to the second.
<X extends Comparable<? super X>>
Expression<X>	least(Expression<X> x)
Create an aggregate expression for finding the least of the values (strings, dates, etc).
Expression<Integer>	length(Expression<String> x)
Create expression to return length of a string.
<Y extends Comparable<? super Y>>
Predicate	lessThan(Expression<? extends Y> x, Expression<? extends Y> y)
Create a predicate for testing whether the first argument is less than the second.
<Y extends Comparable<? super Y>>
Predicate	lessThan(Expression<? extends Y> x, Y y)
Create a predicate for testing whether the first argument is less than the second.
<Y extends Comparable<? super Y>>
Predicate	lessThanOrEqualTo(Expression<? extends Y> x, Expression<? extends Y> y)
Create a predicate for testing whether the first argument is less than or equal to the second.
<Y extends Comparable<? super Y>>
Predicate	lessThanOrEqualTo(Expression<? extends Y> x, Y y)
Create a predicate for testing whether the first argument is less than or equal to the second.
Predicate	like(Expression<String> x, Expression<String> pattern)
Create a predicate for testing whether the expression satisfies the given pattern.
Predicate	like(Expression<String> x, Expression<String> pattern, char escapeChar)
Create a predicate for testing whether the expression satisfies the given pattern.
Predicate	like(Expression<String> x, Expression<String> pattern, Expression<Character> escapeChar)
Create a predicate for testing whether the expression satisfies the given pattern.
Predicate	like(Expression<String> x, String pattern)
Create a predicate for testing whether the expression satisfies the given pattern.
Predicate	like(Expression<String> x, String pattern, char escapeChar)
Create a predicate for testing whether the expression satisfies the given pattern.
Predicate	like(Expression<String> x, String pattern, Expression<Character> escapeChar)
Create a predicate for testing whether the expression satisfies the given pattern.
<T> Expression<T>	literal(T value)
Create an expression for a literal.
Expression<Integer>	locate(Expression<String> x, Expression<String> pattern)
Create expression to locate the position of one string within another, returning position of first character if found.
Expression<Integer>	locate(Expression<String> x, Expression<String> pattern, Expression<Integer> from)
Create expression to locate the position of one string within another, returning position of first character if found.
Expression<Integer>	locate(Expression<String> x, String pattern)
Create expression to locate the position of one string within another, returning position of first character if found.
Expression<Integer>	locate(Expression<String> x, String pattern, int from)
Create expression to locate the position of one string within another, returning position of first character if found.
Expression<String>	lower(Expression<String> x)
Create expression for converting a string to lowercase.
Predicate	lt(Expression<? extends Number> x, Expression<? extends Number> y)
Create a predicate for testing whether the first argument is less than the second.
Predicate	lt(Expression<? extends Number> x, Number y)
Create a predicate for testing whether the first argument is less than the second.
<N extends Number>
Expression<N>	max(Expression<N> x)
Create an aggregate expression applying the numerical max operation.
<N extends Number>
Expression<N>	min(Expression<N> x)
Create an aggregate expression applying the numerical min operation.
Expression<Integer>	mod(Expression<Integer> x, Expression<Integer> y)
Create an expression that returns the modulus of its arguments.
Expression<Integer>	mod(Expression<Integer> x, Integer y)
Create an expression that returns the modulus of its arguments.
Expression<Integer>	mod(Integer x, Expression<Integer> y)
Create an expression that returns the modulus of its arguments.
<N extends Number>
Expression<N>	neg(Expression<N> x)
Create an expression that returns the arithmetic negation of its argument.
Predicate	not(Expression<Boolean> restriction)
Create a negation of the given restriction.
Predicate	notEqual(Expression<?> x, Expression<?> y)
Create a predicate for testing the arguments for inequality.
Predicate	notEqual(Expression<?> x, Object y)
Create a predicate for testing the arguments for inequality.
Predicate	notLike(Expression<String> x, Expression<String> pattern)
Create a predicate for testing whether the expression does not satisfy the given pattern.
Predicate	notLike(Expression<String> x, Expression<String> pattern, char escapeChar)
Create a predicate for testing whether the expression does not satisfy the given pattern.
Predicate	notLike(Expression<String> x, Expression<String> pattern, Expression<Character> escapeChar)
Create a predicate for testing whether the expression does not satisfy the given pattern.
Predicate	notLike(Expression<String> x, String pattern)
Create a predicate for testing whether the expression does not satisfy the given pattern.
Predicate	notLike(Expression<String> x, String pattern, char escapeChar)
Create a predicate for testing whether the expression does not satisfy the given pattern.
Predicate	notLike(Expression<String> x, String pattern, Expression<Character> escapeChar)
Create a predicate for testing whether the expression does not satisfy the given pattern.
<Y> Expression<Y>	nullif(Expression<Y> x, Expression<?> y)
Create an expression that tests whether its argument are equal, returning null if they are and the value of the first expression if they are not.
<Y> Expression<Y>	nullif(Expression<Y> x, Y y)
Create an expression that tests whether its argument are equal, returning null if they are and the value of the first expression if they are not.
<T> Expression<T>	nullLiteral(Class<T> resultClass)
Create an expression for a null literal with the given type.
Predicate	or(Expression<Boolean> x, Expression<Boolean> y)
Create a disjunction of the given boolean expressions.
Predicate	or(Predicate... restrictions)
Create a disjunction of the given restriction predicates.
<T> ParameterExpression<T>	parameter(Class<T> paramClass)
Create a parameter expression.
<T> ParameterExpression<T>	parameter(Class<T> paramClass, String name)
Create a parameter expression with the given name.
<N extends Number>
Expression<N>	prod(Expression<? extends N> x, Expression<? extends N> y)
Create an expression that returns the product of its arguments.
<N extends Number>
Expression<N>	prod(Expression<? extends N> x, N y)
Create an expression that returns the product of its arguments.
<N extends Number>
Expression<N>	prod(N x, Expression<? extends N> y)
Create an expression that returns the product of its arguments.
Expression<Number>	quot(Expression<? extends Number> x, Expression<? extends Number> y)
Create an expression that returns the quotient of its arguments.
Expression<Number>	quot(Expression<? extends Number> x, Number y)
Create an expression that returns the quotient of its arguments.
Expression<Number>	quot(Number x, Expression<? extends Number> y)
Create an expression that returns the quotient of its arguments.
<R> CriteriaBuilder.Case<R>	selectCase()
Create a general case expression.
<C,R> CriteriaBuilder.SimpleCase<C,R>	selectCase(Expression<? extends C> expression)
Create a simple case expression.
<C extends Collection<?>>
Expression<Integer>	size(C collection)
Create an expression that tests the size of a collection.
<C extends Collection<?>>
Expression<Integer>	size(Expression<C> collection)
Create an expression that tests the size of a collection.
<Y> Expression<Y>	some(Subquery<Y> subquery)
Create a some expression over the subquery results.
Expression<Double>	sqrt(Expression<? extends Number> x)
Create an expression that returns the square root of its argument.
Expression<String>	substring(Expression<String> x, Expression<Integer> from)
Create an expression for substring extraction.
Expression<String>	substring(Expression<String> x, Expression<Integer> from, Expression<Integer> len)
Create an expression for substring extraction.
Expression<String>	substring(Expression<String> x, int from)
Create an expression for substring extraction.
Expression<String>	substring(Expression<String> x, int from, int len)
Create an expression for substring extraction.
<N extends Number>
Expression<N>	sum(Expression<? extends N> x, Expression<? extends N> y)
Create an expression that returns the sum of its arguments.
<N extends Number>
Expression<N>	sum(Expression<? extends N> x, N y)
Create an expression that returns the sum of its arguments.
<N extends Number>
Expression<N>	sum(Expression<N> x)
Create an aggregate expression applying the sum operation.
<N extends Number>
Expression<N>	sum(N x, Expression<? extends N> y)
Create an expression that returns the sum of its arguments.
Expression<Double>	sumAsDouble(Expression<Float> x)
Create an aggregate expression applying the sum operation to a Float-valued expression, returning a Double result.
Expression<Long>	sumAsLong(Expression<Integer> x)
Create an aggregate expression applying the sum operation to an Integer-valued expression, returning a Long result.
Expression<BigDecimal>	toBigDecimal(Expression<? extends Number> number)
Typecast.
Expression<BigInteger>	toBigInteger(Expression<? extends Number> number)
Typecast.
Expression<Double>	toDouble(Expression<? extends Number> number)
Typecast.
Expression<Float>	toFloat(Expression<? extends Number> number)
Typecast.
Expression<Integer>	toInteger(Expression<? extends Number> number)
Typecast.
Expression<Long>	toLong(Expression<? extends Number> number)
Typecast.
Expression<String>	toString(Expression<Character> character)
Typecast.
<X,T,E extends T>
CollectionJoin<X,E>	treat(CollectionJoin<X,T> join, Class<E> type)
Downcast CollectionJoin object to the specified type.
<X,T,V extends T>
Join<X,V>	treat(Join<X,T> join, Class<V> type)
Downcast Join object to the specified type.
<X,T,E extends T>
ListJoin<X,E>	treat(ListJoin<X,T> join, Class<E> type)
Downcast ListJoin object to the specified type.
<X,K,T,V extends T>
MapJoin<X,K,V>	treat(MapJoin<X,K,T> join, Class<V> type)
Downcast MapJoin object to the specified type.
<X,T extends X>
Path<T>	treat(Path<X> path, Class<T> type)
Downcast Path object to the specified type.
<X,T extends X>
Root<T>	treat(Root<X> root, Class<T> type)
Downcast Root object to the specified type.
<X,T,E extends T>
SetJoin<X,E>	treat(SetJoin<X,T> join, Class<E> type)
Downcast SetJoin object to the specified type.
Expression<String>	trim(char t, Expression<String> x)
Create expression to trim character from both ends of a string.
Expression<String>	trim(CriteriaBuilder.Trimspec ts, char t, Expression<String> x)
Create expression to trim character from a string.
Expression<String>	trim(CriteriaBuilder.Trimspec ts, Expression<Character> t, Expression<String> x)
Create expression to trim character from a string.
Expression<String>	trim(CriteriaBuilder.Trimspec ts, Expression<String> x)
Create expression to trim blanks from a string.
Expression<String>	trim(Expression<Character> t, Expression<String> x)
Create expression to trim character from both ends of a string.
Expression<String>	trim(Expression<String> x)
Create expression to trim blanks from both ends of a string.
CompoundSelection<Tuple>	tuple(Selection<?>... selections)
Create a tuple-valued selection item.
Expression<String>	upper(Expression<String> x)
Create expression for converting a string to uppercase.
<V,M extends Map<?,V>>
Expression<Collection<V>>	values(M map)
Create an expression that returns the values of a map.
         */
    }
}
