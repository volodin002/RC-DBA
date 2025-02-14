using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RC.DBA.Query.Linq
{
    /// <summary>
    /// A basic abstract LINQ query provider
    /// </summary>
    public abstract class QueryProviderImpl : IQueryProvider, IQueryText
    {
        protected QueryProviderImpl()
        {
            //ExpressionVisitor
        }

        IQueryable<T> IQueryProvider.CreateQuery<T>(Expression expression)
        {
            return new QueryableImpl<T>(this, expression);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            Type elementType = TypeHelper.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(QueryableImpl<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        S IQueryProvider.Execute<S>(Expression expression)
        {
            return (S)this.Execute(expression);
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return this.Execute(expression);
        }

        public abstract string GetQueryText(Expression expression);
        public abstract object Execute(Expression expression);
    }
}
