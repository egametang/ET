namespace ET
{
    public static class UnitFFactory
    {
        public static LSUnit Init(LSWorld lsWorld, LockStepUnitInfo unitInfo)
        {
	        LSUnitComponent lsUnitComponent = lsWorld.AddComponent<LSUnitComponent>();
	        LSUnit lsUnit = lsUnitComponent.AddChildWithId<LSUnit>(unitInfo.PlayerId);
	        
	        lsUnit.Position = unitInfo.Position;
	        lsUnit.Rotation = unitInfo.Rotation;
	        
	        EventSystem.Instance.Publish(lsUnit.DomainScene(), new EventType.LSAfterUnitCreate() {LsUnit = lsUnit});
            return lsUnit;
        }
    }
}
