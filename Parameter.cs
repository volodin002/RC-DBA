using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;


namespace RC.DBA
{
    public class Parameter<T> : Parameter
    {
        public Parameter(string name)
        {
            Name = name;
        }

        public Parameter(string name, T value)
        {
            Name = name;
            Value = value;
        }

        public T Value;

        public override object GetValue()
        {
            return Value;
        }

        public void SetValue(T value)
        {
            Value = value;
        }

        public override void SetValue(object value)
        {
            if (value == null || value == DBNull.Value)
                Value = default(T);
            else
                Value = (T)value;
        }

        public override Type ParameterType()
        {
            return typeof(T);
        }

        public override DbType ParameterDbTypeFromType()
        {
            var type = typeof(T);
            if (type.IsValueType) type = Helper.GetNonNullableType(type);

            if (type == typeof(string))
                return DbType.String;
            else if (type == typeof(int))
                return DbType.Int32;
            else if (type == typeof(bool))
                return DbType.Boolean;
            else if (type == typeof(DateTime))
                return DbType.DateTime;
            else if (type == typeof(long))
                return DbType.Int64;
            else if (type == typeof(float))
                return DbType.Single;
            else if (type == typeof(double))
                return DbType.Double;
            else if (type == typeof(decimal))
                return DbType.Decimal;
            else if (type == typeof(Guid))
                return DbType.Guid;
            else if (type == typeof(byte[]))
                return DbType.Binary;
            else
                return DbType.Object;

        }

        internal override Parameter Copy()
        {
            return new Parameter<T>(Name) { Value = Value, Direction = Direction, Size = Size };
        }
    }

    public class ParameterArray<T> : Parameter<T[]>
    {
        public ParameterArray(string name) : base(name)
        {
        }

        public ParameterArray(string name, T[] value) : base(name, value)
        {
        }
 
        public override void SetValue(object value)
        {
            if (value == null)
                Value = null;
            else
                Value = (T[])value;
        }
       
        internal override Parameter Copy()
        {
            return new ParameterArray<T>(Name) { Value = Value, Direction = Direction, Size = Size };
        }

        public override DbParameter DbParameter(DbCommand cmd)
        {
            throw new NotImplementedException("DbParameter method cannto be used for ParameterArray<T> class");
        }

        public override DbParameter AddDbParameter(DbCommand cmd)
        {
            if (Value == null) return null;

            var length = Value.Length;
            if(length == 0) return null;

            for (int i = 0; i < length; i++)
            {
                var value = Value[i];
                var p = cmd.CreateParameter();
                p.ParameterName = $"{Name}{i.ToString()}";
                
                p.Value = value != null 
                        ? (object)value
                        : DBNull.Value;

                if (value == null)
                    p.DbType = ParameterDbTypeFromType();

                p.Direction = Direction;
                if (Size > 0)
                    p.Size = Size;

                cmd.Parameters.Add(p);
            }

            return null;
        }
    }

    public abstract class Parameter
    {
        public string Name;

        public int Size;

        public System.Data.ParameterDirection Direction = System.Data.ParameterDirection.Input;

        //int getPosition();
        public abstract object GetValue();

        public abstract void SetValue(object value);

        public abstract Type ParameterType();

        public abstract DbType ParameterDbTypeFromType();

        internal abstract Parameter Copy();

        
        public static Parameter<T> Create<T>(string name, T value)
        {
            return new Parameter<T>(name) { Value = value };
        }

        public static Parameter CreateFromObject(string name, Type type, object value)
        {
           
            if (type == typeof(string))
                return Parameter.Create<string>(name, value != null ? (string)value : null);
            else if (type == typeof(int))
                return Parameter.Create<int>(name, value != null ? (int)value : 0);
            else if (type == typeof(int?))
                return Parameter.Create<int?>(name, value != null ? (int?)value : null);
            else if (type == typeof(bool))
                return Parameter.Create<bool>(name, value != null ? (bool)value : false);
            else if (type == typeof(bool?))
                return Parameter.Create<bool?>(name, value != null ? (bool?)value : null);
            else if (type == typeof(DateTime))
                return Parameter.Create<DateTime>(name, value != null ? (DateTime)value : new DateTime(0L));
            else if (type == typeof(DateTime?))
                return Parameter.Create<DateTime?>(name, value != null ? (DateTime?)value : null);
            else if (type == typeof(long))
                return Parameter.Create<long>(name, value != null ? (long)value : 0L);
            else if (type == typeof(long?))
                return Parameter.Create<long?>(name, value != null ? (long?)value : null);
            else if (type == typeof(float))
                return Parameter.Create<float>(name, value != null ? (float)value : 0f);
            else if (type == typeof(float?))
                return Parameter.Create<float?>(name, value != null ? (float?)value : null);
            else if (type == typeof(double))
                return Parameter.Create<double>(name, value != null ? (double)value : 0d);
            else if (type == typeof(double?))
                return Parameter.Create<double?>(name, value != null ? (double?)value : null);
            else if (type == typeof(decimal))
                return Parameter.Create<decimal>(name, value != null ? (decimal)value : 0);
            else if (type == typeof(decimal?))
                return Parameter.Create<decimal?>(name, value != null ? (decimal?)value : null);
            else if (type == typeof(Guid))
                return Parameter.Create<Guid>(name, value != null ? (Guid)value : new Guid());
            else if (type == typeof(Guid?))
                return Parameter.Create<Guid?>(name, value != null ? (Guid?)value : null);
            else
                throw new NotSupportedException($"Parameter with type '{type.FullName}' is not supported");
        }

        public static Parameter<T> Create<T>(string name, T value, int size)
        {
            return new Parameter<T>(name) { Value = value, Size = size };
        }

        public static ParameterArray<T> CreateArray<T>(string name, T[] value)
        {
            return new ParameterArray<T>(name) { Value = value };
        }

        public static ParameterArray<T> CreateArray<T>(string name, T[] value, int size)
        {
            return new ParameterArray<T>(name) { Value = value, Size = size };
        }

        public virtual DbParameter DbParameter(DbCommand cmd)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = Name;
            var value = GetValue();
            p.Value = value ?? DBNull.Value;
            if (value == null)
                p.DbType = ParameterDbTypeFromType();

            p.Direction = Direction;
            if (Size > 0)
                p.Size = Size;

            return p;
        }

        public virtual DbParameter AddDbParameter(DbCommand cmd)
        {
            var p = DbParameter(cmd);
            cmd.Parameters.Add(p);

            return p;
        }

        public Parameter AddPrefix(string prefix)
        {
            Name += prefix;
            return this;
        }
    }
}
