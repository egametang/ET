using ET.EventType;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ET
{
    public class ChangeRotation_ChangeUnitViewRotation : AEvent<EventType.ChangeRotation>
    {
        protected override async ETTask Run(ChangeRotation a)
        {
            GameObjectComponent gameObjectComponent = a.Unit.GetComponent<GameObjectComponent>();
            if (gameObjectComponent != null && gameObjectComponent.IsDisposed == false)
            {
                gameObjectComponent.GameObject.transform.rotation = a.Unit.Rotation;
            }
            await ETTask.CompletedTask;
        }
    }
}
