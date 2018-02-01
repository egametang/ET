
namespace ProtoBuf
{
    /// <summary>
    /// Specifies the type of prefix that should be applied to messages.
    /// </summary>
    public enum PrefixStyle
    {
        /// <summary>
        /// No length prefix is applied to the data; the data is terminated only be the end of the stream.
        /// </summary>
        None,
        /// <summary>
        /// A base-128 length prefix is applied to the data (efficient for short messages).
        /// </summary>
        Base128,
        /// <summary>
        /// A fixed-length (little-endian) length prefix is applied to the data (useful for compatibility).
        /// </summary>
        Fixed32,
                /// <summary>
        /// A fixed-length (big-endian) length prefix is applied to the data (useful for compatibility).
        /// </summary>
        Fixed32BigEndian
    }
}
