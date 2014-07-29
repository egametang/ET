namespace Component.Config
{
    public class GlobalConfig: IType
    {
        public int Type { get; set; }
    }

    [Config]
    public class GlobalCategory: ConfigCategory<GlobalConfig>
    {
    }
}