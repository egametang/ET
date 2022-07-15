using UnityEngine;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class ChangeRotation_SyncGameObjectRotation: AEvent<Unit, EventType.ChangeRotation>
    {
        protected override async ETTask Run(Unit unit, EventType.ChangeRotation args)
        {
            GameObjectComponent gameObjectComponent = unit.GetComponent<GameObjectComponent>();
            if (gameObjectComponent == null)
            {
                return;
            }
            Transform transform = gameObjectComponent.GameObject.transform;
            transform.rotation = unit.Rotation;
            await ETTask.CompletedTask;
        }
    }
}
