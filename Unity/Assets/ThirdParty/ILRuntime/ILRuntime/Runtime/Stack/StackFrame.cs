using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.Method;

namespace ILRuntime.Runtime.Stack
{
    class IntegerReference
    {
        public int Value { get; set; }
    }
    unsafe struct StackFrame
    {
        public ILMethod Method;
        public StackObject* LocalVarPointer;
        public StackObject* BasePointer;
        public StackObject* ValueTypeBasePointer;
        public IntegerReference Address;
        public int ManagedStackBase;
    }
}
