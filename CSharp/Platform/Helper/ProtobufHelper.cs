using System.IO;
using ProtoBuf;

namespace Helper
{
	public static class ProtobufHelper
	{
		public static byte[] ProtoToBytes<T>(T message)
		{
			var ms = new MemoryStream();
			Serializer.Serialize(ms, message);
			return ms.ToArray();
		}
	}
}
