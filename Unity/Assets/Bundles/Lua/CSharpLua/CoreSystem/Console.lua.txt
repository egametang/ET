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
local toString = System.toString

local print = print
local select = select
local string = string
local byte = string.byte
local char = string.char
local Format = string.Format

local function getWriteValue(v, ...)
  if select("#", ...) ~= 0 then
    return Format(v, ...)
  end
  return toString(v)
end

local Console = System.define("System.Console", {
  WriteLine = function (v, ...)
    print(getWriteValue(v, ...))     
  end,
  WriteLineChar = function (v)
    print(char(v))     
  end
})

local io = io
if io then
  local stdin = io.stdin
  local stdout = io.stdout
  local read = stdin.read
  local write = stdout.write

  function Console.Read()
    local ch = read(stdin, 1)
    return byte(ch)
  end

  function Console.ReadLine()
     return read(stdin)
  end

  function Console.Write(v, ...)
    write(stdout, getWriteValue(v, ...))
  end

  function Console.WriteChar(v)
     write(stdout, char(v))
  end
end
