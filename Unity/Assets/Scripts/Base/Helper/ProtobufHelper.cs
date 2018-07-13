﻿using System;
using System.ComponentModel;
using System.IO;
using Microsoft.IO;
using ProtoBuf;

namespace ETModel
{
	public static class ProtobufHelper
	{
		private static readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();

		public static byte[] ToBytes(object message)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Serializer.Serialize(ms, message);
				return ms.ToArray();
			}
		}

		public static T FromBytes<T>(byte[] bytes)
		{
			T t;
			using (MemoryStream ms = recyclableMemoryStreamManager.GetStream("protobuf", bytes, 0, bytes.Length))
			{
				t = Serializer.Deserialize<T>(ms);
			}
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
			T t;
			using (MemoryStream ms = recyclableMemoryStreamManager.GetStream("protobuf", bytes, index, length))
			{
				t = Serializer.Deserialize<T>(ms);
			}
			ISupportInitialize iSupportInitialize = t as ISupportInitialize;
			if (iSupportInitialize == null)
			{
				return t;
			}
			iSupportInitialize.EndInit();
			return t;
		}

		public static object FromBytes(Type type, byte[] bytes)
		{
			object t;
			using (MemoryStream ms = recyclableMemoryStreamManager.GetStream("protobuf", bytes, 0, bytes.Length))
			{
				t = Serializer.NonGeneric.Deserialize(type, ms);
			}
			ISupportInitialize iSupportInitialize = t as ISupportInitialize;
			if (iSupportInitialize == null)
			{
				return t;
			}
			iSupportInitialize.EndInit();
			return t;
		}

		public static object FromBytes(Type type, byte[] bytes, int index, int length)
		{
			object t;
			using (MemoryStream ms = recyclableMemoryStreamManager.GetStream("protobuf", bytes, index, length))
			{
				t = Serializer.NonGeneric.Deserialize(type, ms);
			}
			ISupportInitialize iSupportInitialize = t as ISupportInitialize;
			if (iSupportInitialize == null)
			{
				return t;
			}
			iSupportInitialize.EndInit();
			return t;
		}
		
		public static object FromStream(Type type, Stream stream)
		{
			object t = Serializer.NonGeneric.Deserialize(type, stream);
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