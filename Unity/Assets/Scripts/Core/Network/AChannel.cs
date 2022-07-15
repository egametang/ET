using System;
using System.IO;
using System.Net;

namespace ET
{
	public enum ChannelType
	{
		Connect,
		Accept,
	}

	public struct Packet
	{
		public const int MinPacketSize = 2;
		public const int OpcodeIndex = 8;
		public const int KcpOpcodeIndex = 0;
		public const int OpcodeLength = 2;
		public const int ActorIdIndex = 0;
		public const int ActorIdLength = 8;
		public const int MessageIndex = 10;

		public ushort Opcode;
		public long ActorId;
		public MemoryStream MemoryStream;
	}

	public abstract class AChannel: IDisposable
	{
		public long Id;
		
		public ChannelType ChannelType { get; protected set; }

		public int Error { get; set; }
		
		public IPEndPoint RemoteAddress { get; set; }

		
		public bool IsDisposed
		{
			get
			{
				return this.Id == 0;	
			}
		}

		public abstract void Dispose();
	}
}