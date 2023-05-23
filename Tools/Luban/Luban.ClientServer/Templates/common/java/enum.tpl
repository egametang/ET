{{~
    name = x.name
    namespace_with_top_module = x.namespace_with_top_module
    comment = x.comment
    items = x.items
~}}

package {{namespace_with_top_module}};
{{~if comment != '' ~}}
/**
 * {{comment | html.escape}}
 */
{{~end~}}
public final class {{name}} {
    {{~ for item in items ~}}
{{~if item.comment != '' ~}}
    /**
     * {{item.escape_comment}}
     */
{{~end~}}
    public static final int {{item.name}} = {{item.int_value}};
    {{~end~}}
}
