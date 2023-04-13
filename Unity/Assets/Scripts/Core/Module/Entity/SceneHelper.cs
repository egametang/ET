namespace ET
{
    public static class SceneHelper
    {
        public static int DomainZone(this Entity entity)
        {
            return (entity.Domain as Scene)?.Zone ?? 0;
        }

        public static Scene DomainScene(this Entity entity)
        {
            return entity.Domain as Scene;
        }
    }
}