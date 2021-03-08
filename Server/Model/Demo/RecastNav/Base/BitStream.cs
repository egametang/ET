﻿using System;

namespace cyh.game
{
	public unsafe class BitStream
	{
		byte[] array;
		ulong length;
		ulong read_position;
		ulong write_position;

		public ulong Length { get { return this.length; } }
		public ulong Count { get { return (this.write_position - this.read_position) >> 3; } }

		public BitStream()
		{
			this.length = 128;
			this.array = new byte[this.length];
		}

		public BitStream(byte[] data)
		{
			this.array = data;
			this.length = (ulong)data.Length;
			this.write_position = this.length << 3;
		}

		void resize()
		{
			Array.Resize<byte>(ref this.array, (int)this.length << 1);
		}

		public void Write(ulong value, int bitCount)
		{
			if (bitCount > 64)
				bitCount = 64;
			if (this.write_position >= this.length - 8)
				this.resize();
			byte offset = (byte)(this.write_position & 7);
			ulong pos = (this.write_position >> 3);
			if (offset != 0)
			{
				this.array[pos] |= (byte)(value << offset);
				offset = (byte)(8 - offset);
				value >>= offset;
				fixed (byte* ptr = &this.array[pos + 1])
				{
					*(ulong*)ptr = value;
				}
			}
			else
			{
				fixed (byte* ptr = &this.array[pos])
				{
					*(ulong*)ptr = value;
				}
			}
			this.write_position += (ulong)bitCount;
		}

		public ulong Read(int bitCount)
		{
			if (bitCount > 64)
				bitCount = 64;
			ulong bitsLeft = this.write_position - this.read_position;
			if (bitsLeft >= (uint)bitCount)
			{
				ulong value;
				byte offset = (byte)(this.read_position & 7);
				ulong pos = (this.read_position >> 3);
				if (offset != 0)
				{
					fixed (byte* ptr = &this.array[pos])
					{
						value = *(ulong*)ptr;
					}
					value = value >> offset;
					this.read_position += (uint)bitCount;
					offset = (byte)(64 - offset);
					if (bitCount > offset)
					{
						pos = (this.read_position >> 3);
						fixed (byte* ptr = &this.array[pos])
						{
							value |= ((ulong)*ptr) << offset;
						}
					}
				}
				else
				{
					fixed (byte* ptr = &this.array[pos])
					{
						value = *(ulong*)ptr;
					}
					this.read_position += (uint)bitCount;
				}
				value &= 0xFFFFFFFFFFFFFFFF >> (64 - bitCount);
				return value;
			}
			return 0;
		}
	}
}
