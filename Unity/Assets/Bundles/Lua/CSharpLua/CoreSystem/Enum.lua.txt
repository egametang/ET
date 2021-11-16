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
local Int = System.Int
local Number = System.Number
local band = System.band
local bor = System.bor
local ArgumentNullException = System.ArgumentNullException
local ArgumentException = System.ArgumentException

local assert = assert
local pairs = pairs
local tostring = tostring
local type = type

local function toString(this, cls)
  if this == nil then return "" end
  if cls then
    for k, v in pairs(cls) do
      if v == this then
        return k
      end
    end
  end
  return tostring(this)
end

local function hasFlag(this, flag)
  if this == flag then
    return true
  end
  return band(this, flag) ~= 0
end

Number.EnumToString = toString
Number.HasFlag = hasFlag
System.EnumToString = toString
System.EnumHasFlag = hasFlag

local function tryParseEnum(enumType, value, ignoreCase)
  if enumType == nil then throw(ArgumentNullException("enumType")) end
  local cls = enumType[1] or enumType
  if cls.class ~= "E" then throw(ArgumentException("Arg_MustBeEnum")) end
  if value == nil then
    return
  end
  if ignoreCase then
    value = value:lower()
  end
  local i, j, s, r = 1
  while true do
    i, j, s = value:find("%s*(%a+)%s*", i)
    if not i then
      return
    end
    for k, v in pairs(cls) do
      if ignoreCase then
        k = k:lower()
      end
      if k == s then
        if not r then
          r = v
        else
          r = bor(r, v)
        end
        break
      end
    end
    i = value:find(',', j + 1)
    if not i then
      break
    end
    i = i + 1
  end
  return r
end

System.define("System.Enum", {
  CompareToObj = Int.CompareToObj,
  EqualsObj = Int.EqualsObj,
  default = Int.default,
  ToString = toString,
  HasFlag = hasFlag,
  GetName = function (enumType, value)
    if enumType == nil then throw(ArgumentNullException("enumType")) end
    if value == nil then throw(ArgumentNullException("value")) end
    if not enumType:getIsEnum() then throw(ArgumentException("Arg_MustBeEnum")) end
    for k, v in pairs(enumType[1]) do
      if v == value then
        return k
      end
    end
  end,
  GetNames = function (enumType)
    if enumType == nil then throw(ArgumentNullException("enumType")) end
    if not enumType:getIsEnum() then throw(ArgumentException("Arg_MustBeEnum")) end
    local t = {}
    local count = 1
    for k, v in pairs(enumType[1]) do
      if type(v) == "number" then
        t[count] = k
        count = count + 1
      end
    end
    return System.arrayFromTable(t, System.String)
  end,
  GetValues = function (enumType)
    if enumType == nil then throw(ArgumentNullException("enumType")) end
    if not enumType:getIsEnum() then throw(ArgumentException("Arg_MustBeEnum")) end
    local t = {}
    local count = 1
    for k, v in pairs(enumType[1]) do
      if type(v) == "number" then
        t[count] = v
        count = count + 1
      end
    end
    return System.arrayFromTable(t, System.Int32)
  end,
  IsDefined = function (enumType, value)
    if enumType == nil then throw(ArgumentNullException("enumType")) end
    if value == nil then throw(ArgumentNullException("value")) end
    if not enumType:getIsEnum() then throw(ArgumentException("Arg_MustBeEnum")) end
    local cls = enumType[1]
    local t = type(value)
    if t == "string" then
      return cls[value] ~= nil
    elseif t == "number" then
      for k, v in pairs(cls) do
        if v == value then
          return true
        end
      end
      return false
    end
    throw(System.InvalidOperationException())
  end,
  Parse = function (enumType, value, ignoreCase)
    local result = tryParseEnum(enumType, value, ignoreCase)
    if result == nil then
      throw(ArgumentException("Requested value '" .. value .. "' was not found."))
    end
    return result
  end,
  TryParse = function (type, value, ignoreCase)
    local result = tryParseEnum(type, value, ignoreCase)
    if result == nil then
      return false, 0
    end
    return true, result
  end
})
