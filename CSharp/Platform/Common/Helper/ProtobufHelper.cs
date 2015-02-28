using System.ComponentModel;
using System.IO;
using ProtoBuf;

namespace Common.Helper
{
	public static class ProtobufHelper
	{
		public static byte[] ToBytes<T>(T message)
		{
			MemoryStream ms = new MemoryStream();
			Serializer.Serialize(ms, message);
			return ms.ToArray();
		}

		public static T FromBytes<T>(byte[] bytes)
		{
			MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length);
			T t = Serializer.Deserialize<T>(ms);
			ISupportInitialize iSupportInitialize = t as ISupportInitialize;
			if (iSupportInitialize == null)
			{
				return t;
			}
			iSupportInitialize.EndInit();
			return t;
		}

		public static T FromBytes<T>(byte[] bytes, int index, int length)
		{
			MemoryStream ms = new MemoryStream(bytes, index, length);
			T t = Serializer.Deserialize<T>(ms);
			ISupportInitialize iSupportInitialize = t as ISupportInitialize;
			if (iSupportInitialize == null)
			{
				return t;
			}
			iSupportInitialize.EndInit();
			return t;
		}
	}
}