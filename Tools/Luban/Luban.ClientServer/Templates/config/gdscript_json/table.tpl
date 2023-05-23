{{
    name = x.gdscript_full_name
    key_type = x.key_ttype
    key_type1 =  x.key_ttype1
    key_type2 =  x.key_ttype2
    value_type =  x.value_ttype
}}
class {{name}}:
{{~if x.is_map_table ~}}
    var _data_map = {}
    var _data_list = []

    func _init(_json_) -> void:
        self._data_map = {}
        self._data_list = []
        
        for _json2_ in _json_:
            var _v
            {{gdscript_deserialize_value '_v' '_json2_' value_type}}
            self._data_list.append(_v)
            self._data_map[_v.{{x.index_field.convention_name}}] = _v

    func get_data_map() -> Dictionary:
        return self._data_map
    func get_data_list() -> Array:
        return self._data_list

    func get(key): 
        return self._data_map.get(key)
{{~else if x.multi_key ~}}
{{~ for INDEX in x.index_list ~}}
    var _data_{{INDEX.index_field.convention_name}}_map = {}    
{{~ end ~}}
    var _data_list = []

    func _init(_json_) -> void:
        {{~ for INDEX in x.index_list ~}}
        self._data_{{INDEX.index_field.convention_name}}_map = {}
        {{~ end ~}}
        self._data_list = []
        
        for _json2_ in _json_:
            var _v
            {{gdscript_deserialize_value '_v' '_json2_' value_type}}
            self._data_list.append(_v)
        {{~ for INDEX in x.index_list ~}}
            self._data_{{INDEX.index_field.convention_name}}_map[_v.{{INDEX.index_field.convention_name}}] = _v
        {{~ end ~}}

    func get_data_map() -> Dictionary:
        return self._data_{{x.index_field.convention_name }}_map
    func get_data_list() -> Array:
        return self._data_list
{{~ for INDEX in x.index_list ~}}
    func get_by_{{INDEX.index_field.convention_name}}({{INDEX.index_field.convention_name}}):
        return self._data_{{INDEX.index_field.name}}_map.get({{INDEX.index_field.convention_name}})
{{~ end ~}}

    func get(key): 
        return self._data_{{x.index_field.convention_name }}_map.get(key)
{{~else if x.is_list_table ~}}
    var _data_list
    func _init(_json_) -> void:
        self._data_list = []
        
        for _json2_ in _json_:
            var _v
            {{gdscript_deserialize_value '_v' '_json2_' value_type}}
            self._data_list.append(_v)

    func get_data_list():
        return self._data_list

    func get(index):
        return self._data_list[index]
{{~else~}}
    var _data: Dictionary
    func _init(_json_) -> void:
        assert(len(_json_) == 1, 'table mode=one, but size != 1')
        self._data = _json_[0]

    func get_data() -> Dictionary: 
        return self._data

  {{~ for field in value_type.bean.hierarchy_export_fields ~}}
    {{~if field.comment != '' ~}}
    # {{field.escape_comment}}
    {{~end~}}
    func {{field.convention_name}}():
        return self._data.{{field.convention_name}}
  {{~end~}}
{{~end~}}