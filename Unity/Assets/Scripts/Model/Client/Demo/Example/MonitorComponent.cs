namespace ET.Client
{
    [ComponentOf(typeof(Computer))]
    //IAwake允许传入指定的参数，最多四个（可在源码里按需添加）
    public class MonitorComponent : Entity ,IAwake<int>,IDestroy
    {
        public int Brightness;
    }
}
