using UnityEngine;

namespace ET
{
    public static class UnitFactory
    {
        public static Unit Create(Entity domain, UnitInfo unitInfo)
        {
	        Unit unit = EntityFactory.CreateWithId<Unit, int>(domain, unitInfo.UnitId, unitInfo.ConfigId);
	        unit.Position = new Vector3(unitInfo.X, unitInfo.Y, unitInfo.Z);
	        
	        unit.AddComponent<MoveComponent>();
	        NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
	        for (int i = 0; i < unitInfo.Ks.Count; ++i)
	        {
		        numericComponent.Set((NumericType)unitInfo.Ks[i], unitInfo.Vs[i]);
	        }

	        unit.AddComponent<ObjectWait>();

	        unit.AddComponent<XunLuoPathComponent>();

	        UnitComponent unitComponent = domain.GetComponent<UnitComponent>();
            unitComponent.Add(unit);
            
            Game.EventSystem.Publish(new EventType.AfterUnitCreate() {Unit = unit});
            return unit;
        }
    }
}
