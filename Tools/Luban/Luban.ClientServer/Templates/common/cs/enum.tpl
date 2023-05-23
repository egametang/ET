{{~
    name = x.name
    namespace_with_top_module = x.namespace_with_top_module
    comment = x.comment
    items = x.items
~}}


{{cs_start_name_space_grace x.namespace_with_top_module}} 
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
        {{item.name}} = {{item.value}},
        {{~end~}}
    }

{{cs_end_name_space_grace x.namespace_with_top_module}} 
