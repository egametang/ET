#if !NO_RUNTIME
using System;

namespace ProtoBuf.Serializers
{
    sealed class CharSerializer : UInt16Serializer
    {
        static readonly Type expectedType = typeof(char);

        public CharSerializer(ProtoBuf.Meta.TypeModel model) : base(model)
        {

        }

        public override Type ExpectedType => expectedType;

        public override void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteUInt16((ushort)(char)value, dest);
        }

        public override object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value == null); // since replaces
            return (char)source.ReadUInt16();
        }

        // no need for any special IL here; ushort and char are
        // interchangeable as long as there is no boxing/unboxing
    }
}
#endif