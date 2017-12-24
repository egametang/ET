using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// 牌库
    /// </summary>
    public class DeckComponent : Component
    {
        /// <summary>
        /// 排列表
        /// </summary>
        public readonly List<Card> library = new List<Card>();

        /// <summary>
        /// 牌张数
        /// </summary>
        public int CardsCount { get { return this.library.Count; } }
    }
}
