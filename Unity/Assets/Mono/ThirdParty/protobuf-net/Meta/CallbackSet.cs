#if !NO_RUNTIME
using System;

#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
using IKVM.Reflection;
#else
using System.Reflection;
#endif

namespace ProtoBuf.Meta
{
    /// <summary>
    /// Represents the set of serialization callbacks to be used when serializing/deserializing a type.
    /// </summary>
    public class CallbackSet
    {
        private readonly MetaType metaType;
        internal CallbackSet(MetaType metaType)
        {
            if (metaType == null) throw new ArgumentNullException("metaType");
            this.metaType = metaType;
        }
        internal MethodInfo this[TypeModel.CallbackType callbackType]
        {
            get
            {
                switch (callbackType)
                {
                    case TypeModel.CallbackType.BeforeSerialize: return beforeSerialize;
                    case TypeModel.CallbackType.AfterSerialize: return afterSerialize;
                    case TypeModel.CallbackType.BeforeDeserialize: return beforeDeserialize;
                    case TypeModel.CallbackType.AfterDeserialize: return afterDeserialize;
                    default: throw new ArgumentException("Callback type not supported: " + callbackType.ToString(), "callbackType");
                }
            }
        }
        internal static bool CheckCallbackParameters(TypeModel model, MethodInfo method)
        {
            ParameterInfo[] args = method.GetParameters();
            for (int i = 0; i < args.Length; i++)
            {
                Type paramType = args[i].ParameterType;
                if(paramType == model.MapType(typeof(SerializationContext))) {}
                else if(paramType == model.MapType(typeof(System.Type))) {}
#if PLAT_BINARYFORMATTER
                else if(paramType == model.MapType(typeof(System.Runtime.Serialization.StreamingContext))) {}
#endif
                else return false;
            }
            return true;
        }
        private MethodInfo SanityCheckCallback(TypeModel model, MethodInfo callback)
        {
            metaType.ThrowIfFrozen();
            if (callback == null) return callback; // fine
            if (callback.IsStatic) throw new ArgumentException("Callbacks cannot be static", "callback");
            if (callback.ReturnType != model.MapType(typeof(void))
                || !CheckCallbackParameters(model, callback))
            {
                throw CreateInvalidCallbackSignature(callback);
            }
            return callback;
        }
        internal static Exception CreateInvalidCallbackSignature(MethodInfo method)
        {
            return new NotSupportedException("Invalid callback signature in " + method.DeclaringType.FullName + "." + method.Name);
        }
        private MethodInfo beforeSerialize, afterSerialize, beforeDeserialize, afterDeserialize;
        /// <summary>Called before serializing an instance</summary>
        public MethodInfo BeforeSerialize
        {
            get { return beforeSerialize; }
            set { beforeSerialize = SanityCheckCallback(metaType.Model, value); }
        }
        /// <summary>Called before deserializing an instance</summary>
        public MethodInfo BeforeDeserialize
        {
            get { return beforeDeserialize; }
            set { beforeDeserialize = SanityCheckCallback(metaType.Model, value); }
        }
        /// <summary>Called after serializing an instance</summary>
        public MethodInfo AfterSerialize
        {
            get { return afterSerialize; }
            set { afterSerialize = SanityCheckCallback(metaType.Model, value); }
        }
        /// <summary>Called after deserializing an instance</summary>
        public MethodInfo AfterDeserialize
        {
            get { return afterDeserialize; }
            set { afterDeserialize = SanityCheckCallback(metaType.Model, value); }
        }
        /// <summary>
        /// True if any callback is set, else False
        /// </summary>
        public bool NonTrivial
        {
            get
            {
                return beforeSerialize != null || beforeDeserialize != null
                    || afterSerialize != null || afterDeserialize != null;
            }
        }
    }
}
#endif