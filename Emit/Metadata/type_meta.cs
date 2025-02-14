using RC.DBA.Attributes;
using RC.DBA.Metamodel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RC.DBA.Emit
{
    class type_meta_base
    {
        public readonly Type type;
        public List<prop_meta> props;
        public List<obj_prop_meta> obj_props;
        public List<obj_prop_meta> collection_props;
        protected IModelManager _manager;
        public type_meta_base(Type type, IModelManager modelManager)
        {
            this.type = type;
            _manager = modelManager;
        }

        public bool HasCollectionProp()
        {
            return collection_props != null && collection_props.Count > 0;
        }

        internal obj_prop_meta create_obj_prop(MemberInfo memInfo)
        {
            Type obj_type;
            if (memInfo is PropertyInfo propInfo)
                obj_type = propInfo.PropertyType;
            else
                obj_type = ((FieldInfo)memInfo).FieldType;

            bool is_collection = false;

            if (typeof(IEnumerable).IsAssignableFrom(obj_type))
            {
                is_collection = true;
                var element_types = obj_type.GetGenericArguments();
                if (element_types.Length != 1)
                    throw new NotSupportedException(obj_type.FullName + " is not supported as entity property type");

                obj_type = element_types[0];
            }


            var prop = new obj_prop_meta(memInfo, obj_type, _manager);
            

            if (is_collection)
            {
                if (collection_props == null) collection_props = new List<obj_prop_meta>();
                collection_props.Add(prop);
            }
            else
            {
                if (obj_props == null) obj_props = new List<obj_prop_meta>();
                obj_props.Add(prop);
            }
            return prop;
        }
    }

    class type_meta : type_meta_base
    {
        public type_meta_base[] implementations; // implementation of base Type
        public prop_meta id_prop;
        public prop_meta_loc descriminator_prop;
        

        public type_meta(Type type, IModelManager modelManager) : base(type, modelManager)
        {
            if (type.IsAbstract)
            {
                SetTypeImplementations();
            }
        }

        internal void SetTypeImplementations()
        {
            /*
            var types = type.Assembly.GetTypes()
                .Where(t => 
                    t.IsClass && 
                    !t.IsAbstract && 
                    type.IsAssignableFrom(t) && 
                    t.GetCustomAttribute<DescriminatorValueAttribute>() != null
                )
                .ToList();
                */
            var types = _manager.GetImplementations(type).Select(t => t.ClassType).ToList();

            if (types.Count == 0) return;
            int max = types.Max(t => t.GetCustomAttribute<DescriminatorValueAttribute>().Value);

            type_meta_base[] res = new type_meta_base[max + 1];
            foreach (var t in types)
            {
                var attr = t.GetCustomAttribute<DescriminatorValueAttribute>();
                res[attr.Value] = new type_meta_base(t, _manager);
            }

            implementations = res;
        }

        internal void set_meta_prop(string name, int names_indx, string[] names, int ordinal)
        {
            if (names.Length - 1 > names_indx)
            {
                set_meta_obj_prop(name, names_indx, names, ordinal);
            }
            else
            {
                set_meta_simple_prop(name, names_indx, names, ordinal);
            }

        }

        void set_meta_obj_prop(string name, int names_indx, string[] names, int ordinal)
        {

            string field = names[names_indx];
            string typeName = null;
            if (field.Contains('$'))
            {
                var fields = field.Split('$');
                if (fields.Length == 2)
                {
                    typeName = fields[0];
                    field = fields[1];
                }
            }

            obj_prop_meta prop = null;
            prop = obj_props?.FirstOrDefault(p => p.prop.Name == field);
            if (prop == null) prop = collection_props?.FirstOrDefault(p => p.prop.Name == field);
            if (prop == null)
            {
                MemberInfo memInfo = get_member(type, field); 
                if (memInfo != null)
                {
                    prop =  create_obj_prop(memInfo);
                }
                
                if (prop == null && implementations != null)
                {

                    foreach (var impl in implementations)
                    {
                        if (impl == null) continue;

                        MemberInfo m = get_member(impl.type, field); 
                            
                        if (m == null) continue;
                        if (typeName != null && !HasTypeName(impl.type, typeName)) continue;

                        var prop0 = impl.obj_props?.FirstOrDefault(x => x.prop.Name == field);
                        if (prop0 == null) prop0 = collection_props?.FirstOrDefault(x => x.prop.Name == field);

                        if (prop0 == null)
                            prop0 = impl.create_obj_prop(m);

                        if (prop0 != null)
                        {
                            prop0.set_meta_prop(name, names_indx + 1, names, ordinal);
                            prop = prop0;
                            break;
                        }
                    }
                }

            }

            if (prop != null)
                prop.set_meta_prop(name, names_indx + 1, names, ordinal);
        }

        void set_meta_simple_prop(string name, int names_indx, string[] names, int ordinal)
        {
            string field = names[names_indx];
            if (field == "$type")
            {
                descriminator_prop = new prop_meta_loc { field_name = name, ordinal = ordinal };
                if (implementations == null)
                    SetTypeImplementations();
                
                return;
            }

            var propInfo = get_member(type, field);
            if (propInfo != null)
            {
                var entity = _manager.Entity(type);
                var propName = propInfo.Name;
                MethodInfo convertMethod = entity.GetAttributeConvertMethod(propName);

                var prop = new prop_meta()
                {
                    field_name = name,
                    prop = propInfo,
                    ordinal = ordinal,
                    convertMethod = convertMethod
                };
                if (entity.IsKey(propName))
                    id_prop = prop;
                else
                {
                    if (props == null) props = new List<prop_meta>();
                    props.Add(prop);
                }

                return;
            }

            if (implementations == null && field.Contains('$'))
            {
                SetTypeImplementations();
            }

            if (implementations == null) return;


            string typeName = null;
            if (field.Contains('$'))
            {
                var fields = field.Split('$');
                if (fields.Length == 2)
                {
                    typeName = fields[0];
                    field = fields[1];
                }
            }


            foreach (var impl in implementations)
            {
                if (impl == null) continue;

                var implType = impl.type;
                var p = get_member(implType, field);
                if (p == null) continue;
                if (typeName != null && !HasTypeName(implType, typeName)) continue;

                var entity = _manager.Entity(implType);
                MethodInfo convertMethod = entity?.GetAttributeConvertMethod(p.Name);

                var prop = new prop_meta()
                {
                    field_name = name,
                    prop = p,
                    ordinal = ordinal,
                    convertMethod = convertMethod,
                    
                };

                if (impl.props == null) impl.props = new List<prop_meta>();
                impl.props.Add(prop);
            }

        }


        static bool HasTypeName(Type type, string name)
        {
            while (type != null)
            {
                if (type.Name == name) return true;
                type = type.BaseType;
            }

            return false;
        }

        internal List<type_meta> get_types_with_cache(bool forceRootType)
        {
            var types = new HashSet<Type>();
            var res = new List<type_meta>();
            if(forceRootType && this.id_prop != null)
            {
                if (types.Add(this.type)) res.Add(this);
            }
            foreach (var m in get_types_with_collection_prop(this))
            {
                if (m.id_prop == null) continue;
                if(types.Add(m.type)) res.Add(m);
            }

            return res;
        }

        static IEnumerable<type_meta> get_types_with_collection_prop(type_meta meta)
        {
            if (meta.collection_props != null && meta.collection_props.Count > 0)
            {
                yield return meta;
                foreach(var p in meta.collection_props)
                {
                    yield return p;

                    foreach (var p0 in get_types_with_collection_prop(p))
                        yield return p0;
                }

            }

            if (meta.obj_props != null)
            {
                bool hasCollection = false;
                foreach (var p in meta.obj_props.SelectMany(x => get_types_with_collection_prop(x)))
                {
                    hasCollection = true;
                    yield return p;
                }
                if (hasCollection) yield return meta;
            }

            if (meta.implementations != null)
            {
                if (meta.implementations.Any(x => x != null && x.collection_props != null && x.collection_props.Count > 0))
                    yield return meta;

                foreach (var p in meta.implementations.Where(x => x != null && x.obj_props != null).SelectMany(x => x.obj_props))
                {
                    foreach (var m in get_types_with_collection_prop(p))
                        yield return m;
                }
                    
            }
        }

        static MemberInfo get_member(Type type, string field)
        {
            return (MemberInfo)type.GetProperty(field)
                    ?? type.GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

    }
}
