using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace RC.DBA.Query
{
    public class SqlQuery<T> : SqlQuery
    {
        public Func<DbDataReader, IList<T>> factory;
        public SqlQuery(string sql, params Parameter[] parameters) : base(sql, parameters) { }

        public SqlQuery(string sql, Func<DbDataReader, IList<T>> factory, params Parameter[] parameters) : base(sql, parameters)
        {
            this.factory = factory;
        }

        protected SqlQuery(string sql, Func<DbDataReader, IList<T>> factory, IEnumerable<Parameter> parameters, int length) : base(sql, parameters, length)
        {
            this.factory = factory;
        }

        protected SqlQuery(string sql, Func<DbDataReader, IList<T>> factory, Collections.HashMap<string, Parameter> parameters) : base(sql, parameters)
        {
            this.factory = factory;
        }


        public SqlQuery<T> SetParamValue<TValue>(string name, TValue value)
        {
            var p = GetParam<TValue>(name);
            p.Value = value;
            return this;
        }

        public SqlQuery<T> SetParam<TValue>(Parameter<TValue> parameter)
        {
            _Parameters.Set(parameter.Name, parameter);
            return this;
        }

        public SqlQuery<T> GetQuery()
        {
            if (factory == null) return this;

            var parameters = _Parameters.Copy();
            for (int i = 0, count = parameters.Count; i < count; i++)
            {
                parameters.SetValueByIndex(i, parameters.GetValueByIndex(i).Copy());
            }
            return new SqlQuery<T>(Sql, factory, parameters)
            {
                CommandTimeout = CommandTimeout,
                CommandType = CommandType
            };
        }

        public SqlQuery<T> SetOffsetParamValue(int value)
        {
            var p = GetParam<int>(Offset_Parameter_Name);
            if (p != null) p.Value = value;
            else _Parameters.Add(Offset_Parameter_Name, Parameter.Create(Offset_Parameter_Name, value));

            return this;
        }

        public SqlQuery<T> SetLimitParamValue(int value)
        {
            var p = GetParam<int>(Limit_Parameter_Name);
            if (p != null) p.Value = value;
            else _Parameters.Add(Limit_Parameter_Name, Parameter.Create(Limit_Parameter_Name, value));

            return this;
        }
    }

    public class SqlQuery<T, T1> : SqlQuery
    {
        public Func<DbDataReader, IList<ValueTuple<T, T1>>> factory;
        public string alias1;
        public SqlQuery(string sql, string alias1, params Parameter[] parameters) : base(sql, parameters) { this.alias1 = alias1; }

        public SqlQuery(string sql, Func<DbDataReader, IList<ValueTuple<T, T1>>> factory, string alias1, params Parameter[] parameters) : base(sql, parameters)
        {
            this.factory = factory;
            this.alias1 = alias1;
        }

        protected SqlQuery(string sql, Func<DbDataReader, IList<ValueTuple<T, T1>>> factory, string alias1, IEnumerable<Parameter> parameters, int length) : base(sql, parameters, length)
        {
            this.factory = factory;
            this.alias1 = alias1;
        }

        protected SqlQuery(string sql, Func<DbDataReader, IList<ValueTuple<T, T1>>> factory, string alias1, Collections.HashMap<string, Parameter> parameters) : base(sql, parameters)
        {
            this.factory = factory;
            this.alias1 = alias1;
        }


        public SqlQuery<T, T1> SetParamValue<TValue>(string name, TValue value)
        {
            var p = GetParam<TValue>(name);
            p.Value = value;
            return this;
        }

        public SqlQuery<T, T1> SetParam<TValue>(Parameter<TValue> parameter)
        {
            _Parameters.Set(parameter.Name, parameter);
            return this;
        }

        public SqlQuery<T, T1> GetQuery()
        {
            if (factory == null) return this;

            var parameters = _Parameters.Copy();
            for (int i = 0, count = parameters.Count; i < count; i++)
            {
                parameters.SetValueByIndex(i, parameters.GetValueByIndex(i).Copy());
            }
            return new SqlQuery<T, T1>(Sql, factory, alias1, parameters)
            {
                CommandTimeout = CommandTimeout,
                CommandType = CommandType
            };
        }

        public SqlQuery<T, T1> SetOffsetParamValue(int value)
        {
            var p = GetParam<int>(Offset_Parameter_Name);
            if (p != null) p.Value = value;
            else _Parameters.Add(Offset_Parameter_Name, Parameter.Create(Offset_Parameter_Name, value));

            return this;
        }

        public SqlQuery<T, T1> SetLimitParamValue(int value)
        {
            var p = GetParam<int>(Limit_Parameter_Name);
            if (p != null) p.Value = value;
            else _Parameters.Add(Limit_Parameter_Name, Parameter.Create(Limit_Parameter_Name, value));

            return this;
        }
    }

    public class SqlQuery<T, T1, T2> : SqlQuery
    {
        public Func<DbDataReader, IList<ValueTuple<T, T1, T2>>> factory;
        public string alias1;
        public string alias2;
        public SqlQuery(string sql, string alias1, string alias2,  params Parameter[] parameters) : base(sql, parameters) { 
            this.alias1 = alias1;
            this.alias2 = alias2;
        }

        public SqlQuery(string sql, Func<DbDataReader, IList<ValueTuple<T, T1, T2>>> factory, string alias1, string alias2, params Parameter[] parameters) : base(sql, parameters)
        {
            this.factory = factory;
            this.alias1 = alias1;
            this.alias2 = alias2;
        }

        protected SqlQuery(string sql, Func<DbDataReader, IList<ValueTuple<T, T1, T2>>> factory, string alias1, string alias2, IEnumerable<Parameter> parameters, int length) : base(sql, parameters, length)
        {
            this.factory = factory;
            this.alias1 = alias1;
            this.alias2 = alias2;
        }

        protected SqlQuery(string sql, Func<DbDataReader, IList<ValueTuple<T, T1, T2>>> factory, string alias1, string alias2, Collections.HashMap<string, Parameter> parameters) : base(sql, parameters)
        {
            this.factory = factory;
            this.alias1 = alias1;
            this.alias2 = alias2;
        }


        public SqlQuery<T, T1, T2> SetParamValue<TValue>(string name, TValue value)
        {
            var p = GetParam<TValue>(name);
            p.Value = value;
            return this;
        }

        public SqlQuery<T, T1, T2> SetParam<TValue>(Parameter<TValue> parameter)
        {
            _Parameters.Set(parameter.Name, parameter);
            return this;
        }

        public SqlQuery<T, T1, T2> GetQuery()
        {
            if (factory == null) return this;

            var parameters = _Parameters.Copy();
            for (int i = 0, count = parameters.Count; i < count; i++)
            {
                parameters.SetValueByIndex(i, parameters.GetValueByIndex(i).Copy());
            }
            return new SqlQuery<T, T1, T2>(Sql, factory, alias1, alias2, parameters)
            {
                CommandTimeout = CommandTimeout,
                CommandType = CommandType
            };
        }

        public SqlQuery<T, T1, T2> SetOffsetParamValue(int value)
        {
            var p = GetParam<int>(Offset_Parameter_Name);
            if (p != null) p.Value = value;
            else _Parameters.Add(Offset_Parameter_Name, Parameter.Create(Offset_Parameter_Name, value));

            return this;
        }

        public SqlQuery<T, T1, T2> SetLimitParamValue(int value)
        {
            var p = GetParam<int>(Limit_Parameter_Name);
            if (p != null) p.Value = value;
            else _Parameters.Add(Limit_Parameter_Name, Parameter.Create(Limit_Parameter_Name, value));

            return this;
        }
    }

    public class SqlQuery
    {
        public const string Limit_Parameter_Name = "Limit_GnP23";
        public const string Offset_Parameter_Name = "Offset_GnP23";

        protected readonly Collections.HashMap<string, Parameter> _Parameters;
        public readonly string Sql;
        public int CommandTimeout = -1;
        public System.Data.CommandType CommandType = System.Data.CommandType.Text;

        public IEnumerable<Parameter> Parameters => _Parameters.Values;

        public SqlQuery(string sql, params Parameter[] parameters)
        {
            Sql = sql;
            int length = parameters.Length;
            _Parameters = new Collections.HashMap<string, Parameter>(length);
            for (int i = 0; i < length; i++)
            {
                var p = parameters[i];
                _Parameters.Set(p.Name, p.Copy());
            }
        }

        protected SqlQuery(string sql, IEnumerable<Parameter> parameters, int length)
        {
            Sql = sql;
            _Parameters = new Collections.HashMap<string, Parameter>(length);
            foreach (var p in parameters)
                _Parameters.Set(p.Name, p.Copy());
        }

        protected SqlQuery(string sql, Collections.HashMap<string, Parameter> parameters)
        {
            Sql = sql;
            _Parameters = parameters;
        }

        public Parameter<TValue> GetParam<TValue>(string name)
        {
            //int index = _Parameters.IndexOf(name);
            //if (index < 0)
            //    return null;
            //else
            //    return (Parameter<TValue>)_Parameters.GetValueByIndex(index);

            Parameter p = null;
            _Parameters.TryGetValue(name, ref p);
            return (Parameter<TValue>)p;
        }
    }


    public class UpdateObjectCompiledQuery<T>
    {
        public readonly string Sql;
        private readonly Func<T, Parameter[]> _ParametersFactory;
        public int CommandTimeout = -1;
        internal UpdateObjectCompiledQuery(string sql, Func<T, Parameter[]> parametersFactory)
        {
            Sql = sql;
            _ParametersFactory = parametersFactory;
        }

        public Parameter[] GetParameters(T obj)
        {
            return _ParametersFactory(obj);
        }

       public SqlQuery<T> GetQuery(T obj)
       {
            return new SqlQuery<T>(Sql, GetParameters(obj)) { CommandTimeout = CommandTimeout };
       }

    }

    public class BatchUpdateCompiledQuery<T>
    {
        private readonly Func<T[], string> _SqlFactory;
        private readonly Func<T[], IEnumerable<Parameter>> _ParametersFactory;
        public int CommandTimeout = -1;
        internal BatchUpdateCompiledQuery(Func<T[], string> sqlFactory, Func<T[], IEnumerable<Parameter>> parametersFactory)
        {
            _SqlFactory = sqlFactory;
            _ParametersFactory = parametersFactory;
        }

        public IEnumerable<Parameter> GetParameters(T[] items)
        {
            return _ParametersFactory(items);
        }

        public SqlQuery<T> GetQuery(T[] items)
        {
            return new SqlQuery<T>(_SqlFactory(items), GetParameters(items).ToArray()) { CommandTimeout = CommandTimeout };
        }

    }
}
