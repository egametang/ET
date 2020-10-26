using UnityEngine;

namespace ET
{
    public class AfterUnitCreate_CreateUnitView: AEvent<EventType.AfterUnitCreate>
    {
        public override async ETTask Run(EventType.AfterUnitCreate args)
        {
            // Unit View层
            ResourcesComponent resourcesComponent = Game.Scene.GetComponent<ResourcesComponent>();
            GameObject bundleGameObject = (GameObject)resourcesComponent.GetAsset("Unit.unity3d", "Unit");
            GameObject prefab = bundleGameObject.Get<GameObject>("Skeleton");
	        
            GameObject go = UnityEngine.Object.Instantiate(prefab);
            
            args.Unit.AddComponent<AnimatorComponent>();
        }
    }
}