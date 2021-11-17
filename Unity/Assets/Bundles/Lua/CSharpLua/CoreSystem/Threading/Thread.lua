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
local throw = System.throw
local trunc = System.trunc
local post = System.post
local addTimer = System.addTimer
local Exception = System.Exception
local ArgumentNullException = System.ArgumentNullException
local ArgumentOutOfRangeException = System.ArgumentOutOfRangeException
local NotSupportedException = System.NotSupportedException

local assert = assert
local type = type
local setmetatable = setmetatable
local coroutine = coroutine
local ccreate = coroutine.create
local cresume = coroutine.resume
local cstatus = coroutine.status
local cyield = coroutine.yield

local mainThread

local ThreadStateException = define("System.Threading.ThreadStateException", {
  __tostring = Exception.ToString,
  base = { Exception },

  __ctor__ = function(this, message, innerException)
     Exception.__ctor__(this, message or "Thread is running or terminated; it cannot restart.", innerException)
  end
})

local ThreadAbortException = define("System.Threading.ThreadAbortException", {
  __tostring = Exception.ToString,
  base = { Exception },
  __ctor__ = function(this, message, innerException)
    Exception.__ctor__(this, message or "Thread aborted.", innerException)
end
})

local nextThreadId = 1
local currentThread

local function getThreadId()
  local id = nextThreadId
  nextThreadId = nextThreadId + 1
  return id
end

local function checkTimeout(timeout)
  if type(timeout) == "table" then
    timeout = trunc(timeout:getTotalMilliseconds())
  end
  if timeout < -1 or timeout > 2147483647 then
    throw(ArgumentOutOfRangeException("timeout"))
  end
  return timeout
end

local function resume(t, obj)
  local prevThread = currentThread
  currentThread = t
  local co = assert(t.co)
  local ok, v = cresume(co, obj)
  currentThread = prevThread
  if ok then
    if type(v) == "function" then
      v()
    elseif cstatus(co) == "dead" then
      local joinThread = t.joinThread
      if joinThread then
        resume(joinThread, true)
      end
      t.co = false
    end
  else
    t.co = false
    print("Warning: Thread.run" , v)
  end
end

local function run(t, obj)
  post(function ()
    resume(t, obj)
  end)
end

local Thread =  define("System.Threading.Thread", {
  IsBackground = false,
  IsThreadPoolThread = false,
  Priority = 2,
  ApartmentState = 2,
  Abort = function ()
    throw(ThreadAbortException())
  end,
  getCurrentThread = function ()
    return currentThread
  end,
  __ctor__ = function (this, start)
	  if start == nil then throw(ArgumentNullException("start")) end
    this.start = start
  end,
  getIsAlive = function (this)
    local co = this.co
    return co and cstatus(co) ~= "dead"
  end,
  ManagedThreadId = function (this)
	  local id = this.id
    if not id then
      id = getThreadId()
      this.id = id
    end
    return id
  end,
  Sleep = function (timeout)
    local current = currentThread
    if current == mainThread then
      throw(NotSupportedException("mainThread not support"))
    end
    timeout = checkTimeout(timeout)
    local f
    if timeout ~= -1 then
      f = function ()
        addTimer(function () 
          resume(current) 
        end, timeout)
      end
    end
    cyield(f)
  end,
  Yield = function ()
    local current = currentThread
    if current == mainThread then
      return false
    end
    cyield(function ()
      run(current)
    end)
    return true
  end,
  Join = function (this, timeout)
    if currentThread == mainThread then
      throw(NotSupportedException("mainThread not support"))
    end
    if this.joinThread then
      throw(ThreadStateException())
    end
    this.joinThread = currentThread  
    if timeout == nil then
      cyield()
    else
      timeout = checkTimeout(timeout)
      local f
      if timeout ~= -1 then
        f = function ()
          addTimer(function ()
            resume(currentThread, false)
          end, timeout)
        end
      end
      return cyield(f)
    end
  end,
  Start = function (this, parameter)
    if this.co ~= nil then throw(ThreadStateException()) end
    local co = ccreate(this.start)
    this.co = co
    this.start = nil
    run(this, parameter)
  end,
  waitTask = function (taskContinueActions)
    if currentThread == mainThread then
      throw(NotSupportedException("mainThread not support"))
    end
    taskContinueActions[#taskContinueActions + 1] = function ()
      resume(currentThread)
    end
    cyield()
  end,
})

mainThread = setmetatable({ id = getThreadId() }, Thread)
currentThread = mainThread

System.ThreadStateException = ThreadStateException
System.ThreadAbortException = ThreadAbortException
System.Thread = Thread
