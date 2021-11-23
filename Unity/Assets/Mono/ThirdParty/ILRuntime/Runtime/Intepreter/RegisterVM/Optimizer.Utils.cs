using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.Mono.Cecil;
using ILRuntime.Mono.Cecil.Cil;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter.OpCodes;

namespace ILRuntime.Runtime.Intepreter.RegisterVM
{
    partial class Optimizer
    {
        public static bool SupportIntemediateValue(OpCodeREnum op)
        {
            switch (op)
            {
                case OpCodeREnum.Add:
                case OpCodeREnum.Sub:
                case OpCodeREnum.Mul:
                case OpCodeREnum.Div:
                case OpCodeREnum.Rem:
                case OpCodeREnum.Rem_Un:
                case OpCodeREnum.And:
                case OpCodeREnum.Or:
                case OpCodeREnum.Xor:
                case OpCodeREnum.Shl:
                case OpCodeREnum.Shr:
                case OpCodeREnum.Shr_Un:
                case OpCodeREnum.Beq:
                case OpCodeREnum.Beq_S:
                case OpCodeREnum.Bge:
                case OpCodeREnum.Bge_S:
                case OpCodeREnum.Bge_Un:
                case OpCodeREnum.Bge_Un_S:
                case OpCodeREnum.Bgt:
                case OpCodeREnum.Bgt_S:
                case OpCodeREnum.Bgt_Un:
                case OpCodeREnum.Bgt_Un_S:
                case OpCodeREnum.Bne_Un:
                case OpCodeREnum.Bne_Un_S:
                case OpCodeREnum.Ble:
                case OpCodeREnum.Ble_S:
                case OpCodeREnum.Ble_Un:
                case OpCodeREnum.Ble_Un_S:
                case OpCodeREnum.Blt:
                case OpCodeREnum.Blt_S:
                case OpCodeREnum.Blt_Un:
                case OpCodeREnum.Blt_Un_S:
                case OpCodeREnum.Ceq:
                case OpCodeREnum.Cgt:
                case OpCodeREnum.Cgt_Un:
                case OpCodeREnum.Clt:
                case OpCodeREnum.Clt_Un:
                    return true;
                default:
                    return false;
            }
        }

        public static void ReplaceRegisterWithConstant(ref OpCodeR op, ref OpCodeR constant)
        {
            switch (op.Code)
            {
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
                case OpCodeREnum.Ceqi:
                case OpCodeREnum.Cgti:
                case OpCodeREnum.Cgti_Un:
                case OpCodeREnum.Clti:
                case OpCodeREnum.Clti_Un:
                    op.Register3 = 0;
                    break;
                case OpCodeREnum.Beqi:
                case OpCodeREnum.Beq_S:
                case OpCodeREnum.Bgei:
                case OpCodeREnum.Bgei_Un:
                case OpCodeREnum.Bgti:
                case OpCodeREnum.Bgti_Un:
                case OpCodeREnum.Bnei_Un:
                case OpCodeREnum.Blei:
                case OpCodeREnum.Blei_Un:
                case OpCodeREnum.Blti:
                case OpCodeREnum.Blti_Un:
                    op.Register2 = 0;
                    op.Operand4 = op.Operand;
                    break;
            }

            switch (constant.Code)
            {
                case OpCodeREnum.Ldc_I4:
                case OpCodeREnum.Ldc_I4_S:
                    op.Operand = constant.Operand;
                    break;
                case OpCodeREnum.Ldc_I4_0:
                    op.Operand = 0;
                    break;
                case OpCodeREnum.Ldc_I4_1:
                    op.Operand = 1;
                    break;
                case OpCodeREnum.Ldc_I4_2:
                    op.Operand = 2;
                    break;
                case OpCodeREnum.Ldc_I4_3:
                    op.Operand = 3;
                    break;
                case OpCodeREnum.Ldc_I4_4:
                    op.Operand = 4;
                    break;
                case OpCodeREnum.Ldc_I4_5:
                    op.Operand = 5;
                    break;
                case OpCodeREnum.Ldc_I4_6:
                    op.Operand = 6;
                    break;
                case OpCodeREnum.Ldc_I4_7:
                    op.Operand = 7;
                    break;
                case OpCodeREnum.Ldc_I4_8:
                    op.Operand = 8;
                    break;
                case OpCodeREnum.Ldc_I4_M1:
                    op.Operand = -1;
                    break;
                case OpCodeREnum.Ldc_I8:
                    op.OperandLong = constant.OperandLong;
                    break;
                case OpCodeREnum.Ldc_R4:
                    op.OperandFloat = constant.OperandFloat;
                    break;
                case OpCodeREnum.Ldc_R8:
                    op.OperandDouble = constant.OperandDouble;
                    break;
            }
        }

        public static OpCodeREnum GetIntemediateValueOpcode(OpCodeREnum op)
        {
            switch (op)
            {
                case OpCodeREnum.Add:
                    return OpCodeREnum.Addi;
                case OpCodeREnum.Sub:
                    return OpCodeREnum.Subi;
                case OpCodeREnum.Mul:
                    return OpCodeREnum.Muli;
                case OpCodeREnum.Div:
                    return OpCodeREnum.Divi;
                case OpCodeREnum.Rem:
                    return OpCodeREnum.Remi;
                case OpCodeREnum.Rem_Un:
                    return OpCodeREnum.Remi_Un;
                case OpCodeREnum.And:
                    return OpCodeREnum.Andi;
                case OpCodeREnum.Or:
                    return OpCodeREnum.Ori;
                case OpCodeREnum.Xor:
                    return OpCodeREnum.Xori;
                case OpCodeREnum.Shl:
                    return OpCodeREnum.Shli;
                case OpCodeREnum.Shr:
                    return OpCodeREnum.Shri;
                case OpCodeREnum.Shr_Un:
                    return OpCodeREnum.Shri_Un;
                case OpCodeREnum.Beq:
                case OpCodeREnum.Beq_S:
                    return OpCodeREnum.Beqi;                
                case OpCodeREnum.Bge:
                case OpCodeREnum.Bge_S:
                    return OpCodeREnum.Bgei;
                case OpCodeREnum.Bge_Un:
                case OpCodeREnum.Bge_Un_S:
                    return OpCodeREnum.Bgei_Un;
                case OpCodeREnum.Bgt:
                case OpCodeREnum.Bgt_S:
                    return OpCodeREnum.Bgti;
                case OpCodeREnum.Bgt_Un:
                case OpCodeREnum.Bgt_Un_S:
                    return OpCodeREnum.Bgti_Un;
                case OpCodeREnum.Bne_Un:
                case OpCodeREnum.Bne_Un_S:
                    return OpCodeREnum.Bnei_Un;
                case OpCodeREnum.Ble:
                case OpCodeREnum.Ble_S:
                    return OpCodeREnum.Blei;
                case OpCodeREnum.Ble_Un:
                case OpCodeREnum.Ble_Un_S:
                    return OpCodeREnum.Blei_Un;
                case OpCodeREnum.Blt:
                case OpCodeREnum.Blt_S:
                    return OpCodeREnum.Blti;
                case OpCodeREnum.Blt_Un:
                case OpCodeREnum.Blt_Un_S:
                    return OpCodeREnum.Blti_Un;
                case OpCodeREnum.Ceq:
                    return OpCodeREnum.Ceqi;
                case OpCodeREnum.Cgt:
                    return OpCodeREnum.Cgti;
                case OpCodeREnum.Cgt_Un:
                    return OpCodeREnum.Cgti_Un;
                case OpCodeREnum.Clt:
                    return OpCodeREnum.Clti;
                case OpCodeREnum.Clt_Un:
                    return OpCodeREnum.Clti_Un;
                default:
                    throw new NotSupportedException();
            }
        }

        public static bool SupportOperandSwap(OpCodeREnum op)
        {
            switch (op)
            {
                case OpCodeREnum.Add:
                case OpCodeREnum.Mul:
                case OpCodeREnum.And:
                case OpCodeREnum.Or:
                case OpCodeREnum.Xor:
                case OpCodeREnum.Beq:
                case OpCodeREnum.Beq_S:
                case OpCodeREnum.Bne_Un:
                case OpCodeREnum.Bne_Un_S:
                case OpCodeREnum.Ceq:
                    return true;
                default:
                    return false;
            }
        }

        public static bool HasInverseOpcode(OpCodeREnum op)
        {
            switch (op)
            {
                case OpCodeREnum.Bge:
                case OpCodeREnum.Bge_S:
                case OpCodeREnum.Bge_Un:
                case OpCodeREnum.Bge_Un_S:
                case OpCodeREnum.Bgt:
                case OpCodeREnum.Bgt_S:
                case OpCodeREnum.Bgt_Un:
                case OpCodeREnum.Bgt_Un_S:
                case OpCodeREnum.Ble:
                case OpCodeREnum.Ble_S:
                case OpCodeREnum.Ble_Un:
                case OpCodeREnum.Ble_Un_S:
                case OpCodeREnum.Blt:
                case OpCodeREnum.Blt_S:
                case OpCodeREnum.Blt_Un:
                case OpCodeREnum.Blt_Un_S:
                case OpCodeREnum.Cgt:
                case OpCodeREnum.Cgt_Un:
                case OpCodeREnum.Clt:
                case OpCodeREnum.Clt_Un:
                    return true;
                default:
                    return false;
            }
        }

        public static OpCodeREnum GetInverseOpcode(OpCodeREnum op)
        {
            switch (op)
            {
                case OpCodeREnum.Bge:
                case OpCodeREnum.Bge_S:
                    return OpCodeREnum.Ble;
                case OpCodeREnum.Bge_Un:
                case OpCodeREnum.Bge_Un_S:
                    return OpCodeREnum.Ble_Un;
                case OpCodeREnum.Bgt:
                case OpCodeREnum.Bgt_S:
                    return OpCodeREnum.Blt;
                case OpCodeREnum.Bgt_Un:
                case OpCodeREnum.Bgt_Un_S:
                    return OpCodeREnum.Blt_Un;
                case OpCodeREnum.Ble:
                case OpCodeREnum.Ble_S:
                    return OpCodeREnum.Bge;
                case OpCodeREnum.Ble_Un:
                case OpCodeREnum.Ble_Un_S:
                    return OpCodeREnum.Bge_Un;
                case OpCodeREnum.Blt:
                case OpCodeREnum.Blt_S:
                    return OpCodeREnum.Bgt;
                case OpCodeREnum.Blt_Un:
                case OpCodeREnum.Blt_Un_S:
                    return OpCodeREnum.Bgt_Un;
                case OpCodeREnum.Cgt:
                    return OpCodeREnum.Clt;
                case OpCodeREnum.Clt:
                    return OpCodeREnum.Cgt;
                case OpCodeREnum.Clt_Un:
                    return OpCodeREnum.Cgt_Un;
                default:
                    throw new NotSupportedException();
            }
        }

        public static bool IsLoadConstant(OpCodeREnum op)
        {
            switch (op)
            {
                case OpCodeREnum.Ldc_I4:
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
                case OpCodeREnum.Ldc_I4_S:
                case OpCodeREnum.Ldc_I8:
                case OpCodeREnum.Ldc_R4:
                case OpCodeREnum.Ldc_R8:
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsBranching(OpCodeREnum op)
        {
            switch (op)
            {
                case OpCodeREnum.Br_S:
                case OpCodeREnum.Br:
                case OpCodeREnum.Brtrue:
                case OpCodeREnum.Brtrue_S:
                case OpCodeREnum.Brfalse:
                case OpCodeREnum.Brfalse_S:
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
                    return true;
            }
            return false;
        }
        public static bool IsIntermediateBranching(OpCodeREnum op)
        {
            switch (op)
            {
                case OpCodeREnum.Beqi:
                case OpCodeREnum.Beq_S:
                case OpCodeREnum.Bgei:
                case OpCodeREnum.Bgei_Un:
                case OpCodeREnum.Bgti:
                case OpCodeREnum.Bgti_Un:
                case OpCodeREnum.Bnei_Un:
                case OpCodeREnum.Blei:
                case OpCodeREnum.Blei_Un:
                case OpCodeREnum.Blti:
                case OpCodeREnum.Blti_Un:
                    return true;
            }
            return false;
        }
        public static bool GetOpcodeSourceRegister(ref OpCodeR op, bool hasReturn, out short r1, out short r2, out short r3)
        {
            r1 = -1;
            r2 = -1;
            r3 = -1;
            switch (op.Code)
            {
                case OpCodeREnum.Move:
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
                case OpCodeREnum.Box:
                case OpCodeREnum.Unbox:
                case OpCodeREnum.Unbox_Any:
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
                case OpCodeREnum.Ldloca:
                case OpCodeREnum.Ldloca_S:
                case OpCodeREnum.Ldarg_S:
                case OpCodeREnum.Ldarga:
                case OpCodeREnum.Ldarga_S:
                case OpCodeREnum.Ldlen:
                case OpCodeREnum.Newarr:
                case OpCodeREnum.Ldfld:
                case OpCodeREnum.Ldflda:
                case OpCodeREnum.Ldvirtftn:
                case OpCodeREnum.Isinst:
                    r1 = op.Register2;
                    return true;
                case OpCodeREnum.Stind_I:
                case OpCodeREnum.Stind_I1:
                case OpCodeREnum.Stind_I2:
                case OpCodeREnum.Stind_I4:
                case OpCodeREnum.Stind_I8:
                case OpCodeREnum.Stind_R4:
                case OpCodeREnum.Stind_R8:
                case OpCodeREnum.Stind_Ref:
                case OpCodeREnum.Stobj:
                case OpCodeREnum.Stfld:
                    r1 = op.Register1;
                    r2 = op.Register2;
                    return true;
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
                case OpCodeREnum.Ldc_I4:
                case OpCodeREnum.Ldc_I4_S:
                case OpCodeREnum.Ldc_I8:
                case OpCodeREnum.Ldc_R4:
                case OpCodeREnum.Ldc_R8:
                case OpCodeREnum.Ldstr:
                case OpCodeREnum.Ldtoken:
                case OpCodeREnum.Ldftn:
                case OpCodeREnum.Ldsfld:
                case OpCodeREnum.Ldsflda:
                case OpCodeREnum.Constrained:
                    return false;
                case OpCodeREnum.Callvirt:
                case OpCodeREnum.Call:
                case OpCodeREnum.Newobj:
                    r1 = op.Register2;
                    r2 = op.Register3;
                    r3 = op.Register4;
                    return true;

                case OpCodeREnum.Br_S:
                case OpCodeREnum.Br:
                case OpCodeREnum.Nop:
                case OpCodeREnum.InlineStart:
                case OpCodeREnum.InlineEnd:
                case OpCodeREnum.Castclass:
                case OpCodeREnum.Readonly:
                case OpCodeREnum.Leave:
                case OpCodeREnum.Leave_S:
                case OpCodeREnum.Endfinally:
                case OpCodeREnum.Volatile:
                case OpCodeREnum.Rethrow:
                    return false;
                case OpCodeREnum.Brtrue:
                case OpCodeREnum.Brtrue_S:
                case OpCodeREnum.Brfalse:
                case OpCodeREnum.Brfalse_S:
                case OpCodeREnum.Push:
                case OpCodeREnum.Initobj:
                case OpCodeREnum.Throw:
                case OpCodeREnum.Stsfld:
                case OpCodeREnum.Switch:
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
                    r1 = op.Register1;
                    return true;
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
                    r1 = op.Register1;
                    r2 = op.Register2;
                    return true;
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
                case OpCodeREnum.Ldelem_I1:
                case OpCodeREnum.Ldelem_U1:
                case OpCodeREnum.Ldelem_I2:
                case OpCodeREnum.Ldelem_U2:
                case OpCodeREnum.Ldelem_I4:
                case OpCodeREnum.Ldelem_U4:
                case OpCodeREnum.Ldelem_I8:
                case OpCodeREnum.Ldelem_R4:
                case OpCodeREnum.Ldelem_R8:
                case OpCodeREnum.Ldelem_Any:
                case OpCodeREnum.Ldelem_Ref:
                case OpCodeREnum.Ldelema:
                    r1 = op.Register2;
                    r2 = op.Register3;
                    return true;
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
                case OpCodeREnum.Ceqi:
                case OpCodeREnum.Cgti:
                case OpCodeREnum.Cgti_Un:
                case OpCodeREnum.Clti:
                case OpCodeREnum.Clti_Un:
                    r1 = op.Register2;
                    return true;
                case OpCodeREnum.Stelem_I:
                case OpCodeREnum.Stelem_I1:
                case OpCodeREnum.Stelem_I2:
                case OpCodeREnum.Stelem_I4:
                case OpCodeREnum.Stelem_I8:
                case OpCodeREnum.Stelem_R4:
                case OpCodeREnum.Stelem_R8:
                case OpCodeREnum.Stelem_Ref:
                case OpCodeREnum.Stelem_Any:
                    r1 = op.Register1;
                    r2 = op.Register2;
                    r3 = op.Register3;
                    return true;
                case OpCodeREnum.Ret:
                    if (hasReturn)
                    {
                        r1 = op.Register1;
                        return true;
                    }
                    else
                        return false;
                default:
                    throw new NotImplementedException();
            }
        }

        public static bool GetOpcodeDestRegister(ref OpCodes.OpCodeR op, out short r1)
        {
            r1 = -1;
            switch (op.Code)
            {
                case OpCodes.OpCodeREnum.Move:
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
                case OpCodeREnum.Box:
                case OpCodeREnum.Unbox:
                case OpCodeREnum.Unbox_Any:
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
                case OpCodeREnum.Ldc_I4:
                case OpCodeREnum.Ldc_I4_S:
                case OpCodeREnum.Ldc_I8:
                case OpCodeREnum.Ldc_R4:
                case OpCodeREnum.Ldc_R8:
                case OpCodeREnum.Ldstr:
                case OpCodeREnum.Callvirt:
                case OpCodeREnum.Call:
                case OpCodeREnum.Newobj:
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
                case OpCodeREnum.Ldloca:
                case OpCodeREnum.Ldloca_S:
                case OpCodeREnum.Ldarg_S:
                case OpCodeREnum.Ldarga:
                case OpCodeREnum.Ldarga_S:
                case OpCodeREnum.Ldlen:
                case OpCodeREnum.Newarr:
                case OpCodeREnum.Ldfld:
                case OpCodeREnum.Ldflda:
                case OpCodeREnum.Ldtoken:
                case OpCodeREnum.Isinst:
                case OpCodeREnum.Ldsfld:
                case OpCodeREnum.Ldsflda:
                case OpCodeREnum.Ldftn:
                case OpCodeREnum.Ldvirtftn:
                case OpCodeREnum.Ldelem_I1:
                case OpCodeREnum.Ldelem_U1:
                case OpCodeREnum.Ldelem_I2:
                case OpCodeREnum.Ldelem_U2:
                case OpCodeREnum.Ldelem_I4:
                case OpCodeREnum.Ldelem_U4:
                case OpCodeREnum.Ldelem_I8:
                case OpCodeREnum.Ldelem_R4:
                case OpCodeREnum.Ldelem_R8:
                case OpCodeREnum.Ldelem_Any:
                case OpCodeREnum.Ldelem_Ref:
                case OpCodeREnum.Ldelema:
                    r1 = op.Register1;
                    return true;
                case OpCodeREnum.Br_S:
                case OpCodeREnum.Br:
                case OpCodeREnum.Brtrue:
                case OpCodeREnum.Brtrue_S:
                case OpCodeREnum.Brfalse:
                case OpCodeREnum.Brfalse_S:
                case OpCodeREnum.Switch:
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
                case OpCodeREnum.Nop:
                case OpCodeREnum.Constrained:
                case OpCodeREnum.Ret:
                case OpCodeREnum.Push:
                case OpCodeREnum.Initobj:
                case OpCodeREnum.InlineStart:
                case OpCodeREnum.InlineEnd:
                case OpCodeREnum.Stind_I:
                case OpCodeREnum.Stind_I1:
                case OpCodeREnum.Stind_I2:
                case OpCodeREnum.Stind_I4:
                case OpCodeREnum.Stind_I8:
                case OpCodeREnum.Stind_R4:
                case OpCodeREnum.Stind_R8:
                case OpCodeREnum.Stind_Ref:
                case OpCodeREnum.Stobj:
                case OpCodeREnum.Stelem_I:
                case OpCodeREnum.Stelem_I1:
                case OpCodeREnum.Stelem_I2:
                case OpCodeREnum.Stelem_I4:
                case OpCodeREnum.Stelem_I8:
                case OpCodeREnum.Stelem_R4:
                case OpCodeREnum.Stelem_R8:
                case OpCodeREnum.Stelem_Ref:
                case OpCodeREnum.Stelem_Any:
                case OpCodeREnum.Stfld:
                case OpCodeREnum.Stsfld:
                case OpCodeREnum.Throw:
                case OpCodeREnum.Castclass:
                case OpCodeREnum.Readonly:
                case OpCodeREnum.Leave:
                case OpCodeREnum.Leave_S:
                case OpCodeREnum.Endfinally:
                case OpCodeREnum.Volatile:
                case OpCodeREnum.Rethrow:
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
                    return false;
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
                case OpCodeREnum.Ceqi:
                case OpCodeREnum.Cgti:
                case OpCodeREnum.Cgti_Un:
                case OpCodeREnum.Clti:
                case OpCodeREnum.Clti_Un:
                    r1 = op.Register1;
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }

        static void ReplaceOpcodeSource(ref OpCodes.OpCodeR op, int idx, short src)
        {
            switch (op.Code)
            {
                case OpCodes.OpCodeREnum.Move:
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
                case OpCodeREnum.Box:
                case OpCodeREnum.Unbox:
                case OpCodeREnum.Unbox_Any:
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
                case OpCodeREnum.Ldloca:
                case OpCodeREnum.Ldloca_S:
                case OpCodeREnum.Ldarg_S:
                case OpCodeREnum.Ldarga:
                case OpCodeREnum.Ldarga_S:
                case OpCodeREnum.Ldlen:
                case OpCodeREnum.Newarr:
                case OpCodeREnum.Ldfld:
                case OpCodeREnum.Ldflda:
                case OpCodeREnum.Ldvirtftn:
                case OpCodeREnum.Isinst:
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
                case OpCodeREnum.Ceqi:
                case OpCodeREnum.Cgti:
                case OpCodeREnum.Cgti_Un:
                case OpCodeREnum.Clti:
                case OpCodeREnum.Clti_Un:
                    op.Register2 = src;
                    break;

                case OpCodeREnum.Stind_I:
                case OpCodeREnum.Stind_I1:
                case OpCodeREnum.Stind_I2:
                case OpCodeREnum.Stind_I4:
                case OpCodeREnum.Stind_I8:
                case OpCodeREnum.Stind_R4:
                case OpCodeREnum.Stind_R8:
                case OpCodeREnum.Stind_Ref:
                case OpCodeREnum.Stobj:
                case OpCodeREnum.Stfld:
                    switch (idx)
                    {
                        case 0:
                            op.Register1 = src;
                            break;
                        case 1:
                            op.Register2 = src;
                            break;
                        default:
                            throw new NotSupportedException();
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
                case OpCodeREnum.Ldelem_I1:
                case OpCodeREnum.Ldelem_U1:
                case OpCodeREnum.Ldelem_I2:
                case OpCodeREnum.Ldelem_U2:
                case OpCodeREnum.Ldelem_I4:
                case OpCodeREnum.Ldelem_U4:
                case OpCodeREnum.Ldelem_I8:
                case OpCodeREnum.Ldelem_R4:
                case OpCodeREnum.Ldelem_R8:
                case OpCodeREnum.Ldelem_Any:
                case OpCodeREnum.Ldelem_Ref:
                case OpCodeREnum.Ldelema:
                    switch (idx)
                    {
                        case 0:
                            op.Register2 = src;
                            break;
                        case 1:
                            op.Register3 = src;
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    break;
                case OpCodeREnum.Stelem_I:
                case OpCodeREnum.Stelem_I1:
                case OpCodeREnum.Stelem_I2:
                case OpCodeREnum.Stelem_I4:
                case OpCodeREnum.Stelem_I8:
                case OpCodeREnum.Stelem_R4:
                case OpCodeREnum.Stelem_R8:
                case OpCodeREnum.Stelem_Ref:
                case OpCodeREnum.Stelem_Any:
                    switch (idx)
                    {
                        case 0:
                            op.Register1 = src;
                            break;
                        case 1:
                            op.Register2 = src;
                            break;
                        case 2:
                            op.Register3 = src;
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    break;
                case OpCodeREnum.Call:
                case OpCodeREnum.Callvirt:
                case OpCodeREnum.Newobj:

                    switch (idx)
                    {
                        case 0:
                            op.Register2 = src;
                            break;
                        case 1:
                            op.Register3 = src;
                            break;
                        case 2:
                            op.Register4 = src;
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    break;
                case OpCodeREnum.Brtrue:
                case OpCodeREnum.Brtrue_S:
                case OpCodeREnum.Brfalse:
                case OpCodeREnum.Brfalse_S:
                case OpCodeREnum.Switch:
                case OpCodeREnum.Push:
                case OpCodeREnum.Throw:
                case OpCodeREnum.Stsfld:
                case OpCodeREnum.Initobj:
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
                    op.Register1 = src;
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
                    switch (idx)
                    {
                        case 0:
                            op.Register1 = src;
                            break;
                        case 1:
                            op.Register2 = src;
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    break;
                case OpCodeREnum.Ret:
                    op.Register1 = src;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        static void ReplaceOpcodeDest(ref OpCodes.OpCodeR op, short dst)
        {
            switch (op.Code)
            {
                case OpCodes.OpCodeREnum.Move:
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
                case OpCodeREnum.Ldc_I4:
                case OpCodeREnum.Ldc_I4_S:
                case OpCodeREnum.Ldc_I8:
                case OpCodeREnum.Ldc_R4:
                case OpCodeREnum.Ldc_R8:
                case OpCodeREnum.Ldstr:
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
                case OpCodeREnum.Box:
                case OpCodeREnum.Unbox:
                case OpCodeREnum.Unbox_Any:
                case OpCodeREnum.Call:
                case OpCodeREnum.Callvirt:
                case OpCodeREnum.Newobj:
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
                case OpCodeREnum.Ldloca:
                case OpCodeREnum.Ldloca_S:
                case OpCodeREnum.Ldarg_S:
                case OpCodeREnum.Ldarga:
                case OpCodeREnum.Ldarga_S:
                case OpCodeREnum.Ldlen:
                case OpCodeREnum.Newarr:
                case OpCodeREnum.Ldfld:
                case OpCodeREnum.Ldflda:
                case OpCodeREnum.Ldsfld:
                case OpCodeREnum.Ldsflda:
                case OpCodeREnum.Ldtoken:
                case OpCodeREnum.Ldftn:
                case OpCodeREnum.Ldvirtftn:
                case OpCodeREnum.Isinst:
                case OpCodeREnum.Ldelem_I1:
                case OpCodeREnum.Ldelem_U1:
                case OpCodeREnum.Ldelem_I2:
                case OpCodeREnum.Ldelem_U2:
                case OpCodeREnum.Ldelem_I4:
                case OpCodeREnum.Ldelem_U4:
                case OpCodeREnum.Ldelem_I8:
                case OpCodeREnum.Ldelem_R4:
                case OpCodeREnum.Ldelem_R8:
                case OpCodeREnum.Ldelem_Any:
                case OpCodeREnum.Ldelem_Ref:
                case OpCodeREnum.Ldelema:
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
                case OpCodeREnum.Ceqi:
                case OpCodeREnum.Cgti:
                case OpCodeREnum.Cgti_Un:
                case OpCodeREnum.Clti:
                case OpCodeREnum.Clti_Un:
                    op.Register1 = dst;
                    break;
                case OpCodeREnum.Br_S:
                case OpCodeREnum.Br:
                case OpCodeREnum.Brtrue:
                case OpCodeREnum.Brtrue_S:
                case OpCodeREnum.Brfalse:
                case OpCodeREnum.Brfalse_S:
                case OpCodeREnum.Switch:
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
                case OpCodeREnum.Nop:
                case OpCodeREnum.Ret:
                case OpCodeREnum.Push:
                case OpCodeREnum.Constrained:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
