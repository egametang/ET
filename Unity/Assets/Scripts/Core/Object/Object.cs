namespace ET
{
    public abstract class Object
    {
        public override string ToString()
        {
            return MongoHelper.ToJson(this);
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