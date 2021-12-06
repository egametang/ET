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
        public static int CleanupRegister(List<OpCodeR> ins, int locRegStart, bool hasReturn)
        {
            short maxRegNum = (short)locRegStart;
            HashSet<short> usedRegisters = new HashSet<short>();
            //arguments can not be cleaned
            for (short i = 0; i < locRegStart; i++)
                usedRegisters.Add(i);
            for (int i = 0; i < ins.Count; i++)
            {
                var X = ins[i];
                short xSrc, xSrc2, xSrc3, xDst;
                if(GetOpcodeSourceRegister(ref X, hasReturn, out xSrc, out xSrc2, out xSrc3))
                {
                    if (xSrc >= 0)
                    {
                        if (xSrc > maxRegNum)
                            maxRegNum = xSrc;
                        usedRegisters.Add(xSrc);
                    }
                    if (xSrc2 >= 0)
                    {
                        if (xSrc2 > maxRegNum)
                            maxRegNum = xSrc2;
                        usedRegisters.Add(xSrc2);
                    }
                    if (xSrc3 >= 0)
                    {
                        if (xSrc3 > maxRegNum)
                            maxRegNum = xSrc3;
                        usedRegisters.Add(xSrc3);
                    }
                }

                if(GetOpcodeDestRegister(ref X, out xDst))
                {
                    if (xDst >= 0)
                    {
                        if (xDst > maxRegNum)
                            maxRegNum = xDst;
                        usedRegisters.Add(xDst);
                    }
                }
            }

            List<short> unusedRegisters = new List<short>();
            for(short i = 0; i <= maxRegNum; i++)
            {
                if (!usedRegisters.Contains(i))
                    unusedRegisters.Add(i);
            }

            for(short i = 0; i < unusedRegisters.Count; i++)
            {
                short r = (short)(unusedRegisters[i] - i);
                for (int j = 0; j < ins.Count; j++)
                {
                    var X = ins[j];
                    short xSrc, xSrc2, xSrc3, xDst;
                    bool replaced = false;

                    if (GetOpcodeSourceRegister(ref X, hasReturn, out xSrc, out xSrc2, out xSrc3))
                    {
                        if (xSrc > r)
                        {
                            ReplaceOpcodeSource(ref X, 0, (short)(xSrc - 1));
                            replaced = true;
                        }
                        if (xSrc2 > r)
                        {
                            ReplaceOpcodeSource(ref X, 1, (short)(xSrc2 - 1));
                            replaced = true;
                        }
                        if (xSrc3 > r)
                        {
                            ReplaceOpcodeSource(ref X, 2, (short)(xSrc3 - 1));
                            replaced = true;
                        }
                    }

                    if (GetOpcodeDestRegister(ref X, out xDst))
                    {
                        if (xDst > r)
                        {
                            ReplaceOpcodeDest(ref X, (short)(xDst - 1));
                            replaced = true;
                        }
                    }

                    if (replaced)
                        ins[j] = X;
                }
            }

            return maxRegNum - unusedRegisters.Count + 1;
        }
    }
}
