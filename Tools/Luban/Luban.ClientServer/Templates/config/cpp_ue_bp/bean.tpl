#pragma once
#include ""CoreMinimal.h""
#include ""UCfgObj.h""


{{ue_bp_includes}}

#include ""{{ue_bp_header_file_name_without_suffix}}.generated.h""

UCLASS(BlueprintType)
class X6PROTO_API {{ue_bp_full_name}} : public {{if parent_def_type}} {{parent_def_type.ue_bp_full_name}} {{else}} UCfgObj {{end}}
{
	GENERATED_BODY()

public:


    {{~for field in export_fields ~}}
	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta = (DisplayName = ""{{field.name}}""))
    {{field.ctype.ue_bp_cpp_define_type}} {{field.name}};
    {{~end~}}
};
