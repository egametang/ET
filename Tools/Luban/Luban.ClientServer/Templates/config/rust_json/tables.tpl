{{
    name = x.name
    namespace = x.namespace
    tables = x.tables
}}

type JsonLoader = fn(&str) -> Result<json::JsonValue, LoadError>;

#[allow(non_camel_case_types)]
pub struct {{name}} {
    {{~ for table in tables ~}}

{{~if table.comment != '' ~}}
    /**
     * {{table.escape_comment}}
     */
{{~end~}}
    pub {{string.downcase table.name}}: {{table.rust_full_name}},
    {{~end~}}
}

impl {{name}} {
    #[allow(dead_code)]
    pub fn new(loader: JsonLoader) -> std::result::Result<Tables, LoadError> {
        let tables = Tables {
        {{~for table in tables ~}}
            {{string.downcase table.name}}: {{table.rust_full_name}}::new(&loader("{{table.output_data_file}}")?)?,
        {{~end~}}
        };
        return Ok(tables);
    }
}
