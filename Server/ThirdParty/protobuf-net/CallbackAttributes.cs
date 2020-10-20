using System;
using System.ComponentModel;

namespace ProtoBuf
{
    /// <summary>Specifies a method on the root-contract in an hierarchy to be invoked before serialization.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
#if !CF && !PORTABLE && !COREFX && !PROFILE259
    [ImmutableObject(true)]
#endif
    public sealed class ProtoBeforeSerializationAttribute : Attribute { }

    /// <summary>Specifies a method on the root-contract in an hierarchy to be invoked after serialization.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
#if !CF && !PORTABLE && !COREFX && !PROFILE259
    [ImmutableObject(true)]
#endif
    public sealed class ProtoAfterSerializationAttribute : Attribute { }

    /// <summary>Specifies a method on the root-contract in an hierarchy to be invoked before deserialization.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
#if !CF && !PORTABLE && !COREFX && !PROFILE259
    [ImmutableObject(true)]
#endif
    public sealed class ProtoBeforeDeserializationAttribute : Attribute { }

    /// <summary>Specifies a method on the root-contract in an hierarchy to be invoked after deserialization.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
#if !CF && !PORTABLE && !COREFX && !PROFILE259
    [ImmutableObject(true)]
#endif
    public sealed class ProtoAfterDeserializationAttribute : Attribute { }
}
