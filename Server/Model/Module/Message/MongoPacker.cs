using System;
using System.IO;

namespace ETModel
{
	public class MongoPacker: IMessagePacker
	{
		public byte[] SerializeTo(object obj)
		{
			return MongoHelper.ToBson(obj);
		}

		public void SerializeTo(object obj, MemoryStream stream)
		{
			MongoHelper.ToBson(obj, stream);
		}

		public object DeserializeFrom(Type type, byte[] bytes, int index, int count)
		{
			return MongoHelper.FromBson(type, bytes, index, count);
		}

		public object DeserializeFrom(object instance, byte[] bytes, int index, int count)
		{
			return MongoHelper.FromBson(instance, bytes, index, count);
		}

		public object DeserializeFrom(Type type, MemoryStream stream)
		{
			return MongoHelper.FromStream(type, stream);
		}

		public object DeserializeFrom(object instance, MemoryStream stream)
		{
			return MongoHelper.FromBson(instance, stream);
		}
	}
}
