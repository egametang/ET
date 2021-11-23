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

	public enum MetadataScopeType {
		AssemblyNameReference,
		ModuleReference,
		ModuleDefinition,
	}

	public interface IMetadataScope : IMetadataTokenProvider {
		MetadataScopeType MetadataScopeType { get; }
		string Name { get; set; }
	}
}
