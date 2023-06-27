using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.DemoView)]
    public class FiberInit_DemoView: AInvokeHandler<FiberInit>
    {
        public override void Handle(FiberInit fiberInit)
        {
            Fiber fiber = fiberInit.Fiber;
            
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get((int)fiber.Id);
            fiber.AddComponent<NetServerComponent, IPEndPoint>(startSceneConfig.InnerIPOutPort);
        }
    }
}