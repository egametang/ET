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
using System.Text;

using Mono.Collections.Generic;

using MD = Mono.Cecil.Metadata;

namespace Mono.Cecil {

	public sealed class GenericInstanceType : TypeSpecification, IGenericInstance, IGenericContext {

		Collection<TypeReference> arguments;

		public bool HasGenericArguments {
			get { return !arguments.IsNullOrEmpty (); }
		}

		public Collection<TypeReference> GenericArguments {
			get { return arguments ?? (arguments = new Collection<TypeReference> ()); }
		}

		public override TypeReference DeclaringType {
			get { return ElementType.DeclaringType; }
			set { throw new NotSupportedException (); }
		}

		public override string FullName {
			get {
				var name = new StringBuilder ();
				name.Append (base.FullName);
				this.GenericInstanceFullName (name);
				return name.ToString ();
			}
		}

		public override bool IsGenericInstance {
			get { return true; }
		}

		public override bool ContainsGenericParameter {
			get { return this.ContainsGenericParameter () || base.ContainsGenericParameter; }
		}

		IGenericParameterProvider IGenericContext.Type {
			get { return ElementType; }
		}

		public GenericInstanceType (TypeReference type)
			: base (type)
		{
			base.IsValueType = type.IsValueType;
			this.etype = MD.ElementType.GenericInst;
		}
	}
}
