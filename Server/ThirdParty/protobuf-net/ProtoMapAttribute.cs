using System;

namespace ProtoBuf
{
    /// <summary>
    /// Controls the formatting of elements in a dictionary, and indicates that
    /// "map" rules should be used: duplicates *replace* earlier values, rather
    /// than throwing an exception
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ProtoMapAttribute : Attribute
    {
        /// <summary>
        /// Describes the data-format used to store the key
        /// </summary>
        public DataFormat KeyFormat { get; set; }
        /// <summary>
        /// Describes the data-format used to store the value
        /// </summary>
        public DataFormat ValueFormat { get; set; }

        /// <summary>
        /// Disables "map" handling; dictionaries will use ".Add(key,value)" instead of  "[key] = value",
        /// which means duplicate keys will cause an exception (instead of retaining the final value); if
        /// a proto schema is emitted, it will be produced using "repeated" instead of "map"
        /// </summary>
        public bool DisableMap { get; set; }
    }
}
