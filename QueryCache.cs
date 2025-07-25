﻿using RC.DBA.Query;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.DBA
{
    public class QueryCache
    {
        private static ConcurrentDictionary<string, SqlQuery> _cache = new ConcurrentDictionary<string, SqlQuery>();

        public static SqlQuery<T> CreateOrGet<T>(DbContext ctx, string key, Func<IQueryBuilder, SqlQuery<T>> factory)
        {
            if (_cache.TryGetValue(key, out var query))
                return ((SqlQuery<T>)query).GetQuery();

            var queryBuilder = ctx.GetQueryBuilder();
            var query0 = factory(queryBuilder);

            _cache.TryAdd(key, query0);
            
            return query0;
        }

        public static SqlQueryWithCount<T> CreateOrGet<T>(DbContext ctx, string key, Func<IQueryBuilder, SqlQueryWithCount<T>> factory)
        {
            if (_cache.TryGetValue(key, out var query))
                return ((SqlQueryWithCount<T>)query).GetQuery();

            var queryBuilder = ctx.GetQueryBuilder();
            var query0 = factory(queryBuilder);

            _cache.TryAdd(key, query0);

            return query0;
        }

        public static SqlQuery<T, T1> CreateOrGet<T, T1>(DbContext ctx, string key, Func<IQueryBuilder, SqlQuery<T, T1>> factory)
        {
            if (_cache.TryGetValue(key, out var query))
                return ((SqlQuery<T, T1>)query).GetQuery();

            var queryBuilder = ctx.GetQueryBuilder();
            var query0 = factory(queryBuilder);

            _cache.TryAdd(key, query0);

            return query0;
        }

        public void Clear() => _cache.Clear();

        public bool Remove(string key, out SqlQuery query) => _cache.TryRemove(key, out query);
    }

    public class UpdateQueryCache
    {
        private static ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

        public static UpdateObjectCompiledQuery<T> CreateOrGet<T>(DbContext ctx, string key, Func<IQueryBuilder, UpdateObjectCompiledQuery<T>> factory)
        {
            if (_cache.TryGetValue(key, out var query))
                return ((UpdateObjectCompiledQuery<T>)query);

            var queryBuilder = ctx.GetQueryBuilder();
            var query0 = factory(queryBuilder);

            _cache.TryAdd(key, query0);

            return query0;
        }

        public void Clear() => _cache.Clear();

        public bool Remove(string key, out object query) => _cache.TryRemove(key, out query);
    }
}
