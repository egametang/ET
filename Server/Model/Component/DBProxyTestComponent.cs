using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    [ObjectEvent]
    public class TestComponentEvent : ObjectEvent<DBProxyTestComponent>, IStart
    {
        public void Start()
        {
            this.Get().Start();
        }
    }

    public class DBProxyTestComponent : Component
    {
        public async void Start()
        {
            DBProxyComponent dbProxyComponent = Game.Scene.GetComponent<DBProxyComponent>();

            //插入一个
            DBProxyTestEntity t1 = EntityFactory.Create<DBProxyTestEntity>();
            t1.Info = "t1";
            t1.Level = 1;
            await dbProxyComponent.Insert(t1);

            //批量插入
            //List<Entity> list = new List<Entity>();
            //DBProxyTestEntity t2 = EntityFactory.Create<DBProxyTestEntity>();
            //t2.Info = "t2";
            //t2.Level = 2;
            //DBProxyTestEntity t3 = EntityFactory.Create<DBProxyTestEntity>();
            //t3.Info = "t3";
            //t3.Level = 3;
            //list.Add(t2);
            //list.Add(t3);
            //await dbProxyComponent.InsertBatch(list);

            //删除一个
            //await dbProxyComponent.Delete("{Level:1}",$"{typeof(DBProxyTestEntity).Name}");

            //删除多个
            //await dbProxyComponent.DeleteBatch("{Level:1}", $"{typeof(DBProxyTestEntity).Name}");

            //修改一个
            //DBUpdateOptions options = new DBUpdateOptions();
            ////自定义过滤器
            //options.Filter = "{Level:0}";
            ////自定义修改器
            //options.Update = "{$set:{Level:1}}";
            //options.IsUpsert = false;//找不到数据时是否插入
            //await dbProxyComponent.Update(options, $"{typeof(DBProxyTestEntity).Name}");

            //修改多个
            //DBUpdateOptions options = new DBUpdateOptions();
            ////自定义过滤器
            //options.Filter = "{Level:1}";
            ////自定义修改器
            //options.Update = "{$set:{Level:2}}";
            //options.IsUpsert = false;//找不到数据时是否插入
            //await dbProxyComponent.UpdateBatch(options, $"{typeof(DBProxyTestEntity).Name}");

            ////查询一个
            //DBFindOptions options = new DBFindOptions();
            ////自定义过滤器
            //options.Filter = "{Info:'test'}";
            ////自定义返回字段，1为显示，0为隐藏
            //options.Projection = "{_t:1,Level:1}";
            ////自定义排序,1为升序，-1为降序
            //options.Sort = "{Level:1}";
            //DBProxyTestEntity t4 = await dbProxyComponent.Find(options, $"{typeof(DBProxyTestEntity).Name}") as DBProxyTestEntity;
            //Log.Info(MongoHelper.ToJson(t4));

            ////查询多个
            //DBFindOptions options = new DBFindOptions();
            ////自定义过滤器
            //options.Filter = "{Info:'test'}";
            ////自定义返回字段，1为显示，0为隐藏
            //options.Projection = "{_t:1,Level:1}";
            ////自定义排序,1为升序，-1为降序
            //options.Sort = "{Level:1}";
            ////跳过检索数量
            //options.Skip = 1;
            ////返回数量
            //options.Limit = 1;
            //List<Entity> list1 = await dbProxyComponent.FindBatch(options, $"{typeof(DBProxyTestEntity).Name}");
            //List<DBProxyTestEntity> list2 = new List<DBProxyTestEntity>();
            //list1.ForEach(e => list2.Add((DBProxyTestEntity)e));
            //Log.Info(MongoHelper.ToJson(list2));

            ////查询并删除
            //DBFindAndDeleteOptions options = new DBFindAndDeleteOptions();
            ////自定义过滤器
            //options.Filter = "{Info:'test'}";
            ////自定义返回字段,1为显示,0为隐藏
            //options.Projection = "{_t:1,Level:1}";
            ////自定义排序,1为升序，-1为降序
            //options.Sort = "{Level:1}";
            ////设置超时时间
            //options.MaxTime = new TimeSpan(0, 0, 10);
            ////删除成功返回该对象
            //DBProxyTestEntity t5 = await dbProxyComponent.FindAndDelete(options, $"{typeof(DBProxyTestEntity).Name}") as DBProxyTestEntity;
            //Log.Info(MongoHelper.ToJson(t5));
        }
    }
}
