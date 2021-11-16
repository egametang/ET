local System = System
local SystemNumerics = System.Numerics

local abs = math.abs
local min = math.min
local max = math.max
local sqrt = math.sqrt

local IComparable = System.IComparable
local IComparable_1 = System.IComparable_1
local IEquatable_1 = System.IEquatable_1
local IFormattable = System.IFormattable

local new = function (cls, ...)
    local this = setmetatable({}, cls)
    return this, cls.__ctor__(this, ...)
end

local Vector2 = {}

Vector2.__ctor__ = function(this, X, Y)
    if Y == nil then
        this.X = X or 0
        this.Y = X or 0
    else
        this.X = X or 0
        this.Y = Y or 0
    end
    local mt = getmetatable(this)
    mt.__unm = Vector2.op_UnaryNegation
    setmetatable(this, mt)
end

Vector2.base = function (_, T)
    return { IComparable, IComparable_1(T), IEquatable_1(T), IFormattable }
end

Vector2.getZero = function ()
    return new(Vector2, 0, 0, 0)
end
Vector2.getOne = function ()
    return new(Vector2, 1.0, 1.0)
end
Vector2.getUnitX = function ()
    return new(Vector2, 1.0, 0.0)
end
Vector2.getUnitY = function ()
    return new(Vector2, 0.0, 1.0)
end

Vector2.CopyTo = function(this, array, index)
    if index == nil then
        index = 0
    end

    if array == nil then
        System.throw(System.NullReferenceException())
    end

    if index < 0 or index >= #array then
        System.throw(System.ArgumentOutOfRangeException())
    end
    if (#array - index) < 2 then
        System.throw(System.ArgumentException())
    end

    array:set(index, this.X)
    array:set(index + 1, this.Y)
end

Vector2.Equals = function (this, other)
    if not (System.is(other, Vector2)) then
        return false
    end
    other = System.cast(Vector2, other)
    return this.X == other.X and this.Y == other.Y
end

Vector2.Dot = function (value1, value2)
    return value1.X * value2.X + value1.Y * value2.Y
end

Vector2.Min = function (value1, value2)
    return new(Vector2, (value1.X < value2.X) and value1.X or value2.X, (value1.Y < value2.Y) and value1.Y or value2.Y)
end

Vector2.Max = function (value1, value2)
    return new(Vector2, (value1.X > value2.X) and value1.X or value2.X, (value1.Y > value2.Y) and value1.Y or value2.Y)
end

Vector2.Abs = function (value)
    return new(Vector2, abs(value.X), abs(value.Y))
end

Vector2.SquareRoot = function (value)
    return new(Vector2, System.ToSingle(sqrt(value.X)), System.ToSingle(sqrt(value.Y)))
end

Vector2.op_Addition = function (left, right)
    return new(Vector2, left.X + right.X, left.Y + right.Y)
end

Vector2.Add = function (left, right)
    return Vector2.op_Addition(left, right)
end

Vector2.op_Subtraction = function (left, right)
    return new(Vector2, left.X - right.X, left.Y - right.Y)
end

Vector2.Subtract = function (left, right)
    return Vector2.op_Subtraction(left, right)
end

Vector2.op_Multiply = function (left, right)
    if type(left) == "number" then
        left = new(Vector2, left)
    end

    if type(right) == "number" then
        right = new(Vector2, right)
    end

    return new(Vector2, left.X * right.X, left.Y * right.Y)
end

Vector2.Multiply = function (left, right)
    return Vector2.op_Multiply(left, right)
end

Vector2.op_Division = function (left, right)
    if type(right) == "number" then
        return Vector2.op_Multiply(left, 1.0 / right)
    end
    return new(Vector2, left.X / right.X, left.Y / right.Y)
end

Vector2.Divide = function (left, right)
    return Vector2.op_Division(left, right)
end

Vector2.op_UnaryNegation = function (value)
    return Vector2.op_Subtraction(Vector2.getZero(), value)
end

Vector2.Negate = function (value)
    return - value
end

Vector2.op_Equality = function (left, right)
    return (left.X == right.X and left.Y == right.Y )
end

Vector2.op_Inequality = function (left, right)
    return (left.X ~= right.X or left.Y ~= right.Y)
end

Vector2.GetHashCode = function (this)
    local hash = this.X:GetHashCode()
    hash = SystemNumerics.HashCodeHelper.CombineHashCodes(hash, this.Y:GetHashCode())
    return hash
end

Vector2.ToString = function (this)
    local sb = System.StringBuilder()
    local separator = 44 --[[',']]
    sb:AppendChar(60 --[['<']])
    sb:Append(this.X:ToString())
    sb:AppendChar(separator)
    sb:AppendChar(32 --[[' ']])
    sb:Append(this.Y:ToString())
    sb:AppendChar(62 --[['>']])
    return sb:ToString()
end

Vector2.Length = function (this)
    local ls = this.X * this.X + this.Y * this.Y
    return System.ToSingle(sqrt(ls))
end

Vector2.LengthSquared = function (this)
    return this.X * this.X + this.Y * this.Y
end

Vector2.Distance = function (value1, value2)
    local dx = value1.X - value2.X
    local dy = value1.Y - value2.Y

    local ls = dx * dx + dy * dy

    return System.ToSingle(sqrt(ls))
end

Vector2.DistanceSquared = function (value1, value2)
    local dx = value1.X - value2.X
    local dy = value1.Y - value2.Y

    return dx * dx + dy * dy
end

Vector2.Normalize = function (value)
    local ls = value.X * value.X + value.Y * value.Y
    local invNorm = 1.0 / System.ToSingle(sqrt(ls))

    return new(Vector2, value.X * invNorm, value.Y * invNorm)
end

Vector2.Reflect = function (vector, normal)
    local dot = vector.X * normal.X + vector.Y * normal.Y

    return new(Vector2, vector.X - 2.0 * dot * normal.X, vector.Y - 2.0 * dot * normal.Y)
end

Vector2.Clamp = function (value1, min, max)
    -- This compare order is very important!!!
    -- We must follow HLSL behavior in the case user specified min value is bigger than max value.
    local x = value1.X
    x = (x > max.X) and max.X or x
    x = (x < min.X) and min.X or x

    local y = value1.Y
    y = (y > max.Y) and max.Y or y
    y = (y < min.Y) and min.Y or y

    return new(Vector2, x, y)
end

Vector2.Lerp = function (value1, value2, amount)
    return new(Vector2, value1.X + (value2.X - value1.X) * amount, value1.Y + (value2.Y - value1.Y) * amount)
end

Vector2.Transform = function (position, matrix)
    if matrix.M41 == nil then
        -- 3x2 matrix
        return new(Vector2, position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M31, 
                            position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M32
                            )
    elseif matrix.X == nil then
        -- 4x4 matrix
        return new(Vector2, position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41, 
                            position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42
                            )
    else 
        -- Quaternion
        local x2 = matrix.X + matrix.X
        local y2 = matrix.Y + matrix.Y
        local z2 = matrix.Z + matrix.Z

        local wz2 = matrix.W * z2
        local xx2 = matrix.X * x2
        local xy2 = matrix.X * y2
        local yy2 = matrix.Y * y2
        local zz2 = matrix.Z * z2

        return new(Vector2, position.X * (1.0 - yy2 - zz2) + position.Y * (xy2 - wz2),
                            position.X * (xy2 + wz2) + position.Y * (1.0 - xx2 - zz2)
                                )    
    end
end

Vector2.TransformNormal = function (normal, matrix)
    if matrix.M41 == nil then
        -- 3.2 matirx
        return new(Vector2, normal.X * matrix.M11 + normal.Y * matrix.M21, 
                            normal.X * matrix.M12 + normal.Y * matrix.M22
                                )
    else
        -- 4x4 matrix
        return new(Vector2, normal.X * matrix.M11 + normal.Y * matrix.M21, 
                            normal.X * matrix.M12 + normal.Y * matrix.M22
                                )
    end
 end

System.defStc("System.Numerics.Vector2", Vector2)