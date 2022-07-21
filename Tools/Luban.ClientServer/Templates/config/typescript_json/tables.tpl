{{
    name = x.name
    namespace = x.namespace
    tables = x.tables

}}

type JsonLoader = (file: string) => any

export class {{name}} {
    {{~ for table in tables ~}}
    private _{{table.name}}: {{table.full_name}}
{{~if table.comment != '' ~}}
    /**
     * {{table.escape_comment}}
     */
{{~end~}}
    get {{table.name}}(): {{table.full_name}}  { return this._{{table.name}};}
    {{~end~}}

    constructor(loader: JsonLoader) {
        let tables = new Map<string, any>()
        {{~for table in tables ~}}
        this._{{table.name}} = new {{table.full_name}}(loader('{{table.output_data_file}}'))
        tables.set('{{table.full_name}}', this._{{table.name}})
        {{~end~}}

        {{~ for table in tables ~}}
        this._{{table.name}}.resolve(tables)
        {{~end~}}
    }
}
