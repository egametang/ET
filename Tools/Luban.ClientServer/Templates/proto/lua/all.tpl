{{
    enums = x.enums
    beans = x.beans
    protos = x.protos
}}
local setmetatable = setmetatable
local pairs = pairs
local ipairs = ipairs
local tinsert = table.insert

local function SimpleClass()
    local class = {}
    class.__index = class
    class.New = function(...)
        local ctor = class.ctor
        local o = ctor and ctor(...) or {}
        setmetatable(o, class)
        return o
    end
    return class
end

local function get_map_size(m)
    local n = 0
    for _ in pairs(m) do
        n = n + 1
    end
    return n
end

local enums =
{
    {{~ for c in enums ~}}
    ---@class {{c.full_name}}
    {{~ for item in c.items ~}}
     ---@field public {{item.name}} int
    {{~end~}}
    ['{{c.full_name}}'] = {  {{ for item in c.items }} {{item.name}}={{item.int_value}}, {{end}} };
    {{~end~}}
}


local function InitTypes(methods)
    local readBool = methods.readBool
    local writeBool = methods.writeBool
    local readByte = methods.readByte
    local writeByte = methods.writeByte
    local readShort = methods.readShort
    local writeShort = methods.writeShort
    local readFshort = methods.readFshort
    local writeInt = methods.writeInt
    local readInt = methods.readInt
    local writeFint = methods.writeFint
    local readFint = methods.readFint
    local readLong = methods.readLong
    local writeLong = methods.writeLong
    local readFlong = methods.readFlong
    local writeFlong = methods.writeFlong
    local readFloat = methods.readFloat
    local writeFloat = methods.writeFloat
    local readDouble = methods.readDouble
    local writeDouble = methods.writeDouble
    local readSize = methods.readSize
    local writeSize = methods.writeSize

    local readString = methods.readString
    local writeString = methods.writeString
    local readBytes = methods.readBytes
    local writeBytes = methods.writeBytes

    local function readVector2(bs)
        return { x = readFloat(bs), y = readFloat(bs) }
    end
    
    local function writeVector2(bs, v)
        writeFloat(bs, v.x)
        writeFloat(bs, v.y)
    end

    local function readVector3(bs)
        return { x = readFloat(bs), y = readFloat(bs), z = readFloat(bs) }
    end
    
    local function writeVector3(bs, v)
        writeFloat(bs, v.x)
        writeFloat(bs, v.y)
        writeFloat(bs, v.z)
    end


    local function readVector4(bs)
        return { x = readFloat(bs), y = readFloat(bs), z = readFloat(bs), w = readFloat(bs) }
    end
    
    local function writeVector4(bs, v)
        writeFloat(bs, v.x)
        writeFloat(bs, v.y)
        writeFloat(bs, v.z)
        writeFloat(bs, v.w)
    end

    local function writeList(bs, list, keyFun)
        writeSize(bs, #list)
        for _, v in pairs(list) do 
            keyFun(bs, v)    
        end
    end

    local function readList(bs, keyFun)
        local list = {}
        local v
        for i = 1, readSize(bs) do
            tinsert(list, keyFun(bs))
        end
        return list
    end

    local writeArray = writeList
    local readArray = readList

    local function writeSet(bs, set, keyFun)
        writeSize(bs, #set)
        for _, v in ipairs(set) do
            keyFun(bs, v)
        end
    end

    local function readSet(bs, keyFun)
        local set = {}
        local v
        for i = 1, readSize(bs) do
            tinsert(set, keyFun(bs))
        end
        return set
    end

    local function writeMap(bs, map, keyFun, valueFun)
        writeSize(bs, get_map_size(map))
        for k, v in pairs(map) do
            keyFun(bs, k)
            valueFun(bs, v)
        end
    end

    local function readMap(bs, keyFun, valueFun)
        local map = {}
        for i = 1, readSize(bs) do
            local k = keyFun(bs)
            local v = valueFun(bs)
            map[k] = v
        end
        return map
    end

    local function readNullableBool(bs)
        if readBool(bs) then
            return readBool(bs)
        end
    end

    local default_vector2 = {x=0,y=0}
    local default_vector3 = {x=0,y=0,z=0}
    local default_vector4 = {x=0,y=0,z=0,w=0}

    local beans = {}
{{ for bean in beans }}
    do
    ---@class {{bean.full_name}} {{if bean.parent_def_type}}:{{bean.parent}} {{end}}
    {{~ for field in bean.fields~}}
     ---@field public {{field.name}} {{lua_comment_type field.ctype}}
    {{~end}}
        local class = SimpleClass()
        class._id = {{bean.id}}
        class._name = '{{bean.full_name}}'
        local id2name = { {{for c in bean.hierarchy_not_abstract_children}} [{{c.id}}] = '{{c.full_name}}', {{end}} }
{{if bean.is_abstract_type}}
        class._serialize = function(bs, self)
            writeInt(bs, {{bean.id}})
            beans[self._name]._serialize(bs, self)
        end
        class._deserialize = function(bs)
            local id = readInt(bs)
            return beans[id2name[id]]._deserialize(bs)
        end
{{else}}
        class._serialize = function(bs, self)
        {{~ for field in bean.hierarchy_fields ~}}
            {{lua_serialize_while_nil 'bs' ('self.' + field.name) field.ctype}}
        {{~end~}}
        end
        class._deserialize = function(bs)
            local o = {
        {{~ for field in bean.hierarchy_fields ~}}
            {{~if !(need_marshal_bool_prefix field.ctype)~}}
                {{field.name}} = {{lua_undering_deserialize 'bs' field.ctype}},
            {{~else~}}
            {{field.name}} = {{if !field.ctype.is_bool}}readBool(bs) and {{lua_undering_deserialize 'bs' field.ctype}} or nil {{else}} readNullableBool(bs) {{end}},
            {{~end~}}
        {{~end~}}
            }
            setmetatable(o, class)
            return o
        end
{{end}}
        beans[class._name] = class
    end
{{end}}

    local protos = { }
{{ for proto in protos }}
    do
    ---@class {{proto.full_name}}
    {{~ for field in proto.fields~}}
     ---@field public {{field.name}} {{lua_comment_type field.ctype}}
    {{~end}}
        local class = SimpleClass()
        class._id = {{proto.id}}
        class._name = '{{proto.full_name}}'
        class._serialize = function(bs, self)
        {{~ for field in proto.fields ~}}
            {{lua_serialize_while_nil 'bs' ('self.' + field.name) field.ctype}}
        {{~end~}}
        end
        class._deserialize = function(bs)
            local o = {
        {{~ for field in proto.fields ~}}
            {{~if !(need_marshal_bool_prefix field.ctype)~}}
                {{field.name}} = {{lua_undering_deserialize 'bs' field.ctype}},
            {{~else~}}
            {{field.name}} = {{if !field.ctype.is_bool}}readBool(bs) and {{lua_undering_deserialize 'bs' field.ctype}} or nil {{else}} readNullableBool(bs) {{end}},
            {{~end~}}
        {{~end~}}
            }
            setmetatable(o, class)
            return o
        end
        protos[class._id] = class
        protos[class._name] = class
    end
{{end}}

    return { enums = enums, beans = beans, protos = protos }
    end

return { InitTypes = InitTypes}
