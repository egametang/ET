using UnityEngine;

namespace ET
{
    public class ChangeRotation_SyncGameObjectRotation: AEvent<EventType.ChangeRotation>
    {
        protected override async ETTask Run(EventType.ChangeRotation args)
        {
            Transform transform = args.Unit.GetComponent<GameObjectComponent>().GameObject.transform;
            transform.rotation = args.Unit.Rotation;
            await ETTask.CompletedTask;
        }
    }
}