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
local bnot = System.bnot
local trueFn = System.trueFn
local falseFn = System.falseFn
local lengthFn = System.lengthFn
local er = System.er

local InvalidOperationException = System.InvalidOperationException
local NullReferenceException = System.NullReferenceException
local ArgumentException = System.ArgumentException
local ArgumentNullException = System.ArgumentNullException
local ArgumentOutOfRangeException = System.ArgumentOutOfRangeException
local IndexOutOfRangeException = System.IndexOutOfRangeException
local NotSupportedException = System.NotSupportedException
local EqualityComparer = System.EqualityComparer
local Comparer_1 = System.Comparer_1
local IEnumerable_1 = System.IEnumerable_1
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

local function clear(t)
  local size = #t
  if size > 0 then
    for i = 1, size do
      t[i] = nil
    end
    versions[t] = (versions[t] or 0) + 1
  end
end

local function first(t)
  local v = t[1]
  if v == nil then throw(InvalidOperationException())  end
  if v ~= null then
    return v
  end
  return nil
end

local function last(t)
  local n = #t
  if n == 0 then throw(InvalidOperationException()) end
  local v = t[n]
  if v ~= null then
    return v
  end
  return nil
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

local function binarySearch(t, ...)
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
  if type(comparer) == "function" then
    compare = comparer
  elseif comparer == nil then
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
  return bnot(lo)
end

local function getSortComp(t, comparer)
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
    tsort(t, getSortComp(t, comparer))
    versions[t] = (versions[t] or 0) + 1
  end
end

local function addOrder(t, v, comparer)
  local i = binarySearch(t, v, comparer)
  if i >= 0 then return false end
  tinsert(t, bnot(i) + 1, v == nil and null or v)
  return true
end

local function addOrderRange(t, collection)
  if collection == nil then throw(ArgumentNullException("collection")) end
  local comparer, n = t.comparer, #t
  if collection.GetEnumerator == arrayEnumerator then
    for i = 1, #collection do
      local v = collection[i]
      if v == null then v = nil end
      addOrder(t, v, comparer)
    end
  else
    for _, v in each(collection) do
      addOrder(t, v, comparer)
    end
  end
  if #t ~= n then
    versions[t] = (versions[t] or 0) + 1
  end
end

local function exceptWithOrder(t, other)
  if other == nil then throw(ArgumentNullException("other")) end
  if #t == 0 then return end
  if t == other then
    clear(t)
    return
  end
  local comparer, n = t.comparer, #t
  if other.GetEnumerator == arrayEnumerator then
    for i = 1, n do
      local v = t[i]
      if v == null then v = nil end
      local i = binarySearch(t, v, comparer)
      if i >= 0 then
        tremove(t, i + 1)
      end
    end
  else
    for _, v in each(other) do
      local i = binarySearch(t, v, comparer)
      if i >= 0 then
        tremove(t, i + 1)
      end
    end
  end
  if #t ~= n then
    versions[t] = (versions[t] or 0) + 1
  end
end

local function intersectWithOrder(t, other)
  if other == nil then throw(ArgumentNullException("other")) end
  if #t == 0 then return end
  if t == other then return end
  local comparer = t.comparer
  local array = arrayFromTable({}, t.__genericT__)
  for _, v in each(other) do
    local i = binarySearch(t, v, comparer)
    if i >= 0 then
      array[#array + 1] = v == nil and null or v
    end
  end
  clear(t)
  addOrderRange(t, array)
end

local function isSubsetOfOrderWithSameEC(t, n, comparer, set)
  for i = 1, n do
    local v = t[i]
    if v == null then v = nil end
    local j = binarySearch(set, v, comparer)
    if j < 0 then
      return false
    end
  end
  return true
end

local function checkOrderUniqueAndUnfoundElements(t, n, comparer, other, returnIfUnfound)
  if n == 0 then
    local numElementsInOther = 0
    for _, v in each(other) do
      numElementsInOther = numElementsInOther + 1
      break
    end
    return 0, numElementsInOther
  end
  local uniqueFoundCount, unfoundCount, mark = 0, 0, {}
  for _, v in each(other) do
    local i = binarySearch(t, v, comparer)
    if i >= 0 then
      if not mark[i] then
        mark[i] = true
        uniqueFoundCount = uniqueFoundCount + 1
      end
    else
      unfoundCount = unfoundCount + 1
      if returnIfUnfound then
        break
      end
    end
  end
  return uniqueFoundCount, unfoundCount
end

local function isProperSubsetOfOrder(t, other)
  if other == nil then throw(ArgumentNullException("other")) end
  if t == other then return false end
  local comparer, n = t.comparer, #t
  if System.is(other, System.SortedSet(t.__genericT__)) then
    if comparer == other.comparer then
      if n >= #other then return false end
      return isSubsetOfOrderWithSameEC(t, n, comparer, set)
    end
  end
  local uniqueCount, unfoundCount = checkOrderUniqueAndUnfoundElements(t, n, comparer, other)
  return uniqueCount == n and unfoundCount > 0
end

local function isProperSupersetOfOrder(t, other)
  if other == nil then throw(ArgumentNullException("other")) end
  local n = #t
  if n == 0 then return false end
  if System.is(other, System.ICollection) then
    if other:getCount() == 0 then
      return true
    end
  end
  local comparer = t.comparer
  if System.is(other, System.SortedSet(t.__genericT__)) then
    if comparer == other.comparer then
      local n1 = #other
      if n1 >= n then return false end
      return isSubsetOfOrderWithSameEC(other, n1, comparer, t)
    end
  end
  local uniqueCount, unfoundCount = checkOrderUniqueAndUnfoundElements(t, n, comparer, other, true)
  return uniqueCount < n and unfoundCount == 0
end

local function isSubsetOfOrder(t, other)
  if other == nil then throw(ArgumentNullException("other")) end
  local n = #t
  if n == 0 then return true end
  local comparer = t.comparer
  if System.is(other, System.SortedSet(t.__genericT__)) then
    if comparer == other.comparer then
      if n > #other then return false end
      return isSubsetOfOrderWithSameEC(t, n, comparer, set)
    end
  end
  local uniqueCount, unfoundCount = checkOrderUniqueAndUnfoundElements(t, n, comparer, other)
  return uniqueCount == n and unfoundCount >= 0
end

local function isSupersetOfOrder(t, other)
  if other == nil then throw(ArgumentNullException("other")) end
  if System.is(other, System.ICollection) then
    if other:getCount() == 0 then
      return true
    end
  end
  local comparer, n = t.comparer, #t
  if System.is(other, System.SortedSet(t.__genericT__)) then
    if comparer == other.comparer then
      local n1 = #other
      if n < n1 then return false end
      return isSubsetOfOrderWithSameEC(other, n1, comparer, t)
    end
  end
  for _, v in each(other) do
    local i = binarySearch(t, v, comparer)
    if i < 0 then
      return false
    end
  end
  return true
end

local function isOverlapsOrder(t, other)
  if other == nil then throw(ArgumentNullException("other")) end
  local n = #t
  if n == 0 then return false end
  if System.is(other, System.ICollection) and other:getCount() == 0 then return false end
  local comparer = t.comparer
  if System.is(other, System.SortedSet(t.__genericT__)) and comparer == other.comparer then
    local c = comparer.Compare
    if c(comparer, first(t), last(other)) > 0 or c(comparer, last(t), first(other)) < 0 then
      return false
    end
  end
  for _, v in each(other) do
    local i = binarySearch(t, v, comparer)
    if i >= 0 then
      return true
    end
  end
  return false
end

local function equalsOrder(t, other)
  if other == nil then throw(ArgumentNullException("other")) end
  local comparer, n = t.comparer, #t
  if System.is(other, System.SortedSet(t.__genericT__)) and comparer == other.comparer then
    local n1 = #other
    if n ~= n1 then return false end
    local c = comparer.Compare
    for i = 1, n do
      local v, v1 = t[i], other[i]
      if v == null then v = nil end
      if v1 == null then v1 = nil end
      if c(comparer, v, v1) ~= 0 then
        return false
      end
    end
    return true
  end
  local uniqueCount, unfoundCount = checkOrderUniqueAndUnfoundElements(t, n, comparer, other, true)
  return uniqueCount == n and unfoundCount == 0
end

local function symmetricExceptWithOrder(t, other)
  if other == nil then throw(ArgumentNullException("other")) end
  local n = #t
  if n == 0 then
    addOrderRange(other)
    return
  end
  if t == other then
    clear(t)
    return
  end
  for _, v in each(other) do
    local i = binarySearch(t, v, comparer)
    if i >= 0 then
      tremove(t, i + 1)
    else
      addOrder(t, v, comparer)
    end
  end
end

local function tryGetValueOrder(t, equalValue)
  local i = binarySearch(t, equalValue, t.comparer)
  if i >= 0 then
    local v = t[i + 1]
    if v == null then v = nil end
    return true, v
  end
  return false, t.__genericT__:default()
end

local function checkOrderDictKeyValueObj(t, k, v)
  if k == nil then throw(ArgumentNullException("key")) end
  local TValue = t.__genericTValue__
  if v == nil and TValue:default() ~= nil then throw(ArgumentNullException("value")) end
  local TKey = t.__genericTKey__
  if not System.is(k, TKey) then 
    throw(ArgumentException(er.Arg_WrongType(k, TKey.__name__), "key")) 
  end
  if not System.is(v, TValue) then
    throw(ArgumentException(er.Arg_WrongType(v, TValue.__name__), "value")) 
  end
end

local function getOrderDictIndex(t, k, isObj)
  if k == nil then throw(ArgumentNullException("key")) end
  if isObj and not System.is(k, t.__genericTKey__) then
    return -1
  end
  return binarySearch(t, k, t.keyComparer)
end

local function addOrderDict(t, k, v, keyComparer, T, version)
  local i = binarySearch(t, k, keyComparer)
  if i >= 0 then
    throw(ArgumentException(er.Argument_AddingDuplicate(k)))
  end
  tinsert(t, bnot(i) + 1, setmetatable({ k, v }, T))
  if version then
    versions[t] = (versions[t] or 0) + 1
  end
end

local function getOrderDict(t, k, isObj)
  local i = getOrderDictIndex(t, k)
  if i >= 0 then
    return t[i + 1][2]
  end
  if isObj then
    return nil
  end
  throw(System.KeyNotFoundException(er.Arg_KeyNotFoundWithKey(k)))
end

local function setOrderDict(t, k, v)
  local i = getOrderDictIndex(t, k)
  if i >= 0 then
    t[i + 1][2] = v
  else
    tinsert(t, bnot(i) + 1, setmetatable({ k, v }, t.__genericT__))
    versions[t] = (versions[t] or 0) + 1
  end
end

local function getViewBetweenOrder(t, lowerValue, upperValue)
  local comparer = t.comparer
  if comparer:Compare(lowerValue, upperValue) > 0 then
    throw(ArgumentException("lowerBound is greater than upperBound"))
  end
  local n = #t
  local i = binarySearch(t, lowerValue, comparer)
  if i < 0 then i = bnot(i) end
  local set = { comparer = comparer }
  if i < n then
    local j = binarySearch(t, upperValue, comparer)
    if j < 0 then j = bnot(j) - 1 end
    if i <= j then
      tmove(t, i + 1, j + 1, 1, set)
    end
  end
  return setmetatable(set, System.SortedSet(t.__genericT__))
end

local function heapDown(t, k, n, c)
  local j
  while true do
    j = k * 2
    if j <= n and j > 0 then
      if j < n and c(t[j], t[j + 1]) > 0 then
        j = j + 1
      end
      if c(t[k], t[j]) <= 0 then
        break
      end
      t[j], t[k] = t[k], t[j]
      k = j
    else
      break
    end
  end
end

local function heapUp(t, k, n, c)
  while k > 1 do
    local j = div(k, 2)
    if c(t[j], t[k]) <= 0 then
      break
    end
    t[j], t[k] = t[k], t[j]
    k = j
  end
end

local function heapify(t, c)
  local n = #t
  for i = div(n, 2), 1, -1 do
    heapDown(t, i, n, c)  
  end
end

local function heapAdd(t, v, c)
  local n = #t + 1
  t[n] = v
  heapUp(t, n, n, c)
end

local function heapPop(t, c)
  local n = #t
  if n == 0 then return end
  local v = t[1]
  t[1] = t[n]
  t[n] = nil
  heapDown(t, 1, n - 1, c)
  return v
end

local SortedSetEqualityComparerFn
local SortedSetEqualityComparer = {
  __ctor__ = function (this, equalityComparer, comparer)
    local T = this.__genericT__
    this.comparer = comparer or Comparer_1(T).getDefault()
    this.equalityComparer = equalityComparer or EqualityComparer(T).getDefault()
  end,
  EqualsOf = function (this, x, y)
    if x == nil then return y == nil end
    if y == nil then return false end
    return equalsOrder(x, y)
  end,
  GetHashCodeOf = function (this, set)
    local hashCode = 0
    if set ~= nil then
      for i = 1, #set do
        local v = set[i]
        if v == null then v = nil end
        hashCode = System.xor(hashCode, System.band(this.equalityComparer:GetHashCodeOf(v), 0x7FFFFFFF))
      end
    end
    return hashCode
  end,
  EqualsObj = function (this, obj)
    if System.is(obj, SortedSetEqualityComparerFn(this.__genericT__)) then
      return this.comparer == obj.comparer
    end
    return false
  end,
  GetHashCode = function (this)
    return System.xor(this.comparer:GetHashCode(), this.equalityComparer:GetHashCode())
  end
}

SortedSetEqualityComparerFn = define("System.SortedSetEqualityComparer", function (T)
  return {
    base = { System.IEqualityComparer_1(T) },
    __genericT__ = T
  }
end, SortedSetEqualityComparer, 1)

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
}, 1)

arrayEnumerator = function (t, T)
  if not T then T = t.__genericT__ end
  return setmetatable({ list = t, index = 1, version = versions[t], currnet = T:default() }, ArrayEnumerator(T))
end

local ArrayReverseEnumerable 

local function reverseEnumerator(t)
  local T = t.__genericT__
  return setmetatable({ list = t, index = #t, version = versions[t], currnet = T:default() }, ArrayReverseEnumerable(T))
end

ArrayReverseEnumerable = define("System.ArrayReverseEnumerable", function (T)
  return {
    base = { IEnumerable_1(T), IEnumerator_1(T) }
  }
end, {
  getCurrent = System.getCurrent, 
  Dispose = System.emptyFn,
  GetEnumerator = function (this)
    return reverseEnumerator(this.list)
  end,
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
}, 1)

local function reverseEnumerable(t)
  return setmetatable({ list = t }, ArrayReverseEnumerable(t.__genericT__))
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
  setCapacity = function (t, len)
    if len < #t then throw(ArgumentOutOfRangeException("Value", er.ArgumentOutOfRange_SmallCapacity())) end
  end,
  ctorList = function (t, ...)
    local n = select("#", ...)
    if n == 0 then return end
    local collection = ...
    if type(collection) == "number" then return end
    addRange(t, collection)
  end,
  ctorOrderSet = function(t, ...)
    local n = select("#", ...)
    if n == 0 then
      t.comparer = Comparer_1(t.__genericT__).getDefault()
    else
      local collection, comparer = ...
      if collection == nil then throw(ArgumentNullException("collection")) end
      t.comparer = comparer or Comparer_1(t.__genericT__).getDefault()
      if collection then
        addOrderRange(t, collection)
      end
    end
  end,
  ctorOrderDict = function (t, ...)
    local n = select("#", ...)
    local dictionary, comparer
    if n ~= 0 then
      dictionary, comparer = ...
      if dictionary == nil then throw(ArgumentNullException("dictionary")) end
    end
    if comparer == nil then comparer = Comparer_1(t.__genericT__).getDefault() end
    local c = comparer.Compare
    local keyComparer = function (_, p, v)
      return c(comparer, p[1], v)
    end
    t.comparer, t.keyComparer = comparer, keyComparer
    if type(dictionary) == "table" then
      local T = t.__genericT__
      for _, p in each(dictionary) do
        local k, v = p[1], p[2]
        addOrderDict(t, k, v, keyComparer, T)
      end
    end
  end,
  add = add,
  addObj = function (this, item)
    if not System.is(item, this.__genericT__) then
      throw(ArgumentException())
    end
    return add(this, item)
  end,
  addRange = addRange,
  addOrder = function (t, v)
    local success = addOrder(t, v, t.comparer)
    if success then
      versions[t] = (versions[t] or 0) + 1
    end
    return success
  end,
  addOrderRange = addOrderRange,
  addOrderDict = function (t, k, v)
    if k == nil then throw(ArgumentNullException("key")) end
    addOrderDict(t, k, v, t.keyComparer, t.__genericT__, true)
  end,
  addPairOrderDict = function (t, ...)
    local k, v
    if select("#", ...) == 1 then
      local pair = ... 
      k, v = pair[1], pair[2]
    else
      k, v = ...
    end
    if k == nil then throw(ArgumentNullException("key")) end
    addOrderDict(t, k ,v, t.keyComparer, t.__genericT__, true)
  end,
  addOrderDictObj = function (t, k, v)
    checkOrderDictKeyValueObj(t, k, v)
    addOrderDict(t, k ,v, t.keyComparer, t.__genericT__, true)
  end,
  AsReadOnly = function (t)
    return System.ReadOnlyCollection(t.__genericT__)(t)
  end,
  clear = clear,
  containsOrder = function (t, v)
    return binarySearch(t, v, t.comparer) >= 0
  end,
  containsOrderDict = function (t, k)
    return getOrderDictIndex(t, k) >= 0
  end,
  containsOrderDictObj = function (t, k)
    return getOrderDictIndex(t, k, true) >= 0
  end,
  createSetComparer = function (T, equalityComparer)
    return SortedSetEqualityComparerFn(T)(equalityComparer)
  end,
  exceptWithOrder = exceptWithOrder,
  findAll = function (t, match)
    return setmetatable(findAll(t, match), System.List(t.__genericT__))
  end,
  first = first,
  firstOrDefault = function (t)
    local v = t[1]
    if v == nil then
      return t.__genericT__:default()
    elseif v == null then
      return nil
    else
      return v
    end
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
  getViewBetweenOrder = getViewBetweenOrder,
  getOrderComparer = function (t) return t.comparer end,
  intersectWithOrder = intersectWithOrder,
  isProperSubsetOfOrder = isProperSubsetOfOrder,
  isProperSupersetOfOrder = isProperSupersetOfOrder,
  isSubsetOfOrder = isSubsetOfOrder,
  isSupersetOfOrder = isSupersetOfOrder,
  isOverlapsOrder = isOverlapsOrder,
  equalsOrder = equalsOrder,
  symmetricExceptWithOrder = symmetricExceptWithOrder,
  heapDown = heapDown,
  heapUp = heapUp,
  heapify = heapify,
  heapAdd = heapAdd,
  heapPop = heapPop,
  last = last,
  lastOrDefault = function (t)
    local n = #t
    local v = t[n]
    if v == nil then
      return t.__genericT__:default()
    elseif v == null then
      return nil
    else
      return v
    end
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
    index = index + 1
    if t[index] == nil then throw(ArgumentOutOfRangeException("index"))  end
    tremove(t, index)
    versions[t] = (versions[t] or 0) + 1
  end,
  removeOrder = function (t, v)
    local i = binarySearch(t, v, t.comparer)
    if i >= 0 then
      tremove(t, i + 1)
      versions[t] = (versions[t] or 0) + 1
      return true
    end
    return false
  end,
  removeOrderDict = function (t, k)
    local i = getOrderDictIndex(t, k)
    if i >= 0 then
      tremove(t, i + 1)
      versions[t] = (versions[t] or 0) + 1
      return true
    end
    return false
  end,
  removePairOrderDict = function (t, p)
    local i = getOrderDictIndex(t, p[1])
    if i >= 0 then
      local v = t[i + 1][2]
      local comparer = EqualityComparer(this.__genericTValue__).getDefault()
      if comparer:EqualsOf(p[2], v) then
        tremove(this, i)
        return true
      end
    end
    return false
  end,
  tryGetValueOrder = tryGetValueOrder,
  tryGetValueOrderDict = function (t, k)
    local i = getOrderDictIndex(t, k)
    if i >= 0 then
      local p = t[i + 1]
      return true, p[2]
    end
    return false, t.__genericTValue__:default()
  end,
  getRange = function (t, index, count)
    if count < 0 or index > #t - count then
      throw(ArgumentOutOfRangeException("index or count"))
    end
    local list = {}
    tmove(t, index + 1, index + count, 1, list)
    return setmetatable(list, System.List(t.__genericT__))
  end,
  getOrderDict = getOrderDict,
  getOrderDictObj = function (t, k)
    return getOrderDict(t, k, true)
  end,
  setOrderDict = setOrderDict,
  setOrderDictObj = function (t, k, v)
    checkOrderDictKeyValueObj(t, k, v)
    setOrderDict(t, k, v)
  end,
  indexKeyOrderDict = function (t, k)
    local i = getOrderDictIndex(t, k)
    if i < 0 then i = -1 end
    return i
  end, 
  indexOfValue = function (t, v)
    local len = #t
    if len > 0 then
      local comparer = EqualityComparer(t.__genericTValue__).getDefault()
      local equals = comparer.EqualsOf
      for i = 1, len do
        if equals(comparer, v, t[i][2]) then
          return i - 1
        end
      end
    end
    return -1
  end,
  reverseEnumerator = reverseEnumerator,
  reverseEnumerable = reverseEnumerable,
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
  BinarySearch = binarySearch,
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
      t = Array(T){}
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
    while i < j do
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
        local comp = getSortComp(t, comparer)
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
  CopyTo = function (t, ...)
    local n = select("#", ...)
    local sourceIndex, array, arrayIndex, len
    if n == 1 then
      sourceIndex, arrayIndex, len, array = 0, 0, #t, ...
      checkIndexAndCount(array, 0, len)
    elseif n == 2 then
      sourceIndex, len, array, arrayIndex = 0, #t, ...
      checkIndexAndCount(array, arrayIndex, len)
    elseif n == 3 then
      sourceIndex, array, arrayIndex, len = 0, ...
      checkIndexAndCount(t, sourceIndex, len)
      checkIndexAndCount(array, arrayIndex, len)
    else
      sourceIndex, array, arrayIndex, len = ...
      checkIndexAndCount(t, sourceIndex, len)
      checkIndexAndCount(array, arrayIndex, len)
    end

    local T = t.__genericT__
    if T.class == "S" then
      local default = T:default()
      if type(default) == "table" then
        for i = 1, len do
          array[arrayIndex + i] = t[sourceIndex + i]:__clone__()
        end
        return
      end
    end

    tmove(t, sourceIndex + 1, sourceIndex + len, arrayIndex + 1, array)
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

function Array.__call(ArrayT, n, t)
  if type(n) == "table" then
    t = n
  elseif t ~= nil then
    for i = 1, n  do
      if t[i] == nil then
        t[i] = null
      end
    end
  else
    t = {}
    if n > 0 then
      local T = ArrayT.__genericT__
      local default = T:default()
      if default == nil then
        fill(t, 1, n, null)
      elseif type(default) ~= "table" then
        fill(t, 1, n, default)
      else
        for i = 1, n do
          t[i] = T:default()
        end
      end
    end
  end
  return setmetatable(t, ArrayT)
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
    base = { IEnumerable_1(T), IEnumerator_1(T), System.IDisposable },
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
}, 1)

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
end, ReadOnlyCollection, 1)
