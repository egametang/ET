using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Other;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntime.Runtime.Stack
{
    unsafe class RuntimeStack : IDisposable
    {
        ILIntepreter intepreter;
        StackObject* pointer;
        StackObject* endOfMemory;
        StackObject* valueTypePtr;

        IntPtr nativePointer;

#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
        IList<object> managedStack = new List<object>(32);
#else
        IList<object> managedStack = new UncheckedList<object>(32);
#endif

        Stack<StackFrame> frames = new Stack<StackFrame>();
        const int MAXIMAL_STACK_OBJECTS = 1024 * 16;

        public Stack<StackFrame> Frames { get { return frames; } }
        public RuntimeStack(ILIntepreter intepreter)
        {
            this.intepreter = intepreter;

            nativePointer = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(StackObject) * MAXIMAL_STACK_OBJECTS);
            pointer = (StackObject*)nativePointer.ToPointer();
            endOfMemory = Add(pointer, MAXIMAL_STACK_OBJECTS);
            valueTypePtr = endOfMemory - 1;
        }

        ~RuntimeStack()
        {
            Dispose();
        }

        public StackObject* StackBase
        {
            get
            {
                return pointer;
            }
        }

        public StackObject* ValueTypeStackPointer
        {
            get
            {
                return valueTypePtr;
            }
        }

        public StackObject* ValueTypeStackBase
        {
            get
            {
                return endOfMemory - 1;
            }
        }

        public IList<object> ManagedStack { get { return managedStack; } }

        public void ResetValueTypePointer()
        {
            valueTypePtr = endOfMemory - 1;
        }

        public void InitializeFrame(ILMethod method, StackObject* esp, out StackFrame res)
        {
            if (esp < pointer || esp >= endOfMemory)
                throw new StackOverflowException();
            if (frames.Count > 0 && frames.Peek().BasePointer > esp)
                throw new StackOverflowException();
            res = new StackFrame();
            res.LocalVarPointer = esp;
            res.Method = method;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
            res.Address = new IntegerReference();
            for (int i = 0; i < method.LocalVariableCount; i++)
            {
                var p = Add(esp, i);
                p->ObjectType = ObjectTypes.Null;
            }
#endif
            res.BasePointer = method.LocalVariableCount > 0 ? Add(esp, method.LocalVariableCount) : esp;
            res.ManagedStackBase = managedStack.Count;
            res.ValueTypeBasePointer = valueTypePtr;
            //frames.Push(res);
        }
        public void PushFrame(ref StackFrame frame)
        {
            frames.Push(frame);
        }

        public StackObject* PopFrame(ref StackFrame frame, StackObject* esp)
        {
            if (frames.Count > 0 && frames.Peek().BasePointer == frame.BasePointer)
                frames.Pop();
            else
                throw new NotSupportedException();
            StackObject* returnVal = esp - 1;
            var method = frame.Method;
            StackObject* ret = ILIntepreter.Minus(frame.LocalVarPointer, method.ParameterCount);
            int mStackBase = frame.ManagedStackBase;
            if (method.HasThis)
                ret--;
            if(method.ReturnType != intepreter.AppDomain.VoidType)
            {
                *ret = *returnVal;
                if(ret->ObjectType == ObjectTypes.Object)
                {
                    ret->Value = mStackBase;
                    managedStack[mStackBase] = managedStack[returnVal->Value];
                    mStackBase++;
                }
                else if(ret->ObjectType == ObjectTypes.ValueTypeObjectReference)
                {
                    StackObject* oriAddr = frame.ValueTypeBasePointer;
                    RelocateValueType(ret, ref frame.ValueTypeBasePointer, ref mStackBase);
                    *(StackObject**)&ret->Value = oriAddr;
                }
                ret++;
            }
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
            ((List<object>)managedStack).RemoveRange(mStackBase, managedStack.Count - mStackBase);
#else
            ((UncheckedList<object>)managedStack).RemoveRange(mStackBase, managedStack.Count - mStackBase);
#endif
            valueTypePtr = frame.ValueTypeBasePointer;
            return ret;
        }

        void RelocateValueType(StackObject* src, ref StackObject* dst, ref int mStackBase)
        {
            StackObject* descriptor = *(StackObject**)&src->Value;
            if (descriptor > dst)
                throw new StackOverflowException();
            *dst = *descriptor;
            int cnt = descriptor->ValueLow;
            StackObject* endAddr = ILIntepreter.Minus(dst, cnt + 1);
            for(int i = 0; i < cnt; i++)
            {
                StackObject* addr = ILIntepreter.Minus(descriptor, i + 1);
                StackObject* tarVal = ILIntepreter.Minus(dst, i + 1);
                *tarVal = *addr;
                switch (addr->ObjectType)
                {
                    case ObjectTypes.Object:
                    case ObjectTypes.ArrayReference:
                    case ObjectTypes.FieldReference:
                        if (tarVal->Value >= mStackBase)
                        {
                            int oldIdx = addr->Value;
                            tarVal->Value = mStackBase;
                            managedStack[mStackBase] = managedStack[oldIdx];
                            mStackBase++;
                        }
                        break;
                    case ObjectTypes.ValueTypeObjectReference:
                        var newAddr = endAddr;
                        RelocateValueType(addr, ref endAddr, ref mStackBase);
                        *(StackObject**)&tarVal->Value = newAddr;
                        break;
                }
            }
            dst = endAddr;
        }

        public void AllocValueType(StackObject* ptr, IType type)
        {
            if (type.IsValueType)
            {
                int fieldCount = 0;
                if(type is ILType)
                {
                    fieldCount = ((ILType)type).TotalFieldCount;
                }
                else
                {
                    fieldCount = ((CLRType)type).TotalFieldCount;
                }
                ptr->ObjectType = ObjectTypes.ValueTypeObjectReference;
                var dst = valueTypePtr;
                *(StackObject**)&ptr->Value = dst;
                dst->ObjectType = ObjectTypes.ValueTypeDescriptor;
                dst->Value = type.GetHashCode();
                dst->ValueLow = fieldCount;
                valueTypePtr = ILIntepreter.Minus(valueTypePtr, fieldCount + 1);
                if (valueTypePtr <= StackBase)
                    throw new StackOverflowException();
                InitializeValueTypeObject(type, dst);
            }
            else
                throw new ArgumentException(type.FullName + " is not a value type.", "type");
        }

        void InitializeValueTypeObject(IType type, StackObject* ptr)
        {
            if (type is ILType)
            {
                ILType t = (ILType)type;
                for (int i = 0; i < t.FieldTypes.Length; i++)
                {
                    var ft = t.FieldTypes[i];
                    StackObject* val = ILIntepreter.Minus(ptr, t.FieldStartIndex + i + 1);
                    if (ft.IsPrimitive)
                        StackObject.Initialized(val, ft);
                    else
                    {
                        if (ft.IsValueType)
                        {
                            if (ft is ILType || ((CLRType)ft).ValueTypeBinder != null)
                                AllocValueType(val, ft);
                            else
                            {
                                val->ObjectType = ObjectTypes.Object;
                                val->Value = managedStack.Count;
                                managedStack.Add(((CLRType)ft).CreateDefaultInstance());
                            }
                        }
                        else
                        {
                            val->ObjectType = ObjectTypes.Object;
                            val->Value = managedStack.Count;
                            managedStack.Add(null);
                        }
                    }
                }
                if (type.BaseType != null && type.BaseType is ILType)
                    InitializeValueTypeObject((ILType)type.BaseType, ptr);
            }
            else
            {
                CLRType t = (CLRType)type;
                var cnt = t.TotalFieldCount;
                for(int i = 0; i < cnt; i++)
                {
                    var it = t.OrderedFieldTypes[i] as CLRType;
                    StackObject* val = ILIntepreter.Minus(ptr, i + 1);
                    if (it.IsPrimitive)
                        StackObject.Initialized(val, it);
                    else
                    {
                        if (it.IsValueType)
                        {
                            if (it.ValueTypeBinder != null)
                                AllocValueType(val, it);
                            else
                            {
                                val->ObjectType = ObjectTypes.Object;
                                val->Value = managedStack.Count;
                                managedStack.Add(it.CreateDefaultInstance());
                            }
                        }
                        else
                        {
                            val->ObjectType = ObjectTypes.Object;
                            val->Value = managedStack.Count;
                            managedStack.Add(null);
                        }
                    }
                }
            }
        }

        public void ClearValueTypeObject(IType type, StackObject* ptr)
        {
            if (type is ILType)
            {
                ILType t = (ILType)type;
                for (int i = 0; i < t.FieldTypes.Length; i++)
                {
                    var ft = t.FieldTypes[i];
                    StackObject* val = ILIntepreter.Minus(ptr, t.FieldStartIndex + i + 1);
                    if (ft.IsPrimitive)
                        StackObject.Initialized(val, ft);
                    else
                    {
                        switch (val->ObjectType)
                        {
                            case ObjectTypes.ValueTypeObjectReference:
                                ClearValueTypeObject(ft, *(StackObject**)&val->Value);
                                break;
                            default:
                                if (ft.IsValueType)
                                {
                                    if(ft is ILType)
                                    {
                                        throw new NotImplementedException();
                                    }
                                    else
                                    {
                                        managedStack[val->Value] = ((CLRType)ft).CreateDefaultInstance();
                                    }
                                }
                                else
                                    managedStack[val->Value] = null;
                                break;
                        }
                    }
                }
                if (type.BaseType != null && type.BaseType is ILType)
                    ClearValueTypeObject((ILType)type.BaseType, ptr);
            }
            else
            {
                CLRType t = (CLRType)type;
                var cnt = t.TotalFieldCount;
                for (int i = 0; i < cnt; i++)
                {
                    var vt = t.OrderedFieldTypes[i] as CLRType;
                    StackObject* val = ILIntepreter.Minus(ptr, i + 1);
                    if (vt.IsPrimitive)
                        StackObject.Initialized(val, vt);
                    else
                    {
                        switch (val->ObjectType)
                        {
                            case ObjectTypes.ValueTypeObjectReference:
                                {
                                    var dst = *(StackObject**)&val->Value;
                                    ClearValueTypeObject(vt, dst);
                                }
                                break;
                            default:
                                if (vt.IsValueType)
                                {
                                    managedStack[val->Value] = vt.CreateDefaultInstance();
                                }
                                else
                                    managedStack[val->Value] = null;
                                break;
                        }
                    }
                }
            }
        }

        public void FreeValueTypeObject(StackObject* esp)
        {
            if (esp->ObjectType != ObjectTypes.ValueTypeObjectReference)
                return;
            int start = int.MaxValue;
            int end = int.MinValue;
            StackObject* endAddr;
            CountValueTypeManaged(esp, ref start, ref end, &endAddr);

            if (endAddr == valueTypePtr)
                valueTypePtr = *(StackObject**)&esp->Value;
            else
                throw new NotSupportedException();
            if (start != int.MaxValue)
            {
                if (end == managedStack.Count - 1)
                {
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                    ((List<object>)managedStack).RemoveRange(start, managedStack.Count - start);
#else
                    ((UncheckedList<object>)managedStack).RemoveRange(start, managedStack.Count - start);
#endif
                }
                else
                    throw new NotSupportedException();
            }
        }

        void CountValueTypeManaged(StackObject* esp, ref int start, ref int end, StackObject** endAddr)
        {
            StackObject* descriptor = *(StackObject**)&esp->Value;
            int cnt = descriptor->ValueLow;
            *endAddr = ILIntepreter.Minus(descriptor, cnt + 1);
            for (int i = 0; i < cnt; i++)
            {
                StackObject* addr = ILIntepreter.Minus(descriptor, i + 1);
                switch (addr->ObjectType)
                {
                    case ObjectTypes.Object:
                    case ObjectTypes.ArrayReference:
                    case ObjectTypes.FieldReference:
                        {
                            if (start == int.MaxValue)
                            {
                                start = addr->Value;
                                end = start;
                            }
                            else if (addr->Value == end + 1)
                                end++;
                            else
                                throw new NotSupportedException();
                        }
                        break;
                    case ObjectTypes.ValueTypeObjectReference:
                        CountValueTypeManaged(addr, ref start, ref end, endAddr);
                        break;
                }

            }
        }

        public void Dispose()
        {
            if (nativePointer != IntPtr.Zero)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(nativePointer);
                nativePointer = IntPtr.Zero;
            }
        }

        StackObject* Add(StackObject* a, int b)
        {
            return (StackObject*)((long)a + sizeof(StackObject) * b);
        }
    }
}
