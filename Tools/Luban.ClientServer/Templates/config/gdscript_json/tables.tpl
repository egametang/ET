{{
    name = x.name
    namespace = x.namespace
    tables = x.tables
}}
{{~ for table in tables ~}}
var {{table.name}}: {{table.gdscript_full_name}}
{{~end~}}

func _init(loader) -> void:
    {{~for table in tables ~}}
    self.{{table.name}} = {{table.gdscript_full_name}}.new(loader.call('{{table.output_data_file}}'))
    {{~end~}}