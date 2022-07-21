{{
    name = x.name
    full_name = x.full_name
    parent_def_type = x.parent_def_type
    fields = x.fields
    hierarchy_fields = x.hierarchy_fields
    is_abstract_type = x.is_abstract_type
    readonly_name = 'IReadOnly' + name
}}

{{x.typescript_namespace_begin}}
{{~if x.comment != '' ~}}
/**
 * {{x.escape_comment}}
 */
{{~end~}}
export {{x.ts_class_modifier}} class {{name}} extends {{if parent_def_type}} {{x.parent}} {{else}} TxnBeanBase {{end}}{
    {{~ for field in fields~}}
{{~if field.comment != '' ~}}
    /**
     * {{field.escape_comment}}
     */
{{~end~}}
    {{if is_abstract_type}}protected{{else}}private{{end}} {{field.internal_name}}: {{db_ts_define_type field.ctype}} 
    {{~end}}

    constructor() {
        super()
        {{~ for field in fields~}}
        {{db_ts_init_field field.internal_name_with_this field.log_type field.ctype }}
        {{~end~}}
    }

    {{~ for field in fields~}}
        {{~ctype = field.ctype~}}
        {{~if has_setter ctype~}}
    private static {{field.log_type}} = class extends FieldLoggerGeneric2<{{name}}, {{db_ts_define_type ctype}}> {
        constructor(self:{{name}}, value: {{db_ts_define_type ctype}}) { super(self, value) }

        get fieldId(): number { return this.host.getObjectId() + {{field.id}} }

        get tagId(): number { return FieldTag.{{tag_name ctype}} }

        commit() { this.host.{{field.internal_name}} = this.value }

        writeBlob(_buf: ByteBuf) {
            {{ts_write_blob '_buf' 'this.value' ctype}}
        }
    }

{{~if field.comment != '' ~}}
    /**
     * {{field.escape_comment}}
     */
{{~end~}}
    get {{field.convention_name}}(): {{db_ts_define_type ctype}} {
        if (this.isManaged) {
            var txn = TransactionContext.current
            if (txn == null) return {{field.internal_name_with_this}}
            let log: any = txn.getField(this.getObjectId() + {{field.id}})
            return log != null ? log.value : {{field.internal_name_with_this}}
        } else {
            return {{field.internal_name_with_this}};
        }
    }

{{~if field.comment != '' ~}}
    /**
     * {{field.escape_comment}}
     */
{{~end~}}
    set {{field.convention_name}}(value: {{db_ts_define_type ctype}}) {
        {{~if db_field_cannot_null~}}
        if (value == null) throw new Error()
        {{~end~}}
        if (this.isManaged) {
            let txn = TransactionContext.current!
            txn.putFieldLong(this.getObjectId() + {{field.id}}, new {{name}}.{{field.log_type}}(this, value))
            {{~if ctype.need_set_children_root~}}
            value?.initRoot(this.getRoot())
            {{~end~}}
        } else {
            {{field.internal_name_with_this}} = value
        } 
    }

        {{~else~}}
{{~if field.comment != '' ~}}
    /**
     * {{field.escape_comment}}
     */
{{~end~}}
    get {{field.convention_name}}(): {{db_ts_define_type ctype}}  { return {{field.internal_name_with_this}} }
        {{~end~}}
    {{~end~}}

    {{~if is_abstract_type~}}
    static serialize{{name}}Any(_buf: ByteBuf, x: {{name}}) {
        if (x == null) { _buf.WriteInt(0); return }
        _buf.WriteInt(x.getTypeId())
        x.serialize(_buf)
    }

    deserialize{{name}}Any(_buf: ByteBuf): {{name}}{
        let x: {{name}}
        switch (_buf.ReadInt()) {
        {{~ for child in x.hierarchy_not_abstract_children~}}
            case {{child.full_name}}.__ID__: x = new {{child.full_name}}(); break
        {{~end~}}
            default: throw new Error()
        }
        x.deserialize(_buf)
        return x
    }
    {{~else~}}
    serialize(_buf: ByteBuf) {
        _buf.WriteNumberAsLong(this.getObjectId())
        {{~ for field in hierarchy_fields~}}
        { _buf.WriteInt(FieldTag.{{tag_name field.ctype}} | ({{field.id}} << FieldTag.TAG_SHIFT)); {{db_ts_compatible_serialize '_buf' field.internal_name_with_this field.ctype}} }
        {{~end}}
    }

    deserialize(_buf: ByteBuf) {
        this.setObjectId(_buf.ReadLongAsNumber())
        while(_buf.NotEmpty) {
            let _tag_ = _buf.ReadInt()
            switch (_tag_) {
            {{~ for field in hierarchy_fields~}}
            case FieldTag.{{tag_name field.ctype}} | ({{field.id}} << FieldTag.TAG_SHIFT) : { {{db_ts_compatible_deserialize '_buf' field.internal_name_with_this field.ctype}};  break; }
            {{~end~}}
            default: { _buf.SkipUnknownField(_tag_); break; }
            }
        }
    }

    static readonly __ID__ = {{x.id}}
    getTypeId(): number { return {{name}}.__ID__ }
    {{~end~}}

    initChildrenRoot(root: TKey) {
        {{~ for field in hierarchy_fields~}}
        {{~if need_set_children_root field.ctype~}}
        {{field.internal_name_with_this}}?.initRoot(root)
        {{~end~}}
        {{~end}}
    }

    toString(): string {
        return '{{full_name}}{ '
    {{~ for field in hierarchy_fields~}}
        + '{{field.convention_name}}:' + {{ts_to_string ('this.' + field.convention_name) field.ctype}} + ','
    {{~end~}}
        + '}'
    }
}

{{x.typescript_namespace_end}}