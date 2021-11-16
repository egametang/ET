local System = System
local SystemNumerics = System.Numerics

local sqrt = math.sqrt
local sin = math.sin
local cos = math.cos
local acos = math.acos

local IComparable = System.IComparable
local IComparable_1 = System.IComparable_1
local IEquatable_1 = System.IEquatable_1

local new = function (cls, ...)
    local this = setmetatable({}, cls)
    return this, cls.__ctor__(this, ...)
end

local Quaternion = {}

Quaternion.__ctor__ = function(this, x, y, z, w)
    if x == nil then
        this.X = 0
        this.Y = 0
        this.Z = 0
        this.W = 0
    elseif z == nil then
        -- Quaternion(Vector3, Single)
        this.X = x.X or 0
        this.Y = x.Y or 0
        this.Z = x.Z or 0
        this.W = z or 0
    else
        -- Quaternion(Single, Single, Single, Single)
        this.X = x or 0
        this.Y = y or 0
        this.Z = z or 0
        this.W = w or 0
    end
    local mt = getmetatable(this)
    mt.__unm = Quaternion.op_UnaryNegation
    setmetatable(this, mt)
end

Quaternion.base = function (_, T)
    return { IComparable, IComparable_1(T), IEquatable_1(T) }
end

Quaternion.getIdentity = function ()
    return new(Quaternion, 0, 0, 0, 1)
end

Quaternion.getIsIdentity = function (this)
    return this.X == 0 and this.Y == 0 and this.Z == 0 and this.W == 1
end

Quaternion.Length = function (this)
    local ls = this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W

    return System.ToSingle(sqrt(ls))
end

Quaternion.LengthSquared = function (this)
    return this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W
end

Quaternion.Normalize = function (value)
    local ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z + value.W * value.W
    local invNorm = 1.0 / System.ToSingle(sqrt(ls))

    return new(Quaternion, value.X * invNorm, value.Y * invNorm, value.Z * invNorm, value.W * invNorm)
end

Quaternion.Conjugate = function (value)
    return new(Quaternion, - value.X, - value.Y, - value.Z, value.W)
end

Quaternion.Inverse = function (value)
    --  -1   (       a              -v       )
    -- q   = ( -------------   ------------- )
    --       (  a^2 + |v|^2  ,  a^2 + |v|^2  )

    local ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z + value.W * value.W
    local invNorm = 1.0 / ls

    return new(Quaternion, - value.X * invNorm, - value.Y * invNorm, - value.Z * invNorm, value.W * invNorm)
end

Quaternion.CreateFromAxisAngle = function (axis, angle)
    local halfAngle = angle * 0.5
    local s = System.ToSingle(sin(halfAngle))
    local c = System.ToSingle(cos(halfAngle))

    return new(Quaternion, axis.X * s, axis.Y * s, axis.Z * s, c)
end

Quaternion.CreateFromYawPitchRoll = function (yaw, pitch, roll)
    --  Roll first, about axis the object is facing, then
    --  pitch upward, then yaw to face into the new heading
    local sr, cr, sp, cp, sy, cy

    local halfRoll = roll * 0.5
    sr = System.ToSingle(sin(halfRoll))
    cr = System.ToSingle(cos(halfRoll))

    local halfPitch = pitch * 0.5
    sp = System.ToSingle(sin(halfPitch))
    cp = System.ToSingle(cos(halfPitch))

    local halfYaw = yaw * 0.5
    sy = System.ToSingle(sin(halfYaw))
    cy = System.ToSingle(cos(halfYaw))

    return new(Quaternion, cy * sp * cr + sy * cp * sr, sy * cp * cr - cy * sp * sr, cy * cp * sr - sy * sp * cr, cy * cp * cr + sy * sp * sr)
end

Quaternion.CreateFromRotationMatrix = function (matrix)
    local trace = matrix.M11 + matrix.M22 + matrix.M33

    local q = new(Quaternion)

    if trace > 0.0 then
      local s = System.ToSingle(sqrt(trace + 1.0))
      q.W = s * 0.5
      s = 0.5 / s
      q.X = (matrix.M23 - matrix.M32) * s
      q.Y = (matrix.M31 - matrix.M13) * s
      q.Z = (matrix.M12 - matrix.M21) * s
    else
      if matrix.M11 >= matrix.M22 and matrix.M11 >= matrix.M33 then
        local s = System.ToSingle(sqrt(1.0 + matrix.M11 - matrix.M22 - matrix.M33))
        local invS = 0.5 / s
        q.X = 0.5 * s
        q.Y = (matrix.M12 + matrix.M21) * invS
        q.Z = (matrix.M13 + matrix.M31) * invS
        q.W = (matrix.M23 - matrix.M32) * invS
      elseif matrix.M22 > matrix.M33 then
        local s = System.ToSingle(sqrt(1.0 + matrix.M22 - matrix.M11 - matrix.M33))
        local invS = 0.5 / s
        q.X = (matrix.M21 + matrix.M12) * invS
        q.Y = 0.5 * s
        q.Z = (matrix.M32 + matrix.M23) * invS
        q.W = (matrix.M31 - matrix.M13) * invS
      else
        local s = System.ToSingle(sqrt(1.0 + matrix.M33 - matrix.M11 - matrix.M22))
        local invS = 0.5 / s
        q.X = (matrix.M31 + matrix.M13) * invS
        q.Y = (matrix.M32 + matrix.M23) * invS
        q.Z = 0.5 * s
        q.W = (matrix.M12 - matrix.M21) * invS
      end
    end

    return q:__clone__()
end

Quaternion.Dot = function (quaternion1, quaternion2)
    return quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W
end

Quaternion.Slerp = function (quaternion1, quaternion2, amount)
    local t = amount

    local cosOmega = quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W

    local flip = false

    if cosOmega < 0.0 then
      flip = true
      cosOmega = - cosOmega
    end

    local s1, s2

    if cosOmega > (0.999999 --[[1.0f - epsilon]]) then
      -- Too close, do straight linear interpolation.
      s1 = 1.0 - t
      s2 = (flip) and - t or t
    else
      local omega = System.ToSingle(acos(cosOmega))
      local invSinOmega = System.ToSingle((1 / sin(omega)))

      s1 = System.ToSingle(sin((1.0 - t) * omega)) * invSinOmega
      s2 = (flip) and System.ToSingle(- sin(t * omega)) * invSinOmega or System.ToSingle(sin(t * omega)) * invSinOmega
    end

    return new(Quaternion, s1 * quaternion1.X + s2 * quaternion2.X, s1 * quaternion1.Y + s2 * quaternion2.Y, s1 * quaternion1.Z + s2 * quaternion2.Z, s1 * quaternion1.W + s2 * quaternion2.W)
end

Quaternion.Lerp = function (quaternion1, quaternion2, amount)
    local t = amount
    local t1 = 1.0 - t

    local r = new(Quaternion)

    local dot = quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W

    if dot >= 0.0 then
      r.X = t1 * quaternion1.X + t * quaternion2.X
      r.Y = t1 * quaternion1.Y + t * quaternion2.Y
      r.Z = t1 * quaternion1.Z + t * quaternion2.Z
      r.W = t1 * quaternion1.W + t * quaternion2.W
    else
      r.X = t1 * quaternion1.X - t * quaternion2.X
      r.Y = t1 * quaternion1.Y - t * quaternion2.Y
      r.Z = t1 * quaternion1.Z - t * quaternion2.Z
      r.W = t1 * quaternion1.W - t * quaternion2.W
    end

    -- Normalize it.
    local ls = r.X * r.X + r.Y * r.Y + r.Z * r.Z + r.W * r.W
    local invNorm = 1.0 / System.ToSingle(sqrt(ls))

    r.X = r.X * invNorm
    r.Y = r.Y * invNorm
    r.Z = r.Z * invNorm
    r.W = r.W * invNorm

    return r:__clone__()
end

Quaternion.Concatenate = function (value1, value2)

    -- Concatenate rotation is actually q2 * q1 instead of q1 * q2.
    -- So that's why value2 goes q1 and value1 goes q2.
    local q1x = value2.X
    local q1y = value2.Y
    local q1z = value2.Z
    local q1w = value2.W

    local q2x = value1.X
    local q2y = value1.Y
    local q2z = value1.Z
    local q2w = value1.W

    -- cross(av, bv)
    local cx = q1y * q2z - q1z * q2y
    local cy = q1z * q2x - q1x * q2z
    local cz = q1x * q2y - q1y * q2x

    local dot = q1x * q2x + q1y * q2y + q1z * q2z

    return new(Quaternion, q1x * q2w + q2x * q1w + cx, q1y * q2w + q2y * q1w + cy, q1z * q2w + q2z * q1w + cz, q1w * q2w - dot)
end

Quaternion.Negate = function (value)
    return new(Quaternion, - value.X, - value.Y, - value.Z, - value.W)
end

Quaternion.Add = function (value1, value2)
    return new(Quaternion, value1.X + value2.X, value1.Y + value2.Y, value1.Z + value2.Z, value1.W + value2.W)
end

Quaternion.Subtract = function (value1, value2)
    return new(Quaternion, value1.X - value2.X, value1.Y - value2.Y, value1.Z - value2.Z, value1.W - value2.W)
end

Quaternion.Multiply = function (value1, value2)
    if value2.X == nil then
        -- scalar
        return new(Quaternion, value1.X * value2, value1.Y * value2, value1.Z * value2, value1.W * value2)
    else
        -- quaternion
        local q1x = value1.X
        local q1y = value1.Y
        local q1z = value1.Z
        local q1w = value1.W

        local q2x = value2.X
        local q2y = value2.Y
        local q2z = value2.Z
        local q2w = value2.W

        -- cross(av, bv)
        local cx = q1y * q2z - q1z * q2y
        local cy = q1z * q2x - q1x * q2z
        local cz = q1x * q2y - q1y * q2x

        local dot = q1x * q2x + q1y * q2y + q1z * q2z

        return new(Quaternion, q1x * q2w + q2x * q1w + cx, q1y * q2w + q2y * q1w + cy, q1z * q2w + q2z * q1w + cz, q1w * q2w - dot)
    end    
end

Quaternion.Divide = function (value1, value2)

    local q1x = value1.X
    local q1y = value1.Y
    local q1z = value1.Z
    local q1w = value1.W

    ---------------------------------------
    -- Inverse part.
    local ls = value2.X * value2.X + value2.Y * value2.Y + value2.Z * value2.Z + value2.W * value2.W
    local invNorm = 1.0 / ls

    local q2x = - value2.X * invNorm
    local q2y = - value2.Y * invNorm
    local q2z = - value2.Z * invNorm
    local q2w = value2.W * invNorm

    ---------------------------------------
    -- Multiply part.

    -- cross(av, bv)
    local cx = q1y * q2z - q1z * q2y
    local cy = q1z * q2x - q1x * q2z
    local cz = q1x * q2y - q1y * q2x

    local dot = q1x * q2x + q1y * q2y + q1z * q2z

    return new(Quaternion, q1x * q2w + q2x * q1w + cx, q1y * q2w + q2y * q1w + cy, q1z * q2w + q2z * q1w + cz, q1w * q2w - dot)
end

Quaternion.op_UnaryNegation = function (value)
    return new(Quaternion, - value.X, - value.Y, - value.Z, - value.W)
end

Quaternion.op_Addition = function (value1, value2)
    return new(Quaternion, value1.X + value2.X, value1.Y + value2.Y, value1.Z + value2.Z, value1.W + value2.W)
end

Quaternion.op_Subtraction = function (value1, value2)
    return new(Quaternion, value1.X - value2.X, value1.Y - value2.Y, value1.Z - value2.Z, value1.W - value2.W)
end

Quaternion.op_Multiply = function (value1, value2)
    if value2.X == nil then
        -- scalar
        return new(Quaternion, value1.X * value2, value1.Y * value2, value1.Z * value2, value1.W * value2)
    else
        -- quaternion
        local q1x = value1.X
        local q1y = value1.Y
        local q1z = value1.Z
        local q1w = value1.W

        local q2x = value2.X
        local q2y = value2.Y
        local q2z = value2.Z
        local q2w = value2.W

        -- cross(av, bv)
        local cx = q1y * q2z - q1z * q2y
        local cy = q1z * q2x - q1x * q2z
        local cz = q1x * q2y - q1y * q2x

        local dot = q1x * q2x + q1y * q2y + q1z * q2z

        return new(Quaternion, q1x * q2w + q2x * q1w + cx, q1y * q2w + q2y * q1w + cy, q1z * q2w + q2z * q1w + cz, q1w * q2w - dot)
    end    
end

Quaternion.op_Division = function (value1, value2)

    local q1x = value1.X
    local q1y = value1.Y
    local q1z = value1.Z
    local q1w = value1.W

    ---------------------------------------
    -- Inverse part.
    local ls = value2.X * value2.X + value2.Y * value2.Y + value2.Z * value2.Z + value2.W * value2.W
    local invNorm = 1.0 / ls

    local q2x = - value2.X * invNorm
    local q2y = - value2.Y * invNorm
    local q2z = - value2.Z * invNorm
    local q2w = value2.W * invNorm

    ---------------------------------------
    -- Multiply part.

    -- cross(av, bv)
    local cx = q1y * q2z - q1z * q2y
    local cy = q1z * q2x - q1x * q2z
    local cz = q1x * q2y - q1y * q2x

    local dot = q1x * q2x + q1y * q2y + q1z * q2z

    return new(Quaternion, q1x * q2w + q2x * q1w + cx, q1y * q2w + q2y * q1w + cy, q1z * q2w + q2z * q1w + cz, q1w * q2w - dot)
end

Quaternion.op_Equality = function (value1, value2)
    return (value1.X == value2.X and value1.Y == value2.Y and value1.Z == value2.Z and value1.W == value2.W)
end

Quaternion.op_Inequality = function (value1, value2)
    return (value1.X ~= value2.X or value1.Y ~= value2.Y or value1.Z ~= value2.Z or value1.W ~= value2.W)
end

Quaternion.Equals = function (this, obj)
    if System.is(obj, Quaternion) then
        return (this.X == obj.X and this.Y == obj.Y and this.Z == obj.Z and this.W == obj.W)
    end
    return false
end

Quaternion.ToString = function (this)
    local sb = System.StringBuilder()
    sb:Append("{")
    sb:Append("X: ")
    sb:Append(this.X:ToString())
    sb:Append(" Y: ")
    sb:Append(this.Y:ToString())
    sb:Append(" Z: ")
    sb:Append(this.Z:ToString())
    sb:Append(" W: ")
    sb:Append(this.W:ToString())
    sb:Append("}")
    return sb:ToString()
end

Quaternion.GetHashCode = function (this)
    return this.X:GetHashCode() + this.Y:GetHashCode() + this.Z:GetHashCode() + this.W:GetHashCode()
end

System.defStc("System.Numerics.Quaternion", Quaternion)