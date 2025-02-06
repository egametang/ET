namespace ET
{
    public static class EntityHelper
    {
        public static int Zone(this Entity entity)
        {
            return entity.IScene.Fiber.Zone;
        }

        public static Scene Scene(this Entity entity)
        {
            return entity.IScene as Scene;
        }
        
        public static T Scene<T>(this Entity entity) where T: class, IScene 
        {
            return entity.IScene as T;
        }
        
        public static Scene Root(this Entity entity)
        {
            return entity.IScene.Fiber.Root;
        }
        
        public static Fiber Fiber(this Entity entity)
        {
            return entity.IScene.Fiber;
        }
    }
}