using System;

namespace ET.Server
{

    public static class BattleSceneManagerComponentSystem
    {
        public static async ETTask<BattleScene> CreateBattleScene(this BattleSceneManagerComponent self, Match2Map_GetRoom request)
        {
            await ETTask.CompletedTask;
            
            BattleScene battleScene = self.AddChild<BattleScene>();
            
            battleScene.AddComponent<RoomServerComponent, Match2Map_GetRoom>(request);
            
            battleScene.AddComponent<ServerFrameRecvComponent>();

            battleScene.LSWorld = new LSWorld(SceneType.LockStepClient);

            battleScene.AddComponent<MailBoxComponent, MailboxType>(MailboxType.UnOrderMessageDispatcher);
            
            return battleScene;
        }
    }
}