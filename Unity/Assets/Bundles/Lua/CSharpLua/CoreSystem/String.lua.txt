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
local Char = System.Char
local throw = System.throw
local emptyFn = System.emptyFn
local lengthFn = System.lengthFn
local systemToString = System.toString
local debugsetmetatable = System.debugsetmetatable
local ArgumentException = System.ArgumentException
local ArgumentNullException = System.ArgumentNullException
local ArgumentOutOfRangeException = System.ArgumentOutOfRangeException
local FormatException = System.FormatException
local IndexOutOfRangeException = System.IndexOutOfRangeException

local string = string
local char = string.char
local rep = string.rep
local lower = string.lower
local upper = string.upper
local byte = string.byte
local sub = string.sub
local find = string.find
local gsub = string.gsub

local table = table
local tconcat = table.concat
local unpack = table.unpack
local getmetatable = getmetatable
local setmetatable = setmetatable
local select = select
local type = type
local tonumber = tonumber
local String

local function toString(t, isch, format)
  if isch then return char(t) end
  return systemToString(t, format)
end

local function checkIndex(value, startIndex, count)
  if value == nil then throw(ArgumentNullException("value")) end
  local len = #value
  if not startIndex then
    startIndex, count = 0, len
  elseif not count then
    if startIndex < 0 or startIndex > len then
      throw(ArgumentOutOfRangeException("startIndex"))
    end
    count = len - startIndex
  else
    if startIndex < 0 or startIndex > len then
      throw(ArgumentOutOfRangeException("startIndex"))
    end
    if count < 0 or count > len - startIndex then
      throw(ArgumentOutOfRangeException("count"))
    end
  end
  return startIndex, count, len
end

local function ctor(String, value, startIndex, count)
  if type(value) == "number" then
    if startIndex <= 0 then throw(ArgumentOutOfRangeException("count")) end
    return rep(char(value), startIndex)
  end
  startIndex, count = checkIndex(value, startIndex, count)
  return char(unpack(value, startIndex + 1, startIndex + count))
end

local function get(this, index)
  local c = byte(this, index + 1)
  if not c then
    throw(IndexOutOfRangeException())
  end
  return c
end

local function compare(strA, strB, ignoreCase)
  if strA == nil then
    if strB == nil then
      return 0
    end
    return -1
  elseif strB == nil then
    return 1
  end

  if ignoreCase then
    strA, strB = lower(strA), lower(strB)
  end

  if strA < strB then return -1 end
  if strA > strB then return 1 end
  return 0
end

local function compareFull(...)
  local n = select("#", ...)
  if n == 2 then
    return compare(...)
  elseif n == 3 then
    local strA, strB, ignoreCase = ...
    if type(ignoreCase) == "number" then
      ignoreCase = ignoreCase % 2 ~= 0
    end
    return compare(strA, strB, ignoreCase)
  elseif n == 4 then
    local strA, strB, ignoreCase, options = ...
    if type(options) == "number" then
      ignoreCase = options == 1 or options == 268435456
    end
    return compare(strA, strB, ignoreCase)
  else
    local strA, indexA, strB, indexB, length, ignoreCase, options = ...
    if type(ignoreCase) == "number" then
      ignoreCase = ignoreCase % 2 ~= 0
    elseif type(options) == "number" then
      ignoreCase = options == 1 or options == 268435456
    end
    checkIndex(strA, indexA, length)
    checkIndex(strB, indexB, length)
    strA, strB = sub(strA, indexA + 1, indexA +  length), sub(strB, indexB + 1, indexB + length)
    return compare(strA, strB, ignoreCase) 
  end
end

local function concat(...)
  local t = {}
  local count = 1
  local len = select("#", ...)
  if len == 1 then
    local v = ...
    if System.isEnumerableLike(v) then
      local isch = v.__genericT__ == Char
      for _, v in System.each(v) do
        t[count] = toString(v, isch)
        count = count + 1
      end
    else
      return toString(v)
    end
  else
    for i = 1, len do
      local v = select(i, ...)
      t[count] = toString(v)
      count = count + 1
    end
  end
  return tconcat(t)
end

local function equals(this, value, comparisonType)
  if not comparisonType then
    return this == value
  end
  return compare(this, value, comparisonType % 2 ~= 0) == 0
end

local function throwFormatError()
  throw(FormatException("Input string was not in a correct format."))
end

local function formatBuild(format, len, select, ...)
  local t, count = {}, 1
  local i, j, s = 1
  while true do
    local startPos  = i
    while true do
      i, j, s = find(format, "([{}])", i)
      if not i then
        if count == 1 then
          return format
        end
        t[count] = sub(format, startPos)
        return table.concat(t)
      end
      local pos = i - 1
      i = i + 1
      local c = byte(format, i)
      if not c then throwFormatError() end
      if s == '{' then
        if c == 123 then
          i = i + 1
        else
          pos = i - 2
          if pos >= startPos then
            t[count] = sub(format, startPos, pos)
            count = count + 1
          end
          break
        end
      else
        if c == 125 then
          i = i + 1
        else
          throwFormatError()
        end
      end
      if pos >= startPos then
        t[count] = sub(format, startPos, pos)
        count = count + 1
      end
      t[count] = s
      count = count + 1
      startPos = i
    end
    local r, alignment, formatString
    i, j, s, r = find(format, "^(%d+)(.-)}", i)
    if not i then throwFormatError() end
    s = tonumber(s) + 1
    if s > len then throwFormatError() end
    if r ~= "" then
      local i, j, c, d = find(r, "^,([-]?)(%d+)")
      if i then
        alignment = tonumber(d)
        if c == '-' then alignment = -alignment end
        i = j + 1
      end
      i, j, c = find(r, "^:(.*)$", i)
      if i then
        formatString = c
      elseif not alignment then
        throwFormatError()
      end
    end
    s = select(s, ...)
    if s ~= nil and s ~= System.null then
      s = toString(s, false, formatString)
      if alignment then
        s = ("%" .. alignment .. "s"):format(s)
      end
      t[count] = s
      count = count + 1
    end
    i = j + 1
  end
end

local function selectTable(i, t)
  return t[i]
end

local function format(format, ...)
  if format == nil then throw(ArgumentNullException()) end
  local len = select("#", ...)
  if len == 1 then
    local args = ...
    if System.isArrayLike(args) then
      return formatBuild(format, #args, selectTable, args)
    end
  end
  return formatBuild(format, len, select, ...)
end

local function isNullOrEmpty(value)
  return value == nil or #value == 0
end

local function isNullOrWhiteSpace(value)
  return value == nil or find(value, "^%s*$") ~= nil
end

local function joinEnumerable(separator, values)
  if values == nil then throw(ArgumentNullException("values")) end
  if type(separator) == "number" then
    separator = char(separator)
  end
  local isch = values.__genericT__ == Char
  local t = {}
  local len = 1
  for _, v in System.each(values) do
    if v ~= nil then
      t[len] = toString(v, isch)
      len = len + 1
    end
  end
  return tconcat(t, separator)
end

local function joinParams(separator, ...)
  if type(separator) == "number" then
    separator = char(separator)
  end
  local t = {}
  local len = 1
  local n = select("#", ...)
  if n == 1 then
    local values = ...
    if System.isArrayLike(values) then
      for i = 0, #values - 1 do
        local v = values:get(i)
        if v ~= nil then
          t[len] = toString(v)
          len = len + 1
        end
      end
      return tconcat(t, separator) 
    end
  end
  for i = 1, n do
    local v = select(i, ...)
    if v ~= nil then
      t[len] = toString(v)
      len = len + 1
    end
  end
  return tconcat(t, separator) 
end

local function join(separator, value, startIndex, count)
  if type(separator) == "number" then
    separator = char(separator)
  end
  local t = {}
  local len = 1
  if startIndex then  
    checkIndex(value, startIndex, count)
    for i = startIndex + 1, startIndex + count do
      local v = value[i]
      if v ~= System.null then
        t[len] = v
        len = len + 1
      end
    end
  else
    for _, v in System.each(value) do
      if v ~= nil then
        t[len] = v
        len = len + 1
      end
    end
  end
  return tconcat(t, separator)
end

local function compareToObj(this, v)
  if v == nil then return 1 end
  if type(v) ~= "string" then
    throw(ArgumentException("Arg_MustBeString"))
  end
  return compare(this, v)
end

local function escape(s)
  return gsub(s, "([%%%^%.])", "%%%1")
end

local function contains(this, value, comparisonType)
  if value == nil then throw(ArgumentNullException("value")) end
  if type(value) == "number" then
    value = char(value)
  end
  if comparisonType then
    local ignoreCase = comparisonType % 2 ~= 0
    if ignoreCase then
      this, value = lower(this), lower(value)
    end
  end 
  return find(this, escape(value)) ~= nil
end

local function copyTo(this, sourceIndex, destination, destinationIndex, count)
  if destination == nil then throw(ArgumentNullException("destination")) end
  if count < 0 then throw(ArgumentOutOfRangeException("count")) end
  local len = #this
  if sourceIndex < 0 or count > len - sourceIndex then throw(ArgumentOutOfRangeException("sourceIndex")) end
  if destinationIndex > #destination - count or destinationIndex < 0 then throw(ArgumentOutOfRangeException("destinationIndex")) end
  if count > 0 then
    destinationIndex = destinationIndex + 1
    for i = sourceIndex + 1, sourceIndex + count do
      destination[destinationIndex] = byte(this, i)
      destinationIndex = destinationIndex + 1
    end
  end
end

local function endsWith(this, suffix)
  return suffix == "" or sub(this, -#suffix) == suffix
end

local function equalsObj(this, v)
  if type(v) == "string" then
    return this == v
  end
  return false
end

local CharEnumerator = System.define("System.CharEnumerator", {
  base = { System.IEnumerator_1(System.Char), System.IDisposable, System.ICloneable },
  getCurrent = System.getCurrent,
  Dispose = emptyFn,
  MoveNext = function (this)
    local index, s = this.index, this.s
    if index <= #s then
      this.current = byte(s, index)
      this.index = index + 1
      return true
    end
    return false
  end
})

local function getEnumerator(this)
  return setmetatable({ s = this, index = 1 }, CharEnumerator)
end

local function getTypeCode()
  return 18
end

local function indexOf(this, value, startIndex, count, comparisonType)
  if value == nil then throw(ArgumentNullException("value")) end
  startIndex, count = checkIndex(this, startIndex, count)
  if type(value) == "number" then value = char(value) end
  local ignoreCase = comparisonType and comparisonType % 2 ~= 0
  if ignoreCase then
    this, value = lower(this), lower(value)
  end
  local i, j = find(this, escape(value), startIndex + 1)
  if i then
    local e = startIndex + count
    if j <= e then
      return i - 1
    end
    return - 1
  end
  return -1
end

local function indexOfAny(this, anyOf, startIndex, count)
  if anyOf == nil then throw(ArgumentNullException("chars")) end
  startIndex, count = checkIndex(this, startIndex, count)
  anyOf = "[" .. escape(char(unpack(anyOf))) .. "]"
  local i, j = find(this, anyOf, startIndex + 1)
  if i then
    local e = startIndex + count
    if j <= e then
      return i - 1
    end
    return - 1
  end
  return -1
end

local function insert(this, startIndex, value) 
  if value == nil then throw(ArgumentNullException("value")) end
  if startIndex < 0 or startIndex > #this then throw(ArgumentOutOfRangeException("startIndex")) end
  return sub(this, 1, startIndex) .. value .. sub(this, startIndex + 1)
end

local function chechLastIndexOf(value, startIndex, count)
  if value == nil then throw(ArgumentNullException("value")) end
  local len = #value
  if not startIndex then
    startIndex, count = len - 1, len
  elseif not count then
    count = len == 0 and 0 or (startIndex + 1)
  end
  if len == 0 then
    if startIndex ~= -1 and startIndex ~= 0 then
      throw(ArgumentOutOfRangeException("startIndex"))
    end
    if count ~= 0 then
      throw(ArgumentOutOfRangeException("count"))
    end
  end
  if startIndex < 0 or startIndex >= len then
    throw(ArgumentOutOfRangeException("startIndex"))
  end
  if count < 0 or startIndex - count + 1 < 0 then
    throw(ArgumentOutOfRangeException("count"))
  end
  return startIndex, count, len
end

local function lastIndexOf(this, value, startIndex, count, comparisonType)
  if value == nil then throw(ArgumentNullException("value")) end
  startIndex, count = chechLastIndexOf(this, startIndex, count)
  if type(value) == "number" then value = char(value) end
  local ignoreCase = comparisonType and comparisonType % 2 ~= 0
  if ignoreCase then
    this, value = lower(this), lower(value)
  end
  value = escape(value)
  local e = startIndex + 1
  local f = e - count + 1
  local index = -1  
  while true do
    local i, j = find(this, value, f)
    if not i or j > e then
      break
    end
    index = i - 1
    f = j + 1
  end
  return index
end

local function lastIndexOfAny(this, anyOf, startIndex, count)
  if anyOf == nil then throw(ArgumentNullException("chars")) end
  startIndex, count = chechLastIndexOf(this, startIndex, count)
  anyOf = "[" .. escape(char(unpack(anyOf))) .. "]"
  local f, e = startIndex - count + 1, startIndex + 1
  local index = -1
  while true do
    local i, j = find(this, anyOf, f)
    if not i or j > e then
      break
    end
    index = i - 1
    f = j + 1
  end
  return index
end

local function padLeft(this, totalWidth, paddingChar) 
  local len = #this;
  if len >= totalWidth then
    return this
  else
    paddingChar = paddingChar or 0x20
    return rep(char(paddingChar), totalWidth - len) .. this
  end
end

local function padRight(this, totalWidth, paddingChar) 
  local len = #this
  if len >= totalWidth then
    return this
  else
    paddingChar = paddingChar or 0x20
    return this .. rep(char(paddingChar), totalWidth - len)
  end
end

local function remove(this, startIndex, count) 
  startIndex, count = checkIndex(this, startIndex, count)
  return sub(this, 1, startIndex) .. sub(this, startIndex + 1 + count)
end

local function replace(this, a, b)
  if type(a) == "number" then
    a, b = char(a), char(b)
  end
  return gsub(this, escape(a), b)
end

local function findAny(s, strings, startIndex)
  local findBegin, findEnd
  for i = 1, #strings do
    local posBegin, posEnd = find(s, escape(strings[i]), startIndex)
    if posBegin then
      if not findBegin or posBegin < findBegin then
        findBegin, findEnd = posBegin, posEnd
      else
        break
      end
    end
  end
  return findBegin, findEnd
end

local function split(this, strings, count, options) 
  local t = {}
  local find = find
  if type(strings) == "table" then
    if #strings == 0 then
      return t
    end

    if type(strings[1]) == "string" then
      find = findAny
    else
      strings = char(unpack(strings))
      strings = escape(strings)
      strings = "[" .. strings .. "]"
    end
  elseif type(strings) == "string" then       
    strings = escape(strings)         
  else
    strings = char(strings)
    strings = escape(strings)
  end

  local len = 1
  local startIndex = 1
  while true do
    local posBegin, posEnd = find(this, strings, startIndex)
    posBegin = posBegin or 0
    local subStr = sub(this, startIndex, posBegin -1)
    if options ~= 1 or #subStr > 0 then
      t[len] = subStr
      len = len + 1
      if count then
        count = count -1
        if count == 0 then
          if posBegin ~= 0 then
            t[len - 1] = sub(this, startIndex)
          end
          break
        end
      end
    end
    if posBegin == 0 then
      break
    end 
    startIndex = posEnd + 1
  end   
  return System.arrayFromTable(t, String) 
end

local function startsWith(this, prefix)
  return sub(this, 1, #prefix) == prefix
end

local function substring(this, startIndex, count)
  startIndex, count = checkIndex(this, startIndex, count)
  return sub(this, startIndex + 1, startIndex + count)
end

local function toCharArray(str, startIndex, count)
  startIndex, count = checkIndex(str, startIndex, count)
  local t = {}
  local len = 1
  for i = startIndex + 1, startIndex + count do
    t[len] = byte(str, i)
    len = len + 1
  end
  return System.arrayFromTable(t, System.Char)
end

local function trim(this, chars, ...)
  if not chars then
    chars = "^%s*(.-)%s*$"
  else
    if type(chars) == "table" then
      chars = char(unpack(chars))
    else
      chars = char(chars, ...)
    end
    chars = escape(chars)
    chars = "^[" .. chars .. "]*(.-)[" .. chars .. "]*$"
  end
  return (gsub(this, chars, "%1"))
end

local function trimEnd(this, chars, ...)
  if not chars then
    chars = "(.-)%s*$"
  else
    if type(chars) == "table" then
      chars = char(unpack(chars))
    else
      chars = char(chars, ...)
    end
    chars = escape(chars)
    chars = "(.-)[" .. chars .. "]*$"
  end
  return (gsub(this, chars, "%1"))
end

local function trimStart(this, chars, ...)
  if not chars then
    chars = "^%s*(.-)"
  else
    if type(chars) == "table" then
      chars = char(unpack(chars))
    else
      chars = char(chars, ...)
    end
    chars = escape(chars)
    chars = "^[" .. chars .. "]*(.-)"
  end
  return (gsub(this, chars, "%1"))
end

local function inherits(_, T)
  return { System.IEnumerable_1(System.Char), System.IComparable, System.IComparable_1(T), System.IConvertible, System.IEquatable_1(T), System.ICloneable }
end

string.traceback = emptyFn  -- make throw(str) not fail
string.getLength = lengthFn
string.getCount = lengthFn
string.get = get
string.Compare = compareFull
string.CompareOrdinal = compareFull
string.Concat = concat
string.Copy = System.identityFn
string.Equals = equals
string.Format = format
string.IsNullOrEmpty = isNullOrEmpty
string.IsNullOrWhiteSpace = isNullOrWhiteSpace
string.JoinEnumerable = joinEnumerable
string.JoinParams = joinParams
string.Join = join
string.CompareTo = compare
string.CompareToObj = compareToObj
string.Contains = contains
string.CopyTo = copyTo
string.EndsWith = endsWith
string.EqualsObj = equalsObj
string.GetEnumerator = getEnumerator
string.GetTypeCode = getTypeCode
string.IndexOf = indexOf
string.IndexOfAny = indexOfAny
string.Insert = insert
string.LastIndexOf = lastIndexOf
string.LastIndexOfAny = lastIndexOfAny
string.PadLeft = padLeft
string.PadRight = padRight
string.Remove = remove
string.Replace = replace
string.Split = split
string.StartsWith = startsWith
string.Substring = substring
string.ToCharArray = toCharArray
string.ToLower = lower
string.ToLowerInvariant = lower
string.ToString = System.identityFn
string.ToUpper = upper
string.ToUpperInvariant = upper
string.Trim = trim
string.TrimEnd = trimEnd
string.TrimStart = trimStart

if debugsetmetatable then
  String = string
  String.__genericT__ = System.Char
  String.base = inherits
  System.define("System.String", String)

  debugsetmetatable("", String)
  local Object = System.Object
  local StringMetaTable = setmetatable({ __index = Object, __call = ctor }, Object)
  setmetatable(String, StringMetaTable)
else
  string.__call = ctor
  string.__index = string
  
  String = getmetatable("")
  String.__genericT__ = System.Char
  String.base = inherits
  System.define("System.String", String)
  String.__index = string
  setmetatable(String, string)
  setmetatable(string, System.Object)  
end
