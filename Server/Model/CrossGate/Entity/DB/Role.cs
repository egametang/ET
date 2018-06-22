using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 玩家全部信息
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Role : Entity
    {
        public string Name { get; set; }
        public int CharacterID { get; set; }

        public int HealthState { get; set; }
        public int SoulLost { get; set; }

        public long Coin { get; set; }
        public long LockCoin { get; set; }
        public long Dimond { get; set; }
        public long RCoin { get; set; }

        public int VipDays { get; set; }
        public int Level { get; set; }

        public int Job { get; set; }
        public int JobLevel { get; set; }

        public int CardTimeHour { get; set; }
        public int CardTimeMinute { get; set; }

        public string BattlingPetGuid { get; set; }

        public string CustomTitle { get; set; }
        public int SecletTitleId { get; set; }

        public bool IsFront { get; set; }

        public string MapID { get; set; }
        public int Dong { get; set; }
        public int Nan { get; set; }

        public int NowExp { get; set; }

        public float HPNow { get; set; }
        public float MPNow { get; set; }

        public int BPLeft { get; set; }
        public int HealthBP { get; set; }
        public int StrBP { get; set; }
        public int DefBP { get; set; }
        public int SpeedBP { get; set; }
        public int MagicBP { get; set; }

        public int Meili { get; set; }
        public int Naili { get; set; }
        public int Zhili { get; set; }
        public int Lingqiao { get; set; }

        public int Zhanji { get; set; }
        public int Rongyao { get; set; }

        public int MaxItemCount { get; set; }
        public int MaxBankItemCount { get; set; }
        public int MaxPetCount { get; set; }
        public int MaxBankPetCount { get; set; }
        public int MaxSkillCount { get; set; }

        public int FamilyId { get; set; }

        public int Fram { get; set; }
        public int FramRestrict { get; set; }

        public List<int> DressId { get; set; }
        public List<int> TujianId { get; set; }

        public DateTime LastEatTime { get; set; }
    }
}
