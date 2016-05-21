//
// ModuleDefinition.cs
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SR = System.Reflection;
using Mono.Cecil.Cil;
using Mono.Cecil.Metadata;
using Mono.Cecil.PE;
using Mono.Collections.Generic;

namespace Mono.Cecil
{

    public enum ReadingMode
    {
        Immediate = 1,
        Deferred = 2,
    }

    public sealed class ReaderParameters
    {

        ReadingMode reading_mode;
        IAssemblyResolver assembly_resolver;
        IMetadataResolver metadata_resolver;
        Stream symbol_stream;
        ISymbolReaderProvider symbol_reader_provider;
        bool read_symbols;

        public ReadingMode ReadingMode
        {
            get { return reading_mode; }
            set { reading_mode = value; }
        }

        public IAssemblyResolver AssemblyResolver
        {
            get { return assembly_resolver; }
            set { assembly_resolver = value; }
        }

        public IMetadataResolver MetadataResolver
        {
            get { return metadata_resolver; }
            set { metadata_resolver = value; }
        }

        public Stream SymbolStream
        {
            get { return symbol_stream; }
            set { symbol_stream = value; }
        }

        public ISymbolReaderProvider SymbolReaderProvider
        {
            get { return symbol_reader_provider; }
            set { symbol_reader_provider = value; }
        }

        public bool ReadSymbols
        {
            get { return read_symbols; }
            set { read_symbols = value; }
        }

        public ReaderParameters()
            : this(ReadingMode.Deferred)
        {
        }

        public ReaderParameters(ReadingMode readingMode)
        {
            this.reading_mode = readingMode;
        }
    }


    public sealed class ModuleDefinition : ModuleReference, ICustomAttributeProvider
    {

        internal Image Image;
        internal MetadataSystem MetadataSystem;
        internal ReadingMode ReadingMode;
        internal ISymbolReaderProvider SymbolReaderProvider;

        internal ISymbolReader symbol_reader;
        internal IAssemblyResolver assembly_resolver;
        internal IMetadataResolver metadata_resolver;
        internal TypeSystem type_system;

        readonly MetadataReader reader;
        readonly string fq_name;

        internal ModuleKind kind;
        TargetRuntime runtime;
        TargetArchitecture architecture;
        ModuleAttributes attributes;
        ModuleCharacteristics characteristics;
        Guid mvid;

        internal AssemblyDefinition assembly;
        MethodDefinition entry_point;

        Collection<CustomAttribute> custom_attributes;
        Collection<AssemblyNameReference> references;
        Collection<ModuleReference> modules;
        Collection<Resource> resources;
        Collection<ExportedType> exported_types;
        TypeDefinitionCollection types;

        public bool IsMain
        {
            get { return kind != ModuleKind.NetModule; }
        }

        public ModuleKind Kind
        {
            get { return kind; }
            set { kind = value; }
        }

        public TargetRuntime Runtime
        {
            get { return runtime; }
            set { runtime = value; }
        }

        public TargetArchitecture Architecture
        {
            get { return architecture; }
            set { architecture = value; }
        }

        public ModuleAttributes Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }

        public ModuleCharacteristics Characteristics
        {
            get { return characteristics; }
            set { characteristics = value; }
        }

        public string FullyQualifiedName
        {
            get { return fq_name; }
        }

        public Guid Mvid
        {
            get { return mvid; }
            set { mvid = value; }
        }

        internal bool HasImage
        {
            get { return Image != null; }
        }

        public bool HasSymbols
        {
            get { return symbol_reader != null; }
        }

        public ISymbolReader SymbolReader
        {
            get { return symbol_reader; }
        }

        public override MetadataScopeType MetadataScopeType
        {
            get { return MetadataScopeType.ModuleDefinition; }
        }

        public AssemblyDefinition Assembly
        {
            get { return assembly; }
        }


        public IAssemblyResolver AssemblyResolver
        {
            get { return assembly_resolver ?? (assembly_resolver = new DefaultAssemblyResolver()); }
        }

        public IMetadataResolver MetadataResolver
        {
            get
            {
                if (metadata_resolver == null)
                    metadata_resolver=new MetadataResolver(this.AssemblyResolver);

                return metadata_resolver;
            }
        }

        public TypeSystem TypeSystem
        {
            get
            {
                if (type_system == null)
                    type_system = TypeSystem.CreateTypeSystem(this);
                return type_system;
            }
        }

        public bool HasAssemblyReferences
        {
            get
            {
                if (references != null)
                    return references.Count > 0;

                return HasImage && Image.HasTable(Table.AssemblyRef);
            }
        }

        public Collection<AssemblyNameReference> AssemblyReferences
        {
            get
            {
                if (references != null)
                    return references;

                if (HasImage)
                    return Read(ref references, this, (_, reader) => reader.ReadAssemblyReferences());

                return references = new Collection<AssemblyNameReference>();
            }
        }

        public bool HasModuleReferences
        {
            get
            {
                if (modules != null)
                    return modules.Count > 0;

                return HasImage && Image.HasTable(Table.ModuleRef);
            }
        }

        public Collection<ModuleReference> ModuleReferences
        {
            get
            {
                if (modules != null)
                    return modules;

                if (HasImage)
                    return Read(ref modules, this, (_, reader) => reader.ReadModuleReferences());

                return modules = new Collection<ModuleReference>();
            }
        }

        public bool HasResources
        {
            get
            {
                if (resources != null)
                    return resources.Count > 0;

                if (HasImage)
                    return Image.HasTable(Table.ManifestResource) || Read(this, (_, reader) => reader.HasFileResource());

                return false;
            }
        }

        public Collection<Resource> Resources
        {
            get
            {
                if (resources != null)
                    return resources;

                if (HasImage)
                    return Read(ref resources, this, (_, reader) => reader.ReadResources());

                return resources = new Collection<Resource>();
            }
        }

        public bool HasCustomAttributes
        {
            get
            {
                if (custom_attributes != null)
                    return custom_attributes.Count > 0;

                return Mixin.GetHasCustomAttributes(this, this);
            }
        }

        public Collection<CustomAttribute> CustomAttributes
        {
            get { return custom_attributes ?? (Mixin.GetCustomAttributes(this, ref custom_attributes, this)); }
        }

        public bool HasTypes
        {
            get
            {
                if (types != null)
                    return types.Count > 0;

                return HasImage && Image.HasTable(Table.TypeDef);
            }
        }

        public Collection<TypeDefinition> Types
        {
            get
            {
                if (types != null)
                    return types;

                if (HasImage)
                    return Read(ref types, this, (_, reader) => reader.ReadTypes());

                return types = new TypeDefinitionCollection(this);
            }
        }

        public bool HasExportedTypes
        {
            get
            {
                if (exported_types != null)
                    return exported_types.Count > 0;

                return HasImage && Image.HasTable(Table.ExportedType);
            }
        }

        public Collection<ExportedType> ExportedTypes
        {
            get
            {
                if (exported_types != null)
                    return exported_types;

                if (HasImage)
                    return Read(ref exported_types, this, (_, reader) => reader.ReadExportedTypes());

                return exported_types = new Collection<ExportedType>();
            }
        }

        public MethodDefinition EntryPoint
        {
            get
            {
                if (entry_point != null)
                    return entry_point;

                if (HasImage)
                    return Read(ref entry_point, this, (_, reader) => reader.ReadEntryPoint());

                return entry_point = null;
            }
            set { entry_point = value; }
        }

        internal ModuleDefinition()
        {
            this.MetadataSystem = new MetadataSystem();
            this.token = new MetadataToken(TokenType.Module, 1);
        }

        internal ModuleDefinition(Image image)
            : this()
        {
            this.Image = image;
            this.kind = image.Kind;
            this.runtime = image.Runtime;
            this.architecture = image.Architecture;
            this.attributes = image.Attributes;
            this.characteristics = image.Characteristics;
            this.fq_name = image.FileName;

            this.reader = new MetadataReader(this);
        }

        public bool HasTypeReference(string fullName)
        {
            return HasTypeReference(string.Empty, fullName);
        }

        public bool HasTypeReference(string scope, string fullName)
        {
            CheckFullName(fullName);

            if (!HasImage)
                return false;

            return GetTypeReference(scope, fullName) != null;
        }

        public bool TryGetTypeReference(string fullName, out TypeReference type)
        {
            return TryGetTypeReference(string.Empty, fullName, out type);
        }

        public bool TryGetTypeReference(string scope, string fullName, out TypeReference type)
        {
            CheckFullName(fullName);

            if (!HasImage)
            {
                type = null;
                return false;
            }

            return (type = GetTypeReference(scope, fullName)) != null;
        }

        TypeReference GetTypeReference(string scope, string fullname)
        {
            return Read(new Row<string, string>(scope, fullname), (row, reader) => reader.GetTypeReference(row.Col1, row.Col2));
        }

        public IEnumerable<TypeReference> GetTypeReferences()
        {
            if (!HasImage)
                return Empty<TypeReference>.Array;

            return Read(this, (_, reader) => reader.GetTypeReferences());
        }

        public IEnumerable<MemberReference> GetMemberReferences()
        {
            if (!HasImage)
                return Empty<MemberReference>.Array;

            return Read(this, (_, reader) => reader.GetMemberReferences());
        }

        public TypeReference GetType(string fullName, bool runtimeName)
        {
            return runtimeName
                ? TypeParser.ParseType(this, fullName)
                : GetType(fullName);
        }

        public TypeDefinition GetType(string fullName)
        {
            CheckFullName(fullName);

            var position = fullName.IndexOf('/');
            if (position > 0)
                return GetNestedType(fullName);

            return ((TypeDefinitionCollection)this.Types).GetType(fullName);
        }

        public TypeDefinition GetType(string @namespace, string name)
        {
            Mixin.CheckName(name);

            return ((TypeDefinitionCollection)this.Types).GetType(@namespace ?? string.Empty, name);
        }

        public IEnumerable<TypeDefinition> GetTypes()
        {
            return GetTypes(Types);
        }

        static IEnumerable<TypeDefinition> GetTypes(Collection<TypeDefinition> types)
        {
            for (int i = 0; i < types.Count; i++)
            {
                var type = types[i];

                yield return type;

                if (!type.HasNestedTypes)
                    continue;

                foreach (var nested in GetTypes(type.NestedTypes))
                    yield return nested;
            }
        }

        static void CheckFullName(string fullName)
        {
            if (fullName == null)
                throw new ArgumentNullException("fullName");
            if (fullName.Length == 0)
                throw new ArgumentException();
        }

        TypeDefinition GetNestedType(string fullname)
        {
            var names = fullname.Split('/');
            var type = GetType(names[0]);

            if (type == null)
                return null;

            for (int i = 1; i < names.Length; i++)
            {
                var nested_type = Mixin.GetNestedType(type, names[i]);
                if (nested_type == null)
                    return null;

                type = nested_type;
            }

            return type;
        }

        internal FieldDefinition Resolve(FieldReference field)
        {
            return MetadataResolver.Resolve(field);
        }

        internal MethodDefinition Resolve(MethodReference method)
        {
            return MetadataResolver.Resolve(method);
        }

        internal TypeDefinition Resolve(TypeReference type)
        {
            return MetadataResolver.Resolve(type);
        }


        public IMetadataTokenProvider LookupToken(int token)
        {
            return LookupToken(new MetadataToken((uint)token));
        }

        public IMetadataTokenProvider LookupToken(MetadataToken token)
        {
            return Read(token, (t, reader) => reader.LookupToken(t));
        }

        readonly object module_lock = new object();

        internal object SyncRoot
        {
            get { return module_lock; }
        }

        internal TRet Read<TItem, TRet>(TItem item, Func<TItem, MetadataReader, TRet> read)
        {
            lock (module_lock)
            {
                var position = reader.position;
                var context = reader.context;

                var ret = read(item, reader);

                reader.position = position;
                reader.context = context;

                return ret;
            }
        }

        internal TRet Read<TItem, TRet>(ref TRet variable, TItem item, Func<TItem, MetadataReader, TRet> read) where TRet : class
        {
            lock (module_lock)
            {
                if (variable != null)
                    return variable;

                var position = reader.position;
                var context = reader.context;

                var ret = read(item, reader);

                reader.position = position;
                reader.context = context;

                return variable = ret;
            }
        }

        public bool HasDebugHeader
        {
            get { return Image != null && !Image.Debug.IsZero; }
        }

        public ImageDebugDirectory GetDebugHeader(out byte[] header)
        {
            if (!HasDebugHeader)
                throw new InvalidOperationException();

            return Image.GetDebugHeader(out header);
        }

        void ProcessDebugHeader()
        {
            if (!HasDebugHeader)
                return;

            byte[] header;
            var directory = GetDebugHeader(out header);

            if (!symbol_reader.ProcessDebugHeader(directory, header))
                throw new InvalidOperationException();
        }



        public void ReadSymbols()
        {
            if (string.IsNullOrEmpty(fq_name))
                throw new InvalidOperationException();

            var provider = SymbolProvider.GetPlatformReaderProvider();
            if (provider == null)
                throw new InvalidOperationException();

            ReadSymbols(provider.GetSymbolReader(this, fq_name));
        }

        public void ReadSymbols(ISymbolReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            symbol_reader = reader;

            ProcessDebugHeader();
        }

        public static ModuleDefinition ReadModule(string fileName)
        {
            return ReadModule(fileName, new ReaderParameters(ReadingMode.Deferred));
        }

        public static ModuleDefinition ReadModule(Stream stream)
        {
            return ReadModule(stream, new ReaderParameters(ReadingMode.Deferred));
        }

        public static ModuleDefinition ReadModule(string fileName, ReaderParameters parameters)
        {
            using (var stream = GetFileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ReadModule(stream, parameters);
            }
        }

        static void CheckStream(object stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
        }

        public static ModuleDefinition ReadModule(Stream stream, ReaderParameters parameters)
        {
            CheckStream(stream);
            if (!stream.CanRead || !stream.CanSeek)
                throw new ArgumentException();
            Mixin.CheckParameters(parameters);

            return ModuleReader.CreateModuleFrom(
                ImageReader.ReadImageFrom(stream),
                parameters);
        }

        static Stream GetFileStream(string fileName, FileMode mode, FileAccess access, FileShare share)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");
            if (fileName.Length == 0)
                throw new ArgumentException();

            return new FileStream(fileName, mode, access, share);
        }



    }

    static partial class Mixin
    {

        public static void CheckParameters(object parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");
        }

        public static bool HasImage(ModuleDefinition self)
        {
            return self != null && self.HasImage;
        }

        public static bool IsCorlib(ModuleDefinition module)
        {
            if (module.Assembly == null)
                return false;

            return module.Assembly.Name.Name == "mscorlib";
        }

        public static string GetFullyQualifiedName(Stream self)
        {

            return string.Empty;
        }

        public static TargetRuntime ParseRuntime(string self)
        {
            switch (self[1])
            {
                case '1':
                    return self[3] == '0'
                        ? TargetRuntime.Net_1_0
                        : TargetRuntime.Net_1_1;
                case '2':
                    return TargetRuntime.Net_2_0;
                case '4':
                default:
                    return TargetRuntime.Net_4_0;
            }
        }
    }
}
