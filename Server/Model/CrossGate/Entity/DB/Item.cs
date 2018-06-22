using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 物品信息
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Item : Entity
    {
        public Equipable EqInfo { get; set; }

        public int SlotID { get; set; }
        public int ItemID { get; set; }

        public int Count { get; set; }
        public int Price { get; set; }

        public int ItemType { get; set; }
        public int ItemLevel { get; set; }

        public int UsageNow { get; set; }
        public int UsageTotal { get; set; }

        public int OverlayCount { get; set; }
        public int MiniumSellCount { get; set; }

        public bool Jianding { get; set; }

        public bool IsDropDispare { get; set; }
        public bool IsLogoutDespare { get; set; }

        public bool CanMail { get; set; }
        public bool CanUse { get; set; }
        public bool CanUseInBattle { get; set; }

        public int UseInBattleRange { get; set; }

        public string UseFunction { get; set; }
        public string ItemSpecial { get; set; }
        public string ItemFunction { get; set; }
        public string SpecialFunciton { get; set; }

        public int SpecialFuncitonParameter1 { get; set; }
        public int SpecialFuncitonParameter2 { get; set; }

        public int WeaponDimondID { get; set; }
        public int EquipDimondID { get; set; }

        public string Guid { get; set; }
    }
}
