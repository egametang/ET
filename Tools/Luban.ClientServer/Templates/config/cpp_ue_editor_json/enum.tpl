#pragma once
#include ""CoreMinimal.h""

namespace editor
{

{{cpp_namespace_begin}}

    enum class {{ue_fname}}
    {
        {{~for item in items ~}}
        {{item.name}} = {{item.value}},
        {{~end~}}
    };

    bool X6PROTOEDITOR_API {{ue_fname}}ToString({{ue_fname}} value, FString& s);
    bool X6PROTOEDITOR_API {{ue_fname}}FromString(const FString& s, {{ue_fname}}& x);

{{cpp_namespace_end}}

}