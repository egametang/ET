using System;
using UnityEngine;

namespace ET
{
    public class AfterUnitCreate_CreateUnitView: AEvent<EventType.AfterUnitCreate>
    {
        protected override async ETTask Run(EventType.AfterUnitCreate arg)
        {
            switch (arg.Unit.UnitType)
            {
                case UnitType.Player:
                    break;
                case UnitType.Monster:
                    break;
                case UnitType.NPC:
                    break;
            }
            
            // Unit View层
            // 这里可以改成异步加载，demo就不搞了
            GameObject bundleGameObject = (GameObject)ResourcesComponent.Instance.GetAsset("Unit.unity3d", "Unit");
            GameObject prefab = bundleGameObject.Get<GameObject>("Skeleton");
	        
            GameObject go = UnityEngine.Object.Instantiate(prefab, GlobalComponent.Instance.Unit, true);
            go.transform.position = arg.Unit.Position;
            arg.Unit.AddComponent<GameObjectComponent>().GameObject = go;
            arg.Unit.AddComponent<AnimatorComponent>();
            await ETTask.CompletedTask;
        }
    }
}