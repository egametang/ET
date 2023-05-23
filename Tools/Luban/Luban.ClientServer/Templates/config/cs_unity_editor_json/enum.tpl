{{~
    name = x.name
    comment = x.comment
    items = x.items
    itemType = 'Bright.Config.EditorEnumItemInfo'
~}}

{{cs_start_name_space_grace x.namespace_with_editor_top_module}}
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

    public static partial class {{name}}_Metadata
    {
        {{~ for item in items ~}}
        public static readonly {{itemType}} {{item.name}} = new {{itemType}}("{{item.name}}", "{{item.alias}}", {{item.int_value}}, "{{item.comment}}");
        {{~end~}}

        private static readonly System.Collections.Generic.List<{{itemType}}> __items = new System.Collections.Generic.List<{{itemType}}>
        {
        {{~ for item in items ~}}
            {{item.name}},
        {{~end~}}
        };

        public static System.Collections.Generic.List<{{itemType}}> GetItems() => __items;

        public static {{itemType}} GetByName(string name)
        {
            return __items.Find(c => c.Name == name);
        }

        public static {{itemType}} GetByNameOrAlias(string name)
        {
            return __items.Find(c => c.Name == name || c.Alias == name);
        }

        public static {{itemType}} GetByValue(int value)
        {
            return __items.Find(c => c.Value == value);
        }
    }

{{cs_end_name_space_grace x.namespace_with_editor_top_module}}
