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

local maxExpiration = 9223372036854775807  --[[Int64.MaxValue]]
local LinkedListEvent =  System.LinkedList(System.Object) 
local TimeoutQueue = define("System.TimeoutQueue", (function ()
  local getNextId, Insert, Add, AddRepeating, AddRepeating1, getNextExpiration, Erase, RunLoop, 
  getCount, Contains, IsNext, __ctor__
  __ctor__ = function (this)
    this.ids_ = {}
    this.events_ = LinkedListEvent()
  end
  getNextId = function (this)
    local default = this.nextId_
    this.nextId_ = default + 1
    return default
  end
  Insert = function (this, e)
    this.ids_[e.Id] = e
    local next = this.events_:getFirst()
    while next ~= nil and next.Value.Expiration <= e.Expiration do
      next = next:getNext()
    end
    if next ~= nil then
      e.LinkNode = this.events_:AddBefore(next, e)
    else
      e.LinkNode = this.events_:AddLast(e)
    end
  end
  Add = function (this, now, delay, callback)
    return AddRepeating1(this, now, delay, 0, callback)
  end
  AddRepeating = function (this, now, interval, callback)
    return AddRepeating1(this, now, interval, interval, callback)
  end
  AddRepeating1 = function (this, now, delay, interval, callback)
    local id = getNextId(this)
    Insert(this, {
      Id = id,
      Expiration = now + delay,
      RepeatInterval = interval,
      Callback = callback
    })
    return id
  end
  getNextExpiration = function (this)
    return this.events_.Count > 0 and this.events_:getFirst().Value.Expiration or maxExpiration
  end
  Erase = function (this, id)
    local e = this.ids_[id]
    if e then
      this.ids_[id] = nil
      this.events_:RemoveNode(e.LinkNode)
      return true
    end
    return false
  end
  RunLoop = function (this, now)
    while true do
      local nextExp = getNextExpiration(this)
      if nextExp <= now then
        local e = this.events_:getFirst().Value
        Erase(this, e.Id)
        if e.RepeatInterval > 0 then
          e.Expiration = now + e.RepeatInterval
          Insert(this, e)
        end
        e.Callback(e.Id, now)
      else
        return nextExp
      end
    end
  end
  getCount = function (this)
    return this.events_.Count
  end
  Contains = function (this, id)
    return this.ids_[id] ~= nil
  end
	IsNext = function (this, id)
		local first = this.events_:getFirst()
		local nextId = first and first.Value.Id
		return nextId == id
	end
  return {
    MaxExpiration = maxExpiration,
    nextId_ = 1,
    Add = Add,
    AddRepeating = AddRepeating,
    AddRepeating1 = AddRepeating1,
    getNextExpiration = getNextExpiration,
    Erase = Erase,
    RunLoop = RunLoop,
    getCount = getCount,
    Contains = Contains,
    __ctor__ = __ctor__,
		IsNext = IsNext
  }
end)())

local timerQueue = TimeoutQueue()
local driverTimer

local function runTimerQueue()
  local now = currentTimeMillis()
  local nextExpiration = timerQueue:RunLoop(now)
  if nextExpiration ~= maxExpiration then
    driverTimer = setTimeout(runTimerQueue, nextExpiration - now)
  else
    driverTimer = nil
  end
end

local function addTimer(fn, dueTime, period)
  local now = currentTimeMillis()
  local id = timerQueue:AddRepeating1(now, dueTime, period or 0, fn)
  if timerQueue:IsNext(id) then
    if driverTimer then
      clearTimeout(driverTimer)
    end
    driverTimer = setTimeout(runTimerQueue, dueTime)
  end
  return id
end

local function removeTimer(id)
  local isNext = timerQueue:IsNext(id)
	timerQueue:Erase(id)
	if isNext then
		clearTimeout(driverTimer)
		local delay = timerQueue:getNextExpiration() - currentTimeMillis()
		driverTimer = setTimeout(runTimerQueue, delay)
	end
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
  if this.id == -1 then throw(ObjectDisposedException()) end
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
    this.id = -1
  end,
  __gc = close
})

