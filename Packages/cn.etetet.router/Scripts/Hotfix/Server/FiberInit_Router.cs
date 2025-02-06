using System.Net;

namespace ET.Server
{
    [Invoke(SceneType.Router)]
    public class FiberInit_Router: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get((int)root.Id);
            
            // 开发期间使用OuterIPPort，云服务器因为本机没有OuterIP，所以要改成InnerIPPort，然后在云防火墙中端口映射到InnerIPPort
            StartProcessConfig startProcessConfig = StartProcessConfigCategory.Instance.Get(startSceneConfig.Process);
            IPEndPoint outIPPort = NetworkHelper.ToIPEndPoint($"{startProcessConfig.OuterIP}:{startSceneConfig.Port}");
            root.AddComponent<RouterComponent, IPEndPoint, string>(outIPPort, startProcessConfig.InnerIP);
            Log.Console($"Router create: {root.Fiber.Id}");
            await ETTask.CompletedTask;
        }
    }
}