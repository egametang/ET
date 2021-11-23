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
	public enum PInvokeAttributes : ushort {
		NoMangle			= 0x0001,	// PInvoke is to use the member name as specified

		// Character set
		CharSetMask			= 0x0006,
		CharSetNotSpec		= 0x0000,
		CharSetAnsi			= 0x0002,
		CharSetUnicode		= 0x0004,
		CharSetAuto			= 0x0006,

		SupportsLastError	= 0x0040,	// Information about target function. Not relevant for fields

		// Calling convetion
		CallConvMask		= 0x0700,
		CallConvWinapi		= 0x0100,
		CallConvCdecl		= 0x0200,
		CallConvStdCall		= 0x0300,
		CallConvThiscall	= 0x0400,
		CallConvFastcall	= 0x0500,

		BestFitMask			= 0x0030,
		BestFitEnabled		= 0x0010,
		BestFitDisabled		= 0x0020,

		ThrowOnUnmappableCharMask = 0x3000,
		ThrowOnUnmappableCharEnabled = 0x1000,
		ThrowOnUnmappableCharDisabled = 0x2000,
	}
}
