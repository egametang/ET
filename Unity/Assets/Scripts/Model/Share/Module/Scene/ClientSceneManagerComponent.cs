namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class ClientSceneManagerComponent: Entity, IAwake, IDestroy
    {
        [StaticField]
        public static ClientSceneManagerComponent Instance;
    }
}