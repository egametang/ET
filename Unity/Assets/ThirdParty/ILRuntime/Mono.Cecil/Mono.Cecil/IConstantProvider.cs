//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

namespace ILRuntime.Mono.Cecil {

	public interface IConstantProvider : IMetadataTokenProvider {

		bool HasConstant { get; set; }
		object Constant { get; set; }
	}

	static partial class Mixin {

		internal static object NoValue = new object ();
		internal static object NotResolved = new object ();

		public static void ResolveConstant (
			this IConstantProvider self,
			ref object constant,
			ModuleDefinition module)
		{
			if (module == null) {
				constant = Mixin.NoValue;
				return;
			}

			lock (module.SyncRoot) {
				if (constant != Mixin.NotResolved)
					return;
				if (module.HasImage ())
					constant = module.Read (self, (provider, reader) => reader.ReadConstant (provider));
				else
					constant = Mixin.NoValue;
			}
		}
	}
}
