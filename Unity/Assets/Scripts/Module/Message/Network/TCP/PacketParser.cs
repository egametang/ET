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

	public class Packet: IDisposable
	{
		public const int SizeLength = 2;
		public const int MinSize = 3;
		public const int MaxSize = 60000;
		public const int FlagIndex = 0;
		public const int OpcodeIndex = 1;
		public const int MessageIndex = 3;
		
		public static RecyclableMemoryStreamManager memoryStreamManager = new RecyclableMemoryStreamManager();

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
		
		public MemoryStream Stream { get; private set; }

		public Packet(int length)
		{
			this.Stream = memoryStreamManager.GetStream("Packet", length);
		}

		public void Dispose()
		{
			if (this.Stream == null)
			{
				return;
			}
			
			this.Stream.Close();
			this.Stream = null;
		}
	}

	public class PacketParser: IDisposable
	{
		private readonly CircularBuffer buffer;
		private ushort packetSize;
		private ParserState state;
		public Packet packet = new Packet(ushort.MaxValue);
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
							this.packet.Stream.Seek(0, SeekOrigin.Begin);
							this.packet.Stream.SetLength(this.packetSize);
							byte[] bytes = this.packet.Stream.GetBuffer();
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

		public Packet GetPacket()
		{
			this.isOK = false;
			return this.packet;
		}

		public void Dispose()
		{
			if (this.packet == null)
			{
				return;
			}
			this.packet?.Dispose();
			this.packet = null;
		}
	}
}