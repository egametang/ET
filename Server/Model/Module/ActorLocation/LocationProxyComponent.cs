namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class LocationProxyComponent: Entity, IAwake, IDestroy
    {
        public static LocationProxyComponent Instance;
    }
}