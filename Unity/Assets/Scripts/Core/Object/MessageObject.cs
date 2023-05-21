using System;
using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    public abstract class MessageObject: ProtoObject, IDisposable
    {
        [BsonIgnore]
        public bool IsFromPool;

        public virtual void Dispose()
        {
        }
    }
}