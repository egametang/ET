#pragma once
#include ""CoreMinimal.h""

#include ""{{ue_bp_header_file_name_without_suffix}}.generated.h""

UENUM(BlueprintType)
enum class {{ue_bp_full_name}} : uint8
{
    {{~if !contains_value_equal0_item~}}
    __DEFAULT__ = 0,
    {{~end~}}
    {{~if contains_any_ue_enum_compatible_item~}}
    {{~for item in items ~}}
    {{if item.int_value >= 256}}//{{end}}{{item.name}} = {{item.value}}     UMETA(DisplayName = ""{{item.alias_or_name}}""),
    {{~end~}}
    {{~else~}}
    DUMMY UMETA(DisplayName = ""DUMMY""),
    {{~end~}}
};
