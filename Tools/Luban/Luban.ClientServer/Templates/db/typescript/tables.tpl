
export class {{name}} {
    static readonly tableList: TxnTable[] = [
    {{~ for table in tables~}}
        {{table.full_name}}.table,
    {{~end}}
    ]
}
