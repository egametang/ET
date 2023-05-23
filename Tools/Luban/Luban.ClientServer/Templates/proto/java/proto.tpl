package {{x.namespace_with_top_module}};

import bright.serialization.*;

{{
    name = x.name
    parent_def_type = x.parent_def_type
    fields = x.fields
}}

{{~if x.comment != '' ~}}
/**
 * {{x.escape_comment}}
 */
{{~end~}}
public final class {{name}} extends bright.net.Protocol
{
    {{~ for field in fields ~}}
{{~if field.comment != '' ~}}
    /**
     * {{field.escape_comment}}
     */
{{~end~}}
    public {{java_define_type field.ctype}} {{field.convention_name}};
    {{~end~}}

    public static final int __ID__ = {{x.id}};

    @Override
    public int getTypeId() { return __ID__; }

    @Override
    public void serialize(ByteBuf _buf)
    {
        {{~ for field in fields ~}}
        {{java_serialize '_buf' field.convention_name field.ctype}}
        {{~end~}}
    }

    @Override
    public void deserialize(ByteBuf _buf)
    {
        {{~ for field in fields ~}}
        {{java_deserialize '_buf' field.convention_name field.ctype}}
        {{~end~}}
    }

    @Override
    public String toString() {
        return "{{full_name}}{ "
    {{~for field in fields ~}}
        + "{{field.convention_name}}:" + {{java_to_string field.convention_name field.ctype}} + ","
    {{~end~}}
        + "}";
    }
}
