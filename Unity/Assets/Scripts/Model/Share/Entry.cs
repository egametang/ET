using MemoryPack;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace ET
{
    public struct EntryEvent1
    {
    }   
    
    public struct EntryEvent2
    {
    } 
    
    public struct EntryEvent3
    {
    }
    
    public static class Entry
    {
        public static void Init()
        {
            
        }
        
        public static void Start()
        {
            StartAsync().Coroutine();
        }
        
        private static async ETTask StartAsync()
        {
            //根据平台不同初始化时间精度
            WinPeriod.Init();

            // 注册Mongo type
            MongoRegister.Init();
            // 注册Entity序列化器
            EntitySerializeRegister.Init();
            //创建无需热重载的通用组件
            World.Instance.AddSingleton<IdGenerater>();
            World.Instance.AddSingleton<OpcodeType>();
            World.Instance.AddSingleton<ObjectPool>();
            World.Instance.AddSingleton<MessageQueue>();
            World.Instance.AddSingleton<NetServices>();
            World.Instance.AddSingleton<NavmeshComponent>();
            World.Instance.AddSingleton<LogMsg>();
            
            //创建需要reload的各种code单例（各种消息分发组件/EventSystem）
            CodeTypes.Instance.CreateCode();
            
            //添加配置加载单例，加载全部标记[Config]标签头的类
            await World.Instance.AddSingleton<ConfigLoader>().LoadAsync();

            //因为上面初始化了EventSystem，所以这里创建纤程后会分发到对应的事件管线
            //检查SceneType.Main的引用后，发现该事件的Invoke为AInvokeHandler类
            await FiberManager.Instance.Create(SchedulerType.Main, ConstFiberId.Main, 0, SceneType.Main, "");
        }
    }
}