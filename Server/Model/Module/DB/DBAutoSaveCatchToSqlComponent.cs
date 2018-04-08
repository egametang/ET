using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ETModel
{
    [ObjectSystem]
    public class DBAutoSaveCatchToSqlComponentSystem : AwakeSystem<DBAutoSaveCatchToSqlComponent>
    {
        public override void Awake(DBAutoSaveCatchToSqlComponent self)
        {
            self.Awake();
        }
    }
    /// <summary>
    /// 定时自动将存储的数据写入 数据库;
    /// </summary>
    public class DBAutoSaveCatchToSqlComponent : Component
    {
        private TimerComponent timerComponent;
        DBCacheComponent dbCacheComponent;
        DBProxyComponent dvProxyComponent;

        public void Awake()
        {
            timerComponent = Game.Scene.GetComponent<TimerComponent>();
            dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
            dvProxyComponent = Game.Scene.GetComponent<DBProxyComponent>();

            CheckSaveCatch();
        }
        //判断数据是否改变 工程浩大  到时间了全部写入数据库就好
        async void CheckSaveCatch()
        {
            while (true)
            {
                await timerComponent.WaitAsync(10000);//5分钟存一次数据
                foreach (KeyValuePair<string, Dictionary<long, ComponentWithId>> tem in dbCacheComponent.cache)
                {
                    string collectionName = tem.Key;
                    List< ComponentWithId> arr = tem.Value.Values.ToList();
                    Log.Info($"开始保存数据到数据库 集合是 {collectionName}");
                    await dvProxyComponent.SaveBatch(arr,false);
                }

            }
        }
    }
}
