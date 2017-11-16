﻿using UnityEngine;

namespace Model
{
    public static class UnitFactory
    {
        public static Unit Create(long id)
        {
	        ResourcesComponent resourcesComponent = Game.Scene.GetComponent<ResourcesComponent>();
	        GameObject bundleGameObject = resourcesComponent.GetAsset<GameObject>("Unit", "Unit");
	        GameObject prefab = bundleGameObject.Get<GameObject>("Skeleton");
	        
            UnitComponent unitComponent = Game.Scene.GetComponent<UnitComponent>();
            
	        Unit unit = EntityFactory.CreateWithId<Unit>(id);
	        unit.GameObject = UnityEngine.Object.Instantiate(prefab);
	        GameObject parent = GameObject.Find($"/Global/Unit");
	        unit.GameObject.transform.SetParent(parent.transform, false);
			unit.AddComponent<AnimatorComponent>();
	        unit.AddComponent<MoveComponent>();

            unitComponent.Add(unit);
            return unit;
        }
    }
}