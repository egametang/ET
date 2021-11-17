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
local each = System.each
local identityFn = System.identityFn
local wrap = System.wrap
local unWrap = System.unWrap
local is = System.is
local cast = System.cast
local Int32 = System.Int32
local isArrayLike = System.isArrayLike
local isDictLike = System.isDictLike
local Array = System.Array
local arrayEnumerator = Array.GetEnumerator

local NullReferenceException = System.NullReferenceException
local ArgumentNullException = System.ArgumentNullException
local ArgumentOutOfRangeException = System.ArgumentOutOfRangeException
local InvalidOperationException = System.InvalidOperationException
local EqualityComparer = System.EqualityComparer
local Comparer_1 = System.Comparer_1
local Empty = System.Array.Empty

local IEnumerable_1 = System.IEnumerable_1
local IEnumerable = System.IEnumerable
local IEnumerator_1 = System.IEnumerator_1
local IEnumerator = System.IEnumerator

local assert = assert
local getmetatable = getmetatable
local setmetatable = setmetatable
local select = select
local pairs = pairs
local tsort = table.sort

local InternalEnumerable = define("System.Linq.InternalEnumerable", function(T) 
  return {
    base = { IEnumerable_1(T) }
  }
end, nil, 1)

local function createEnumerable(T, GetEnumerator)
  assert(T)
  return setmetatable({ __genericT__ = T, GetEnumerator = GetEnumerator }, InternalEnumerable(T))
end

local InternalEnumerator = define("System.Linq.InternalEnumerator", function(T) 
  return {
    base = { IEnumerator_1(T) }
  }
end, nil, 1)

local function createEnumerator(T, source, tryGetNext, init)
  assert(T)
  local state = 1
  local current
  local en
  return setmetatable({
    MoveNext = function()
      if state == 1 then
        state = 2
        if source then
          en = source:GetEnumerator() 
        end
        if init then
          init(en) 
        end
      end
      if state == 2 then
        local ok, v = tryGetNext(en)
        if ok then
          current = v
          return true
        elseif en then
          local dispose = en.Dispose
          if dispose then
            dispose(en)
          end    
        end
       end
       return false
    end,
    getCurrent = function()
      return current
    end
  }, InternalEnumerator(T))
end

local Enumerable = {}
define("System.Linq.Enumerable", Enumerable)

function Enumerable.Where(source, predicate)
  if source == nil then throw(ArgumentNullException("source")) end
  if predicate == nil then throw(ArgumentNullException("predicate")) end
  local T = source.__genericT__
  return createEnumerable(T, function() 
    local index = -1
    return createEnumerator(T, source, function(en)
      while en:MoveNext() do
        local current = en:getCurrent()
        index = index + 1
        if predicate(current, index) then
          return true, current
        end
      end 
      return false
    end)
  end)
end

function Enumerable.Select(source, selector, T)
  if source == nil then throw(ArgumentNullException("source")) end
  if selector == nil then throw(ArgumentNullException("selector")) end
  return createEnumerable(T, function()
    local index = -1
    return createEnumerator(T, source, function(en) 
      if en:MoveNext() then
        index = index + 1
        return true, selector(en:getCurrent(), index)
      end
      return false
    end)
  end)
end

local function selectMany(source, collectionSelector, resultSelector, T)
  if source == nil then throw(ArgumentNullException("source")) end
  if collectionSelector == nil then throw(ArgumentNullException("collectionSelector")) end
  if resultSelector == nil then throw(ArgumentNullException("resultSelector")) end
  return createEnumerable(T, function() 
    local element, midEn
    local index = -1
    return createEnumerator(T, source, function(en) 
      while true do
        if midEn and midEn:MoveNext() then
          return true, resultSelector(element, midEn:getCurrent())
        else
          if not en:MoveNext() then return false end
          index = index + 1
          local current = en:getCurrent()
          midEn = collectionSelector(current, index):GetEnumerator()
          if midEn == nil then
            throw(NullReferenceException())
          end
          element = current
        end  
      end
    end)
  end)
end

local function identityFnOfSelectMany(s, x)
  return x
end

function Enumerable.SelectMany(source, ...)
  local len = select("#", ...)
  if len == 2 then
    local collectionSelector, T = ...
    return selectMany(source, collectionSelector, identityFnOfSelectMany, T)
  else
    return selectMany(source, ...)
  end
end

function Enumerable.Take(source, count)
  if source == nil then throw(ArgumentNullException("source")) end
  local T = source.__genericT__
  return createEnumerable(T, function()
    return createEnumerator(T, source, function(en)
      if count > 0 then
        if en:MoveNext() then
          count = count - 1
          return true, en:getCurrent()
        end
      end
      return false
    end)
  end)
end

function Enumerable.TakeWhile(source, predicate)
  if source == nil then throw(ArgumentNullException("source")) end
  if predicate == nil then throw(ArgumentNullException("predicate")) end
  local T = source.__genericT__
  return createEnumerable(T, function()
    local index = -1
    return createEnumerator(T, source, function(en)
      if en:MoveNext() then
        local current = en:getCurrent()
        index = index + 1
        if not predicate(current, index) then
          return false
        end
        return true, current
      end
      return false
    end)
  end)
end

function Enumerable.Skip(source, count)
  if source == nil then throw(ArgumentNullException("source")) end
  local T = source.__genericT__
  return createEnumerable(T, function()
    return createEnumerator(T, source, function(en)
      while count > 0 and en:MoveNext() do count = count - 1 end
      if count <= 0 then
        if en:MoveNext() then
          return true, en:getCurrent() 
        end
      end
      return false
    end)
  end)
end

function Enumerable.SkipWhile(source, predicate)
  if source == nil then throw(ArgumentNullException("source")) end
  if predicate == nil then throw(ArgumentNullException("predicate")) end
  local T = source.__genericT__
  return createEnumerable(T, function()
    local index = -1
    local isSkipEnd = false
    return createEnumerator(T, source, function(en)
      while not isSkipEnd do
        if en:MoveNext() then
          local current = en:getCurrent()
          index = index + 1
          if not predicate(current, index) then
            isSkipEnd = true
            return true, current
          end     
        else 
          return false
        end
      end
      if en:MoveNext() then
        return true, en:getCurrent()
      end
      return false
    end)
  end)
end

local IGrouping = System.defInf("System.Linq.IGrouping_2", function (TKey, TElement)
  return {
    base = { IEnumerable_1(TElement) } 
  }
end)

local Grouping = define("System.Linq.Grouping", function (TKey, TElement)
  return {
    __genericT__ = TElement,
    base = { IGrouping(TKey, TElement) },
    GetEnumerator = arrayEnumerator,
    getKey = function (this)
      return this.key
    end,
    getCount = function (this)
      return #this
    end
  }
end, nil, 2)

local function getGrouping(this, key)
  local hashCode = this.comparer:GetHashCodeOf(key)
  local groupIndex = this.indexs[hashCode]
  return this.groups[groupIndex]
end

local Lookup = {
  __ctor__ = function (this, comparer)
    this.comparer = comparer or EqualityComparer(this.__genericTKey__).getDefault()
    this.groups = {}
    this.indexs = {}
  end,
  get = function (this, key)
    local grouping = getGrouping(this, key)
    if grouping ~= nil then return grouping end 
    return Empty(this.__genericTElement__)
  end,
  GetCount = function (this)
    return #this.groups
  end,
  Contains = function (this, key)
    return getGrouping(this, key) ~= nil
  end,
  GetEnumerator = function (this)
    return arrayEnumerator(this.groups, IGrouping)
  end
}

local LookupFn = define("System.Linq.Lookup", function(TKey, TElement)
  local cls = {
    __genericTKey__ = TKey,
    __genericTElement__ = TElement,
  }
  return cls
end, Lookup, 2)

local function addToLookup(this, key, value)
  local hashCode = this.comparer:GetHashCodeOf(key)
  local groupIndex = this.indexs[hashCode]
  local group
  if groupIndex == nil then
	  groupIndex = #this.groups + 1
	  this.indexs[hashCode] = groupIndex
	  group = setmetatable({ key = key }, Grouping(this.__genericTKey__, this.__genericTElement__))
	  this.groups[groupIndex] = group
  else
	  group = this.groups[groupIndex]
	  assert(group)
  end
  group[#group + 1] = wrap(value)
end

local function createLookup(source, keySelector, elementSelector, comparer, TKey, TElement)
  local lookup = LookupFn(TKey, TElement)(comparer)
  for _, item in each(source) do
    addToLookup(lookup, keySelector(item), elementSelector(item))
  end
  return lookup
end

local function createLookupForJoin(source, keySelector, comparer, TKey, TElement)
  local lookup = LookupFn(TKey, TElement)(comparer)
  for _, item in each(source) do
    local key = keySelector(item)
    if key ~= nil then
      addToLookup(lookup, key, item)
    end
  end
  return lookup
end

function Enumerable.Join(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer, TKey, TResult)
  if outer == nil then throw(ArgumentNullException("outer")) end
  if inner == nil then throw(ArgumentNullException("inner")) end
  if outerKeySelector == nil then throw(ArgumentNullException("outerKeySelector")) end
  if innerKeySelector == nil then throw(ArgumentNullException("innerKeySelector")) end
  if resultSelector == nil then throw(ArgumentNullException("resultSelector")) end
  local lookup = createLookupForJoin(inner, innerKeySelector, comparer, TKey, inner.__genericT__)
  return createEnumerable(TResult, function ()
    local item, grouping, index
    return createEnumerator(TResult, outer, function (en)
      while true do
        if grouping ~= nil then
          index = index + 1
          if index < #grouping then
            return true, resultSelector(item, unWrap(grouping[index + 1]))
          end
        end
        if not en:MoveNext() then return false end
        local current = en:getCurrent()
        item = current
        grouping = getGrouping(lookup, outerKeySelector(current))
        index = -1
      end
    end)
  end)
end

function Enumerable.GroupJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer, TKey, TResult)
  if outer == nil then throw(ArgumentNullException("outer")) end
  if inner == nil then throw(ArgumentNullException("inner")) end
  if outerKeySelector == nil then throw(ArgumentNullException("outerKeySelector")) end
  if innerKeySelector == nil then throw(ArgumentNullException("innerKeySelector")) end
  if resultSelector == nil then throw(ArgumentNullException("resultSelector")) end
  local lookup = createLookupForJoin(inner, innerKeySelector, comparer, TKey, inner.__genericT__)
  return createEnumerable(TResult, function ()
    return createEnumerator(TResult, outer, function (en)
      if en:MoveNext() then
        local item = en:getCurrent()
        return true, resultSelector(item, lookup:get(outerKeySelector(item)))
      end
      return false
    end)
  end)
end

local function ordered(source, compare)
  local T = source.__genericT__
  local orderedEnumerable = createEnumerable(T, function()
    local t = {}
    local index = 0
    return createEnumerator(T, source, function() 
      index = index + 1
      local v = t[index]
      if v ~= nil then
        return true, unWrap(v)
      end
      return false
    end, 
    function() 
      local count = 1
      if isDictLike(source) then
        for k, v in pairs(source) do
          t[count] = setmetatable({ Key = k, Value = v }, T)
          count = count + 1
        end
      else
        for _, v in each(source) do
          t[count] = wrap(v)
          count = count + 1
        end
      end
      if count > 1 then
        tsort(t, function(x, y)
          return compare(unWrap(x), unWrap(y)) < 0 
        end)
      end
    end)
  end)
  orderedEnumerable.source = source
  orderedEnumerable.compare = compare
  return orderedEnumerable
end

local function orderBy(source, keySelector, comparer, TKey, descending)
  if source == nil then throw(ArgumentNullException("source")) end
  if keySelector == nil then throw(ArgumentNullException("keySelector")) end
  if comparer == nil then comparer = Comparer_1(TKey).getDefault() end 
  local keys = {}
  local function getKey(t) 
    local k = keys[t]
    if k == nil then
      k = keySelector(t)
      keys[t] = k
    end
    return k
  end
  local c = comparer.Compare
  local compare
  if descending then
    compare = function(x, y)
      return -c(comparer, getKey(x), getKey(y))
    end
  else
    compare = function(x, y)
      return c(comparer, getKey(x), getKey(y))
    end
  end
  return ordered(source, compare)
end

function Enumerable.OrderBy(source, keySelector, comparer, TKey)
  return orderBy(source, keySelector, comparer, TKey, false)
end

function Enumerable.OrderByDescending(source, keySelector, comparer, TKey)
  return orderBy(source, keySelector, comparer, TKey, true)
end

local function thenBy(source, keySelector, comparer, TKey, descending)
  if source == nil then throw(ArgumentNullException("source")) end
  if keySelector == nil then throw(ArgumentNullException("keySelector")) end
  if comparer == nil then comparer = Comparer_1(TKey).getDefault() end
  local keys = {}
  local function getKey(t) 
    local k = keys[t]
    if k == nil then
      k = keySelector(t)
      keys[t] = k
    end
    return k
  end
  local c = comparer.Compare
  local compare
  local parentSource, parentCompare = source.source, source.compare
  if descending then
    compare = function(x, y)
      local v = parentCompare(x, y)
      if v ~= 0 then
        return v
      else
        return -c(comparer, getKey(x), getKey(y))
      end
    end
  else
    compare = function(x, y)
      local v = parentCompare(x, y)
      if v ~= 0 then
        return v
      else
        return c(comparer, getKey(x), getKey(y))
      end
    end
  end
  return ordered(parentSource, compare)
end

function Enumerable.ThenBy(source, keySelector, comparer, TKey)
  return thenBy(source, keySelector, comparer, TKey, false)
end

function Enumerable.ThenByDescending(source, keySelector, comparer, TKey)
  return thenBy(source, keySelector, comparer, TKey, true)
end

local function groupBy(source, keySelector, elementSelector, comparer, TKey, TElement)
  if source == nil then throw(ArgumentNullException("source")) end
  if keySelector == nil then throw(ArgumentNullException("keySelector")) end
  if elementSelector == nil then throw(ArgumentNullException("elementSelector")) end
  return createEnumerable(IGrouping, function()
    return createLookup(source, keySelector, elementSelector, comparer, TKey, TElement):GetEnumerator()
  end)
end

function Enumerable.GroupBy(source, ...)
  if source == nil then throw(ArgumentNullException("source")) end
  local len = select("#", ...)
  if len == 2 then
    local keySelector, TKey = ...
    return groupBy(source, keySelector, identityFn, nil, TKey, source.__genericT__)
  elseif len == 3 then
    local keySelector, comparer, TKey = ...
    return groupBy(source, keySelector, identityFn, comparer, TKey, source.__genericT__)
  elseif len == 4 then
    local keySelector, elementSelector, TKey, TElement = ...
    return groupBy(source, keySelector, elementSelector, nil, TKey, TElement)
  else
    return groupBy(source, ...)
  end
end

local function groupBySelect(source, keySelector, elementSelector, resultSelector, comparer, TKey, TElement, TResult)
  if source == nil then throw(ArgumentNullException("source")) end
  if keySelector == nil then throw(ArgumentNullException("keySelector")) end
  if elementSelector == nil then throw(ArgumentNullException("elementSelector")) end
  if resultSelector == nil then throw(ArgumentNullException("resultSelector")) end
  return createEnumerable(TResult, function()
    local lookup = createLookup(source, keySelector, elementSelector, comparer, TKey, TElement)
    return createEnumerator(TResult, lookup, function(en)
      if en:MoveNext() then
        local current = en:getCurrent()
        return resultSelector(current.key, current)
      end
      return false
    end)
  end)
end

function Enumerable.GroupBySelect(source, ...)
  local len = select("#", ...)
  if len == 4 then
    local keySelector, resultSelector, TKey, TResult = ...
    return groupBySelect(source, keySelector, identityFn, resultSelector, nil, TKey, source.__genericT__, TResult)
  elseif len == 5 then
    local keySelector, resultSelector, comparer, TKey, TResult = ...
    return groupBySelect(source, keySelector, identityFn, resultSelector, comparer, TKey, source.__genericT__, TResult)
  elseif len == 6 then
    local keySelector, elementSelector, resultSelector, TKey, TElement, TResult = ...
    return groupBySelect(source, keySelector, elementSelector, resultSelector, nil, TKey, TElement, TResult)
  else
    return groupBySelect(source, ...)
  end
end

function Enumerable.Concat(first, second)
  if first == nil then throw(ArgumentNullException("first")) end
  if second == nil then throw(ArgumentNullException("second")) end
  local T = first.__genericT__
  return createEnumerable(T, function()
    local secondEn
    return createEnumerator(T, first, function(en)
      if secondEn == nil then
        if en:MoveNext() then
          return true, en:getCurrent()
        end
        secondEn = second:GetEnumerator()
      end
      if secondEn:MoveNext() then
        return true, secondEn:getCurrent()
      end
      return false
    end)
  end)
end

function Enumerable.Zip(first, second, resultSelector, TResult) 
  if first == nil then throw(ArgumentNullException("first")) end
  if second == nil then throw(ArgumentNullException("second")) end
  if resultSelector == nil then throw(ArgumentNullException("resultSelector")) end
  return createEnumerable(TResult, function()
    local e2
    return createEnumerator(TResult, first, function(e1)
      if e1:MoveNext() and e2:MoveNext() then
          return true, resultSelector(e1:getCurrent(), e2:getCurrent())
      end
    end, 
    function()
      e2 = second:GetEnumerator()
    end)
  end)
end

local function addToSet(set, v, getHashCode, comparer)
  local hashCode = getHashCode(comparer, v)
  if set[hashCode] == nil then
    set[hashCode] = true
    return true
  end
  return false
end

local function removeFromSet(set, v, getHashCode, comparer)
  local hashCode = getHashCode(comparer, v)
  if set[hashCode] ~= nil then
    set[hashCode] = nil
    return true
  end
  return false
end

local function getComparer(source, comparer)
  return comparer or EqualityComparer(source.__genericT__).getDefault()
end

function Enumerable.Distinct(source, comparer)
  if source == nil then throw(ArgumentNullException("source")) end
  local T = source.__genericT__
  return createEnumerable(T, function()
    local set = {}
    comparer = getComparer(source, comparer)
    local getHashCode = comparer.GetHashCodeOf
    return createEnumerator(T, source, function(en)
      while en:MoveNext() do
        local current = en:getCurrent()
        if addToSet(set, current, getHashCode, comparer) then
          return true, current  
        end
      end
      return false
    end)
  end)
end

function Enumerable.Union(first, second, comparer)
  if first == nil then throw(ArgumentNullException("first")) end
  if second == nil then throw(ArgumentNullException("second")) end
  local T = first.__genericT__
  return createEnumerable(T, function()
    local set = {}
    comparer = getComparer(first, comparer)
    local getHashCode = comparer.GetHashCodeOf
    local secondEn
    return createEnumerator(T, first, function(en)
      if secondEn == nil then
        while en:MoveNext() do
          local current = en:getCurrent()
          if addToSet(set, current, getHashCode, comparer) then
            return true, current  
          end
        end
        secondEn = second:GetEnumerator()
      end
      while secondEn:MoveNext() do
        local current = secondEn:getCurrent()
        if addToSet(set, current, getHashCode, comparer) then
          return true, current  
        end
      end
      return false
    end)
  end)
end

function Enumerable.Intersect(first, second, comparer)
  if first == nil then throw(ArgumentNullException("first")) end
  if second == nil then throw(ArgumentNullException("second")) end
  local T = first.__genericT__
  return createEnumerable(T, function()
    local set = {}
    comparer = getComparer(first, comparer)
    local getHashCode = comparer.GetHashCodeOf
    return createEnumerator(T, first, function(en)
      while en:MoveNext() do
        local current = en:getCurrent()
        if removeFromSet(set, current, getHashCode, comparer) then
          return true, current
        end
      end
      return false
    end,
    function()
      for _, v in each(second) do
        addToSet(set, v, getHashCode, comparer)
      end
    end)
  end) 
end

function Enumerable.Except(first, second, comparer)
  if first == nil then throw(ArgumentNullException("first")) end
  if second == nil then throw(ArgumentNullException("second")) end
  local T = first.__genericT__
  return createEnumerable(T, function()
    local set = {}
    comparer = getComparer(first, comparer)
    local getHashCode = comparer.GetHashCodeOf
    return createEnumerator(T, first, function(en) 
      while en:MoveNext() do
        local current = en:getCurrent()
        if addToSet(set, current, getHashCode, comparer) then
          return true, current  
        end
      end
      return false
    end,
    function()
      for _, v in each(second) do
        addToSet(set, v, getHashCode, comparer)
      end
    end)
  end)
end

function Enumerable.Reverse(source)
  if source == nil then throw(ArgumentNullException("source")) end
  local T = source.__genericT__
  return createEnumerable(T, function()
    local t = {}    
    local index
    return createEnumerator(T, nil, function() 
      if index > 1 then
        index = index - 1
        return true, t[index]
      end
      return false
    end,
    function()
      local count = 1
      for _, v in each(source) do
        t[count] = v
        count = count + 1
      end  
      index = count
    end)
  end)
end

function Enumerable.SequenceEqual(first, second, comparer)
  if first == nil then throw(ArgumentNullException("first")) end
  if second == nil then throw(ArgumentNullException("second")) end
  comparer = getComparer(first, comparer)
  local equals = comparer.EqualsOf
  local e1 = first:GetEnumerator()
  local e2 = second:GetEnumerator()
  while e1:MoveNext() do
    if not(e2:MoveNext() and equals(comparer, e1:getCurrent(), e2:getCurrent())) then
      return false
    end
  end
  if e2:MoveNext() then
    return false
  end
  return true
end

Enumerable.ToArray = Array.toArray

function Enumerable.ToList(source)
  return System.List(source.__genericT__)(source)
end

local function toDictionary(source, keySelector, elementSelector, comparer, TKey, TValue)
  if source == nil then throw(ArgumentNullException("source")) end
  if keySelector == nil then throw(ArgumentNullException("keySelector")) end
  if elementSelector == nil then throw(ArgumentNullException("elementSelector")) end
  local dict = System.Dictionary(TKey, TValue)(comparer)
  for _, v in each(source) do
    dict:Add(keySelector(v), elementSelector(v))
  end
  return dict
end

function Enumerable.ToDictionary(source, ...)
  local len = select("#", ...)
  if len == 2 then
    local keySelector, TKey = ...
    return toDictionary(source, keySelector, identityFn, nil, TKey, source.__genericT__)
  elseif len == 3 then
    local keySelector, comparer, TKey = ...
    return toDictionary(source, keySelector, identityFn, comparer, TKey, source.__genericT__)
  elseif len == 4 then
    local keySelector, elementSelector, TKey, TElement = ...
    return toDictionary(source, keySelector, elementSelector, nil, TKey, TElement)
  else
    return toDictionary(source, ...)
  end
end

local function toLookup(source, keySelector, elementSelector, comparer, TKey, TElement )
  if source == nil then throw(ArgumentNullException("source")) end
  if keySelector == nil then throw(ArgumentNullException("keySelector")) end
  if elementSelector == nil then throw(ArgumentNullException("elementSelector")) end
  return createLookup(source, keySelector, elementSelector, comparer, TKey, TElement)
end

function Enumerable.ToLookup(source, ...)
  local len = select("#", ...)
  if len == 2 then
    local keySelector, TKey = ...
    return toLookup(source, keySelector, identityFn, nil, TKey, source.__genericT__)
  elseif len == 3 then
    local keySelector, comparer, TKey = ...
    return toLookup(source, keySelector, identityFn, comparer, TKey, source.__genericT__)
  elseif len == 4 then
    local keySelector, elementSelector, TKey, TElement = ...
    return toLookup(source, keySelector, elementSelector, nil, TKey, TElement)
  else
    return toLookup(source, ...)
  end
end

function Enumerable.DefaultIfEmpty(source)
  if source == nil then throw(ArgumentNullException("source")) end
  local T = source.__genericT__
  local state 
  return createEnumerable(T, function()
    return createEnumerator(T, source, function(en)
      if not state then
        if en:MoveNext() then
          state = 1
          return true, en:getCurrent()
        end
        state = 2
        return true, T:default()
      elseif state == 1 then
        if en:MoveNext() then
          return true, en:getCurrent()
        end
      end
      return false
    end)
  end)
end

function Enumerable.OfType(source, T)
  if source == nil then throw(ArgumentNullException("source")) end
  return createEnumerable(T, function()
    return createEnumerator(T, source, function(en) 
      while en:MoveNext() do
        local current = en:getCurrent()
        if is(current, T) then
          return true, current
        end
      end
      return false
    end)
  end)
end

function Enumerable.Cast(source, T)
  if source == nil then throw(ArgumentNullException("source")) end
  if is(source, IEnumerable_1(T)) then return source end
  return createEnumerable(T, function()
    return createEnumerator(T, source, function(en) 
      if en:MoveNext() then
        return true, cast(T, en:getCurrent())
      end
      return false
    end)
  end)
end

local function first(source, ...)
  if source == nil then throw(ArgumentNullException("source")) end
  local len = select("#", ...)
  if len == 0 then
    if isArrayLike(source) then
      local count = #source
      if count > 0 then
        return true, unWrap(source[1])
      end
    else
      local en = source:GetEnumerator()
      if en:MoveNext() then 
        return true, en:getCurrent()
      end
    end
    return false, 0
  else
    local predicate = ...
    if predicate == nil then throw(ArgumentNullException("predicate")) end
    for _, v in each(source) do
      if predicate(v) then 
        return true, v
      end
    end
    return false, 1
  end
end

function Enumerable.First(source, ...)
  local ok, result = first(source, ...)
  if ok then return result end
  if result == 0 then
    throw(InvalidOperationException("NoElements"))
  end
  throw(InvalidOperationException("NoMatch"))
end

function Enumerable.FirstOrDefault(source, ...)
  local ok, result = first(source, ...)
  return ok and result or source.__genericT__:default()
end

local function last(source, ...)
  if source == nil then throw(ArgumentNullException("source")) end
  local len = select("#", ...)
  if len == 0 then
    if isArrayLike(source) then
      local count = #source
      if count > 0 then
        return true, unWrap(source[count])
      end
    else
      local en = source:GetEnumerator()
      if en:MoveNext() then 
        local result
        repeat
          result = en:getCurrent()
        until not en:MoveNext()
        return true, result
      end
    end
    return false, 0
  else
    local predicate = ...
    if predicate == nil then throw(ArgumentNullException("predicate")) end
    local result, found
    for _, v in each(source) do
      if predicate(v) then
        result = v
        found = true
      end
    end    
    if found then return true, result end
    return false, 1
  end
end

function Enumerable.Last(source, ...)
  local ok, result = last(source, ...)
  if ok then return result end
  if result == 0 then
    throw(InvalidOperationException("NoElements"))
  end
  throw(InvalidOperationException("NoMatch"))
end

function Enumerable.LastOrDefault(source, ...)
  local ok, result = last(source, ...)
  return ok and result or source.__genericT__:default()
end

local function single(source, ...)
  if source == nil then throw(ArgumentNullException("source")) end
  local len = select("#", ...)
  if len == 0 then
    if isArrayLike(source) then
      local count = #source
      if count == 0 then
        return false, 0
      elseif count == 1 then
        return true, unWrap(source[1])
      end
    else
      local en = source:GetEnumerator()
      if not en:MoveNext() then return false, 0 end
      local result = en:getCurrent()
      if not en:MoveNext() then
        return true, result
      end
    end
    return false, 1
  else
    local predicate = ...
    if predicate == nil then throw(ArgumentNullException("predicate")) end
    local result, found
    for _, v in each(source) do
      if predicate(v) then
        result = v
        if found then
          return false, 1
        end
        found = true
      end
    end
    if foun then return true, result end    
    return false, 0    
  end
end

function Enumerable.Single(source, ...)
  local ok, result = single(source, ...)
  if ok then return result end
  if result == 0 then
    throw(InvalidOperationException("NoElements"))
  end
  throw(InvalidOperationException("MoreThanOneMatch"))
end

function Enumerable.SingleOrDefault(source, ...)
  local ok, result = single(source, ...)
  return ok and result or source.__genericT__:default()
end

local function elementAt(source, index)
  if source == nil then throw(ArgumentNullException("source")) end
  if index >= 0 then
    if isArrayLike(source) then
      local count = #source
      if index < count then
        return true, unWrap(source[index + 1])
      end
    else
      local en = source:GetEnumerator()
      while true do
        if not en:MoveNext() then break end
        if index == 0 then return true, en:getCurrent() end
        index = index - 1
      end
    end
  end
  return false
end

function Enumerable.ElementAt(source, index)
  local ok, result = elementAt(source, index)
  if ok then return result end
  throw(ArgumentOutOfRangeException("index"))
end

function Enumerable.ElementAtOrDefault(source, index)
  local ok, result = elementAt(source, index)
  return ok and result or source.__genericT__:default()
end

function Enumerable.Range(start, count)
  if count < 0 then throw(ArgumentOutOfRangeException("count")) end
  return createEnumerable(Int32, function()
    local index = -1
    return createEnumerator(Int32, nil, function()
      index = index + 1
      if index < count then
        return true, start + index  
      end
      return false
    end)
  end)
end

function Enumerable.Repeat(element, count, T)
  if count < 0 then throw(ArgumentOutOfRangeException("count")) end
  return createEnumerable(T, function()
    local index = -1
    return createEnumerator(T, nil, function()
      index = index + 1
      if index < count then
        return true, element  
      end
      return false
    end)
  end)
end

function Enumerable.Any(source, ...)
  if source == nil then throw(ArgumentNullException("source")) end
  local len = select("#", ...)
  if len == 0 then
    local en = source:GetEnumerator()
    return en:MoveNext()
  else
    local predicate = ...
    if predicate == nil then throw(ArgumentNullException("predicate")) end
    for _, v in each(source) do
      if predicate(v) then
        return true
      end
    end
    return false
  end
end

function Enumerable.All(source, predicate)
  if source == nil then throw(ArgumentNullException("source")) end
  if predicate == nil then throw(ArgumentNullException("predicate")) end
  for _, v in each(source) do
    if not predicate(v) then
      return false
    end
  end
  return true
end

function Enumerable.Count(source, ...)
  if source == nil then throw(ArgumentNullException("source")) end
  local len = select("#", ...)
  if len == 0 then
    if isArrayLike(source) then
      return #source
    end
    local count = 0
    local en = source:GetEnumerator()
    while en:MoveNext() do 
      count = count + 1 
    end
    return count
  else
    local predicate = ...
    if predicate == nil then throw(ArgumentNullException("predicate")) end
    local count = 0
    for _, v in each(source) do
      if predicate(v) then
        count = count + 1
      end
    end
    return count
  end
end

function Enumerable.Contains(source, value, comparer)
  if source == nil then throw(ArgumentNullException("source")) end
  comparer = getComparer(source, comparer)
  local equals = comparer.EqualsOf
  for _, v in each(source) do
    if equals(comparer, v, value) then
      return true
    end
  end
  return false
end

function Enumerable.Aggregate(source, ...)
  if source == nil then throw(ArgumentNullException("source")) end
  local len = select("#", ...);
  if len == 1 then
    local func = ...
    if func == nil then throw(ArgumentNullException("func")) end
    local e = source:GetEnumerator()
    if not e:MoveNext() then throw(InvalidOperationException("NoElements")) end
    local result = e:getCurrent()
    while e:MoveNext() do
      result = func(result, e:getCurrent())
    end
    return result
  elseif len == 2 then
    local seed, func = ...
    if func == nil then throw(ArgumentNullException("func")) end
    local result = seed
    for _, element in each(source) do
      result = func(result, element)
    end
    return result
  else 
    local seed, func, resultSelector = ...
    if func == nil then throw(ArgumentNullException("func")) end
    if resultSelector == nil then throw(ArgumentNullException("resultSelector")) end
    local result = seed
    for _, element in each(source) do
      result = func(result, element)
    end
    return resultSelector(result)
  end
end

function Enumerable.Sum(source, ...)
  if source == nil then throw(ArgumentNullException("source")) end
  local len = select("#", ...)
  if len == 0 then
    local sum = 0
    for _, v in each(source) do
      sum = sum + v
    end
    return sum
  else
    local selector = ...
    if selector == nil then throw(ArgumentNullException("selector")) end
    local sum = 0
    for _, v in each(source) do
      sum = sum + selector(v)
    end
    return sum
  end
end

local function minOrMax(compareFn, source, ...)
  if source == nil then throw(ArgumentNullException("source")) end
  local len = select("#", ...)
  local selector, T 
  if len == 0 then
    selector, T = identityFn, source.__genericT__
  else
    selector, T = ...
    if selector == nil then throw(ArgumentNullException("selector")) end
  end
  local comparer = Comparer_1(T).getDefault()
  local compare = comparer.Compare
  local value = T:default()
  if value == nil then
    for _, x in each(source) do
      x = selector(x)
      if x ~= nil and (value == nil or compareFn(compare, comparer, x, value)) then
        value = x
      end 
    end
    return value
  else
    local hasValue = false
    for _, x in each(source) do
      x = selector(x)
      if hasValue then
        if compareFn(compare, comparer, x, value) then
          value = x
        end
      else
        value = x
        hasValue = true
      end
    end
    if hasValue then return value end
    throw(InvalidOperationException("NoElements"))
  end
end

local function minFn(compare, comparer, x, y)
  return compare(comparer, x, y) < 0
end

function Enumerable.Min(source, ...)
  return minOrMax(minFn, source, ...)
end

local function maxFn(compare, comparer, x, y)
  return compare(comparer, x, y) > 0
end

function Enumerable.Max(source, ...)
  return minOrMax(maxFn, source, ...)
end

function Enumerable.Average(source, ...)
  if source == nil then throw(ArgumentNullException("source")) end
  local sum, count = 0, 0
  local len = select("#", ...)
  if len == 0 then
    for _, v in each(source) do
      sum = sum + v
      count = count + 1
    end
  else
    local selector = ...
    if selector == nil then throw(ArgumentNullException("selector")) end
    for _, v in each(source) do
      sum = sum + selector(v)
      count = count + 1
    end
  end
  if count > 0 then
    return sum / count
  end
  throw(InvalidOperationException("NoElements"))
end
