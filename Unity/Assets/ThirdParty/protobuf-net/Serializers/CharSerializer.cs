#if !NO_RUNTIME
using System;

#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
using IKVM.Reflection;
#else

#endif


namespace ProtoBuf.Serializers
{
    sealed class CharSerializer : UInt16Serializer
    {
#if FEAT_IKVM
        readonly Type expectedType;
#else
        static readonly Type expectedType = typeof(char);
#endif
        public CharSerializer(ProtoBuf.Meta.TypeModel model) : base(model)
        {
#if FEAT_IKVM
            expectedType = model.MapType(typeof(char));
#endif
        }
        public override Type ExpectedType { get { return expectedType; } }

#if !FEAT_IKVM
        public override void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteUInt16((ushort)(char)value, dest);
        }
        public override object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value == null); // since replaces
            return (char)source.ReadUInt16();
        }
#endif
        // no need for any special IL here; ushort and char are
        // interchangeable as long as there is no boxing/unboxing
    }
}
#endif