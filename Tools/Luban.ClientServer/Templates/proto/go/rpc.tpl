{{-
    go_full_name = x.go_full_name
    parent_def_type = x.parent_def_type
    is_abstract_type = x.is_abstract_type
    hierarchy_fields = x.hierarchy_fields
    hierarchy_not_abstract_children = x.hierarchy_not_abstract_children
    arg_type = x.targ_type
    res_type = x.tres_type
-}}

package {{x.top_module}}

import (
    "bright/serialization"
    "errors"
)

{{x.go_bin_import}}

type {{go_full_name}} struct {
    SeqId int64
    Arg {{go_define_type arg_type}}
    Res {{go_define_type res_type}}
}

const TypeId_{{go_full_name}} = {{x.id}}

func (*{{go_full_name}}) GetTypeId() int32 {
    return {{x.id}}
}

func (_v *{{go_full_name}})Serialize(_buf *serialization.ByteBuf) {
    _buf.WriteLong(_v.SeqId)
    if _v.SeqId & 0x1 == 0 {
        {{go_serialize_field  arg_type "_v.Arg" '_buf'}}
    } else {
        {{go_serialize_field  res_type "_v.Res" '_buf'}}
    }
}

func (_v *{{go_full_name}})Deserialize(_buf *serialization.ByteBuf) (err error) {
    if _v.SeqId, err = _buf.ReadLong() ; err != nil {
        return
    }
    if _v.SeqId & 0x1 == 0 {
        {{go_deserialize_field arg_type "_v.Arg" '_buf' 'err'}}
    } else {
        {{go_deserialize_field res_type "_v.Res" '_buf' 'err'}}
    }
    return
}
