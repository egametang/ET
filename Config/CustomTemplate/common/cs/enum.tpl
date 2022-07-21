{{~
    name = x.name
    namespace_with_top_module = x.namespace_with_top_module
    comment = x.comment
    items = x.items
~}}

namespace {{namespace_with_top_module}}
{
{{~if comment != '' ~}}
    /// <summary>
    /// {{comment | html.escape}}
    /// </summary>
{{~end~}}
    {{~if x.is_flags~}}
    [System.Flags]
    {{~end~}}
    public enum {{name}}
    {
        {{~ for item in items ~}}
{{~if item.comment != '' ~}}
        /// <summary>
        /// {{item.escape_comment}}
        /// </summary>
{{~end~}}
{{if (has_tag item 'range_attr')}}
        [ET.NumericRange({{(get_tag item 'range_attr')}})]
{{~end~}}
        {{item.name}} = {{item.value}},
{{if (has_tag item 'numeric')}}
        {{item.name}}Base = {{item.value}} * 10 + 1,
        {{item.name}}Add = {{item.value}} * 10 + 2,
        {{item.name}}Pct = {{item.value}} * 10 + 3,
        {{item.name}}FinalAdd = {{item.value}} * 10 + 4,
        {{item.name}}FinalPct = {{item.value}} * 10 + 5,
{{~end~}}
        {{~end~}}
    }
}
