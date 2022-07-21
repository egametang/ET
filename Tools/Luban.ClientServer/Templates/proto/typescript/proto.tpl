{{
    name = x.name
    full_name = x.full_name
    parent = x.parent
    fields = x.fields
}}
{{x.typescript_namespace_begin}}

{{~if x.comment != '' ~}}
/**
 * {{x.escape_comment}}
 */
{{~end~}}
export class {{name}} extends Protocol {
    static readonly __ID__ = {{x.id}}
    getTypeId() { return {{name}}.__ID__ }


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
        {{~ for field in fields ~}}
        {{ts_bin_serialize ('this.' + field.convention_name) '_buf_' field.ctype}}
        {{~end~}}
    }

    deserialize(_buf_ : ByteBuf) {
        {{~ for field in fields ~}}
        {{ts_bin_deserialize ('this.' + field.convention_name) '_buf_' field.ctype}}
        {{~end~}}
    }

    toString(): string {
        return '{{full_name}}{ '
    {{~ for field in fields ~}}
            + '{{field.convention_name}}:' + this.{{field.convention_name}} + ','
    {{~end~}}
        + '}'
    }
}
{{x.typescript_namespace_end}}
