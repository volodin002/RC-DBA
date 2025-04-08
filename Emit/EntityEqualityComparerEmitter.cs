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
        Dictionary<Type, LocalBuilder> _locals = new Dictionary<Type, LocalBuilder>();


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
            var methodNum = System.Threading.Interlocked.Increment(ref _DynamicMethodNumber);
            var method = new DynamicMethod("_$EqualityComparer_" + typeof(T).Name + methodNum.ToString(),
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
            var type = attr.MemberType;
            var isNullable = type.IsValueType && Helper.IsNullableType(type); // is Nullable<Type>
            MethodInfo hasValueGetter = null;

            gen.Emit(OpCodes.Ldarg_0); //... => [item0]
            
            if (isNullable)
            {
                hasValueGetter = type.GetProperty("HasValue").GetGetMethod();

                EmitGetMemberRef(gen, attr); // [item0] => [item0.&attr] (Nullable<Type>&)
                gen.Emit(OpCodes.Call, hasValueGetter); // [item0.&attr](Nullable<Type>&) => [Nullable<Type>.HasValue]
            }
            else
                EmitGetMember(gen, attr); // [item0] => [value]

            gen.Emit(OpCodes.Ldarg_1); //... => [item1]
            
            if (isNullable)
            {
                EmitGetMemberRef(gen, attr); // [item1] => [item1.&attr] (Nullable<Type>&)
                gen.Emit(OpCodes.Call, hasValueGetter); // [item1.&attr](Nullable<Type>&) => [Nullable<Type>.HasValue]

                gen.Emit(OpCodes.Ceq); // if(item0.Nullable<Type>.HasValue == item1.Nullable<Type>.HasValue) =>

                gen.Emit(OpCodes.Brfalse, _returnFalse); // one has value but other has not value => this properties is not equal
            }
            else
                EmitGetMember(gen, attr); // [item1] => [value]

            if (type.IsValueType) // value type
            {
                if (isNullable) // Nullable<Type>
                {
                    MethodInfo getDefaultVal = type.GetMethod("GetValueOrDefault", Type.EmptyTypes);

                    gen.Emit(OpCodes.Ldarg_0);
                    EmitGetMemberRef(gen, attr);
                    gen.Emit(OpCodes.Call, getDefaultVal);

                    gen.Emit(OpCodes.Ldarg_1);
                    EmitGetMemberRef(gen, attr);
                    gen.Emit(OpCodes.Call, getDefaultVal);

                    type = Helper.GetNonNullableType(type);
                }

                var code = Type.GetTypeCode(type);
                switch (code)
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
                        if (op == null) throw new NotSupportedException($"Cannot find op_Equality in {type}");
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

        private static void EmitGetMember(ILGenerator gen, IEntityAttribute attr) // [item] => [item.value]
        {
            if (attr.IsProperty)
                gen.Emit(OpCodes.Callvirt, attr.GetGetter());
            else
                gen.Emit(OpCodes.Ldfld, (FieldInfo)attr.Member);
        }

        /// <summary>
        /// For Value Type members only!!!
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="valMember"></param>
        /// <exception cref="NotSupportedException"></exception>
        private void EmitGetMemberRef(ILGenerator gen, IEntityAttribute attr) // [item] => [item.&value]
        {
            if (attr.IsProperty)
            {
                var type = attr.MemberType;
                
                if (!_locals.TryGetValue(type, out var loc))
                {
                    _locals[type] = loc = gen.DeclareLocal(type);
                }

                gen.Emit(OpCodes.Callvirt, attr.GetGetter());
                gen.Emit(OpCodes.Stloc, loc);
                gen.Emit(OpCodes.Ldloca, loc);
            }
            else
                gen.Emit(OpCodes.Ldflda, (FieldInfo)attr.Member);

        }

    }
}
