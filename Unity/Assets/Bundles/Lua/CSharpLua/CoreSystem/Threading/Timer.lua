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
local Array = System.Array
local heapAdd = Array.heapAdd
local heapPop = Array.heapPop
local heapDown = Array.heapDown
local currentTimeMillis = System.currentTimeMillis
local ArgumentNullException = System.ArgumentNullException
local ArgumentOutOfRangeException  = System.ArgumentOutOfRangeException
local NotImplementedException = System.NotImplementedException
local ObjectDisposedException = System.ObjectDisposedException

local type = type

local config = System.config
local setTimeout = config.setTimeout
local clearTimeout = config.clearTimeout

if setTimeout and clearTimeout then
  System.post = function (fn) 
    setTimeout(fn, 0) 
  end
else
  System.post = function (fn)
    fn()
  end
  local function notset()
    throw(NotImplementedException("System.config.setTimeout or clearTimeout is not registered."))
  end
  setTimeout = notset
  clearTimeout = notset
end

local TimeoutQueue = define("System.TimeoutQueue", (function ()
  local Add, AddRepeating, AddRepeating1, getNextExpiration, Erase, RunLoop, Contains, __ctor__
  __ctor__ = function (this)
	this.c = function (a, b) return a.Expiration - b.Expiration end
  end
  Add = function (this, now, delay, callback)
    return AddRepeating1(this, now, delay, 0, callback)
  end
  AddRepeating = function (this, now, interval, callback)
    return AddRepeating1(this, now, interval, interval, callback)
  end
  AddRepeating1 = function (this, now, delay, interval, callback)
    local id = {
      Expiration = now + delay,
      RepeatInterval = interval,
      Callback = callback
    }
    heapAdd(this, id, this.c)
    return id
  end
  getNextExpiration = function (this)
    if #this > 0 then return this[1].Expiration end
  end
  Erase = function (this, id)
    if not id.cancel then
      id.cancel = true
      return true
    end
    return false
  end
  RunLoop = function (this, now)
    while true do
      local e = this[1]
      if e == nil then break end
      if e.cancel then
        heapPop(this, this.c)
      else
        if e.Expiration <= now then
          if e.RepeatInterval <= 0 then
            heapPop(this, this.c)
            e.cancel = true
          else
            e.Expiration = now + e.RepeatInterval
            heapDown(this, 1, #this, this.c)
          end
          e.Callback(e, now)
        else
          return e.Expiration
        end
      end
    end
  end
  Contains = function (this, id)
    return not id.cancel
  end
  return {
    nextId_ = 1,
    Add = Add,
    AddRepeating = AddRepeating,
    AddRepeating1 = AddRepeating1,
    getNextExpiration = getNextExpiration,
    Erase = Erase,
    RunLoop = RunLoop,
    Contains = Contains,
    __ctor__ = __ctor__
  }
end)())

local timerQueue = TimeoutQueue()
local driverTimer

local function runTimerQueue()
  local now = currentTimeMillis()
  local nextExpiration = timerQueue:RunLoop(now)
  if nextExpiration then
    driverTimer = setTimeout(runTimerQueue, nextExpiration - now)
  else
    driverTimer = nil
  end
end

local function addTimer(fn, dueTime, period)
  local now = currentTimeMillis()
  local id = timerQueue:AddRepeating1(now, dueTime, period or 0, fn)
  if timerQueue[1] == id then
    if driverTimer then
      clearTimeout(driverTimer)
    end
    driverTimer = setTimeout(runTimerQueue, dueTime)
  end
  return id
end

local function removeTimer(id)
  return timerQueue:Erase(id)
end

System.addTimer = addTimer
System.removeTimer = removeTimer

local function close(this)
  local id = this.id
  if id then
    removeTimer(id)
  end
end

local function change(this, dueTime, period)
  if type(dueTime) == "table" then
    dueTime = dueTime:getTotalMilliseconds()
    period = period:getTotalMilliseconds()
  end
  if dueTime < -1 or dueTime > 0xfffffffe then
    throw(ArgumentOutOfRangeException("dueTime"))
  end
  if period < -1 or period > 0xfffffffe then
    throw(ArgumentOutOfRangeException("period"))
  end
  if this.id == false then throw(ObjectDisposedException()) end
  close(this)
  if dueTime ~= -1 then
    this.id = addTimer(this.callback, dueTime, period)
  end
  return true
end

System.Timer = define("System.Threading.Timer", {
  __ctor__ =  function (this, callback, state,  dueTime, period)
    if callback == nil then throw(ArgumentNullException("callback")) end
    this.callback = function () callback(state) end
    change(this, dueTime, period)
  end,
  Change = change,
  Dispose = function (this)
    close(this)
    this.id = false
  end,
  getActiveCount = function ()
    local count = 0
    for i = 1, #timerQueue do
      if not timerQueue[i].cancel then
        count = count + 1
      end
    end
    return count
  end,
  __gc = close
})

