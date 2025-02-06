using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    [EntitySystemOf(typeof(RouterAddressComponent))]
    public static partial class RouterAddressComponentSystem
    {
        [EntitySystem]
        private static void Awake(this RouterAddressComponent self, string address)
        {
            self.Address = address;
            string ip = self.Address.Substring(0, self.Address.LastIndexOf(":"));
            self.AddressFamily = IPAddress.Parse(ip).AddressFamily;
        }
        
        public static async ETTask Init(this RouterAddressComponent self)
        {
            await self.GetAllRouter();
        }

        private static async ETTask GetAllRouter(this RouterAddressComponent self)
        {
            string url = $"http://{self.Address}/get_router?v={RandomGenerator.RandUInt32()}";
            Log.Debug($"start get router info: {url}");
            string routerInfo = await HttpClientHelper.Get(url);
            Log.Debug($"recv router info: {routerInfo}");
            HttpGetRouterResponse httpGetRouterResponse = MongoHelper.FromJson<HttpGetRouterResponse>(routerInfo);
            self.Info = httpGetRouterResponse;
            Log.Debug($"start get router info finish: {MongoHelper.ToJson(httpGetRouterResponse)}");
            
            // 打乱顺序
            RandomGenerator.BreakRank(self.Info.Routers);
            
            self.WaitTenMinGetAllRouter().NoContext();
        }
        
        // 等10分钟再获取一次
        public static async ETTask WaitTenMinGetAllRouter(this RouterAddressComponent self)
        {
            await self.Root().GetComponent<TimerComponent>().WaitAsync(5 * 60 * 1000);
            if (self.IsDisposed)
            {
                return;
            }
            await self.GetAllRouter();
        }

        public static IPEndPoint GetAddress(this RouterAddressComponent self)
        {
            if (self.Info.Routers.Count == 0)
            {
                return null;
            }

            string address = self.Info.Routers[self.RouterIndex++ % self.Info.Routers.Count];
            Log.Info($"get router address: {self.RouterIndex - 1} {address}");
            string[] ss = address.Split(':');
            IPAddress ipAddress = IPAddress.Parse(ss[0]);
            if (self.AddressFamily == AddressFamily.InterNetworkV6)
            { 
                ipAddress = ipAddress.MapToIPv6();
            }
            return new IPEndPoint(ipAddress, int.Parse(ss[1]));
        }
        
        public static IPEndPoint GetRealmAddress(this RouterAddressComponent self, string account)
        {
            int v = account.Mode(self.Info.Realms.Count);
            string address = self.Info.Realms[v];
            
            IPAddress ipAddress = IPAddress.Parse(address.Substring(0, address.LastIndexOf(":")));
            int port = int.Parse(address.Substring(address.LastIndexOf(":") + 1));
            //if (self.IPAddress.AddressFamily == AddressFamily.InterNetworkV6)
            //{ 
            //    ipAddress = ipAddress.MapToIPv6();
            //}
            return new IPEndPoint(ipAddress, port);
        }
    }
}