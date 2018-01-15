using System;

#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
using IKVM.Reflection;
#else
using System.Reflection;
#endif

namespace ProtoBuf
{
    /// <summary>
    /// Declares a member to be used in protocol-buffer serialization, using
    /// the given Tag. A DataFormat may be used to optimise the serialization
    /// format (for instance, using zigzag encoding for negative numbers, or 
    /// fixed-length encoding for large values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,
        AllowMultiple = false, Inherited = true)]
    public class ProtoMemberAttribute : Attribute
        , IComparable
#if !NO_GENERICS
        , IComparable<ProtoMemberAttribute>
#endif

    {
        /// <summary>
        /// Compare with another ProtoMemberAttribute for sorting purposes
        /// </summary>
        public int CompareTo(object other) { return CompareTo(other as ProtoMemberAttribute); }
        /// <summary>
        /// Compare with another ProtoMemberAttribute for sorting purposes
        /// </summary>
        public int CompareTo(ProtoMemberAttribute other)
        {
            if (other == null) return -1;
            if ((object)this == (object)other) return 0;
            int result = this.tag.CompareTo(other.tag);
            if (result == 0) result = string.CompareOrdinal(this.name, other.name);
            return result;
        }

        /// <summary>
        /// Creates a new ProtoMemberAttribute instance.
        /// </summary>
        /// <param name="tag">Specifies the unique tag used to identify this member within the type.</param>
        public ProtoMemberAttribute(int tag) : this(tag, false)
        { }

        internal ProtoMemberAttribute(int tag, bool forced)
        {
            if (tag <= 0 && !forced) throw new ArgumentOutOfRangeException("tag");
            this.tag = tag;
        }

#if !NO_RUNTIME
        internal MemberInfo Member;
        internal bool TagIsPinned;
#endif
        /// <summary>
        /// Gets or sets the original name defined in the .proto; not used
        /// during serialization.
        /// </summary>
        public string Name { get { return name; } set { name = value; } }
        private string name;

        /// <summary>
        /// Gets or sets the data-format to be used when encoding this value.
        /// </summary>
        public DataFormat DataFormat { get { return dataFormat; } set { dataFormat = value; } }
        private DataFormat dataFormat; 

        /// <summary>
        /// Gets the unique tag used to identify this member within the type.
        /// </summary>
        public int Tag { get { return tag; } }
        private int tag;
        internal void Rebase(int tag) { this.tag = tag; }

        /// <summary>
        /// Gets or sets a value indicating whether this member is mandatory.
        /// </summary>
        public bool IsRequired {
            get { return (options & MemberSerializationOptions.Required) == MemberSerializationOptions.Required; }
            set {
                if (value) options |= MemberSerializationOptions.Required;
                else options &= ~MemberSerializationOptions.Required;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this member is packed.
        /// This option only applies to list/array data of primitive types (int, double, etc).
        /// </summary>
        public bool IsPacked
        {
            get { return (options & MemberSerializationOptions.Packed) == MemberSerializationOptions.Packed;}
            set {
                if (value) options |= MemberSerializationOptions.Packed;
                else options &= ~MemberSerializationOptions.Packed;
            }
        }

        /// <summary>
        /// Indicates whether this field should *repace* existing values (the default is false, meaning *append*).
        /// This option only applies to list/array data.
        /// </summary>
        public bool OverwriteList
        {
            get { return (options & MemberSerializationOptions.OverwriteList) == MemberSerializationOptions.OverwriteList; }
            set
            {
                if (value) options |= MemberSerializationOptions.OverwriteList;
                else options &= ~MemberSerializationOptions.OverwriteList;
            }
        }

        /// <summary>
        /// Enables full object-tracking/full-graph support.
        /// </summary>
        public bool AsReference
        {
            get { return (options & MemberSerializationOptions.AsReference) == MemberSerializationOptions.AsReference; }
            set
            {
                if (value) options |= MemberSerializationOptions.AsReference;
                else options &= ~MemberSerializationOptions.AsReference;

                options |= MemberSerializationOptions.AsReferenceHasValue;
            }
        }

        internal bool AsReferenceHasValue
        {
            get { return (options & MemberSerializationOptions.AsReferenceHasValue) == MemberSerializationOptions.AsReferenceHasValue; }
            set {
                if (value) options |= MemberSerializationOptions.AsReferenceHasValue;
                else options &= ~MemberSerializationOptions.AsReferenceHasValue;
            }
        }

        /// <summary>
        /// Embeds the type information into the stream, allowing usage with types not known in advance.
        /// </summary>
        public bool DynamicType
        {
            get { return (options & MemberSerializationOptions.DynamicType) == MemberSerializationOptions.DynamicType; }
            set
            {
                if (value) options |= MemberSerializationOptions.DynamicType;
                else options &= ~MemberSerializationOptions.DynamicType;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this member is packed (lists/arrays).
        /// </summary>
        public MemberSerializationOptions Options { get { return options; } set { options = value; } }
        private MemberSerializationOptions options;

        
    }

    /// <summary>
    /// Additional (optional) settings that control serialization of members
    /// </summary>
    [Flags]
    public enum MemberSerializationOptions
    {
        /// <summary>
        /// Default; no additional options
        /// </summary>
        None = 0,
        /// <summary>
        /// Indicates that repeated elements should use packed (length-prefixed) encoding
        /// </summary>
        Packed = 1,
        /// <summary>
        /// Indicates that the given item is required
        /// </summary>
        Required = 2,
        /// <summary>
        /// Enables full object-tracking/full-graph support
        /// </summary>
        AsReference = 4,
        /// <summary>
        /// Embeds the type information into the stream, allowing usage with types not known in advance
        /// </summary>
        DynamicType = 8,
        /// <summary>
        /// Indicates whether this field should *repace* existing values (the default is false, meaning *append*).
        /// This option only applies to list/array data.
        /// </summary>
        OverwriteList = 16,
        /// <summary>
        /// Determines whether the types AsReferenceDefault value is used, or whether this member's AsReference should be used
        /// </summary>
        AsReferenceHasValue = 32
    }

    /// <summary>
    /// Declares a member to be used in protocol-buffer serialization, using
    /// the given Tag and MemberName. This allows ProtoMemberAttribute usage
    /// even for partial classes where the individual members are not
    /// under direct control.
    /// A DataFormat may be used to optimise the serialization
    /// format (for instance, using zigzag encoding for negative numbers, or 
    /// fixed-length encoding for large values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,
            AllowMultiple = true, Inherited = false)]
    public sealed class ProtoPartialMemberAttribute : ProtoMemberAttribute
    {
        /// <summary>
        /// Creates a new ProtoMemberAttribute instance.
        /// </summary>
        /// <param name="tag">Specifies the unique tag used to identify this member within the type.</param>
        /// <param name="memberName">Specifies the member to be serialized.</param>
        public ProtoPartialMemberAttribute(int tag, string memberName)
            : base(tag)
        {
            if (Helpers.IsNullOrEmpty(memberName)) throw new ArgumentNullException("memberName");
            this.memberName = memberName;
        }
        /// <summary>
        /// The name of the member to be serialized.
        /// </summary>
        public string MemberName { get { return memberName; } }
        private readonly string memberName;
    }
}
