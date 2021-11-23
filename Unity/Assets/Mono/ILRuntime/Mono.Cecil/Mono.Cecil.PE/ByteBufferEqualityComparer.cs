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
using System.Collections.Generic;

namespace ILRuntime.Mono.Cecil.PE {

	sealed class ByteBufferEqualityComparer : IEqualityComparer<ByteBuffer> {

		public bool Equals (ByteBuffer x, ByteBuffer y)
		{
			if (x.length != y.length)
				return false;

			var x_buffer = x.buffer;
			var y_buffer = y.buffer;

			for (int i = 0; i < x.length; i++)
				if (x_buffer [i] != y_buffer [i])
					return false;

			return true;
		}

		public int GetHashCode (ByteBuffer buffer)
		{
			// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
			const int fnv_offset_bias = unchecked((int)2166136261);
			const int fnv_prime = 16777619;

			var hash_code = fnv_offset_bias;
			var bytes = buffer.buffer;

			for (int i = 0; i < buffer.length; i++)
				hash_code = unchecked ((hash_code ^ bytes [i]) * fnv_prime);

			return hash_code;
 		}
	}
}
