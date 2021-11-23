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
using System.Runtime.InteropServices;
using System.Threading;
using SR = System.Reflection;

using ILRuntime.Mono.Collections.Generic;
using ILRuntime.Mono.Cecil.Cil;
using ILRuntime.Mono.Cecil.PE;

namespace ILRuntime.Mono.Cecil.Cil {

	[StructLayout (LayoutKind.Sequential)]
	public struct ImageDebugDirectory {
		public const int Size = 28;

		public int Characteristics;
		public int TimeDateStamp;
		public short MajorVersion;
		public short MinorVersion;
		public ImageDebugType Type;
		public int SizeOfData;
		public int AddressOfRawData;
		public int PointerToRawData;
	}

	public enum ImageDebugType {
		CodeView = 2,
		Deterministic = 16,
		EmbeddedPortablePdb = 17,
	}

	public sealed class ImageDebugHeader {

		readonly ImageDebugHeaderEntry [] entries;

		public bool HasEntries {
			get { return !entries.IsNullOrEmpty (); }
		}

		public ImageDebugHeaderEntry [] Entries {
			get { return entries; }
		}

		public ImageDebugHeader (ImageDebugHeaderEntry [] entries)
		{
			this.entries = entries ?? Empty<ImageDebugHeaderEntry>.Array;
		}

		public ImageDebugHeader ()
			: this (Empty<ImageDebugHeaderEntry>.Array)
		{
		}

		public ImageDebugHeader (ImageDebugHeaderEntry entry)
			: this (new [] { entry })
		{
		}
	}

	public sealed class ImageDebugHeaderEntry {

		ImageDebugDirectory directory;
		readonly byte [] data;

		public ImageDebugDirectory Directory {
			get { return directory; }
			internal set { directory = value; }
		}

		public byte [] Data {
			get { return data; }
		}

		public ImageDebugHeaderEntry (ImageDebugDirectory directory, byte [] data)
		{
			this.directory = directory;
			this.data = data ?? Empty<byte>.Array;
		}
	}

	public sealed class ScopeDebugInformation : DebugInformation {

		internal InstructionOffset start;
		internal InstructionOffset end;
		internal ImportDebugInformation import;
		internal Collection<ScopeDebugInformation> scopes;
		internal Collection<VariableDebugInformation> variables;
		internal Collection<ConstantDebugInformation> constants;

		public InstructionOffset Start {
			get { return start; }
			set { start = value; }
		}

		public InstructionOffset End {
			get { return end; }
			set { end = value; }
		}

		public ImportDebugInformation Import {
			get { return import; }
			set { import = value; }
		}

		public bool HasScopes {
			get { return !scopes.IsNullOrEmpty (); }
		}

		public Collection<ScopeDebugInformation> Scopes {
			get {
				if (scopes == null)
					Interlocked.CompareExchange (ref scopes, new Collection<ScopeDebugInformation> (), null);

				return scopes;
			}
		}

		public bool HasVariables {
			get { return !variables.IsNullOrEmpty (); }
		}

		public Collection<VariableDebugInformation> Variables {
			get {
				if (variables == null)
					Interlocked.CompareExchange (ref variables, new Collection<VariableDebugInformation> (), null);

				return variables;
			}
		}

		public bool HasConstants {
			get { return !constants.IsNullOrEmpty (); }
		}

		public Collection<ConstantDebugInformation> Constants {
			get {
				if (constants == null)
					Interlocked.CompareExchange (ref constants, new Collection<ConstantDebugInformation> (), null);

				return constants;
			}
		}

		internal ScopeDebugInformation ()
		{
			this.token = new MetadataToken (TokenType.LocalScope);
		}

		public ScopeDebugInformation (Instruction start, Instruction end)
			: this ()
		{
			if (start == null)
				throw new ArgumentNullException ("start");

			this.start = new InstructionOffset (start);

			if (end != null)
				this.end = new InstructionOffset (end);
		}

		public bool TryGetName (VariableDefinition variable, out string name)
		{
			name = null;
			if (variables == null || variables.Count == 0)
				return false;

			for (int i = 0; i < variables.Count; i++) {
				if (variables [i].Index == variable.Index) {
					name = variables [i].Name;
					return true;
				}
			}

			return false;
		}
	}

	public struct InstructionOffset {

		readonly Instruction instruction;
		readonly int? offset;

		public int Offset {
			get {
				if (instruction != null)
					return instruction.Offset;
				if (offset.HasValue)
					return offset.Value;

				throw new NotSupportedException ();
			}
		}

		public bool IsEndOfMethod {
			get { return instruction == null && !offset.HasValue; }
		}

		internal bool IsResolved => instruction != null || !offset.HasValue;

		internal Instruction ResolvedInstruction => instruction;

		public InstructionOffset (Instruction instruction)
		{
			if (instruction == null)
				throw new ArgumentNullException ("instruction");

			this.instruction = instruction;
			this.offset = null;
		}

		public InstructionOffset (int offset)
		{
			this.instruction = null;
			this.offset = offset;
		}
	}

	[Flags]
	public enum VariableAttributes : ushort {
		None = 0,
		DebuggerHidden = 1,
	}

	public struct VariableIndex {
		readonly VariableDefinition variable;
		readonly int? index;

		public int Index {
			get {
				if (variable != null)
					return variable.Index;
				if (index.HasValue)
					return index.Value;

				throw new NotSupportedException ();
			}
		}

		internal bool IsResolved => variable != null;

		internal VariableDefinition ResolvedVariable => variable;

		public VariableIndex (VariableDefinition variable)
		{
			if (variable == null)
				throw new ArgumentNullException ("variable");

			this.variable = variable;
			this.index = null;
		}

		public VariableIndex (int index)
		{
			this.variable = null;
			this.index = index;
		}
	}

	public abstract class DebugInformation : ICustomDebugInformationProvider {

		internal MetadataToken token;
		internal Collection<CustomDebugInformation> custom_infos;

		public MetadataToken MetadataToken {
			get { return token; }
			set { token = value; }
		}

		public bool HasCustomDebugInformations {
			get { return !custom_infos.IsNullOrEmpty (); }
		}

		public Collection<CustomDebugInformation> CustomDebugInformations {
			get {
				if (custom_infos == null)
					Interlocked.CompareExchange (ref custom_infos, new Collection<CustomDebugInformation> (), null);

				return custom_infos;
			}
		}

		internal DebugInformation ()
		{
		}
	}

	public sealed class VariableDebugInformation : DebugInformation {

		string name;
		ushort attributes;
		internal VariableIndex index;

		public int Index {
			get { return index.Index; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public VariableAttributes Attributes {
			get { return (VariableAttributes) attributes; }
			set { attributes = (ushort) value; }
		}

		public bool IsDebuggerHidden {
			get { return attributes.GetAttributes ((ushort) VariableAttributes.DebuggerHidden); }
			set { attributes = attributes.SetAttributes ((ushort) VariableAttributes.DebuggerHidden, value); }
		}

		internal VariableDebugInformation (int index, string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			this.index = new VariableIndex (index);
			this.name = name;
		}

		public VariableDebugInformation (VariableDefinition variable, string name)
		{
			if (variable == null)
				throw new ArgumentNullException ("variable");
			if (name == null)
				throw new ArgumentNullException ("name");

			this.index = new VariableIndex (variable);
			this.name = name;
			this.token = new MetadataToken (TokenType.LocalVariable);
		}
	}

	public sealed class ConstantDebugInformation : DebugInformation {

		string name;
		TypeReference constant_type;
		object value;

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public TypeReference ConstantType {
			get { return constant_type; }
			set { constant_type = value; }
		}

		public object Value {
			get { return value; }
			set { this.value = value; }
		}

		public ConstantDebugInformation (string name, TypeReference constant_type, object value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			this.name = name;
			this.constant_type = constant_type;
			this.value = value;
			this.token = new MetadataToken (TokenType.LocalConstant);
		}
	}

	public enum ImportTargetKind : byte {
		ImportNamespace = 1,
		ImportNamespaceInAssembly = 2,
		ImportType = 3,
		ImportXmlNamespaceWithAlias = 4,
		ImportAlias = 5,
		DefineAssemblyAlias = 6,
		DefineNamespaceAlias = 7,
		DefineNamespaceInAssemblyAlias = 8,
		DefineTypeAlias = 9,
	}

	public sealed class ImportTarget {

		internal ImportTargetKind kind;

		internal string @namespace;
		internal TypeReference type;
		internal AssemblyNameReference reference;
		internal string alias;

		public string Namespace {
			get { return @namespace; }
			set { @namespace = value; }
		}

		public TypeReference Type {
			get { return type; }
			set { type = value; }
		}

		public AssemblyNameReference AssemblyReference {
			get { return reference; }
			set { reference = value; }
		}

		public string Alias {
			get { return alias; }
			set { alias = value; }
		}

		public ImportTargetKind Kind {
			get { return kind; }
			set { kind = value; }
		}

		public ImportTarget (ImportTargetKind kind)
		{
			this.kind = kind;
		}
	}

	public sealed class ImportDebugInformation : DebugInformation {

		internal ImportDebugInformation parent;
		internal Collection<ImportTarget> targets;

		public bool HasTargets {
			get { return !targets.IsNullOrEmpty (); }
		}

		public Collection<ImportTarget> Targets {
			get
			{
				if (targets == null)
					Interlocked.CompareExchange (ref targets, new Collection<ImportTarget> (), null);

				return targets;
			}
		}

		public ImportDebugInformation Parent {
			get { return parent; }
			set { parent = value; }
		}

		public ImportDebugInformation ()
		{
			this.token = new MetadataToken (TokenType.ImportScope);
		}
	}

	public interface ICustomDebugInformationProvider : IMetadataTokenProvider {
		bool HasCustomDebugInformations { get; }
		Collection<CustomDebugInformation> CustomDebugInformations { get; }
	}

	public enum CustomDebugInformationKind {
		Binary,
		StateMachineScope,
		DynamicVariable,
		DefaultNamespace,
		AsyncMethodBody,
		EmbeddedSource,
		SourceLink,
	}

	public abstract class CustomDebugInformation : DebugInformation {

		Guid identifier;

		public Guid Identifier {
			get { return identifier; }
		}

		public abstract CustomDebugInformationKind Kind { get; }

		internal CustomDebugInformation (Guid identifier)
		{
			this.identifier = identifier;
			this.token = new MetadataToken (TokenType.CustomDebugInformation);
		}
	}

	public sealed class BinaryCustomDebugInformation : CustomDebugInformation {

		byte [] data;

		public byte [] Data {
			get { return data; }
			set { data = value; }
		}

		public override CustomDebugInformationKind Kind {
			get { return CustomDebugInformationKind.Binary; }
		}

		public BinaryCustomDebugInformation (Guid identifier, byte [] data)
			: base (identifier)
		{
			this.data = data;
		}
	}

	public sealed class AsyncMethodBodyDebugInformation : CustomDebugInformation {

		internal InstructionOffset catch_handler;
		internal Collection<InstructionOffset> yields;
		internal Collection<InstructionOffset> resumes;
		internal Collection<MethodDefinition> resume_methods;

		public InstructionOffset CatchHandler {
			get { return catch_handler; }
			set { catch_handler = value; }
		}

		public Collection<InstructionOffset> Yields {
			get {
				if (yields == null)
					Interlocked.CompareExchange (ref yields, new Collection<InstructionOffset> (), null);

				return yields;
			}
		}

		public Collection<InstructionOffset> Resumes {
			get {
				if (resumes == null)
					Interlocked.CompareExchange (ref resumes, new Collection<InstructionOffset> (), null);

				return resumes;
			}
		}

		public Collection<MethodDefinition> ResumeMethods {
			get { return resume_methods ?? (resume_methods = new Collection<MethodDefinition> ()); }
		}

		public override CustomDebugInformationKind Kind {
			get { return CustomDebugInformationKind.AsyncMethodBody; }
		}

		public static Guid KindIdentifier = new Guid ("{54FD2AC5-E925-401A-9C2A-F94F171072F8}");

		internal AsyncMethodBodyDebugInformation (int catchHandler)
			: base (KindIdentifier)
		{
			this.catch_handler = new InstructionOffset (catchHandler);
		}

		public AsyncMethodBodyDebugInformation (Instruction catchHandler)
			: base (KindIdentifier)
		{
			this.catch_handler = new InstructionOffset (catchHandler);
		}

		public AsyncMethodBodyDebugInformation ()
			: base (KindIdentifier)
		{
			this.catch_handler = new InstructionOffset (-1);
		}
	}

	public sealed class StateMachineScope {

		internal InstructionOffset start;
		internal InstructionOffset end;

		public InstructionOffset Start {
			get { return start; }
			set { start = value; }
		}

		public InstructionOffset End {
			get { return end; }
			set { end = value; }
		}

		internal StateMachineScope (int start, int end)
		{
			this.start = new InstructionOffset (start);
			this.end = new InstructionOffset (end);
		}

		public StateMachineScope (Instruction start, Instruction end)
		{
			this.start = new InstructionOffset (start);
			this.end = end != null ? new InstructionOffset (end) : new InstructionOffset ();
		}
	}

	public sealed class StateMachineScopeDebugInformation : CustomDebugInformation {

		internal Collection<StateMachineScope> scopes;

		public Collection<StateMachineScope> Scopes {
			get { return scopes ?? (scopes = new Collection<StateMachineScope> ()); }
		}

		public override CustomDebugInformationKind Kind {
			get { return CustomDebugInformationKind.StateMachineScope; }
		}

		public static Guid KindIdentifier = new Guid ("{6DA9A61E-F8C7-4874-BE62-68BC5630DF71}");

		public StateMachineScopeDebugInformation ()
			: base (KindIdentifier)
		{
		}
	}

	public sealed class EmbeddedSourceDebugInformation : CustomDebugInformation {

		internal byte [] content;
		internal bool compress;

		public byte [] Content {
			get { return content; }
			set { content = value; }
		}

		public bool Compress {
			get { return compress; }
			set { compress = value; }
		}

		public override CustomDebugInformationKind Kind {
			get { return CustomDebugInformationKind.EmbeddedSource; }
		}

		public static Guid KindIdentifier = new Guid ("{0E8A571B-6926-466E-B4AD-8AB04611F5FE}");

		public EmbeddedSourceDebugInformation (byte [] content, bool compress)
			: base (KindIdentifier)
		{
			this.content = content;
			this.compress = compress;
		}
	}

	public sealed class SourceLinkDebugInformation : CustomDebugInformation {

		internal string content;

		public string Content {
			get { return content; }
			set { content = value; }
		}

		public override CustomDebugInformationKind Kind {
			get { return CustomDebugInformationKind.SourceLink; }
		}

		public static Guid KindIdentifier = new Guid ("{CC110556-A091-4D38-9FEC-25AB9A351A6A}");

		public SourceLinkDebugInformation (string content)
			: base (KindIdentifier)
		{
			this.content = content;
		}
	}

	public sealed class MethodDebugInformation : DebugInformation {

		internal MethodDefinition method;
		internal Collection<SequencePoint> sequence_points;
		internal ScopeDebugInformation scope;
		internal MethodDefinition kickoff_method;
		internal int code_size;
		internal MetadataToken local_var_token;

		public MethodDefinition Method {
			get { return method; }
		}

		public bool HasSequencePoints {
			get { return !sequence_points.IsNullOrEmpty (); }
		}

		public Collection<SequencePoint> SequencePoints {
			get {
				if (sequence_points == null)
					Interlocked.CompareExchange (ref sequence_points, new Collection<SequencePoint> (), null);

				return sequence_points;
			}
		}

		public ScopeDebugInformation Scope {
			get { return scope; }
			set { scope = value; }
		}

		public MethodDefinition StateMachineKickOffMethod {
			get { return kickoff_method; }
			set { kickoff_method = value; }
		}

		internal MethodDebugInformation (MethodDefinition method)
		{
			if (method == null)
				throw new ArgumentNullException ("method");

			this.method = method;
			this.token = new MetadataToken (TokenType.MethodDebugInformation, method.MetadataToken.RID);
		}

		public SequencePoint GetSequencePoint (Instruction instruction)
		{
			if (!HasSequencePoints)
				return null;

			for (int i = 0; i < sequence_points.Count; i++)
				if (sequence_points [i].Offset == instruction.Offset)
					return sequence_points [i];

			return null;
		}

		public IDictionary<Instruction, SequencePoint> GetSequencePointMapping ()
		{
			var instruction_mapping = new Dictionary<Instruction, SequencePoint> ();
			if (!HasSequencePoints || !method.HasBody)
				return instruction_mapping;

			var offset_mapping = new Dictionary<int, SequencePoint> (sequence_points.Count);

			for (int i = 0; i < sequence_points.Count; i++) {
				if (!offset_mapping.ContainsKey (sequence_points [i].Offset))
					offset_mapping.Add (sequence_points [i].Offset, sequence_points [i]);
			}

			var instructions = method.Body.Instructions;

			for (int i = 0; i < instructions.Count; i++) {
				SequencePoint sequence_point;
				if (offset_mapping.TryGetValue (instructions [i].Offset, out sequence_point))
					instruction_mapping.Add (instructions [i], sequence_point);
			}

			return instruction_mapping;
		}

		public IEnumerable<ScopeDebugInformation> GetScopes ()
		{
			if (scope == null)
				return Empty<ScopeDebugInformation>.Array;

			return GetScopes (new[] { scope });
		}

		static IEnumerable<ScopeDebugInformation> GetScopes (IList<ScopeDebugInformation> scopes)
		{
			for (int i = 0; i < scopes.Count; i++) {
				var scope = scopes [i];

				yield return scope;

				if (!scope.HasScopes)
					continue;

				foreach (var sub_scope in GetScopes (scope.Scopes))
					yield return sub_scope;
			}
		}

		public bool TryGetName (VariableDefinition variable, out string name)
		{
			name = null;

			var has_name = false;
			var unique_name = "";

			foreach (var scope in GetScopes ()) {
				string slot_name;
				if (!scope.TryGetName (variable, out slot_name))
					continue;

				if (!has_name) {
					has_name = true;
					unique_name = slot_name;
					continue;
				}

				if (unique_name != slot_name)
					return false;
			}

			name = unique_name;
			return has_name;
		}
	}

	public interface ISymbolReader : IDisposable {

		ISymbolWriterProvider GetWriterProvider ();
		bool ProcessDebugHeader (ImageDebugHeader header);
		MethodDebugInformation Read (MethodDefinition method);
	}

	public interface ISymbolReaderProvider {
		ISymbolReader GetSymbolReader (ModuleDefinition module, string fileName);
		ISymbolReader GetSymbolReader (ModuleDefinition module, Stream symbolStream);
	}

#if !NET_CORE
	[Serializable]
#endif
	public sealed class SymbolsNotFoundException : FileNotFoundException {

		public SymbolsNotFoundException (string message) : base (message)
		{
		}

#if !NET_CORE
		SymbolsNotFoundException (
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
			: base (info, context)
		{
		}
#endif
	}

#if !NET_CORE
	[Serializable]
#endif
	public sealed class SymbolsNotMatchingException : InvalidOperationException {

		public SymbolsNotMatchingException (string message) : base (message)
		{
		}

#if !NET_CORE
		SymbolsNotMatchingException (
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
			: base (info, context)
		{
		}
#endif
	}

	public class DefaultSymbolReaderProvider : ISymbolReaderProvider {

		readonly bool throw_if_no_symbol;

		public DefaultSymbolReaderProvider ()
			: this (throwIfNoSymbol: true)
		{
		}

		public DefaultSymbolReaderProvider (bool throwIfNoSymbol)
		{
			throw_if_no_symbol = throwIfNoSymbol;
		}

		public ISymbolReader GetSymbolReader (ModuleDefinition module, string fileName)
		{
			if (module.Image.HasDebugTables ())
				return null;

			if (module.HasDebugHeader) {
				var header = module.GetDebugHeader ();
				var entry = header.GetEmbeddedPortablePdbEntry ();
				if (entry != null)
					return new EmbeddedPortablePdbReaderProvider ().GetSymbolReader (module, fileName);
			}

			var pdb_file_name = Mixin.GetPdbFileName (fileName);

			if (File.Exists (pdb_file_name)) {
				if (Mixin.IsPortablePdb (Mixin.GetPdbFileName (fileName)))
					return new PortablePdbReaderProvider ().GetSymbolReader (module, fileName);

				try {
					return SymbolProvider.GetReaderProvider (SymbolKind.NativePdb).GetSymbolReader (module, fileName);
				} catch (Exception) {
					// We might not include support for native pdbs.
				}
			}

			var mdb_file_name = Mixin.GetMdbFileName (fileName);
			if (File.Exists (mdb_file_name)) {
				try {
					return SymbolProvider.GetReaderProvider (SymbolKind.Mdb).GetSymbolReader (module, fileName);
				} catch (Exception) {
					// We might not include support for mdbs.
				}
			}

			if (throw_if_no_symbol)
				throw new SymbolsNotFoundException (string.Format ("No symbol found for file: {0}", fileName));

			return null;
		}

		public ISymbolReader GetSymbolReader (ModuleDefinition module, Stream symbolStream)
		{
			if (module.Image.HasDebugTables ())
				return null;

			if (module.HasDebugHeader) {
				var header = module.GetDebugHeader ();
				var entry = header.GetEmbeddedPortablePdbEntry ();
				if (entry != null)
					return new EmbeddedPortablePdbReaderProvider ().GetSymbolReader (module, "");
			}

			Mixin.CheckStream (symbolStream);
			Mixin.CheckReadSeek (symbolStream);

			var position = symbolStream.Position;

			const int portablePdbHeader = 0x424a5342;

			var reader = new BinaryStreamReader (symbolStream);
			var intHeader = reader.ReadInt32 ();
			symbolStream.Position = position;

			if (intHeader == portablePdbHeader) {
				return new PortablePdbReaderProvider ().GetSymbolReader (module, symbolStream);
			}

			const string nativePdbHeader = "Microsoft C/C++ MSF 7.00";

			var bytesHeader = reader.ReadBytes (nativePdbHeader.Length);
			symbolStream.Position = position;
			var isNativePdb = true;

			for (var i = 0; i < bytesHeader.Length; i++) {
				if (bytesHeader [i] != (byte) nativePdbHeader [i]) {
					isNativePdb = false;
					break;
				}
			}

			if (isNativePdb) {
				try {
					return SymbolProvider.GetReaderProvider (SymbolKind.NativePdb).GetSymbolReader (module, symbolStream);
				} catch (Exception) {
					// We might not include support for native pdbs.
				}
			}

			const long mdbHeader = 0x45e82623fd7fa614;

			var longHeader = reader.ReadInt64 ();
			symbolStream.Position = position;

			if (longHeader == mdbHeader) {
				try {
					return SymbolProvider.GetReaderProvider (SymbolKind.Mdb).GetSymbolReader (module, symbolStream);
				} catch (Exception) {
					// We might not include support for mdbs.
				}
			}

			if (throw_if_no_symbol)
				throw new SymbolsNotFoundException (string.Format ("No symbols found in stream"));

			return null;
		}
	}

	enum SymbolKind {
		NativePdb,
		PortablePdb,
		EmbeddedPortablePdb,
		Mdb,
	}

	static class SymbolProvider {

		static SR.AssemblyName GetSymbolAssemblyName (SymbolKind kind)
		{
			if (kind == SymbolKind.PortablePdb)
				throw new ArgumentException ();

			var suffix = GetSymbolNamespace (kind);

			var cecil_name = typeof(SymbolProvider).Assembly.GetName();

			var name = new SR.AssemblyName
			{
				Name = cecil_name.Name + "." + suffix,
				Version = cecil_name.Version,

#if NET_CORE
				CultureName = cecil_name.CultureName,
#else
				CultureInfo = cecil_name.CultureInfo,
#endif
			};

			name.SetPublicKeyToken (cecil_name.GetPublicKeyToken ());

			return name;
		}

		static Type GetSymbolType (SymbolKind kind, string fullname)
		{
			var type = Type.GetType (fullname);
			if (type != null)
				return type;

			var assembly_name = GetSymbolAssemblyName (kind);

			type = Type.GetType (fullname + ", " + assembly_name.FullName);
			if (type != null)
				return type;

			try {
				var assembly = SR.Assembly.Load (assembly_name);
				if (assembly != null)
					return assembly.GetType (fullname);
			} catch (FileNotFoundException) {
			} catch (FileLoadException) {
			}

			return null;
		}

		public static ISymbolReaderProvider GetReaderProvider (SymbolKind kind)
		{
			if (kind == SymbolKind.PortablePdb)
				return new PortablePdbReaderProvider ();
			if (kind == SymbolKind.EmbeddedPortablePdb)
				return new EmbeddedPortablePdbReaderProvider ();

			var provider_name = GetSymbolTypeName (kind, "ReaderProvider");
			var type = GetSymbolType (kind, provider_name);
			if (type == null)
				throw new TypeLoadException ("Could not find symbol provider type " + provider_name);

			return (ISymbolReaderProvider) Activator.CreateInstance (type);
		}

		static string GetSymbolTypeName (SymbolKind kind, string name)
		{
			return "Mono.Cecil" + "." + GetSymbolNamespace (kind) + "." + kind + name;
		}

		static string GetSymbolNamespace (SymbolKind kind)
		{
			if (kind == SymbolKind.PortablePdb || kind == SymbolKind.EmbeddedPortablePdb)
				return "Cil";
			if (kind == SymbolKind.NativePdb)
				return "Pdb";
			if (kind == SymbolKind.Mdb)
				return "Mdb";

			throw new ArgumentException ();
		}
	}

	public interface ISymbolWriter : IDisposable {

		ISymbolReaderProvider GetReaderProvider ();
		ImageDebugHeader GetDebugHeader ();
		void Write (MethodDebugInformation info);
	}

	public interface ISymbolWriterProvider {

		ISymbolWriter GetSymbolWriter (ModuleDefinition module, string fileName);
		ISymbolWriter GetSymbolWriter (ModuleDefinition module, Stream symbolStream);
	}

	public class DefaultSymbolWriterProvider : ISymbolWriterProvider {

		public ISymbolWriter GetSymbolWriter (ModuleDefinition module, string fileName)
		{
			var reader = module.SymbolReader;
			if (reader == null)
				throw new InvalidOperationException ();

			if (module.Image != null && module.Image.HasDebugTables ())
				return null;

			return reader.GetWriterProvider ().GetSymbolWriter (module, fileName);
		}

		public ISymbolWriter GetSymbolWriter (ModuleDefinition module, Stream symbolStream)
		{
			throw new NotSupportedException ();
		}
	}
}

namespace ILRuntime.Mono.Cecil {

	static partial class Mixin {

		public static ImageDebugHeaderEntry GetCodeViewEntry (this ImageDebugHeader header)
		{
			return GetEntry (header, ImageDebugType.CodeView);
		}

		public static ImageDebugHeaderEntry GetDeterministicEntry (this ImageDebugHeader header)
		{
			return GetEntry (header, ImageDebugType.Deterministic);
		}

		public static ImageDebugHeader AddDeterministicEntry (this ImageDebugHeader header)
		{
			var entry = new ImageDebugHeaderEntry (new ImageDebugDirectory { Type = ImageDebugType.Deterministic }, Empty<byte>.Array);
			if (header == null)
				return new ImageDebugHeader (entry);

			var entries = new ImageDebugHeaderEntry [header.Entries.Length + 1];
			Array.Copy (header.Entries, entries, header.Entries.Length);
			entries [entries.Length - 1] = entry;
			return new ImageDebugHeader (entries);
		}

		public static ImageDebugHeaderEntry GetEmbeddedPortablePdbEntry (this ImageDebugHeader header)
		{
			return GetEntry (header, ImageDebugType.EmbeddedPortablePdb);
		}

		private static ImageDebugHeaderEntry GetEntry (this ImageDebugHeader header, ImageDebugType type)
		{
			if (!header.HasEntries)
				return null;

			for (var i = 0; i < header.Entries.Length; i++) {
				var entry = header.Entries [i];
				if (entry.Directory.Type == type)
					return entry;
			}

			return null;
		}

		public static string GetPdbFileName (string assemblyFileName)
		{
			return Path.ChangeExtension (assemblyFileName, ".pdb");
		}

		public static string GetMdbFileName (string assemblyFileName)
		{
			return assemblyFileName + ".mdb";
		}

		public static bool IsPortablePdb (string fileName)
		{
			using (var file = new FileStream (fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				return IsPortablePdb (file);
		}

		public static bool IsPortablePdb (Stream stream)
		{
			const uint ppdb_signature = 0x424a5342;

			if (stream.Length < 4) return false;
			var position = stream.Position;
			try {
				var reader = new BinaryReader (stream);
				return reader.ReadUInt32 () == ppdb_signature;
			} finally {
				stream.Position = position;
			}
		}
	}
}
