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
local define = System.define
local Object = System.Object

local tconcat = table.concat
local type = type
local debug = debug

local function getMessage(this)
  return this.message or ("Exception of type '%s' was thrown."):format(this.__name__)
end

local traceback = (debug and debug.traceback) or System.config.traceback or function () return "" end
System.traceback = traceback

local function toString(this)
  local t = { this.__name__ }
  local count = 2
  local message, innerException, stackTrace = getMessage(this), this.innerException, this.errorStack
  t[count] = ": "
  t[count + 1] = message
  count = count + 2
  if innerException then
    t[count] = "---> "
    t[count + 1] = innerException:ToString()
    count = count + 2
  end
  if stackTrace then
    t[count] = stackTrace
  end
  return tconcat(t)
end

local function ctorOfException(this, message, innerException)
  this.message = message
  this.innerException = innerException
end

local Exception = define("System.Exception", {
  __tostring = toString,
  __ctor__ = ctorOfException,
  ToString = toString,
  getMessage = getMessage,
  getInnerException = function(this) 
    return this.innerException
  end,
  getStackTrace = function(this) 
    return this.errorStack
  end,
  getData = function (this)
    local data = this.data
    if not data then
      data = System.Dictionary(Object, Object)()
      this.data = data
    end
    return data
  end,
  traceback = function(this, lv)
    this.errorStack = traceback("", lv and lv + 3 or 3)
  end
})

local SystemException = define("System.SystemException", {
  __tostring = toString,
  base = { Exception },
  __ctor__ = function (this, message, innerException)
    ctorOfException(this, message or "System error.", innerException)
  end
})

local ArgumentException = define("System.ArgumentException", {
  __tostring = toString,
  base = { SystemException },
  __ctor__ = function(this, message, paramName, innerException)
    if type(paramName) == "table" then
      paramName, innerException = nil, paramName
    end
    ctorOfException(this, message or "Value does not fall within the expected range.", innerException)
    this.paramName = paramName
    if paramName and #paramName > 0 then
      this.message = this.message .. "\nParameter name: " .. paramName
    end
  end,
  getParamName = function(this) 
    return this.paramName
  end
})

define("System.ArgumentNullException", {
  __tostring = toString,
  base = { ArgumentException },
  __ctor__ = function(this, paramName, message, innerException) 
    ArgumentException.__ctor__(this, message or "Value cannot be null.", paramName, innerException)
  end
})

define("System.ArgumentOutOfRangeException", {
  __tostring = toString,
  base = { ArgumentException },
  __ctor__ = function(this, paramName, message, innerException, actualValue) 
    ArgumentException.__ctor__(this, message or "Specified argument was out of the range of valid values.", paramName, innerException)
    this.actualValue = actualValue
  end,
  getActualValue = function(this) 
    return this.actualValue
  end
})

define("System.IndexOutOfRangeException", {
   __tostring = toString,
   base = { SystemException },
   __ctor__ = function (this, message, innerException)
    ctorOfException(this, message or "Index was outside the bounds of the array.", innerException)
  end
})

define("System.CultureNotFoundException", {
  __tostring = toString,
  base = { ArgumentException },
  __ctor__ = function(this, paramName, invalidCultureName, message, innerException, invalidCultureId) 
    if not message then 
      message = "Culture is not supported."
      if paramName then
        message = message .. "\nParameter name = " .. paramName
      end
      if invalidCultureName then
        message = message .. "\n" .. invalidCultureName .. " is an invalid culture identifier."
      end
    end
    ArgumentException.__ctor__(this, message, paramName, innerException)
    this.invalidCultureName = invalidCultureName
    this.invalidCultureId = invalidCultureId
  end,
  getInvalidCultureName = function(this)
    return this.invalidCultureName
  end,
  getInvalidCultureId = function(this) 
    return this.invalidCultureId
  end
})

local KeyNotFoundException = define("System.Collections.Generic.KeyNotFoundException", {
  __tostring = toString,
  base = { SystemException },
  __ctor__ = function(this, message, innerException) 
    ctorOfException(this, message or "The given key was not present in the dictionary.", innerException)
  end
})
System.KeyNotFoundException = KeyNotFoundException

local ArithmeticException = define("System.ArithmeticException", {
  __tostring = toString,
  base = { SystemException },
  __ctor__ = function(this, message, innerException) 
    ctorOfException(this, message or "Overflow or underflow in the arithmetic operation.", innerException)
  end
})

define("System.DivideByZeroException", {
  __tostring = toString,
  base = { ArithmeticException },
  __ctor__ = function(this, message, innerException) 
    ArithmeticException.__ctor__(this, message or "Attempted to divide by zero.", innerException)
  end
})

define("System.OverflowException", {
  __tostring = toString,
  base = { ArithmeticException },
  __ctor__ = function(this, message, innerException) 
    ArithmeticException.__ctor__(this, message or "Arithmetic operation resulted in an overflow.", innerException)
  end
})

define("System.FormatException", {
  __tostring = toString,
  base = { SystemException },
  __ctor__ = function(this, message, innerException) 
    ctorOfException(this, message or "Invalid format.", innerException)
  end
})

define("System.InvalidCastException", {
  __tostring = toString,
  base = { SystemException },
  __ctor__ = function(this, message, innerException) 
    ctorOfException(this, message or "Specified cast is not valid.", innerException)
  end
})

local InvalidOperationException = define("System.InvalidOperationException", {
  __tostring = toString,
  base = { SystemException },
  __ctor__ = function(this, message, innerException) 
    ctorOfException(this, message or "Operation is not valid due to the current state of the object.", innerException)
  end
})

define("System.NotImplementedException", {
  __tostring = toString,
  base = { SystemException },
  __ctor__ = function(this, message, innerException) 
    ctorOfException(this, message or "The method or operation is not implemented.", innerException)
  end
})

define("System.NotSupportedException", {
  __tostring = toString,
  base = { SystemException },
  __ctor__ = function(this, message, innerException) 
    ctorOfException(this, message or "Specified method is not supported.", innerException)
  end
})

define("System.NullReferenceException", {
  __tostring = toString,
  base = { SystemException },
  __ctor__ = function(this, message, innerException) 
    ctorOfException(this, message or "Object reference not set to an instance of an object.", innerException)
  end
})

define("System.RankException", {
  __tostring = toString,
  base = { Exception },
  __ctor__ = function(this, message, innerException) 
    ctorOfException(this, message or "Attempted to operate on an array with the incorrect number of dimensions.", innerException)
  end
})

define("System.TypeLoadException", {
  __tostring = toString,
  base = { Exception },
  __ctor__ = function(this, message, innerException) 
    ctorOfException(this, message or "Failed when load type.", innerException)
  end
})

define("System.ObjectDisposedException", {
  __tostring = toString,
  base = { InvalidOperationException },
  __ctor__ = function(this, objectName, message, innerException)
    ctorOfException(this, message or "Cannot access a disposed object.", innerException)
    this.objectName = objectName
    if objectName and #objectName > 0 then
      this.message = this.message .. "\nObject name: '" .. objectName .. "'."
    end
  end
})

local function toStringOfAggregateException(this)
  local t = { toString(this) }
  local count = 2
  for i = 0, this.innerExceptions:getCount() - 1 do
    t[count] = "\n---> (Inner Exception #"
    t[count + 1] = i
    t[count + 2] = ") "
    t[count + 3] = this.innerExceptions:get(i):ToString()
    t[count + 4] = "<---\n"
    count = count + 5
  end
  return tconcat(t)
end

define("System.AggregateException", {
  ToString = toStringOfAggregateException,
  __tostring = toStringOfAggregateException,
  base = { Exception },
  __ctor__ = function (this, message, innerExceptions)
    if type(message) == "table" then
      message, innerExceptions = nil, message
    end
    Exception.__ctor__(this, message or "One or more errors occurred.")
    local ReadOnlyCollection = System.ReadOnlyCollection(Exception)
    if innerExceptions then
      if System.is(innerExceptions, Exception) then
        local list = System.List(Exception)()
        list:Add(innerExceptions)
        this.innerExceptions = ReadOnlyCollection(list)
      else
        if not System.isArrayLike(innerExceptions) then
          innerExceptions = System.Array.toArray(innerExceptions)
        end
        this.innerExceptions = ReadOnlyCollection(innerExceptions)
      end
    else
      this.innerExceptions = ReadOnlyCollection(System.Array.Empty(Exception))
    end
  end,
  getInnerExceptions = function (this)
    return this.innerExceptions
  end
})

System.SwitchExpressionException = define("System.Runtime.CompilerServices", {
  __tostring = toString,
  base = { InvalidOperationException },
  __ctor__ = function(this, message, innerException)
    ctorOfException(this, message or "Non-exhaustive switch expression failed to match its input.", innerException)
  end
})
