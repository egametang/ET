using UnityEngine;

namespace ET.Client
{
    public class ChangeRotation_SyncGameObjectRotation: AEvent<EventType.ChangeRotation>
    {
        protected override async ETTask Run(EventType.ChangeRotation args)
        {
            GameObjectComponent gameObjectComponent = args.Unit.GetComponent<GameObjectComponent>();
            if (gameObjectComponent == null)
            {
                return;
            }
            Transform transform = gameObjectComponent.GameObject.transform;
            transform.rotation = args.Unit.Rotation;
            await ETTask.CompletedTask;
        }
    }
}
