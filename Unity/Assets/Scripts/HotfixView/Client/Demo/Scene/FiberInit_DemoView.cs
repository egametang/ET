using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.DemoView)]
    public class FiberInit_DemoView: AInvokeHandler<FiberInit>
    {
        public override void Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get((int)root.Id);
            root.AddComponent<NetServerComponent, IPEndPoint>(startSceneConfig.InnerIPOutPort);
        }
    }
}