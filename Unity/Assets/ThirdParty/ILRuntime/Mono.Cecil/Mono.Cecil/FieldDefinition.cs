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
using System.Threading;
using ILRuntime.Mono.Collections.Generic;

namespace ILRuntime.Mono.Cecil {

	public sealed class FieldDefinition : FieldReference, IMemberDefinition, IConstantProvider, IMarshalInfoProvider {

		ushort attributes;
		Collection<CustomAttribute> custom_attributes;

		int offset = Mixin.NotResolvedMarker;

		internal int rva = Mixin.NotResolvedMarker;
		byte [] initial_value;

		object constant = Mixin.NotResolved;

		MarshalInfo marshal_info;

		void ResolveLayout ()
		{
			if (offset != Mixin.NotResolvedMarker)
				return;

			if (!HasImage) {
				offset = Mixin.NoDataMarker;
				return;
			}

			lock (Module.SyncRoot) {
				if (offset != Mixin.NotResolvedMarker)
					return;
				offset = Module.Read (this, (field, reader) => reader.ReadFieldLayout (field));
			}
		}

		public bool HasLayoutInfo {
			get {
				if (offset >= 0)
					return true;

				ResolveLayout ();

				return offset >= 0;
			}
		}

		public int Offset {
			get {
				if (offset >= 0)
					return offset;

				ResolveLayout ();

				return offset >= 0 ? offset : -1;
			}
			set { offset = value; }
		}

		internal FieldDefinitionProjection WindowsRuntimeProjection {
			get { return (FieldDefinitionProjection) projection; }
			set { projection = value; }
		}

		void ResolveRVA ()
		{
			if (rva != Mixin.NotResolvedMarker)
				return;

			if (!HasImage)
				return;

			lock (Module.SyncRoot) {
				if (rva != Mixin.NotResolvedMarker)
					return;
				rva = Module.Read (this, (field, reader) => reader.ReadFieldRVA (field));
			}
		}

		public int RVA {
			get {
				if (rva > 0)
					return rva;

				ResolveRVA ();

				return rva > 0 ? rva : 0;
			}
		}

		public byte [] InitialValue {
			get {
				if (initial_value != null)
					return initial_value;

				ResolveRVA ();

				if (initial_value == null)
					initial_value = Empty<byte>.Array;

				return initial_value;
			}
			set {
				initial_value = value;
				rva = 0;
			}
		}

		public FieldAttributes Attributes {
			get { return (FieldAttributes) attributes; }
			set {
				if (IsWindowsRuntimeProjection && (ushort) value != attributes)
					throw new InvalidOperationException ();

				attributes = (ushort) value;
			}
		}

		public bool HasConstant {
			get {
				this.ResolveConstant (ref constant, Module);

				return constant != Mixin.NoValue;
			}
			set { if (!value) constant = Mixin.NoValue; }
		}

		public object Constant {
			get { return HasConstant ? constant : null;	}
			set { constant = value; }
		}

		public bool HasCustomAttributes {
			get {
				if (custom_attributes != null)
					return custom_attributes.Count > 0;

				return this.GetHasCustomAttributes (Module);
			}
		}

		public Collection<CustomAttribute> CustomAttributes {
			get { return custom_attributes ?? (this.GetCustomAttributes (ref custom_attributes, Module)); }
		}

		public bool HasMarshalInfo {
			get {
				if (marshal_info != null)
					return true;

				return this.GetHasMarshalInfo (Module);
			}
		}

		public MarshalInfo MarshalInfo {
			get { return marshal_info ?? (this.GetMarshalInfo (ref marshal_info, Module)); }
			set { marshal_info = value; }
		}

		#region FieldAttributes

		public bool IsCompilerControlled {
			get { return attributes.GetMaskedAttributes ((ushort) FieldAttributes.FieldAccessMask, (ushort) FieldAttributes.CompilerControlled); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) FieldAttributes.FieldAccessMask, (ushort) FieldAttributes.CompilerControlled, value); }
		}

		public bool IsPrivate {
			get { return attributes.GetMaskedAttributes ((ushort) FieldAttributes.FieldAccessMask, (ushort) FieldAttributes.Private); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) FieldAttributes.FieldAccessMask, (ushort) FieldAttributes.Private, value); }
		}

		public bool IsFamilyAndAssembly {
			get { return attributes.GetMaskedAttributes ((ushort) FieldAttributes.FieldAccessMask, (ushort) FieldAttributes.FamANDAssem); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) FieldAttributes.FieldAccessMask, (ushort) FieldAttributes.FamANDAssem, value); }
		}

		public bool IsAssembly {
			get { return attributes.GetMaskedAttributes ((ushort) FieldAttributes.FieldAccessMask, (ushort) FieldAttributes.Assembly); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) FieldAttributes.FieldAccessMask, (ushort) FieldAttributes.Assembly, value); }
		}

		public bool IsFamily {
			get { return attributes.GetMaskedAttributes ((ushort) FieldAttributes.FieldAccessMask, (ushort) FieldAttributes.Family); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) FieldAttributes.FieldAccessMask, (ushort) FieldAttributes.Family, value); }
		}

		public bool IsFamilyOrAssembly {
			get { return attributes.GetMaskedAttributes ((ushort) FieldAttributes.FieldAccessMask, (ushort) FieldAttributes.FamORAssem); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) FieldAttributes.FieldAccessMask, (ushort) FieldAttributes.FamORAssem, value); }
		}

		public bool IsPublic {
			get { return attributes.GetMaskedAttributes ((ushort) FieldAttributes.FieldAccessMask, (ushort) FieldAttributes.Public); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) FieldAttributes.FieldAccessMask, (ushort) FieldAttributes.Public, value); }
		}

		public bool IsStatic {
			get { return attributes.GetAttributes ((ushort) FieldAttributes.Static); }
			set { attributes = attributes.SetAttributes ((ushort) FieldAttributes.Static, value); }
		}

		public bool IsInitOnly {
			get { return attributes.GetAttributes ((ushort) FieldAttributes.InitOnly); }
			set { attributes = attributes.SetAttributes ((ushort) FieldAttributes.InitOnly, value); }
		}

		public bool IsLiteral {
			get { return attributes.GetAttributes ((ushort) FieldAttributes.Literal); }
			set { attributes = attributes.SetAttributes ((ushort) FieldAttributes.Literal, value); }
		}

		public bool IsNotSerialized {
			get { return attributes.GetAttributes ((ushort) FieldAttributes.NotSerialized); }
			set { attributes = attributes.SetAttributes ((ushort) FieldAttributes.NotSerialized, value); }
		}

		public bool IsSpecialName {
			get { return attributes.GetAttributes ((ushort) FieldAttributes.SpecialName); }
			set { attributes = attributes.SetAttributes ((ushort) FieldAttributes.SpecialName, value); }
		}

		public bool IsPInvokeImpl {
			get { return attributes.GetAttributes ((ushort) FieldAttributes.PInvokeImpl); }
			set { attributes = attributes.SetAttributes ((ushort) FieldAttributes.PInvokeImpl, value); }
		}

		public bool IsRuntimeSpecialName {
			get { return attributes.GetAttributes ((ushort) FieldAttributes.RTSpecialName); }
			set { attributes = attributes.SetAttributes ((ushort) FieldAttributes.RTSpecialName, value); }
		}

		public bool HasDefault {
			get { return attributes.GetAttributes ((ushort) FieldAttributes.HasDefault); }
			set { attributes = attributes.SetAttributes ((ushort) FieldAttributes.HasDefault, value); }
		}

		#endregion

		public override bool IsDefinition {
			get { return true; }
		}

		public new TypeDefinition DeclaringType {
			get { return (TypeDefinition) base.DeclaringType; }
			set { base.DeclaringType = value; }
		}

		public FieldDefinition (string name, FieldAttributes attributes, TypeReference fieldType)
			: base (name, fieldType)
		{
			this.attributes = (ushort) attributes;
		}

		public override FieldDefinition Resolve ()
		{
			return this;
		}
	}

	static partial class Mixin {

		public const int NotResolvedMarker = -2;
		public const int NoDataMarker = -1;
	}
}
