#include "il2cpp-config-mono.h"
/*
    Must be defined in config.h because this option is broken and only works
    In files that include config.h... including it in additional files
    (such as ones that include glib.h and thus eglib-config.h) can cause
    compiler failures because the attribute is properly set and the function is
    called internally when it shouldn't be.
*/
#if defined(USE_MONO_INSIDE_RUNTIME)
    #define MONO_INSIDE_RUNTIME 1
#endif
