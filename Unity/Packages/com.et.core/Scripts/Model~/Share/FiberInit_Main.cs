using System.Collections.Generic;
using MemoryPack;

namespace ET
{
    [Invoke(SceneType.Main)]
    public class FiberInit_Main: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            // 注册序列化
            MongoRegister.Init();
            MemoryPackRegister.Init();
            
            Scene root = fiberInit.Fiber.Root;
           
            await EventSystem.Instance.PublishAsync(root, new EntryEvent1());
            await EventSystem.Instance.PublishAsync(root, new EntryEvent2());
            await EventSystem.Instance.PublishAsync(root, new EntryEvent3());
            
            Log.Debug($"run success!");
        }
    }
}