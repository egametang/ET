using System;

namespace Base
{
	internal enum ParserState
	{
		PacketHeader,
		MessageHeaderSize,
		MessageHeader,
		MessageBodySize,
		MessageBody,
	}

	internal class Packet
	{
		public PacketHeader packetHeader = new PacketHeader();
		public MessageHeader messageHeader = new MessageHeader();
		public MessageBody messageBody = new MessageBody();
	}

	internal class PacketHeader
	{
		public uint Flag;
		public uint Size;
	}

	internal class MessageHeader
	{
		public int Size;
		public byte Format;
		public int Status;
		public int Seq;
		public long Session;
		public byte[] Command = new byte[4];
		public byte[] Module;
	}

	internal class MessageBody
	{
		public int Size;
		public byte[] Content;
	}

	internal class PacketParser
	{
		private readonly TBuffer buffer;

		private readonly byte[] intBuffer = new byte[4];
		private readonly byte[] longBuffer = new byte[8];
		private readonly byte[] byteBuffer = new byte[1];

		private Packet packet = new Packet();

		private ParserState state;
		private bool isOK;

		public PacketParser(TBuffer buffer)
		{
			this.buffer = buffer;
		}

		private void Parse()
		{
			if (this.isOK)
			{
				return;
			}

			bool finish = false;
			while (!finish)
			{
				switch (this.state)
				{
					case ParserState.PacketHeader:
						if (this.buffer.Count < 8)
						{
							finish = true;
						}
						else
						{
							this.buffer.RecvFrom(this.intBuffer);
							this.packet.packetHeader.Flag = BitConverter.ToUInt32(this.intBuffer, 0);
							this.buffer.RecvFrom(this.intBuffer);
							this.packet.packetHeader.Size = BitConverter.ToUInt32(this.intBuffer, 0);
							if (this.packet.packetHeader.Size > 1024 * 1024)
							{
								throw new Exception($"packet too large, size: {this.packet.packetHeader.Size}");
							}
							this.state = ParserState.MessageHeaderSize;
						}
						break;
					case ParserState.MessageHeaderSize:
						if (this.buffer.Count < 4)
						{
							finish = true;
						}
						else
						{
							this.buffer.RecvFrom(this.intBuffer);
							this.packet.messageHeader.Size = BitConverter.ToInt32(this.intBuffer, 0);
							finish = true;
						}
						break;
					case ParserState.MessageHeader:
						if (this.buffer.Count < this.packet.messageHeader.Size)
						{
							finish = true;
						}
						else
						{
							this.buffer.RecvFrom(this.byteBuffer);
							this.packet.messageHeader.Format = this.byteBuffer[0];

							this.buffer.RecvFrom(this.intBuffer);
							this.packet.messageHeader.Status = BitConverter.ToInt32(this.intBuffer, 0);

							this.buffer.RecvFrom(this.intBuffer);
							this.packet.messageHeader.Seq = BitConverter.ToInt32(this.intBuffer, 0);

							this.buffer.RecvFrom(this.longBuffer);
							this.packet.messageHeader.Session = BitConverter.ToInt64(this.longBuffer, 0);

							this.buffer.RecvFrom(this.packet.messageHeader.Command);

							this.packet.messageHeader.Module = new byte[this.packet.messageHeader.Size - 21];
							this.buffer.RecvFrom(this.packet.messageHeader.Module);

							finish = true;
						}
						break;

					case ParserState.MessageBodySize:
						if (this.buffer.Count < 4)
						{
							finish = true;
						}
						else
						{
							this.buffer.RecvFrom(this.intBuffer);
							this.packet.messageBody.Size = BitConverter.ToInt32(this.intBuffer, 0);
							finish = true;
						}
						break;
					case ParserState.MessageBody:
						if (this.buffer.Count < this.packet.messageBody.Size)
						{
							finish = true;
						}
						else
						{
							this.packet.messageBody.Content = new byte[this.packet.messageBody.Size];
							this.buffer.RecvFrom(this.packet.messageBody.Content);
							finish = true;
						}
						break;
				}
			}
		}

		public byte[] GetPacket()
		{
			this.Parse();
			if (!this.isOK)
			{
				return null;
			}
			byte[] result = new byte[4 + this.packet.messageBody.Content.Length];
			Array.Copy(result, this.packet.messageHeader.Command, 4);
			Array.Copy(result, 4, this.packet.messageBody.Content, 0, this.packet.messageBody.Content.Length);
			this.isOK = false;
			return result;
		}
	}
}