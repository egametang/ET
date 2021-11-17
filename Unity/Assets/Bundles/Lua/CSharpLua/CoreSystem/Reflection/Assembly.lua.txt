--[[
Copyright 2017 YANG Huan (sy.yanghuan@gmail.com).

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
--]]

local System = System
local define = System.define
local throw = System.throw
local div = System.div
local Type = System.Type
local typeof = System.typeof
local getClass = System.getClass
local is = System.is
local band = System.band
local arrayFromTable = System.arrayFromTable
local toLuaTable = System.toLuaTable

local Exception = System.Exception
local NotSupportedException = System.NotSupportedException
local ArgumentException = System.ArgumentException
local ArgumentNullException = System.ArgumentNullException

local assert = assert
local pairs = pairs
local getmetatable = getmetatable
local setmetatable = setmetatable
local rawget = rawget
local type = type
local unpack = table.unpack
local select = select
local floor = math.floor

local TargetException = define("System.Reflection.TargetException", {
  __tostring = Exception.ToString,
  base = { Exception }
})

local TargetParameterCountException = define("System.Reflection.TargetParameterCountException", {
  __tostring = Exception.ToString,
  base = { Exception },
  __ctor__ = function(this, message, innerException) 
    Exception.__ctor__(this, message or "Parameter count mismatch.", innerException)
  end,
})

local AmbiguousMatchException = define("System.Reflection.AmbiguousMatchException", {
  __tostring = Exception.ToString,
  base = { System.SystemException },
  __ctor__ = function(this, message, innerException) 
    Exception.__ctor__(this, message or "Ambiguous match found.", innerException)
  end,
})

local MissingMethodException = define("System.MissingMethodException", {
  __tostring = Exception.ToString,
  base = { Exception },
  __ctor__ = function(this, message, innerException) 
    Exception.__ctor__(this, message or "Specified method could not be found.", innerException)
  end
})

local function throwNoMatadata(sign)
  throw(NotSupportedException("not found metadata for " .. sign), 1)
end

local function eq(left, right)
  return left[1] == right[1] and left.name == right.name
end

local function getName(this)
  return this.name
end

local function isAccessibility(memberInfo, kind)
  local metadata = memberInfo.metadata
  if not metadata then
    throwNoMatadata(memberInfo.c.__name__ .. "." .. memberInfo.name)
  end
  return band(metadata[2], 0x7) == kind
end

local MemberInfo = define("System.Reflection.MemberInfo", {
  getName = getName,
  EqualsObj = function (this, obj)
    if getmetatable(this) ~= getmetatable(obj) then
      return false
    end
    return eq(this, obj)
  end,
  getMemberType = function (this)
    return this.memberType
  end,
  getDeclaringType = function (this)
    return typeof(this.c)
  end,
  getIsStatic = function (this)
    local metadata = this.metadata
    if not metadata then
      throwNoMatadata(this.c.__name__ .. "." .. this.name)
    end
    return band(metadata[2], 0x8) == 1
  end,
  getIsPrivate = function (this)
    return isAccessibility(this, 1)
  end,
  getIsFamilyAndAssembly = function (this)
    return isAccessibility(this, 2)
  end,
  getIsFamily = function (this)
    return isAccessibility(this, 3)
  end,
  getIsAssembly = function (this)
    return isAccessibility(this, 4)
  end,
  getIsFamilyOrAssembly = function (this)
    return isAccessibility(this, 5)
  end,
  getIsPublic = function (this)
    return isAccessibility(this, 6)
  end
})

local function getFieldOrPropertyType(this)
  local metadata = this.metadata
  if not metadata then
    throwNoMatadata(this.c.__name__ .. "." .. this.name)
  end
  return typeof(metadata[3])
end

local function checkObj(obj, cls)
  if not is(obj, cls) then
    throw(ArgumentException("Object does not match target type.", "obj"), 1)
  end
end

local function checkTarget(cls, obj, metadata)
  if band(metadata[2], 0x8) == 0 then
    if obj == nil then
      throw(TargetException())
    end
    checkObj(obj, cls)
  else
    return true
  end
end

local function checkValue(value, valueClass)
  if value == nil then
    if valueClass.class == "S" then
      value = valueClass:default()
    end
  else
    checkObj(value, valueClass)
  end
  return value
end

local function getOrSetField(this, obj, isSet, value)
  local cls, metadata = this.c, this.metadata
  if metadata then
    if checkTarget(cls, obj, metadata) then
      obj = cls
    end
    local name = metadata[4]
    if type(name) ~= "string" then
      name = this.name
    end
    if isSet then
      obj[name] = checkValue(value, metadata[3])
    else
      return obj[name]
    end
  else
    if obj ~= nil then
      checkObj(obj, cls)
    else
      obj = cls
    end
    if isSet then
      obj[this.name] = value
    else
      return obj[this.name]
    end
  end
end

local function isMetadataDefined(metadata, index, attributeType)
  attributeType = attributeType[1]
  for i = index, #metadata do
    if is(metadata[i], attributeType) then
      return true
    end
  end
  return false
end

local function fillMetadataCustomAttributes(t, metadata, index, attributeType)
  local count = #t + 1
  if attributeType then
    attributeType = attributeType[1]
    for i = index, #metadata do
      if is(metadata[i], attributeType) then
        t[count] = metadata[i]
        count = count + 1
      end
    end
  else
    for i = index, #metadata do
      t[count] = metadata[i]
      count = count + 1
    end
  end
end

local FieldInfo = define("System.Reflection.FieldInfo", {
  __eq = eq,
  base = { MemberInfo },
  memberType = 4,
  getFieldType = getFieldOrPropertyType,
  GetValue = getOrSetField,
  SetValue = function (this, obj, value)
    getOrSetField(this, obj, true, value)
  end,
  IsDefined = function (this, attributeType)
    if attributeType == nil then throw(ArgumentNullException()) end
    local metadata = this.metadata
    if metadata then
      return isMetadataDefined(metadata, 4, attributeType)
    end
    return false
  end,
  GetCustomAttributes = function (this, attributeType, inherit)
    if type(attributeType) == "boolean" then
      attributeType, inherit = nil, attributeType
    else
      if attributeType == nil then throw(ArgumentNullException()) end
    end
    local t = {}
    local metadata = this.metadata
    if metadata then
      local index = 4
      if type(metadata[index]) == "string" then
        index = 5
      end
      fillMetadataCustomAttributes(t, metadata, index, attributeType)
    end
    return arrayFromTable(t, System.Attribute) 
  end
})

local function getOrSetProperty(this, obj, isSet, value)
  local cls, metadata = this.c, this.metadata
  if metadata then
    local isStatic
    if checkTarget(cls, obj, metadata) then
      obj = cls
      isStatic = true
    end
    if isSet then
      value = checkValue(value, metadata[3])
    end
    local kind = band(metadata[2], 0x300)
    if kind == 0 then
      local name = metadata[4]
      if type(name) ~= "string" then
        name = this.name
      end
      if isSet then
        obj[name] = value
      else
        return obj[name]
      end
    else
      local index
      if kind == 0x100 then
        index = isSet and 5 or 4      
      elseif kind == 0x200 then
        if isSet then
          throw(ArgumentException("Property Set method was not found."))
        end
        index = 4
      else
        if not isSet then
          throw(ArgumentException("Property Get method was not found."))
        end  
        index = 4
      end
      local fn = metadata[index]
      if type(fn) == "table" then
        fn = fn[1]
      end
      if isSet then
        if isStatic then
          fn(value)
        else
          fn(obj, value)
        end  
      else
        return fn(obj)
      end
    end
  else
    local isStatic
    if obj ~= nil then
      checkObj(obj, cls)
    else
      obj = cls
      isStatic = true
    end
    if this.isField then
      if isSet then
        obj[this.name] = value
      else
        return obj[this.name]
      end
    else
      if isSet then
        local fn = obj["set" .. this.name]
        if fn == nil then
          throw(ArgumentException("Property Set method not found."))
        end
        if isStatic then
          fn(value)
        else
          fn(obj, value)
        end
      else
        local fn = obj["get" .. this.name]
        if fn == nil then
          throw(ArgumentException("Property Get method not found."))
        end
        return fn(obj)
      end
    end
  end
end

local function getPropertyAttributesIndex(metadata)
  local kind = band(metadata[2], 0x300)
  local index
  if kind == 0 then
    index = 4
  elseif kind == 0x100 then
    index = 6
  else
    index = 5
  end
  return index
end

local PropertyInfo = define("System.Reflection.PropertyInfo", {
  __eq = eq,
  base = { MemberInfo },
  memberType = 16,
  getPropertyType = getFieldOrPropertyType,
  GetValue = getOrSetProperty,
  SetValue = function (this, obj, value)
    getOrSetProperty(this, obj, true, value)
  end,
  IsDefined = function (this, attributeType)
    if attributeType == nil then throw(ArgumentNullException()) end
    local metadata = this.metadata
    if metadata then
      local index = getPropertyAttributesIndex(metadata)
      return isMetadataDefined(metadata, index, attributeType)
    end
    return false
  end,
  GetCustomAttributes = function (this, attributeType, inherit)
    if type(attributeType) == "boolean" then
      attributeType, inherit = nil, attributeType
    else
      if attributeType == nil then throw(ArgumentNullException()) end
    end
    local t = {}
    local metadata = this.metadata
    if metadata then
      local index = getPropertyAttributesIndex(metadata)
      fillMetadataCustomAttributes(t, metadata, index, attributeType)
    end
    return arrayFromTable(t, System.Attribute) 
  end
})

local function hasPublicFlag(flags)
  return band(flags, 0x7) == 6
end

local function getMethodParameterCount(flags)
  local count = band(flags, 0xFF00)
  if count ~= 0 then
    count = count / 256
  end
  return floor(count)
end

local function getMethodAttributesIndex(metadata)
  local flags = metadata[2]
  local index
  local typeParametersCount = band(flags, 0xFF0000)
  if typeParametersCount == 0 then
    local parameterCount = getMethodParameterCount(flags)
    if band(flags, 0x80) == 0 then
      index = 4 + parameterCount
    else
      index = 5 + parameterCount
    end
  else
    index = 5
  end
  return index
end

local MethodInfo = define("System.Reflection.MethodInfo", {
  __eq = eq,
  base = { MemberInfo },
  memberType = 8,
  getReturnType = function (this)
    local metadata = this.metadata
    if not metadata then
      throwNoMatadata(this.c.__name__ .. "." .. this.name)
    end
    local flags = metadata[2]
    if band(flags, 0x80) == 0 then
      return Type.Void
    end
    if band(flags, 0xC00) > 0 then
      assert(false, "not implement for generic method")
    end
    local parameterCount = getMethodParameterCount(flags)
    return typeof(metadata[4 + parameterCount])
  end,
  Invoke = function (this, obj, parameters)
    local cls, metadata = this.c, this.metadata
    if metadata then
      local isStatic
      if checkTarget(cls, obj, metadata) then
        isStatic = true
      end
      local t = {}
      local parameterCount = getMethodParameterCount(metadata[2])
      if parameterCount == 0 then
        if parameters ~= nil and #parameters > 0 then
          throw(TargetParameterCountException())
        end
      else
        if parameters == nil and #parameters ~= parameterCount then
          throw(TargetParameterCountException())
        end
        for i = 4, 3 + parameterCount do
          local j = #t
          local paramValue, mtData = parameters:get(j), metadata[i]
          if mtData ~= nil then
            paramValue = checkValue(paramValue, mtData)
          end
          t[j + 1] = paramValue
        end
      end
      local f = metadata[3]
      if isStatic then
        if t then
          return f(unpack(t, 1, parameterCount))
        else
          return f()
        end
      else
        if t then
          return f(obj, unpack(t, 1, parameterCount))
        else
          return f(obj)
        end
      end
    else
      local f = assert(this.f)
      if obj ~= nil then
        checkObj(obj, cls)
        if parameters ~= nil then
          local t = toLuaTable(parameters)
          return f(obj, unpack(t, 1, #parameters))
        else
          return f(obj)
        end
      else
        if parameters ~= nil then
          local t = toLuaTable(parameters)
          return f(unpack(t, 1, #parameters))
        else
          return f()
        end
      end
    end
  end,
  IsDefined = function (this, attributeType, inherit)
    if attributeType == nil then throw(ArgumentNullException()) end
    local metadata = this.metadata
    if metadata then
      local index = getMethodAttributesIndex(metadata)
      return isMetadataDefined(metadata, index, attributeType)
    end
    return false
  end,
  GetCustomAttributes = function (this, attributeType, inherit)
    if type(attributeType) == "boolean" then
      attributeType, inherit = nil, attributeType
    else
      if attributeType == nil then throw(ArgumentNullException()) end
    end
    local t = {}
    local metadata = this.metadata
    if metadata then
      local index = getMethodAttributesIndex(metadata)
      fillMetadataCustomAttributes(t, metadata, index, attributeType)
    end
    return arrayFromTable(t, System.Attribute)
  end
})

local function buildFieldInfo(cls, name, metadata)
  return setmetatable({ c = cls, name = name, metadata = metadata }, FieldInfo)
end

local function buildPropertyInfo(cls, name, metadata, isField)
  return setmetatable({ c = cls, name = name, metadata = metadata, isField = isField }, PropertyInfo)
end

local function buildMethodInfo(cls, name, metadata, f)
  return setmetatable({ c = cls, name = name, metadata = metadata, f = f }, MethodInfo)
end

-- https://en.cppreference.com/w/cpp/algorithm/lower_bound
local function lowerBound(t, first, last, value, comp)
  local count = last - first
  local it, step
  while count > 0 do
    it = first
    step = div(count, 2)
    it = it + step
    if comp(t[it], value) then
      it = it + 1
      first = it
      count = count - (step + 1)
    else
      count = step
    end
  end
  return first
end

local function metadataItemCompByName(item, name)
  return item[1] < name
end

local function binarySearchByName(metadata, name)
  local last = #metadata + 1
  local index = lowerBound(metadata, 1, last, name, metadataItemCompByName)
  if index ~= last then
    return metadata[index], index
  end
  return nil
end

function Type.GetField(this, name)
  if name == nil then throw(ArgumentNullException()) end
  local cls = this[1]
  local metadata = cls.__metadata__
  if metadata then
    local fields = metadata.fields
    if fields then
      local field = binarySearchByName(fields, name)
      if field then
        return buildFieldInfo(cls, name, field)
      end
      return nil
    end
  end
  if type(cls[name]) ~= "function" then
    return buildFieldInfo(cls, name)
  end
end

function Type.GetFields(this)
  local t = {}
  local cls = this[1]
  local count = 1
  repeat
    local metadata = rawget(cls, "__metadata__")
    if metadata then
      local fields = metadata.fields
      if fields then
        for i = 1, #fields do
          local field = fields[i]
          if hasPublicFlag(field[2]) then
            t[count] = buildFieldInfo(cls, field[1], field)
            count = count + 1
          end
        end
      else
        metadata = nil
      end
    end
    if not metadata then
      for k, v in pairs(cls) do
        if type(v) ~= "function" then
          t[count] = buildFieldInfo(cls, k)
          count = count + 1
        end
      end
    end
    cls = getmetatable(cls)
  until cls == nil 
  return arrayFromTable(t, FieldInfo)
end

function Type.GetProperty(this, name)
  if name == nil then throw(ArgumentNullException()) end
  local cls = this[1]
  local metadata = cls.__metadata__
  if metadata then
    local properties = metadata.properties
    if properties then
      local property = binarySearchByName(properties, name)
      if property then
        return buildPropertyInfo(cls, name, property)
      end
      return nil
    end
  end
  if cls["get" .. name] or cls["set" .. name] then
    return buildPropertyInfo(cls, name)
  else
    return buildPropertyInfo(cls, name, nil, true)
  end
end

function Type.GetProperties(this)
  local t = {}
  local cls = this[1]
  local count = 1
  repeat
    local metadata = rawget(cls, "__metadata__")
    if metadata then
      local properties = metadata.properties
      if properties then
        for i = 1, #properties do
          local property = properties[i]
          if hasPublicFlag(property[2]) then
            t[count] = buildPropertyInfo(cls, property[1], property)
            count = count + 1
          end
        end
      end
    end
    cls = getmetatable(cls)
  until cls == nil 
  return arrayFromTable(t, PropertyInfo)
end

function Type.GetMethod(this, name)
  if name == nil then throw(ArgumentNullException()) end
  local cls = this[1]
  local metadata = cls.__metadata__
  if metadata then
    local methods = metadata.methods
    if methods then
      local item, index = binarySearchByName(methods, name)
      if item then
        local next = methods[index + 1]
        if next and next[1] == name then
          throw(AmbiguousMatchException())
        end
        return buildMethodInfo(cls, name, item)
      end
      return nil
    end
  end
  local f = cls[name]
  if type(f) == "function" then
    return buildMethodInfo(cls, name, nil, f)
  end
end

function Type.GetMethods(this)
  local t = {}
  local cls = this[1]
  local count = 1
  repeat
    local metadata = rawget(cls, "__metadata__")
    if metadata then
      local methods = metadata.methods
      if methods then
        for i = 1, #methods do
          local method = methods[i]
          if hasPublicFlag(method[2]) then
            t[count] = buildMethodInfo(cls, method[1], method)
            count = count + 1
          end
        end
      else
        metadata = nil
      end
    end
    if not metadata then
      for k, v in pairs(cls) do
        if type(v) == "function" then
          t[count] = buildMethodInfo(cls, k, nil, v)
          count = count + 1
        end
      end
    end
    cls = getmetatable(cls)
  until cls == nil 
  return arrayFromTable(t, MethodInfo)
end

function Type.GetMembers(this)
  local t = arrayFromTable({}, MemberInfo)
  t:addRange(this:GetFields())
  t:addRange(this:GetProperties())
  t:addRange(this:GetMethods())
  return t
end

function Type.IsDefined(this, attributeType, inherit)
  if attributeType == nil then throw(ArgumentNullException()) end
  local cls = this[1]
  if not inherit then
    local metadata = rawget(cls, "__metadata__")
    if metadata then
      local class  = metadata.class
      if class then
        return isMetadataDefined(class, 2, attributeType)
      end
    end
    return false
  else
    repeat
      local metadata = rawget(cls, "__metadata__")
      if metadata then
        local class  = metadata.class
        if class then
          if isMetadataDefined(class, 2, attributeType) then
            return true
          end
        end
      end
      cls = getmetatable(cls)
    until cls == nil
    return false
  end
end

function Type.GetCustomAttributes(this, attributeType, inherit)
  if type(attributeType) == "boolean" then
    attributeType, inherit = nil, attributeType
  else
    if attributeType == nil then throw(ArgumentNullException()) end
  end
  local cls = this[1]
  local t = {}
  if not inherit then
    local metadata = rawget(cls, "__metadata__")
    if metadata then
      local class  = metadata.class
      if class then
        fillMetadataCustomAttributes(t, class, 2, attributeType)
      end
    end
  else
    repeat
      local metadata = rawget(cls, "__metadata__")
      if metadata then
        local class  = metadata.class
        if class then
          fillMetadataCustomAttributes(t, class, 2, attributeType)
        end
      end
      cls = getmetatable(cls)
    until cls == nil
  end
  return arrayFromTable(t, System.Attribute)
end

local Assembly, coreSystemAssembly
local function getAssembly(t)
  local assembly = t[1].__assembly__
  if assembly then
    return setmetatable(assembly, Assembly)
  end
  return coreSystemAssembly
end

local function getAssemblyName(this)
  local name = this.name or "CSharpLua.CoreLib"
  return name .. ", Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
end

Assembly = define("System.Reflection.Assembly", {
  GetName = getAssemblyName,
  getFullName = getAssemblyName,
  GetAssembly = getAssembly,
  GetTypeFrom = Type.GetTypeFrom,
  GetEntryAssembly = function ()
    local entryAssembly = System.entryAssembly
    if entryAssembly then
      return setmetatable(entryAssembly, Assembly)
    end
    return nil
  end,
  getEntryPoint = function (this)
    local entryPoint = this.entryPoint
    if entryPoint ~= nil then
      local _, _, t, name = entryPoint:find("(.*)%.(.*)")
      local cls = getClass(t)
      local f = assert(cls[name])
      return buildMethodInfo(cls, name, nil, f)
    end
    return nil
  end,
  GetExportedTypes = function (this)
    if this.exportedTypes then
      return this.exportedTypes
    end
    local t = {}
    local classes = this.classes
    if classes then
      for i = 1, #classes do
        t[i] = typeof(classes[i])
      end
    end
    local array = arrayFromTable(t, Type, true)
    this.exportedTypes = array
    return array
  end
})
coreSystemAssembly = Assembly()

function System.GetExecutingAssembly(assembly)
	return setmetatable(assembly, Assembly)
end

Type.getAssembly = getAssembly

function Type.getAssemblyQualifiedName(this)
  return this:getName() .. ', ' .. getName(assembly)
end

function Type.getAttributes(this)
  local cls = this[1]
  local metadata = rawget(cls, "__metadata__")
  if metadata then
    metadata = metadata.class
    if metadata then
      return metadata[1]
    end
  end
  throwNoMatadata(cls.__name__)
end

function Type.GetGenericArguments(this)
  local t = {}
  local count = 1

  local cls = this[1]
  local metadata = rawget(cls, "__metadata__")
  if metadata then
    metadata = metadata.class
    if metadata then
      local flags = metadata[1]
      local typeParameterCount = band(flags, 0xFF00)
      if typeParameterCount ~= 0 then
        typeParameterCount = typeParameterCount / 256
        for i = 2, 1 + typeParameterCount do
          t[count] = typeof(metadata[i])
          count = count + 1
        end
      end
      return arrayFromTable(t, Type)
    end
  end

  local name = cls.__name__ 
  local i = name:find("%[")
  if i then
    i = i + 1
    while true do
      local j = name:find(",", i) or -1
      local clsName = name:sub(i, j - 1)
      t[count] = typeof(System.getClass(clsName))
      if j == -1 then
        break
      end
      count = count + 1
      i = j + 1
    end
  end
  return arrayFromTable(t, Type)
end

local Attribute = System.Attribute

function Attribute.GetCustomAttribute(element, attributeType, inherit)
  return element:GetCustomAttribute(attributeType, inherit)
end

function Attribute.GetCustomAttributes(element, attributeType, inherit)
  return element:GetCustomAttributes(attributeType, inherit)
end

function Attribute.IsDefined(element, attributeType, inherit)
	return element:IsDefined(attributeType, inherit)
end

local function createInstance(T, nonPublic)
  local metadata = rawget(T, "__metadata__")
  if metadata then
    local methods = metadata.methods
    if methods then
      local ctorMetadata = methods[1]
      if ctorMetadata[1] == ".ctor" then
        local flags = ctorMetadata[2]
        if nonPublic or hasPublicFlag(flags) then
          local parameterCount = getMethodParameterCount(flags)
          if parameterCount == 0 then
            return T()
          end
        end
        throw(MissingMethodException())
      end
    end
  end
  return T()
end

local function isCtorMatch(method, n, f, ...)
  local flags = method[2]
  if hasPublicFlag(flags) then
    local parameterCount = getMethodParameterCount(flags)
    if parameterCount == n then
      for j = 4, 3 + parameterCount do
        local p = f(j - 3, ...)
        if not is(p, method[j]) then
          return false
        end
      end
      return true
    end
  end
  return false
end

local function findMatchCtor(T, n, f, ...)
  local metadata = rawget(T, "__metadata__")
  if metadata then
    local hasCtor
    local methods = metadata.methods
    for i = 1, #methods do
      local method = methods[i]
      if method[1] == ".ctor" then
        if isCtorMatch(method, n, f, ...) then
          return i
        end
        hasCtor = true
      else
        break
      end
    end
    if hasCtor then
      throw(MissingMethodException())
    end
  end
end

define("System.Activator", {
  CreateInstance = function (type, ...)
    if type == nil then throw(ArgumentNullException("type")) end
    if getmetatable(type) ~= Type then
      return createInstance(type)
    end
    local T, n = type[1], select("#", ...)
    if n == 0 then
      return createInstance(T)
    elseif n == 1 then
      local args = ...
      if System.isArrayLike(args) then
        n = #args
        if n == 0 then
          return createInstance(T)
        end
        local i = findMatchCtor(T, n, function (i, args) return args:get(i - 1) end, args)
        if i and i ~= 1 then
          return System.new(T, i, unpack(args, 1, n))
        end
        return T(unpack(args, 1, n))
      end
    end
    local i = findMatchCtor(T, n, select, ...)
    if i and i ~= 1 then
      return System.new(T, i, ...)
    end
    return T(...)
  end,
  CreateInstance1 = function (type, nonPublic)
    if type == nil then throw(ArgumentNullException("type")) end
    return createInstance(type[1], nonPublic)
  end
})
