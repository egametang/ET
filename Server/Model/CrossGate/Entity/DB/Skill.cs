using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 技能信息
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Skill : Entity
    {
        public int SkillID { get; set; }
        public int Level { get; set; }
        public int NowExp { get; set; }
        public int Index { get; set; }
    }
}
