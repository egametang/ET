using System;
using System.ComponentModel;

using ProtoBuf.Meta;

namespace ProtoBuf
{
    /// <summary>
    /// Indicates the known-types to support for an individual
    /// message. This serializes each level in the hierarchy as
    /// a nested message to retain wire-compatibility with
    /// other protocol-buffer implementations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
    public sealed class ProtoIncludeAttribute : Attribute
    {
        ///<summary>
        /// Creates a new instance of the ProtoIncludeAttribute.
        /// </summary>
        /// <param name="tag">The unique index (within the type) that will identify this data.</param>
        /// <param name="knownType">The additional type to serialize/deserialize.</param>
        public ProtoIncludeAttribute(int tag, Type knownType)
            : this(tag, knownType == null ? "" : knownType.AssemblyQualifiedName) { }

        /// <summary>
        /// Creates a new instance of the ProtoIncludeAttribute.
        /// </summary>
        /// <param name="tag">The unique index (within the type) that will identify this data.</param>
        /// <param name="knownTypeName">The additional type to serialize/deserialize.</param>
        public ProtoIncludeAttribute(int tag, string knownTypeName)
        {
            if (tag <= 0) throw new ArgumentOutOfRangeException(nameof(tag), "Tags must be positive integers");
            if (string.IsNullOrEmpty(knownTypeName)) throw new ArgumentNullException(nameof(knownTypeName), "Known type cannot be blank");
            Tag = tag;
            KnownTypeName = knownTypeName;
        }

        /// <summary>
        /// Gets the unique index (within the type) that will identify this data.
        /// </summary>
        public int Tag { get; }

        /// <summary>
        /// Gets the additional type to serialize/deserialize.
        /// </summary>
        public string KnownTypeName { get; }

        /// <summary>
        /// Gets the additional type to serialize/deserialize.
        /// </summary>
        public Type KnownType => TypeModel.ResolveKnownType(KnownTypeName, null, null);

        /// <summary>
        /// Specifies whether the inherited sype's sub-message should be
        /// written with a length-prefix (default), or with group markers.
        /// </summary>
        [DefaultValue(DataFormat.Default)]
        public DataFormat DataFormat { get; set; } = DataFormat.Default;
    }
}
