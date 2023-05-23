#pragma once
#include ""CoreMinimal.h""
#include ""FCfgObj.h""

{{editor_cpp_includes}}

namespace editor
{

{{cpp_namespace_begin}}

struct X6PROTOEDITOR_API {{ue_fname}} : public {{if parent_def_type}} {{parent_def_type.ue_fname}}{{else}}FCfgObj{{end}}
{
    {{~for field in fields ~}}
    {{field.ctype.editor_ue_cpp_define_type}} {{field.name}};
    {{~end~}}

{{~if !is_abstract_type~}}
    bool Load(FJsonObject* _json) override;
    bool Save(FJsonObject*& result) override;
{{~end~}}

    static bool Create(FJsonObject* _json, {{ue_fname}}*& result);
};


{{cpp_namespace_end}}