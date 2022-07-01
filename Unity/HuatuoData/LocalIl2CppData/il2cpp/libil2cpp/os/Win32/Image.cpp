#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS

#include "os/Image.h"

#include "WindowsHeaders.h"
#include <cstdio>

EXTERN_C IMAGE_DOS_HEADER __ImageBase;

namespace il2cpp
{
namespace os
{
namespace Image
{
    static void InitializeManagedSection()
    {
        PIMAGE_NT_HEADERS ntHeaders = (PIMAGE_NT_HEADERS)(((char*)&__ImageBase) + __ImageBase.e_lfanew);
        PIMAGE_SECTION_HEADER sectionHeader = (PIMAGE_SECTION_HEADER)((char*)&ntHeaders->OptionalHeader + ntHeaders->FileHeader.SizeOfOptionalHeader);
        for (int i = 0; i < ntHeaders->FileHeader.NumberOfSections; i++)
        {
            if (strncmp(IL2CPP_BINARY_SECTION_NAME, (char*)sectionHeader->Name, IMAGE_SIZEOF_SHORT_NAME) == 0)
            {
                void* start = (char*)&__ImageBase + sectionHeader->VirtualAddress;
                void* end = (char*)start + sectionHeader->Misc.VirtualSize;
                SetManagedSectionStartAndEnd(start, end);
            }
            sectionHeader++;
        }
    }

    void Initialize()
    {
        InitializeManagedSection();
    }

    void* GetImageBase()
    {
        return &__ImageBase;
    }

#if IL2CPP_ENABLE_NATIVE_INSTRUCTION_POINTER_EMISSION
    struct PdbInfo
    {
        DWORD     Signature;
        GUID      Guid;
        DWORD     Age;
        char      PdbFileName[1];
    };

    void GetImageUUID(char* uuid)
    {
        uintptr_t base_pointer = (uintptr_t)((char*)&__ImageBase);

        PIMAGE_NT_HEADERS ntHeaders = (PIMAGE_NT_HEADERS)(base_pointer + __ImageBase.e_lfanew);
        IMAGE_DATA_DIRECTORY* dir = &ntHeaders->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_DEBUG];
        IMAGE_DEBUG_DIRECTORY* dbg_dir = (IMAGE_DEBUG_DIRECTORY*)(base_pointer + dir->VirtualAddress);

        if (IMAGE_DEBUG_TYPE_CODEVIEW == dbg_dir->Type)
        {
            PdbInfo* pdb_info = (PdbInfo*)(base_pointer + dbg_dir->AddressOfRawData);

            // Crash reporting expects the GUID without dashes (the format used by symbol servers)
            _snprintf_s(uuid, 41, 40, "%02hhX%02hhX%02hhX%02hhX%02hhX%02hhX%02hhX%02hhX%02hhX%02hhX%02hhX%02hhX%02hhX%02hhX%02hhX%02hhX%X",
                (unsigned char)(pdb_info->Guid.Data1 >> 24),
                (unsigned char)(pdb_info->Guid.Data1 >> 16),
                (unsigned char)(pdb_info->Guid.Data1 >> 8),
                (unsigned char)pdb_info->Guid.Data1,
                (unsigned char)(pdb_info->Guid.Data2 >> 8),
                (unsigned char)pdb_info->Guid.Data2,
                (unsigned char)(pdb_info->Guid.Data3 >> 8),
                (unsigned char)pdb_info->Guid.Data3,
                pdb_info->Guid.Data4[0], pdb_info->Guid.Data4[1], pdb_info->Guid.Data4[2], pdb_info->Guid.Data4[3],
                pdb_info->Guid.Data4[4], pdb_info->Guid.Data4[5], pdb_info->Guid.Data4[6], pdb_info->Guid.Data4[7],
                pdb_info->Age);
        }
    }

#endif
}
}
}

#endif
