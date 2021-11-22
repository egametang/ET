using System;
using System.IO;

namespace ET
{
	public enum ParserState
	{
		PacketSize,
		PacketBody
	}

	public class PacketParser
	{
		private readonly CircularBuffer buffer;
		private int packetSize;
		private ParserState state;
		public AService service;
		private readonly byte[] cache = new byte[8];
		public const int InnerPacketSizeLength = 4;
		public const int OuterPacketSizeLength = 2;
		public MemoryStream MemoryStream;

		public PacketParser(CircularBuffer buffer, AService service)
		{
			this.buffer = buffer;
			this.service = service;
		}

		public bool Parse()
		{
			while (true)
			{
				switch (this.state)
				{
					case ParserState.PacketSize:
					{
						if (this.service.ServiceType == ServiceType.Inner)
						{
							if (this.buffer.Length < InnerPacketSizeLength)
							{
								return false;
							}

							this.buffer.Read(this.cache, 0, InnerPacketSizeLength);

							this.packetSize = BitConverter.ToInt32(this.cache, 0);
							if (this.packetSize > ushort.MaxValue * 16 || this.packetSize < Packet.MinPacketSize)
							{
								throw new Exception($"recv packet size error, 可能是外网探测端口: {this.packetSize}");
							}
						}
						else
						{
							if (this.buffer.Length < OuterPacketSizeLength)
							{
								return false;
							}

							this.buffer.Read(this.cache, 0, OuterPacketSizeLength);

							this.packetSize = BitConverter.ToUInt16(this.cache, 0);
							if (this.packetSize < Packet.MinPacketSize)
							{
								throw new Exception($"recv packet size error, 可能是外网探测端口: {this.packetSize}");
							}
						}

						this.state = ParserState.PacketBody;
						break;
					}
					case ParserState.PacketBody:
					{
						if (this.buffer.Length < this.packetSize)
						{
							return false;
						}

						MemoryStream memoryStream = MessageSerializeHelper.GetStream(this.packetSize);
						this.buffer.Read(memoryStream, this.packetSize);
						//memoryStream.SetLength(this.packetSize - Packet.MessageIndex);
						this.MemoryStream = memoryStream;

						if (this.service.ServiceType == ServiceType.Inner)
						{
							memoryStream.Seek(Packet.MessageIndex, SeekOrigin.Begin);
						}
						else
						{
							memoryStream.Seek(Packet.OpcodeLength, SeekOrigin.Begin);
						}

						this.state = ParserState.PacketSize;
						return true;
					}
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}