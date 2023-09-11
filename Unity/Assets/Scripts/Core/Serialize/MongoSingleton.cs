using MongoDB.Bson.IO;

namespace ET
{
    public class MongoSingleton: Singleton<MongoSingleton>, ISingletonAwake
    {
        [StaticField]
        public readonly JsonWriterSettings DefaultSettings = new() { OutputMode = JsonOutputMode.RelaxedExtendedJson };
        
        public void Awake()
        {
        }

        protected override void Destroy()
        {
            // 清理注册的Mongo元数据
        }
    }
}