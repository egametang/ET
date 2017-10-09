namespace Model
{
    public class AccountInfo : EntityDB
    {
        public string Account;
        public string Password;
        public long RegisterTime;

        public AccountInfo(long id) : base(id)
        { 

        }
    }
}