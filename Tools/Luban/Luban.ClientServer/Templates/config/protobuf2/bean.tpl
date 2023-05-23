{{~
    name = x.name
    parent_def_type = x.parent_def_type
    export_fields = x.export_fields
    hierarchy_export_fields = x.hierarchy_export_fields
~}}

message {{x.pb_full_name}} {
{{~if x.is_abstract_type ~}}
    oneof value {
        {{~for c in x.hierarchy_not_abstract_children~}}
        {{c.pb_full_name}} {{c.name}} = {{c.auto_id}};
        {{~end~}}
    }
{{~else~}}
    {{~for f in hierarchy_export_fields ~}}
    {{protobuf_pre_decorator f.ctype}} {{protobuf_define_type f.ctype}} {{f.name}} = {{f.auto_id}} {{protobuf_suffix_options f.ctype}};
    {{~end~}}
{{~end~}}
}