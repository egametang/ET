namespace ETModel
{
    public enum MapType
    {
        // 城市
        City,
        // 野外
        Field,
        // 副本
        Copy,
    }
    
    [NoObjectPool]
    public class MapConfig: AConfigComponent
    {
        public MapType MapType;
    }
}