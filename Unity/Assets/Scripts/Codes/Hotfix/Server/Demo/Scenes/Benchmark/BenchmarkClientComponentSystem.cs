using System.Collections.Generic;
using System.Net.Sockets;
using ET.Client;

namespace ET.Server
{
    public static class BenchmarkClientComponentSystem
    {
        public class AwakeSystem: AwakeSystem<BenchmarkClientComponent>
        {
            protected override void Awake(BenchmarkClientComponent self)
            {
                for (int i = 0; i < 50; ++i)
                {
                    self.Start().Coroutine();
                }
            }
        }

        private static async ETTask Start(this BenchmarkClientComponent self)
        {
            await TimerComponent.Instance.WaitAsync(1000);

            Scene scene = await SceneFactory.CreateServerScene(self, IdGenerater.Instance.GenerateId(), IdGenerater.Instance.GenerateInstanceId(),
                self.DomainZone(), "bechmark", SceneType.Benchmark);
            
            NetClientComponent netClientComponent = scene.AddComponent<NetClientComponent, AddressFamily>(AddressFamily.InterNetwork);

            using Session session = netClientComponent.Create(StartSceneConfigCategory.Instance.BenchmarkServer.OuterIPPort);
            List<ETTask> list = new List<ETTask>(100000);

            async ETTask Call(Session s)
            {
                await s.Call(new C2G_Benchmark());
            }
            
            for (int j = 0; j < 100000000; ++j)
            {
                list.Clear();
                for (int i = 0; i < list.Capacity; ++i)
                {
                    list.Add(Call(session));
                }
                await ETTaskHelper.WaitAll(list);
            }
        }
    }
}