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
local lengthFn = System.lengthFn
local Array = System.Array
local ArrayDictionary = System.ArrayDictionary

local SortedList = {
  __ctor__ = Array.ctorOrderDict,
  getComparer = Array.getOrderComparer,
  getCount = lengthFn,
  getCapacity = lengthFn,
  setCapacity = Array.setCapacity,
  AddKeyValue = Array.addOrderDict,
  AddKeyValueObj = Array.addOrderDictObj,
  Add = Array.addPairOrderDict,
  Clear = Array.clear,
  ContainsKey = Array.containsOrderDict,
  ContainsValue = ArrayDictionary.ContainsValue,
  ContainsKeyObj = Array.containsOrderDictObj,
  GetEnumerator = Array.GetEnumerator,
  IndexOfKey = Array.indexKeyOrderDict,
  IndexOfValue =  Array.indexOfValue,
  RemoveKey = Array.removeOrderDict,
  Remove =  Array.removePairOrderDict,
  RemoveAt = Array.removeAt,
  TrimExcess = System.emptyFn,
  getKeys = ArrayDictionary.getKeys,
  getValues = ArrayDictionary.getValues,
  get = Array.getOrderDict,
  getObj = Array.getOrderDictObj,
  set = Array.setOrderDict,
  setObj = Array.setOrderDictObj,
}

local SortedListFn = System.define("System.Collections.Generic.SortedList", function(TKey, TValue) 
return { 
  base = { System.IDictionary_2(TKey, TValue), System.IDictionary, System.IReadOnlyDictionary_2(TKey, TValue) },
  __genericT__ = System.KeyValuePair(TKey, TValue),
  __genericTKey__ = TKey,
  __genericTValue__ = TValue,
}
end, SortedList, 2)

System.SortedList = SortedListFn

