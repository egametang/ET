using System;
using System.ComponentModel;
using System.IO;
using Google.Protobuf;

namespace ETModel
{
	public static class ProtobufHelper
	{
		public static byte[] ToBytes(object message)
		{
			return ((Google.Protobuf.IMessage) message).ToByteArray();
		}
		
		public static object FromBytes(Type type, byte[] bytes, int index, int count)
		{
			object message = Activator.CreateInstance(type);
			((Google.Protobuf.IMessage)message).MergeFrom(bytes, index, count);
			ISupportInitialize iSupportInitialize = message as ISupportInitialize;
			if (iSupportInitialize == null)
			{
				return message;
			}
			iSupportInitialize.EndInit();
			return message;
		}
		
		public static object FromStream(Type type, MemoryStream stream)
		{
			object message = Activator.CreateInstance(type);
			((Google.Protobuf.IMessage)message).MergeFrom(stream.GetBuffer(), (int)stream.Position, (int)stream.Length);
			ISupportInitialize iSupportInitialize = message as ISupportInitialize;
			if (iSupportInitialize == null)
			{
				return message;
			}
			iSupportInitialize.EndInit();
			return message;
		}
		
		public static object FromStream(object message, MemoryStream stream)
		{
			// 这个message可以从池中获取，减少gc
			((Google.Protobuf.IMessage)message).MergeFrom(stream.GetBuffer(), (int)stream.Position, (int)stream.Length);
			ISupportInitialize iSupportInitialize = message as ISupportInitialize;
			if (iSupportInitialize == null)
			{
				return message;
			}
			iSupportInitialize.EndInit();
			return message;
		}
	}
}