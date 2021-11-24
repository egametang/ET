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

	public sealed class LinkedResource : Resource {

		internal byte [] hash;
		string file;

		public byte [] Hash {
			get { return hash; }
		}

		public string File {
			get { return file; }
			set { file = value; }
		}

		public override ResourceType ResourceType {
			get { return ResourceType.Linked; }
		}

		public LinkedResource (string name, ManifestResourceAttributes flags)
			: base (name, flags)
		{
		}

		public LinkedResource (string name, ManifestResourceAttributes flags, string file)
			: base (name, flags)
		{
			this.file = file;
		}
	}
}
