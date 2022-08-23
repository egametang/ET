namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class LocationProxyComponent: Entity, IAwake, IDestroy
    {
        [StaticField]
        public static LocationProxyComponent Instance;
    }
}