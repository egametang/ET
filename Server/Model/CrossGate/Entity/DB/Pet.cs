using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 宠物信息
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Pet : Entity
    {
        public string Name { get; set; }
        public int Level { get; set; }

        public int EnemyBaseID { get; set; }
        public int CharacterID { get; set; }
        public int Guid { get; set; }

        public int NowExp { get; set; }
        public int ZhongZhu { get; set; }
        public int BaseTime { get; set; }
        public int HealthState { get; set; }
        public int BattleState { get; set; }

        public int HighestDang { get; set; }
        public int HealthDang { get; set; }
        public int StrDang { get; set; }
        public int DefDang { get; set; }
        public int SpeedDang { get; set; }
        public int MagicDang { get; set; }

        public int RamdonHealthDang { get; set; }
        public int RamdonStrDang { get; set; }
        public int RamdonDefDang { get; set; }
        public int RamdonSpeedDang { get; set; }
        public int RamdonMagicDang { get; set; }

        public int HpNow { get; set; }
        public int MpNow { get; set; }

        public int LeftBP { get; set; }
        public int HealthBP { get; set; }
        public int StrBP { get; set; }
        public int DefendBP { get; set; }
        public int SpeedBP { get; set; }
        public int MagicBP { get; set; }

        public int AddHealthBP { get; set; }
        public int AddStrBP { get; set; }
        public int AddDefendBP { get; set; }
        public int AddSpeedBP { get; set; }
        public int AddMagicBP { get; set; }

        public int Di { get; set; }
        public int Shui { get; set; }
        public int Huo { get; set; }
        public int Feng { get; set; }

        public int MaxSkillCount { get; set; }
        public List<int> Skill { get; set; }

        public int Bisha { get; set; }
        public int Fanji { get; set; }
        public int Minzhong { get; set; }

        public int Kanghunshui { get; set; }
        public int Kangshihua { get; set; }
        public int Kangjiuzui { get; set; }
        public int Kanghunluan { get; set; }
        public int Kangyiwang { get; set; }

        public DateTime LastEatTime { get; set; }

        public int ZhuanshuItemId { get; set; }
        public bool ZhuanshuItemIdHave { get; set; }

        public bool TradeAble { get; set; }
        public bool IsCatchByCard { get; set; }
        public int CatchedLevel { get; set; }
    }
}
