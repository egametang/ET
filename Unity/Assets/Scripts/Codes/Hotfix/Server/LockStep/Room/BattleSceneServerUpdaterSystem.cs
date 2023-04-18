using System;

namespace ET.Server
{
    [FriendOf(typeof(BattleScene))]
    public static class BattleSceneServerUpdaterSystem
    {
        [FriendOf(typeof(BattleScene))]
        public class UpdateSystem: UpdateSystem<BattleSceneServerUpdater>
        {
            protected override void Update(BattleSceneServerUpdater self)
            {
                BattleScene battleScene = self.GetParent<BattleScene>();
                long timeNow = TimeHelper.ServerFrameTime();
                if (timeNow > battleScene.StartTime + battleScene.Frame * LSConstValue.UpdateInterval)
                {
                    OneFrameMessages oneFrameMessages = battleScene.FrameBuffer.GetFrameMessage(battleScene.Frame);
                    battleScene.Update(oneFrameMessages);
                    ++battleScene.Frame;
                }
            }
        }
    }
}