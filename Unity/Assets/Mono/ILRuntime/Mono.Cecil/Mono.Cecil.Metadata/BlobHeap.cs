//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using System;

namespace ILRuntime.Mono.Cecil.Metadata {

	sealed class BlobHeap : Heap {

		public BlobHeap (byte [] data)
			: base (data)
		{
		}

		public byte [] Read (uint index)
		{
			if (index == 0 || index > this.data.Length - 1)
				return Empty<byte>.Array;

			int position = (int) index;
			int length = (int) data.ReadCompressedUInt32 (ref position);

			if (length > data.Length - position)
				return Empty<byte>.Array;

			var buffer = new byte [length];

			Buffer.BlockCopy (data, position, buffer, 0, length);

			return buffer;
		}

		public void GetView (uint signature, out byte [] buffer, out int index, out int length)
		{
			if (signature == 0 || signature > data.Length - 1) {
				buffer = null;
				index = length = 0;
				return;
			}

			buffer = data;

			index = (int) signature;
			length = (int) buffer.ReadCompressedUInt32 (ref index);
		}
	}
}
