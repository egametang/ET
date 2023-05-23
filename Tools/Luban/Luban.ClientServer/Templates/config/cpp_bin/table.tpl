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

{{~if x.is_list_table ~}}
{{~if x.is_union_index~}}
typedef struct t_{{name}}_MultiKey
{
{{~for idx in x.index_list~}}
     {{cpp_define_type idx.type}} {{idx.index_field.name}};
{{~end~}}
    bool operator==(const t_{{name}}_MultiKey& stOther) const
	{
        bool bEqual = true;
        {{~for idx in x.index_list~}}
        bEqual = bEqual && (stOther.{{idx.index_field.name}} == {{idx.index_field.name}});
        {{~end~}}
        return bEqual;
    }
}t_{{name}}_MultiKey;

typedef struct t_{{name}}_MultiKey_HashFunc
{
	std::size_t operator()(const t_{{name}}_MultiKey& stMK) const
	{
        std::size_t sHash = 0;
        {{~for idx in x.index_list~}}            
        sHash ^= std::hash<{{cpp_define_type idx.type}}>()(stMK.{{idx.index_field.name}});            
        {{~end~}}
		return sHash;
	}
}t_{{name}}_MultiKey_HashFunc;
{{~end~}}
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
    {{~if x.is_union_index~}}
        ::bright::HashMapMultiKey<t_{{name}}_MultiKey, {{cpp_define_type value_type}}, t_{{name}}_MultiKey_HashFunc> _dataMap;
    {{~else if !x.index_list.empty?~}}
        {{~for idx in x.index_list~}}
        ::bright::HashMap<{{cpp_define_type idx.type}}, {{cpp_define_type value_type}}> _dataMap_{{idx.index_field.name}};
        {{~end~}}
    {{~else~}}
    {{~end~}}    
    
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
                {{~if x.is_union_index~}}
                t_{{name}}_MultiKey stKey;
                {{~for idx in x.index_list~}}
                stKey.{{idx.index_field.name}} = _v->{{idx.index_field.name}};
                {{~end~}}
                _dataMap[stKey] = _v;
                {{~else if !x.index_list.empty?~}}
                {{~for idx in x.index_list~}}
                _dataMap_{{idx.index_field.name}}[_v->{{idx.index_field.name}}] = _v;
                {{~end~}}
                {{~else~}}
                {{~end~}}
            }
            return true;
        }

        const ::bright::Vector<{{cpp_define_type value_type}}>& getDataList() const { return _dataList; }

        {{~if x.is_union_index~}}
        ::bright::HashMapMultiKey<t_{{name}}_MultiKey, {{cpp_define_type value_type}}, t_{{name}}_MultiKey_HashFunc>& getDataMap()
        {
            return _dataMap;
        }
        {{value_type.bean.cpp_full_name}}* getRaw(t_{{name}}_MultiKey& key)
        {
            auto it = _dataMap.find(key);
            return it != _dataMap.end() ? it->second.get() : nullptr;
        }
        {{cpp_define_type value_type}} get(t_{{name}}_MultiKey& key)
        {
            auto it = _dataMap.find(key);
            return it != _dataMap.end() ? it->second : nullptr;
        }
        {{~else if !x.index_list.empty?~}}
        {{~for idx in x.index_list~}}
        ::bright::HashMap<{{cpp_define_type idx.type}}, {{cpp_define_type value_type}}>& getDataMapBy{{idx.index_field.name}}()
        {
            return _dataMap_{{idx.index_field.name}};
        }
        {{value_type.bean.cpp_full_name}}* getRawBy{{idx.index_field.name}}({{cpp_define_type idx.type}} key)
        {                    
            auto it = _dataMap_{{idx.index_field.name}}.find(key);
            return it != _dataMap_{{idx.index_field.name}}.end() ? it->second.get() : nullptr;
        }
        {{cpp_define_type value_type}} getBy{{idx.index_field.name}}({{cpp_define_type idx.type}} key)
        {
            auto it = _dataMap_{{idx.index_field.name}}.find(key);
            return it != _dataMap_{{idx.index_field.name}}.end() ? it->second : nullptr;
        }
        {{~end~}}
        {{~else~}}
            {{value_type.bean.cpp_full_name}}* getRaw(size_t index) const
            { 
                return _dataList[index].get();
            }

            {{cpp_define_type value_type}} get(size_t index) const
            { 
                return _dataList[index];
            }
        {{~end~}}
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