syntax = "proto2";

package {{x.namespace}};

// luban internal types begin
message Vector2 {
    required float x = 1;
    required float y = 2;
}

message Vector3 {
    required float x = 1;
    required float y = 2;
    required float z = 3;
}

message Vector4 {
    required float x = 1;
    required float y = 2;
    required float z = 3;
    required float w = 4;
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

