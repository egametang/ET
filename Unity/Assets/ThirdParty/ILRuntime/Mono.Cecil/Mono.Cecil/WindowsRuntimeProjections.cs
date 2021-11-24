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
using System.Collections.Generic;
using System.Threading;
using ILRuntime.Mono.Collections.Generic;

namespace ILRuntime.Mono.Cecil {

	sealed class TypeDefinitionProjection {

		public readonly TypeAttributes Attributes;
		public readonly string Name;
		public readonly TypeDefinitionTreatment Treatment;
		public readonly Collection<MethodDefinition> RedirectedMethods;
		public readonly Collection<KeyValuePair<InterfaceImplementation, InterfaceImplementation>> RedirectedInterfaces;

		public TypeDefinitionProjection(TypeDefinition type, TypeDefinitionTreatment treatment, Collection<MethodDefinition> redirectedMethods, Collection<KeyValuePair<InterfaceImplementation, InterfaceImplementation>> redirectedInterfaces)
		{
			Attributes = type.Attributes;
			Name = type.Name;
			Treatment = treatment;
			RedirectedMethods = redirectedMethods;
			RedirectedInterfaces = redirectedInterfaces;
		}
	}

	sealed class TypeReferenceProjection {

		public readonly string Name;
		public readonly string Namespace;
		public readonly IMetadataScope Scope;
		public readonly TypeReferenceTreatment Treatment;

		public TypeReferenceProjection (TypeReference type, TypeReferenceTreatment treatment)
		{
			Name = type.Name;
			Namespace = type.Namespace;
			Scope = type.Scope;
			Treatment = treatment;
		}
	}

	sealed class MethodDefinitionProjection {

		public readonly MethodAttributes Attributes;
		public readonly MethodImplAttributes ImplAttributes;
		public readonly string Name;
		public readonly MethodDefinitionTreatment Treatment;

		public MethodDefinitionProjection (MethodDefinition method, MethodDefinitionTreatment treatment)
		{
			Attributes = method.Attributes;
			ImplAttributes = method.ImplAttributes;
			Name = method.Name;
			Treatment = treatment;
		}
	}

	sealed class FieldDefinitionProjection {

		public readonly FieldAttributes Attributes;
		public readonly FieldDefinitionTreatment Treatment;

		public FieldDefinitionProjection (FieldDefinition field, FieldDefinitionTreatment treatment)
		{
			Attributes = field.Attributes;
			Treatment = treatment;
		}
	}

	sealed class CustomAttributeValueProjection {

		public readonly AttributeTargets Targets;
		public readonly CustomAttributeValueTreatment Treatment;

		public CustomAttributeValueProjection (AttributeTargets targets, CustomAttributeValueTreatment treatment)
		{
			Targets = targets;
			Treatment = treatment;
		}
	}

	sealed class WindowsRuntimeProjections {

		struct ProjectionInfo {

			public readonly string WinRTNamespace;
			public readonly string ClrNamespace;
			public readonly string ClrName;
			public readonly string ClrAssembly;
			public readonly bool Attribute;

			public ProjectionInfo (string winrt_namespace, string clr_namespace, string clr_name, string clr_assembly, bool attribute = false)
			{
				WinRTNamespace = winrt_namespace;
				ClrNamespace = clr_namespace;
				ClrName = clr_name;
				ClrAssembly = clr_assembly;
				Attribute = attribute;
			}
		}

		static readonly Version version = new Version (4, 0, 0, 0);

		static readonly byte[] contract_pk_token = {
			0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A
		};

		static readonly byte[] contract_pk = {
			0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00, 0x06, 0x02, 0x00, 0x00,
			0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x31, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
			0x07, 0xD1, 0xFA, 0x57, 0xC4, 0xAE, 0xD9, 0xF0, 0xA3, 0x2E, 0x84, 0xAA, 0x0F, 0xAE, 0xFD, 0x0D,
			0xE9, 0xE8, 0xFD, 0x6A, 0xEC, 0x8F, 0x87, 0xFB, 0x03, 0x76, 0x6C, 0x83, 0x4C, 0x99, 0x92, 0x1E,
			0xB2, 0x3B, 0xE7, 0x9A, 0xD9, 0xD5, 0xDC, 0xC1, 0xDD, 0x9A, 0xD2, 0x36, 0x13, 0x21, 0x02, 0x90,
			0x0B, 0x72, 0x3C, 0xF9, 0x80, 0x95, 0x7F, 0xC4, 0xE1, 0x77, 0x10, 0x8F, 0xC6, 0x07, 0x77, 0x4F,
			0x29, 0xE8, 0x32, 0x0E, 0x92, 0xEA, 0x05, 0xEC, 0xE4, 0xE8, 0x21, 0xC0, 0xA5, 0xEF, 0xE8, 0xF1,
			0x64, 0x5C, 0x4C, 0x0C, 0x93, 0xC1, 0xAB, 0x99, 0x28, 0x5D, 0x62, 0x2C, 0xAA, 0x65, 0x2C, 0x1D,
			0xFA, 0xD6, 0x3D, 0x74, 0x5D, 0x6F, 0x2D, 0xE5, 0xF1, 0x7E, 0x5E, 0xAF, 0x0F, 0xC4, 0x96, 0x3D,
			0x26, 0x1C, 0x8A, 0x12, 0x43, 0x65, 0x18, 0x20, 0x6D, 0xC0, 0x93, 0x34, 0x4D, 0x5A, 0xD2, 0x93
		};

		static Dictionary<string, ProjectionInfo> projections;

		static Dictionary<string, ProjectionInfo> Projections
		{
			get {
				if (projections != null)
					return projections;


				var new_projections = new Dictionary<string, ProjectionInfo> {
					{ "AttributeTargets", new ProjectionInfo ("Windows.Foundation.Metadata", "System", "AttributeTargets", "System.Runtime") },
					{ "AttributeUsageAttribute", new ProjectionInfo ("Windows.Foundation.Metadata", "System", "AttributeUsageAttribute", "System.Runtime", attribute: true) },
					{ "Color", new ProjectionInfo ("Windows.UI", "Windows.UI", "Color", "System.Runtime.WindowsRuntime") },
					{ "CornerRadius", new ProjectionInfo ("Windows.UI.Xaml", "Windows.UI.Xaml", "CornerRadius", "System.Runtime.WindowsRuntime.UI.Xaml") },
					{ "DateTime", new ProjectionInfo ("Windows.Foundation", "System", "DateTimeOffset", "System.Runtime") },
					{ "Duration", new ProjectionInfo ("Windows.UI.Xaml", "Windows.UI.Xaml", "Duration", "System.Runtime.WindowsRuntime.UI.Xaml") },
					{ "DurationType", new ProjectionInfo ("Windows.UI.Xaml", "Windows.UI.Xaml", "DurationType", "System.Runtime.WindowsRuntime.UI.Xaml") },
					{ "EventHandler`1", new ProjectionInfo ("Windows.Foundation", "System", "EventHandler`1", "System.Runtime") },
					{ "EventRegistrationToken", new ProjectionInfo ("Windows.Foundation", "System.Runtime.InteropServices.WindowsRuntime", "EventRegistrationToken", "System.Runtime.InteropServices.WindowsRuntime") },
					{ "GeneratorPosition", new ProjectionInfo ("Windows.UI.Xaml.Controls.Primitives", "Windows.UI.Xaml.Controls.Primitives", "GeneratorPosition", "System.Runtime.WindowsRuntime.UI.Xaml") },
					{ "GridLength", new ProjectionInfo ("Windows.UI.Xaml", "Windows.UI.Xaml", "GridLength", "System.Runtime.WindowsRuntime.UI.Xaml") },
					{ "GridUnitType", new ProjectionInfo ("Windows.UI.Xaml", "Windows.UI.Xaml", "GridUnitType", "System.Runtime.WindowsRuntime.UI.Xaml") },
					{ "HResult", new ProjectionInfo ("Windows.Foundation", "System", "Exception", "System.Runtime") },
					{ "IBindableIterable", new ProjectionInfo ("Windows.UI.Xaml.Interop", "System.Collections", "IEnumerable", "System.Runtime") },
					{ "IBindableVector", new ProjectionInfo ("Windows.UI.Xaml.Interop", "System.Collections", "IList", "System.Runtime") },
					{ "IClosable", new ProjectionInfo ("Windows.Foundation", "System", "IDisposable", "System.Runtime") },
					{ "ICommand", new ProjectionInfo ("Windows.UI.Xaml.Input", "System.Windows.Input", "ICommand", "System.ObjectModel") },
					{ "IIterable`1", new ProjectionInfo ("Windows.Foundation.Collections", "System.Collections.Generic", "IEnumerable`1", "System.Runtime") },
					{ "IKeyValuePair`2", new ProjectionInfo ("Windows.Foundation.Collections", "System.Collections.Generic", "KeyValuePair`2", "System.Runtime") },
					{ "IMapView`2", new ProjectionInfo ("Windows.Foundation.Collections", "System.Collections.Generic", "IReadOnlyDictionary`2", "System.Runtime") },
					{ "IMap`2", new ProjectionInfo ("Windows.Foundation.Collections", "System.Collections.Generic", "IDictionary`2", "System.Runtime") },
					{ "INotifyCollectionChanged", new ProjectionInfo ("Windows.UI.Xaml.Interop", "System.Collections.Specialized", "INotifyCollectionChanged", "System.ObjectModel") },
					{ "INotifyPropertyChanged", new ProjectionInfo ("Windows.UI.Xaml.Data", "System.ComponentModel", "INotifyPropertyChanged", "System.ObjectModel") },
					{ "IReference`1", new ProjectionInfo ("Windows.Foundation", "System", "Nullable`1", "System.Runtime") },
					{ "IVectorView`1", new ProjectionInfo ("Windows.Foundation.Collections", "System.Collections.Generic", "IReadOnlyList`1", "System.Runtime") },
					{ "IVector`1", new ProjectionInfo ("Windows.Foundation.Collections", "System.Collections.Generic", "IList`1", "System.Runtime") },
					{ "KeyTime", new ProjectionInfo ("Windows.UI.Xaml.Media.Animation", "Windows.UI.Xaml.Media.Animation", "KeyTime", "System.Runtime.WindowsRuntime.UI.Xaml") },
					{ "Matrix", new ProjectionInfo ("Windows.UI.Xaml.Media", "Windows.UI.Xaml.Media", "Matrix", "System.Runtime.WindowsRuntime.UI.Xaml") },
					{ "Matrix3D", new ProjectionInfo ("Windows.UI.Xaml.Media.Media3D", "Windows.UI.Xaml.Media.Media3D", "Matrix3D", "System.Runtime.WindowsRuntime.UI.Xaml") },
					{ "Matrix3x2", new ProjectionInfo ("Windows.Foundation.Numerics", "System.Numerics", "Matrix3x2", "System.Numerics.Vectors") },
					{ "Matrix4x4", new ProjectionInfo ("Windows.Foundation.Numerics", "System.Numerics", "Matrix4x4", "System.Numerics.Vectors") },
					{ "NotifyCollectionChangedAction", new ProjectionInfo ("Windows.UI.Xaml.Interop", "System.Collections.Specialized", "NotifyCollectionChangedAction", "System.ObjectModel") },
					{ "NotifyCollectionChangedEventArgs", new ProjectionInfo ("Windows.UI.Xaml.Interop", "System.Collections.Specialized", "NotifyCollectionChangedEventArgs", "System.ObjectModel") },
					{ "NotifyCollectionChangedEventHandler", new ProjectionInfo ("Windows.UI.Xaml.Interop", "System.Collections.Specialized", "NotifyCollectionChangedEventHandler", "System.ObjectModel") },
					{ "Plane", new ProjectionInfo ("Windows.Foundation.Numerics", "System.Numerics", "Plane", "System.Numerics.Vectors") },
					{ "Point", new ProjectionInfo ("Windows.Foundation", "Windows.Foundation", "Point", "System.Runtime.WindowsRuntime") },
					{ "PropertyChangedEventArgs", new ProjectionInfo ("Windows.UI.Xaml.Data", "System.ComponentModel", "PropertyChangedEventArgs", "System.ObjectModel") },
					{ "PropertyChangedEventHandler", new ProjectionInfo ("Windows.UI.Xaml.Data", "System.ComponentModel", "PropertyChangedEventHandler", "System.ObjectModel") },
					{ "Quaternion", new ProjectionInfo ("Windows.Foundation.Numerics", "System.Numerics", "Quaternion", "System.Numerics.Vectors") },
					{ "Rect", new ProjectionInfo ("Windows.Foundation", "Windows.Foundation", "Rect", "System.Runtime.WindowsRuntime") },
					{ "RepeatBehavior", new ProjectionInfo ("Windows.UI.Xaml.Media.Animation", "Windows.UI.Xaml.Media.Animation", "RepeatBehavior", "System.Runtime.WindowsRuntime.UI.Xaml") },
					{ "RepeatBehaviorType", new ProjectionInfo ("Windows.UI.Xaml.Media.Animation", "Windows.UI.Xaml.Media.Animation", "RepeatBehaviorType", "System.Runtime.WindowsRuntime.UI.Xaml") },
					{ "Size", new ProjectionInfo ("Windows.Foundation", "Windows.Foundation", "Size", "System.Runtime.WindowsRuntime") },
					{ "Thickness", new ProjectionInfo ("Windows.UI.Xaml", "Windows.UI.Xaml", "Thickness", "System.Runtime.WindowsRuntime.UI.Xaml") },
					{ "TimeSpan", new ProjectionInfo ("Windows.Foundation", "System", "TimeSpan", "System.Runtime") },
					{ "TypeName", new ProjectionInfo ("Windows.UI.Xaml.Interop", "System", "Type", "System.Runtime") },
					{ "Uri", new ProjectionInfo ("Windows.Foundation", "System", "Uri", "System.Runtime") },
					{ "Vector2", new ProjectionInfo ("Windows.Foundation.Numerics", "System.Numerics", "Vector2", "System.Numerics.Vectors") },
					{ "Vector3", new ProjectionInfo ("Windows.Foundation.Numerics", "System.Numerics", "Vector3", "System.Numerics.Vectors") },
					{ "Vector4", new ProjectionInfo ("Windows.Foundation.Numerics", "System.Numerics", "Vector4", "System.Numerics.Vectors") },
				};

				Interlocked.CompareExchange (ref projections, new_projections, null);
				return projections;
			}
		}

		readonly ModuleDefinition module;
		Version corlib_version = new Version (255, 255, 255, 255);
		AssemblyNameReference[] virtual_references;

		AssemblyNameReference[] VirtualReferences {
			get {
				if (virtual_references == null) {
					// force module to read its assembly references. that will in turn initialize virtual_references
					Mixin.Read (module.AssemblyReferences);
				}

				return virtual_references;
			}
		}

		public WindowsRuntimeProjections (ModuleDefinition module)
		{
			this.module = module;
		}

		public static void Project (TypeDefinition type)
		{
			var treatment = TypeDefinitionTreatment.None;
			var metadata_kind = type.Module.MetadataKind;
			Collection<MethodDefinition> redirectedMethods = null;
			Collection<KeyValuePair<InterfaceImplementation, InterfaceImplementation>> redirectedInterfaces = null;

			if (type.IsWindowsRuntime) {
				if (metadata_kind == MetadataKind.WindowsMetadata) {
					treatment = GetWellKnownTypeDefinitionTreatment (type);
					if (treatment != TypeDefinitionTreatment.None) {
						ApplyProjection (type, new TypeDefinitionProjection (type, treatment, redirectedMethods, redirectedInterfaces));
						return;
					}

					var base_type = type.BaseType;
					if (base_type != null && IsAttribute (base_type)) {
						treatment = TypeDefinitionTreatment.NormalAttribute;
					} else {
						treatment = GenerateRedirectionInformation (type, out redirectedMethods, out redirectedInterfaces);
					}
				} 
				else if (metadata_kind == MetadataKind.ManagedWindowsMetadata && NeedsWindowsRuntimePrefix (type))
					treatment = TypeDefinitionTreatment.PrefixWindowsRuntimeName;

				if (treatment == TypeDefinitionTreatment.PrefixWindowsRuntimeName || treatment == TypeDefinitionTreatment.NormalType)
					if (!type.IsInterface && HasAttribute (type, "Windows.UI.Xaml", "TreatAsAbstractComposableClassAttribute"))
						treatment |= TypeDefinitionTreatment.Abstract;
			}
			else if (metadata_kind == MetadataKind.ManagedWindowsMetadata && IsClrImplementationType (type))
				treatment = TypeDefinitionTreatment.UnmangleWindowsRuntimeName;

			if (treatment != TypeDefinitionTreatment.None)
				ApplyProjection (type, new TypeDefinitionProjection (type, treatment, redirectedMethods, redirectedInterfaces));
		}

		static TypeDefinitionTreatment GetWellKnownTypeDefinitionTreatment (TypeDefinition type)
		{
			ProjectionInfo info;
			if (!Projections.TryGetValue (type.Name, out info))
				return TypeDefinitionTreatment.None;

			var treatment = info.Attribute ? TypeDefinitionTreatment.RedirectToClrAttribute : TypeDefinitionTreatment.RedirectToClrType;

			if (type.Namespace == info.ClrNamespace)
				return treatment;

			if (type.Namespace == info.WinRTNamespace)
				return treatment | TypeDefinitionTreatment.Internal;

			return TypeDefinitionTreatment.None;
		}

		private static TypeDefinitionTreatment GenerateRedirectionInformation (TypeDefinition type, out Collection<MethodDefinition> redirectedMethods, out Collection<KeyValuePair<InterfaceImplementation, InterfaceImplementation>> redirectedInterfaces)
		{
			bool implementsProjectedInterface = false;
			redirectedMethods = null;
			redirectedInterfaces = null;

			foreach (var implementedInterface in type.Interfaces) {
				if (IsRedirectedType (implementedInterface.InterfaceType)) {
					implementsProjectedInterface = true;
					break;
				}
			}

			if (!implementsProjectedInterface)
				return TypeDefinitionTreatment.NormalType;

			var allImplementedInterfaces = new HashSet<TypeReference> (new TypeReferenceEqualityComparer ());
			redirectedMethods = new Collection<MethodDefinition> ();
			redirectedInterfaces = new Collection<KeyValuePair<InterfaceImplementation, InterfaceImplementation>> ();

			foreach (var @interface in type.Interfaces) {
				var interfaceType = @interface.InterfaceType;

				if (IsRedirectedType (interfaceType)) {
					allImplementedInterfaces.Add (interfaceType);
					CollectImplementedInterfaces (interfaceType, allImplementedInterfaces);
				}
			}

			foreach (var implementedInterface in type.Interfaces) {
				var interfaceType = implementedInterface.InterfaceType;
				if (IsRedirectedType (implementedInterface.InterfaceType)) {
					var etype = interfaceType.GetElementType ();
					var unprojectedType = new TypeReference (etype.Namespace, etype.Name, etype.Module, etype.Scope) {
						DeclaringType = etype.DeclaringType,
						projection = etype.projection
					};

					RemoveProjection (unprojectedType);

					var genericInstanceType = interfaceType as GenericInstanceType;
					if (genericInstanceType != null) {
						var genericUnprojectedType = new GenericInstanceType (unprojectedType);
						foreach (var genericArgument in genericInstanceType.GenericArguments)
							genericUnprojectedType.GenericArguments.Add (genericArgument);

						unprojectedType = genericUnprojectedType;
					}

					var unprojectedInterface = new InterfaceImplementation (unprojectedType);
					redirectedInterfaces.Add (new KeyValuePair<InterfaceImplementation, InterfaceImplementation> (implementedInterface, unprojectedInterface));
				}
			}

			// Interfaces don't inherit methods of the interfaces they implement
			if (!type.IsInterface) {
				foreach (var implementedInterface in allImplementedInterfaces) {
					RedirectInterfaceMethods (implementedInterface, redirectedMethods);
				}
			}

			return TypeDefinitionTreatment.RedirectImplementedMethods;
		}

		private static void CollectImplementedInterfaces (TypeReference type, HashSet<TypeReference> results)
		{
			var typeResolver = TypeResolver.For (type);
			var typeDef = type.Resolve ();

			foreach (var implementedInterface in typeDef.Interfaces) {
				var interfaceType = typeResolver.Resolve (implementedInterface.InterfaceType);
				results.Add (interfaceType);
				CollectImplementedInterfaces (interfaceType, results);
			}
		}

		private static void RedirectInterfaceMethods (TypeReference interfaceType, Collection<MethodDefinition> redirectedMethods)
		{
			var typeResolver = TypeResolver.For (interfaceType);
			var typeDef = interfaceType.Resolve ();

			foreach (var method in typeDef.Methods) {
				var redirectedMethod = new MethodDefinition (method.Name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.NewSlot, typeResolver.Resolve (method.ReturnType));
				redirectedMethod.ImplAttributes = MethodImplAttributes.Runtime;

				foreach (var parameter in method.Parameters) {
					redirectedMethod.Parameters.Add (new ParameterDefinition (parameter.Name, parameter.Attributes, typeResolver.Resolve (parameter.ParameterType)));
				}

				redirectedMethod.Overrides.Add (typeResolver.Resolve (method));
				redirectedMethods.Add (redirectedMethod);
			}
		}

		private static bool IsRedirectedType (TypeReference type)
		{
			var typeRefProjection = type.GetElementType ().projection as TypeReferenceProjection;
			return typeRefProjection != null && typeRefProjection.Treatment == TypeReferenceTreatment.UseProjectionInfo;
		}

		static bool NeedsWindowsRuntimePrefix (TypeDefinition type)
		{
			if ((type.Attributes & (TypeAttributes.VisibilityMask | TypeAttributes.Interface)) != TypeAttributes.Public)
				return false;

			var base_type = type.BaseType;
			if (base_type == null || base_type.MetadataToken.TokenType != TokenType.TypeRef)
				return false;

			if (base_type.Namespace == "System")
				switch (base_type.Name)
				{
					case "Attribute":
					case "MulticastDelegate":
					case "ValueType":
						return false;
				}

			return true;
		}

		static bool IsClrImplementationType (TypeDefinition type)
		{
			if ((type.Attributes & (TypeAttributes.VisibilityMask | TypeAttributes.SpecialName)) != TypeAttributes.SpecialName)
				return false;
			return type.Name.StartsWith ("<CLR>");
		}

		public static void ApplyProjection (TypeDefinition type, TypeDefinitionProjection projection)
		{
			if (projection == null)
				return;

			var treatment = projection.Treatment;

			switch (treatment & TypeDefinitionTreatment.KindMask) {
			case TypeDefinitionTreatment.NormalType:
				type.Attributes |= TypeAttributes.WindowsRuntime | TypeAttributes.Import;
				break;

			case TypeDefinitionTreatment.NormalAttribute:
				type.Attributes |= TypeAttributes.WindowsRuntime | TypeAttributes.Sealed;
				break;

			case TypeDefinitionTreatment.UnmangleWindowsRuntimeName:
				type.Attributes = type.Attributes & ~TypeAttributes.SpecialName | TypeAttributes.Public;
				type.Name = type.Name.Substring ("<CLR>".Length);
				break;

			case TypeDefinitionTreatment.PrefixWindowsRuntimeName:
				type.Attributes = type.Attributes & ~TypeAttributes.Public | TypeAttributes.Import;
				type.Name = "<WinRT>" + type.Name;
				break;

			case TypeDefinitionTreatment.RedirectToClrType:
				type.Attributes = type.Attributes & ~TypeAttributes.Public | TypeAttributes.Import;
				break;

			case TypeDefinitionTreatment.RedirectToClrAttribute:
				type.Attributes = type.Attributes & ~TypeAttributes.Public;
				break;

			case TypeDefinitionTreatment.RedirectImplementedMethods: {
					type.Attributes |= TypeAttributes.WindowsRuntime | TypeAttributes.Import;

					foreach (var redirectedInterfacePair in projection.RedirectedInterfaces) {
						type.Interfaces.Add (redirectedInterfacePair.Value);

						foreach (var customAttribute in redirectedInterfacePair.Key.CustomAttributes)
							redirectedInterfacePair.Value.CustomAttributes.Add (customAttribute);

						redirectedInterfacePair.Key.CustomAttributes.Clear ();

						foreach (var method in type.Methods) {
							foreach (var @override in method.Overrides) {
								if (TypeReferenceEqualityComparer.AreEqual (@override.DeclaringType, redirectedInterfacePair.Key.InterfaceType)) {
									@override.DeclaringType = redirectedInterfacePair.Value.InterfaceType;
								}
							}
						}
					}

					foreach (var method in projection.RedirectedMethods) {
						type.Methods.Add (method);
					}
				}
				break;
			}

			if ((treatment & TypeDefinitionTreatment.Abstract) != 0)
				type.Attributes |= TypeAttributes.Abstract;

			if ((treatment & TypeDefinitionTreatment.Internal) != 0)
				type.Attributes &= ~TypeAttributes.Public;

			type.WindowsRuntimeProjection = projection;
		}

		public static TypeDefinitionProjection RemoveProjection (TypeDefinition type)
		{
			if (!type.IsWindowsRuntimeProjection)
				return null;

			var projection = type.WindowsRuntimeProjection;
			type.WindowsRuntimeProjection = null;

			type.Attributes = projection.Attributes;
			type.Name = projection.Name;

			if (projection.Treatment == TypeDefinitionTreatment.RedirectImplementedMethods) {
				foreach (var method in projection.RedirectedMethods) {
					type.Methods.Remove (method);
				}

				foreach (var redirectedInterfacePair in projection.RedirectedInterfaces) {
					foreach (var method in type.Methods) {
						foreach (var @override in method.Overrides) {
							if (TypeReferenceEqualityComparer.AreEqual (@override.DeclaringType, redirectedInterfacePair.Value.InterfaceType)) {
								@override.DeclaringType = redirectedInterfacePair.Key.InterfaceType;
							}
						}
					}

					foreach (var customAttribute in redirectedInterfacePair.Value.CustomAttributes)
						redirectedInterfacePair.Key.CustomAttributes.Add (customAttribute);

					redirectedInterfacePair.Value.CustomAttributes.Clear ();
					type.Interfaces.Remove (redirectedInterfacePair.Value);
				}
			}

			return projection;
		}

		public static void Project (TypeReference type)
		{
			TypeReferenceTreatment treatment;

			ProjectionInfo info;
			if (Projections.TryGetValue (type.Name, out info) && info.WinRTNamespace == type.Namespace)
				treatment = TypeReferenceTreatment.UseProjectionInfo;
			else
				treatment = GetSpecialTypeReferenceTreatment (type);

			if (treatment != TypeReferenceTreatment.None)
				ApplyProjection (type, new TypeReferenceProjection (type, treatment));
		}

		static TypeReferenceTreatment GetSpecialTypeReferenceTreatment (TypeReference type)
		{
			if (type.Namespace == "System") {
				if (type.Name == "MulticastDelegate")
					return TypeReferenceTreatment.SystemDelegate;
				if (type.Name == "Attribute")
					return TypeReferenceTreatment.SystemAttribute;
			}

			return TypeReferenceTreatment.None;
		}

		static bool IsAttribute (TypeReference type)
		{
			if (type.MetadataToken.TokenType != TokenType.TypeRef)
				return false;
			return type.Name == "Attribute" && type.Namespace == "System";
		}

		static bool IsEnum (TypeReference type)
		{
			if (type.MetadataToken.TokenType != TokenType.TypeRef)
				return false;
			return type.Name == "Enum" && type.Namespace == "System";
		}

		public static void ApplyProjection (TypeReference type, TypeReferenceProjection projection)
		{
			if (projection == null)
				return;

			switch (projection.Treatment)
			{
				case TypeReferenceTreatment.SystemDelegate:
				case TypeReferenceTreatment.SystemAttribute:
					type.Scope = type.Module.Projections.GetAssemblyReference ("System.Runtime");
					break;

				case TypeReferenceTreatment.UseProjectionInfo:
					var info = Projections [type.Name];
					type.Name = info.ClrName;
					type.Namespace = info.ClrNamespace;
					type.Scope = type.Module.Projections.GetAssemblyReference (info.ClrAssembly);
					break;
			}

			type.WindowsRuntimeProjection = projection;
		}

		public static TypeReferenceProjection RemoveProjection (TypeReference type)
		{
			if (!type.IsWindowsRuntimeProjection)
				return null;

			var projection = type.WindowsRuntimeProjection;
			type.WindowsRuntimeProjection = null;

			type.Name = projection.Name;
			type.Namespace = projection.Namespace;
			type.Scope = projection.Scope;

			return projection;
		}

		public static void Project (MethodDefinition method)
		{
			var treatment = MethodDefinitionTreatment.None;
			var other = false;
			var declaring_type = method.DeclaringType;

			if (declaring_type.IsWindowsRuntime)
			{
				if (IsClrImplementationType (declaring_type))
					treatment = MethodDefinitionTreatment.None;
				else if (declaring_type.IsNested)
					treatment = MethodDefinitionTreatment.None;
				else if (declaring_type.IsInterface)
					treatment = MethodDefinitionTreatment.Runtime | MethodDefinitionTreatment.InternalCall;
				else if (declaring_type.Module.MetadataKind == MetadataKind.ManagedWindowsMetadata && !method.IsPublic)
					treatment = MethodDefinitionTreatment.None;
				else
				{
					other = true;

					var base_type = declaring_type.BaseType;
					if (base_type != null && base_type.MetadataToken.TokenType == TokenType.TypeRef)
					{
						switch (GetSpecialTypeReferenceTreatment(base_type))
						{
							case TypeReferenceTreatment.SystemDelegate:
								treatment = MethodDefinitionTreatment.Runtime | MethodDefinitionTreatment.Public;
								other = false;
								break;

							case TypeReferenceTreatment.SystemAttribute:
								treatment = MethodDefinitionTreatment.Runtime | MethodDefinitionTreatment.InternalCall;
								other = false;
								break;
						}
					}
				}
			}

			if (other)
			{
				var seen_redirected = false;
				var seen_non_redirected = false;

				foreach (var @override in method.Overrides) {
					if (@override.MetadataToken.TokenType == TokenType.MemberRef && ImplementsRedirectedInterface (@override)) {
						seen_redirected = true;
					} else {
						seen_non_redirected = true;
					}
				}

				if (seen_redirected && !seen_non_redirected) {
					treatment = MethodDefinitionTreatment.Runtime | MethodDefinitionTreatment.InternalCall | MethodDefinitionTreatment.Private;
					other = false;
				}
			}

			if (other)
				treatment |= GetMethodDefinitionTreatmentFromCustomAttributes(method);

			if (treatment != MethodDefinitionTreatment.None)
				ApplyProjection (method, new MethodDefinitionProjection (method, treatment));
		}

		static MethodDefinitionTreatment GetMethodDefinitionTreatmentFromCustomAttributes(MethodDefinition method)
		{
			var treatment = MethodDefinitionTreatment.None;

			foreach (var attribute in method.CustomAttributes)
			{
				var type = attribute.AttributeType;
				if (type.Namespace != "Windows.UI.Xaml")
					continue;
				if (type.Name == "TreatAsPublicMethodAttribute")
					treatment |= MethodDefinitionTreatment.Public;
				else if (type.Name == "TreatAsAbstractMethodAttribute")
					treatment |= MethodDefinitionTreatment.Abstract;
			}

			return treatment;
		}

		public static void ApplyProjection (MethodDefinition method, MethodDefinitionProjection projection)
		{
			if (projection == null)
				return;

			var treatment = projection.Treatment;

			if ((treatment & MethodDefinitionTreatment.Abstract) != 0)
				method.Attributes |= MethodAttributes.Abstract;

			if ((treatment & MethodDefinitionTreatment.Private) != 0)
				method.Attributes = (method.Attributes & ~MethodAttributes.MemberAccessMask) | MethodAttributes.Private;

			if ((treatment & MethodDefinitionTreatment.Public) != 0)
				method.Attributes = (method.Attributes & ~MethodAttributes.MemberAccessMask) | MethodAttributes.Public;

			if ((treatment & MethodDefinitionTreatment.Runtime) != 0)
				method.ImplAttributes |= MethodImplAttributes.Runtime;

			if ((treatment & MethodDefinitionTreatment.InternalCall) != 0)
				method.ImplAttributes |= MethodImplAttributes.InternalCall;

			method.WindowsRuntimeProjection = projection;
		}

		public static MethodDefinitionProjection RemoveProjection (MethodDefinition method)
		{
			if (!method.IsWindowsRuntimeProjection)
				return null;

			var projection = method.WindowsRuntimeProjection;
			method.WindowsRuntimeProjection = null;

			method.Attributes = projection.Attributes;
			method.ImplAttributes = projection.ImplAttributes;
			method.Name = projection.Name;

			return projection;
		}

		public static void Project (FieldDefinition field)
		{
			var treatment = FieldDefinitionTreatment.None;
			var declaring_type = field.DeclaringType;

			if (declaring_type.Module.MetadataKind == MetadataKind.WindowsMetadata && field.IsRuntimeSpecialName && field.Name == "value__") {
				var base_type = declaring_type.BaseType;
				if (base_type != null && IsEnum (base_type))
					treatment = FieldDefinitionTreatment.Public;
			}

			if (treatment != FieldDefinitionTreatment.None)
				ApplyProjection (field, new FieldDefinitionProjection (field, treatment));
		}

		public static void ApplyProjection (FieldDefinition field, FieldDefinitionProjection projection)
		{
			if (projection == null)
				return;

			if (projection.Treatment == FieldDefinitionTreatment.Public)
				field.Attributes = (field.Attributes & ~FieldAttributes.FieldAccessMask) | FieldAttributes.Public;

			field.WindowsRuntimeProjection = projection;
		}

		public static FieldDefinitionProjection RemoveProjection (FieldDefinition field)
		{
			if (!field.IsWindowsRuntimeProjection)
				return null;

			var projection = field.WindowsRuntimeProjection;
			field.WindowsRuntimeProjection = null;

			field.Attributes = projection.Attributes;

			return projection;
		}

		static bool ImplementsRedirectedInterface (MemberReference member)
		{
			var declaring_type = member.DeclaringType;
			TypeReference type;
			switch (declaring_type.MetadataToken.TokenType) {
				case TokenType.TypeRef:
					type = declaring_type;
					break;

				case TokenType.TypeSpec:
					if (!declaring_type.IsGenericInstance)
						return false;

					type = ((TypeSpecification) declaring_type).ElementType;
					if (type.MetadataType != MetadataType.Class || type.MetadataToken.TokenType != TokenType.TypeRef)
						return false;

					break;

				default:
					return false;
			}

			var projection = RemoveProjection (type);

			var found = false;

			ProjectionInfo info;
			if (Projections.TryGetValue (type.Name, out info) && type.Namespace == info.WinRTNamespace) {
				found = true;
			}

			ApplyProjection (type, projection);

			return found;
		}


		public void AddVirtualReferences (Collection<AssemblyNameReference> references)
		{
			var corlib = GetCoreLibrary (references);
			corlib_version = corlib.Version;
			corlib.Version = version;

			if (virtual_references == null) {
				var winrt_references = GetAssemblyReferences (corlib);
				Interlocked.CompareExchange (ref virtual_references, winrt_references, null);
			}

			foreach (var reference in virtual_references)
				references.Add (reference);
		}

		public void RemoveVirtualReferences (Collection<AssemblyNameReference> references)
		{
			var corlib = GetCoreLibrary (references);
			corlib.Version = corlib_version;

			foreach (var reference in VirtualReferences)
				references.Remove (reference);
		}

		static AssemblyNameReference[] GetAssemblyReferences (AssemblyNameReference corlib)
		{
			var system_runtime = new AssemblyNameReference ("System.Runtime", version);
			var system_runtime_interopservices_windowsruntime = new AssemblyNameReference ("System.Runtime.InteropServices.WindowsRuntime", version);
			var system_objectmodel = new AssemblyNameReference ("System.ObjectModel", version);
			var system_runtime_windowsruntime = new AssemblyNameReference ("System.Runtime.WindowsRuntime", version);
			var system_runtime_windowsruntime_ui_xaml = new AssemblyNameReference ("System.Runtime.WindowsRuntime.UI.Xaml", version);
			var system_numerics_vectors = new AssemblyNameReference ("System.Numerics.Vectors", version);

			if (corlib.HasPublicKey) {
				system_runtime_windowsruntime.PublicKey =
				system_runtime_windowsruntime_ui_xaml.PublicKey = corlib.PublicKey;

				system_runtime.PublicKey =
				system_runtime_interopservices_windowsruntime.PublicKey =
				system_objectmodel.PublicKey =
				system_numerics_vectors.PublicKey = contract_pk;
			}
			else {
				system_runtime_windowsruntime.PublicKeyToken =
				system_runtime_windowsruntime_ui_xaml.PublicKeyToken = corlib.PublicKeyToken;

				system_runtime.PublicKeyToken =
				system_runtime_interopservices_windowsruntime.PublicKeyToken =
				system_objectmodel.PublicKeyToken =
				system_numerics_vectors.PublicKeyToken = contract_pk_token;
			}

			return new[] {
				system_runtime,
				system_runtime_interopservices_windowsruntime,
				system_objectmodel,
				system_runtime_windowsruntime,
				system_runtime_windowsruntime_ui_xaml,
				system_numerics_vectors,
			};
		}

		static AssemblyNameReference GetCoreLibrary (Collection<AssemblyNameReference> references)
		{
			foreach (var reference in references)
				if (reference.Name == "mscorlib")
					return reference;

			throw new BadImageFormatException ("Missing mscorlib reference in AssemblyRef table.");
		}

		AssemblyNameReference GetAssemblyReference (string name)
		{
			foreach (var assembly in VirtualReferences)
				if (assembly.Name == name)
					return assembly;

			throw new Exception ();
		}

		public static void Project (ICustomAttributeProvider owner, CustomAttribute attribute)
		{
			if (!IsWindowsAttributeUsageAttribute (owner, attribute))
				return;

			var treatment = CustomAttributeValueTreatment.None;
			var type = (TypeDefinition) owner;

			if (type.Namespace == "Windows.Foundation.Metadata") {
				if (type.Name == "VersionAttribute")
					treatment = CustomAttributeValueTreatment.VersionAttribute;
				else if (type.Name == "DeprecatedAttribute")
					treatment = CustomAttributeValueTreatment.DeprecatedAttribute;
			}

			if (treatment == CustomAttributeValueTreatment.None) {
				var multiple = HasAttribute (type, "Windows.Foundation.Metadata", "AllowMultipleAttribute");
				treatment = multiple ? CustomAttributeValueTreatment.AllowMultiple : CustomAttributeValueTreatment.AllowSingle;
			}

			if (treatment != CustomAttributeValueTreatment.None) {
				var attribute_targets = (AttributeTargets) attribute.ConstructorArguments [0].Value;
				ApplyProjection (attribute, new CustomAttributeValueProjection (attribute_targets, treatment));
			}
		}

		static bool IsWindowsAttributeUsageAttribute (ICustomAttributeProvider owner, CustomAttribute attribute)
		{
			if (owner.MetadataToken.TokenType != TokenType.TypeDef)
				return false;

			var constructor = attribute.Constructor;

			if (constructor.MetadataToken.TokenType != TokenType.MemberRef)
				return false;

			var declaring_type = constructor.DeclaringType;

			if (declaring_type.MetadataToken.TokenType != TokenType.TypeRef)
				return false;

			// declaring type is already projected
			return declaring_type.Name == "AttributeUsageAttribute" && declaring_type.Namespace == /*"Windows.Foundation.Metadata"*/"System";
		}

		static bool HasAttribute (TypeDefinition type, string @namespace, string name)
		{
			foreach (var attribute in type.CustomAttributes) {
				var attribute_type = attribute.AttributeType;
				if (attribute_type.Name == name && attribute_type.Namespace == @namespace)
					return true;
			}
			return false;
		}

		public static void ApplyProjection (CustomAttribute attribute, CustomAttributeValueProjection projection)
		{
			if (projection == null)
				return;

			bool version_or_deprecated;
			bool multiple;

			switch (projection.Treatment) {
			case CustomAttributeValueTreatment.AllowSingle:
				version_or_deprecated = false;
				multiple = false;
				break;

			case CustomAttributeValueTreatment.AllowMultiple:
				version_or_deprecated = false;
				multiple = true;
				break;

			case CustomAttributeValueTreatment.VersionAttribute:
			case CustomAttributeValueTreatment.DeprecatedAttribute:
				version_or_deprecated = true;
				multiple = true;
				break;

			default:
				throw new ArgumentException ();
			}

			var attribute_targets = (AttributeTargets) attribute.ConstructorArguments [0].Value;
			if (version_or_deprecated)
				attribute_targets |= AttributeTargets.Constructor | AttributeTargets.Property;
			attribute.ConstructorArguments [0] = new CustomAttributeArgument (attribute.ConstructorArguments [0].Type, attribute_targets);

			attribute.Properties.Add (new CustomAttributeNamedArgument ("AllowMultiple", new CustomAttributeArgument (attribute.Module.TypeSystem.Boolean, multiple)));

			attribute.projection = projection;
		}

		public static CustomAttributeValueProjection RemoveProjection (CustomAttribute attribute)
		{
			if (attribute.projection == null)
				return null;

			var projection = attribute.projection;
			attribute.projection = null;

			attribute.ConstructorArguments [0] = new CustomAttributeArgument (attribute.ConstructorArguments [0].Type, projection.Targets);
			attribute.Properties.Clear ();

			return projection;
		}
	}
}
