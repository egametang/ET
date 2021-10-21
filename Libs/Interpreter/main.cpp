#include "main.h"
#include <stdio.h>
#include <stdarg.h>
#include <mono\jit\jit.h>
#include <mono/metadata/environment.h>
#include <mono/utils/mono-publib.h>
#include <mono/utils/mono-logger.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/exception.h>

void(*log)(const char* buf, int len);

void interpreter_log(const char* fmt, ...)
{
    if (log == 0)
    {
        return;
    }

    char buffer[1024];
    va_list argptr;
    va_start(argptr, fmt);
    int n = vsprintf_s(buffer, fmt, argptr);
    va_end(argptr);
    log(buffer, n);
}

void interpreter_set_log(void(*plog)(const char* buf, int len))
{
    log = plog;
}

void interpreter_init(const char* bundleDir, const char* dllName)
{
    mono_set_dirs(bundleDir, bundleDir);
    interpreter_log("1111111111111111111111 %s  %s", bundleDir, dllName);
    //MonoDomain* domain = mono_jit_init(dllName);
}
