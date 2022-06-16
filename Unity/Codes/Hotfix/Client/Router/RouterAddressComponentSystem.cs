using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ET.Client
{
    [FriendOf(typeof(RouterAddressComponent))]
    public static class RouterAddressComponentSystem
    {
        public class RouterAddressComponentAwakeSystem: AwakeSystem<RouterAddressComponent, string>
        {
            public override void Awake(RouterAddressComponent self, string routerManagerAddress)
            {
                self.RouterManagerAddress = routerManagerAddress;
            }
        }
        
        public static async ETTask Init(this RouterAddressComponent self)
        {
            await self.GetAllRouter();
        }

        public static async ETTask GetAllRouter(this RouterAddressComponent self)
        {
            string url = $"http://{self.RouterManagerAddress}/get_router?v={RandomHelper.RandUInt32()}";
            Log.Debug($"start get router info: {url}");
            string routerInfo = await HttpClientHelper.Get(url);
            Log.Debug($"recv router info: {routerInfo}");
            HttpGetRouterResponse httpGetRouterResponse = JsonHelper.FromJson<HttpGetRouterResponse>(routerInfo);
            self.Info = httpGetRouterResponse;
            Log.Debug($"start get router info finish: {JsonHelper.ToJson(httpGetRouterResponse)}");
            
            // 打乱顺序
            RandomHelper.BreakRank(self.Info.Routers);
            
            self.WaitTenMinGetAllRouter().Coroutine();
        }
        
        // 等10分钟再获取一次
        public static async ETTask WaitTenMinGetAllRouter(this RouterAddressComponent self)
        {
            await TimerComponent.Instance.WaitAsync(5 * 60 * 1000);
            if (self.IsDisposed)
            {
                return;
            }
            await self.GetAllRouter();
        }

        public static string GetAddress(this RouterAddressComponent self)
        {
            if (self.Info.Routers.Count == 0)
            {
                return null;
            }

            return self.Info.Routers[self.RouterIndex++ % self.Info.Routers.Count];
        }
        
        public static string GetRealmAddress(this RouterAddressComponent self, string account)
        {
            int v = account.Mode(self.Info.Realms.Count);
            return self.Info.Realms[v];
        }
    }
}