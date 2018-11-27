using UnityEngine;

namespace ETModel
{
    public static class UnitFactory
    {
        public static Unit Create(long id)
        {
	        ResourcesComponent resourcesComponent = Game.Scene.GetComponent<ResourcesComponent>();
	        GameObject bundleGameObject = (GameObject)resourcesComponent.GetAsset("Unit.unity3d", "Unit");
	        GameObject prefab = bundleGameObject.Get<GameObject>("Skeleton");
	        
            UnitComponent unitComponent = Game.Scene.GetComponent<UnitComponent>();
            
	        Unit unit = ComponentFactory.CreateWithId<Unit>(id);
	        unit.GameObject = UnityEngine.Object.Instantiate(prefab);
	        GameObject parent = GameObject.Find($"/Global/Unit");
	        unit.GameObject.transform.SetParent(parent.transform, false);
			unit.AddComponent<AnimatorComponent>();
	        unit.AddComponent<MoveComponent>();
	        unit.AddComponent<TurnComponent>();
	        unit.AddComponent<UnitPathComponent>();

            unitComponent.Add(unit);
            return unit;
        }
    }
}