{{
    name = x.name
    full_name = x.full_name
    parent = x.parent
    fields = x.fields
    targ_type = x.targ_type
    tres_type = x.tres_type
}}
package {{x.namespace_with_top_module}};

import bright.serialization.*;

public final class {{name}} extends bright.net.Rpc<{{java_define_type targ_type}}, {{java_define_type tres_type}}> {

    public static final int __ID__ = {{x.id}};

    @Override
    public int getTypeId() { return __ID__; }

    @Override
    public void serialize(ByteBuf _buf) { 
        throw new UnsupportedOperationException();
    }

    @Override
    public void deserialize(ByteBuf _buf) {
        throw new UnsupportedOperationException();
    }
}
