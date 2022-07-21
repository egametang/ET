{{ 
    name = x.rust_full_name
    key_type = x.key_ttype
    value_type =  x.value_ttype
}}
{{~if x.comment != '' ~}}
/**
 * {{x.escape_comment}}
 */
{{~end~}}
#[allow(non_camel_case_types)]
pub struct {{name}} {
    {{~if x.is_map_table ~}}
    data_list: Vec<std::rc::Rc<{{rust_define_type value_type}}>>,
    data_map: std::collections::HashMap<{{rust_define_type key_type}}, std::rc::Rc<{{rust_define_type value_type}}>>,
    {{~else if x.is_list_table ~}}
    data_list: Vec<std::rc::Rc<{{rust_define_type value_type}}>>,
    {{~else~}}
    data: {{rust_class_name value_type}},
    {{~end~}}
}

impl {{name}}{
    pub fn new(__js: &json::JsonValue) -> Result<{{name}}, LoadError> {
    {{~if x.is_map_table ~}}
        if !__js.is_array() {
            return Err(LoadError{});
        }
        let mut t = {{name}} {
            data_list : Vec::new(),
            data_map: std::collections::HashMap::new(),
        };
        
        for __e in __js.members() {
            let __v = std::rc::Rc::new(match {{rust_class_name value_type}}::new(__e) {
                Ok(x) => x,
                Err(err) => return Err(err),
            });
            let __v2 = std::rc::Rc::clone(&__v);
            t.data_list.push(__v);
{{~if !value_type.bean.is_abstract_type~}}
            t.data_map.insert(__v2.{{x.index_field.convention_name}}.clone(), __v2);
{{~else~}}
            match &*__v2 {
    {{~for child in value_type.bean.hierarchy_not_abstract_children~}}
                {{rust_class_name value_type}}::{{child.name}}(__w__) => t.data_map.insert(__w__.{{x.index_field.convention_name}}.clone(), __v2),
    {{~end~}}
            };
{{~end~}}
        }
        Ok(t)
    }
    #[allow(dead_code)]
    pub fn get_data_map(self:&{{name}}) -> &std::collections::HashMap<{{rust_define_type key_type}}, std::rc::Rc<{{rust_define_type value_type}}>> { &self.data_map }
    #[allow(dead_code)]
    pub fn get_data_list(self:&{{name}}) -> &Vec<std::rc::Rc<{{rust_define_type value_type}}>> { &self.data_list }
    #[allow(dead_code)]
    pub fn get(self:&{{name}}, key: &{{rust_define_type key_type}}) -> std::option::Option<&std::rc::Rc<{{rust_define_type value_type}}>> { self.data_map.get(key) }
    
    {{~else if x.is_list_table ~}}
        if !__js.is_array() {
            return Err(LoadError{});
        }
        let mut t = {{name}} {
            data_list : Vec::new(),
        };
        
        for __e in __js.members() {
            let __v = std::rc::Rc::new(match {{rust_class_name value_type}}::new(__e) {
                Ok(x) => x,
                Err(err) => return Err(err),
            });
            let __v2 = std::rc::Rc::clone(&__v);
            t.data_list.push(__v);
        }
        Ok(t)
    }

    #[allow(dead_code)]
    pub fn get_data_list(self:&{{name}}) -> &Vec<std::rc::Rc<{{rust_define_type value_type}}>> { &self.data_list }
    #[allow(dead_code)]
    pub fn get(self:&{{name}}, index: usize) -> &std::rc::Rc<{{rust_define_type value_type}}> { &self.data_list[index] }
    {{~else~}}
        if !__js.is_array() || __js.len() != 1 {
            return Err(LoadError{});
        }
        let __v = match {{rust_class_name value_type}}::new(&__js[0]) {
            Ok(x) => x,
            Err(err) => return Err(err),
        };
        let t = {{name}} {
            data: __v,
        };
        Ok(t)
    }
    #[allow(dead_code)]
    pub fn get_data(self:&{{name}}) -> &{{rust_define_type value_type}} { &self.data }
    {{~end~}}
}