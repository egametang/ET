using System;
using System.IO;
using System.ComponentModel;
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
            if (obj is ISupportInitialize supportInitialize)
            {
                supportInitialize.BeginInit();
            }

            return obj.ToJson(defaultSettings);
        }

        public static string ToJson(object obj, JsonWriterSettings settings)
        {
            if (obj is ISupportInitialize supportInitialize)
            {
                supportInitialize.BeginInit();
            }

            return obj.ToJson(settings);
        }

        public static T FromJson<T>(string str)
        {
            try
            {
                var o = BsonSerializer.Deserialize<T>(str);
                if (o is ISupportInitialize supportInitialize)
                {
                    supportInitialize.EndInit();
                }

                return o;
            }
            catch (Exception e)
            {
                throw new Exception($"from json error: {typeof(T).FullName} {str}", e);
            }
        }

        public static object FromJson(Type type, string str)
        {
            try
            {
                var o = BsonSerializer.Deserialize(str, type);
                if (o is ISupportInitialize supportInitialize)
                {
                    supportInitialize.EndInit();
                }

                return o;
            }
            catch (Exception e)
            {
                throw new Exception($"from json error: {type.FullName} {str}", e);
            }
        }

        public static byte[] Serialize(object obj)
        {
            if (obj is ISupportInitialize supportInitialize)
            {
                supportInitialize.BeginInit();
            }

            return obj.ToBson();
        }

        public static void Serialize(object message, MemoryStream stream)
        {
            if (message is ISupportInitialize supportInitialize)
            {
                supportInitialize.BeginInit();
            }

            using BsonBinaryWriter bsonWriter = new(stream, BsonBinaryWriterSettings.Defaults);

            BsonSerializationContext context = BsonSerializationContext.CreateRoot(bsonWriter);
            BsonSerializationArgs    args    = default;
            args.NominalType = typeof(object);
            IBsonSerializer serializer = BsonSerializer.LookupSerializer(args.NominalType);
            serializer.Serialize(context, args, message);
        }

        public static object Deserialize(Type type, byte[] bytes)
        {
            try
            {
                var o = BsonSerializer.Deserialize(bytes, type);
                if (o is ISupportInitialize supportInitialize)
                {
                    supportInitialize.EndInit();
                }

                return o;
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
                var o = BsonSerializer.Deserialize(memoryStream, type);
                if (o is ISupportInitialize supportInitialize)
                {
                    supportInitialize.EndInit();
                }

                return o;
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
                var o = BsonSerializer.Deserialize(stream, type);
                if (o is ISupportInitialize supportInitialize)
                {
                    supportInitialize.EndInit();
                }

                return o;
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
                var o = (T)BsonSerializer.Deserialize(memoryStream, typeof(T));
                if (o is ISupportInitialize supportInitialize)
                {
                    supportInitialize.EndInit();
                }

                return o;
            }
            catch (Exception e)
            {
                throw new Exception($"from bson error: {typeof(T).FullName} {bytes.Length}", e);
            }
        }

        public static T Deserialize<T>(byte[] bytes, int index, int count)
        {
            return (T)Deserialize(typeof(T), bytes, index, count);
        }

        public static T Clone<T>(T t)
        {
            return Deserialize<T>(Serialize(t));
        }
    }
}