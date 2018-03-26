﻿using System;
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Collections.Generic;
using System.Reflection;

namespace ETModel
{
	public static class MongoHelper
	{
		static MongoHelper()
		{
			Type bsonClassMap = typeof(BsonClassMap);
			MethodInfo methodInfo = bsonClassMap.GetMethod("RegisterClassMap", new Type[] { });

			Type[] types = typeof(Game).Assembly.GetTypes();
			foreach (Type type in types)
			{
				if (!type.IsSubclassOf(typeof(Component)))
				{
					continue;
				}
				methodInfo.MakeGenericMethod(type).Invoke(null, null);
			}

			BsonSerializer.RegisterSerializer(new EnumSerializer<NumericType>(BsonType.String));
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

            EnumerableInterfaceImplementerSerializer<List<int>> e =
                new EnumerableInterfaceImplementerSerializer<List<int>>();

            EnumerableInterfaceImplementerSerializer<List<int>, int> elistint =
                new EnumerableInterfaceImplementerSerializer<List<int>, int>();
        }

	}
}