namespace ET.Client
{
    public static class MyUnitHelper
    {
        public static UnitF GetMyUnitF(this BattleScene scene)
        {
            PlayerComponent playerComponent = scene.GetParent<Scene>().GetComponent<PlayerComponent>();
            long myId = playerComponent.MyId;
            return scene.LSScene.Get(myId) as UnitF;
        }
    }
}