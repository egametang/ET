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

local setmetatable = setmetatable
local getmetatable = getmetatable
local type = type
local pairs  = pairs
local assert = assert
local table = table
local tremove = table.remove
local tconcat = table.concat
local floor = math.floor
local ceil = math.ceil
local error = error
local select = select
local xpcall = xpcall
local rawget = rawget
local rawset = rawset
local rawequal = rawequal
local tostring = tostring
local string = string
local sfind = string.find
local ssub = string.sub
local debug = debug
local next = next
local global = _G
local prevSystem = rawget(global, "System")

local emptyFn = function() end
local nilFn = function() return nil end
local falseFn = function() return false end
local trueFn = function() return true end
local identityFn = function(x) return x end
local lengthFn = function (t) return #t end
local zeroFn = function() return 0 end
local oneFn = function() return 1 end
local equals = function(x, y) return x == y end
local getCurrent = function(t) return t.current end
local assembly, metadatas
local System, Object, ValueType

local function new(cls, ...)
  local this = setmetatable({}, cls)
  return this, cls.__ctor__(this, ...)
end

local function throw(e, lv)
  if e == nil then e = System.NullReferenceException() end
  e:traceback(lv)
  error(e)
end

local function xpcallErr(e)
  if e == nil then
    e = System.Exception("script error")
    e:traceback()
  elseif type(e) == "string" then
    if sfind(e, "attempt to index") then
      e = System.NullReferenceException(e)
    elseif sfind(e, "attempt to divide by zero") then  
      e = System.DivideByZeroException(e)
    else
      e = System.Exception(e)
    end
    e:traceback()
  end
  return e
end

local function try(try, catch, finally)
  local ok, status, result = xpcall(try, xpcallErr)
  if not ok then
    if catch then
      if finally then
        ok, status, result = xpcall(catch, xpcallErr, status)
      else
        ok, status, result = true, catch(status)
      end
      if ok then
        if status == 1 then
          ok = false
          status = result
        end
      end
    end
  end
  if finally then
    finally()
  end
  if not ok then
    error(status)
  end
  return status, result
end

local function set(className, cls)
  local scope = global
  local starIndex = 1
  while true do
    local pos = sfind(className, "[%.+]", starIndex) or 0
    local name = ssub(className, starIndex, pos -1)
    if pos ~= 0 then
      local t = rawget(scope, name)
      if t == nil then
        if cls then
          t = {}
          rawset(scope, name, t)
        else
          return nil
        end
      end
      scope = t
      starIndex = pos + 1
    else
      if cls then
        assert(rawget(scope, name) == nil, className)
        rawset(scope, name, cls)
        return cls
      else
        return rawget(scope, name)
      end
    end
  end
end

local function multiKey(t, ...)
  local n, i, k = select("#", ...), 1
  while true do
    k = assert(select(i, ...))
    if i == n then
      break
    end
    local tk = t[k]
    if tk == nil then
      tk = {}
      t[k] = tk
    end
    t = tk
    i = i + 1
  end
  return t, k
end

local function genericName(name, ...)
  if name:byte(-2) == 95 then
    name = ssub(name, 1, -3)
  end
  local n = select("#", ...)
  local t = { name, "`", n, "[" }
  local count = 5
  local hascomma
  for i = 1, n do
    local cls = select(i, ...)
    if hascomma then
      t[count] = ","
      count = count + 1
    else
      hascomma = true
    end
    t[count] = cls.__name__
    count = count + 1
  end
  t[count] = "]"
  return tconcat(t)
end

local enumMetatable = { class = "E", default = zeroFn, __index = false, interface = false, __call = function (_, v) return v or 0 end }
enumMetatable.__index = enumMetatable

local interfaceMetatable = { class = "I", default = nilFn, __index = false }
interfaceMetatable.__index = interfaceMetatable

local ctorMetatable = { __call = function (ctor, ...) return ctor[1](...) end }

local function applyExtends(cls)
  local extends = cls.base
  if extends then
    if type(extends) == "function" then
      extends = extends(global, cls)
    end
    cls.base = nil
  end
  return extends
end

local function applyMetadata(cls)
  local metadata = cls.__metadata__
  if metadata then
    if metadatas then
      metadatas[#metadatas + 1] = function (global)
        cls.__metadata__ = metadata(global)
      end
    else
      cls.__metadata__ = metadata(global)
    end
  end
end

local function setBase(cls, kind)
  local ctor = cls.__ctor__
  if ctor and type(ctor) == "table" then
    setmetatable(ctor, ctorMetatable)
  end
  local extends = applyExtends(cls)
  applyMetadata(cls)

  cls.__index = cls 
  cls.__call = new
  
  if kind == "S" then
    if extends then
      cls.interface = extends
    end
    setmetatable(cls, ValueType)
  else
    if extends then
      local base = extends[1]
      if not base then error(cls.__name__ .. "'s base is nil") end
      if base.class == "I" then
        cls.interface = extends
        setmetatable(cls, Object)
      else
        setmetatable(cls, base)
        if #extends > 1 then
          tremove(extends, 1)
          cls.interface = extends
        end
      end
    else
      setmetatable(cls, Object)
    end
  end
end

local function staticCtorSetBase(cls)
  setmetatable(cls, nil)
  local t = cls[cls]
  for k, v in pairs(t) do
    cls[k] = v
  end
  cls[cls] = nil
  local kind = cls.class
  cls.class = nil
  setBase(cls, kind)
  cls:static()
  cls.static = nil
end

local staticCtorMetatable = {
  __index = function(cls, key)
    staticCtorSetBase(cls)
    return cls[key]
  end,
  __newindex = function(cls, key, value)
    staticCtorSetBase(cls)
    cls[key] = value
  end,
  __call = function(cls, ...)
    staticCtorSetBase(cls)
    return new(cls, ...)
  end
}

local function setHasStaticCtor(cls, kind)
  local name = cls.__name__
  cls.__name__ = nil
  local t = {}
  for k, v in pairs(cls) do
    t[k] = v
    cls[k] = nil
  end
  cls[cls] = t
  cls.__name__ = name
  cls.class = kind
  cls.__call = new
  cls.__index = cls
  setmetatable(cls, staticCtorMetatable)
end

local function defCore(name, kind, cls, generic)
  cls = cls or {}
  cls.__name__ = name
  cls.__assembly__ = assembly
  if not generic then
    set(name, cls)
  end
  if kind == "C" or kind == "S" then
    if cls.static == nil then
      setBase(cls, kind)
    else
      setHasStaticCtor(cls, kind)
    end
  elseif kind == "I" then
    local extends = applyExtends(cls)
    if extends then 
      cls.interface = extends 
    end
    applyMetadata(cls)
    setmetatable(cls, interfaceMetatable)
  elseif kind == "E" then
    applyMetadata(cls)
    setmetatable(cls, enumMetatable)
  else
    assert(false, kind)
  end
  return cls
end

local function def(name, kind, cls, generic)
  if type(cls) == "function" then
    local mt = {}
    local fn = function(_, ...)
      local gt, gk = multiKey(mt, ...)
      local t = gt[gk]
      if t == nil then
        local class, super  = cls(...)
        t = defCore(genericName(name, ...), kind, class or {}, true)
        if generic then
          setmetatable(t, super or generic)
        end
        gt[gk] = t
      end
      return t
    end

    local base = kind ~= "S" and Object or ValueType
    local caller = setmetatable({ __call = fn, __index = base }, base)
    if generic then
      generic.__index = generic
      generic.__call = new
    end
    return set(name, setmetatable(generic or {}, caller))
  else
    return defCore(name, kind, cls, generic)
  end
end

local function defCls(name, cls, generic)
  return def(name, "C", cls, generic)
end

local function defInf(name, cls)
  return def(name, "I", cls)
end

local function defStc(name, cls, generic)
  return def(name, "S", cls, generic)
end

local function defEnum(name, cls)
  return def(name, "E", cls)
end

local function defArray(name, cls, Array, MultiArray)
  Array.__index = Array
  MultiArray.__index =  MultiArray
  setmetatable(MultiArray, Array)

  local mt = {}
  local function create(Array, T)
    local ArrayT = mt[T]
    if ArrayT == nil then
      ArrayT = defCore(T.__name__ .. "[]", "C", cls(T), true)
      setmetatable(ArrayT, Array)
      mt[T] = ArrayT
    end
    return ArrayT
  end

  local mtMulti = {}
  local function createMulti(MultiArray, T, dimension)
    local gt, gk = multiKey(mtMulti, T, dimension)
    local ArrayT = gt[gk]
    if ArrayT == nil then
      local name = T.__name__ .. "[" .. (","):rep(dimension - 1) .. "]"
      ArrayT = defCore(name, "C", cls(T), true)
      setmetatable(ArrayT, MultiArray)
      gt[gk] = ArrayT
    end
    return ArrayT
  end

  return set(name, setmetatable(Array, {
    __index = Object,
    __call = function (Array, T, dimension)
      if not dimension then
        return create(Array, T)
      else
        return createMulti(MultiArray, T, dimension)
      end
    end
  }))
end

local function trunc(num)
  return num > 0 and floor(num) or ceil(num)
end

local function when(f, ...)
  local ok, r = pcall(f, ...)
  return ok and r
end

System = {
  emptyFn = emptyFn,
  falseFn = falseFn,
  trueFn = trueFn,
  identityFn = identityFn,
  lengthFn = lengthFn,
  zeroFn = zeroFn,
  oneFn = oneFn,
  equals = equals,
  getCurrent = getCurrent,
  try = try,
  when = when,
  throw = throw,
  getClass = set,
  multiKey = multiKey,
  define = defCls,
  defInf = defInf,
  defStc = defStc,
  defEnum = defEnum,
  defArray = defArray,
  enumMetatable = enumMetatable,
  trunc = trunc,
  global = global
}
if prevSystem then
  setmetatable(System, { __index = prevSystem })
end
global.System = System

local debugsetmetatable = debug and debug.setmetatable
System.debugsetmetatable = debugsetmetatable

local _, _, version = sfind(_VERSION, "^Lua (.*)$")
version = tonumber(version)
System.luaVersion = version

if version < 5.3 then
  local bnot, band, bor, xor, sl, sr
  local bit = rawget(global, "bit")
  if not bit then
    local ok, b = pcall(require, "bit")
    if ok then
      bit = b
    end
  end
  if bit then
    bnot, band, bor, xor, sl, sr = bit.bnot, bit.band, bit.bor, bit.bxor, bit.lshift, bit.rshift
  else
    local function disable()
      throw(System.NotSupportedException("bit operation is not enabled."))
    end
    bnot, band, bor, xor, sl, sr  = disable, disable, disable, disable, disable, disable
  end

  System.bnot = bnot
  System.band = band
  System.bor = bor
  System.xor = xor
  System.sl = sl
  System.sr = sr

  function System.div(x, y) 
    if y == 0 then throw(System.DivideByZeroException(), 1) end
    return trunc(x / y)
  end

  function System.mod(x, y)
    if y == 0 then throw(System.DivideByZeroException(), 1) end
    local v = x % y
    if v ~= 0 and x * y < 0 then
      return v - y
    end
    return v
  end
  
  function System.modf(x, y)
    local v = x % y
    if v ~= 0 and x * y < 0 then
      return v - y
    end
    return v
  end

  function System.toUInt(v, max, mask, checked)
    if v >= 0 and v <= max then
      return v
    end
    if checked then
      throw(System.OverflowException(), 1) 
    end
    return band(v, mask)
  end

  function System.ToUInt(v, max, mask, checked)
    v = trunc(v)
    if v >= 0 and v <= max then
      return v
    end
    if checked then
      throw(System.OverflowException(), 1) 
    end
    if v < -2147483648 or v > 2147483647 then
      return 0
    end
    return band(v, mask)
  end

  local function toInt(v, mask, umask)
    v = band(v, mask)
    local uv = band(v, umask)
    if uv ~= v then
      v = xor(uv - 1, umask)
      if uv ~= 0 then
        v = -v
      end
    end
    return v
  end

  function System.toInt(v, min, max, mask, umask, checked)
    if v >= min and v <= max then
      return v
    end
    if checked then
      throw(System.OverflowException(), 1) 
    end
    return toInt(v, mask, umask)
  end

  function System.ToInt(v, min, max, mask, umask, checked)
    v = trunc(v)
    if v >= min and v <= max then
      return v
    end
    if checked then
      throw(System.OverflowException(), 1) 
    end
    if v < -2147483648 or v > 2147483647 then
      return 0
    end
    return toInt(v, mask, umask)
  end

  local function toUInt32(v)
    if v <= -2251799813685248 or v >= 2251799813685248 then  -- 2 ^ 51, Lua BitOp used 51 and 52
      throw(System.InvalidCastException()) 
    end
    v = band(v, 0xffffffff)
    local uv = band(v, 0x7fffffff)
    if uv ~= v then
      return uv + 0x80000000
    end
    return v
  end

  function System.toUInt32(v, checked)
    if v >= 0 and v <= 4294967295 then
      return v
    end
    if checked then
      throw(System.OverflowException(), 1) 
    end
    return toUInt32(v)
  end

  function System.ToUInt32(v, checked)
    v = trunc(v)
    if v >= 0 and v <= 4294967295 then
      return v
    end
    if checked then
      throw(System.OverflowException(), 1) 
    end
    return toUInt32(v)
  end

  function System.toInt32(v, checked)
    if v >= -2147483648 and v <= 2147483647 then
      return v
    end
    if checked then
      throw(System.OverflowException(), 1) 
    end
    if v <= -2251799813685248 or v >= 2251799813685248 then  -- 2 ^ 51, Lua BitOp used 51 and 52
      throw(System.InvalidCastException()) 
    end
    return band(v, 0xffffffff)
  end

  function System.toInt64(v, checked) 
    if v >= -9223372036854775808 and v <= 9223372036854775807 then
      return v
    end
    if checked then
      throw(System.OverflowException(), 1) 
    end
    throw(System.InvalidCastException()) -- 2 ^ 51, Lua BitOp used 51 and 52
  end

  function System.toUInt64(v, checked)
    if v >= 0 then
      return v
    end
    if checked then
      throw(System.OverflowException(), 1) 
    end
    if v >= -2147483648 then
      return band(v, 0x7fffffff) + 0xffffffff80000000
    end
    throw(System.InvalidCastException()) 
  end

  function System.ToUInt64(v, checked)
    v = trunc(v)
    if v >= 0 and v <= 18446744073709551615 then
      return v
    end
    if checked then
      throw(System.OverflowException(), 1) 
    end
    if v >= -2147483648 and v <= 2147483647 then
      v = band(v, 0xffffffff)
      local uv = band(v, 0x7fffffff)
      if uv ~= v then
        return uv + 0xffffffff80000000
      end
      return v
    end
    throw(System.InvalidCastException()) 
  end

  if table.pack == nil then
    table.pack = function(...)
      return { n = select("#", ...), ... }
    end
  end

  if table.unpack == nil then
    table.unpack = assert(unpack)
  end

  if table.move == nil then
    table.move = function(a1, f, e, t, a2)
      if a2 == nil then a2 = a1 end
      if t > f then
        t = e - f + t
        while e >= f do
          a2[t] = a1[e]
          t = t - 1
          e = e - 1
        end
      else
        while f <= e do
          a2[t] = a1[f]
          t = t + 1
          f = f + 1
        end
      end
    end
  end
else
  load[[
  local System = System
  local throw = System.throw
  local trunc = System.trunc
  
  function System.bnot(x) return ~x end 
  function System.band(x, y) return x & y end
  function System.bor(x, y) return x | y end
  function System.xor(x, y) return x ~ y end
  function System.sl(x, y) return x << y end
  function System.sr(x, y) return x >> y end
  function System.div(x, y) if x ~ y < 0 then return -(-x // y) end return x // y end

  function System.mod(x, y)
    local v = x % y
    if v ~= 0 and 1.0 * x * y < 0 then
      return v - y
    end
    return v
  end

  local function toUInt(v, max, mask, checked)  
    if v >= 0 and v <= max then
      return v
    end
    if checked then
      throw(System.OverflowException(), 2) 
    end
    return v & mask
  end
  System.toUInt = toUInt

  function System.ToUInt(v, max, mask, checked)
    v = trunc(v)
    if v >= 0 and v <= max then
      return v
    end
    if checked then
      throw(System.OverflowException(), 2) 
    end
    if v < -2147483648 or v > 2147483647 then
      return 0
    end
    return v & mask
  end
  
  local function toSingedInt(v, mask, umask)
    v = v & mask
    local uv = v & umask
    if uv ~= v then
      v = (uv - 1) ~ umask
      if uv ~= 0 then
        v = -v
      end
    end
    return v
  end
  
  local function toInt(v, min, max, mask, umask, checked)
    if v >= min and v <= max then
      return v
    end
    if checked then
      throw(System.OverflowException(), 2) 
    end
    return toSingedInt(v, mask, umask)
  end
  System.toInt = toInt
  
  function System.ToInt(v, min, max, mask, umask, checked)
    v = trunc(v)
    if v >= min and v <= max then
      return v
    end
    if checked then
      throw(System.OverflowException(), 2) 
    end
    if v < -2147483648 or v > 2147483647 then
      return 0
    end
    return toSingedInt(v, mask, umask)
  end

  function System.toUInt32(v, checked)
    return toUInt(v, 4294967295, 0xffffffff, checked)
  end
  
  function System.ToUInt32(v, checked)
    v = trunc(v)
    if v >= 0 and v <= 4294967295 then
      return v
    end
    if checked then
      throw(System.OverflowException(), 1) 
    end
    return v & 0xffffffff
  end
  
  function System.toInt32(v, checked)
    return toInt(v, -2147483648, 2147483647, 0xffffffff, 0x7fffffff, checked)
  end

  function System.toInt64(v, checked)
    return toInt(v, -9223372036854775808, 9223372036854775807, 0xffffffffffffffff, 0x7fffffffffffffff, checked)
  end

  function System.toUInt64(v, checked)
    if v >= 0 then
      return v
    end
    if checked then
      throw(System.OverflowException(), 1) 
    end
    return (v & 0x7fffffffffffffff) + 0x8000000000000000
  end

  function System.ToUInt64(v, checked)
    v = trunc(v)
    if v >= 0 and v <= 18446744073709551615 then
      return v
    end
    if checked then
      throw(System.OverflowException(), 1) 
    end
    v = v & 0xffffffffffffffff
    local uv = v & 0x7fffffffffffffff
    if uv ~= v then
      return uv + 0x8000000000000000
    end
    return v
  end

  ]]()
end

local toUInt = System.toUInt
local toInt = System.toInt
local ToUInt = System.ToUInt
local ToInt = System.ToInt

function System.toByte(v, checked)
  return toUInt(v, 255, 0xff, checked)
end

function System.toSByte(v, checked)
  return toInt(v, -128, 127, 0xff, 0x7f, checked)
end

function System.toInt16(v, checked)
  return toInt(v, -32768, 32767, 0xffff, 0x7fff, checked)
end

function System.toUInt16(v, checked)
  return toUInt(v, 65535, 0xffff, checked)
end

function System.ToByte(v, checked)
  return ToUInt(v, 255, 0xff, checked)
end

function System.ToSByte(v, checked)
  return ToInt(v, -128, 127, 0xff, 0x7f, checked)
end

function System.ToInt16(v, checked)
  return ToInt(v, -32768, 32767, 0xffff, 0x7fff, checked)
end

function System.ToUInt16(v, checked)
  return ToUInt(v, 65535, 0xffff, checked)
end

function System.ToInt32(v, checked)
  v = trunc(v)
  if v >= -2147483648 and v <= 2147483647 then
    return v
  end
  if checked then
    throw(System.OverflowException(), 1) 
  end
  return -2147483648
end

function System.ToInt64(v, checked)
  v = trunc(v)
  if v >= -9223372036854775808 and v <= 9223372036854775807 then
    return v
  end
  if checked then
    throw(System.OverflowException(), 1) 
  end
  return -9223372036854775808
end

function System.ToSingle(v, checked)
  if v >= -3.40282347E+38 and v <= 3.40282347E+38 then
    return v
  end
  if checked then
    throw(System.OverflowException(), 1) 
  end
  if v > 0 then
    return 1 / 0 
  else
    return -1 / 0
  end
end

function System.using(t, f)
  local dispose = t and t.Dispose
  if dispose ~= nil then
    local ok, status, ret = xpcall(f, xpcallErr, t)   
    dispose(t)
    if not ok then
      error(status)
    end
    return status, ret
  else
    return f(t)    
  end
end

function System.usingX(f, ...)
  local ok, status, ret = xpcall(f, xpcallErr, ...)
  for i = 1, select("#", ...) do
    local t = select(i, ...)
    if t ~= nil then
      local dispose = t.Dispose
      if dispose ~= nil then
        dispose(t)
      end
    end
  end
  if not ok then
    error(status)
  end
  return status, ret
end

function System.apply(t, f)
  f(t)
  return t
end

function System.default(T)
  return T:default()
end

function System.property(name)
  local function g(this)
    return this[name]
  end
  local function s(this, v)
    this[name] = v
  end
  return g, s
end

function System.new(cls, index, ...)
  local this = setmetatable({}, cls)
  return this, cls.__ctor__[index](this, ...)
end

function System.base(this)
  return getmetatable(getmetatable(this))
end

local equalsObj, compareObj, toString
if debugsetmetatable then
  equalsObj = function (x, y)
    if x == y then
      return true
    end
    if x == nil or y == nil then
      return false
    end
    local ix = x.EqualsObj
    if ix ~= nil then
      return ix(x, y)
    end
    local iy = y.EqualsObj
    if iy ~= nil then
      return iy(y, x)
    end
    return false
  end

  compareObj = function (a, b)
    if a == b then return 0 end
    if a == nil then return -1 end
    if b == nil then return 1 end
    local ia = a.CompareToObj
    if ia ~= nil then
      return ia(a, b)
    end
    local ib = b.CompareToObj
    if ib ~= nil then
      return -ib(b, a)
    end
    throw(System.ArgumentException("Argument_ImplementIComparable"))
  end

  toString = function (t)
    return t ~= nil and t:ToString() or ""
  end

  debugsetmetatable(nil, {
    __concat = function(a, b)
      if a == nil then
        if b == nil then
          return ""
        else
          return b
        end
      else
        return a
      end
    end,
    __add = function (a, b)
      if a == nil then
        if b == nil or type(b) == "number" then
          return nil
        end
        return b
      end
      return nil
    end,
    __sub = nilFn,
    __mul = nilFn,
    __div = nilFn,
    __mod = nilFn,
    __unm = nilFn,
    __lt = falseFn,
    __le = falseFn,

    -- lua 5.3
    __idiv = nilFn,
    __band = nilFn,
    __bor = nilFn,
    __bxor = nilFn,
    __bnot = nilFn,
    __shl = nilFn,
    __shr = nilFn,
  })
else
  equalsObj = function (x, y)
    if x == y then
      return true
    end
    if x == nil or y == nil then
      return false
    end
    local t = type(x)
    if t == "table" then
      local ix = x.EqualsObj
      if ix ~= nil then
        return ix(x, y)
      end
    elseif t == "number" then
      return System.Number.EqualsObj(x, y)
    end
    t = type(y)
    if t == "table" then
      local iy = y.EqualsObj
      if iy ~= nil then
        return iy(y, x)
      end
    end
    return false
  end

  compareObj = function (a, b)
    if a == b then return 0 end
    if a == nil then return -1 end
    if b == nil then return 1 end
    local t = type(a)
    if t == "number" then
      return System.Number.CompareToObj(a, b)
    elseif t == "boolean" then
      return System.Boolean.CompareToObj(a, b)
    else
      local ia = a.CompareToObj
      if ia ~= nil then
        return ia(a, b)
      end
    end
    t = type(b)
    if t == "number" then
      return -System.Number.CompareToObj(b, a)
    elseif t == "boolean" then
      return -System.Boolean.CompareToObj(a, b)
    else
      local ib = b.CompareToObj
      if ib ~= nil then
        return -ib(b, a)
      end
    end
    throw(System.ArgumentException("Argument_ImplementIComparable"))
  end

  toString = function (obj)
    if obj == nil then return "" end
    local t = type(obj) 
    if t == "table" then
      return obj:ToString()
    elseif t == "boolean" then
      return obj and "True" or "False"
    elseif t == "function" then
      return "System.Delegate"
    end
    return tostring(obj)
  end
end

System.equalsObj = equalsObj
System.compareObj = compareObj
System.toString = toString

Object = defCls("System.Object", {
  __call = new,
  __ctor__ = emptyFn,
  default = nilFn,
  class = "C",
  EqualsObj = equals,
  ReferenceEquals = rawequal,
  GetHashCode = identityFn,
  EqualsStatic = equalsObj,
  GetType = false,
  ToString = function(this) return this.__name__ end
})
setmetatable(Object, { __call = new })

ValueType = defCls("System.ValueType", {
  class = "S",
  default = function(T) 
    return T()
  end,
  __clone__ = function(this)
    if type(this) == "table" then
      local cls = getmetatable(this)
      local t = {}
      for k, v in pairs(this) do
        if type(v) == "table" and v.class == "S" then
          t[k] = v:__clone__()
        else
          t[k] = v
        end
      end
      return setmetatable(t, cls)
    end
    return this
  end,
  __copy__ = function (this, obj)
    for k, v in pairs(obj) do
      if type(v) == "table" and v.class == "S" then
        this[k] = v:__clone__()
      else
        this[k] = v
      end
    end
    for k, v in pairs(this) do
      if v ~= nil and rawget(obj, k) == nil then
        this[k] = nil
      end
    end
  end,
  EqualsObj = function (this, obj)
    if getmetatable(this) ~= getmetatable(obj) then return false end
    for k, v in pairs(this) do
      if not equalsObj(v, obj[k]) then
        return false
      end
    end
    return true
  end,
  GetHashCode = function (this)
    throw(System.NotSupportedException(this.__name__ .. " User-defined struct not support GetHashCode"), 1)
  end
})

local AnonymousType
AnonymousType = defCls("System.AnonymousType", {
  EqualsObj = function (this, obj)
    if getmetatable(obj) ~= AnonymousType then return false end
    for k, v in pairs(this) do
      if not equalsObj(v, obj[k]) then
        return false
      end
    end
    return true
  end
})

local function anonymousTypeCreate(T, t)
  return setmetatable(t, T)
end

local anonymousTypeMetaTable = setmetatable({ __index = Object, __call = anonymousTypeCreate }, Object)
setmetatable(AnonymousType, anonymousTypeMetaTable)

local pack, unpack = table.pack, table.unpack

local function tupleDeconstruct(t) 
  return unpack(t, 1, t.n)
end

local function tupleEquals(t, other)
  for i = 1, t.n do
    if not equalsObj(t[i], other[i]) then
      return false
    end
  end
  return true
end

local function tupleEqualsObj(t, obj)
  if getmetatable(obj) ~= getmetatable(t) or t.n ~= obj.n then
    return false
  end
  return tupleEquals(t, obj)
end

local function tupleCompareTo(t, other)
  for i = 1, t.n do
    local v = compareObj(t[i], other[i])
    if v ~= 0 then
      return v
    end
  end
  return 0
end

local function tupleCompareToObj(t, obj)
  if obj == nil then return 1 end
  if getmetatable(obj) ~= getmetatable(t) or t.n ~= obj.n then
    throw(System.ArgumentException())
  end
  return tupleCompareTo(t, obj)
end

local function tupleToString(t)
  local a = { "(" }
  local count = 2
  for i = 1, t.n do
    if i ~= 1 then
      a[count] = ", "
      count = count + 1
    end
    local v = t[i]
    if v ~= nil then
      a[count] = v:ToString()
      count = count + 1
    end
  end
  a[count] = ")"
  return tconcat(a)
end

local function tupleLength(t)
  return t.n
end

local function tupleGet(t, index)
  if index < 0 or index >= t.n then
    throw(System.IndexOutOfRangeException())
  end
  return t[index + 1]
end

local function tupleGetRest(t)
  return t[8]
end

local function tupleCreate(T, ...)
  return setmetatable(pack(...), T)
end

local Tuple = defCls("System.Tuple", {
  Deconstruct = tupleDeconstruct,
  ToString = tupleToString,
  EqualsObj = tupleEqualsObj,
  CompareToObj = tupleCompareToObj,
  getLength = tupleLength,
  get = tupleGet,
  getRest = tupleGetRest
})
local tupleMetaTable = setmetatable({ __index  = Object, __call = tupleCreate }, Object)
setmetatable(Tuple, tupleMetaTable)

local ValueTuple = defStc("System.ValueTuple", {
  Deconstruct = tupleDeconstruct,
  ToString = tupleToString,
  __eq = tupleEquals,
  Equals = tupleEquals,
  EqualsObj = tupleEqualsObj,
  CompareTo = tupleCompareTo,
  CompareToObj = tupleCompareToObj,
  getLength = tupleLength,
  get = tupleGet,
  default = function ()
    throw(System.NotSupportedException("not support default(T) when T is ValueTuple"))
  end
})
local valueTupleMetaTable = setmetatable({ __index  = ValueType, __call = tupleCreate }, ValueType)
setmetatable(ValueTuple, valueTupleMetaTable)

local function recordEquals(t, other)
  if getmetatable(t) == getmetatable(other) then
    for k, v in pairs(t) do
      if not equalsObj(v, other[k]) then
        return false
      end
    end
    return true
  end
  return false
end

defCls("System.RecordType", {
  __eq = recordEquals,
  __clone__ = function (this)
    local cls = getmetatable(this)
    local t = {}
    for k, v in pairs(this) do
      t[k] = v
    end
    return setmetatable(t, cls)
  end,
  Equals = recordEquals,
  PrintMembers = function (this, builder)
    local p = pack(this.__members__())
    local n = p.n
    for i = 2, n do
      local k = p[i]
      local v = this[k]
      builder:Append(k)
      builder:Append(" = ")
      if v ~= nil then
        builder:Append(toString(v))
      end
      if i ~= n then
        builder:Append(", ")
      end
    end
  end,
  ToString = function (this)
    local p = pack(this.__members__())
    local n = p.n
    local t = { p[1], "{" }
    local count = 3
    for i = 2, n do
      local k = p[i]
      local v = this[k]
      t[count] = k
      t[count + 1] = "="
      if v ~= nil then
        if i ~= n then
          t[count + 2] = toString(v) .. ','
        else
          t[count + 2] = toString(v)
        end
      else
        if i ~= n then
          t[count + 2] = ','
        end
      end
      if v == nil and i == n then
        count = count + 2
      else
        count = count + 3
      end
    end
    t[count] = "}"
    return tconcat(t, ' ')
  end
})

local Attribute = defCls("System.Attribute")
defCls("System.FlagsAttribute", { base = { Attribute } })

local Nullable = { 
  default = nilFn,
  Value = function (this)
    if this == nil then
      throw(System.InvalidOperationException("Nullable object must have a value."))
    end
    return this
  end,
  EqualsObj = equalsObj,
  GetHashCode = function (this)
    if this == nil then
      return 0
    end
    if type(this) == "table" then
      return this:GetHashCode()
    end
    return this
  end,
  clone = function (t)
    if type(t) == "table" then
      return t:__clone__()
    end
    return t
  end
}

defStc("System.Nullable", function (T)
  return { 
    __genericT__ = T 
  }
end, Nullable)

function System.isNullable(T)
  return getmetatable(T) == Nullable
end

local Index = defStc("System.Index", {
  End = -0.0,
  Start = 0,
  IsFromEnd = function (this)
    return 1 / this < 0 
  end,
  GetOffset = function (this, length)
    if 1 / this < 0 then
      return length + this
    end
    return this
  end,
  ToString = function (this)
    return ((1 / this < 0) and '^' or '') .. this
  end
})
setmetatable(Index, { 
  __call = function (value, fromEnd)
    if value < 0 then
      throw(System.ArgumentOutOfRangeException("Non-negative number required."))
    end
    if fromEnd then
      if value == 0 then
        return -0.0
      end
      return -value
    end
    return value
  end
})

local function pointerAddress(p)
  local address = p[3]
  if address == nil then
    address = ssub(tostring(p), 7)
    p[3] = address
  end
  return address + p[2]
end

local Pointer
local function newPointer(t, i)
  return setmetatable({ t, i }, Pointer)
end

Pointer = {
  __index = false,
  get = function(this)
    local t, i = this[1], this[2]
    return t[i]
  end,
  set = function(this, value)
    local t, i = this[1], this[2]
    t[i] = value
  end,
  __add = function(this, count)
    return newPointer(this[1], this[2] + count)
  end,
  __sub = function(this, count)
    return newPointer(this[1], this[2] - count)
  end,
  __lt = function(t1, t2)
    return pointerAddress(t1) < pointerAddress(t2)
  end,
  __le = function(t1, t2)
    return pointerAddress(t1) <= pointerAddress(t2)
  end
}
Pointer.__index = Pointer

function System.stackalloc(t)
  return newPointer(t, 1)
end

local modules, imports = {}, {}
function System.import(f)
  imports[#imports + 1] = f
end

local namespace
local function defIn(kind, name, f)
  local namespaceName, isClass = namespace[1], namespace[2]
  if #namespaceName > 0 then
    name = namespaceName .. (isClass and "+" or ".") .. name
  end
  assert(modules[name] == nil, name)
  namespace[1], namespace[2] = name, kind == "C" or kind == "S"
  local t = f(assembly)
  namespace[1], namespace[2] = namespaceName, isClass
  modules[isClass and name:gsub("+", ".") or name] = function()
    return def(name, kind, t)
  end
end

namespace = {
  "",
  false,
  __index = false,
  class = function(name, f) defIn("C", name, f) end,
  struct = function(name, f) defIn("S", name, f) end,
  interface = function(name, f) defIn("I", name, f) end,
  enum = function(name, f) defIn("E", name, f) end,
  namespace = function(name, f)
    local namespaceName = namespace[1]
    name = namespaceName .. "." .. name
    namespace[1] = name
    f(namespace)
    namespace[1] = namespaceName
  end
}
namespace.__index = namespace

function System.namespace(name, f)
  if not assembly then assembly = setmetatable({}, namespace) end
  namespace[1] = name
  f(namespace)
  namespace[1], namespace[2] = "", false
end

function System.init(t)
  local path, files = t.path, t.files
  if files then
    path = (path and #path > 0) and (path .. '.') or ""
    for i = 1, #files do
      require(path .. files[i])
    end
  end

  metadatas = {}
  local types = t.types
  if types then
    local classes = {}
    for i = 1, #types do
      local name = types[i]
      local cls = assert(modules[name], name)()
      classes[i] = cls
    end
    assembly.classes = classes
  end

  for i = 1, #imports do
    imports[i](global)
  end

  local b, e = 1, #metadatas
  while true do
    for i = b, e do
      metadatas[i](global)
    end
    local len = #metadatas
    if len == e then
      break
    end
    b, e = e + 1, len
  end

  local main = t.Main
  if main then
    assembly.entryPoint = main
    System.entryAssembly = assembly
  end

  local attributes = t.assembly
  if attributes then
    if type(attributes) == "function" then
      attributes = attributes(global)
    end
    for k, v in pairs(attributes) do
      assembly[k] = v
    end
  end

  local current = assembly
  modules, imports, assembly, metadatas = {}, {}, nil, nil
  return current
end

System.config = rawget(global, "CSharpLuaSystemConfig") or {}
local isSingleFile = rawget(global, "CSharpLuaSingleFile")
if not isSingleFile then
  return function (config)
    if config then
      System.config = config 
    end
  end
end
