using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ILRuntime.CLR.Method;

namespace ILRuntime.Runtime.Intepreter.OpCodes
{
  
    /// <summary>
    /// IL指令
    /// </summary>
    struct OpCode
    {
        /// <summary>
        /// 当前指令
        /// </summary>
        public OpCodeEnum Code;

        /// <summary>
        ///  Int32 操作数
        /// </summary>
        public int TokenInteger;

        /// <summary>
        /// Int64 操作数
        /// </summary>
        public long TokenLong;
    }

    /// <summary>
    /// Register machine opcode
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    struct OpCodeR
    {
        [FieldOffset(0)]
        public OpCodeREnum Code;
        [FieldOffset(4)]
        public short Register1;
        [FieldOffset(6)]
        public short Register2;
        [FieldOffset(8)]
        public short Register3;
        [FieldOffset(10)]
        public short Register4;
        [FieldOffset(8)]
        public int Operand;
        [FieldOffset(8)]
        public float OperandFloat;
        [FieldOffset(12)]
        public int Operand2;
        [FieldOffset(16)]
        public int Operand3;
        [FieldOffset(12)]
        public long OperandLong;
        [FieldOffset(12)]
        public double OperandDouble;
        [FieldOffset(20)]
        public int Operand4;

        public override string ToString()
        {

            return ToString(null);
        }

        public string ToString(Enviorment.AppDomain domain)
        {
            string param = null;
            switch (Code)
            {
                case OpCodeREnum.Move:
                case OpCodeREnum.Ldind_I:
                case OpCodeREnum.Ldind_I1:
                case OpCodeREnum.Ldind_I2:
                case OpCodeREnum.Ldind_I4:
                case OpCodeREnum.Ldind_I8:
                case OpCodeREnum.Ldind_R4:
                case OpCodeREnum.Ldind_R8:
                case OpCodeREnum.Ldind_U1:
                case OpCodeREnum.Ldind_U2:
                case OpCodeREnum.Ldind_U4:
                case OpCodeREnum.Ldind_Ref:
                case OpCodeREnum.Ldobj:
                case OpCodeREnum.Stind_I:
                case OpCodeREnum.Stind_I1:
                case OpCodeREnum.Stind_I2:
                case OpCodeREnum.Stind_I4:
                case OpCodeREnum.Stind_I8:
                case OpCodeREnum.Stind_R4:
                case OpCodeREnum.Stind_R8:
                case OpCodeREnum.Stind_Ref:
                case OpCodeREnum.Stobj:
                case OpCodeREnum.Ldloca:
                case OpCodeREnum.Ldloca_S:
                case OpCodeREnum.Ldarga:
                case OpCodeREnum.Ldarga_S:
                case OpCodeREnum.Conv_I:
                case OpCodeREnum.Conv_I1:
                case OpCodeREnum.Conv_I2:
                case OpCodeREnum.Conv_I4:
                case OpCodeREnum.Conv_I8:
                case OpCodeREnum.Conv_Ovf_I:
                case OpCodeREnum.Conv_Ovf_I1:
                case OpCodeREnum.Conv_Ovf_I1_Un:
                case OpCodeREnum.Conv_Ovf_I2:
                case OpCodeREnum.Conv_Ovf_I2_Un:
                case OpCodeREnum.Conv_Ovf_I4:
                case OpCodeREnum.Conv_Ovf_I4_Un:
                case OpCodeREnum.Conv_Ovf_I8:
                case OpCodeREnum.Conv_Ovf_I8_Un:
                case OpCodeREnum.Conv_Ovf_I_Un:
                case OpCodeREnum.Conv_Ovf_U:
                case OpCodeREnum.Conv_Ovf_U1:
                case OpCodeREnum.Conv_Ovf_U1_Un:
                case OpCodeREnum.Conv_Ovf_U2:
                case OpCodeREnum.Conv_Ovf_U2_Un:
                case OpCodeREnum.Conv_Ovf_U4:
                case OpCodeREnum.Conv_Ovf_U4_Un:
                case OpCodeREnum.Conv_Ovf_U8:
                case OpCodeREnum.Conv_Ovf_U8_Un:
                case OpCodeREnum.Conv_Ovf_U_Un:
                case OpCodeREnum.Conv_R4:
                case OpCodeREnum.Conv_R8:
                case OpCodeREnum.Conv_R_Un:
                case OpCodeREnum.Conv_U:
                case OpCodeREnum.Conv_U1:
                case OpCodeREnum.Conv_U2:
                case OpCodeREnum.Conv_U4:
                case OpCodeREnum.Conv_U8:
                case OpCodeREnum.Not:
                case OpCodeREnum.Neg:
                    param = string.Format("r{0}, r{1}", Register1, Register2);
                    break;
                case OpCodeREnum.Box:
                case OpCodeREnum.Unbox:
                case OpCodeREnum.Unbox_Any:
                case OpCodeREnum.Isinst:
                    if (domain == null)
                        param = string.Format("r{0}, r{1}, {2}", Register1, Register2, Operand);
                    else
                    {
                        var type = domain.GetType(Operand);
                        param = string.Format("r{0}, r{1}, {2}", Register1, Register2, type);
                    }
                    break;

                case OpCodeREnum.Stfld:
                case OpCodeREnum.Ldfld:
                    param = string.Format("r{0}, r{1}, 0x{2:X8}", Register1, Register2, OperandLong);
                    break;
                case OpCodeREnum.Stsfld:
                case OpCodeREnum.Ldsfld:
                    param = string.Format("r{0}, 0x{1:X8}", Register1, OperandLong);
                    break;

                case OpCodeREnum.Beqi:
                case OpCodeREnum.Bgei:
                case OpCodeREnum.Bgei_Un:
                case OpCodeREnum.Bgti:
                case OpCodeREnum.Bgti_Un:
                case OpCodeREnum.Bnei_Un:
                case OpCodeREnum.Blei:
                case OpCodeREnum.Blei_Un:
                case OpCodeREnum.Blti:
                case OpCodeREnum.Blti_Un:
                    if (Operand != 0)
                    {
                        param = string.Format("r{0},{1},{2}", Register1, Operand, Operand4);
                    }
                    else if (OperandLong != 0)
                    {
                        param = string.Format("r{0},{1},{2}", Register1, OperandLong, Operand4);
                    }
                    else if (OperandFloat != 0)
                    {
                        param = string.Format("r{0},{1},{2}", Register1, OperandFloat, Operand4);
                    }
                    else if (OperandDouble != 0)
                    {
                        param = string.Format("r{0},{1},{2}", Register1, OperandDouble, Operand4);
                    }
                    else
                    {
                        param = string.Format("r{0},0,{1}", Register1, Operand4);
                    }
                    break;
                case OpCodeREnum.Ceqi:
                case OpCodeREnum.Cgti:
                case OpCodeREnum.Cgti_Un:
                case OpCodeREnum.Clti:
                case OpCodeREnum.Clti_Un:
                case OpCodeREnum.Addi:
                case OpCodeREnum.Subi:
                case OpCodeREnum.Muli:
                case OpCodeREnum.Divi:
                case OpCodeREnum.Remi:
                case OpCodeREnum.Remi_Un:
                case OpCodeREnum.Andi:
                case OpCodeREnum.Ori:
                case OpCodeREnum.Xori:
                case OpCodeREnum.Shli:
                case OpCodeREnum.Shri:
                case OpCodeREnum.Shri_Un:
                    if (Operand != 0)
                    {
                        param = string.Format("r{0},r{1},{2}", Register1, Register2, Operand);
                    }
                    else if (OperandLong != 0)
                    {
                        param = string.Format("r{0},r{1},{2}", Register1, Register2, OperandLong);
                    }
                    else if (OperandFloat != 0)
                    {
                        param = string.Format("r{0},r{1},{2}", Register1, Register2, OperandFloat);
                    }
                    else if (OperandDouble != 0)
                    {
                        param = string.Format("r{0},r{1},{2}", Register1, Register2, OperandDouble);
                    }
                    else
                    {
                        param = string.Format("r{0},r{1},0", Register1, Register2);
                    }
                    break;
                case OpCodeREnum.Add:
                case OpCodeREnum.Add_Ovf:
                case OpCodeREnum.Add_Ovf_Un:
                case OpCodeREnum.Sub:
                case OpCodeREnum.Sub_Ovf:
                case OpCodeREnum.Sub_Ovf_Un:
                case OpCodeREnum.Mul:
                case OpCodeREnum.Mul_Ovf:
                case OpCodeREnum.Mul_Ovf_Un:
                case OpCodeREnum.Div:
                case OpCodeREnum.Div_Un:
                case OpCodeREnum.Rem:
                case OpCodeREnum.Rem_Un:
                case OpCodeREnum.Xor:
                case OpCodeREnum.And:
                case OpCodeREnum.Or:
                case OpCodeREnum.Shl:
                case OpCodeREnum.Shr:
                case OpCodeREnum.Shr_Un:
                case OpCodeREnum.Clt:
                case OpCodeREnum.Clt_Un:
                case OpCodeREnum.Cgt:
                case OpCodeREnum.Cgt_Un:
                case OpCodeREnum.Ceq:
                case OpCodeREnum.Stelem_I1:
                case OpCodeREnum.Stelem_I2:
                case OpCodeREnum.Stelem_I:
                case OpCodeREnum.Stelem_I4:
                case OpCodeREnum.Stelem_R4:
                case OpCodeREnum.Stelem_R8:
                case OpCodeREnum.Stelem_Any:
                case OpCodeREnum.Stelem_Ref:
                case OpCodeREnum.Ldelem_I1:
                case OpCodeREnum.Ldelem_I2:
                case OpCodeREnum.Ldelem_I:
                case OpCodeREnum.Ldelem_I4:
                case OpCodeREnum.Ldelem_R4:
                case OpCodeREnum.Ldelem_R8:
                case OpCodeREnum.Ldelem_Ref:
                case OpCodeREnum.Ldelem_Any:
                case OpCodeREnum.Ldelema:
                    param = string.Format("r{0},r{1},r{2}", Register1, Register2, Register3);
                    break;
                case OpCodeREnum.Ldc_I4_0:
                case OpCodeREnum.Ldc_I4_1:
                case OpCodeREnum.Ldc_I4_2:
                case OpCodeREnum.Ldc_I4_3:
                case OpCodeREnum.Ldc_I4_4:
                case OpCodeREnum.Ldc_I4_5:
                case OpCodeREnum.Ldc_I4_6:
                case OpCodeREnum.Ldc_I4_7:
                case OpCodeREnum.Ldc_I4_8:
                case OpCodeREnum.Ldc_I4_M1:
                case OpCodeREnum.Ldnull:
                case OpCodeREnum.Ret:
                case OpCodeREnum.Push:
                    param = string.Format("r{0}", Register1);
                    break;
                case OpCodeREnum.Brtrue:
                case OpCodeREnum.Brtrue_S:
                case OpCodeREnum.Brfalse:
                case OpCodeREnum.Brfalse_S:
                case OpCodeREnum.Switch:
                    param = string.Format("r{0}, {1}", Register1, Operand);
                    break;
                case OpCodeREnum.Ldftn:
                    if (domain == null)
                    {
                        param = string.Format("r{0}, {1}", Register1, Operand2);
                    }
                    else
                    {
                        IMethod m = domain.GetMethod(Operand2);
                        if (m is CLR.Method.CLRMethod)
                            param = m != null ? string.Format("r{0}, {1}::{2}", Register1, m.DeclearingType.FullName, m) : string.Format("r{0}, {1}", Register1, Operand2);
                        else
                            param = m != null ? string.Format("r{0}, {1}", Register1, m) : string.Format("r{0}, {1}", Register1, Operand2);
                    }
                    break;

                case OpCodeREnum.Ldvirtftn:
                    if (domain == null)
                    {
                        param = string.Format("r{0}, r{1} {2}", Register1, Register2, Operand2);
                    }
                    else
                    {
                        IMethod m = domain.GetMethod(Operand2);
                        if (m is CLR.Method.CLRMethod)
                            param = m != null ? string.Format("r{0}, r{1}, {2}::{3}", Register1, Register2, m.DeclearingType.FullName, m) : string.Format("r{0}, r{1}, {2}", Register1, Register2, Operand2);
                        else
                            param = m != null ? string.Format("r{0}, r{1}, {2}", Register1, Register2, m) : string.Format("r{0}, r{1}, {2}", Register1, Register2, Operand2);
                    }
                    break;
                case OpCodeREnum.Constrained:
                    {
                        if (domain == null)
                        {
                            param = Operand.ToString();
                        }
                        else
                        {
                            var m = domain.GetType(Operand);
                            param = m != null ? m.ToString() : Operand.ToString();
                        }
                    }
                    break;
                case OpCodeREnum.Call:
                case OpCodeREnum.Callvirt:
                case OpCodeREnum.Newobj:
                    {
                        string retR = Register1 >= 0 ? "r" + Register1 : "-";
                        if (Register2 >= 0)
                            retR += ", r" + Register2;
                        if (Register3 >= 0)
                            retR += ", r" + Register3;
                        if (Register4 >= 0)
                            retR += ", r" + Register4;

                        if (domain == null)
                        {
                            param = string.Format("{0}, {1}", retR, Operand2);
                        }
                        else
                        {
                            IMethod m = domain.GetMethod(Operand2);
                            if (m is CLR.Method.CLRMethod)
                                param = m != null ? string.Format("{0}, {1}::{2}", retR, m.DeclearingType.FullName, m) : string.Format("{0}, {1}", retR, Operand2);
                            else
                                param = m != null ? string.Format("{0}, {1}", retR, m) : string.Format("{0}, {1}", retR, Operand2);
                        }
                    }
                    break;
                case OpCodeREnum.Blt:
                case OpCodeREnum.Blt_S:
                case OpCodeREnum.Blt_Un:
                case OpCodeREnum.Blt_Un_S:
                case OpCodeREnum.Ble:
                case OpCodeREnum.Ble_S:
                case OpCodeREnum.Ble_Un:
                case OpCodeREnum.Ble_Un_S:
                case OpCodeREnum.Bgt:
                case OpCodeREnum.Bgt_S:
                case OpCodeREnum.Bgt_Un:
                case OpCodeREnum.Bgt_Un_S:
                case OpCodeREnum.Bge:
                case OpCodeREnum.Bge_S:
                case OpCodeREnum.Bge_Un:
                case OpCodeREnum.Bge_Un_S:
                case OpCodeREnum.Beq:
                case OpCodeREnum.Beq_S:
                case OpCodeREnum.Bne_Un:
                case OpCodeREnum.Bne_Un_S:
                    param = string.Format("r{0}, r{1}, {2}", Register1, Register2, Operand);
                    break;
                case OpCodeREnum.Br:
                case OpCodeREnum.Br_S:
                case OpCodeREnum.Leave:
                case OpCodeREnum.Leave_S:
                    param = string.Format("{0}", Operand);
                    break;
                case OpCodeREnum.Ldc_I4:
                case OpCodeREnum.Ldc_I4_S:
                    param = string.Format("r{0},{1}", Register1, Operand);
                    break;
                case OpCodeREnum.Ldc_I8:
                    param = string.Format("r{0},{1}", Register1, OperandLong);
                    break;
                case OpCodeREnum.Ldc_R4:
                    param = string.Format("r{0},{1}", Register1, OperandFloat);
                    break;
                case OpCodeREnum.Ldc_R8:
                    param = string.Format("r{0},{1}", Register1, OperandDouble);
                    break;
                case OpCodeREnum.Ldstr:
                    if (domain == null)
                        param = string.Format("r{0},0x{1:X}", Register1, OperandLong);
                    else
                        param = string.Format("r{0},\"{1}\"", Register1, domain.GetString(OperandLong));
                    break;
                case OpCodeREnum.Ldtoken:
                    if (domain == null)
                        param = string.Format("r{0},0x{1:X}", Register1, OperandLong);
                    else
                    {
                        switch (Operand)
                        {
                            case 0:
                                {
                                    var type = domain.GetType((int)(OperandLong >> 32));
                                    int fieldIdx = (int)OperandLong;
                                    param = string.Format("r{0},{1}.{2}", Register1, type.FullName, (type is CLR.TypeSystem.ILType) ? ((CLR.TypeSystem.ILType)type).TypeDefinition.Fields[fieldIdx].Name : ((CLR.TypeSystem.CLRType)type).Fields[fieldIdx].Name);                                    
                                }
                                break;
                            case 1:
                                {
                                    var type = domain.GetType((int)OperandLong);
                                    param = string.Format("r{0},\"{1}\"", Register1, type);
                                }
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    break;
                case OpCodeREnum.Initobj:
                    if (domain == null)
                        param = string.Format("r{0}, {1}", Register1, Operand);
                    else
                    {
                        var type = domain.GetType(Operand);
                        param = string.Format("r{0}, {1}", Register1, type);
                    }
                    break;
                case OpCodeREnum.Newarr:
                    if (domain == null)
                        param = string.Format("r{0}, r{1}", Register1, Register2);
                    else
                    {
                        var type = domain.GetType(Operand);
                        param = string.Format("r{0}, {2}, r{1}", Register1, Register2, type);
                    }
                    break;
            }
            return string.Format("{0} {1}", Code.ToString().ToLower().Replace('_', '.'), param);
        }
    }
}
