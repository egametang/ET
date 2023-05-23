
    type ProtocolFactory = () => Protocol

    export class {{name}} {
        static readonly Factories = new Map<number, ProtocolFactory>([

        {{~ for proto in protos ~}}
            [{{proto.full_name}}.__ID__, () => new {{proto.full_name}}()],
        {{~end~}}

        {{~ for rpc in rpcs ~}}
            // TODO RPC .. [{{rpc.full_name}}.__ID__] = () => new {{rpc.full_name}}(),
        {{~end~}}
        ])
    }
