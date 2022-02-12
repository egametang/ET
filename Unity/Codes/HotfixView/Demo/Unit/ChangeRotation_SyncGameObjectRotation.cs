using UnityEngine;

namespace ET
{
    public class ChangeRotation_SyncGameObjectRotation: AEvent<EventType.ChangeRotation>
    {
        protected override async ETTask Run(EventType.ChangeRotation arg)
        {
            GameObjectComponent gameObjectComponent = arg.Unit.GetComponent<GameObjectComponent>();
            if (gameObjectComponent == null)
            {
                return;
            }
            Transform transform = gameObjectComponent.GameObject.transform;
            transform.rotation = arg.Unit.Rotation;
            await ETTask.CompletedTask;
        }
    }
}
