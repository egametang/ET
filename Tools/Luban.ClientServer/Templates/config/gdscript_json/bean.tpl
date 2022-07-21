{{
    name = x.gdscript_full_name
    is_abstract_type = x.is_abstract_type
    parent_def_type = x.parent_def_type
    export_fields = x.export_fields
    hierarchy_export_fields = x.hierarchy_export_fields
}}
class {{name}}:
    {{if parent_def_type}}
    extends {{parent_def_type.gdscript_full_name}}
    {{end}}
{{~if x.is_abstract_type~}}
    static func from_json(_json_):
        var type = _json_['{{x.json_type_name_key}}']
        match type:
        {{~ for child in x.hierarchy_not_abstract_children~}}
            "{{cs_impl_data_type child x}}":
                return {{child.gdscript_full_name}}.new(_json_)
        {{~end~}}
            _:
                assert(false)

{{~end~}}
{{~ for field in export_fields ~}}
    var {{field.convention_name}}
{{~end~}}
{{~if parent_def_type~}}
    func _init(_json_).(_json_) -> void:
{{~else~}}
    func _init(_json_) -> void:
{{~end~}}
        {{~if export_fields~}}
        {{~ for field in export_fields ~}}
        {{gdscript_deserialize_field ('self.' + field.convention_name) '_json_' field.name field.ctype}}
        {{~end~}}
        {{~end~}}
        {{~if export_fields.empty?}}
        pass
        {{~end~}}