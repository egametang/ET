using System;
using System.Collections.Generic;
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using UnityEngine;

namespace ET
{
    public static class MongoHelper
    {
        public static void Register(Dictionary<string, Type> types)
        {
            // 自动注册IgnoreExtraElements

            ConventionPack conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };

            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

            BsonSerializer.RegisterSerializer(typeof(Vector3), new StructBsonSerialize<Vector3>());
            BsonSerializer.RegisterSerializer(typeof(Vector4), new StructBsonSerialize<Vector4>());
            BsonSerializer.RegisterSerializer(typeof(Quaternion), new StructBsonSerialize<Quaternion>());

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
        
        public static string ToJson(object obj)
        {
            return obj.ToJson();
        }

        public static string ToJson(object obj, JsonWriterSettings settings)
        {
            return obj.ToJson(settings);
        }

        public static T FromJson<T>(string str)
        {
            try
            {
                return BsonSerializer.Deserialize<T>(str);
            }
            catch (Exception e)
            {
                throw new Exception($"{str}\n{e}");
            }
        }

        public static object FromJson(Type type, string str)
        {
            return BsonSerializer.Deserialize(str, type);
        }

        public static byte[] ToBson(object obj)
        {
            return obj.ToBson();
        }

        public static void ToStream(object message, MemoryStream stream)
        {
            using (BsonBinaryWriter bsonWriter = new BsonBinaryWriter(stream, BsonBinaryWriterSettings.Defaults))
            {
                BsonSerializationContext context = BsonSerializationContext.CreateRoot(bsonWriter);
                BsonSerializationArgs args = default;
                args.NominalType = typeof (object);
                IBsonSerializer serializer = BsonSerializer.LookupSerializer(args.NominalType);
                serializer.Serialize(context, args, message);
            }
        }

        public static object FromBson(Type type, byte[] bytes)
        {
            try
            {
                return BsonSerializer.Deserialize(bytes, type);
            }
            catch (Exception e)
            {
                throw new Exception($"from bson error: {type.Name}", e);
            }
        }

        public static object FromBson(Type type, byte[] bytes, int index, int count)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(bytes, index, count))
                {
                    return BsonSerializer.Deserialize(memoryStream, type);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"from bson error: {type.Name}", e);
            }
        }

        public static object FromStream(Type type, Stream stream)
        {
            try
            {
                return BsonSerializer.Deserialize(stream, type);
            }
            catch (Exception e)
            {
                throw new Exception($"from bson error: {type.Name}", e);
            }
        }

        public static T FromBson<T>(byte[] bytes)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    return (T) BsonSerializer.Deserialize(memoryStream, typeof (T));
                }
            }
            catch (Exception e)
            {
                throw new Exception($"from bson error: {typeof (T).Name}", e);
            }
        }

        public static T FromBson<T>(byte[] bytes, int index, int count)
        {
            return (T) FromBson(typeof (T), bytes, index, count);
        }

        public static T Clone<T>(T t)
        {
            return FromBson<T>(ToBson(t));
        }
    }
}