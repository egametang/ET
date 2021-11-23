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

using ILRuntime.Mono.Cecil.Metadata;

namespace ILRuntime.Mono.Cecil {

	public sealed class GenericParameter : TypeReference, ICustomAttributeProvider {

		internal int position;
		internal GenericParameterType type;
		internal IGenericParameterProvider owner;

		ushort attributes;
		GenericParameterConstraintCollection constraints;
		Collection<CustomAttribute> custom_attributes;

		public GenericParameterAttributes Attributes {
			get { return (GenericParameterAttributes) attributes; }
			set { attributes = (ushort) value; }
		}

		public int Position {
			get { return position; }
		}

		public GenericParameterType Type {
			get { return type; }
		}

		public IGenericParameterProvider Owner {
			get { return owner; }
		}

		public bool HasConstraints {
			get {
				if (constraints != null)
					return constraints.Count > 0;

				return HasImage && Module.Read (this, (generic_parameter, reader) => reader.HasGenericConstraints (generic_parameter));
			}
		}

		public Collection<GenericParameterConstraint> Constraints {
			get {
				if (constraints != null)
					return constraints;

				if (HasImage)
					return Module.Read (ref constraints, this, (generic_parameter, reader) => reader.ReadGenericConstraints (generic_parameter));

				Interlocked.CompareExchange (ref constraints, new GenericParameterConstraintCollection (this), null);
				return constraints;
			}
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

		public override IMetadataScope Scope {
			get {
				if (owner == null)
					return null;

				return owner.GenericParameterType == GenericParameterType.Method
					? ((MethodReference) owner).DeclaringType.Scope
					: ((TypeReference) owner).Scope;
			}
			set { throw new InvalidOperationException (); }
		}

		public override TypeReference DeclaringType {
			get { return owner as TypeReference; }
			set { throw new InvalidOperationException (); }
		}

		public MethodReference DeclaringMethod {
			get { return owner as MethodReference; }
		}

		public override ModuleDefinition Module {
			get { return module ?? owner.Module; }
		}

		public override string Name {
			get {
				if (!string.IsNullOrEmpty (base.Name))
					return base.Name;

				return base.Name = (type == GenericParameterType.Method ? "!!" : "!") + position;
			}
		}

		public override string Namespace {
			get { return string.Empty; }
			set { throw new InvalidOperationException (); }
		}

		public override string FullName {
			get { return Name; }
		}

		public override bool IsGenericParameter {
			get { return true; }
		}

		public override bool ContainsGenericParameter {
			get { return true; }
		}

		public override MetadataType MetadataType {
			get { return (MetadataType) etype; }
		}

		#region GenericParameterAttributes

		public bool IsNonVariant {
			get { return attributes.GetMaskedAttributes ((ushort) GenericParameterAttributes.VarianceMask, (ushort) GenericParameterAttributes.NonVariant); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) GenericParameterAttributes.VarianceMask, (ushort) GenericParameterAttributes.NonVariant, value); }
		}

		public bool IsCovariant {
			get { return attributes.GetMaskedAttributes ((ushort) GenericParameterAttributes.VarianceMask, (ushort) GenericParameterAttributes.Covariant); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) GenericParameterAttributes.VarianceMask, (ushort) GenericParameterAttributes.Covariant, value); }
		}

		public bool IsContravariant {
			get { return attributes.GetMaskedAttributes ((ushort) GenericParameterAttributes.VarianceMask, (ushort) GenericParameterAttributes.Contravariant); }
			set { attributes = attributes.SetMaskedAttributes ((ushort) GenericParameterAttributes.VarianceMask, (ushort) GenericParameterAttributes.Contravariant, value); }
		}

		public bool HasReferenceTypeConstraint {
			get { return attributes.GetAttributes ((ushort) GenericParameterAttributes.ReferenceTypeConstraint); }
			set { attributes = attributes.SetAttributes ((ushort) GenericParameterAttributes.ReferenceTypeConstraint, value); }
		}

		public bool HasNotNullableValueTypeConstraint {
			get { return attributes.GetAttributes ((ushort) GenericParameterAttributes.NotNullableValueTypeConstraint); }
			set { attributes = attributes.SetAttributes ((ushort) GenericParameterAttributes.NotNullableValueTypeConstraint, value); }
		}

		public bool HasDefaultConstructorConstraint {
			get { return attributes.GetAttributes ((ushort) GenericParameterAttributes.DefaultConstructorConstraint); }
			set { attributes = attributes.SetAttributes ((ushort) GenericParameterAttributes.DefaultConstructorConstraint, value); }
		}

		#endregion

		public GenericParameter (IGenericParameterProvider owner)
			: this (string.Empty, owner)
		{
		}

		public GenericParameter (string name, IGenericParameterProvider owner)
			: base (string.Empty, name)
		{
			if (owner == null)
				throw new ArgumentNullException ();

			this.position = -1;
			this.owner = owner;
			this.type = owner.GenericParameterType;
			this.etype = ConvertGenericParameterType (this.type);
			this.token = new MetadataToken (TokenType.GenericParam);

		}

		internal GenericParameter (int position, GenericParameterType type, ModuleDefinition module)
			: base (string.Empty, string.Empty)
		{
			Mixin.CheckModule (module);

			this.position = position;
			this.type = type;
			this.etype = ConvertGenericParameterType (type);
			this.module = module;
			this.token = new MetadataToken (TokenType.GenericParam);
		}

		static ElementType ConvertGenericParameterType (GenericParameterType type)
		{
			switch (type) {
			case GenericParameterType.Type:
				return ElementType.Var;
			case GenericParameterType.Method:
				return ElementType.MVar;
			}

			throw new ArgumentOutOfRangeException ();
		}

		public override TypeDefinition Resolve ()
		{
			return null;
		}
	}

	sealed class GenericParameterCollection : Collection<GenericParameter> {

		readonly IGenericParameterProvider owner;

		internal GenericParameterCollection (IGenericParameterProvider owner)
		{
			this.owner = owner;
		}

		internal GenericParameterCollection (IGenericParameterProvider owner, int capacity)
			: base (capacity)
		{
			this.owner = owner;
		}

		protected override void OnAdd (GenericParameter item, int index)
		{
			UpdateGenericParameter (item, index);
		}

		protected override void OnInsert (GenericParameter item, int index)
		{
			UpdateGenericParameter (item, index);

			for (int i = index; i < size; i++)
				items[i].position = i + 1;
		}

		protected override void OnSet (GenericParameter item, int index)
		{
			UpdateGenericParameter (item, index);
		}

		void UpdateGenericParameter (GenericParameter item, int index)
		{
			item.owner = owner;
			item.position = index;
			item.type = owner.GenericParameterType;
		}

		protected override void OnRemove (GenericParameter item, int index)
		{
			item.owner = null;
			item.position = -1;
			item.type = GenericParameterType.Type;

			for (int i = index + 1; i < size; i++)
				items[i].position = i - 1;
		}
	}

	public sealed class GenericParameterConstraint : ICustomAttributeProvider {

		internal GenericParameter generic_parameter;
		internal MetadataToken token;

		TypeReference constraint_type;
		Collection<CustomAttribute> custom_attributes;

		public TypeReference ConstraintType {
			get { return constraint_type; }
			set { constraint_type = value; }
		}

		public bool HasCustomAttributes {
			get {
				if (custom_attributes != null)
					return custom_attributes.Count > 0;

				if (generic_parameter == null)
					return false;

				return this.GetHasCustomAttributes (generic_parameter.Module);
			}
		}

		public Collection<CustomAttribute> CustomAttributes {
			get {
				if (generic_parameter == null) {
					if (custom_attributes == null)
						Interlocked.CompareExchange (ref custom_attributes, new Collection<CustomAttribute> (), null);
					return custom_attributes;
				}

				return custom_attributes ?? (this.GetCustomAttributes (ref custom_attributes, generic_parameter.Module));
			}
		}

		public MetadataToken MetadataToken {
			get { return token; }
			set { token = value; }
		}

		internal GenericParameterConstraint (TypeReference constraintType, MetadataToken token)
		{
			this.constraint_type = constraintType;
			this.token = token;
		}

		public GenericParameterConstraint (TypeReference constraintType)
		{
			Mixin.CheckType (constraintType, Mixin.Argument.constraintType);

			this.constraint_type = constraintType;
			this.token = new MetadataToken (TokenType.GenericParamConstraint);
		}
	}

	class GenericParameterConstraintCollection : Collection<GenericParameterConstraint>
	{
		readonly GenericParameter generic_parameter;

		internal GenericParameterConstraintCollection (GenericParameter genericParameter)
		{
			this.generic_parameter = genericParameter;
		}

		internal GenericParameterConstraintCollection (GenericParameter genericParameter, int length)
			: base (length)
		{
			this.generic_parameter = genericParameter;
		}

		protected override void OnAdd (GenericParameterConstraint item, int index)
		{
			item.generic_parameter = generic_parameter;
		}

		protected override void OnInsert (GenericParameterConstraint item, int index)
		{
			item.generic_parameter = generic_parameter;
		}

		protected override void OnSet (GenericParameterConstraint item, int index)
		{
			item.generic_parameter = generic_parameter;
		}

		protected override void OnRemove (GenericParameterConstraint item, int index)
		{
			item.generic_parameter = null;
		}
	}
}
