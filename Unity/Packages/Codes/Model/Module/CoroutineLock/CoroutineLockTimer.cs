namespace ET
{
    public struct CoroutineLockTimer
    {
        public CoroutineLock CoroutineLock;
        public long CoroutineLockInstanceId;

        public CoroutineLockTimer(CoroutineLock coroutineLock)
        {
            this.CoroutineLock = coroutineLock;
            this.CoroutineLockInstanceId = coroutineLock.InstanceId;
        }
    }
}