using Bright.Serialization;
using System.Collections.Generic;


{{cs_start_name_space_grace x.namespace_with_top_module}}
   {{ 
        name = x.name
        key_type = x.key_ttype
        key_type1 =  x.key_ttype1
        key_type2 =  x.key_ttype2
        value_type =  x.value_ttype
    }}
{{~if x.comment != '' ~}}
/// <summary>
/// {{x.escape_comment}}
/// </summary>
{{~end~}}
public partial class {{name}}
{
    {{~if x.is_map_table ~}}
    private readonly Dictionary<{{cs_define_type key_type}}, {{cs_define_type value_type}}> _dataMap;
    private readonly List<{{cs_define_type value_type}}> _dataList;
    
    public {{name}}(ByteBuf _buf)
    {
        _dataMap = new Dictionary<{{cs_define_type key_type}}, {{cs_define_type value_type}}>();
        _dataList = new List<{{cs_define_type value_type}}>();
        
        for(int n = _buf.ReadSize() ; n > 0 ; --n)
        {
            {{cs_define_type value_type}} _v;
            {{cs_deserialize '_buf' '_v' value_type}}
            _dataList.Add(_v);
            _dataMap.Add(_v.{{x.index_field.convention_name}}, _v);
        }
        PostInit();
    }

    public Dictionary<{{cs_define_type key_type}}, {{cs_define_type value_type}}> DataMap => _dataMap;
    public List<{{cs_define_type value_type}}> DataList => _dataList;

{{~if value_type.is_dynamic~}}
    public T GetOrDefaultAs<T>({{cs_define_type key_type}} key) where T : {{cs_define_type value_type}} => _dataMap.TryGetValue(key, out var v) ? (T)v : null;
    public T GetAs<T>({{cs_define_type key_type}} key) where T : {{cs_define_type value_type}} => (T)_dataMap[key];
{{~end~}}
    public {{cs_define_type value_type}} GetOrDefault({{cs_define_type key_type}} key) => _dataMap.TryGetValue(key, out var v) ? v : null;
    public {{cs_define_type value_type}} Get({{cs_define_type key_type}} key) => _dataMap[key];
    public {{cs_define_type value_type}} this[{{cs_define_type key_type}} key] => _dataMap[key];

    public void Resolve(Dictionary<string, object> _tables)
    {
        foreach(var v in _dataList)
        {
            v.Resolve(_tables);
        }
        PostResolve();
    }

    public void TranslateText(System.Func<string, string, string> translator)
    {
        foreach(var v in _dataList)
        {
            v.TranslateText(translator);
        }
    }
        {{~else if x.is_list_table ~}}
    private readonly List<{{cs_define_type value_type}}> _dataList;

    {{~if x.is_union_index~}}
    private {{cs_table_union_map_type_name x}} _dataMapUnion;
    {{~else if !x.index_list.empty?~}}
    {{~for idx in x.index_list~}}
    private Dictionary<{{cs_define_type idx.type}}, {{cs_define_type value_type}}> _dataMap_{{idx.index_field.name}};
    {{~end~}}
    {{~end~}}

    public {{name}}(ByteBuf _buf)
    {
        _dataList = new List<{{cs_define_type value_type}}>();
        
        for(int n = _buf.ReadSize() ; n > 0 ; --n)
        {
            {{cs_define_type value_type}} _v;
            {{cs_deserialize '_buf' '_v' value_type}}
            _dataList.Add(_v);
        }
    {{~if x.is_union_index~}}
        _dataMapUnion = new {{cs_table_union_map_type_name x}}();
        foreach(var _v in _dataList)
        {
            _dataMapUnion.Add(({{cs_table_key_list x "_v"}}), _v);
        }
    {{~else if !x.index_list.empty?~}}
    {{~for idx in x.index_list~}}
        _dataMap_{{idx.index_field.name}} = new Dictionary<{{cs_define_type idx.type}}, {{cs_define_type value_type}}>();
    {{~end~}}
    foreach(var _v in _dataList)
    {
    {{~for idx in x.index_list~}}
        _dataMap_{{idx.index_field.name}}.Add(_v.{{idx.index_field.convention_name}}, _v);
    {{~end~}}
    }
    {{~end~}}
        PostInit();
    }


    public List<{{cs_define_type value_type}}> DataList => _dataList;

    {{~if x.is_union_index~}}
    public {{cs_define_type value_type}} Get({{cs_table_get_param_def_list x}}) => _dataMapUnion.TryGetValue(({{cs_table_get_param_name_list x}}), out {{cs_define_type value_type}} __v) ? __v : null;
    {{~else if !x.index_list.empty? ~}}
        {{~for idx in x.index_list~}}
    public {{cs_define_type value_type}} GetBy{{idx.index_field.convention_name}}({{cs_define_type idx.type}} key) => _dataMap_{{idx.index_field.name}}.TryGetValue(key, out {{cs_define_type value_type}} __v) ? __v : null;
        {{~end~}}
    {{~end~}}

    public void Resolve(Dictionary<string, object> _tables)
    {
        foreach(var v in _dataList)
        {
            v.Resolve(_tables);
        }
        PostResolve();
    }

    public void TranslateText(System.Func<string, string, string> translator)
    {
        foreach(var v in _dataList)
        {
            v.TranslateText(translator);
        }
    }
    {{~else~}}

     private readonly {{cs_define_type value_type}} _data;

    public {{name}}(ByteBuf _buf)
    {
        int n = _buf.ReadSize();
        if (n != 1) throw new SerializationException("table mode=one, but size != 1");
        {{cs_deserialize '_buf' '_data' value_type}}
        PostInit();
    }


    {{~ for field in value_type.bean.hierarchy_export_fields ~}}
{{~if field.comment != '' ~}}
    /// <summary>
    /// {{field.escape_comment}}
    /// </summary>
{{~end~}}
     public {{cs_define_type field.ctype}} {{field.convention_name}} => _data.{{field.convention_name}};
    {{~end~}}

    public void Resolve(Dictionary<string, object> _tables)
    {
        _data.Resolve(_tables);
        PostResolve();
    }

    public void TranslateText(System.Func<string, string, string> translator)
    {
        _data.TranslateText(translator);
    }

    {{~end~}}
    
    partial void PostInit();
    partial void PostResolve();
}

{{cs_end_name_space_grace x.namespace_with_top_module}}