{{~
    name = x.name
    key_type = x.key_ttype
    value_type =  x.value_ttype
~}}

message {{x.pb_full_name}} {
    repeated {{protobuf_define_type value_type}} data_list = 1 [packed = false];
}