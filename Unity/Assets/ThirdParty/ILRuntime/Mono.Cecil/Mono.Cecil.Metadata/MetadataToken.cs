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

namespace ILRuntime.Mono.Cecil {

	public struct MetadataToken : IEquatable<MetadataToken> {

		readonly uint token;

		public uint RID	{
			get { return token & 0x00ffffff; }
		}

		public TokenType TokenType {
			get { return (TokenType) (token & 0xff000000); }
		}

		public static readonly MetadataToken Zero = new MetadataToken ((uint) 0);

		public MetadataToken (uint token)
		{
			this.token = token;
		}

		public MetadataToken (TokenType type)
			: this (type, 0)
		{
		}

		public MetadataToken (TokenType type, uint rid)
		{
			token = (uint) type | rid;
		}

		public MetadataToken (TokenType type, int rid)
		{
			token = (uint) type | (uint) rid;
		}

		public int ToInt32 ()
		{
			return (int) token;
		}

		public uint ToUInt32 ()
		{
			return token;
		}

		public override int GetHashCode ()
		{
			return (int) token;
		}

		public bool Equals (MetadataToken other)
		{
			return other.token == token;
		}

		public override bool Equals (object obj)
		{
			if (obj is MetadataToken) {
				var other = (MetadataToken) obj;
				return other.token == token;
			}

			return false;
		}

		public static bool operator == (MetadataToken one, MetadataToken other)
		{
			return one.token == other.token;
		}

		public static bool operator != (MetadataToken one, MetadataToken other)
		{
			return one.token != other.token;
		}

		public override string ToString ()
		{
			return string.Format ("[{0}:0x{1}]", TokenType, RID.ToString ("x4"));
		}
	}
}
