using System;

namespace ET
{
    public readonly struct CoroutineLock: IDisposable
    {
        private readonly CoroutineLockComponent coroutineLockComponent;
        private readonly CoroutineLockType coroutineLockType;
        private readonly long key;
        private readonly short index;

        public CoroutineLock(CoroutineLockComponent coroutineLockComponent, CoroutineLockType type, long k, short index)
        {
            this.coroutineLockComponent = coroutineLockComponent;
            this.coroutineLockType = type;
            this.key = k;
            this.index = index;
        }

        public void Dispose()
        {
            coroutineLockComponent.Notify(coroutineLockType, this.key, this.index);
        }
    }
}