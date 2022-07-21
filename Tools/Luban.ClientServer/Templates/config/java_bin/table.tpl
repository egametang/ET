package {{x.namespace_with_top_module}};

import bright.serialization.*;

{{~
    name = x.name
    key_type = x.key_ttype
    key_type1 =  x.key_ttype1
    key_type2 =  x.key_ttype2
    value_type =  x.value_ttype
~}}

{{~if x.comment != '' ~}}
/**
 * {{x.escape_comment}}
 */
{{~end~}}
public final class {{name}} {
    {{~if x.is_map_table ~}}
    private final java.util.HashMap<{{java_box_define_type key_type}}, {{java_box_define_type value_type}}> _dataMap;
    private final java.util.ArrayList<{{java_box_define_type value_type}}> _dataList;
    
    public {{name}}(ByteBuf _buf) {
        _dataMap = new java.util.HashMap<{{java_box_define_type key_type}}, {{java_box_define_type value_type}}>();
        _dataList = new java.util.ArrayList<{{java_box_define_type value_type}}>();
        
        for(int n = _buf.readSize() ; n > 0 ; --n) {
            {{java_box_define_type value_type}} _v;
            {{java_deserialize '_buf' '_v' value_type}}
            _dataList.add(_v);
            _dataMap.put(_v.{{x.index_field.convention_name}}, _v);
        }
    }

    public java.util.HashMap<{{java_box_define_type key_type}}, {{java_box_define_type value_type}}> getDataMap() { return _dataMap; }
    public java.util.ArrayList<{{java_box_define_type value_type}}> getDataList() { return _dataList; }

{{~if value_type.is_dynamic~}}
    @SuppressWarnings("unchecked")
    public <T extends {{java_box_define_type value_type}}> T getAs({{java_define_type key_type}} key) { return (T)_dataMap.get(key); }
{{~end~}}
    public {{java_box_define_type value_type}} get({{java_define_type key_type}} key) { return _dataMap.get(key); }

    public void resolve(java.util.HashMap<String, Object> _tables) {
        for({{java_box_define_type value_type}} v : _dataList) {
            v.resolve(_tables);
        }
    }
    {{~else if x.is_list_table ~}}
    private final java.util.ArrayList<{{java_box_define_type value_type}}> _dataList;
    
    public {{name}}(ByteBuf _buf) {
        _dataList = new java.util.ArrayList<{{java_box_define_type value_type}}>();
        
        for(int n = _buf.readSize() ; n > 0 ; --n) {
            {{java_box_define_type value_type}} _v;
            {{java_deserialize '_buf' '_v' value_type}}
            _dataList.add(_v);
        }
    }

    public java.util.ArrayList<{{java_box_define_type value_type}}> getDataList() { return _dataList; }

    public {{java_box_define_type value_type}} get(int index) { return _dataList.get(index); }

    public void resolve(java.util.HashMap<String, Object> _tables) {
        for({{java_box_define_type value_type}} v : _dataList) {
            v.resolve(_tables);
        }
    }

    {{~else~}}
    private final {{java_define_type value_type}} _data;

    public final {{java_define_type value_type}} data() { return _data; }

    public {{name}}(ByteBuf _buf) {
        int n = _buf.readSize();
        if (n != 1) throw new SerializationException("table mode=one, but size != 1");
        {{java_deserialize '_buf' '_data' value_type}}
    }


    {{~ for field in value_type.bean.hierarchy_export_fields ~}}
{{~if field.comment != '' ~}}
    /**
     * {{field.escape_comment}}
     */
{{~end~}}
     public {{java_define_type field.ctype}} {{field.convention_getter_name}}() { return _data.{{field.convention_name}}; }
    {{~end~}}

    public void resolve(java.util.HashMap<String, Object> _tables) {
        _data.resolve(_tables);
    }
    {{~end~}}
}