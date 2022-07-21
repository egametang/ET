
{{
    name = x.name
    parent_def_type = x.parent_def_type
    export_fields = x.export_fields
    hierarchy_export_fields = x.hierarchy_export_fields
}}

{{x.typescript_namespace_begin}}
{{~if x.comment != '' ~}}
/**
 * {{x.escape_comment}}
 */
{{~end~}}
export {{if x.is_abstract_type}} abstract {{end}} class {{name}} {{if parent_def_type}} extends {{x.parent}}{{end}} {
{{~if x.is_abstract_type~}}
    static constructorFrom(_buf_: ByteBuf): {{name}} {
        switch (_buf_.ReadInt()) {
        {{~ for child in x.hierarchy_not_abstract_children~}}
            case {{child.id}}: return new {{child.full_name}}(_buf_)
        {{~end~}}
            default: throw new Error()
        }
    }
{{~end~}}

    constructor(_buf_: ByteBuf) {
        {{~if parent_def_type~}}
        super(_buf_)
        {{~end~}}
        {{~ for field in export_fields ~}}
        {{ts_bin_constructor ('this.' + field.convention_name) '_buf_' field.ctype}}
        {{~end~}}
    }

    {{~ for field in export_fields ~}}
{{~if field.comment != '' ~}}
    /**
     * {{field.escape_comment}}
     */
{{~end~}}
    readonly {{field.convention_name}}: {{ts_define_type field.ctype}}
    {{~if field.gen_ref~}}
    {{field.ts_ref_validator_define}}
    {{~end~}}
    {{~end~}}

    resolve(_tables: Map<string, any>) {
        {{~if parent_def_type~}}
        super.resolve(_tables)
        {{~end~}}
        {{~ for field in export_fields ~}}
        {{~if field.gen_ref~}}
        {{ts_ref_validator_resolve field}}
        {{~else if field.has_recursive_ref~}}
        {{ts_recursive_resolve field '_tables'}}
        {{~end~}}
        {{~end~}}
    }
}

{{x.typescript_namespace_end}}
