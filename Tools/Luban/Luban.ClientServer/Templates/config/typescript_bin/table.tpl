{{ 
	name = x.name
	key_type = x.key_ttype
	key_type1 =  x.key_ttype1
	key_type2 =  x.key_ttype2
	value_type =  x.value_ttype
}}
{{x.typescript_namespace_begin}}
{{~if x.comment != '' ~}}
/**
 * {{x.escape_comment}}
 */
{{~end~}}
export class {{name}} {
    {{~if x.is_map_table ~}}
    private _dataMap: Map<{{ts_define_type key_type}}, {{ts_define_type value_type}}>
    private _dataList: {{ts_define_type value_type}}[]
    
    constructor(_buf_: ByteBuf) {
        this._dataMap = new Map<{{ts_define_type key_type}}, {{ts_define_type value_type}}>()
        this._dataList = []
        
        for(let n = _buf_.ReadInt() ; n > 0 ; n--) {
            let _v: {{ts_define_type value_type}}
            {{ts_bin_constructor '_v' '_buf_' value_type}}
            this._dataList.push(_v)
            this._dataMap.set(_v.{{x.index_field.convention_name}}, _v)
        }
    }

    getDataMap(): Map<{{ts_define_type key_type}}, {{ts_define_type value_type}}> { return this._dataMap }
    getDataList(): {{ts_define_type value_type}}[] { return this._dataList }

    get(key: {{ts_define_type key_type}}): {{ts_define_type value_type}} | undefined { return this._dataMap.get(key) }

    resolve(_tables: Map<string, any>) {
        for(var v of this._dataList) {
            v.resolve(_tables)
        }
    }
    {{~else if x.is_list_table ~}}
    private _dataList: {{ts_define_type value_type}}[]
    
    constructor(_buf_: ByteBuf) {
        this._dataList = []
        
        for(let n = _buf_.ReadInt() ; n > 0 ; n--) {
            let _v: {{ts_define_type value_type}}
            {{ts_bin_constructor '_v' '_buf_' value_type}}
            this._dataList.push(_v)
        }
    }

    getDataList(): {{ts_define_type value_type}}[] { return this._dataList }

    get(index: number): {{ts_define_type value_type}} | undefined { return this._dataList[index] }

    resolve(_tables: Map<string, any>) {
        for(var v of this._dataList) {
            v.resolve(_tables)
        }
    }

    {{~else~}}

     private _data: {{ts_define_type value_type}}

    constructor(_buf_: ByteBuf) {
        if (_buf_.ReadInt() != 1) throw new Error('table mode=one, but size != 1')
        {{ts_bin_constructor 'this._data' '_buf_' value_type}}
    }

    getData(): {{ts_define_type value_type}} { return this._data }

    {{~ for field in value_type.bean.hierarchy_export_fields ~}}
{{~if field.comment != '' ~}}
    /**
     * {{field.escape_comment}}
     */
{{~end~}}
     get {{field.convention_name}}(): {{ts_define_type field.ctype}} { return this._data.{{field.convention_name}} }
    {{~end~}}

    resolve(_tables: Map<string, any>) {
        this._data.resolve(_tables)
    }

    {{end}}
}
{{x.typescript_namespace_end}}
