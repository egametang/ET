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
local define = System.defStc
local equals = System.equals
local zeroFn = System.zeroFn
local identityFn = System.identityFn
local debugsetmetatable = System.debugsetmetatable

local IComparable = System.IComparable
local IComparable_1 = System.IComparable_1
local IEquatable_1 = System.IEquatable_1
local IConvertible = System.IConvertible
local IFormattable = System.IFormattable

local ArgumentException = System.ArgumentException
local ArgumentNullException = System.ArgumentNullException
local FormatException = System.FormatException
local OverflowException = System.OverflowException

local type = type
local tonumber = tonumber
local floor = math.floor
local setmetatable = setmetatable
local tostring = tostring

local function hexForamt(x, n)
  return n == "" and "%" .. x or "%0" .. n .. x
end

local function floatForamt(x, n)
  return n == "" and "%.f" or "%." .. n .. 'f'
end

local function integerFormat(x, n)
  return n == "" and "%d" or "%0" .. n .. 'd'
end

local function exponentialFormat(x, n)
  return n == "" and "%" .. x or "%." .. n .. x
end

local formats = {
  ['x'] = hexForamt,
  ['X'] = hexForamt,
  ['f'] = floatForamt,
  ['F'] = floatForamt,
  ['d'] = integerFormat,
  ['D'] = integerFormat,
  ['e'] = exponentialFormat,
  ['E'] = exponentialFormat
}

local function toStringWithFormat(this, format)
  if #format ~= 0 then
    local i, j, x, n = format:find("^%s*([xXdDfFeE])(%d?)%s*$")
    if i then
      local f = formats[x]
      if f then
        format = f(x, n)
      end
      return format:format(this)
    end
  end
  return tostring(this)
end

local function toString(this, format)
  if format then
    return toStringWithFormat(this, format)
  end
  return tostring(this)
end

local function compareInt(this, v)
  if this < v then return -1 end
  if this > v then return 1 end
  return 0
end

local function inherits(_, T)
  return { IComparable, IComparable_1(T), IEquatable_1(T), IConvertible, IFormattable }
end

local Int = define("System.Int", {
  base = inherits,
  default = zeroFn,
  CompareTo = compareInt,
  Equals = equals,
  ToString = toString,
  GetHashCode = identityFn,
  CompareToObj = function (this, v)
    if v == nil then return 1 end
    if type(v) ~= "number" then
      throw(ArgumentException("Arg_MustBeInt"))
    end
    return compareInt(this, v)
  end,
  EqualsObj = function (this, v)
    if type(v) ~= "number" then
      return false
    end
    return this == v
  end
})
Int.__call = zeroFn

local function parseInt(s, min, max)
  if s == nil then
    return nil, 1        
  end
  local v = tonumber(s)
  if v == nil or v ~= floor(v) then
    return nil, 2
  end
  if v < min or v > max then
    return nil, 3
  end
  return v
end

local function tryParseInt(s, min, max)
  local v = parseInt(s, min, max)
  if v then
    return true, v
  end
  return false, 0
end

local function parseIntWithException(s, min, max)
  local v, err = parseInt(s, min, max)
  if v then
    return v    
  end
  if err == 1 then
    throw(ArgumentNullException())
  elseif err == 2 then
    throw(FormatException())
  else
    throw(OverflowException())
  end
end

local SByte = define("System.SByte", {
  Parse = function (s)
    return parseIntWithException(s, -128, 127)
  end,
  TryParse = function (s)
    return tryParseInt(s, -128, 127)
  end
})
setmetatable(SByte, Int)

local Byte = define("System.Byte", {
  Parse = function (s)
    return parseIntWithException(s, 0, 255)
  end,
  TryParse = function (s)
    return tryParseInt(s, 0, 255)
  end
})
setmetatable(Byte, Int)

local Int16 = define("System.Int16", {
  Parse = function (s)
    return parseIntWithException(s, -32768, 32767)
  end,
  TryParse = function (s)
    return tryParseInt(s, -32768, 32767)
  end
})
setmetatable(Int16, Int)

local UInt16 = define("System.UInt16", {
  Parse = function (s)
    return parseIntWithException(s, 0, 65535)
  end,
  TryParse = function (s)
    return tryParseInt(s, 0, 65535)
  end
})
setmetatable(UInt16, Int)

local Int32 = define("System.Int32", {
  Parse = function (s)
    return parseIntWithException(s, -2147483648, 2147483647)
  end,
  TryParse = function (s)
    return tryParseInt(s, -2147483648, 2147483647)
  end
})
setmetatable(Int32, Int)

local UInt32 = define("System.UInt32", {
  Parse = function (s)
    return parseIntWithException(s, 0, 4294967295)
  end,
  TryParse = function (s)
    return tryParseInt(s, 0, 4294967295)
  end
})
setmetatable(UInt32, Int)

local Int64 = define("System.Int64", {
  Parse = function (s)
    return parseIntWithException(s, -9223372036854775808, 9223372036854775807)
  end,
  TryParse = function (s)
    return tryParseInt(s, -9223372036854775808, 9223372036854775807)
  end
})
setmetatable(Int64, Int)

local UInt64 = define("System.UInt64", {
  Parse = function (s)
    return parseIntWithException(s, 0, 18446744073709551615.0)
  end,
  TryParse = function (s)
    return tryParseInt(s, 0, 18446744073709551615.0)
  end
})
setmetatable(UInt64, Int)

local nan = 0 / 0
local posInf = 1 / 0
local negInf = - 1 / 0
local nanHashCode = {}

--http://lua-users.org/wiki/InfAndNanComparisons
local function isNaN(v)
  return v ~= v
end

local function compareDouble(this, v)
  if this < v then return -1 end
  if this > v then return 1 end
  if this == v then return 0 end
  if isNaN(this) then
    return isNaN(v) and 0 or -1
  else 
    return 1
  end
end

local function equalsDouble(this, v)
  if this == v then return true end
  return isNaN(this) and isNaN(v)
end

local function equalsObj(this, v)
  if type(v) ~= "number" then
    return false
  end
  return equalsDouble(this, v)
end

local function getHashCode(this)
  return isNaN(this) and nanHashCode or this
end

local Number = define("System.Number", {
  base = inherits,
  default = zeroFn,
  CompareTo = compareDouble,
  Equals = equalsDouble,
  ToString = toString,
  NaN = nan,
  IsNaN = isNaN,
  NegativeInfinity = negInf,
  PositiveInfinity = posInf,
  EqualsObj = equalsObj,
  GetHashCode = getHashCode,
  CompareToObj = function (this, v)
    if v == nil then return 1 end
    if type(v) ~= "number" then
      throw(ArgumentException("Arg_MustBeNumber"))
    end
    return compareDouble(this, v)
  end,
  IsFinite = function (v)
    return v ~= posInf and v ~= negInf and not isNaN(v)
  end,
  IsInfinity = function (v)
    return v == posInf or v == negInf
  end,
  IsNegativeInfinity = function (v)
    return v == negInf
  end,
  IsPositiveInfinity = function (v)
    return v == posInf
  end
})
Number.__call = zeroFn
if debugsetmetatable then
  debugsetmetatable(0, Number)
end

local function parseDouble(s)
  if s == nil then
    return nil, 1
  end
  local v = tonumber(s)
  if v == nil then
    return nil, 2
  end
  return v
end

local function parseDoubleWithException(s)
  local v, err = parseDouble(s)
  if v then
    return v    
  end
  if err == 1 then
    throw(ArgumentNullException())
  else
    throw(FormatException())
  end
end

local Single = define("System.Single", {
  Parse = function (s)
    local v = parseDoubleWithException(s)
    if v < -3.40282347E+38 or v > 3.40282347E+38 then
      throw(OverflowException())
    end
    return v
  end,
  TryParse = function (s)
    local v = parseDouble(s)
    if v and v >= -3.40282347E+38 and v < 3.40282347E+38 then
      return true, v
    end
    return false, 0
  end
})
setmetatable(Single, Number)

local Double = define("System.Double", {
  Parse = parseDoubleWithException,
  TryParse = function (s)
    local v = parseDouble(s)
    if v then
      return true, v
    end
    return false, 0
  end
})
setmetatable(Double, Number)

if not debugsetmetatable then
  local NullReferenceException = System.NullReferenceException
  local systemToString = System.toString

  function System.ObjectEqualsObj(this, obj)
    if this == nil then throw(NullReferenceException()) end
    local t = type(this)
    if t == "number" then
      return equalsObj(this, obj)
    elseif t == "table" then
      return this:EqualsObj(obj)
    end
    return this == obj
  end

  function System.ObjectGetHashCode(this)
    if this == nil then throw(NullReferenceException()) end
    local t = type(this)
    if t == "number" then
      return getHashCode(this)
    elseif t == "table" then
      return this:GetHashCode()
    end
    return this
  end

  function System.ObjectToString(this)
    if this == nil then throw(NullReferenceException()) end
    return systemToString(this)
  end

  function System.IComparableCompareTo(this, other)
    if this == nil then throw(NullReferenceException()) end
    local t = type(this)
    if t == "number" then
      return compareDouble(this, other)
    elseif t == "boolean" then
      return System.Boolean.CompareTo(this, other)
    end
    return this:CompareTo(other)
  end

  function System.IEquatableEquals(this, other)
    if this == nil then throw(NullReferenceException()) end
    local t = type(this)
    if t == "number" then
      return equalsDouble(this, other)
    elseif t == "boolean" then
      return System.Boolean.Equals(this, other)
    end
    return this:Equals(other)
  end

  function System.IFormattableToString(this, format, formatProvider)
    if this == nil then throw(NullReferenceException()) end
    local t = type(this)
    if t == "number" then
      return toString(this, format, formatProvider)
    end
    return this:ToString(format, formatProvider)
  end
end

