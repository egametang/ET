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
local equalsObj = System.equalsObj
local compareObj = System.compareObj
local ArgumentException = System.ArgumentException
local ArgumentNullException = System.ArgumentNullException

local type = type

local EqualityComparer
EqualityComparer = define("System.EqualityComparer", function (T)
  local equals
  local Equals = T.Equals
  if Equals then
    if T.class == 'S' then
      equals = Equals 
    else
      equals = function (x, y) 
        return x:Equals(y) 
      end 
    end
  else
    equals = equalsObj
  end
  local function getHashCode(x)
    if type(x) == "table" then
      return x:GetHashCode()
    end
    return x
  end
  local defaultComparer
  return {
    __genericT__ = T,
    base = { System.IEqualityComparer_1(T), System.IEqualityComparer }, 
    getDefault = function ()
      local comparer = defaultComparer 
      if comparer == nil then
        comparer = EqualityComparer(T)()
        defaultComparer = comparer
      end
      return comparer
    end,
    EqualsOf = function (this, x, y)
      if x ~= nil then
        if y ~= nil then return equals(x, y) end
        return false
      end                 
      if y ~= nil then return false end
      return true
    end,
    GetHashCodeOf = function (this, obj)
      if obj == nil then return 0 end
      return getHashCode(obj)
    end,
    GetHashCodeObjOf = function (this, obj)
      if obj == nil then return 0 end
      if System.is(obj, T) then return getHashCode(obj) end
      throw(ArgumentException("Type of argument is not compatible with the generic comparer."))
    end,
    EqualsObjOf = function (this, x, y)
      if x == y then return true end
      if x == nil or y == nil then return false end
      local is = System.is
      if is(x, T) and is(y, T) then return equals(x, y) end
      throw(ArgumentException("Type of argument is not compatible with the generic comparer."))
    end
  }
end, nil, 1)

local function compare(this, a, b)
  return compareObj(a, b)
end

define("System.Comparer", (function ()
  local Comparer
  Comparer = {
    base = { System.IComparer },
    static = function (this)
      local default = Comparer()
      this.Default = default
      this.DefaultInvariant = default
    end,
    Compare = compare
  }
  return Comparer
end)())

local Comparer, ComparisonComparer

ComparisonComparer = define("System.ComparisonComparer", function (T)
  return {
    base = { Comparer(T) },
    __ctor__ = function (this, comparison)
      this.comparison = comparison
    end,
    Compare = function (this, x, y)
      return this.comparison(x, y)
    end
  }
end, nil, 1)

Comparer = define("System.Comparer_1", function (T)
  local Compare
  local compareTo = T.CompareTo
  if compareTo then
    if T.class ~= 'S' then
      compareTo = function (x, y)
        return x:CompareTo(y)
      end
    end
    Compare = function (this, x, y)
      if x ~= nil then
        if y ~= nil then 
          return compareTo(x, y) 
        end
        return 1
      end                 
      if y ~= nil then return -1 end
      return 0
    end
  else
    Compare = compare
  end

  local defaultComparer
  local function getDefault()
    local comparer = defaultComparer 
    if comparer == nil then
      comparer = Comparer(T)()
      defaultComparer = comparer
    end
    return comparer
  end

  local function Create(comparison)
    if comparison == nil then throw(ArgumentNullException("comparison")) end
    return ComparisonComparer(T)(comparison)
  end

  return {
    __genericT__ = T,
    base = { System.IComparer_1(T), System.IComparer }, 
    getDefault = getDefault,
    getDefaultInvariant = getDefault,
    Compare = Compare,
    Create = Create
  }
end)
