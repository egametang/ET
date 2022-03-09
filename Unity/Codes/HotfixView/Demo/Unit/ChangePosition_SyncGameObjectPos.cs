using UnityEngine;

namespace ET
{
    public class ChangePosition_SyncGameObjectPos: AEvent<EventType.ChangePosition>
    {
        protected override async ETTask Run(EventType.ChangePosition arg)
        {
            GameObjectComponent gameObjectComponent = arg.Unit.GetComponent<GameObjectComponent>();
            if (gameObjectComponent == null)
            {
                return;
            }
            Transform transform = gameObjectComponent.GameObject.transform;
            transform.position = arg.Unit.Position;
            await ETTask.CompletedTask;
        }
    }
}