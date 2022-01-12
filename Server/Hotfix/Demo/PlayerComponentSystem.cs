using System;

namespace ET
{
    public static class PlayerComponentSystem
    {
        public class AwakeSystem : AwakeSystem<PlayerComponent>
        {
            public override void Awake(PlayerComponent self)
            {
            }
        }

        [ObjectSystem]
        public class PlayerComponentDestroySystem: DestroySystem<PlayerComponent>
        {
            public override void Destroy(PlayerComponent self)
            {
            }
        }
    }
}