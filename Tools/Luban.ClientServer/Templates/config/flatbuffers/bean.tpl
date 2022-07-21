{{~
    name = x.name
    parent_def_type = x.parent_def_type
    export_fields = x.export_fields
    hierarchy_export_fields = x.hierarchy_export_fields
~}}

{{~if x.is_abstract_type ~}}
union {{x.flat_buffers_full_name}} {
    {{~for c in x.hierarchy_not_abstract_children~}}
    {{c.flat_buffers_full_name}},
    {{~end~}}
}
{{~else~}}
table {{x.flat_buffers_full_name}} {
    {{~for f in hierarchy_export_fields ~}}
    {{f.name}}:{{flat_buffers_define_type f.ctype}}{{flat_buffers_type_metadata f.ctype}};
    {{~end~}}
}
{{~end~}}
