namespace ETModel
{
    public enum CoroutineLockType
    {
        None = 0,
        Location, // location进程上使用
        ActorLocationSender, // ActorLocationSender中队列消息 
        Mailbox, // Mailbox中队列
        AccountName, // Realm上验证账号时使用
        AccountId, // Gate上登陆账号时使用
        UnitId,  // Map服务器上线下线时使用
        SendMail, // 发送Mail时使用
        DB, // 存储数据库
        LevelSeal,//玩家请求是否达到等级封印时使用
        ClientChangeScene, // 客户端切换场景
        
        // Client
        Resources,
        ResourcesLoader,
        
        Max, // 这个必须在最后
    }
}