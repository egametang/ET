using Bright.Serialization;
using System.Collections.Generic;
using SimpleJSON;

{{
    name = x.name
    parent_def_type = x.parent_def_type
    parent = x.parent
    hierarchy_fields = x.hierarchy_fields
    fields = x.fields
}}

{{cs_start_name_space_grace x.namespace_with_editor_top_module}}

{{~if x.comment != '' ~}}
/// <summary>
/// {{x.escape_comment}}
/// </summary>
{{~end~}}
public {{x.cs_class_modifier}} partial class {{name}} : {{if parent_def_type}} {{parent}} {{else}} Bright.Config.EditorBeanBase {{end}}
{
    public {{name}}()
    {
        {{~ for field in fields ~}}
        {{~if (cs_editor_need_init field.ctype) && !field.ctype.is_nullable ~}}
            {{field.convention_name}} = {{cs_editor_init_value field.ctype}};
        {{~end~}}
        {{~end~}}
    }

    {{~if !x.is_abstract_type~}}
    public override void LoadJson(SimpleJSON.JSONObject _json)
    {
        {{~ for field in hierarchy_fields ~}}
        { 
            var _fieldJson = _json["{{field.name}}"];
            if (_fieldJson != null)
            {
                {{cs_unity_editor_json_load '_fieldJson' field.convention_name field.ctype}}
            }
        }
        
        {{~end~}}
    }

    public override void SaveJson(SimpleJSON.JSONObject _json)
    {
        {{~if parent~}}
        _json["{{x.json_type_name_key}}"] = "{{x.full_name}}";
        {{~end~}}
        {{~ for field in hierarchy_fields ~}}
            {{~if field.ctype.is_nullable}}
        if ({{field.convention_name}} != null)
        {
            {{cs_unity_editor_json_save '_json' field.name field.convention_name field.ctype}}
        }
            {{~else~}}
        {
                {{~if (cs_is_editor_raw_nullable field.ctype)}}
            if ({{field.convention_name}} == null) { throw new System.ArgumentNullException(); }
                {{~end~}}
            {{cs_unity_editor_json_save '_json' field.name field.convention_name field.ctype}}
        }
            {{~end~}}
        {{~end~}}
    }
    {{~end~}}

    public static {{name}} LoadJson{{name}}(SimpleJSON.JSONNode _json)
    {
    {{~if x.is_abstract_type~}}
        string type = _json["{{x.json_type_name_key}}"];
        {{name}} obj;
        switch (type)
        {
        {{~for child in x.hierarchy_not_abstract_children~}}
            {{~if child.namespace == x.namespace && x.namespace != '' ~}}
            case "{{child.full_name}}":   
            {{~end~}}
            case "{{cs_impl_data_type child x}}":obj = new {{child.full_name}}(); break;
        {{~end~}}
            default: throw new SerializationException();
        }
    {{~else~}}
        {{name}} obj = new {{x.full_name}}();
    {{~end~}}
        obj.LoadJson((SimpleJSON.JSONObject)_json);
        return obj;
    }
        
    public static void SaveJson{{name}}({{name}} _obj, SimpleJSON.JSONNode _json)
    {
    {{~if x.is_abstract_type~}}
        _json["{{x.json_type_name_key}}"] = _obj.GetType().Name;
    {{~end~}}
        _obj.SaveJson((SimpleJSON.JSONObject)_json);
    }

    {{~ for field in fields ~}}
{{~if field.comment != '' ~}}
    /// <summary>
    /// {{field.escape_comment}}
    /// </summary>
{{~end~}}
    public {{cs_editor_define_type field.ctype}} {{field.convention_name}} { get; set; }

    {{~end~}}
}

{{cs_end_name_space_grace x.namespace_with_editor_top_module}}