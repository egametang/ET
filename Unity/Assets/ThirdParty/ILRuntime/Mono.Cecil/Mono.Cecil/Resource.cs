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

	public enum ResourceType {
		Linked,
		Embedded,
		AssemblyLinked,
	}

	public abstract class Resource {

		string name;
		uint attributes;

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public ManifestResourceAttributes Attributes {
			get { return (ManifestResourceAttributes) attributes; }
			set { attributes = (uint) value; }
		}

		public abstract ResourceType ResourceType {
			get;
		}

		#region ManifestResourceAttributes

		public bool IsPublic {
			get { return attributes.GetMaskedAttributes ((uint) ManifestResourceAttributes.VisibilityMask, (uint) ManifestResourceAttributes.Public); }
			set { attributes = attributes.SetMaskedAttributes ((uint) ManifestResourceAttributes.VisibilityMask, (uint) ManifestResourceAttributes.Public, value); }
		}

		public bool IsPrivate {
			get { return attributes.GetMaskedAttributes ((uint) ManifestResourceAttributes.VisibilityMask, (uint) ManifestResourceAttributes.Private); }
			set { attributes = attributes.SetMaskedAttributes ((uint) ManifestResourceAttributes.VisibilityMask, (uint) ManifestResourceAttributes.Private, value); }
		}

		#endregion

		internal Resource (string name, ManifestResourceAttributes attributes)
		{
			this.name = name;
			this.attributes = (uint) attributes;
		}
	}
}
