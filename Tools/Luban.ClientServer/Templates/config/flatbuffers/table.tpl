{{~
    name = x.name
    key_type = x.key_ttype
    value_type =  x.value_ttype
~}}

table {{x.flat_buffers_full_name}} {
    // WARN! The name 'data_list' is used by FlatBuffersJsonExporter. don't modify it!
    data_list:[{{flat_buffers_define_type value_type}}](required);
}

root_type {{x.flat_buffers_full_name}};