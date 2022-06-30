#include "os/Assert.h"

#if IL2CPP_DEBUG

#if IL2CPP_USE_GENERIC_ASSERT

#include <cstdio>
#include <cstdlib>

void il2cpp_assert(const char* assertion, const char* file, unsigned int line)
{
    printf("Assertion failed: %s, file %s, line %u\n", assertion, file, line);
    abort();
}

#endif // IL2CPP_USE_GENERIC_ASSERT

#endif // IL2CPP_DEBUG
