

using System.Collections.Generic;
using System.IO;

namespace ET.Server
{
    public static partial class MessageHelper
    {
        public static void NoticeUnitAdd(Unit unit, Unit sendUnit)
        {
            M2C_CreateUnits createUnits = M2C_CreateUnits.Create();
            createUnits.Units.Add(UnitHelper.CreateUnitInfo(sendUnit));
            MessageHelper.SendToClient(unit, createUnits);
        }
        
        public static void NoticeUnitRemove(Unit unit, Unit sendUnit)
        {
            M2C_RemoveUnits removeUnits = M2C_RemoveUnits.Create();
            removeUnits.Units.Add(sendUnit.Id);
            MessageHelper.SendToClient(unit, removeUnits);
        }
        
        public static void Broadcast(Unit unit, IActorMessage message)
        {
            (message as MessageObject).IsFromPool = false;
            Dictionary<long, AOIEntity> dict = unit.GetBeSeePlayers();
            // 网络底层做了优化，同一个消息不会多次序列化
            ActorLocationSenderOneType oneTypeLocationType = unit.Root().GetComponent<ActorLocationSenderComponent>().Get(LocationType.GateSession);
            foreach (AOIEntity u in dict.Values)
            {
                oneTypeLocationType.Send(u.Unit.Id, message);
            }
        }
        
        public static void SendToClient(Unit unit, IActorMessage message)
        {
            unit.Root().GetComponent<ActorLocationSenderComponent>().Get(LocationType.GateSession).Send(unit.Id, message);
        }
        
        /// <summary>
        /// 发送协议给Actor
        /// </summary>
        public static void SendActor(Scene root, ActorId actorId, IActorMessage message)
        {
            root.GetComponent<ActorSenderComponent>().Send(actorId, message);
        }
    }
}