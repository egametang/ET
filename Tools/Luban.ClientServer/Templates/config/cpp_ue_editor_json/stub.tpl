#include ""JsonUtil.h""

{{includes}}

namespace editor
{

{{~for type in types~}}
{{type.cpp_namespace_begin}}
{{~if type.is_bean~}}
{{~if type.is_abstract_type~}}
    bool {{type.ue_fname}}::Create(FJsonObject* _json, {{type.ue_fname}}*& result)
    {
        FString type;
        if (_json->TryGetStringField(FString(""{{type.json_type_name_key}}""), type))
        {
            {{~for child in type.hierarchy_not_abstract_children~}}
            if (type == ""{{cs_impl_data_type child x}}"")
            {
                result = new {{child.ue_fname}}();
            } else
            {{~end~}}
            {
                result = nullptr;
                return false;
            }
            if (!result->Load(_json))
            {
                delete result;
                return false;
            }
            return true;
        }
        else
        {
            result = nullptr;
            return false;
        }
    }
{{~else~}}
    bool {{type.ue_fname}}::Create(FJsonObject* _json, {{type.ue_fname}}*& result)
    {
        result = new {{type.ue_fname}}();
        if (!result->Load(_json))
        {
            delete result;
            return false;
        }
        return true;
    }


        bool {{type.ue_fname}}::Save(FJsonObject*& result)
        {
            auto _json = new FJsonObject();
            _json->SetStringField(""{{type.json_type_name_key}}"", ""{{type.name}}"");

{{~for field in type.hierarchy_fields~}}
            {{field.editor_ue_cpp_save}}
{{~end~}}
            result = _json;
            return true;
        }

        bool {{type.ue_fname}}::Load(FJsonObject* _json)
        {
{{~for field in type.hierarchy_fields~}}
            {{field.editor_ue_cpp_load}}
{{~end~}}
            return true;
        }
{{~end~}}
{{~else~}}

bool {{type.ue_fname}}ToString({{type.ue_fname}} value, FString& s)
{
    {{~for item in type.items ~}}
    if (value == {{type.ue_fname}}::{{item.name}}) { s = ""{{item.name}}""; return true; }
    {{~end~}}
    return false;
}
bool {{type.ue_fname}}FromString(const FString& s, {{type.ue_fname}}& value)
{
    {{~for item in type.items ~}}
        if (s == ""{{item.name}}"")
        {
            value = {{type.ue_fname}}::{{item.name}};
            return true;
        }
    {{~end~}}
    return false;
}

{{~end~}}
{{type.cpp_namespace_end}}
{{~end~}}
}