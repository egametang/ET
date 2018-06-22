using System;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ETModel
{
    /// <summary>
    /// 账号信息
    /// </summary>
    [ProtoContract]
    [BsonIgnoreExtraElements]
    public class AccountInfo : Entity
    {
        //用户名
        public string Account { get; set; }

        //密码
        public string Password { get; set; }

        //密码Md5
        public string PasswordGuid { get; set; }

        //安全问题
        public string SafeQuestion { get; set; }

        //安全答案
        public string SafeAnswer { get; set; }

        //支付宝帐号
        public string Alipay { get; set; }

        //是否停权
        public bool IsAccountBand { get; set; }

        //允许登录时间
        public DateTime AllowLoginTime { get; set; }

        //帐号创建时间
        public DateTime CrateTime { get; set; }

        //最后登录时间
        public DateTime LastLoginTime { get; set; }
    }
}
