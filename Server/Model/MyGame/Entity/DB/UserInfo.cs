using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 用户信息
    /// </summary>
    [BsonIgnoreExtraElements]
    public class UserInfo : Entity
    {
        //昵称
        public string NickName { get; set; }

        //性别
        public int Sex { get; set; }

        //头像
        public int Photo { get; set; }

        //余额
        public float Gold { get; set; }

        //房卡
        public int RoomCard { get; set; }
    }
}
