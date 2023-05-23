
export class Vector2 {
    static deserializeFromJson(json: any): Vector2 {
        let x = json.x
        let y = json.y
        if (x == null || y == null) {
            throw new Error()
        }
        return new Vector2(x, y)
    }

    x: number
    y: number
    constructor(x: number = 0, y: number = 0) {
        this.x = x
        this.y = y
    }
}

export class Vector3 {
    static deserializeFromJson(json: any): Vector3 {
        let x = json.x
        let y = json.y
        let z = json.z
        if (x == null || y == null || z == null) {
            throw new Error()
        }
        return new Vector3(x, y, z)
    }

    x: number
    y: number
    z: number

    constructor(x: number = 0, y: number = 0, z: number = 0) {
        this.x = x
        this.y = y
        this.z = z
    }
}

export class Vector4 {
    static deserializeFromJson(json: any): Vector4 {
        let x = json.x
        let y = json.y
        let z = json.z
        let w = json.w
        if (x == null || y == null || z == null || w == null) {
            throw new Error()
        }
        return new Vector4(x, y, z, w)
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
}
