using System;

namespace ET
{
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class FriendOfAttribute : Attribute
    {
        public Type Type;
        
        public FriendOfAttribute(Type type)
        {
            this.Type = type;
        }
    }
}