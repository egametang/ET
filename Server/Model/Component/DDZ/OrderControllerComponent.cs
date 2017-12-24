using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// 出牌顺序控制器
    /// </summary>
    public class OrderControllerComponent : Component
    {
        public KeyValuePair<long, bool> FirstAuthority { get; set; }

        public long Biggest { get; set; }

        public long CurrentAuthority { get; set; }

        public int SelectLordIndex { get; set; }
    }
}
