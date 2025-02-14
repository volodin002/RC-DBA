using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using RC.DBA.Collections;
using RC.DBA.Emit;

namespace RC.DBA
{
    public static class DbManager
    {
        public static List<T> LoadFromDb<T>(IDataReader reader, Func<IDataReader, T> mapper)
        {
            var items = new List<T>();
            while (reader.Read())
            {
                items.Add(mapper(reader));
            }

            return items;
        }

        public static IEnumerable<T> LoadOneByOneFromDb<T>(IDataReader reader, Func<IDataReader, T> mapper)
        {
            while (reader.Read())
            {
                yield return mapper(reader);
            }
        }

        public static List<T> LoadFromDb<T>(DbConnection con, string sqlText, ref Func<IDataReader, T> mapper,
            Func<SqlDataReaderMapper<T>> mapperFactory)
        {
            List<T> items;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sqlText;
                using (var reader = cmd.ExecuteReader())
                {
                    if (mapper == null)
                        mapper = mapperFactory().Build(reader);

                    items = LoadFromDb(reader, mapper);
                }
            }
            

            return items;
        }

        public static List<T> LoadFromDb<T>(DbConnection con, string sqlText, ref Func<IDataReader, T> mapper,
            Func<SqlDataReaderMapper<T>> mapperFactory, int commandTimeout)
        {
            List<T> items;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandTimeout = commandTimeout;
                cmd.CommandText = sqlText;
                using (var reader = cmd.ExecuteReader())
                {
                    if (mapper == null)
                        mapper = mapperFactory().Build(reader);

                    items = LoadFromDb(reader, mapper);
                }
            }
            

            return items;
        }

        public static List<T> LoadFromDb<T>(DbConnection con, string sqlText, ref Func<IDataReader, T> mapper,
            Func<SqlDataReaderMapper<T>> mapperFactory,
            Action<DbCommand> addParameters)
        {
            List<T> items;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sqlText;
                addParameters?.Invoke(cmd);

                using (var reader = cmd.ExecuteReader())
                {
                    if (mapper == null)
                        mapper = mapperFactory().Build(reader);

                    items = LoadFromDb(reader, mapper);
                }
            }
            

            return items;
        }

        public static List<T> LoadFromDb<T>(DbConnection con, string sqlText, Func<IDataReader, T> mapper, params DbParameter[] parameters)
        {
            List<T> items;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sqlText;
                foreach (var param in parameters)
                    cmd.Parameters.Add(param);

                using (var reader = cmd.ExecuteReader())
                {
                    items = LoadFromDb(reader, mapper);
                }
            }


            return items;
        }

        public static List<T> LoadFromDb<T>(DbConnection con, DbTransaction trn, string sqlText, Func<IDataReader, T> mapper, params DbParameter[] parameters)
        {
            List<T> items;
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = trn;
                cmd.CommandText = sqlText;
                foreach (var param in parameters)
                    cmd.Parameters.Add(param);

                using (var reader = cmd.ExecuteReader())
                {
                    items = LoadFromDb(reader, mapper);
                }
            }


            return items;
        }

        public static IEnumerable<T> LoadOneByOneFromDb<T>(DbConnection con, DbTransaction trn, string sqlText, Func<IDataReader, T> mapper, params DbParameter[] parameters)
        {
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = trn;
                cmd.CommandText = sqlText;
                foreach (var param in parameters)
                    cmd.Parameters.Add(param);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return mapper(reader);
                    }
                }
            }

        }

        public static IEnumerable<T> LoadOneByOneFromDb<T>(DbConnection con, string sqlText, Func<IDataReader, T> mapper, params DbParameter[] parameters)
        {
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sqlText;
                foreach (var param in parameters)
                    cmd.Parameters.Add(param);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return mapper(reader);
                    }
                }
            }

        }

        public static IEnumerable<T> LoadOneByOneFromDb<T>(DbConnection con, DbTransaction trn, string sqlText, Func<IDataReader, T> mapper, int commandTimeout, params DbParameter[] parameters)
        {
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = trn;
                cmd.CommandText = sqlText;
                cmd.CommandTimeout = commandTimeout;
                foreach (var param in parameters)
                    cmd.Parameters.Add(param);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return mapper(reader);
                    }
                }
            }

        }

        public static IEnumerable<T> LoadOneByOneFromDb<T>(DbConnection con, string sqlText, Func<IDataReader, T> mapper, int commandTimeout, params DbParameter[] parameters)
        {
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sqlText;
                cmd.CommandTimeout = commandTimeout;
                foreach (var param in parameters)
                    cmd.Parameters.Add(param);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return mapper(reader);
                    }
                }
            }

        }

        public static IEnumerable<T> LoadOneByOneFromDb<T>(DbConnection con, DbTransaction trn, string sqlText, ref Func<IDataReader, T> mapper, Func<SqlDataReaderMapper<T>> mapperFactory, Action<DbCommand> addParameters, int commandTimeout)
        {
            var cmd = con.CreateCommand();
            
            cmd.Transaction = trn;
            cmd.CommandText = sqlText;
            cmd.CommandTimeout = commandTimeout;
            addParameters?.Invoke(cmd);

            DbDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();

                if (mapper == null)
                    mapper = mapperFactory().Build(reader);

            }
            catch { 
                ((IDisposable)reader)?.Dispose();
                cmd?.Dispose();

                throw; 
            }

            return new ValueEnumerator<T>(cmd, reader, mapper);
        }

        public static IEnumerable<(T, T1)> LoadOneByOneFromDb<T, T1, T2>(DbConnection con, DbTransaction trn, string sqlText,
            ref Func<IDataReader, (T, T1)> mapper,
            Func<SqlDataReaderMapper<T>> mapperFactory, string alias1, 
            Action<DbCommand> addParameters, int commandTimeout)
        {
            var cmd = con.CreateCommand();

            cmd.Transaction = trn;
            cmd.CommandText = sqlText;
            cmd.CommandTimeout = commandTimeout;
            addParameters?.Invoke(cmd);

            DbDataReader reader = null;

            try
            {
                reader = cmd.ExecuteReader();

                if (mapper == null)
                    mapper = mapperFactory().Build<T1>(reader, alias1);
            }
            catch
            {
                ((IDisposable)reader)?.Dispose();
                cmd?.Dispose();

                throw;
            }

            return new ValueEnumerator<(T, T1)>(cmd, reader, mapper);
        }

        public static IEnumerable<(T, T1, T2)> LoadOneByOneFromDb<T, T1, T2>(DbConnection con, DbTransaction trn, string sqlText, 
            ref Func<IDataReader, (T, T1, T2)> mapper, 
            Func<SqlDataReaderMapper<T>> mapperFactory, string alias1, string alias2, 
            Action<DbCommand> addParameters, int commandTimeout)
        {
            var cmd = con.CreateCommand();

            cmd.Transaction = trn;
            cmd.CommandText = sqlText;
            cmd.CommandTimeout = commandTimeout;
            addParameters?.Invoke(cmd);

            DbDataReader reader = null;

            try
            {
                reader = cmd.ExecuteReader();

                if (mapper == null)
                    mapper = mapperFactory().Build<T1, T2>(reader, alias1, alias2);
            }
            catch
            {
                ((IDisposable)reader)?.Dispose();
                cmd?.Dispose();

                throw;
            }

            return new ValueEnumerator<(T, T1, T2)>(cmd, reader, mapper);
        }

        public static List<T> LoadFromDb<T>(DbConnection con, string sqlText, ref Func<IDataReader, T> mapper,
            Func<SqlDataReaderMapper<T>> mapperFactory,
            Action<DbCommand> addParameters, int commandTimeout)
        {
            List<T> items;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandTimeout = commandTimeout;
                cmd.CommandText = sqlText;

                addParameters?.Invoke(cmd);

                using (var reader = cmd.ExecuteReader())
                {
                    if (mapper == null)
                        mapper = mapperFactory().Build(reader);

                    items = LoadFromDb(reader, mapper);
                }
            }

            return items;
        }

        public static T LoadOneFromDd<T>(DbConnection con, string sqlText, ref Func<IDataReader, T> mapper,
            Func<SqlDataReaderMapper<T>> mapperFactory,
            Action<DbCommand> addParameters)
        {
            T item;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sqlText;
                addParameters?.Invoke(cmd);

                using (var reader = cmd.ExecuteReader())
                {
                    if (mapper == null)
                        mapper = mapperFactory().Build(reader);

                    if (reader.Read())
                        item = mapper(reader);
                    else
                        item = default;
                }
            }
            

            return item;
        }

        public static T LoadOneFromDd<T>(DbConnection con, string sqlText, ref Func<IDataReader, T> mapper,
           Func<SqlDataReaderMapper<T>> mapperFactory)
        {
            T item;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sqlText;

                using (var reader = cmd.ExecuteReader())
                {
                    if (mapper == null)
                        mapper = mapperFactory().Build(reader);

                    if (reader.Read())
                        item = mapper(reader);
                    else
                        item = default(T);
                }
            }
            
            return item;
        }

        public static T LoadOneFromDb<T>(DbConnection con, string sqlText, ref Func<IDataReader, T> mapper,
           Func<SqlDataReaderMapper<T>> mapperFactory, int commandTimeout)
        {
            T item;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sqlText;
                cmd.CommandTimeout = commandTimeout;
                using (var reader = cmd.ExecuteReader())
                {
                    if (mapper == null)
                        mapper = mapperFactory().Build(reader);

                    if (reader.Read())
                        item = mapper(reader);
                    else
                        item = default(T);
                }
            }
            

            return item;
        }

        public static T LoadOneFromDb<T>(IDataReader reader, Func<IDataReader, T> mapper)
        {
            T item;
            if (reader.Read())
                item = mapper(reader);
            else
                item = default(T);

            return item;
        }
        public static T LoadOneFromDb<T>(DbConnection con, string sqlText, Func<IDataReader, T> mapper)
        {
            T item;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sqlText;
                using (var reader = cmd.ExecuteReader())
                {
                    item = LoadOneFromDb<T>(reader, mapper);
                }
            }


            return item;
        }

        public static DbCommand AddParameter<T>(this DbCommand cmd, string parameterName, T value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = parameterName;
            p.Value = value as object ?? DBNull.Value;
            cmd.Parameters.Add(p);

            return cmd;
        }

        public static void Execute(DbConnection con, string sqlText,
            Action<DbCommand> addParameters)
        {
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sqlText;
                addParameters?.Invoke(cmd);

                cmd.ExecuteNonQuery();
            }   
        }

        public static void Execute(DbConnection con, string sqlText,
            Action<DbCommand> addParameters, int commandTimeout)
        {

            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sqlText;
                cmd.CommandTimeout = commandTimeout;
                addParameters?.Invoke(cmd);

                cmd.ExecuteNonQuery();
            }
            
        }

        public static (string sql, IList<Parameter> parameters) GetInsertQuery<T>(T item, SqlInsertMapper<T> mapper)
        {
            var (sql, parametersFactory) = mapper.Build();

            return (sql, parametersFactory(item));           

        }

        public struct ValueEnumerator<T> : IEnumerable<T>, IEnumerator<T>
        {
            DbCommand _cmd;
            DbDataReader _reader;
            Func<IDataReader, T> _mapper;
            private T _current;

            internal ValueEnumerator(DbCommand cmd, DbDataReader reader, Func<IDataReader, T> mapper)
            {
                _cmd = cmd;
                _reader = reader;
                _mapper = mapper;
                _current = default(T);
            }

            public void Dispose()
            {
                _reader.Dispose();
                _cmd.Dispose();

                _cmd = null;
                _reader = null; 
                _mapper = null;
            }

            public bool MoveNext()
            {
                if(_reader.Read())
                {
                    _current = _mapper(_reader);
                    return true;
                }

                _current = default(T);
                return false;
            }

            public void Reset() => throw new NotImplementedException();
            
            public T Current => _current;

            object IEnumerator.Current => throw new NotImplementedException();

            #region // IEnumerable<T>

            /// we avoid boxing!!! compiler can use this method in foreach !!!
            public ValueEnumerator<T> GetEnumerator() => this;

            /// <internalonly/> we avoid boxing and hide interface implementation!!!
            IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;

            /// <internalonly/> we avoid boxing and hide interface implementation!!!
            IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

            #endregion // IEnumerable<T>
        }

    }
}
