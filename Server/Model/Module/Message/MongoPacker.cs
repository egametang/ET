using System;

namespace ETModel
{
	public class MongoPacker: IMessagePacker
	{
		public byte[] SerializeToByteArray(object obj)
		{
			return MongoHelper.ToBson(obj);
		}

		public string SerializeToText(object obj)
		{
			return MongoHelper.ToJson(obj);
		}

		public object DeserializeFrom(Type type, byte[] bytes)
		{
			return MongoHelper.FromBson(type, bytes);
		}

		public object DeserializeFrom(Type type, byte[] bytes, int index, int count)
		{
			return MongoHelper.FromBson(type, bytes, index, count);
		}

		public T DeserializeFrom<T>(byte[] bytes)
		{
			return MongoHelper.FromBson<T>(bytes);
		}

		public T DeserializeFrom<T>(byte[] bytes, int index, int count)
		{
			return MongoHelper.FromBson<T>(bytes, index, count);
		}

		public T DeserializeFrom<T>(string str)
		{
			return MongoHelper.FromJson<T>(str);
		}

		public object DeserializeFrom(Type type, string str)
		{
			return MongoHelper.FromJson(type, str);
		}
	}
}
