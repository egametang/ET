using ET.EventType;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public class ChangePosition_ChangeUnitViewPosition : AEvent<EventType.ChangePosition>
    {
        protected override async ETTask Run(ChangePosition a)
        {
            GameObjectComponent gameObjectComponent= a.Unit.GetComponent<GameObjectComponent>();
            if (gameObjectComponent!=null&&gameObjectComponent.IsDisposed==false)
            {
                gameObjectComponent.GameObject.transform.position = a.Unit.Position;
            }
            await ETTask.CompletedTask;
        }
    }
}

