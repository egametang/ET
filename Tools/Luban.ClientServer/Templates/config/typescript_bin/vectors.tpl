
export class Vector2 implements ISerializable  {

    static deserializeFrom(buf: ByteBuf): Vector2 {
        var v = new Vector2()
        v.deserialize(buf)
        return v
    }

    x: number
    y: number
    constructor(x: number = 0, y: number = 0) {
        this.x = x
        this.y = y
    }

    serialize(_buf_: ByteBuf) {
        _buf_.WriteFloat(this.x)
        _buf_.WriteFloat(this.y)
    }

    deserialize(buf: ByteBuf) {
        this.x = buf.ReadFloat()
        this.y = buf.ReadFloat()
    }
}

export class Vector3 implements ISerializable{
    static deserializeFrom(buf: ByteBuf): Vector3 {
        var v = new Vector3()
        v.deserialize(buf)
        return v
    }

    x: number
    y: number
    z: number

    constructor(x: number = 0, y: number = 0, z: number = 0) {
        this.x = x
        this.y = y
        this.z = z
    }

    serialize(_buf_: ByteBuf) {
        _buf_.WriteFloat(this.x)
        _buf_.WriteFloat(this.y)
        _buf_.WriteFloat(this.z)
    }

    deserialize(buf: ByteBuf) {
        this.x = buf.ReadFloat()
        this.y = buf.ReadFloat()
        this.z = buf.ReadFloat()
    }
}

export class Vector4 implements ISerializable {
    static deserializeFrom(buf: ByteBuf): Vector4 {
        var v = new Vector4()
        v.deserialize(buf)
        return v
    }
    
    x: number
    y: number
    z: number
    w: number

    constructor(x: number = 0, y: number = 0, z: number = 0, w: number = 0) {
        this.x = x
        this.y = y
        this.z = z
        this.w = w
    }

    serialize(_buf_: ByteBuf) {
        _buf_.WriteFloat(this.x)
        _buf_.WriteFloat(this.y)
        _buf_.WriteFloat(this.z)
        _buf_.WriteFloat(this.w)
    }

    deserialize(buf: ByteBuf) {
        this.x = buf.ReadFloat()
        this.y = buf.ReadFloat()
        this.z = buf.ReadFloat()
        this.z = buf.ReadFloat()
    }
}
