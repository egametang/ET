using System;
using System.Diagnostics;
using ETModel;

namespace ETHotfix
{
    public class XXX: ComponentWithId
    {
        public string   Name;
        public int      Fucker;
        public float    Lover;
        public DateTime Time;
    }
    
    [ObjectSystem]
    public class TestComponentAwakeSystem : AwakeSystem<TestComponent>
    {
        public override void Awake(TestComponent self)
        {
            self.Awake("sss");
        }
    }

    public static class TestComponentEX
    {
        public static async void Awake(this TestComponent test,string gogo)
        {
            Log.Info("Run!!!!!!");
            var cache = Game.Scene.GetComponent<DBProxyComponent>();
            int id = 1234;
            var list = await cache.Query<XXX>(t => t.Id == id && t.Name == gogo);
            Log.Info(list.Count.ToString());
        }
    }
}