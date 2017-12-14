using System;

namespace Model
{
	internal enum ParserState
	{
		PacketSize,
		PacketBody
	}

	public struct Packet
	{
		public byte[] Bytes { get; set; }
		public int Length { get; set; }

		public Packet(int length)
		{
			this.Length = 0;
			this.Bytes = new byte[length];
		}

		public Packet(byte[] bytes)
		{
			this.Bytes = bytes;
			this.Length = bytes.Length;
		}
	}

	internal class PacketParser
	{
		private readonly CircularBuffer buffer;

		private ushort packetSize;
		private ParserState state;
		private Packet packet = new Packet(8 * 1024);
		private bool isOK;

		public PacketParser(CircularBuffer buffer)
		{
			this.buffer = buffer;
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
						if (this.buffer.Count < 2)
						{
							finish = true;
						}
						else
						{
							this.buffer.RecvFrom(this.packet.Bytes, 2);
							this.packetSize = BitConverter.ToUInt16(this.packet.Bytes, 0);
							if (packetSize > 60000)
							{
								throw new Exception($"packet too large, size: {this.packetSize}");
							}
							this.state = ParserState.PacketBody;
						}
						break;
					case ParserState.PacketBody:
						if (this.buffer.Count < this.packetSize)
						{
							finish = true;
						}
						else
						{
							this.buffer.RecvFrom(this.packet.Bytes, this.packetSize);
							this.packet.Length = this.packetSize;
							this.isOK = true;
							this.state = ParserState.PacketSize;
							finish = true;
						}
						break;
				}
			}
			return this.isOK;
		}

		public Packet GetPacket()
		{
			this.isOK = false;
			return this.packet;
		}
	}
}