using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.Mono.Cecil;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Stack;
using ILRuntime.Runtime.Intepreter.OpCodes;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Intepreter
{
    unsafe struct RegisterFrameInfo
    {
        public ILIntepreter Intepreter;
        public int FrameManagedBase;
        public int LocalManagedBase;
        public StackObject* StackBase;
        public StackObject* RegisterStart;
        public StackObject* RegisterEnd;
        public IList<object> ManagedStack;
    }
    public unsafe partial class ILIntepreter
    {
        /*void InitializeRegisterLocal(StackObject* loc, IType t, IList<object> mStack)
        {
            bool isEnum = false;
            isEnum = t.IsEnum;
            if (!t.IsByRef && t.IsValueType && !t.IsPrimitive && !isEnum)
            {
                if (t is ILType)
                {
                    stack.AllocValueType(loc, t);
                }
                else
                {
                    CLRType cT = (CLRType)t;
                    if (cT.ValueTypeBinder != null)
                    {
                        stack.AllocValueType(loc, t);
                    }
                    else
                    {
                        var obj = ((CLRType)t).CreateDefaultInstance();
                        loc->ObjectType = ObjectTypes.Object;
                        loc->Value = locBase + i;
                        mStack[locBase + i] = obj;
                    }
                }
            }
            else
            {
                if (t.IsPrimitive || isEnum)
                {
                    StackObject.Initialized(loc, t);
                }
                else
                {
                    loc->ObjectType = ObjectTypes.Object;
                    loc->Value = locBase + i;
                }
            }
        }*/

        internal StackObject* ExecuteR(ILMethod method, StackObject* esp, out bool unhandledException)
        {
            
#if DEBUG
            if (method == null)
                throw new NullReferenceException();
#endif
#if DEBUG && !NO_PROFILER
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == AppDomain.UnityMainThreadID)

#if UNITY_5_5_OR_NEWER
                UnityEngine.Profiling.Profiler.BeginSample(method.ToString());
#else
                UnityEngine.Profiler.BeginSample(method.ToString());
#endif

#endif
            OpCodeR[] body = method.BodyRegister;
            StackFrame frame;
            stack.InitializeFrame(method, esp, out frame);
            frame.IsRegister = true;
            int finallyEndAddress = 0;
            Exception lastCaughtEx = null;

            var stackRegStart = frame.LocalVarPointer;
            StackObject* r = frame.LocalVarPointer - method.ParameterCount;
            IList<object> mStack = stack.ManagedStack;
            int paramCnt = method.ParameterCount;
            if (method.HasThis)//this parameter is always object reference
            {
                r--;
                paramCnt++;
                /// 为确保性能，暂时先确保开发的时候，安全检查完备。
                /// 当然手机端运行时可能会出现为空的类对象可正常调用成员函数，导致成员函数里面访问成员变量报错时可能使得根据Log跟踪BUG时方向错误。
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                if (!method.DeclearingType.IsValueType)
                {
                    var thisObj = RetriveObject(r, mStack);
                    if (thisObj == null)
                        throw new NullReferenceException();
                }
#endif
            }
            unhandledException = false;
            var hasReturn = method.ReturnType != AppDomain.VoidType;

            //Managed Stack reserved for arguments(In case of starg)
            for (int i = 0; i < paramCnt; i++)
            {
                var a = (r + i);
                switch (a->ObjectType)
                {
                    /*case ObjectTypes.Null:
                        //Need to reserve place for null, in case of starg
                        a->ObjectType = ObjectTypes.Object;
                        a->Value = mStack.Count;
                        mStack.Add(null);
                        break;*/
                    case ObjectTypes.ValueTypeObjectReference:
                        //CloneStackValueType(a, a, mStack);
                        break;
                    case ObjectTypes.Object:
                    case ObjectTypes.FieldReference:
                    case ObjectTypes.ArrayReference:
                        {
                            if (i > 0 || !method.HasThis)//this instance should not be cloned
                                mStack[a->Value] = CheckAndCloneValueType(mStack[a->Value], AppDomain);
                        }
                        break;
                }
            }
            frame.ManagedStackBase -= paramCnt;
            stack.PushFrame(ref frame);

            int locBase = mStack.Count;
            int locCnt = method.LocalVariableCount;
            int stackRegCnt = method.StackRegisterCount;
            RegisterFrameInfo info;
            info.Intepreter = this;
            info.StackBase = stack.StackBase;
            info.LocalManagedBase = locBase;
            info.FrameManagedBase = frame.ManagedStackBase;
            info.RegisterStart = r;
            info.ManagedStack = mStack;

            object obj;

            /*for (int i = 0; i < locCnt; i++)
            {
                InitializeRegisterLocal(method, i, v1, locBase, mStack);
            }*/
            esp = stackRegStart + stackRegCnt + locCnt;

            info.RegisterEnd = esp;

            for (int i = 0; i < stackRegCnt + locCnt; i++)
            {
                var loc = stackRegStart + i;
                loc->ObjectType = ObjectTypes.Object;
                loc->Value = mStack.Count;                
                mStack.Add(null);
            }
            var bp = stack.ValueTypeStackPointer;
            ValueTypeBasePointer = bp;
            var ehs = method.ExceptionHandlerRegister;

            StackObject* reg1, reg2, reg3, objRef, objRef2, val, dst, ret;
            bool transfer;
            int intVal = 0;
            long longVal = 0;
            float floatVal = 0;
            double doubleVal = 0;
            IType type;
            Type clrType;

            fixed (OpCodeR* ptr = body)
            {
                OpCodeR* ip = ptr;
                OpCodeREnum code = ip->Code;
                bool returned = false;
                while (!returned)
                {
                    try
                    {
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                        if (ShouldBreak)
                            Break();
                        var insOffset = (int)(ip - ptr);
                        frame.Address.Value = insOffset;
                        AppDomain.DebugService.CheckShouldBreak(method, this, insOffset);
#endif
                        code = ip->Code;

                        switch (code)
                        {
                            #region Arguments and Local Variable
                            /*case OpCodeREnum.Ldarg:
                            case OpCodeREnum.Ldarg_S:
                                {
                                    reg1 = (r + ip->Register2);
                                    CopyToRegister(ref info, ip->Register1, reg1);
                                }
                                break;*/
                            case OpCodeREnum.Ldarga:
                            case OpCodeREnum.Ldarga_S:
                                reg1 = (r + ip->Register2);
                                reg2 = (r + ip->Register1);

                                reg2->ObjectType = ObjectTypes.StackObjectReference;
                                *(long*)&reg2->Value = (long)reg1;
                                break;
                            #endregion
                            #region Load Constants
                            case OpCodeREnum.Ldc_I4_M1:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = -1;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_0:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 0;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_1:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 1;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_2:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 2;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_3:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 3;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_4:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 4;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_5:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 5;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_6:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 6;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_7:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 7;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_8:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 8;
                                }
                                break;

                            case OpCodeREnum.Ldc_I4:
                            case OpCodeREnum.Ldc_I4_S:
                                reg1 = (r + ip->Register1);
                                reg1->ObjectType = ObjectTypes.Integer;
                                reg1->Value = ip->Operand;
                                break;
                            case OpCodeREnum.Ldc_R4:
                                {
                                    reg1 = (r + ip->Register1);
                                    *(float*)(&reg1->Value) = ip->OperandFloat;
                                    reg1->ObjectType = ObjectTypes.Float;
                                }
                                break;
                            case OpCodeREnum.Ldc_I8:
                                {
                                    reg1 = (r + ip->Register1);
                                    *(long*)(&reg1->Value) = ip->OperandLong;
                                    reg1->ObjectType = ObjectTypes.Long;
                                }
                                break;
                            case OpCodeREnum.Ldc_R8:
                                {
                                    reg1 = (r + ip->Register1);
                                    *(double*)(&reg1->Value) = ip->OperandDouble;
                                    reg1->ObjectType = ObjectTypes.Double;
                                }
                                break;
                            case OpCodeREnum.Ldstr:
                                AssignToRegister(ref info, ip->Register1, AppDomain.GetString(ip->OperandLong));
                                break;
                            case OpCodeREnum.Ldnull:
                                //reg1 = (r + ip->Register1);
                                AssignToRegister(ref info, ip->Register1, null, true);
                                //WriteNull(reg1);
                                break;
                            #endregion

                            #region Althemetics
                            case OpCodeREnum.Add:
                                {
                                    reg1 = (r + ip->Register2); 
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) + *((long*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value + reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            reg3->ObjectType = ObjectTypes.Float;
                                            *((float*)&reg3->Value) = *((float*)&reg1->Value) + *((float*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            reg3->ObjectType = ObjectTypes.Double;
                                            *((double*)&reg3->Value) = *((double*)&reg1->Value) + *((double*)&reg2->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Addi:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) + ip->OperandLong;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value + ip->Operand;
                                            break;
                                        case ObjectTypes.Float:
                                            reg3->ObjectType = ObjectTypes.Float;
                                            *((float*)&reg3->Value) = *((float*)&reg1->Value) + ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            reg3->ObjectType = ObjectTypes.Double;
                                            *((double*)&reg3->Value) = *((double*)&reg1->Value) + ip->OperandDouble;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Sub:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) - *((long*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value - reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            reg3->ObjectType = ObjectTypes.Float;
                                            *((float*)&reg3->Value) = *((float*)&reg1->Value) - *((float*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            reg3->ObjectType = ObjectTypes.Double;
                                            *((double*)&reg3->Value) = *((double*)&reg1->Value) - *((double*)&reg2->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;

                            case OpCodeREnum.Subi:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) - ip->OperandLong;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value - ip->Operand;
                                            break;
                                        case ObjectTypes.Float:
                                            reg3->ObjectType = ObjectTypes.Float;
                                            *((float*)&reg3->Value) = *((float*)&reg1->Value) - ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            reg3->ObjectType = ObjectTypes.Double;
                                            *((double*)&reg3->Value) = *((double*)&reg1->Value) - ip->OperandDouble;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Mul:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) * *((long*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value * reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            reg3->ObjectType = ObjectTypes.Float;
                                            *((float*)&reg3->Value) = *((float*)&reg1->Value) * *((float*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            reg3->ObjectType = ObjectTypes.Double;
                                            *((double*)&reg3->Value) = *((double*)&reg1->Value) * *((double*)&reg2->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;

                            case OpCodeREnum.Muli:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) * ip->OperandLong;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value * ip->Operand;
                                            break;
                                        case ObjectTypes.Float:
                                            reg3->ObjectType = ObjectTypes.Float;
                                            *((float*)&reg3->Value) = *((float*)&reg1->Value) * ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            reg3->ObjectType = ObjectTypes.Double;
                                            *((double*)&reg3->Value) = *((double*)&reg1->Value) * ip->OperandDouble;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Div:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) / *((long*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value / reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            reg3->ObjectType = ObjectTypes.Float;
                                            *((float*)&reg3->Value) = *((float*)&reg1->Value) / *((float*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            reg3->ObjectType = ObjectTypes.Double;
                                            *((double*)&reg3->Value) = *((double*)&reg1->Value) / *((double*)&reg2->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;

                            case OpCodeREnum.Divi:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) / ip->OperandLong;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value / ip->Operand;
                                            break;
                                        case ObjectTypes.Float:
                                            reg3->ObjectType = ObjectTypes.Float;
                                            *((float*)&reg3->Value) = *((float*)&reg1->Value) / ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            reg3->ObjectType = ObjectTypes.Double;
                                            *((double*)&reg3->Value) = *((double*)&reg1->Value) / ip->OperandDouble;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Div_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((ulong*)&reg3->Value) = *((ulong*)&reg1->Value) / *((ulong*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = (int)((uint)reg1->Value / (uint)reg2->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Divi_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((ulong*)&reg3->Value) = *((ulong*)&reg1->Value) / (ulong)ip->OperandLong;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = (int)((uint)reg1->Value / (uint)ip->Operand);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Rem:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) % *((long*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value % reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            reg3->ObjectType = ObjectTypes.Float;
                                            *((float*)&reg3->Value) = *((float*)&reg1->Value) % *((float*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            reg3->ObjectType = ObjectTypes.Double;
                                            *((double*)&reg3->Value) = *((double*)&reg1->Value) % *((double*)&reg2->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Remi:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) % ip->OperandLong;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value % ip->Operand;
                                            break;
                                        case ObjectTypes.Float:
                                            reg3->ObjectType = ObjectTypes.Float;
                                            *((float*)&reg3->Value) = *((float*)&reg1->Value) % ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            reg3->ObjectType = ObjectTypes.Double;
                                            *((double*)&reg3->Value) = *((double*)&reg1->Value) % ip->OperandDouble;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Rem_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((ulong*)&reg3->Value) = *((ulong*)&reg1->Value) % *((ulong*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = (int)((uint)reg1->Value % (uint)reg2->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Remi_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((ulong*)&reg3->Value) = *((ulong*)&reg1->Value) % (ulong)ip->OperandLong;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = (int)((uint)reg1->Value % (uint)ip->Operand);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Xor:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) ^ *((long*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value ^ reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Xori:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) ^ ip->OperandLong;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value ^ ip->Operand;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.And:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) & *((long*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value & reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Andi:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) & ip->OperandLong;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value & ip->Operand;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Or:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) | *((long*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value | reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Ori:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) | ip->OperandLong;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value | ip->Operand;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Shl:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) << reg2->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value << reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Shli:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) << ip->Operand;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value << ip->Operand;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Shr:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) >> reg2->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value >> reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;

                            case OpCodeREnum.Shri:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) >> ip->Operand;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value >> ip->Operand;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Shr_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((ulong*)&reg3->Value) = *((ulong*)&reg1->Value) >> reg2->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            *((uint*)&reg3->Value) = (uint)reg1->Value >> reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Shri_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((ulong*)&reg3->Value) = *((ulong*)&reg1->Value) >> ip->Operand;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            *((uint*)&reg3->Value) = (uint)reg1->Value >> ip->Operand;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Not:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = ~*((long*)&reg1->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = ~reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Neg:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = -*((long*)&reg1->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = -reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            reg3->ObjectType = ObjectTypes.Float;
                                            *((float*)&reg3->Value) = -*((float*)&reg1->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            reg3->ObjectType = ObjectTypes.Double;
                                            *((double*)&reg3->Value) = -*((double*)&reg1->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            #endregion

                            #region Conversion
                            case OpCodeREnum.Conv_U1:
                            case OpCodeREnum.Conv_Ovf_U1:
                            case OpCodeREnum.Conv_Ovf_U1_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                        case ObjectTypes.Integer:
                                            intVal = (byte)reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            intVal = (byte)*(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            intVal = (byte)*(double*)&reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Integer;
                                    reg2->Value = intVal;
                                    reg2->ValueLow = 0;
                                }
                                break;
                            case OpCodeREnum.Conv_I1:
                            case OpCodeREnum.Conv_Ovf_I1:
                            case OpCodeREnum.Conv_Ovf_I1_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                        case ObjectTypes.Integer:
                                            intVal = (sbyte)reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            intVal = (sbyte)*(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            intVal = (sbyte)*(double*)&reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Integer;
                                    reg2->Value = intVal;
                                    reg2->ValueLow = 0;
                                }
                                break;
                            case OpCodeREnum.Conv_U2:
                            case OpCodeREnum.Conv_Ovf_U2:
                            case OpCodeREnum.Conv_Ovf_U2_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                        case ObjectTypes.Integer:
                                            intVal = (ushort)reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            intVal = (ushort)*(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            intVal = (ushort)*(double*)&reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Integer;
                                    reg2->Value = intVal;
                                    reg2->ValueLow = 0;
                                }
                                break;
                            case OpCodeREnum.Conv_I2:
                            case OpCodeREnum.Conv_Ovf_I2:
                            case OpCodeREnum.Conv_Ovf_I2_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                        case ObjectTypes.Integer:
                                            intVal = (short)(reg1->Value);
                                            break;
                                        case ObjectTypes.Float:
                                            intVal = (short)*(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            intVal = (short)*(double*)&reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Integer;
                                    reg2->Value = intVal;
                                    reg2->ValueLow = 0;
                                }
                                break;
                            case OpCodeREnum.Conv_U4:
                            case OpCodeREnum.Conv_U:
                            case OpCodeREnum.Conv_Ovf_U4:
                            case OpCodeREnum.Conv_Ovf_U4_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register1); 
                                    uint uintVal;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            uintVal = (uint)*(long*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            uintVal = (uint)reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            uintVal = (uint)*(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            uintVal = (uint)*(double*)&reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Integer;
                                    reg2->Value = (int)uintVal;
                                    reg2->ValueLow = 0;
                                }
                                break;
                            case OpCodeREnum.Conv_I4:
                            case OpCodeREnum.Conv_I:
                            case OpCodeREnum.Conv_Ovf_I:
                            case OpCodeREnum.Conv_Ovf_I_Un:
                            case OpCodeREnum.Conv_Ovf_I4:
                            case OpCodeREnum.Conv_Ovf_I4_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            intVal = (int)*(long*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            intVal = (int)*(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            intVal = (int)*(double*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            intVal = reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Integer;
                                    reg2->Value = intVal;
                                }
                                break;
                            case OpCodeREnum.Conv_I8:
                            case OpCodeREnum.Conv_Ovf_I8:
                            case OpCodeREnum.Conv_Ovf_I8_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            longVal = reg1->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            longVal = *(long*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            longVal = (long)*(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            longVal = (long)*(double*)&reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Long;
                                    *(long*)(&reg2->Value) = longVal;
                                }
                                break;
                            case OpCodeREnum.Conv_U8:
                            case OpCodeREnum.Conv_Ovf_U8:
                            case OpCodeREnum.Conv_Ovf_U8_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register1);
                                    ulong ulongVal;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            ulongVal = (uint)reg1->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            ulongVal = (ulong)*(long*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            ulongVal = (ulong)*(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            ulongVal = (ulong)*(double*)&reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Long;
                                    *(ulong*)(&reg2->Value) = ulongVal;
                                }
                                break;
                            case OpCodeREnum.Conv_R4:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            floatVal = (float)*(long*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            floatVal = *(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            floatVal = (float)*(double*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            floatVal = reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Float;
                                    *(float*)&reg2->Value = floatVal;
                                }
                                break;
                            case OpCodeREnum.Conv_R8:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            doubleVal = (double)*(long*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            doubleVal = *(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            doubleVal = reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            doubleVal = *(double*)&reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Double;
                                    *(double*)&reg2->Value = doubleVal;
                                }
                                break;
                            case OpCodeREnum.Conv_R_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register1);
                                    bool isDouble = false;
                                    double val2 = 0;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            doubleVal = (double)*(ulong*)&reg1->Value;
                                            isDouble = true;
                                            break;
                                        case ObjectTypes.Float:
                                            floatVal = *(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            floatVal = (uint)reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            doubleVal = *(double*)&reg1->Value;
                                            isDouble = true;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (isDouble)
                                    {
                                        reg2->ObjectType = ObjectTypes.Double;
                                        *(double*)&reg2->Value = doubleVal;
                                    }
                                    else
                                    {
                                        reg2->ObjectType = ObjectTypes.Float;
                                        *(float*)&reg2->Value = floatVal;
                                    }
                                }
                                break;
                            #endregion

                            #region Load Store
                            case OpCodeREnum.Move:
                                {
                                    reg1 = (r + ip->Register2);
                                    CopyToRegister(ref info, ip->Register1, reg1);
                                }
                                break;

                            case OpCodeREnum.Push:
                                {
                                    reg1 = (r + ip->Register1);
                                    CopyToStack(esp, reg1, mStack);
                                    if (ip->Operand == 1)
                                        mStack.Add(null);
                                    esp++;
                                }
                                break;
                            case OpCodeREnum.Ldloca:
                            case OpCodeREnum.Ldloca_S:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register1);

                                    reg2->ObjectType = ObjectTypes.StackObjectReference;
                                    *(long*)&reg2->Value = (long)reg1;
                                }
                                break;
                            case OpCodeREnum.Ldobj:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var t = AppDomain.GetType(ip->Operand);
                                                obj = mStack[reg1->Value];
                                                var idx = reg1->ValueLow;
                                                intVal = GetManagedStackIndex(ref info, ip->Register1);
                                                LoadFromArrayReference(obj, idx, reg2, t, mStack, intVal);
                                            }
                                            break;
                                        case ObjectTypes.StackObjectReference:
                                            {
                                                CopyToRegister(ref info, ip->Register1, GetObjectAndResolveReference(reg1));
                                            }
                                            break;
                                        case ObjectTypes.FieldReference:
                                            {
                                                obj = mStack[reg1->Value];
                                                int idx = reg1->ValueLow;
                                                if (obj is ILTypeInstance)
                                                {
                                                    ((ILTypeInstance)obj).CopyToRegister(idx, ref info, ip->Register1);
                                                }
                                                else
                                                {
                                                    type = AppDomain.GetType(ip->Operand);
                                                    if (!((CLRType)type).CopyFieldToStack(idx, obj, this, ref esp, mStack))
                                                    {
                                                        obj = ((CLRType)type).GetFieldValue(idx, obj);
                                                        AssignToRegister(ref info, ip->Register1, obj);
                                                    }
                                                    else
                                                    {
                                                        esp = PopToRegister(ref info, ip->Register1, esp);
                                                    }
                                                }
                                            }
                                            break;
                                        case ObjectTypes.StaticFieldReference:
                                            {
                                                type = AppDomain.GetType(reg1->Value);
                                                int idx = reg1->ValueLow;
                                                if (type is ILType)
                                                {
                                                    ((ILType)type).StaticInstance.CopyToRegister(idx, ref info, ip->Register1);
                                                }
                                                else
                                                {
                                                    if (!((CLRType)type).CopyFieldToStack(idx, null, this, ref esp, mStack))
                                                    {
                                                        obj = ((CLRType)type).GetFieldValue(idx, null);
                                                        AssignToRegister(ref info, ip->Register1, obj);
                                                    }
                                                    else
                                                    {
                                                        esp = PopToRegister(ref info, ip->Register1, esp);
                                                    }
                                                }
                                            }
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Stobj:
                                {
                                    val = (r + ip->Register2);
                                    objRef = (r + ip->Register1);
                                    switch (objRef->ObjectType)
                                    {
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var t = AppDomain.GetType(ip->Operand);
                                                StoreValueToArrayReference(objRef, val, t, mStack);
                                            }
                                            break;
                                        case ObjectTypes.StackObjectReference:
                                            {
                                                objRef = GetObjectAndResolveReference(objRef);
                                                if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                                {
                                                    switch (val->ObjectType)
                                                    {
                                                        case ObjectTypes.Object:
                                                            dst = ILIntepreter.ResolveReference(objRef);
                                                            CopyValueTypeToStack(dst, mStack[val->Value], mStack);
                                                            break;
                                                        case ObjectTypes.ValueTypeObjectReference:
                                                            CopyStackValueType(val, objRef, mStack);
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else
                                                {
                                                    if (val->ObjectType >= ObjectTypes.Object)
                                                    {
                                                        mStack[objRef->Value] = mStack[val->Value];
                                                        objRef->ValueLow = val->ValueLow;
                                                    }
                                                    else
                                                    {
                                                        *objRef = *val;
                                                    }
                                                }
                                            }
                                            break;
                                        case ObjectTypes.FieldReference:
                                            {
                                                obj = mStack[objRef->Value];
                                                int idx = objRef->ValueLow;
                                                if (obj is ILTypeInstance)
                                                {
                                                    ((ILTypeInstance)obj).AssignFromStack(idx, val, AppDomain, mStack);
                                                }
                                                else
                                                {
                                                    var t = AppDomain.GetType(ip->Operand);
                                                    if (!((CLRType)t).AssignFieldFromStack(idx, ref obj, this, val, mStack))
                                                        ((CLRType)t).SetFieldValue(idx, ref obj, t.TypeForCLR.CheckCLRTypes(StackObject.ToObject(val, AppDomain, mStack)));
                                                }
                                            }
                                            break;
                                        case ObjectTypes.StaticFieldReference:
                                            {
                                                var t = AppDomain.GetType(objRef->Value);
                                                if (t is ILType)
                                                {
                                                    ((ILType)t).StaticInstance.AssignFromStack(objRef->ValueLow, val, AppDomain, mStack);
                                                }
                                                else
                                                {
                                                    obj = null;
                                                    if (!((CLRType)t).AssignFieldFromStack(objRef->ValueLow, ref obj, this, val, mStack))
                                                        ((CLRType)t).SetStaticFieldValue(objRef->ValueLow, t.TypeForCLR.CheckCLRTypes(StackObject.ToObject(val, AppDomain, mStack)));
                                                }
                                            }
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldind_I:
                            case OpCodeREnum.Ldind_I1:
                            case OpCodeREnum.Ldind_I2:
                            case OpCodeREnum.Ldind_I4:
                            case OpCodeREnum.Ldind_U1:
                            case OpCodeREnum.Ldind_U2:
                            case OpCodeREnum.Ldind_U4:
                                {
                                    reg1 = (r + ip->Register2);
                                    dst = (r + ip->Register1);
                                    val = GetObjectAndResolveReference(reg1);
                                    switch (val->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                //Free(dst);
                                                LoadFromFieldReferenceToRegister(ref info, instance, idx, ip->Register1);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                //Free(dst);
                                                intVal = GetManagedStackIndex(ref info, ip->Register1);
                                                LoadFromArrayReference(instance, idx, dst, instance.GetType().GetElementType(), mStack, intVal);
                                            }
                                            break;
                                        case ObjectTypes.StaticFieldReference:
                                            {
                                                type = AppDomain.GetType(val->Value);
                                                var idx = val->ValueLow;
                                                if (type is ILType)
                                                {
                                                    ((ILType)type).StaticInstance.CopyToRegister(idx, ref info, ip->Register1);
                                                }
                                                else
                                                {
                                                    if (!((CLRType)type).CopyFieldToStack(idx, null, this, ref esp, mStack))
                                                    {
                                                        var ft = ((CLRType)type).GetField(idx);
                                                        obj = ((CLRType)type).GetFieldValue(idx, null);
                                                        if (obj is CrossBindingAdaptorType)
                                                            obj = ((CrossBindingAdaptorType)obj).ILInstance;
                                                        AssignToRegister(ref info, ip->Register1, obj, false);
                                                    }
                                                    else
                                                    {
                                                        esp = PopToRegister(ref info, ip->Register1, esp);
                                                    }
                                                }
                                            }
                                            break;
                                        default:
                                            {
                                                dst->ObjectType = ObjectTypes.Integer;
                                                dst->Value = val->Value;
                                                dst->ValueLow = 0;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldind_I8:
                                {
                                    reg1 = (r + ip->Register2);
                                    dst = (r + ip->Register1);
                                    val = GetObjectAndResolveReference(reg1);
                                    switch (val->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                //Free(dst);
                                                LoadFromFieldReferenceToRegister(ref info, instance, idx, ip->Register1);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                //Free(dst);
                                                intVal = GetManagedStackIndex(ref info, ip->Register1);
                                                LoadFromArrayReference(instance, idx, dst, instance.GetType().GetElementType(), mStack, intVal);
                                            }
                                            break;
                                        case ObjectTypes.StaticFieldReference:
                                            {
                                                type = AppDomain.GetType(val->Value);
                                                var idx = val->ValueLow;
                                                if (type is ILType)
                                                {
                                                    ((ILType)type).StaticInstance.CopyToRegister(idx, ref info, ip->Register1);
                                                }
                                                else
                                                {
                                                    if (!((CLRType)type).CopyFieldToStack(idx, null, this, ref esp, mStack))
                                                    {
                                                        var ft = ((CLRType)type).GetField(idx);
                                                        obj = ((CLRType)type).GetFieldValue(idx, null);
                                                        if (obj is CrossBindingAdaptorType)
                                                            obj = ((CrossBindingAdaptorType)obj).ILInstance;
                                                        AssignToRegister(ref info, ip->Register1, obj, false);
                                                    }
                                                    else
                                                    {
                                                        esp = PopToRegister(ref info, ip->Register1, esp);
                                                    }
                                                }
                                            }
                                            break;
                                        default:
                                            {
                                                *dst = *val;
                                                dst->ObjectType = ObjectTypes.Long;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldind_R4:
                                {
                                    reg1 = (r + ip->Register2);
                                    dst = (r + ip->Register1);
                                    val = GetObjectAndResolveReference(reg1);
                                    switch (val->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                //Free(dst);
                                                LoadFromFieldReferenceToRegister(ref info, instance, idx, ip->Register1);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                //Free(dst);
                                                intVal = GetManagedStackIndex(ref info, ip->Register1);
                                                LoadFromArrayReference(instance, idx, dst, instance.GetType().GetElementType(), mStack);
                                            }
                                            break;
                                        case ObjectTypes.StaticFieldReference:
                                            {
                                                type = AppDomain.GetType(val->Value);
                                                var idx = val->ValueLow;
                                                if (type is ILType)
                                                {
                                                    ((ILType)type).StaticInstance.CopyToRegister(idx, ref info, ip->Register1);
                                                }
                                                else
                                                {
                                                    if (!((CLRType)type).CopyFieldToStack(idx, null, this, ref esp, mStack))
                                                    {
                                                        var ft = ((CLRType)type).GetField(idx);
                                                        obj = ((CLRType)type).GetFieldValue(idx, null);
                                                        if (obj is CrossBindingAdaptorType)
                                                            obj = ((CrossBindingAdaptorType)obj).ILInstance;
                                                        AssignToRegister(ref info, ip->Register1, obj, false);
                                                    }
                                                    else
                                                    {
                                                        esp = PopToRegister(ref info, ip->Register1, esp);
                                                    }
                                                }
                                            }
                                            break;
                                        default:
                                            {
                                                dst->ObjectType = ObjectTypes.Float;
                                                dst->Value = val->Value;
                                                dst->ValueLow = 0;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldind_R8:
                                {
                                    reg1 = (r + ip->Register2);
                                    dst = (r + ip->Register1);
                                    val = GetObjectAndResolveReference(reg1);
                                    switch (val->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                //Free(dst);
                                                LoadFromFieldReferenceToRegister(ref info, instance, idx, ip->Register1);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                //Free(dst);
                                                intVal = GetManagedStackIndex(ref info, ip->Register1);
                                                LoadFromArrayReference(instance, idx, dst, instance.GetType().GetElementType(), mStack, intVal);
                                            }
                                            break;
                                        case ObjectTypes.StaticFieldReference:
                                            {
                                                type = AppDomain.GetType(val->Value);
                                                var idx = val->ValueLow;
                                                if (type is ILType)
                                                {
                                                    ((ILType)type).StaticInstance.CopyToRegister(idx, ref info, ip->Register1);
                                                }
                                                else
                                                {
                                                    if (!((CLRType)type).CopyFieldToStack(idx, null, this, ref esp, mStack))
                                                    {
                                                        var ft = ((CLRType)type).GetField(idx);
                                                        obj = ((CLRType)type).GetFieldValue(idx, null);
                                                        if (obj is CrossBindingAdaptorType)
                                                            obj = ((CrossBindingAdaptorType)obj).ILInstance;
                                                        AssignToRegister(ref info, ip->Register1, obj, false);
                                                    }
                                                    else
                                                    {
                                                        esp = PopToRegister(ref info, ip->Register1, esp);
                                                    }
                                                }
                                            }
                                            break;
                                        default:
                                            {
                                                *dst = *val;
                                                dst->ObjectType = ObjectTypes.Double;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldind_Ref:
                                {
                                    reg1 = (r + ip->Register2);
                                    dst = (r + ip->Register1);
                                    val = GetObjectAndResolveReference(reg1);
                                    switch (val->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            obj = mStack[val->Value];
                                            intVal = val->ValueLow;
                                            //Free(dst);
                                            LoadFromFieldReferenceToRegister(ref info, obj, intVal, ip->Register1);
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                obj = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                //Free(dst);
                                                intVal = GetManagedStackIndex(ref info, ip->Register1);
                                                LoadFromArrayReference(obj, idx, dst, obj.GetType().GetElementType(), mStack, intVal);
                                            }
                                            break;
                                        case ObjectTypes.StaticFieldReference:
                                            {
                                                type = AppDomain.GetType(val->Value);
                                                var idx = val->ValueLow;
                                                if (type is ILType)
                                                {
                                                    ((ILType)type).StaticInstance.CopyToRegister(idx, ref info, ip->Register1);
                                                }
                                                else
                                                {
                                                    if (!((CLRType)type).CopyFieldToStack(idx, null, this, ref esp, mStack))
                                                    {
                                                        var ft = ((CLRType)type).GetField(idx);
                                                        obj = ((CLRType)type).GetFieldValue(idx, null);
                                                        if (obj is CrossBindingAdaptorType)
                                                            obj = ((CrossBindingAdaptorType)obj).ILInstance;
                                                        AssignToRegister(ref info, ip->Register1, obj, false);
                                                    }
                                                    else
                                                    {
                                                        esp = PopToRegister(ref info, ip->Register1, esp);
                                                    }
                                                }
                                            }
                                            break;
                                        default:
                                            CopyToRegister(ref info, ip->Register1, val);
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stind_I:
                            case OpCodeREnum.Stind_I1:
                            case OpCodeREnum.Stind_I2:
                            case OpCodeREnum.Stind_I4:
                            case OpCodeREnum.Stind_R4:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register1);
                                    dst = GetObjectAndResolveReference(reg2);
                                    switch (dst->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                obj = mStack[dst->Value];
                                                StoreValueToFieldReference(ref obj, dst->ValueLow, reg1, mStack);
                                                mStack[dst->Value] = obj;
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                StoreValueToArrayReference(dst, reg1, mStack[dst->Value].GetType().GetElementType(), mStack);
                                            }
                                            break;
                                        case ObjectTypes.StaticFieldReference:
                                            {
                                                type = AppDomain.GetType(dst->Value);
                                                int idx = dst->ValueLow;
                                                if (type != null)
                                                {
                                                    if (type is ILType)
                                                    {
                                                        ILType t = type as ILType;
                                                        t.StaticInstance.AssignFromStack(idx, reg1, AppDomain, mStack);
                                                    }
                                                    else
                                                    {
                                                        CLRType t = type as CLRType;
                                                        var f = t.GetField(idx);
                                                        obj = null;
                                                        if (!((CLRType)t).AssignFieldFromStack(idx, ref obj, this, reg1, mStack))
                                                            t.SetStaticFieldValue(idx, f.FieldType.CheckCLRTypes(CheckAndCloneValueType(StackObject.ToObject(reg1, domain, mStack), domain)));
                                                    }
                                                }
                                                else
                                                    throw new TypeLoadException();
                                            }
                                            break;
                                        default:
                                            {
                                                dst->Value = reg1->Value;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stind_I8:
                                {
                                    val = (r + ip->Register2);
                                    reg2 = (r + ip->Register1);
                                    dst = GetObjectAndResolveReference(reg2);
                                    switch (dst->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                obj = mStack[dst->Value];
                                                StoreValueToFieldReference(ref obj, dst->ValueLow, val, mStack);
                                                mStack[dst->Value] = obj;
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                StoreValueToArrayReference(dst, val, typeof(long), mStack);
                                            }
                                            break;
                                        case ObjectTypes.StaticFieldReference:
                                            {
                                                type = AppDomain.GetType(dst->Value);
                                                int idx = dst->ValueLow;
                                                if (type != null)
                                                {
                                                    if (type is ILType)
                                                    {
                                                        ILType t = type as ILType;
                                                        t.StaticInstance.AssignFromStack(idx, val, AppDomain, mStack);
                                                    }
                                                    else
                                                    {
                                                        CLRType t = type as CLRType;
                                                        var f = t.GetField(idx);
                                                        obj = null;
                                                        if (!((CLRType)t).AssignFieldFromStack(idx, ref obj, this, val, mStack))
                                                            t.SetStaticFieldValue(idx, f.FieldType.CheckCLRTypes(CheckAndCloneValueType(StackObject.ToObject(val, domain, mStack), domain)));
                                                    }
                                                }
                                                else
                                                    throw new TypeLoadException();
                                            }
                                            break;
                                        default:
                                            {
                                                dst->Value = val->Value;
                                                dst->ValueLow = val->ValueLow;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stind_R8:
                                {
                                    val = (r + ip->Register2);
                                    reg2 = (r + ip->Register1);
                                    dst = GetObjectAndResolveReference(reg2);
                                    switch (dst->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                obj = mStack[dst->Value];
                                                StoreValueToFieldReference(ref obj, dst->ValueLow, val, mStack);
                                                mStack[dst->Value] = obj;
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                StoreValueToArrayReference(dst, val, typeof(double), mStack);
                                            }
                                            break;
                                        case ObjectTypes.StaticFieldReference:
                                            {
                                                type = AppDomain.GetType(dst->Value);
                                                int idx = dst->ValueLow;
                                                if (type != null)
                                                {
                                                    if (type is ILType)
                                                    {
                                                        ILType t = type as ILType;
                                                        t.StaticInstance.AssignFromStack(idx, val, AppDomain, mStack);
                                                    }
                                                    else
                                                    {
                                                        CLRType t = type as CLRType;
                                                        var f = t.GetField(idx);
                                                        obj = null;
                                                        if (!((CLRType)t).AssignFieldFromStack(idx, ref obj, this, val, mStack))
                                                            t.SetStaticFieldValue(idx, f.FieldType.CheckCLRTypes(CheckAndCloneValueType(StackObject.ToObject(val, domain, mStack), domain)));
                                                    }
                                                }
                                                else
                                                    throw new TypeLoadException();
                                            }
                                            break;
                                        default:
                                            {
                                                dst->Value = val->Value;
                                                dst->ValueLow = val->ValueLow;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stind_Ref:
                                {
                                    val = (r + ip->Register2);
                                    reg2 = (r + ip->Register1);
                                    dst = GetObjectAndResolveReference(reg2);
                                    switch (dst->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                obj = mStack[dst->Value];
                                                StoreValueToFieldReference(ref obj, dst->ValueLow, val, mStack);
                                                mStack[dst->Value] = obj;
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                StoreValueToArrayReference(dst, val, typeof(object), mStack);
                                            }
                                            break;
                                        case ObjectTypes.StaticFieldReference:
                                            {
                                                type = AppDomain.GetType(dst->Value);
                                                int idx = dst->ValueLow;
                                                if (type != null)
                                                {
                                                    if (type is ILType)
                                                    {
                                                        ILType t = type as ILType;
                                                        t.StaticInstance.AssignFromStack(idx, val, AppDomain, mStack);
                                                    }
                                                    else
                                                    {
                                                        CLRType t = type as CLRType;
                                                        var f = t.GetField(idx);
                                                        obj = null;
                                                        if (!((CLRType)t).AssignFieldFromStack(idx, ref obj, this, val, mStack))
                                                            t.SetStaticFieldValue(idx, f.FieldType.CheckCLRTypes(CheckAndCloneValueType(StackObject.ToObject(val, domain, mStack), domain)));
                                                    }
                                                }
                                                else
                                                    throw new TypeLoadException();
                                            }
                                            break;
                                        default:
                                            {
                                                switch (val->ObjectType)
                                                {
                                                    case ObjectTypes.Object:
                                                        mStack[dst->Value] = mStack[val->Value];
                                                        break;
                                                    case ObjectTypes.Null:
                                                        mStack[dst->Value] = null;
                                                        break;
                                                    default:
                                                        throw new NotImplementedException();
                                                }
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldtoken:
                                {
                                    switch (ip->Operand)
                                    {
                                        case 0:
                                            {
                                                type = AppDomain.GetType((int)(ip->OperandLong >> 32));
                                                if (type != null)
                                                {
                                                    if (type is ILType)
                                                    {
                                                        ILType t = type as ILType;

                                                        t.StaticInstance.CopyToRegister((int)ip->OperandLong, ref info, ip->Register1);
                                                    }
                                                    else
                                                        throw new NotImplementedException();
                                                }
                                            }
                                            break;
                                        case 1:
                                            {
                                                type = AppDomain.GetType((int)ip->OperandLong);
                                                if (type != null)
                                                {
                                                    AssignToRegister(ref info, ip->Register1, type.ReflectionType);
                                                }
                                                else
                                                    throw new TypeLoadException();
                                            }
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            #endregion

                            #region Control Flow
                            case OpCodeREnum.Ret:
                                if (hasReturn)
                                {
                                    reg1 = (r + ip->Register1);
                                    CopyToStack(esp, reg1, mStack);
                                    esp++;
                                }
                                returned = true;
                                break;
                            case OpCodeREnum.Br_S:
                            case OpCodeREnum.Br:
                                ip = ptr + ip->Operand;
                                continue;
                            case OpCodeREnum.Brtrue:
                            case OpCodeREnum.Brtrue_S:
                                {
                                    reg1 = (r + ip->Register1);
                                    bool res = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = reg1->Value != 0;
                                            break;
                                        case ObjectTypes.Long:
                                            res = *(long*)&reg1->Value != 0;
                                            break;
                                        case ObjectTypes.Object:
                                            res = mStack[reg1->Value] != null;
                                            break;
                                    }
                                    if (res)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }
                                }
                                break;
                            case OpCodeREnum.Brfalse:
                            case OpCodeREnum.Brfalse_S:
                                {
                                    reg1 = (r + ip->Register1);
                                    bool res = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Null:
                                            res = true;
                                            break;
                                        case ObjectTypes.Integer:
                                            res = reg1->Value == 0;
                                            break;
                                        case ObjectTypes.Long:
                                            res = *(long*)&reg1->Value == 0;
                                            break;
                                        case ObjectTypes.Object:
                                            res = mStack[reg1->Value] == null;
                                            break;
                                    }
                                    if (res)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }
                                }
                                break;
                            case OpCodeREnum.Beq:
                            case OpCodeREnum.Beq_S:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    transfer = false;
                                    if (reg1->ObjectType == reg2->ObjectType)
                                    {
                                        switch (reg1->ObjectType)
                                        {
                                            case ObjectTypes.Null:
                                                transfer = true;
                                                break;
                                            case ObjectTypes.Integer:
                                                transfer = reg1->Value == reg2->Value;
                                                break;
                                            case ObjectTypes.Long:
                                                transfer = *(long*)&reg1->Value == *(long*)&reg2->Value;
                                                break;
                                            case ObjectTypes.Float:
                                                transfer = *(float*)&reg1->Value == *(float*)&reg2->Value;
                                                break;
                                            case ObjectTypes.Double:
                                                transfer = *(double*)&reg1->Value == *(double*)&reg2->Value;
                                                break;
                                            case ObjectTypes.Object:
                                                transfer = mStack[reg1->Value] == mStack[reg2->Value];
                                                break;
                                            default:
                                                throw new NotImplementedException();
                                        }
                                    }
                                    else if (reg1->ObjectType == ObjectTypes.Null || reg2->ObjectType == ObjectTypes.Null)
                                    {
                                        if (reg1->ObjectType == ObjectTypes.Null && reg2->ObjectType == ObjectTypes.Object)
                                            transfer = mStack[reg2->Value] == null;
                                        else if (reg1->ObjectType == ObjectTypes.Object && reg2->ObjectType == ObjectTypes.Null)
                                            transfer = mStack[reg1->Value] == null;
                                    }
                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Beqi:
                                {
                                    reg1 = (r + ip->Register1);
                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Null:
                                            transfer = ip->Operand == 0;
                                            break;
                                        case ObjectTypes.Integer:
                                            transfer = reg1->Value == ip->Operand;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&reg1->Value == ip->OperandLong;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value == ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value == ip->OperandDouble;
                                            break;
                                        case ObjectTypes.Object:
                                            transfer = mStack[reg1->Value] == null && ip->Operand == 0;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand4;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Bne_Un:
                            case OpCodeREnum.Bne_Un_S:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    transfer = false;
                                    if (reg1->ObjectType == reg2->ObjectType)
                                    {
                                        switch (reg1->ObjectType)
                                        {
                                            case ObjectTypes.Null:
                                                transfer = false;
                                                break;
                                            case ObjectTypes.Integer:
                                                transfer = (uint)reg1->Value != (uint)reg2->Value;
                                                break;
                                            case ObjectTypes.Float:
                                                transfer = *(float*)&reg1->Value != *(float*)&reg2->Value;
                                                break;
                                            case ObjectTypes.Long:
                                                transfer = *(long*)&reg1->Value != *(long*)&reg2->Value;
                                                break;
                                            case ObjectTypes.Double:
                                                transfer = *(double*)&reg1->Value != *(double*)&reg2->Value;
                                                break;
                                            case ObjectTypes.Object:
                                                transfer = mStack[reg1->Value] != mStack[reg2->Value];
                                                break;
                                            default:
                                                throw new NotImplementedException();
                                        }
                                    }
                                    else if (reg1->ObjectType == ObjectTypes.Null || reg2->ObjectType == ObjectTypes.Null)
                                    {
                                        if (reg1->ObjectType == ObjectTypes.Null && reg2->ObjectType == ObjectTypes.Object)
                                            transfer = mStack[reg2->Value] != null;
                                        else if (reg1->ObjectType == ObjectTypes.Object && reg2->ObjectType == ObjectTypes.Null)
                                            transfer = mStack[reg1->Value] != null;
                                    }
                                    else
                                        transfer = true;
                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Bnei_Un:
                                {
                                    reg1 = (r + ip->Register1);
                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Null:
                                            transfer = ip->Operand != 0;
                                            break;
                                        case ObjectTypes.Integer:
                                            transfer = (uint)reg1->Value != (uint)ip->Operand;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value != ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&reg1->Value != ip->OperandLong;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value != ip->OperandDouble;
                                            break;
                                        case ObjectTypes.Object:
                                            transfer = mStack[reg1->Value] != null || ip->Operand != 0;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand4;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Blt:
                            case OpCodeREnum.Blt_S:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = reg1->Value < reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&reg1->Value < *(long*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value < *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value < *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Blti:
                                {
                                    reg1 = (r + ip->Register1);
                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = reg1->Value < ip->Operand;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&reg1->Value < ip->OperandLong;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value < ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value < ip->OperandDouble;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand4;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Blt_Un:
                            case OpCodeREnum.Blt_Un_S:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)reg1->Value < (uint)reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(ulong*)&reg1->Value < *(ulong*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value < *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value < *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Blti_Un:
                                {
                                    reg1 = (r + ip->Register1);
                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)reg1->Value < (uint)ip->Operand;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(ulong*)&reg1->Value < (ulong)ip->Operand;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value < ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value < ip->OperandDouble;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand4;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Ble:
                            case OpCodeREnum.Ble_S:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = reg1->Value <= reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&reg1->Value <= *(long*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value <= *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value <= *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Blei:
                                {
                                    reg1 = (r + ip->Register1);
                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = reg1->Value <= ip->Operand;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&reg1->Value <= ip->OperandLong;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value <= ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value <= ip->OperandDouble;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand4;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Ble_Un:
                            case OpCodeREnum.Ble_Un_S:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)reg1->Value <= (uint)reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(ulong*)&reg1->Value <= *(ulong*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value <= *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value <= *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Blei_Un:
                                {
                                    reg1 = (r + ip->Register1);
                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)reg1->Value <= (uint)ip->Operand;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(ulong*)&reg1->Value <= (ulong)ip->OperandLong;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value <= ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value <= ip->OperandDouble;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand4;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Bgt:
                            case OpCodeREnum.Bgt_S:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);

                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = reg1->Value > reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&reg1->Value > *(long*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value > *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value > *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Bgti:
                                {
                                    reg1 = (r + ip->Register1);

                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = reg1->Value > ip->Operand;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&reg1->Value > ip->OperandLong;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value > ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value > ip->OperandDouble;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand4;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Bgt_Un:
                            case OpCodeREnum.Bgt_Un_S:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)reg1->Value > (uint)reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(ulong*)&reg1->Value > *(ulong*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value > *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value > *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Bgti_Un:
                                {
                                    reg1 = (r + ip->Register1);
                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)reg1->Value > (uint)ip->Operand;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(ulong*)&reg1->Value > (ulong)ip->OperandLong;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value >ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value > ip->OperandDouble;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand4;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Bge:
                            case OpCodeREnum.Bge_S:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = reg1->Value >= reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&reg1->Value >= *(long*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value >= *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value >= *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Bgei:
                                {
                                    reg1 = (r + ip->Register1);
                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = reg1->Value >= ip->Operand;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&reg1->Value >= ip->OperandLong;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value >= ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value >= ip->OperandDouble;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand4;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Bge_Un:
                            case OpCodeREnum.Bge_Un_S:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)reg1->Value >= (uint)reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(ulong*)&reg1->Value >= *(ulong*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value >= *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value >= *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Bgei_Un:
                                {
                                    reg1 = (r + ip->Register1);
                                    transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)reg1->Value >= (uint)ip->Operand;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(ulong*)&reg1->Value >= (ulong)ip->OperandLong;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value >= ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value >= ip->OperandDouble;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand4;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Switch:
                                {
                                    intVal = (r + ip->Register1)->Value;
                                    var table = method.JumpTablesRegister[ip->Operand];
                                    if (intVal >= 0 && intVal < table.Length)
                                    {
                                        ip = ptr + table[intVal];
                                        continue;
                                    }
                                }
                                break;
                            case OpCodeREnum.Leave:
                            case OpCodeREnum.Leave_S:
                                {
                                    if (ehs != null)
                                    {
                                        ExceptionHandler eh = null;

                                        int addr = (int)(ip - ptr);
                                        var sql = from e in ehs
                                                  where addr >= e.TryStart && addr <= e.TryEnd && (ip->Operand < e.TryStart || ip->Operand > e.TryEnd) && e.HandlerType == ExceptionHandlerType.Finally
                                                  select e;
                                        eh = sql.FirstOrDefault();
                                        if (eh != null)
                                        {
                                            finallyEndAddress = ip->Operand;
                                            ip = ptr + eh.HandlerStart;
                                            continue;
                                        }
                                    }
                                    ip = ptr + ip->Operand;
                                    continue;
                                }

                            case OpCodeREnum.Endfinally:
                                {
                                    if (finallyEndAddress < 0)
                                    {
                                        unhandledException = true;
                                        finallyEndAddress = 0;
                                        throw lastCaughtEx;
                                    }
                                    else
                                    {
                                        ip = ptr + finallyEndAddress;
                                        finallyEndAddress = 0;
                                        continue;
                                    }
                                }
                            case OpCodeREnum.Call:
                            case OpCodeREnum.Callvirt:
                                {
                                    IMethod m = domain.GetMethod(ip->Operand2);
                                    if (m == null)
                                    {
                                        //Irrelevant method
                                        int cnt = Math.Max(ip->Operand3 - RegisterVM.JITCompiler.CallRegisterParamCount, 0);
                                        //Balance the stack
                                        for (int i = 0; i < cnt; i++)
                                        {
                                            Free(esp - 1);
                                            esp--;
                                        }
                                    }
                                    else
                                    {
                                        bool isILMethod = m is ILMethod;
                                        bool useRegister = isILMethod && ((ILMethod)m).ShouldUseRegisterVM;
                                        if (ip->Operand4 == 0)
                                        {
                                            intVal = m.HasThis ? m.ParameterCount + 1 : m.ParameterCount;
                                            intVal = intVal - Math.Max((intVal - RegisterVM.JITCompiler.CallRegisterParamCount), 0);
                                            for (int i = 0; i < intVal; i++)
                                            {
                                                switch (i)
                                                {
                                                    case 0:
                                                        reg1 = (r + ip->Register2);
                                                        break;
                                                    case 1:
                                                        reg1 = (r + ip->Register3);
                                                        break;
                                                    case 2:
                                                        reg1 = (r + ip->Register4);
                                                        break;
                                                    default:
                                                        throw new NotSupportedException();
                                                }
                                                CopyToStack(esp, reg1, mStack);
                                                if (useRegister && reg1->ObjectType < ObjectTypes.Object)
                                                {
                                                    mStack.Add(null);
                                                }
                                                esp++;
                                            }
                                        }
                                        if (isILMethod)
                                        {
                                            ILMethod ilm = (ILMethod)m;
                                            bool processed = false;
                                            if (m.IsDelegateInvoke)
                                            {
                                                var instance = StackObject.ToObject((esp - (m.ParameterCount + 1)), domain, mStack);
                                                if (instance is IDelegateAdapter)
                                                {
                                                    esp = ((IDelegateAdapter)instance).ILInvoke(this, esp, mStack);
                                                    processed = true;
                                                }
                                            }
                                            if (!processed)
                                            {
                                                if (code == OpCodeREnum.Callvirt)
                                                {
                                                    objRef = GetObjectAndResolveReference(esp - (ilm.ParameterCount + 1));
                                                    if (objRef->ObjectType == ObjectTypes.Null)
                                                        throw new NullReferenceException();
                                                    if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                                    {
                                                        dst = *(StackObject**)&objRef->Value;
                                                        var ft = domain.GetTypeByIndex(dst->Value) as ILType;
                                                        ilm = ft.GetVirtualMethod(ilm) as ILMethod;
                                                    }
                                                    else
                                                    {
                                                        obj = mStack[objRef->Value];
                                                        if (obj == null)
                                                            throw new NullReferenceException();
                                                        ilm = ((ILTypeInstance)obj).Type.GetVirtualMethod(ilm) as ILMethod;
                                                    }
                                                }
                                                if (useRegister)
                                                    esp = ExecuteR(ilm, esp, out unhandledException);
                                                else
                                                {
                                                    esp = Execute(ilm, esp, out unhandledException);
                                                }
                                                ValueTypeBasePointer = bp;
                                                if (unhandledException)
                                                    returned = true;
                                            }
                                        }
                                        else
                                        {
                                            CLRMethod cm = (CLRMethod)m;
                                            bool processed = false;
                                            if (cm.IsDelegateInvoke)
                                            {
                                                var instance = StackObject.ToObject((esp - (cm.ParameterCount + 1)), domain, mStack);
                                                if (instance is IDelegateAdapter)
                                                {
                                                    esp = ((IDelegateAdapter)instance).ILInvoke(this, esp, mStack);
                                                    processed = true;
                                                }
                                            }

                                            if (!processed)
                                            {
                                                var redirect = cm.Redirection;
                                                if (redirect != null)
                                                    esp = redirect(this, esp, mStack, cm, false);
                                                else
                                                {
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                                                    if (!allowUnboundCLRMethod)
                                                        throw new NotSupportedException(cm.ToString() + " is not bound!");
#endif
#if DEBUG && !NO_PROFILER
                                                    if (System.Threading.Thread.CurrentThread.ManagedThreadId == AppDomain.UnityMainThreadID)

#if UNITY_5_5_OR_NEWER
                                                        UnityEngine.Profiling.Profiler.BeginSample(cm.ToString());
#else
                                                        UnityEngine.Profiler.BeginSample(cm.ToString());
#endif
#endif
                                                    object result = cm.Invoke(this, esp, mStack);
#if DEBUG && !NO_PROFILER
                                                    if (System.Threading.Thread.CurrentThread.ManagedThreadId == AppDomain.UnityMainThreadID)
#if UNITY_5_5_OR_NEWER
                                                        UnityEngine.Profiling.Profiler.EndSample();
#else
                                                        UnityEngine.Profiler.EndSample();
#endif

#endif
                                                    if (result is CrossBindingAdaptorType)
                                                        result = ((CrossBindingAdaptorType)result).ILInstance;
                                                    int paramCount = cm.ParameterCount;
                                                    for (int i = 1; i <= paramCount; i++)
                                                    {
                                                        Free(esp - (i));
                                                    }
                                                    esp = esp - (paramCount);
                                                    if (cm.HasThis)
                                                    {
                                                        Free(esp - 1);
                                                        esp--;
                                                    }
                                                    if (cm.ReturnType != AppDomain.VoidType && !cm.IsConstructor)
                                                    {
                                                        esp = PushObject(esp, mStack, result, cm.ReturnType.TypeForCLR == typeof(object));
                                                    }
                                                }
                                            }
                                        }

                                        if (m.ReturnType != AppDomain.VoidType && !m.IsConstructor)
                                        {
                                            esp = PopToRegister(ref info, ip->Register1, esp);
                                        }
                                    }
                                }
                                break;
                            #endregion

                            #region FieldOperation
                            case OpCodeREnum.Stfld:
                                {
                                    reg2 = (r + ip->Register2);
                                    objRef = GetObjectAndResolveReference((r + ip->Register1));
                                    if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                    {
                                        dst = ILIntepreter.ResolveReference(objRef);
                                        var ft = domain.GetTypeByIndex(dst->Value);
                                        if (ft is ILType)
                                            CopyToValueTypeField(dst, (int)ip->OperandLong, reg2, mStack);
                                        else
                                            CopyToValueTypeField(dst, ((CLRType)ft).FieldIndexMapping[(int)ip->OperandLong], reg2, mStack);
                                    }
                                    else
                                    {
                                        obj = RetriveObject(objRef, mStack);

                                        if (obj != null)
                                        {
                                            if (obj is ILTypeInstance)
                                            {
                                                ILTypeInstance instance = obj as ILTypeInstance;
                                                instance.AssignFromStack((int)ip->OperandLong, reg2, AppDomain, mStack);
                                            }
                                            else
                                            {
                                                var t = obj.GetType();
                                                type = AppDomain.GetType((int)(ip->OperandLong >> 32));
                                                if (type != null)
                                                {
                                                    var fieldToken = (int)ip->OperandLong;
                                                    var f = ((CLRType)type).GetField(fieldToken);
                                                    CopyToStack(esp, reg2, mStack);
                                                    if (!((CLRType)type).AssignFieldFromStack(fieldToken, ref obj, this, esp, mStack))
                                                        ((CLRType)type).SetFieldValue(fieldToken, ref obj, f.FieldType.CheckCLRTypes(CheckAndCloneValueType(StackObject.ToObject(reg2, domain, mStack), domain)));
                                                    Free(esp);
                                                    //Writeback
                                                    if (t.IsValueType)
                                                    {
                                                        switch (objRef->ObjectType)
                                                        {
                                                            case ObjectTypes.Object:
                                                                mStack[objRef->Value] = obj;
                                                                break;
                                                            case ObjectTypes.FieldReference:
                                                                {
                                                                    var oldObj = mStack[objRef->Value];
                                                                    intVal = objRef->ValueLow;
                                                                    if (oldObj is ILTypeInstance)
                                                                    {
                                                                        ((ILTypeInstance)oldObj)[intVal] = obj;
                                                                    }
                                                                    else
                                                                    {
                                                                        var it = AppDomain.GetType(oldObj.GetType());
                                                                        ((CLRType)it).SetFieldValue(intVal, ref oldObj, obj);
                                                                    }
                                                                }
                                                                break;
                                                            case ObjectTypes.ArrayReference:
                                                                {
                                                                    var arr = mStack[objRef->Value] as Array;
                                                                    int idx = objRef->ValueLow;
                                                                    arr.SetValue(obj, idx);
                                                                }
                                                                break;
                                                            case ObjectTypes.StaticFieldReference:
                                                                {
                                                                    var it = AppDomain.GetType(objRef->Value);
                                                                    intVal = objRef->ValueLow;
                                                                    if (it is ILType)
                                                                    {
                                                                        ((ILType)it).StaticInstance[intVal] = obj;
                                                                    }
                                                                    else
                                                                    {
                                                                        ((CLRType)it).SetStaticFieldValue(intVal, obj);
                                                                    }
                                                                }
                                                                break;
                                                            case ObjectTypes.ValueTypeObjectReference:
                                                                {
                                                                    dst = ILIntepreter.ResolveReference(objRef);
                                                                    var ct = domain.GetTypeByIndex(dst->Value) as CLRType;
                                                                    var binder = ct.ValueTypeBinder;
                                                                    binder.CopyValueTypeToStack(obj, dst, mStack);
                                                                }
                                                                break;
                                                            default:
                                                                throw new NotImplementedException();
                                                        }
                                                    }
                                                }
                                                else
                                                    throw new TypeLoadException();
                                            }
                                        }
                                        else
                                            throw new NullReferenceException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldfld:
                                {
                                    reg2 = (r + ip->Register2);
                                    objRef = GetObjectAndResolveReference(reg2);
                                    if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                    {
                                        dst = *(StackObject**)&objRef->Value;
                                        var ft = domain.GetTypeByIndex(dst->Value);
                                        if (ft is ILType)
                                            val = dst - ((int)ip->OperandLong + 1);
                                        else
                                            val = dst - (((CLRType)ft).FieldIndexMapping[(int)ip->OperandLong] + 1);
                                        //TODO: Check master modification
                                        CopyToRegister(ref info, ip->Register1, val);
                                    }
                                    else
                                    {
                                        obj = RetriveObject(objRef, mStack);
                                        if (obj != null)
                                        {
                                            if (obj is ILTypeInstance)
                                            {
                                                ILTypeInstance instance = obj as ILTypeInstance;
                                                instance.CopyToRegister((int)ip->OperandLong, ref info, ip->Register1);//Check #345
                                            }
                                            else
                                            {
                                                //var t = obj.GetType();
                                                type = AppDomain.GetType((int)(ip->OperandLong >> 32));
                                                if (type != null)
                                                {
                                                    var token = (int)ip->OperandLong;
                                                    if (!((CLRType)type).CopyFieldToStack(token, obj, this, ref esp, mStack))
                                                    {
                                                        var ft = ((CLRType)type).GetField(token);
                                                        obj = ((CLRType)type).GetFieldValue(token, obj);
                                                        if (obj is CrossBindingAdaptorType)
                                                            obj = ((CrossBindingAdaptorType)obj).ILInstance;

                                                        AssignToRegister(ref info, ip->Register1, obj, ft.FieldType == typeof(object));
                                                    }
                                                    else
                                                    {
                                                        esp = PopToRegister(ref info, ip->Register1, esp);
                                                    }
                                                }
                                                else
                                                    throw new TypeLoadException();
                                            }
                                        }
                                        else
                                            throw new NullReferenceException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldflda:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    objRef = GetObjectAndResolveReference(reg2);
                                    if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                    {
                                        var ft = domain.GetType((int)(ip->OperandLong >> 32));
                                        StackObject* fieldAddr;
                                        if (ft is ILType)
                                        {
                                            fieldAddr = ILIntepreter.ResolveReference(objRef) - ((int)ip->OperandLong + 1);
                                        }
                                        else
                                        {
                                            fieldAddr = ILIntepreter.ResolveReference(objRef) - (((CLRType)ft).FieldIndexMapping[(int)ip->OperandLong] + 1);
                                        }
                                        reg1->ObjectType = ObjectTypes.StackObjectReference;
                                        *(long*)&reg1->Value = (long)fieldAddr;
                                    }
                                    else
                                    {
                                        obj = RetriveObject(objRef, mStack);
                                        if (obj != null)
                                        {
                                            AssignToRegister(ref info, ip->Register1, obj);
                                            reg1->ObjectType = ObjectTypes.FieldReference;
                                            reg1->ValueLow = (int)ip->OperandLong;
                                        }
                                        else
                                            throw new NullReferenceException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Stsfld:
                                {
                                    type = AppDomain.GetType((int)(ip->OperandLong >> 32));
                                    if (type != null)
                                    {
                                        reg1 = (r + ip->Register1);
                                        if (type is ILType)
                                        {
                                            ILType t = type as ILType;
                                            t.StaticInstance.AssignFromStack((int)ip->OperandLong, reg1, AppDomain, mStack);
                                        }
                                        else
                                        {
                                            CLRType t = type as CLRType;
                                            intVal = (int)ip->OperandLong;
                                            var f = t.GetField(intVal);
                                            obj = null;
                                            CopyToStack(esp, reg1, mStack);

                                            if (!((CLRType)t).AssignFieldFromStack(intVal, ref obj, this, esp, mStack))
                                                t.SetStaticFieldValue(intVal, f.FieldType.CheckCLRTypes(CheckAndCloneValueType(StackObject.ToObject(reg1, domain, mStack), domain)));
                                            Free(esp);
                                        }
                                    }
                                    else
                                        throw new TypeLoadException();
                                }
                                break;
                            case OpCodeREnum.Ldsfld:
                                {
                                    type = AppDomain.GetType((int)(ip->OperandLong >> 32));
                                    if (type != null)
                                    {
                                        if (type is ILType)
                                        {
                                            ILType t = type as ILType;
                                            t.StaticInstance.CopyToRegister((int)ip->OperandLong, ref info, ip->Register1);
                                        }
                                        else
                                        {
                                            CLRType t = type as CLRType;
                                            intVal = (int)ip->OperandLong;
                                            if (!((CLRType)type).CopyFieldToStack(intVal, null, this, ref esp, mStack))
                                            {
                                                var f = t.GetField(intVal);
                                                obj = t.GetFieldValue(intVal, null);
                                                if (obj is CrossBindingAdaptorType)
                                                    obj = ((CrossBindingAdaptorType)obj).ILInstance;
                                                AssignToRegister(ref info, ip->Register1, obj, f.FieldType == typeof(object));
                                            }
                                            else
                                            {
                                                esp = PopToRegister(ref info, ip->Register1, esp);
                                            }
                                        }
                                    }
                                    else
                                        throw new TypeLoadException();
                                }
                                break;
                            case OpCodeREnum.Ldsflda:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg1->ObjectType = ObjectTypes.StaticFieldReference;
                                    reg1->Value = (int)(ip->OperandLong >> 32);
                                    reg1->ValueLow = (int)(ip->OperandLong);
                                }
                                break;
                            #endregion

                            #region Initialization & Instantiation
                            case OpCodeREnum.Newobj:
                                {
                                    IMethod m = domain.GetMethod(ip->Operand2);
                                    if (m != null)
                                    {
                                        intVal = m.ParameterCount;
                                        intVal = intVal - Math.Max((intVal - RegisterVM.JITCompiler.CallRegisterParamCount), 0);
                                        for (int i = 0; i < intVal; i++)
                                        {
                                            switch (i)
                                            {
                                                case 0:
                                                    reg1 = (r + ip->Register2);
                                                    break;
                                                case 1:
                                                    reg1 = (r + ip->Register3);
                                                    break;
                                                case 2:
                                                    reg1 = (r + ip->Register4);
                                                    break;
                                                default:
                                                    throw new NotSupportedException();
                                            }
                                            CopyToStack(esp, reg1, mStack);
                                            esp++;
                                        }
                                    }
                                    if (m is ILMethod)
                                    {
                                        type = m.DeclearingType as ILType;
                                        if (type.IsDelegate)
                                        {
                                            objRef = GetObjectAndResolveReference(esp - 1 - 1);
                                            var mi = (IMethod)mStack[(esp - 1)->Value];
                                            object ins;
                                            if (objRef->ObjectType == ObjectTypes.Null)
                                                ins = null;
                                            else
                                                ins = mStack[objRef->Value];
                                            Free(esp - 1);
                                            Free(esp - 1 - 1);
                                            esp = esp - 1 - 1;
                                            object dele;
                                            var ilMethod = mi as ILMethod;
                                            if (ilMethod != null)
                                            {
                                                if (ins != null)
                                                {
                                                    dele = ((ILTypeInstance)ins).GetDelegateAdapter(ilMethod);
                                                    if (dele == null)
                                                    {
                                                        var invokeMethod = type.GetMethod("Invoke", mi.ParameterCount);
                                                        if (invokeMethod == null && ilMethod.IsExtend)
                                                        {
                                                            invokeMethod = type.GetMethod("Invoke", mi.ParameterCount - 1);
                                                        }
                                                        dele = domain.DelegateManager.FindDelegateAdapter(
                                                            (ILTypeInstance)ins, ilMethod, invokeMethod);
                                                    }
                                                }
                                                else
                                                {
                                                    if (ilMethod.DelegateAdapter == null)
                                                    {
                                                        var invokeMethod = type.GetMethod("Invoke", mi.ParameterCount);
                                                        ilMethod.DelegateAdapter = domain.DelegateManager.FindDelegateAdapter(null, ilMethod, invokeMethod);
                                                    }
                                                    dele = ilMethod.DelegateAdapter;
                                                }
                                            }

                                            else
                                            {
                                                throw new NotImplementedException();
                                            }
                                            esp = PushObject(esp, mStack, dele);
                                        }
                                        else
                                        {
                                            reg1 = esp - m.ParameterCount;
                                            obj = null;
                                            bool isValueType = type.IsValueType;
                                            bool useRegister = ((ILMethod)m).ShouldUseRegisterVM;
                                            if (isValueType)
                                            {
                                                stack.AllocValueType(esp, type);
                                                objRef = esp + 1;
                                                objRef->ObjectType = ObjectTypes.StackObjectReference;
                                                *(StackObject**)&objRef->Value = esp;
                                                if (useRegister)
                                                    mStack.Add(null);
                                                objRef++;
                                            }
                                            else
                                            {
                                                obj = ((ILType)type).Instantiate(false);
#if DEBUG
                                                if (obj == null)
                                                    throw new NullReferenceException();
#endif
                                                objRef = PushObject(esp, mStack, obj);//this parameter for constructor
                                            }
                                            esp = objRef;
                                            for (int i = 0; i < m.ParameterCount; i++)
                                            {
                                                CopyToStack(esp, reg1 + i, mStack);
                                                if (esp->ObjectType < ObjectTypes.Object && useRegister)
                                                {
                                                    mStack.Add(null);
                                                }
                                                esp++;
                                            }
                                            if (useRegister)
                                                esp = ExecuteR(((ILMethod)m), esp, out unhandledException);
                                            else
                                            {
                                                esp = Execute(((ILMethod)m), esp, out unhandledException);
                                            }
                                            ValueTypeBasePointer = bp;
                                            if (isValueType)
                                            {
                                                var ins = objRef - 1 - 1;
                                                *reg1 = *ins;
                                                esp = reg1 + 1;
                                            }
                                            else
                                            {
                                                //PushToRegister(ref info, ip->Register1, obj);
                                                //mStack[reg1->Value] = obj;
                                                esp = PushObject(reg1, mStack, obj);//new constructedObj
                                            }
                                        }
                                        if (unhandledException)
                                            returned = true;
                                    }
                                    else
                                    {
                                        CLRMethod cm = (CLRMethod)m;
                                        //Means new object();
                                        if (cm == null)
                                        {
                                            esp = PushObject(esp, mStack, new object());
                                        }
                                        else
                                        {
                                            if (cm.DeclearingType.IsDelegate)
                                            {
                                                objRef = GetObjectAndResolveReference(esp - 1 - 1);
                                                var mi = (IMethod)mStack[(esp - 1)->Value];
                                                object ins;
                                                if (objRef->ObjectType == ObjectTypes.Null)
                                                    ins = null;
                                                else
                                                    ins = mStack[objRef->Value];
                                                Free(esp - 1);
                                                Free(esp - 1 - 1);
                                                esp = esp - 1 - 1;
                                                object dele;
                                                var ilMethod = mi as ILMethod;
                                                if (ilMethod != null)
                                                {
                                                    dele = domain.DelegateManager.FindDelegateAdapter((CLRType)cm.DeclearingType, (ILTypeInstance)ins, ilMethod);
                                                }
                                                else
                                                {
                                                    if (ins is ILTypeInstance)
                                                        ins = ((ILTypeInstance)ins).CLRInstance;
                                                    dele = Delegate.CreateDelegate(cm.DeclearingType.TypeForCLR, ins, ((CLRMethod)mi).MethodInfo);
                                                }
                                                esp = PushObject(esp, mStack, dele);
                                            }
                                            else
                                            {
                                                var redirect = cm.Redirection;
                                                if (redirect != null)
                                                    esp = redirect(this, esp, mStack, cm, true);
                                                else
                                                {
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                                                    if (!allowUnboundCLRMethod)
                                                        throw new NotSupportedException(cm.ToString() + " is not bound!");
#endif
                                                    object result = cm.Invoke(this, esp, mStack, true);
                                                    int paramCount = cm.ParameterCount;
                                                    for (int i = 1; i <= paramCount; i++)
                                                    {
                                                        Free(esp - i);
                                                    }
                                                    esp = esp - (paramCount);
                                                    esp = PushObject(esp, mStack, result);//new constructedObj
                                                }
                                            }
                                        }
                                    }
                                    esp = PopToRegister(ref info, ip->Register1, esp);
                                }
                                break;
                            case OpCodeREnum.Box:
                                {
                                    reg1 = (r + ip->Register1);
                                    objRef = (r + ip->Register2);
                                    type = domain.GetType(ip->Operand);
                                    if (type != null)
                                    {
                                        if (type is ILType)
                                        {
                                            if (((ILType)type).IsEnum)
                                            {
                                                ILEnumTypeInstance ins = new Intepreter.ILEnumTypeInstance((ILType)type);
                                                ins.AssignFromStack(0, objRef, AppDomain, mStack);
                                                ins.Boxed = true;
                                                AssignToRegister(ref info, ip->Register1, ins, true);
                                            }
                                            else
                                            {
                                                switch (objRef->ObjectType)
                                                {
                                                    case ObjectTypes.Null:
                                                        break;
                                                    case ObjectTypes.ValueTypeObjectReference:
                                                        {
                                                            ILTypeInstance ins = ((ILType)type).Instantiate(false);
                                                            ins.AssignFromStack(objRef, domain, mStack);
                                                            //FreeStackValueType(obj);
                                                            AssignToRegister(ref info, ip->Register1, ins, true);
                                                        }
                                                        break;
                                                    default:
                                                        {
                                                            obj = mStack[objRef->Value];
                                                            //Free(obj);
                                                            if (type.IsArray)
                                                            {
                                                                AssignToRegister(ref info, ip->Register1, obj, true);
                                                            }
                                                            else
                                                            {
                                                                ILTypeInstance ins = (ILTypeInstance)obj;
                                                                if (ins != null)
                                                                {
                                                                    if (ins.IsValueType)
                                                                    {
                                                                        ins.Boxed = true;
                                                                    }
                                                                    AssignToRegister(ref info, ip->Register1, ins, true);
                                                                }
                                                                else
                                                                {
                                                                    AssignToRegister(ref info, ip->Register1, null, false);
                                                                }
                                                            }
                                                        }
                                                        break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (type.TypeForCLR.IsPrimitive)
                                            {
                                                var t = type.TypeForCLR;
                                                if (t == typeof(int))
                                                {
                                                    switch (objRef->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            AssignToRegister(ref info, ip->Register1, objRef->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            AssignToRegister(ref info, ip->Register1, 0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(bool))
                                                {
                                                    switch (objRef->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            AssignToRegister(ref info, ip->Register1, objRef->Value == 1, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            AssignToRegister(ref info, ip->Register1, false, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(byte))
                                                {
                                                    switch (objRef->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            AssignToRegister(ref info, ip->Register1, (byte)objRef->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            AssignToRegister(ref info, ip->Register1, (byte)0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(short))
                                                {
                                                    switch (objRef->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            AssignToRegister(ref info, ip->Register1, (short)objRef->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            AssignToRegister(ref info, ip->Register1, (short)0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(long))
                                                {
                                                    switch (objRef->ObjectType)
                                                    {
                                                        case ObjectTypes.Long:
                                                            AssignToRegister(ref info, ip->Register1, *(long*)&objRef->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            AssignToRegister(ref info, ip->Register1, 0L, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(float))
                                                {
                                                    switch (objRef->ObjectType)
                                                    {
                                                        case ObjectTypes.Float:
                                                            AssignToRegister(ref info, ip->Register1, *(float*)&objRef->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            AssignToRegister(ref info, ip->Register1, 0f, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(double))
                                                {
                                                    switch (objRef->ObjectType)
                                                    {
                                                        case ObjectTypes.Double:
                                                            AssignToRegister(ref info, ip->Register1, *(double*)&objRef->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            AssignToRegister(ref info, ip->Register1, 0.0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(char))
                                                {
                                                    switch (objRef->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            AssignToRegister(ref info, ip->Register1, (char)objRef->Value, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(uint))
                                                {
                                                    switch (objRef->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            AssignToRegister(ref info, ip->Register1, (uint)objRef->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            AssignToRegister(ref info, ip->Register1, (uint)0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(ushort))
                                                {
                                                    switch (objRef->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            AssignToRegister(ref info, ip->Register1, (ushort)objRef->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            AssignToRegister(ref info, ip->Register1, (ushort)0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(ulong))
                                                {
                                                    switch (objRef->ObjectType)
                                                    {
                                                        case ObjectTypes.Long:
                                                            AssignToRegister(ref info, ip->Register1, *(ulong*)&objRef->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            AssignToRegister(ref info, ip->Register1, (ulong)0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(sbyte))
                                                {
                                                    switch (objRef->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            AssignToRegister(ref info, ip->Register1, (sbyte)objRef->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            AssignToRegister(ref info, ip->Register1, (sbyte)0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else
                                                    throw new NotImplementedException();
                                            }
                                            else if (type.TypeForCLR.IsEnum)
                                            {
                                                AssignToRegister(ref info, ip->Register1, Enum.ToObject(type.TypeForCLR, StackObject.ToObject(objRef, AppDomain, mStack)), true);
                                            }
                                            else
                                            {
                                                if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                                {
                                                    dst = *(StackObject**)&objRef->Value;
                                                    var vt = domain.GetTypeByIndex(dst->Value);
                                                    if (vt != type)
                                                        throw new InvalidCastException();
                                                    obj = ((CLRType)vt).ValueTypeBinder.ToObject(dst, mStack);
                                                    AssignToRegister(ref info, ip->Register1, obj, true);
                                                }
                                                else if (objRef->ObjectType == ObjectTypes.Object)
                                                {
                                                    obj = mStack[objRef->Value];
                                                    AssignToRegister(ref info, ip->Register1, obj, true);
                                                }
                                                else
                                                {
                                                    CopyToRegister(ref info, ip->Register1, objRef);
                                                }
                                            }
                                        }
                                    }
                                    else
                                        throw new NullReferenceException();

                                    //esp = PopToRegister(ref info, ip->Register1, esp);
                                }
                                break;
                            case OpCodeREnum.Constrained:
                                {
                                    type = domain.GetType(ip->Operand);
                                    var m = domain.GetMethod((int)ip->Operand2);
                                    var pCnt = m.ParameterCount;
                                    objRef = esp - (pCnt + 1);
                                    var insIdx = mStack.Count;
                                    if (objRef->ObjectType < ObjectTypes.Object)
                                    {
                                        bool moved = false;
                                        //move parameters
                                        for (int i = 0; i < pCnt; i++)
                                        {
                                            var pPtr = esp - (i + 1);
                                            if (pPtr->ObjectType >= ObjectTypes.Object)
                                            {
                                                var oldVal = pPtr->Value;
                                                insIdx--;
                                                if (!moved)
                                                {
                                                    pPtr->Value = mStack.Count;
                                                    mStack.Add(mStack[oldVal]);
                                                    mStack[oldVal] = null;
                                                    moved = true;
                                                }
                                                else
                                                {
                                                    mStack[oldVal + 1] = mStack[oldVal];
                                                    mStack[oldVal] = null;
                                                    pPtr->Value = oldVal + 1;
                                                }
                                            }
                                        }
                                        if (!moved)
                                        {
                                            mStack.Add(null);
                                        }
                                    }
                                    else
                                        insIdx = objRef->Value;
                                    objRef2 = GetObjectAndResolveReference(objRef);
                                    if (type != null)
                                    {
                                        if (type is ILType)
                                        {
                                            var t = (ILType)type;
                                            if (t.IsEnum)
                                            {
                                                ILEnumTypeInstance ins = new ILEnumTypeInstance(t);
                                                switch (objRef2->ObjectType)
                                                {
                                                    case ObjectTypes.FieldReference:
                                                        {
                                                            var owner = mStack[objRef2->Value] as ILTypeInstance;
                                                            int idx = objRef2->ValueLow;
                                                            //Free(objRef);
                                                            owner.PushToStack(idx, objRef, this, mStack);
                                                            ins.AssignFromStack(0, objRef, AppDomain, mStack);
                                                            ins.Boxed = true;
                                                        }
                                                        break;
                                                    case ObjectTypes.StaticFieldReference:
                                                        {
                                                            var st = AppDomain.GetType(objRef2->Value) as ILType;
                                                            int idx = objRef2->ValueLow;
                                                            //Free(objRef);
                                                            st.StaticInstance.PushToStack(idx, objRef, this, mStack);
                                                            ins.AssignFromStack(0, objRef, AppDomain, mStack);
                                                            ins.Boxed = true;
                                                        }
                                                        break;
                                                    case ObjectTypes.ArrayReference:
                                                        {
                                                            var arr = mStack[objRef2->Value];
                                                            var idx = objRef2->ValueLow;
                                                            //Free(objRef);
                                                            LoadFromArrayReference(arr, idx, objRef, t, mStack);
                                                            ins.AssignFromStack(0, objRef, AppDomain, mStack);
                                                            ins.Boxed = true;
                                                        }
                                                        break;
                                                    default:
                                                        ins.AssignFromStack(0, objRef2, AppDomain, mStack);
                                                        ins.Boxed = true;
                                                        break;
                                                }
                                                objRef->ObjectType = ObjectTypes.Object;
                                                objRef->Value = insIdx;
                                                mStack[insIdx] = ins;

                                                //esp = PushObject(esp - 1, mStack, ins);
                                            }
                                            else if (objRef2->ObjectType != ObjectTypes.ValueTypeObjectReference)
                                            {
                                                object res = RetriveObject(objRef2, mStack);
                                                //Free(objRef);
                                                objRef->ObjectType = ObjectTypes.Object;
                                                objRef->Value = insIdx;
                                                mStack[insIdx] = res;
                                                //esp = PushObject(objRef, mStack, res, true);
                                            }
                                        }
                                        else
                                        {
                                            var tt = type.TypeForCLR;
                                            if (tt.IsEnum)
                                            {
                                                mStack[insIdx] = Enum.ToObject(tt, StackObject.ToObject(objRef2, AppDomain, mStack));
                                                objRef->ObjectType = ObjectTypes.Object;
                                                objRef->Value = insIdx;
                                                //esp = PushObject(esp - 1, mStack, Enum.ToObject(tt, StackObject.ToObject(obj, AppDomain, mStack)), true);
                                            }
                                            else if (tt.IsPrimitive)
                                            {
                                                mStack[insIdx] = tt.CheckCLRTypes(StackObject.ToObject(objRef2, AppDomain, mStack));
                                                objRef->ObjectType = ObjectTypes.Object;
                                                objRef->Value = insIdx;
                                                //esp = PushObject(esp - 1, mStack, tt.CheckCLRTypes(StackObject.ToObject(obj, AppDomain, mStack)));
                                            }
                                            else if (objRef2->ObjectType != ObjectTypes.ValueTypeObjectReference)
                                            {
                                                object res = RetriveObject(objRef2, mStack);
                                                //Free(objRef);
                                                objRef->ObjectType = ObjectTypes.Object;
                                                objRef->Value = insIdx;
                                                mStack[insIdx] = res;
                                                //esp = PushObject(objRef, mStack, res, true);
                                            }
                                        }
                                    }
                                    else
                                        throw new NullReferenceException();
                                }
                                break;
                            case OpCodeREnum.Unbox:
                            case OpCodeREnum.Unbox_Any:
                                {
                                    objRef = (r + ip->Register2);
                                    if (objRef->ObjectType == ObjectTypes.Object)
                                    {
                                        obj = mStack[objRef->Value];
                                        if (obj != null)
                                        {
                                            var t = domain.GetType(ip->Operand);
                                            if (t != null)
                                            {
                                                clrType = t.TypeForCLR;
                                                bool isEnumObj = obj is ILEnumTypeInstance;
                                                if ((t is CLRType) && clrType.IsPrimitive && !isEnumObj)
                                                {
                                                    reg1 = (r + ip->Register1);
                                                    if (clrType == typeof(int))
                                                    {
                                                        intVal = obj.ToInt32();
                                                        reg1->ObjectType = ObjectTypes.Integer;
                                                        reg1->Value = intVal;
                                                    }
                                                    else if (clrType == typeof(bool))
                                                    {
                                                        var boolVal = (bool)obj;
                                                        reg1->ObjectType = ObjectTypes.Integer;
                                                        reg1->Value = boolVal ? 1 : 0;
                                                    }
                                                    else if (clrType == typeof(short))
                                                    {
                                                        short shortVal = obj.ToInt16();
                                                        reg1->ObjectType = ObjectTypes.Integer;
                                                        reg1->Value = shortVal;
                                                    }
                                                    else if (clrType == typeof(long))
                                                    {
                                                        longVal = obj.ToInt64();
                                                        reg1->ObjectType = ObjectTypes.Long;
                                                        *(long*)&reg1->Value = longVal;
                                                    }
                                                    else if (clrType == typeof(float))
                                                    {
                                                        floatVal = obj.ToFloat();
                                                        reg1->ObjectType = ObjectTypes.Float;
                                                        *(float*)&reg1->Value = floatVal;
                                                    }
                                                    else if (clrType == typeof(byte))
                                                    {
                                                        byte bVal = (byte)obj;
                                                        reg1->ObjectType = ObjectTypes.Integer;
                                                        reg1->Value = bVal;
                                                    }
                                                    else if (clrType == typeof(double))
                                                    {
                                                        doubleVal = obj.ToDouble();
                                                        reg1->ObjectType = ObjectTypes.Double;
                                                        *(double*)&reg1->Value = doubleVal;
                                                    }
                                                    else if (clrType == typeof(char))
                                                    {
                                                        char cVal = (char)obj;
                                                        reg1->ObjectType = ObjectTypes.Integer;
                                                        *(char*)&reg1->Value = cVal;
                                                    }
                                                    else if (clrType == typeof(uint))
                                                    {
                                                        uint uVal = (uint)obj;
                                                        reg1->ObjectType = ObjectTypes.Integer;
                                                        reg1->Value = (int)uVal;
                                                    }
                                                    else if (clrType == typeof(ushort))
                                                    {
                                                        ushort usVal = (ushort)obj;
                                                        reg1->ObjectType = ObjectTypes.Integer;
                                                        reg1->Value = usVal;
                                                    }
                                                    else if (clrType == typeof(ulong))
                                                    {
                                                        ulong ulVal = (ulong)obj;
                                                        reg1->ObjectType = ObjectTypes.Long;
                                                        *(ulong*)&reg1->Value = ulVal;
                                                    }
                                                    else if (clrType == typeof(sbyte))
                                                    {
                                                        sbyte sbVal = (sbyte)obj;
                                                        reg1->ObjectType = ObjectTypes.Integer;
                                                        reg1->Value = sbVal;
                                                    }
                                                    else
                                                        throw new NotImplementedException();
                                                }
                                                else if (t.IsValueType)
                                                {
                                                    if (obj is ILTypeInstance)
                                                    {
                                                        var res = ((ILTypeInstance)obj);
                                                        if (res is ILEnumTypeInstance)
                                                        {
                                                            res.CopyToRegister(0, ref info, ip->Register1);
                                                        }
                                                        else
                                                        {
                                                            if (res.Boxed)
                                                            {
                                                                res = res.Clone();
                                                                res.Boxed = false;
                                                            }
                                                            AssignToRegister(ref info, ip->Register1, res);
                                                        }
                                                    }
                                                    else
                                                        AssignToRegister(ref info, ip->Register1, obj);

                                                }
                                                else
                                                {
                                                    AssignToRegister(ref info, ip->Register1, obj);
                                                }
                                            }
                                            else
                                                throw new TypeLoadException();
                                        }
                                        else
                                            throw new NullReferenceException();
                                    }
                                    else if (objRef->ObjectType < ObjectTypes.StackObjectReference)
                                    {
                                        //Nothing to do with primitive types
                                    }
                                    else
                                        throw new InvalidCastException();
                                }
                                break;
                            case OpCodeREnum.Initobj:
                                {
                                    reg1 = (r + ip->Register1);
                                    objRef = ip->Operand2 == 1 ? reg1 : GetObjectAndResolveReference(reg1);
                                    type = domain.GetType(ip->Operand);
                                    if (type is ILType)
                                    {
                                        ILType it = (ILType)type;
                                        if (it.IsValueType)
                                        {
                                            if (it.IsEnum || it.IsPrimitive)
                                            {
                                                StackObject.Initialized(objRef, type);
                                            }
                                            else
                                            {
                                                if (objRef >= info.RegisterStart && objRef < info.RegisterEnd)
                                                {
                                                    stack.AllocValueType(objRef, type, true);
                                                }
                                                else
                                                {
                                                    switch (objRef->ObjectType)
                                                    {
                                                        case ObjectTypes.Null:
                                                            throw new NullReferenceException();
                                                        case ObjectTypes.ValueTypeObjectReference:
                                                            stack.ClearValueTypeObject(type, ILIntepreter.ResolveReference(objRef));
                                                            break;
                                                        case ObjectTypes.Object:
                                                            {
                                                                obj = mStack[objRef->Value];
                                                                if (obj == null)
                                                                {
                                                                    throw new NotSupportedException();
                                                                }

                                                                if (obj is ILTypeInstance)
                                                                {
                                                                    ILTypeInstance instance = obj as ILTypeInstance;
                                                                    instance.Clear();
                                                                }
                                                                else
                                                                    throw new NotSupportedException();
                                                            }
                                                            break;
                                                        case ObjectTypes.ArrayReference:
                                                            {
                                                                var arr = mStack[objRef->Value] as Array;
                                                                var idx = objRef->ValueLow;
                                                                obj = arr.GetValue(idx);
                                                                if (obj == null)
                                                                    arr.SetValue(it.Instantiate(), idx);
                                                                else
                                                                {
                                                                    if (obj is ILTypeInstance)
                                                                    {
                                                                        ILTypeInstance instance = obj as ILTypeInstance;
                                                                        instance.Clear();
                                                                    }
                                                                    else
                                                                        throw new NotImplementedException();
                                                                }
                                                            }
                                                            break;
                                                        case ObjectTypes.FieldReference:
                                                            {
                                                                obj = mStack[objRef->Value];
                                                                if (obj != null)
                                                                {
                                                                    if (obj is ILTypeInstance)
                                                                    {
                                                                        ILTypeInstance instance = obj as ILTypeInstance;
                                                                        var tar = instance[objRef->ValueLow] as ILTypeInstance;
                                                                        if (tar != null)
                                                                            tar.Clear();
                                                                        else
                                                                            throw new NotSupportedException();
                                                                    }
                                                                    else
                                                                        throw new NotSupportedException();
                                                                }
                                                                else
                                                                    throw new NullReferenceException();
                                                            }
                                                            break;
                                                        case ObjectTypes.StaticFieldReference:
                                                            {
                                                                var t = AppDomain.GetType(objRef->Value);
                                                                int idx = objRef->ValueLow;
                                                                if (t is ILType)
                                                                {
                                                                    var tar = ((ILType)t).StaticInstance[idx] as ILTypeInstance;
                                                                    if (tar != null)
                                                                        tar.Clear();
                                                                    else
                                                                        throw new NotSupportedException();
                                                                }
                                                                else
                                                                    throw new NotSupportedException();
                                                            }
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            PushNull(esp);
                                            switch (objRef->ObjectType)
                                            {
                                                case ObjectTypes.StaticFieldReference:
                                                    {
                                                        var t = AppDomain.GetType(objRef->Value) as ILType;
                                                        t.StaticInstance.AssignFromStack(objRef->ValueLow, esp, AppDomain, mStack);
                                                    }
                                                    break;
                                                case ObjectTypes.FieldReference:
                                                    {
                                                        var instance = mStack[objRef->Value] as ILTypeInstance;
                                                        instance.AssignFromStack(objRef->ValueLow, esp, AppDomain, mStack);
                                                    }
                                                    break;
                                                default:
                                                    {
                                                        if (objRef->ObjectType >= ObjectTypes.Object)
                                                            mStack[objRef->Value] = null;
                                                        else
                                                        {
                                                            if (reg1->ObjectType != ObjectTypes.StackObjectReference)
                                                                WriteNull(ref info, ip->Register1);
                                                            else if (objRef >= info.RegisterStart && objRef < info.RegisterEnd)
                                                            {
                                                                short reg = (short)(objRef - info.RegisterStart);
                                                                WriteNull(ref info, reg);
                                                            }
                                                            else
                                                                throw new NotSupportedException();
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                        {
                                            stack.ClearValueTypeObject(type, ILIntepreter.ResolveReference(objRef));
                                        }
                                        else if (objRef->ObjectType == ObjectTypes.FieldReference)
                                        {
                                            var instance = mStack[objRef->Value] as ILTypeInstance;
                                            if (instance != null)
                                            {
                                                instance.InitializeField(objRef->ValueLow);
                                            }
                                            else
                                                throw new NotImplementedException();
                                        }
                                        else if (type.IsPrimitive || type.IsEnum)
                                            StackObject.Initialized(objRef, type);
                                        else
                                        {
                                            if (!type.IsValueType)
                                            {
                                                if (objRef->ObjectType >= ObjectTypes.Object)
                                                    mStack[objRef->Value] = null;
                                                else
                                                {
                                                    if (objRef >= info.RegisterStart && objRef < info.RegisterEnd)
                                                    {
                                                        short reg = (short)(objRef - info.RegisterStart);
                                                        WriteNull(ref info, reg);
                                                    }
                                                    else
                                                        throw new NotSupportedException();
                                                }
                                            }
                                            else
                                            {
                                                var cT = (CLRType)type;
                                                if (cT.ValueTypeBinder != null)
                                                {
                                                    if (objRef >= info.RegisterStart && objRef < info.RegisterEnd)
                                                    {
                                                        stack.AllocValueType(objRef, type, true);
                                                        continue;
                                                    }
                                                    else
                                                        throw new NotSupportedException();
                                                }
                                                obj = cT.CreateDefaultInstance();
                                                if (objRef->ObjectType >= ObjectTypes.Object)
                                                    mStack[objRef->Value] = obj;
                                                else
                                                {
                                                    if (objRef >= info.RegisterStart && objRef < info.RegisterEnd)
                                                    {
                                                        short reg = (short)(objRef - info.RegisterStart);
                                                        AssignToRegister(ref info, reg, obj);
                                                    }
                                                    else
                                                        throw new NotSupportedException();
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            case OpCodeREnum.Isinst:
                                {
                                    reg2 = (r + ip->Register2);
                                    type = domain.GetType(ip->Operand);
                                    if (type != null)
                                    {
                                        objRef = GetObjectAndResolveReference(reg2);
                                        if (objRef->ObjectType <= ObjectTypes.Double)
                                        {
                                            var tclr = type.TypeForCLR;
                                            switch (objRef->ObjectType)
                                            {
                                                case ObjectTypes.Integer:
                                                    {
                                                        if (tclr != typeof(int) && tclr != typeof(bool) && tclr != typeof(short) && tclr != typeof(byte) && tclr != typeof(ushort) && tclr != typeof(uint))
                                                        {
                                                            WriteNull(ref info, ip->Register1);
                                                        }
                                                    }
                                                    break;
                                                case ObjectTypes.Long:
                                                    {
                                                        if (tclr != typeof(long) && tclr != typeof(ulong))
                                                        {
                                                            WriteNull(ref info, ip->Register1);
                                                        }
                                                    }
                                                    break;
                                                case ObjectTypes.Float:
                                                    {
                                                        if (tclr != typeof(float))
                                                        {
                                                            WriteNull(ref info, ip->Register1);
                                                        }
                                                    }
                                                    break;
                                                case ObjectTypes.Double:
                                                    {
                                                        if (tclr != typeof(double))
                                                        {
                                                            WriteNull(ref info, ip->Register1);
                                                        }
                                                    }
                                                    break;
                                                case ObjectTypes.Null:
                                                    WriteNull(ref info, ip->Register1);
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            obj = RetriveObject(objRef, mStack);

                                            if (obj != null)
                                            {
                                                if (obj is ILTypeInstance)
                                                {
                                                    if (((ILTypeInstance)obj).CanAssignTo(type))
                                                    {
                                                        AssignToRegister(ref info, ip->Register1, obj);
                                                    }
                                                    else
                                                    {
                                                        WriteNull(ref info, ip->Register1);
                                                    }
                                                }
                                                else
                                                {
                                                    if (type.TypeForCLR.IsAssignableFrom(obj.GetType()))
                                                    {
                                                        AssignToRegister(ref info, ip->Register1, obj, true);
                                                    }
                                                    else
                                                    {
                                                        WriteNull(ref info, ip->Register1);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                WriteNull(ref info, ip->Register1);
                                            }
                                        }
                                    }
                                    else
                                        throw new NullReferenceException();
                                }
                                break;

                            case OpCodeREnum.Ldftn:
                                {
                                    IMethod m = domain.GetMethod(ip->Operand2);
                                    AssignToRegister(ref info, ip->Register1, m);
                                }
                                break;
                            case OpCodeREnum.Ldvirtftn:
                                {
                                    IMethod m = domain.GetMethod(ip->Operand2);
                                    objRef = (r + ip->Register2);
                                    if (m is ILMethod)
                                    {
                                        ILMethod ilm = (ILMethod)m;

                                        obj = mStack[objRef->Value];
                                        m = ((ILTypeInstance)obj).Type.GetVirtualMethod(ilm) as ILMethod;
                                    }
                                    else
                                    {
                                        obj = mStack[objRef->Value];
                                        if (obj is ILTypeInstance)
                                            m = ((ILTypeInstance)obj).Type.GetVirtualMethod(m);
                                        else if (obj is CrossBindingAdaptorType)
                                        {
                                            m = ((CrossBindingAdaptorType)obj).ILInstance.Type.BaseType.GetVirtualMethod(m);
                                        }
                                    }
                                    AssignToRegister(ref info, ip->Register1, m);
                                }
                                break;
                            #endregion

                            #region Compare
                            case OpCodeREnum.Ceq:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    bool res = false;
                                    if (reg1->ObjectType == reg2->ObjectType)
                                    {
                                        switch (reg1->ObjectType)
                                        {
                                            case ObjectTypes.Integer:
                                            case ObjectTypes.Float:
                                                res = reg1->Value == reg2->Value;
                                                break;
                                            case ObjectTypes.Object:
                                                res = mStack[reg1->Value] == mStack[reg2->Value];
                                                break;
                                            case ObjectTypes.FieldReference:
                                                res = mStack[reg1->Value] == mStack[reg2->Value] && reg1->ValueLow == reg2->ValueLow;
                                                break;
                                            case ObjectTypes.Null:
                                                res = true;
                                                break;
                                            default:
                                                res = reg1->Value == reg2->Value && reg1->ValueLow == reg2->ValueLow;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        switch (reg1->ObjectType)
                                        {
                                            case ObjectTypes.Object:
                                                res = mStack[reg1->Value] == null && reg2->ObjectType == ObjectTypes.Null;
                                                break;
                                            case ObjectTypes.Null:
                                                res = reg2->ObjectType == ObjectTypes.Object && mStack[reg2->Value] == null;
                                                break;
                                        }
                                    }
                                    if (res)
                                        WriteOne(reg3);
                                    else
                                        WriteZero(reg3);

                                }
                                break;
                            case OpCodeREnum.Ceqi:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    bool res = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Float:
                                            res = *(float*)&reg1->Value == ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Long:
                                            res = *(long*)&reg1->Value == ip->OperandLong;
                                            break;
                                        case ObjectTypes.Double:
                                            res = *(double*)&reg1->Value == ip->OperandDouble;
                                            break;
                                        case ObjectTypes.Integer:
                                            res = reg1->Value == ip->Operand;
                                            break;
                                        case ObjectTypes.Null:
                                            res = ip->Operand == 0;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        WriteOne(reg3);
                                    else
                                        WriteZero(reg3);

                                }
                                break;
                            case OpCodeREnum.Clt:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    bool res = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = reg1->Value < reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            res = *(long*)&reg1->Value < *(long*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&reg1->Value < *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            res = *(double*)&reg1->Value < *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        WriteOne(reg3);
                                    else
                                        WriteZero(reg3);
                                }
                                break;
                            case OpCodeREnum.Clti:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    bool res = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = reg1->Value < ip->Operand;
                                            break;
                                        case ObjectTypes.Long:
                                            res = *(long*)&reg1->Value < ip->OperandLong;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&reg1->Value < ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            res = *(double*)&reg1->Value < ip->OperandDouble;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        WriteOne(reg3);
                                    else
                                        WriteZero(reg3);
                                }
                                break;
                            case OpCodeREnum.Clt_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    bool res = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = (uint)reg1->Value < (uint)reg2->Value && reg2->ObjectType != ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Long:
                                            res = (ulong)*(long*)&reg1->Value < (ulong)*(long*)&reg2->Value && reg2->ObjectType != ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&reg1->Value < *(float*)&reg2->Value && reg2->ObjectType != ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Double:
                                            res = *(double*)&reg1->Value < *(double*)&reg2->Value && reg2->ObjectType != ObjectTypes.Null;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        WriteOne(reg3);
                                    else
                                        WriteZero(reg3);
                                }
                                break;
                            case OpCodeREnum.Clti_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    bool res = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = (uint)reg1->Value < (uint)ip->Operand;
                                            break;
                                        case ObjectTypes.Long:
                                            res = (ulong)*(long*)&reg1->Value < (ulong)ip->OperandLong;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&reg1->Value < ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            res = *(double*)&reg1->Value < ip->OperandDouble;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        WriteOne(reg3);
                                    else
                                        WriteZero(reg3);
                                }
                                break;
                            case OpCodeREnum.Cgt:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    bool res = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = reg1->Value > reg2->Value || reg2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Long:
                                            res = *(long*)&reg1->Value > *(long*)&reg2->Value || reg2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&reg1->Value > *(float*)&reg2->Value || reg2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Double:
                                            res = *(double*)&reg1->Value > *(double*)&reg2->Value || reg2->ObjectType == ObjectTypes.Null;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        WriteOne(reg3);
                                    else
                                        WriteZero(reg3);
                                }
                                break;
                            case OpCodeREnum.Cgti:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    bool res = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = reg1->Value > ip->Operand;
                                            break;
                                        case ObjectTypes.Long:
                                            res = *(long*)&reg1->Value > ip->OperandLong;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&reg1->Value > ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            res = *(double*)&reg1->Value > ip->OperandDouble;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        WriteOne(reg3);
                                    else
                                        WriteZero(reg3);
                                }
                                break;
                            case OpCodeREnum.Cgt_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg2 = (r + ip->Register3);
                                    reg3 = (r + ip->Register1);
                                    bool res = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = ((uint)reg1->Value > (uint)reg2->Value) || reg2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Long:
                                            res = (ulong)*(long*)&reg1->Value > (ulong)*(long*)&reg2->Value || reg2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&reg1->Value > *(float*)&reg2->Value || reg2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Double:
                                            res = *(double*)&reg1->Value > *(double*)&reg2->Value || reg2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Object:
                                            res = mStack[reg1->Value] != null && (reg2->ObjectType == ObjectTypes.Null || mStack[reg2->Value] == null);
                                            break;
                                        case ObjectTypes.Null:
                                            res = false;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        WriteOne(reg3);
                                    else
                                        WriteZero(reg3);
                                }
                                break;
                            case OpCodeREnum.Cgti_Un:
                                {
                                    reg1 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);
                                    bool res = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = ((uint)reg1->Value > (uint)ip->Operand);
                                            break;
                                        case ObjectTypes.Long:
                                            res = (ulong)*(long*)&reg1->Value > (ulong)ip->OperandLong;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&reg1->Value > ip->OperandFloat;
                                            break;
                                        case ObjectTypes.Double:
                                            res = *(double*)&reg1->Value > ip->OperandDouble;
                                            break;
                                        case ObjectTypes.Object:
                                            res = mStack[reg1->Value] != null && ip->Operand != 0;
                                            break;
                                        case ObjectTypes.Null:
                                            res = false;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        WriteOne(reg3);
                                    else
                                        WriteZero(reg3);
                                }
                                break;
                            #endregion

                            #region Array
                            case OpCodeREnum.Newarr:
                                {
                                    reg2 = (r + ip->Register2);
                                    type = domain.GetType(ip->Operand);
                                    object arr = null;
                                    if (type != null)
                                    {
                                        if (type.TypeForCLR != typeof(ILTypeInstance))
                                        {
                                            if (type is CLRType)
                                            {
                                                arr = ((CLRType)type).CreateArrayInstance(reg2->Value);
                                            }
                                            else
                                            {
                                                arr = Array.CreateInstance(type.TypeForCLR, reg2->Value);
                                            }

                                            //Register Type
                                            AppDomain.GetType(arr.GetType());
                                        }
                                        else
                                        {
                                            arr = new ILTypeInstance[reg2->Value];
                                            ILTypeInstance[] ilArr = (ILTypeInstance[])arr;
                                            if (type.IsValueType)
                                            {
                                                for (int i = 0; i < reg2->Value; i++)
                                                {
                                                    ilArr[i] = ((ILType)type).Instantiate(true);
                                                }
                                            }
                                        }
                                    }
                                    AssignToRegister(ref info, ip->Register1, arr);
                                }
                                break;
                            case OpCodeREnum.Stelem_Ref:
                            case OpCodeREnum.Stelem_Any:
                                {
                                    reg1 = (r + ip->Register3);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register1);

                                    val = GetObjectAndResolveReference(reg1);
                                    Array arr = mStack[reg3->Value] as Array;

                                    if (arr is object[])
                                    {
                                        switch (val->ObjectType)
                                        {
                                            case ObjectTypes.Null:
                                                arr.SetValue(null, reg2->Value);
                                                break;
                                            case ObjectTypes.Object:
                                                ArraySetValue(arr, mStack[val->Value], reg2->Value);
                                                break;
                                            case ObjectTypes.Integer:
                                                arr.SetValue(val->Value, reg2->Value);
                                                break;
                                            case ObjectTypes.Long:
                                                arr.SetValue(*(long*)&val->Value, reg2->Value);
                                                break;
                                            case ObjectTypes.Float:
                                                arr.SetValue(*(float*)&val->Value, reg2->Value);
                                                break;
                                            case ObjectTypes.Double:
                                                arr.SetValue(*(double*)&val->Value, reg2->Value);
                                                break;
                                            case ObjectTypes.ValueTypeObjectReference:
                                                ArraySetValue(arr, StackObject.ToObject(val, domain, mStack), reg2->Value);
                                                FreeStackValueType(esp - 1);
                                                break;
                                            default:
                                                throw new NotImplementedException();
                                        }
                                    }
                                    else
                                    {
                                        switch (val->ObjectType)
                                        {
                                            case ObjectTypes.Object:
                                                ArraySetValue(arr, mStack[val->Value], reg2->Value);
                                                break;
                                            case ObjectTypes.Integer:
                                                {
                                                    StoreIntValueToArray(arr, val, reg2);
                                                }
                                                break;
                                            case ObjectTypes.Long:
                                                {
                                                    if (arr is long[])
                                                    {
                                                        ((long[])arr)[reg2->Value] = *(long*)&val->Value;
                                                    }
                                                    else
                                                    {
                                                        ((ulong[])arr)[reg2->Value] = *(ulong*)&val->Value;
                                                    }
                                                }
                                                break;
                                            case ObjectTypes.Float:
                                                {
                                                    ((float[])arr)[reg2->Value] = *(float*)&val->Value;
                                                }
                                                break;
                                            case ObjectTypes.Double:
                                                {
                                                    ((double[])arr)[reg2->Value] = *(double*)&val->Value;
                                                }
                                                break;
                                            case ObjectTypes.ValueTypeObjectReference:
                                                ArraySetValue(arr, StackObject.ToObject(val, domain, mStack), reg2->Value);
                                                FreeStackValueType(esp - 1);
                                                break;
                                            default:
                                                throw new NotImplementedException();
                                        }
                                    }
                                }
                                break;
                            case OpCodeREnum.Stelem_I1:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register3);

                                    byte[] arr = mStack[reg1->Value] as byte[];
                                    if (arr != null)
                                    {
                                        arr[reg2->Value] = (byte)reg3->Value;
                                    }
                                    else
                                    {
                                        bool[] arr2 = mStack[reg1->Value] as bool[];
                                        if (arr2 != null)
                                        {
                                            arr2[reg2->Value] = reg3->Value == 1;
                                        }
                                        else
                                        {
                                            sbyte[] arr3 = mStack[reg1->Value] as sbyte[];
                                            arr3[reg2->Value] = (sbyte)reg3->Value;
                                        }
                                    }
                                }
                                break;
                            case OpCodeREnum.Stelem_I2:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register3);

                                    short[] arr = mStack[reg1->Value] as short[];
                                    if (arr != null)
                                    {
                                        arr[reg2->Value] = (short)reg3->Value;
                                    }
                                    else
                                    {
                                        ushort[] arr2 = mStack[reg1->Value] as ushort[];
                                        if (arr2 != null)
                                        {
                                            arr2[reg2->Value] = (ushort)reg3->Value;
                                        }
                                        else
                                        {
                                            char[] arr3 = mStack[reg1->Value] as char[];
                                            arr3[reg2->Value] = (char)reg3->Value;
                                        }
                                    }
                                }
                                break;
                            case OpCodeREnum.Stelem_I4:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register3);

                                    int[] arr = mStack[reg1->Value] as int[];
                                    if (arr != null)
                                    {
                                        arr[reg2->Value] = reg3->Value;
                                    }
                                    else
                                    {
                                        uint[] arr2 = mStack[reg1->Value] as uint[];
                                        arr2[reg2->Value] = (uint)reg3->Value;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stelem_R4:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register3);

                                    float[] arr = mStack[reg1->Value] as float[];
                                    arr[reg2->Value] = *(float*)(&reg3->Value);
                                }
                                break;
                            case OpCodeREnum.Stelem_I8:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register3);

                                    long[] arr = mStack[reg1->Value] as long[];
                                    if (arr != null)
                                    {
                                        arr[reg2->Value] = *(long*)(&reg3->Value);
                                    }
                                    else
                                    {
                                        ulong[] arr2 = mStack[reg1->Value] as ulong[];
                                        arr2[reg2->Value] = *(ulong*)(&reg3->Value);
                                    }
                                }
                                break;
                            case OpCodeREnum.Stelem_R8:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register3);

                                    double[] arr = mStack[reg1->Value] as double[];
                                    arr[reg2->Value] = *(double*)(&reg3->Value);
                                }
                                break;
                            case OpCodeREnum.Ldlen:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    Array arr = mStack[reg2->Value] as Array;

                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = arr.Length;
                                }
                                break;
                            case OpCodeREnum.Ldelema:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register3);

                                    Array arr = mStack[reg2->Value] as Array;
                                    intVal = reg3->Value;

                                    reg1->ObjectType = ObjectTypes.ArrayReference;
                                    reg1->Value = GetManagedStackIndex(ref info, ip->Register1);
                                    mStack[reg1->Value] = arr;
                                    reg1->ValueLow = intVal;
                                }
                                break;
                            case OpCodeREnum.Ldelem_Ref:
                            case OpCodeREnum.Ldelem_Any:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register3);
                                    Array arr = mStack[reg2->Value] as Array;
                                    obj = arr.GetValue(reg3->Value);
                                    if (obj is CrossBindingAdaptorType)
                                        obj = ((CrossBindingAdaptorType)obj).ILInstance;

                                    if (obj is ILTypeInstance)
                                    {
                                        ILTypeInstance ins = (ILTypeInstance)obj;
                                        if (!(ins is DelegateAdapter) && ins.Type.IsValueType && !ins.Boxed)
                                        {
                                            AllocValueType(reg1, ins.Type);
                                            dst = ILIntepreter.ResolveReference(reg1);
                                            ins.CopyValueTypeToStack(dst, mStack);
                                        }
                                        else
                                            AssignToRegister(ref info, ip->Register1, obj, true);
                                    }
                                    else
                                        AssignToRegister(ref info, ip->Register1, obj, !arr.GetType().GetElementType().IsPrimitive);
                                }
                                break;
                            case OpCodeREnum.Ldelem_I1:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register3);

                                    bool[] arr = mStack[reg2->Value] as bool[];
                                    if (arr != null)
                                    {
                                        reg1->ObjectType = ObjectTypes.Integer;
                                        reg1->Value = arr[reg3->Value] ? 1 : 0;
                                    }
                                    else
                                    {
                                        sbyte[] arr2 = mStack[reg2->Value] as sbyte[];
                                        reg1->ObjectType = ObjectTypes.Integer;
                                        reg1->Value = arr2[reg3->Value];
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldelem_U1:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register3);

                                    byte[] arr = mStack[reg2->Value] as byte[];
                                    if (arr != null)
                                    {
                                        reg1->ObjectType = ObjectTypes.Integer;
                                        reg1->Value = arr[reg3->Value];
                                    }
                                    else
                                    {
                                        bool[] arr2 = mStack[reg2->Value] as bool[];
                                        reg1->ObjectType = ObjectTypes.Integer;
                                        reg1->Value = arr2[reg3->Value] ? 1 : 0;
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldelem_I2:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register3);

                                    short[] arr = mStack[reg2->Value] as short[];
                                    if (arr != null)
                                    {
                                        reg1->ObjectType = ObjectTypes.Integer;
                                        reg1->Value = arr[reg3->Value];
                                    }
                                    else
                                    {
                                        char[] arr2 = mStack[reg2->Value] as char[];
                                        reg1->ObjectType = ObjectTypes.Integer;
                                        reg1->Value = arr2[reg3->Value];
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldelem_U2:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register3);

                                    ushort[] arr = mStack[reg2->Value] as ushort[];
                                    if (arr != null)
                                    {
                                        reg1->ObjectType = ObjectTypes.Integer;
                                        reg1->Value = arr[reg3->Value];
                                    }
                                    else
                                    {
                                        char[] arr2 = mStack[reg2->Value] as char[];
                                        reg1->ObjectType = ObjectTypes.Integer;
                                        reg1->Value = arr2[reg3->Value];
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldelem_I4:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register3);

                                    int[] arr = mStack[reg2->Value] as int[];
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = arr[reg3->Value];
                                }
                                break;
                            case OpCodeREnum.Ldelem_U4:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register3);

                                    uint[] arr = mStack[reg2->Value] as uint[];
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = (int)arr[reg3->Value];
                                }
                                break;
                            case OpCodeREnum.Ldelem_I8:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register3);

                                    long[] arr = mStack[reg2->Value] as long[];
                                    if (arr != null)
                                    {
                                        reg1->ObjectType = ObjectTypes.Long;
                                        *(long*)(&reg1->Value) = arr[reg3->Value];
                                    }
                                    else
                                    {
                                        ulong[] arr2 = mStack[reg2->Value] as ulong[];
                                        reg1->ObjectType = ObjectTypes.Long;
                                        *(ulong*)(&reg1->Value) = arr2[reg3->Value];
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldelem_R4:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register3);

                                    float[] arr = mStack[reg2->Value] as float[];
                                    reg1->ObjectType = ObjectTypes.Float;
                                    *(float*)&reg1->Value = arr[reg3->Value];
                                }
                                break;
                            case OpCodeREnum.Ldelem_R8:
                                {
                                    reg1 = (r + ip->Register1);
                                    reg2 = (r + ip->Register2);
                                    reg3 = (r + ip->Register3);

                                    double[] arr = mStack[reg2->Value] as double[];
                                    reg1->ObjectType = ObjectTypes.Double;
                                    *(double*)&reg1->Value = arr[reg3->Value];
                                }
                                break;
                            #endregion

                            case OpCodeREnum.Throw:
                                {
                                    objRef = GetObjectAndResolveReference((r + ip->Register1));
                                    var ex = mStack[objRef->Value] as Exception;
                                    throw ex;
                                }
                            case OpCodeREnum.Rethrow:
                                throw lastCaughtEx;
                            default:
                                throw new NotSupportedException("Not supported opcode " + code);
                        }
                        ip++;
                    }
                    catch (Exception ex)
                    {
                        if (ehs != null)
                        {
                            int addr = (int)(ip - ptr);
                            var eh = GetCorrespondingExceptionHandler(ehs, ex, addr, ExceptionHandlerType.Catch, true);

                            if (eh == null)
                            {
                                eh = GetCorrespondingExceptionHandler(ehs, ex, addr, ExceptionHandlerType.Catch, false);
                            }
                            if (eh != null)
                            {
                                if (ex is ILRuntimeException)
                                {
                                    ILRuntimeException ire = (ILRuntimeException)ex;
                                    var inner = ire.InnerException;
                                    inner.Data["ThisInfo"] = ire.ThisInfo;
                                    inner.Data["StackTrace"] = ire.StackTrace;
                                    inner.Data["LocalInfo"] = ire.LocalInfo;
                                    ex = inner;
                                }
                                else
                                {
                                    var debugger = AppDomain.DebugService;
                                    if (method.HasThis)
                                        ex.Data["ThisInfo"] = debugger.GetThisInfo(this);
                                    else
                                        ex.Data["ThisInfo"] = "";
                                    ex.Data["StackTrace"] = debugger.GetStackTrace(this);
                                    ex.Data["LocalInfo"] = debugger.GetLocalVariableInfo(this);
                                }
                                //Clear call stack
                                while (stack.Frames.Peek().BasePointer != frame.BasePointer)
                                {
                                    var f = stack.Frames.Peek();
                                    esp = stack.PopFrame(ref f, esp);
                                    if (f.Method.ReturnType != AppDomain.VoidType)
                                    {
                                        Free(esp - 1);
                                        esp--;
                                    }
                                }
                                lastCaughtEx = ex;
                                short exReg = (short)(paramCnt + locCnt);
                                AssignToRegister(ref info, exReg, ex);
                                unhandledException = false;
                                var sql = from e in ehs
                                          where addr >= e.TryStart && addr <= e.TryEnd && (eh.HandlerStart < e.TryStart || eh.HandlerStart > e.TryEnd) && e.HandlerType == ExceptionHandlerType.Finally
                                          select e;
                                var eh2 = sql.FirstOrDefault();
                                if (eh2 != null)
                                {
                                    finallyEndAddress = eh.HandlerStart;
                                    ip = ptr + eh2.HandlerStart;
                                    continue;
                                }
                                ip = ptr + eh.HandlerStart;
                                continue;
                            }

                            eh = GetCorrespondingExceptionHandler(ehs, null, addr, ExceptionHandlerType.Fault, false);
                            if (eh == null)
                                eh = GetCorrespondingExceptionHandler(ehs, null, addr, ExceptionHandlerType.Finally, false);
                            if (eh != null)
                            {
                                unhandledException = false;
                                finallyEndAddress = -1;
                                lastCaughtEx = ex is ILRuntimeException ? ex : new ILRuntimeException(ex.Message, this, method, ex);
                                ip = ptr + eh.HandlerStart;
                                continue;
                            }
                        }
                        if (unhandledException)
                        {
                            throw ex;
                        }

                        unhandledException = true;
                        returned = true;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                        if (!AppDomain.DebugService.Break(this, ex))
#endif
                        {
                            var newEx = new ILRuntimeException(ex.Message, this, method, ex);
                            throw newEx;
                        }
                    }
                }
            }

#if DEBUG && !NO_PROFILER
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == AppDomain.UnityMainThreadID)
#if UNITY_5_5_OR_NEWER
                UnityEngine.Profiling.Profiler.EndSample();
#else
                UnityEngine.Profiler.EndSample();
#endif
#endif
            //ClearStack
            return stack.PopFrame(ref frame, esp);
        }

        void LoadFromFieldReferenceToRegister(ref RegisterFrameInfo info, object obj, int idx, short reg)
        {
            if (obj is ILTypeInstance)
            {
                ((ILTypeInstance)obj).CopyToRegister(idx, ref info, reg);
            }
            else
            {
                CLRType t = AppDomain.GetType(obj.GetType()) as CLRType;
                StackObject so;
                StackObject* esp = &so;
                var mStack = info.ManagedStack;
                if (!t.CopyFieldToStack(idx, obj, this, ref esp, mStack))
                {
                    AssignToRegister(ref info, reg, t.GetFieldValue(idx, obj));
                }
                else
                {
                    PopToRegister(ref info, reg, esp);
                }
            }
        }

#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal void CopyToRegister(ref RegisterFrameInfo info, short reg, StackObject* val, IList<object> mStackSrc = null)
        {
            var mStack = info.ManagedStack;

            var v = info.RegisterStart + reg;
            var idx = GetManagedStackIndex(ref info, reg);

            if (mStackSrc == null)
                mStackSrc = mStack;
            switch (val->ObjectType)
            {
                case ObjectTypes.Null:
                    v->ObjectType = ObjectTypes.Object;
                    v->Value = idx;
                    mStack[idx] = null;
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var type = info.Intepreter.AppDomain.GetType(val->Value);
                        if (type is ILType)
                        {
                            var st = type as ILType;
                            if (st.IsValueType)
                            {
                                if (v->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                {
                                    var dst = *(StackObject**)&v->Value;
                                    if (dst->Value != st.GetHashCode())
                                    {
                                        stack.FreeRegisterValueType(v);
                                        stack.AllocValueType(v, st, true);
                                    }
                                }
                                st.StaticInstance.CopyToRegister(val->ValueLow, ref info, reg);
                            }
                            else if (st.IsPrimitive)
                            {
                                st.StaticInstance.PushToStack(val->ValueLow, v, info.Intepreter, mStack);
                            }
                            else
                            {
                                v->ObjectType = ObjectTypes.Object;
                                v->Value = idx;
                                mStack[idx] = st.StaticInstance[val->ValueLow];
                            }
                        }
                        else
                        {
                            var st = type as CLRType;
                            var binder = st.ValueTypeBinder;
                            if (binder != null)
                            {
                                if (v->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                {
                                    var dst = *(StackObject**)&v->Value;
                                    if (dst->Value != st.GetHashCode())
                                    {
                                        stack.FreeRegisterValueType(v);
                                        stack.AllocValueType(v, st, true);
                                    }
                                }
                                StackObject tmp;
                                StackObject* esp = &tmp;
                                if (!st.CopyFieldToStack(val->ValueLow, null, this, ref esp, mStack))
                                {
                                    var obj = ((CLRType)type).GetFieldValue(val->ValueLow, null);
                                    if (obj is CrossBindingAdaptorType)
                                        obj = ((CrossBindingAdaptorType)obj).ILInstance;
                                    AssignToRegister(ref info, reg, obj, false);
                                }
                                else
                                {
                                    PopToRegister(ref info, reg, esp);
                                }
                            }
                            else
                            {
                                var obj = st.GetFieldValue(val->ValueLow, null);
                                if (obj is CrossBindingAdaptorType)
                                    obj = ((CrossBindingAdaptorType)obj).ILInstance;
                                v->ObjectType = ObjectTypes.Object;
                                v->Value = idx;
                                mStack[idx] = obj;
                            }
                        }
                    }
                    break;
                case ObjectTypes.Object:
                case ObjectTypes.FieldReference:
                case ObjectTypes.ArrayReference:
                    if (v->ObjectType == ObjectTypes.ValueTypeObjectReference)
                    {
                        var obj = mStackSrc[val->Value];
                        if (obj is ILTypeInstance)
                        {
                            var st = ((ILTypeInstance)obj).Type;
                            //Delegate and enum instance's type is null
                            if (st != null && st.IsValueType)
                            {
                                var dst = *(StackObject**)&v->Value;
                                if (dst->Value != st.GetHashCode())
                                {
                                    stack.FreeRegisterValueType(v);
                                    stack.AllocValueType(v, st, true);
                                }
                                ((ILTypeInstance)obj).CopyValueTypeToStack(dst, mStackSrc);
                            }
                            else
                            {
                                v->ObjectType = ObjectTypes.Object;
                                v->Value = idx;
                                mStack[idx] = obj;
                            }
                        }
                        else
                        {
                            if (obj != null)
                            {
                                var st = domain.GetType(obj.GetType()) as CLRType;
                                var binder = st.ValueTypeBinder;
                                if (binder != null)
                                {
                                    var dst = *(StackObject**)&v->Value;
                                    if (dst->Value != st.GetHashCode())
                                    {
                                        stack.FreeRegisterValueType(v);
                                        stack.AllocValueType(v, st, true);
                                    }
                                    binder.CopyValueTypeToStack(obj, dst, mStackSrc);
                                }
                                else
                                {
                                    v->ObjectType = ObjectTypes.Object;
                                    v->Value = idx;
                                    mStack[idx] = obj;
                                }
                            }
                            else
                            {
                                v->ObjectType = ObjectTypes.Object;
                                v->Value = idx;
                                mStack[idx] = obj;
                            }
                        }
                    }
                    else
                    {
                        *v = *val;
                        mStack[idx] = CheckAndCloneValueType(mStackSrc[v->Value], domain);
                        v->Value = idx;
                    }
                    break;
                case ObjectTypes.ValueTypeObjectReference:
                    if (v->ObjectType == ObjectTypes.ValueTypeObjectReference)
                    {
                        bool noCheck = false;
                        if(!CanCopyStackValueType(val,v))
                        {
                            var dst = *(StackObject**)&val->Value;
                            var ct = domain.GetTypeByIndex(dst->Value);
                            stack.FreeRegisterValueType(v);
                            StackObject* endAddr = null;
                            int start = int.MaxValue, end = 0;
                            stack.CountValueTypeManaged(v, ref start, ref end, &endAddr);
                            noCheck = val <= ResolveReference(v) && val > endAddr;
                            stack.AllocValueType(v, ct, true, noCheck);
                        }
#if DEBUG
                        CopyStackValueType(val, v, mStack, noCheck);
#else
                        CopyStackValueType(val, v, mStack);
#endif
                    }
                    else
                    {
                        if (v >= info.RegisterStart && v < info.RegisterEnd)
                        {
                            var dst = ResolveReference(val);
                            var type = domain.GetTypeByIndex(dst->Value);
                            stack.AllocValueType(v, type, true);
                            CopyStackValueType(val, v, mStack);
                        }
                        else
                            throw new NotImplementedException();
                    }
                    //FreeStackValueType(val);
                    break;
                default:
                    *v = *val;
                    mStack[idx] = null;
                    break;
            }
        }

#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        static int GetManagedStackIndex(ref RegisterFrameInfo info, short reg)
        {
            return info.FrameManagedBase + reg;
        }

        internal static void AssignToRegister(ref RegisterFrameInfo info, short reg, object obj, bool isBox = false)
        {
            var mStack = info.ManagedStack;

            var dst = info.RegisterStart + reg;
            var idx = GetManagedStackIndex(ref info, reg);
            if (obj != null)
            {
            
                if (!isBox)
                {
                    var typeFlags = obj.GetType().GetTypeFlags();

                    if ((typeFlags & CLR.Utils.Extensions.TypeFlags.IsPrimitive) != 0)
                    {
                        UnboxObject(dst, obj, mStack);
                    }
                    else if ((typeFlags & CLR.Utils.Extensions.TypeFlags.IsEnum) != 0)
                    {
                        dst->ObjectType = ObjectTypes.Integer;
                        dst->Value = Convert.ToInt32(obj);
                    }
                    else
                    {
                        dst->ObjectType = ObjectTypes.Object;
                        dst->Value = idx;
                        mStack[idx] = obj;
                    }
                }
                else
                {
                    dst->ObjectType = ObjectTypes.Object;
                    dst->Value = idx;
                    mStack[idx] = obj;
                }
            }
            else
            {
                dst->ObjectType = ObjectTypes.Object;
                dst->Value = idx;
                mStack[idx] = null;
            }
        }

        StackObject* PopToRegister(ref RegisterFrameInfo info, short reg, StackObject* esp)
        {
            var val = esp - 1;
            if (val->ObjectType == ObjectTypes.ValueTypeObjectReference)
            {
                var v = info.RegisterStart + reg;
                if (CanCopyStackValueType(val, v))
                {
                    CopyStackValueType(val, v, info.ManagedStack);
                    Free(val);
                }
                else
                {
                    if(v->ObjectType == ObjectTypes.ValueTypeObjectReference)
                    {
                        stack.FreeRegisterValueType(v);
                    }
                    stack.AllocValueTypeAndCopy(v, val);
                }
            }
            else
            {
                CopyToRegister(ref info, reg, val);
                Free(val);
            }
            return val;
        }

        public static void WriteOne(StackObject* esp)
        {
            esp->ObjectType = ObjectTypes.Integer;
            esp->Value = 1;
        }

        public static void WriteZero(StackObject* esp)
        {
            esp->ObjectType = ObjectTypes.Integer;
            esp->Value = 0;
        }

        internal static void WriteNull(ref RegisterFrameInfo info, short reg)
        {
            var esp = info.RegisterStart + reg;
            int idx = GetManagedStackIndex(ref info, reg);
            esp->ObjectType = ObjectTypes.Object;
            esp->Value = idx;
            esp->ValueLow = 0;
            info.ManagedStack[idx] = null;
        }
    }
}
