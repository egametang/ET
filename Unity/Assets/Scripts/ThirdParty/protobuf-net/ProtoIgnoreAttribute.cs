using System;

namespace ProtoBuf
{
    /// <summary>
    /// Indicates that a member should be excluded from serialization; this
    /// is only normally used when using implict fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,
        AllowMultiple = false, Inherited = true)]
    public class ProtoIgnoreAttribute : Attribute { }

    /// <summary>
    /// Indicates that a member should be excluded from serialization; this
    /// is only normally used when using implict fields. This allows
    /// ProtoIgnoreAttribute usage
    /// even for partial classes where the individual members are not
    /// under direct control.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,
            AllowMultiple = true, Inherited = false)]
    public sealed class ProtoPartialIgnoreAttribute : ProtoIgnoreAttribute
    {
        /// <summary>
        /// Creates a new ProtoPartialIgnoreAttribute instance.
        /// </summary>
        /// <param name="memberName">Specifies the member to be ignored.</param>
        public ProtoPartialIgnoreAttribute(string memberName)
            : base()
        {
            if (string.IsNullOrEmpty(memberName)) throw new ArgumentNullException(nameof(memberName));

            MemberName = memberName;
        }
        /// <summary>
        /// The name of the member to be ignored.
        /// </summary>
        public string MemberName { get; }
    }
}
