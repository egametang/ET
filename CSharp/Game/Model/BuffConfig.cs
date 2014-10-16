using System.Collections.Generic;
using Common.Config;

namespace Model
{
    public class BuffConfig: AConfig
    {
        public BuffType Type { get; set; }
        public string Name { get; set; }
        public long Duration { get; set; }
        public int MaxStack { get; set; }
        public List<int> Effects { get; set; }

        public BuffConfig()
        {
            this.Effects = new List<int>();
        }
    }

    [Config]
    public class BuffCategory : ACategory<BuffConfig>
    {
    }
}
