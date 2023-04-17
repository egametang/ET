using TrueSync;
using UnityEngine;

namespace ET.Client
{
    public static class LockStepOperaComponentSystem
    {
        [ObjectSystem]
        public class UpdateSystem: UpdateSystem<LockStepOperaComponent>
        {
            protected override void Update(LockStepOperaComponent self)
            {
                TSVector2 v = new();
                if (Input.GetKeyDown(KeyCode.W))
                {
                    v.y += 1;
                }
                
                if (Input.GetKeyDown(KeyCode.A))
                {
                    v.x -= 1;
                }
                
                if (Input.GetKeyDown(KeyCode.S))
                {
                    v.y -= 1;
                }
                
                if (Input.GetKeyDown(KeyCode.D))
                {
                    v.x += 1;
                }
            }
        }
    }
}