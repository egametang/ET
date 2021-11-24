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

	public class ModuleReference : IMetadataScope {

		string name;

		internal MetadataToken token;

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public virtual MetadataScopeType MetadataScopeType {
			get { return MetadataScopeType.ModuleReference; }
		}

		public MetadataToken MetadataToken {
			get { return token; }
			set { token = value; }
		}

		internal ModuleReference ()
		{
			this.token = new MetadataToken (TokenType.ModuleRef);
		}

		public ModuleReference (string name)
			: this ()
		{
			this.name = name;
		}

		public override string ToString ()
		{
			return name;
		}
	}
}
