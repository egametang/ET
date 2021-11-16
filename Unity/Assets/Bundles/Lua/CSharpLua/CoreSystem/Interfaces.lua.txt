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
local defInf = System.defInf
local emptyFn = System.emptyFn

local IComparable = defInf("System.IComparable")
local IFormattable = defInf("System.IFormattable")
local IConvertible = defInf("System.IConvertible")
defInf("System.IFormatProvider")
defInf("System.ICloneable")

defInf("System.IComparable_1", emptyFn)
defInf("System.IEquatable_1", emptyFn)

defInf("System.IPromise")
defInf("System.IDisposable")

local IEnumerable = defInf("System.IEnumerable")
local IEnumerator = defInf("System.IEnumerator")

local ICollection = defInf("System.ICollection", {
  base = { IEnumerable }
})

defInf("System.IList", {
  base = { ICollection }
})

defInf("System.IDictionary", {
  base = { ICollection }
})

defInf("System.IEnumerator_1", function(T) 
  return {
    base = { IEnumerator }
  }
end)

local IEnumerable_1 = defInf("System.IEnumerable_1", function(T) 
  return {
    base = { IEnumerable }
  }
end)

local ICollection_1 = defInf("System.ICollection_1", function(T) 
  return { 
    base = { IEnumerable_1(T) } 
  }
end)

local IReadOnlyCollection_1 = defInf("System.IReadOnlyCollection_1", function (T)
  return { 
    base = { IEnumerable_1(T) } 
  }
end)

defInf("System.IReadOnlyList_1", function (T)
  return { 
    base = { IReadOnlyCollection_1(T) } 
  }
end)

defInf('System.IDictionary_2', function(TKey, TValue) 
  return {
    base = { ICollection_1(System.KeyValuePair(TKey, TValue)) }
  }
end)

defInf("System.IReadOnlyDictionary_2", function(TKey, TValue) 
  return {
    base = { IReadOnlyCollection_1(System.KeyValuePair(TKey, TValue)) }
  }
end)

defInf("System.IList_1", function(T) 
  return {
    base = { ICollection_1(T) }
  }
end)

defInf("System.ISet_1", function(T) 
  return {
    base = { ICollection_1(T) }
  }
end)

defInf("System.IComparer")
defInf("System.IComparer_1", emptyFn)
defInf("System.IEqualityComparer")
defInf("System.IEqualityComparer_1", emptyFn)

System.enumMetatable.interface = { IComparable, IFormattable, IConvertible }
