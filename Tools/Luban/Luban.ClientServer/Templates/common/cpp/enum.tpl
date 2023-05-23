{{~
    name = x.name
    namespace_with_top_module = x.namespace_with_top_module
    comment = x.comment
    items = x.items
~}}

{{x.cpp_namespace_begin}}
{{~if comment != '' ~}}
/**
 * {{comment | html.escape}}
 */
{{~end~}}
enum class {{name}}
{
    {{~ for item in items ~}}
{{~if item.comment != '' ~}}
    /**
     * {{item.escape_comment}}
     */
{{~end~}}
    {{item.name}} = {{item.value}},
    {{~end~}}
};
{{x.cpp_namespace_end}}