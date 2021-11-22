using System;
using System.ComponentModel;

namespace ET
{
    public abstract class Object: ISupportInitialize, IDisposable
    {
        public virtual void BeginInit()
        {
        }
        
        public virtual void EndInit()
        {
        }

        public virtual void Dispose()
        {
        }
        
        public override string ToString()
        {
            return JsonHelper.ToJson(this);
        }
    }
}