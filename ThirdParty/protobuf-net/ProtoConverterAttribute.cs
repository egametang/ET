using System;

namespace ProtoBuf
{
    /// <summary>
    /// Indicates that a static member should be considered the same as though
    /// were an implicit / explicit conversion operator; in particular, this
    /// is useful for conversions that operator syntax does not allow, such as
    /// to/from interface types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ProtoConverterAttribute : Attribute { }
}