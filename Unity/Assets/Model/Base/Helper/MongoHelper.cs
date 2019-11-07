using System;
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Conventions;
using UnityEngine;

namespace ETModel
{
	public static class MongoHelper
	{
        static MongoHelper()
        {
	        // 自动注册IgnoreExtraElements
	        
	        ConventionPack conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
	        
	        ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);
	        
            Type[] types = typeof(Game).Assembly.GetTypes();
           
            foreach (Type type in types)
            {
                if (!type.IsSubclassOf(typeof(Entity)))
                {
                    continue;
                }

                if (type.IsGenericType)
                {
                    continue;
                }

                try
                {
	                BsonClassMap.LookupClassMap(type);
                }
                catch (Exception e)
                {
	                Log.Error($"11111111111111111: {type.Name} {e}");
                }
                
            }
#if SERVER
            BsonSerializer.RegisterSerializer(typeof(Vector3), new StructBsonSerialize<Vector3>());
#else
            BsonSerializer.RegisterSerializer(typeof(Vector4), new StructBsonSerialize<Vector4>());
            BsonSerializer.RegisterSerializer(typeof(Vector2Int), new StructBsonSerialize<Vector2Int>());
#endif
        }
        
        public static void Init()
        {
	        // 调用这个是为了调用MongoHelper的静态方法
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
			return BsonSerializer.Deserialize<T>(str);
		}

		public static object FromJson(Type type, string str)
		{
			return BsonSerializer.Deserialize(str, type);
		}

		public static byte[] ToBson(object obj)
		{
			return obj.ToBson();
		}
		
		public static void ToBson(object obj, MemoryStream stream)
		{
			using (BsonBinaryWriter bsonWriter = new BsonBinaryWriter(stream, BsonBinaryWriterSettings.Defaults))
			{
				BsonSerializationContext context = BsonSerializationContext.CreateRoot(bsonWriter);
				BsonSerializationArgs args = default (BsonSerializationArgs);
				args.NominalType = typeof(object);
				IBsonSerializer serializer = BsonSerializer.LookupSerializer(args.NominalType);
				serializer.Serialize(context, args, obj);
			}
		}
		
		public static object FromBson(Type type, byte[] bytes)
		{
			return BsonSerializer.Deserialize(bytes, type);
		}

		public static object FromBson(Type type, byte[] bytes, int index, int count)
		{
			using (MemoryStream memoryStream = new MemoryStream(bytes, index, count))
			{
				return BsonSerializer.Deserialize(memoryStream, type);
			}
		}
		
		public static object FromBson(object instance, byte[] bytes, int index, int count)
		{
			using (MemoryStream memoryStream = new MemoryStream(bytes, index, count))
			{
				return BsonSerializer.Deserialize(memoryStream, instance.GetType());
			}
		}
		
		public static object FromBson(object instance, Stream stream)
		{
			return BsonSerializer.Deserialize(stream, instance.GetType());
		}
		
		public static object FromStream(Type type, Stream stream)
		{
			return BsonSerializer.Deserialize(stream, type);
		}

		public static T FromBson<T>(byte[] bytes)
		{
			using (MemoryStream memoryStream = new MemoryStream(bytes))
			{
				return (T) BsonSerializer.Deserialize(memoryStream, typeof (T));
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


        public static void AvoidAOT()
        {
            ArraySerializer<int> aint = new ArraySerializer<int>();
            ArraySerializer<string> astring = new ArraySerializer<string>();
            ArraySerializer<long> along = new ArraySerializer<long>();
            EnumerableInterfaceImplementerSerializer<List<int>> e = new EnumerableInterfaceImplementerSerializer<List<int>>();
            EnumerableInterfaceImplementerSerializer<List<int>, int> elistint = new EnumerableInterfaceImplementerSerializer<List<int>, int>();
        }

	}


}