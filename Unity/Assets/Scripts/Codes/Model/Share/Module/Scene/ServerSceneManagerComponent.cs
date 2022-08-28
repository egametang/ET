namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class ServerSceneManagerComponent: Entity, IAwake, IDestroy
    {
        [StaticField]
        public static ServerSceneManagerComponent Instance;
    }
}