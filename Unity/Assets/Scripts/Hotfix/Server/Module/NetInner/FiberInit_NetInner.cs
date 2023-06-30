using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.NetInner)]
    public class FiberInit_NetInner: AInvokeHandler<FiberInit>
    {
        public override void Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;

            StartMachineConfig startMachineConfig = StartMachineConfigCategory.Instance.Get(fiberInit.Fiber.Process);
            IPEndPoint ipEndPoint = NetworkHelper.ToIPEndPoint($"{startMachineConfig.InnerIP}:{startMachineConfig.WatcherPort}");
            root.AddComponent<NetInnerComponent, IPEndPoint>(ipEndPoint);
        }
    }
}