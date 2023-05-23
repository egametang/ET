package {{namespace}};

import java.util.*;
import bright.net.*;

public final class {{name}} {
    static final Map<Integer, IProtocolCreator> _factories = new HashMap<>();

    static {
        {{~ for proto in protos ~}}
        _factories.put({{proto.full_name_with_top_module}}.__ID__, {{proto.full_name_with_top_module}}::new);
        {{~end~}}

        {{~ for rpc in rpcs ~}}
        _factories.put({{rpc.full_name_with_top_module}}.__ID__, {{rpc.full_name_with_top_module}}::new);
        {{~end~}}
    }

    public static Map<Integer, IProtocolCreator> getFactories() {
        return _factories;
    }
}