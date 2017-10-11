namespace Model
{
    public class AccountInfo : EntityDB
    {
        public string Account;          // 账号Id/微信udid
        public string Password;         // 账号密码，微信登录可忽略
        public int Money;               // 玩家金钱
        public string NickName;         // 玩家昵称
        public string IconAdress;       // 玩家微信头像地址
        public long RegisterTime;       // 玩家注册时间
        public long LastOnLineTime;     // 玩家上次登录时间
        public long LastOffLineTime;    // 玩家上次离线时间

        public AccountInfo(long id) : base(id)
        {

        }
    }
}