
namespace ProtoBuf.Serializers
{
    /*
    sealed class KeyValuePairDecorator : IProtoSerializer
    {
        private readonly Type pairType;
        private readonly IProtoSerializer keyTail, valueTail;
        public KeyValuePairDecorator(Type pairType, IProtoSerializer keyTail, IProtoSerializer valueTail) {
            Helpers.DebugAssert(pairType != null);
            Helpers.DebugAssert(keyTail != null);
            Helpers.DebugAssert(valueTail != null);
            Helpers.DebugAssert(pairType == typeof(System.Collections.Generic.KeyValuePair<,>).MakeGenericType(keyTail.ExpectedType,valueTail.ExpectedType), "Key/value type mismatch");
            this.pairType = pairType;
            this.keyTail = keyTail;
            this.valueTail = valueTail;
        }

        public Type ExpectedType { get { return pairType;}}
        public bool ReturnsValue { get { return true; } }
        public bool RequiresOldValue { get { return true; } }
        public abstract void Write(object value, ProtoWriter dest)
        {

        }
        public abstract object Read(object value, ProtoReader source)
        {

        }
#if FEAT_COMPILER
        void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            throw new NotImplementedException();
        }
        void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom) {
            throw new NotImplementedException();
        }
#endif
    }*/
}
