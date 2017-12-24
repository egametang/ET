using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// 手牌组件
    /// </summary>
    public class HandCardsComponent : Component
    {
        public readonly List<Card> Library = new List<Card>();

        public Identity AccessIdentity { get; set; }

        public bool IsAuto { get; set; }

        public int CardsCount { get { return Library.Count; } }
    }
}
