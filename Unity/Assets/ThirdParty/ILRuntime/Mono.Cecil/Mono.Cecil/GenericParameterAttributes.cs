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

namespace ILRuntime.Mono.Cecil {

	[Flags]
	public enum GenericParameterAttributes : ushort {
		VarianceMask	= 0x0003,
		NonVariant		= 0x0000,
		Covariant		= 0x0001,
		Contravariant	= 0x0002,

		SpecialConstraintMask			= 0x001c,
		ReferenceTypeConstraint			= 0x0004,
		NotNullableValueTypeConstraint	= 0x0008,
		DefaultConstructorConstraint	= 0x0010
	}
}
