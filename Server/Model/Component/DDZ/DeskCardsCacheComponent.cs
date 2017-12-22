using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// 出牌记录组件
    /// </summary>
    public class DeskCardsCacheComponent : Component
    {
        /// <summary>
        /// 牌库
        /// </summary>
        public readonly List<Card> library = new List<Card>();

        /// <summary>
        /// 地主牌
        /// </summary>
        public readonly List<Card> LordCards = new List<Card>();

        /// <summary>
        /// 牌统计
        /// </summary>
        public int CardsCount { get { return this.library.Count; } }

        /// <summary>
        /// 权值
        /// </summary>
        public CardsType Rule { get; set; }

        /// <summary>
        /// 最小权值
        /// </summary>
        public int MinWeight { get { return (int)this.library[0].CardWeight; } }
    }
}
