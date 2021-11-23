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

	public enum NativeType {
		None = 0x66,

		Boolean = 0x02,
		I1 = 0x03,
		U1 = 0x04,
		I2 = 0x05,
		U2 = 0x06,
		I4 = 0x07,
		U4 = 0x08,
		I8 = 0x09,
		U8 = 0x0a,
		R4 = 0x0b,
		R8 = 0x0c,
		LPStr = 0x14,
		Int = 0x1f,
		UInt = 0x20,
		Func = 0x26,
		Array = 0x2a,

		// Msft specific
		Currency = 0x0f,
		BStr = 0x13,
		LPWStr = 0x15,
		LPTStr = 0x16,
		FixedSysString = 0x17,
		IUnknown = 0x19,
		IDispatch = 0x1a,
		Struct = 0x1b,
		IntF = 0x1c,
		SafeArray = 0x1d,
		FixedArray = 0x1e,
		ByValStr = 0x22,
		ANSIBStr = 0x23,
		TBStr = 0x24,
		VariantBool = 0x25,
		ASAny = 0x28,
		LPStruct = 0x2b,
		CustomMarshaler = 0x2c,
		Error = 0x2d,
		Max = 0x50
	}
}
