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
local trueFn = System.trueFn
local falseFn = System.falseFn
local lengthFn = System.lengthFn

local InvalidOperationException = System.InvalidOperationException
local NullReferenceException = System.NullReferenceException
local ArgumentException = System.ArgumentException
local ArgumentNullException = System.ArgumentNullException
local ArgumentOutOfRangeException = System.ArgumentOutOfRangeException
local IndexOutOfRangeException = System.IndexOutOfRangeException
local NotSupportedException = System.NotSupportedException
local EqualityComparer = System.EqualityComparer
local Comparer_1 = System.Comparer_1
local IEnumerator_1 = System.IEnumerator_1

local assert = assert
local select = select
local getmetatable = getmetatable
local setmetatable = setmetatable
local type = type
local table = table
local tinsert = table.insert
local tremove = table.remove
local tmove = table.move
local tsort = table.sort
local pack = table.pack
local unpack = table.unpack
local error = error
local coroutine = coroutine
local ccreate = coroutine.create
local cresume = coroutine.resume
local cyield = coroutine.yield

local null = {}
local arrayEnumerator
local arrayFromTable

local versions = setmetatable({}, { __mode = "k" })
System.versions = versions

local function throwFailedVersion()
  throw(InvalidOperationException("Collection was modified; enumeration operation may not execute."))
end

local function checkIndex(t, index) 
  if index < 0 or index >= #t then
    throw(ArgumentOutOfRangeException("index"))
  end
end

local function checkIndexAndCount(t, index, count)
  if t == nil then throw(ArgumentNullException("array")) end
  if index < 0 or count < 0 or index + count > #t then
    throw(ArgumentOutOfRangeException("index or count"))
  end
end

local function wrap(v)
  if v == nil then 
    return null 
  end
  return v
end

local function unWrap(v)
  if v == null then 
    return nil 
  end
  return v
end

local function ipairs(t)
  local version = versions[t]
  return function (t, i)
    if version ~= versions[t] then
      throwFailedVersion()
    end
    local v = t[i]
    if v ~= nil then
      if v == null then
        v = nil
      end
      return i + 1, v
    end
  end, t, 1
end

local function eachFn(en)
  if en:MoveNext() then
    return true, en:getCurrent()
  end
  return nil
end

local function each(t)
  if t == nil then throw(NullReferenceException(), 1) end
  local getEnumerator = t.GetEnumerator
  if getEnumerator == arrayEnumerator then
    return ipairs(t)
  end
  local en = getEnumerator(t)
  return eachFn, en
end

function System.isArrayLike(t)
  return type(t) == "table" and t.GetEnumerator == arrayEnumerator
end

function System.isEnumerableLike(t)
  return type(t) == "table" and t.GetEnumerator ~= nil
end

function System.toLuaTable(array)
  local t = {}
  for i = 1, #array do
    local item = array[i]
    if item ~= null then
      t[i] = item
    end
  end   
  return t
end

System.null = null
System.Void = null
System.each = each
System.ipairs = ipairs
System.throwFailedVersion = throwFailedVersion

System.wrap = wrap
System.unWrap = unWrap
System.checkIndex = checkIndex
System.checkIndexAndCount = checkIndexAndCount

local Array
local emptys = {}

local function get(t, index)
  local v = t[index + 1]
  if v == nil then
    throw(ArgumentOutOfRangeException("index"))
  end
  if v ~= null then 
    return v
  end
  return nil
end

local function set(t, index, v)
  index = index + 1
  if t[index] == nil then
    throw(ArgumentOutOfRangeException("index"))
  end
  t[index] = v == nil and null or v
  versions[t] = (versions[t] or 0) + 1
end

local function add(t, v)
  local n = #t
  t[n + 1] = v == nil and null or v
  versions[t] = (versions[t] or 0) + 1
  return n
end

local function addRange(t, collection)
  if collection == nil then throw(ArgumentNullException("collection")) end
  local count = #t + 1
  if collection.GetEnumerator == arrayEnumerator then
    tmove(collection, 1, #collection, count, t)
  else
    for _, v in each(collection) do
      t[count] = v == nil and null or v
      count = count + 1
    end
  end
  versions[t] = (versions[t] or 0) + 1
end

local function unset()
  throw(NotSupportedException("Collection is read-only."))
end

local function fill(t, f, e, v)
  while f <= e do
    t[f] = v
    f = f + 1
  end
end

local function buildArray(T, len, t)
  if t == nil then 
    t = {}
    if len > 0 then
      local genericT = T.__genericT__
      local default = genericT:default()
      if default == nil then
        fill(t, 1, len, null)
      elseif type(default) ~= "table" then
        fill(t, 1, len, default)
      else
        for i = 1, len do
          t[i] = genericT:default()
        end
      end
    end
  else
    if len > 0 then
      local default = T.__genericT__:default()
      if default == nil then
        for i = 1, len do
          if t[i] == nil then
            t[i] = null
          end
        end
      end
    end
  end
  return setmetatable(t, T)
end

local function indexOf(t, v, startIndex, count)
  if t == nil then throw(ArgumentNullException("array")) end
  local len = #t
  if not startIndex then
    startIndex, count = 0, len
  elseif not count then
    if startIndex < 0 or startIndex > len then
      throw(ArgumentOutOfRangeException("startIndex"))
    end
    count = len - startIndex
  else
    if startIndex < 0 or startIndex > len then
      throw(ArgumentOutOfRangeException("startIndex"))
    end
    if count < 0 or count > len - startIndex then
      throw(ArgumentOutOfRangeException("count"))
    end
  end
  local comparer = EqualityComparer(t.__genericT__).getDefault()
  local equals = comparer.EqualsOf
  for i = startIndex + 1, startIndex + count do
    local item = t[i]
    if item == null then item = nil end
    if equals(comparer, item, v) then
      return i - 1
    end
  end
  return -1
end

local function findIndex(t, startIndex, count, match)
  if t == nil then throw(ArgumentNullException("array")) end
  local len = #t
  if not count then
    startIndex, count, match = 0, len, startIndex
  elseif not match then
    if startIndex < 0 or startIndex > len then
      throw(ArgumentOutOfRangeException("startIndex"))
    end
    count, match = len - startIndex, count
  else
    if startIndex < 0 or startIndex > len then
      throw(ArgumentOutOfRangeException("startIndex"))
    end
    if count < 0 or count > len - startIndex then
      throw(ArgumentOutOfRangeException("count"))
    end
  end
  if match == nil then throw(ArgumentNullException("match")) end
  local endIndex = startIndex + count
  for i = startIndex + 1, endIndex  do
    local item = t[i]
    if item == null then item = nil end
    if match(item) then
      return i - 1
    end
  end
  return -1
end

local function copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length, reliable)
  if not reliable then
    checkIndexAndCount(sourceArray, sourceIndex, length)
    checkIndexAndCount(destinationArray, destinationIndex, length)
  end
  tmove(sourceArray, sourceIndex + 1, sourceIndex + length, destinationIndex + 1, destinationArray)
end

local function removeRange(t, index, count)
  local n = #t
  if count < 0 or index > n - count then
    throw(ArgumentOutOfRangeException("index or count"))
  end
  if count > 0 then
    if index + count < n then
      tmove(t, index + count + 1, n, index + 1)
    end
    fill(t, n - count + 1, n, nil)
    versions[t] = (versions[t] or 0) + 1
  end
end

local function findAll(t, match)
  if t == nil then throw(ArgumentNullException("array")) end
  if match == nil then throw(ArgumentNullException("match")) end
  local list = {}
  local count = 1
  for i = 1, #t do
    local item = t[i]
    if (item == null and match(nil)) or match(item) then
      list[count] = item
      count = count + 1
    end
  end
  return list
end

local function getComp(t, comparer)
  local compare
  if comparer then
    if type(comparer) == "function" then
      compare = comparer
    else
      local Compare = comparer.Compare
      if Compare then
        compare = function (x, y) return Compare(comparer, x, y) end
      else
        compare = comparer
      end
    end
  else
    comparer = Comparer_1(t.__genericT__).getDefault()
    local Compare = comparer.Compare
    compare = function (x, y) return Compare(comparer, x, y) end
  end
  return function(x, y) 
    if x == null then x = nil end
    if y == null then y = nil end
    return compare(x, y) < 0
  end
end

local function sort(t, comparer)
  if #t > 1 then
    tsort(t, getComp(t, comparer))
    versions[t] = (versions[t] or 0) + 1
  end
end

local ArrayEnumerator = define("System.ArrayEnumerator", function (T)
  return {
    base = { IEnumerator_1(T) }
  }
end, {
  getCurrent = System.getCurrent, 
  Dispose = System.emptyFn,
  Reset = function (this)
    this.index = 1
    this.current = nil
  end,
  MoveNext = function (this)
    local t = this.list
    if this.version ~= versions[t] then
      throwFailedVersion()
    end
    local index = this.index
    local v = t[index]
    if v ~= nil then
      if v == null then
        this.current = nil
      else
        this.current = v
      end
      this.index = index + 1
      return true
    end
    this.current = nil
    return false
  end
})

arrayEnumerator = function (t, T)
  if not T then T = t.__genericT__ end
  return setmetatable({ list = t, index = 1, version = versions[t], currnet = T:default() }, ArrayEnumerator(T))
end

local ArrayReverseEnumerator = define("System.ArrayReverseEnumerator", function (T)
  return {
    base = { IEnumerator_1(T) }
  }
end, {
  getCurrent = System.getCurrent, 
  Dispose = System.emptyFn,
  Reset = function (this)
    this.index = #this.list
    this.current = nil
  end,
  MoveNext = function (this)
    local t = this.list
    if this.version ~= versions[t] then
      throwFailedVersion()
    end
    local index = this.index
    local v = t[index]
    if v ~= nil then
      if v == null then
        this.current = nil
      else
        this.current = v
      end
      this.index = index - 1
      return true
    end
    this.current = nil
    return false
  end
})

local function reverseEnumerator(t)
  local T = t.__genericT__
  return setmetatable({ list = t, index = #t, version = versions[t], currnet = T:default() }, ArrayReverseEnumerator(T))
end

local function checkArrayIndex(index1, index2)
  if index2 then
    throw(ArgumentException("Indices length does not match the array rank."))
  elseif type(index1) == "table" then
    if #index1 ~= 1 then
      throw(ArgumentException("Indices length does not match the array rank."))
    else
      index1 = index1[1]
    end
  end
  return index1
end

Array = {
  version = 0,
  new = buildArray,
  set = set,
  get = get,
  ctorList = function (t, ...)
    local len = select("#", ...)
    if len == 0 then return end
    local collection = ...
    if type(collection) == "number" then return end
    addRange(t, collection)
  end,
  add = add,
  addObj = function (this, item)
    if not System.is(item, this.__genericT__) then
      throw(ArgumentException())
    end
    return add(this, item)
  end,
  addRange = addRange,
  AsReadOnly = function (t)
    return System.ReadOnlyCollection(t.__genericT__)(t)
  end,
  clear = function (t)
    local size = #t
    if size > 0 then
      for i = 1, size do
        t[i] = nil
      end
      versions[t] = (versions[t] or 0) + 1
    end
  end,
  findAll = function (t, match)
    return setmetatable(findAll(t, match), System.List(t.__genericT__))
  end,
  first = function (t)
    if #t == 0 then throw(InvalidOperationException()) end
    local v = t[1]
    if v ~= null then
      return v
    end
    return nil
  end,
  insert = function (t, index, v)
    if index < 0 or index > #t then
      throw(ArgumentOutOfRangeException("index"))
    end
    tinsert(t, index + 1, v == nil and null or v)
    versions[t] = (versions[t] or 0) + 1
  end,
  insertRange = function (t, index, collection) 
    if collection == nil then throw(ArgumentNullException("collection")) end
    local len = #t
    if index < 0 or index > len then
      throw(ArgumentOutOfRangeException("index"))
    end
    if t.GetEnumerator == arrayEnumerator then
      local count = #collection
      if count > 0 then
        if index < len then
          tmove(t, index + 1, len, index + 1 + count, t)
        end
        if t == collection then
          tmove(t, 1, index, index + 1, t)
          tmove(t, index + 1 + count, count * 2, index * 2 + 1, t)
        else
          tmove(collection, 1, count, index + 1, t)
        end
      end
    else
      for _, v in each(collection) do
        index = index + 1
        tinsert(t, index, v == nil and null or v)
      end
    end
    versions[t] = (versions[t] or 0) + 1
  end,
  last = function (t)
    local n = #t
    if n == 0 then throw(InvalidOperationException()) end
    local v = t[n]
    if v ~= null then
      return v
    end
    return nil
  end,
  popFirst = function (t)
    if #t == 0 then throw(InvalidOperationException()) end
    local v = t[1]
    tremove(t, 1)
    versions[t] = (versions[t] or 0) + 1
    if v ~= null then
      return v
    end
    return nil
  end,
  popLast = function (t)
    local n = #t
    if n == 0 then throw(InvalidOperationException()) end
    local v = t[n]
    t[n] = nil
    if v ~= null then
      return v
    end
    return nil
  end,
  removeRange = removeRange,
  remove = function (t, v)
    local index = indexOf(t, v)
    if index >= 0 then
      tremove(t, index + 1)
      versions[t] = (versions[t] or 0) + 1
      return true
    end
    return false
  end,
  removeAll = function (t, match)
    if match == nil then throw(ArgumentNullException("match")) end
    local size = #t
    local freeIndex = 1
    while freeIndex <= size do
      local item = t[freeIndex]
      if item == null then  item = nil end
      if match(item) then
        break
      end
      freeIndex = freeIndex + 1 
    end
    if freeIndex > size then return 0 end
  
    local current = freeIndex + 1
    while current <= size do 
      while current <= size do
        local item = t[current]
        if item == null then item = nil end
        if not match(item) then
          break
        end
        current = current + 1 
      end
      if current <= size then
        t[freeIndex] = t[current]
        freeIndex = freeIndex + 1
        current = current + 1
      end
    end
    freeIndex = freeIndex -1
    local count = size - freeIndex
    removeRange(t, freeIndex, count)
    return count
  end,
  removeAt = function (t, index)
    local v = tremove(t, index + 1)
    if v == nil then
      throw(ArgumentOutOfRangeException("index"))
    end
    versions[t] = (versions[t] or 0) + 1
  end,
  getRange = function (t, index, count)
    if count < 0 or index > #t - count then
      throw(ArgumentOutOfRangeException("index or count"))
    end
    local list = {}
    tmove(t, index + 1, index + count, 1, list)
    return setmetatable(list, System.List(t.__genericT__))
  end,
  reverseEnumerator = reverseEnumerator,
  getCount = lengthFn,
  getSyncRoot = System.identityFn,
  getLongLength = lengthFn,
  getLength = lengthFn,
  getIsSynchronized = falseFn,
  getIsReadOnly = falseFn,
  getIsFixedSize = trueFn,
  getRank = System.oneFn,
  Add = unset,
  Clear = unset,
  Insert = unset,
  Remove = unset,
  RemoveAt = unset,
  BinarySearch = function (t, ...)
    if t == nil then throw(ArgumentNullException("array")) end
    local len = #t
    local index, count, v, comparer
    local n = select("#", ...)
    if n == 1 or n == 2 then
      index, count, v, comparer = 0, len, ...
    else
      index, count, v, comparer = ...
    end
    checkIndexAndCount(t, index, count)
    local compare
    if comparer == nil then
      comparer = Comparer_1(t.__genericT__).getDefault()
      compare = comparer.Compare 
    else
      compare = comparer.Compare
    end
    local lo = index
    local hi = index + count - 1
    while lo <= hi do
      local i = lo + div(hi - lo, 2)
      local item = t[i + 1]
      if item == null then item = nil end
      local order = compare(comparer, item, v);
      if order == 0 then return i end
      if order < 0 then
        lo = i + 1
      else
        hi = i - 1
      end
    end
    return -1
  end,
  ClearArray = function (t, index, length)
    if t == nil then throw(ArgumentNullException("array")) end
    if index < 0 or length < 0 or index + length > #t then
      throw(IndexOutOfRangeException())
    end
    local default = t.__genericT__:default()
    if default == nil then default = null end
    fill(t, index + 1, index + length, default)
  end,
  Contains = function (t, v)
    return indexOf(t, v) ~= -1
  end,
  Copy = function (t, ...)
    local len = select("#", ...)     
    if len == 2 then
      local array, length = ...
      copy(t, 0, array, 0, length)
    else 
      copy(t, ...)
    end
  end,
  CreateInstance = function (elementType, length)
    return buildArray(Array(elementType[1]), length)
  end,
  Empty = function (T)
    local t = emptys[T]
    if t == nil then
      t = Array(T)()
      emptys[T] = t
    end
    return t
  end,
  Exists = function (t, match)
    return findIndex(t, match) ~= -1
  end,
  Fill = function (t, value, startIndex, count)
    if t == nil then throw(ArgumentNullException("array")) end
    local len = #t
    if not startIndex then
      startIndex, count = 0, len
    else
      if startIndex < 0 or startIndex > len then
        throw(ArgumentOutOfRangeException("startIndex"))
      end
      if count < 0 or count > len - startIndex then
        throw(ArgumentOutOfRangeException("count"))
      end
    end
    fill(t, startIndex + 1, startIndex + count, value)
  end,
  Find = function (t, match)
    if t == nil then throw(ArgumentNullException("array")) end
    if match == nil then throw(ArgumentNullException("match")) end
    for i = 1, #t do
      local item = t[i]
      if item == null then item = nil end
      if match(item) then
        return item
      end
    end
    return t.__genericT__:default()
  end,
  FindAll = function (t, match)
    return setmetatable(findAll(t, match), Array(t.__genericT__))
  end,
  FindIndex = findIndex,
  FindLast = function (t, match)
    if t == nil then throw(ArgumentNullException("array")) end
    if match == nil then throw(ArgumentNullException("match")) end
    for i = #t, 1, -1 do
      local item = t[i]
      if item == null then item = nil end
      if match(item) then
        return item
      end
    end
    return t.__genericT__:default()
  end,
  FindLastIndex = function (t, startIndex, count, match)
    if t == nil then throw(ArgumentNullException("array")) end
    local len = #t
    if not count then
      startIndex, count, match = len - 1, len, startIndex
    elseif not match then
      count, match = startIndex + 1, count
    end
    if match == nil then throw(ArgumentNullException("match")) end
    if count < 0 or startIndex - count + 1 < 0 then
      throw(ArgumentOutOfRangeException("count"))
    end
    local endIndex = startIndex - count + 1
    for i = startIndex + 1, endIndex + 1, -1 do
      local item = t[i]
      if item == null then
        item = nil
      end
      if match(item) then
        return i - 1
      end
    end
    return -1
  end,
  ForEach = function (t, action)
    if action == nil then throw(ArgumentNullException("action")) end
    for i = 1, #t do
      local item = t[i]
      if item == null then item = nil end
      action(item)
    end
  end,
  IndexOf = indexOf,
  LastIndexOf = function (t, value, startIndex, count)
    if t == nil then throw(ArgumentNullException("array")) end
    local len = #t
    if not startIndex then
      startIndex, count = len - 1, len
    elseif not count then
      count = len == 0 and 0 or (startIndex + 1)
    end
    if len == 0 then
      if startIndex ~= -1 and startIndex ~= 0 then
        throw(ArgumentOutOfRangeException("startIndex"))
      end
      if count ~= 0 then
        throw(ArgumentOutOfRangeException("count"))
      end
    end
    if startIndex < 0 or startIndex >= len then
      throw(ArgumentOutOfRangeException("startIndex"))
    end
    if count < 0 or startIndex - count + 1 < 0 then
      throw(ArgumentOutOfRangeException("count"))
    end
    local comparer = EqualityComparer(t.__genericT__).getDefault()
    local equals = comparer.EqualsOf
    local endIndex = startIndex - count + 1
    for i = startIndex + 1, endIndex + 1, -1 do
      local item = t[i]
      if item == null then item = nil end
      if equals(comparer, item, value) then
        return i - 1
      end
    end
    return -1
  end,
  Resize = function (t, newSize, T)
    if newSize < 0 then throw(ArgumentOutOfRangeException("newSize")) end
    if t == nil then
      return buildArray(Array(T), newSize)
    end
    local len = #t
    if len > newSize then
      fill(t, newSize + 1, len, nil)
    elseif len < newSize then
      local default = t.__genericT__:default()
      if default == nil then default = null end
      fill(t, len + 1, newSize, default)
    end
    return t
  end,
  Reverse = function (t, index, count)
    if not index then
      index = 0
      count = #t
    else
      if count < 0 or index > #t - count then
        throw(ArgumentOutOfRangeException("index or count"))
      end
    end
    local i, j = index + 1, index + count
    while i <= j do
      t[i], t[j] = t[j], t[i]
      i = i + 1
      j = j - 1
    end
    versions[t] = (versions[t] or 0) + 1
  end,
  Sort = function (t, ...)
    if t == nil then throw(ArgumentNullException("array")) end
    local len = select("#", ...)
    if len == 0 then
      sort(t)
    elseif len == 1 then
      local comparer = ...
      sort(t, comparer)
    else
      local index, count, comparer = ...
      if count > 1 then
        local comp = getComp(t, comparer)
        if index == 0 and count == #t then
          tsort(t, comp)
        else
          checkIndexAndCount(t, index, count)
          local arr = {}
          tmove(t, index + 1, index + count, 1, arr)
          tsort(arr, comp)
          tmove(arr, 1, count, index + 1, t)
        end
        versions[t] = (versions[t] or 0) + 1
      end
    end
  end,
  toArray = function (t)
    local array = {}    
    if t.GetEnumerator == arrayEnumerator then
      tmove(t, 1, #t, 1, array)
    else
      local count = 1
      for _, v in each(t) do
        array[count] = v == nil and null or v
        count = count + 1
      end
    end
    return arrayFromTable(array, t.__genericT__)
  end,
  TrueForAll = function (t, match)
    if t == nil then throw(ArgumentNullException("array")) end
    if match == nil then throw(ArgumentNullException("match")) end
    for i = 1, #t do
      local item = t[i]
      if item == null then item = nil end
      if not match(item) then
        return false
      end
    end
    return true
  end,
  Clone = function (this)
    local t = setmetatable({}, getmetatable(this))
    tmove(this, 1, #this, 1, t)
    return t
  end,
  CopyTo = function (this, array, index)
    local n = #this
    checkIndexAndCount(array, index, n)
    local T = this.__genericT__
    if T.class == "S" then
      local default = T:default()
      if type(default) == "table" then
        for i = 1, n do
          array[i + index] = this[i]:__clone__()
        end
        return
      end
    end
    tmove(this, 1, n, index + 1, array)
  end,
  GetEnumerator = arrayEnumerator,
  GetLength = function (this, dimension)
    if dimension ~= 0 then throw(IndexOutOfRangeException()) end
    return #this
  end,
  GetLowerBound = function (this, dimension)
    if dimension ~= 0 then throw(IndexOutOfRangeException()) end
    return 0
  end,
  GetUpperBound = function (this, dimension)
    if dimension ~= 0 then throw(IndexOutOfRangeException()) end
    return #this - 1
  end,
  GetValue = function (this, index1, index2)
    if index1 == nil then throw(ArgumentNullException("indices")) end
    return get(this, checkArrayIndex(index1, index2))
  end,
  SetValue = function (this, value, index1, index2)
    if index1 == nil then throw(ArgumentNullException("indices")) end
    set(this, checkArrayIndex(index1, index2), System.castWithNullable(this.__genericT__, value))
  end,
  Clone = function (this)
    local array = {}
    tmove(this, 1, #this, 1, array)
    return arrayFromTable(array, this.__genericT__)
  end
}

function Array.__call(T, ...)
  return buildArray(T, select("#", ...), { ... })
end

function System.arrayFromList(t)
  return setmetatable(t, Array(t.__genericT__))
end

arrayFromTable = function (t, T, readOnly)
  assert(T)
  local array = setmetatable(t, Array(T))
  if readOnly then
    array.set = unset
  end
  return array
end

System.arrayFromTable = arrayFromTable

local function getIndex(t, ...)
  local rank = t.__rank__
  local id = 0
  local len = #rank
  for i = 1, len do
    id = id * rank[i] + select(i, ...)
  end
  return id, len
end

local function checkMultiArrayIndex(t, index1, ...)
  if index1 == nil then throw(ArgumentNullException("indices")) end
  local rank = t.__rank__
  local len = #rank
  if type(index1) == "table" then
    if #index1 ~= len then
      throw(ArgumentException("Indices length does not match the array rank."))
    end
    local id = 0
    for i = 1, len do
      id = id * rank[i] + index1[i]
    end
    return id
  elseif len ~= select("#", ...) + 1 then
    throw(ArgumentException("Indices length does not match the array rank."))
  end
  return getIndex(t, index1, ...)
end

local MultiArray = { 
  set = function (this, ...)
    local index, len = getIndex(this, ...)
    set(this, index, select(len + 1, ...))
  end,
  get = function (this, ...)
    local index = getIndex(this, ...)
    return get(this, index)
  end,
  getRank = function (this)
    return #this.__rank__
  end,
  GetLength = function (this, dimension)
    local rank = this.__rank__
    if dimension < 0 or dimension >= #rank then throw(IndexOutOfRangeException()) end
    return rank[dimension + 1]
  end,
  GetLowerBound = function (this, dimension)
    local rank = this.__rank__
    if dimension < 0 or dimension >= #rank then throw(IndexOutOfRangeException()) end
    return 0
  end,
  GetUpperBound = function (this, dimension)
    local rank = this.__rank__
    if dimension < 0 or dimension >= #rank then throw(IndexOutOfRangeException()) end
    return rank[dimension + 1] - 1
  end,
  GetValue = function (this, ...)
    return get(this, checkMultiArrayIndex(this, ...))
  end,
  SetValue = function (this, value, ...)
    set(this, checkMultiArrayIndex(this, ...), System.castWithNullable(this.__genericT__, value))
  end,
  Clone = function (this)
    local array = { __rank__ = this.__rank__ }
    tmove(this, 1, #this, 1, array)
    return arrayFromTable(array, this.__genericT__)
  end
}

function MultiArray.__call(T, rank, t)
  local len = 1
  for i = 1, #rank do
    len = len * rank[i]
  end
  t = buildArray(T, len, t)
  t.__rank__ = rank
  return t
end

System.defArray("System.Array", function(T) 
  return { 
    base = { System.ICloneable, System.IList_1(T), System.IReadOnlyList_1(T), System.IList }, 
    __genericT__ = T
  }
end, Array, MultiArray)

local cpool = {}
local function createCoroutine(f)
  local c = tremove(cpool)
  if c == nil then
    c = ccreate(function (...)
      f(...)
      while true do
        f = nil
        cpool[#cpool + 1] = c
        f = cyield(cpool)
        f(cyield())
      end
    end)
  else
    cresume(c, f)
  end
  return c
end

System.ccreate = createCoroutine
System.cpool = cpool
System.cresume = cresume
System.yield = cyield

local YieldEnumerable
YieldEnumerable = define("System.YieldEnumerable", function (T)
  return {
    base = { System.IEnumerable_1(T), System.IEnumerator_1(T), System.IDisposable },
    __genericT__ = T
  }
end, {
  getCurrent = System.getCurrent, 
  Dispose = System.emptyFn,
  GetEnumerator = function (this)
    return setmetatable({ f = this.f, args = this.args }, YieldEnumerable(this.__genericT__))
  end,
  MoveNext = function (this)
    local c = this.c
    if c == false then
      return false
    end
  
    local ok, v
    if c == nil then
      c = createCoroutine(this.f)
      this.c = c
      local args = this.args
      ok, v = cresume(c, unpack(args, 1, args.n))
      this.args = nil
    else
      ok, v = cresume(c)
    end
  
    if ok then
      if v == cpool then
        this.c = false
        this.current = nil
        return false
      else
        this.current = v
        return true
      end
    else
      error(v)
    end
  end
})

local function yieldIEnumerable(f, T, ...)
  return setmetatable({ f = f, args = pack(...) }, YieldEnumerable(T))
end

System.yieldIEnumerable = yieldIEnumerable
System.yieldIEnumerator = yieldIEnumerable

local ReadOnlyCollection = {
  __ctor__ = function (this, list)
    if not list then throw(ArgumentNullException("list")) end
    this.list = list
  end,
  getCount = function (this)
    return #this.list
  end,
  get = function (this, index)
    return this.list:get(index)
  end,
  Contains = function (this, value)
    return this.list:Contains(value)
  end,
  GetEnumerator = function (this)
    return this.list:GetEnumerator()
  end,
  CopyTo = function (this, array, index)
    this.list:CopyTo(array, index)
  end,
  IndexOf = function (this, value)
    return this.list:IndexOf(value)
  end,
  getIsSynchronized = falseFn,
  getIsReadOnly = trueFn,
  getIsFixedSize = trueFn,
}

define("System.ReadOnlyCollection", function (T)
  return { 
    base = { System.IList_1(T), System.IList, System.IReadOnlyList_1(T) }, 
    __genericT__ = T
  }
end, ReadOnlyCollection)
