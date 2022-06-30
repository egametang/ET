#include <stdarg.h>
#include <cstdio>
#include <cassert>
#include "Logging.h"
#include "Output.h"

using namespace il2cpp::utils;

static void DefaultLogCallback(const char* message)
{
    Output::WriteToStdout(message);
    Output::WriteToStdout("\n");
}

Il2CppLogCallback Logging::s_Callback = DefaultLogCallback;

void Logging::Write(const char* format, ...)
{
    IL2CPP_ASSERT(s_Callback != NULL);

    if (format == NULL)
        return;

    va_list va;
    va_start(va, format);

    const char* prefix = "[libil2cpp] ";
    const int bufferSize = 1024 * 5;
    char buffer[bufferSize];
    memcpy(buffer, prefix, 12);
    vsnprintf(buffer + 12, bufferSize - 12, format, va);

    s_Callback(buffer);

    va_end(va);
}

void Logging::SetLogCallback(Il2CppLogCallback method)
{
    IL2CPP_ASSERT(method != NULL);
    s_Callback = method;
}

bool Logging::IsLogCallbackSet()
{
    return s_Callback != DefaultLogCallback;
}
