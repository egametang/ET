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
    class RegisterVMSymbolLink
    {
        public int BaseRegisterIndex;
        public RegisterVMSymbol Value;
    }

    struct InlineMethodInfo
    {
        public short LocalStartRegister;
        public ILMethod Method;
    }
    struct RegisterVMSymbol
    {
        public Instruction Instruction;
        public ILMethod Method;
        public RegisterVMSymbolLink ParentSymbol;
    }
    class CodeBasicBlock
    {
        List<Instruction> instructions = new List<Instruction>();
        List<OpCodeR> finalInstructions = new List<OpCodeR>();
        HashSet<int> canRemove = new HashSet<int>();
        HashSet<int> pendingCP = new HashSet<int>();
        HashSet<CodeBasicBlock> prevBlocks = new HashSet<CodeBasicBlock>();
        HashSet<CodeBasicBlock> nextBlocks = new HashSet<CodeBasicBlock>();
        Dictionary<int, RegisterVMSymbol> instructionMapping = new Dictionary<int, RegisterVMSymbol>();
        short endRegister = -1;
        Instruction entry;
        public List<Instruction> Instructions { get { return instructions; } }

        public List<OpCodeR> FinalInstructions { get { return finalInstructions; } }

        public HashSet<int> CanRemove { get { return canRemove; } }

        public HashSet<int> PendingCP { get { return pendingCP; } }

        public HashSet<CodeBasicBlock> PreviousBlocks { get { return prevBlocks; } }

        public HashSet<CodeBasicBlock> NextBlocks { get { return nextBlocks; } }

        public Dictionary<int, RegisterVMSymbol> InstructionMapping { get { return instructionMapping; } }

        public bool NeedLoadConstantElimination { get; set; }
        public short EndRegister
        {
            get
            { return endRegister; }
            set
            {
                endRegister = value;
            }
        }

        public void AddInstruction(Instruction op)
        {
            if (instructions.Count == 0)
                entry = op;
            instructions.Add(op);
        }

        public static List<CodeBasicBlock> BuildBasicBlocks(MethodBody body, out Dictionary<Instruction, int> entryMapping)
        {
            entryMapping = new Dictionary<Instruction, int>();
            HashSet<Instruction> branchTargets = new HashSet<Instruction>();
            foreach (var i in body.Instructions)
            {
                switch (i.OpCode.OperandType)
                {
                    case OperandType.InlineBrTarget:
                    case OperandType.ShortInlineBrTarget:
                        branchTargets.Add((Instruction)i.Operand);
                        break;
                    case OperandType.InlineSwitch:
                        {
                            var arr = i.Operand as Instruction[];
                            foreach (var j in arr)
                                branchTargets.Add(j);
                        }
                        break;
                }
            }

            List<CodeBasicBlock> res = new List<CodeBasicBlock>();
            CodeBasicBlock cur = new CodeBasicBlock();
            res.Add(cur);
            foreach (var i in body.Instructions)
            {
                if (branchTargets.Contains(i))
                {
                    if(cur.entry != null && cur.entry != i)
                    {
                        entryMapping[cur.entry] = res.Count - 1;
                        cur = new CodeBasicBlock();
                        res.Add(cur);
                    }
                }
                cur.AddInstruction(i);
                if (i.OpCode.Code == Code.Switch || i.OpCode.Code == Code.Throw || i.OpCode.OperandType == OperandType.InlineBrTarget || i.OpCode.OperandType == OperandType.ShortInlineBrTarget)
                {
                    if (cur.entry != null)
                    {
                        if (i.OpCode.OperandType == OperandType.InlineBrTarget || i.OpCode.OperandType == OperandType.ShortInlineBrTarget)
                        {
                            if (cur.entry != (Instruction)i.Operand)
                            {
                                entryMapping[cur.entry] = res.Count - 1;
                                cur = new CodeBasicBlock();
                                res.Add(cur);
                            }
                        }
                        else
                        {
                            if (i.Operand is Instruction)
                            {
                                if (cur.entry != (Instruction)i.Operand)
                                {
                                    entryMapping[cur.entry] = res.Count - 1;
                                    cur = new CodeBasicBlock();
                                    res.Add(cur);
                                }
                            }
                            else
                            {
                                entryMapping[cur.entry] = res.Count - 1;
                                cur = new CodeBasicBlock();
                                res.Add(cur);
                            }
                        }
                    }
                }
            }
            if (cur.entry != null)
                entryMapping[cur.entry] = res.Count - 1;
            else
                res.RemoveAt(res.Count - 1);

            for (int i = 0; i < res.Count; i++)
            {
                var block = res[i];
                var lastIns = block.instructions[block.instructions.Count - 1];
                switch (lastIns.OpCode.OperandType)
                {
                    case OperandType.ShortInlineBrTarget:
                    case OperandType.InlineBrTarget:
                        {
                            var dstBlock = res[entryMapping[(Instruction)lastIns.Operand]];
                            dstBlock.prevBlocks.Add(block);
                            block.nextBlocks.Add(dstBlock);
                            switch (lastIns.OpCode.Code)
                            {
                                case Code.Brfalse:
                                case Code.Brfalse_S:
                                case Code.Brtrue:
                                case Code.Brtrue_S:
                                case Code.Beq:
                                case Code.Beq_S:
                                case Code.Bge:
                                case Code.Bge_S:
                                case Code.Bge_Un:
                                case Code.Bge_Un_S:
                                case Code.Bgt:
                                case Code.Bgt_S:
                                case Code.Bgt_Un:
                                case Code.Bgt_Un_S:
                                case Code.Ble:
                                case Code.Ble_S:
                                case Code.Ble_Un:
                                case Code.Ble_Un_S:
                                case Code.Blt:
                                case Code.Blt_S:
                                case Code.Blt_Un:
                                case Code.Blt_Un_S:
                                case Code.Bne_Un:
                                case Code.Bne_Un_S:
                                    if (i < res.Count - 1)
                                    {
                                        var next = res[i + 1];
                                        block.nextBlocks.Add(next);
                                        next.prevBlocks.Add(block);
                                    }
                                    break;
                                default:
                                    continue;
                            }
                        }
                        break;
                    case OperandType.InlineSwitch:
                        {
                            Instruction[] targets = (Instruction[])lastIns.Operand;
                            foreach(var t in targets)
                            {
                                var dstBlock = res[entryMapping[t]];
                                dstBlock.prevBlocks.Add(block);
                                block.nextBlocks.Add(dstBlock);
                            }
                        }
                        break;
                }
                if (i < res.Count - 1)
                {
                    var next = res[i + 1];
                    block.nextBlocks.Add(next);
                    next.prevBlocks.Add(block);
                }
            }
            return res;
        }
    }
}
