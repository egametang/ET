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
local null = System.null
local falseFn = System.falseFn
local each = System.each
local lengthFn = System.lengthFn
local versions = System.versions
local Array = System.Array
local toString = System.toString

local checkIndexAndCount = System.checkIndexAndCount
local throwFailedVersion = System.throwFailedVersion
local ArgumentNullException = System.ArgumentNullException
local ArgumentException = System.ArgumentException
local KeyNotFoundException = System.KeyNotFoundException
local EqualityComparer = System.EqualityComparer
local NotSupportedException = System.NotSupportedException

local assert = assert
local pairs = pairs
local next = next
local select = select
local getmetatable = getmetatable
local setmetatable = setmetatable
local tconcat = table.concat
local tremove = table.remove
local type = type

local counts = setmetatable({}, { __mode = "k" })
System.counts = counts

local function getCount(this)
  local t = counts[this]
  if t then
    return t[1]
  end
  return 0
end

local function pairsFn(t, i)
  local count =  counts[t]
  if count then
    if count[2] ~= count[3] then
      throwFailedVersion()
    end
  end
  local k, v = next(t, i)
  if v == null then
    return k
  end
  return k, v
end

function System.pairs(t)
  local count = counts[t]
  if count then
    count[3] = count[2]
  end
  return pairsFn, t
end

local KeyValuePairFn
local KeyValuePair = {
  __ctor__ = function (this, ...)
    if select("#", ...) == 0 then
      this[1], this[2] = this.__genericTKey__:default(), this.__genericTValue__:default()
    else
      this[1], this[2] = ...
    end
  end,
  Create = function (key, value, TKey, TValue)
    return setmetatable({ key, value }, KeyValuePairFn(TKey, TValue))
  end,
  Deconstruct = function (this)
    return this[1], this[2]
  end,
  ToString = function (this)
    local t = { "[" }
    local count = 2
    local k, v = this[1], this[2]
    if k ~= nil then
      t[count] = toString(k)
      count = count + 1
    end
    t[count] = ", "
    count = count + 1
    if v ~= nil then
      t[count] = toString(v)
      count = count + 1
    end
    t[count] = "]"
    return tconcat(t)
  end
}

KeyValuePairFn = System.defStc("System.Collections.Generic.KeyValuePair", function(TKey, TValue)
  local cls = {
    __genericTKey__ = TKey,
    __genericTValue__ = TValue,
  }
  return cls
end, KeyValuePair, 2)
System.KeyValuePair = KeyValuePairFn

local function isKeyValuePair(t)
  return getmetatable(getmetatable(t)) == KeyValuePair
end

local DictionaryEnumerator = define("System.Collections.Generic.DictionaryEnumerator", {
  getCurrent = System.getCurrent, 
  Dispose = System.emptyFn,
  MoveNext = function (this)
    local t, kind = this.dict, this.kind
    local count = counts[t]
    if this.version ~= (count and count[2] or 0) then
      throwFailedVersion()
    end
    local k, v = next(t, this.index)
    if k ~= nil then
      if kind then
        kind[1] = k
        if v == null then v = nil end
        kind[2] = v
      elseif kind == false then
        if v == null then v = nil end
        this.current = v
      else
        this.current = k
      end
      this.index = k
      return true
    else
      if kind then
        kind[1], kind[2] = kind.__genericTKey__:default(), kind.__genericTValue__:default()
      elseif kind == false then
        this.current = t.__genericTValue__:default()
      else
        this.current = t.__genericTKey__:default()
      end
      return false
    end
  end
})

local function dictionaryEnumerator(t, kind)
  local current
  if not kind then
    local TKey, TValue = t.__genericTKey__, t.__genericTValue__
    kind = setmetatable({ TKey:default(), TValue:default() }, t.__genericT__)
    current = kind
  elseif kind == 1 then
    local TKey = t.__genericTKey__
    current = TKey:default()
    kind = nil
  else
    local TValue = t.__genericTValue__
    current = TValue:default()
    kind = false
  end
  local count = counts[t]
  local en = {
    dict = t,
    version = count and count[2] or 0,
    kind = kind,
    current = current
  }
  return setmetatable(en, DictionaryEnumerator)
end

local DictionaryCollection = define("System.Collections.Generic.DictionaryCollection", function (T)
    return {
      base = { System.ICollection_1(T), System.IReadOnlyCollection_1(T), System.ICollection },
      __genericT__ = T
    }
  end, {
  __ctor__ = function (this, dict, kind)
    this.dict = dict
    this.kind = kind
  end,
  getCount = function (this)
    return getCount(this.dict)
  end,
  GetEnumerator = function (this)
    return dictionaryEnumerator(this.dict, this.kind)
  end
}, 1)

local function add(this, key, value)
  if key == nil then throw(ArgumentNullException("key")) end
  if this[key] ~= nil then throw(ArgumentException("key already exists")) end
  this[key] = value == nil and null or value
  local t = counts[this]
  if t then
    t[1] = t[1] + 1
    t[2] = t[2] + 1
  else
    counts[this] = { 1, 1 }
  end
end

local function remove(this, key)
  if key == nil then throw(ArgumentNullException("key")) end
  if this[key] ~= nil then
    this[key] = nil
    local t = counts[this]
    t[1] = t[1] - 1
    t[2] = t[2] + 1
    return true
  end
  return false
end

local function buildFromDictionary(this, dictionary)
  if dictionary == nil then throw(ArgumentNullException("dictionary")) end
  local count = 0
  for k, v in pairs(dictionary) do
    this[k] = v
    count = count + 1
  end
  counts[this] = { count, 0 }
end

local ArrayDictionaryFn
local function buildHasComparer(this, ...)
   local Dictionary = ArrayDictionaryFn(this.__genericTKey__, this.__genericTValue__)
   Dictionary.__ctor__(this, ...)
   return setmetatable(this, Dictionary)
end

local Dictionary = {
  getIsFixedSize = falseFn,
  getIsReadOnly = falseFn,
  __ctor__ = function (this, ...) 
    local n = select("#", ...)
    if n == 0 then
    elseif n == 1 then
      local comparer = ...
      if comparer == nil or type(comparer) == "number" then  
      else
        local equals = comparer.EqualsOf
        if equals == nil then
          buildFromDictionary(this, comparer)
        else
          buildHasComparer(this, ...)
        end
      end
    else
      local dictionary, comparer = ...
      if comparer ~= nil then 
        buildHasComparer(this, ...)
      end
      if type(dictionary) ~= "number" then 
        buildFromDictionary(this, dictionary)
      end
    end
  end,
  AddKeyValue = add,
  Add = function (this, ...)
    local k, v
    if select("#", ...) == 1 then
      local pair = ... 
      k, v = pair[1], pair[2]
    else
      k, v = ...
    end
    add(this, k ,v)
  end,
  Clear = function (this)
    for k, v in pairs(this) do
      this[k] = nil
    end
    counts[this] = nil
  end,
  ContainsKey = function (this, key)
    if key == nil then throw(ArgumentNullException("key")) end
    return this[key] ~= nil 
  end,
  ContainsValue = function (this, value)
    if value == nil then
      for _, v in pairs(this) do
        if v == null then
          return true
        end
      end
    else
      local comparer = EqualityComparer(this.__genericTValue__).getDefault()
      local equals = comparer.EqualsOf
        for _, v in pairs(this) do
          if v ~= null then
            if equals(comparer, value, v ) then
              return true
            end
          end
      end
    end
    return false
  end,
  Contains = function (this, pair)
    local key = pair[1]
    if key == nil then throw(ArgumentNullException("key")) end
    local value = this[key]
    if value ~= nil then
      if value == null then value = nil end
      local comparer = EqualityComparer(this.__genericTValue__).getDefault()
      if comparer:EqualsOf(value, pair[2]) then
        return true
      end
    end
    return false
  end,
  CopyTo = function (this, array, index)
    local count = getCount(this)
    checkIndexAndCount(array, index, count)
    if count > 0 then
      local KeyValuePair = this.__genericT__
      index = index + 1
      for k, v in pairs(this) do
        if v == null then v = nil end
        array[index] = setmetatable({ k, v }, KeyValuePair)
        index = index + 1
      end
    end
  end,
  RemoveKey = remove,
  Remove = function (this, key)
    if isKeyValuePair(key) then
      local k, v = key[1], key[2]
      if k == nil then throw(ArgumentNullException("key")) end
      local value = this[k]
      if value ~= nil then
        if value == null then value = nil end
        local comparer = EqualityComparer(this.__genericTValue__).getDefault()
        if comparer:EqualsOf(value, v) then
          remove(this, k)
          return true
        end
      end
      return false
    end
    return remove(this, key)
  end,
  TryGetValue = function (this, key)
    if key == nil then throw(ArgumentNullException("key")) end
    local value = this[key]
    if value == nil then
      return false, this.__genericTValue__:default()
    end
    if value == null then return true end
    return true, value
  end,
  getComparer = function (this)
    return EqualityComparer(this.__genericTKey__).getDefault()
  end,
  getCount = getCount,
  get = function (this, key)
    if key == nil then throw(ArgumentNullException("key")) end
    local value = this[key]
    if value == nil then throw(KeyNotFoundException()) end
    if value ~= null then
      return value
    end
    return nil
  end,
  set = function (this, key, value)
    if key == nil then throw(ArgumentNullException("key")) end
    local t = counts[this]
    if t then
      if this[key] == nil then
        t[1] = t[1] + 1
      end
      t[2] = t[2] + 1
    else
      counts[this] = { 1, 1 }
    end
    this[key] = value == nil and null or value
  end,
  GetEnumerator = dictionaryEnumerator,
  getKeys = function (this)
    return DictionaryCollection(this.__genericTKey__)(this, 1)
  end,
  getValues = function (this)
    return DictionaryCollection(this.__genericTValue__)(this, 2)
  end
}

local ArrayDictionaryEnumerator = define("System.Collections.Generic.ArrayDictionaryEnumerator", function (T)
  return {
    base = { System.IEnumerator_1(T) }
  }
end, {
  getCurrent = System.getCurrent, 
  Dispose = System.emptyFn,
  MoveNext = function (this)
    local t = this.list
    if this.version ~= versions[t] then
      throwFailedVersion()
    end
    local index = this.index
    local pair = t[index]
    if pair ~= nil then
      if this.kind then
        this.current = pair[2]
      else
        this.current = pair[1]
      end
      this.index = index + 1
      return true
    end
    this.current = nil
    return false
  end
}, 1)

local arrayDictionaryEnumerator = function (t, kind, T)
  return setmetatable({ list = t, kind = kind, index = 1, version = versions[t], currnet = T:default() }, ArrayDictionaryEnumerator(T))
end

local ArrayDictionaryCollection = define("System.Collections.Generic.ArrayDictionaryCollection", function (T)
  return {
    base = { System.ICollection_1(T), System.IReadOnlyCollection_1(T), System.ICollection },
    __genericT__ = T
  }
  end, {
  __ctor__ = function (this, dict, kind)
    this.dict = dict
    this.kind = kind
  end,
  getCount = function (this)
    return #this.dict
  end,
  get = function (this, index)
    local p = this.dict[index + 1]
    if p == nil then throw(System.ArgumentOutOfRangeException()) end
    if this.kind then
      return p[2]
    end
    return p[1]
  end,
  Contains = function (this, v)
    if this.kind then
      return this.dict:ContainsValue(v)
    end 
    return this.dict:ContainsKey(v)
  end,
  GetEnumerator = function (this)
    return arrayDictionaryEnumerator(this.dict, this.kind, this.__genericT__)
  end
}, 1)

local ArrayDictionary = (function ()
  local function buildFromDictionary(this, dictionary)
    if dictionary == nil then throw(ArgumentNullException("dictionary")) end
    local count = 1
    local KeyValuePair = this.__genericT__
    for _, pair in each(dictionary) do
      local k, v = pair[1], pair[2]
      if type(k) == "table" and k.class == 'S' then
        k = k:__clone__()
      end
      this[count] = setmetatable({ k, v }, KeyValuePair)
      count = count + 1
    end
  end 
  
  local function add(this, key, value, set)
    if key == nil then throw(ArgumentNullException("key")) end
    local len = #this
    if len > 0 then
      local comparer = this.comparer
      local equals = comparer.EqualsOf
      for i = 1, len do
        if equals(comparer, this[i][1], key) then
          if set then
            this[i][2] = value
            return
          else
            throw(ArgumentException("key already exists"))
          end
        end
      end
    end
    this[len + 1] = setmetatable({ key, value }, this.__genericT__)
    versions[this] = (versions[this] or 0) + 1
  end
  
  local function remove(this, key)
    if key == nil then throw(ArgumentNullException("key")) end
    local len = #this
    if len > 0 then
      local comparer = this.comparer
      local equals = comparer.EqualsOf
      for i = 1, len do
        if equals(comparer, this[i][1], key) then
          tremove(this, i)
          versions[this] = (versions[this] or 0) + 1
          return true
        end
      end
    end
    return false
  end
 
  return {
    getIsFixedSize = falseFn,
    getIsReadOnly = falseFn,
    __ctor__ = function (this, ...)
      local Comparer
      local n = select("#", ...)
      if n == 0 then
      elseif n == 1 then
        local comparer = ...
        if comparer == nil or type(comparer) == "number" then  
        else
          local equals = comparer.EqualsOf
          if equals == nil then
            buildFromDictionary(this, comparer)
          else
            Comparer = comparer
          end
        end
      else
        local dictionary, comparer = ...
        if type(dictionary) ~= "number" then 
           buildFromDictionary(this, dictionary)
        end
        Comparer = comparer
      end
      this.comparer = Comparer or EqualityComparer(this.__genericTKey__).getDefault()
    end,
    AddKeyValue = add,
    Add = function (this, ...)
      local k, v
      if select("#", ...) == 1 then
        local pair = ... 
        k, v = pair[1], pair[2]
      else
        k, v = ...
      end
      add(this, k ,v)
    end,
    Clear = Array.clear,
    ContainsKey = function (this, key)
      if key == nil then throw(ArgumentNullException("key")) end
      local len = #this
      if len > 0 then
        local comparer = this.comparer
        local equals = comparer.EqualsOf
        for i = 1, len do
          if equals(comparer, this[i][1], key) then
            return true
          end
        end
      end
      return false
    end,
    ContainsValue = function (this, value)
      local len = #this
      if len > 0 then
        local comparer = EqualityComparer(this.__genericTValue__).getDefault()
        local equals = comparer.EqualsOf
        for i = 1, len do
          if equals(comparer, value, this[i][2]) then
            return true
          end
        end
      end
      return false
    end,
    Contains = function (this, pair)
      local key = pair[1]
      if key == nil then throw(ArgumentNullException("key")) end
      local len = #this
      if len > 0 then
        local comparer = this.comparer
        local equals = comparer.EqualsOf
        for i = 1, len do
          local t = this[i]
          if equals(comparer, t[1], key) then
            local comparer = EqualityComparer(this.__genericTValue__).getDefault()
            if comparer:EqualsOf(t[2], pair[2]) then
              return true
            end 
          end
        end
      end
      return false
    end,
    CopyTo = function (this, array, index)
      local count = #this
      checkIndexAndCount(array, index, count)
      if count > 0 then
        local KeyValuePair = this.__genericT__
        index = index + 1
        for i = 1, count do
          local t = this[i]
          array[index] = setmetatable({ t[1]:__clone__(), t[2] }, KeyValuePair)
          index = index + 1
        end
      end
    end,
    RemoveKey = remove,
    Remove = function (this, key)
      if isKeyValuePair(key) then
        local len = #this
        local k, v = key[1], key[2]
        for i = 1, #this do
          local pair = this[i]
          if pair[1]:EqualsObj(k) then
            local comparer = EqualityComparer(this.__genericTValue__).getDefault()
            if comparer:EqualsOf(pair[2], v) then
              tremove(this, i)
              return true
            end
          end
        end
      end
      return false
    end,
    TryGetValue = function (this, key)
      if key == nil then throw(ArgumentNullException("key")) end
      local len = #this
      if len > 0 then
        local comparer = this.comparer
        local equals = comparer.EqualsOf
        for i = 1, len do
          local pair = this[i]
          if equals(comparer, pair[1], key) then
            return true, pair[2]
          end
        end
      end
      return false, this.__genericTValue__:default()
    end,
    getComparer = function (this)
      return this.comparer
    end,
    getCount = lengthFn,
    get = function (this, key)
      if key == nil then throw(ArgumentNullException("key")) end
      local len = #this
      if len > 0 then
        local comparer = this.comparer
        local equals = comparer.EqualsOf
        for i = 1, len do
          local pair = this[i]
          if equals(comparer, pair[1], key) then
            return pair[2]
          end
        end
      end
      throw(KeyNotFoundException())
    end,
    set = function (this, key, value)
      add(this, key, value, true)
    end,
    GetEnumerator = Array.GetEnumerator,
    getKeys = function (this)
      return ArrayDictionaryCollection(this.__genericTKey__)(this)
    end,
    getValues = function (this)
      return ArrayDictionaryCollection(this.__genericTValue__)(this, true)
    end
  }
end)()

ArrayDictionaryFn = define("System.Collections.Generic.ArrayDictionary", function(TKey, TValue) 
  return { 
    base = { System.IDictionary_2(TKey, TValue), System.IDictionary, System.IReadOnlyDictionary_2(TKey, TValue) },
    __genericT__ = KeyValuePairFn(TKey, TValue),
    __genericTKey__ = TKey,
    __genericTValue__ = TValue,
  }
end, ArrayDictionary, 2)

function System.dictionaryFromTable(t, TKey, TValue)
  return setmetatable(t, Dictionary(TKey, TValue))
end

function System.isDictLike(t)
  return type(t) == "table" and t.GetEnumerator == dictionaryEnumerator
end

local DictionaryFn = define("System.Collections.Generic.Dictionary", function(TKey, TValue)
  local array, len
  if TKey.class == 'S' and type(TKey:default()) == "table" then
    array = ArrayDictionary
  else
    len = getCount
  end
  return { 
    base = { System.IDictionary_2(TKey, TValue), System.IDictionary, System.IReadOnlyDictionary_2(TKey, TValue) },
    __genericT__ = KeyValuePairFn(TKey, TValue),
    __genericTKey__ = TKey,
    __genericTValue__ = TValue,
    __len = len
  }, array
end, Dictionary, 2)

System.Dictionary = DictionaryFn
System.ArrayDictionary = ArrayDictionaryFn

local Object = System.Object
System.Hashtable = DictionaryFn(Object, Object)
