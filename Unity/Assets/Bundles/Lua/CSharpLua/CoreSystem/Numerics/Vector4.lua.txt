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

local Vector4 = {}

Vector4.__ctor__ = function(this, X, Y, Z, W)
    if W == nil then
        if Z == nil then
            -- 1 var constructor
            if Y == nil then
                this.X = X or 0
                this.Y = X or 0
                this.Z = X or 0
                this.W = X or 0
            else
            -- 2 var constructor
                this.X = X.X
                this.Y = X.Y
                this.Z = X.Z
                this.W = Y or 0
            end
        else
        -- 3 var constructor
        this.X = X.X
        this.Y = X.Y
        this.Z = Y or 0
        this.W = Z or 0
        end
    else
    -- 4 var constructor
        this.X = X or 0
        this.Y = Y or 0
        this.Z = Z or 0
        this.W = W or 0
    end
    local mt = getmetatable(this)
    mt.__unm = Vector4.op_UnaryNegation
    setmetatable(this, mt)
end

Vector4.base = function (_, T)
    return { IComparable, IComparable_1(T), IEquatable_1(T), IFormattable }
end

Vector4.getOne = function() return new(Vector4, 1.0, 1.0, 1.0, 1.0) end
Vector4.getZero = function() return new(Vector4, 0, 0, 0, 0) end
Vector4.getUnitX = function() return new(Vector4, 1.0, 0.0, 0.0, 0.0) end
Vector4.getUnitY = function() return new(Vector4, 0.0, 1.0, 0.0, 0.0) end
Vector4.getUnitZ = function() return new(Vector4, 0.0, 0.0, 1.0, 0.0) end
Vector4.getUnitW = function() return new(Vector4, 0.0, 0.0, 0.0, 1.0) end

Vector4.CopyTo = function(this, array, index)
    if index == nil then
        index = 0
    end

    if array == nil then
        System.throw(System.NullReferenceException())
    end

    if index < 0 or index >= #array then
        System.throw(System.ArgumentOutOfRangeException())
    end
    if (#array - index) < 4 then
        System.throw(System.ArgumentException())
    end

    array:set(index, this.X)
    array:set(index + 1, this.Y)
    array:set(index + 2, this.Z)
    array:set(index + 3, this.W)
end

Vector4.Equals = function(this, other)
    if not (System.is(other, Vector4)) then
        return false
    end
    other = System.cast(Vector4, other)
    return this.X == other.X and this.Y == other.Y and this.Z == other.Z and this.W == other.W
end

Vector4.Dot = function(vector1, vector2)
    return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z + vector1.W * vector2.W
end

Vector4.Min = function(value1, value2)
    return new(Vector4, (value1.X < value2.X) and value1.X or value2.X, (value1.Y < value2.Y) and value1.Y or value2.Y, (value1.Z < value2.Z) and value1.Z or value2.Z, (value1.W < value2.W) and value1.W or value2.W)
end

Vector4.Max = function(value1, value2)
    return new(Vector4, (value1.X > value2.X) and value1.X or value2.X, (value1.Y > value2.Y) and value1.Y or value2.Y, (value1.Z > value2.Z) and value1.Z or value2.Z, (value1.W > value2.W) and value1.W or value2.W)
end

Vector4.Abs = function(value)
    return new(Vector4, abs(value.X), abs(value.Y), abs(value.Z), abs(value.W))
end

Vector4.SquareRoot = function(value)
    return new(Vector4, System.ToSingle(sqrt(value.X)), System.ToSingle(sqrt(value.Y)), System.ToSingle(sqrt(value.Z)), System.ToSingle(sqrt(value.W)))
end

Vector4.op_Addition = function(left, right)
    return new(Vector4, left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W)
end

Vector4.Add = function(left, right)
    return Vector4.op_Addition(left, right)
end

Vector4.op_Subtraction = function(left, right)
    return new(Vector4, left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W)
end

Vector4.Subtract = function(left, right)
    return Vector4.op_Subtraction(left, right)
end

Vector4.op_Multiply = function(left, right)
    if type(left) == "number" then
        left = new(Vector4, left)
    end

    if type(right) == "number" then
        right = new(Vector4, right)
    end
    return new(Vector4, left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W)
end

Vector4.Multiply = function(left, right)
    return Vector4.op_Multiply(left, right)
end

Vector4.op_Division = function(left, right)
    if type(right) == "number" then
        return Vector4.op_Multiply(left, 1.0 / right)
    end
    return new(Vector4, left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W)
end

Vector4.Divide = function(left, right)
    return Vector4.op_Division(left, right)
end

Vector4.op_UnaryNegation = function(value)
    return Vector4.op_Subtraction(Vector4.getZero(), value)
end

Vector4.Negate = function(value)
    return - value
end

Vector4.op_Equality = function(left, right)
    return left.X == right.X and left.Y == right.Y and left.Z == right.Z and left.W == right.W
end

Vector4.op_Inequality = function(left, right)
    return not (Vector4.op_Equality(left, right))
end

Vector4.GetHashCode = function(this)
    local hash = this.X:GetHashCode()
    hash = SystemNumerics.HashCodeHelper.CombineHashCodes(hash, this.Y:GetHashCode())
    hash = SystemNumerics.HashCodeHelper.CombineHashCodes(hash, this.Z:GetHashCode())
    hash = SystemNumerics.HashCodeHelper.CombineHashCodes(hash, this.W:GetHashCode())
      
    return hash
end

Vector4.ToString = function(this)
    local sb = System.StringBuilder()
    local separator = 44 --[[',']]
    sb:AppendChar(60 --[['<']])
    sb:Append((this.X):ToString())
    sb:AppendChar(separator)
    sb:AppendChar(32 --[[' ']])
    sb:Append((this.Y):ToString())
    sb:AppendChar(separator)
    sb:AppendChar(32 --[[' ']])
    sb:Append((this.Z):ToString())
    sb:AppendChar(separator)
    sb:AppendChar(32 --[[' ']])
    sb:Append((this.W):ToString())
    sb:AppendChar(62 --[['>']])
    return sb:ToString()
end

Vector4.Length = function(this)
    local ls = this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W
    return System.ToSingle(sqrt(ls))
end

Vector4.LengthSquared = function(this)
    return this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W
end

Vector4.Distance = function(value1, value2)
    local dx = value1.X - value2.X
    local dy = value1.Y - value2.Y
    local dz = value1.Z - value2.Z
    local dw = value1.W - value2.W

    local ls = dx * dx + dy * dy + dz * dz + dw * dw

    return System.ToSingle(sqrt(ls))
end

Vector4.DistanceSquared = function(value1, value2)
    local dx = value1.X - value2.X
    local dy = value1.Y - value2.Y
    local dz = value1.Z - value2.Z
    local dw = value1.W - value2.W

    return dx * dx + dy * dy + dz * dz + dw * dw
end

Vector4.Normalize = function(vector)
    local ls = vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z + vector.W * vector.W
    local invNorm = 1.0 / System.ToSingle(sqrt(ls))

    return new(Vector4, vector.X * invNorm, vector.Y * invNorm, vector.Z * invNorm, vector.W * invNorm)
end

Vector4.Clamp = function(value1, min, max)
    local x = value1.X
    x = (x > max.X) and max.X or x
    x = (x < min.X) and min.X or x

    local y = value1.Y
    y = (y > max.Y) and max.Y or y
    y = (y < min.Y) and min.Y or y

    local z = value1.Z
    z = (z > max.Z) and max.Z or z
    z = (z < min.Z) and min.Z or z

    local w = value1.W
    w = (w > max.W) and max.W or w
    w = (w < min.W) and min.W or w

    return new(Vector4, x, y, z, w)
end

Vector4.Lerp = function(value1, value2, amount)
    return new(Vector4, value1.X + (value2.X - value1.X) * amount, value1.Y + (value2.Y - value1.Y) * amount, value1.Z + (value2.Z - value1.Z) * amount, value1.W + (value2.W - value1.W) * amount)
end

Vector4.Transform = function(position, matrix)
    if matrix.X == nil then
        -- 4x4 matrix
        if matrix.W == nil then
            if matrix.Z == nil then
                -- vector2
                return new(Vector4, position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41, 
                                    position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42,
                                    position.X * matrix.M13 + position.Y * matrix.M23 + matrix.M43, 
                                    position.X * matrix.M14 + position.Y * matrix.M24 + matrix.M44
                                )
            else
                -- vector3
                return new(Vector4, position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41, 
                                    position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42, 
                                    position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43, 
                                    position.X * matrix.M14 + position.Y * matrix.M24 + position.Z * matrix.M34 + matrix.M44
                                )
            end
        else
            -- vector4
            return new(Vector4, position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + position.W * matrix.M41, 
                                position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + position.W * matrix.M42,
                                position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + position.W * matrix.M43, 
                                position.X * matrix.M14 + position.Y * matrix.M24 + position.Z * matrix.M34 + position.W * matrix.M44
                            )
        end
    else
        -- quaternion
        if matrix.W == nil then
            if matrix.Z == nil then
                -- vector2
                local x2 = matrix.X + matrix.X
                local y2 = matrix.Y + matrix.Y
                local z2 = matrix.Z + matrix.Z

                local wx2 = matrix.W * x2
                local wy2 = matrix.W * y2
                local wz2 = matrix.W * z2
                local xx2 = matrix.X * x2
                local xy2 = matrix.X * y2
                local xz2 = matrix.X * z2
                local yy2 = matrix.Y * y2
                local yz2 = matrix.Y * z2
                local zz2 = matrix.Z * z2

                return new(Vector4, position.X * (1.0 - yy2 - zz2) + position.Y * (xy2 - wz2), 
                                    position.X * (xy2 + wz2) + position.Y * (1.0 - xx2 - zz2), 
                                    position.X * (xz2 - wy2) + position.Y * (yz2 + wx2), 
                                    1.0
                                )
            else
                -- vector3
                local x2 = matrix.X + matrix.X
                local y2 = matrix.Y + matrix.Y
                local z2 = matrix.Z + matrix.Z

                local wx2 = matrix.W * x2
                local wy2 = matrix.W * y2
                local wz2 = matrix.W * z2
                local xx2 = matrix.X * x2
                local xy2 = matrix.X * y2
                local xz2 = matrix.X * z2
                local yy2 = matrix.Y * y2
                local yz2 = matrix.Y * z2
                local zz2 = matrix.Z * z2

                return new(Vector4, position.X * (1.0 - yy2 - zz2) + position.Y * (xy2 - wz2) + position.Z * (xz2 + wy2), 
                                    position.X * (xy2 + wz2) + position.Y * (1.0 - xx2 - zz2) + position.Z * (yz2 - wx2), 
                                    position.X * (xz2 - wy2) + position.Y * (yz2 + wx2) + position.Z * (1.0 - xx2 - yy2), 
                                    1.0
                                )
            end
        else
            -- vector4
            local x2 = matrix.X + matrix.X
            local y2 = matrix.Y + matrix.Y
            local z2 = matrix.Z + matrix.Z

            local wx2 = matrix.W * x2
            local wy2 = matrix.W * y2
            local wz2 = matrix.W * z2
            local xx2 = matrix.X * x2
            local xy2 = matrix.X * y2
            local xz2 = matrix.X * z2
            local yy2 = matrix.Y * y2
            local yz2 = matrix.Y * z2
            local zz2 = matrix.Z * z2

            return new(Vector4, position.X * (1.0 - yy2 - zz2) + position.Y * (xy2 - wz2) + position.Z * (xz2 + wy2), 
                                position.X * (xy2 + wz2) + position.Y * (1.0 - xx2 - zz2) + position.Z * (yz2 - wx2), 
                                position.X * (xz2 - wy2) + position.Y * (yz2 + wx2) + position.Z * (1.0 - xx2 - yy2), 
                                position.W
                            )
        end
    end
end

System.defStc("System.Numerics.Vector4", Vector4)
