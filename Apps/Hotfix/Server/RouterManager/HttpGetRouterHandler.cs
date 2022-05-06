using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ET.Server
{
    [HttpHandler(SceneType.RouterManager, "/get_router")]
    public class HttpGetRouterHandler : IHttpHandler
    {
        public async ETTask Handle(Entity domain, HttpListenerContext context)
        {
            HttpGetRouterResponse response = new HttpGetRouterResponse();
            response.Realms = new List<string>();
            response.Routers = new List<string>();
            foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.Realms)
            {
                response.Realms.Add(startSceneConfig.InnerIPOutPort.ToString());
            }
            foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.Routers)
            {
                response.Routers.Add($"{startSceneConfig.StartProcessConfig.OuterIP}:{startSceneConfig.OuterPort}");
            }
            HttpHelper.Response(context, response);
            await ETTask.CompletedTask;
        }
    }
}
