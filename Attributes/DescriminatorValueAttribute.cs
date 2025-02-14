using System;


namespace RC.DBA.Attributes
{
    public class DescriminatorValueAttribute : Attribute
    {
        public int Value { get; }

        public DescriminatorValueAttribute(int val)
        {
            this.Value = val;
        }
    }
}
