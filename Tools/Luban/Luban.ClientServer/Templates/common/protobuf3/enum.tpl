{{~
    name = x.name
    namespace_with_top_module = x.namespace_with_top_module
    comment = x.comment
    items = x.items
~}}

enum {{x.pb_full_name}} {
    {{~if items.empty? || items[0].int_value != 0~}}
    {{x.pb_full_name}}_EMPTY_PLACEHOLDER = 0;
    {{~end~}}
    {{~for item in items ~}}
    {{x.pb_full_name}}_{{item.name}} = {{item.int_value}};
    {{~end~}}
}