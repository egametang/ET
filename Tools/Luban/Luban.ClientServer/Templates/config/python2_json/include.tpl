from enum import Enum
import abc

class Vector2:
    def __init__(self, x, y):
        self.x = x
        self.y = y
    def __str__(self):
        return '{%g,%g}' % (self.x, self.y)

    @staticmethod
    def fromJson(_json_):
        x = _json_['x']
        y = _json_['y']
        if (x == None or y == None):
            raise Exception()
        return Vector2(x, y)


class Vector3:
    def __init__(self, x, y, z):
        self.x = x
        self.y = y
        self.z = z
    def __str__(self):
        return '{%f,%f,%f}' % (self.x, self.y, self.z)
    @staticmethod
    def fromJson(_json_):
        x = _json_['x']
        y = _json_['y']
        z = _json_['z']
        if (x == None or y == None or z == None):
            raise Exception()
        return Vector3(x, y, z)

class Vector4:
    def __init__(self, x, y, z, w):
        self.x = x
        self.y = y
        self.z = z
        self.w = w
    def __str__(self):
        return '{%g,%g,%g,%g}' % (self.x, self.y, self.z, self.w)
        
    @staticmethod
    def fromJson(_json_):
        x = _json_['x']
        y = _json_['y']
        z = _json_['z']
        w = _json_['w']
        if (x == None or y == None or z == None or w == None):
            raise Exception()
        return Vector4(x, y, z, w)

