local enums =
{
    {{~ for c in enums ~}}
    ---@class {{c.full_name}}
    {{~ for item in c.items ~}}
     ---@field public {{item.name}} integer
    {{~end~}}
    ['{{c.full_name}}'] = {  {{ for item in c.items }} {{item.name}}={{item.int_value}}, {{end}} };
    {{~end~}}
}

local tables =
{
{{~for table in tables ~}}
    {{~if table.is_map_table ~}}
    { name='{{table.name}}', file='{{table.output_data_file}}', mode='map', index='{{table.index}}', value_type='{{table.value_ttype.bean.full_name}}' },
    {{~else if table.is_list_table ~}}
    { name='{{table.name}}', file='{{table.output_data_file}}', mode='list', index='{{table.index}}', value_type='{{table.value_ttype.bean.full_name}}' },
    {{~else~}}
    { name='{{table.name}}', file='{{table.output_data_file}}', mode='one', value_type='{{table.value_ttype.bean.full_name}}'},
    {{end}}
{{~end~}}
}

return { enums = enums, tables = tables }
