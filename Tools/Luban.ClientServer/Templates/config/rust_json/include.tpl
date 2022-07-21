pub struct LoadError {

}

impl std::fmt::Debug for LoadError {
    fn fmt(&self, _: &mut std::fmt::Formatter<'_>) -> std::result::Result<(), std::fmt::Error> { Ok(()) }
}

#[allow(dead_code)]
pub struct Vector2 {
    pub x:f32,
    pub y:f32,
}

impl Vector2 {
    pub fn new(__js:&json::JsonValue) -> Result<Vector2, LoadError> {
        Ok(Vector2{
            x:  match __js["x"].as_f32() { Some(__x__) => __x__, None => return Err(LoadError{})},
            y: match __js["y"].as_f32() { Some(__x__) => __x__, None => return Err(LoadError{})},
         })
    }
}

#[allow(dead_code)]
pub struct Vector3 {
    pub x:f32,
    pub y:f32,
    pub z:f32,
}

impl Vector3 {
    pub fn new(__js:&json::JsonValue) -> Result<Vector3, LoadError> {
        Ok(Vector3{
            x:  match __js["x"].as_f32() { Some(__x__) => __x__, None => return Err(LoadError{})},
            y: match __js["y"].as_f32() { Some(__x__) => __x__, None => return Err(LoadError{})},
            z: match __js["z"].as_f32() { Some(__x__) => __x__, None => return Err(LoadError{})},
         })
    }
}

#[allow(dead_code)]
pub struct Vector4 {
    pub x:f32,
    pub y:f32,
    pub z:f32,
    pub w:f32,
}


impl Vector4 {
    pub fn new(__js:&json::JsonValue) -> Result<Vector4, LoadError> {
        Ok(Vector4{
            x:  match __js["x"].as_f32() { Some(__x__) => __x__, None => return Err(LoadError{})},
            y: match __js["y"].as_f32() { Some(__x__) => __x__, None => return Err(LoadError{})},
            z: match __js["z"].as_f32() { Some(__x__) => __x__, None => return Err(LoadError{})},
            w: match __js["w"].as_f32() { Some(__x__) => __x__, None => return Err(LoadError{})},
         })
    }
}