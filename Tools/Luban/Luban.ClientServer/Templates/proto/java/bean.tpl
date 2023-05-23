package {{x.namespace_with_top_module}};

import bright.serialization.*;

{{
    name = x.name
    parent_def_type = x.parent_def_type
    fields = x.fields
    hierarchy_fields = x.hierarchy_fields
    is_abstract_type = x.is_abstract_type
}}

{{~if x.comment != '' ~}}
/**
 * {{x.escape_comment}}
 */
{{~end~}}
public {{x.java_class_modifier}} class {{name}} extends {{if parent_def_type}}{{parent_def_type.full_name_with_top_module}}{{else}}bright.serialization.AbstractBean{{end}} {


    {{~ for field in fields ~}}
{{~if field.comment != '' ~}}
    /**
     * {{field.escape_comment}}
     */
{{~end~}}
    public {{java_define_type field.ctype}} {{field.convention_name}};
    {{~end~}}

{{~if !is_abstract_type~}}
    public static final int __ID__ = {{x.id}};

    @Override
    public int getTypeId() { return __ID__; }
{{~else~}}
    public abstract int getTypeId();
{{~end~}}

    @Override
    public void serialize(ByteBuf _buf) { 
        {{~if parent_def_type~}}
        super.serialize(_buf);
        {{~end~}}
        {{~ for field in fields ~}}
        {{java_serialize '_buf' field.convention_name field.ctype}}
        {{~end~}}
    }

    @Override
    public void deserialize(ByteBuf _buf)
    {
        {{~if parent_def_type~}}
        super.deserialize(_buf);
        {{~end~}}
        {{~ for field in fields ~}}
        {{java_deserialize '_buf' field.convention_name field.ctype}}
        {{~end~}}
    }

    public static void serialize{{name}}(ByteBuf _buf, {{name}} v) {
        {{~if is_abstract_type~}}
        _buf.writeInt(v.getTypeId());
        {{~end~}}
        v.serialize(_buf);
    }

    public static {{name}} deserialize{{name}}(ByteBuf _buf) {
        {{~if is_abstract_type~}}
        {{name}} v;
        switch (_buf.readInt()) {
        {{~for child in x.hierarchy_not_abstract_children~}}
            case {{child.full_name_with_top_module}}.__ID__: v = new {{child.full_name_with_top_module}}(); break;
        {{~end~}}
            default: throw new SerializationException();
        }
        {{~else~}}
        {{name}} v = new {{name}}();
        {{~end~}}
        v.deserialize(_buf);
        return v;
    }

    @Override
    public String toString() {
        return "{{full_name}}{ "
    {{~for field in hierarchy_fields ~}}
        + "{{field.convention_name}}:" + {{java_to_string field.convention_name field.ctype}} + ","
    {{~end~}}
        + "}";
    }
}
