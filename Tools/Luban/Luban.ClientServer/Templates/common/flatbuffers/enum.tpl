{{~
    name = x.name
    namespace_with_top_module = x.namespace_with_top_module
    comment = x.comment
    items = x.items
~}}

enum {{x.flat_buffers_full_name}}:int {
    {{~if !x.has_zero_value_item~}}
    __GENERATE_DEFAULT_VALUE = 0,
    {{~end~}}
    {{~for item in items ~}}
    {{x.pb_full_name}}_{{item.name}} = {{item.int_value}},
    {{~end~}}
}