
{{
    name = x.py_full_name
    is_abstract_type = x.is_abstract_type
    parent_def_type = x.parent_def_type
    export_fields = x.export_fields
    hierarchy_export_fields = x.hierarchy_export_fields
}}

class {{name}} {{if parent_def_type}}({{parent_def_type.py_full_name}}){{end}}:
{{~if x.is_abstract_type~}}
    _childrenTypes = None

    @staticmethod
    def fromJson(_json_):
        childrenTypes = {{name}}._childrenTypes
        if not childrenTypes:
            childrenTypes = {{name}}._childrenTypes = {
        {{~ for child in x.hierarchy_not_abstract_children~}}
            '{{cs_impl_data_type child x}}': {{child.py_full_name}},
        {{~end~}}
    }
        type = _json_['{{x.json_type_name_key}}']
        child = {{name}}._childrenTypes.get(type)
        if child != None:
            return  child(_json_)
        else:
            raise Exception()
{{~end~}}

    def __init__(self, _json_):
        {{~if parent_def_type~}}
        {{parent_def_type.py_full_name}}.__init__(self, _json_)
        {{~end~}}
        {{~ for field in export_fields ~}}
        {{py3_deserialize_field ('self.' + field.convention_name) '_json_' field.name field.ctype}}
        {{~end~}}
        {{~if export_fields.empty?}}
        pass
        {{~end~}}