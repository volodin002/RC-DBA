using RC.DBA.Metamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace RC.DBA.Emit
{
    public class EntityEqualityComparerEmitter<T>
    {
        private static int _DynamicMethodNumber;

        List<IEntityAttribute> _attributes;
        IModelManager _modelManager;

        Label _returnFalse;
        Dictionary<Type, LocalBuilder> _locals;


        public EntityEqualityComparerEmitter(IModelManager modelManager)
        {
            var entity = modelManager.Entity<T>();
            _attributes = entity.AllAttributes.Where(a => !a.IsAssociation).ToList();

            _modelManager = modelManager;
        }

        public EntityEqualityComparerEmitter<T> Exclude<TProp>(Expression<Func<T, TProp>> expression)
        {
            var memberInfo = Helper.Member(expression);
            var entityAttrs = _modelManager.Entity<T>().GetAttributes(memberInfo.Name);
            foreach(var attr in entityAttrs)
            {
                _attributes.Remove(attr);
            }

            return this;
        }

        public Func<T, T, bool> EmitEqualityComparer()
        {
            System.Threading.Interlocked.Increment(ref _DynamicMethodNumber);
            var method = new DynamicMethod("_$EqualityComparer_" + typeof(T).Name + _DynamicMethodNumber.ToString(),
               typeof(bool), new[] { typeof(T), typeof(T) }, true);

            var gen = method.GetILGenerator();
            _returnFalse = gen.DefineLabel();

            foreach (var attr in _attributes)
            {
                EmitAttributeCopmare(gen, attr);       
            }

            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Ret); // return true;
            
            gen.MarkLabel(_returnFalse);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Ret); // return false;
            
            return method.CreateDelegate(typeof(Func<T, T, bool>)) as Func<T, T, bool>;
        }

        private void EmitAttributeCopmare(ILGenerator gen, IEntityAttribute attr)
        {
            var member = attr.Member;
            var type = attr.MemberType;
            var isNullable = type.IsValueType && Helper.IsNullableType(type); // is Nullable<Type>
            MethodInfo hasValueGetter = null;
            MethodInfo getDefaultVal = null;
            LocalBuilder loc = null;

            gen.Emit(OpCodes.Ldarg_0);
            EmitGetMember(gen, attr);

            if (isNullable)
            {
                hasValueGetter = type.GetProperty("HasValue").GetGetMethod();
                getDefaultVal = type.GetMethod("GetValueOrDefault", Type.EmptyTypes);

                if (_locals == null) _locals = new Dictionary<Type, LocalBuilder>();
                if(!_locals.TryGetValue(type, out loc))
                {
                    _locals[type] = loc = gen.DeclareLocal(type);
                }

                gen.Emit(OpCodes.Stloc, loc);
                gen.Emit(OpCodes.Ldloca, loc);
                gen.Emit(OpCodes.Call, hasValueGetter);
            }

            gen.Emit(OpCodes.Ldarg_1);
            if (attr.IsProperty)
                gen.Emit(OpCodes.Callvirt, attr.GetGetter());
            else
                gen.Emit(OpCodes.Ldfld, (FieldInfo)member);

            if (isNullable)
            {
                gen.Emit(OpCodes.Stloc, loc);
                gen.Emit(OpCodes.Ldloca, loc);
                gen.Emit(OpCodes.Call, hasValueGetter);

                gen.Emit(OpCodes.Ceq);

                gen.Emit(OpCodes.Brfalse, _returnFalse); // one has value but other has not value => this properties is not equal
            }

            if (type.IsValueType) // value type
            {
                if(isNullable) // Nullable<Type>
                {
                    gen.Emit(OpCodes.Ldarg_0);
                    EmitGetMember(gen, attr);
                    gen.Emit(OpCodes.Stloc, loc);
                    gen.Emit(OpCodes.Ldloca, loc);
                    gen.Emit(OpCodes.Call, getDefaultVal);

                    gen.Emit(OpCodes.Ldarg_1);
                    EmitGetMember(gen, attr);
                    gen.Emit(OpCodes.Stloc, loc);
                    gen.Emit(OpCodes.Ldloca, loc);
                    gen.Emit(OpCodes.Call, getDefaultVal);

                    type = Helper.GetNonNullableType(type);
                }

                var code = Type.GetTypeCode(type);
                switch(code)
                {
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.Char:
                    case TypeCode.Double:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.SByte:
                    case TypeCode.Single:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        gen.Emit(OpCodes.Ceq);
                        break;
                    default:
                        var op = type.GetMethod("op_Equality", BindingFlags.Static | BindingFlags.Public);
                        gen.Emit(OpCodes.Call, op);
                        break;
                }
                
                
            }
            else // Ref type
            {
                if (type == typeof(string))
                    EmitStringCompare(gen);
                else
                    throw new NotSupportedException("Type " + type.FullName + 
                        " is not supported in " + typeof(EntityEqualityComparerEmitter<T>).Name);
            }

            gen.Emit(OpCodes.Brfalse, _returnFalse);
        }

        private static void EmitStringCompare(ILGenerator gen)
        {
            var op = typeof(string).GetMethod("op_Equality", BindingFlags.Static | BindingFlags.Public);

            gen.Emit(OpCodes.Call, op);
        }

        private static void EmitGetMember(ILGenerator gen, IEntityAttribute attr)
        {
            if (attr.IsProperty)
                gen.Emit(OpCodes.Callvirt, attr.GetGetter());
            else
                gen.Emit(OpCodes.Ldfld, (FieldInfo)attr.Member);
        }

    }
}
