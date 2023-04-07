namespace ET
{
    public abstract class Object
    {
        public override string ToString()
        {
            return JsonHelper.ToJson(this);
        }
        
        public string ToJson()
        {
            return MongoHelper.ToJson(this);
        }
        
        public byte[] ToBson()
        {
            return MongoHelper.Serialize(this);
        }
    }
}