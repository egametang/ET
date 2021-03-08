using System;
using System.ComponentModel;
using ProtoBuf;

namespace ET
{
    public abstract class Object: ISupportInitialize, IDisposable
    {
        public Object()
        {
        }

        public virtual void BeginInit()
        {
        }

        [ProtoAfterDeserialization]
        public virtual void AfterDeserialization()
        {
            this.EndInit();
        }

        public virtual void EndInit()
        {
        }

        public virtual void Dispose()
        {
        }
    }

    public class ProtoObject: Object
    {
    }
}