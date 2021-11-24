using System;
using System.ComponentModel;

namespace ET
{
    public abstract class Object
    {
        public override string ToString()
        {
            return JsonHelper.ToJson(this);
        }
    }
}