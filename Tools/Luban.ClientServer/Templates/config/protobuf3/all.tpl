syntax = "proto3";

package {{x.namespace}};

// luban internal types begin
message Vector2 {
    float x = 1;
    float y = 2;
}

message Vector3 {
    float x = 1;
    float y = 2;
    float z = 3;
}

message Vector4 {
    float x = 1;
    float y = 2;
    float z = 3;
    float w = 4;
}
// luban internal types end

{{~for enum in x.enums ~}}
{{enum}}
{{~end~}}

{{~for bean in x.beans~}}
{{bean}}
{{~end~}}

{{~for table in x.tables~}}
{{table}}
{{~end~}}

