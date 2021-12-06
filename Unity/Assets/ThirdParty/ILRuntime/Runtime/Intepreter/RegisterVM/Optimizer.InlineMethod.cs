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
        public const int MaximalInlineInstructionCount = 20;
        public static void InlineMethod(CodeBasicBlock block, ILMethod method, RegisterVMSymbolLink symbolLink, ref Dictionary<int, int[]> jumpTables, short baseRegIdx, bool hasReturn)
        {
            var ins = block.FinalInstructions;
            var body = method.BodyRegister;
            OpCodeR start = new OpCodeR();
            start.Code = OpCodeREnum.InlineStart;
            ins.Add(start);
            int branchStart = ins.Count;
            int branchOffset = 0;
            List<int> reloc = new List<int>();
            if (body != null)
            {
                for (int i = 0; i < body.Length; i++)
                {
                    var opcode = body[i];
                    short r1 = 0;
                    short r2 = 0;
                    short r3 = 0;
                    if (GetOpcodeSourceRegister(ref opcode, hasReturn, out r1, out r2, out r3))
                    {
                        if (r1 >= 0)
                        {
                            ReplaceOpcodeSource(ref opcode, 0, (short)(r1 + baseRegIdx));
                        }
                        if (r2 >= 0)
                        {
                            ReplaceOpcodeSource(ref opcode, 1, (short)(r2 + baseRegIdx));
                        }
                        if (r3 >= 0)
                        {
                            ReplaceOpcodeSource(ref opcode, 2, (short)(r3 + baseRegIdx));
                        }
                    }
                    if (GetOpcodeDestRegister(ref opcode, out r1))
                    {
                        if (r1 >= 0)
                            ReplaceOpcodeDest(ref opcode, (short)(r1 + baseRegIdx));
                    }

                    if (opcode.Code == OpCodeREnum.Ret)
                    {
                        bool needMove = hasReturn && opcode.Register1 != baseRegIdx;
                        if (needMove)
                        {
                            opcode.Code = OpCodeREnum.Move;
                            opcode.Register2 = opcode.Register1;
                            opcode.Register1 = baseRegIdx;
                            ins.Add(opcode);
                            branchOffset++;
                        }
                        if (i < body.Length - 1)
                        {
                            if (needMove)
                            {
                                for (int j = branchStart; j < ins.Count; j++)
                                {
                                    var op2 = ins[j];
                                    if (IsBranching(op2.Code))
                                    {
                                        if (op2.Operand > i)
                                        {
                                            op2.Operand++;
                                            ins[j] = op2;
                                        }
                                    }
                                    else if (IsIntermediateBranching(op2.Code))
                                    {
                                        if (op2.Operand4 > i)
                                        {
                                            op2.Operand4++;
                                            ins[j] = op2;
                                        }
                                    }
                                    else if(op2.Code == OpCodeREnum.Switch)
                                    {
                                        var targets = jumpTables[op2.Operand];
                                        for(int k = 0; k < targets.Length; k++)
                                        {
                                            if (targets[k] > i)
                                                targets[k]++;
                                        }
                                    }
                                }
                            }
                            reloc.Add(ins.Count);
                            opcode.Code = OpCodeREnum.Br;
                            ins.Add(opcode);
                        }
                        continue;
                    }

                    if (IsBranching(opcode.Code))
                    {
                        opcode.Operand += branchOffset;
                    }
                    if (opcode.Code == OpCodeREnum.Switch)
                    {
                        int[] targets = method.JumpTablesRegister[opcode.Operand];
                        int[] newTargets = new int[targets.Length];
                        for (int j = 0; j < targets.Length; j++)
                        {
                            newTargets[j] = targets[j] + branchOffset;
                        }
                        if (jumpTables == null)
                            jumpTables = new Dictionary<int, int[]>();
                        opcode.Operand = newTargets.GetHashCode();
                        jumpTables.Add(opcode.Operand, newTargets);
                    }
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                    RegisterVMSymbol oriIns;
                    if (method.RegisterVMSymbols.TryGetValue(i, out oriIns))
                    {
                        oriIns.ParentSymbol = symbolLink;
                        block.InstructionMapping.Add(ins.Count, oriIns);
                    }
#endif
                    ins.Add(opcode);
                }
            }

            foreach (var i in reloc)
            {
                var opcode = ins[i];
                opcode.Operand = ins.Count - branchStart;
                ins[i] = opcode;
            }
            start.Code = OpCodeREnum.InlineEnd;
            ins.Add(start);
        }
    }
}
