{{~
    name = x.name
    namespace_with_top_module = x.namespace_with_top_module
    comment = x.comment
    items = x.items
~}}

{{~if comment != '' ~}}
'''
{{comment | html.escape}}
'''
{{~end~}}
class {{x.py_full_name}}(Enum):
    {{~ for item in items ~}}
{{~if item.comment != '' ~}}
    '''
    {{item.escape_comment}}
    '''
{{~end~}}
    {{item.name}} = {{item.value}}
    {{~end~}}
    {{~if (items == empty)~}}
    pass
    {{~end~}}
