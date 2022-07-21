{{
    is_value_type = x.is_value_type
    is_abstract_type = x.is_abstract_type
    name = x.name
    full_name = x.full_name
    parent_def_type = x.parent_def_type
    parent = x.parent
    fields = x.fields
    hierarchy_fields = x.hierarchy_fields
}}


{{x.typescript_namespace_begin}}

{{~if x.comment != '' ~}}
/**
 * {{x.escape_comment}}
 */
{{~end~}}
export {{if x.is_abstract_type}} abstract {{end}} class {{name}} extends {{if parent_def_type}}{{x.parent}}{{else}}BeanBase{{end}} {
{{~if x.is_abstract_type~}}
    static serializeTo(_buf_ : ByteBuf, _bean_ : {{name}}) {
        _buf_.WriteInt(_bean_.getTypeId())
        _bean_.serialize(_buf_)
    }

    static deserializeFrom(_buf_ : ByteBuf) : {{name}} {
        let  _bean_ :{{name}}
        switch (_buf_.ReadInt()) {
        {{~ for child in x.hierarchy_not_abstract_children~}}
            case {{child.id}}: _bean_ = new {{child.full_name}}(); break
        {{~end~}}
            default: throw new Error()
        }
        _bean_.deserialize(_buf_)
        return _bean_
    }
{{else}}
    static readonly __ID__ = {{x.id}}
    getTypeId() { return {{name}}.__ID__ }
{{~end~}}



    {{~ for field in fields ~}}
{{~if field.comment != '' ~}}
    /**
     * {{field.escape_comment}}
     */
{{~end~}}
     {{field.convention_name}}{{if field.is_nullable}}?{{end}} : {{ts_define_type field.ctype}}
    {{~end~}}

    constructor() {
        super()
    {{~ for field in fields ~}}
        this.{{field.convention_name}} = {{ts_ctor_default_value field.ctype}}
    {{~end~}}
    }
    

    serialize(_buf_ : ByteBuf) {
        {{~if parent_def_type~}}
        super.serialize(_buf_)
        {{~end~}}
        {{~ for field in fields ~}}
        {{ts_bin_serialize ('this.' + field.convention_name) '_buf_' field.ctype}}
        {{~end~}}
    }

    deserialize(_buf_ : ByteBuf) {
        {{~if parent_def_type~}}
        super.deserialize(_buf_)
        {{~end~}}
        {{~ for field in fields ~}}
        {{ts_bin_deserialize ('this.' + field.convention_name) '_buf_' field.ctype}}
        {{~end~}}
    }

    toString(): string {
        return '{{full_name}}{ '
    {{~ for field in hierarchy_fields ~}}
            + '{{field.convention_name}}:' + this.{{field.convention_name}} + ','
    {{~end~}}
        + '}'
    }
}
{{x.typescript_namespace_end}}