using System.Collections.Generic;
using MongoDB.Bson;

namespace ET.Server
{
    public static partial class TransferHelper
    {
        public static async ETTask TransferAtFrameFinish(Unit unit, ActorId sceneInstanceId, string sceneName)
        {
            await unit.VProcess().WaitFrameFinish();

            await TransferHelper.Transfer(unit, sceneInstanceId, sceneName);
        }
        

        public static async ETTask Transfer(Unit unit, ActorId sceneInstanceId, string sceneName)
        {
            // location加锁
            long unitId = unit.Id;
            
            M2M_UnitTransferRequest request = new() {Entitys = new List<byte[]>()};
            request.OldActorId = unit.GetActorId();
            request.Unit = unit.ToBson();
            foreach (Entity entity in unit.Components.Values)
            {
                if (entity is ITransfer)
                {
                    request.Entitys.Add(entity.ToBson());
                }
            }
            unit.Dispose();
            
            await LocationProxyComponent.Instance.Lock(LocationType.Unit, unitId, request.OldActorId);
            await ActorMessageSenderComponent.Instance.Call(sceneInstanceId, request);
        }
    }
}