using System;
using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [DisableNew]
    public abstract class MessageObject: ProtoObject, IMessage, IPool
    {
        public virtual void Dispose()
        {
        }

        [BsonIgnore]
        public bool IsFromPool { get; set; }
    }
}