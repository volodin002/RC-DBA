using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RC.DBA.Metamodel;

namespace RC.DBA
{
    public class DbContext
    {
        private IModelManager _manager;

        public delegate IList<TResult> FactoryWithCount<TResult>(DbDataReader reader, ref int count);

        public IModelManager Manager => _manager;
        public DbContext(IModelManager manager)
        {
            _manager = manager;
        }

        public static IModelManager CreateManager()
        {
            return new Metamodel.Impl.ModelManager();
        }

        public static IModelManager CreateManager(IEnumerable<Type> types)
        {
            return new Metamodel.Impl.ModelManager(types);
        }

        public static IModelManager CreateManager(params Type[] types)
        {
            return new Metamodel.Impl.ModelManager(types);
        }

        public Query.IQueryBuilder GetQueryBuilder()
        {
            return new Query.Impl.QueryBuilderImpl(_manager);
        }

        public static T GetValue<T>(DbCommand cmd, Func<DbDataReader, T> factory)
        {
            using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
            {
                return factory(reader);
            }
        }

        public static T GetValue<T>(DbConnection con, string sql, Func<DbDataReader, T> factory, IEnumerable<Parameter> parameters = null)
        {
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sql;
                SetParameters(cmd, parameters);

                if (con.State != System.Data.ConnectionState.Open) con.Open();

                using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                {
                    return factory(reader);
                }
            }
        }

        public static List<T> GetValueList<T>(DbConnection con, string sql, Func<DbDataReader, T> factory, IEnumerable<Parameter> parameters = null)
        {
            var list = new List<T>();
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sql;
                SetParameters(cmd, parameters);

                if (con.State != System.Data.ConnectionState.Open) con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(factory(reader));
                }
            }

            return list;
        }

        public T GetValue<T>(DbConnection con, string sql, IEnumerable<Parameter> parameters = null) 
        {
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sql;
                SetParameters(cmd, parameters);

                if (con.State != System.Data.ConnectionState.Open) con.Open();

                using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                {
                    var factory = _manager.GetReadValueFactory<T>();
                    return factory(reader);
                }
            }

        }

        public List<T> GetValueList<T>(DbConnection con, string sql, IEnumerable<Parameter> parameters = null) 
        {
            var list = new List<T>();
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sql;
                SetParameters(cmd, parameters);

                if (con.State != System.Data.ConnectionState.Open) con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    var factory = _manager.GetValueFactory<T>();
                    while (reader.Read())
                        list.Add(factory(reader));
                }
            }
            return list;
        }

        public T GetValue<T>(DbConnection con, string sql, params Parameter[] parameters) 
        {
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sql;

                SetParameters(cmd, parameters);

                if (con.State != System.Data.ConnectionState.Open) con.Open();

                using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                {
                    var factory = _manager.GetReadValueFactory<T>();
                    return factory(reader);
                }
            }
        }

        public List<T> GetValueList<T>(DbConnection con, string sql, params Parameter[] parameters) 
        {
            var list = new List<T>();
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sql;
                SetParameters(cmd, parameters);

                if (con.State != System.Data.ConnectionState.Open) con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    var factory = _manager.GetValueFactory<T>();
                    while (reader.Read())
                        list.Add(factory(reader));
                }
            }
            return list;
        }

        public T GetValue<T>(DbConnection con, string sql, DbTransaction tran, IEnumerable<Parameter> parameters = null) 
        {
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sql;
                if (tran != null) cmd.Transaction = tran;

                SetParameters(cmd, parameters);

                using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                {
                    var factory = _manager.GetReadValueFactory<T>();
                    return factory(reader);
                }
            }

        }

        public List<T> GetValueList<T>(DbConnection con, string sql, DbTransaction tran, IEnumerable<Parameter> parameters = null) 
        {
            var list = new List<T>();
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sql;
                if (tran != null) cmd.Transaction = tran;

                SetParameters(cmd, parameters);

                using (var reader = cmd.ExecuteReader())
                {
                    var factory = _manager.GetValueFactory<T>();
                    while (reader.Read())
                        list.Add(factory(reader));
                }
            }
            return list;
        }

        public T GetValue<T>(DbConnection con, string sql, DbTransaction tran, params Parameter[] parameters) 
        {
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sql;
                if (tran != null) cmd.Transaction = tran;

                SetParameters(cmd, parameters);

                using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                {
                    var factory = _manager.GetReadValueFactory<T>();
                    return factory(reader);
                }
            }
        }

        public List<T> GetValueList<T>(DbConnection con, string sql, DbTransaction tran, params Parameter[] parameters) 
        {
            var list = new List<T>();
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sql;
                if (tran != null) cmd.Transaction = tran;

                SetParameters(cmd, parameters);

                using (var reader = cmd.ExecuteReader())
                {
                    var factory = _manager.GetValueFactory<T>();
                    while (reader.Read())
                        list.Add(factory(reader));
                }
            }
            return list;
        }

        public static IList<T> GetResultList<T>(DbCommand cmd, Func<DbDataReader, IList<T>> factory)
        {
            IList<T> items;
            using (var reader = cmd.ExecuteReader())
            {
                items = factory(reader);
            }
        
            return items;
        }

        public static IList<T> GetResultList<T>(DbConnection con, string sql, Func<DbDataReader, IList<T>> factory, IEnumerable<Parameter> parameters = null)
        {
            IList<T> items;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sql;
                SetParameters(cmd, parameters);

                if (con.State != System.Data.ConnectionState.Open) con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    items = factory(reader);
                }
            }

            return items;
        }

        public IList<T> GetResultList<T>(DbConnection con, string sql, ref Func<DbDataReader, IList<T>> factory, IEnumerable<Parameter> parameters = null)
        {
            IList<T> items;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sql;
                SetParameters(cmd, parameters);

                if (con.State != System.Data.ConnectionState.Open) con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (factory == null)
                    {
                        factory = Emit.DbContextFactoryEmiter.EmitResultListFactory<T>(reader, _manager);
                    }
                    items = factory(reader);
                }
            }

            return items;
        }

        public IList<T> GetResultList<T>(DbConnection con, DbTransaction tran, Query.SqlQuery<T> query)
        {
            IList<T> items;
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = query.Sql;
                cmd.CommandType = query.CommandType;
                if (query.CommandTimeout >= 0)
                    cmd.CommandTimeout = query.CommandTimeout;
                
                foreach (var p in query.Parameters)
                    p.AddDbParameter(cmd);
                

                using (var reader = cmd.ExecuteReader())
                {
                    var factory = query.factory;
                    if (factory == null)
                    {
                        factory = Emit.DbContextFactoryEmiter.EmitResultListFactory<T>(reader, _manager);
                        query.factory = factory;
                    }
                    
                    items = factory(reader);
                }
            }

            return items;
            
        }

        public IList<T> GetResultList<T>(DbConnection con, DbTransaction tran, Query.SqlQueryWithCount<T> query, ref int count)
        {
            IList<T> items;
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = query.Sql;
                cmd.CommandType = query.CommandType;
                if (query.CommandTimeout >= 0)
                    cmd.CommandTimeout = query.CommandTimeout;

                foreach (var p in query.Parameters)
                    p.AddDbParameter(cmd);


                using (var reader = cmd.ExecuteReader())
                {
                    var factory = query.factory;
                    if (factory == null)
                    {
                        factory = Emit.DbContextFactoryEmiter.EmitResultListFactoryWithCount<T>(reader, _manager);
                        query.factory = factory;
                    }

                    items = factory(reader, ref count);
                }
            }

            return items;

        }

        public IList<ValueTuple<T, T1>> GetResultList<T, T1>(DbConnection con, DbTransaction tran, Query.SqlQuery<T, T1> query)
        {
            IList<ValueTuple<T, T1>> items;
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = query.Sql;
                cmd.CommandType = query.CommandType;
                if (query.CommandTimeout >= 0)
                    cmd.CommandTimeout = query.CommandTimeout;

                foreach (var p in query.Parameters)
                    p.AddDbParameter(cmd);


                using (var reader = cmd.ExecuteReader())
                {
                    var factory = query.factory;
                    if (factory == null)
                    {
                        factory = Emit.DbContextFactoryEmiter.EmitResultListFactory<T, T1>(reader, _manager, query.alias1);
                        query.factory = factory;
                    }

                    items = factory(reader);
                }
            }

            return items;

        }

        public IList<ValueTuple<T, T1, T2>> GetResultList<T, T1, T2>(DbConnection con, DbTransaction tran, Query.SqlQuery<T, T1, T2> query)
        {
            IList<ValueTuple<T, T1, T2>> items;
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = query.Sql;
                cmd.CommandType = query.CommandType;
                if (query.CommandTimeout >= 0)
                    cmd.CommandTimeout = query.CommandTimeout;

                foreach (var p in query.Parameters)
                    p.AddDbParameter(cmd);


                using (var reader = cmd.ExecuteReader())
                {
                    var factory = query.factory;
                    if (factory == null)
                    {
                        factory = Emit.DbContextFactoryEmiter.EmitResultListFactory<T, T1, T2>(reader, _manager, query.alias1, query.alias2);
                        query.factory = factory;
                    }

                    items = factory(reader);
                }
            }

            return items;

        }

        public async Task<IList<T>> GetResultListAsync<T>(DbConnection con, DbTransaction tran, Query.SqlQuery<T> query)
        {
            IList<T> items;
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = query.Sql;
                cmd.CommandType = query.CommandType;
                if (query.CommandTimeout >= 0)
                    cmd.CommandTimeout = query.CommandTimeout;

                foreach (var p in query.Parameters)
                    p.AddDbParameter(cmd);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var factory = query.factory;
                    if (factory == null)
                    {
                        factory = Emit.DbContextFactoryEmiter.EmitResultListFactory<T>(reader, _manager);
                        query.factory = factory;
                    }
                    items = factory(reader);
                }
            }

            return items;

        }

        public IList<T> GetResultList<T>(DbConnection con, Query.SqlQuery<T> query)
        {
            IList<T> items;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = query.Sql;
                cmd.CommandType = query.CommandType;
                if (query.CommandTimeout >= 0)
                    cmd.CommandTimeout = query.CommandTimeout;

                foreach (var p in query.Parameters)
                    p.AddDbParameter(cmd);

                if (con.State != System.Data.ConnectionState.Open) con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    var factory = query.factory;
                    if (factory == null)
                    {
                        factory = Emit.DbContextFactoryEmiter.EmitResultListFactory<T>(reader, _manager);
                        query.factory = factory;
                    }
                    items = factory(reader);
                }
            }

            return items;

        }

        public IList<T> GetResultList<T>(DbConnection con, Query.SqlQueryWithCount<T> query, ref int count)
        {
            IList<T> items;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = query.Sql;
                cmd.CommandType = query.CommandType;
                if (query.CommandTimeout >= 0)
                    cmd.CommandTimeout = query.CommandTimeout;

                foreach (var p in query.Parameters)
                    p.AddDbParameter(cmd);

                if (con.State != System.Data.ConnectionState.Open) con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    var factory = query.factory;
                    if (factory == null)
                    {
                        factory = Emit.DbContextFactoryEmiter.EmitResultListFactoryWithCount<T>(reader, _manager);
                        query.factory = factory;
                    }
                    items = factory(reader, ref count);
                }
            }

            return items;

        }

        public IList<ValueTuple<T, T1>> GetResultList<T, T1>(DbConnection con, Query.SqlQuery<T, T1> query)
        {
            IList<ValueTuple<T, T1>> items;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = query.Sql;
                cmd.CommandType = query.CommandType;
                if (query.CommandTimeout >= 0)
                    cmd.CommandTimeout = query.CommandTimeout;

                foreach (var p in query.Parameters)
                    p.AddDbParameter(cmd);


                using (var reader = cmd.ExecuteReader())
                {
                    var factory = query.factory;
                    if (factory == null)
                    {
                        factory = Emit.DbContextFactoryEmiter.EmitResultListFactory<T, T1>(reader, _manager, query.alias1);
                        query.factory = factory;
                    }

                    items = factory(reader);
                }
            }

            return items;

        }

        public IList<ValueTuple<T, T1, T2>> GetResultList<T, T1, T2>(DbConnection con, Query.SqlQuery<T, T1, T2> query)
        {
            IList<ValueTuple<T, T1, T2>> items;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = query.Sql;
                cmd.CommandType = query.CommandType;
                if (query.CommandTimeout >= 0)
                    cmd.CommandTimeout = query.CommandTimeout;

                foreach (var p in query.Parameters)
                    p.AddDbParameter(cmd);


                using (var reader = cmd.ExecuteReader())
                {
                    var factory = query.factory;
                    if (factory == null)
                    {
                        factory = Emit.DbContextFactoryEmiter.EmitResultListFactory<T, T1, T2>(reader, _manager, query.alias1, query.alias2);
                        query.factory = factory;
                    }

                    items = factory(reader);
                }
            }

            return items;

        }


        //public IList<T> GetResultList<T>(DbConnection con, string sql, object queryCacheKey, params Parameter[] parameters)
        //{
        //    return GetResultList<T>(con, sql, queryCacheKey, (IEnumerable<Parameter>)parameters);
        //}

        //public IList<T> GetResultList<T>(DbConnection con, DbTransaction tran, string sql, object queryCacheKey, params Parameter[] parameters)
        //{
        //    return GetResultList<T>(con, tran, sql, queryCacheKey, (IEnumerable<Parameter>)parameters);
        //}

        //public IList<T> GetResultList<T>(DbConnection con, Query.SqlQuery<T>.Query query)
        //{
        //    return GetResultList<T>(con, query.Sql, query.CacheKey, query.Parameters);
        //}

        //public IList<T> GetResultList<T>(DbConnection con, DbTransaction tran, Query.SqlQuery<T>.Query query)
        //{
        //    return GetResultList<T>(con, tran, query.Sql, query.CacheKey, query.Parameters);
        //}

        //public Task<IList<T>> GetResultListAsync<T>(DbConnection con, DbTransaction tran, Query.SqlQuery<T>.Query query)
        //{
        //    return GetResultListAsync<T>(con, tran, query.Sql, query.CacheKey, query.Parameters);
        //}


        public static IEnumerable<T> GetResultItems<T>(DbCommand cmd, IDbObjectFactory<T> factory)
        {
            using (var reader = cmd.ExecuteReader())
            {
                if (!factory.OnFirst(reader)) return Enumerable.Empty<T>();
                while (factory.OnNext(reader)) { }
            }

            return factory.OnComplete();
        }

        public static int Execute(DbConnection con, DbTransaction tran, string sql, IEnumerable<Parameter> parameters = null)
        {
            int res;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sql;
                if (tran != null) cmd.Transaction = tran;
                SetParameters(cmd, parameters);

                res = cmd.ExecuteNonQuery();

                SetOutputParametersValue(parameters, cmd);

            }

            return res;
        }

        public static int Execute<T>(DbConnection con, DbTransaction tran, Query.SqlQuery<T> query)
        {
            int res;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = query.Sql;
                cmd.CommandType = query.CommandType;
                if (query.CommandTimeout >= 0)
                    cmd.CommandTimeout = query.CommandTimeout;

                if (tran != null) cmd.Transaction = tran;
                
                foreach (var p in query.Parameters)
                    p.AddDbParameter(cmd);
                

                res = cmd.ExecuteNonQuery();

                SetOutputParametersValue(query.Parameters, cmd);

            }

            return res;
        }

        public static int Execute<T>(DbConnection con, Query.SqlQuery<T> query)
        {
            return Execute<T>(con, null, query);
        }

        public static int Execute(DbConnection con, string sql, IEnumerable<Parameter> parameters = null)
        {
            return Execute(con, null, sql, parameters);
        }

        public static int Execute(DbConnection con, string sql, params Parameter[] parameters)
        {
            return Execute(con, sql, (IEnumerable<Parameter>)parameters);
        }

        public static int Execute(DbConnection con, DbTransaction tran, string sql, params Parameter[] parameters)
        {
            return Execute(con, tran, sql, (IEnumerable<Parameter>)parameters);
        }

        private static void SetOutputParametersValue(IEnumerable<Parameter> parameters, DbCommand cmd)
        {
            if (parameters == null) return;

            var outParams = parameters
                .Where(p => p.Direction != System.Data.ParameterDirection.Input)
                .ToDictionary(p => p.Name);

            foreach (var p in cmd.Parameters.OfType<DbParameter>().Where(p => p.Direction != System.Data.ParameterDirection.Input))
            {
                outParams[p.ParameterName].SetValue(p.Value);
            }
            
        }

        private static void SetParameters(DbCommand cmd, IEnumerable<Parameter> parameters)
        {
            if (parameters == null) return;
            
            foreach (var p in parameters)
                p.AddDbParameter(cmd);
        }

        private static void SetParameters(DbCommand cmd, params Parameter[] parameters)
        {
            var length = parameters.Length;
            if (length == 0) return;

            for (int i = 0; i < length; i++)
            {
                parameters[i].AddDbParameter(cmd);
            }
            /*
            var dbParameters = new DbParameter[length];
            for (int i = 0; i < length; i++)
            {
                dbParameters[i] = parameters[i].DbParameter(cmd);
            }
            cmd.Parameters.AddRange(dbParameters);
            */
        }
    }
}
