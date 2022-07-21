{{ 
    name = x.py_full_name
    key_type = x.key_ttype
    key_type1 =  x.key_ttype1
    key_type2 =  x.key_ttype2
    value_type =  x.value_ttype
}}

class {{name}}:
    {{~if x.is_map_table ~}}

    def __init__(self, _json_ ):
        self._dataMap = {}
        self._dataList = []
        
        for _json2_ in _json_:
            {{py3_deserialize_value '_v' '_json2_' value_type}}
            self._dataList.append(_v)
            self._dataMap[_v.{{x.index_field.convention_name}}] = _v

    def getDataMap(self) : return self._dataMap
    def getDataList(self) : return self._dataList

    def get(self, key) : return self._dataMap.get(key)
    {{~else if x.is_list_table ~}}

    def __init__(self, _json_ ):
        self._dataList = []
        
        for _json2_ in _json_:
            {{py3_deserialize_value '_v' '_json2_' value_type}}
            self._dataList.append(_v)

    def getDataList(self) : return self._dataList

    def get(self, index) : return self._dataList[index]

    {{~else~}}

    def __init__(self, _json_):
        if (len(_json_) != 1): raise Exception('table mode=one, but size != 1')
        {{py3_deserialize_value 'self._data' '_json_[0]' value_type}}

    def getData(self) : return self._data

    {{~ for field in value_type.bean.hierarchy_export_fields ~}}
{{~if field.comment != '' ~}}
    '''
    {{field.escape_comment}}
    '''
{{~end~}}
    def {{field.convention_name}}(self) : return self._data.{{field.convention_name}}
    {{~end~}}
    {{~end~}}
