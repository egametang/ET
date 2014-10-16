using Common.Config;

namespace Model
{
    public class GlobalConfig: AConfig
    {
        public int Type { get; set; }
    }

    [Config]
    public class GlobalCategory: ACategory<GlobalConfig>
    {
    }
}