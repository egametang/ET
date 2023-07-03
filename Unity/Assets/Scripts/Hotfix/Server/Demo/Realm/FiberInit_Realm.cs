using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.Realm)]
    public class FiberInit_Realm: AInvokeHandler<FiberInit>
    {
        public override void Handle(FiberInit fiberInit)
        {
            Fiber fiber = fiberInit.Fiber;
            
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get(fiber.Id);
            fiber.Root.AddComponent<NetServerComponent, IPEndPoint>(startSceneConfig.InnerIPPort);
        }
    }
}