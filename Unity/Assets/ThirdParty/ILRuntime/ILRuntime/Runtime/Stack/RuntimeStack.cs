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
        StackObjectAllocator allocator;
        IntPtr nativePointer;

#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
        IList<object> managedStack = new List<object>(32);
#else
        IList<object> managedStack = new UncheckedList<object>(32);
#endif
        UncheckedStack<StackFrame> frames = new UncheckedStack<StackFrame>();
        public const int MAXIMAL_STACK_OBJECTS = 1024 * 16;

        public UncheckedStack<StackFrame> Frames { get { return frames; } }
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
            internal set
            {
                if (value > ValueTypeStackBase)
                    throw new StackOverflowException();
                valueTypePtr = value;
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
#if DEBUG
            if (esp < pointer || esp >= endOfMemory)
                throw new StackOverflowException();
            if (frames.Count > 0 && frames.Peek().BasePointer > esp)
                throw new StackOverflowException();
#endif
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
            frames.Push(ref frame);
        }

        public StackObject* PopFrame(ref StackFrame frame, StackObject* esp)
        {
#if DEBUG
            if (frames.Count > 0 && frames.Peek().BasePointer == frame.BasePointer)
#endif
                frames.Pop();
#if DEBUG
            else
                throw new NotSupportedException();
#endif
            StackObject* returnVal = esp - 1;
            var method = frame.Method;
            StackObject* ret = ILIntepreter.Minus(frame.LocalVarPointer, method.ParameterCount);
            int mStackBase = frame.ManagedStackBase;
            if (method.HasThis)
                ret--;
            if (allocator != null)
                allocator.FreeBefore(frame.ValueTypeBasePointer);
            for (StackObject* ptr = ret; ptr < frame.LocalVarPointer; ptr++)
            {
                if (ptr->ObjectType == ObjectTypes.ValueTypeObjectReference)
                {
                    var addr = ILIntepreter.ResolveReference(ptr);
                    int start = int.MaxValue;
                    int end = int.MaxValue;
                    var tmp = addr;
                    CountValueTypeManaged(ptr, ref start, ref end, &tmp);

                    if (addr > frame.ValueTypeBasePointer)
                    {
                        frame.ValueTypeBasePointer = addr;
                    }
                    if (start < mStackBase)
                        mStackBase = start;
                }
            }
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
                    *(long*)&ret->Value = (long)oriAddr;
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

        public void RelocateValueTypeAndFreeAfterDst(StackObject* src, StackObject* dst)
        {
            var objRef2 = dst;
            dst = ILIntepreter.ResolveReference(dst);
            int start = int.MaxValue;
            int end = int.MaxValue;
            CountValueTypeManaged(objRef2, ref start, ref end, &objRef2);
            RelocateValueType(src, ref dst, ref start);
            ValueTypeStackPointer = dst;
            if (start <= end)
                RemoveManagedStackRange(start, end);
        }

        void RelocateValueType(StackObject* src, ref StackObject* dst, ref int mStackBase)
        {
            StackObject* descriptor = ILIntepreter.ResolveReference(src);
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
                        *(long*)&tarVal->Value = (long)newAddr;
                        break;
                }
            }
            dst = endAddr;
        }

        int CountValueTypeManaged(IType type)
        {
            int res = 0;
            if (type is ILType)
            {
                ILType t = (ILType)type;
                for (int i = 0; i < t.FieldTypes.Length; i++)
                {
                    var ft = t.FieldTypes[i];
                    if (!ft.IsPrimitive && !ft.IsEnum)
                    {
                        if (ft.IsValueType)
                        {
                            if (!(ft is ILType) && ((CLRType)ft).ValueTypeBinder == null)
                            {
                                res++;
                            }
                        }
                        else
                        {
                            res++;
                        }
                    }
                }
                if (type.BaseType != null && type.BaseType is ILType)
                    res += CountValueTypeManaged((ILType)type.BaseType);
            }
            else
            {
                CLRType t = (CLRType)type;
                var cnt = t.TotalFieldCount;
                for (int i = 0; i < cnt; i++)
                {
                    var it = t.OrderedFieldTypes[i] as CLRType;
                    if (!it.IsPrimitive && it.IsEnum)
                    {
                        if (it.IsValueType)
                        {
                            if (it.ValueTypeBinder == null)
                            {
                                res++;
                            }
                        }
                        else
                        {
                            res++;
                        }
                    }
                }
            }
            return res;
        }

        void AllocBlock(int size, out StackObject* dst, out int managedIdx)
        {
            dst = valueTypePtr;
            valueTypePtr = ILIntepreter.Minus(valueTypePtr, size);
            if (valueTypePtr <= StackBase)
                throw new StackOverflowException();
            managedIdx = managedStack.Count;            
        }

        public void ClearAllocator()
        {
            if (allocator != null)
                allocator.Clear();
        }

        public void AllocValueTypeAndCopy(StackObject* ptr, StackObject* src)
        {
            var dst = ILIntepreter.ResolveReference(src);
            var type = intepreter.AppDomain.GetTypeByIndex(dst->Value);
            int size, managedCount;
            type.GetValueTypeSize(out size, out managedCount);
            if (allocator == null)
                allocator = new StackObjectAllocator(AllocBlock);
            StackObjectAllocation alloc;
            if(allocator.AllocExisting(ptr, size, managedCount, out alloc))
            {
                if (dst != alloc.Address)
                {
                    dst = alloc.Address;
                    ptr->ObjectType = ObjectTypes.ValueTypeObjectReference;
                    *(long*)&ptr->Value = (long)dst;
                    int managedIdx = alloc.ManagedIndex;
                    InitializeValueTypeObject(type, dst, true, ref managedIdx);
                    intepreter.CopyStackValueType(src, ptr, managedStack);
                    FreeValueTypeObject(src);
                }
                else
                {
                    ptr->ObjectType = ObjectTypes.ValueTypeObjectReference;
                    *(long*)&ptr->Value = (long)dst;
                }
            }
            else
            {
                int start = int.MaxValue;
                int end = int.MinValue;
                StackObject* endAddr;
                CountValueTypeManaged(src, ref start, ref end, &endAddr);
                if (endAddr == valueTypePtr)
                    valueTypePtr = dst;
                allocator.RegisterAllocation(ptr, dst, size, start, managedCount);
                ptr->ObjectType = ObjectTypes.ValueTypeObjectReference;
                *(long*)&ptr->Value = (long)dst;
            }
        }

        public void AllocValueType(StackObject* ptr, IType type, bool register = false)
        {
            if (type.IsValueType)
            {
                StackObject* dst;
                int size, managedCount;
                type.GetValueTypeSize(out size, out managedCount);
                int managedIdx = -1;
                if (register)
                {
                    if (allocator == null)
                        allocator = new StackObjectAllocator(AllocBlock);
                    var allocation = allocator.Alloc(ptr, size, managedCount);
                    dst = allocation.Address;
                    managedIdx = allocation.ManagedIndex;
                }
                else
                {
                    dst = valueTypePtr;
                    managedIdx = managedStack.Count;

                    valueTypePtr = ILIntepreter.Minus(valueTypePtr, size);
                    if (valueTypePtr <= StackBase)
                        throw new StackOverflowException();
                }

                ptr->ObjectType = ObjectTypes.ValueTypeObjectReference;
                *(long*)&ptr->Value = (long)dst;
                InitializeValueTypeObject(type, dst, register, ref managedIdx);
            }
            else
                throw new ArgumentException(type.FullName + " is not a value type.", "type");
        }

        internal void InitializeValueTypeObject(IType type, StackObject* ptr, bool register, ref int managedIdx)
        {
            ptr->ObjectType = ObjectTypes.ValueTypeDescriptor;
            ptr->Value = type.TypeIndex;
            ptr->ValueLow = type.TotalFieldCount;
            StackObject* endPtr = ptr - (type.TotalFieldCount + 1);
            
            if (type is ILType)
            {
                ILType t = (ILType)type;
                for (int i = 0; i < t.FieldTypes.Length; i++)
                {
                    var ft = t.FieldTypes[i];
                    StackObject* val = ILIntepreter.Minus(ptr, t.FieldStartIndex + i + 1);
                    if (ft.IsPrimitive)
                        *val = ft.DefaultObject;
                    else if (ft.IsEnum)
                        StackObject.Initialized(val, ft);
                    else
                    {
                        if (ft.IsValueType)
                        {
                            if (ft is ILType || ((CLRType)ft).ValueTypeBinder != null)
                            {
                                val->ObjectType = ObjectTypes.ValueTypeObjectReference;
                                *(long*)&val->Value = (long)endPtr;
                                InitializeValueTypeObject(ft, endPtr, register, ref managedIdx);
                                int size, mCnt;
                                ft.GetValueTypeSize(out size, out mCnt);
                                endPtr -= size;
                            }
                            else
                            {
                                val->ObjectType = ObjectTypes.Object;
                                val->Value = managedIdx;
                                if (managedIdx < managedStack.Count)
                                    managedStack[managedIdx] = ((CLRType)ft).CreateDefaultInstance();
                                else
                                    managedStack.Add(((CLRType)ft).CreateDefaultInstance());
                                managedIdx++;
                            }
                        }
                        else
                        {
                            val->ObjectType = ObjectTypes.Object;
                            val->Value = managedIdx;
                            if (managedIdx < managedStack.Count)
                                managedStack[managedIdx] = null;
                            else
                                managedStack.Add(null);
                            managedIdx++;
                        }
                    }
                }
                if (type.BaseType != null && type.BaseType is ILType)
                    InitializeValueTypeObject((ILType)type.BaseType, ptr, register, ref managedIdx);
            }
            else
            {
                CLRType t = (CLRType)type;
                var cnt = t.TotalFieldCount;
                for (int i = 0; i < cnt; i++)
                {
                    var it = t.OrderedFieldTypes[i] as CLRType;
                    StackObject* val = ILIntepreter.Minus(ptr, i + 1);
                    if (it.IsPrimitive)
                        *val = it.DefaultObject;
                    else if (it.IsEnum)
                        StackObject.Initialized(val, it);
                    else
                    {
                        if (it.IsValueType)
                        {
                            if (it.ValueTypeBinder != null)
                            {
                                val->ObjectType = ObjectTypes.ValueTypeObjectReference;
                                *(long*)&val->Value = (long)endPtr;
                                InitializeValueTypeObject(it, endPtr, register, ref managedIdx);
                                int size, mCnt;
                                it.GetValueTypeSize(out size, out mCnt);
                                endPtr -= size;
                            }
                            else
                            {
                                val->ObjectType = ObjectTypes.Object;
                                val->Value = managedIdx;
                                if (managedIdx < managedStack.Count)
                                    managedStack[managedIdx] = it.CreateDefaultInstance();
                                else
                                    managedStack.Add(it.CreateDefaultInstance());
                                managedIdx++;
                            }
                        }
                        else
                        {
                            val->ObjectType = ObjectTypes.Object;
                            val->Value = managedIdx;
                            if (managedIdx < managedStack.Count)
                                managedStack[managedIdx] = null;
                            else
                                managedStack.Add(null);
                            managedIdx++;
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
                    if (ft.IsPrimitive || ft.IsEnum)
                        StackObject.Initialized(val, ft);
                    else
                    {
                        switch (val->ObjectType)
                        {
                            case ObjectTypes.ValueTypeObjectReference:
                                ClearValueTypeObject(ft, ILIntepreter.ResolveReference(val));
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
                                    var dst = ILIntepreter.ResolveReference(val);
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

        internal void RemoveManagedStackRange(int start, int end)
        {
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

        public void FreeRegisterValueType(StackObject* esp)
        {
            if (esp->ObjectType != ObjectTypes.ValueTypeObjectReference)
                return;
            allocator.Free(esp);
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
                valueTypePtr = ILIntepreter.ResolveReference(esp);
            else
                throw new NotSupportedException();
            RemoveManagedStackRange(start, end);
        }

        public void CountValueTypeManaged(StackObject* esp, ref int start, ref int end, StackObject** endAddr)
        {
            StackObject* descriptor = ILIntepreter.ResolveReference(esp);
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
