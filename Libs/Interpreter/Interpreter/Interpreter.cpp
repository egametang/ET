#include "Interpreter.h"
#include <stdio.h>
#include <stdarg.h>
#include <string.h>
#include <mono/jit/jit.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/mono-config.h>
#include <mono/utils/mono-publib.h>
#include <mono/utils/mono-logger.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/exception.h>

void(*log)(const char* buf, int len);

void log_format(const char* fmt, ...)
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

void interpreter_log(const char* str)
{
    log_format("%s", str);
}

void interpreter_init(const char* bundleDir, const char* dllName)
{
    mono_config_parse(NULL);
    mono_set_dirs(bundleDir, bundleDir);
    log_format("1111111111111111111111 %s  %s %d", bundleDir, dllName, strlen(bundleDir));
    char str[100];
    memset(str, 0, 100);
    sprintf_s(str, "%s/%s", bundleDir, dllName);
    MonoDomain* monoDomain = mono_jit_init("aaaaa");
    //MonoAssembly* assembly = mono_domain_assembly_open(monoDomain, dllName);
    //MonoImage* monoImage = mono_assembly_get_image(assembly);
    //MonoClass* entityClass = mono_class_from_name(monoImage, "ET", "TestEntry");
    //MonoMethod* processMethod = mono_class_get_method_from_name(entityClass, "Test", 0);
    //
    //void* args[2];
    //args[0] = new int(5);
    //args[1] = new int(100);
    //MonoObject* exception = NULL;
    //MonoObject* result = mono_runtime_invoke(processMethod, nullptr, args, &exception);
}
