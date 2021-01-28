namespace ET
{
    public static class SceneHelper
    {
        public static Scene ZoneScene(this Entity entity)
        {
            return Game.Scene.Get(entity.DomainZone());
        }
    }
}