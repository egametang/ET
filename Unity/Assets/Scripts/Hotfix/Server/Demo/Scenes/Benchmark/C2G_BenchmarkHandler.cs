using System;

namespace ET.Server
{
    public static partial class C2G_BenchmarkHandler
    {
        [MessageRpcHandler(SceneType.BenchmarkServer)]
        private static async ETTask Run(Session session, C2G_Benchmark request, G2C_Benchmark response)
        {
            BenchmarkServerComponent benchmarkServerComponent = session.DomainScene().GetComponent<BenchmarkServerComponent>();
            if (benchmarkServerComponent.Count++ % 1000000 == 0)
            {
                Log.Debug($"benchmark count: {benchmarkServerComponent.Count} {TimeHelper.ClientNow()}");
            }
            await ETTask.CompletedTask;
        }
    }
}