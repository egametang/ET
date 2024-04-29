using System;
using System.ComponentModel;
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace ET
{
    public static class MongoHelper
    {
        [StaticField]
        private static readonly JsonWriterSettings defaultSettings = new() { OutputMode = JsonOutputMode.RelaxedExtendedJson };
        
        public static string ToJson(object obj)
        {
            try
            {
                if (obj is ISupportInitialize supportInitialize)
                {
                    supportInitialize.BeginInit();
                }
                return obj.ToJson(defaultSettings);
            }
            catch (Exception e)
            {
                throw new Exception($"to json error {obj.GetType().FullName}\n{e}");
            }
        }

        public static string ToJson(object obj, JsonWriterSettings settings)
        {
            try
            {
                if (obj is ISupportInitialize supportInitialize)
                {
                    supportInitialize.BeginInit();
                }
                return obj.ToJson(settings);
            }
            catch (Exception e)
            {
                throw new Exception($"to json error {obj.GetType().FullName}\n{e}");
            }
        }

        public static T FromJson<T>(string str)
        {
            try
            {
                return BsonSerializer.Deserialize<T>(str);
            }
            catch (Exception e)
            {
                throw new Exception($"from json error: {str}\n{e}");
            }
        }

        public static object FromJson(Type type, string str)
        {
            try
            {
                return BsonSerializer.Deserialize(str, type);
            }
            catch (Exception e)
            {
                throw new Exception($"from json error: {str}\n{e}");
            }
        }

        public static byte[] Serialize(object obj)
        {
            try
            {
                if (obj is ISupportInitialize supportInitialize)
                {
                    supportInitialize.BeginInit();
                }
                return obj.ToBson();
            }
            catch (Exception e)
            {
                throw new Exception($"Serialize error {obj.GetType().FullName}\n{e}");
            }
        }

        public static void Serialize(object message, MemoryStream stream)
        {
            try
            {
                if (message is ISupportInitialize supportInitialize)
                {
                    supportInitialize.BeginInit();
                }

                using BsonBinaryWriter bsonWriter = new(stream, BsonBinaryWriterSettings.Defaults);
            
                BsonSerializationContext context = BsonSerializationContext.CreateRoot(bsonWriter);
                BsonSerializationArgs args = default;
                args.NominalType = typeof (object);
                IBsonSerializer serializer = BsonSerializer.LookupSerializer(args.NominalType);
                serializer.Serialize(context, args, message);
            }
            catch (Exception e)
            {
                throw new Exception($"Serialize error {message.GetType().FullName}\n{e}");
            }
        }

        public static object Deserialize(Type type, byte[] bytes)
        {
            try
            {
                return BsonSerializer.Deserialize(bytes, type);
            }
            catch (Exception e)
            {
                throw new Exception($"from bson error: {type.FullName} {bytes.Length}", e);
            }
        }

        public static object Deserialize(Type type, byte[] bytes, int index, int count)
        {
            try
            {
                using MemoryStream memoryStream = new(bytes, index, count);
                
                return BsonSerializer.Deserialize(memoryStream, type);
            }
            catch (Exception e)
            {
                throw new Exception($"from bson error: {type.FullName} {bytes.Length} {index} {count}", e);
            }
        }

        public static object Deserialize(Type type, Stream stream)
        {
            try
            {
                return BsonSerializer.Deserialize(stream, type);
            }
            catch (Exception e)
            {
                throw new Exception($"from bson error: {type.FullName} {stream.Position} {stream.Length}", e);
            }
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            try
            {
                using MemoryStream memoryStream = new(bytes);
                
                return (T)BsonSerializer.Deserialize(memoryStream, typeof (T));
            }
            catch (Exception e)
            {
                throw new Exception($"from bson error: {typeof (T).FullName} {bytes.Length}", e);
            }
        }

        public static T Deserialize<T>(byte[] bytes, int index, int count)
        {
            return (T)Deserialize(typeof (T), bytes, index, count);
        }

        public static T Clone<T>(T t)
        {
            return Deserialize<T>(Serialize(t));
        }
    }
}