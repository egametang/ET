using System;

namespace Model
{
    /// <summary>
    /// 牌类
    /// </summary>
    public class Card : IEquatable<Card>
    {
        /// <summary>
        /// 权值
        /// </summary>
        public Weight CardWeight { get; private set; }

        /// <summary>
        /// 花色
        /// </summary>
        public Suits CardSuits { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="weight"></param>
        /// <param name="suits"></param>
        public Card(Weight weight, Suits suits)
        {
            this.CardWeight = weight;
            this.CardSuits = suits;
        }

        public bool Equals(Card other)
        {
            return this.CardWeight == other.CardWeight && this.CardSuits == other.CardSuits;
        }

        public string GetName()
        {
            return this.CardSuits == Suits.None ? this.CardWeight.ToString() : $"{this.CardSuits.ToString()}{this.CardWeight.ToString()}";
        }
    }
}
