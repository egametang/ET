local System = System
local SystemNumerics = System.Numerics

local sqrt = math.sqrt
local abs = math.abs

local IComparable = System.IComparable
local IComparable_1 = System.IComparable_1
local IEquatable_1 = System.IEquatable_1

local new = function (cls, ...)
    local this = setmetatable({}, cls)
    return this, cls.__ctor__(this, ...)
end

local Plane = {}

Plane.__ctor__ = function(this, val1, val2, val3, val4)
    if val4 == nil then
        if val2 == nil then
            -- Plane(Vector4)
            this.Normal = SystemNumerics.Vector3(val1.X, val1.Y, val1.Z)
            this.D = val1.W
        else
            -- Plane(Vector3, Single)
            this.Normal = val1:__clone__()
            this.D = val2
        end
    else
        -- Plane(Single, Single, Single, Single)
        this.Normal = SystemNumerics.Vector3(val1, val2, val3)
        this.D = val4
    end
end

Plane.base = function (_, T)
    return { IComparable, IComparable_1(T), IEquatable_1(T) }
end

Plane.CreateFromVertices = function (point1, point2, point3)
    local ax = point2.X - point1.X
    local ay = point2.Y - point1.Y
    local az = point2.Z - point1.Z

    local bx = point3.X - point1.X
    local by = point3.Y - point1.Y
    local bz = point3.Z - point1.Z

    -- N=Cross(a,b)
    local nx = ay * bz - az * by
    local ny = az * bx - ax * bz
    local nz = ax * by - ay * bx

    -- Normalize(N)
    local ls = nx * nx + ny * ny + nz * nz
    local invNorm = 1.0 / System.ToSingle(sqrt(ls))

    local normal = SystemNumerics.Vector3(nx * invNorm, ny * invNorm, nz * invNorm)

    return new(Plane, normal:__clone__(), - (normal.X * point1.X + normal.Y * point1.Y + normal.Z * point1.Z))
end

Plane.Normalize = function (value)
    -- smallest such that 1.0+FLT_EPSILON != 1.0
    local f = value.Normal.X * value.Normal.X + value.Normal.Y * value.Normal.Y + value.Normal.Z * value.Normal.Z

    if abs(f - 1.0) < 1.192093E-07 --[[FLT_EPSILON]] then
        return value:__clone__()
        -- It already normalized, so we don't need to further process.
    end

    local fInv = 1.0 / System.ToSingle(sqrt(f))

    return new(Plane, value.Normal.X * fInv, value.Normal.Y * fInv, value.Normal.Z * fInv, value.D * fInv)
  end

Plane.Transform = function (plane, matrix)
    if matrix.X == nil then
        -- matrix
        local m
        local default
        default, m = SystemNumerics.Matrix4x4.Invert(matrix)

        local x = plane.Normal.X local y = plane.Normal.Y local z = plane.Normal.Z local w = plane.D

        return new(Plane, x * m.M11 + y * m.M12 + z * m.M13 + w * m.M14, x * m.M21 + y * m.M22 + z * m.M23 + w * m.M24, x * m.M31 + y * m.M32 + z * m.M33 + w * m.M34, x * m.M41 + y * m.M42 + z * m.M43 + w * m.M44)
    else
        -- quaternion
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
  
        local m11 = 1.0 - yy2 - zz2
        local m21 = xy2 - wz2
        local m31 = xz2 + wy2
  
        local m12 = xy2 + wz2
        local m22 = 1.0 - xx2 - zz2
        local m32 = yz2 - wx2
  
        local m13 = xz2 - wy2
        local m23 = yz2 + wx2
        local m33 = 1.0 - xx2 - yy2
  
        local x = plane.Normal.X local y = plane.Normal.Y local z = plane.Normal.Z
  
        return new(Plane, x * m11 + y * m21 + z * m31, x * m12 + y * m22 + z * m32, x * m13 + y * m23 + z * m33, plane.D)  
    end
end

Plane.Dot = function (plane, value)
    return plane.Normal.X * value.X + plane.Normal.Y * value.Y + plane.Normal.Z * value.Z + plane.D * value.W
end

Plane.DotCoordinate = function (plane, value)
    return plane.Normal.X * value.X + plane.Normal.Y * value.Y + plane.Normal.Z * value.Z + plane.D
end

Plane.DotNormal = function (plane, value)
    return plane.Normal.X * value.X + plane.Normal.Y * value.Y + plane.Normal.Z * value.Z
end

Plane.op_Equality = function (value1, value2)
    return (value1.Normal.X == value2.Normal.X and value1.Normal.Y == value2.Normal.Y and value1.Normal.Z == value2.Normal.Z and value1.D == value2.D)
end

Plane.op_Inequality = function (value1, value2)
    return (value1.Normal.X ~= value2.Normal.X or value1.Normal.Y ~= value2.Normal.Y or value1.Normal.Z ~= value2.Normal.Z or value1.D ~= value2.D)
end

Plane.Equals = function (this, obj)
    if System.is(obj, Plane) then
        return (this.Normal.X == obj.Normal.X and this.Normal.Y == obj.Normal.Y and this.Normal.Z == obj.Normal.Z and this.D == obj.D)
    end
    return false
end

Plane.ToString = function (this)
    local sb = System.StringBuilder()
    sb:Append("{")
    sb:Append("Normal: ")
    sb:Append(this.Normal:ToString())
    sb:Append(" D: ")
    sb:Append(this.D:ToString())
    sb:Append("}")
    return sb:ToString()
end

Plane.GetHashCode = function (this)
    return this.Normal:GetHashCode() + this.D:GetHashCode()
end

System.defStc("System.Numerics.Plane", Plane)