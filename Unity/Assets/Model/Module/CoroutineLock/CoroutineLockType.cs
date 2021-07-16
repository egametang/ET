namespace ET
{
    public enum CoroutineLockType
    {
        None = 0,
        Location,                  // location进程上使用
        ActorLocationSender,       // ActorLocationSender中队列消息 
        Mailbox,                   // Mailbox中队列
        AccountName,               // Realm上验证账号时使用
        GateAccountLock,           // Gate上登陆账号时使用
        LockPlayerName,            // 锁定玩家角色名字
        UnitId,                    // Map服务器上线下线时使用
        GateOffline,               // Gate上下线用
        SendMail,                  // 发送Mail时使用
        DB,                        // 存储数据库
        LevelSeal,                 //玩家请求是否达到等级封印时使用
        ClientChangeScene,         // 客户端切换场景
        GetCityUnit,               // 获得城池Unit
        GetFamilyUnit,             // 获得家族Unit
        GetFamilyEntity,           // 获得家族实体
        LockFamilyName,            // 家族名字
        EnterFamilyMainScene,      // 进入家族领地
        EnterFamilyTrial,          // 进入家族试炼场
        OpenTower,                 // 开启塔林
        ZiJinKu,                   // 开启紫金窟
        EnterCityBattlePreScene,   // 进入城战准备副本
        TransferOtherCopy,         // 请求分配到一个新的战场
        CityBattleAuction,         // 城战竞拍
        CityBattleRecruitSubMoney, // 城战招募状态扣钱
        JoinOrExitFamily,          // 玩家加入、退出、踢出家族
        HitDevilPlayerData,        //棒打魔王结算增加烧饼，玩家登陆到棒打魔王服.
        MedicalNumber,             //伏鼎牌照抽奖
        GetExtraExpress,           // 领取快递
        GetYinYangUnit,            // 获取阴阳玑Unit
        GetFTUnitInfo,             // 获取好友组队服玩家信息
        TuTeng,                    // 批量获取组合图腾
        GetPlayerChat,             //获取聊天存储记录
        LoadSystemComponent,       // 加载各个系统组件
        ChargeByOid,               // 充值
        UnitCache,                 // 查询缓存
        QuestDrop,                 //任务掉落
        Login,                     // 登陆时,反正访问数据库峰值太高的排队
        CityBroadcast,             // 城池广播
        HomeWorld,                 // 师傅世界服操作
        GetCache,                  // 获取缓存
        GetHomeUnit,               // 获取家园玩家
        HandleFamilyEntity,        // 处理家族实体
        AskScene,                  // 请求场景
        GMService,
        UnitCacheGet, // UnitCache查询组件
        Auction,      // 拍卖
        Match,
        JGSettleRecord, // 九宫结算记录
        AugurMgrBuyNumber,
        GetTeaUnit,              // 获取茶楼Unit
        AnimalCheck_LockRoom, // 锁住房间

        // Client
        Resources,
        ResourcesLoader,
        GiveTax, // 给国库上税
        
        SceneMapResourcesLoader,
        SceneMapResources,

        ChangeModel,//切换模型频繁

        Max, // 这个必须在最后
    }
}