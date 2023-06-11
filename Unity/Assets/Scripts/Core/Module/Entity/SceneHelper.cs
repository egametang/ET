namespace ET
{
    public static class SceneHelper
    {
        public static int DomainZone(this Entity entity)
        {
            return (entity.IScene as Scene)?.Zone ?? 0;
        }

        public static Scene DomainScene(this Entity entity)
        {
            return entity.IScene as Scene;
        }
        
        public static SceneType DomainSceneType(this Entity entity)
        {
            return entity.IScene.SceneType;
        }
    }
}