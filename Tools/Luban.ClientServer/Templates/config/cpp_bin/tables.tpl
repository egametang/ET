{{~
    tables = x.tables
    name = x.name
~}}
class {{name}}
{
    public:
    {{~for table in tables ~}}
{{~if table.comment != '' ~}}
    /**
     * {{table.escape_comment}}
     */
{{~end~}}
     {{table.cpp_full_name}} {{table.name}};
    {{~end~}}

    bool load(::bright::Loader<ByteBuf> loader)
    {
        ::bright::HashMap<::bright::String, void*> __tables__;

        ByteBuf buf;
        {{~for table in tables~}}
        if (!loader(buf, "{{table.output_data_file}}")) return false;
        if (!{{table.name}}.load(buf)) return false;
        __tables__["{{table.full_name}}"] = &{{table.name}};
        {{~end~}}

        {{~for table in tables ~}}
        {{table.name}}.resolve(__tables__); 
        {{~end~}}
        return true;
    }
};
