namespace ET
{
    public static class EntityHelper
    {
        public static int DomainZone(this Entity entity)
        {
            return (entity.IScene as Scene)?.Zone ?? 0;
        }

        public static Scene Scene(this Entity entity)
        {
            return entity.IScene as Scene;
        }
        
        public static T Scene<T>(this Entity entity) where T: class, IScene 
        {
            return entity.IScene as T;
        }
        
        public static Fiber Fiber(this Entity entity)
        {
            return entity.IScene.Root as Fiber;
        }
    }
}