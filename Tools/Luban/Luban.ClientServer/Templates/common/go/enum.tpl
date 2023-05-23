{{~
    name = x.name
    namespace_with_top_module = x.namespace_with_top_module
    comment = x.comment
    items = x.items
~}}

package {{x.top_module}}

const (
    {{~for item in items ~}}
    {{x.go_full_name}}_{{item.name}} = {{item.int_value}}
    {{~end~}}
)
