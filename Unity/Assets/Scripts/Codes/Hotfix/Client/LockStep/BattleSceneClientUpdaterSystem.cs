using System;

namespace ET.Client
{
    [FriendOf(typeof(BattleScene))]
    public static class BattleSceneClientUpdaterSystem
    {
        [FriendOf(typeof(BattleScene))]
        public class UpdateSystem: UpdateSystem<BattleSceneClientUpdater>
        {
            protected override void Update(BattleSceneClientUpdater self)
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