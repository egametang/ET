using TrueSync;
using UnityEngine;

namespace ET.Client
{
    [FriendOf(typeof(BattleSceneClientUpdater))]
    public static class LockStepOperaComponentSystem
    {
        [FriendOf(typeof(BattleSceneClientUpdater))]
        public class UpdateSystem: UpdateSystem<LockStepOperaComponent>
        {
            protected override void Update(LockStepOperaComponent self)
            {
                TSVector2 v = new();
                if (Input.GetKey(KeyCode.W))
                {
                    v.y += 1;
                }
                
                if (Input.GetKey(KeyCode.A))
                {
                    v.x -= 1;
                }
                
                if (Input.GetKey(KeyCode.S))
                {
                    v.y -= 1;
                }
                
                if (Input.GetKey(KeyCode.D))
                {
                    v.x += 1;
                }

                BattleSceneClientUpdater battleSceneClientUpdater = self.GetParent<Room>().GetComponent<BattleSceneClientUpdater>();
                battleSceneClientUpdater.InputInfo.V = v;
            }
        }
    }
}