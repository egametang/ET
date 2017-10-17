//
// IConstantProvider.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2011 Jb Evain
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace Mono.Cecil {

	public interface IConstantProvider : IMetadataTokenProvider {

		bool HasConstant { get; set; }
		object Constant { get; set; }
	}

	static partial class Mixin {

		internal static object NoValue = new object ();
		internal static object NotResolved = new object ();

		public static void ResolveConstant (
			IConstantProvider self,
			ref object constant,
			ModuleDefinition module)
		{
			lock (module.SyncRoot) {
				if (constant != Mixin.NotResolved)
					return;
				if (Mixin.HasImage (module))
					constant = module.Read (self, (provider, reader) => reader.ReadConstant (provider));
				else
					constant = Mixin.NoValue;
			}
		}
	}
}
