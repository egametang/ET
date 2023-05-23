{{~
    go_full_name = x.go_full_name
    parent_def_type = x.parent_def_type
    is_abstract_type = x.is_abstract_type
    hierarchy_fields = x.hierarchy_export_fields
    hierarchy_not_abstract_children = x.hierarchy_not_abstract_children
~}}

package {{x.top_module}}

{{x.go_json_import}}

type {{go_full_name}} struct {
    {{~for field in hierarchy_fields ~}}
    {{field.convention_name}} {{go_define_type field.ctype}}
    {{~end~}}
}

const TypeId_{{go_full_name}} = {{x.id}}

func (*{{go_full_name}}) GetTypeId() int32 {
    return {{x.id}}
}

func (_v *{{go_full_name}})Deserialize(_buf map[string]interface{}) (err error) {
    {{~for field in hierarchy_fields ~}}
    {{go_deserialize_json_field field.ctype ("_v." + field.convention_name) field.name '_buf'}}
    {{~end~}}
    return
}

{{~if is_abstract_type~}}
func Deserialize{{go_full_name}}(_buf map[string]interface{}) (interface{}, error) {
    var id string
    var _ok_ bool
    if id, _ok_ = _buf["{{x.json_type_name_key}}"].(string) ; !_ok_ {
        return nil, errors.New("type id missing")
    }
    switch id {
        {{~for child in hierarchy_not_abstract_children~}}
        case "{{cs_impl_data_type child x}}": _v := &{{child.go_full_name}}{}; if err := _v.Deserialize(_buf); err != nil { return nil, errors.New("{{child.full_name}}") } else { return _v, nil }
        {{~end~}}
        default: return nil, errors.New("unknown type id")
    }
}
{{~else~}}
func Deserialize{{go_full_name}}(_buf map[string]interface{}) (*{{go_full_name}}, error) {
    v := &{{go_full_name}}{}
    if err := v.Deserialize(_buf); err == nil {
        return v, nil
    } else {
        return nil, err
    }
}
{{~end~}}
