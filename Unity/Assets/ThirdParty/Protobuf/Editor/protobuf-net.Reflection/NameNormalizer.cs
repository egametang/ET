using Google.Protobuf.Reflection;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace ProtoBuf.Reflection
{

    internal class ParserException : Exception
    {
        public int ColumnNumber { get; }
        public int LineNumber { get; }
        public string File { get; }
        public string Text { get; }
        public string LineContents { get; }
        public bool IsError { get; }
        internal ParserException(Token token, string message, bool isError)
            : base(message ?? "error")
        {
            ColumnNumber = token.ColumnNumber;
            LineNumber = token.LineNumber;
            File = token.File;
            LineContents = token.LineContents;
            Text = token.Value ?? "";
            IsError = isError;
        }
    }
    /// <summary>
    /// Provides general purpose name suggestions
    /// </summary>
    public abstract class NameNormalizer
    {
        private class NullNormalizer : NameNormalizer
        {
            protected override string GetName(string identifier) => identifier;
            /// <summary>
            /// Suggest a name with idiomatic pluralization
            /// </summary>
            public override string Pluralize(string identifier) => identifier;
        }
        private class DefaultNormalizer : NameNormalizer
        {
            protected override string GetName(string identifier) => AutoCapitalize(identifier);
            /// <summary>
            /// Suggest a name with idiomatic pluralization
            /// </summary>
            public override string Pluralize(string identifier) => AutoPluralize(identifier);
        }
        /// <summary>
        /// Suggest a name with idiomatic name capitalization
        /// </summary>
        protected static string AutoCapitalize(string identifier)
        {
            //if (string.IsNullOrEmpty(identifier)) return identifier;
            //// if all upper-case, make proper-case
            //if (Regex.IsMatch(identifier, @"^[_A-Z0-9]*$"))
            //{
            //    return Regex.Replace(identifier, @"(^|_)([A-Z0-9])([A-Z0-9]*)",
            //        match => match.Groups[2].Value.ToUpperInvariant() + match.Groups[3].Value.ToLowerInvariant());
            //}
            //// if all lower-case, make proper case
            //if (Regex.IsMatch(identifier, @"^[_a-z0-9]*$"))
            //{
            //    return Regex.Replace(identifier, @"(^|_)([a-z0-9])([a-z0-9]*)",
            //        match => match.Groups[2].Value.ToUpperInvariant() + match.Groups[3].Value.ToLowerInvariant());
            //}
            //// just remove underscores - leave their chosen casing alone
            //return identifier.Replace("_", "");
            return identifier;
        }
        /// <summary>
        /// Suggest a name with idiomatic pluralization
        /// </summary>
        protected static string AutoPluralize(string identifier)
        {
            // horribly Anglo-centric and only covers common cases; but: is swappable

            //if (string.IsNullOrEmpty(identifier) || identifier.Length == 1) return identifier;

            //if (identifier.EndsWith("ss") || identifier.EndsWith("o")) return identifier + "es";
            //if (identifier.EndsWith("is") && identifier.Length > 2) return identifier.Substring(0, identifier.Length - 2) + "es";

            //if (identifier.EndsWith("s")) return identifier; // misses some things (bus => buses), but: might already be pluralized

            //if (identifier.EndsWith("y") && identifier.Length > 2)
            //{   // identity => identities etc
            //    switch (identifier[identifier.Length - 2])
            //    {
            //        case 'a':
            //        case 'e':
            //        case 'i':
            //        case 'o':
            //        case 'u':
            //            break; // only for consonant prefix
            //        default:
            //            return identifier.Substring(0, identifier.Length - 1) + "ies";
            //    }
            //}
            //return identifier + "s";
            return identifier;
        }
        /// <summary>
        /// Name normalizer with default protobuf-net behaviour, using .NET idioms
        /// </summary>
        public static NameNormalizer Default { get; } = new DefaultNormalizer();
        /// <summary>
        /// Name normalizer that passes through all identifiers without any changes
        /// </summary>
        public static NameNormalizer Null { get; } = new NullNormalizer();
        /// <summary>
        /// Suggest a normalized identifier
        /// </summary>
        protected abstract string GetName(string identifier);
        /// <summary>
        /// Suggest a name with idiomatic pluralization
        /// </summary>
        public abstract string Pluralize(string identifier);

        /// <summary>
        /// Suggest a normalized identifier
        /// </summary>
        public virtual string GetName(FileDescriptorProto definition)
        {
            var ns = definition?.Options?.GetOptions()?.Namespace;
            if (!string.IsNullOrWhiteSpace(ns)) return ns;
            ns = definition.Options?.CsharpNamespace;
            if (string.IsNullOrWhiteSpace(ns)) ns = GetName(definition.Package);

            return string.IsNullOrWhiteSpace(ns) ? null : ns;
        }
        /// <summary>
        /// Suggest a normalized identifier
        /// </summary>
        public virtual string GetName(DescriptorProto definition)
        {
            var name = definition?.Options?.GetOptions()?.Name;
            if (!string.IsNullOrWhiteSpace(name)) return name;
            return GetName(definition.Parent as DescriptorProto, GetName(definition.Name), definition.Name, false);
        }
        /// <summary>
        /// Suggest a normalized identifier
        /// </summary>
        public virtual string GetName(EnumDescriptorProto definition)
        {
            var name = definition?.Options?.GetOptions()?.Name;
            if (!string.IsNullOrWhiteSpace(name)) return name;
            return GetName(definition.Parent as DescriptorProto, GetName(definition.Name), definition.Name, false);
        }
        /// <summary>
        /// Suggest a normalized identifier
        /// </summary>
        public virtual string GetName(EnumValueDescriptorProto definition)
        {
            var name = definition?.Options?.GetOptions()?.Name;
            if (!string.IsNullOrWhiteSpace(name)) return name;
            return AutoCapitalize(definition.Name);
        }
        /// <summary>
        /// Suggest a normalized identifier
        /// </summary>
        public virtual string GetName(FieldDescriptorProto definition)
        {
            var name = definition?.Options?.GetOptions()?.Name;
            if (!string.IsNullOrWhiteSpace(name)) return name;
            var preferred = GetName(definition.Name);
            if (definition.label == FieldDescriptorProto.Label.LabelRepeated)
            {
                preferred = Pluralize(preferred);
            }
            return GetName(definition.Parent as DescriptorProto, preferred, definition.Name, true);
        }
        /// <summary>
        /// Obtain a set of all names defined for a message
        /// </summary>
        protected HashSet<string> BuildConflicts(DescriptorProto parent, bool includeDescendents)
        {
            var conflicts = new HashSet<string>();
            if (parent != null)
            {
                conflicts.Add(GetName(parent));
                if (includeDescendents)
                {
                    foreach (var type in parent.NestedTypes)
                    {
                        conflicts.Add(GetName(type));
                    }
                    foreach (var type in parent.EnumTypes)
                    {
                        conflicts.Add(GetName(type));
                    }
                }
            }
            return conflicts;
        }
        /// <summary>
        /// Get the preferred name for an element
        /// </summary>
        protected virtual string GetName(DescriptorProto parent, string preferred, string fallback, bool includeDescendents)
        {
            var conflicts = BuildConflicts(parent, includeDescendents);

            if (!conflicts.Contains(preferred)) return preferred;
            if (!conflicts.Contains(fallback)) return fallback;

            var attempt = preferred + "Value";
            if (!conflicts.Contains(attempt)) return attempt;

            attempt = fallback + "Value";
            if (!conflicts.Contains(attempt)) return attempt;

            int i = 1;
            while (true)
            {
                attempt = preferred + i.ToString();
                if (!conflicts.Contains(attempt)) return attempt;
            }
        }
    }

}
