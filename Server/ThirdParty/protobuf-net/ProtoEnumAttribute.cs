using System;

namespace ProtoBuf
{
    /// <summary>
    /// Used to define protocol-buffer specific behavior for
    /// enumerated values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ProtoEnumAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the specific value to use for this enum during serialization.
        /// </summary>
        public int Value
        {
            get { return enumValue; }
            set { this.enumValue = value; hasValue = true; }
        }

        /// <summary>
        /// Indicates whether this instance has a customised value mapping
        /// </summary>
        /// <returns>true if a specific value is set</returns>
        public bool HasValue() => hasValue;

        private bool hasValue;
        private int enumValue;

        /// <summary>
        /// Gets or sets the defined name of the enum, as used in .proto
        /// (this name is not used during serialization).
        /// </summary>
        public string Name { get; set; }
    }
}
