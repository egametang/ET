{{
    name = x.name
    key_ttype = x.key_ttype
    value_ttype = x.value_ttype
    base_table_type = x.base_table_type
    internal_table_type = x.internal_table_type
}}
using System;
using System.Threading.Tasks;

namespace {{x.namespace_with_top_module}}
{

{{~if x.comment != '' ~}}
/// <summary>
/// {{x.escape_comment}}
/// </summary>
{{~end~}}
public sealed class {{name}}
{
    public static {{base_table_type}} Table { get; } = new {{internal_table_type}}();

        private class {{internal_table_type}} : {{base_table_type}}
        {
            public {{internal_table_type}}() : base({{x.table_uid}}, "{{x.full_name}}")
            {

            }
        };

    public static ValueTask<{{db_cs_define_type value_ttype}}> GetAsync({{db_cs_define_type key_ttype}} key)
    {
        return Table.GetAsync(key);
    }

    public static ValueTask<{{db_cs_define_type value_ttype}}> CreateIfNotExistAsync({{db_cs_define_type key_ttype}} key)
    {
        return Table.CreateIfNotExistAsync(key);
    }

    public static Task InsertAsync({{db_cs_define_type key_ttype}} key, {{db_cs_define_type value_ttype}} value)
    {
        return Table.InsertAsync(key, value);
    }

    public static Task RemoveAsync({{db_cs_define_type key_ttype}} key)
    {
        return Table.RemoveAsync(key);
    }

    public static Task PutAsync({{db_cs_define_type key_ttype}} key, {{db_cs_define_type value_ttype}} value)
    {
        return Table.PutAsync(key, value);
    }

    public static ValueTask<{{db_cs_readonly_define_type value_ttype}}> SelectAsync({{db_cs_define_type key_ttype}} key)
    {
        return Table.SelectAsync<{{db_cs_readonly_define_type value_ttype}}>(key);
    }
}
}
