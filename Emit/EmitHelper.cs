using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace RC.DBA.Emit
{
    public class EmitHelper
    {
        public static void EmitNewWithDefaultCtor(ILGenerator gen, Type type)
        {
            if (type.IsValueType)
            {
                var local = gen.DeclareLocal(type);
                gen.Emit(OpCodes.Ldloca_S, local);
                gen.Emit(OpCodes.Initobj, type);
                gen.Emit(OpCodes.Ldloca_S, local);
            }
            else
            {
                gen.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
            }
        }

        public static void EmitNullableCtor(ILGenerator gen, Type valueType)
        {
            if (Helper.IsNullableType(valueType)) // If valueType is Nullable<?>
            {
                // [..., value]

                var type = Helper.GetNonNullableType(valueType);
                var ctor = typeof(Nullable<>).MakeGenericType(type).GetConstructor(new Type[] { type });
                gen.Emit(OpCodes.Newobj, ctor);

                // [..., new Nullable<valueType>(value)]
            }
        }

        public static void EmitDefault(ILGenerator il, Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Object:
                case TypeCode.DateTime:
                    if (type.IsValueType)
                    {
                        // Type.GetTypeCode on an enum returns the underlying
                        // integer TypeCode, so we won't get here.
                        //Debug.Assert(!type.IsEnum);

                        // This is the IL for default(T) if T is a generic type 
                        // parameter, so it should work for any type. It's also 
                        // the standard pattern for structs.
                        LocalBuilder lb = il.DeclareLocal(type);
                        il.Emit(OpCodes.Ldloca, lb);
                        il.Emit(OpCodes.Initobj, type);
                        il.Emit(OpCodes.Ldloc, lb);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldnull);
                    }
                    break;

                case TypeCode.Empty:
                case TypeCode.String:
                case TypeCode.DBNull:
                    il.Emit(OpCodes.Ldnull);
                    break;

                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    il.Emit(OpCodes.Ldc_I4_0);
                    break;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Conv_I8);
                    break;

                case TypeCode.Single:
                    il.Emit(OpCodes.Ldc_R4, default(Single));
                    break;

                case TypeCode.Double:
                    il.Emit(OpCodes.Ldc_R8, default(Double));
                    break;

                case TypeCode.Decimal:
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Newobj, typeof(Decimal).GetConstructor(new Type[] { typeof(int) }));
                    break;

                default:
                    throw new NotSupportedException($"Not supported: {type.Name}");
            }
        }
    }
}
