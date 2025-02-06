using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ET.Server
{
    [HttpHandler(SceneType.RouterManager, "/get_router")]
    public class HttpGetRouterHandler : IHttpHandler
    {
        public async ETTask Handle(Scene scene, HttpListenerContext context)
        {
            HttpGetRouterResponse httpGetRouterResponse = HttpGetRouterResponse.Create();
            List<StartSceneConfig> realms = StartSceneConfigCategory.Instance.GetBySceneType(SceneType.Realm);
            foreach (StartSceneConfig startSceneConfig in realms)
            {
                // 这里是要用InnerIP，因为云服务器上realm绑定不了OuterIP的,所以realm的内网外网的socket都是监听内网地址
                httpGetRouterResponse.Realms.Add(startSceneConfig.InnerIPPort.ToString());
            }
            foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.GetBySceneType(SceneType.Router))
            {
                httpGetRouterResponse.Routers.Add($"{startSceneConfig.StartProcessConfig.OuterIP}:{startSceneConfig.Port}");
            }
            
            HttpListenerRequest request = context.Request;
            using HttpListenerResponse response = context.Response;
            
            if (request.HttpMethod == "OPTIONS")
            {
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
                response.AddHeader("Access-Control-Max-Age", "1728000");
            }
            response.AppendHeader("Access-Control-Allow-Origin", "*");
            
            byte[] bytes = MongoHelper.ToJson(httpGetRouterResponse).ToUtf8();
            response.StatusCode = 200;
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            await scene.Root().GetComponent<TimerComponent>().WaitAsync(1000);
        }
    }
}