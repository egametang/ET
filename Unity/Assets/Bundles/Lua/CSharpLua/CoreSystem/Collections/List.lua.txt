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
local falseFn = System.falseFn
local lengthFn = System.lengthFn
local Array = System.Array

local List = {
  __ctor__ = Array.ctorList,
  getCapacity = lengthFn,
  setCapacity = Array.setCapacity,
  getCount = lengthFn,
  getIsFixedSize = falseFn,
  getIsReadOnly = falseFn,
  get = Array.get,
  set = Array.set,
  Add = Array.add,
  AddObj = Array.addObj,
  AddRange = Array.addRange,
  AsReadOnly = Array.AsReadOnly,
  BinarySearch = Array.BinarySearch,
  Clear = Array.clear,
  Contains = Array.Contains,
  CopyTo = Array.CopyTo,
  Exists = Array.Exists,
  Find = Array.Find,
  FindAll = Array.findAll,
  FindIndex = Array.FindIndex,
  FindLast = Array.FindLast,
  FindLastIndex = Array.FindLastIndex,
  ForEach = Array.ForEach,
  GetEnumerator = Array.GetEnumerator,
  GetRange = Array.getRange,
  IndexOf = Array.IndexOf,
  Insert = Array.insert,
  InsertRange = Array.insertRange,
  LastIndexOf = Array.LastIndexOf,
  Remove = Array.remove,
  RemoveAll = Array.removeAll,
  RemoveAt = Array.removeAt,
  RemoveRange = Array.removeRange,
  Reverse = Array.Reverse,
  Sort = Array.Sort,
  TrimExcess = System.emptyFn,
  ToArray = Array.toArray,
  TrueForAll = Array.TrueForAll
}

function System.listFromTable(t, T)
  return setmetatable(t, List(T))
end

local ListFn = System.define("System.Collections.Generic.List", function(T) 
  return { 
    base = { System.IList_1(T), System.IReadOnlyList_1(T), System.IList }, 
    __genericT__ = T,
  }
end, List, 1)

System.List = ListFn
System.ArrayList = ListFn(System.Object)
