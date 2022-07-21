{{~
    name = x.name
    namespace = x.namespace
    tables = x.tables
~}}
package {{namespace}};

import com.google.gson.JsonElement;

public final class {{name}}
{
    public  interface  IJsonLoader {
        JsonElement load(String file) throws java.io.IOException;
    }

    {{~for table in tables ~}}
{{~if table.comment != '' ~}}
    /**
     * {{table.escape_comment}}
     */
{{~end~}}
    private final {{table.full_name_with_top_module}} {{table.inner_name}};
    public {{table.full_name_with_top_module}} get{{table.name}}() { return {{table.inner_name}}; }
    {{~end~}}

    public {{name}}(IJsonLoader loader) throws java.io.IOException {
        java.util.HashMap<String, Object> tables = new java.util.HashMap<>();
        {{~for table in tables ~}}
        {{table.inner_name}} = new {{table.full_name_with_top_module}}(loader.load("{{table.output_data_file}}")); 
        tables.put("{{table.full_name}}", {{table.inner_name}});
        {{~end~}}

        {{~ for table in tables ~}}
        {{table.inner_name}}.resolve(tables); 
        {{~end~}}
    }
}
