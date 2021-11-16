local System = System
local SystemNumerics = System.Numerics

local tan = math.tan
local cos = math.cos
local sin = math.sin
local abs = math.abs

local IComparable = System.IComparable
local IComparable_1 = System.IComparable_1
local IEquatable_1 = System.IEquatable_1

local new = function (cls, ...)
    local this = setmetatable({}, cls)
    return this, cls.__ctor__(this, ...)
end

local Matrix3x2 = {}

Matrix3x2.__ctor__ = function(this, m11, m12, m21, m22, m31, m32)
    this.M11 = m11 or 0
    this.M12 = m12 or 0
    this.M21 = m21 or 0
    this.M22 = m22 or 0
    this.M31 = m31 or 0
    this.M32 = m32 or 0
    local mt = getmetatable(this)
    mt.__unm = Matrix3x2.op_UnaryNegation
    setmetatable(this, mt)
end

Matrix3x2.base = function (_, T)
    return { IComparable, IComparable_1(T), IEquatable_1(T) }
end

Matrix3x2.getIdentity = function ()
    return new(Matrix3x2, 1, 0, 0, 1, 0, 0)
end

Matrix3x2.getIsIdentity = function (this)
    return this.M11 == 1 and this.M22 == 1 and this.M12 == 0 and this.M21 == 0 and this.M31 == 0 and this.M32 == 0
end

Matrix3x2.getTranslation = function (this)
    return SystemNumerics.Vector2(this.M31, this.M32)
end

Matrix3x2.setTranslation = function (this, value)
    this.M31 = value.X
    this.M32 = value.Y
end

Matrix3x2.CreateTranslation = function (position, yPosition)
    if yPosition == nil then
        -- Vector2
        local result = new(Matrix3x2)

        result.M11 = 1.0
        result.M12 = 0.0
        result.M21 = 0.0
        result.M22 = 1.0

        result.M31 = position.X
        result.M32 = position.Y

        return result:__clone__()
    else
        -- singles
        local result = new(Matrix3x2)

        result.M11 = 1.0
        result.M12 = 0.0
        result.M21 = 0.0
        result.M22 = 1.0
  
        result.M31 = position
        result.M32 = yPosition
  
        return result:__clone__()
    end
end

Matrix3x2.CreateScale = function(val1, val2, val3)
    if val3 == nil then
        if val2 == nil then
            if val1.X == nil then
                -- CreateScale(Single)
                local result = new(Matrix3x2)

                result.M11 = val1
                result.M12 = 0.0
                result.M21 = 0.0
                result.M22 = val1
                result.M31 = 0.0
                result.M32 = 0.0
          
                return result:__clone__()
            else
                -- CreateScale(Vector2)
                local result = new(Matrix3x2)

                result.M11 = val1.X
                result.M12 = 0.0
                result.M21 = 0.0
                result.M22 = val1.Y
                result.M31 = 0.0
                result.M32 = 0.0

                return result:__clone__()
            end
        else
            if val2.X == nil then
                -- CreateScale(Single, Single)
                local result = new(Matrix3x2)

                result.M11 = val1
                result.M12 = 0.0
                result.M21 = 0.0
                result.M22 = val2
                result.M31 = 0.0
                result.M32 = 0.0

                return result:__clone__()
            else
                if val1.X == nil then
                    -- CreateScale(Single, Vector2)
                    local result = new(Matrix3x2)

                    local tx = val2.X * (1 - val1)
                    local ty = val2.Y * (1 - val1)

                    result.M11 = val1
                    result.M12 = 0.0
                    result.M21 = 0.0
                    result.M22 = val1
                    result.M31 = tx
                    result.M32 = ty

                    return result:__clone__()
                else
                    -- CreateScale(Vector2, Vector2)
                    local result = new(Matrix3x2)

                    local tx = val2.X * (1 - val1.X)
                    local ty = val2.Y * (1 - val1.Y)

                    result.M11 = val1.X
                    result.M12 = 0.0
                    result.M21 = 0.0
                    result.M22 = val1.Y
                    result.M31 = tx
                    result.M32 = ty

                    return result:__clone__()
                end
            end
        end
    else
        -- CreateScale(Single, Single, Vector2)
        local result = new(Matrix3x2)

        local tx = val3.X * (1 - val1)
        local ty = val3.Y * (1 - val2)

        result.M11 = val1
        result.M12 = 0.0
        result.M21 = 0.0
        result.M22 = val2
        result.M31 = tx
        result.M32 = ty

        return result:__clone__()
    end
end

Matrix3x2.CreateSkew = function (radiansX, radiansY, centerPoint)
    if centerPoint == nil then

        local result = new(Matrix3x2)

        local xTan = System.ToSingle(tan(radiansX))
        local yTan = System.ToSingle(tan(radiansY))

        result.M11 = 1.0
        result.M12 = yTan
        result.M21 = xTan
        result.M22 = 1.0
        result.M31 = 0.0
        result.M32 = 0.0

        return result:__clone__()
    else
        local result = new(Matrix3x2)

        local xTan = System.ToSingle(tan(radiansX))
        local yTan = System.ToSingle(tan(radiansY))

        local tx = - centerPoint.Y * xTan
        local ty = - centerPoint.X * yTan

        result.M11 = 1.0
        result.M12 = yTan
        result.M21 = xTan
        result.M22 = 1.0
        result.M31 = tx
        result.M32 = ty

        return result:__clone__()
    end
end

Matrix3x2.CreateRotation = function (radians, centerPoint)
    if centerPoint == nil then

        local result = new(Matrix3x2)

        radians = System.ToSingle(math.IEEERemainder(radians, 6.28318530717959 --[[Math.PI * 2]]))

        local c, s

        -- 0.1% of a degree

        if radians > - 1.745329E-05 --[[epsilon]] and radians < 1.745329E-05 --[[epsilon]] then
            -- Exact case for zero rotation.
            c = 1
            s = 0
        elseif radians > 1.57077887350062 --[[Math.PI / 2 - epsilon]] and radians < 1.57081378008917 --[[Math.PI / 2 + epsilon]] then
            -- Exact case for 90 degree rotation.
            c = 0
            s = 1
        elseif radians < -3.14157520029552 --[[-Math.PI + epsilon]] or radians > 3.14157520029552 --[[Math.PI - epsilon]] then
            -- Exact case for 180 degree rotation.
            c = - 1
            s = 0
        elseif radians > -1.57081378008917 --[[-Math.PI / 2 - epsilon]] and radians < -1.57077887350062 --[[-Math.PI / 2 + epsilon]] then
            -- Exact case for 270 degree rotation.
            c = 0
            s = - 1
        else
            -- Arbitrary rotation.
            c = System.ToSingle(cos(radians))
            s = System.ToSingle(sin(radians))
        end

        -- [  c  s ]
        -- [ -s  c ]
        -- [  0  0 ]
        result.M11 = c
        result.M12 = s
        result.M21 = - s
        result.M22 = c
        result.M31 = 0.0
        result.M32 = 0.0

        return result:__clone__()
    else
        local result = new(Matrix3x2)

        radians = System.ToSingle(math.IEEERemainder(radians, 6.28318530717959 --[[Math.PI * 2]]))
  
        local c, s
  
        -- 0.1% of a degree
  
        if radians > - 1.745329E-05 --[[epsilon]] and radians < 1.745329E-05 --[[epsilon]] then
          -- Exact case for zero rotation.
          c = 1
          s = 0
        elseif radians > 1.57077887350062 --[[Math.PI / 2 - epsilon]] and radians < 1.57081378008917 --[[Math.PI / 2 + epsilon]] then
          -- Exact case for 90 degree rotation.
          c = 0
          s = 1
        elseif radians < -3.14157520029552 --[[-Math.PI + epsilon]] or radians > 3.14157520029552 --[[Math.PI - epsilon]] then
          -- Exact case for 180 degree rotation.
          c = - 1
          s = 0
        elseif radians > -1.57081378008917 --[[-Math.PI / 2 - epsilon]] and radians < -1.57077887350062 --[[-Math.PI / 2 + epsilon]] then
          -- Exact case for 270 degree rotation.
          c = 0
          s = - 1
        else
          -- Arbitrary rotation.
          c = System.ToSingle(cos(radians))
          s = System.ToSingle(sin(radians))
        end
  
        local x = centerPoint.X * (1 - c) + centerPoint.Y * s
        local y = centerPoint.Y * (1 - c) - centerPoint.X * s
  
        -- [  c  s ]
        -- [ -s  c ]
        -- [  x  y ]
        result.M11 = c
        result.M12 = s
        result.M21 = - s
        result.M22 = c
        result.M31 = x
        result.M32 = y
  
        return result:__clone__()
    end
end

Matrix3x2.GetDeterminant = function (this)
    -- There isn't actually any such thing as a determinant for a non-square matrix,
    -- but this 3x2 type is really just an optimization of a 3x3 where we happen to
    -- know the rightmost column is always (0, 0, 1). So we expand to 3x3 format:
    --
    --  [ M11, M12, 0 ]
    --  [ M21, M22, 0 ]
    --  [ M31, M32, 1 ]
    --
    -- Sum the diagonal products:
    --  (M11 * M22 * 1) + (M12 * 0 * M31) + (0 * M21 * M32)
    --
    -- Subtract the opposite diagonal products:
    --  (M31 * M22 * 0) + (M32 * 0 * M11) + (1 * M21 * M12)
    --
    -- Collapse out the constants and oh look, this is just a 2x2 determinant!

    return (this.M11 * this.M22) - (this.M21 * this.M12)
end

Matrix3x2.Invert = function (matrix, result)
    local det = (matrix.M11 * matrix.M22) - (matrix.M21 * matrix.M12)

    if result == nil then
        result = new(Matrix3x2)
    end

    if abs(det) < 1.401298E-45 --[[Single.Epsilon]] then
      result = new(Matrix3x2, System.Single.NaN, System.Single.NaN, System.Single.NaN, System.Single.NaN, System.Single.NaN, System.Single.NaN)
      return false, result
    end

    local invDet = 1.0 / det

    result.M11 = matrix.M22 * invDet
    result.M12 = - matrix.M12 * invDet
    result.M21 = - matrix.M21 * invDet
    result.M22 = matrix.M11 * invDet
    result.M31 = (matrix.M21 * matrix.M32 - matrix.M31 * matrix.M22) * invDet
    result.M32 = (matrix.M31 * matrix.M12 - matrix.M11 * matrix.M32) * invDet

    return true, result
end

Matrix3x2.Lerp = function (matrix1, matrix2, amount)
    local result = new(Matrix3x2)

    -- First row
    result.M11 = matrix1.M11 + (matrix2.M11 - matrix1.M11) * amount
    result.M12 = matrix1.M12 + (matrix2.M12 - matrix1.M12) * amount

    -- Second row
    result.M21 = matrix1.M21 + (matrix2.M21 - matrix1.M21) * amount
    result.M22 = matrix1.M22 + (matrix2.M22 - matrix1.M22) * amount

    -- Third row
    result.M31 = matrix1.M31 + (matrix2.M31 - matrix1.M31) * amount
    result.M32 = matrix1.M32 + (matrix2.M32 - matrix1.M32) * amount

    return result:__clone__()
end

Matrix3x2.Negate = function (value)
    local result = new(Matrix3x2)

    result.M11 = - value.M11
    result.M12 = - value.M12
    result.M21 = - value.M21
    result.M22 = - value.M22
    result.M31 = - value.M31
    result.M32 = - value.M32

    return result:__clone__()
end

Matrix3x2.Add = function (value1, value2)
    local result = new(Matrix3x2)

    result.M11 = value1.M11 + value2.M11
    result.M12 = value1.M12 + value2.M12
    result.M21 = value1.M21 + value2.M21
    result.M22 = value1.M22 + value2.M22
    result.M31 = value1.M31 + value2.M31
    result.M32 = value1.M32 + value2.M32

    return result:__clone__()
end

Matrix3x2.Subtract = function (value1, value2)
    local result = new(Matrix3x2)

    result.M11 = value1.M11 - value2.M11
    result.M12 = value1.M12 - value2.M12
    result.M21 = value1.M21 - value2.M21
    result.M22 = value1.M22 - value2.M22
    result.M31 = value1.M31 - value2.M31
    result.M32 = value1.M32 - value2.M32

    return result:__clone__()
end

Matrix3x2.Multiply = function (value1, value2)
    if value2.M11 == nil then
        -- scalar
        local result = new(Matrix3x2)

        result.M11 = value1.M11 * value2
        result.M12 = value1.M12 * value2
        result.M21 = value1.M21 * value2
        result.M22 = value1.M22 * value2
        result.M31 = value1.M31 * value2
        result.M32 = value1.M32 * value2

        return result:__clone__()
    else
        -- matrix
        local result = new(Matrix3x2)

        -- First row
        result.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21
        result.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22

        -- Second row
        result.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21
        result.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22

        -- Third row
        result.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value2.M31
        result.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value2.M32

        return result:__clone__()
    end
end

Matrix3x2.op_UnaryNegation = function (value)
    local m = new(Matrix3x2)

    m.M11 = - value.M11
    m.M12 = - value.M12
    m.M21 = - value.M21
    m.M22 = - value.M22
    m.M31 = - value.M31
    m.M32 = - value.M32

    return m:__clone__()
end

Matrix3x2.op_Addition = function (value1, value2)
    local m = new(Matrix3x2)

    m.M11 = value1.M11 + value2.M11
    m.M12 = value1.M12 + value2.M12
    m.M21 = value1.M21 + value2.M21
    m.M22 = value1.M22 + value2.M22
    m.M31 = value1.M31 + value2.M31
    m.M32 = value1.M32 + value2.M32

    return m:__clone__()
end

Matrix3x2.op_Subtraction = function (value1, value2)
    local m = new(Matrix3x2)

    m.M11 = value1.M11 - value2.M11
    m.M12 = value1.M12 - value2.M12
    m.M21 = value1.M21 - value2.M21
    m.M22 = value1.M22 - value2.M22
    m.M31 = value1.M31 - value2.M31
    m.M32 = value1.M32 - value2.M32

    return m:__clone__()
end

Matrix3x2.op_Multiply = function (value1, value2)
    if value2.M11 == nil then
        -- scalar
        local result = new(Matrix3x2)

        result.M11 = value1.M11 * value2
        result.M12 = value1.M12 * value2
        result.M21 = value1.M21 * value2
        result.M22 = value1.M22 * value2
        result.M31 = value1.M31 * value2
        result.M32 = value1.M32 * value2

        return result:__clone__()
    else
        -- matrix
        local result = new(Matrix3x2)

        -- First row
        result.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21
        result.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22

        -- Second row
        result.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21
        result.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22

        -- Third row
        result.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value2.M31
        result.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value2.M32

        return result:__clone__()
    end
end

Matrix3x2.op_Equality = function (value1, value2)
    return (value1.M11 == value2.M11 and value1.M22 == value2.M22 and value1.M12 == value2.M12 and value1.M21 == value2.M21 and value1.M31 == value2.M31 and value1.M32 == value2.M32)
end

Matrix3x2.op_Inequality = function (value1, value2)
    return (value1.M11 ~= value2.M11 or value1.M12 ~= value2.M12 or value1.M21 ~= value2.M21 or value1.M22 ~= value2.M22 or value1.M31 ~= value2.M31 or value1.M32 ~= value2.M32)
end

Matrix3x2.Equals = function (this, other)
    if System.is(other, Matrix3x2) then
        return (this.M11 == other.M11 and this.M22 == other.M22 and this.M12 == other.M12 and this.M21 == other.M21 and this.M31 == other.M31 and this.M32 == other.M32)
    end
    return false
end

Matrix3x2.ToString = function (this)
    local sb = System.StringBuilder()
    sb:Append("{ ")
    sb:Append("{")
    sb:Append("M11: ")
    sb:Append(this.M11:ToString())
    sb:Append(" M12: ")
    sb:Append(this.M12:ToString())
    sb:Append("} ")
    sb:Append("{")
    sb:Append("M21: ")
    sb:Append(this.M21:ToString())
    sb:Append(" M22: ")
    sb:Append(this.M22:ToString())
    sb:Append("} ")
    sb:Append("{")
    sb:Append("M31: ")
    sb:Append(this.M31:ToString())
    sb:Append(" M32: ")
    sb:Append(this.M32:ToString())
    sb:Append("} ")
    sb:Append("}")
    return sb:ToString()
end

Matrix3x2.GetHashCode = function (this)
    return this.M11:GetHashCode() + this.M12:GetHashCode() + this.M21:GetHashCode() + this.M22:GetHashCode() + this.M31:GetHashCode() + this.M32:GetHashCode()
end

System.defStc("System.Numerics.Matrix3x2", Matrix3x2)