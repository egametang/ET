using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ET
{
    [AsyncMethodBuilder(typeof (AsyncETTaskCompletedMethodBuilder))]
    public struct ETTaskCompleted: ICriticalNotifyCompletion
    {
        [DebuggerHidden]
        public ETTaskCompleted GetAwaiter()
        {
            return this;
        }

        [DebuggerHidden]
        public bool IsCompleted => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void GetResult()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void OnCompleted(Action continuation)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void UnsafeOnCompleted(Action continuation)
        {
        }
    }
}