using System;
using System.IO;

namespace ETModel
{
	public enum ParserState
	{
		PacketSize,
		PacketBody
	}

	public class Packet
	{
		public const int MinSize = 3;
		public const int MaxSize = 60000;
		public const int FlagIndex = 0;
		public const int OpcodeIndex = 1;
		public const int Index = 3;

		/// <summary>
		/// 只读，不允许修改
		/// </summary>
		public byte[] Bytes
		{
			get
			{
				return this.Stream.GetBuffer();
			}
		}
		
		public byte Flag { get; set; }
		public ushort Opcode { get; set; }
		public MemoryStream Stream { get; }

		public Packet(int length)
		{
			this.Stream = new MemoryStream(length);
		}

		public Packet(byte[] bytes)
		{
			this.Stream = new MemoryStream(bytes);
		}
	}

	public class PacketParser
	{
		private readonly CircularBuffer buffer;
		private ushort packetSize;
		private ParserState state;
		public readonly Packet packet = new Packet(ushort.MaxValue);
		private readonly byte[] cache = new byte[2];
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
						if (this.buffer.Length < 2)
						{
							finish = true;
						}
						else
						{
							this.buffer.Read(this.packet.Bytes, 0, 2);
							packetSize = BitConverter.ToUInt16(this.packet.Bytes, 0);
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
							this.buffer.Read(this.cache, 0, 1);
							this.packet.Flag = this.cache[0];
							this.buffer.Read(this.cache, 0, 2);
							this.packet.Opcode = BitConverter.ToUInt16(this.cache, 0);
							
							this.packet.Stream.Seek(0, SeekOrigin.Begin);
							this.packet.Stream.SetLength(this.packetSize - Packet.Index);
							this.buffer.Read(this.packet.Stream.GetBuffer(), 0, this.packetSize - Packet.Index);
							
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