using System.Collections.Generic;
using MemoryPack;

namespace ET
{
    [Invoke((long)SceneType.Main)]
    public class FiberInit_Main: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
           
            await EventSystem.Instance.PublishAsync(root, new EntryEvent1());
            await EventSystem.Instance.PublishAsync(root, new EntryEvent2());
            await EventSystem.Instance.PublishAsync(root, new EntryEvent3());


            AA aa = root.AddComponent<AA>();

            BB bb = aa.AddComponent<BB>();
            CC cc = aa.AddComponent<CC>();
            bb.B = 1;
            cc.C = 2;

            byte[] bytes = MemoryPackSerializer.Serialize(aa);

            AA aa2 = MemoryPackSerializer.Deserialize<AA>(bytes);
            
            Log.Debug($"11111111111111111111111111: {aa2}");
            
            byte[] bytes2 = MongoHelper.Serialize(aa);

            AA aa3 = MongoHelper.Deserialize<AA>(bytes2);
            
            Log.Debug($"11111111111111111111111111: {aa3}");
        }
    }
}