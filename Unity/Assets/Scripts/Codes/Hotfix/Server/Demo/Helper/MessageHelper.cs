

using System.Collections.Generic;
using System.IO;

namespace ET.Server
{
    public static class MessageHelper
    {
        public static void NoticeUnitAdd(Unit unit, Unit sendUnit)
        {
            M2C_CreateUnits createUnits = new() { Units = new List<UnitInfo>() };
            createUnits.Units.Add(UnitHelper.CreateUnitInfo(sendUnit));
            MessageHelper.SendToClient(unit, createUnits);
        }
        
        public static void NoticeUnitRemove(Unit unit, Unit sendUnit)
        {
            M2C_RemoveUnits removeUnits = new() {Units = new List<long>()};
            removeUnits.Units.Add(sendUnit.Id);
            MessageHelper.SendToClient(unit, removeUnits);
        }
        
        public static void Broadcast(Unit unit, IActorMessage message)
        {
            Dictionary<long, AOIEntity> dict = unit.GetBeSeePlayers();
            // 网络底层做了优化，同一个消息不会多次序列化
            ActorLocationSenderOneType oneTypeLocationType = ActorLocationSenderComponent.Instance.Get(LocationType.Player);
            foreach (AOIEntity u in dict.Values)
            {
                oneTypeLocationType.Send(u.Unit.Id, message);
            }
        }
        
        public static void SendToClient(Unit unit, IActorMessage message)
        {
            ActorLocationSenderComponent.Instance.Get(LocationType.Player).Send(unit.Id, message);
        }
        
        
        public static void SendToLocationActor(int locationType, long id, IActorLocationMessage message)
        {
            ActorLocationSenderComponent.Instance.Get(locationType).Send(id, message);
        }
        
        /// <summary>
        /// 发送协议给Actor
        /// </summary>
        /// <param name="actorId">注册Actor的InstanceId</param>
        /// <param name="message"></param>
        public static void SendActor(long actorId, IActorMessage message)
        {
            ActorMessageSenderComponent.Instance.Send(actorId, message);
        }
    }
}