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

	public interface IMarshalInfoProvider : IMetadataTokenProvider {

		bool HasMarshalInfo { get; }
		MarshalInfo MarshalInfo { get; set; }
	}

	static partial class Mixin {

		public static bool GetHasMarshalInfo (
			this IMarshalInfoProvider self,
			ModuleDefinition module)
		{
			return module.HasImage () && module.Read (self, (provider, reader) => reader.HasMarshalInfo (provider));
		}

		public static MarshalInfo GetMarshalInfo (
			this IMarshalInfoProvider self,
			ref MarshalInfo variable,
			ModuleDefinition module)
		{
			return module.HasImage ()
				? module.Read (ref variable, self, (provider, reader) => reader.ReadMarshalInfo (provider))
				: null;
		}
	}
}
