namespace ET
{
    public static class SceneHelper
    {
        public static int DomainZone(this Entity entity)
        {
            return ((Scene) entity.Domain).Zone;
        }
        
        public static Scene DomainScene(this Entity entity)
        {
            return (Scene) entity.Domain;
        }

        public static Scene ZoneScene(this Entity entity)
        {
            return Game.Scene.Get(entity.DomainZone());
        }
    }
}