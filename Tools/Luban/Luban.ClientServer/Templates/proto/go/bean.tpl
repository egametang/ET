{{-
    go_full_name = x.go_full_name
    parent_def_type = x.parent_def_type
    is_abstract_type = x.is_abstract_type
    hierarchy_fields = x.hierarchy_fields
    hierarchy_not_abstract_children = x.hierarchy_not_abstract_children
-}}

package {{x.top_module}}

import (
    "bright/serialization"
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
    {{~for field in hierarchy_fields ~}}
    {{go_serialize_field  field.ctype ("_v." + field.convention_name) '_buf'}}
    {{~end~}}
}

func (_v *{{go_full_name}})Deserialize(_buf *serialization.ByteBuf) (err error) {
    {{~for field in hierarchy_fields ~}}
    {{go_deserialize_field field.ctype ("_v." + field.convention_name) '_buf' 'err'}}
    {{~end~}}
    return
}

{{~if is_abstract_type~}}
func Serialize{{go_full_name}}(_v interface{}, _buf *serialization.ByteBuf) {
    _b := _v.(serialization.ISerializable)
    _buf.WriteInt(_b.GetTypeId())
    _b.Serialize(_buf)
}

func Deserialize{{go_full_name}}(_buf *serialization.ByteBuf) (_v serialization.ISerializable, err error) {
    var id int32
    if id, err = _buf.ReadInt() ; err != nil {
        return
    }
    switch id {
        {{~for child in hierarchy_not_abstract_children~}}
        case {{child.id}}: _v = &{{child.go_full_name}}{}; if err = _v.Deserialize(_buf); err != nil { return nil, err } else { return }
        {{~end~}}
        default: return nil, errors.New("unknown type id")
    }
}
{{~else~}}
func Serialize{{go_full_name}}(_v serialization.ISerializable, _buf *serialization.ByteBuf) {
    _v.Serialize(_buf)
}

func Deserialize{{go_full_name}}(_buf *serialization.ByteBuf) (*{{go_full_name}}, error) {
    v := &{{go_full_name}}{}
    if err := v.Deserialize(_buf); err == nil {
        return v, nil
    } else {
        return nil, err
    }
}
{{~end~}}
