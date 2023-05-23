{{~
    name = x.name
    namespace = x.namespace
    tables = x.tables
~}}

package {{namespace}}

type JsonLoader func(string) ([]map[string]interface{}, error)

type {{name}} struct {
    {{~for table in tables ~}}
    {{table.name}} *{{table.go_full_name}}
    {{~end~}}
}

func NewTables(loader JsonLoader) (*{{name}}, error) {
    var err error
    var buf []map[string]interface{}

    tables := &{{name}}{}
    {{~for table in tables ~}}
    if buf, err = loader("{{table.output_data_file}}") ; err != nil {
        return nil, err
    }
    if tables.{{table.name}}, err = New{{table.go_full_name}}(buf) ; err != nil {
        return nil, err
    }
    {{~end~}}
    return tables, nil
}
