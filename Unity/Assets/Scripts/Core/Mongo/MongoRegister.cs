using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using Unity.Mathematics;

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

            BsonSerializer.RegisterSerializer(typeof(float3), new StructBsonSerialize<float3>());
            BsonSerializer.RegisterSerializer(typeof(float4), new StructBsonSerialize<float4>());
            BsonSerializer.RegisterSerializer(typeof(quaternion), new StructBsonSerialize<quaternion>());

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