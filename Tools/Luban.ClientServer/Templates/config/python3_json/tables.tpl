{{
    name = x.name
    namespace = x.namespace
    tables = x.tables
}}

class {{name}}:
    {{~ for table in tables ~}}
    #def {{table.name}} : return self._{{table.name}}
    {{~end~}}

    def __init__(self, loader):
        {{~for table in tables ~}}
        self.{{table.name}} = {{table.py_full_name}}(loader('{{table.output_data_file}}')); 
        {{~end~}}
