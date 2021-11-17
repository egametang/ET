--[[
Copyright YANG Huan (sy.yanghuan@gmail.com).

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
local Array = System.Array

local SortedSet = {
  __ctor__ = Array.ctorOrderSet,
  getMin = Array.firstOrDefault,
  getMax = Array.lastOrDefault,
  getCount = System.lengthFn,
  getComparer = Array.getOrderComparer,
  CreateSetComparer = Array.createSetComparer,
  Add = Array.addOrder,
  Clear = Array.clear,
  Contains = Array.containsOrder,
  CopyTo = Array.CopyTo,
  ExceptWith = Array.exceptWithOrder,
  GetEnumerator = Array.GetEnumerator,
  GetViewBetween = Array.getViewBetweenOrder,
  IntersectWith = Array.intersectWithOrder,
  IsProperSubsetOf = Array.isProperSubsetOfOrder,
  IsProperSupersetOf = Array.isProperSupersetOfOrder,
  IsSubsetOf = Array.isSubsetOfOrder,
  IsSupersetOf = Array.isSupersetOfOrder,
  Overlaps = Array.isOverlapsOrder,
  Remove = Array.removeOrder,
  RemoveWhere = Array.removeAll,
  Reverse = Array.reverseEnumerable,
  SetEquals = Array.equalsOrder,
  SymmetricExceptWith = Array.symmetricExceptWithOrder,
  TryGetValue = Array.tryGetValueOrder,
  UnionWith = Array.addOrderRange,
}

local SortedSetFn = System.define("System.Collections.Generic.SortedSet", function(T) 
  return { 
    base = { System.ICollection_1(T), System.IReadOnlyCollection_1(T), System.ISet_1(T) }, 
    __genericT__ = T,
  }
end, SortedSet, 1)

System.SortedSet = SortedSetFn
