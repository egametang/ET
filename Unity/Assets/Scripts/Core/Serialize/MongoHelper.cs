using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using Unity.Mathematics;

namespace ET
{
    public static class MongoHelper
    {
        private class StructBsonSerialize<TValue>: StructSerializerBase<TValue> where TValue : struct
        {
            public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TValue value)
            {
                Type nominalType = args.NominalType;

                IBsonWriter bsonWriter = context.Writer;

                bsonWriter.WriteStartDocument();

                FieldInfo[] fields = nominalType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (FieldInfo field in fields)
                {
                    bsonWriter.WriteName(field.Name);
                    BsonSerializer.Serialize(bsonWriter, field.FieldType, field.GetValue(value));
                }

                bsonWriter.WriteEndDocument();
            }

            public override TValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                //boxing is required for SetValue to work
                object obj = new TValue();
                Type actualType = args.NominalType;
                IBsonReader bsonReader = context.Reader;

                bsonReader.ReadStartDocument();

                while (bsonReader.State != BsonReaderState.EndOfDocument)
                {
                    switch (bsonReader.State)
                    {
                        case BsonReaderState.Name:
                        {
                            string name = bsonReader.ReadName(Utf8NameDecoder.Instance);
                            FieldInfo field = actualType.GetField(name);
                            if (field != null)
                            {
                                object value = BsonSerializer.Deserialize(bsonReader, field.FieldType);
                                field.SetValue(obj, value);
                            }

                            break;
                        }
                        case BsonReaderState.Type:
                        {
                            bsonReader.ReadBsonType();
                            break;
                        }
                        case BsonReaderState.Value:
                        {
                            bsonReader.SkipValue();
                            break;
                        }
                    }
                }

                bsonReader.ReadEndDocument();

                return (TValue)obj;
            }
        }

        [StaticField]
        private static readonly JsonWriterSettings defaultSettings = new() { OutputMode = JsonOutputMode.RelaxedExtendedJson };

        static MongoHelper()
        {
            // 自动注册IgnoreExtraElements

            ConventionPack conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };

            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

            RegisterStruct<float2>();
            RegisterStruct<float3>();
            RegisterStruct<float4>();
            RegisterStruct<quaternion>();

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

        public static void Init()
        {
        }

        public static void RegisterStruct<T>() where T : struct
        {
            BsonSerializer.RegisterSerializer(typeof (T), new StructBsonSerialize<T>());
        }

        public static string ToJson(object obj)
        {
            return obj.ToJson(defaultSettings);
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

        public static byte[] Serialize(object obj)
        {
            return obj.ToBson();
        }

        public static void Serialize(object message, MemoryStream stream)
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

        public static object Deserialize(Type type, byte[] bytes)
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

        public static object Deserialize(Type type, byte[] bytes, int index, int count)
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

        public static object Deserialize(Type type, Stream stream)
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

        public static T Deserialize<T>(byte[] bytes)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    return (T)BsonSerializer.Deserialize(memoryStream, typeof (T));
                }
            }
            catch (Exception e)
            {
                throw new Exception($"from bson error: {typeof (T).Name}", e);
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