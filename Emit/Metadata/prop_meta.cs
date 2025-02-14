using RC.DBA.Metamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;


namespace RC.DBA.Emit
{
    class prop_meta
    {
        //public PropertyInfo prop;
        public MemberInfo prop;
        public int ordinal;
        public string field_name;

        public MethodInfo convertMethod;

    }

    class prop_meta_loc : prop_meta
    {
        public LocalBuilder loc; // Emitter local variable reference
    }

    class obj_prop_meta : type_meta
    {
        public readonly MemberInfo prop;

        public obj_prop_meta(MemberInfo prop, Type type, IModelManager modelManager) : base(type, modelManager)
        {
            this.prop = prop;
        }
    }
}
