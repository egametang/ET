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

return function(dir, conf)
  dir = (dir and #dir > 0) and (dir .. ".CoreSystem.") or "CoreSystem."
  local require = require
  local load = function(module) return require(dir .. module) end
    
  load("Core")(conf)
  load("Interfaces")
  load("Exception")
  load("Number")
  load("Char")
  load("String")
  load("Boolean")
  load("Delegate")
  load("Enum")
  load("TimeSpan")
  load("DateTime")
  load("Collections.EqualityComparer")
  load("Array")
  load("Type")
  load("Collections.List")
  load("Collections.Dictionary")
  load("Collections.Queue")
  load("Collections.Stack")
  load("Collections.HashSet")
  load("Collections.LinkedList")
  load("Collections.Linq")
  load("Convert")
  load("Math")
  load("Random")
  load("Text.StringBuilder")
  load("Console")
  load("IO.File")
  load("Reflection.Assembly")
  load("Threading.Timer")
  load("Threading.Thread")
  load("Threading.Task")
  load("Utilities")
end
