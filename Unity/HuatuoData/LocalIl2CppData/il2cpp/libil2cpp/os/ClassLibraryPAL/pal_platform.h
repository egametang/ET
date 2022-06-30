#pragma once

#include "il2cpp-config.h"

#if IL2CPP_TARGET_LINUX
#include "os/Linux/pal_linux.h"
#elif IL2CPP_TARGET_DARWIN
#include "os/OSX/pal_darwin.h"
#elif IL2CPP_TARGET_ANDROID
#include "os/Android/pal_android.h"
#elif IL2CPP_TARGET_JAVASCRIPT
#include "os/Emscripten/pal_emscripten.h"
#elif IL2CPP_TARGET_SWITCH
#include "pal/pal_switch.h"
#elif IL2CPP_TARGET_PS4 || IL2CPP_TARGET_PS5
#include "pal/pal_playstation.h"
#endif
