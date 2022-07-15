using System;

namespace ProtoBuf.Meta
{
    /// <summary>
    /// Event data associated with new types being added to a model
    /// </summary>
    public sealed class TypeAddedEventArgs : EventArgs
    {
        internal TypeAddedEventArgs(MetaType metaType)
        {
            MetaType = metaType;
            ApplyDefaultBehaviour = true;
        }

        /// <summary>
        /// Whether or not to apply the default mapping behavior
        /// </summary>
        public bool ApplyDefaultBehaviour { get; set; }
        /// <summary>
        /// The configuration of the type being added
        /// </summary>
        public MetaType MetaType { get; }
        /// <summary>
        /// The type that was added to the model
        /// </summary>
        public Type Type => MetaType.Type;
        /// <summary>
        /// The model that is being changed
        /// </summary>
        public RuntimeTypeModel Model => MetaType.Model as RuntimeTypeModel;
    }
}