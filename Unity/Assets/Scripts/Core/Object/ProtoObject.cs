using System;
using System.ComponentModel;

namespace ET
{
    public abstract class ProtoObject: Object, ISupportInitialize
    {
        public object Clone()
        {
            byte[] bytes = MongoHelper.Serialize(this);
            return MongoHelper.Deserialize(this.GetType(), bytes, 0, bytes.Length);
        }
        
        public virtual void BeginInit()
        {
        }
        
        
        public virtual void EndInit()
        {
        }
    }
}