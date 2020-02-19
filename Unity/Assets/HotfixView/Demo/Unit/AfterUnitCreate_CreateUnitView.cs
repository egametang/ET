using UnityEngine;

namespace ET
{
    [Event(EventIdType.AfterUnitCreate)]
    public class AfterUnitCreate_CreateUnitView: AEvent<Unit>
    {
        public override void Run(Unit unit)
        {
            // Unit View层
            ResourcesComponent resourcesComponent = Game.Scene.GetComponent<ResourcesComponent>();
            GameObject bundleGameObject = (GameObject)resourcesComponent.GetAsset("Unit.unity3d", "Unit");
            GameObject prefab = bundleGameObject.Get<GameObject>("Skeleton");
	        
            GameObject go = UnityEngine.Object.Instantiate(prefab);
            
            unit.AddComponent<AnimatorComponent>();
        }
    }
}