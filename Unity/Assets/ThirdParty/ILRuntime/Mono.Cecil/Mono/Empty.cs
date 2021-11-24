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
using ILRuntime.Mono.Collections.Generic;

namespace ILRuntime.Mono {

	static class Empty<T> {

		public static readonly T [] Array = new T [0];
	}

	class ArgumentNullOrEmptyException : ArgumentException {

		public ArgumentNullOrEmptyException (string paramName)
			: base ("Argument null or empty", paramName)
		{
		}
	}
}

namespace ILRuntime.Mono.Cecil {

	static partial class Mixin {

		public static bool IsNullOrEmpty<T> (this T [] self)
		{
			return self == null || self.Length == 0;
		}

		public static bool IsNullOrEmpty<T> (this Collection<T> self)
		{
			return self == null || self.size == 0;
		}

		public static T [] Resize<T> (this T [] self, int length)
		{
			Array.Resize (ref self, length);
			return self;
		}

		public static T [] Add<T> (this T [] self, T item)
		{
			if (self == null) {
				self = new [] { item };
				return self;
			}

			self = self.Resize (self.Length + 1);
			self [self.Length - 1] = item;
			return self;
		}
	}
}
