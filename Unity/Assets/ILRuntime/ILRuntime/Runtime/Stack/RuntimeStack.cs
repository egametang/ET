using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntime.Runtime.Stack
{
    unsafe class RuntimeStack : IDisposable
    {
        ILIntepreter intepreter;
        StackObject* pointer;
        StackObject* endOfMemory;
        IntPtr nativePointer;
        List<object> managedStack = new List<object>(32);
        Stack<StackFrame> frames = new Stack<StackFrame>();
        const int MAXIMAL_STACK_OBJECTS = 1024 * 16;

        public Stack<StackFrame> Frames { get { return frames; } }
        public RuntimeStack(ILIntepreter intepreter)
        {
            this.intepreter = intepreter;

            nativePointer = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(StackObject) * MAXIMAL_STACK_OBJECTS);
            pointer = (StackObject*)nativePointer.ToPointer();
            endOfMemory = Add(pointer, MAXIMAL_STACK_OBJECTS);
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

        public List<object> ManagedStack { get { return managedStack; } }

        public void InitializeFrame(ILMethod method, StackObject* esp, out StackFrame res)
        {
            if (esp < pointer || esp >= endOfMemory)
                throw new StackOverflowException();
            if (frames.Count > 0 && frames.Peek().BasePointer > esp)
                throw new StackOverflowException();
            res = new StackFrame();
            res.LocalVarPointer = esp;
            res.Method = method;
#if DEBUG
            res.Address = new IntegerReference();
            for (int i = 0; i < method.LocalVariableCount; i++)
            {
                var p = Add(esp, i);
                p->ObjectType = ObjectTypes.Null;
            }
#endif
            res.BasePointer = method.LocalVariableCount > 0 ? Add(esp, method.LocalVariableCount + 1) : esp;
            res.ManagedStackBase = managedStack.Count;
            //frames.Push(res);
        }
        public void PushFrame(ref StackFrame frame)
        {
            frames.Push(frame);
        }

        public StackObject* PopFrame(ref StackFrame frame, StackObject* esp, List<object> mStack)
        {
            if (frames.Count > 0 && frames.Peek().BasePointer == frame.BasePointer)
                frames.Pop();
            else
                throw new NotSupportedException();
            StackObject* returnVal = esp - 1;
            StackObject* ret = frame.LocalVarPointer - frame.Method.ParameterCount;
            int mStackBase = frame.ManagedStackBase;
            if (frame.Method.HasThis)
                ret--;
            if(frame.Method.ReturnType != intepreter.AppDomain.VoidType)
            {
                *ret = *returnVal;
                if(ret->ObjectType == ObjectTypes.Object)
                {
                    ret->Value = mStackBase;
                    mStack[mStackBase] = mStack[returnVal->Value];
                    mStackBase++;
                }
                ret++;
            }
            mStack.RemoveRange(mStackBase, mStack.Count - mStackBase);
            return ret;
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
