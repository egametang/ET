using System.Net;

namespace ET.Server
{
    [Invoke((int)SceneType.Realm)]
    public class FiberInit_Realm: AInvokeHandler<FiberInit>
    {
        public override void Handle(FiberInit fiberInit)
        {
            Fiber fiber = fiberInit.Fiber;
            
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get((int)fiber.Id);
            fiber.AddComponent<NetServerComponent, IPEndPoint>(startSceneConfig.InnerIPOutPort);
        }
    }
}