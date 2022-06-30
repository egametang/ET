#include "il2cpp-config.h"
#include "Allocator.h"

static allocate_func s_Allocator;

extern "C"
{
    void register_allocator(allocate_func allocator)
    {
        s_Allocator = allocator;
    }
}

void* Allocator::Allocate(size_t size)
{
    IL2CPP_ASSERT(s_Allocator);
    return s_Allocator(size);
}

char* Allocator::CopyToAllocatedStringBuffer(const std::string& input)
{
    size_t size = input.size();
    char* buffer = (char*)Allocator::Allocate(size + 1);
    input.copy(buffer, size);
    buffer[size] = '\0';
    return buffer;
}

char* Allocator::CopyToAllocatedStringBuffer(const char* input)
{
    size_t size = strlen(input);
    char* buffer = (char*)Allocator::Allocate(size + 1);
    strcpy(buffer, input);
    return buffer;
}

void Allocator::CopyStringVectorToNullTerminatedArray(const std::vector<std::string>& input, void*** output)
{
    if (output != NULL)
    {
        size_t numberOfAddresses = input.size();
        *output = (void**)Allocate(sizeof(void*) * (numberOfAddresses + 1));
        for (size_t i = 0; i < numberOfAddresses; ++i)
            (*output)[i] = CopyToAllocatedStringBuffer(input[i].c_str());

        (*output)[numberOfAddresses] = NULL;
    }
}

void Allocator::CopyDataVectorToNullTerminatedArray(const std::vector<void*>& input, void*** output, int32_t elementSize)
{
    if (output != NULL)
    {
        size_t numberOfEntries = input.size();
        *output = (void**)Allocate(sizeof(void*) * (numberOfEntries + 1));
        for (size_t i = 0; i < numberOfEntries; ++i)
        {
            (*output)[i] = (void*)Allocate(elementSize);
            memcpy((*output)[i], input[i], elementSize);
        }

        (*output)[numberOfEntries] = NULL;
    }
}
