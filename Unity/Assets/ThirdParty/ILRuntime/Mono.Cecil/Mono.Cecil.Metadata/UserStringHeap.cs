//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

namespace ILRuntime.Mono.Cecil.Metadata {

	sealed class UserStringHeap : StringHeap {

		public UserStringHeap (byte [] data)
			: base (data)
		{
		}

		protected override string ReadStringAt (uint index)
		{
			int start = (int) index;

			uint length = (uint) (data.ReadCompressedUInt32 (ref start) & ~1);
			if (length < 1)
				return string.Empty;

			var chars = new char [length / 2];

			for (int i = start, j = 0; i < start + length; i += 2)
				chars [j++] = (char) (data [i] | (data [i + 1] << 8));

			return new string (chars);
		}
	}
}
