using System;

#if PLAT_BINARYFORMATTER && !(COREFX || PROFILE259)
using System.Runtime.Serialization;
#endif
namespace ProtoBuf
{
    /// <summary>
    /// Indicates an error during serialization/deserialization of a proto stream.
    /// </summary>
#if PLAT_BINARYFORMATTER && !(COREFX || PROFILE259)
    [Serializable]
#endif
    public class ProtoException : Exception
    {
        /// <summary>Creates a new ProtoException instance.</summary>
        public ProtoException() { }

        /// <summary>Creates a new ProtoException instance.</summary>
        public ProtoException(string message) : base(message) { }

        /// <summary>Creates a new ProtoException instance.</summary>
        public ProtoException(string message, Exception innerException) : base(message, innerException) { }

#if PLAT_BINARYFORMATTER && !(COREFX || PROFILE259)
        /// <summary>Creates a new ProtoException instance.</summary>
        protected ProtoException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }
}
