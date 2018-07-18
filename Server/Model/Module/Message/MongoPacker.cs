using System;
using System.IO;

namespace ETModel
{
	public class MongoPacker: IMessagePacker
	{
		public byte[] SerializeToByteArray(object obj)
		{
			return MongoHelper.ToBson(obj);
		}

		public object DeserializeFrom(Type type, byte[] bytes, int index, int count)
		{
			return MongoHelper.FromBson(type, bytes, index, count);
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
