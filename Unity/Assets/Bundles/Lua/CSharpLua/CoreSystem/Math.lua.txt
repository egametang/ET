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
local trunc = System.trunc

local math = math
local floor = math.floor
local min = math.min
local max = math.max
local abs = math.abs

local function bigMul(a, b)
  return a * b
end

local function divRem(a, b)
  local remainder = a % b
  return (a - remainder) / b, remainder
end

local function round(value, digits, mode)
  local mult = 10 ^ (digits or 0)
  local i = value * mult
  if mode == 1 then
    value = trunc(i + (value >= 0 and 0.5 or -0.5))
  else
    value = trunc(i)
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
  end
  return value / mult
end

local function sign(v)
  return v == 0 and 0 or (v > 0 and 1 or -1) 
end

local function IEEERemainder(x, y)
  if x ~= x then
    return x
  end
  if y ~= y then
    return y
  end
  local regularMod = System.mod(x, y)
  if regularMod ~= regularMod then
    return regularMod
  end
  if regularMod == 0 and x < 0 then
    return -0.0
  end
  local alternativeResult = regularMod - abs(y) * sign(x)
  local i, j = abs(alternativeResult), abs(regularMod)
  if i == j then
    local divisionResult = x / y
    local roundedResult = round(divisionResult)
    if abs(roundedResult) > abs(divisionResult) then
      return alternativeResult
    else
      return regularMod
    end
  end
  if i < j then
    return alternativeResult
  else
    return regularMod
  end
end

local function clamp(a, b, c)
  return min(max(a, b), c)
end

local function truncate(d)
  return trunc(d) * 1.0
end

local exp = math.exp
local cosh = math.cosh or function(x) return (exp(x) + exp(-x)) / 2.0 end
local pow = math.pow or function(x, y) return x ^ y end
local sinh = math.sinh or function(x) return (exp(x) - exp(-x)) / 2.0 end
local tanh = math.tanh or function(x) return sinh(x) / cosh(x) end

local Math = math
Math.Abs = abs
Math.Acos = math.acos
Math.Asin = math.asin
Math.Atan = math.atan
Math.Atan2 = math.atan2 or math.atan
Math.BigMul = bigMul
Math.Ceiling = math.ceil
Math.Clamp = clamp
Math.Cos = math.cos
Math.Cosh = cosh
Math.DivRem = divRem
Math.Exp = exp
Math.Floor = math.floor
Math.IEEERemainder = IEEERemainder
Math.Log = math.log
Math.Log10 = math.log10
Math.Max = math.max
Math.Min = math.min
Math.Pow = pow
Math.Round = round
Math.Sign = sign
Math.Sin = math.sin
Math.Sinh = sinh
Math.Sqrt = math.sqrt
Math.Tan = math.tan
Math.Tanh = tanh
Math.Truncate = truncate

System.define("System.Math", Math)