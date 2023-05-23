
{{
    name = x.rust_full_name
    parent_def_type = x.parent_def_type
    export_fields = x.export_fields
    hierarchy_export_fields = x.hierarchy_export_fields
}}

{{~if x.comment != '' ~}}
/**
 * {{x.escape_comment}}
 */
{{~end~}}
{{~if !x.is_abstract_type~}}
#[allow(non_camel_case_types)]
pub struct {{name}} {
{{~for field in hierarchy_export_fields~}}
pub {{field.convention_name}}: {{rust_define_type field.ctype}},
{{~end~}}
}

impl {{name}} {
    #[allow(dead_code)]
    pub fn new(__js: &json::JsonValue) -> Result<{{name}}, LoadError> {
        let __b = {{name}} {
{{~for field in hierarchy_export_fields~}}
            {{field.convention_name}}: {{rust_json_constructor ('__js["' + field.name + '"]') field.ctype}},
{{~end~}}
        };
        Ok(__b)
    }
}
{{~else~}}
#[allow(non_camel_case_types)]
pub enum {{name}} {
{{~for child in x.hierarchy_not_abstract_children~}}
  {{child.name}}(Box<{{child.rust_full_name}}>),
{{~end~}}
}

impl {{name}} {
    #[allow(dead_code)]
    pub fn new(__js: &json::JsonValue) -> Result<{{name}}, LoadError> {
        let __b = match __js["{{x.json_type_name_key}}"].as_str() {
            Some(type_name) => match type_name {
{{~for child in x.hierarchy_not_abstract_children~}}
                "{{cs_impl_data_type child x}}" => {{name}}::{{child.name}}(Box::new({{child.rust_full_name + '::new(&__js)?'}})),
{{~end~}}
                _ => return Err(LoadError{})
                },
            None => return Err(LoadError{})
        };
        Ok(__b)
    }
}
{{~end~}}
