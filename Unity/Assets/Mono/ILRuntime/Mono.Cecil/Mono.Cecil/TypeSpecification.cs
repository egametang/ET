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

using ILRuntime.Mono.Cecil.Metadata;

namespace ILRuntime.Mono.Cecil {

	public abstract class TypeSpecification : TypeReference {

		readonly TypeReference element_type;

		public TypeReference ElementType {
			get { return element_type; }
		}

		public override string Name {
			get { return element_type.Name; }
			set { throw new InvalidOperationException (); }
		}

		public override string Namespace {
			get { return element_type.Namespace; }
			set { throw new InvalidOperationException (); }
		}

		public override IMetadataScope Scope {
			get { return element_type.Scope; }
			set { throw new InvalidOperationException (); }
		}

		public override ModuleDefinition Module {
			get { return element_type.Module; }
		}

		public override string FullName {
			get { return element_type.FullName; }
		}

		public override bool ContainsGenericParameter {
			get { return element_type.ContainsGenericParameter; }
		}

		public override MetadataType MetadataType {
			get { return (MetadataType) etype; }
		}

		internal TypeSpecification (TypeReference type)
			: base (null, null)
		{
			this.element_type = type;
			this.token = new MetadataToken (TokenType.TypeSpec);
		}

		public override TypeReference GetElementType ()
		{
			return element_type.GetElementType ();
		}
	}
}
