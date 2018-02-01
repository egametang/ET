namespace ProtoBuf.Meta
{
    /// <summary>
    /// Indiate the variant of the protobuf .proto DSL syntax to use
    /// </summary>
    public enum ProtoSyntax
    {
        /// <summary>
        /// https://developers.google.com/protocol-buffers/docs/proto
        /// </summary>
        Proto2 = 0,
        /// <summary>
        /// https://developers.google.com/protocol-buffers/docs/proto3
        /// </summary>
        Proto3 = 1,
    }
}
