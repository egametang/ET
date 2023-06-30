using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.Location)]
    public class FiberInit_Location: AInvokeHandler<FiberInit>
    {
        public override void Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;

            root.AddComponent<LocationManagerComoponent>();
        }
    }
}