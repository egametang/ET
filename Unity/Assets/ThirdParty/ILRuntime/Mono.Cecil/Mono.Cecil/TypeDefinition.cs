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
using ILRuntime.Mono.Cecil.Metadata;
using ILRuntime.Mono.Collections.Generic;

namespace ILRuntime.Mono.Cecil {

	public sealed class TypeDefinition : TypeReference, IMemberDefinition, ISecurityDeclarationProvider {

		uint attributes;
		TypeReference base_type;
		internal Range fields_range;
		internal Range methods_range;

		short packing_size = Mixin.NotResolvedMarker;
		int class_size = Mixin.NotResolvedMarker;

		InterfaceImplementationCollection interfaces;
		Collection<TypeDefinition> nested_types;
		Collection<MethodDefinition> methods;
		Collection<FieldDefinition> fields;
		Collection<EventDefinition> events;
		Collection<PropertyDefinition> properties;
		Collection<CustomAttribute> custom_attributes;
		Collection<SecurityDeclaration> security_declarations;

		public TypeAttributes Attributes {
			get { return (TypeAttributes) attributes; }
			set {
				if (IsWindowsRuntimeProjection && (ushort) value != attributes)
					throw new InvalidOperationException ();

				attributes = (uint) value;
			}
		}

		public TypeReference BaseType {
			get { return base_type; }
			set { base_type = value; }
		}

		public override string Name {
			get { return base.Name; }
			set {
				if (IsWindowsRuntimeProjection && value != base.Name)
					throw new InvalidOperationException ();

				base.Name = value;
			}
		}

		void ResolveLayout ()
		{
			if (!HasImage) {
				packing_size = Mixin.NoDataMarker;
				class_size = Mixin.NoDataMarker;
				return;
			}

			lock (Module.SyncRoot) {
				if (packing_size != Mixin.NotResolvedMarker || class_size != Mixin.NotResolvedMarker)
					return;

				var row = Module.Read (this, (type, reader) => reader.ReadTypeLayout (type));

				packing_size = row.Col1;
				class_size = row.Col2;
			}
		}

		public bool HasLayoutInfo {
			get {
				if (packing_size >= 0 || class_size >= 0)
					return true;

				ResolveLayout ();

				return packing_size >= 0 || class_size >= 0;
			}
		}

		public short PackingSize {
			get {
				if (packing_size >= 0)
					return packing_size;

				ResolveLayout ();

				return packing_size >= 0 ? packing_size : (short) -1;
			}
			set { packing_size = value; }
		}

		public int ClassSize {
			get {
				if (class_size >= 0)
					return class_size;

				ResolveLayout ();

				return class_size >= 0 ? class_size : -1;
			}
			set { class_size = value; }
		}

		public bool HasInterfaces {
			get {
				if (interfaces != null)
					return interfaces.Count > 0;

				return HasImage && Module.Read (this, (type, reader) => reader.HasInterfaces (type));
			}
		}

		public Collection<InterfaceImplementation> Interfaces {
			get {
				if (interfaces != null)
					return interfaces;

				if (HasImage)
					return Module.Read (ref interfaces, this, (type, reader) => reader.ReadInterfaces (type));

				Interlocked.CompareExchange (ref interfaces, new InterfaceImplementationCollection (this), null);
				return interfaces;
			}
		}

		public bool HasNestedTypes {
			get {
				if (nested_types != null)
					return nested_types.Count > 0;

				return HasImage && Module.Read (this, (type, reader) => reader.HasNestedTypes (type));
			}
		}

		public Collection<TypeDefinition> NestedTypes {
			get {
				if (nested_types != null)
					return nested_types;

				if (HasImage)
					return Module.Read (ref nested_types, this, (type, reader) => reader.ReadNestedTypes (type));

				Interlocked.CompareExchange (ref nested_types, new MemberDefinitionCollection<TypeDefinition> (this), null);
				return nested_types;
			}
		}

		public bool HasMethods {
			get {
				if (methods != null)
					return methods.Count > 0;

				return HasImage && methods_range.Length > 0;
			}
		}

		public Collection<MethodDefinition> Methods {
			get {
				if (methods != null)
					return methods;

				if (HasImage)
					return Module.Read (ref methods, this, (type, reader) => reader.ReadMethods (type));

				Interlocked.CompareExchange (ref methods, new MemberDefinitionCollection<MethodDefinition> (this) , null);
				return methods;
			}
		}

		public bool HasFields {
			get {
				if (fields != null)
					return fields.Count > 0;

				return HasImage && fields_range.Length > 0;
			}
		}

		public Collection<FieldDefinition> Fields {
			get {
				if (fields != null)
					return fields;

				if (HasImage)
					return Module.Read (ref fields, this, (type, reader) => reader.ReadFields (type));

				Interlocked.CompareExchange (ref fields, new MemberDefinitionCollection<FieldDefinition> (this), null);
				return fields;
			}
		}

		public bool HasEvents {
			get {
				if (events != null)
					return events.Count > 0;

				return HasImage && Module.Read (this, (type, reader) => reader.HasEvents (type));
			}
		}

		public Collection<EventDefinition> Events {
			get {
				if (events != null)
					return events;

				if (HasImage)
					return Module.Read (ref events, this, (type, reader) => reader.ReadEvents (type));

				Interlocked.CompareExchange (ref events, new MemberDefinitionCollection<EventDefinition> (this), null);
				return events;
			}
		}

		public bool HasProperties {
			get {
				if (properties != null)
					return properties.Count > 0;

				return HasImage && Module.Read (this, (type, reader) => reader.HasProperties (type));
			}
		}

		public Collection<PropertyDefinition> Properties {
			get {
				if (properties != null)
					return properties;

				if (HasImage)
					return Module.Read (ref properties, this, (type, reader) => reader.ReadProperties (type));

				Interlocked.CompareExchange (ref properties, new MemberDefinitionCollection<PropertyDefinition> (this), null);
				return properties;
			}
		}

		public bool HasSecurityDeclarations {
			get {
				if (security_declarations != null)
					return security_declarations.Count > 0;

				return this.GetHasSecurityDeclarations (Module);
			}
		}

		public Collection<SecurityDeclaration> SecurityDeclarations {
			get { return security_declarations ?? (this.GetSecurityDeclarations (ref security_declarations, Module)); }
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

		public override bool HasGenericParameters {
			get {
				if (generic_parameters != null)
					return generic_parameters.Count > 0;

				return this.GetHasGenericParameters (Module);
			}
		}

		public override Collection<GenericParameter> GenericParameters {
			get { return generic_parameters ?? (this.GetGenericParameters (ref generic_parameters, Module)); }
		}

		#region TypeAttributes

		public bool IsNotPublic {
			get { return attributes.GetMaskedAttributes ((uint) TypeAttributes.VisibilityMask, (uint) TypeAttributes.NotPublic); }
			set { attributes = attributes.SetMaskedAttributes ((uint) TypeAttributes.VisibilityMask, (uint) TypeAttributes.NotPublic, value); }
		}

		public bool IsPublic {
			get { return attributes.GetMaskedAttributes ((uint) TypeAttributes.VisibilityMask, (uint) TypeAttributes.Public); }
			set { attributes = attributes.SetMaskedAttributes ((uint) TypeAttributes.VisibilityMask, (uint) TypeAttributes.Public, value); }
		}

		public bool IsNestedPublic {
			get { return attributes.GetMaskedAttributes ((uint) TypeAttributes.VisibilityMask, (uint) TypeAttributes.NestedPublic); }
			set { attributes = attributes.SetMaskedAttributes ((uint) TypeAttributes.VisibilityMask, (uint) TypeAttributes.NestedPublic, value); }
		}

		public bool IsNestedPrivate {
			get { return attributes.GetMaskedAttributes ((uint) TypeAttributes.VisibilityMask, (uint) TypeAttributes.NestedPrivate); }
			set { attributes = attributes.SetMaskedAttributes ((uint) TypeAttributes.VisibilityMask, (uint) TypeAttributes.NestedPrivate, value); }
		}

		public bool IsNestedFamily {
			get { return attributes.GetMaskedAttributes ((uint) TypeAttributes.VisibilityMask, (uint) TypeAttributes.NestedFamily); }
			set { attributes = attributes.SetMaskedAttributes ((uint) TypeAttributes.VisibilityMask, (uint) TypeAttributes.NestedFamily, value); }
		}

		public bool IsNestedAssembly {
			get { return attributes.GetMaskedAttributes ((uint) TypeAttributes.VisibilityMask, (uint) TypeAttributes.NestedAssembly); }
			set { attributes = attributes.SetMaskedAttributes ((uint) TypeAttributes.VisibilityMask, (uint) TypeAttributes.NestedAssembly, value); }
		}

		public bool IsNestedFamilyAndAssembly {
			get { return attributes.GetMaskedAttributes ((uint) TypeAttributes.VisibilityMask, (uint) TypeAttributes.NestedFamANDAssem); }
			set { attributes = attributes.SetMaskedAttributes ((uint) TypeAttributes.VisibilityMask, (uint) TypeAttributes.NestedFamANDAssem, value); }
		}

		public bool IsNestedFamilyOrAssembly {
			get { return attributes.GetMaskedAttributes ((uint) TypeAttributes.VisibilityMask, (uint) TypeAttributes.NestedFamORAssem); }
			set { attributes = attributes.SetMaskedAttributes ((uint) TypeAttributes.VisibilityMask, (uint) TypeAttributes.NestedFamORAssem, value); }
		}

		public bool IsAutoLayout {
			get { return attributes.GetMaskedAttributes ((uint) TypeAttributes.LayoutMask, (uint) TypeAttributes.AutoLayout); }
			set { attributes = attributes.SetMaskedAttributes ((uint) TypeAttributes.LayoutMask, (uint) TypeAttributes.AutoLayout, value); }
		}

		public bool IsSequentialLayout {
			get { return attributes.GetMaskedAttributes ((uint) TypeAttributes.LayoutMask, (uint) TypeAttributes.SequentialLayout); }
			set { attributes = attributes.SetMaskedAttributes ((uint) TypeAttributes.LayoutMask, (uint) TypeAttributes.SequentialLayout, value); }
		}

		public bool IsExplicitLayout {
			get { return attributes.GetMaskedAttributes ((uint) TypeAttributes.LayoutMask, (uint) TypeAttributes.ExplicitLayout); }
			set { attributes = attributes.SetMaskedAttributes ((uint) TypeAttributes.LayoutMask, (uint) TypeAttributes.ExplicitLayout, value); }
		}

		public bool IsClass {
			get { return attributes.GetMaskedAttributes ((uint) TypeAttributes.ClassSemanticMask, (uint) TypeAttributes.Class); }
			set { attributes = attributes.SetMaskedAttributes ((uint) TypeAttributes.ClassSemanticMask, (uint) TypeAttributes.Class, value); }
		}

		public bool IsInterface {
			get { return attributes.GetMaskedAttributes ((uint) TypeAttributes.ClassSemanticMask, (uint) TypeAttributes.Interface); }
			set { attributes = attributes.SetMaskedAttributes ((uint) TypeAttributes.ClassSemanticMask, (uint) TypeAttributes.Interface, value); }
		}

		public bool IsAbstract {
			get { return attributes.GetAttributes ((uint) TypeAttributes.Abstract); }
			set { attributes = attributes.SetAttributes ((uint) TypeAttributes.Abstract, value); }
		}

		public bool IsSealed {
			get { return attributes.GetAttributes ((uint) TypeAttributes.Sealed); }
			set { attributes = attributes.SetAttributes ((uint) TypeAttributes.Sealed, value); }
		}

		public bool IsSpecialName {
			get { return attributes.GetAttributes ((uint) TypeAttributes.SpecialName); }
			set { attributes = attributes.SetAttributes ((uint) TypeAttributes.SpecialName, value); }
		}

		public bool IsImport {
			get { return attributes.GetAttributes ((uint) TypeAttributes.Import); }
			set { attributes = attributes.SetAttributes ((uint) TypeAttributes.Import, value); }
		}

		public bool IsSerializable {
			get { return attributes.GetAttributes ((uint) TypeAttributes.Serializable); }
			set { attributes = attributes.SetAttributes ((uint) TypeAttributes.Serializable, value); }
		}

		public bool IsWindowsRuntime {
			get { return attributes.GetAttributes ((uint) TypeAttributes.WindowsRuntime); }
			set { attributes = attributes.SetAttributes ((uint) TypeAttributes.WindowsRuntime, value); }
		}

		public bool IsAnsiClass {
			get { return attributes.GetMaskedAttributes ((uint) TypeAttributes.StringFormatMask, (uint) TypeAttributes.AnsiClass); }
			set { attributes = attributes.SetMaskedAttributes ((uint) TypeAttributes.StringFormatMask, (uint) TypeAttributes.AnsiClass, value); }
		}

		public bool IsUnicodeClass {
			get { return attributes.GetMaskedAttributes ((uint) TypeAttributes.StringFormatMask, (uint) TypeAttributes.UnicodeClass); }
			set { attributes = attributes.SetMaskedAttributes ((uint) TypeAttributes.StringFormatMask, (uint) TypeAttributes.UnicodeClass, value); }
		}

		public bool IsAutoClass {
			get { return attributes.GetMaskedAttributes ((uint) TypeAttributes.StringFormatMask, (uint) TypeAttributes.AutoClass); }
			set { attributes = attributes.SetMaskedAttributes ((uint) TypeAttributes.StringFormatMask, (uint) TypeAttributes.AutoClass, value); }
		}

		public bool IsBeforeFieldInit {
			get { return attributes.GetAttributes ((uint) TypeAttributes.BeforeFieldInit); }
			set { attributes = attributes.SetAttributes ((uint) TypeAttributes.BeforeFieldInit, value); }
		}

		public bool IsRuntimeSpecialName {
			get { return attributes.GetAttributes ((uint) TypeAttributes.RTSpecialName); }
			set { attributes = attributes.SetAttributes ((uint) TypeAttributes.RTSpecialName, value); }
		}

		public bool HasSecurity {
			get { return attributes.GetAttributes ((uint) TypeAttributes.HasSecurity); }
			set { attributes = attributes.SetAttributes ((uint) TypeAttributes.HasSecurity, value); }
		}

		#endregion

		public bool IsEnum {
			get { return base_type != null && base_type.IsTypeOf ("System", "Enum"); }
		}

		public override bool IsValueType {
			get {
				if (base_type == null)
					return false;

				return base_type.IsTypeOf ("System", "Enum") || (base_type.IsTypeOf ("System", "ValueType") && !this.IsTypeOf ("System", "Enum"));
			}
			set {
				throw new NotSupportedException ();
			}
		}

		public override bool IsPrimitive {
			get {
				ElementType primitive_etype;
				return MetadataSystem.TryGetPrimitiveElementType (this, out primitive_etype) && primitive_etype.IsPrimitive ();
			}
		}

		public override MetadataType MetadataType {
			get {
				ElementType primitive_etype;
				if (MetadataSystem.TryGetPrimitiveElementType (this, out primitive_etype))
					return (MetadataType) primitive_etype;

				return base.MetadataType;
			}
		}

		public override bool IsDefinition {
			get { return true; }
		}

		public new TypeDefinition DeclaringType {
			get { return (TypeDefinition) base.DeclaringType; }
			set { base.DeclaringType = value; }
		}

		internal new TypeDefinitionProjection WindowsRuntimeProjection {
			get { return (TypeDefinitionProjection) projection; }
			set { projection = value; }
		}

		public TypeDefinition (string @namespace, string name, TypeAttributes attributes)
			: base (@namespace, name)
		{
			this.attributes = (uint) attributes;
			this.token = new MetadataToken (TokenType.TypeDef);
		}

		public TypeDefinition (string @namespace, string name, TypeAttributes attributes, TypeReference baseType) :
			this (@namespace, name, attributes)
		{
			this.BaseType = baseType;
		}

		protected override void ClearFullName ()
		{
			base.ClearFullName ();

			if (!HasNestedTypes)
				return;

			var nested_types = this.NestedTypes;

			for (int i = 0; i < nested_types.Count; i++)
				nested_types [i].ClearFullName ();
		}

		public override TypeDefinition Resolve ()
		{
			return this;
		}
	}

	public sealed class InterfaceImplementation : ICustomAttributeProvider
	{
		internal TypeDefinition type;
		internal MetadataToken token;

		TypeReference interface_type;
		Collection<CustomAttribute> custom_attributes;

		public TypeReference InterfaceType {
			get { return interface_type; }
			set { interface_type = value; }
		}

		public bool HasCustomAttributes {
			get {
				if (custom_attributes != null)
					return custom_attributes.Count > 0;

				if (type == null)
					return false;

				return this.GetHasCustomAttributes (type.Module);
			}
		}

		public Collection<CustomAttribute> CustomAttributes {
			get {
				if (type == null) {
					if (custom_attributes == null)
						Interlocked.CompareExchange (ref custom_attributes, new Collection<CustomAttribute> (), null);
					return custom_attributes;
				}

				return custom_attributes ?? (this.GetCustomAttributes (ref custom_attributes, type.Module));
			}
		}

		public MetadataToken MetadataToken {
			get { return token; }
			set { token = value; }
		}

		internal InterfaceImplementation (TypeReference interfaceType, MetadataToken token)
		{
			this.interface_type = interfaceType;
			this.token = token;
		}

		public InterfaceImplementation (TypeReference interfaceType)
		{
			Mixin.CheckType (interfaceType, Mixin.Argument.interfaceType);

			this.interface_type = interfaceType;
			this.token = new MetadataToken (TokenType.InterfaceImpl);
		}
	}

	class InterfaceImplementationCollection : Collection<InterfaceImplementation>
	{
		readonly TypeDefinition type;

		internal InterfaceImplementationCollection (TypeDefinition type)
		{
			this.type = type;
		}

		internal InterfaceImplementationCollection (TypeDefinition type, int length)
			: base (length)
		{
			this.type = type;
		}

		protected override void OnAdd (InterfaceImplementation item, int index)
		{
			item.type = type;
		}

		protected override void OnInsert (InterfaceImplementation item, int index)
		{
			item.type = type;
		}

		protected override void OnSet (InterfaceImplementation item, int index)
		{
			item.type = type;
		}

		protected override void OnRemove (InterfaceImplementation item, int index)
		{
			item.type = null;
		}
	}

	static partial class Mixin {

		public static TypeReference GetEnumUnderlyingType (this TypeDefinition self)
		{
			var fields = self.Fields;

			for (int i = 0; i < fields.Count; i++) {
				var field = fields [i];
				if (!field.IsStatic)
					return field.FieldType;
			}

			throw new ArgumentException ();
		}

		public static TypeDefinition GetNestedType (this TypeDefinition self, string fullname)
		{
			if (!self.HasNestedTypes)
				return null;

			var nested_types = self.NestedTypes;

			for (int i = 0; i < nested_types.Count; i++) {
				var nested_type = nested_types [i];

				if (nested_type.TypeFullName () == fullname)
					return nested_type;
			}

			return null;
		}
	}
}
