{{x.cpp_namespace_begin}}

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
class {{name}}
{
    {{~if x.is_map_table ~}}
    private:
    ::bright::HashMap<{{cpp_define_type key_type}}, {{cpp_define_type value_type}}> _dataMap;
    ::bright::Vector<{{cpp_define_type value_type}}> _dataList;
    
    public:
    bool load(ByteBuf& _buf)
    {        
        int n;
        if (!_buf.readSize(n)) return false;
        for(; n > 0 ; --n)
        {
            {{cpp_define_type value_type}} _v;
            {{cpp_deserialize '_buf' '_v' value_type}}
            _dataList.push_back(_v);
            _dataMap[_v->{{x.index_field.convention_name}}] = _v;
        }
        return true;
    }

    const ::bright::HashMap<{{cpp_define_type key_type}}, {{cpp_define_type value_type}}>& getDataMap() const { return _dataMap; }
    const ::bright::Vector<{{cpp_define_type value_type}}>& getDataList() const { return _dataList; }

    {{value_type.bean.cpp_full_name}}* getRaw({{cpp_define_type key_type}} key)
    { 
        auto it = _dataMap.find(key);
        return it != _dataMap.end() ? it->second.get() : nullptr;
    }

    {{cpp_define_type value_type}} get({{cpp_define_type key_type}} key)
    { 
        auto it = _dataMap.find(key);
        return it != _dataMap.end() ? it->second : nullptr;
    }

    void resolve(::bright::HashMap<::bright::String, void*>& _tables)
    {
        for(auto v : _dataList)
        {
            v->resolve(_tables);
        }
    }

    {{~else if x.is_list_table~}}
    private:
    ::bright::Vector<{{cpp_define_type value_type}}> _dataList;
    
    public:
    bool load(ByteBuf& _buf)
    {        
        int n;
        if (!_buf.readSize(n)) return false;
        for(; n > 0 ; --n)
        {
            {{cpp_define_type value_type}} _v;
            {{cpp_deserialize '_buf' '_v' value_type}}
            _dataList.push_back(_v);
        }
        return true;
    }

    const ::bright::Vector<{{cpp_define_type value_type}}>& getDataList() const { return _dataList; }

    {{value_type.bean.cpp_full_name}}* getRaw(size_t index) const
    { 
        return _dataList[index].get();
    }

    {{cpp_define_type value_type}} get(size_t index) const
    { 
        return _dataList[index];
    }

    void resolve(::bright::HashMap<::bright::String, void*>& _tables)
    {
        for(auto v : _dataList)
        {
            v->resolve(_tables);
        }
    }
    {{~else~}}
     private:
    {{cpp_define_type value_type}} _data;

    public:
    {{cpp_define_type value_type}} data() const { return _data; }

    bool load(ByteBuf& _buf)
    {
        int n;
        if (!_buf.readSize(n)) return false;
        if (n != 1) return false;
        {{cpp_deserialize '_buf' '_data' value_type}}
        return true;
    }

    void resolve(::bright::HashMap<::bright::String, void*>& _tables)
    {
        _data->resolve(_tables);
    }

    {{~ for field in value_type.bean.hierarchy_export_fields ~}}
{{~if field.comment != '' ~}}
    /**
     * {{field.escape_comment}}
     */
{{~end~}}
    {{cpp_define_type field.ctype}}& {{field.convention_getter_name}}() const { return _data->{{field.convention_name}}; }
    {{~end~}}
    {{~end~}}
};
{{x.cpp_namespace_end}}