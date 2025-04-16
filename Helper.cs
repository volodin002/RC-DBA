using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RC.DBA
{
    public static class Helper
    {
        #region Linq
        public static void Apply<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
                action(item);
        }

        public static bool SafeAny<T>(this IEnumerable<T> items)
        {
            return items != null && items.Any();
        }

        public static bool SafeAny<TSource>(this IEnumerable<TSource> items, Func<TSource, bool> predicate)
        {
            return items != null && items.Any(predicate);
        }

        public static IEnumerable<TSource> SafeExcept<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            return (first ?? Enumerable.Empty<TSource>()).Except(second ?? Enumerable.Empty<TSource>(), comparer);
        }

        #endregion // Linq

        #region Expressions
        public static MemberInfo Member<T, TMember>(Expression<Func<T, TMember>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                return null;

            return memberExpression.Member;
        }

        public static MemberInfo MemberOfTypeCall<T, TMember>(Expression<Func<T, TMember>> expression)
        {
            var unaryEpression = expression.Body as UnaryExpression; // (x => x.Property as string)
            if (unaryEpression != null)
            {
                var memberExpression = unaryEpression.Operand as MemberExpression;
                
                if (memberExpression != null) return memberExpression.Member;

            }

            var callExpression = expression.Body as MethodCallExpression; // (x => x.Property.OfType())
            if (callExpression != null && callExpression.Method.Name == "OfType")
            {
                var arg = callExpression.Arguments[0];
                var memberExpression = arg as MemberExpression;
                if (memberExpression == null) return null;

                return memberExpression.Member;
            }

            return null;
        }

        public static PropertyInfo Property<T, TProp>(Expression<Func<T, TProp>> expression) => Member(expression) as PropertyInfo;
        

        public static FieldInfo Field<T, TField>(Expression<Func<T, TField>> expression) => Member(expression) as FieldInfo;


        public static PropertyInfo PropertyOfTypeCall<T, TProp>(Expression<Func<T, TProp>> expression) => MemberOfTypeCall(expression) as PropertyInfo;
        

        public static FieldInfo FieldOfTypeCall<T, TProp>(Expression<Func<T, TProp>> expression) => MemberOfTypeCall(expression) as FieldInfo;
       

        public static MethodInfo Method<T, TMember>(Expression<Func<T, TMember>> expression)
        {
            var methodCallExpression = expression.Body as MethodCallExpression;
            if (methodCallExpression == null)
                return null;

            return methodCallExpression.Method;
        }

        #endregion // Expressions

        #region Reflection

        /// <summary>
        /// Can value of this type be equal to null.
        /// (Is it reference type of Nullable<T>)
        /// </summary>
        internal static bool IsTypeCanBeNull(Type type)
        {
            return !type.IsValueType || IsNullableType(type);
        }

        /// <summary>
        /// Check is type is Nullable<T>
        /// </summary>
        /// <param name="type"></param>
        public static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// If type is Nullable<T> return typeof(T) else return type himself.
        /// </summary>
        public static Type GetNonNullableType(Type type)
        {
            return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
        }

        internal static bool IsBaseType(Type type, Type isBase)
        {
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType == isBase) return true;
                baseType = baseType.BaseType;
            }

            return false;
        }

        #endregion // Reflection


        public static bool IsEquals<T1, T2>(T1 x1, T2 x2)
        {
            if (x1 == null && x2 == null) return true;

            if (x1 != null && x2 != null)
            {
                if (x1.GetType() == x2.GetType()) return x1.Equals(x2);

                return x1.ToString() == x2.ToString();
            }

            return false;
        }

        public static int GetHash<T1, T2>(T1 x1, T2 x2)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17 * 23 + (x1 != null ? x1.GetHashCode() : 0);
                return hash * 23 + (x2 != null ? x1.GetHashCode() : 0);
            }
        }

        public static int GetHash<T1, T2, T3>(T1 x1, T2 x2, T3 x3)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17 * 23 + (x1 != null ? x1.GetHashCode() : 0);
                hash = hash * 23 + (x2 != null ? x2.GetHashCode() : 0);
                return hash * 23 + (x3 != null ? x3.GetHashCode() : 0);
            }
        }

        public static IEqualityComparer<T> GetEqualityComparer<T>(Func<T, T, bool> compareFunction, Func<T, int> hashFunction)
        {
            return new GenericEqualityComparer<T>(compareFunction, hashFunction);
        }

        public static IEqualityComparer<T> GetEqualityComparer<T, TVal>(Func<T, TVal> valueFactory)
        {
            return new ValueEqualityComparer<T, TVal>(valueFactory);
        }

        class GenericEqualityComparer<T> : IEqualityComparer<T>
        {
            Func<T, T, bool> _compareFunction;
            Func<T, int> _hashFunction;

            public GenericEqualityComparer(Func<T, T, bool> compareFunction, Func<T, int> hashFunction)
            {
                _compareFunction = compareFunction;
                _hashFunction = hashFunction;
            }

            public bool Equals(T x, T y)
            {
                return _compareFunction(x, y);
            }

            public int GetHashCode(T obj)
            {
                return _hashFunction(obj);
            }
        }

        class ValueEqualityComparer<T, TVal> : IEqualityComparer<T>
        {

            Func<T, TVal> _valueFactory;

            public ValueEqualityComparer(Func<T, TVal> valueFactory)
            {
                _valueFactory = valueFactory;
            }

            public bool Equals(T x, T y)
            {
                return GetHashCode(x) == GetHashCode(y);
            }

            public int GetHashCode(T obj)
            {
                TVal val = _valueFactory(obj);
                if (val == null) return 0;

                return val.GetHashCode();
            }
        }

        public static byte[] GetBytesBufferFromReader(DbDataReader reader, int ordinal)
        {
            long offset = 0;
            const int bufferSize = 64;
            var buffer = new byte[bufferSize];
            long bytesRead;

            var stream = new MemoryStream();
            while ((bytesRead = reader.GetBytes(ordinal, offset, buffer, 0, bufferSize)) > 0)
            {
                stream.Write(buffer, 0, (int)bytesRead);

                offset += bytesRead;
            }

            return stream.ToArray();
        }

        public static bool IsAssociationPropertyType(Type type)
        {
            var notNullabeType = Helper.GetNonNullableType(type);
            return
                notNullabeType != typeof(Guid)
                &&
                type != typeof(byte[])
                &&
                Type.GetTypeCode(notNullabeType) == TypeCode.Object;
        }

        public static class TypeGuid
        {
            public static readonly Guid StringGuid = typeof(string).GUID;
            public static readonly Guid IntGuid = typeof(int).GUID;
            public static readonly Guid BoolGuid = typeof(bool).GUID;
            public static readonly Guid DoubleGuid = typeof(double).GUID;
            public static readonly Guid DateTimeGuid = typeof(DateTime).GUID;
        }
    }

    public static class TypeOf<T>
    {
        public static bool IsMemberOf<TMember>(Expression<Func<T, TMember>> expression, MemberInfo member)
        {
            return Helper.Member<T, TMember>(expression) == member;
        }

        public static MemberInfo MemberOf<TMember>(Expression<Func<T, TMember>> expression) => Helper.Member<T, TMember>(expression);
       
    }
}
