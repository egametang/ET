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
local ArgumentNullException = System.ArgumentNullException
local ArgumentOutOfRangeException = System.ArgumentOutOfRangeException

local setmetatable = setmetatable
local byte = string.byte

local isSeparatorTable = {
  [32] = true,
  [160] = true,
  [0x2028] = true,
  [0x2029] = true,
  [0x0020] = true,
  [0x00A0] = true,
  [0x1680] = true,
  [0x180E] = true,
  [0x202F] = true,
  [0x205F] = true,
  [0x3000] = true,
}

local isSymbolTable = {
  [36] = true,
  [43] = true,
  [60] = true, 
  [61] = true, 
  [62] = true, 
  [94] = true, 
  [96] = true,
  [124] = true,
  [126] = true,
  [172] = true, 
  [180] = true,
  [182] = true,
  [184] = true,
  [215] = true,
  [247] = true,
}

--https://msdn.microsoft.com/zh-cn/library/t809ektx(v=vs.110).aspx
local isWhiteSpace = {
  [0x0020] = true,
  [0x00A0] = true,
  [0x1680] = true,
  [0x202F] = true,
  [0x205F] = true,
  [0x3000] = true,
  [0x2028] = true,
  [0x2029] = true,
  [0x0085] = true,
}

local function get(s, index)
  if s == nil then throw(ArgumentNullException("s")) end
  local c = byte(s, index + 1)
  if not c then throw(ArgumentOutOfRangeException("index")) end
  return c
end

local function isDigit(c, index)
  if index then
    c = get(c, index)
  end
  return (c >= 48 and c <= 57)
end

-- https://msdn.microsoft.com/zh-cn/library/yyxz6h5w(v=vs.110).aspx
local function isLetter(c, index)    
  if index then
    c = get(c, index) 
  end
  if c < 128 then
    return (c >= 65 and c <= 90) or (c >= 97 and c <= 122)
  else  
    return (c >= 0x0400 and c <= 0x042F) 
      or (c >= 0x03AC and c <= 0x03CE) 
      or (c == 0x01C5 or c == 0x1FFC) 
      or (c >= 0x02B0 and c <= 0x02C1) 
      or (c >= 0x1D2C and c <= 0x1D61) 
      or (c >= 0x05D0 and c <= 0x05EA)
      or (c >= 0x0621 and c <= 0x063A)
      or (c >= 0x4E00 and c <= 0x9FC3) 
  end
end

local Char = System.defStc("System.Char", {
  ToString = string.char,
  CompareTo = Int.CompareTo,
  CompareToObj = Int.CompareToObj,
  Equals = Int.Equals,
  EqualsObj = Int.EqualsObj,
  GetHashCode = Int.GetHashCode,
  default = Int.default,
  IsControl = function (c, index)
    if index then
      c = get(c, index)
    end
    return (c >=0 and c <= 31) or (c >= 127 and c <= 159)
  end,
  IsDigit = isDigit,
  IsLetter = isLetter,
  IsLetterOrDigit = function (c, index)
    if index then
      c = get(c, index)
    end
    return isDigit(c) or isLetter(c)
  end,
  IsLower = function (c, index)
    if index then
      c = get(c, index)
    end
    return (c >= 97 and c <= 122) or (c >= 945 and c <= 969)
  end,
  IsNumber = function (c, index)
    if index then
      c = get(c, index)
    end
    return (c >= 48 and c <= 57) or c == 178 or c == 179 or c == 185 or c == 188 or c == 189 or c == 190
  end,
  IsPunctuation = function (c, index)
    if index then
      c = get(c, index)
    end
    if c < 256 then
      return (c >= 0x0021 and c <= 0x0023) 
        or (c >= 0x0025 and c <= 0x002A) 
        or (c >= 0x002C and c <= 0x002F) 
        or (c >= 0x003A and c <= 0x003B) 
        or (c >= 0x003F and c <= 0x0040)  
        or (c >= 0x005B and c <= 0x005D)
        or c == 0x5F or c == 0x7B or c == 0x007D or c == 0x00A1 or c == 0x00AB or c == 0x00AD or c == 0x00B7 or c == 0x00BB or c == 0x00BF
    end
    return false
  end,
  IsSeparator = function (c, index)
    if index then
      c = get(c, index)
    end
    return (c >= 0x2000 and c <= 0x200A) or isSeparatorTable[c] == true
  end,
  IsSymbol = function (c, index)
    if index then
      c = get(c, index)
    end
    if c < 256 then
      return (c >= 162 and c <= 169) or (c >= 174 and c <= 177) or isSymbolTable(c) == true
    end
    return false
  end,
  IsUpper = function (c, index)
    if index then
      c = get(c, index)
    end
    return (c >= 65 and c <= 90) or (c >= 913 and c <= 937)
  end,
  IsWhiteSpace = function (c, index)
    if index then
      c = get(c, index)
    end
    return (c >= 0x2000 and c <= 0x200A) or (c >= 0x0009 and c <= 0x000d) or isWhiteSpace[c] == true
  end,
  Parse = function (s)
    if s == nil then
      throw(System.ArgumentNullException())
    end
    if #s ~= 1 then
      throw(System.FormatException())
    end
    return s:byte()
  end,
  TryParse = function (s)
    if s == nil or #s ~= 1 then
      return false, 0
    end 
    return true, s:byte()
  end,
  ToLower = function (c)
    if (c >= 65 and c <= 90) or (c >= 913 and c <= 937) then
      return c + 32
    end
    return c
  end,
  ToUpper = function (c)
    if (c >= 97 and c <= 122) or (c >= 945 and c <= 969) then
      return c - 32
    end
    return c
  end,
  IsHighSurrogate = function (c, index) 
    if index then
      c = get(c, index)
    end
    return c >= 0xD800 and c <= 0xDBFF
  end,
  IsLowSurrogate = function (c, index) 
    if index then
      c = get(c, index)
    end
    return c >= 0xDC00 and c <= 0xDFFF
  end,
  IsSurrogate = function (c, index) 
    if index then
      c = get(c, index)
    end
    return c >= 0xD800 and c <= 0xDFFF
  end,
  base = function (_, T)
    return { System.IComparable, System.IComparable_1(T), System.IEquatable_1(T) }
  end
})

local ValueType = System.ValueType
local charMetaTable = setmetatable({ __index = ValueType, __call = Char.default }, ValueType)
setmetatable(Char, charMetaTable)
