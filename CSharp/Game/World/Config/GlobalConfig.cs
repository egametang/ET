using Common.Config;

namespace World.Config
{
    public class GlobalConfig: IConfig
    {
        public int Id { get; set; }
    }

    [Config]
    public class GlobalCategory: ACategory<GlobalConfig>
    {
    }
}