{{-
    go_full_name = x.go_full_name
    parent_def_type = x.parent_def_type
    is_abstract_type = x.is_abstract_type
    fields = x.fields
    hierarchy_not_abstract_children = x.hierarchy_not_abstract_children
-}}

package {{x.top_module}}

import (
    "bright/serialization"
)

{{x.go_bin_import}}

type {{go_full_name}} struct {
    {{~for field in fields ~}}
    {{field.convention_name}} {{go_define_type field.ctype}}
    {{~end~}}
}

const TypeId_{{go_full_name}} = {{x.id}}

func (*{{go_full_name}}) GetTypeId() int32 {
    return {{x.id}}
}

func (_v *{{go_full_name}})Serialize(_buf *serialization.ByteBuf) {
    {{~for field in fields ~}}
    {{go_serialize_field  field.ctype ("_v." + field.convention_name) '_buf'}}
    {{~end~}}
}

func (_v *{{go_full_name}})Deserialize(_buf *serialization.ByteBuf) (err error) {
    {{~for field in fields ~}}
    {{go_deserialize_field field.ctype ("_v." + field.convention_name) '_buf' 'err'}}
    {{~end~}}
    return
}
