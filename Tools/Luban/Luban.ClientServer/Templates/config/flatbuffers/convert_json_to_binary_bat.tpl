{{~
    name = x.name
    namespace = x.namespace
    tables = x.tables
~}}

echo off

set FLATC=%1
set SCHEMA_FILE=%2
set DATA_DIR=%3
set OUTPUT_DIR=%4

{{~for table in tables~}}
%FLATC% -o %OUTPUT_DIR% -b %SCHEMA_FILE% --root-type {{if namespace != ''}}{{namespace}}.{{end}}{{table.flat_buffers_full_name}} %DATA_DIR%\{{table.output_data_file}}.json
{{~end~}}