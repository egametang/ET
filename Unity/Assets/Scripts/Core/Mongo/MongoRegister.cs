using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using UnityEngine;

namespace ET
{
    public static class MongoRegister
    {
        public static void Init()
        {
        }
        
        static MongoRegister()
        {
            // 自动注册IgnoreExtraElements

            ConventionPack conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };

            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

            BsonSerializer.RegisterSerializer(typeof(Vector3), new StructBsonSerialize<Vector3>());
            BsonSerializer.RegisterSerializer(typeof(Vector4), new StructBsonSerialize<Vector4>());
            BsonSerializer.RegisterSerializer(typeof(Quaternion), new StructBsonSerialize<Quaternion>());

            Dictionary<string, Type> types = EventSystem.Instance.GetTypes();
            foreach (Type type in types.Values)
            {
                if (!type.IsSubclassOf(typeof (Object)))
                {
                    continue;
                }

                if (type.IsGenericType)
                {
                    continue;
                }

                BsonClassMap.LookupClassMap(type);
            }
        }
    }
}