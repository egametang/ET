namespace {{x.namespace}};

// luban internal types begin
struct Vector2 {
    x:float;
    y:float;
}

struct Vector3 {
    x:float;
    y:float;
    z:float;
}

struct Vector4 {
    x:float;
    y:float;
    z:float;
    w:float;
}
// luban internal types end

{{~for enum in x.enums ~}}
{{enum}}
{{~end~}}

{{~for bean in x.beans~}}
{{bean}}
{{~end~}}

{{~for map in x.maps~}}
table KeyValue_{{flat_buffers_define_type map.key_type}}_{{flat_buffers_define_type map.value_type}} {
    key: {{flat_buffers_define_type map.key_type}}{{flat_buffers_type_metadata map.key_type}};
    value: {{flat_buffers_define_type map.value_type}}{{flat_buffers_type_metadata map.value_type}};
}
{{~end~}}

{{~for table in x.tables~}}
{{table}}
{{~end~}}

