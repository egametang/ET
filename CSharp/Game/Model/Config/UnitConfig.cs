using Common.Config;

namespace Model
{
    public class UnitConfig: AConfig
    {
        public int Type { get; set; }
    }

    [Config]
    public class UnitCategory : ACategory<UnitConfig>
    {
    }
}
