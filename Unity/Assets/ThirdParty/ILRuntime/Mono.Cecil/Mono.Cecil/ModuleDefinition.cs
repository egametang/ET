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
using System.IO;
using System.Threading;
using SR = System.Reflection;

using ILRuntime.Mono.Cecil.Cil;
using ILRuntime.Mono.Cecil.Metadata;
using ILRuntime.Mono.Cecil.PE;
using ILRuntime.Mono.Collections.Generic;

namespace ILRuntime.Mono.Cecil {

	public enum ReadingMode {
		Immediate = 1,
		Deferred = 2,
	}

	public sealed class ReaderParameters {

		ReadingMode reading_mode;
		internal IAssemblyResolver assembly_resolver;
		internal IMetadataResolver metadata_resolver;
		internal IMetadataImporterProvider metadata_importer_provider;
		internal IReflectionImporterProvider reflection_importer_provider;
		Stream symbol_stream;
		ISymbolReaderProvider symbol_reader_provider;
		bool read_symbols;
		bool throw_symbols_mismatch;
		bool projections;
		bool in_memory;
		bool read_write;

		public ReadingMode ReadingMode {
			get { return reading_mode; }
			set { reading_mode = value; }
		}

		public bool InMemory {
			get { return in_memory; }
			set { in_memory = value; }
		}

		public IAssemblyResolver AssemblyResolver {
			get { return assembly_resolver; }
			set { assembly_resolver = value; }
		}

		public IMetadataResolver MetadataResolver {
			get { return metadata_resolver; }
			set { metadata_resolver = value; }
		}

		public IMetadataImporterProvider MetadataImporterProvider {
			get { return metadata_importer_provider; }
			set { metadata_importer_provider = value; }
		}

		public IReflectionImporterProvider ReflectionImporterProvider {
			get { return reflection_importer_provider; }
			set { reflection_importer_provider = value; }
		}

		public Stream SymbolStream {
			get { return symbol_stream; }
			set { symbol_stream = value; }
		}

		public ISymbolReaderProvider SymbolReaderProvider {
			get { return symbol_reader_provider; }
			set { symbol_reader_provider = value; }
		}

		public bool ReadSymbols {
			get { return read_symbols; }
			set { read_symbols = value; }
		}

		public bool ThrowIfSymbolsAreNotMatching {
			get { return throw_symbols_mismatch; }
			set { throw_symbols_mismatch = value; }
		}

		public bool ReadWrite {
			get { return read_write; }
			set { read_write = value; }
		}

		public bool ApplyWindowsRuntimeProjections {
			get { return projections; }
			set { projections = value; }
		}

		public ReaderParameters ()
			: this (ReadingMode.Deferred)
		{
		}

		public ReaderParameters (ReadingMode readingMode)
		{
			this.reading_mode = readingMode;
			this.throw_symbols_mismatch = true;
		}
	}

	public sealed class ModuleParameters {

		ModuleKind kind;
		TargetRuntime runtime;
		uint? timestamp;
		TargetArchitecture architecture;
		IAssemblyResolver assembly_resolver;
		IMetadataResolver metadata_resolver;
		IMetadataImporterProvider metadata_importer_provider;
		IReflectionImporterProvider reflection_importer_provider;

		public ModuleKind Kind {
			get { return kind; }
			set { kind = value; }
		}

		public TargetRuntime Runtime {
			get { return runtime; }
			set { runtime = value; }
		}

		public uint? Timestamp {
			get { return timestamp; }
			set { timestamp = value; }
		}

		public TargetArchitecture Architecture {
			get { return architecture; }
			set { architecture = value; }
		}

		public IAssemblyResolver AssemblyResolver {
			get { return assembly_resolver; }
			set { assembly_resolver = value; }
		}

		public IMetadataResolver MetadataResolver {
			get { return metadata_resolver; }
			set { metadata_resolver = value; }
		}

		public IMetadataImporterProvider MetadataImporterProvider {
			get { return metadata_importer_provider; }
			set { metadata_importer_provider = value; }
		}

		public IReflectionImporterProvider ReflectionImporterProvider {
			get { return reflection_importer_provider; }
			set { reflection_importer_provider = value; }
		}

		public ModuleParameters ()
		{
			this.kind = ModuleKind.Dll;
			this.Runtime = GetCurrentRuntime ();
			this.architecture = TargetArchitecture.I386;
		}

		static TargetRuntime GetCurrentRuntime ()
		{
			return typeof (object).Assembly.ImageRuntimeVersion.ParseRuntime ();
		}
	}

	public sealed class WriterParameters {

		uint? timestamp;
		Stream symbol_stream;
		ISymbolWriterProvider symbol_writer_provider;
		bool write_symbols;
		byte [] key_blob;
		string key_container;
		SR.StrongNameKeyPair key_pair;

		public uint? Timestamp {
			get { return timestamp; }
			set { timestamp = value; }
		}

		public Stream SymbolStream {
			get { return symbol_stream; }
			set { symbol_stream = value; }
		}

		public ISymbolWriterProvider SymbolWriterProvider {
			get { return symbol_writer_provider; }
			set { symbol_writer_provider = value; }
		}

		public bool WriteSymbols {
			get { return write_symbols; }
			set { write_symbols = value; }
		}

		public bool HasStrongNameKey {
			get { return key_pair != null || key_blob != null || key_container != null; }
		}

		public byte [] StrongNameKeyBlob {
			get { return key_blob; }
			set { key_blob = value; }
		}

		public string StrongNameKeyContainer {
			get { return key_container; }
			set { key_container = value; }
		}

		public SR.StrongNameKeyPair StrongNameKeyPair {
			get { return key_pair; }
			set { key_pair = value; }
		}

		public bool DeterministicMvid { get; set; }
	}

	public sealed class ModuleDefinition : ModuleReference, ICustomAttributeProvider, ICustomDebugInformationProvider, IDisposable {

		internal Image Image;
		internal MetadataSystem MetadataSystem;
		internal ReadingMode ReadingMode;
		internal ISymbolReaderProvider SymbolReaderProvider;

		internal ISymbolReader symbol_reader;
		internal Disposable<IAssemblyResolver> assembly_resolver;
		internal IMetadataResolver metadata_resolver;
		internal TypeSystem type_system;
		internal readonly MetadataReader reader;
		readonly string file_name;

		internal string runtime_version;
		internal ModuleKind kind;
		WindowsRuntimeProjections projections;
		MetadataKind metadata_kind;
		TargetRuntime runtime;
		TargetArchitecture architecture;
		ModuleAttributes attributes;
		ModuleCharacteristics characteristics;
		Guid mvid;

		internal ushort linker_version = 8;
		internal ushort subsystem_major = 4;
		internal ushort subsystem_minor = 0;
		internal uint timestamp;

		internal AssemblyDefinition assembly;
		MethodDefinition entry_point;

		internal IReflectionImporter reflection_importer;
		internal IMetadataImporter metadata_importer;

		Collection<CustomAttribute> custom_attributes;
		Collection<AssemblyNameReference> references;
		Collection<ModuleReference> modules;
		Collection<Resource> resources;
		Collection<ExportedType> exported_types;
		TypeDefinitionCollection types;

		internal Collection<CustomDebugInformation> custom_infos;

		internal MetadataBuilder metadata_builder;

		public bool IsMain {
			get { return kind != ModuleKind.NetModule; }
		}

		public ModuleKind Kind {
			get { return kind; }
			set { kind = value; }
		}

		public MetadataKind MetadataKind {
			get { return metadata_kind; }
			set { metadata_kind = value; }
		}

		internal WindowsRuntimeProjections Projections {
			get {
				if (projections == null)
					Interlocked.CompareExchange (ref projections, new WindowsRuntimeProjections (this), null);

				return projections;
			}
		}

		public TargetRuntime Runtime {
			get { return runtime; }
			set {
				runtime = value;
				runtime_version = runtime.RuntimeVersionString ();
			}
		}

		public string RuntimeVersion {
			get { return runtime_version; }
			set {
				runtime_version = value;
				runtime = runtime_version.ParseRuntime ();
			}
		}

		public TargetArchitecture Architecture {
			get { return architecture; }
			set { architecture = value; }
		}

		public ModuleAttributes Attributes {
			get { return attributes; }
			set { attributes = value; }
		}

		public ModuleCharacteristics Characteristics {
			get { return characteristics; }
			set { characteristics = value; }
		}

		[Obsolete ("Use FileName")]
		public string FullyQualifiedName {
			get { return file_name; }
		}

		public string FileName {
			get { return file_name; }
		}

		public Guid Mvid {
			get { return mvid; }
			set { mvid = value; }
		}

		internal bool HasImage {
			get { return Image != null; }
		}

		public bool HasSymbols {
			get { return symbol_reader != null; }
		}

		public ISymbolReader SymbolReader {
			get { return symbol_reader; }
		}

		public override MetadataScopeType MetadataScopeType {
			get { return MetadataScopeType.ModuleDefinition; }
		}

		public AssemblyDefinition Assembly {
			get { return assembly; }
		}

		internal IReflectionImporter ReflectionImporter {
			get {
				if (reflection_importer == null)
					Interlocked.CompareExchange (ref reflection_importer, new DefaultReflectionImporter (this), null);

				return reflection_importer;
			}
		}

		internal IMetadataImporter MetadataImporter {
			get {
				if (metadata_importer == null)
					Interlocked.CompareExchange (ref metadata_importer, new DefaultMetadataImporter (this), null);

				return metadata_importer;
			}
		}

		public IAssemblyResolver AssemblyResolver {
			get {
				if (assembly_resolver.value == null) {
					lock (module_lock) {
						assembly_resolver = Disposable.Owned (new DefaultAssemblyResolver () as IAssemblyResolver);
					}
				}

				return assembly_resolver.value;
			}
		}

		public IMetadataResolver MetadataResolver {
			get {
				if (metadata_resolver == null)
					Interlocked.CompareExchange (ref metadata_resolver, new MetadataResolver (this.AssemblyResolver), null);

				return metadata_resolver;
			}
		}

		public TypeSystem TypeSystem {
			get {
				if (type_system == null)
					Interlocked.CompareExchange (ref type_system, TypeSystem.CreateTypeSystem (this), null);

				return type_system;
			}
		}

		public bool HasAssemblyReferences {
			get {
				if (references != null)
					return references.Count > 0;

				return HasImage && Image.HasTable (Table.AssemblyRef);
			}
		}

		public Collection<AssemblyNameReference> AssemblyReferences {
			get {
				if (references != null)
					return references;

				if (HasImage)
					return Read (ref references, this, (_, reader) => reader.ReadAssemblyReferences ());

				Interlocked.CompareExchange (ref references, new Collection<AssemblyNameReference> (), null);
				return references;
			}
		}

		public bool HasModuleReferences {
			get {
				if (modules != null)
					return modules.Count > 0;

				return HasImage && Image.HasTable (Table.ModuleRef);
			}
		}

		public Collection<ModuleReference> ModuleReferences {
			get {
				if (modules != null)
					return modules;

				if (HasImage)
					return Read (ref modules, this, (_, reader) => reader.ReadModuleReferences ());

				Interlocked.CompareExchange (ref modules, new Collection<ModuleReference> (), null);
				return modules;
			}
		}

		public bool HasResources {
			get {
				if (resources != null)
					return resources.Count > 0;

				if (HasImage)
					return Image.HasTable (Table.ManifestResource) || Read (this, (_, reader) => reader.HasFileResource ());

				return false;
			}
		}

		public Collection<Resource> Resources {
			get {
				if (resources != null)
					return resources;

				if (HasImage)
					return Read (ref resources, this, (_, reader) => reader.ReadResources ());

				Interlocked.CompareExchange (ref resources, new Collection<Resource> (), null);
				return resources;
			}
		}

		public bool HasCustomAttributes {
			get {
				if (custom_attributes != null)
					return custom_attributes.Count > 0;

				return this.GetHasCustomAttributes (this);
			}
		}

		public Collection<CustomAttribute> CustomAttributes {
			get { return custom_attributes ?? (this.GetCustomAttributes (ref custom_attributes, this)); }
		}

		public bool HasTypes {
			get {
				if (types != null)
					return types.Count > 0;

				return HasImage && Image.HasTable (Table.TypeDef);
			}
		}

		public Collection<TypeDefinition> Types {
			get {
				if (types != null)
					return types;

				if (HasImage)
					return Read (ref types, this, (_, reader) => reader.ReadTypes ());

				Interlocked.CompareExchange (ref types, new TypeDefinitionCollection (this), null);
				return types;
			}
		}

		public bool HasExportedTypes {
			get {
				if (exported_types != null)
					return exported_types.Count > 0;

				return HasImage && Image.HasTable (Table.ExportedType);
			}
		}

		public Collection<ExportedType> ExportedTypes {
			get {
				if (exported_types != null)
					return exported_types;

				if (HasImage)
					return Read (ref exported_types, this, (_, reader) => reader.ReadExportedTypes ());

				Interlocked.CompareExchange (ref exported_types, new Collection<ExportedType> (), null);
				return exported_types;
			}
		}

		public MethodDefinition EntryPoint {
			get {
				if (entry_point != null)
					return entry_point;

				if (HasImage)
					return Read (ref entry_point, this, (_, reader) => reader.ReadEntryPoint ());

				return entry_point = null;
			}
			set { entry_point = value; }
		}

		public bool HasCustomDebugInformations {
			get {
				return custom_infos != null && custom_infos.Count > 0;
			}
		}

		public Collection<CustomDebugInformation> CustomDebugInformations {
			get {
				if (custom_infos == null)
					Interlocked.CompareExchange (ref custom_infos, new Collection<CustomDebugInformation> (), null);

				return custom_infos;
			}
		}

		internal ModuleDefinition ()
		{
			this.MetadataSystem = new MetadataSystem ();
			this.token = new MetadataToken (TokenType.Module, 1);
		}

		internal ModuleDefinition (Image image)
			: this ()
		{
			this.Image = image;
			this.kind = image.Kind;
			this.RuntimeVersion = image.RuntimeVersion;
			this.architecture = image.Architecture;
			this.attributes = image.Attributes;
			this.characteristics = image.Characteristics;
			this.linker_version = image.LinkerVersion;
			this.subsystem_major = image.SubSystemMajor;
			this.subsystem_minor = image.SubSystemMinor;
			this.file_name = image.FileName;
			this.timestamp = image.Timestamp;

			this.reader = new MetadataReader (this);
		}

		public void Dispose ()
		{
			if (Image != null)
				Image.Dispose ();

			if (symbol_reader != null)
				symbol_reader.Dispose ();

			if (assembly_resolver.value != null)
				assembly_resolver.Dispose ();
		}

		public bool HasTypeReference (string fullName)
		{
			return HasTypeReference (string.Empty, fullName);
		}

		public bool HasTypeReference (string scope, string fullName)
		{
			Mixin.CheckFullName (fullName);

			if (!HasImage)
				return false;

			return GetTypeReference (scope, fullName) != null;
		}

		public bool TryGetTypeReference (string fullName, out TypeReference type)
		{
			return TryGetTypeReference (string.Empty, fullName, out type);
		}

		public bool TryGetTypeReference (string scope, string fullName, out TypeReference type)
		{
			Mixin.CheckFullName (fullName);

			if (!HasImage) {
				type = null;
				return false;
			}

			return (type = GetTypeReference (scope, fullName)) != null;
		}

		TypeReference GetTypeReference (string scope, string fullname)
		{
			return Read (new Row<string, string> (scope, fullname), (row, reader) => reader.GetTypeReference (row.Col1, row.Col2));
		}

		public IEnumerable<TypeReference> GetTypeReferences ()
		{
			if (!HasImage)
				return Empty<TypeReference>.Array;

			return Read (this, (_, reader) => reader.GetTypeReferences ());
		}

		public IEnumerable<MemberReference> GetMemberReferences ()
		{
			if (!HasImage)
				return Empty<MemberReference>.Array;

			return Read (this, (_, reader) => reader.GetMemberReferences ());
		}

		public IEnumerable<CustomAttribute> GetCustomAttributes ()
		{
			if (!HasImage)
				return Empty<CustomAttribute>.Array;

			return Read (this, (_, reader) => reader.GetCustomAttributes ());
		}

		public TypeReference GetType (string fullName, bool runtimeName)
		{
			return runtimeName
				? TypeParser.ParseType (this, fullName, typeDefinitionOnly: true)
				: GetType (fullName);
		}

		public TypeDefinition GetType (string fullName)
		{
			Mixin.CheckFullName (fullName);

			var position = fullName.IndexOf ('/');
			if (position > 0)
				return GetNestedType (fullName);

			return ((TypeDefinitionCollection) this.Types).GetType (fullName);
		}

		public TypeDefinition GetType (string @namespace, string name)
		{
			Mixin.CheckName (name);

			return ((TypeDefinitionCollection) this.Types).GetType (@namespace ?? string.Empty, name);
		}

		public IEnumerable<TypeDefinition> GetTypes ()
		{
			return GetTypes (Types);
		}

		static IEnumerable<TypeDefinition> GetTypes (Collection<TypeDefinition> types)
		{
			for (int i = 0; i < types.Count; i++) {
				var type = types [i];

				yield return type;

				if (!type.HasNestedTypes)
					continue;

				foreach (var nested in GetTypes (type.NestedTypes))
					yield return nested;
			}
		}

		TypeDefinition GetNestedType (string fullname)
		{
			var names = fullname.Split ('/');
			var type = GetType (names [0]);

			if (type == null)
				return null;

			for (int i = 1; i < names.Length; i++) {
				var nested_type = type.GetNestedType (names [i]);
				if (nested_type == null)
					return null;

				type = nested_type;
			}

			return type;
		}

		internal FieldDefinition Resolve (FieldReference field)
		{
			return MetadataResolver.Resolve (field);
		}

		internal MethodDefinition Resolve (MethodReference method)
		{
			return MetadataResolver.Resolve (method);
		}

		internal TypeDefinition Resolve (TypeReference type)
		{
			return MetadataResolver.Resolve (type);
		}

		static void CheckContext (IGenericParameterProvider context, ModuleDefinition module)
		{
			if (context == null)
				return;

			if (context.Module != module)
				throw new ArgumentException ();
		}

		[Obsolete ("Use ImportReference", error: false)]
		public TypeReference Import (Type type)
		{
			return ImportReference (type, null);
		}

		public TypeReference ImportReference (Type type)
		{
			return ImportReference (type, null);
		}

		[Obsolete ("Use ImportReference", error: false)]
		public TypeReference Import (Type type, IGenericParameterProvider context)
		{
			return ImportReference (type, context);
		}

		public TypeReference ImportReference (Type type, IGenericParameterProvider context)
		{
			Mixin.CheckType (type);
			CheckContext (context, this);

			return ReflectionImporter.ImportReference (type, context);
		}

		[Obsolete ("Use ImportReference", error: false)]
		public FieldReference Import (SR.FieldInfo field)
		{
			return ImportReference (field, null);
		}

		[Obsolete ("Use ImportReference", error: false)]
		public FieldReference Import (SR.FieldInfo field, IGenericParameterProvider context)
		{
			return ImportReference (field, context);
		}

		public FieldReference ImportReference (SR.FieldInfo field)
		{
			return ImportReference (field, null);
		}

		public FieldReference ImportReference (SR.FieldInfo field, IGenericParameterProvider context)
		{
			Mixin.CheckField (field);
			CheckContext (context, this);

			return ReflectionImporter.ImportReference (field, context);
		}

		[Obsolete ("Use ImportReference", error: false)]
		public MethodReference Import (SR.MethodBase method)
		{
			return ImportReference (method, null);
		}

		[Obsolete ("Use ImportReference", error: false)]
		public MethodReference Import (SR.MethodBase method, IGenericParameterProvider context)
		{
			return ImportReference (method, context);
		}

		public MethodReference ImportReference (SR.MethodBase method)
		{
			return ImportReference (method, null);
		}

		public MethodReference ImportReference (SR.MethodBase method, IGenericParameterProvider context)
		{
			Mixin.CheckMethod (method);
			CheckContext (context, this);

			return ReflectionImporter.ImportReference (method, context);
		}

		[Obsolete ("Use ImportReference", error: false)]
		public TypeReference Import (TypeReference type)
		{
			return ImportReference (type, null);
		}

		[Obsolete ("Use ImportReference", error: false)]
		public TypeReference Import (TypeReference type, IGenericParameterProvider context)
		{
			return ImportReference (type, context);
		}

		public TypeReference ImportReference (TypeReference type)
		{
			return ImportReference (type, null);
		}

		public TypeReference ImportReference (TypeReference type, IGenericParameterProvider context)
		{
			Mixin.CheckType (type);

			if (type.Module == this)
				return type;

			CheckContext (context, this);

			return MetadataImporter.ImportReference (type, context);
		}

		[Obsolete ("Use ImportReference", error: false)]
		public FieldReference Import (FieldReference field)
		{
			return ImportReference (field, null);
		}

		[Obsolete ("Use ImportReference", error: false)]
		public FieldReference Import (FieldReference field, IGenericParameterProvider context)
		{
			return ImportReference (field, context);
		}

		public FieldReference ImportReference (FieldReference field)
		{
			return ImportReference (field, null);
		}

		public FieldReference ImportReference (FieldReference field, IGenericParameterProvider context)
		{
			Mixin.CheckField (field);

			if (field.Module == this)
				return field;

			CheckContext (context, this);

			return MetadataImporter.ImportReference (field, context);
		}

		[Obsolete ("Use ImportReference", error: false)]
		public MethodReference Import (MethodReference method)
		{
			return ImportReference (method, null);
		}

		[Obsolete ("Use ImportReference", error: false)]
		public MethodReference Import (MethodReference method, IGenericParameterProvider context)
		{
			return ImportReference (method, context);
		}

		public MethodReference ImportReference (MethodReference method)
		{
			return ImportReference (method, null);
		}

		public MethodReference ImportReference (MethodReference method, IGenericParameterProvider context)
		{
			Mixin.CheckMethod (method);

			if (method.Module == this)
				return method;

			CheckContext (context, this);

			return MetadataImporter.ImportReference (method, context);
		}

		public IMetadataTokenProvider LookupToken (int token)
		{
			return LookupToken (new MetadataToken ((uint) token));
		}

		public IMetadataTokenProvider LookupToken (MetadataToken token)
		{
			return Read (token, (t, reader) => reader.LookupToken (t));
		}

		readonly object module_lock = new object();

		internal object SyncRoot {
			get { return module_lock; }
		}

		internal void Read<TItem> (TItem item, Action<TItem, MetadataReader> read)
		{
			lock (module_lock) {
				var position = reader.position;
				var context = reader.context;

				read (item, reader);

				reader.position = position;
				reader.context = context;
			}
		}

		internal TRet Read<TItem, TRet> (TItem item, Func<TItem, MetadataReader, TRet> read)
		{
			lock (module_lock) {
				var position = reader.position;
				var context = reader.context;

				var ret = read (item, reader);

				reader.position = position;
				reader.context = context;

				return ret;
			}
		}

		internal TRet Read<TItem, TRet> (ref TRet variable, TItem item, Func<TItem, MetadataReader, TRet> read) where TRet : class
		{
			lock (module_lock) {
				if (variable != null)
					return variable;

				var position = reader.position;
				var context = reader.context;

				var ret = read (item, reader);

				reader.position = position;
				reader.context = context;

				return variable = ret;
			}
		}

		public bool HasDebugHeader {
			get { return Image != null && Image.DebugHeader != null; }
		}

		public ImageDebugHeader GetDebugHeader ()
		{
			return Image.DebugHeader ?? new ImageDebugHeader ();
		}

		public static ModuleDefinition CreateModule (string name, ModuleKind kind)
		{
			return CreateModule (name, new ModuleParameters { Kind = kind });
		}

		public static ModuleDefinition CreateModule (string name, ModuleParameters parameters)
		{
			Mixin.CheckName (name);
			Mixin.CheckParameters (parameters);

			var module = new ModuleDefinition {
				Name = name,
				kind = parameters.Kind,
				timestamp = parameters.Timestamp ?? Mixin.GetTimestamp (),
				Runtime = parameters.Runtime,
				architecture = parameters.Architecture,
				mvid = Guid.NewGuid (),
				Attributes = ModuleAttributes.ILOnly,
				Characteristics = (ModuleCharacteristics) 0x8540,
			};

			if (parameters.AssemblyResolver != null)
				module.assembly_resolver = Disposable.NotOwned (parameters.AssemblyResolver);

			if (parameters.MetadataResolver != null)
				module.metadata_resolver = parameters.MetadataResolver;

			if (parameters.MetadataImporterProvider != null)
				module.metadata_importer = parameters.MetadataImporterProvider.GetMetadataImporter (module);

			if (parameters.ReflectionImporterProvider != null)
				module.reflection_importer = parameters.ReflectionImporterProvider.GetReflectionImporter (module);

			if (parameters.Kind != ModuleKind.NetModule) {
				var assembly = new AssemblyDefinition ();
				module.assembly = assembly;
				module.assembly.Name = CreateAssemblyName (name);
				assembly.main_module = module;
			}

			module.Types.Add (new TypeDefinition (string.Empty, "<Module>", TypeAttributes.NotPublic));

			return module;
		}

		static AssemblyNameDefinition CreateAssemblyName (string name)
		{
			if (name.EndsWith (".dll") || name.EndsWith (".exe"))
				name = name.Substring (0, name.Length - 4);

			return new AssemblyNameDefinition (name, Mixin.ZeroVersion);
		}

		public void ReadSymbols ()
		{
			if (string.IsNullOrEmpty (file_name))
				throw new InvalidOperationException ();

			var provider = new DefaultSymbolReaderProvider (throwIfNoSymbol: true);
			ReadSymbols (provider.GetSymbolReader (this, file_name), throwIfSymbolsAreNotMaching: true);
		}

		public void ReadSymbols (ISymbolReader reader)
		{
			ReadSymbols(reader, throwIfSymbolsAreNotMaching: true);
		}

		public void ReadSymbols (ISymbolReader reader, bool throwIfSymbolsAreNotMaching)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			symbol_reader = reader;

			if (!symbol_reader.ProcessDebugHeader (GetDebugHeader ())) {
				symbol_reader = null;

				if (throwIfSymbolsAreNotMaching)
					throw new SymbolsNotMatchingException ("Symbols were found but are not matching the assembly");

				return;
			}

			if (HasImage && ReadingMode == ReadingMode.Immediate) {
				var immediate_reader = new ImmediateModuleReader (Image);
				immediate_reader.ReadSymbols (this);
			}
		}

		public static ModuleDefinition ReadModule (string fileName)
		{
			return ReadModule (fileName, new ReaderParameters (ReadingMode.Deferred));
		}

		public static ModuleDefinition ReadModule (string fileName, ReaderParameters parameters)
		{
			var stream = GetFileStream (fileName, FileMode.Open, parameters.ReadWrite ? FileAccess.ReadWrite : FileAccess.Read, FileShare.Read);

			if (parameters.InMemory) {
				var memory = new MemoryStream (stream.CanSeek ? (int) stream.Length : 0);
				using (stream)
					stream.CopyTo (memory);

				memory.Position = 0;
				stream = memory;
			}

			try {
				return ReadModule (Disposable.Owned (stream), fileName, parameters);
			} catch (Exception) {
				stream.Dispose ();
				throw;
			}
		}

		static Stream GetFileStream (string fileName, FileMode mode, FileAccess access, FileShare share)
		{
			Mixin.CheckFileName (fileName);

			return new FileStream (fileName, mode, access, share);
		}

		public static ModuleDefinition ReadModule (Stream stream)
		{
			return ReadModule (stream, new ReaderParameters (ReadingMode.Deferred));
		}

		public static ModuleDefinition ReadModule (Stream stream, ReaderParameters parameters)
		{
			Mixin.CheckStream (stream);
			Mixin.CheckReadSeek (stream);

			return ReadModule (Disposable.NotOwned (stream), stream.GetFileName (), parameters);
		}

		static ModuleDefinition ReadModule (Disposable<Stream> stream, string fileName, ReaderParameters parameters)
		{
			Mixin.CheckParameters (parameters);

			return ModuleReader.CreateModule (
				ImageReader.ReadImage (stream, fileName),
				parameters);
		}

		public void Write (string fileName)
		{
			Write (fileName, new WriterParameters ());
		}

		public void Write (string fileName, WriterParameters parameters)
		{
			Mixin.CheckParameters (parameters);
			var file = GetFileStream (fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
			ModuleWriter.WriteModule (this, Disposable.Owned (file), parameters);
		}

		public void Write ()
		{
			Write (new WriterParameters ());
		}

		public void Write (WriterParameters parameters)
		{
			if (!HasImage)
				throw new InvalidOperationException ();

			Write (Image.Stream.value, parameters);
		}

		public void Write (Stream stream)
		{
			Write (stream, new WriterParameters ());
		}

		public void Write (Stream stream, WriterParameters parameters)
		{
			Mixin.CheckStream (stream);
			Mixin.CheckWriteSeek (stream);
			Mixin.CheckParameters (parameters);

			ModuleWriter.WriteModule (this, Disposable.NotOwned (stream), parameters);
		}
	}

	static partial class Mixin {

		public enum Argument {
			name,
			fileName,
			fullName,
			stream,
			type,
			method,
			field,
			parameters,
			module,
			modifierType,
			eventType,
			fieldType,
			declaringType,
			returnType,
			propertyType,
			interfaceType,
			constraintType,
		}

		public static void CheckName (object name)
		{
			if (name == null)
				throw new ArgumentNullException (Argument.name.ToString ());
		}

		public static void CheckName (string name)
		{
			if (string.IsNullOrEmpty (name))
				throw new ArgumentNullOrEmptyException (Argument.name.ToString ());
		}

		public static void CheckFileName (string fileName)
		{
			if (string.IsNullOrEmpty (fileName))
				throw new ArgumentNullOrEmptyException (Argument.fileName.ToString ());
		}

		public static void CheckFullName (string fullName)
		{
			if (string.IsNullOrEmpty (fullName))
				throw new ArgumentNullOrEmptyException (Argument.fullName.ToString ());
		}

		public static void CheckStream (object stream)
		{
			if (stream == null)
				throw new ArgumentNullException (Argument.stream.ToString ());
		}

		public static void CheckWriteSeek (Stream stream)
		{
			if (!stream.CanWrite || !stream.CanSeek)
				throw new ArgumentException ("Stream must be writable and seekable.");
		}

		public static void CheckReadSeek (Stream stream)
		{
			if (!stream.CanRead || !stream.CanSeek)
				throw new ArgumentException ("Stream must be readable and seekable.");
		}

		public static void CheckType (object type)
		{
			if (type == null)
				throw new ArgumentNullException (Argument.type.ToString ());
		}

		public static void CheckType (object type, Argument argument)
		{
			if (type == null)
				throw new ArgumentNullException (argument.ToString ());
		}

		public static void CheckField (object field)
		{
			if (field == null)
				throw new ArgumentNullException (Argument.field.ToString ());
		}

		public static void CheckMethod (object method)
		{
			if (method == null)
				throw new ArgumentNullException (Argument.method.ToString ());
		}

		public static void CheckParameters (object parameters)
		{
			if (parameters == null)
				throw new ArgumentNullException (Argument.parameters.ToString ());
		}

		public static uint GetTimestamp ()
		{
			return (uint) DateTime.UtcNow.Subtract (new DateTime (1970, 1, 1)).TotalSeconds;
		}

		public static bool HasImage (this ModuleDefinition self)
		{
			return self != null && self.HasImage;
		}

		public static string GetFileName (this Stream self)
		{
			var file_stream = self as FileStream;
			if (file_stream == null)
				return string.Empty;

			return Path.GetFullPath (file_stream.Name);
		}

		public static TargetRuntime ParseRuntime (this string self)
		{
			if (string.IsNullOrEmpty (self))
				return TargetRuntime.Net_4_0;

			switch (self [1]) {
			case '1':
				return self [3] == '0'
					? TargetRuntime.Net_1_0
					: TargetRuntime.Net_1_1;
			case '2':
				return TargetRuntime.Net_2_0;
			case '4':
			default:
				return TargetRuntime.Net_4_0;
			}
		}

		public static string RuntimeVersionString (this TargetRuntime runtime)
		{
			switch (runtime) {
			case TargetRuntime.Net_1_0:
				return "v1.0.3705";
			case TargetRuntime.Net_1_1:
				return "v1.1.4322";
			case TargetRuntime.Net_2_0:
				return "v2.0.50727";
			case TargetRuntime.Net_4_0:
			default:
				return "v4.0.30319";
			}
		}

		public static bool IsWindowsMetadata (this ModuleDefinition module)
		{
			return module.MetadataKind != MetadataKind.Ecma335;
		}

		public static byte [] ReadAll (this Stream self)
		{
			int read;
			var memory = new MemoryStream ((int) self.Length);
			var buffer = new byte [1024];

			while ((read = self.Read (buffer, 0, buffer.Length)) != 0)
				memory.Write (buffer, 0, read);

			return memory.ToArray ();
		}

		public static void Read (object o)
		{
		}
	}
}
