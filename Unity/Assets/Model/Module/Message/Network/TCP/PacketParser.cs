using System;
using System.IO;
using Microsoft.IO;

namespace ETModel
{
	public enum ParserState
	{
		PacketSize,
		PacketBody
	}

	public static class Packet
	{
		public const int SizeLength = 2;
		public const int MinSize = 3;
		public const int MaxSize = 60000;
		public const int FlagIndex = 0;
		public const int OpcodeIndex = 1;
		public const int MessageIndex = 3;
	}

	public class PacketParser
	{
		private readonly CircularBuffer buffer;
		private ushort packetSize;
		private ParserState state;
		public MemoryStream memoryStream;
		private bool isOK;

		public PacketParser(CircularBuffer buffer, MemoryStream memoryStream)
		{
			this.buffer = buffer;
			this.memoryStream = memoryStream;
		}

		public bool Parse()
		{
			if (this.isOK)
			{
				return true;
			}

			bool finish = false;
			while (!finish)
			{
				switch (this.state)
				{
					case ParserState.PacketSize:
						if (this.buffer.Length < 2)
						{
							finish = true;
						}
						else
						{
							this.buffer.Read(this.memoryStream.GetBuffer(), 0, 2);
							packetSize = BitConverter.ToUInt16(this.memoryStream.GetBuffer(), 0);
							if (packetSize < Packet.MinSize || packetSize > Packet.MaxSize)
							{
								throw new Exception($"packet size error: {this.packetSize}");
							}
							this.state = ParserState.PacketBody;
						}
						break;
					case ParserState.PacketBody:
						if (this.buffer.Length < this.packetSize)
						{
							finish = true;
						}
						else
						{
							this.memoryStream.Seek(0, SeekOrigin.Begin);
							this.memoryStream.SetLength(this.packetSize);
							byte[] bytes = this.memoryStream.GetBuffer();
							this.buffer.Read(bytes, 0, this.packetSize);
							this.isOK = true;
							this.state = ParserState.PacketSize;
							finish = true;
						}
						break;
				}
			}
			return this.isOK;
		}

		public MemoryStream GetPacket()
		{
			this.isOK = false;
			return this.memoryStream;
		}
	}
}