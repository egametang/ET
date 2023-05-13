using System;

namespace ET.Client
{
    public static class GameObjectComponentSystem
    {
        [EntitySystem]
        public class DestroySystem: DestroySystem<GameObjectComponent>
        {
            protected override void Destroy(GameObjectComponent self)
            {
                UnityEngine.Object.Destroy(self.GameObject);
            }
        }
    }
}