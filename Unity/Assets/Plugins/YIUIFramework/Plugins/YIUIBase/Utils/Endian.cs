namespace YIUIFramework
{
    /// <summary>
    /// The utility class help for CPU endian transform.
    /// </summary>
    public static class Endian
    {
        /// <summary>
        /// Reverse the bytes order (16-bit).
        /// </summary>
        /// <param name="value">The value wait for reverse.</param>
        /// <returns>The reversed byte.</returns>
        public static ushort ReverseBytes(ushort value)
        {
            return (ushort)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }

        /// <summary>
        /// Reverse the bytes order (32-bit).
        /// </summary>
        /// <param name="value">The value wait for reverse.</param>
        /// <returns>The reversed byte.</returns>
        public static uint ReverseBytes(uint value)
        {
            return
                (value & 0x000000FFU) << 24 |
                (value & 0x0000FF00U) << 8 |
                (value & 0x00FF0000U) >> 8 |
                (value & 0xFF000000U) >> 24;
        }

        /// <summary>
        /// Reverse the bytes order (64-bit).
        /// </summary>
        /// <param name="value">The value wait for reverse.</param>
        /// <returns>The reversed byte.</returns>
        public static ulong ReverseBytes(ulong value)
        {
            return
                (value & 0x00000000000000FFUL) << 56 |
                (value & 0x000000000000FF00UL) << 40 |
                (value & 0x0000000000FF0000UL) << 24 |
                (value & 0x00000000FF000000UL) << 8 |
                (value & 0x000000FF00000000UL) >> 8 |
                (value & 0x0000FF0000000000UL) >> 24 |
                (value & 0x00FF000000000000UL) >> 40 |
                (value & 0xFF00000000000000UL) >> 56;
        }
    }
}