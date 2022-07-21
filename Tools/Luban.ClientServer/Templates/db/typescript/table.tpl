{{
    name = x.name
    key_ttype = x.key_ttype
    value_ttype = x.value_ttype
    internal_table_type = x.internal_table_type
}}

{{x.typescript_namespace_begin}}
 class {{internal_table_type}} extends TxnTableGeneric<{{db_ts_define_type key_ttype}},{{db_ts_define_type value_ttype}}> {
    constructor() {
        super({{x.table_uid}}, '{{x.full_name}}')
    }

    newValue(): {{db_ts_define_type value_ttype}} { return new {{db_ts_define_type value_ttype}}() }

    serializeKey(buf: ByteBuf, key: {{db_ts_define_type key_ttype}}) {
        {{db_ts_compatible_serialize_without_segment 'buf' 'key' key_ttype}}
    }

    serializeValue(buf: ByteBuf, value: {{db_ts_define_type value_ttype}}) {
        {{db_ts_compatible_serialize_without_segment 'buf' 'value' value_ttype}}
    }

    deserializeKey(buf: ByteBuf): {{db_ts_define_type key_ttype}} {
        let key: {{db_ts_define_type key_ttype}}
        {{db_ts_compatible_deserialize_without_segment 'buf' 'key' key_ttype}}
        return key
    }

    deserializeValue(buf: ByteBuf): {{db_ts_define_type value_ttype}} {
        let value = new {{db_ts_define_type value_ttype}}()
        {{db_ts_compatible_deserialize_without_segment 'buf' 'value' value_ttype}}
        return value
    }
}

{{~if x.comment != '' ~}}
/**
 * {{x.escape_comment}}
 */
{{~end~}}
export class {{name}} {
    static readonly _table = new {{internal_table_type}}();
    static get table(): TxnTableGeneric<{{db_ts_define_type key_ttype}},{{db_ts_define_type value_ttype}}> { return this._table }

    static getAsync(key: {{db_ts_define_type key_ttype}}): Promise<{{db_ts_define_type value_ttype}}> {
        return {{name}}._table.getAsync(key);
    }

    static createIfNotExistAsync(key: {{db_ts_define_type key_ttype}}): Promise<{{db_ts_define_type value_ttype}}> {
        return {{name}}._table.createIfNotExistAsync(key);
    }

    static insertAsync(key: {{db_ts_define_type key_ttype}}, value: {{db_ts_define_type value_ttype}}): Promise<void> {
        return {{name}}._table.insertAsync(key, value);
    }

    static removeAsync(key: {{db_ts_define_type key_ttype}}): Promise<void> {
        return {{name}}._table.removeAsync(key);
    }

    static put(key: {{db_ts_define_type key_ttype}}, value: {{db_ts_define_type value_ttype}}): Promise<void> {
        return {{name}}._table.putAsync(key, value);
    }

    static selectAsync(key: {{db_ts_define_type key_ttype}}): Promise<{{db_ts_define_type value_ttype}}> {
        return {{name}}._table.selectAsync(key);
    }
}

{{x.typescript_namespace_end}}
