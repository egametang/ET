using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ETModel
{
    [AsyncMethodBuilder(typeof (ETAsyncTaskVoidMethodBuilder))]
    public struct ETTaskVoid
    {
        public void Forget()
        {
        }

        [DebuggerHidden]
        public Awaiter GetAwaiter()
        {
            return new Awaiter();
        }

        public struct Awaiter: ICriticalNotifyCompletion
        {
            [DebuggerHidden]
            public bool IsCompleted => true;

            [DebuggerHidden]
            public void GetResult()
            {
                throw new Exception("UniTask can't await, always fire-and-forget. use Forget instead of await.");
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