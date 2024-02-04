using System.Net;

namespace ET.Server
{
    [EntitySystemOf(typeof(BenchmarkServerComponent))]
    public static partial class BenchmarkServerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BenchmarkServerComponent self)
        {
        }
    }
}