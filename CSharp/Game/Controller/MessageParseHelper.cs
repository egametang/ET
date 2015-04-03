using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Common.Helper;
using Model;
using MongoDB.Bson;

namespace Controller
{
	public static class MessageParseHelper
	{
		public static void LogicParseClientToGateToLogicMessage(byte[] message, Env env)
		{
			ushort opcode = BitConverter.ToUInt16(message, 0);
			env[EnvKey.Opcode] = opcode;
			byte[] bytes = new byte[12];
			Array.Copy(message, 2, bytes, 0, 12);
			env[EnvKey.MessageUnitId] = new ObjectId(bytes);
			Type messageType =
					World.Instance.GetComponent<MessageComponent>().GetClassType(opcode);
			env[EnvKey.Message] = MongoHelper.FromBson(messageType, message, 14, message.Length - 14);
		}

		public static void GateParseClientToGateMessage(byte[] message, Env env)
		{
			ushort opcode = BitConverter.ToUInt16(message, 0);
			env[EnvKey.Opcode] = opcode;
			Type messageType =
					World.Instance.GetComponent<MessageComponent>().GetClassType(opcode);
			env[EnvKey.Message] = MongoHelper.FromBson(messageType, message, 2, message.Length - 2);
		}

		public static void LogicParseRpcRequestMessage(byte[] message, Env env)
		{
			ushort opcode = BitConverter.ToUInt16(message, 0);
			int requestId = BitConverter.ToInt32(message, 2);
			env[EnvKey.Opcode] = opcode;
			env[EnvKey.RpcRequestId] = requestId;
			Type messageType = World.Instance.GetComponent<MessageComponent>().GetClassType(opcode);
			env[EnvKey.Message] = MongoHelper.FromBson(messageType, message, 2, message.Length - 2);
		}

		/// <summary>
		/// 客户端的消息经gate转发给logic server需要在协议中插入unitid
		/// </summary>
		public static byte[] ClientToGateMessageChangeToLogicMessage(byte[] messageBytes, ObjectId id)
		{
			byte[] idBuffer = id.ToByteArray();
			byte[] buffer = new byte[messageBytes.Length + 12];
			Array.Copy(messageBytes, 0, buffer, 0, 2);
			Array.Copy(idBuffer, 0, buffer, 2, idBuffer.Length);
			Array.Copy(messageBytes, 2, buffer, 14, messageBytes.Length - 2);
			return buffer;
		}

		/// <summary>
		/// Logic的消息经gate转发给client,需要在协议中删除unitid
		/// </summary>
		public static byte[] LogicToGateMessageChangeToClientMessage(byte[] messageBytes)
		{
			byte[] buffer = new byte[messageBytes.Length - 12];
			Array.Copy(messageBytes, 0, buffer, 0, 2);
			Array.Copy(messageBytes, 14, buffer, 2, messageBytes.Length - 14);
			return buffer;
		}
	}
}
