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
local throw = System.throw
local clear = System.Array.clear
local toString = System.toString
local ArgumentNullException = System.ArgumentNullException
local ArgumentOutOfRangeException = System.ArgumentOutOfRangeException
local IndexOutOfRangeException = System.IndexOutOfRangeException

local table = table
local tconcat = table.concat
local schar = string.char
local ssub = string.sub
local sbyte = string.byte
local type = type
local select = select

local function build(this, value, startIndex, length)
  value = value:Substring(startIndex, length)
  local len = #value
  if len > 0 then
    this[#this + 1] = value
    this.Length = len
  end
end

local function getItemIndex(this, index)
  for i = 1, #this do
    local s = this[i]
    local len = #s
    local begin = index
    index = index - len
    if index < 0 then
      begin = begin + 1
      local ch = sbyte(s, begin)
      if not ch then
        break
      end
      return i, s, begin, ch
    end
  end
end

local function getLength(this)
  return this.Length
end

local StringBuilder = System.define("System.Text.StringBuilder", { 
  Length = 0,
  ToString = tconcat,
  __tostring = tconcat,
  __ctor__ = function (this, ...)
    local len = select("#", ...)
    if len == 0 then
    elseif len == 1 or len == 2 then
      local value = ...
      if type(value) == "string" then
        build(this, value, 0, #value)
      else
        build(this, "", 0, 0)
      end
    else 
      local value, startIndex, length = ...
      build(this, value, startIndex, length)
    end
  end,
  get = function (this, index)
    local _, _, _, ch = getItemIndex(this, index)
    if not _ then
      throw(IndexOutOfRangeException())
    end
    return ch
  end,
  set = function (this, index, value)
    local i, s, j = getItemIndex(this, index)
    if not i then
      throw(ArgumentOutOfRangeException("index"))
    end
    this[i] = ssub(s, 1, j - 1) .. schar(value) .. ssub(s, j + 1)
  end,
  setCapacity = function (this, value)
    if value < this.Length then
      throw(ArgumentOutOfRangeException())
    end
  end,
  getCapacity = getLength,
  getMaxCapacity = getLength,
  getLength = getLength,
  setLength = function (this, value) 
    if value < 0 then throw(ArgumentOutOfRangeException("value")) end
    if value == 0 then
      this:Clear()
      return
    end
    local delta = value - this.Length
    if delta > 0 then
      this:AppendCharRepeat(0, delta)
    else
      local length, remain = #this, value
      for i = 1, length do
        local s = this[i]
        local len = #s
        if len >= remain then
          if len ~= remain then
            s = ssub(s, 0, remain)
            this[i] = s
          end
          for j = i + 1, length do
            this[j] = nil
          end
          break
        end
        remain = remain - len
      end
      this.Length = this.Length + delta
    end  
  end,
  Append = function (this, value, startIndex, count)
    if not startIndex then
      if value ~= nil then
        value = toString(value)
        if value ~= nil then
          this[#this + 1] = value
          this.Length =  this.Length + #value
        end
      end
    else
      if value == nil then
        throw(ArgumentNullException("value"))
      end
      value = value:Substring(startIndex, count)
      this[#this + 1] = value
      this.Length =  this.Length + #value
    end
    return this
  end,
  AppendChar = function (this, v) 
    v = schar(v)
    this[#this + 1] = v
    this.Length = this.Length + 1
    return this
  end,
  AppendCharRepeat = function (this, v, repeatCount)
    if repeatCount < 0 then throw(ArgumentOutOfRangeException("repeatCount")) end
    if repeatCount == 0 then return this end
    v = schar(v)
    local count = #this + 1
    for i = 1, repeatCount do
      this[count] = v
      count = count + 1
    end
    this.Length = this.Length + repeatCount
    return this
  end,
  AppendFormat = function (this, format, ...)
    local value = format:Format(...)
    this[#this + 1] = value
    this.Length = this.Length + #value
    return this
  end,
  AppendLine = function (this, value)
    local count = 1
    local len = #this + 1
    if value ~= nil then
      this[len] = value
      len = len + 1
      count = count + #value
    end
    this[len] = "\n"
    this.Length = this.Length + count
    return this
  end,
  Clear = function (this)
    clear(this)
    this.Length = 0
    return this
  end,
  Insert = function (this, index, value)
    local length = this.Length
    if value ~= nil then
      if index == length then
        this:Append(value)
      else
        value = toString(value)
        if value ~= nil then
          local i, s, j = getItemIndex(this, index)
          if not i then
            throw(ArgumentOutOfRangeException("index"))
          end
          this[i] = ssub(s, 1, j - 1) .. value .. ssub(s, j)
          this.Length = length + #value
        end
      end
    end
  end
})
System.StringBuilder = StringBuilder
