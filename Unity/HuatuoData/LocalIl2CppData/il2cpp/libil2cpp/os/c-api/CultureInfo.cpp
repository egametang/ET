#include "os/c-api/il2cpp-config-platforms.h"

#if IL2CPP_TINY

#include "os/Locale.h"
#include "os/MarshalAlloc.h"
#include <cstring>

extern "C"
{
    char* STDCALL UnityPalGetCurrentLocaleName()
    {
        auto locale = il2cpp::os::Locale::GetLocale();
        char* allocatedLocal = (char*)il2cpp::os::MarshalAlloc::Allocate(locale.size() + 1);
        strcpy(allocatedLocal, locale.c_str());
        return allocatedLocal;
    }
}

#endif
