#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"

#include "../Memory-c-api.h"
#include "../../Memory.h"

SUITE(Memory)
{
    TEST(Allocate)
    {
        char *memory = (char*)UnityPalAlignedAlloc(1024, sizeof(void*));
        CHECK_NOT_NULL(memory);

        for (int i = 0; i < 1024; ++i)
            memory[i] = 'a';

        for (int i = 0; i < 1024; ++i)
            CHECK_EQUAL('a', memory[i]);

        UnityPalAlignedFree(memory);
    }

    TEST(Reallocate)
    {
        char *memory = (char*)UnityPalAlignedAlloc(1024, sizeof(void*));
        CHECK_NOT_NULL(memory);

        for (int i = 0; i < 1024; ++i)
            memory[i] = 'a';

        for (int i = 0; i < 1024; ++i)
            CHECK_EQUAL('a', memory[i]);

        memory = (char*)UnityPalAlignedReAlloc(memory, 2048, sizeof(void*));
        CHECK_NOT_NULL(memory);

        for (int i = 1024; i < 2048; ++i)
            memory[i] = 'b';

        for (int i = 0; i < 1024; ++i)
            CHECK_EQUAL('a', memory[i]);

        for (int i = 1024; i < 2048; ++i)
            CHECK_EQUAL('b', memory[i]);

        UnityPalAlignedFree(memory);
    }
}

#endif // ENABLE_UNIT_TESTS
