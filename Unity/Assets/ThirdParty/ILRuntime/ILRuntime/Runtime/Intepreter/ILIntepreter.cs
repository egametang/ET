using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Stack;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter.OpCodes;
using ILRuntime.Runtime.Debugger;
using ILRuntime.CLR.Utils;
using ILRuntime.Other;

namespace ILRuntime.Runtime.Intepreter
{
    public unsafe class ILIntepreter
    {
        Enviorment.AppDomain domain;
        RuntimeStack stack;
        object _lockObj;
        bool allowUnboundCLRMethod;

        internal RuntimeStack Stack { get { return stack; } }
        public bool ShouldBreak { get; set; }
        public StepTypes CurrentStepType { get; set; }
        public StackObject* LastStepFrameBase { get; set; }
        public int LastStepInstructionIndex { get; set; }
        StackObject* ValueTypeBasePointer;
        public ILIntepreter(Enviorment.AppDomain domain)
        {
            this.domain = domain;
            stack = new RuntimeStack(this);
            allowUnboundCLRMethod = domain.AllowUnboundCLRMethod;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
            _lockObj = new object();
#endif
        }

        public Enviorment.AppDomain AppDomain { get { return domain; } }

        public void Break()
        {
            //Clear old debug state
            ClearDebugState();
            lock (_lockObj)
            {
                Monitor.Wait(_lockObj);
            }
        }

        public void Resume()
        {
            lock (_lockObj)
                Monitor.Pulse(_lockObj);
        }

        public void ClearDebugState()
        {
            ShouldBreak = false;
            CurrentStepType = StepTypes.None;
            LastStepFrameBase = (StackObject*)0;
            LastStepInstructionIndex = 0;
        }
        public object Run(ILMethod method, object instance, object[] p)
        {
            IList<object> mStack = stack.ManagedStack;
            int mStackBase = mStack.Count;
            StackObject* esp = stack.StackBase;
            stack.ResetValueTypePointer();
            if (method.HasThis)
            {
                if (instance is CrossBindingAdaptorType)
                    instance = ((CrossBindingAdaptorType)instance).ILInstance;
                if (instance == null)
                    throw new NullReferenceException("instance should not be null!");
                esp = PushObject(esp, mStack, instance);
            }
            esp = PushParameters(method, esp, p);
            bool unhandledException;
            esp = Execute(method, esp, out unhandledException);
            object result = method.ReturnType != domain.VoidType ? method.ReturnType.TypeForCLR.CheckCLRTypes(StackObject.ToObject((esp - 1), domain, mStack)) : null;
            //ClearStack
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
            ((List<object>)mStack).RemoveRange(mStackBase, mStack.Count - mStackBase);
#else
            ((UncheckedList<object>)mStack).RemoveRange(mStackBase, mStack.Count - mStackBase);
#endif
            return result;
        }
        internal StackObject* Execute(ILMethod method, StackObject* esp, out bool unhandledException)
        {
            if (method == null)
                throw new NullReferenceException();
#if UNITY_EDITOR
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == AppDomain.UnityMainThreadID)

#if UNITY_5_5_OR_NEWER
                UnityEngine.Profiling.Profiler.BeginSample(method.ToString());
#else
                UnityEngine.Profiler.BeginSample(method.ToString());
#endif

#endif
            OpCode[] body = method.Body;
            StackFrame frame;
            stack.InitializeFrame(method, esp, out frame);
            StackObject* v1 = frame.LocalVarPointer;
            StackObject* v2 = frame.LocalVarPointer + 1;
            StackObject* v3 = frame.LocalVarPointer + 1 + 1;
            StackObject* v4 = Add(frame.LocalVarPointer, 3);
            int finallyEndAddress = 0;

            esp = frame.BasePointer;
            StackObject* arg = Minus(frame.LocalVarPointer, method.ParameterCount);
            IList<object> mStack = stack.ManagedStack;
            int paramCnt = method.ParameterCount;
            if (method.HasThis)//this parameter is always object reference
            {
                arg--;
                paramCnt++;
            }
            unhandledException = false;

            //Managed Stack reserved for arguments(In case of starg)
            for (int i = 0; i < paramCnt; i++)
            {
                var a = Add(arg, i);
                switch (a->ObjectType)
                {
                    case ObjectTypes.Null:
                        //Need to reserve place for null, in case of starg
                        a->ObjectType = ObjectTypes.Object;
                        a->Value = mStack.Count;
                        mStack.Add(null);
                        break;
                    case ObjectTypes.ValueTypeObjectReference:
                        CloneStackValueType(a, a, mStack);
                        break;
                    case ObjectTypes.Object:
                    case ObjectTypes.FieldReference:
                    case ObjectTypes.ArrayReference:
                        frame.ManagedStackBase--;
                        break;
                }
            }

            stack.PushFrame(ref frame);

            int locBase = mStack.Count;
            //Managed Stack reserved for local variable
            for (int i = 0; i < method.LocalVariableCount; i++)
            {
                mStack.Add(null);
            }

            for (int i = 0; i < method.LocalVariableCount; i++)
            {
                var v = method.Variables[i];
                if (v.VariableType.IsValueType && !v.VariableType.IsPrimitive)
                {
                    var t = AppDomain.GetType(v.VariableType, method.DeclearingType, method);
                    if (t is ILType)
                    {
                        //var obj = ((ILType)t).Instantiate(false);
                        var loc = Add(v1, i);
                        stack.AllocValueType(loc, t);

                        /*loc->ObjectType = ObjectTypes.Object;
                        loc->Value = mStack.Count;
                        mStack.Add(obj);*/

                    }
                    else
                    {
                        CLRType cT = (CLRType)t;
                        var loc = Add(v1, i);
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
                    if (v.VariableType.IsPrimitive)
                    {
                        var t = AppDomain.GetType(v.VariableType, method.DeclearingType, method);
                        var loc = Add(v1, i);
                        StackObject.Initialized(loc, t);
                    }
                    else
                    {
                        var loc = Add(v1, i);
                        loc->ObjectType = ObjectTypes.Object;
                        loc->Value = locBase + i;
                    }
                }
            }
            var bp = stack.ValueTypeStackPointer;
            ValueTypeBasePointer = bp;
            fixed (OpCode* ptr = body)
            {
                OpCode* ip = ptr;
                OpCodeEnum code = ip->Code;
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
                            case OpCodeEnum.Ldarg_0:
                                CopyToStack(esp, arg, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Ldarg_1:
                                CopyToStack(esp, arg + 1, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Ldarg_2:
                                CopyToStack(esp, arg + 1 + 1, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Ldarg_3:
                                CopyToStack(esp, arg + 1 + 1 + 1, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Ldarg:
                            case OpCodeEnum.Ldarg_S:
                                CopyToStack(esp, Add(arg, ip->TokenInteger), mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Ldarga:
                            case OpCodeEnum.Ldarga_S:
                                {
                                    var a = Add(arg, ip->TokenInteger);
                                    esp->ObjectType = ObjectTypes.StackObjectReference;
                                    *(StackObject**)&esp->Value = a;
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Starg:
                            case OpCodeEnum.Starg_S:
                                {
                                    var a = Add(arg, ip->TokenInteger);
                                    var val = esp - 1;
                                    int idx = a->Value;
                                    bool isObj = a->ObjectType >= ObjectTypes.Object;
                                    if (val->ObjectType >= ObjectTypes.Object)
                                    {
                                        if (a->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                        {
                                            var dst = *((StackObject**)&a->Value);
                                            CopyValueTypeToStack(dst, mStack[val->Value], mStack);
                                        }
                                        else
                                        {
                                            a->ObjectType = val->ObjectType;
                                            mStack[a->Value] = mStack[val->Value];
                                            a->ValueLow = val->ValueLow;
                                        }
                                    }
                                    else
                                    {
                                        if (a->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                        {
                                            if (val->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                            {
                                                CopyStackValueType(val, a, mStack);
                                                FreeStackValueType(val);
                                            }
                                            else
                                                throw new NotSupportedException();
                                        }
                                        else
                                        {
                                            *a = *val;
                                            if (isObj)
                                            {
                                                a->Value = idx;
                                                if (val->ObjectType == ObjectTypes.Null)
                                                {
                                                    mStack[a->Value] = null;
                                                }
                                            }
                                        }
                                    }
                                    Free(val);
                                    esp--;
                                }
                                break;
                            case OpCodeEnum.Stloc_0:
                                {
                                    esp--;
                                    int idx = locBase;
                                    StLocSub(esp, v1, bp, idx, mStack);
                                }
                                break;
                            case OpCodeEnum.Ldloc_0:
                                CopyToStack(esp, v1, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Stloc_1:
                                {
                                    esp--;
                                    int idx = locBase + 1;
                                    StLocSub(esp, v2, bp, idx, mStack);
                                }
                                break;
                            case OpCodeEnum.Ldloc_1:
                                CopyToStack(esp, v2, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Stloc_2:
                                {
                                    esp--;
                                    int idx = locBase + 2;
                                    StLocSub(esp, v3, bp, idx, mStack);
                                    break;
                                }
                            case OpCodeEnum.Ldloc_2:
                                CopyToStack(esp, v3, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Stloc_3:
                                {
                                    esp--;
                                    int idx = locBase + 3;

                                    StLocSub(esp, v4, bp, idx, mStack);
                                }
                                break;
                            case OpCodeEnum.Ldloc_3:
                                CopyToStack(esp, v4, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Stloc:
                            case OpCodeEnum.Stloc_S:
                                {
                                    esp--;
                                    var v = Add(frame.LocalVarPointer, ip->TokenInteger);
                                    int idx = locBase + ip->TokenInteger;
                                    StLocSub(esp, v, bp, idx, mStack);
                                }
                                break;
                            case OpCodeEnum.Ldloc:
                            case OpCodeEnum.Ldloc_S:
                                {
                                    var v = Add(frame.LocalVarPointer, ip->TokenInteger);
                                    CopyToStack(esp, v, mStack);
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Ldloca:
                            case OpCodeEnum.Ldloca_S:
                                {
                                    var v = Add(frame.LocalVarPointer, ip->TokenInteger);
                                    esp->ObjectType = ObjectTypes.StackObjectReference;
                                    *(StackObject**)&esp->Value = v;
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Ldobj:
                                {
                                    var objRef = esp - 1;
                                    switch (objRef->ObjectType)
                                    {
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var t = AppDomain.GetType(ip->TokenInteger);
                                                var obj = mStack[objRef->Value];
                                                var idx = objRef->ValueLow;
                                                Free(objRef);
                                                LoadFromArrayReference(obj, idx, objRef, t, mStack);
                                            }
                                            break;
                                        case ObjectTypes.StackObjectReference:
                                            {
                                                var obj = GetObjectAndResolveReference(objRef);
                                                *objRef = *obj;
                                                if (objRef->ObjectType >= ObjectTypes.Object)
                                                {
                                                    objRef->Value = mStack.Count;
                                                    mStack.Add(mStack[obj->Value]);
                                                }
                                            }
                                            break;
                                        case ObjectTypes.FieldReference:
                                            {
                                                var obj = mStack[objRef->Value];
                                                int idx = objRef->ValueLow;
                                                Free(objRef);
                                                if (obj is ILTypeInstance)
                                                {
                                                    ((ILTypeInstance)obj).PushToStack(idx, objRef, AppDomain, mStack);
                                                }
                                                else
                                                {
                                                    var t = AppDomain.GetType(ip->TokenInteger);
                                                    obj = ((CLRType) t).GetFieldValue(idx, obj);
                                                    PushObject(objRef, mStack, obj);
                                                }
                                            }
                                            break;
                                        case ObjectTypes.StaticFieldReference:
                                            {
                                                var t = AppDomain.GetType(objRef->Value);
                                                int idx = objRef->ValueLow;
                                                Free(objRef);
                                                if (t is ILType)
                                                {
                                                    ((ILType)t).StaticInstance.PushToStack(idx, objRef, AppDomain, mStack);
                                                }
                                                else
                                                {
                                                    var obj = ((CLRType)t).GetFieldValue(idx, null);
                                                    PushObject(objRef, mStack, obj);
                                                }
                                            }
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeEnum.Stobj:
                                {
                                    var objRef = esp - 1 - 1;
                                    var val = esp - 1;
                                    switch (objRef->ObjectType)
                                    {
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var t = AppDomain.GetType(ip->TokenInteger);
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
                                                            StackObject* dst = *(StackObject**)&objRef->Value;
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
                                                var obj = mStack[objRef->Value];
                                                int idx = objRef->ValueLow;
                                                if (obj is ILTypeInstance)
                                                {
                                                    ((ILTypeInstance)obj).AssignFromStack(idx, val, AppDomain, mStack);
                                                }
                                                else
                                                {
                                                    var t = AppDomain.GetType(ip->TokenInteger);
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
                                                    ((CLRType)t).SetStaticFieldValue(objRef->ValueLow, t.TypeForCLR.CheckCLRTypes(StackObject.ToObject(val, AppDomain, mStack)));
                                                }
                                            }
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);
                                    esp = esp - 1 - 1;
                                }
                                break;
                            #endregion

                            #region Load Constants
                            case OpCodeEnum.Ldc_I4_M1:
                                esp->Value = -1;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_0:
                                esp->Value = 0;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_1:
                                esp->Value = 1;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_2:
                                esp->Value = 2;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_3:
                                esp->Value = 3;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_4:
                                esp->Value = 4;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_5:
                                esp->Value = 5;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_6:
                                esp->Value = 6;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_7:
                                esp->Value = 7;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_8:
                                esp->Value = 8;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4:
                            case OpCodeEnum.Ldc_I4_S:
                                esp->Value = ip->TokenInteger;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I8:
                                {
                                    *(long*)(&esp->Value) = ip->TokenLong;
                                    esp->ObjectType = ObjectTypes.Long;
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Ldc_R4:
                                {
                                    *(float*)(&esp->Value) = *(float*)&ip->TokenInteger;
                                    esp->ObjectType = ObjectTypes.Float;
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Ldc_R8:
                                {
                                    *(double*)(&esp->Value) = *(double*)&ip->TokenLong;
                                    esp->ObjectType = ObjectTypes.Double;
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Ldnull:
                                {
                                    esp = PushNull(esp);
                                }
                                break;
                            case OpCodeEnum.Ldind_I:
                            case OpCodeEnum.Ldind_I1:
                            case OpCodeEnum.Ldind_I2:
                            case OpCodeEnum.Ldind_I4:
                            case OpCodeEnum.Ldind_U1:
                            case OpCodeEnum.Ldind_U2:
                            case OpCodeEnum.Ldind_U4:
                                {
                                    var val = GetObjectAndResolveReference(esp - 1);
                                    var dst = esp - 1;
                                    switch (val->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(dst);
                                                LoadFromFieldReference(instance, idx, dst, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(dst);
                                                LoadFromArrayReference(instance, idx, dst, instance.GetType().GetElementType(), mStack);
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
                            case OpCodeEnum.Ldind_I8:
                                {
                                    var val = GetObjectAndResolveReference(esp - 1);
                                    var dst = esp - 1;
                                    switch (val->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(dst);
                                                LoadFromFieldReference(instance, idx, dst, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(dst);
                                                LoadFromArrayReference(instance, idx, dst, instance.GetType().GetElementType(), mStack);
                                            }
                                            break;
                                        default:
                                            {
                                                *dst = *val;
                                                dst->ObjectType = ObjectTypes.Long;
                                                dst->ValueLow = 0;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeEnum.Ldind_R4:
                                {
                                    var val = GetObjectAndResolveReference(esp - 1);
                                    var dst = esp - 1;
                                    switch (val->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(dst);
                                                LoadFromFieldReference(instance, idx, dst, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(dst);
                                                LoadFromArrayReference(instance, idx, dst, instance.GetType().GetElementType(), mStack);
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
                            case OpCodeEnum.Ldind_R8:
                                {
                                    var val = GetObjectAndResolveReference(esp - 1);
                                    var dst = esp - 1;
                                    switch (val->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(dst);
                                                LoadFromFieldReference(instance, idx, dst, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(dst);
                                                LoadFromArrayReference(instance, idx, dst, instance.GetType().GetElementType(), mStack);
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
                            case OpCodeEnum.Ldind_Ref:
                                {
                                    var val = GetObjectAndResolveReference(esp - 1);
                                    var dst = esp - 1;
                                    switch (val->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(dst);
                                                LoadFromFieldReference(instance, idx, dst, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(dst);
                                                LoadFromArrayReference(instance, idx, dst, instance.GetType().GetElementType(), mStack);
                                            }
                                            break;
                                        default:
                                            {
                                                dst->ObjectType = ObjectTypes.Object;
                                                dst->Value = mStack.Count;
                                                mStack.Add(mStack[val->Value]);
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeEnum.Stind_I:
                            case OpCodeEnum.Stind_I1:
                            case OpCodeEnum.Stind_I2:
                            case OpCodeEnum.Stind_I4:
                            case OpCodeEnum.Stind_R4:
                                {
                                    var dst = GetObjectAndResolveReference(esp - 1 - 1);
                                    var val = esp - 1;
                                    switch (dst->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                StoreValueToFieldReference(mStack[dst->Value], dst->ValueLow, val, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                StoreValueToArrayReference(dst, val, mStack[dst->Value].GetType().GetElementType(), mStack);
                                            }
                                            break;
                                        default:
                                            {
                                                dst->Value = val->Value;
                                            }
                                            break;
                                    }
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);
                                    esp = esp - 1 - 1;
                                }
                                break;
                            case OpCodeEnum.Stind_I8:
                                {
                                    var dst = GetObjectAndResolveReference(esp - 1 - 1);
                                    var val = esp - 1;
                                    switch (dst->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                StoreValueToFieldReference(mStack[dst->Value], dst->ValueLow, val, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                StoreValueToArrayReference(dst, val, typeof(long), mStack);
                                            }
                                            break;
                                        default:
                                            {
                                                dst->Value = val->Value;
                                                dst->ValueLow = val->ValueLow;
                                            }
                                            break;
                                    }

                                    Free(esp - 1);
                                    Free(esp - 1 - 1);
                                    esp = esp - 1 - 1;
                                }
                                break;
                            case OpCodeEnum.Stind_R8:
                                {
                                    var dst = GetObjectAndResolveReference(esp - 1 - 1);
                                    var val = esp - 1;
                                    switch (dst->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                StoreValueToFieldReference(mStack[dst->Value], dst->ValueLow, val, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                StoreValueToArrayReference(dst, val, typeof(double), mStack);
                                            }
                                            break;
                                        default:
                                            {
                                                dst->Value = val->Value;
                                                dst->ValueLow = val->ValueLow;
                                            }
                                            break;
                                    }

                                    Free(esp - 1);
                                    Free(esp - 1 - 1);
                                    esp = esp - 1 - 1;
                                }
                                break;
                            case OpCodeEnum.Stind_Ref:
                                {
                                    var dst = GetObjectAndResolveReference(esp - 1 - 1);
                                    var val = esp - 1;
                                    switch (dst->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                StoreValueToFieldReference(mStack[dst->Value], dst->ValueLow, val, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                StoreValueToArrayReference(dst, val, typeof(object), mStack);
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

                                    Free(esp - 1);
                                    Free(esp - 1 - 1);
                                    esp = esp - 1 - 1;
                                }
                                break;
                            case OpCodeEnum.Ldstr:
                                esp = PushObject(esp, mStack, AppDomain.GetString(ip->TokenLong));
                                break;
                            #endregion

                            #region Althemetics
                            case OpCodeEnum.Add:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 1 - 1;
                                    esp = a;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            *((long*)&esp->Value) = *((long*)&a->Value) + *((long*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->Value = a->Value + b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            *((float*)&esp->Value) = *((float*)&a->Value) + *((float*)&b->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            *((double*)&esp->Value) = *((double*)&a->Value) + *((double*)&b->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Sub:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 1 - 1;
                                    esp = a;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            *((long*)&esp->Value) = *((long*)&a->Value) - *((long*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->Value = a->Value - b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            *((float*)&esp->Value) = *((float*)&a->Value) - *((float*)&b->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            *((double*)&esp->Value) = *((double*)&a->Value) - *((double*)&b->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Mul:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 1 - 1;
                                    esp = a;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            *((long*)&esp->Value) = *((long*)&a->Value) * *((long*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->Value = a->Value * b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            *((float*)&esp->Value) = *((float*)&a->Value) * *((float*)&b->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            *((double*)&esp->Value) = *((double*)&a->Value) * *((double*)&b->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Div:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 1 - 1;
                                    esp = a;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            *((long*)&esp->Value) = *((long*)&a->Value) / *((long*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->Value = a->Value / b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            *((float*)&esp->Value) = *((float*)&a->Value) / *((float*)&b->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            *((double*)&esp->Value) = *((double*)&a->Value) / *((double*)&b->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Div_Un:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 1 - 1;
                                    esp = a;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            *((ulong*)&esp->Value) = *((ulong*)&a->Value) / *((ulong*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->Value = (int)((uint)a->Value / (uint)b->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Rem:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 1 - 1;
                                    esp = a;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            *((long*)&esp->Value) = *((long*)&a->Value) % *((long*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->Value = a->Value % b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            *(float*)&esp->Value = *(float*)&a->Value % *(float*)&b->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            *(double*)&esp->Value = *(double*)&a->Value % *(double*)&b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Rem_Un:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 1 - 1;
                                    esp = a;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            *((ulong*)&esp->Value) = *((ulong*)&a->Value) % *((ulong*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->Value = (int)((uint)a->Value % (uint)b->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Xor:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 1 - 1;
                                    esp = a;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            *((long*)&esp->Value) = *((long*)&a->Value) ^ *((long*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->Value = a->Value ^ b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.And:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 1 - 1;
                                    esp = a;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            *((long*)&esp->Value) = *((long*)&a->Value) & *((long*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->Value = a->Value & b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Or:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 1 - 1;
                                    esp = a;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            *((long*)&esp->Value) = *((long*)&a->Value) | *((long*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->Value = a->Value | b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Shl:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 1 - 1;
                                    esp = a;
                                    int bits = b->Value;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            *((long*)&esp->Value) = *((long*)&a->Value) << bits;
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->Value = a->Value << bits;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Shr:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 1 - 1;
                                    esp = a;
                                    int bits = b->Value;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            *((long*)&esp->Value) = *((long*)&a->Value) >> bits;
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->Value = a->Value >> bits;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Shr_Un:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 1 - 1;
                                    esp = a;
                                    int bits = b->Value;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            *((ulong*)&esp->Value) = *((ulong*)&a->Value) >> bits;
                                            break;
                                        case ObjectTypes.Integer:
                                            *(uint*)&esp->Value = (uint)a->Value >> bits;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Not:
                                {
                                    StackObject* a = esp - 1;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            *((long*)&a->Value) = ~*((long*)&a->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            a->Value = ~a->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeEnum.Neg:
                                {
                                    StackObject* a = esp - 1;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            *((long*)&a->Value) = -*((long*)&a->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            a->Value = -a->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            *((float*)&a->Value) = -*((float*)&a->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            *((double*)&a->Value) = -*((double*)&a->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            #endregion

                            #region Control Flows
                            case OpCodeEnum.Ret:
                                returned = true;
                                break;
                            case OpCodeEnum.Brtrue:
                            case OpCodeEnum.Brtrue_S:
                                {
                                    esp--;
                                    bool res = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = esp->Value != 0;
                                            break;
                                        case ObjectTypes.Long:
                                            res = *(long*)&esp->Value != 0;
                                            break;
                                        case ObjectTypes.Object:
                                            res = mStack[esp->Value] != null;
                                            break;
                                    }
                                    if (res)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        Free(esp);
                                        continue;
                                    }
                                    else
                                        Free(esp);
                                }
                                break;
                            case OpCodeEnum.Brfalse:
                            case OpCodeEnum.Brfalse_S:
                                {
                                    esp--;
                                    bool res = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Null:
                                            res = true;
                                            break;
                                        case ObjectTypes.Integer:
                                            res = esp->Value == 0;
                                            break;
                                        case ObjectTypes.Long:
                                            res = *(long*)&esp->Value == 0;
                                            break;
                                        case ObjectTypes.Object:
                                            res = mStack[esp->Value] == null;
                                            Free(esp);
                                            break;
                                        default:
                                            Free(esp);
                                            break;
                                    }
                                    if (res)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }
                                }
                                break;
                            case OpCodeEnum.Beq:
                            case OpCodeEnum.Beq_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 1 - 1;
                                    bool transfer = false;
                                    if (a->ObjectType == b->ObjectType)
                                    {
                                        switch (a->ObjectType)
                                        {
                                            case ObjectTypes.Null:
                                                transfer = true;
                                                break;
                                            case ObjectTypes.Integer:
                                                transfer = a->Value == b->Value;
                                                break;
                                            case ObjectTypes.Long:
                                                transfer = *(long*)&a->Value == *(long*)&b->Value;
                                                break;
                                            case ObjectTypes.Float:
                                                transfer = *(float*)&a->Value == *(float*)&b->Value;
                                                break;
                                            case ObjectTypes.Double:
                                                transfer = *(double*)&a->Value == *(double*)&b->Value;
                                                break;
                                            case ObjectTypes.Object:
                                                transfer = mStack[a->Value] == mStack[b->Value];
                                                break;
                                            default:
                                                throw new NotImplementedException();
                                        }
                                    }
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);
                                    esp = esp - 1 - 1;
                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Bne_Un:
                            case OpCodeEnum.Bne_Un_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 1 - 1;
                                    bool transfer = false;
                                    if (a->ObjectType == b->ObjectType)
                                    {
                                        switch (a->ObjectType)
                                        {
                                            case ObjectTypes.Null:
                                                transfer = false;
                                                break;
                                            case ObjectTypes.Integer:
                                                transfer = (uint)a->Value != (uint)b->Value;
                                                break;
                                            case ObjectTypes.Float:
                                                transfer = *(float*)&a->Value != *(float*)&b->Value;
                                                break;
                                            case ObjectTypes.Long:
                                                transfer = *(long*)&a->Value != *(long*)&b->Value;
                                                break;
                                            case ObjectTypes.Double:
                                                transfer = *(double*)&a->Value != *(double*)&b->Value;
                                                break;
                                            case ObjectTypes.Object:
                                                transfer = mStack[a->Value] != mStack[b->Value];
                                                break;
                                            default:
                                                throw new NotImplementedException();
                                        }
                                    }
                                    else
                                        transfer = true;
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);
                                    esp = esp - 1 - 1;
                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Bgt:
                            case OpCodeEnum.Bgt_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 1 - 1;
                                    esp = esp - 1 - 1;
                                    bool transfer = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = a->Value > b->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&a->Value > *(long*)&b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&a->Value > *(float*)&b->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&a->Value > *(double*)&b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Bgt_Un:
                            case OpCodeEnum.Bgt_Un_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 1 - 1;
                                    esp = esp - 1 - 1;
                                    bool transfer = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)a->Value > (uint)b->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(ulong*)&a->Value > *(ulong*)&b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&a->Value > *(float*)&b->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&a->Value > *(double*)&b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Bge:
                            case OpCodeEnum.Bge_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 1 - 1;
                                    esp = esp - 1 - 1;
                                    bool transfer = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = a->Value >= b->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&a->Value >= *(long*)&b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&a->Value >= *(float*)&b->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&a->Value >= *(double*)&b->Value;
                                            break;

                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Bge_Un:
                            case OpCodeEnum.Bge_Un_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 1 - 1;
                                    esp = esp - 1 - 1;
                                    bool transfer = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)a->Value >= (uint)b->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(ulong*)&a->Value >= *(ulong*)&b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&a->Value >= *(float*)&b->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&a->Value >= *(double*)&b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Blt:
                            case OpCodeEnum.Blt_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 1 - 1;
                                    esp = esp - 1 - 1;
                                    bool transfer = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = a->Value < b->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&a->Value < *(long*)&b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&a->Value < *(float*)&b->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&a->Value < *(double*)&b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Blt_Un:
                            case OpCodeEnum.Blt_Un_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 1 - 1;
                                    esp = esp - 1 - 1;
                                    bool transfer = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)a->Value < (uint)b->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(ulong*)&a->Value < *(ulong*)&b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&a->Value < *(float*)&b->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&a->Value < *(double*)&b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Ble:
                            case OpCodeEnum.Ble_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 1 - 1;
                                    esp = esp - 1 - 1;
                                    bool transfer = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = a->Value <= b->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&a->Value <= *(long*)&b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&a->Value <= *(float*)&b->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&a->Value <= *(double*)&b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Ble_Un:
                            case OpCodeEnum.Ble_Un_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 1 - 1;
                                    esp = esp - 1 - 1;
                                    bool transfer = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)a->Value <= (uint)b->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(ulong*)&a->Value <= *(ulong*)&b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&a->Value <= *(float*)&b->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&a->Value <= *(double*)&b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Br_S:
                            case OpCodeEnum.Br:
                                ip = ptr + ip->TokenInteger;
                                continue;
                            case OpCodeEnum.Switch:
                                {
                                    var val = (esp - 1)->Value;
                                    Free(esp - 1);
                                    esp--;
                                    var table = method.JumpTables[ip->TokenInteger];
                                    if (val >= 0 && val < table.Length)
                                    {
                                        ip = ptr + table[val];
                                        continue;
                                    }
                                }
                                break;
                            case OpCodeEnum.Leave:
                            case OpCodeEnum.Leave_S:
                                {
                                    if (method.ExceptionHandler != null)
                                    {
                                        ExceptionHandler eh = null;

                                        int addr = ip->TokenInteger;
                                        var sql = from e in method.ExceptionHandler
                                                  where addr == e.HandlerEnd + 1 && e.HandlerType == ExceptionHandlerType.Finally || e.HandlerType == ExceptionHandlerType.Fault
                                                  select e;
                                        eh = sql.FirstOrDefault();
                                        if (eh != null)
                                        {
                                            finallyEndAddress = ip->TokenInteger;
                                            ip = ptr + eh.HandlerStart;
                                            continue;
                                        }
                                    }
                                    ip = ptr + ip->TokenInteger;
                                    continue;
                                }
                            case OpCodeEnum.Endfinally:
                                {
                                    ip = ptr + finallyEndAddress;
                                    finallyEndAddress = 0;
                                    continue;
                                }
                            case OpCodeEnum.Call:
                            case OpCodeEnum.Callvirt:
                                {
                                    IMethod m = domain.GetMethod(ip->TokenInteger);
                                    if (m == null)
                                    {
                                        //Irrelevant method
                                        int cnt = (int)ip->TokenLong;
                                        //Balance the stack
                                        for (int i = 0; i < cnt; i++)
                                        {
                                            Free(esp - 1);
                                            esp--;
                                        }
                                    }
                                    else
                                    {
                                        if (m is ILMethod)
                                        {
                                            ILMethod ilm = (ILMethod)m;
                                            bool processed = false;
                                            if (m.IsDelegateInvoke)
                                            {
                                                var instance = StackObject.ToObject((Minus(esp, m.ParameterCount + 1)), domain, mStack);
                                                if (instance is IDelegateAdapter)
                                                {
                                                    esp = ((IDelegateAdapter)instance).ILInvoke(this, esp, mStack);
                                                    processed = true;
                                                }
                                            }
                                            if (!processed)
                                            {
                                                if (code == OpCodeEnum.Callvirt)
                                                {
                                                    var objRef = GetObjectAndResolveReference(Minus(esp, ilm.ParameterCount + 1));
                                                    if (objRef->ObjectType == ObjectTypes.Null)
                                                        throw new NullReferenceException();
                                                    if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                                    {
                                                        StackObject* dst = *(StackObject**)&objRef->Value;
                                                        var ft = domain.GetType(dst->Value) as ILType;
                                                        ilm = ft.GetVirtualMethod(ilm) as ILMethod;
                                                    }
                                                    else
                                                    {
                                                        var obj = mStack[objRef->Value];
                                                        if (obj == null)
                                                            throw new NullReferenceException();
                                                        ilm = ((ILTypeInstance)obj).Type.GetVirtualMethod(ilm) as ILMethod;
                                                    }                                                    
                                                }
                                                esp = Execute(ilm, esp, out unhandledException);
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
                                                var instance = StackObject.ToObject((Minus(esp, cm.ParameterCount + 1)), domain, mStack);
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
#if UNITY_EDITOR
                                                    if (System.Threading.Thread.CurrentThread.ManagedThreadId == AppDomain.UnityMainThreadID)

#if UNITY_5_5_OR_NEWER
                                                        UnityEngine.Profiling.Profiler.BeginSample(cm.ToString());
#else
                                                        UnityEngine.Profiler.BeginSample(cm.ToString());
#endif
#endif
                                                    object result = cm.Invoke(this, esp, mStack);
#if UNITY_EDITOR
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
                                                        Free(Minus(esp, i));
                                                    }
                                                    esp = Minus(esp, paramCount);
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

                                    }
                                }
                                break;
                            #endregion

                            #region FieldOperation
                            case OpCodeEnum.Stfld:
                                {
                                    var objRef = GetObjectAndResolveReference(esp - 1 - 1);
                                    if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                    {
                                        StackObject* dst = *(StackObject**)&objRef->Value;
                                        var ft = domain.GetType(dst->Value);
                                        if (ft is ILType)
                                            CopyToValueTypeField(dst, (int)ip->TokenLong, esp - 1, mStack);
                                        else
                                            CopyToValueTypeField(dst, ((CLRType)ft).FieldIndexMapping[(int)ip->TokenLong], esp - 1, mStack);
                                    }
                                    else
                                    {
                                        object obj = RetriveObject(objRef, mStack);

                                        if (obj != null)
                                        {
                                            if (obj is ILTypeInstance)
                                            {
                                                ILTypeInstance instance = obj as ILTypeInstance;
                                                StackObject* val = esp - 1;
                                                instance.AssignFromStack((int)ip->TokenLong, val, AppDomain, mStack);
                                            }
                                            else
                                            {
                                                var t = obj.GetType();
                                                var type = AppDomain.GetType((int)(ip->TokenLong >> 32));
                                                if (type != null)
                                                {
                                                    var val = esp - 1;
                                                    var fieldToken = (int)ip->TokenLong;
                                                    var f = ((CLRType)type).GetField(fieldToken);
                                                    ((CLRType)type).SetFieldValue(fieldToken, ref obj, f.FieldType.CheckCLRTypes(CheckAndCloneValueType(StackObject.ToObject(val, domain, mStack), domain)));
                                                    //Writeback
                                                    if (t.IsValueType)
                                                    {
                                                        switch (objRef->ObjectType)
                                                        {
                                                            case ObjectTypes.Object:
                                                                break;
                                                            case ObjectTypes.FieldReference:
                                                                {
                                                                    var oldObj = mStack[objRef->Value];
                                                                    int idx = objRef->ValueLow;
                                                                    if (oldObj is ILTypeInstance)
                                                                    {
                                                                        ((ILTypeInstance)oldObj)[idx] = obj;
                                                                    }
                                                                    else
                                                                    {
                                                                        var it = AppDomain.GetType(oldObj.GetType());
                                                                        ((CLRType)it).SetFieldValue(idx, ref oldObj, obj);
                                                                    }
                                                                }
                                                                break;
                                                            case ObjectTypes.StaticFieldReference:
                                                                {
                                                                    var it = AppDomain.GetType(objRef->Value);
                                                                    int idx = objRef->ValueLow;
                                                                    if (it is ILType)
                                                                    {
                                                                        ((ILType)it).StaticInstance[idx] = obj;
                                                                    }
                                                                    else
                                                                    {
                                                                        ((CLRType)it).SetStaticFieldValue(idx, obj);
                                                                    }
                                                                }
                                                                break;
                                                            case ObjectTypes.ValueTypeObjectReference:
                                                                {
                                                                    var dst = *(StackObject**)&objRef->Value;
                                                                    var ct = domain.GetType(dst->Value) as CLRType;
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
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);
                                    esp = esp - 1 - 1;
                                }
                                break;
                            case OpCodeEnum.Ldfld:
                                {
                                    var ret = esp - 1;
                                    StackObject* objRef = GetObjectAndResolveReference(ret);
                                    if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                    {
                                        var dst = *(StackObject**)&objRef->Value;
                                        var ft = domain.GetType(dst->Value);
                                        if (ft is ILType)
                                            dst = Minus(dst, (int)ip->TokenLong + 1);
                                        else
                                            dst = Minus(dst, ((CLRType)ft).FieldIndexMapping[(int)ip->TokenLong] + 1);
                                        CopyToStack(ret, dst, mStack);
                                    }
                                    else
                                    {
                                        object obj = RetriveObject(objRef, mStack);
                                        Free(ret);
                                        if (obj != null)
                                        {
                                            if (obj is ILTypeInstance)
                                            {
                                                ILTypeInstance instance = obj as ILTypeInstance;
                                                instance.PushToStack((int)ip->TokenLong, ret, AppDomain, mStack);
                                            }
                                            else
                                            {
                                                //var t = obj.GetType();
                                                var type = AppDomain.GetType((int)(ip->TokenLong >> 32));
                                                if (type != null)
                                                {
                                                    var token = (int)ip->TokenLong;
                                                    var ft = ((CLRType)type).GetField(token);
                                                    var val = ((CLRType)type).GetFieldValue(token, obj);
                                                    if (val is CrossBindingAdaptorType)
                                                        val = ((CrossBindingAdaptorType)val).ILInstance;
                                                    PushObject(ret, mStack, val, ft.FieldType == typeof(object));
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
                            case OpCodeEnum.Ldflda:
                                {
                                    StackObject* objRef = GetObjectAndResolveReference(esp - 1);
                                    if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                    {
                                        var dst = esp - 1;
                                        var ft = domain.GetType((int)(ip->TokenLong >> 32));
                                        StackObject* fieldAddr;
                                        if (ft is ILType)
                                        {
                                            fieldAddr = Minus(*(StackObject**)&objRef->Value, (int)ip->TokenLong + 1);                                            
                                        }
                                        else
                                        {
                                            fieldAddr = Minus(*(StackObject**)&objRef->Value, ((CLRType)ft).FieldIndexMapping[(int)ip->TokenLong] + 1);
                                        }
                                        dst->ObjectType = ObjectTypes.StackObjectReference;
                                        *(StackObject**)&dst->Value = fieldAddr;
                                    }
                                    else
                                    {
                                        object obj = RetriveObject(objRef, mStack);

                                        Free(esp - 1);
                                        if (obj != null)
                                        {
                                            if (obj is ILTypeInstance)
                                            {
                                                ILTypeInstance instance = obj as ILTypeInstance;
                                                instance.PushFieldAddress((int)ip->TokenLong, esp - 1, mStack);
                                            }
                                            else
                                            {
                                                objRef = esp - 1;
                                                objRef->ObjectType = ObjectTypes.FieldReference;
                                                objRef->Value = mStack.Count;
                                                mStack.Add(obj);
                                                objRef->ValueLow = (int)ip->TokenLong;
                                            }
                                        }
                                        else
                                            throw new NullReferenceException();
                                    }
                                }
                                break;
                            case OpCodeEnum.Stsfld:
                                {
                                    IType type = AppDomain.GetType((int)(ip->TokenLong >> 32));
                                    if (type != null)
                                    {
                                        if (type is ILType)
                                        {
                                            ILType t = type as ILType;
                                            StackObject* val = esp - 1;
                                            t.StaticInstance.AssignFromStack((int)ip->TokenLong, val, AppDomain, mStack);
                                        }
                                        else
                                        {
                                            CLRType t = type as CLRType;
                                            int idx = (int)ip->TokenLong;
                                            var f = t.GetField(idx);
                                            StackObject* val = esp - 1;
                                            t.SetStaticFieldValue(idx, f.FieldType.CheckCLRTypes(CheckAndCloneValueType(StackObject.ToObject(val, domain, mStack), domain)));
                                        }
                                    }
                                    else
                                        throw new TypeLoadException();
                                    Free(esp - 1);
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Ldsfld:
                                {
                                    IType type = AppDomain.GetType((int)(ip->TokenLong >> 32));
                                    if (type != null)
                                    {
                                        if (type is ILType)
                                        {
                                            ILType t = type as ILType;
                                            t.StaticInstance.PushToStack((int)ip->TokenLong, esp, AppDomain, mStack);
                                        }
                                        else
                                        {
                                            CLRType t = type as CLRType;
                                            int idx = (int)ip->TokenLong;
                                            var f = t.GetField(idx);
                                            var val = t.GetFieldValue(idx, null);
                                            if (val is CrossBindingAdaptorType)
                                                val = ((CrossBindingAdaptorType)val).ILInstance;
                                            PushObject(esp, mStack, val, f.FieldType == typeof(object));
                                        }
                                    }
                                    else
                                        throw new TypeLoadException();
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Ldsflda:
                                {
                                    int type = (int)(ip->TokenLong >> 32);
                                    int fieldIdx = (int)(ip->TokenLong);
                                    esp->ObjectType = ObjectTypes.StaticFieldReference;
                                    esp->Value = type;
                                    esp->ValueLow = fieldIdx;
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Ldtoken:
                                {
                                    switch (ip->TokenInteger)
                                    {
                                        case 0:
                                            {
                                                IType type = AppDomain.GetType((int)(ip->TokenLong >> 32));
                                                if (type != null)
                                                {
                                                    if (type is ILType)
                                                    {
                                                        ILType t = type as ILType;
                                                        t.StaticInstance.PushToStack((int)ip->TokenLong, esp, AppDomain, mStack);
                                                    }
                                                    else
                                                        throw new NotImplementedException();
                                                }
                                            }
                                            esp++;
                                            break;
                                        case 1:
                                            {
                                                IType type = AppDomain.GetType((int)ip->TokenLong);
                                                if (type != null)
                                                {
                                                    esp = PushObject(esp, mStack, type.ReflectionType);
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
                            case OpCodeEnum.Ldftn:
                                {
                                    IMethod m = domain.GetMethod(ip->TokenInteger);
                                    esp = PushObject(esp, mStack, m);
                                }
                                break;
                            case OpCodeEnum.Ldvirtftn:
                                {
                                    IMethod m = domain.GetMethod(ip->TokenInteger);
                                    var objRef = esp - 1;
                                    if (m is ILMethod)
                                    {
                                        ILMethod ilm = (ILMethod)m;

                                        var obj = mStack[objRef->Value];
                                        m = ((ILTypeInstance)obj).Type.GetVirtualMethod(ilm) as ILMethod;
                                    }
                                    else
                                    {
                                        var obj = mStack[objRef->Value];
                                        if (obj is ILTypeInstance)
                                            m = ((ILTypeInstance)obj).Type.GetVirtualMethod(m);
                                        else if (obj is CrossBindingAdaptorType)
                                        {
                                            m = ((CrossBindingAdaptorType)obj).ILInstance.Type.BaseType.GetVirtualMethod(m);
                                        }
                                    }
                                    Free(objRef);
                                    esp = PushObject(objRef, mStack, m);
                                }
                                break;
                            #endregion

                            #region Compare
                            case OpCodeEnum.Ceq:
                                {
                                    StackObject* obj1 = esp - 1 - 1;
                                    StackObject* obj2 = esp - 1;
                                    bool res = false;
                                    if (obj1->ObjectType == obj2->ObjectType)
                                    {
                                        switch (obj1->ObjectType)
                                        {
                                            case ObjectTypes.Integer:
                                            case ObjectTypes.Float:
                                                res = obj1->Value == obj2->Value;
                                                break;
                                            case ObjectTypes.Object:
                                                res = mStack[obj1->Value] == mStack[obj2->Value];
                                                break;
                                            case ObjectTypes.FieldReference:
                                                res = mStack[obj1->Value] == mStack[obj2->Value] && obj1->ValueLow == obj2->ValueLow;
                                                break;
                                            case ObjectTypes.Null:
                                                res = true;
                                                break;
                                            default:
                                                res = obj1->Value == obj2->Value && obj1->ValueLow == obj2->ValueLow;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        switch (obj1->ObjectType)
                                        {
                                            case ObjectTypes.Object:
                                                res = mStack[obj1->Value] == null && obj2->ObjectType == ObjectTypes.Null;
                                                break;
                                            case ObjectTypes.Null:
                                                res = obj2->ObjectType == ObjectTypes.Object && mStack[obj2->Value] == null;
                                                break;
                                        }
                                    }
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);
                                    if (res)
                                        esp = PushOne(esp - 1 - 1);
                                    else
                                        esp = PushZero(esp - 1 - 1);

                                }
                                break;
                            case OpCodeEnum.Clt:
                                {
                                    StackObject* obj1 = esp - 1 - 1;
                                    StackObject* obj2 = esp - 1;
                                    bool res = false;
                                    switch (obj1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = obj1->Value < obj2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            res = *(long*)&obj1->Value < *(long*)&obj2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&obj1->Value < *(float*)&obj2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            res = *(double*)&obj1->Value < *(double*)&obj2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        esp = PushOne(esp - 1 - 1);
                                    else
                                        esp = PushZero(esp - 1 - 1);
                                }
                                break;
                            case OpCodeEnum.Clt_Un:
                                {
                                    StackObject* obj1 = esp - 1 - 1;
                                    StackObject* obj2 = esp - 1;
                                    bool res = false;
                                    switch (obj1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = (uint)obj1->Value < (uint)obj2->Value && obj2->ObjectType != ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Long:
                                            res = (ulong)*(long*)&obj1->Value < (ulong)*(long*)&obj2->Value && obj2->ObjectType != ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&obj1->Value < *(float*)&obj2->Value && obj2->ObjectType != ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Double:
                                            res = *(double*)&obj1->Value < *(double*)&obj2->Value && obj2->ObjectType != ObjectTypes.Null;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        esp = PushOne(esp - 1 - 1);
                                    else
                                        esp = PushZero(esp - 1 - 1);
                                }
                                break;
                            case OpCodeEnum.Cgt:
                                {
                                    StackObject* obj1 = esp - 1 - 1;
                                    StackObject* obj2 = esp - 1;
                                    bool res = false;
                                    switch (obj1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = obj1->Value > obj2->Value || obj2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Long:
                                            res = *(long*)&obj1->Value > *(long*)&obj2->Value || obj2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&obj1->Value > *(float*)&obj2->Value || obj2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Double:
                                            res = *(double*)&obj1->Value > *(double*)&obj2->Value || obj2->ObjectType == ObjectTypes.Null;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        esp = PushOne(esp - 1 - 1);
                                    else
                                        esp = PushZero(esp - 1 - 1);
                                }
                                break;
                            case OpCodeEnum.Cgt_Un:
                                {
                                    StackObject* obj1 = esp - 1 - 1;
                                    StackObject* obj2 = esp - 1;
                                    bool res = false;
                                    switch (obj1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = ((uint)obj1->Value > (uint)obj2->Value) || obj2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Long:
                                            res = (ulong)*(long*)&obj1->Value > (ulong)*(long*)&obj2->Value || obj2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&obj1->Value > *(float*)&obj2->Value || obj2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Double:
                                            res = *(double*)&obj1->Value > *(double*)&obj2->Value || obj2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Object:
                                            res = mStack[obj1->Value] != null && obj2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Null:
                                            res = false;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        esp = PushOne(esp - 1 - 1);
                                    else
                                        esp = PushZero(esp - 1 - 1);
                                }
                                break;
                            #endregion

                            #region Initialization & Instantiation
                            case OpCodeEnum.Newobj:
                                {
                                    IMethod m = domain.GetMethod(ip->TokenInteger);
                                    if (m is ILMethod)
                                    {
                                        ILType type = m.DeclearingType as ILType;
                                        if (type.IsDelegate)
                                        {
                                            var objRef = GetObjectAndResolveReference(esp - 1 - 1);
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
                                            if (mi is ILMethod)
                                            {
                                                if (ins != null)
                                                {
                                                    dele = ((ILTypeInstance)ins).GetDelegateAdapter((ILMethod)mi);
                                                    if (dele == null)
                                                        dele = domain.DelegateManager.FindDelegateAdapter((ILTypeInstance)ins, (ILMethod)mi);
                                                }
                                                else
                                                {
                                                    if (((ILMethod)mi).DelegateAdapter == null)
                                                    {
                                                        ((ILMethod)mi).DelegateAdapter = domain.DelegateManager.FindDelegateAdapter(null, (ILMethod)mi);
                                                    }
                                                    dele = ((ILMethod)mi).DelegateAdapter;
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
                                            var a = esp - m.ParameterCount;
                                            StackObject* objRef;
                                            ILTypeInstance obj = null;
                                            bool isValueType = type.IsValueType;
                                            if (isValueType)
                                            {
                                                stack.AllocValueType(esp, type);
                                                objRef = esp + 1;
                                                objRef->ObjectType = ObjectTypes.StackObjectReference;
                                                *(StackObject**)&objRef->Value = esp;
                                                objRef++;
                                            }
                                            else
                                            {
                                                obj = type.Instantiate(false);
                                                objRef = PushObject(esp, mStack, obj);//this parameter for constructor
                                            }
                                            esp = objRef;
                                            for (int i = 0; i < m.ParameterCount; i++)
                                            {
                                                CopyToStack(esp, a + i, mStack);
                                                esp++;
                                            }
                                            esp = Execute((ILMethod)m, esp, out unhandledException);
                                            ValueTypeBasePointer = bp;
                                            if (isValueType)
                                            {
                                                var ins = objRef - 1 - 1;
                                                *a = *ins;
                                                esp = a + 1;
                                            }
                                            else
                                                esp = PushObject(a, mStack, obj);//new constructedObj
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
                                                var objRef = GetObjectAndResolveReference(esp - 1 - 1);
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
                                                if (mi is ILMethod)
                                                {
                                                    if (ins != null)
                                                    {
                                                        dele = ((ILTypeInstance)ins).GetDelegateAdapter((ILMethod)mi);
                                                        if (dele == null)
                                                            dele = domain.DelegateManager.FindDelegateAdapter((ILTypeInstance)ins, (ILMethod)mi);
                                                    }
                                                    else
                                                    {
                                                        if (((ILMethod)mi).DelegateAdapter == null)
                                                        {
                                                            ((ILMethod)mi).DelegateAdapter = domain.DelegateManager.FindDelegateAdapter(null, (ILMethod)mi);
                                                        }
                                                        dele = ((ILMethod)mi).DelegateAdapter;
                                                    }
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
                                                    object result = cm.Invoke(this, esp, mStack, true);
                                                    int paramCount = cm.ParameterCount;
                                                    for (int i = 1; i <= paramCount; i++)
                                                    {
                                                        Free(esp - i);
                                                    }
                                                    esp = Minus(esp, paramCount);
                                                    esp = PushObject(esp, mStack, result);//new constructedObj
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            case OpCodeEnum.Constrained:
                                {
                                    var type = domain.GetType(ip->TokenInteger);
                                    var m = domain.GetMethod((int)ip->TokenLong);
                                    var pCnt = m.ParameterCount;
                                    var objRef = Minus(esp, pCnt + 1);
                                    var insIdx = mStack.Count;
                                    if (objRef->ObjectType < ObjectTypes.Object)
                                    {
                                        bool moved = false;
                                        //move parameters
                                        for (int i = 0; i < pCnt; i++)
                                        {
                                            var pPtr = Minus(esp, i + 1);
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
                                    var obj = GetObjectAndResolveReference(objRef);
                                    if (type != null)
                                    {
                                        if (type is ILType)
                                        {
                                            var t = (ILType)type;
                                            if (t.IsEnum)
                                            {
                                                ILEnumTypeInstance ins = new ILEnumTypeInstance(t);
                                                switch (obj->ObjectType)
                                                {
                                                    case ObjectTypes.FieldReference:
                                                        {
                                                            var owner = mStack[obj->Value] as ILTypeInstance;
                                                            int idx = obj->ValueLow;
                                                            //Free(objRef);
                                                            owner.PushToStack(idx, objRef, AppDomain, mStack);
                                                            ins.AssignFromStack(0, objRef, AppDomain, mStack);
                                                            ins.Boxed = true;
                                                        }
                                                        break;
                                                    case ObjectTypes.StaticFieldReference:
                                                        {
                                                            var st = AppDomain.GetType(obj->Value) as ILType;
                                                            int idx = obj->ValueLow;
                                                            //Free(objRef);
                                                            st.StaticInstance.PushToStack(idx, objRef, AppDomain, mStack);
                                                            ins.AssignFromStack(0, objRef, AppDomain, mStack);
                                                            ins.Boxed = true;
                                                        }
                                                        break;
                                                    case ObjectTypes.ArrayReference:
                                                        {
                                                            var arr = mStack[obj->Value];
                                                            var idx = obj->ValueLow;
                                                            //Free(objRef);
                                                            LoadFromArrayReference(arr, idx, objRef, t, mStack);
                                                            ins.AssignFromStack(0, objRef, AppDomain, mStack);
                                                            ins.Boxed = true;
                                                        }
                                                        break;
                                                    default:
                                                        ins.AssignFromStack(0, obj, AppDomain, mStack);
                                                        ins.Boxed = true;
                                                        break;
                                                }
                                                objRef->ObjectType = ObjectTypes.Object;
                                                objRef->Value = insIdx;
                                                mStack[insIdx] = ins;

                                                //esp = PushObject(esp - 1, mStack, ins);
                                            }
                                            else
                                            {
                                                object res = RetriveObject(obj, mStack);
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
                                                mStack[insIdx] = Enum.ToObject(tt, StackObject.ToObject(obj, AppDomain, mStack));
                                                objRef->ObjectType = ObjectTypes.Object;
                                                objRef->Value = insIdx;
                                                //esp = PushObject(esp - 1, mStack, Enum.ToObject(tt, StackObject.ToObject(obj, AppDomain, mStack)), true);
                                            }
                                            else if (tt.IsPrimitive)
                                            {
                                                mStack[insIdx] = tt.CheckCLRTypes(StackObject.ToObject(obj, AppDomain, mStack));
                                                objRef->ObjectType = ObjectTypes.Object;
                                                objRef->Value = insIdx;
                                                //esp = PushObject(esp - 1, mStack, tt.CheckCLRTypes(StackObject.ToObject(obj, AppDomain, mStack)));
                                            }
                                            else
                                            {
                                                object res = RetriveObject(obj, mStack);
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
                            case OpCodeEnum.Box:
                                {
                                    var obj = esp - 1;
                                    var type = domain.GetType(ip->TokenInteger);
                                    if (type != null)
                                    {
                                        if (type is ILType)
                                        {
                                            if (((ILType)type).IsEnum)
                                            {
                                                ILEnumTypeInstance ins = new Intepreter.ILEnumTypeInstance((ILType)type);
                                                ins.AssignFromStack(0, obj, AppDomain, mStack);
                                                ins.Boxed = true;
                                                esp = PushObject(obj, mStack, ins, true);
                                            }
                                            else
                                            {
                                                switch (obj->ObjectType)
                                                {
                                                    case ObjectTypes.Null:
                                                        break;
                                                    case ObjectTypes.ValueTypeObjectReference:
                                                        {
                                                            ILTypeInstance ins = ((ILType)type).Instantiate(false);
                                                            ins.AssignFromStack(obj, domain, mStack);
                                                            FreeStackValueType(obj);
                                                            esp = PushObject(obj, mStack, ins, true);
                                                        }
                                                        break;
                                                    default:
                                                        {
                                                            var val = mStack[obj->Value];
                                                            Free(obj);
                                                            if (type.IsArray)
                                                            {
                                                                esp = PushObject(obj, mStack, val, true);
                                                            }
                                                            else
                                                            {
                                                                ILTypeInstance ins = (ILTypeInstance)val;
                                                                if (ins != null)
                                                                {
                                                                    if (ins.IsValueType)
                                                                    {
                                                                        ins.Boxed = true;
                                                                    }
                                                                    esp = PushObject(obj, mStack, ins, true);
                                                                }
                                                                else
                                                                {
                                                                    esp = PushNull(obj);
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
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            esp = PushObject(obj, mStack, obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, 0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(bool))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            esp = PushObject(obj, mStack, (obj->Value == 1), true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, false, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(byte))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            esp = PushObject(obj, mStack, (byte)obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, 0L, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(short))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            esp = PushObject(obj, mStack, (short)obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, (short)0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(long))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Long:
                                                            esp = PushObject(obj, mStack, *(long*)&obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, 0L, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(float))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Float:
                                                            esp = PushObject(obj, mStack, *(float*)&obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, 0f, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(double))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Double:
                                                            esp = PushObject(obj, mStack, *(double*)&obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, 0.0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(char))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            esp = PushObject(obj, mStack, (char)obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(uint))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            esp = PushObject(obj, mStack, (uint)obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, (uint)0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(ushort))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            esp = PushObject(obj, mStack, (ushort)obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, (ushort)0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(ulong))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Long:
                                                            esp = PushObject(obj, mStack, *(ulong*)&obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, (ulong)0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(sbyte))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            esp = PushObject(obj, mStack, (sbyte)obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, (sbyte)0, true);
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
                                                esp = PushObject(obj, mStack, Enum.ToObject(type.TypeForCLR, StackObject.ToObject(obj, AppDomain, mStack)), true);
                                            }
                                            else
                                            {
                                                if(obj->ObjectType== ObjectTypes.ValueTypeObjectReference)
                                                {
                                                    var dst = *(StackObject**)&obj->Value;
                                                    var vt = domain.GetType(dst->Value);
                                                    if (vt != type)
                                                        throw new InvalidCastException();
                                                    object ins = ((CLRType)vt).ValueTypeBinder.ToObject(dst, mStack);
                                                    FreeStackValueType(obj);
                                                    esp = PushObject(obj, mStack, ins, true);
                                                }
                                                //nothing to do for CLR type boxing
                                            }
                                        }
                                    }
                                    else
                                        throw new NullReferenceException();
                                }
                                break;
                            case OpCodeEnum.Unbox:
                            case OpCodeEnum.Unbox_Any:
                                {
                                    var objRef = esp - 1;
                                    if (objRef->ObjectType == ObjectTypes.Object)
                                    {
                                        object obj = mStack[objRef->Value];
                                        Free(objRef);
                                        if (obj != null)
                                        {
                                            var t = domain.GetType(ip->TokenInteger);
                                            if (t != null)
                                            {
                                                var type = t.TypeForCLR;
                                                bool isEnumObj = obj is ILEnumTypeInstance;
                                                if ((t is CLRType) && type.IsPrimitive && !isEnumObj)
                                                {
                                                    if (type == typeof(int))
                                                    {
                                                        int val = obj.ToInt32();
                                                        objRef->ObjectType = ObjectTypes.Integer;
                                                        objRef->Value = val;
                                                    }
                                                    else if (type == typeof(bool))
                                                    {
                                                        bool val = (bool)obj;
                                                        objRef->ObjectType = ObjectTypes.Integer;
                                                        objRef->Value = val ? 1 : 0;
                                                    }
                                                    else if (type == typeof(short))
                                                    {
                                                        short val = obj.ToInt16();
                                                        objRef->ObjectType = ObjectTypes.Integer;
                                                        objRef->Value = val;
                                                    }
                                                    else if (type == typeof(long))
                                                    {
                                                        long val = obj.ToInt64();
                                                        objRef->ObjectType = ObjectTypes.Long;
                                                        *(long*)&objRef->Value = val;
                                                    }
                                                    else if (type == typeof(float))
                                                    {
                                                        float val = obj.ToFloat();
                                                        objRef->ObjectType = ObjectTypes.Float;
                                                        *(float*)&objRef->Value = val;
                                                    }
                                                    else if (type == typeof(byte))
                                                    {
                                                        byte val = (byte)obj;
                                                        objRef->ObjectType = ObjectTypes.Integer;
                                                        objRef->Value = val;
                                                    }
                                                    else if (type == typeof(double))
                                                    {
                                                        double val = obj.ToDouble();
                                                        objRef->ObjectType = ObjectTypes.Double;
                                                        *(double*)&objRef->Value = val;
                                                    }
                                                    else if (type == typeof(char))
                                                    {
                                                        char val = (char)obj;
                                                        objRef->ObjectType = ObjectTypes.Integer;
                                                        *(char*)&objRef->Value = val;
                                                    }
                                                    else if (type == typeof(uint))
                                                    {
                                                        uint val = (uint)obj;
                                                        objRef->ObjectType = ObjectTypes.Integer;
                                                        objRef->Value = (int)val;
                                                    }
                                                    else if (type == typeof(ushort))
                                                    {
                                                        ushort val = (ushort)obj;
                                                        objRef->ObjectType = ObjectTypes.Integer;
                                                        objRef->Value = val;
                                                    }
                                                    else if (type == typeof(ulong))
                                                    {
                                                        ulong val = (ulong)obj;
                                                        objRef->ObjectType = ObjectTypes.Long;
                                                        *(ulong*)&objRef->Value = val;
                                                    }
                                                    else if (type == typeof(sbyte))
                                                    {
                                                        sbyte val = (sbyte)obj;
                                                        objRef->ObjectType = ObjectTypes.Integer;
                                                        objRef->Value = val;
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
                                                            res.PushToStack(0, objRef, AppDomain, mStack);
                                                        }
                                                        else
                                                        {
                                                            if (res.Boxed)
                                                            {
                                                                res = res.Clone();
                                                                res.Boxed = false;
                                                            }
                                                            PushObject(objRef, mStack, res);
                                                        }
                                                    }
                                                    else
                                                        PushObject(objRef, mStack, obj);
                                                }
                                                else
                                                {
                                                    PushObject(objRef, mStack, obj);
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
                            case OpCodeEnum.Initobj:
                                {
                                    var objRef = GetObjectAndResolveReference(esp - 1);
                                    var type = domain.GetType(ip->TokenInteger);
                                    if (type is ILType)
                                    {
                                        ILType it = (ILType)type;
                                        if (it.IsValueType)
                                        {
                                            switch (objRef->ObjectType)
                                            {
                                                case ObjectTypes.Null:
                                                    throw new NullReferenceException();
                                                case ObjectTypes.ValueTypeObjectReference:
                                                    stack.ClearValueTypeObject(type, *(StackObject**)&objRef->Value);
                                                    break;
                                                case ObjectTypes.Object:
                                                    {
                                                        var obj = mStack[objRef->Value];
                                                        if (obj != null)
                                                        {
                                                            if (obj is ILTypeInstance)
                                                            {
                                                                ILTypeInstance instance = obj as ILTypeInstance;
                                                                instance.Clear();
                                                            }
                                                            else
                                                                throw new NotSupportedException();
                                                        }
                                                        else
                                                            throw new NullReferenceException();
                                                    }
                                                    break;
                                                case ObjectTypes.ArrayReference:
                                                    {
                                                        var arr = mStack[objRef->Value] as Array;
                                                        var idx = objRef->ValueLow;
                                                        var obj = arr.GetValue(idx);
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
                                                        var obj = mStack[objRef->Value];
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
                                                default:
                                                    throw new NotImplementedException();
                                            }

                                            Free(esp - 1);
                                            esp--;
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
                                                        Free(esp - 1);
                                                        esp--;
                                                    }
                                                    break;
                                                default:
                                                    {
                                                        if (objRef->ObjectType >= ObjectTypes.Object)
                                                            mStack[objRef->Value] = null;
                                                        else
                                                            PushNull(objRef);
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                        {
                                            stack.ClearValueTypeObject(type, *(StackObject**)&objRef->Value);
                                        }
                                        else if(type.IsPrimitive)
                                            StackObject.Initialized(objRef, type);
                                        Free(esp - 1);
                                        esp--;
                                    }
                                }
                                break;
                            case OpCodeEnum.Isinst:
                                {
                                    var objRef = esp - 1;
                                    var oriRef = objRef;
                                    var type = domain.GetType(ip->TokenInteger);
                                    if (type != null)
                                    {
                                        objRef = GetObjectAndResolveReference(objRef);
                                        if (objRef->ObjectType <= ObjectTypes.Double)
                                        {
                                            var tclr = type.TypeForCLR;
                                            switch (objRef->ObjectType)
                                            {
                                                case ObjectTypes.Integer:
                                                    {
                                                        if (tclr != typeof(int) && tclr != typeof(bool) && tclr != typeof(short) && tclr != typeof(byte) && tclr != typeof(ushort) && tclr !=typeof(uint))
                                                        {
                                                            oriRef->ObjectType = ObjectTypes.Null;
                                                            oriRef->Value = -1;
                                                            oriRef->ValueLow = 0;
                                                        }
                                                    }
                                                    break;
                                                case ObjectTypes.Long:
                                                    {
                                                        if (tclr != typeof(long) && tclr != typeof(ulong))
                                                        {
                                                            oriRef->ObjectType = ObjectTypes.Null;
                                                            oriRef->Value = -1;
                                                            oriRef->ValueLow = 0;
                                                        }
                                                    }
                                                    break;
                                                case ObjectTypes.Float:
                                                    {
                                                        if (tclr != typeof(float))
                                                        {
                                                            oriRef->ObjectType = ObjectTypes.Null;
                                                            oriRef->Value = -1;
                                                            oriRef->ValueLow = 0;
                                                        }
                                                    }
                                                    break;
                                                case ObjectTypes.Double:
                                                    {
                                                        if (tclr != typeof(double))
                                                        {
                                                            oriRef->ObjectType = ObjectTypes.Null;
                                                            oriRef->Value = -1;
                                                            oriRef->ValueLow = 0;
                                                        }
                                                    }
                                                    break;
                                                case ObjectTypes.Null:
                                                    oriRef->ObjectType = ObjectTypes.Null;
                                                    oriRef->Value = -1;
                                                    oriRef->ValueLow = 0;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            var obj = RetriveObject(objRef, mStack);
                                            Free(oriRef);

                                            if (obj != null)
                                            {
                                                if (obj is ILTypeInstance)
                                                {
                                                    if (((ILTypeInstance)obj).CanAssignTo(type))
                                                    {
                                                        esp = PushObject(oriRef, mStack, obj);
                                                    }
                                                    else
                                                    {
#if !DEBUG || DISABLE_ILRUNTIME_DEBUG
                                                        oriRef->ObjectType = ObjectTypes.Null;
                                                        oriRef->Value = -1;
                                                        oriRef->ValueLow = 0;
#endif
                                                    }
                                                }
                                                else
                                                {
                                                    if (type.TypeForCLR.IsAssignableFrom(obj.GetType()))
                                                    {
                                                        esp = PushObject(oriRef, mStack, obj, true);
                                                    }
                                                    else
                                                    {
#if !DEBUG || DISABLE_ILRUNTIME_DEBUG
                                                        oriRef->ObjectType = ObjectTypes.Null;
                                                        oriRef->Value = -1;
                                                        oriRef->ValueLow = 0;
#endif
                                                    }
                                                }
                                            }
                                            else
                                            {
#if !DEBUG || DISABLE_ILRUNTIME_DEBUG
                                                    oriRef->ObjectType = ObjectTypes.Null;
                                                    oriRef->Value = -1;
                                                    oriRef->ValueLow = 0;
#endif
                                            }
                                        }
                                    }
                                    else
                                        throw new NullReferenceException();
                                }
                                break;
                            #endregion

                            #region Array
                            case OpCodeEnum.Newarr:
                                {
                                    var cnt = (esp - 1);
                                    var type = domain.GetType(ip->TokenInteger);
                                    object arr = null;
                                    if (type != null)
                                    {
                                        if (type.TypeForCLR != typeof(ILTypeInstance))
                                        {
                                            if (type is CLRType)
                                            {
                                                arr = ((CLRType)type).CreateArrayInstance(cnt->Value);
                                            }
                                            else
                                            {
                                                arr = Array.CreateInstance(type.TypeForCLR, cnt->Value);
                                            }

                                            //Register Type
                                            AppDomain.GetType(arr.GetType());
                                        }
                                        else
                                        {
                                            arr = new ILTypeInstance[cnt->Value];
                                            ILTypeInstance[] ilArr = (ILTypeInstance[])arr;
                                            if (type.IsValueType)
                                            {
                                                for(int i = 0; i < cnt->Value; i++)
                                                {
                                                    ilArr[i] = ((ILType)type).Instantiate(true);
                                                }
                                            }
                                        }
                                    }
                                    cnt->ObjectType = ObjectTypes.Object;
                                    cnt->Value = mStack.Count;
                                    mStack.Add(arr);
                                }
                                break;
                            case OpCodeEnum.Stelem_Ref:
                            case OpCodeEnum.Stelem_Any:
                                {
                                    var val = GetObjectAndResolveReference(esp - 1);
                                    var idx = esp - 1 - 1;
                                    var arrRef = esp - 1 - 1 - 1;
                                    Array arr = mStack[arrRef->Value] as Array;
                                    if (arr is object[])
                                    {
                                        switch (val->ObjectType)
                                        {
                                            case ObjectTypes.Null:
                                                arr.SetValue(null, idx->Value);
                                                break;
                                            case ObjectTypes.Object:
                                                ArraySetValue(arr, mStack[val->Value], idx->Value);
                                                break;
                                            case ObjectTypes.Integer:
                                                arr.SetValue(val->Value, idx->Value);
                                                break;
                                            case ObjectTypes.Long:
                                                arr.SetValue(*(long*)&val->Value, idx->Value);
                                                break;
                                            case ObjectTypes.Float:
                                                arr.SetValue(*(float*)&val->Value, idx->Value);
                                                break;
                                            case ObjectTypes.Double:
                                                arr.SetValue(*(double*)&val->Value, idx->Value);
                                                break;
                                            case ObjectTypes.ValueTypeObjectReference:
                                                ArraySetValue(arr, StackObject.ToObject(val, domain, mStack), idx->Value);
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
                                                ArraySetValue(arr, mStack[val->Value], idx->Value);
                                                break;
                                            case ObjectTypes.Integer:
                                                {
                                                    StoreIntValueToArray(arr, val, idx);
                                                }
                                                break;
                                            case ObjectTypes.Long:
                                                {
                                                    if (arr is long[])
                                                    {
                                                        ((long[])arr)[idx->Value] = *(long*)&val->Value;
                                                    }
                                                    else
                                                    {
                                                        ((ulong[])arr)[idx->Value] = *(ulong*)&val->Value;
                                                    }
                                                }
                                                break;
                                            case ObjectTypes.Float:
                                                {
                                                    ((float[])arr)[idx->Value] = *(float*)&val->Value;
                                                }
                                                break;
                                            case ObjectTypes.Double:
                                                {
                                                    ((double[])arr)[idx->Value] = *(double*)&val->Value;
                                                }
                                                break;
                                            case ObjectTypes.ValueTypeObjectReference:
                                                ArraySetValue(arr, StackObject.ToObject(val, domain, mStack), idx->Value);
                                                FreeStackValueType(esp - 1);
                                                break;
                                            default:
                                                throw new NotImplementedException();
                                        }
                                    }
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);
                                    Free(esp - 1 - 1 - 1);
                                    esp = esp - 1 - 1 - 1;
                                }
                                break;

                            case OpCodeEnum.Ldelem_Ref:
                            case OpCodeEnum.Ldelem_Any:
                                {
                                    var idx = esp - 1;
                                    var arrRef = esp - 1 - 1;
                                    Array arr = mStack[arrRef->Value] as Array;
                                    object val = arr.GetValue(idx->Value);
                                    if (val is CrossBindingAdaptorType)
                                        val = ((CrossBindingAdaptorType)val).ILInstance;
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);

                                    if (val is ILTypeInstance)
                                    {
                                        ILTypeInstance ins = (ILTypeInstance)val;
                                        if (ins.Type.IsValueType && !ins.Boxed)
                                        {
                                            AllocValueType(arrRef, ins.Type);
                                            StackObject* dst = *((StackObject**)&arrRef->Value);
                                            ins.CopyValueTypeToStack(dst, mStack);
                                            esp = idx;
                                        }
                                        else
                                            esp = PushObject(esp - 1 - 1, mStack, val, true);
                                    }
                                    else
                                        esp = PushObject(esp - 1 - 1, mStack, val, true);
                                }
                                break;
                            case OpCodeEnum.Stelem_I1:
                                {
                                    var val = esp - 1;
                                    var idx = esp - 1 - 1;
                                    var arrRef = esp - 1 - 1 - 1;
                                    byte[] arr = mStack[arrRef->Value] as byte[];
                                    if (arr != null)
                                    {
                                        arr[idx->Value] = (byte)val->Value;
                                    }
                                    else
                                    {
                                        bool[] arr2 = mStack[arrRef->Value] as bool[];
                                        if (arr2 != null)
                                        {
                                            arr2[idx->Value] = val->Value == 1;
                                        }
                                        else
                                        {
                                            sbyte[] arr3 = mStack[arrRef->Value] as sbyte[];
                                            arr3[idx->Value] = (sbyte)val->Value;
                                        }
                                    }
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);
                                    Free(esp - 1 - 1 - 1);
                                    esp = esp - 1 - 1 - 1;
                                }
                                break;
                            case OpCodeEnum.Ldelem_I1:
                                {
                                    var idx = esp - 1;
                                    var arrRef = esp - 1 - 1;
                                    bool[] arr = mStack[arrRef->Value] as bool[];
                                    int val;
                                    if (arr != null)
                                        val = arr[idx->Value] ? 1 : 0;
                                    else
                                    {
                                        sbyte[] arr2 = mStack[arrRef->Value] as sbyte[];
                                        val = arr2[idx->Value];
                                    }

                                    Free(esp - 1);
                                    Free(esp - 1 - 1);

                                    arrRef->ObjectType = ObjectTypes.Integer;
                                    arrRef->Value = val;
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Ldelem_U1:
                                {
                                    var idx = (esp - 1);
                                    var arrRef = esp - 1 - 1;
                                    byte[] arr = mStack[arrRef->Value] as byte[];
                                    int val;
                                    if (arr != null)
                                        val = arr[idx->Value];
                                    else
                                    {
                                        bool[] arr2 = mStack[arrRef->Value] as bool[];
                                        val = arr2[idx->Value] ? 1 : 0;
                                    }
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);

                                    arrRef->ObjectType = ObjectTypes.Integer;
                                    arrRef->Value = val;
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Stelem_I2:
                                {
                                    var val = esp - 1;
                                    var idx = esp - 1 - 1;
                                    var arrRef = esp - 1 - 1 - 1;
                                    short[] arr = mStack[arrRef->Value] as short[];
                                    if (arr != null)
                                    {
                                        arr[idx->Value] = (short)val->Value;
                                    }
                                    else
                                    {
                                        ushort[] arr2 = mStack[arrRef->Value] as ushort[];
                                        if (arr2 != null)
                                        {
                                            arr2[idx->Value] = (ushort)val->Value;
                                        }
                                        else
                                        {
                                            char[] arr3 = mStack[arrRef->Value] as char[];
                                            arr3[idx->Value] = (char)val->Value;
                                        }
                                    }
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);
                                    Free(esp - 1 - 1 - 1);
                                    esp = esp - 1 - 1 - 1;
                                }
                                break;
                            case OpCodeEnum.Ldelem_I2:
                                {
                                    var idx = (esp - 1)->Value;
                                    var arrRef = esp - 1 - 1;
                                    short[] arr = mStack[arrRef->Value] as short[];
                                    int val = 0;
                                    if (arr != null)
                                    {
                                        val = arr[idx];
                                    }
                                    else
                                    {
                                        char[] arr2 = mStack[arrRef->Value] as char[];
                                        val = arr2[idx];
                                    }
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);

                                    arrRef->ObjectType = ObjectTypes.Integer;
                                    arrRef->Value = val;
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Ldelem_U2:
                                {
                                    var idx = (esp - 1)->Value;
                                    var arrRef = esp - 1 - 1;
                                    ushort[] arr = mStack[arrRef->Value] as ushort[];
                                    int val = 0;
                                    if (arr != null)
                                    {
                                        val = arr[idx];
                                    }
                                    else
                                    {
                                        char[] arr2 = mStack[arrRef->Value] as char[];
                                        val = arr2[idx];
                                    }
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);

                                    arrRef->ObjectType = ObjectTypes.Integer;
                                    arrRef->Value = val;
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Stelem_I4:
                                {
                                    var val = esp - 1;
                                    var idx = esp - 1 - 1;
                                    var arrRef = esp - 1 - 1 - 1;
                                    int[] arr = mStack[arrRef->Value] as int[];
                                    if (arr != null)
                                    {
                                        arr[idx->Value] = val->Value;
                                    }
                                    else
                                    {
                                        uint[] arr2 = mStack[arrRef->Value] as uint[];
                                        arr2[idx->Value] = (uint)val->Value;
                                    }
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);
                                    Free(esp - 1 - 1 - 1);
                                    esp = esp - 1 - 1 - 1;
                                }
                                break;
                            case OpCodeEnum.Ldelem_I4:
                                {
                                    var idx = (esp - 1)->Value;
                                    var arrRef = esp - 1 - 1;
                                    int[] arr = mStack[arrRef->Value] as int[];

                                    Free(esp - 1);
                                    Free(esp - 1 - 1);

                                    arrRef->ObjectType = ObjectTypes.Integer;
                                    arrRef->Value = arr[idx];
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Ldelem_U4:
                                {
                                    var idx = (esp - 1)->Value;
                                    var arrRef = esp - 1 - 1;
                                    uint[] arr = mStack[arrRef->Value] as uint[];

                                    Free(esp - 1);
                                    Free(esp - 1 - 1);

                                    arrRef->ObjectType = ObjectTypes.Integer;
                                    arrRef->Value = (int)arr[idx];
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Stelem_I8:
                                {
                                    var val = esp - 1;
                                    var idx = esp - 1 - 1;
                                    var arrRef = esp - 1 - 1 - 1;
                                    long[] arr = mStack[arrRef->Value] as long[];
                                    if (arr != null)
                                    {
                                        arr[idx->Value] = *(long*)&val->Value;
                                    }
                                    else
                                    {
                                        ulong[] arr2 = mStack[arrRef->Value] as ulong[];
                                        arr2[idx->Value] = *(ulong*)&val->Value;
                                    }
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);
                                    Free(esp - 1 - 1 - 1);
                                    esp = esp - 1 - 1 - 1;
                                }
                                break;
                            case OpCodeEnum.Ldelem_I8:
                                {
                                    var idx = esp - 1;
                                    var arrRef = esp - 1 - 1;
                                    long[] arr = mStack[arrRef->Value] as long[];
                                    long val;
                                    if (arr != null)
                                        val = arr[idx->Value];
                                    else
                                    {
                                        ulong[] arr2 = mStack[arrRef->Value] as ulong[];
                                        val = (long)arr2[idx->Value];
                                    }
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);

                                    arrRef->ObjectType = ObjectTypes.Long;
                                    *(long*)&arrRef->Value = val;
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Stelem_R4:
                                {
                                    var val = esp - 1;
                                    var idx = esp - 1 - 1;
                                    var arrRef = esp - 1 - 1 - 1;
                                    float[] arr = mStack[arrRef->Value] as float[];
                                    arr[idx->Value] = *(float*)&val->Value;
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);
                                    Free(esp - 1 - 1 - 1);
                                    esp = esp - 1 - 1 - 1;
                                }
                                break;
                            case OpCodeEnum.Ldelem_R4:
                                {
                                    var idx = (esp - 1)->Value;
                                    var arrRef = esp - 1 - 1;
                                    float[] arr = mStack[arrRef->Value] as float[];

                                    Free(esp - 1);
                                    Free(esp - 1 - 1);

                                    arrRef->ObjectType = ObjectTypes.Float;
                                    *(float*)&arrRef->Value = arr[idx];
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Stelem_R8:
                                {
                                    var val = esp - 1;
                                    var idx = esp - 1 - 1;
                                    var arrRef = esp - 1 - 1 - 1;
                                    double[] arr = mStack[arrRef->Value] as double[];
                                    arr[idx->Value] = *(double*)&val->Value;
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);
                                    Free(esp - 1 - 1 - 1);
                                    esp = esp - 1 - 1 - 1;
                                }
                                break;
                            case OpCodeEnum.Ldelem_R8:
                                {
                                    var idx = (esp - 1)->Value;
                                    var arrRef = esp - 1 - 1;
                                    double[] arr = mStack[arrRef->Value] as double[];

                                    Free(esp - 1);
                                    Free(esp - 1 - 1);

                                    arrRef->ObjectType = ObjectTypes.Double;
                                    *(double*)&arrRef->Value = arr[idx];
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Ldlen:
                                {
                                    var arrRef = esp - 1;
                                    Array arr = mStack[arrRef->Value] as Array;
                                    Free(esp - 1);

                                    arrRef->ObjectType = ObjectTypes.Integer;
                                    arrRef->Value = arr.Length;
                                }
                                break;
                            case OpCodeEnum.Ldelema:
                                {
                                    var arrRef = esp - 1 - 1;
                                    var idx = (esp - 1)->Value;

                                    Array arr = mStack[arrRef->Value] as Array;
                                    Free(esp - 1);
                                    Free(esp - 1 - 1);

                                    arrRef->ObjectType = ObjectTypes.ArrayReference;
                                    arrRef->Value = mStack.Count;
                                    mStack.Add(arr);
                                    arrRef->ValueLow = idx;
                                    esp--;
                                }
                                break;
                            #endregion

                            #region Conversion
                            case OpCodeEnum.Conv_U1:
                            case OpCodeEnum.Conv_Ovf_U1:
                            case OpCodeEnum.Conv_Ovf_U1_Un:
                                {
                                    var obj = esp - 1;
                                    int val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                        case ObjectTypes.Integer:
                                            val = (byte)obj->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            val = (byte)*(float*)&obj->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            val = (byte)*(double*)&obj->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Integer;
                                    obj->Value = val;
                                    obj->ValueLow = 0;
                                }
                                break;
                            case OpCodeEnum.Conv_I1:
                            case OpCodeEnum.Conv_Ovf_I1:
                            case OpCodeEnum.Conv_Ovf_I1_Un:
                                {
                                    var obj = esp - 1;
                                    int val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                        case ObjectTypes.Integer:
                                            val = (sbyte)obj->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            val = (sbyte)*(float*)&obj->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            val = (sbyte)*(double*)&obj->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Integer;
                                    obj->Value = val;
                                    obj->ValueLow = 0;
                                }
                                break;
                            case OpCodeEnum.Conv_U2:
                            case OpCodeEnum.Conv_Ovf_U2:
                            case OpCodeEnum.Conv_Ovf_U2_Un:
                                {
                                    var obj = esp - 1;
                                    int val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                        case ObjectTypes.Integer:
                                            val = (ushort)obj->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            val = (ushort)*(float*)&obj->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            val = (ushort)*(double*)&obj->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Integer;
                                    obj->Value = val;
                                    obj->ValueLow = 0;
                                }
                                break;
                            case OpCodeEnum.Conv_I2:
                            case OpCodeEnum.Conv_Ovf_I2:
                            case OpCodeEnum.Conv_Ovf_I2_Un:
                                {
                                    var obj = esp - 1;
                                    int val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                        case ObjectTypes.Integer:
                                            val = (short)(obj->Value);
                                            break;
                                        case ObjectTypes.Float:
                                            val = (short)*(float*)&obj->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            val = (short)*(double*)&obj->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Integer;
                                    obj->Value = val;
                                    obj->ValueLow = 0;
                                }
                                break;
                            case OpCodeEnum.Conv_U4:
                            case OpCodeEnum.Conv_U:
                            case OpCodeEnum.Conv_Ovf_U4:
                            case OpCodeEnum.Conv_Ovf_U4_Un:
                                {
                                    var obj = esp - 1;
                                    uint val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            val = (uint)*(long*)&obj->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            val = (uint)obj->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            val = (uint)*(float*)&obj->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            val = (uint)*(double*)&obj->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Integer;
                                    obj->Value = (int)val;
                                    obj->ValueLow = 0;
                                }
                                break;
                            case OpCodeEnum.Conv_I4:
                            case OpCodeEnum.Conv_I:
                            case OpCodeEnum.Conv_Ovf_I:
                            case OpCodeEnum.Conv_Ovf_I_Un:
                            case OpCodeEnum.Conv_Ovf_I4:
                            case OpCodeEnum.Conv_Ovf_I4_Un:
                                {
                                    var obj = esp - 1;
                                    int val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            val = (int)*(long*)&obj->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            val = (int)*(float*)&obj->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            val = (int)*(double*)&obj->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            val = obj->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Integer;
                                    obj->Value = val;
                                }
                                break;
                            case OpCodeEnum.Conv_U8:
                            case OpCodeEnum.Conv_I8:
                            case OpCodeEnum.Conv_Ovf_I8:
                            case OpCodeEnum.Conv_Ovf_I8_Un:
                            case OpCodeEnum.Conv_Ovf_U8:
                            case OpCodeEnum.Conv_Ovf_U8_Un:
                                {
                                    var obj = esp - 1;
                                    long val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            val = obj->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            ip++;
                                            continue;
                                        case ObjectTypes.Float:
                                            val = (long)*(float*)&obj->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            val = (long)*(double*)&obj->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Long;
                                    *(long*)(&obj->Value) = val;
                                }
                                break;
                            case OpCodeEnum.Conv_R4:
                                {
                                    var obj = esp - 1;
                                    float val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            val = (float)*(long*)&obj->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            ip++;
                                            continue;
                                        case ObjectTypes.Double:
                                            val = (float)*(double*)&obj->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            val = obj->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Float;
                                    *(float*)&obj->Value = val;
                                }
                                break;
                            case OpCodeEnum.Conv_R8:
                                {
                                    var obj = esp - 1;
                                    double val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            val = (double)*(long*)&obj->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            val = *(float*)&obj->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            val = obj->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            ip++;
                                            continue;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Double;
                                    *(double*)&obj->Value = val;
                                }
                                break;
                            case OpCodeEnum.Conv_R_Un:
                                {
                                    var obj = esp - 1;
                                    bool isDouble = false;
                                    float val = 0;
                                    double val2 = 0;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            val2 = (double)*(ulong*)&obj->Value;
                                            isDouble = true;
                                            break;
                                        case ObjectTypes.Float:
                                            ip++;
                                            continue;
                                        case ObjectTypes.Integer:
                                            val = (uint)obj->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            ip++;
                                            continue;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (isDouble)
                                    {
                                        obj->ObjectType = ObjectTypes.Double;
                                        *(double*)&obj->Value = val2;
                                    }
                                    else
                                    {
                                        obj->ObjectType = ObjectTypes.Float;
                                        *(float*)&obj->Value = val;
                                    }
                                }
                                break;
                            #endregion

                            #region Stack operation
                            case OpCodeEnum.Pop:
                                {
                                    Free(esp - 1);
                                    esp--;
                                }
                                break;
                            case OpCodeEnum.Dup:
                                {
                                    var obj = esp - 1;
                                    *esp = *obj;
                                    if (esp->ObjectType >= ObjectTypes.Object)
                                    {
                                        esp->Value = mStack.Count;
                                        mStack.Add(mStack[obj->Value]);
                                    }
                                    esp++;
                                }
                                break;
                            #endregion

                            case OpCodeEnum.Throw:
                                {
                                    var obj = GetObjectAndResolveReference(esp - 1);
                                    var ex = mStack[obj->Value] as Exception;
                                    Free(obj);
                                    esp--;
                                    throw ex;
                                }
                            case OpCodeEnum.Nop:
                            case OpCodeEnum.Volatile:
                            case OpCodeEnum.Castclass:
                            case OpCodeEnum.Readonly:
                                break;
                            default:
                                throw new NotSupportedException("Not supported opcode " + code);
                        }
                        ip++;
                    }
                    catch (Exception ex)
                    {
                        if (method.ExceptionHandler != null)
                        {
                            int addr = (int)(ip - ptr);
                            var eh = GetCorrespondingExceptionHandler(method, ex, addr, ExceptionHandlerType.Catch, true);

                            if (eh == null)
                            {
                                eh = GetCorrespondingExceptionHandler(method, ex, addr, ExceptionHandlerType.Catch, false);
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
                                    ex.Data["StackTrace"] = debugger.GetStackTrance(this);
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
                                esp = PushObject(esp, mStack, ex);
                                unhandledException = false;
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
#if UNITY_EDITOR
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

        void DumpStack(StackObject* esp)
        {
            AppDomain.DebugService.DumpStack(esp, stack);
        }

        void CloneStackValueType(StackObject* src, StackObject* dst, IList<object> mStack)
        {
            StackObject* descriptor = *(StackObject**)&src->Value;
            stack.AllocValueType(dst, AppDomain.GetType(descriptor->Value));
            StackObject* dstDescriptor = *(StackObject**)&dst->Value;
            int cnt = descriptor->ValueLow;
            for(int i = 0; i < cnt; i++)
            {
                StackObject* val = Minus(descriptor, i + 1);
                CopyToValueTypeField(dstDescriptor, i, val, mStack);
            }
        }

        void CopyStackValueType(StackObject* src, StackObject* dst, IList<object> mStack)
        {
            StackObject* descriptor = *(StackObject**)&src->Value;
            StackObject* dstDescriptor = *(StackObject**)&dst->Value;
            if (descriptor->Value != dstDescriptor->Value)
                throw new InvalidCastException();
            int cnt = descriptor->ValueLow;
            for(int i = 0; i < cnt; i++)
            {
                StackObject* srcVal = Minus(descriptor, i + 1);
                StackObject* dstVal = Minus(dstDescriptor, i + 1);
                if (srcVal->ObjectType != dstVal->ObjectType)
                    throw new NotSupportedException();
                switch (dstVal->ObjectType)
                {
                    case ObjectTypes.Object:
                    case ObjectTypes.ArrayReference:
                    case ObjectTypes.FieldReference:
                        mStack[dstVal->Value] = mStack[srcVal->Value];
                        break;
                    case ObjectTypes.ValueTypeObjectReference:
                        CopyStackValueType(srcVal, dstVal, mStack);
                        break;
                    default:
                        *dstVal = *srcVal;
                        break;
                }
            }
        }

        void CopyValueTypeToStack(StackObject*dst, object ins, IList<object> mStack)
        {
            if (ins is ILTypeInstance)
            {
                ((ILTypeInstance)ins).CopyValueTypeToStack(dst, mStack);
            }
            else
            {
                if (ins is CrossBindingAdaptorType)
                {
                    ((CrossBindingAdaptorType)ins).ILInstance.CopyValueTypeToStack(dst, mStack);
                }
                else
                {
                    var vb = ((CLRType)domain.GetType(dst->Value)).ValueTypeBinder;
                    vb.CopyValueTypeToStack(ins, dst, mStack);
                }
            }
        }

        void CopyToValueTypeField(StackObject* obj, int idx, StackObject* val, IList<object> mStack)
        {
            StackObject* dst = Minus(obj, idx + 1);
            switch (val->ObjectType)
            {
                case ObjectTypes.Null:
                    {
                        mStack[dst->Value] = null;
                    }
                    break;
                case ObjectTypes.Object:
                case ObjectTypes.FieldReference:
                case ObjectTypes.ArrayReference:
                    {
                        if (dst->ObjectType == ObjectTypes.ValueTypeObjectReference)
                        {
                            var ins = mStack[val->Value];
                            dst = *(StackObject**)&dst->Value;

                            CopyValueTypeToStack(dst, ins, mStack);
                        }
                        else
                        {
                            mStack[dst->Value] = CheckAndCloneValueType(mStack[val->Value], domain);                            
                        }
                    }
                    break;
                case ObjectTypes.ValueTypeObjectReference:
                    {
                        if (dst->ObjectType == ObjectTypes.ValueTypeObjectReference)
                        {
                            CopyStackValueType(val, dst, mStack);
                        }
                        else
                            throw new NotImplementedException();
                    }
                    break;
                default:
                    *dst = *val;
                    break;
            }
        }

        void StLocSub(StackObject* esp, StackObject* v, StackObject* bp, int idx, IList<object> mStack)
        {
            switch (esp->ObjectType)
            {
                case ObjectTypes.Null:
                    v->ObjectType = ObjectTypes.Object;
                    v->Value = idx;
                    mStack[idx] = null;
                    break;
                case ObjectTypes.Object:
                case ObjectTypes.FieldReference:
                case ObjectTypes.ArrayReference:
                    if (v->ObjectType == ObjectTypes.ValueTypeObjectReference)
                    {
                        var obj = mStack[esp->Value];
                        if (obj is ILTypeInstance)
                        {
                            var dst = *(StackObject**)&v->Value;
                            ((ILTypeInstance)obj).CopyValueTypeToStack(dst, mStack);
                        }
                        else
                        {
                            var dst = *(StackObject**)&v->Value;
                            var ct = domain.GetType(dst->Value) as CLRType;
                            var binder = ct.ValueTypeBinder;
                            binder.CopyValueTypeToStack(obj, dst, mStack);
                        }                            
                    }
                    else
                    {
                        *v = *esp;
                        mStack[idx] = CheckAndCloneValueType(mStack[v->Value], domain);
                        v->Value = idx;
                    }
                    Free(esp);
                    break;
                case ObjectTypes.ValueTypeObjectReference:
                    if (v->ObjectType == ObjectTypes.ValueTypeObjectReference)
                    {
                        CopyStackValueType(esp, v, mStack);
                    }
                    else
                        throw new NotImplementedException();
                    FreeStackValueType(esp);
                    break;
                default:
                    *v = *esp;
                    mStack[idx] = null;
                    break;
            }
        }

        object RetriveObject(StackObject* esp, IList<object> mStack)
        {
            StackObject* objRef = GetObjectAndResolveReference(esp);
            if (objRef->ObjectType == ObjectTypes.Null)
                return null;
            object obj = null;
            switch (objRef->ObjectType)
            {
                case ObjectTypes.Object:
                    obj = mStack[objRef->Value];
                    break;
                case ObjectTypes.FieldReference:
                    {
                        obj = mStack[objRef->Value];
                        int idx = objRef->ValueLow;
                        if (obj is ILTypeInstance)
                        {
                            obj = ((ILTypeInstance)obj)[idx];
                        }
                        else
                        {
                            var t = AppDomain.GetType(obj.GetType());
                            obj = ((CLRType) t).GetFieldValue(idx, obj);
                        }
                    }
                    break;
                case ObjectTypes.ArrayReference:
                    {
                        Array arr = mStack[objRef->Value] as Array;
                        int idx = objRef->ValueLow;
                        obj = arr.GetValue(idx);
                        obj = obj.GetType().CheckCLRTypes(obj);
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = AppDomain.GetType(objRef->Value);
                        int idx = objRef->ValueLow;
                        if (t is ILType)
                        {
                            obj = ((ILType)t).StaticInstance[idx];
                        }
                        else
                        {
                            obj = ((CLRType)t).GetFieldValue(idx, null);
                        }
                    }
                    break;
                case ObjectTypes.ValueTypeObjectReference:
                    obj = StackObject.ToObject(objRef, domain, mStack);
                    break;
                default:
                    throw new NotImplementedException();
            }
            return obj;
        }

        void ArraySetValue(Array arr, object obj, int idx)
        {
            if (obj == null)
                arr.SetValue(null, idx);
            else
                arr.SetValue(arr.GetType().GetElementType().CheckCLRTypes(obj), idx);
        }

        void StoreIntValueToArray(Array arr, StackObject* val, StackObject* idx)
        {
            {
                int[] tmp = arr as int[];
                if (tmp != null)
                {
                    tmp[idx->Value] = val->Value;
                    return;
                }
            }
            {
                short[] tmp = arr as short[];
                if (tmp != null)
                {
                    tmp[idx->Value] = (short)val->Value;
                    return;
                }
            }
            {
                byte[] tmp = arr as byte[];
                if (tmp != null)
                {
                    tmp[idx->Value] = (byte)val->Value;
                    return;
                }
            }
            {
                bool[] tmp = arr as bool[];
                if (tmp != null)
                {
                    tmp[idx->Value] = val->Value == 1;
                    return;
                }
            }
            {
                uint[] tmp = arr as uint[];
                if (tmp != null)
                {
                    tmp[idx->Value] = (uint)val->Value;
                    return;
                }
            }
            {
                ushort[] tmp = arr as ushort[];
                if (tmp != null)
                {
                    tmp[idx->Value] = (ushort)val->Value;
                    return;
                }
            }
            {
                char[] tmp = arr as char[];
                if (tmp != null)
                {
                    tmp[idx->Value] = (char)val->Value;
                    return;
                }
            }
            {
                sbyte[] tmp = arr as sbyte[];
                if (tmp != null)
                {
                    tmp[idx->Value] = (sbyte)val->Value;
                    return;
                }
            }
            throw new NotImplementedException();
        }

        ExceptionHandler GetCorrespondingExceptionHandler(ILMethod method, object obj, int addr, ExceptionHandlerType type, bool explicitMatch)
        {
            ExceptionHandler res = null;
            int distance = int.MaxValue;
            Exception ex = obj is ILRuntimeException ? ((ILRuntimeException)obj).InnerException : obj as Exception;
            foreach (var i in method.ExceptionHandler)
            {
                if (i.HandlerType == type)
                {
                    if (addr >= i.TryStart && addr <= i.TryEnd)
                    {
                        if (CheckExceptionType(i.CatchType, ex, explicitMatch))
                        {
                            int d = addr - i.TryStart;
                            if (d < distance)
                            {
                                distance = d;
                                res = i;
                            }
                        }
                    }
                }
            }
            return res;
        }

        void LoadFromFieldReference(object obj, int idx, StackObject* dst, IList<object> mStack)
        {
            if (obj is ILTypeInstance)
            {
                ((ILTypeInstance)obj).PushToStack(idx, dst, AppDomain, mStack);
            }
            else
            {
                CLRType t = AppDomain.GetType(obj.GetType()) as CLRType;
                ILIntepreter.PushObject(dst, mStack, t.GetFieldValue(idx, obj));
            }
        }

        void StoreValueToFieldReference(object obj, int idx, StackObject* val, IList<object> mStack)
        {
            if (obj is ILTypeInstance)
            {
                ((ILTypeInstance)obj).AssignFromStack(idx, val, AppDomain, mStack);
            }
            else
            {
                CLRType t = AppDomain.GetType(obj.GetType()) as CLRType;
                var v = obj.GetType().CheckCLRTypes(CheckAndCloneValueType(StackObject.ToObject(val, AppDomain, mStack), AppDomain));
                t.SetFieldValue(idx, ref obj, v);
            }
        }

        void LoadFromArrayReference(object obj, int idx, StackObject* objRef, IType t, IList<object> mStack)
        {
            var nT = t.TypeForCLR;
            LoadFromArrayReference(obj, idx, objRef, nT, mStack);
        }

        void LoadFromArrayReference(object obj, int idx, StackObject* objRef, Type nT, IList<object> mStack)
        {
            if (nT.IsPrimitive)
            {
                if (nT == typeof(int))
                {
                    int[] arr = obj as int[];
                    objRef->ObjectType = ObjectTypes.Integer;
                    objRef->Value = arr[idx];
                    objRef->ValueLow = 0;
                }
                else if (nT == typeof(short))
                {
                    short[] arr = obj as short[];
                    objRef->ObjectType = ObjectTypes.Integer;
                    objRef->Value = arr[idx];
                    objRef->ValueLow = 0;
                }
                else if (nT == typeof(long))
                {
                    long[] arr = obj as long[];
                    objRef->ObjectType = ObjectTypes.Long;
                    *(long*)&objRef->Value = arr[idx];
                }
                else if (nT == typeof(float))
                {
                    float[] arr = obj as float[];
                    objRef->ObjectType = ObjectTypes.Float;
                    *(float*)&objRef->Value = arr[idx];
                    objRef->ValueLow = 0;
                }
                else if (nT == typeof(double))
                {
                    double[] arr = obj as double[];
                    objRef->ObjectType = ObjectTypes.Double;
                    *(double*)&objRef->Value = arr[idx];
                }
                else if (nT == typeof(byte))
                {
                    byte[] arr = obj as byte[];
                    objRef->ObjectType = ObjectTypes.Integer;
                    objRef->Value = arr[idx];
                    objRef->ValueLow = 0;
                }
                else if (nT == typeof(char))
                {
                    char[] arr = obj as char[];
                    objRef->ObjectType = ObjectTypes.Integer;
                    objRef->Value = arr[idx];
                    objRef->ValueLow = 0;
                }
                else if (nT == typeof(uint))
                {
                    uint[] arr = obj as uint[];
                    objRef->ObjectType = ObjectTypes.Integer;
                    *(uint*)&objRef->Value = arr[idx];
                    objRef->ValueLow = 0;
                }
                else if (nT == typeof(sbyte))
                {
                    sbyte[] arr = obj as sbyte[];
                    objRef->ObjectType = ObjectTypes.Integer;
                    objRef->Value = arr[idx];
                    objRef->ValueLow = 0;
                }
                else if (nT == typeof(ulong))
                {
                    ulong[] arr = obj as ulong[];
                    objRef->ObjectType = ObjectTypes.Long;
                    *(ulong*)&objRef->Value = arr[idx];
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                Array arr = obj as Array;
                objRef->ObjectType = ObjectTypes.Object;
                objRef->Value = mStack.Count;
                mStack.Add(arr.GetValue(idx));
                objRef->ValueLow = 0;
            }
        }

        void StoreValueToArrayReference(StackObject* objRef, StackObject* val, IType t, IList<object> mStack)
        {
            var nT = t.TypeForCLR;
            StoreValueToArrayReference(objRef, val, nT, mStack);
        }

        void StoreValueToArrayReference(StackObject* objRef, StackObject* val, Type nT, IList<object> mStack)
        {
            if (nT.IsPrimitive)
            {
                if (nT == typeof(int))
                {
                    int[] arr = mStack[objRef->Value] as int[];
                    arr[objRef->ValueLow] = val->Value;
                }
                else if (nT == typeof(short))
                {
                    short[] arr = mStack[objRef->Value] as short[];
                    arr[objRef->ValueLow] = (short)val->Value;
                }
                else if (nT == typeof(long))
                {
                    long[] arr = mStack[objRef->Value] as long[];
                    arr[objRef->ValueLow] = *(long*)&val->Value;
                }
                else if (nT == typeof(float))
                {
                    float[] arr = mStack[objRef->Value] as float[];
                    arr[objRef->ValueLow] = *(float*)&val->Value;
                }
                else if (nT == typeof(double))
                {
                    double[] arr = mStack[objRef->Value] as double[];
                    arr[objRef->ValueLow] = *(double*)&val->Value;
                }
                else if (nT == typeof(byte))
                {
                    byte[] arr = mStack[objRef->Value] as byte[];
                    arr[objRef->ValueLow] = (byte)val->Value;
                }
                else if (nT == typeof(char))
                {
                    char[] arr = mStack[objRef->Value] as char[];
                    arr[objRef->ValueLow] = (char)val->Value;
                }
                else if (nT == typeof(uint))
                {
                    uint[] arr = mStack[objRef->Value] as uint[];
                    arr[objRef->ValueLow] = (uint)val->Value;
                }
                else if (nT == typeof(sbyte))
                {
                    sbyte[] arr = mStack[objRef->Value] as sbyte[];
                    arr[objRef->ValueLow] = (sbyte)val->Value;
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                Array arr = mStack[objRef->Value] as Array;
                arr.SetValue(mStack[val->Value], objRef->ValueLow);
            }
        }

        bool CheckExceptionType(IType catchType, object exception, bool explicitMatch)
        {
            if (catchType is CLRType)
            {
                if (explicitMatch)
                    return exception.GetType() == catchType.TypeForCLR;
                else
                    return catchType.TypeForCLR.IsAssignableFrom(exception.GetType());
            }
            else
                throw new NotImplementedException();
        }

        public static StackObject* GetObjectAndResolveReference(StackObject* esp)
        {
            if (esp->ObjectType == ObjectTypes.StackObjectReference)
            {
                return *(StackObject**)&esp->Value;
            }
            else
                return esp;
        }

        StackObject* PushParameters(IMethod method, StackObject* esp, object[] p)
        {
            IList<object> mStack = stack.ManagedStack;
            var plist = method.Parameters;
            int pCnt = plist != null ? plist.Count : 0;
            int pCnt2 = p != null ? p.Length : 0;
            if (pCnt != pCnt2)
                throw new ArgumentOutOfRangeException("Parameter mismatch");
            if (pCnt2 > 0)
            {
                for (int i = 0; i < p.Length; i++)
                {
                    bool isBox = false;
                    if (plist != null && i < plist.Count)
                        isBox = plist[i] == AppDomain.ObjectType;
                    object obj = p[i];
                    if (obj is CrossBindingAdaptorType)
                        obj = ((CrossBindingAdaptorType)obj).ILInstance;
                    esp = ILIntepreter.PushObject(esp, mStack, obj, isBox);
                }
            }
            return esp;
        }

        public void CopyToStack(StackObject* dst, StackObject* src, IList<object> mStack)
        {
            *dst = *src;
            if (dst->ObjectType >= ObjectTypes.Object)
            {
                dst->Value = mStack.Count;
                var obj = mStack[src->Value];
                mStack.Add(obj);
            }
        }

        internal static object CheckAndCloneValueType(object obj, Enviorment.AppDomain domain)
        {
            if (obj != null)
            {
                if (obj is ILTypeInstance)
                {
                    ILTypeInstance ins = obj as ILTypeInstance;
                    if (ins.IsValueType)
                    {
                        return ins.Clone();
                    }
                }
                else
                {
                    var type = obj.GetType();
                    var typeFlags = type.GetTypeFlags();

                    var isPrimitive = (typeFlags & CLR.Utils.Extensions.TypeFlags.IsPrimitive) != 0;
                    var isValueType = (typeFlags & CLR.Utils.Extensions.TypeFlags.IsValueType) != 0;

                    if (!isPrimitive && isValueType)
                    {
                        var t = domain.GetType(type);
                        return ((CLRType)t).PerformMemberwiseClone(obj);
                    }
                }
            }
            return obj;
        }
        public static StackObject* PushOne(StackObject* esp)
        {
            esp->ObjectType = ObjectTypes.Integer;
            esp->Value = 1;
            return esp + 1;
        }

        public static StackObject* PushZero(StackObject* esp)
        {
            esp->ObjectType = ObjectTypes.Integer;
            esp->Value = 0;
            return esp + 1;
        }

        public static StackObject* PushNull(StackObject* esp)
        {
            esp->ObjectType = ObjectTypes.Null;
            esp->Value = -1;
            esp->ValueLow = 0;
            return esp + 1;
        }

        public static void UnboxObject(StackObject* esp, object obj, IList<object> mStack = null, Enviorment.AppDomain domain = null)
        {
            if (esp->ObjectType == ObjectTypes.ValueTypeObjectReference && domain != null)
            {
                var dst = *(StackObject**)&esp->Value;
                var vt = domain.GetType(dst->Value);

                if (obj is ILTypeInstance)
                {
                    var ins = (ILTypeInstance)obj;
                    ins.CopyValueTypeToStack(dst, mStack);
                }
                else if (obj is CrossBindingAdaptorType)
                {
                    var ins = ((CrossBindingAdaptorType)obj).ILInstance;
                    ins.CopyValueTypeToStack(dst, mStack);
                }
                else
                {
                    ((CLRType)vt).ValueTypeBinder.CopyValueTypeToStack(obj, dst, mStack);
                }
            }
            else if (obj is int)
            {
                esp->ObjectType = ObjectTypes.Integer;
                esp->Value = (int)obj;
            }
            else if (obj is bool)
            {
                esp->ObjectType = ObjectTypes.Integer;
                esp->Value = (bool)(obj) ? 1 : 0;
            }
            else if (obj is short)
            {
                esp->ObjectType = ObjectTypes.Integer;
                esp->Value = (short)obj;
            }
            else if (obj is long)
            {
                esp->ObjectType = ObjectTypes.Long;
                *(long*)(&esp->Value) = (long)obj;
            }
            else if (obj is float)
            {
                esp->ObjectType = ObjectTypes.Float;
                *(float*)(&esp->Value) = (float)obj;
            }
            else if (obj is byte)
            {
                esp->ObjectType = ObjectTypes.Integer;
                esp->Value = (byte)obj;
            }
            else if (obj is uint)
            {
                esp->ObjectType = ObjectTypes.Integer;
                esp->Value = (int)(uint)obj;
            }
            else if (obj is ushort)
            {
                esp->ObjectType = ObjectTypes.Integer;
                esp->Value = (int)(ushort)obj;
            }
            else if (obj is char)
            {
                esp->ObjectType = ObjectTypes.Integer;
                esp->Value = (int)(char)obj;
            }
            else if (obj is double)
            {
                esp->ObjectType = ObjectTypes.Double;
                *(double*)(&esp->Value) = (double)obj;
            }
            else if (obj is ulong)
            {
                esp->ObjectType = ObjectTypes.Long;
                *(ulong*)(&esp->Value) = (ulong)obj;
            }
            else if (obj is sbyte)
            {
                esp->ObjectType = ObjectTypes.Integer;
                esp->Value = (sbyte)obj;
            }            
            else
                throw new NotImplementedException();
        }

        public static StackObject* PushObject(StackObject* esp, IList<object> mStack, object obj, bool isBox = false)
        {
            if (obj != null)
            {
                if (!isBox)
                {
                    var typeFlags = obj.GetType().GetTypeFlags();

                    if ((typeFlags & CLR.Utils.Extensions.TypeFlags.IsPrimitive) != 0)
                    {
                        UnboxObject(esp, obj, mStack);
                    }
                    else if ((typeFlags & CLR.Utils.Extensions.TypeFlags.IsEnum) != 0)
                    {
                        esp->ObjectType = ObjectTypes.Integer;
                        esp->Value = Convert.ToInt32(obj);
                    }
                    else
                    {
                        esp->ObjectType = ObjectTypes.Object;
                        esp->Value = mStack.Count;
                        mStack.Add(obj);
                    }
                }
                else
                {
                    esp->ObjectType = ObjectTypes.Object;
                    esp->Value = mStack.Count;
                    mStack.Add(obj);
                }
            }
            else
            {
                return PushNull(esp);
            }
            return esp + 1;
        }

        //Don't ask me why add this funky method for this, otherwise Unity won't calculate the right value
        public static StackObject* Add(StackObject* a, int b)
        {
            return (StackObject*)((long)a + sizeof(StackObject) * b);
        }

        public static StackObject* Minus(StackObject* a, int b)
        {
            return (StackObject*)((long)a - sizeof(StackObject) * b);
        }

        public void Free(StackObject* esp)
        {
            if (esp->ObjectType >= ObjectTypes.Object)
            {
                if (esp->Value == stack.ManagedStack.Count - 1)
                    stack.ManagedStack.RemoveAt(esp->Value);
            }
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
            esp->ObjectType = ObjectTypes.Null;
            esp->Value = -1;
            esp->ValueLow = 0;
#endif
        }
        public void FreeStackValueType(StackObject* esp)
        {
            if (esp->ObjectType == ObjectTypes.ValueTypeObjectReference)
            {
                var addr = *(StackObject**)&esp->Value;
                if (addr <= ValueTypeBasePointer)//Only Stack allocation after base pointer should be freed, local variable are freed automatically
                    stack.FreeValueTypeObject(esp);
            }
        }

        public void AllocValueType(StackObject* ptr, IType type)
        {
            stack.AllocValueType(ptr, type);
        }
    }
}
