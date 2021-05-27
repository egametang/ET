namespace ET
{
    public enum CoroutineLockType
    {
        None = 0,
        Location, // location进程上使用
        ActorLocationSender, // ActorLocationSender中队列消息 
        Mailbox, // Mailbox中队列
        DB, // 存储数据库
        Resources,
        //必须放最后
        Max,
    }
}