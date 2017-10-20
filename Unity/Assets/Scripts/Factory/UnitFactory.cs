using UnityEngine;

namespace Model
{
    public static class UnitFactory
    {
        public static Unit Create(long id)
        {
            UnitComponent unitComponent = Game.Scene.GetComponent<UnitComponent>();
            GameObject prefab = ((GameObject) Resources.Load("Unit")).Get<GameObject>("Skeleton");
            
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