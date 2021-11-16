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

local io = io
if io then

local System = System
local define = System.define
local throw = System.throw
local each = System.each

local open = io.open
local remove = os.remove

local IOException = define("System.IO.IOException", {
  __tostring = System.Exception.ToString,
  base = { System.Exception },
  __ctor__ = function(this, message, innerException) 
    System.Exception.__ctor__(this, message or "I/O error occurred.", innerException)
  end,
})

local function openFile(path, mode)
  local f, err = open(path, mode)
  if f == nil then
    throw(IOException(err))
  end
  return f
end

local function readAll(path, mode)
  local f = openFile(path, mode)
  local bytes = f:read("*all")
  f:close()
  return bytes
end

local function writeAll(path, contents, mode)
  local f = openFile(path, mode)
  f:write(contents)
  f:close()
end

define("System.IO.File", {
  ReadAllBytes = function (path)
    return readAll(path, "rb")
  end,
  ReadAllText = function (path)
    return readAll(path, "r")
  end,
  ReadAllLines = function (path)
    local t = {}
    local count = 1
    for line in io.lines(path) do
      t[count] = line
      count = count + 1
    end
    return System.arrayFromTable(t, System.String)
  end,  
  WriteAllBytes = function (path, contents)
    writeAll(path, contents, "wb")
  end,
  WriteAllText = function (path, contents)
    writeAll(path, contents, "w")
  end,
  WriteAllLines = function (path, contents)
    local f = openFile(path, "w")
    for _, line in each(contents) do
      if line == nil then
        f:write("\n")
      else
        f:write(line, "\n")
      end
    end
    f:close()
  end,
  Exists = function (path)
    local file = io.open(path, "rb")
    if file then file:close() end
    return file ~= nil
  end,
  Delete = function (path)
    local ok, err = remove(path)
    if not ok then
      throw(IOException(err))
    end
  end
})

end
