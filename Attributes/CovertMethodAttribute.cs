using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RC.DBA.Attributes
{
    public class CovertMethodAttribute : Attribute
    {
        private MethodInfo _ConvertMethod;
        public MethodInfo ConvertMethod { get => _ConvertMethod; set => _ConvertMethod= value; }
        public CovertMethodAttribute(Type staticClassType, string methodName)
        {
            _ConvertMethod = staticClassType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
        }
    }
}
