namespace ET
{
    public static class UnitHelper
    {
        public static Unit GetMyUnit(Scene zoneScene)
        {
            UnitComponent unitComponent = zoneScene.GetComponent<UnitComponent>();
            return unitComponent.Get(unitComponent.MyId);
        }
    }
}