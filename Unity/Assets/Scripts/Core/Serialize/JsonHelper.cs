using System;

namespace ET
{
    public static class JsonHelper
    {
        public static string ToJson(object o)
        {
            return MongoHelper.ToJson(o);
        }
        
        public static object FromJson(Type type, string json)
        {
            return MongoHelper.FromJson(type, json);
        }
        
        public static T FromJson<T>(string json)
        {
            return MongoHelper.FromJson<T>(json);
        }
    }
}