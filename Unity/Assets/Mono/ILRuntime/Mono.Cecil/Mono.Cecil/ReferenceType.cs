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

using MD = ILRuntime.Mono.Cecil.Metadata;

namespace ILRuntime.Mono.Cecil {

	public sealed class ByReferenceType : TypeSpecification {

		public override string Name {
			get { return base.Name + "&"; }
		}

		public override string FullName {
			get { return base.FullName + "&"; }
		}

		public override bool IsValueType {
			get { return false; }
			set { throw new InvalidOperationException (); }
		}

		public override bool IsByReference {
			get { return true; }
		}

		public ByReferenceType (TypeReference type)
			: base (type)
		{
			Mixin.CheckType (type);
			this.etype = MD.ElementType.ByRef;
		}
	}
}
