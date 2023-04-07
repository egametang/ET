using Unity.Mathematics;

namespace ET.Client
{
    public static class UnitFFactory
    {
        public static UnitF Create(Scene currentScene, LockStepUnitInfo unitInfo)
        {
	        UnitFComponent unitComponent = currentScene.GetComponent<UnitFComponent>();
	        UnitF unit = unitComponent.AddChildWithId<UnitF>(unitInfo.PlayerId);
	        
	        unit.Position = unitInfo.Position;
	        unit.Rotation = unitInfo.Rotation;
	        
	        EventSystem.Instance.Publish(unit.DomainScene(), new EventType.LockStepAfterUnitCreate() {UnitF = unit});
            return unit;
        }
    }
}
