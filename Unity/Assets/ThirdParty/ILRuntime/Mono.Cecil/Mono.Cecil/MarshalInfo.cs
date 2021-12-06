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

	public class MarshalInfo {

		internal NativeType native;

		public NativeType NativeType {
			get { return native; }
			set { native = value; }
		}

		public MarshalInfo (NativeType native)
		{
			this.native = native;
		}
	}

	public sealed class ArrayMarshalInfo : MarshalInfo {

		internal NativeType element_type;
		internal int size_parameter_index;
		internal int size;
		internal int size_parameter_multiplier;

		public NativeType ElementType {
			get { return element_type; }
			set { element_type = value; }
		}

		public int SizeParameterIndex {
			get { return size_parameter_index; }
			set { size_parameter_index = value; }
		}

		public int Size {
			get { return size; }
			set { size = value; }
		}

		public int SizeParameterMultiplier {
			get { return size_parameter_multiplier; }
			set { size_parameter_multiplier = value; }
		}

		public ArrayMarshalInfo ()
			: base (NativeType.Array)
		{
			element_type = NativeType.None;
			size_parameter_index = -1;
			size = -1;
			size_parameter_multiplier = -1;
		}
	}

	public sealed class CustomMarshalInfo : MarshalInfo {

		internal Guid guid;
		internal string unmanaged_type;
		internal TypeReference managed_type;
		internal string cookie;

		public Guid Guid {
			get { return guid; }
			set { guid = value; }
		}

		public string UnmanagedType {
			get { return unmanaged_type; }
			set { unmanaged_type = value; }
		}

		public TypeReference ManagedType {
			get { return managed_type; }
			set { managed_type = value; }
		}

		public string Cookie {
			get { return cookie; }
			set { cookie = value; }
		}

		public CustomMarshalInfo ()
			: base (NativeType.CustomMarshaler)
		{
		}
	}

	public sealed class SafeArrayMarshalInfo : MarshalInfo {

		internal VariantType element_type;

		public VariantType ElementType {
			get { return element_type; }
			set { element_type = value; }
		}

		public SafeArrayMarshalInfo ()
			: base (NativeType.SafeArray)
		{
			element_type = VariantType.None;
		}
	}

	public sealed class FixedArrayMarshalInfo : MarshalInfo {

		internal NativeType element_type;
		internal int size;

		public NativeType ElementType {
			get { return element_type; }
			set { element_type = value; }
		}

		public int Size {
			get { return size; }
			set { size = value; }
		}

		public FixedArrayMarshalInfo ()
			: base (NativeType.FixedArray)
		{
			element_type = NativeType.None;
		}
	}

	public sealed class FixedSysStringMarshalInfo : MarshalInfo {

		internal int size;

		public int Size {
			get { return size; }
			set { size = value; }
		}

		public FixedSysStringMarshalInfo ()
			: base (NativeType.FixedSysString)
		{
			size = -1;
		}
	}
}
