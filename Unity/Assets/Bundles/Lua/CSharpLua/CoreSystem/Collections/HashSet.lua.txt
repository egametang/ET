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
local throw = System.throw
local each = System.each
local Dictionary = System.Dictionary
local wrap = System.wrap
local unWrap = System.unWrap
local getEnumerator = Dictionary.GetEnumerator 
local ArgumentNullException = System.ArgumentNullException

local assert = assert
local pairs = pairs
local select = select

local counts = System.counts

local function build(this, collection, comparer)
  if comparer ~= nil then
    assert(false)
  end
  if collection == nil then
    throw(ArgumentNullException("collection"))
  end
  this:UnionWith(collection)
end

local function checkUniqueAndUnfoundElements(this, other, returnIfUnfound)
  if #this == 0 then
    local numElementsInOther = 0
    for _, item in each(other) do
      numElementsInOther = numElementsInOther + 1
      break
    end
    return 0, numElementsInOther
  end
  local set, uniqueCount, unfoundCount = {}, 0, 0
  for _, item in each(other) do
    item = wrap(item)
      if this[item] ~= nil then
        if set[item] == nil then
          set[item] = true
          uniqueCount = uniqueCount + 1
        end
      else
      unfoundCount = unfoundCount + 1
      if returnIfUnfound then
        break
      end
    end
  end
  return uniqueCount, unfoundCount
end

local HashSet = {
  __ctor__ = function (this, ...)
    local len = select("#", ...)
    if len == 0 then
    elseif len == 1 then
      local collection = ...
      if collection == nil then return end
      if collection.getEnumerator ~= nil then
        build(this, collection, nil)
      else
        assert(true)
      end
    else 
      build(this, ...)
    end
  end,
  Clear = Dictionary.Clear,
  getCount = Dictionary.getCount,
  getIsReadOnly = System.falseFn,
  Contains = function (this, item)
    item = wrap(item)
    return this[item] ~= nil
  end,
  Remove = function (this, item)
    item = wrap(item)
    if this[item] then
      this[item] = nil
      local t = counts[this]
      t[1] = t[1] - 1
      t[2] = t[2] + 1
      return true
    end
    return false
  end,
  GetEnumerator = function (this)
    return getEnumerator(this, 1)
  end,
  Add = function (this, v)
    v = wrap(v)
    if this[v] == nil then
      this[v] = true
      local t = counts[this]
      if t then
        t[1] = t[1] + 1
        t[2] = t[2] + 1
      else
        counts[this] = { 1, 1 }
      end
      return true
    end
    return false
  end,
  UnionWith = function (this, other)
    if other == nil then
      throw(ArgumentNullException("other"))
    end
    local count = 0
    for _, v in each(collection) do
      v = wrap(v)
      if this[v] == nil then
        this[v] = true
        count = count + 1
      end
    end
    if count > 0 then
      local t = counts[this]
      if t then
        t[1] = t[1] + count
        t[2] = t[2] + 1
      else
        counts[this] = { count, 1 }  
      end
    end
  end,
  IntersectWith = function (this, other)
    if other == nil then
      throw(ArgumentNullException("other"))
    end
    local set = {}
    for _, v in each(other) do
      v = wrap(v)
      if this[v] ~= nil then
        set[v] = true
      end
    end
    local count = 0
    for v, _ in pairs(this) do
      if set[v] == nil then
        this[v] = nil
        count = count + 1
      end
    end
    if count > 0 then
      local t = counts[this]
      t[1] = t[1] - count
      t[2] = t[2] + 1
    end
  end,
  ExceptWith = function (this, other)
    if other == nil then
      throw(ArgumentNullException("other"))
    end
    if other == this then
      this:Clear()
      return
    end
    local count = 0
    for _, v in each(other) do
      v = wrap(v)
      if this[v] ~= nil then
        this[v] = nil
        count = count + 1
      end
    end
    if count > 0 then
      local t = counts[this]
      t[1] = t[1] - count
      t[2] = t[2] + 1
    end
  end,
  SymmetricExceptWith = function (this, other)
    if other == nil then throw(ArgumentNullException("other")) end
    if other == this then
      this:Clear()
      return
    end
    local set = {}
    local count = 0
    local changed = false
    for _, v in each(other) do
      v = wrap(v)
      if this[v] == nil then
        this[v] = true
        count = count + 1
        changed = true
        set[v] = true
      elseif set[v] == nil then 
        this[v] = nil
        count = count - 1
        changed = true
      end
    end
    if changed then
      local t = counts[this]
      if t then
        t[1] = t[1] + count
        t[2] = t[2] + 1
      else
        counts[this] = { count, 1 }
      end
    end
  end,
  IsSubsetOf = function (this, other)
    if other == nil then
      throw(ArgumentNullException("other"))
    end
    local count = #this
    if count == 0 then
      return true
    end
    local uniqueCount, unfoundCount = checkUniqueAndUnfoundElements(this, other, false)
    return uniqueCount == count and unfoundCount >= 0
  end,
  IsProperSubsetOf = function (this, other)
    if other == nil then
      throw(ArgumentNullException("other"))
    end
    local uniqueCount, unfoundCount = checkUniqueAndUnfoundElements(this, other, false)
    return uniqueCount == #this and unfoundCount > 0
  end,
  IsSupersetOf = function (this, other)
    if other == nil then
      throw(ArgumentNullException("other"))
    end
    for _, element in each(other) do
      element = wrap(element)
      if this[element] == nil then
        return false
      end
    end
    return true
  end,
  IsProperSupersetOf = function (this, other)
    if other == nil then
      throw(ArgumentNullException("other"))
    end
    local count = #this
    if count == 0 then
      return false
    end
    local uniqueCount, unfoundCount = checkUniqueAndUnfoundElements(this, other, true)
    return uniqueCount < count and unfoundCount == 0
  end,
  Overlaps = function (this, other)
    if other == nil then
      throw(ArgumentNullException("other"))
    end
    if #this == 0 then
      return false
    end
    for _, element in each(other) do
      element = wrap(element)
      if this[element] ~= nil then
        return true
      end
    end
    return false
  end,
  SetEquals = function (this, other)
    if other == nil then
      throw(ArgumentNullException("other"))
    end
    local uniqueCount, unfoundCount = checkUniqueAndUnfoundElements(this, other, true)
    return uniqueCount == #this and unfoundCount == 0
  end,
  RemoveWhere = function (this, match)
    if match == nil then
      throw(ArgumentNullException("match"))
    end
    local numRemoved = 0
    for v, _ in pairs(this) do
      if match(unWrap(v)) then
        this[v] = nil
        numRemoved = numRemoved + 1
      end
    end
    if numRemoved > 0 then
      local t = counts[this]
      t[1] = t[1] - numRemoved
      t[2] = t[2] + 1
    end
    return numRemoved
  end,
  TrimExcess = System.emptyFn
}

function System.hashSetFromTable(t, T)
  return setmetatable(t, HashSet(T))
end

System.HashSet = System.define("System.Collections.Generic.HashSet", function(T) 
  return { 
    base = { System.ICollection_1(T), System.ISet_1(T) }, 
    __genericT__ = T,
    __genericTKey__ = T,
    __len = HashSet.getCount
  }
end, HashSet)
