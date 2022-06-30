#include "il2cpp-config.h"
#include "utils/Memory.h"

#if (IL2CPP_TARGET_JAVASCRIPT || IL2CPP_TARGET_LINUX || IL2CPP_TARGET_LUMIN && !RUNTIME_TINY) || IL2CPP_TARGET_ANDROID

#include "os/Image.h"

#if IL2CPP_TARGET_JAVASCRIPT
#include <emscripten/emscripten.h>
#else
#include <dlfcn.h>
#endif

#if IL2CPP_ENABLE_NATIVE_INSTRUCTION_POINTER_EMISSION && (IL2CPP_TARGET_ANDROID || IL2CPP_TARGET_LINUX)
#include <elf.h>

#if __x86_64__ || __aarch64__
#  define ElfW(type) Elf64_##type
#else
#  define ElfW(type) Elf32_##type
#endif

#endif

#include <cstdio>

extern char __start_il2cpp;
extern char __stop_il2cpp;

namespace il2cpp
{
namespace os
{
namespace Image
{
    void* GetImageBase()
    {
#if IL2CPP_TARGET_JAVASCRIPT
        emscripten_log(EM_LOG_NO_PATHS | EM_LOG_CONSOLE | EM_LOG_ERROR | EM_LOG_JS_STACK, "Warning: libil2cpp/os/Posix/Image.cpp: GetImageBase() called, but dynamic libraries are not available.");
        return NULL;
#else
        Dl_info info;
        void* const anySymbol = reinterpret_cast<void*>(&GetImageBase);
        if (dladdr(anySymbol, &info))
            return info.dli_fbase;
        else
            return NULL;
#endif
    }

    static IL2CPP_METHOD_ATTR void NoGeneratedCodeWorkaround()
    {
    }

    void InitializeManagedSection()
    {
        NoGeneratedCodeWorkaround();
        // Since the native linker creates the __start_il2cpp and __stop_il2cpp
        // globals, we can only use them when IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS
        // is defined. Otherwise, they will not exist, and this usage of them will cause
        // an unresolved external error in the native linker. This should be the only
        // place in runtime code that IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS is used.
#if IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS
        SetManagedSectionStartAndEnd(&__start_il2cpp, &__stop_il2cpp);
#endif
    }

    void Initialize()
    {
        InitializeManagedSection();
    }

#if IL2CPP_ENABLE_NATIVE_INSTRUCTION_POINTER_EMISSION
#if IL2CPP_TARGET_ANDROID || IL2CPP_TARGET_LINUX
    char* GetELFImageBuildID()
    {
        size_t imageBase = (size_t)GetImageBase();
        ElfW(Ehdr) * ehdr = (ElfW(Ehdr) *)imageBase;
        ElfW(Phdr) * phdr = (ElfW(Phdr) *)(imageBase + ehdr->e_phoff);

        for (int i = 0; i < ehdr->e_phnum; i++)
        {
            if (phdr[i].p_type == PT_NOTE)
            {
                size_t nhdr_ptr = phdr[i].p_offset + imageBase;
                int j = 0;
                while (nhdr_ptr < imageBase + phdr[i].p_offset + phdr[i].p_memsz)
                {
                    ElfW(Nhdr) * nhdr = (ElfW(Nhdr) *)nhdr_ptr;
                    if (nhdr->n_type == NT_GNU_BUILD_ID)
                    {
                        uint8_t* image_build_id = (uint8_t *)((size_t)nhdr + sizeof(ElfW(Nhdr)) + nhdr->n_namesz);
                        char* build_id = static_cast<char*>(IL2CPP_MALLOC(41));
                        for (int j = 0; j < nhdr->n_descsz; j++)
                        {
                            snprintf(&build_id[j * 2], 3, "%02x", image_build_id[j]);
                        }
                        return build_id;
                    }
                    nhdr_ptr += sizeof(ElfW(Nhdr)) + nhdr->n_descsz + nhdr->n_namesz;
                }
                break;
            }
        }

        return NULL;
    }

#endif

    char* GetImageUUID()
    {
#if IL2CPP_TARGET_ANDROID || IL2CPP_TARGET_LINUX
        return GetELFImageBuildID();
#else
#error Implement GetImageUUID for this platform
#endif
    }

    char* GetImageName()
    {
#if IL2CPP_TARGET_ANDROID || IL2CPP_TARGET_LINUX
        Dl_info info;
        void* const anySymbol = reinterpret_cast<void*>(&GetImageBase);
        if (dladdr(anySymbol, &info))
        {
            size_t nameSize = strlen(info.dli_fname);
            char* imageName = (char*)IL2CPP_MALLOC(nameSize);
            strncpy(imageName, info.dli_fname, nameSize);
            return imageName;
        }
        else
            return NULL;
#else
#error Implement GetImageName for this platform
#endif
    }

#endif
}
}
}

#endif
