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

    public static {{db_cs_define_type value_ttype}} Get({{db_cs_define_type key_ttype}} key)
    {
        return Table.Get(key);
    }

    public static {{db_cs_define_type value_ttype}} CreateIfNotExist({{db_cs_define_type key_ttype}} key)
    {
        return Table.CreateIfNotExist(key);
    }

    public static void Insert({{db_cs_define_type key_ttype}} key, {{db_cs_define_type value_ttype}} value)
    {
        Table.Insert(key, value);
    }

    public static void Remove({{db_cs_define_type key_ttype}} key)
    {
        Table.Remove(key);
    }

    public static void Put({{db_cs_define_type key_ttype}} key, {{db_cs_define_type value_ttype}} value)
    {
        Table.Put(key, value);
    }

    public static {{db_cs_readonly_define_type value_ttype}} Select({{db_cs_define_type key_ttype}} key)
    {
        return Table.Select(key);
    }

    public static ValueTask<{{db_cs_readonly_define_type value_ttype}}> SelectAsync({{db_cs_define_type key_ttype}} key)
    {
        return Table.SelectAsync<{{db_cs_readonly_define_type value_ttype}}>(key);
    }
}
}
