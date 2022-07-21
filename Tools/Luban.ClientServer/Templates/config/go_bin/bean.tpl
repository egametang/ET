{{~
    go_full_name = x.go_full_name
    parent_def_type = x.parent_def_type
    is_abstract_type = x.is_abstract_type
    hierarchy_fields = x.hierarchy_export_fields
    hierarchy_not_abstract_children = x.hierarchy_not_abstract_children
~}}

package {{x.top_module}}

import (
    "{{assembly.args.go_bright_module_name}}/serialization"
)

{{x.go_bin_import}}

type {{go_full_name}} struct {
    {{~for field in hierarchy_fields ~}}
    {{field.convention_name}} {{go_define_type field.ctype}}
    {{~end~}}
}

const TypeId_{{go_full_name}} = {{x.id}}

func (*{{go_full_name}}) GetTypeId() int32 {
    return {{x.id}}
}

func (_v *{{go_full_name}})Serialize(_buf *serialization.ByteBuf) {
    // not support
}

func (_v *{{go_full_name}})Deserialize(_buf *serialization.ByteBuf) (err error) {
    {{~for field in hierarchy_fields ~}}
    {{go_deserialize_field field.ctype ("_v." + field.convention_name) '_buf' 'err'}}
    {{~end~}}
    return
}

{{~if is_abstract_type~}}
func Deserialize{{go_full_name}}(_buf *serialization.ByteBuf) (interface{}, error) {
    var id int32
    var err error
    if id, err = _buf.ReadInt() ; err != nil {
        return nil, err
    }
    switch id {
        {{~for child in hierarchy_not_abstract_children~}}
        case {{child.id}}: _v := &{{child.go_full_name}}{}; if err = _v.Deserialize(_buf); err != nil { return nil, errors.New("{{child.full_name}}") } else { return _v, nil }
        {{~end~}}
        default: return nil, errors.New("unknown type id")
    }
}

{{~else~}}
func Deserialize{{go_full_name}}(_buf *serialization.ByteBuf) (*{{go_full_name}}, error) {
    v := &{{go_full_name}}{}
    if err := v.Deserialize(_buf); err == nil {
        return v, nil
    } else {
        return nil, err
    }
}
{{~end~}}
