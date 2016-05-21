//
// PInvokeInfo.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2011 Jb Evain
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace Mono.Cecil {

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
			get { return Mixin.GetAttributes(attributes,(ushort) PInvokeAttributes.NoMangle); }
			set { attributes =Mixin.SetAttributes(attributes,(ushort) PInvokeAttributes.NoMangle, value); }
		}

		public bool IsCharSetNotSpec {
			get { return Mixin.GetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CharSetMask, (ushort) PInvokeAttributes.CharSetNotSpec); }
			set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CharSetMask, (ushort) PInvokeAttributes.CharSetNotSpec, value); }
		}

		public bool IsCharSetAnsi {
			get { return Mixin.GetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CharSetMask, (ushort) PInvokeAttributes.CharSetAnsi); }
			set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CharSetMask, (ushort) PInvokeAttributes.CharSetAnsi, value); }
		}

		public bool IsCharSetUnicode {
			get { return Mixin.GetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CharSetMask, (ushort) PInvokeAttributes.CharSetUnicode); }
			set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CharSetMask, (ushort) PInvokeAttributes.CharSetUnicode, value); }
		}

		public bool IsCharSetAuto {
			get { return Mixin.GetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CharSetMask, (ushort) PInvokeAttributes.CharSetAuto); }
			set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CharSetMask, (ushort) PInvokeAttributes.CharSetAuto, value); }
		}

		public bool SupportsLastError {
			get { return Mixin.GetAttributes(attributes,(ushort) PInvokeAttributes.SupportsLastError); }
			set { attributes =Mixin.SetAttributes(attributes,(ushort) PInvokeAttributes.SupportsLastError, value); }
		}

		public bool IsCallConvWinapi {
			get { return Mixin.GetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvWinapi); }
			set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvWinapi, value); }
		}

		public bool IsCallConvCdecl {
			get { return Mixin.GetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvCdecl); }
			set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvCdecl, value); }
		}

		public bool IsCallConvStdCall {
			get { return Mixin.GetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvStdCall); }
			set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvStdCall, value); }
		}

		public bool IsCallConvThiscall {
			get { return Mixin.GetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvThiscall); }
			set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvThiscall, value); }
		}

		public bool IsCallConvFastcall {
			get { return Mixin.GetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvFastcall); }
			set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort) PInvokeAttributes.CallConvMask, (ushort) PInvokeAttributes.CallConvFastcall, value); }
		}

		public bool IsBestFitEnabled {
			get { return Mixin.GetMaskedAttributes(attributes,(ushort) PInvokeAttributes.BestFitMask, (ushort) PInvokeAttributes.BestFitEnabled); }
			set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort) PInvokeAttributes.BestFitMask, (ushort) PInvokeAttributes.BestFitEnabled, value); }
		}

		public bool IsBestFitDisabled {
			get { return Mixin.GetMaskedAttributes(attributes,(ushort) PInvokeAttributes.BestFitMask, (ushort) PInvokeAttributes.BestFitDisabled); }
			set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort) PInvokeAttributes.BestFitMask, (ushort) PInvokeAttributes.BestFitDisabled, value); }
		}

		public bool IsThrowOnUnmappableCharEnabled {
			get { return Mixin.GetMaskedAttributes(attributes,(ushort) PInvokeAttributes.ThrowOnUnmappableCharMask, (ushort) PInvokeAttributes.ThrowOnUnmappableCharEnabled); }
			set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort) PInvokeAttributes.ThrowOnUnmappableCharMask, (ushort) PInvokeAttributes.ThrowOnUnmappableCharEnabled, value); }
		}

		public bool IsThrowOnUnmappableCharDisabled {
			get { return Mixin.GetMaskedAttributes(attributes,(ushort) PInvokeAttributes.ThrowOnUnmappableCharMask, (ushort) PInvokeAttributes.ThrowOnUnmappableCharDisabled); }
			set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort) PInvokeAttributes.ThrowOnUnmappableCharMask, (ushort) PInvokeAttributes.ThrowOnUnmappableCharDisabled, value); }
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
