{{~
    name = x.name
    namespace_with_top_module = x.namespace_with_top_module
    comment = x.comment
    items = x.items
~}}

{{~if comment != '' ~}}
/**
 * {{comment | html.escape}}
 */
{{~end~}}
#[allow(dead_code)]
#[allow(non_camel_case_types)]
pub enum {{x.rust_full_name}} {
    {{~for item in items ~}}
{{~if item.comment != '' ~}}
    /**
     * {{item.escape_comment}}
     */
{{~end~}}
    {{item.name}} = {{item.int_value}},
    {{~end~}}
}

