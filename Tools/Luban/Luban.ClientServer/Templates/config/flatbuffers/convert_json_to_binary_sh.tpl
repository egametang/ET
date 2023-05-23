{{~
    name = x.name
    namespace = x.namespace
    tables = x.tables
~}}

@echo off

FLATC=$1
SCHEMA_FILE=$2
DATA_DIR=$3
OUTPUT_DIR=$4

{{~for table in tables~}}
$FLATC -o $OUTPUT_DIR -b $SCHEMA_FILE --root-type {{if namespace != ''}}{{namespace}}.{{end}}{{table.flat_buffers_full_name}} $DATA_DIR/{{table.output_data_file}}.json
{{~end~}}