{{~
    go_full_name = x.go_full_name
    key_type = x.key_ttype
    key_type1 =  x.key_ttype1
    key_type2 =  x.key_ttype2
    value_type =  x.value_ttype
    index_field = x.index_field
    index_field1 = x.index_field1
    index_field2 = x.index_field2
~}}

package {{x.top_module}}

import "{{assembly.args.go_bright_module_name}}/serialization"

{{~if x.is_map_table~}}
type {{go_full_name}} struct {
    _dataMap map[{{go_define_type key_type}}]{{go_define_type value_type}}
    _dataList []{{go_define_type value_type}}
}

func New{{go_full_name}}(_buf *serialization.ByteBuf) (*{{go_full_name}}, error) {
	if size, err := _buf.ReadSize() ; err != nil {
		return nil, err
	} else {
		_dataList := make([]{{go_define_type value_type}}, 0, size)
		dataMap := make(map[{{go_define_type key_type}}]{{go_define_type value_type}})

		for i := 0 ; i < size ; i++ {
			if _v, err2 := {{go_deserialize_type value_type '_buf'}}; err2 != nil {
				return nil, err2
			} else {
				_dataList = append(_dataList, _v)
{{~if value_type.is_dynamic ~}}
        {{~for child in value_type.bean.hierarchy_not_abstract_children~}}
                if __v, __is := _v.(*{{child.go_full_name}}) ; __is {
                    dataMap[__v.{{index_field.convention_name}}] = _v
                    continue
                }
        {{~end~}}
{{~else~}}
				dataMap[_v.{{index_field.convention_name}}] = _v
{{~end~}}
			}
		}
		return &{{go_full_name}}{_dataList:_dataList, _dataMap:dataMap}, nil
	}
}

func (table *{{go_full_name}}) GetDataMap() map[{{go_define_type key_type}}]{{go_define_type value_type}} {
    return table._dataMap
}

func (table *{{go_full_name}}) GetDataList() []{{go_define_type value_type}} {
    return table._dataList
}

func (table *{{go_full_name}}) Get(key {{go_define_type key_type}}) {{go_define_type value_type}} {
    return table._dataMap[key]
}

{{~else if x.is_list_table~}}
type {{go_full_name}} struct {
    _dataList []{{go_define_type value_type}}
}

func New{{go_full_name}}(_buf *serialization.ByteBuf) (*{{go_full_name}}, error) {
	if size, err := _buf.ReadSize() ; err != nil {
		return nil, err
	} else {
		_dataList := make([]{{go_define_type value_type}}, 0, size)

		for i := 0 ; i < size ; i++ {
			if _v, err2 := {{go_deserialize_type value_type '_buf'}}; err2 != nil {
				return nil, err2
			} else {
				_dataList = append(_dataList, _v)
			}
		}
		return &{{go_full_name}}{_dataList:_dataList}, nil
	}
}

func (table *{{go_full_name}}) GetDataList() []{{go_define_type value_type}} {
    return table._dataList
}

func (table *{{go_full_name}}) Get(index int) {{go_define_type value_type}} {
    return table._dataList[index]
}

{{~else~}}

import "errors"

type {{go_full_name}} struct {
    _data {{go_define_type value_type}}
}

func New{{go_full_name}}(_buf *serialization.ByteBuf) (*{{go_full_name}}, error) {
	if size, err := _buf.ReadSize() ; err != nil {
		return nil, err
    } else if size != 1 {
        return nil, errors.New(" size != 1 ")
	} else {
		if _v, err2 := {{go_deserialize_type value_type '_buf'}}; err2 != nil {
			return nil, err2
		} else {
		    return &{{go_full_name}}{_data:_v}, nil
		}
	}
}

func (table *{{go_full_name}}) Get() {{go_define_type value_type}} {
    return table._data
}

{{~end~}}
