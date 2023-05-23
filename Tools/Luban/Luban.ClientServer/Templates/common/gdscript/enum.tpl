{{~
    name = x.name
    namespace_with_top_module = x.namespace_with_top_module
    comment = x.comment
    items = x.items
~}}

{{~if comment != '' ~}}

# {{comment | html.escape}}
{{~end~}}
enum {{x.gdscript_full_name}}{
    {{~ for item in items ~}}
{{~if item.comment != '' ~}}
    {{item.name}} = {{item.int_value}}, # {{item.escape_comment}}
{{~else~}}
    {{item.name}} = {{item.int_value}},
{{~end~}}
    {{~end~}}
}