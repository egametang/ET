using System.Collections.Generic;
using System.Net.Sockets;
using ET.Client;

namespace ET.Server
{
    [EntitySystemOf(typeof(BenchmarkClientComponent))]
    public static partial class BenchmarkClientComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BenchmarkClientComponent self)
        {
            for (int i = 0; i < 2; ++i)
            {
                self.Start().Coroutine();
            }
        }

        private static async ETTask Start(this BenchmarkClientComponent self)
        {
            NetComponent netClientComponent = self.Root().GetComponent<NetComponent>();
            using Session session = netClientComponent.Create(StartSceneConfigCategory.Instance.Benchmark.OuterIPPort);
            List<ETTask> list = new List<ETTask>(1000);

            async ETTask Call(Session s)
            {
                using G2C_Benchmark benchmark = await s.Call(C2G_Benchmark.Create()) as G2C_Benchmark;
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