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
        public static void EliminateConstantLoad(List<CodeBasicBlock> blocks, bool hasReturn)
        {
            foreach (var b in blocks)
            {
                if (!b.NeedLoadConstantElimination)
                    continue;
                var lst = b.FinalInstructions;
                HashSet<int> canRemove = b.CanRemove;
                //HashSet<int> pendingBCP = b.PendingCP;
                bool isInline = false;
                for (int i = 0; i < lst.Count; i++)
                {
                    OpCodeR X = lst[i];
                    if (X.Code == OpCodeREnum.InlineStart)
                    {
                        isInline = true;
                        continue;
                    }
                    if (X.Code == OpCodeREnum.InlineEnd)
                    {
                        isInline = false;
                        continue;
                    }
                    if (isInline)
                        continue;
                    if (IsLoadConstant(X.Code))
                    {
                        short xDst;
                        GetOpcodeDestRegister(ref X, out xDst);

                        bool propagationInline = false;
                        for (int j = i + 1; j < lst.Count; j++)
                        {
                            OpCodeR Y = lst[j];
                            if (Y.Code == OpCodeREnum.InlineStart)
                                propagationInline = true;
                            else if (Y.Code == OpCodeREnum.InlineEnd)
                            {
                                propagationInline = false;
                            }
                            short r1, r2, r3;
                            GetOpcodeSourceRegister(ref Y, hasReturn, out r1, out r2, out r3);
                            if (r1 == xDst || r2 == xDst || r3 == xDst)
                            {
                                if (SupportIntemediateValue(Y.Code))
                                {
                                    if (r2 == xDst)
                                    {
                                        if (!propagationInline)
                                        {
                                            Y.Code = GetIntemediateValueOpcode(Y.Code);
                                            ReplaceRegisterWithConstant(ref Y, ref X);
                                            lst[j] = Y;
                                            canRemove.Add(i);
                                        }
                                    }
                                    else if (r1 == xDst)
                                    {
                                        if (!propagationInline)
                                        {
                                            if (SupportOperandSwap(Y.Code))
                                            {
                                                ReplaceOpcodeSource(ref Y, 0, r2);
                                                Y.Code = GetIntemediateValueOpcode(Y.Code);
                                                ReplaceRegisterWithConstant(ref Y, ref X);
                                                lst[j] = Y;
                                                canRemove.Add(i);
                                            }
                                            else if (HasInverseOpcode(Y.Code))
                                            {
                                                ReplaceOpcodeSource(ref Y, 0, r2);
                                                Y.Code = GetIntemediateValueOpcode(GetInverseOpcode(Y.Code));
                                                ReplaceRegisterWithConstant(ref Y, ref X);
                                                lst[j] = Y;
                                                canRemove.Add(i);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                            short yDst;
                            GetOpcodeDestRegister(ref Y, out yDst);
                            if (yDst == xDst)
                                break;
                        }
                    }
                }
            }
        }
    }
}
