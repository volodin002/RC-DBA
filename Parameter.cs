using System;
using System.Collections.Generic;
using System.Data.Common;


namespace RC.DBA
{
    public class Parameter<T> : Parameter
    {
        public Parameter(string name)
        {
            Name = name;
        }

        public T Value;

        public override object GetValue()
        {
            return Value;
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

        internal override Parameter Copy()
        {
            return new Parameter<T>(Name) { Value = Value, Direction = Direction, Size = Size };
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

        internal abstract Parameter Copy();

        
        public static Parameter<T> Create<T>(string name, T value)
        {
            return new Parameter<T>(name) { Value = value };
        }

        public static Parameter<T> Create<T>(string name, T value, int size)
        {
            return new Parameter<T>(name) { Value = value, Size = size };
        }

        public DbParameter DbParameter(DbCommand cmd)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = Name;
            p.Value = GetValue() ?? DBNull.Value;
            p.Direction = Direction;
            if (Size > 0)
                p.Size = Size;

            return p;
        }

        public DbParameter AddDbParameter(DbCommand cmd)
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
