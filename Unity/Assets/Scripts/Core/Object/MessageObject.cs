using System;
using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    public abstract class MessageObject: ProtoObject
    {
        [BsonIgnore]
        public bool IsFromPool;
    }
}