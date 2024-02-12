using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.RouterManager)]
    public class FiberInit_RouterManager: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get((int)root.Id);
            string httpAddress = $"http://{startSceneConfig.StartProcessConfig.OuterIP}:{startSceneConfig.Port}/";
            Log.Console("RouterManager 地址: " + httpAddress);
            root.AddComponent<HttpComponent, string>(httpAddress);
            await ETTask.CompletedTask;
        }
    }
}
