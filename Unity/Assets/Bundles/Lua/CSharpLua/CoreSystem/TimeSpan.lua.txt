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
local div = System.div
local trunc = System.trunc
local ArgumentException = System.ArgumentException
local OverflowException = System.OverflowException
local ArgumentNullException = System.ArgumentNullException
local FormatException = System.FormatException

local assert = assert
local getmetatable = getmetatable
local select = select
local sformat = string.format
local sfind = string.find
local tostring = tostring
local tonumber = tonumber
local floor = math.floor
local log10 = math.log10

local TimeSpan
local zero

local function compare(t1, t2)
  if t1.ticks > t2.ticks then return 1 end
  if t1.ticks < t2.ticks then return -1 end
  return 0
end

local function add(this, ts) 
  return TimeSpan(this.ticks + ts.ticks)
end

local function subtract(this, ts) 
  return TimeSpan(this.ticks - ts.ticks)
end

local function negate(this) 
  local ticks = this.ticks
  if ticks == -9223372036854775808 then
    throw(OverflowException("Overflow_NegateTwosCompNum"))
  end
  return TimeSpan(-ticks)
end

local function interval(value, scale)
  if value ~= value then 
    throw(ArgumentException("Arg_CannotBeNaN"))
  end
  local tmp = value * scale
  local millis = tmp + (value >=0 and 0.5 or -0.5)
  if millis > 922337203685477 or millis < -922337203685477 then
    throw(OverflowException("Overflow_TimeSpanTooLong"))
  end
  return TimeSpan(trunc(millis) * 10000)
end

local function getPart(this, i, j)
  local t = this.ticks
  local v = div(t, i) % j
  if v ~= 0 and t < 0 then
    return v - j
  end
  return v
end

local function parse(s)
  if s == nil then return nil, 1 end
  local i, j, k, sign, ch
  local day, hour, minute, second, milliseconds = 0, 0, 0, 0, 0
  i, j, sign, day = sfind(s, "^%s*([-]?)(%d+)")
  if not i then return end
  k = j + 1
  i, j, ch = sfind(s, "^([%.:])", k)
  if not i then 
    i, j = sfind(s, "^%s*$", k)
    if not i then return end
    k = -1
  else
    k = j + 1
    if ch == '.' then
      i, j, hour, minute = sfind(s, "^(%d+):(%d+)", k)
      if not i then return end
      k = j + 1
      i, j, second = sfind(s, "^:(%d+)", k)
      if not i then return end
    else
      i, j, hour = sfind(s, "^(%d+)", k)
      if not i then return end
      k = j + 1
      i, j, minute = sfind(s, "^:(%d+)", k)
      if not i then
        i, j = sfind(s, "^%s*$", k)
        if not i then
          return
        end
        day, hour, minute = 0, day, hour
        k = -1
      else
        k = j
        i, j, second = sfind(s, "^:(%d+)", k + 1)
        if not i then
          day, hour, minute, second = 0, day, hour, minute
          j = k
        end
      end
    end
  end
  if k ~= -1 then
    k = j + 1
    i, j, milliseconds = sfind(s, "^%.(%d+)%s*$", k)
    if not i then
      i, j = sfind(s, "^%s*$", k)
      if not i then return end
      milliseconds = 0
    else
      milliseconds = tonumber(milliseconds)
      local n = floor(log10(milliseconds) + 1)
      if n > 3 then
        if n > 7 then return end
        milliseconds = milliseconds / (10 ^ (n - 3))
      end
    end
  end
  if sign == '-' then
    day, hour, minute, second, milliseconds = -day, -hour, -minute, -second, -milliseconds
  end
  return TimeSpan(day, hour, minute, second, milliseconds)
end

TimeSpan = System.defStc("System.TimeSpan", {
  ticks = 0,
  __ctor__ = function (this, ...)
    local ticks
    local length = select("#", ...)
    if length == 0 then
    elseif length == 1 then
      ticks = ...
    elseif length == 3 then
      local hours, minutes, seconds = ...
      ticks = (((hours * 60 + minutes) * 60) + seconds) * 10000000
    elseif length == 4 then
      local days, hours, minutes, seconds = ...
      ticks = ((((days * 24 + hours) * 60 + minutes) * 60) + seconds) * 10000000
    elseif length == 5 then
      local days, hours, minutes, seconds, milliseconds = ...
      ticks = (((((days * 24 + hours) * 60 + minutes) * 60) + seconds) * 1000 + milliseconds) * 10000
    else 
      assert(ticks)
    end
    this.ticks = ticks
  end,
  Compare = compare,
  CompareTo = compare,
  CompareToObj = function (this, t)
    if t == nil then return 1 end
    if getmetatable(t) ~= TimeSpan then
      throw(ArgumentException("Arg_MustBeTimeSpan"))
    end
    compare(this, t)
  end,
  Equals = function (t1, t2)
    return t1.ticks == t2.ticks
  end,
  EqualsObj = function(this, t)
    if getmetatable(t) == TimeSpan then
      return this.ticks == t.ticks
    end
    return false
  end,
  GetHashCode = function (this)
    return this.ticks
  end,
  getTicks = function (this) 
    return this.ticks
  end,
  getDays = function (this) 
    return div(this.ticks, 864000000000)
  end,
  getHours = function(this)
    return getPart(this, 36000000000, 24)
  end,
  getMinutes = function (this)
    return getPart(this, 600000000, 60)
  end,
  getSeconds = function (this)
    return getPart(this, 10000000, 60)
  end,
  getMilliseconds = function (this)
    return getPart(this, 10000, 1000)
  end,
  getTotalDays = function (this) 
    return this.ticks / 864000000000
  end,
  getTotalHours = function (this) 
    return this.ticks / 36000000000
  end,
  getTotalMilliseconds = function (this) 
    return this.ticks / 10000
  end,
  getTotalMinutes = function (this) 
    return this.ticks / 600000000
  end,
  getTotalSeconds = function (this) 
    return this.ticks / 10000000
  end,
  Add = add,
  Subtract = subtract,
  Duration = function (this) 
    local ticks = this.ticks
    if ticks == -9223372036854775808 then
      throw(OverflowException("Overflow_Duration"))
    end
    return TimeSpan(ticks >= 0 and ticks or - ticks)
  end,
  Negate = negate,
  ToString = function (this) 
    local day, milliseconds = this:getDays(), this.ticks % 10000000
    local daysStr = day == 0 and "" or (day .. ".")
    local millisecondsStr = milliseconds == 0 and "" or (".%07d"):format(milliseconds)
    return sformat("%s%02d:%02d:%02d%s", daysStr, this:getHours(), this:getMinutes(), this:getSeconds(), millisecondsStr)
  end,
  Parse = function (s)
    local v, err = parse(s)
    if v then
      return v
    end
    if err == 1 then
      throw(ArgumentNullException())
    else
      throw(FormatException())
    end
  end,
  TryParse = function (s)
    local v = parse(s)
    if v then
      return true, v
    end
    return false, zero
  end,
  __add = add,
  __sub = subtract,
  __unm = negate,
  __eq = function (t1, t2)
    return t1.ticks == t2.ticks
  end,
  __lt = function (t1, t2)
    return t1.ticks < t2.ticks
  end,
  __le = function (t1, t2)
    return t1.ticks <= t2.ticks
  end,
  FromDays = function (value) 
    return interval(value, 864e5)
  end,
  FromHours = function (value) 
    return interval(value, 36e5)
  end,
  FromMilliseconds = function (value) 
    return interval(value, 1)
  end,
  FromMinutes = function (value) 
    return interval(value, 6e4)
  end,
  FromSeconds = function (value) 
    return interval(value, 1000)
  end,
  FromTicks = function (value) 
    return TimeSpan(value)
  end,
  base = function (_, T)
    return { System.IComparable, System.IComparable_1(T), System.IEquatable_1(T) }
  end,
  default = function ()
    return zero
  end,
  Zero = false,
  MaxValue = false,
  MinValue = false
})

zero = TimeSpan(0)
TimeSpan.Zero = zero
TimeSpan.MaxValue = TimeSpan(9223372036854775807)
TimeSpan.MinValue = TimeSpan(-9223372036854775808)
