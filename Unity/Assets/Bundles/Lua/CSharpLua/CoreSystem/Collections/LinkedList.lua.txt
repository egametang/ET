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
local each = System.each
local checkIndexAndCount = System.checkIndexAndCount
local ArgumentNullException = System.ArgumentNullException
local InvalidOperationException = System.InvalidOperationException
local EqualityComparer = System.EqualityComparer

local setmetatable = setmetatable
local select = select

local LinkedListNode = define("System.Collections.Generic.LinkedListNode", {
  __ctor__ = function (this, value)
    this.Value = value
  end,
  getNext = function (this)
    local next = this.next
    if next == nil or next == this.List.head then
      return nil
    end
    return next
  end,
  getPrevious = function (this)
    local prev = this.prev
    if prev == nil or this == this.List.head then
      return nil
    end
    return prev
  end
})
System.LinkedListNode = LinkedListNode

local function newLinkedListNode(list, value)
  return setmetatable({ List = assert(list), Value = value }, LinkedListNode)
end

local function vaildateNewNode(this, node)
  if node == nil then
    throw(ArgumentNullException("node"))
  end
  if node.List ~= nil then
    throw(InvalidOperationException("ExternalLinkedListNode"))
  end
end

local function vaildateNode(this, node)
  if node == nil then
    throw(ArgumentNullException("node"))
  end
  if node.List ~= this then
    throw(InvalidOperationException("ExternalLinkedListNode"))
  end
end

local function insertNodeBefore(this, node, newNode)
  newNode.next = node
  newNode.prev = node.prev
  node.prev.next = newNode
  node.prev = newNode
  this.Count = this.Count + 1
  this.version = this.version + 1
end

local function insertNodeToEmptyList(this, newNode)
  newNode.next = newNode
  newNode.prev = newNode
  this.head = newNode
  this.Count = this.Count + 1
  this.version = this.version + 1
end

local function invalidate(this)
  this.List = nil
  this.next = nil
  this.prev = nil
end

local function remvoeNode(this, node)
  if node.next == node then
    this.head = nil
  else
    node.next.prev = node.prev
    node.prev.next = node.next
    if this.head == node then
      this.head = node.next
    end
  end
  invalidate(node)
  this.Count = this.Count - 1
  this.version = this.version + 1
end

local LinkedListEnumerator = { 
  __index = false,
  getCurrent = System.getCurrent, 
  Dispose = System.emptyFn,
  MoveNext = function (this)
    local list = this.list
    local node = this.node
    if this.version ~= list.version then
      System.throwFailedVersion()
    end
    if node == nil then
      return false
    end
    this.current = node.Value
    node = node.next
    if node == list.head then
      node = nil
    end
    this.node = node
    return true
  end
}
LinkedListEnumerator.__index = LinkedListEnumerator

local LinkedList = { 
  Count = 0, 
  version = 0,
  __ctor__ = function (this, ...)
    local len = select("#", ...)
    if len == 1 then
      local collection = ...
      if collection == nil then
        throw(ArgumentNullException("collection"))
      end
      for _, item in each(collection) do
        this:AddLast(item)
      end
    end
  end,
  getCount = function (this)
    return this.Count
  end,
  getFirst = function(this)    
    return this.head
  end,
  getLast = function (this)
    local head = this.head
    return head ~= nil and head.prev or nil
  end,
  AddAfterNode = function (this, node, newNode)
    vaildateNode(this, node)
    vaildateNewNode(this, newNode)
    insertNodeBefore(this, node.next, newNode)
    newNode.List = this
  end,
  AddAfter = function (this, node, value)    
    vaildateNode(this, node)
    local result = newLinkedListNode(node.List, value)
    insertNodeBefore(this, node.next, result)
    return result
  end,
  AddBeforeNode = function (this, node, newNode)
    vaildateNode(this, node)
    vaildateNewNode(this, newNode)
    insertNodeBefore(this, node, newNode)
    newNode.List = this
    if node == this.head then
      this.head = newNode
    end
  end,
  AddBefore = function (this, node, value)
    vaildateNode(this, node)
    local result = newLinkedListNode(node.List, value)
    insertNodeBefore(this, node, result)
    if node == this.head then
      this.head = result
    end
    return result
  end,
  AddFirstNode = function (this, node)
	  vaildateNewNode(this, node)
    if this.head == nil then
      insertNodeToEmptyList(this, node)
    else
      insertNodeBefore(this, this.head, node)
      this.head = node
    end
    node.List = this
  end,
  AddFirst = function (this, value)
    local result = newLinkedListNode(this, value)
    if this.head == nil then
      insertNodeToEmptyList(this, result)
    else
      insertNodeBefore(this, this.head, result)
      this.head = result
    end
    return result
  end,
  AddLastNode = function (this, node)
    vaildateNewNode(this, node)
    if this.head == nil then
      insertNodeToEmptyList(this, node)
    else
      insertNodeBefore(this, this.head, node)
    end
    node.List = this
  end,
  AddLast = function (this, value)
    local result = newLinkedListNode(this, value)
    if this.head == nil then
      insertNodeToEmptyList(this, result)
    else
      insertNodeBefore(this, this.head, result)
    end
    return result
  end,
  Clear = function (this)
    local current = this.head
    while current ~= nil do
      local temp = current
      current = current.next
      invalidate(temp)
    end
    this.head = nil
    this.Count = 0
    this.version = this.version + 1
  end,
  Contains = function (this, value)
    return this:Find(value) ~= nil
  end,
  CopyTo = function (this, array, index)
    checkIndexAndCount(array, index, this.Count)
    local head = this.head
    local node = head
    if node then
      index = index + 1
      repeat
        local value = node.Value
        if value == nil then value = System.null end
        array[index] = value
        index = index + 1
        node = node.next
      until node == head
    end
  end,
  Find = function (this, value)     
    local head = this.head
    local node = head
    local comparer = EqualityComparer(this.__genericT__).getDefault()
    local equals = comparer.EqualsOf
    if node ~= nil then
      if value ~= nil then
        repeat
          if equals(comparer, node.Value, value) then
            return node
          end
          node = node.next
        until node == head
      else
        repeat 
          if node.Value == nil then
            return node
          end
          node = node.next
        until node == head
      end
    end
    return nil
  end,
  FindLast = function (this, value)
    local head = this.head
    if head == nil then return nil end
    local last = head.prev
    local node = last
    local comparer = EqualityComparer(this.__genericT__).getDefault()
    local equals = comparer.EqualsOf
    if node ~= nil then
      if value ~= nil then
        repeat
          if equals(comparer, node.Value, value) then
            return node
          end
          node = node.prev
        until node == last
      else
        repeat 
          if node.Value == nil then
            return node
          end
          node = node.prev
         until node == last
      end
    end
    return nil
  end,
  RemoveNode = function (this, node)
    vaildateNode(this, node)
    remvoeNode(this, node)
  end,
  Remove = function (this, node)
    node = this:Find(node)
    if node ~= nil then
      remvoeNode(this, node)
    end
    return false
  end,
  RemoveFirst = function (this)
    local head = this.head
    if head == nil then
      throw(InvalidOperationException("LinkedListEmpty"))
    end
    remvoeNode(this, head)
  end,
  RemoveLast = function (this)
    local head = this.head
    if head == nil then
      throw(InvalidOperationException("LinkedListEmpty"))
    end
    remvoeNode(this, head.prev)
  end,
  GetEnumerator = function (this)
    return setmetatable({ list = this, version = this.version, node = this.head }, LinkedListEnumerator)
  end
}

function System.linkedListFromTable(t, T)
  return setmetatable(t, LinkedList(T))
end

System.LinkedList = define("System.Collections.Generic.LinkedList", function(T) 
  return { 
  base = { System.ICollection_1(T), System.ICollection }, 
  __genericT__ = T,
  __len = LinkedList.getCount
  }
end, LinkedList, 1)
