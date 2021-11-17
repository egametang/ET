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
local Array = System.Array

local Stack = {
  __ctor__ = Array.ctorList,
  getCount = Array.getLength,
  Clear = Array.clear,
  Contains = Array.Contains,
  GetEnumerator = Array.reverseEnumerator,
  Push = Array.add,
  Peek = Array.last,
  Pop = Array.popLast,
  ToArray = Array.toArray,
  TrimExcess = System.emptyFn
}

function System.stackFromTable(t, T)
  return setmetatable(t, Stack(T))
end

local StackFn = System.define("System.Collections.Generic.Stack", function(T) 
  return {
    base = { System.IEnumerable_1(T), System.ICollection },
    __genericT__ = T,
  }
end, Stack, 1)

System.Stack = StackFn
System.stack = StackFn(System.Object)
