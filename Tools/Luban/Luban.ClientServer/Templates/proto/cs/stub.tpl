using Bright.Serialization;

namespace {{namespace}}
{
    public static class {{name}}
    {
        public static System.Collections.Generic.Dictionary<int, Bright.Net.Codecs.ProtocolCreator> Factories { get; } = new System.Collections.Generic.Dictionary<int, Bright.Net.Codecs.ProtocolCreator>
        {
        {{~ for proto in protos ~}}
            [{{proto.full_name}}.__ID__] = () => new {{proto.full_name}}(),
        {{~end~}}

        {{~ for rpc in rpcs ~}}
            [{{rpc.full_name}}.__ID__] = () => new {{rpc.full_name}}(),
        {{~end~}}
        };
    }

}
