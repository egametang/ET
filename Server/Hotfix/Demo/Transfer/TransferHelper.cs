namespace ET
{
    public static class TransferHelper
    {
        public static async ETTask Transfer(Unit unit, long sceneInstanceId, string sceneName)
        {
            // 通知客户端开始切场景
            M2C_StartSceneChange m2CStartSceneChange = new M2C_StartSceneChange() {SceneInstanceId = sceneInstanceId, SceneName = sceneName};
            MessageHelper.SendToClient(unit, m2CStartSceneChange);
            
            M2M_UnitTransferRequest request = new M2M_UnitTransferRequest();
            request.Unit = unit;
            foreach (Entity entity in unit.Components.Values)
            {
                if (entity is ITransfer)
                {
                    request.Entitys.Add(entity);
                }
            }
            // 删除Mailbox,让发给Unit的ActorLocation消息重发
            unit.RemoveComponent<MailBoxComponent>();
            
            // location加锁
            long oldInstanceId = unit.InstanceId;
            await LocationProxyComponent.Instance.Lock(unit.Id, unit.InstanceId);
            M2M_UnitTransferResponse response = await ActorMessageSenderComponent.Instance.Call(sceneInstanceId, request) as M2M_UnitTransferResponse;
            await LocationProxyComponent.Instance.UnLock(unit.Id, oldInstanceId, response.NewInstanceId);
            unit.Dispose();
        }
    }
}