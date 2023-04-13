namespace ET
{
    public static class UnitFFactory
    {
        public static UnitF Init(Scene lsScene, LockStepUnitInfo unitInfo)
        {
	        UnitFComponent unitComponent = lsScene.AddComponent<UnitFComponent>();
	        UnitF unit = unitComponent.AddChildWithId<UnitF>(unitInfo.PlayerId);
	        
	        unit.Position = unitInfo.Position;
	        unit.Rotation = unitInfo.Rotation;
	        
	        EventSystem.Instance.Publish(unit.DomainScene(), new EventType.LSAfterUnitCreate() {UnitF = unit});
            return unit;
        }
    }
}
