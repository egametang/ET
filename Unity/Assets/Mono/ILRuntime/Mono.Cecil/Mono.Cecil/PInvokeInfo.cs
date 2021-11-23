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

	public sealed class PInvokeInfo {

		ushort attributes;
		string entry_point;
		ModuleReference module;

		public PInvokeAttributes Attributes {
			get { return (PInvokeAttributes) attributes; }
			set { attributes = (ushort) value; }
		}

		public string EntryPoint {
			get { return entry_point; }
			set { entry_point = value; }
		}

		public ModuleReference Module {
			get { return module; }
			set { module = value; }
		}

		#region PInvokeAttributes

		public bool IsNoMangle {
			get { return attributes.GetAttributes ((ushort) PInvokeAttributes.NoMangle); }
			set { attributes = attributes.SetAttributes ((ushort) PInvokeAttributes.NoMangle, value); }
		}

		public bool IsCharSetNotSpec {
			get { return attributes.GetMaskedAttributes((ushort) PInvokeAttributes.CharSetMask, (ushort) PInvokeAttributes.CharSetNotSpec); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) PInvokeAttributes.CharSetMask, (ushort) PInvokeAttributes.CharSetNotSpec, value); }
		}

		public bool IsCharSetAnsi {
			get { return attributes.GetMaskedAttributes ((ushort) PInvokeAttributes.CharSetMask, (ushort) PInvokeAttributes.CharSetAnsi); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) PInvokeAttributes.CharSetMask, (ushort) PInvokeAttributes.CharSetAnsi, value); }
		}

		public bool IsCharSetUnicode {
			get { return attributes.GetMaskedAttributes ((ushort) PInvokeAttributes.CharSetMask, (ushort) PInvokeAttributes.CharSetUnicode); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) PInvokeAttributes.CharSetMask, (ushort) PInvokeAttributes.CharSetUnicode, value); }
		}

		public bool IsCharSetAuto {
			get { return attributes.GetMaskedAttributes ((ushort) PInvokeAttributes.CharSetMask, (ushort) PInvokeAttributes.CharSetAuto); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) PInvokeAttributes.CharSetMask, (ushort) PInvokeAttributes.CharSetAuto, value); }
		}

		public bool SupportsLastError {
			get { return attributes.GetAttributes ((ushort) PInvokeAttributes.SupportsLastError); }
			set { attributes = attributes.SetAttributes ((ushort) PInvokeAttributes.SupportsLastError, value); }
		}

		public bool IsCallConvWinapi {
			get { return attributes.GetMaskedAttributes((ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvWinapi); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvWinapi, value); }
		}

		public bool IsCallConvCdecl {
			get { return attributes.GetMaskedAttributes ((ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvCdecl); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvCdecl, value); }
		}

		public bool IsCallConvStdCall {
			get { return attributes.GetMaskedAttributes ((ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvStdCall); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvStdCall, value); }
		}

		public bool IsCallConvThiscall {
			get { return attributes.GetMaskedAttributes ((ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvThiscall); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvThiscall, value); }
		}

		public bool IsCallConvFastcall {
			get { return attributes.GetMaskedAttributes ((ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvFastcall); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvFastcall, value); }
		}

		public bool IsBestFitEnabled {
			get { return attributes.GetMaskedAttributes ((ushort) PInvokeAttributes.BestFitMask, (ushort) PInvokeAttributes.BestFitEnabled); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) PInvokeAttributes.BestFitMask, (ushort) PInvokeAttributes.BestFitEnabled, value); }
		}

		public bool IsBestFitDisabled {
			get { return attributes.GetMaskedAttributes ((ushort) PInvokeAttributes.BestFitMask, (ushort) PInvokeAttributes.BestFitDisabled); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) PInvokeAttributes.BestFitMask, (ushort) PInvokeAttributes.BestFitDisabled, value); }
		}

		public bool IsThrowOnUnmappableCharEnabled {
			get { return attributes.GetMaskedAttributes ((ushort) PInvokeAttributes.ThrowOnUnmappableCharMask, (ushort) PInvokeAttributes.ThrowOnUnmappableCharEnabled); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) PInvokeAttributes.ThrowOnUnmappableCharMask, (ushort) PInvokeAttributes.ThrowOnUnmappableCharEnabled, value); }
		}

		public bool IsThrowOnUnmappableCharDisabled {
			get { return attributes.GetMaskedAttributes ((ushort) PInvokeAttributes.ThrowOnUnmappableCharMask, (ushort) PInvokeAttributes.ThrowOnUnmappableCharDisabled); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) PInvokeAttributes.ThrowOnUnmappableCharMask, (ushort) PInvokeAttributes.ThrowOnUnmappableCharDisabled, value); }
		}

		#endregion

		public PInvokeInfo (PInvokeAttributes attributes, string entryPoint, ModuleReference module)
		{
			this.attributes = (ushort) attributes;
			this.entry_point = entryPoint;
			this.module = module;
		}
	}
}
