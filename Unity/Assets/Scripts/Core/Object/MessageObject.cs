﻿using System;
using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    public abstract class MessageObject: ProtoObject, IMessage, IDisposable
    {
        public virtual void Dispose()
        {
        }

        [BsonIgnore]
        public bool IsFromPool { get; set; }
    }
}