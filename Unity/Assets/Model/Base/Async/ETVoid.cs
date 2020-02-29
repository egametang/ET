using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ET
{
    [AsyncMethodBuilder(typeof(AsyncETVoidMethodBuilder))]
    public struct ETVoid
    {
        public void Coroutine()
        {
        }

        [DebuggerHidden]
        public Awaiter GetAwaiter()
        {
            return new Awaiter();
        }

        public struct Awaiter : ICriticalNotifyCompletion
        {
            [DebuggerHidden]
            public bool IsCompleted => true;

            [DebuggerHidden]
            public void GetResult()
            {
                throw new InvalidOperationException("ETAvoid can not await, use Coroutine method instead!");
            }

            [DebuggerHidden]
            public void OnCompleted(Action continuation)
            {
            }

            [DebuggerHidden]
            public void UnsafeOnCompleted(Action continuation)
            {
            }
        }
    }
}