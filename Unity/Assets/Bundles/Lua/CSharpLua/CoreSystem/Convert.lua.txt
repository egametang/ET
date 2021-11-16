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
local cast = System.cast
local as = System.as
local trunc = System.trunc
local define = System.define
local identityFn = System.identityFn
local IConvertible = System.IConvertible
local systemToString = System.toString

local OverflowException = System.OverflowException
local FormatException = System.FormatException
local ArgumentException = System.ArgumentException
local ArgumentOutOfRangeException = System.ArgumentOutOfRangeException
local ArgumentNullException = System.ArgumentNullException
local InvalidCastException = System.InvalidCastException

local SByte = System.SByte
local Byte = System.Byte
local Int16 = System.Int16
local UInt16 = System.UInt16
local Int32 = System.Int32
local UInt32 = System.UInt32
local Int64 = System.Int64
local UInt64 = System.UInt64
local Single = System.Single
local Double = System.Double
local Boolean = System.Boolean
local Char = System.Char
local DateTime = System.DateTime
local String = System.String
local Object = System.Object

local ParseSByte = SByte.Parse
local ParseByte = Byte.Parse
local ParseInt16 = Int16.Parse
local ParseUInt16 = UInt16.Parse
local ParseInt32 = Int32.Parse
local ParseUInt32 = UInt32.Parse
local ParseInt64 = Int64.Parse
local ParseUInt64 = UInt64.Parse

local ParseSingle = Single.Parse
local ParseDouble = Double.Parse
local ParseBoolean = Boolean.Parse

local type = type
local string = string
local sbyte = string.byte
local math = math
local floor = math.floor
local tconcat = table.concat
local getmetatable = getmetatable
local tonumber = tonumber

local function toBoolean(value)
  if value == nil then return false end
  local typename = type(value)
  if typename == "number" then
    return value ~= 0
  elseif typename == "string" then
    return ParseBoolean(value)  
  elseif typename == "boolean" then
    return value
  else
    return cast(IConvertible, value):ToBoolean()   
  end
end

local function toChar(value)
  if value == nil then return 0 end
  local typename = type(value)
  if typename == "number" then
    if value ~= floor(value) or value > 9223372036854775807 or value < -9223372036854775808 then
      throw(InvalidCastException("InvalidCast_FromTo_Char"))
    end
    if value < 0 or value > 65535 then 
      throw(OverflowException("Overflow_Char")) 
    end
    return value
  elseif typename == "string" then
    if #value ~= 1 then
      throw(FormatException("Format_NeedSingleChar"))
    end
    return sbyte(value)
  else
    return cast(IConvertible, value):ToChar()
  end
end

local function parseBits(s, p, n)
  local i, j, v = s:find(p)
  if not i then
    throw(FormatException())
  end
  v = tonumber(v)
  for i = j + 1, #s do
    local ch = sbyte(s, i)
    local b = ch - 48
    if b < 0 or b >= n then
      if not s:find("^%s*$", i) then
        throw(FormatException())
      end
      break
    end
    v = v * n + b
  end
  return v
end

local function parseNumberFromBase(value, fromBase, min, max)
  if fromBase == 2 then
    value = parseBits(value, "^%s*([01])", fromBase)
  elseif fromBase == 8 then
    value = parseBits(value, "^%s*([0-7])", fromBase)
  elseif fromBase == 16 then
    local _, _, v = value:find("^%s*(%w+)%s*$")
    if not v then
      throw(ArgumentException("String cannot contain a minus sign if the base is not 10."))
    end
    local ch = sbyte(v, 2)
    if ch == 120 or ch == 88 then
    else
      v = "0x" .. v
    end
    value = tonumber(v)
    if value == nil then
      throw(FormatException())
    end
  else
    throw(ArgumentException("fromBase")) 
  end
  if max == 127 and value <= 255 then
    return System.toSByte(value)
  end
  if max == 32767 and value <= 65535 then
    return System.toInt16(value)
  end
  if max == 2147483647 and value <= 4294967295 then
    return System.toInt32(value)
  end
  if value < min or value > max then
    throw(OverflowException())
  end
  return value
end

local function toNumber(value, min, max, parse, objectTo, sign)
  if value == nil then return 0 end
  local typename = type(value)
  if typename == "number" then
    if sign == false then
      value = System.ToSingle(value * 1.0)
    elseif sign == true then
      value = value * 1.0
    else
      local i = value
      value = trunc(value)
      if value ~= i then
        local dif = i - value
        if value >= 0 then
          if dif > 0.5 or (dif == 0.5 and value % 2 ~= 0) then
            value = value + 1  
          end
        else
          if dif < -0.5 or (dif == -0.5 and value % 2 ~= 0) then
            value = value - 1  
          end
        end
      end
      if value < min or value > max then
        throw(OverflowException())
      end
    end
    return value
  elseif typename == "string" then
    if sign and sign ~= 10 and type(sign) == "number" then
      return parseNumberFromBase(value, sign, min, max)
    end
    return parse(value) 
  elseif typename == "boolean" then
    return value and 1 or 0
  else
    return objectTo(value)
  end
end

local function objectToSByte(value)
  return cast(IConvertible, value):ToSByte()
end

local function toSByte(value, fromBase)
  return toNumber(value, -128, 127, ParseSByte, objectToSByte, fromBase)
end

local function objectToByte(value)
  return cast(IConvertible, value):ToByte()
end

local function toByte(value, fromBase)
  return toNumber(value, 0, 255, ParseByte, objectToByte, fromBase) 
end

local function objectToInt16(value)
  return cast(IConvertible, value):ToInt16()
end

local function toInt16(value, fromBase)
  return toNumber(value, -32768, 32767, ParseInt16, objectToInt16, fromBase) 
end

local function objectToUInt16(value)
  return cast(IConvertible, value):ToUInt16()
end

local function toUInt16(value, fromBase)
  return toNumber(value, 0, 65535, ParseUInt16, objectToUInt16, fromBase) 
end

local function objectToInt32(value)
  return cast(IConvertible, value):ToInt32()
end

local function toInt32(value, fromBase)
  return toNumber(value, -2147483648, 2147483647, ParseInt32, objectToInt32, fromBase) 
end

local function objectToUInt32(value)
  return cast(IConvertible, value):ToUInt32()
end

local function toUInt32(value, fromBase)
  return toNumber(value, 0, 4294967295, ParseUInt32, objectToUInt32, fromBase) 
end

local function objectToInt64(value)
  return cast(IConvertible, value):ToInt64()
end

local function toInt64(value, fromBase)
  return toNumber(value, -9223372036854775808, 9223372036854775807, ParseInt64, objectToInt64, fromBase) 
end

local function objectToUInt64(value)
  return cast(IConvertible, value):ToUInt64()
end

local function toUInt64(value, fromBase)
  return toNumber(value, 0, 18446744073709551615.0, ParseUInt64, objectToUInt64, fromBase) 
end

local function objectToSingle(value)
  return cast(IConvertible, value):ToSingle()
end

local function toSingle(value)
  return toNumber(value, nil, nil, ParseSingle, objectToSingle, false) 
end

local function objectToDouble(value)
  return cast(IConvertible, value):ToDouble()
end

local function toDouble(value)
  return toNumber(value, nil, nil, ParseDouble, objectToDouble, true) 
end

local function toDateTime(value)
  if value == nil then return DateTime.MinValue end
  if getmetatable(value) == DateTime then return value end
  if type(value) == "string" then return DateTime.Parse(value) end
  return cast(IConvertible, value):ToDateTime()
end

local function toBaseType(ic, targetType)
  local cls = targetType[1]
  if cls == Boolean then return ic:ToBoolean() end
  if cls == Char then return ic:ToChar() end
  if cls == SByte then return ic:ToSByte() end
  if cls == Byte then return ic:ToByte() end
  if cls == Int16 then return ic:ToInt16() end
  if cls == UInt16 then return ic:ToUInt16() end
  if cls == Int32 then return ic:ToInt32() end
  if cls == UInt32 then return ic:ToUInt32() end
  if cls == Int64 then return ic:ToInt64() end
  if cls == UInt64 then return ic:ToUInt64() end
  if cls == Single then return ic:ToSingle() end
  if cls == Double then return ic:ToDouble() end
  if cls == DateTime then return ic:ToDateTime() end
  if cls == String then return ic:ToString() end
  if cls == Object then return value end
end

local function defaultToType(value, targetType)
  if targetType == nil then throw(ArgumentNullException("targetType")) end
  if value:GetType() == targetType then return value end
  local v = toBaseType(value, targetType)
  if v ~= nil then
    return v
  end
  throw(InvalidCastException())
end

local function changeType(value, conversionType)
  if conversionType == nil then
    throw(ArgumentNullException("conversionType"))
  end
  if value == nil then
    if conversionType:getIsValueType() then
      throw(InvalidCastException("InvalidCast_CannotCastNullToValueType"))
    end
    return nil
  end
  local ic = as(value, IConvertible)
  if ic == nil then
    if value:GetType() == conversionType then
      return value
    end
    throw(InvalidCastException("InvalidCast_IConvertible"))
  end
  local v = toBaseType(ic, conversionType)
  if v ~= nil then
    return v
  end
  return ic.ToType(conversionType)
end

local function toBits(num, bits)
  -- returns a table of bits, most significant first.
  bits = bits or math.max(1, select(2, math.frexp(num)))
  local t = {} -- will contain the bits        
  for b = bits, 1, -1 do
    local i =  num % 2
    t[b] = i
    num = System.div(num - i, 2)
  end
  if bits == 64 and t[1] == 0 then
    return tconcat(t, nil, 2, bits)
  end
  return tconcat(t)
end

local function toString(value, toBaseOrProvider, cast)
  if value == nil then
    return ""
  end
  if toBaseOrProvider then
    if type(toBaseOrProvider) == "number" then
      if toBaseOrProvider ~= 10 then
        if cast and value < 0 then
          value = cast(value)
        end
      end
      if toBaseOrProvider == 2 then
        return toBits(value)
      elseif toBaseOrProvider == 8 then
        return ("%o"):format(value)
      elseif toBaseOrProvider == 10 then
        return value .. ""
      elseif toBaseOrProvider == 16 then
        return ("%x"):format(value)
      else
        throw(ArgumentException())
      end
    end
  end
  return systemToString(value)
end

define("System.Convert", {
  ToBoolean = toBoolean,
  ToChar = toChar,
  ToSByte = toSByte,
  ToByte = toByte,
  ToInt16 = toInt16,
  ToUInt16 = toUInt16,
  ToInt32 = toInt32,
  ToUInt32 = toUInt32,
  ToInt64 = toInt64,
  ToUInt64 = toUInt64,
  ToSingle = toSingle,
  ToDouble = toDouble,
  ToDateTime = toDateTime,
  ChangeType = changeType,
  ToString = toString,
  ToStringFromChar = string.char
})

String.ToBoolean = toBoolean
String.ToChar = toChar
String.ToSByte = toSByte
String.ToByte = toByte
String.ToInt16 = toInt16
String.ToUInt16 = toUInt16
String.ToInt32 = toInt32
String.ToUInt32 = toUInt32
String.ToInt64 = toInt64
String.ToUInt64 = toUInt64
String.ToSingle = identityFn
String.ToDouble = toDouble
String.ToDateTime = toDateTime
String.ToType = defaultToType

local function throwInvalidCastException()
  throw(InvalidCastException())
end

local Number = System.Number
Number.ToBoolean = toBoolean
Number.ToChar = toChar
Number.ToSByte = toSByte
Number.ToByte = toByte
Number.ToInt16 = toInt16
Number.ToUInt16 = toUInt16
Number.ToInt32 = toInt32
Number.ToUInt32 = toUInt32
Number.ToInt64 = toInt64
Number.ToUInt64 = toUInt64
Number.ToSingle = toSingle
Number.ToDouble = toDouble
Number.ToDateTime = throwInvalidCastException
Number.ToType = defaultToType

Boolean.ToBoolean = identityFn
Boolean.ToChar = throwInvalidCastException
Boolean.ToSByte = toSByte
Boolean.ToByte = toByte
Boolean.ToInt16 = toInt16
Boolean.ToUInt16 = toUInt16
Boolean.ToInt32 = toInt32
Boolean.ToUInt32 = toUInt32
Boolean.ToInt64 = toInt64
Boolean.ToUInt64 = toUInt64
Boolean.ToSingle = toSingle
Boolean.ToDouble = toDouble
Boolean.ToDateTime = throwInvalidCastException
Boolean.ToType = defaultToType

DateTime.ToBoolean = throwInvalidCastException
DateTime.ToChar = throwInvalidCastException
DateTime.ToSByte = throwInvalidCastException
DateTime.ToByte = throwInvalidCastException
DateTime.ToInt16 = throwInvalidCastException
DateTime.ToUInt16 = throwInvalidCastException
DateTime.ToInt32 = throwInvalidCastException
DateTime.ToUInt32 = throwInvalidCastException
DateTime.ToInt64 = throwInvalidCastException
DateTime.ToUInt64 = throwInvalidCastException
DateTime.ToSingle = throwInvalidCastException
DateTime.ToDouble = throwInvalidCastException
DateTime.ToDateTime = identityFn
DateTime.ToType = defaultToType


-- BitConverter
local band = System.band
local bor = System.bor
local sl = System.sl
local sr = System.sr
local div = System.div
local global = System.global
local systemToInt16 = System.toInt16
local systemToInt32 = System.toInt32
local systemToUInt64 = System.toUInt64
local arrayFromTable = System.arrayFromTable
local NotSupportedException = System.NotSupportedException

local assert = assert
local rawget = rawget
local unpack = table.unpack
local schar = string.char

-- https://github.com/ToxicFrog/vstruct/blob/master/io/endianness.lua#L30
local isLittleEndian = true
if rawget(global, "jit") then
  if require("ffi").abi("be") then
    isLittleEndian = false
  end
else 
  local dump = string.dump
  if dump and sbyte(dump(System.emptyFn, 7)) == 0x00 then
    isLittleEndian = false
  end
end

local function bytes(t)
  return arrayFromTable(t, Byte)    
end

local function checkIndex(value, startIndex, count)
  if value == nil then throw(ArgumentNullException("value")) end
  local len = #value
  if startIndex < 0 or startIndex >= len then
    throw(ArgumentOutOfRangeException("startIndex"))
  end
  if startIndex > len - count then
    throw(ArgumentException())
  end
end

local spack, sunpack, getBytesFromInt64, toInt64
if System.luaVersion < 5.3 then
  local struct = rawget(global, "struct")
  if struct then
    spack, sunpack = struct.pack, struct.upack
  end
  if not spack then
    spack = function ()
      throw(NotSupportedException("not found struct"), 1) 
    end
    sunpack = spack
  end

  getBytesFromInt64 = function (value)
    if value <= -2147483647 or value >= 2147483647 then
      local s = spack("i8", value)
      return bytes({
        sbyte(s, 1),
        sbyte(s, 2),
        sbyte(s, 3),
        sbyte(s, 4),
        sbyte(s, 5),
        sbyte(s, 6),
        sbyte(s, 7),
        sbyte(s, 8)
      })
    end
    return bytes({
      band(value, 0xff),
      band(sr(value, 8), 0xff),
      band(sr(value, 16), 0xff),
      band(sr(value, 24), 0xff),
      0,
      0,
      0,
      0
    })
  end

  toInt64 = function (value, startIndex)
    checkIndex(value, startIndex, 8)
    if value <= -2147483647 or value >= 2147483647 then
      throw(System.NotSupportedException()) 
    end
    if isLittleEndian then
      local i = value[startIndex + 1]
      i = bor(i, sl(value[startIndex + 2], 8))
      i = bor(i, sl(value[startIndex + 3], 16))
      i = bor(i, sl(value[startIndex + 4], 24))
      return i
    else
      local i = value[startIndex + 8]
      i = bor(i, sl(value[startIndex + 7], 8))
      i = bor(i, sl(value[startIndex + 6], 16))
      i = bor(i, sl(value[startIndex + 5], 24))
      return i
    end
  end
else
  spack, sunpack = string.pack, string.unpack
  getBytesFromInt64 = function (value)
    return bytes({
      band(value, 0xff),
      band(sr(value, 8), 0xff),
      band(sr(value, 16), 0xff),
      band(sr(value, 24), 0xff),
      band(sr(value, 32), 0xff),
      band(sr(value, 40), 0xff),
      band(sr(value, 48), 0xff),
      band(sr(value, 56), 0xff)
    })
  end

  toInt64 = function (value, startIndex)
    checkIndex(value, startIndex, 8)
    if isLittleEndian then
      local i = value[startIndex + 1]
      i = bor(i, sl(value[startIndex + 2], 8))
      i = bor(i, sl(value[startIndex + 3], 16))
      i = bor(i, sl(value[startIndex + 4], 24))
      i = bor(i, sl(value[startIndex + 5], 32))
      i = bor(i, sl(value[startIndex + 6], 40))
      i = bor(i, sl(value[startIndex + 7], 48))
      i = bor(i, sl(value[startIndex + 8], 56))
      return i
    else
      local i = value[startIndex + 8]
      i = bor(i, sl(value[startIndex + 7], 8))
      i = bor(i, sl(value[startIndex + 6], 16))
      i = bor(i, sl(value[startIndex + 5], 24))
      i = bor(i, sl(value[startIndex + 4], 32))
      i = bor(i, sl(value[startIndex + 3], 40))
      i = bor(i, sl(value[startIndex + 2], 48))
      i = bor(i, sl(value[startIndex + 1], 56))
      return i
    end
  end
end

local function getBytesFromBoolean(value)
  return bytes({ value and 1 or 0 })
end

local function getBytesFromInt16(value)
  return bytes({
    band(value, 0xff),
    band(sr(value, 8), 0xff),
  })
end

local function getBytesFromInt32(value)
  return bytes({
    band(value, 0xff),
    band(sr(value, 8), 0xff),
    band(sr(value, 16), 0xff),
    band(sr(value, 24), 0xff)
  })
end

local function getBytesFromFloat(value)
  local s = spack("f", value)
  return bytes({
    sbyte(s, 1),
    sbyte(s, 2),
    sbyte(s, 3),
    sbyte(s, 4)
  })
end

local function getBytesFromDouble(value)
  local s = spack("d", value)
  return bytes({
    sbyte(s, 1),
    sbyte(s, 2),
    sbyte(s, 3),
    sbyte(s, 4),
    sbyte(s, 5),
    sbyte(s, 6),
    sbyte(s, 7),
    sbyte(s, 8)
  })
end

local function toBoolean(value, startIndex)
  checkIndex(value, startIndex, 1)
  return value[startIndex + 1] ~= 0 and true or false
end

local function toUInt16(value, startIndex)
  checkIndex(value, startIndex, 2)
  if isLittleEndian then
    value = bor(value[startIndex + 1], sl(value[startIndex + 2], 8))
  else
    value = bor(sl(value[startIndex + 1], 8), value[startIndex + 2])
  end
  return value
end

local function toInt16(value, startIndex)
  value = toUInt16(value, startIndex)
  return systemToInt16(value)
end

local function toUInt32(value, startIndex)
  checkIndex(value, startIndex, 4)
  local i
  if isLittleEndian then
    i = value[startIndex + 1]
    i = bor(i, sl(value[startIndex + 2], 8))
    i = bor(i, sl(value[startIndex + 3], 16))
    i = bor(i, sl(value[startIndex + 4], 24))
  else
    local i = value[startIndex + 4]
    i = bor(i, sl(value[startIndex + 3], 8))
    i = bor(i, sl(value[startIndex + 2], 16))
    i = bor(i, sl(value[startIndex + 1], 24))
  end
  return i
end

local function toInt32(value, startIndex)
  value = toUInt32(value, startIndex)
  return systemToInt32(value)
end

local function toUInt64(value, startIndex)
  value = toInt64(value, startIndex)
  return systemToUInt64(value)
end

local function toSingle(value, startIndex)
  checkIndex(value, startIndex, 4)
  return sunpack("f", schar(unpack(value, startIndex + 1)))
end

local function toDouble(value, startIndex)
  checkIndex(value, startIndex, 8)
  return sunpack("d", schar(unpack(value, startIndex + 1)))
end

local function getHexValue(i)
  assert(i >= 0 and i < 16, "i is out of range.")
  if i < 10 then
    return i + 48
  end
  return i - 10 + 65
end

local function toString(value, startIndex, length)
  if value == nil then throw(ArgumentNullException("value")) end
  local len = #value
  if not startIndex then
    startIndex, length = 0, #value
  elseif not length then
    length = len - startIndex
  end
  if startIndex < 0 or (startIndex >= len and startIndex > 0) then
    throw(ArgumentOutOfRangeException("startIndex"))
  end
  if length < 0 then
    throw(ArgumentOutOfRangeException("length"))
  end
  if startIndex + length > len then
    throw(ArgumentException())
  end
  if length == 0 then
    return ""
  end
  local t = {}
  local len = 1
  for i = startIndex + 1, startIndex + length  do
    local b = value[i]
    t[len] = getHexValue(div(b, 16))
    t[len + 1] = getHexValue(b % 16)
    t[len + 2] = 45
    len = len + 3
  end
  return schar(unpack(t, 1, len - 2))
end

local function doubleToInt64Bits(value)
  assert(isLittleEndian, "This method is implemented assuming little endian with an ambiguous spec.")
  local s = spack("d", value)
  return (sunpack("i8", s))
end

local function int64BitsToDouble(value)
  assert(isLittleEndian, "This method is implemented assuming little endian with an ambiguous spec.")
  local s = spack("i8", value)
  return (sunpack("d", s))
end

define("System.BitConverter", {
  IsLittleEndian = isLittleEndian,
  GetBytesFromBoolean = getBytesFromBoolean,
  GetBytesFromInt16 = getBytesFromInt16,
  GetBytesFromInt32 = getBytesFromInt32,
  GetBytesFromInt64 = getBytesFromInt64,
  GetBytesFromFloat = getBytesFromFloat,
  GetBytesFromDouble = getBytesFromDouble,
  ToBoolean = toBoolean,
  ToChar = toUInt16,
  ToInt16 = toInt16,
  ToUInt16 = toUInt16,
  ToInt32 = toInt32,
  ToUInt32 = toUInt32,
  ToInt64 = toInt64,
  ToUInt64 = toUInt64,
  ToSingle = toSingle,
  ToDouble = toDouble,
  ToString = toString,
  DoubleToInt64Bits = doubleToInt64Bits,
  Int64BitsToDouble = int64BitsToDouble
})
