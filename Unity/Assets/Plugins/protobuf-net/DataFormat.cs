
namespace ProtoBuf
{
    /// <summary>
    /// Sub-format to use when serializing/deserializing data
    /// </summary>
    public enum DataFormat
    {
        /// <summary>
        /// Uses the default encoding for the data-type.
        /// </summary>
        Default,

        /// <summary>
        /// When applied to signed integer-based data (including Decimal), this
        /// indicates that zigzag variant encoding will be used. This means that values
        /// with small magnitude (regardless of sign) take a small amount
        /// of space to encode.
        /// </summary>
        ZigZag,

        /// <summary>
        /// When applied to signed integer-based data (including Decimal), this
        /// indicates that two's-complement variant encoding will be used.
        /// This means that any -ve number will take 10 bytes (even for 32-bit),
        /// so should only be used for compatibility.
        /// </summary>
        TwosComplement,

        /// <summary>
        /// When applied to signed integer-based data (including Decimal), this
        /// indicates that a fixed amount of space will be used.
        /// </summary>
        FixedSize,

        /// <summary>
        /// When applied to a sub-message, indicates that the value should be treated
        /// as group-delimited.
        /// </summary>
        Group
    }
}
