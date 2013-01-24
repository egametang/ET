using System.IO;
using ProtoBuf;

namespace Helper
{
	public static class ProtobufHelper
	{
		public static byte[] ToBytes<T>(T message)
		{
			var ms = new MemoryStream();
			Serializer.Serialize(ms, message);
			return ms.ToArray();
		}

		public static T FromBytes<T>(byte[] bytes)
		{
			var ms = new MemoryStream(bytes, 0, bytes.Length);
			return Serializer.Deserialize<T>(ms);
		}
	}
}
