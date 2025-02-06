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
		private readonly AService service;
		private readonly byte[] cache = new byte[8];
		public const int InnerPacketSizeLength = 4;
		public const int OuterPacketSizeLength = 2;
		public MemoryBuffer MemoryBuffer;

		public PacketParser(CircularBuffer buffer, AService service)
		{
			this.buffer = buffer;
			this.service = service;
		}

		public bool Parse(out MemoryBuffer memoryBuffer)
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
								memoryBuffer = null;
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
								memoryBuffer = null;
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
							memoryBuffer = null;
							return false;
						}

						memoryBuffer = this.service.Fetch(this.packetSize);
						this.buffer.Read(memoryBuffer, this.packetSize);
						//memoryStream.SetLength(this.packetSize - Packet.MessageIndex);

						memoryBuffer.Seek(0, SeekOrigin.Begin);

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