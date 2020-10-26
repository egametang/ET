using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ET
{
    [AsyncMethodBuilder(typeof (ETAsyncTaskMethodBuilder))]
    public struct ETTask
    {
        public static ETTaskCompleted CompletedTask => new ETTaskCompleted();

        private readonly ETTaskCompletionSource awaiter;

        [DebuggerHidden]
        public ETTask(ETTaskCompletionSource awaiter)
        {
            this.awaiter = awaiter;
        }

        [DebuggerHidden]
        public ETTaskCompletionSource GetAwaiter()
        {
            return this.awaiter;
        }

        [DebuggerHidden]
        public void Coroutine()
        {
            InnerCoroutine().Coroutine();
        }

        [DebuggerHidden]
        private async ETVoid InnerCoroutine()
        {
            await this;
        }
    }

    [AsyncMethodBuilder(typeof (ETAsyncTaskMethodBuilder<>))]
    public struct ETTask<T>
    {
        private readonly ETTaskCompletionSource<T> awaiter;

        [DebuggerHidden]
        public ETTask(ETTaskCompletionSource<T> awaiter)
        {
            this.awaiter = awaiter;
        }

        [DebuggerHidden]
        public ETTaskCompletionSource<T> GetAwaiter()
        {
            return this.awaiter;
        }

        [DebuggerHidden]
        public void Coroutine()
        {
            InnerCoroutine().Coroutine();
        }

        private async ETVoid InnerCoroutine()
        {
            await this;
        }
    }
}