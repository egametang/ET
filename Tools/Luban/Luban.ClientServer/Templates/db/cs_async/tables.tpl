using Bright.Serialization;

namespace {{namespace}}
{
   
public static class {{name}}
{
    public static System.Collections.Generic.List<BrightDB.Transaction.TxnTable> TableList { get; } = new System.Collections.Generic.List<BrightDB.Transaction.TxnTable>
    {
    {{~ for table in tables~}}
        {{table.full_name}}.Table,
    {{~end}}
    };
}

}
