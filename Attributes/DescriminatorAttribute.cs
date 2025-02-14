using System;


namespace RC.DBA.Attributes
{
    public class DescriminatorAttribute : Attribute
    {
        public string Column { get; }

        public DescriminatorAttribute(string column)
        {
            this.Column = column;
        }
    }
}
