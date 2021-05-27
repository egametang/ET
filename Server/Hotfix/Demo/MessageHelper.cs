

namespace ET
{
    public static class MessageHelper
    {
        public static void Broadcast(Unit unit, IActorMessage message)
        {
            var units = unit.Domain.GetComponent<UnitComponent>().GetAll();

            if (units == null) return;

            foreach (Unit u in units)
            {
                UnitGateComponent unitGateComponent = u.GetComponent<UnitGateComponent>();
                SendActor(unitGateComponent.GateSessionActorId, message);
            }
        }
        
        
        /// <summary>
        /// 发送协议给ActorLocation
        /// </summary>
        /// <param name="id">注册Actor的Id</param>
        /// <param name="message"></param>
        public static void SendToLocationActor(long id, IActorLocationMessage message)
        {
            ActorLocationSenderComponent.Instance.Send(id, message);
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
        
        /// <summary>
        /// 发送RPC协议给Actor
        /// </summary>
        /// <param name="actorId">注册Actor的InstanceId</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async ETTask<IActorResponse> CallActor(long actorId, IActorRequest message)
        {
            return await ActorMessageSenderComponent.Instance.Call(actorId, message);
        }
        
        /// <summary>
        /// 发送RPC协议给ActorLocation
        /// </summary>
        /// <param name="id">注册Actor的Id</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async ETTask<IActorResponse> CallLocationActor(long id, IActorLocationRequest message)
        {
            return await ActorLocationSenderComponent.Instance.Call(id, message);
        }
    }
}