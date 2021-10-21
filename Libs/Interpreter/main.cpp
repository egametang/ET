#include "main.h"
#include <stdio.h>
#include <mono\jit\jit.h>

void InterpreterInit(char* bundleDir, const char* dllName)
{
    MonoDomain* domain = mono_jit_init(dllName);
}
