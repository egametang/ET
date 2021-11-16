local System = System
local SystemNumerics = System.Numerics

local sqrt = math.sqrt
local abs = math.abs
local tan = math.tan
local cos = math.cos
local sin = math.sin

local IComparable = System.IComparable
local IComparable_1 = System.IComparable_1
local IEquatable_1 = System.IEquatable_1

local new = function (cls, ...)
    local this = setmetatable({}, cls)
    return this, cls.__ctor__(this, ...)
end

local Matrix4x4 = {}

Matrix4x4.__ctor__ = function (this, m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44)
    if m11 == nil then
        this.M11 = 0
        this.M12 = 0
        this.M13 = 0
        this.M14 = 0

        this.M21 = 0
        this.M22 = 0
        this.M23 = 0
        this.M24 = 0

        this.M31 = 0
        this.M32 = 0
        this.M33 = 0
        this.M34 = 0

        this.M41 = 0
        this.M42 = 0
        this.M43 = 0
        this.M44 = 0
    elseif m11.M11 == nil then
        -- from singles
        this.M11 = m11 or 0
        this.M12 = m12 or 0
        this.M13 = m13 or 0
        this.M14 = m14 or 0

        this.M21 = m21 or 0
        this.M22 = m22 or 0
        this.M23 = m23 or 0
        this.M24 = m24 or 0

        this.M31 = m31 or 0
        this.M32 = m32 or 0
        this.M33 = m33 or 0
        this.M34 = m34 or 0

        this.M41 = m41 or 0
        this.M42 = m42 or 0
        this.M43 = m43 or 0
        this.M44 = m44 or 0
    else
        -- from matrix
        this.M11 = m11.M11
        this.M12 = m11.M12
        this.M13 = 0
        this.M14 = 0
        this.M21 = m11.M21
        this.M22 = m11.M22
        this.M23 = 0
        this.M24 = 0
        this.M31 = 0
        this.M32 = 0
        this.M33 = 1
        this.M34 = 0
        this.M41 = m11.M31
        this.M42 = m11.M32
        this.M43 = 0
        this.M44 = 1
    end 
    local mt = getmetatable(this)
    mt.__unm = Matrix4x4.op_UnaryNegation
    setmetatable(this, mt)   
  end

Matrix4x4.base = function (_, T)
    return { IComparable, IComparable_1(T), IEquatable_1(T) }
end

Matrix4x4.getIdentity = function ()
    return new(Matrix4x4, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)
end

Matrix4x4.getIsIdentity = function (this)
    return this.M11 == 1 and this.M22 == 1 and this.M33 == 1 and this.M44 == 1 and this.M12 == 0 and this.M13 == 0 and this.M14 == 0 and this.M21 == 0 and this.M23 == 0 and this.M24 == 0 and this.M31 == 0 and this.M32 == 0 and this.M34 == 0 and this.M41 == 0 and this.M42 == 0 and this.M43 == 0
end

Matrix4x4.getTranslation = function (this)
    return SystemNumerics.Vector3(this.M41, this.M42, this.M43)
end
  
Matrix4x4.setTranslation = function (this, value)
    this.M41 = value.X
    this.M42 = value.Y
    this.M43 = value.Z
end

Matrix4x4.CreateBillboard = function (objectPosition, cameraPosition, cameraUpVector, cameraForwardVector)

    local zaxis = SystemNumerics.Vector3(objectPosition.X - cameraPosition.X, objectPosition.Y - cameraPosition.Y, objectPosition.Z - cameraPosition.Z)

    local norm = zaxis:LengthSquared()

    if norm < 0.0001 --[[epsilon]] then
      zaxis = - cameraForwardVector
    else
      zaxis = SystemNumerics.Vector3.Multiply(zaxis, 1.0 / System.ToSingle(sqrt(norm)))
    end

    local xaxis = SystemNumerics.Vector3.Normalize(SystemNumerics.Vector3.Cross(cameraUpVector, zaxis))

    local yaxis = SystemNumerics.Vector3.Cross(zaxis, xaxis)

    local result = new(Matrix4x4)

    result.M11 = xaxis.X
    result.M12 = xaxis.Y
    result.M13 = xaxis.Z
    result.M14 = 0.0
    result.M21 = yaxis.X
    result.M22 = yaxis.Y
    result.M23 = yaxis.Z
    result.M24 = 0.0
    result.M31 = zaxis.X
    result.M32 = zaxis.Y
    result.M33 = zaxis.Z
    result.M34 = 0.0

    result.M41 = objectPosition.X
    result.M42 = objectPosition.Y
    result.M43 = objectPosition.Z
    result.M44 = 1.0

    return result:__clone__()
end

Matrix4x4.CreateConstrainedBillboard = function (objectPosition, cameraPosition, rotateAxis, cameraForwardVector, objectForwardVector)
    -- 0.1 degrees

    -- Treat the case when object and camera positions are too close.
    local faceDir = SystemNumerics.Vector3(objectPosition.X - cameraPosition.X, objectPosition.Y - cameraPosition.Y, objectPosition.Z - cameraPosition.Z)

    local norm = faceDir:LengthSquared()

    if norm < 0.0001 --[[epsilon]] then
      faceDir = - cameraForwardVector
    else
      faceDir = SystemNumerics.Vector3.Multiply(faceDir, (1.0 / System.ToSingle(sqrt(norm))))
    end

    local yaxis = rotateAxis:__clone__()
    local xaxis
    local zaxis

    -- Treat the case when angle between faceDir and rotateAxis is too close to 0.
    local dot = SystemNumerics.Vector3.Dot(rotateAxis, faceDir)

    if abs(dot) > 0.9982547 --[[minAngle]] then
      zaxis = objectForwardVector:__clone__()

      -- Make sure passed values are useful for compute.
      dot = SystemNumerics.Vector3.Dot(rotateAxis, zaxis)

      if abs(dot) > 0.9982547 --[[minAngle]] then
        zaxis = (abs(rotateAxis.Z) > 0.9982547 --[[minAngle]]) and SystemNumerics.Vector3(1, 0, 0) or SystemNumerics.Vector3(0, 0, - 1)
      end

      xaxis = SystemNumerics.Vector3.Normalize(SystemNumerics.Vector3.Cross(rotateAxis, zaxis))
      zaxis = SystemNumerics.Vector3.Normalize(SystemNumerics.Vector3.Cross(xaxis, rotateAxis))
    else
      xaxis = SystemNumerics.Vector3.Normalize(SystemNumerics.Vector3.Cross(rotateAxis, faceDir))
      zaxis = SystemNumerics.Vector3.Normalize(SystemNumerics.Vector3.Cross(xaxis, yaxis))
    end

    local result = new(Matrix4x4)

    result.M11 = xaxis.X
    result.M12 = xaxis.Y
    result.M13 = xaxis.Z
    result.M14 = 0.0
    result.M21 = yaxis.X
    result.M22 = yaxis.Y
    result.M23 = yaxis.Z
    result.M24 = 0.0
    result.M31 = zaxis.X
    result.M32 = zaxis.Y
    result.M33 = zaxis.Z
    result.M34 = 0.0

    result.M41 = objectPosition.X
    result.M42 = objectPosition.Y
    result.M43 = objectPosition.Z
    result.M44 = 1.0

    return result:__clone__()
end

Matrix4x4.CreateTranslation = function (position, yPosition, zPosition)
    local result = new(Matrix4x4)

    result.M11 = 1.0
    result.M12 = 0.0
    result.M13 = 0.0
    result.M14 = 0.0
    result.M21 = 0.0
    result.M22 = 1.0
    result.M23 = 0.0
    result.M24 = 0.0
    result.M31 = 0.0
    result.M32 = 0.0
    result.M33 = 1.0
    result.M34 = 0.0

    if yPosition ~= nil then
        position = SystemNumerics.Vector3(position, yPosition, zPosition)
    end

    result.M41 = position.X
    result.M42 = position.Y
    result.M43 = position.Z
    result.M44 = 1.0

    return result:__clone__()
end

Matrix4x4.CreateScale = function(val1, val2, val3, val4)
    if val4 == nil then
        if val3 == nil then
            if val2 == nil then
                if val1.X == nil then
                    -- CreateScale(Single)
                    local result = new(Matrix4x4)

                    result.M11 = val1
                    result.M12 = 0.0
                    result.M13 = 0.0
                    result.M14 = 0.0
                    result.M21 = 0.0
                    result.M22 = val1
                    result.M23 = 0.0
                    result.M24 = 0.0
                    result.M31 = 0.0
                    result.M32 = 0.0
                    result.M33 = val1
                    result.M34 = 0.0
                    result.M41 = 0.0
                    result.M42 = 0.0
                    result.M43 = 0.0
                    result.M44 = 1.0

                    return result:__clone__()
                else
                    -- CreateScale(Vector3)
                    local result = new(Matrix4x4)

                    result.M11 = val1.X
                    result.M12 = 0.0
                    result.M13 = 0.0
                    result.M14 = 0.0
                    result.M21 = 0.0
                    result.M22 = val1.Y
                    result.M23 = 0.0
                    result.M24 = 0.0
                    result.M31 = 0.0
                    result.M32 = 0.0
                    result.M33 = val1.Z
                    result.M34 = 0.0
                    result.M41 = 0.0
                    result.M42 = 0.0
                    result.M43 = 0.0
                    result.M44 = 1.0

                    return result:__clone__()
                end
            else
                if val1.X == nil then
                    -- CreateScale(Single, Vector3)
                    local result = new(Matrix4x4)

                    local tx = val2.X * (1 - val1)
                    local ty = val2.Y * (1 - val1)
                    local tz = val2.Z * (1 - val1)

                    result.M11 = val1
                    result.M12 = 0.0
                    result.M13 = 0.0
                    result.M14 = 0.0
                    result.M21 = 0.0
                    result.M22 = val1
                    result.M23 = 0.0
                    result.M24 = 0.0
                    result.M31 = 0.0
                    result.M32 = 0.0
                    result.M33 = val1
                    result.M34 = 0.0
                    result.M41 = tx
                    result.M42 = ty
                    result.M43 = tz
                    result.M44 = 1.0

                    return result:__clone__()
                else
                    -- CreateScale(Vector3, Vector3)
                    local result = new(Matrix4x4)

                    local tx = val2.X * (1 - val1.X)
                    local ty = val2.Y * (1 - val1.Y)
                    local tz = val2.Z * (1 - val1.Z)

                    result.M11 = val1.X
                    result.M12 = 0.0
                    result.M13 = 0.0
                    result.M14 = 0.0
                    result.M21 = 0.0
                    result.M22 = val1.Y
                    result.M23 = 0.0
                    result.M24 = 0.0
                    result.M31 = 0.0
                    result.M32 = 0.0
                    result.M33 = val1.Z
                    result.M34 = 0.0
                    result.M41 = tx
                    result.M42 = ty
                    result.M43 = tz
                    result.M44 = 1.0

                    return result:__clone__()
                end
            end
        else
            -- CreateScale(Single, Single, Single)
            local result = new(Matrix4x4)

            result.M11 = val1
            result.M12 = 0.0
            result.M13 = 0.0
            result.M14 = 0.0
            result.M21 = 0.0
            result.M22 = val2
            result.M23 = 0.0
            result.M24 = 0.0
            result.M31 = 0.0
            result.M32 = 0.0
            result.M33 = val3
            result.M34 = 0.0
            result.M41 = 0.0
            result.M42 = 0.0
            result.M43 = 0.0
            result.M44 = 1.0

            return result:__clone__()
        end
    else
        -- CreateScale(Single, Single, Single, Vector3)
        local result = new(Matrix4x4)

        local tx = val4.X * (1 - val1)
        local ty = val4.Y * (1 - val2)
        local tz = val4.Z * (1 - val3)

        result.M11 = val1
        result.M12 = 0.0
        result.M13 = 0.0
        result.M14 = 0.0
        result.M21 = 0.0
        result.M22 = val2
        result.M23 = 0.0
        result.M24 = 0.0
        result.M31 = 0.0
        result.M32 = 0.0
        result.M33 = val3
        result.M34 = 0.0
        result.M41 = tx
        result.M42 = ty
        result.M43 = tz
        result.M44 = 1.0

        return result:__clone__()
    end
end

Matrix4x4.CreateRotationX = function (radians, centerPoint)

    if centerPoint == nil then
        local result = new(Matrix4x4)

        local c = System.ToSingle(cos(radians))
        local s = System.ToSingle(sin(radians))
    
        -- [  1  0  0  0 ]
        -- [  0  c  s  0 ]
        -- [  0 -s  c  0 ]
        -- [  0  0  0  1 ]
        result.M11 = 1.0
        result.M12 = 0.0
        result.M13 = 0.0
        result.M14 = 0.0
        result.M21 = 0.0
        result.M22 = c
        result.M23 = s
        result.M24 = 0.0
        result.M31 = 0.0
        result.M32 = - s
        result.M33 = c
        result.M34 = 0.0
        result.M41 = 0.0
        result.M42 = 0.0
        result.M43 = 0.0
        result.M44 = 1.0
    
        return result:__clone__()
    else
        local result = new(Matrix4x4)

        local c = System.ToSingle(cos(radians))
        local s = System.ToSingle(sin(radians))
  
        local y = centerPoint.Y * (1 - c) + centerPoint.Z * s
        local z = centerPoint.Z * (1 - c) - centerPoint.Y * s
  
        -- [  1  0  0  0 ]
        -- [  0  c  s  0 ]
        -- [  0 -s  c  0 ]
        -- [  0  y  z  1 ]
        result.M11 = 1.0
        result.M12 = 0.0
        result.M13 = 0.0
        result.M14 = 0.0
        result.M21 = 0.0
        result.M22 = c
        result.M23 = s
        result.M24 = 0.0
        result.M31 = 0.0
        result.M32 = - s
        result.M33 = c
        result.M34 = 0.0
        result.M41 = 0.0
        result.M42 = y
        result.M43 = z
        result.M44 = 1.0
  
        return result:__clone__()
    end    
end

Matrix4x4.CreateRotationY = function(radians, centerPoint)
    if centerPoint == nil then
        local result = new(Matrix4x4)

        local c = System.ToSingle(cos(radians))
        local s = System.ToSingle(sin(radians))
  
        -- [  c  0 -s  0 ]
        -- [  0  1  0  0 ]
        -- [  s  0  c  0 ]
        -- [  0  0  0  1 ]
        result.M11 = c
        result.M12 = 0.0
        result.M13 = - s
        result.M14 = 0.0
        result.M21 = 0.0
        result.M22 = 1.0
        result.M23 = 0.0
        result.M24 = 0.0
        result.M31 = s
        result.M32 = 0.0
        result.M33 = c
        result.M34 = 0.0
        result.M41 = 0.0
        result.M42 = 0.0
        result.M43 = 0.0
        result.M44 = 1.0
  
        return result:__clone__()
    else
        local result = new(Matrix4x4)

        local c = System.ToSingle(cos(radians))
        local s = System.ToSingle(sin(radians))

        local x = centerPoint.X * (1 - c) - centerPoint.Z * s
        local z = centerPoint.Z * (1 - c) + centerPoint.X * s

         -- [  c  0 -s  0 ]
         -- [  0  1  0  0 ]
         -- [  s  0  c  0 ]
         -- [  x  0  z  1 ]
        result.M11 = c
        result.M12 = 0.0
        result.M13 = - s
        result.M14 = 0.0
        result.M21 = 0.0
        result.M22 = 1.0
        result.M23 = 0.0
        result.M24 = 0.0
        result.M31 = s
        result.M32 = 0.0
        result.M33 = c
        result.M34 = 0.0
        result.M41 = x
        result.M42 = 0.0
        result.M43 = z
        result.M44 = 1.0

        return result:__clone__()
    end
end

Matrix4x4.CreateRotationZ = function(radians, centerPoint)
    if centerPoint == nil then
        local result = new(Matrix4x4)

        local c = System.ToSingle(cos(radians))
        local s = System.ToSingle(sin(radians))
  
        -- [  c  s  0  0 ]
        -- [ -s  c  0  0 ]
        -- [  0  0  1  0 ]
        -- [  0  0  0  1 ]
        result.M11 = c
        result.M12 = s
        result.M13 = 0.0
        result.M14 = 0.0
        result.M21 = - s
        result.M22 = c
        result.M23 = 0.0
        result.M24 = 0.0
        result.M31 = 0.0
        result.M32 = 0.0
        result.M33 = 1.0
        result.M34 = 0.0
        result.M41 = 0.0
        result.M42 = 0.0
        result.M43 = 0.0
        result.M44 = 1.0
  
        return result:__clone__()
    else
        local result = new(Matrix4x4)

        local c = System.ToSingle(cos(radians))
        local s = System.ToSingle(sin(radians))
  
        local x = centerPoint.X * (1 - c) + centerPoint.Y * s
        local y = centerPoint.Y * (1 - c) - centerPoint.X * s
  
        -- [  c  s  0  0 ]
        -- [ -s  c  0  0 ]
        -- [  0  0  1  0 ]
        -- [  x  y  0  1 ]
        result.M11 = c
        result.M12 = s
        result.M13 = 0.0
        result.M14 = 0.0
        result.M21 = - s
        result.M22 = c
        result.M23 = 0.0
        result.M24 = 0.0
        result.M31 = 0.0
        result.M32 = 0.0
        result.M33 = 1.0
        result.M34 = 0.0
        result.M41 = x
        result.M42 = y
        result.M43 = 0.0
        result.M44 = 1.0
  
        return result:__clone__()
    end
end

Matrix4x4.CreateFromAxisAngle = function (axis, angle)
    -- a: angle
    -- x, y, z: unit vector for axis.
    --
    -- Rotation matrix M can compute by using below equation.
    --
    --        T               T
    --  M = uu + (cos a)( I-uu ) + (sin a)S
    --
    -- Where:
    --
    --  u = ( x, y, z )
    --
    --      [  0 -z  y ]
    --  S = [  z  0 -x ]
    --      [ -y  x  0 ]
    --
    --      [ 1 0 0 ]
    --  I = [ 0 1 0 ]
    --      [ 0 0 1 ]
    --
    --
    --     [  xx+cosa*(1-xx)   yx-cosa*yx-sina*z zx-cosa*xz+sina*y ]
    -- M = [ xy-cosa*yx+sina*z    yy+cosa(1-yy)  yz-cosa*yz-sina*x ]
    --     [ zx-cosa*zx-sina*y zy-cosa*zy+sina*x   zz+cosa*(1-zz)  ]
    --
    local x = axis.X local y = axis.Y local z = axis.Z
    local sa = System.ToSingle(sin(angle)) local ca = System.ToSingle(cos(angle))
    local xx = x * x local yy = y * y local zz = z * z
    local xy = x * y local xz = x * z local yz = y * z

    local result = new(Matrix4x4)

    result.M11 = xx + ca * (1.0 - xx)
    result.M12 = xy - ca * xy + sa * z
    result.M13 = xz - ca * xz - sa * y
    result.M14 = 0.0
    result.M21 = xy - ca * xy - sa * z
    result.M22 = yy + ca * (1.0 - yy)
    result.M23 = yz - ca * yz + sa * x
    result.M24 = 0.0
    result.M31 = xz - ca * xz + sa * y
    result.M32 = yz - ca * yz - sa * x
    result.M33 = zz + ca * (1.0 - zz)
    result.M34 = 0.0
    result.M41 = 0.0
    result.M42 = 0.0
    result.M43 = 0.0
    result.M44 = 1.0

    return result:__clone__()
end

Matrix4x4.CreatePerspectiveFieldOfView = function (fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance)
    if fieldOfView <= 0.0 or fieldOfView >= 3.14159265358979 --[[Math.PI]] then
      System.throw(System.ArgumentOutOfRangeException("fieldOfView"))
    end

    if nearPlaneDistance <= 0.0 then
      System.throw(System.ArgumentOutOfRangeException("nearPlaneDistance"))
    end

    if farPlaneDistance <= 0.0 then
      System.throw(System.ArgumentOutOfRangeException("farPlaneDistance"))
    end

    if nearPlaneDistance >= farPlaneDistance then
      System.throw(System.ArgumentOutOfRangeException("nearPlaneDistance"))
    end

    local yScale = 1.0 / System.ToSingle(tan(fieldOfView * 0.5))
    local xScale = yScale / aspectRatio

    local result = new(Matrix4x4)

    result.M11 = xScale
    result.M14 = 0.0 result.M13 = result.M14 result.M12 = result.M13

    result.M22 = yScale
    result.M24 = 0.0 result.M23 = result.M24 result.M21 = result.M23

    result.M32 = 0.0 result.M31 = result.M32
    result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance)
    result.M34 = - 1.0

    result.M44 = 0.0 result.M42 = result.M44 result.M41 = result.M42
    result.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance)

    return result:__clone__()
end

Matrix4x4.CreatePerspective = function (width, height, nearPlaneDistance, farPlaneDistance)
    if nearPlaneDistance <= 0.0 then
      System.throw(System.ArgumentOutOfRangeException("nearPlaneDistance"))
    end
    if farPlaneDistance <= 0.0 then
      System.throw(System.ArgumentOutOfRangeException("farPlaneDistance"))
    end
    if nearPlaneDistance >= farPlaneDistance then
      System.throw(System.ArgumentOutOfRangeException("nearPlaneDistance"))
    end
    local result = new(Matrix4x4)

    result.M11 = 2.0 * nearPlaneDistance / width
    result.M14 = 0.0 result.M13 = result.M14 result.M12 = result.M13

    result.M22 = 2.0 * nearPlaneDistance / height
    result.M24 = 0.0 result.M23 = result.M24 result.M21 = result.M23

    result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance)
    result.M32 = 0.0 result.M31 = result.M32
    result.M34 = - 1.0

    result.M44 = 0.0 result.M42 = result.M44 result.M41 = result.M42
    result.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance)

    return result:__clone__()
end

Matrix4x4.CreatePerspectiveOffCenter = function (left, right, bottom, top, nearPlaneDistance, farPlaneDistance)
    if nearPlaneDistance <= 0.0 then
      System.throw(System.ArgumentOutOfRangeException("nearPlaneDistance"))
    end

    if farPlaneDistance <= 0.0 then
      System.throw(System.ArgumentOutOfRangeException("farPlaneDistance"))
    end

    if nearPlaneDistance >= farPlaneDistance then
      System.throw(System.ArgumentOutOfRangeException("nearPlaneDistance"))
    end

    local result = new(Matrix4x4)

    result.M11 = 2.0 * nearPlaneDistance / (right - left)
    result.M14 = 0.0 result.M13 = result.M14 result.M12 = result.M13

    result.M22 = 2.0 * nearPlaneDistance / (top - bottom)
    result.M24 = 0.0 result.M23 = result.M24 result.M21 = result.M23

    result.M31 = (left + right) / (right - left)
    result.M32 = (top + bottom) / (top - bottom)
    result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance)
    result.M34 = - 1.0

    result.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance)
    result.M44 = 0.0 result.M42 = result.M44 result.M41 = result.M42

    return result:__clone__()
end

Matrix4x4.CreateOrthographic = function (width, height, zNearPlane, zFarPlane)
    local result = new(Matrix4x4)

    result.M11 = 2.0 / width
    result.M14 = 0.0 result.M13 = result.M14 result.M12 = result.M13

    result.M22 = 2.0 / height
    result.M24 = 0.0 result.M23 = result.M24 result.M21 = result.M23

    result.M33 = 1.0 / (zNearPlane - zFarPlane)
    result.M34 = 0.0 result.M32 = result.M34 result.M31 = result.M32

    result.M42 = 0.0 result.M41 = result.M42
    result.M43 = zNearPlane / (zNearPlane - zFarPlane)
    result.M44 = 1.0

    return result:__clone__()
end

Matrix4x4.CreateOrthographicOffCenter = function (left, right, bottom, top, zNearPlane, zFarPlane)
    local result = new(Matrix4x4)

    result.M11 = 2.0 / (right - left)
    result.M14 = 0.0 result.M13 = result.M14 result.M12 = result.M13

    result.M22 = 2.0 / (top - bottom)
    result.M24 = 0.0 result.M23 = result.M24 result.M21 = result.M23

    result.M33 = 1.0 / (zNearPlane - zFarPlane)
    result.M34 = 0.0 result.M32 = result.M34 result.M31 = result.M32

    result.M41 = (left + right) / (left - right)
    result.M42 = (top + bottom) / (bottom - top)
    result.M43 = zNearPlane / (zNearPlane - zFarPlane)
    result.M44 = 1.0

    return result:__clone__()
end

Matrix4x4.CreateLookAt = function (cameraPosition, cameraTarget, cameraUpVector)
    local zaxis = SystemNumerics.Vector3.Normalize(SystemNumerics.Vector3.op_Subtraction(cameraPosition, cameraTarget))
    local xaxis = SystemNumerics.Vector3.Normalize(SystemNumerics.Vector3.Cross(cameraUpVector, zaxis))
    local yaxis = SystemNumerics.Vector3.Cross(zaxis, xaxis)

    local result = new(Matrix4x4)

    result.M11 = xaxis.X
    result.M12 = yaxis.X
    result.M13 = zaxis.X
    result.M14 = 0.0
    result.M21 = xaxis.Y
    result.M22 = yaxis.Y
    result.M23 = zaxis.Y
    result.M24 = 0.0
    result.M31 = xaxis.Z
    result.M32 = yaxis.Z
    result.M33 = zaxis.Z
    result.M34 = 0.0
    result.M41 = - SystemNumerics.Vector3.Dot(xaxis, cameraPosition)
    result.M42 = - SystemNumerics.Vector3.Dot(yaxis, cameraPosition)
    result.M43 = - SystemNumerics.Vector3.Dot(zaxis, cameraPosition)
    result.M44 = 1.0

    return result:__clone__()
end

Matrix4x4.CreateWorld = function (position, forward, up)
    local zaxis = SystemNumerics.Vector3.Normalize(- forward)
    local xaxis = SystemNumerics.Vector3.Normalize(SystemNumerics.Vector3.Cross(up, zaxis))
    local yaxis = SystemNumerics.Vector3.Cross(zaxis, xaxis)

    local result = new(Matrix4x4)

    result.M11 = xaxis.X
    result.M12 = xaxis.Y
    result.M13 = xaxis.Z
    result.M14 = 0.0
    result.M21 = yaxis.X
    result.M22 = yaxis.Y
    result.M23 = yaxis.Z
    result.M24 = 0.0
    result.M31 = zaxis.X
    result.M32 = zaxis.Y
    result.M33 = zaxis.Z
    result.M34 = 0.0
    result.M41 = position.X
    result.M42 = position.Y
    result.M43 = position.Z
    result.M44 = 1.0

    return result:__clone__()
end

Matrix4x4.CreateFromQuaternion = function (quaternion)
    local result = new(Matrix4x4)

    local xx = quaternion.X * quaternion.X
    local yy = quaternion.Y * quaternion.Y
    local zz = quaternion.Z * quaternion.Z

    local xy = quaternion.X * quaternion.Y
    local wz = quaternion.Z * quaternion.W
    local xz = quaternion.Z * quaternion.X
    local wy = quaternion.Y * quaternion.W
    local yz = quaternion.Y * quaternion.Z
    local wx = quaternion.X * quaternion.W

    result.M11 = 1.0 - 2.0 * (yy + zz)
    result.M12 = 2.0 * (xy + wz)
    result.M13 = 2.0 * (xz - wy)
    result.M14 = 0.0
    result.M21 = 2.0 * (xy - wz)
    result.M22 = 1.0 - 2.0 * (zz + xx)
    result.M23 = 2.0 * (yz + wx)
    result.M24 = 0.0
    result.M31 = 2.0 * (xz + wy)
    result.M32 = 2.0 * (yz - wx)
    result.M33 = 1.0 - 2.0 * (yy + xx)
    result.M34 = 0.0
    result.M41 = 0.0
    result.M42 = 0.0
    result.M43 = 0.0
    result.M44 = 1.0

    return result:__clone__()
end

Matrix4x4.CreateFromYawPitchRoll = function (yaw, pitch, roll)
    local q = SystemNumerics.Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll)

    return Matrix4x4.CreateFromQuaternion(q:__clone__())
end

Matrix4x4.CreateShadow = function (lightDirection, plane)
    local p = SystemNumerics.Plane.Normalize(plane)

    local dot = p.Normal.X * lightDirection.X + p.Normal.Y * lightDirection.Y + p.Normal.Z * lightDirection.Z
    local a = - p.Normal.X
    local b = - p.Normal.Y
    local c = - p.Normal.Z
    local d = - p.D

    local result = new(Matrix4x4)

    result.M11 = a * lightDirection.X + dot
    result.M21 = b * lightDirection.X
    result.M31 = c * lightDirection.X
    result.M41 = d * lightDirection.X

    result.M12 = a * lightDirection.Y
    result.M22 = b * lightDirection.Y + dot
    result.M32 = c * lightDirection.Y
    result.M42 = d * lightDirection.Y

    result.M13 = a * lightDirection.Z
    result.M23 = b * lightDirection.Z
    result.M33 = c * lightDirection.Z + dot
    result.M43 = d * lightDirection.Z

    result.M14 = 0.0
    result.M24 = 0.0
    result.M34 = 0.0
    result.M44 = dot

    return result:__clone__()
end

Matrix4x4.CreateReflection = function (value)
    value = SystemNumerics.Plane.Normalize(value)

    local a = value.Normal.X
    local b = value.Normal.Y
    local c = value.Normal.Z

    local fa = - 2.0 * a
    local fb = - 2.0 * b
    local fc = - 2.0 * c

    local result = new(Matrix4x4)

    result.M11 = fa * a + 1.0
    result.M12 = fb * a
    result.M13 = fc * a
    result.M14 = 0.0

    result.M21 = fa * b
    result.M22 = fb * b + 1.0
    result.M23 = fc * b
    result.M24 = 0.0

    result.M31 = fa * c
    result.M32 = fb * c
    result.M33 = fc * c + 1.0
    result.M34 = 0.0

    result.M41 = fa * value.D
    result.M42 = fb * value.D
    result.M43 = fc * value.D
    result.M44 = 1.0

    return result:__clone__()
end

Matrix4x4.GetDeterminant = function (this)
    -- | a b c d |     | f g h |     | e g h |     | e f h |     | e f g |
    -- | e f g h | = a | j k l | - b | i k l | + c | i j l | - d | i j k |
    -- | i j k l |     | n o p |     | m o p |     | m n p |     | m n o |
    -- | m n o p |
    --
    --   | f g h |
    -- a | j k l | = a ( f ( kp - lo ) - g ( jp - ln ) + h ( jo - kn ) )
    --   | n o p |
    --
    --   | e g h |     
    -- b | i k l | = b ( e ( kp - lo ) - g ( ip - lm ) + h ( io - km ) )
    --   | m o p |     
    --
    --   | e f h |
    -- c | i j l | = c ( e ( jp - ln ) - f ( ip - lm ) + h ( in - jm ) )
    --   | m n p |
    --
    --   | e f g |
    -- d | i j k | = d ( e ( jo - kn ) - f ( io - km ) + g ( in - jm ) )
    --   | m n o |
    --
    -- Cost of operation
    -- 17 adds and 28 muls.
    --
    -- add: 6 + 8 + 3 = 17
    -- mul: 12 + 16 = 28

    local a = this.M11 local b = this.M12 local c = this.M13 local d = this.M14
    local e = this.M21 local f = this.M22 local g = this.M23 local h = this.M24
    local i = this.M31 local j = this.M32 local k = this.M33 local l = this.M34
    local m = this.M41 local n = this.M42 local o = this.M43 local p = this.M44

    local kp_lo = k * p - l * o
    local jp_ln = j * p - l * n
    local jo_kn = j * o - k * n
    local ip_lm = i * p - l * m
    local io_km = i * o - k * m
    local in_jm = i * n - j * m

    return a * (f * kp_lo - g * jp_ln + h * jo_kn) - b * (e * kp_lo - g * ip_lm + h * io_km) + c * (e * jp_ln - f * ip_lm + h * in_jm) - d * (e * jo_kn - f * io_km + g * in_jm)
end

Matrix4x4.Invert = function (matrix, result)
    --                                       -1
    -- If you have matrix M, inverse Matrix M   can compute
    --
    --     -1       1      
    --    M   = --------- A
    --            det(M)
    --
    -- A is adjugate (adjoint) of M, where,
    --
    --      T
    -- A = C
    --
    -- C is Cofactor matrix of M, where,
    --           i + j
    -- C   = (-1)      * det(M  )
    --  ij                    ij
    --
    --     [ a b c d ]
    -- M = [ e f g h ]
    --     [ i j k l ]
    --     [ m n o p ]
    --
    -- First Row
    --           2 | f g h |
    -- C   = (-1)  | j k l | = + ( f ( kp - lo ) - g ( jp - ln ) + h ( jo - kn ) )
    --  11         | n o p |
    --
    --           3 | e g h |
    -- C   = (-1)  | i k l | = - ( e ( kp - lo ) - g ( ip - lm ) + h ( io - km ) )
    --  12         | m o p |
    --
    --           4 | e f h |
    -- C   = (-1)  | i j l | = + ( e ( jp - ln ) - f ( ip - lm ) + h ( in - jm ) )
    --  13         | m n p |
    --
    --           5 | e f g |
    -- C   = (-1)  | i j k | = - ( e ( jo - kn ) - f ( io - km ) + g ( in - jm ) )
    --  14         | m n o |
    --
    -- Second Row
    --           3 | b c d |
    -- C   = (-1)  | j k l | = - ( b ( kp - lo ) - c ( jp - ln ) + d ( jo - kn ) )
    --  21         | n o p |
    --
    --           4 | a c d |
    -- C   = (-1)  | i k l | = + ( a ( kp - lo ) - c ( ip - lm ) + d ( io - km ) )
    --  22         | m o p |
    --
    --           5 | a b d |
    -- C   = (-1)  | i j l | = - ( a ( jp - ln ) - b ( ip - lm ) + d ( in - jm ) )
    --  23         | m n p |
    --
    --           6 | a b c |
    -- C   = (-1)  | i j k | = + ( a ( jo - kn ) - b ( io - km ) + c ( in - jm ) )
    --  24         | m n o |
    --
    -- Third Row
    --           4 | b c d |
    -- C   = (-1)  | f g h | = + ( b ( gp - ho ) - c ( fp - hn ) + d ( fo - gn ) )
    --  31         | n o p |
    --
    --           5 | a c d |
    -- C   = (-1)  | e g h | = - ( a ( gp - ho ) - c ( ep - hm ) + d ( eo - gm ) )
    --  32         | m o p |
    --
    --           6 | a b d |
    -- C   = (-1)  | e f h | = + ( a ( fp - hn ) - b ( ep - hm ) + d ( en - fm ) )
    --  33         | m n p |
    --
    --           7 | a b c |
    -- C   = (-1)  | e f g | = - ( a ( fo - gn ) - b ( eo - gm ) + c ( en - fm ) )
    --  34         | m n o |
    --
    -- Fourth Row
    --           5 | b c d |
    -- C   = (-1)  | f g h | = - ( b ( gl - hk ) - c ( fl - hj ) + d ( fk - gj ) )
    --  41         | j k l |
    --
    --           6 | a c d |
    -- C   = (-1)  | e g h | = + ( a ( gl - hk ) - c ( el - hi ) + d ( ek - gi ) )
    --  42         | i k l |
    --
    --           7 | a b d |
    -- C   = (-1)  | e f h | = - ( a ( fl - hj ) - b ( el - hi ) + d ( ej - fi ) )
    --  43         | i j l |
    --
    --           8 | a b c |
    -- C   = (-1)  | e f g | = + ( a ( fk - gj ) - b ( ek - gi ) + c ( ej - fi ) )
    --  44         | i j k |
    --
    -- Cost of operation
    -- 53 adds, 104 muls, and 1 div.
    local a = matrix.M11 local b = matrix.M12 local c = matrix.M13 local d = matrix.M14
    local e = matrix.M21 local f = matrix.M22 local g = matrix.M23 local h = matrix.M24
    local i = matrix.M31 local j = matrix.M32 local k = matrix.M33 local l = matrix.M34
    local m = matrix.M41 local n = matrix.M42 local o = matrix.M43 local p = matrix.M44

    local kp_lo = k * p - l * o
    local jp_ln = j * p - l * n
    local jo_kn = j * o - k * n
    local ip_lm = i * p - l * m
    local io_km = i * o - k * m
    local in_jm = i * n - j * m

    local a11 = (f * kp_lo - g * jp_ln + h * jo_kn)
    local a12 = - (e * kp_lo - g * ip_lm + h * io_km)
    local a13 = (e * jp_ln - f * ip_lm + h * in_jm)
    local a14 = - (e * jo_kn - f * io_km + g * in_jm)

    local det = a * a11 + b * a12 + c * a13 + d * a14

    if abs(det) < 1.401298E-45 --[[Single.Epsilon]] then
      result = new(Matrix4x4, System.Single.NaN, System.Single.NaN, System.Single.NaN, System.Single.NaN, System.Single.NaN, System.Single.NaN, System.Single.NaN, System.Single.NaN, System.Single.NaN, System.Single.NaN, System.Single.NaN, System.Single.NaN, System.Single.NaN, System.Single.NaN, System.Single.NaN, System.Single.NaN)
      return false, result
    end

    local invDet = 1.0 / det

    if result == nil then
        result = new(Matrix4x4)
    end

    result.M11 = a11 * invDet
    result.M21 = a12 * invDet
    result.M31 = a13 * invDet
    result.M41 = a14 * invDet

    result.M12 = - (b * kp_lo - c * jp_ln + d * jo_kn) * invDet
    result.M22 = (a * kp_lo - c * ip_lm + d * io_km) * invDet
    result.M32 = - (a * jp_ln - b * ip_lm + d * in_jm) * invDet
    result.M42 = (a * jo_kn - b * io_km + c * in_jm) * invDet

    local gp_ho = g * p - h * o
    local fp_hn = f * p - h * n
    local fo_gn = f * o - g * n
    local ep_hm = e * p - h * m
    local eo_gm = e * o - g * m
    local en_fm = e * n - f * m

    result.M13 = (b * gp_ho - c * fp_hn + d * fo_gn) * invDet
    result.M23 = - (a * gp_ho - c * ep_hm + d * eo_gm) * invDet
    result.M33 = (a * fp_hn - b * ep_hm + d * en_fm) * invDet
    result.M43 = - (a * fo_gn - b * eo_gm + c * en_fm) * invDet

    local gl_hk = g * l - h * k
    local fl_hj = f * l - h * j
    local fk_gj = f * k - g * j
    local el_hi = e * l - h * i
    local ek_gi = e * k - g * i
    local ej_fi = e * j - f * i

    result.M14 = - (b * gl_hk - c * fl_hj + d * fk_gj) * invDet
    result.M24 = (a * gl_hk - c * el_hi + d * ek_gi) * invDet
    result.M34 = - (a * fl_hj - b * el_hi + d * ej_fi) * invDet
    result.M44 = (a * fk_gj - b * ek_gi + c * ej_fi) * invDet

    return true, result
end

Matrix4x4.Transform = function (value, rotation)
    -- Compute rotation matrix.
    local x2 = rotation.X + rotation.X
    local y2 = rotation.Y + rotation.Y
    local z2 = rotation.Z + rotation.Z

    local wx2 = rotation.W * x2
    local wy2 = rotation.W * y2
    local wz2 = rotation.W * z2
    local xx2 = rotation.X * x2
    local xy2 = rotation.X * y2
    local xz2 = rotation.X * z2
    local yy2 = rotation.Y * y2
    local yz2 = rotation.Y * z2
    local zz2 = rotation.Z * z2

    local q11 = 1.0 - yy2 - zz2
    local q21 = xy2 - wz2
    local q31 = xz2 + wy2

    local q12 = xy2 + wz2
    local q22 = 1.0 - xx2 - zz2
    local q32 = yz2 - wx2

    local q13 = xz2 - wy2
    local q23 = yz2 + wx2
    local q33 = 1.0 - xx2 - yy2

    local result = new(Matrix4x4)

    -- First row
    result.M11 = value.M11 * q11 + value.M12 * q21 + value.M13 * q31
    result.M12 = value.M11 * q12 + value.M12 * q22 + value.M13 * q32
    result.M13 = value.M11 * q13 + value.M12 * q23 + value.M13 * q33
    result.M14 = value.M14

    -- Second row
    result.M21 = value.M21 * q11 + value.M22 * q21 + value.M23 * q31
    result.M22 = value.M21 * q12 + value.M22 * q22 + value.M23 * q32
    result.M23 = value.M21 * q13 + value.M22 * q23 + value.M23 * q33
    result.M24 = value.M24

    -- Third row
    result.M31 = value.M31 * q11 + value.M32 * q21 + value.M33 * q31
    result.M32 = value.M31 * q12 + value.M32 * q22 + value.M33 * q32
    result.M33 = value.M31 * q13 + value.M32 * q23 + value.M33 * q33
    result.M34 = value.M34

    -- Fourth row
    result.M41 = value.M41 * q11 + value.M42 * q21 + value.M43 * q31
    result.M42 = value.M41 * q12 + value.M42 * q22 + value.M43 * q32
    result.M43 = value.M41 * q13 + value.M42 * q23 + value.M43 * q33
    result.M44 = value.M44

    return result:__clone__()
end

Matrix4x4.Transpose = function (matrix)
    local result = new(Matrix4x4)

    result.M11 = matrix.M11
    result.M12 = matrix.M21
    result.M13 = matrix.M31
    result.M14 = matrix.M41
    result.M21 = matrix.M12
    result.M22 = matrix.M22
    result.M23 = matrix.M32
    result.M24 = matrix.M42
    result.M31 = matrix.M13
    result.M32 = matrix.M23
    result.M33 = matrix.M33
    result.M34 = matrix.M43
    result.M41 = matrix.M14
    result.M42 = matrix.M24
    result.M43 = matrix.M34
    result.M44 = matrix.M44

    return result:__clone__()
end

Matrix4x4.Lerp = function (matrix1, matrix2, amount)
    local result = new(Matrix4x4)

    -- First row
    result.M11 = matrix1.M11 + (matrix2.M11 - matrix1.M11) * amount
    result.M12 = matrix1.M12 + (matrix2.M12 - matrix1.M12) * amount
    result.M13 = matrix1.M13 + (matrix2.M13 - matrix1.M13) * amount
    result.M14 = matrix1.M14 + (matrix2.M14 - matrix1.M14) * amount

    -- Second row
    result.M21 = matrix1.M21 + (matrix2.M21 - matrix1.M21) * amount
    result.M22 = matrix1.M22 + (matrix2.M22 - matrix1.M22) * amount
    result.M23 = matrix1.M23 + (matrix2.M23 - matrix1.M23) * amount
    result.M24 = matrix1.M24 + (matrix2.M24 - matrix1.M24) * amount

    -- Third row
    result.M31 = matrix1.M31 + (matrix2.M31 - matrix1.M31) * amount
    result.M32 = matrix1.M32 + (matrix2.M32 - matrix1.M32) * amount
    result.M33 = matrix1.M33 + (matrix2.M33 - matrix1.M33) * amount
    result.M34 = matrix1.M34 + (matrix2.M34 - matrix1.M34) * amount

    -- Fourth row
    result.M41 = matrix1.M41 + (matrix2.M41 - matrix1.M41) * amount
    result.M42 = matrix1.M42 + (matrix2.M42 - matrix1.M42) * amount
    result.M43 = matrix1.M43 + (matrix2.M43 - matrix1.M43) * amount
    result.M44 = matrix1.M44 + (matrix2.M44 - matrix1.M44) * amount

    return result:__clone__()
end

Matrix4x4.Negate = function (value)
    local result = new(Matrix4x4)

    result.M11 = - value.M11
    result.M12 = - value.M12
    result.M13 = - value.M13
    result.M14 = - value.M14
    result.M21 = - value.M21
    result.M22 = - value.M22
    result.M23 = - value.M23
    result.M24 = - value.M24
    result.M31 = - value.M31
    result.M32 = - value.M32
    result.M33 = - value.M33
    result.M34 = - value.M34
    result.M41 = - value.M41
    result.M42 = - value.M42
    result.M43 = - value.M43
    result.M44 = - value.M44

    return result:__clone__()
end

Matrix4x4.Add = function (value1, value2)
    local result = new(Matrix4x4)

    result.M11 = value1.M11 + value2.M11
    result.M12 = value1.M12 + value2.M12
    result.M13 = value1.M13 + value2.M13
    result.M14 = value1.M14 + value2.M14
    result.M21 = value1.M21 + value2.M21
    result.M22 = value1.M22 + value2.M22
    result.M23 = value1.M23 + value2.M23
    result.M24 = value1.M24 + value2.M24
    result.M31 = value1.M31 + value2.M31
    result.M32 = value1.M32 + value2.M32
    result.M33 = value1.M33 + value2.M33
    result.M34 = value1.M34 + value2.M34
    result.M41 = value1.M41 + value2.M41
    result.M42 = value1.M42 + value2.M42
    result.M43 = value1.M43 + value2.M43
    result.M44 = value1.M44 + value2.M44

    return result:__clone__()
end

Matrix4x4.Subtract = function (value1, value2)
    local result = new(Matrix4x4)

    result.M11 = value1.M11 - value2.M11
    result.M12 = value1.M12 - value2.M12
    result.M13 = value1.M13 - value2.M13
    result.M14 = value1.M14 - value2.M14
    result.M21 = value1.M21 - value2.M21
    result.M22 = value1.M22 - value2.M22
    result.M23 = value1.M23 - value2.M23
    result.M24 = value1.M24 - value2.M24
    result.M31 = value1.M31 - value2.M31
    result.M32 = value1.M32 - value2.M32
    result.M33 = value1.M33 - value2.M33
    result.M34 = value1.M34 - value2.M34
    result.M41 = value1.M41 - value2.M41
    result.M42 = value1.M42 - value2.M42
    result.M43 = value1.M43 - value2.M43
    result.M44 = value1.M44 - value2.M44

    return result:__clone__()
end

Matrix4x4.Multiply = function(value1, value2)
    if value2.M11 == nil then
        -- scalar
        local result = new(Matrix4x4)

        result.M11 = value1.M11 * value2
        result.M12 = value1.M12 * value2
        result.M13 = value1.M13 * value2
        result.M14 = value1.M14 * value2
        result.M21 = value1.M21 * value2
        result.M22 = value1.M22 * value2
        result.M23 = value1.M23 * value2
        result.M24 = value1.M24 * value2
        result.M31 = value1.M31 * value2
        result.M32 = value1.M32 * value2
        result.M33 = value1.M33 * value2
        result.M34 = value1.M34 * value2
        result.M41 = value1.M41 * value2
        result.M42 = value1.M42 * value2
        result.M43 = value1.M43 * value2
        result.M44 = value1.M44 * value2

        return result:__clone__()
    else
        -- matrix
        local result = new(Matrix4x4)

        -- First row
        result.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21 + value1.M13 * value2.M31 + value1.M14 * value2.M41
        result.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22 + value1.M13 * value2.M32 + value1.M14 * value2.M42
        result.M13 = value1.M11 * value2.M13 + value1.M12 * value2.M23 + value1.M13 * value2.M33 + value1.M14 * value2.M43
        result.M14 = value1.M11 * value2.M14 + value1.M12 * value2.M24 + value1.M13 * value2.M34 + value1.M14 * value2.M44

        -- Second row
        result.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21 + value1.M23 * value2.M31 + value1.M24 * value2.M41
        result.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22 + value1.M23 * value2.M32 + value1.M24 * value2.M42
        result.M23 = value1.M21 * value2.M13 + value1.M22 * value2.M23 + value1.M23 * value2.M33 + value1.M24 * value2.M43
        result.M24 = value1.M21 * value2.M14 + value1.M22 * value2.M24 + value1.M23 * value2.M34 + value1.M24 * value2.M44

        -- Third row
        result.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value1.M33 * value2.M31 + value1.M34 * value2.M41
        result.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value1.M33 * value2.M32 + value1.M34 * value2.M42
        result.M33 = value1.M31 * value2.M13 + value1.M32 * value2.M23 + value1.M33 * value2.M33 + value1.M34 * value2.M43
        result.M34 = value1.M31 * value2.M14 + value1.M32 * value2.M24 + value1.M33 * value2.M34 + value1.M34 * value2.M44

        -- Fourth row
        result.M41 = value1.M41 * value2.M11 + value1.M42 * value2.M21 + value1.M43 * value2.M31 + value1.M44 * value2.M41
        result.M42 = value1.M41 * value2.M12 + value1.M42 * value2.M22 + value1.M43 * value2.M32 + value1.M44 * value2.M42
        result.M43 = value1.M41 * value2.M13 + value1.M42 * value2.M23 + value1.M43 * value2.M33 + value1.M44 * value2.M43
        result.M44 = value1.M41 * value2.M14 + value1.M42 * value2.M24 + value1.M43 * value2.M34 + value1.M44 * value2.M44

        return result:__clone__()
    end
end

Matrix4x4.op_UnaryNegation = function (value)
    local m = new(Matrix4x4)

    m.M11 = - value.M11
    m.M12 = - value.M12
    m.M13 = - value.M13
    m.M14 = - value.M14
    m.M21 = - value.M21
    m.M22 = - value.M22
    m.M23 = - value.M23
    m.M24 = - value.M24
    m.M31 = - value.M31
    m.M32 = - value.M32
    m.M33 = - value.M33
    m.M34 = - value.M34
    m.M41 = - value.M41
    m.M42 = - value.M42
    m.M43 = - value.M43
    m.M44 = - value.M44

    return m:__clone__()
end

Matrix4x4.op_Addition = function (value1, value2)
    local m = new(Matrix4x4)

    m.M11 = value1.M11 + value2.M11
    m.M12 = value1.M12 + value2.M12
    m.M13 = value1.M13 + value2.M13
    m.M14 = value1.M14 + value2.M14
    m.M21 = value1.M21 + value2.M21
    m.M22 = value1.M22 + value2.M22
    m.M23 = value1.M23 + value2.M23
    m.M24 = value1.M24 + value2.M24
    m.M31 = value1.M31 + value2.M31
    m.M32 = value1.M32 + value2.M32
    m.M33 = value1.M33 + value2.M33
    m.M34 = value1.M34 + value2.M34
    m.M41 = value1.M41 + value2.M41
    m.M42 = value1.M42 + value2.M42
    m.M43 = value1.M43 + value2.M43
    m.M44 = value1.M44 + value2.M44

    return m:__clone__()
end

Matrix4x4.op_Subtraction = function (value1, value2)
    local m = new(Matrix4x4)

    m.M11 = value1.M11 - value2.M11
    m.M12 = value1.M12 - value2.M12
    m.M13 = value1.M13 - value2.M13
    m.M14 = value1.M14 - value2.M14
    m.M21 = value1.M21 - value2.M21
    m.M22 = value1.M22 - value2.M22
    m.M23 = value1.M23 - value2.M23
    m.M24 = value1.M24 - value2.M24
    m.M31 = value1.M31 - value2.M31
    m.M32 = value1.M32 - value2.M32
    m.M33 = value1.M33 - value2.M33
    m.M34 = value1.M34 - value2.M34
    m.M41 = value1.M41 - value2.M41
    m.M42 = value1.M42 - value2.M42
    m.M43 = value1.M43 - value2.M43
    m.M44 = value1.M44 - value2.M44

    return m:__clone__()
end

Matrix4x4.op_Multiply = function(value1, value2)
    if value2.M11 == nil then
        -- scalar
        local result = new(Matrix4x4)

        result.M11 = value1.M11 * value2
        result.M12 = value1.M12 * value2
        result.M13 = value1.M13 * value2
        result.M14 = value1.M14 * value2
        result.M21 = value1.M21 * value2
        result.M22 = value1.M22 * value2
        result.M23 = value1.M23 * value2
        result.M24 = value1.M24 * value2
        result.M31 = value1.M31 * value2
        result.M32 = value1.M32 * value2
        result.M33 = value1.M33 * value2
        result.M34 = value1.M34 * value2
        result.M41 = value1.M41 * value2
        result.M42 = value1.M42 * value2
        result.M43 = value1.M43 * value2
        result.M44 = value1.M44 * value2

        return result:__clone__()
    else
        -- matrix
        local result = new(Matrix4x4)

        -- First row
        result.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21 + value1.M13 * value2.M31 + value1.M14 * value2.M41
        result.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22 + value1.M13 * value2.M32 + value1.M14 * value2.M42
        result.M13 = value1.M11 * value2.M13 + value1.M12 * value2.M23 + value1.M13 * value2.M33 + value1.M14 * value2.M43
        result.M14 = value1.M11 * value2.M14 + value1.M12 * value2.M24 + value1.M13 * value2.M34 + value1.M14 * value2.M44

        -- Second row
        result.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21 + value1.M23 * value2.M31 + value1.M24 * value2.M41
        result.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22 + value1.M23 * value2.M32 + value1.M24 * value2.M42
        result.M23 = value1.M21 * value2.M13 + value1.M22 * value2.M23 + value1.M23 * value2.M33 + value1.M24 * value2.M43
        result.M24 = value1.M21 * value2.M14 + value1.M22 * value2.M24 + value1.M23 * value2.M34 + value1.M24 * value2.M44

        -- Third row
        result.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value1.M33 * value2.M31 + value1.M34 * value2.M41
        result.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value1.M33 * value2.M32 + value1.M34 * value2.M42
        result.M33 = value1.M31 * value2.M13 + value1.M32 * value2.M23 + value1.M33 * value2.M33 + value1.M34 * value2.M43
        result.M34 = value1.M31 * value2.M14 + value1.M32 * value2.M24 + value1.M33 * value2.M34 + value1.M34 * value2.M44

        -- Fourth row
        result.M41 = value1.M41 * value2.M11 + value1.M42 * value2.M21 + value1.M43 * value2.M31 + value1.M44 * value2.M41
        result.M42 = value1.M41 * value2.M12 + value1.M42 * value2.M22 + value1.M43 * value2.M32 + value1.M44 * value2.M42
        result.M43 = value1.M41 * value2.M13 + value1.M42 * value2.M23 + value1.M43 * value2.M33 + value1.M44 * value2.M43
        result.M44 = value1.M41 * value2.M14 + value1.M42 * value2.M24 + value1.M43 * value2.M34 + value1.M44 * value2.M44

        return result:__clone__()
    end
end

Matrix4x4.op_Equality = function (value1, value2)
    return (value1.M11 == value2.M11 and value1.M22 == value2.M22 and value1.M33 == value2.M33 and value1.M44 == value2.M44 and value1.M12 == value2.M12 and value1.M13 == value2.M13 and value1.M14 == value2.M14 and value1.M21 == value2.M21 and value1.M23 == value2.M23 and value1.M24 == value2.M24 and value1.M31 == value2.M31 and value1.M32 == value2.M32 and value1.M34 == value2.M34 and value1.M41 == value2.M41 and value1.M42 == value2.M42 and value1.M43 == value2.M43)
end

Matrix4x4.op_Inequality = function (value1, value2)
    return (value1.M11 ~= value2.M11 or value1.M12 ~= value2.M12 or value1.M13 ~= value2.M13 or value1.M14 ~= value2.M14 or value1.M21 ~= value2.M21 or value1.M22 ~= value2.M22 or value1.M23 ~= value2.M23 or value1.M24 ~= value2.M24 or value1.M31 ~= value2.M31 or value1.M32 ~= value2.M32 or value1.M33 ~= value2.M33 or value1.M34 ~= value2.M34 or value1.M41 ~= value2.M41 or value1.M42 ~= value2.M42 or value1.M43 ~= value2.M43 or value1.M44 ~= value2.M44)
end

Matrix4x4.Equals = function (this, other)
    if System.is(other, Matrix4x4) then
        return (this.M11 == other.M11 and this.M22 == other.M22 and this.M33 == other.M33 and this.M44 == other.M44 and this.M12 == other.M12 and this.M13 == other.M13 and this.M14 == other.M14 and this.M21 == other.M21 and this.M23 == other.M23 and this.M24 == other.M24 and this.M31 == other.M31 and this.M32 == other.M32 and this.M34 == other.M34 and this.M41 == other.M41 and this.M42 == other.M42 and this.M43 == other.M43)
    end
    return false
end

Matrix4x4.ToString = function (this)
    local sb = System.StringBuilder()
    sb:Append("{ ")
    sb:Append("{")
    sb:Append("M11: ")
    sb:Append(this.M11:ToString())
    sb:Append(" M12: ")
    sb:Append(this.M12:ToString())
    sb:Append(" M13: ")
    sb:Append(this.M13:ToString())
    sb:Append(" M14: ")
    sb:Append(this.M14:ToString())
    sb:Append("} ")
    sb:Append("{")
    sb:Append("M21: ")
    sb:Append(this.M21:ToString())
    sb:Append(" M22: ")
    sb:Append(this.M22:ToString())
    sb:Append(" M23: ")
    sb:Append(this.M23:ToString())
    sb:Append(" M24: ")
    sb:Append(this.M24:ToString())
    sb:Append("} ")
    sb:Append("{")
    sb:Append("M31: ")
    sb:Append(this.M31:ToString())
    sb:Append(" M32: ")
    sb:Append(this.M32:ToString())
    sb:Append(" M33: ")
    sb:Append(this.M33:ToString())
    sb:Append(" M34: ")
    sb:Append(this.M34:ToString())
    sb:Append("} ")
    sb:Append("{")
    sb:Append("M41: ")
    sb:Append(this.M41:ToString())
    sb:Append(" M42: ")
    sb:Append(this.M42:ToString())
    sb:Append(" M43: ")
    sb:Append(this.M43:ToString())
    sb:Append(" M44: ")
    sb:Append(this.M44:ToString())
    sb:Append("} ")
    sb:Append("}")
    return sb:ToString()
end

Matrix4x4.GetHashCode = function (this)
    return this.M11:GetHashCode() + this.M12:GetHashCode() + this.M13:GetHashCode() + this.M14:GetHashCode() + this.M21:GetHashCode() + this.M22:GetHashCode() + this.M23:GetHashCode() + this.M24:GetHashCode() + this.M31:GetHashCode() + this.M32:GetHashCode() + this.M33:GetHashCode() + this.M34:GetHashCode() + this.M41:GetHashCode() + this.M42:GetHashCode() + this.M43:GetHashCode() + this.M44:GetHashCode()
end

-- https://math.stackexchange.com/questions/237369/given-this-transformation-matrix-how-do-i-decompose-it-into-translation-rotati
-- It appears that this function is not complete, as it appears by the comments
-- Improvement is welcome
Matrix4x4.Decompose = function(matrix, scale, rotation, translation)
    -- throw(NotImplementedException("Matrix4x4.Decompose is not yet implemented"))

    -- Extract Translation
    translation = SystemNumerics.Vector3(matrix.M41, matrix.M42, matrix.M43)

    -- Zero Translation
    matrix.M41 = 0
    matrix.M42 = 0
    matrix.M43 = 0

    -- Extract scales

    local sx = SystemNumerics.Vector3(matrix.M11, matrix.M12, matrix.M13):Length()
    local sy = SystemNumerics.Vector3(matrix.M21, matrix.M22, matrix.M23):Length()
    local sz = SystemNumerics.Vector3(matrix.M31, matrix.M32, matrix.M33):Length()

    scale = SystemNumerics.Vector3(sx, sy, sz)

    -- divide by scale

    local matTemp = matrix:__clone__()

    matTemp.M11 = matTemp.M11 / sx
    matTemp.M12 = matTemp.M12 / sx
    matTemp.M13 = matTemp.M13 / sx

    matTemp.M21 = matTemp.M21 / sy
    matTemp.M22 = matTemp.M22 / sy
    matTemp.M23 = matTemp.M23 / sy

    matTemp.M31 = matTemp.M31 / sz
    matTemp.M32 = matTemp.M32 / sz
    matTemp.M33 = matTemp.M33/ sz

    rotation = SystemNumerics.Quaternion.CreateFromRotationMatrix(matTemp)

    return true, scale, rotation, translation
end

System.defStc("System.Numerics.Matrix4x4", Matrix4x4)