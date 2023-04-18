namespace ET.Client
{
    public static class MyUnitHelper
    {
        public static LSUnit GetMyUnitF(this BattleScene scene)
        {
            PlayerComponent playerComponent = scene.GetParent<Scene>().GetComponent<PlayerComponent>();
            long myId = playerComponent.MyId;
            return scene.LSWorld.Get(myId) as LSUnit;
        }
    }
}