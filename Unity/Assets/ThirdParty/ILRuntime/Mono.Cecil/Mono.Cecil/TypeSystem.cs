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

using ILRuntime.Mono.Cecil.Metadata;

namespace ILRuntime.Mono.Cecil {

	public abstract class TypeSystem {

		sealed class CoreTypeSystem : TypeSystem {

			public CoreTypeSystem (ModuleDefinition module)
				: base (module)
			{
			}

			internal override TypeReference LookupType (string @namespace, string name)
			{
				var type = LookupTypeDefinition (@namespace, name) ?? LookupTypeForwarded (@namespace, name);
				if (type != null)
					return type;

				throw new NotSupportedException ();
			}

			TypeReference LookupTypeDefinition (string @namespace, string name)
			{
				var metadata = module.MetadataSystem;
				if (metadata.Types == null)
					Initialize (module.Types);

				return module.Read (new Row<string, string> (@namespace, name), (row, reader) => {
					var types = reader.metadata.Types;

					for (int i = 0; i < types.Length; i++) {
						if (types [i] == null)
							types [i] = reader.GetTypeDefinition ((uint) i + 1);

						var type = types [i];

						if (type.Name == row.Col2 && type.Namespace == row.Col1)
							return type;
					}

					return null;
				});
			}

			TypeReference LookupTypeForwarded (string @namespace, string name)
			{
				if (!module.HasExportedTypes)
					return null;

				var exported_types = module.ExportedTypes;
				for (int i = 0; i < exported_types.Count; i++) {
					var exported_type = exported_types [i];

					if (exported_type.Name == name && exported_type.Namespace == @namespace)
						return exported_type.CreateReference ();
				}

				return null;
			}

			static void Initialize (object obj)
			{
			}
		}

		sealed class CommonTypeSystem : TypeSystem {

			AssemblyNameReference core_library;

			public CommonTypeSystem (ModuleDefinition module)
				: base (module)
			{
			}

			internal override TypeReference LookupType (string @namespace, string name)
			{
				return CreateTypeReference (@namespace, name);
			}

			public AssemblyNameReference GetCoreLibraryReference ()
			{
				if (core_library != null)
					return core_library;

				if (module.TryGetCoreLibraryReference (out core_library))
					return core_library;

				core_library = new AssemblyNameReference {
					Name = Mixin.mscorlib,
					Version = GetCorlibVersion (),
					PublicKeyToken = new byte [] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 },
				};

				module.AssemblyReferences.Add (core_library);

				return core_library;
			}

			Version GetCorlibVersion ()
			{
				switch (module.Runtime) {
				case TargetRuntime.Net_1_0:
				case TargetRuntime.Net_1_1:
					return new Version (1, 0, 0, 0);
				case TargetRuntime.Net_2_0:
					return new Version (2, 0, 0, 0);
				case TargetRuntime.Net_4_0:
					return new Version (4, 0, 0, 0);
				default:
					throw new NotSupportedException ();
				}
			}

			TypeReference CreateTypeReference (string @namespace, string name)
			{
				return new TypeReference (@namespace, name, module, GetCoreLibraryReference ());
			}
		}

		readonly ModuleDefinition module;

		TypeReference type_object;
		TypeReference type_void;
		TypeReference type_bool;
		TypeReference type_char;
		TypeReference type_sbyte;
		TypeReference type_byte;
		TypeReference type_int16;
		TypeReference type_uint16;
		TypeReference type_int32;
		TypeReference type_uint32;
		TypeReference type_int64;
		TypeReference type_uint64;
		TypeReference type_single;
		TypeReference type_double;
		TypeReference type_intptr;
		TypeReference type_uintptr;
		TypeReference type_string;
		TypeReference type_typedref;

		TypeSystem (ModuleDefinition module)
		{
			this.module = module;
		}

		internal static TypeSystem CreateTypeSystem (ModuleDefinition module)
		{
			if (module.IsCoreLibrary ())
				return new CoreTypeSystem (module);

			return new CommonTypeSystem (module);
		}

		internal abstract TypeReference LookupType (string @namespace, string name);

		TypeReference LookupSystemType (ref TypeReference reference, string name, ElementType element_type)
		{
			lock (module.SyncRoot) {
				if (reference != null)
					return reference;
				var type = LookupType ("System", name);
				type.etype = element_type;
				return reference = type;
			}
		}

		TypeReference LookupSystemValueType (ref TypeReference typeRef, string name, ElementType element_type)
		{
			lock (module.SyncRoot) {
				if (typeRef != null)
					return typeRef;
				var type = LookupType ("System", name);
				type.etype = element_type;
				type.KnownValueType ();
				return typeRef = type;
			}
		}

		[Obsolete ("Use CoreLibrary")]
		public IMetadataScope Corlib {
			get { return CoreLibrary; }
		}

		public IMetadataScope CoreLibrary {
			get {
				var common = this as CommonTypeSystem;
				if (common == null)
					return module;

				return common.GetCoreLibraryReference ();
			}
		}

		public TypeReference Object {
			get { return type_object ?? (LookupSystemType (ref type_object, "Object", ElementType.Object)); }
		}

		public TypeReference Void {
			get { return type_void ?? (LookupSystemType (ref type_void, "Void", ElementType.Void)); }
		}

		public TypeReference Boolean {
			get { return type_bool ?? (LookupSystemValueType (ref type_bool, "Boolean", ElementType.Boolean)); }
		}

		public TypeReference Char {
			get { return type_char ?? (LookupSystemValueType (ref type_char, "Char", ElementType.Char)); }
		}

		public TypeReference SByte {
			get { return type_sbyte ?? (LookupSystemValueType (ref type_sbyte, "SByte", ElementType.I1)); }
		}

		public TypeReference Byte {
			get { return type_byte ?? (LookupSystemValueType (ref type_byte, "Byte", ElementType.U1)); }
		}

		public TypeReference Int16 {
			get { return type_int16 ?? (LookupSystemValueType (ref type_int16, "Int16", ElementType.I2)); }
		}

		public TypeReference UInt16 {
			get { return type_uint16 ?? (LookupSystemValueType (ref type_uint16, "UInt16", ElementType.U2)); }
		}

		public TypeReference Int32 {
			get { return type_int32 ?? (LookupSystemValueType (ref type_int32, "Int32", ElementType.I4)); }
		}

		public TypeReference UInt32 {
			get { return type_uint32 ?? (LookupSystemValueType (ref type_uint32, "UInt32", ElementType.U4)); }
		}

		public TypeReference Int64 {
			get { return type_int64 ?? (LookupSystemValueType (ref type_int64, "Int64", ElementType.I8)); }
		}

		public TypeReference UInt64 {
			get { return type_uint64 ?? (LookupSystemValueType (ref type_uint64, "UInt64", ElementType.U8)); }
		}

		public TypeReference Single {
			get { return type_single ?? (LookupSystemValueType (ref type_single, "Single", ElementType.R4)); }
		}

		public TypeReference Double {
			get { return type_double ?? (LookupSystemValueType (ref type_double, "Double", ElementType.R8)); }
		}

		public TypeReference IntPtr {
			get { return type_intptr ?? (LookupSystemValueType (ref type_intptr, "IntPtr", ElementType.I)); }
		}

		public TypeReference UIntPtr {
			get { return type_uintptr ?? (LookupSystemValueType (ref type_uintptr, "UIntPtr", ElementType.U)); }
		}

		public TypeReference String {
			get { return type_string ?? (LookupSystemType (ref type_string, "String", ElementType.String)); }
		}

		public TypeReference TypedReference {
			get { return type_typedref ?? (LookupSystemValueType (ref type_typedref, "TypedReference", ElementType.TypedByRef)); }
		}
	}

	static partial class Mixin {

		public const string mscorlib = "mscorlib";
		public const string system_runtime = "System.Runtime";
		public const string system_private_corelib = "System.Private.CoreLib";
		public const string netstandard = "netstandard";

		public static bool TryGetCoreLibraryReference (this ModuleDefinition module, out AssemblyNameReference reference)
		{
			var references = module.AssemblyReferences;

			for (int i = 0; i < references.Count; i++) {
				reference = references [i];
				if (IsCoreLibrary (reference))
					return true;
			}

			reference = null;
			return false;

		}

		public static bool IsCoreLibrary (this ModuleDefinition module)
		{
			if (module.Assembly == null)
				return false;

			if (!IsCoreLibrary (module.Assembly.Name))
				return false;

			if (module.HasImage && module.Read (module, (m, reader) => reader.image.GetTableLength (Table.AssemblyRef) > 0))
				return false;

			return true;
		}

		public static void KnownValueType (this TypeReference type)
		{
			if (!type.IsDefinition)
				type.IsValueType = true;
		}

		static bool IsCoreLibrary (AssemblyNameReference reference)
		{
			var name = reference.Name;
			return name == mscorlib
				|| name == system_runtime
				|| name == system_private_corelib
				|| name == netstandard;
		}
	}
}
