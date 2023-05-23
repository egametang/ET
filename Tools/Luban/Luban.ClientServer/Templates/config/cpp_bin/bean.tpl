{{x.cpp_namespace_begin}}

{{
    name = x.name
    parent_def_type = x.parent_def_type
    export_fields = x.export_fields
    hierarchy_export_fields = x.hierarchy_export_fields
}}

{{~if x.comment != '' ~}}
/**
 * {{x.escape_comment}}
 */
{{~end~}}
class {{name}} : public {{if parent_def_type}} {{parent_def_type.cpp_full_name}} {{else}} bright::CfgBean {{end}}
{
    public:

    static bool deserialize{{name}}(ByteBuf& _buf, ::bright::SharedPtr<{{name}}>& _out);

    {{name}}()
    { 

    }

{{~if !hierarchy_export_fields.empty?~}}
    {{name}}({{- for field in hierarchy_export_fields }}{{cpp_define_type field.ctype}} {{field.name}}{{if !for.last}},{{end}} {{end}}) 
    {{~if parent_def_type~}}
            : {{parent_def_type.cpp_full_name}}({{ for field in parent_def_type.hierarchy_export_fields }}{{field.name}}{{if !for.last}}, {{end}}{{end}})
    {{~end~}}
    {

        {{~ for field in export_fields ~}}
        this->{{field.convention_name}} = {{field.name}};
        {{~end~}}
    }
{{~end~}}
    virtual ~{{name}}() {}

    bool deserialize(ByteBuf& _buf);

    {{~ for field in export_fields ~}}
{{~if field.comment != '' ~}}
    /**
     * {{field.escape_comment}}
     */
{{~end~}}
    {{cpp_define_type field.ctype}} {{field.convention_name}};
    {{~if field.index_field~}} 
    ::bright::HashMap<{{cpp_define_type field.index_field.ctype}}, {{cpp_define_type field.ctype.element_type}}> {{field.convention_name}}_Index;
    {{~end~}}
    {{~if field.gen_ref~}}
    {{field.cpp_ref_validator_define}}
    {{~end~}}
    {{~end~}}

{{~if !x.is_abstract_type~}}
    static constexpr int __ID__ = {{x.id}};

    int getTypeId() const { return __ID__; }
{{~end~}}

    virtual void resolve(::bright::HashMap<::bright::String, void*>& _tables);
};

{{x.cpp_namespace_end}}
