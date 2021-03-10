﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace cyh.game
{
	public class BitArray : IList<bool>
	{
		private byte[] array;
		private ulong count;
		public int Count { get { return (int)this.count; } }
		bool ICollection<bool>.IsReadOnly => false;

		public BitArray()
		{
			this.array = Array.Empty<byte>();
			this.count = 0;
		}
		public BitArray(int capacity)
		{
			this.array = new byte[capacity];
			this.count = 0;
		}
		private void resize(int minsize)
		{
			if (minsize == 0)
			{
				if (this.array.Length == 0)
					this.array = new byte[4];
				else
					Array.Resize<byte>(ref this.array, this.array.Length << 1);
			}
			else if (minsize > this.array.Length)
			{
				Array.Resize<byte>(ref this.array, minsize);
			}
		}

		public bool this[int index]
		{
			get
			{
				if (index < 0)
					throw new ArgumentOutOfRangeException();
				int offset = index & 7;
				index >>= 3;
				return (this.array[index] & (1 << offset)) != 0;
			}
			set
			{
				int offset = index & 7;
				if ((ulong)index >= this.count)
				{
					this.count = (ulong)index + 1;
					index >>= 3;
					if (index >= this.array.Length)
						this.resize(index + 1);
				}
				else
					index >>= 3;
				if (value)
					this.array[index] |= (byte)(1 << offset);
				else
					this.array[index] &= (byte)~(1 << offset);
			}
		}

		public bool this[ulong index]
		{
			get
			{
				int offset = (int)(index & 7);
				index >>= 3;
				return (this.array[index] & (1 << offset)) != 0;
			}
			set
			{
				int offset = (int)(index & 7);
				if ((ulong)index >= this.count)
				{
					this.count = (ulong)index + 1;
					index >>= 3;
					if (index >= (ulong)this.array.Length)
						this.resize((int)index + 1);
				}
				else
					index >>= 3;
				if (value)
					this.array[index] |= (byte)(1 << offset);
				else
					this.array[index] &= (byte)~(1 << offset);
			}
		}

		public void Add(bool bit)
		{
			this.count++;
			byte offset = (byte)(this.count & 7);
			int index = (int)(this.count >> 3);
			if (index >= this.array.Length)
				this.resize(0);
			if (bit)
				this.array[index] |= (byte)(1 << offset);
			else
				this.array[index] &= (byte)~(1 << offset);
		}

		public override string ToString()
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder((int)this.count);
			for (ulong i = 0; i < this.count; i++)
			{
				stringBuilder.Append(this[i] ? '1' : '0');
			}
			return stringBuilder.ToString();
		}

		int IList<bool>.IndexOf(bool item)
		{
			throw new NotSupportedException();
		}

		void IList<bool>.Insert(int index, bool item)
		{
			throw new NotSupportedException();
		}

		void IList<bool>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		void ICollection<bool>.Clear()
		{
			this.array = Array.Empty<byte>();
			this.count = 0;
		}

		bool ICollection<bool>.Contains(bool item)
		{
			throw new NotSupportedException();
		}

		void ICollection<bool>.CopyTo(bool[] array, int arrayIndex)
		{
			for (ulong i = 0; i < this.count; i++)
			{
				array[arrayIndex++] = this[i];
			}
		}

		bool ICollection<bool>.Remove(bool item)
		{
			throw new NotSupportedException();
		}

		public IEnumerator<bool> GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		class Enumerator : IEnumerator<bool>
		{
			private BitArray Array;
			private long index = -1;
			public Enumerator(BitArray array)
			{
				this.Array = array;
			}
			public bool Current { get { return this.Array[(ulong)this.index]; } }

			object IEnumerator.Current { get { return this.Array[(ulong)this.index]; } }

			public void Dispose()
			{
				this.index = -1;
			}

			public bool MoveNext()
			{
				this.index++;
				return this.index < (long)this.Array.count;
			}

			public void Reset()
			{
				this.index = -1;
			}
		}


		public static void Set(byte[] data, int index, bool value)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException();
			int offset = index & 7;
			index >>= 3;
			if (index >= data.Length)
				throw new ArgumentOutOfRangeException();
			if (value)
				data[index] |= (byte)(1 << offset);
			else
				data[index] &= (byte)~(1 << offset);
		}

		public static bool Get(byte[] data, int index)
		{
			int offset = index & 7;
			index >>= 3;
			if (index >= data.Length)
				throw new ArgumentOutOfRangeException();
			return (data[index] & (1 << offset)) != 0;
		}
	}
}
