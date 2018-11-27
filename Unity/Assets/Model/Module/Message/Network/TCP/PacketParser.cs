using System;
using System.IO;

namespace ETModel
{
	public enum ParserState
	{
		PacketSize,
		PacketBody
	}

	public static class Packet
	{
		public static int SizeLength = 4;
		public const int MinSize = 3;
		public const int MaxSize = int.MaxValue;
		public const int FlagIndex = 0;
		public const int OpcodeIndex = 1;
		public const int MessageIndex = 3;
	}

	public class PacketParser
	{
		private readonly CircularBuffer buffer;
		private int packetSize;
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
						if (this.buffer.Length < Packet.SizeLength)
						{
							finish = true;
						}
						else
						{
							this.buffer.Read(this.memoryStream.GetBuffer(), 0, Packet.SizeLength);
							
							switch (Packet.SizeLength)
							{
								case 4:
									this.packetSize = BitConverter.ToInt32(this.memoryStream.GetBuffer(), 0);
									break;
								case 2:
									this.packetSize = BitConverter.ToUInt16(this.memoryStream.GetBuffer(), 0);
									break;
								default:
									throw new Exception("packet size must be 2 or 4!");
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