using Google.Protobuf.Reflection;
using ProtoBuf;
using ProtoBuf.Reflection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Google.Protobuf.Reflection
{
#pragma warning disable CS1591

    interface IType
    {
        IType Parent { get; }
        string FullyQualifiedName { get; }

        IType Find(string name);
    }
    partial class FileDescriptorSet
    {
        internal const string Namespace = ".google.protobuf.";
        public Func<string, bool> ImportValidator { get; set; }

        internal List<string> importPaths = new List<string>();
        public void AddImportPath(string path)
        {
            importPaths.Add(path);
        }
        public Error[] GetErrors() => Error.GetArray(Errors);
        internal List<Error> Errors { get; } = new List<Error>();

        public bool Add(string name, bool includeInOutput, TextReader source = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (Path.IsPathRooted(name) || name.Contains(".."))
                throw new ArgumentException("Paths should be relative to the import paths, not rooted", nameof(name));
            FileDescriptorProto descriptor;
            if (TryResolve(name, out descriptor))
            {
                if (includeInOutput) descriptor.IncludeInOutput = true;
                return true; // already exists, that counts as success
            }

            using (var reader = source ?? Open(name))
            {
                if (reader == null) return false; // not found

                descriptor = new FileDescriptorProto
                {
                    Name = name,
                    IncludeInOutput = includeInOutput
                };
                Files.Add(descriptor);

                descriptor.Parse(reader, Errors, name);
                return true;
            }
        }

        private TextReader Open(string name)
        {
            var found = FindFile(name);
            if (found == null) return null;
            return File.OpenText(found);
        }
        string FindFile(string file)
        {
            foreach (var path in importPaths)
            {
                var rel = Path.Combine(path, file);
                if (File.Exists(rel)) return rel;
            }
            return null;
        }

        bool TryResolve(string name, out FileDescriptorProto descriptor)
        {
            descriptor = Files.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            return descriptor != null;
        }

        private void ApplyImports()
        {
            bool didSomething;
            do
            {
                didSomething = false;
                var file = Files.FirstOrDefault(x => x.HasPendingImports);
                if (file != null)
                {
                    // note that GetImports clears the flag
                    foreach (var import in file.GetImports(true))
                    {
                        if (!(ImportValidator?.Invoke(import.Path) ?? true))
                        {
                            Errors.Error(import.Token, $"import of {import.Path} is disallowed");
                        }
                        else if (Add(import.Path, false))
                        {
                            didSomething = true;
                        }
                        else
                        {
                            Errors.Error(import.Token, $"unable to find: '{import.Path}'");
                        }
                    }
                }
            } while (didSomething);
        }

        public void Process()
        {
            ApplyImports();
            foreach (var file in Files)
            {
                using (var ctx = new ParserContext(file, null, Errors))
                {
                    file.BuildTypeHierarchy(this, ctx);
                }
            }
            foreach (var file in Files)
            {
                using (var ctx = new ParserContext(file, null, Errors))
                {
                    file.ResolveTypes(ctx, false);
                }
            }
            foreach (var file in Files)
            {
                using (var ctx = new ParserContext(file, null, Errors))
                {
                    file.ResolveTypes(ctx, true);
                }
            }
        }

        public T Serialize<T>(Func<FileDescriptorSet,object,T> customSerializer, bool includeImports, object state = null)
        {
            T result;
            if (includeImports || Files.All(x => x.IncludeInOutput))
            {
                result = customSerializer(this, state);
            }
            else
            {
                var snapshort = Files.ToArray();
                Files.RemoveAll(x => !x.IncludeInOutput);
                result = customSerializer(this, state);
                Files.Clear();
                Files.AddRange(snapshort);
            }
            return result;
        }

        public void Serialize(Stream destination, bool includeImports)
        {
            Serialize((s,o) => { Serializer.Serialize((Stream)o, s); return true; }, includeImports, destination);
        }

        internal FileDescriptorProto GetFile(string path)
            // try full match first, then name-only match
            => Files.FirstOrDefault(x => string.Equals(x.Name, path, StringComparison.OrdinalIgnoreCase));
    }
    partial class DescriptorProto : ISchemaObject, IType, IMessage
    {
        public static byte[] GetExtensionData(IExtensible obj)
        {
            var ext = obj?.GetExtensionObject(false);
            int len;
            if (ext == null || (len = ext.GetLength()) == 0) return null;
            var s = ext.BeginQuery();
            try
            {
                if (s is MemoryStream) return ((MemoryStream)s).ToArray();

                byte[] buffer = new byte[len];
                int offset = 0, read;
                while ((read = s.Read(buffer, offset, len)) > 0)
                {
                    offset += read;
                    len -= read;
                }
                if (len != 0) throw new EndOfStreamException();
                return buffer;
            }
            finally
            {
                ext.EndQuery(s);
            }
        }
        public static void SetExtensionData(IExtensible obj, byte[] data)
        {
            if (obj == null || data == null || data.Length == 0) return;
            var ext = obj.GetExtensionObject(true);
            (ext as IExtensionResettable)?.Reset();
            var s = ext.BeginAppend();
            try
            {
                s.Write(data, 0, data.Length);
                ext.EndAppend(s, true);
            }
            catch
            {
                ext.EndAppend(s, false);
                throw;
            }
        }

        public override string ToString() => Name;
        internal IType Parent { get; set; }
        IType IType.Parent => Parent;
        string IType.FullyQualifiedName => FullyQualifiedName;
        IType IType.Find(string name)
        {
            return (IType)NestedTypes.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))
                ?? (IType)EnumTypes.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }
        internal string FullyQualifiedName { get; set; }

        List<DescriptorProto> IMessage.Types => NestedTypes;

        internal int MaxField => (Options?.MessageSetWireFormat == true) ? int.MaxValue : FieldDescriptorProto.DefaultMaxField;
        int IMessage.MaxField => MaxField;


        internal static bool TryParse(ParserContext ctx, IHazNames parent, out DescriptorProto obj)
        {
            var name = ctx.Tokens.Consume(TokenType.AlphaNumeric);
            ctx.CheckNames(parent, name, ctx.Tokens.Previous);
            if (ctx.TryReadObject(out obj))
            {
                obj.Name = name;
                return true;
            }
            return false;
        }
        void ISchemaObject.ReadOne(ParserContext ctx)
        {
            var tokens = ctx.Tokens;
            if (tokens.ConsumeIf(TokenType.AlphaNumeric, "message"))
            {
                DescriptorProto obj;
                if (DescriptorProto.TryParse(ctx, this, out obj))
                {
                    NestedTypes.Add(obj);
                }
            }
            else if (tokens.ConsumeIf(TokenType.AlphaNumeric, "enum"))
            {
                EnumDescriptorProto obj;
                if (EnumDescriptorProto.TryParse(ctx, this, out obj))
                    EnumTypes.Add(obj);
            }
            else if (tokens.ConsumeIf(TokenType.AlphaNumeric, "option"))
            {
                Options = ctx.ParseOptionStatement(Options, this);
            }
            else if (tokens.ConsumeIf(TokenType.AlphaNumeric, "reserved"))
            {
                ParseReservedRanges(ctx);
            }
            else if (tokens.ConsumeIf(TokenType.AlphaNumeric, "extensions"))
            {
                ParseExtensionRange(ctx);
            }
            else if (tokens.ConsumeIf(TokenType.AlphaNumeric, "extend"))
            {
                FieldDescriptorProto.ParseExtensions(ctx, this);
            }
            else if (tokens.ConsumeIf(TokenType.AlphaNumeric, "oneof"))
            {
                OneofDescriptorProto.Parse(ctx, this);
            }
            else if (tokens.ConsumeIf(TokenType.AlphaNumeric, "map"))
            {
                ParseMap(ctx);
            }
            else
            {
                FieldDescriptorProto obj;
                if (FieldDescriptorProto.TryParse(ctx, this, false, out obj))
                    Fields.Add(obj);
            }
        }


        private void ParseMap(ParserContext ctx)
        {
            ctx.AbortState = AbortState.Statement;
            var tokens = ctx.Tokens;
            tokens.Consume(TokenType.Symbol, "<");
            var keyName = tokens.Consume(TokenType.AlphaNumeric);
            var keyToken = tokens.Previous;
            FieldDescriptorProto.Type keyType;
            if (FieldDescriptorProto.TryIdentifyType(keyName, out keyType))
            {
                keyName = null;
            }
            switch (keyType)
            {
                case 0:
                case FieldDescriptorProto.Type.TypeBytes:
                case FieldDescriptorProto.Type.TypeMessage:
                case FieldDescriptorProto.Type.TypeGroup:
                case FieldDescriptorProto.Type.TypeFloat:
                case FieldDescriptorProto.Type.TypeDouble:
                    ctx.Errors.Error(tokens.Previous, "invalid map key type (only integral and string types are allowed)");
                    break;
            }
            tokens.Consume(TokenType.Symbol, ",");
            var valueName = tokens.Consume(TokenType.AlphaNumeric);
            var valueToken = tokens.Previous;
            FieldDescriptorProto.Type valueType;
            if (FieldDescriptorProto.TryIdentifyType(valueName, out valueType))
            {
                valueName = null;
            }
            tokens.Consume(TokenType.Symbol, ">");

            var name = tokens.Consume(TokenType.AlphaNumeric);
            var nameToken = tokens.Previous;
            ctx.CheckNames(this, name, nameToken);

            tokens.Consume(TokenType.Symbol, "=");
            int number = tokens.ConsumeInt32();

            var jsonName = FieldDescriptorProto.GetJsonName(name);
            var typeName = jsonName.Substring(0, 1).ToUpperInvariant() + jsonName.Substring(1) + "Entry";
            ctx.CheckNames(this, typeName, nameToken);

            var field = new FieldDescriptorProto
            {
                type = FieldDescriptorProto.Type.TypeMessage,
                TypeName = typeName,
                Name = name,
                JsonName = jsonName,
                Number = number,
                label = FieldDescriptorProto.Label.LabelRepeated,
                TypeToken = nameToken
            };

            if (tokens.ConsumeIf(TokenType.Symbol, "["))
            {
                field.Options = ctx.ParseOptionBlock(field.Options, field);
            }
            Fields.Add(field);

            var msgType = new DescriptorProto
            {
                Name = typeName,
                Fields =
                {
                    new FieldDescriptorProto
                    {
                        label = FieldDescriptorProto.Label.LabelOptional,
                        Name = "key",
                        JsonName = "key",
                        Number = 1,
                        type = keyType,
                        TypeName = keyName,
                        TypeToken = keyToken,
                    },
                    new FieldDescriptorProto
                    {
                        label = FieldDescriptorProto.Label.LabelOptional,
                        Name = "value",
                        JsonName = "value",
                        Number = 2,
                        type = valueType,
                        TypeName = valueName,
                        TypeToken = valueToken,
                    }
                }
            };
            if (msgType.Options == null) msgType.Options = new MessageOptions();
            msgType.Options.MapEntry = true;
            NestedTypes.Add(msgType);

            ctx.AbortState = AbortState.None;
        }

        private void ParseExtensionRange(ParserContext ctx)
        {
            ctx.AbortState = AbortState.Statement;
            var tokens = ctx.Tokens;
            tokens.Previous.RequireProto2(ctx);

            while (true)
            {
                int from = tokens.ConsumeInt32(MaxField), to = from;
                if (tokens.Read().Is(TokenType.AlphaNumeric, "to"))
                {
                    tokens.Consume();
                    to = tokens.ConsumeInt32(MaxField);
                }
                // the end is off by one
                if (to != int.MaxValue) to++;
                ExtensionRanges.Add(new ExtensionRange { Start = from, End = to });

                if (tokens.ConsumeIf(TokenType.Symbol, ","))
                {
                    tokens.Consume();
                }
                else if (tokens.ConsumeIf(TokenType.Symbol, ";"))
                {
                    break;
                }
                else
                {
                    tokens.Read().Throw("unable to parse extension range");
                }
            }
            ctx.AbortState = AbortState.None;
        }



        private void ParseReservedRanges(ParserContext ctx)
        {
            ctx.AbortState = AbortState.Statement;
            var tokens = ctx.Tokens;
            var token = tokens.Read(); // test the first one to determine what we're doing
            switch (token.Type)
            {
                case TokenType.StringLiteral:
                    while (true)
                    {
                        var name = tokens.Consume(TokenType.StringLiteral);
                        var conflict = Fields.FirstOrDefault(x => x.Name == name);
                        if (conflict != null)
                        {
                            ctx.Errors.Error(tokens.Previous, $"'{conflict.Name}' is already in use by field {conflict.Number}");
                        }
                        ReservedNames.Add(name);

                        if (tokens.ConsumeIf(TokenType.Symbol, ","))
                        {
                        }
                        else if (tokens.ConsumeIf(TokenType.Symbol, ";"))
                        {
                            break;
                        }
                        else
                        {
                            tokens.Read().Throw("unable to parse reserved range");
                        }
                    }
                    break;
                case TokenType.AlphaNumeric:
                    while (true)
                    {
                        int from = tokens.ConsumeInt32(), to = from;
                        if (tokens.Read().Is(TokenType.AlphaNumeric, "to"))
                        {
                            tokens.Consume();
                            to = tokens.ConsumeInt32();
                        }
                        var conflict = Fields.FirstOrDefault(x => x.Number >= from && x.Number <= to);
                        if (conflict != null)
                        {
                            ctx.Errors.Error(tokens.Previous, $"field {conflict.Number} is already in use by '{conflict.Name}'");
                        }
                        ReservedRanges.Add(new ReservedRange { Start = from, End = to + 1 });

                        token = tokens.Read();
                        if (token.Is(TokenType.Symbol, ","))
                        {
                            tokens.Consume();
                        }
                        else if (token.Is(TokenType.Symbol, ";"))
                        {
                            tokens.Consume();
                            break;
                        }
                        else
                        {
                            token.Throw();
                        }
                    }
                    break;
                default:
                    throw token.Throw();
            }
            ctx.AbortState = AbortState.None;
        }

        IEnumerable<string> IHazNames.GetNames()
        {
            foreach (var field in Fields) yield return field.Name;
            foreach (var type in NestedTypes) yield return type.Name;
            foreach (var type in EnumTypes) yield return type.Name;
            foreach (var name in ReservedNames) yield return name;
        }
    }

    partial class OneofDescriptorProto : ISchemaObject
    {
        internal DescriptorProto Parent { get; set; }
        internal static void Parse(ParserContext ctx, DescriptorProto parent)
        {
            ctx.AbortState = AbortState.Object;
            var oneOf = new OneofDescriptorProto
            {
                Name = ctx.Tokens.Consume(TokenType.AlphaNumeric)
            };
            parent.OneofDecls.Add(oneOf);
            oneOf.Parent = parent;

            if (ctx.TryReadObjectImpl(oneOf))
            {
                ctx.AbortState = AbortState.None;
            }
        }
        void ISchemaObject.ReadOne(ParserContext ctx)
        {
            var tokens = ctx.Tokens;
            if (tokens.ConsumeIf(TokenType.AlphaNumeric, "option"))
            {
                Options = ctx.ParseOptionStatement(Options, this);
            }
            else
            {
                FieldDescriptorProto field;
                if (FieldDescriptorProto.TryParse(ctx, Parent, true, out field))
                {
                    field.OneofIndex = Parent.OneofDecls.Count() - 1;
                    Parent.Fields.Add(field);
                }
            }
        }
    }
    partial class OneofOptions : ISchemaOptions
    {
        string ISchemaOptions.Extendee => FileDescriptorSet.Namespace + nameof(OneofOptions);
        public byte[] ExtensionData
        {
            get { return DescriptorProto.GetExtensionData(this); }
            set { DescriptorProto.SetExtensionData(this, value); }
        }
        bool ISchemaOptions.Deprecated { get { return false; } set { } }
        bool ISchemaOptions.ReadOne(ParserContext ctx, string key) => false;
    }
    partial class FileDescriptorProto : ISchemaObject, IMessage, IType
    {
        internal static FileDescriptorProto GetFile(IType type)
        {
            while (type != null)
            {
                if (type is FileDescriptorProto) return (FileDescriptorProto)type;
                type = type.Parent;
            }
            return null;
        }
        int IMessage.MaxField => FieldDescriptorProto.DefaultMaxField;
        List<FieldDescriptorProto> IMessage.Fields => null;
        List<FieldDescriptorProto> IMessage.Extensions => Extensions;
        List<DescriptorProto> IMessage.Types => MessageTypes;

        public override string ToString() => Name;

        string IType.FullyQualifiedName => null;
        IType IType.Parent => null;
        IType IType.Find(string name)
        {
            return (IType)MessageTypes.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))
                ?? (IType)EnumTypes.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }
        internal bool HasPendingImports { get; private set; }
        internal FileDescriptorSet Parent { get; private set; }

        internal bool IncludeInOutput { get; set; }

        public bool HasImports() => _imports.Count != 0;
        internal IEnumerable<Import> GetImports(bool resetPendingFlag = false)
        {
            if (resetPendingFlag)
            {
                HasPendingImports = false;
            }
            return _imports;
        }
        readonly List<Import> _imports = new List<Import>();
        internal bool AddImport(string path, bool isPublic, Token token)
        {
            var existing = _imports.FirstOrDefault(x => string.Equals(x.Path, path, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                // we'll allow this to upgrade
                if (isPublic) existing.IsPublic = true;
                return false;
            }
            HasPendingImports = true;
            _imports.Add(new Import { Path = path, IsPublic = isPublic, Token = token });
            return true;
        }

        internal const string SyntaxProto2 = "proto2", SyntaxProto3 = "proto3";

        void ISchemaObject.ReadOne(ParserContext ctx)
        {
            var tokens = ctx.Tokens;
            Token token;
            if (tokens.ConsumeIf(TokenType.AlphaNumeric, "message"))
            {
                DescriptorProto obj;
                if (DescriptorProto.TryParse(ctx, this, out obj))
                    MessageTypes.Add(obj);
            }
            else if (tokens.ConsumeIf(TokenType.AlphaNumeric, "enum"))
            {
                EnumDescriptorProto obj;
                if (EnumDescriptorProto.TryParse(ctx, this, out obj))
                    EnumTypes.Add(obj);
            }
            else if (tokens.ConsumeIf(TokenType.AlphaNumeric, "extend"))
            {
                FieldDescriptorProto.ParseExtensions(ctx, this);
            }
            else if (tokens.ConsumeIf(TokenType.AlphaNumeric, "service"))
            {
                ServiceDescriptorProto obj;
                if (ServiceDescriptorProto.TryParse(ctx, out obj))
                    Services.Add(obj);
            }
            else if (tokens.ConsumeIf(TokenType.AlphaNumeric, "import"))
            {
                ctx.AbortState = AbortState.Statement;
                bool isPublic = tokens.ConsumeIf(TokenType.AlphaNumeric, "public");
                string path = tokens.Consume(TokenType.StringLiteral);

                if (!AddImport(path, isPublic, tokens.Previous))
                {
                    ctx.Errors.Warn(tokens.Previous, $"duplicate import: '{path}'");
                }
                tokens.Consume(TokenType.Symbol, ";");
                ctx.AbortState = AbortState.None;


            }
            else if (tokens.ConsumeIf(TokenType.AlphaNumeric, "syntax"))
            {
                ctx.AbortState = AbortState.Statement;
                if (MessageTypes.Any() || EnumTypes.Any() || Extensions.Any())
                {
                    ctx.Errors.Error(tokens.Previous, "syntax must be set before types are defined");
                }
                tokens.Consume(TokenType.Symbol, "=");
                Syntax = tokens.Consume(TokenType.StringLiteral);
                switch (Syntax)
                {
                    case SyntaxProto2:
                    case SyntaxProto3:
                        break;
                    default:
                        ctx.Errors.Error(tokens.Previous, $"unknown syntax '{Syntax}'");
                        break;
                }
                tokens.Consume(TokenType.Symbol, ";");
                ctx.AbortState = AbortState.None;
            }
            else if (tokens.ConsumeIf(TokenType.AlphaNumeric, "package"))
            {
                ctx.AbortState = AbortState.Statement;
                Package = tokens.Consume(TokenType.AlphaNumeric);
                ctx.AbortState = AbortState.None;
            }
            else if (tokens.ConsumeIf(TokenType.AlphaNumeric, "option"))
            {
                Options = ctx.ParseOptionStatement(Options, this);
            }
            else if (tokens.Peek(out token))
            {
                token.Throw();
            } // else EOF
        }

        public void Parse(TextReader schema, List<Error> errors, string file)
        {
            Syntax = "";
            using (var ctx = new ParserContext(this, new Peekable<Token>(schema.Tokenize(file).RemoveCommentsAndWhitespace()), errors))
            {
                var tokens = ctx.Tokens;
                Token startOfFile;
                tokens.Peek(out startOfFile); // want this for "stuff you didn't do" warnings

                // read the file into the object
                ctx.Fill(this);

                // finish up
                if (string.IsNullOrWhiteSpace(Syntax))
                {
                    ctx.Errors.Warn(startOfFile, "no syntax specified; it is strongly recommended to specify 'syntax=\"proto2\";' or 'syntax=\"proto3\";'");
                }
                if (Syntax == "" || Syntax == SyntaxProto2)
                {
                    Syntax = null; // for output compatibility; is blank even if set to proto2 explicitly
                }
            }
        }


        internal bool TryResolveEnum(string typeName, IType parent, out EnumDescriptorProto @enum, bool allowImports, bool treatAllAsPublic = false)
        {
            IType type;
            if (TryResolveType(typeName, parent, out type, allowImports, true, treatAllAsPublic))
            {
                @enum = type as EnumDescriptorProto;
                return @enum != null;
            }
            @enum = null;
            return false;
        }
        internal bool TryResolveMessage(string typeName, IType parent, out DescriptorProto message, bool allowImports, bool treatAllAsPublic = false)
        {
            IType type;
            if (TryResolveType(typeName, parent, out type, allowImports, true, treatAllAsPublic))
            {
                message = type as DescriptorProto;
                return message != null;
            }
            message = null;
            return false;
        }
        internal static bool TrySplit(string input, out string left, out string right)
        {
            var split = input.IndexOf('.');
            if (split < 0)
            {
                left = right = null;
                return false;
            }
            left = input.Substring(0, split).Trim();
            right = input.Substring(split + 1).Trim();
            return true;
        }
        internal static bool TrySplitLast(string input, out string left, out string right)
        {
            var split = input.LastIndexOf('.');
            if (split < 0)
            {
                left = right = null;
                return false;
            }
            left = input.Substring(0, split).Trim();
            right = input.Substring(split + 1).Trim();
            return true;
        }
		bool TryResolveFromFile(FileDescriptorProto file, string ee, string ion, out FieldDescriptorProto fld, bool withPackageName, bool ai)
		{
			fld = null;
			if (file == null) return false;

			if (withPackageName)
			{
				var pkg = file.Package;
				if (string.IsNullOrWhiteSpace(pkg)) return false; // we're only looking *with* packages right now

				if (!ion.StartsWith(pkg + ".")) return false; // wrong file

				ion = ion.Substring(pkg.Length + 1); // and fully qualified (non-qualified is a second pass)
			}

			return file.TryResolveExtension(ee, ion, out fld, ai, false);
		}
        private bool TryResolveExtension(string extendee, string extension, out FieldDescriptorProto field, bool allowImports = true, bool checkOwnPackage = true)
        {
            bool isRooted = extension.StartsWith(".");
            if (isRooted)
            {
                // rooted
                extension = extension.Substring(1); // remove the root
            }
            string left;
            string right;
            if (TrySplitLast(extension, out left, out right))
            {
                IType type;
                if (TryResolveType(left, null, out type, true, true))
                {
                    field = (type as DescriptorProto)?.Extensions?.FirstOrDefault(x => x.Extendee == extendee
                        && x.Name == right);
                    if (field != null) return true;
                }
            }
            else
            {
                field = Extensions?.FirstOrDefault(x => x.Extendee == extendee && x.Name == extension);
                if (field != null) return true;
            }

            if (checkOwnPackage)
            {
                if (TryResolveFromFile(this, extendee, extension, out field, true, false)) return true;
                if (TryResolveFromFile(this, extendee, extension, out field, false, false)) return true;
            }
            if (allowImports)
            {
                foreach (var import in _imports)
                {
                    var file = Parent?.GetFile(import.Path);
                    if (file != null)
                    {
                        if (TryResolveFromFile(file, extendee, extension, out field, true, import.IsPublic))
                        {
                            import.Used = true;
                            return true;
                        }
                    }
                }
                foreach (var import in _imports)
                {
                    var file = Parent?.GetFile(import.Path);
                    if (file != null)
                    {
                        if (TryResolveFromFile(file, extendee, extension, out field, false, import.IsPublic))
                        {
                            import.Used = true;
                            return true;
                        }
                    }
                }
            }
            field = null;
            return false;
        }
		bool TryResolveFromFile(FileDescriptorProto file, string tn, bool ai, out IType tp, bool withPackageName, bool treatAllAsPublic)
		{
			tp = null;
			if (file == null) return false;

			if (withPackageName)
			{
				var pkg = file.Package;
				if (string.IsNullOrWhiteSpace(pkg)) return false; // we're only looking *with* packages right now

				if (!tn.StartsWith(pkg + ".")) return false; // wrong file

				tn = tn.Substring(pkg.Length + 1); // and fully qualified (non-qualified is a second pass)
			}

			return file.TryResolveType(tn, file, out tp, ai, false, treatAllAsPublic);
		}
        internal bool TryResolveType(string typeName, IType parent, out IType type, bool allowImports, bool checkOwnPackage = true, bool treatAllAsPublic = false)
        {
            bool isRooted = typeName.StartsWith(".");
            string left;
            string right;
            if (isRooted)
            {
                // rooted
                typeName = typeName.Substring(1); // remove the root
            }
            else if (TrySplit(typeName, out left, out right))
            {
                while (parent != null)
                {
                    var next = parent?.Find(left);
                    if (next != null && TryResolveType(right, next, out type, false, treatAllAsPublic)) return true;

                    parent = parent.Parent;
                }
            }
            else
            {
                // simple name
                while (parent != null)
                {
                    type = parent.Find(typeName);
                    if (type != null)
                    {
                        return true;
                    }
                    parent = parent.Parent;
                }
            }

            if (checkOwnPackage && TryResolveFromFile(this, typeName, false, out type, true, treatAllAsPublic)) return true;
            if (checkOwnPackage && TryResolveFromFile(this, typeName, false, out type, false, treatAllAsPublic)) return true;

            // look at imports
            // check for the name including the package prefix
            foreach (var import in _imports)
            {
                if (allowImports || import.IsPublic || treatAllAsPublic)
                {
                    var file = Parent?.GetFile(import.Path);
                    if (TryResolveFromFile(file, typeName, false, out type, true, treatAllAsPublic))
                    {
                        import.Used = true;
                        return true;
                    }
                }
            }

            // now look without package prefix
            foreach (var import in _imports)
            {
                if (allowImports || import.IsPublic || treatAllAsPublic)
                {
                    var file = Parent?.GetFile(import.Path);
                    if (TryResolveFromFile(file, typeName, false, out type, false, treatAllAsPublic))
                    {
                        import.Used = true;
                        return true;
                    }
                }
            }

            type = null;
            return false;

        }

        static void SetParents(string prefix, EnumDescriptorProto parent)
        {
            parent.FullyQualifiedName = prefix + "." + parent.Name;
            foreach (var val in parent.Values)
            {
                val.Parent = parent;
            }
        }
        static void SetParents(string prefix, DescriptorProto parent)
        {
            var fqn = parent.FullyQualifiedName = prefix + "." + parent.Name;
            foreach (var field in parent.Fields)
            {
                field.Parent = parent;
            }
            foreach (var @enum in parent.EnumTypes)
            {
                @enum.Parent = parent;
                SetParents(fqn, @enum);
            }
            foreach (var child in parent.NestedTypes)
            {
                child.Parent = parent;
                SetParents(fqn, child);
            }
            foreach (var ext in parent.Extensions)
            {
                ext.Parent = parent;
            }
        }
        internal void BuildTypeHierarchy(FileDescriptorSet set, ParserContext ctx)
        {
            // build the tree starting at the root
            Parent = set;
            var prefix = string.IsNullOrWhiteSpace(Package) ? "" : ("." + Package);
            foreach (var type in EnumTypes)
            {
                type.Parent = this;
                SetParents(prefix, type);
            }
            foreach (var type in MessageTypes)
            {
                type.Parent = this;
                SetParents(prefix, type);
            }
            foreach (var type in Extensions)
            {
                type.Parent = this;
            }
        }

        static bool ShouldResolveType(FieldDescriptorProto.Type type)
        {
            switch (type)
            {
                case 0:
                case FieldDescriptorProto.Type.TypeMessage:
                case FieldDescriptorProto.Type.TypeEnum:
                case FieldDescriptorProto.Type.TypeGroup:
                    return true;
                default:
                    return false;
            }
        }
        private void ResolveTypes(ParserContext ctx, List<FieldDescriptorProto> fields, IType parent, bool options)
        {
            foreach (var field in fields)
            {
                if (options) ResolveOptions(ctx, field.Options);
                else
                {
                    if (!string.IsNullOrEmpty(field.TypeName) && ShouldResolveType(field.type))
                    {
                        // TODO: use TryResolveType once rather than twice
                        string fqn;
                        DescriptorProto msg;
                        EnumDescriptorProto @enum;
                        if (TryResolveMessage(field.TypeName, parent, out msg, true))
                        {
                            if (field.type != FieldDescriptorProto.Type.TypeGroup)
                            {
                                field.type = FieldDescriptorProto.Type.TypeMessage;
                            }
                            fqn = msg?.FullyQualifiedName;
                        }
                        else if (TryResolveEnum(field.TypeName, parent, out @enum, true))
                        {
                            field.type = FieldDescriptorProto.Type.TypeEnum;
                            if (!string.IsNullOrWhiteSpace(field.DefaultValue)
                                & !@enum.Values.Any(x => x.Name == field.DefaultValue))
                            {
                                ctx.Errors.Error(field.TypeToken, $"enum {@enum.Name} does not contain value '{field.DefaultValue}'");
                            }
                            fqn = @enum?.FullyQualifiedName;
                        }
                        else
                        {
                            ctx.Errors.Add(field.TypeToken.TypeNotFound(field.TypeName));
                            fqn = field.TypeName;
                            field.type = FieldDescriptorProto.Type.TypeMessage; // just an assumption
                        }
                        field.TypeName = fqn;
                    }

                    if (!string.IsNullOrEmpty(field.Extendee))
                    {
                        string fqn;
                        DescriptorProto msg;
                        if (TryResolveMessage(field.Extendee, parent, out msg, true))
                        {
                            fqn = msg?.FullyQualifiedName;
                        }
                        else
                        {
                            ctx.Errors.Add(field.TypeToken.TypeNotFound(field.Extendee));
                            fqn = field.Extendee;
                        }
                        field.Extendee = fqn;
                    }

                    if (field.Options?.Packed ?? false)
                    {
                        bool canPack = FieldDescriptorProto.CanPack(field.type);
                        if (!canPack)
                        {
                            ctx.Errors.Error(field.TypeToken, $"field of type {field.type} cannot be packed");
                            field.Options.Packed = false;
                        }
                    }
                }
            }
        }

        private void ResolveTypes(ParserContext ctx, ServiceDescriptorProto service, bool options)
        {
            if (options) ResolveOptions(ctx, service.Options);
            foreach (var method in service.Methods)
            {
                if (options) ResolveOptions(ctx, method.Options);
                else
                {
                    DescriptorProto msg;
                    if (!TryResolveMessage(method.InputType, this, out msg, true))
                    {
                        ctx.Errors.Add(method.InputTypeToken.TypeNotFound(method.InputType));
                    }
                    method.InputType = msg?.FullyQualifiedName;
                    if (!TryResolveMessage(method.OutputType, this, out msg, true))
                    {
                        ctx.Errors.Add(method.OutputTypeToken.TypeNotFound(method.OutputType));
                    }
                    method.OutputType = msg?.FullyQualifiedName;
                }
            }
        }

        private void ResolveTypes(ParserContext ctx, DescriptorProto type, bool options)
        {
            if (options)
            {
                ResolveOptions(ctx, type.Options);
                foreach (var decl in type.OneofDecls)
                    ResolveOptions(ctx, decl.Options);
            }

            ResolveTypes(ctx, type.Fields, type, options);
            ResolveTypes(ctx, type.Extensions, type, options);
            foreach (var nested in type.NestedTypes)
            {
                ResolveTypes(ctx, nested, options);
            }
            foreach (var nested in type.EnumTypes)
            {
                ResolveTypes(ctx, nested, options);
            }
        }


        IEnumerable<string> IHazNames.GetNames()
        {
            foreach (var type in MessageTypes) yield return type.Name;
            foreach (var type in EnumTypes) yield return type.Name;
        }
        internal void ResolveTypes(ParserContext ctx, bool options)
        {
            if (options) ResolveOptions(ctx, Options);
            foreach (var type in MessageTypes)
            {
                ResolveTypes(ctx, type, options);
            }
            foreach (var type in EnumTypes)
            {
                ResolveTypes(ctx, type, options);
            }
            foreach (var service in Services)
            {
                ResolveTypes(ctx, service, options);
            }
            ResolveTypes(ctx, Extensions, this, options);

            if (options) // can only process deps on the second pass, once options have been resolved
            {
                HashSet<string> publicDependencies = null;
                foreach (var import in _imports)
                {
                    if (!Dependencies.Contains(import.Path))
                        Dependencies.Add(import.Path);
                    if (import.IsPublic)
                    {
                        (publicDependencies ?? (publicDependencies = new HashSet<string>())).Add(import.Path);
                    }
                    if (IncludeInOutput && !import.Used)
                    {
                        ctx.Errors.Warn(import.Token, $"import not used: '{import.Path}'");
                    }
                }
                // note that Dependencies should stay in declaration order to be consistent with protoc
                if (publicDependencies != null)
                {
                    var arr = publicDependencies.Select(path => Dependencies.IndexOf(path)).ToArray();
                    Array.Sort(arr);
                    PublicDependencies = arr;
                }
            }
        }

        private void ResolveTypes(ParserContext ctx, EnumDescriptorProto type, bool options)
        {
            if (options)
            {
                ResolveOptions(ctx, type.Options);
                foreach (var val in type.Values)
                {
                    ResolveOptions(ctx, val.Options);
                }
            }
        }

        private void ResolveOptions(ParserContext ctx, ISchemaOptions options)
        {
            if (options == null || options.UninterpretedOptions.Count == 0) return;

            var extension = ((IExtensible)options).GetExtensionObject(true);
            var target = extension.BeginAppend();
            try
            {
                using (var writer = new ProtoWriter(target, null, null))
                {
                    var hive = OptionHive.Build(options.UninterpretedOptions);

                    // first pass is used to sort the fields so we write them in the right order
                    AppendOptions(this, writer, ctx, options.Extendee, hive.Children, true, 0, false);
                    // second pass applies the data
                    AppendOptions(this, writer, ctx, options.Extendee, hive.Children, false, 0, false);
                }
                options.UninterpretedOptions.RemoveAll(x => x.Applied);
            }
            finally
            {
                extension.EndAppend(target, true);
            }

        }

        class OptionHive
        {
            public OptionHive(string name, bool isExtension, Token token)
            {
                Name = name;
                IsExtension = isExtension;
                Token = token;
            }
            public override string ToString()
            {
                var sb = new StringBuilder();
                Concat(sb);
                return sb.ToString();
            }
            private void Concat(StringBuilder sb)
            {
                bool isFirst = true;
                foreach (var value in Options)
                {
                    if (!isFirst) sb.Append(", ");
                    isFirst = false;
                    sb.Append(value);
                }
                foreach (var child in Children)
                {
                    if (!isFirst) sb.Append(", ");
                    sb.Append(child.Name).Append("={");
                    child.Concat(sb);
                    sb.Append("}");
                }
            }
            public bool IsExtension { get; }
            public string Name { get; }
            public Token Token { get; }
            public List<UninterpretedOption> Options { get; } = new List<UninterpretedOption>();
            public List<OptionHive> Children { get; } = new List<OptionHive>();
            public FieldDescriptorProto Field { get; set; }

            public static OptionHive Build(List<UninterpretedOption> options)
            {
                if (options == null || options.Count == 0) return null;

                var root = new OptionHive(null, false, default(Token));
                foreach (var option in options)
                {
                    var level = root;
                    OptionHive nextLevel = null;
                    foreach (var name in option.Names)
                    {
                        nextLevel = level.Children.FirstOrDefault(x => x.Name == name.name_part && x.IsExtension == name.IsExtension);
                        if (nextLevel == null)
                        {
                            nextLevel = new OptionHive(name.name_part, name.IsExtension, name.Token);
                            level.Children.Add(nextLevel);
                        }
                        level = nextLevel;
                    }
                    level.Options.Add(option);
                }
                return root;
            }
        }
        private static void AppendOptions(FileDescriptorProto file, ProtoWriter writer, ParserContext ctx, string extendee, List<OptionHive> options, bool resolveOnly, int depth, bool messageSet)
        {
            foreach (var option in options)
                AppendOption(file, writer, ctx, extendee, option, resolveOnly, depth, messageSet);

            if (resolveOnly && depth != 0) // fun fact: proto writes root fields in *file* order, but sub-fields in *field* order
            {
                // ascending field order
                options.Sort((x, y) => (x.Field?.Number ?? 0).CompareTo(y.Field?.Number ?? 0));
            }
        }
        private static bool ShouldWrite(FieldDescriptorProto f, string v, string d){
            return f.label != FieldDescriptorProto.Label.LabelOptional || v != (f.DefaultValue ?? d);
        }
        private static void AppendOption(FileDescriptorProto file, ProtoWriter writer, ParserContext ctx, string extendee, OptionHive option, bool resolveOnly, int depth, bool messageSet)
        {
            // resolve the field for this level
            FieldDescriptorProto field = option.Field;
            DescriptorProto msg;
            if (field != null)
            {
                // already resolved
            }
            else if (option.IsExtension)
            {
                if (!file.TryResolveExtension(extendee, option.Name, out field)) field = null;
            }
            else if (file.TryResolveMessage(extendee, null, out msg, true))
            {
                field = msg.Fields.FirstOrDefault(x => x.Name == option.Name);
            }
            else
            {
                field = null;
            }

            if (field == null)
            {
                if (!resolveOnly)
                {
                    ctx.Errors.Error(option.Token, $"unable to resolve custom option '{option.Name}' for '{extendee}'");
                }
                return;
            }
            option.Field = field;

            switch (field.type)
            {
                case FieldDescriptorProto.Type.TypeMessage:
                case FieldDescriptorProto.Type.TypeGroup:
                    var nextFile = GetFile(field.Parent as IType);
                    DescriptorProto fieldType;
                    var nextMessageSet = !resolveOnly && nextFile.TryResolveMessage(field.TypeName, null, out fieldType, true)
                        && (fieldType.Options?.MessageSetWireFormat ?? false);

                    if (option.Children.Count != 0)
                    {
                        if (resolveOnly)
                        {
                            AppendOptions(nextFile, writer, ctx, field.TypeName, option.Children, resolveOnly, depth + 1, nextMessageSet);
                        }
                        else if (messageSet)
                        {
                            ProtoWriter.WriteFieldHeader(1, WireType.StartGroup, writer);
                            var grp = ProtoWriter.StartSubItem(null, writer);

                            ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
                            ProtoWriter.WriteInt32(field.Number, writer);

                            ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
                            var payload = ProtoWriter.StartSubItem(null, writer);

                            AppendOptions(nextFile, writer, ctx, field.TypeName, option.Children, resolveOnly, depth + 1, nextMessageSet);

                            ProtoWriter.EndSubItem(payload, writer);
                            ProtoWriter.EndSubItem(grp, writer);
                        }
                        else
                        {
                            ProtoWriter.WriteFieldHeader(field.Number,
                                field.type == FieldDescriptorProto.Type.TypeGroup ? WireType.StartGroup : WireType.String, writer);
                            var tok = ProtoWriter.StartSubItem(null, writer);

                            AppendOptions(nextFile, writer, ctx, field.TypeName, option.Children, resolveOnly, depth + 1, nextMessageSet);

                            ProtoWriter.EndSubItem(tok, writer);
                        }
                    }
                    if (resolveOnly) return; // nothing more to do

                    if (option.Options.Count == 1 && !option.Options.Single().ShouldSerializeAggregateValue())
                    {
                        // need to write an empty object to match protoc
                        if (messageSet)
                        {
                            ProtoWriter.WriteFieldHeader(1, WireType.StartGroup, writer);
                            var grp = ProtoWriter.StartSubItem(null, writer);

                            ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
                            ProtoWriter.WriteInt32(field.Number, writer);

                            ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
                            var payload = ProtoWriter.StartSubItem(null, writer);
                            ProtoWriter.EndSubItem(payload, writer);
                            ProtoWriter.EndSubItem(grp, writer);
                        }
                        else
                        {
                            ProtoWriter.WriteFieldHeader(field.Number,
                                   field.type == FieldDescriptorProto.Type.TypeGroup ? WireType.StartGroup : WireType.String, writer);
                            var payload = ProtoWriter.StartSubItem(null, writer);
                            ProtoWriter.EndSubItem(payload, writer);
                        }
                        option.Options.Single().Applied = true;
                    }
                    else
                    {
                        foreach (var values in option.Options)
                        {
                            ctx.Errors.Error(option.Token, $"unable to assign custom option '{option.Name}' for '{extendee}'");
                        }
                    }
                    break;
                default:
                    if (resolveOnly) return; // nothing more to do

                    foreach (var child in option.Children)
                    {
                        ctx.Errors.Error(option.Token, $"unable to assign custom option '{child.Name}' for '{extendee}'");
                    }
                    foreach (var value in option.Options)
                    {
                        int i32;
                        switch (field.type)
                        {
                            case FieldDescriptorProto.Type.TypeFloat:
                                float f32;
                                if (!TokenExtensions.TryParseSingle(value.AggregateValue, out f32))
                                {
                                    ctx.Errors.Error(option.Token, $"invalid value for floating point '{field.TypeName}': '{option.Name}' = '{value.AggregateValue}'");
                                    continue;
                                }
                                if (ShouldWrite(field, value.AggregateValue, "0"))
                                {
                                    ProtoWriter.WriteFieldHeader(field.Number, WireType.Fixed32, writer);
                                    ProtoWriter.WriteSingle(f32, writer);
                                }
                                break;
                            case FieldDescriptorProto.Type.TypeDouble:
                                double f64;
                                if (!TokenExtensions.TryParseDouble(value.AggregateValue, out f64))
                                {
                                    ctx.Errors.Error(option.Token, $"invalid value for floating point '{field.TypeName}': '{option.Name}' = '{value.AggregateValue}'");
                                    continue;
                                }
                                if (ShouldWrite(field, value.AggregateValue, "0"))
                                {
                                    ProtoWriter.WriteFieldHeader(field.Number, WireType.Fixed64, writer);
                                    ProtoWriter.WriteDouble(f64, writer);
                                }
                                break;
                            case FieldDescriptorProto.Type.TypeBool:
                                switch (value.AggregateValue)
                                {
                                    case "true":
                                        i32 = 1;
                                        break;
                                    case "false":
                                        i32 = 0;
                                        break;
                                    default:
                                        ctx.Errors.Error(option.Token, $"invalid value for boolean '{field.TypeName}': '{option.Name}' = '{value.AggregateValue}'");
                                        continue;
                                }
                                if (ShouldWrite(field, value.AggregateValue, "false"))
                                {
                                    ProtoWriter.WriteFieldHeader(field.Number, WireType.Variant, writer);
                                    ProtoWriter.WriteInt32(i32, writer);
                                }
                                break;
                            case FieldDescriptorProto.Type.TypeUint32:
                            case FieldDescriptorProto.Type.TypeFixed32:
                                {
                                    uint ui32;
                                    if (!TokenExtensions.TryParseUInt32(value.AggregateValue, out ui32))
                                    {
                                        ctx.Errors.Error(option.Token, $"invalid value for unsigned integer '{field.TypeName}': '{option.Name}' = '{value.AggregateValue}'");
                                        continue;
                                    }
                                    if (ShouldWrite(field, value.AggregateValue, "0"))
                                    {
                                        switch (field.type)
                                        {
                                            case FieldDescriptorProto.Type.TypeUint32:
                                                ProtoWriter.WriteFieldHeader(field.Number, WireType.Variant, writer);
                                                break;
                                            case FieldDescriptorProto.Type.TypeFixed32:
                                                ProtoWriter.WriteFieldHeader(field.Number, WireType.Fixed32, writer);
                                                break;
                                        }
                                        ProtoWriter.WriteUInt32(ui32, writer);
                                    }
                                }
                                break;
                            case FieldDescriptorProto.Type.TypeUint64:
                            case FieldDescriptorProto.Type.TypeFixed64:
                                {
                                    ulong ui64;
                                    if (!TokenExtensions.TryParseUInt64(value.AggregateValue, out ui64))
                                    {
                                        ctx.Errors.Error(option.Token, $"invalid value for unsigned integer '{field.TypeName}': '{option.Name}' = '{value.AggregateValue}'");
                                        continue;
                                    }
                                    if (ShouldWrite(field, value.AggregateValue, "0"))
                                    {
                                        switch (field.type)
                                        {
                                            case FieldDescriptorProto.Type.TypeUint64:
                                                ProtoWriter.WriteFieldHeader(field.Number, WireType.Variant, writer);
                                                break;
                                            case FieldDescriptorProto.Type.TypeFixed64:
                                                ProtoWriter.WriteFieldHeader(field.Number, WireType.Fixed64, writer);
                                                break;
                                        }
                                        ProtoWriter.WriteUInt64(ui64, writer);
                                    }
                                }
                                break;
                            case FieldDescriptorProto.Type.TypeInt32:
                            case FieldDescriptorProto.Type.TypeSint32:
                            case FieldDescriptorProto.Type.TypeSfixed32:
                                if (!TokenExtensions.TryParseInt32(value.AggregateValue, out i32))
                                {
                                    ctx.Errors.Error(option.Token, $"invalid value for integer '{field.TypeName}': '{option.Name}' = '{value.AggregateValue}'");
                                    continue;
                                }
                                if (ShouldWrite(field, value.AggregateValue, "0"))
                                {
                                    switch (field.type)
                                    {
                                        case FieldDescriptorProto.Type.TypeInt32:
                                            ProtoWriter.WriteFieldHeader(field.Number, WireType.Variant, writer);
                                            break;
                                        case FieldDescriptorProto.Type.TypeSint32:
                                            ProtoWriter.WriteFieldHeader(field.Number, WireType.SignedVariant, writer);
                                            break;
                                        case FieldDescriptorProto.Type.TypeSfixed32:
                                            ProtoWriter.WriteFieldHeader(field.Number, WireType.Fixed32, writer);
                                            break;
                                    }
                                    ProtoWriter.WriteInt32(i32, writer);
                                }
                                break;
                            case FieldDescriptorProto.Type.TypeInt64:
                            case FieldDescriptorProto.Type.TypeSint64:
                            case FieldDescriptorProto.Type.TypeSfixed64:
                                {
                                    long i64;
                                    if (!TokenExtensions.TryParseInt64(value.AggregateValue, out i64))
                                    {
                                        ctx.Errors.Error(option.Token, $"invalid value for integer '{field.TypeName}': '{option.Name}' = '{value.AggregateValue}'");
                                        continue;
                                    }
                                    if (ShouldWrite(field, value.AggregateValue, "0"))
                                    {
                                        switch (field.type)
                                        {
                                            case FieldDescriptorProto.Type.TypeInt64:
                                                ProtoWriter.WriteFieldHeader(field.Number, WireType.Variant, writer);
                                                break;
                                            case FieldDescriptorProto.Type.TypeSint64:
                                                ProtoWriter.WriteFieldHeader(field.Number, WireType.SignedVariant, writer);
                                                break;
                                            case FieldDescriptorProto.Type.TypeSfixed64:
                                                ProtoWriter.WriteFieldHeader(field.Number, WireType.Fixed64, writer);
                                                break;
                                        }
                                        ProtoWriter.WriteInt64(i64, writer);
                                    }
                                }
                                break;
                            case FieldDescriptorProto.Type.TypeEnum:
                                EnumDescriptorProto @enum;
                                if (file.TryResolveEnum(field.TypeName, null, out @enum, true, true))
                                {
                                    var found = @enum.Values.FirstOrDefault(x => x.Name == value.AggregateValue);
                                    if (found == null)
                                    {
                                        ctx.Errors.Error(option.Token, $"invalid value for enum '{field.TypeName}': '{option.Name}' = '{value.AggregateValue}'");
                                        continue;
                                    }
                                    else
                                    {
                                        if (ShouldWrite(field, value.AggregateValue, @enum.Values.FirstOrDefault()?.Name))
                                        {
                                            ProtoWriter.WriteFieldHeader(field.Number, WireType.Variant, writer);
                                            ProtoWriter.WriteInt32(found.Number, writer);
                                        }
                                    }
                                }
                                else
                                {
                                    ctx.Errors.Error(option.Token, $"unable to resolve enum '{field.TypeName}': '{option.Name}' = '{value.AggregateValue}'");
                                    continue;
                                }
                                break;
                            case FieldDescriptorProto.Type.TypeString:
                            case FieldDescriptorProto.Type.TypeBytes:
                                if (ShouldWrite(field, value.AggregateValue, ""))
                                {
                                    ProtoWriter.WriteFieldHeader(field.Number, WireType.String, writer);
                                    if (value.AggregateValue == null || value.AggregateValue.IndexOf('\\') < 0)
                                        ProtoWriter.WriteString(value.AggregateValue ?? "", writer);
                                    else
                                    {
                                        using (var ms = new MemoryStream(value.AggregateValue.Length))
                                        {
                                            if (!LoadBytes(ms, value.AggregateValue))
                                            {
                                                ctx.Errors.Error(option.Token, $"invalid escape sequence '{field.TypeName}': '{option.Name}' = '{value.AggregateValue}'");
                                                continue;
                                            }
#if NETSTANDARD1_3
                                            if (ms.TryGetBuffer(out var seg))
                                                ProtoWriter.WriteBytes(seg.Array, seg.Offset, seg.Count, writer);
                                            else
                                                ProtoWriter.WriteBytes(ms.ToArray(), writer);
#else
                                            ProtoWriter.WriteBytes(ms.GetBuffer(), 0, (int)ms.Length, writer);
#endif
                                        }
                                    }
                                }
                                break;
                            default:
                                ctx.Errors.Error(option.Token, $"{field.type} options not yet implemented: '{option.Name}' = '{value.AggregateValue}'");
                                continue;
                        }
                        value.Applied = true;
                    }
                    break;
            }
        }

        private static unsafe bool LoadBytes(Stream ms, string value)
        {
            bool isEscaped = false;
            byte* b = stackalloc byte[10];
            foreach (char c in value)
            {
                if (isEscaped)
                {
                    isEscaped = false;
                    // only a few things remain escaped after ConsumeString:
                    switch (c)
                    {
                        case '\\': ms.WriteByte((byte)'\\'); break;
                        case '\'': ms.WriteByte((byte)'\''); break;
                        case '"': ms.WriteByte((byte)'"'); break;
                        case 'r': ms.WriteByte((byte)'\r'); break;
                        case 'n': ms.WriteByte((byte)'\n'); break;
                        case 't': ms.WriteByte((byte)'\t'); break;
                        default: return false;
                    }
                }
                else if (c == '\\')
                {
                    isEscaped = true;
                }
                else
                {
                    var x = c; // can't take address of readonly local
                    int bytes = Encoding.UTF8.GetBytes(&x, 1, b, 10);
                    for (int i = 0; i < bytes; i++)
                    {
                        ms.WriteByte(b[i]);
                    }
                }
            }
            return !isEscaped;
        }
    }

    partial class EnumDescriptorProto : ISchemaObject, IType
    {
        public override string ToString() => Name;
        internal IType Parent { get; set; }
        string IType.FullyQualifiedName => FullyQualifiedName;
        IType IType.Parent => Parent;
        IType IType.Find(string name) => null;
        internal string FullyQualifiedName { get; set; }

        internal static bool TryParse(ParserContext ctx, IHazNames parent, out EnumDescriptorProto obj)
        {
            var name = ctx.Tokens.Consume(TokenType.AlphaNumeric);
            ctx.CheckNames(parent, name, ctx.Tokens.Previous);
            if (ctx.TryReadObject(out obj))
            {
                obj.Name = name;
                return true;
            }
            return false;
        }

        void ISchemaObject.ReadOne(ParserContext ctx)
        {
            ctx.AbortState = AbortState.Statement;
            var tokens = ctx.Tokens;
            if (tokens.ConsumeIf(TokenType.AlphaNumeric, "option"))
            {
                Options = ctx.ParseOptionStatement(Options, this);
            }
            else
            {
                Values.Add(EnumValueDescriptorProto.Parse(ctx));
            }
            ctx.AbortState = AbortState.None;
        }

    }
    partial class FieldDescriptorProto : ISchemaObject
    {

        public bool IsPacked(string syntax)
        {
            if (label != Label.LabelRepeated) return false;

            var exp = Options?.Packed;
            if (exp.HasValue) return exp.GetValueOrDefault();

            if (syntax != FileDescriptorProto.SyntaxProto2 && FieldDescriptorProto.CanPack(type))
            {
                return true;
            }

            return false;
        }
        public override string ToString() => Name;
        internal const int DefaultMaxField = 536870911;
        internal const int FirstReservedField = 19000;
        internal const int LastReservedField = 19999;

        internal IMessage Parent { get; set; }
        internal Token TypeToken { get; set; }

        internal int MaxField => Parent?.MaxField ?? DefaultMaxField;

		internal static void NotAllowedOneOf(ParserContext ctx)
		{
			var token = ctx.Tokens.Previous;
			ctx.Errors.Error(token, $"'{token.Value}' not allowed with 'oneof'");
		}

        internal static bool TryParse(ParserContext ctx, IMessage parent, bool isOneOf, out FieldDescriptorProto field)
        {
            var tokens = ctx.Tokens;
            ctx.AbortState = AbortState.Statement;
            Label label = Label.LabelOptional; // default

            if (tokens.ConsumeIf(TokenType.AlphaNumeric, "repeated"))
            {
                if (isOneOf) NotAllowedOneOf(ctx);
                label = Label.LabelRepeated;
            }
            else if (tokens.ConsumeIf(TokenType.AlphaNumeric, "required"))
            {
                if (isOneOf) NotAllowedOneOf(ctx);
                else tokens.Previous.RequireProto2(ctx);
                label = Label.LabelRequired;
            }
            else if (tokens.ConsumeIf(TokenType.AlphaNumeric, "optional"))
            {
                if (isOneOf) NotAllowedOneOf(ctx);
                else tokens.Previous.RequireProto2(ctx);
                label = Label.LabelOptional;
            }
            else if (ctx.Syntax == FileDescriptorProto.SyntaxProto2 && !isOneOf)
            {
                // required in proto2
                throw tokens.Read().Throw("expected 'repeated' / 'required' / 'optional'");
            }

            var typeToken = tokens.Read();
            if (typeToken.Is(TokenType.AlphaNumeric, "map"))
            {
                tokens.Previous.Throw($"'{tokens.Previous.Value}' can not be used with 'map'");
            }
            string typeName = tokens.Consume(TokenType.AlphaNumeric);

            var isGroup = typeName == "group";
            if (isGroup)
            {
                //if (isOneOf) NotAllowedOneOf(ctx);
                //else if (parentTyped == null)
                //{
                //    ctx.Errors.Error(tokens.Previous, "group not allowed in this context");
                //}
                ctx.AbortState = AbortState.Object;
            }

            string name = tokens.Consume(TokenType.AlphaNumeric);
            var nameToken = tokens.Previous;
            tokens.Consume(TokenType.Symbol, "=");
            var number = tokens.ConsumeInt32();
            var numberToken = tokens.Previous;

            if (number < 1 || number > parent.MaxField)
            {
                ctx.Errors.Error(numberToken, $"field numbers must be in the range 1-{parent.MaxField}");
            }
            else if (number >= FirstReservedField && number <= LastReservedField)
            {
                ctx.Errors.Warn(numberToken, $"field numbers in the range {FirstReservedField}-{LastReservedField} are reserved; this may cause problems on many implementations");
            }
            ctx.CheckNames(parent, name, nameToken);
            if (parent is DescriptorProto)
            {
                var parentTyped = parent as DescriptorProto;
                var conflict = parentTyped.Fields.FirstOrDefault(x => x.Number == number);
                if (conflict != null)
                {
                    ctx.Errors.Error(numberToken, $"field {number} is already in use by '{conflict.Name}'");
                }
                if (parentTyped.ReservedNames.Contains(name))
                {
                    ctx.Errors.Error(nameToken, $"field '{name}' is reserved");
                }
                if (parentTyped.ReservedRanges.Any(x => x.Start <= number && x.End > number))
                {
                    ctx.Errors.Error(numberToken, $"field {number} is reserved");
                }
            }

            Type type;
            if (isGroup)
            {
                type = Type.TypeGroup;
                typeName = name;

                typeToken.RequireProto2(ctx);

                var firstChar = typeName[0].ToString();
                if (firstChar.ToLowerInvariant() == firstChar)
                {
                    ctx.Errors.Error(nameToken, "group names must start with an upper-case letter");
                }
                name = typeName.ToLowerInvariant();
                DescriptorProto grpType;
                if (ctx.TryReadObject<DescriptorProto>(out grpType))
                {
                    grpType.Name = typeName;
                    ctx.CheckNames(parent, typeName, nameToken);
                    parent?.Types?.Add(grpType);
                }
            }
            else if (TryIdentifyType(typeName, out type))
            {
                typeName = null;
            }

            field = new FieldDescriptorProto
            {
                type = type,
                TypeName = typeName,
                Name = name,
                JsonName = GetJsonName(name),
                Number = number,
                label = label,
                TypeToken = typeToken // internal property that helps give useful error messages
            };

            if (!isGroup)
            {
                if (tokens.ConsumeIf(TokenType.Symbol, "["))
                {
                    field.Options = ctx.ParseOptionBlock(field.Options, field);
                }

                tokens.Consume(TokenType.Symbol, ";");
            }
            ctx.AbortState = AbortState.None;
            return true;
        }
        static readonly char[] Underscores = { '_' };
        internal static string GetJsonName(string name)
            => Regex.Replace(name, "_+([0-9a-zA-Z])", match => match.Groups[1].Value.ToUpperInvariant()).TrimEnd(Underscores);


        internal static bool CanPack(Type type)
        {
            switch (type)
            {
                case Type.TypeBool:
                case Type.TypeDouble:
                case Type.TypeEnum:
                case Type.TypeFixed32:
                case Type.TypeFixed64:
                case Type.TypeFloat:
                case Type.TypeInt32:
                case Type.TypeInt64:
                case Type.TypeSfixed32:
                case Type.TypeSfixed64:
                case Type.TypeSint32:
                case Type.TypeSint64:
                case Type.TypeUint32:
                case Type.TypeUint64:
                    return true;
                default:
                    return false;
            }
        }
		internal static bool Assign(Type @in, out Type @out)
		{
			@out = @in;
			return true;
		}
        internal static bool TryIdentifyType(string typeName, out Type type)
        {
            switch (typeName)
            {
                case "bool": return Assign(Type.TypeBool, out @type);
                case "bytes": return Assign(Type.TypeBytes, out @type);
                case "double": return Assign(Type.TypeDouble, out @type);
                case "fixed32": return Assign(Type.TypeFixed32, out @type);
                case "fixed64": return Assign(Type.TypeFixed64, out @type);
                case "float": return Assign(Type.TypeFloat, out @type);
                case "int32": return Assign(Type.TypeInt32, out @type);
                case "int64": return Assign(Type.TypeInt64, out @type);
                case "sfixed32": return Assign(Type.TypeSfixed32, out @type);
                case "sfixed64": return Assign(Type.TypeSfixed64, out @type);
                case "sint32": return Assign(Type.TypeSint32, out @type);
                case "sint64": return Assign(Type.TypeSint64, out @type);
                case "string": return Assign(Type.TypeString, out @type);
                case "uint32": return Assign(Type.TypeUint32, out @type);
                case "uint64": return Assign(Type.TypeUint64, out @type);
                default:
                    type = default(Type);
                    return false;
            }
        }

        internal static void ParseExtensions(ParserContext ctx, IMessage message)
        {
            var extendee = ctx.Tokens.Consume(TokenType.AlphaNumeric);
            var dummy = new DummyExtensions(extendee, message);
            ctx.TryReadObjectImpl(dummy);
        }

        void ISchemaObject.ReadOne(ParserContext ctx)
        {
            throw new InvalidOperationException();
        }

        class DummyExtensions : ISchemaObject, IHazNames, IMessage
        {
            int IMessage.MaxField => message.MaxField;
            List<DescriptorProto> IMessage.Types => message.Types;
            List<FieldDescriptorProto> IMessage.Extensions => message.Extensions;
            List<FieldDescriptorProto> IMessage.Fields => message.Fields;
            public byte[] ExtensionData
            {
                get { return null; }
                set { }
            }
            IEnumerable<string> IHazNames.GetNames()
            {
                var fields = message.Fields;
                if (fields != null)
                {
                    foreach (var field in fields) yield return field.Name;
                }
                foreach (var field in message.Extensions) yield return field.Name;
                foreach (var type in message.Types) yield return type.Name;
            }

            void ISchemaObject.ReadOne(ParserContext ctx)
            {
                ctx.AbortState = AbortState.Statement;
                FieldDescriptorProto field;
                if (TryParse(ctx, this, false, out field))
                {
                    field.Extendee = extendee;
                    message.Extensions.Add(field);
                }
                ctx.AbortState = AbortState.None;
            }

            private IMessage message;
            private string extendee;

            public DummyExtensions(string extendee, IMessage message)
            {
                this.extendee = extendee;
                this.message = message;
            }
        }
    }

    internal interface IMessage : IHazNames
    {
        int MaxField { get; }
        List<DescriptorProto> Types { get; }
        List<FieldDescriptorProto> Extensions { get; }
        List<FieldDescriptorProto> Fields { get; }
    }

    partial class ServiceDescriptorProto : ISchemaObject
    {
        internal static bool TryParse(ParserContext ctx, out ServiceDescriptorProto obj)
        {
            var name = ctx.Tokens.Consume(TokenType.AlphaNumeric);
            if (ctx.TryReadObject(out obj))
            {
                obj.Name = name;
                return true;
            }
            return false;
        }
        void ISchemaObject.ReadOne(ParserContext ctx)
        {
            ctx.AbortState = AbortState.Statement;
            var tokens = ctx.Tokens;

            if (tokens.ConsumeIf(TokenType.AlphaNumeric, "option"))
            {
                Options = ctx.ParseOptionStatement(Options, this);
            }
            else
            {
                // is a method
                Methods.Add(MethodDescriptorProto.Parse(ctx));
            }
            ctx.AbortState = AbortState.None;
        }
    }

    partial class MethodDescriptorProto : ISchemaObject
    {
        internal Token InputTypeToken { get; set; }
        internal Token OutputTypeToken { get; set; }

        internal static MethodDescriptorProto Parse(ParserContext ctx)
        {
            var tokens = ctx.Tokens;
            tokens.Consume(TokenType.AlphaNumeric, "rpc");
            var name = tokens.Consume(TokenType.AlphaNumeric);
            tokens.Consume(TokenType.Symbol, "(");
            bool isInputStream = tokens.ConsumeIf(TokenType.AlphaNumeric, "stream");
            var inputTypeToken = tokens.Read();
            var inputType = tokens.Consume(TokenType.AlphaNumeric);
            tokens.Consume(TokenType.Symbol, ")");
            tokens.Consume(TokenType.AlphaNumeric, "returns");
            tokens.Consume(TokenType.Symbol, "(");
            bool isOutputStream = tokens.ConsumeIf(TokenType.AlphaNumeric, "stream");
            var outputTypeToken = tokens.Read();
            var outputType = tokens.Consume(TokenType.AlphaNumeric);
            tokens.Consume(TokenType.Symbol, ")");

            var method = new MethodDescriptorProto
            {
                Name = name,
                InputType = inputType,
                OutputType = outputType,
                InputTypeToken = inputTypeToken,
                OutputTypeToken = outputTypeToken
            };
            if (isInputStream) method.ClientStreaming = true;
            if (isOutputStream) method.ServerStreaming = true;
            Token token;
            if (tokens.Peek(out token) && token.Is(TokenType.Symbol, "{"))
            {
                ctx.AbortState = AbortState.Object;
                ctx.TryReadObjectImpl(method);
            }
            else
            {
                tokens.Consume(TokenType.Symbol, ";");
            }
            return method;
        }

        void ISchemaObject.ReadOne(ParserContext ctx)
        {
            ctx.Tokens.Consume(TokenType.AlphaNumeric, "option");
            Options = ctx.ParseOptionStatement(Options, this);
        }
    }

    partial class EnumValueDescriptorProto
    {
        internal static EnumValueDescriptorProto Parse(ParserContext ctx)
        {
            var tokens = ctx.Tokens;
            string name = tokens.Consume(TokenType.AlphaNumeric);
            tokens.Consume(TokenType.Symbol, "=");
            var value = tokens.ConsumeInt32();

            var obj = new EnumValueDescriptorProto { Name = name, Number = value };
            if (tokens.ConsumeIf(TokenType.Symbol, "["))
            {
                obj.Options = ctx.ParseOptionBlock(obj.Options);
            }
            tokens.Consume(TokenType.Symbol, ";");
            return obj;
        }
        internal EnumDescriptorProto Parent { get; set; }

    }
    partial class MessageOptions : ISchemaOptions
    {
        string ISchemaOptions.Extendee => FileDescriptorSet.Namespace + nameof(MessageOptions);
        bool ISchemaOptions.ReadOne(ParserContext ctx, string key)
        {
            switch (key)
            {
                case "map_entry":
                    MapEntry = ctx.Tokens.ConsumeBoolean();
                    ctx.Errors.Error(ctx.Tokens.Previous, "'map_entry' should not be set explicitly; use 'map<TKey,TValue>' instead");
                    return true;
                case "message_set_wire_format": MessageSetWireFormat = ctx.Tokens.ConsumeBoolean(); return true;
                case "no_standard_descriptor_accessor": NoStandardDescriptorAccessor = ctx.Tokens.ConsumeBoolean(); return true;
                default: return false;
            }
        }
        public byte[] ExtensionData
        {
            get { return DescriptorProto.GetExtensionData(this); }
            set { DescriptorProto.SetExtensionData(this, value); }
        }
    }
    partial class MethodOptions : ISchemaOptions
    {
        string ISchemaOptions.Extendee => FileDescriptorSet.Namespace + nameof(MethodOptions);
        bool ISchemaOptions.ReadOne(ParserContext ctx, string key)
        {
            switch (key)
            {
                case "idempotency_level": idempotency_level = ctx.Tokens.ConsumeEnum<IdempotencyLevel>(); return true;
                default: return false;
            }
        }
        public byte[] ExtensionData
        {
            get { return DescriptorProto.GetExtensionData(this); }
            set { DescriptorProto.SetExtensionData(this, value); }
        }
    }
    partial class ServiceOptions : ISchemaOptions
    {
        string ISchemaOptions.Extendee => FileDescriptorSet.Namespace + nameof(ServiceOptions);
        bool ISchemaOptions.ReadOne(ParserContext ctx, string key) => false;

        public byte[] ExtensionData
        {
            get { return DescriptorProto.GetExtensionData(this); }
            set { DescriptorProto.SetExtensionData(this, value); }
        }
    }

    partial class UninterpretedOption
    {
        partial class NamePart
        {
            public override string ToString() => IsExtension ? ("(" + name_part + ")") : name_part;
            internal Token Token { get; set; }
        }
        internal bool Applied { get; set; }
        internal Token Token { get; set; }
    }
    partial class EnumOptions : ISchemaOptions
    {
        string ISchemaOptions.Extendee => FileDescriptorSet.Namespace + nameof(EnumOptions);
        bool ISchemaOptions.ReadOne(ParserContext ctx, string key)
        {
            switch (key)
            {
                case "allow_alias": AllowAlias = ctx.Tokens.ConsumeBoolean(); return true;
                default: return false;
            }
        }
        public byte[] ExtensionData
        {
            get { return DescriptorProto.GetExtensionData(this); }
            set { DescriptorProto.SetExtensionData(this, value); }
        }
    }
    partial class EnumValueOptions : ISchemaOptions
    {
        string ISchemaOptions.Extendee => FileDescriptorSet.Namespace + nameof(EnumValueOptions);
        bool ISchemaOptions.ReadOne(ParserContext ctx, string key) => false;

        public byte[] ExtensionData
        {
            get { return DescriptorProto.GetExtensionData(this); }
            set { DescriptorProto.SetExtensionData(this, value); }
        }
    }
    partial class FieldOptions : ISchemaOptions
    {
        string ISchemaOptions.Extendee => FileDescriptorSet.Namespace + nameof(FieldOptions);
        bool ISchemaOptions.ReadOne(ParserContext ctx, string key)
        {
            switch (key)
            {
                case "jstype": Jstype = ctx.Tokens.ConsumeEnum<JSType>(); return true;
                case "ctype": Ctype = ctx.Tokens.ConsumeEnum<CType>(); return true;
                case "lazy": Lazy = ctx.Tokens.ConsumeBoolean(); return true;
                case "packed": Packed = ctx.Tokens.ConsumeBoolean(); return true;
                case "weak": Weak = ctx.Tokens.ConsumeBoolean(); return true;
                default: return false;
            }
        }

        public byte[] ExtensionData
        {
            get { return DescriptorProto.GetExtensionData(this); }
            set { DescriptorProto.SetExtensionData(this, value); }
        }
    }
    partial class FileOptions : ISchemaOptions
    {
        string ISchemaOptions.Extendee => FileDescriptorSet.Namespace + nameof(FileOptions);
        bool ISchemaOptions.ReadOne(ParserContext ctx, string key)
        {
            switch (key)
            {
                case "optimize_for": OptimizeFor = ctx.Tokens.ConsumeEnum<OptimizeMode>(); return true;
                case "cc_enable_arenas": CcEnableArenas = ctx.Tokens.ConsumeBoolean(); return true;
                case "cc_generic_services": CcGenericServices = ctx.Tokens.ConsumeBoolean(); return true;
#pragma warning disable 0612
                case "java_generate_equals_and_hash": JavaGenerateEqualsAndHash = ctx.Tokens.ConsumeBoolean(); return true;
#pragma warning restore 0612
                case "java_generic_services": JavaGenericServices = ctx.Tokens.ConsumeBoolean(); return true;
                case "java_multiple_files": JavaMultipleFiles = ctx.Tokens.ConsumeBoolean(); return true;
                case "java_string_check_utf8": JavaStringCheckUtf8 = ctx.Tokens.ConsumeBoolean(); return true;
                case "py_generic_services": PyGenericServices = ctx.Tokens.ConsumeBoolean(); return true;

                case "csharp_namespace": CsharpNamespace = ctx.Tokens.ConsumeString(); return true;
                case "go_package": GoPackage = ctx.Tokens.ConsumeString(); return true;
                case "java_outer_classname": JavaOuterClassname = ctx.Tokens.ConsumeString(); return true;
                case "java_package": JavaPackage = ctx.Tokens.ConsumeString(); return true;
                case "objc_class_prefix": ObjcClassPrefix = ctx.Tokens.ConsumeString(); return true;
                case "php_class_prefix": PhpClassPrefix = ctx.Tokens.ConsumeString(); return true;
                case "swift_prefix": SwiftPrefix = ctx.Tokens.ConsumeString(); return true;

                default: return false;
            }
        }
        public byte[] ExtensionData
        {
            get { return DescriptorProto.GetExtensionData(this); }
            set { DescriptorProto.SetExtensionData(this, value); }
        }
    }

#pragma warning restore CS1591
}
namespace ProtoBuf.Reflection
{
    internal static class ErrorExtensions
    {
        public static void Warn(this List<Error> errors, Token token, string message)
            => errors.Add(new Error(token, message, false));
        public static void Error(this List<Error> errors, Token token, string message)
            => errors.Add(new Error(token, message, true));
        public static void Error(this List<Error> errors, ParserException ex)
            => errors.Add(new Error(ex));
    }

    /// <summary>
    /// Describes a generated file
    /// </summary>
    public class CodeFile
    {
        /// <summary>
        /// Get a string representation of this instance
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name;
        /// <summary>
        /// Create a new CodeFile instance
        /// </summary>
        public CodeFile(string name, string text)
        {
            Name = name;
            Text = text;
        }
        /// <summary>
        /// The name (including path if necessary) of this file
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The contents of this file
        /// </summary>
        public string Text { get; }
    }

    /// <summary>
    /// Represents the overall result of a compilation process
    /// </summary>
    public class CompilerResult
    {
        internal CompilerResult(Error[] errors, CodeFile[] files)
        {
            Errors = errors;
            Files = files;
        }
        /// <summary>
        /// The errors from this execution
        /// </summary>
        public Error[] Errors { get; }
        /// <summary>
        /// The output files from this execution
        /// </summary>
        public CodeFile[] Files { get; }
    }

    internal class Import
    {
        public override string ToString() => Path;
        public string Path { get; set; }
        public bool IsPublic { get; set; }
        public Token Token { get; set; }
        public bool Used { get; set; }
    }
    /// <summary>
    /// Describes an error that occurred during processing
    /// </summary>
    public class Error
    {
        /// <summary>
        /// Parse an error from a PROTOC error message
        /// </summary>
        public static Error[] Parse(string stdout, string stderr)
        {
            if (string.IsNullOrWhiteSpace(stdout) && string.IsNullOrWhiteSpace(stderr))
                return noErrors;

            List<Error> errors = new List<Error>();
            using (var reader = new StringReader(stdout))
            {
                Add(reader, errors);
            }
            using (var reader = new StringReader(stderr))
            {
                Add(reader, errors);
            }
            return errors.ToArray();
        }
        static void Add(TextReader lines, List<Error> errors)
        {
            string line;
            while ((line = lines.ReadLine()) != null)
            {
                var s = line;
                bool isError = true;
                int lineNumber = 1, columnNumber = 1;
                if (s[0] == '[')
                {
                    int i = s.IndexOf(']');
                    if (i > 0)
                    {
                        var prefix = line.Substring(1, i).Trim();
                        s = line.Substring(i + 1).Trim();
                        if (prefix.IndexOf("WARNING", StringComparison.OrdinalIgnoreCase) >= 0
                            && prefix.IndexOf("ERROR", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            isError = false;
                        }
                    }
                }
                var match = Regex.Match(s, @"^([^:]+):([0-9]+):([0-9]+):\s+");
                string file = "";
                if (match.Success)
                {
                    file = match.Groups[1].Value;
                    if (!int.TryParse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out lineNumber))
                        lineNumber = 1;
                    if (!int.TryParse(match.Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out columnNumber))
                        columnNumber = 1;
                    s = s.Substring(match.Length).Trim();
                }
                errors.Add(new Error(new Token(" ", lineNumber, columnNumber, TokenType.None, "", 0, file), s, isError));
            }
        }
        internal string ToString(bool includeType) => Text.Length == 0
                ? $"{File}({LineNumber},{ColumnNumber}): {(includeType ? (IsError ? "error: " : "warning: ") : "")}{Message}"
                : $"{File}({LineNumber},{ColumnNumber},{LineNumber},{ColumnNumber + Text.Length}): {(includeType ? (IsError ? "error: " : "warning: ") : "")}{Message}";
        /// <summary>
        /// Get a text representation of this instance
        /// </summary>
        /// <returns></returns>
        public override string ToString() => ToString(true);

        internal static Error[] GetArray(List<Error> errors)
            => errors.Count == 0 ? noErrors : errors.ToArray();

        private static readonly Error[] noErrors = new Error[0];

        internal Error(Token token, string message, bool isError)
        {
            ColumnNumber = token.ColumnNumber;
            LineNumber = token.LineNumber;
            File = token.File;
            LineContents = token.LineContents;
            Message = message;
            IsError = isError;
            Text = token.Value;
        }
        internal Error(ParserException ex)
        {
            ColumnNumber = ex.ColumnNumber;
            LineNumber = ex.LineNumber;
            File = ex.File;
            LineContents = ex.LineContents;
            Message = ex.Message;
            IsError = ex.IsError;
            Text = ex.Text ?? "";
        }
        /// <summary>
        /// True if this instance represents a non-fatal warning
        /// </summary>
        public bool IsWarning => !IsError;
        /// <summary>
        /// True if this instance represents a fatal error
        /// </summary>
        public bool IsError { get; }
        /// <summary>
        /// The file in which this error was identified
        /// </summary>
        public string File { get; }
        /// <summary>
        /// The source text relating to this error
        /// </summary>
        public string Text { get; }
        /// <summary>
        /// The error message
        /// </summary>
        public string Message { get; }
        /// <summary>
        /// The entire line contents in the source in which this error was located
        /// </summary>
        public string LineContents { get; }
        /// <summary>
        /// The line number in which this error was located
        /// </summary>
        public int LineNumber { get; }
        /// <summary>
        /// The column number in which this error was located
        /// </summary>
        public int ColumnNumber { get; }
    }
    enum AbortState
    {
        None, Statement, Object
    }
    interface ISchemaOptions
    {
        List<UninterpretedOption> UninterpretedOptions { get; }
        bool Deprecated { get; set; }
        bool ReadOne(ParserContext ctx, string key);
        byte[] ExtensionData { get; set; }
        string Extendee { get; }
    }

    interface IHazNames
    {
        IEnumerable<string> GetNames();
    }

    interface ISchemaObject
    {
        void ReadOne(ParserContext ctx);
    }
    internal class ParserContext : IDisposable
    {
        public AbortState AbortState { get; set; }
        private void ReadOne<T>(T obj) where T : class, ISchemaObject
        {
            AbortState oldState = AbortState;
            AbortState = AbortState.None;
            Token stateBefore;
            if (!Tokens.Peek(out stateBefore)) return;

            try
            {
                obj.ReadOne(this);
            }
            catch (ParserException ex)
            {
                Errors.Error(ex);
            }
            finally
            {
                var state = AbortState;
                Token stateAfter;
                if (Tokens.Peek(out stateAfter) && stateBefore == stateAfter)
                {
                    // we didn't move! avoid looping forever failing to do the same thing
                    Errors.Error(stateAfter, "unknown error");
                    state = stateAfter.Is(TokenType.Symbol, "}")
                        ? AbortState.Object : AbortState.Statement;
                }
                AbortState = oldState;
                switch (state)
                {
                    case AbortState.Object:
                        Tokens.SkipToEndObject();
                        break;
                    case AbortState.Statement:
                        Tokens.SkipToEndStatement();
                        break;
                }
            }
        }
        public void Fill<T>(T obj) where T : class, ISchemaObject
        {
            var tokens = Tokens;
            Token token;
            while (tokens.Peek(out token))
            {
                if (tokens.ConsumeIf(TokenType.Symbol, ";"))
                { }
                else
                {
                    ReadOne(obj);
                }
            }
        }
        static readonly char[] Period = { '.' };
        private void ReadOption<T>(ref T obj, ISchemaObject parent, List<UninterpretedOption.NamePart> existingNameParts = null) where T : class, ISchemaOptions, new()
        {
            var tokens = Tokens;
            bool isBlock = existingNameParts != null;
            var nameParts = isBlock
                ? new List<UninterpretedOption.NamePart>(existingNameParts) // create a clone we can append to
                : new List<UninterpretedOption.NamePart>();

            do
            {
                if (nameParts.Count != 0) tokens.ConsumeIf(TokenType.AlphaNumeric, ".");

                bool isExtension = tokens.ConsumeIf(TokenType.Symbol, isBlock ? "[" : "(");
                string key = tokens.Consume(TokenType.AlphaNumeric);
                var keyToken = tokens.Previous;
                if (isExtension) tokens.Consume(TokenType.Symbol, isBlock ? "]" : ")");

                if (!isExtension && key.StartsWith("."))
                {
                    key = key.TrimStart(Period);
                }

                key = key.Trim();
                if (isExtension || nameParts.Count == 0 || key.IndexOf('.') < 0)
                {
                    var name = new UninterpretedOption.NamePart { IsExtension = isExtension, name_part = key, Token = keyToken };
                    nameParts.Add(name);
                }
                else
                {
                    foreach (var part in key.Split(Period, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var name = new UninterpretedOption.NamePart { IsExtension = false, name_part = part, Token = keyToken };
                        nameParts.Add(name);
                    }
                }
            } while (!(
            (isBlock && tokens.Is(TokenType.Symbol, "{"))
            || tokens.ConsumeIf(TokenType.Symbol, isBlock ? ":" : "=")));

            if (tokens.ConsumeIf(TokenType.Symbol, "{"))
            {
                if (obj == null) obj = new T();
                bool any = false;
                while (!tokens.ConsumeIf(TokenType.Symbol, "}"))
                {
                    ReadOption(ref obj, parent, nameParts);
                    any = true;
                }
                if (!any)
                {
                    var newOption = new UninterpretedOption();
                    newOption.Names.AddRange(nameParts);
                    obj.UninterpretedOptions.Add(newOption);
                }
            }
            else
            {

                var field = parent as FieldDescriptorProto;
                bool isField = typeof(T) == typeof(FieldOptions) && field != null;
                var singleKey = (nameParts.Count == 1 && !nameParts[0].IsExtension) ? nameParts[0].name_part : null;
                if (singleKey == "default" && isField)
                {
                    string defaultValue = tokens.ConsumeString(field.type == FieldDescriptorProto.Type.TypeBytes);
                    nameParts[0].Token.RequireProto2(this);
                    ParseDefault(tokens.Previous, field.type, ref defaultValue);
                    if (defaultValue != null)
                    {
                        field.DefaultValue = defaultValue;
                    }
                }
                else if (singleKey == "json_name" && isField)
                {
                    string jsonName = tokens.ConsumeString();
                    field.JsonName = jsonName;
                }
                else
                {
                    if (obj == null) obj = new T();
                    if (singleKey == "deprecated")
                    {
                        obj.Deprecated = tokens.ConsumeBoolean();
                    }
                    else if (singleKey == null || !obj.ReadOne(this, singleKey))
                    {
                        var newOption = new UninterpretedOption
                        {
                            AggregateValue = tokens.ConsumeString(),
                            Token = tokens.Previous
                        };
                        newOption.Names.AddRange(nameParts);
                        obj.UninterpretedOptions.Add(newOption);
                    }
                }
            }
        }

        private void ParseDefault(Token token, FieldDescriptorProto.Type type, ref string defaultValue)
        {
            switch (type)
            {
                case FieldDescriptorProto.Type.TypeBool:
                    switch (defaultValue)
                    {
                        case "true":
                        case "false":
                            break;
                        default:
                            Errors.Error(token, "expected 'true' or 'false'");
                            break;
                    }
                    break;
                case FieldDescriptorProto.Type.TypeDouble:
                    switch (defaultValue)
                    {
                        case "inf":
                        case "-inf":
                        case "nan":
                            break;
                        default:
                            double val;
                            if (TokenExtensions.TryParseDouble(defaultValue, out val))
                            {
                                defaultValue = Format(val);
                            }
                            else
                            {
                                Errors.Error(token, "invalid floating-point number");
                            }
                            break;
                    }
                    break;
                case FieldDescriptorProto.Type.TypeFloat:
                    switch (defaultValue)
                    {
                        case "inf":
                        case "-inf":
                        case "nan":
                            break;
                        default:
                            float val;
                            if (TokenExtensions.TryParseSingle(defaultValue, out val))
                            {
                                defaultValue = Format(val);
                            }
                            else
                            {
                                Errors.Error(token, "invalid floating-point number");
                            }
                            break;
                    }
                    break;
                case FieldDescriptorProto.Type.TypeSfixed32:
                case FieldDescriptorProto.Type.TypeInt32:
                case FieldDescriptorProto.Type.TypeSint32:
                    {
                        int val;
                        if (TokenExtensions.TryParseInt32(defaultValue, out val))
                        {
                            defaultValue = val.ToString(CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            Errors.Error(token, "invalid integer");
                        }
                    }
                    break;
                case FieldDescriptorProto.Type.TypeFixed32:
                case FieldDescriptorProto.Type.TypeUint32:
                    {
                        uint val;
                        if (TokenExtensions.TryParseUInt32(defaultValue, out val))
                        {
                            defaultValue = val.ToString(CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            Errors.Error(token, "invalid unsigned integer");
                        }
                    }
                    break;
                case FieldDescriptorProto.Type.TypeSfixed64:
                case FieldDescriptorProto.Type.TypeInt64:
                case FieldDescriptorProto.Type.TypeSint64:
                    {
                        long val;
                        if (TokenExtensions.TryParseInt64(defaultValue, out val))
                        {
                            defaultValue = val.ToString(CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            Errors.Error(token, "invalid integer");
                        }
                    }
                    break;
                case FieldDescriptorProto.Type.TypeFixed64:
                case FieldDescriptorProto.Type.TypeUint64:
                    {
                        ulong val;
                        if (TokenExtensions.TryParseUInt64(defaultValue, out val))
                        {
                            defaultValue = val.ToString(CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            Errors.Error(token, "invalid unsigned integer");
                        }
                    }
                    break;
                case 0:
                case FieldDescriptorProto.Type.TypeBytes:
                case FieldDescriptorProto.Type.TypeString:
                case FieldDescriptorProto.Type.TypeEnum:
                    break;
                default:
                    Errors.Error(token, $"default value not handled: {type}={defaultValue}");
                    break;
            }
        }

        static readonly char[] ExponentChars = { 'e', 'E' };
        static readonly string[] ExponentFormats = { "e0", "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8", "e9", "e10" };
        static string Format(float val)
        {
            string s = val.ToString(CultureInfo.InvariantCulture);
            if (s.IndexOfAny(ExponentChars) < 0) return s;

            foreach (var format in ExponentFormats)
            {
                var tmp = val.ToString(format, CultureInfo.InvariantCulture);
                float x;
                if (float.TryParse(tmp, NumberStyles.Any, CultureInfo.InvariantCulture, out x) && x == val) return tmp;
            }
            return val.ToString("e", CultureInfo.InvariantCulture);

        }

        static string Format(double val)
        {
            string s = val.ToString(CultureInfo.InvariantCulture).ToUpperInvariant();
            if (s.IndexOfAny(ExponentChars) < 0) return s;

            foreach (var format in ExponentFormats)
            {
                var tmp = val.ToString(format, CultureInfo.InvariantCulture);
                double x;
                if (double.TryParse(tmp, NumberStyles.Any, CultureInfo.InvariantCulture, out x) && x == val) return tmp;
            }
            return val.ToString("e", CultureInfo.InvariantCulture);
        }

        public T ParseOptionBlock<T>(T obj, ISchemaObject parent = null) where T : class, ISchemaOptions, new()
        {
            var tokens = Tokens;
            try
            {
                while (true)
                {
                    if (tokens.ConsumeIf(TokenType.Symbol, "]"))
                    {
                        break;
                    }
                    else if (tokens.ConsumeIf(TokenType.Symbol, ","))
                    {
                    }
                    else
                    {
                        ReadOption(ref obj, parent);
                    }
                }
            }
            catch (ParserException ex)
            {
                Errors.Error(ex);
                tokens.SkipToEndOptions();
            }
            return obj;
        }
        public T ParseOptionStatement<T>(T obj, ISchemaObject parent) where T : class, ISchemaOptions, new()
        {
            var tokens = Tokens;
            try
            {
                ReadOption(ref obj, parent);
                tokens.Consume(TokenType.Symbol, ";");
            }
            catch (ParserException ex)
            {
                Errors.Error(ex);
                tokens.SkipToEndStatement();
            }
            return obj;
        }
        public bool TryReadObject<T>(out T obj) where T : class, ISchemaObject, new()
        {
            obj = new T();
            return TryReadObjectImpl(obj);
        }
        internal bool TryReadObjectImpl<T>(T obj) where T : class, ISchemaObject
        {
            var tokens = Tokens;

            try
            {
                tokens.Consume(TokenType.Symbol, "{");
                Token token;
                while (tokens.Peek(out token) && !token.Is(TokenType.Symbol, "}"))
                {
                    if (tokens.ConsumeIf(TokenType.Symbol, ";"))
                    { }
                    else
                    {
                        ReadOne(obj);
                    }
                }
                tokens.Consume(TokenType.Symbol, "}");
                return true;
            }
            catch (ParserException ex)
            {
                Errors.Error(ex);
                tokens.SkipToEndObject();
            }
            obj = null;
            return false;
        }
        public ParserContext(FileDescriptorProto file, Peekable<Token> tokens, List<Error> errors)
        {
            Tokens = tokens;
            Errors = errors;
            _file = file;
        }

        public string Syntax
        {
            get
            {
                var syntax = _file.Syntax;
                return string.IsNullOrEmpty(syntax) ? FileDescriptorProto.SyntaxProto2 : syntax;
            }
        }

        private readonly FileDescriptorProto _file;
        public Peekable<Token> Tokens { get; }
        public List<Error> Errors { get; }

        public void Dispose() { Tokens?.Dispose(); }

        internal void CheckNames(IHazNames parent, string name, Token token
#if DEBUG && NETSTANDARD1_3
            , [System.Runtime.CompilerServices.CallerMemberName] string caller = null
#endif
            )
        {
            if (parent != null && parent.GetNames().Contains(name))
            {
                Errors.Error(token, $"name '{name}' is already in use"
#if DEBUG && NETSTANDARD1_3
             + $" ({caller})"
#endif
                    );
            }
        }
    }
}
