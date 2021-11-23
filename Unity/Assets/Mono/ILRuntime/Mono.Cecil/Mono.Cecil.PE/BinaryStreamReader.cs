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
using System.IO;

namespace ILRuntime.Mono.Cecil.PE {

	class BinaryStreamReader : BinaryReader {

		public int Position {
			get { return (int) BaseStream.Position; }
			set { BaseStream.Position = value; }
		}

		public int Length {
			get { return (int) BaseStream.Length; }
		}

		public BinaryStreamReader (Stream stream)
			: base (stream)
		{
		}

		public void Advance (int bytes)
		{
			BaseStream.Seek (bytes, SeekOrigin.Current);
		}

		public void MoveTo (uint position)
		{
			BaseStream.Seek (position, SeekOrigin.Begin);
		}

		public void Align (int align)
		{
			align--;
			var position = Position;
			Advance (((position + align) & ~align) - position);
		}

		public DataDirectory ReadDataDirectory ()
		{
			return new DataDirectory (ReadUInt32 (), ReadUInt32 ());
		}
	}
}
