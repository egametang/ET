namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class LocationProxyComponent: Entity, IAwake, IDestroy
    {
        public static LocationProxyComponent Instance;
    }
}