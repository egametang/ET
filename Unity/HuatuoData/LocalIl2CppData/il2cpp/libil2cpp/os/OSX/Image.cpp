#include "il2cpp-config.h"
#include "utils/Memory.h"

#if IL2CPP_TARGET_DARWIN

#include "os/Image.h"

#include <mach-o/dyld.h>
#include <mach-o/getsect.h>
#include <mach-o/ldsyms.h>
#include <dlfcn.h>
#include <vector>

namespace il2cpp
{
namespace os
{
namespace Image
{
    static void* s_ImageBase = NULL;

    static int GetImageIndex()
    {
        // OSX/iOS uses ASLR (address space layout randomization), so
        // find where the image is loaded. This is usually zero in the
        // debugger for an executable, but non-zero when a debugger is not
        // present.
        std::vector<char> path;
        uint32_t size = 0;
        int error = _NSGetExecutablePath(NULL, &size);
        IL2CPP_ASSERT(error == -1);
        if (error != -1)
            return -1;

        path.resize(size);
        error = _NSGetExecutablePath(&path[0], &size);
        IL2CPP_ASSERT(error == 0);
        if (error != 0)
            return -1;

        int gameAssemblyImageIndex = -1;
        int executableImageIndex = -1;
        int numberOfImages = _dyld_image_count();
        for (uint32_t i = 0; i < numberOfImages; i++)
        {
            const char* imageName = _dyld_get_image_name(i);
            if (strstr(imageName, "GameAssembly.dylib") != NULL || strstr(imageName, "UnityFramework.framework/UnityFramework") != NULL)
            {
                gameAssemblyImageIndex = i;
                break;
            }
            else if (strcmp(imageName, &path[0]) == 0)
            {
                executableImageIndex = i;
            }
        }

        if (gameAssemblyImageIndex != -1)
            return gameAssemblyImageIndex;
        else if (executableImageIndex != -1)
            return executableImageIndex;

        return -1;
    }

    static void InitializeImageBase()
    {
        // Gets info about the image containing InitializeImageBase
        Dl_info info;
        memset(&info, 0, sizeof(info));
        int error = dladdr((void*)&InitializeImageBase, &info);

        IL2CPP_ASSERT(error != 0);
        if (error == 0)
            return;

        s_ImageBase = info.dli_fbase;
    }

#if IL2CPP_SIZEOF_VOID_P == 8
    typedef section_64 archSectionData_t;
#else
    typedef section archSectionData_t;
#endif

    static const archSectionData_t* SectionDataFor(int imageIndex)
    {
        const struct mach_header* header = _dyld_get_image_header(imageIndex);
#if IL2CPP_SIZEOF_VOID_P == 8
        return getsectbynamefromheader_64((const struct mach_header_64*)header, "__TEXT", IL2CPP_BINARY_SECTION_NAME);
#else
        return getsectbynamefromheader(header, "__TEXT", IL2CPP_BINARY_SECTION_NAME);
#endif
    }

    static void InitializeManagedSection()
    {
        int imageIndex = GetImageIndex();
        if (imageIndex == -1)
            return;

        const archSectionData_t* sectionData = SectionDataFor(imageIndex);
        if (sectionData == NULL)
        {
            // We did not find the managed section of the binary in the image where we
            // think it should live. Maybe Unity is being embedded in another application.
            // Let's search all of the images and find the first one that has our section
            // in the binary.
            //
            // This won't work correctly if more than one Unity binary is embedded, but we
            // don't support that case yet.
            int numberOfImages = _dyld_image_count();
            imageIndex = 0;
            while (sectionData == NULL && imageIndex < numberOfImages)
            {
                sectionData = SectionDataFor(imageIndex);
                imageIndex++;
            }
        }

        if (sectionData != NULL)
        {
            void* start = (void*)((intptr_t)sectionData->offset + (intptr_t)s_ImageBase);
            void* end = (uint8_t*)start + sectionData->size;

            SetManagedSectionStartAndEnd(start, end);
        }
    }

    void Initialize()
    {
        InitializeImageBase();
        InitializeManagedSection();
    }

    void* GetImageBase()
    {
        return s_ImageBase;
    }

#if IL2CPP_ENABLE_NATIVE_INSTRUCTION_POINTER_EMISSION
    char* GetImageUUID()
    {
        const struct mach_header_64* header = (mach_header_64*)_dyld_get_image_header(GetImageIndex());
        const uint8_t *command = (const uint8_t *)(header + 1);

        for (uint32_t idx = 0; idx < header->ncmds; ++idx)
        {
            if (((const struct load_command *)command)->cmd == LC_UUID)
            {
                command += sizeof(struct load_command);
                char* uuid = static_cast<char*>(IL2CPP_MALLOC(33));
                snprintf(uuid, 33, "%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x",
                    command[0], command[1], command[2], command[3],
                    command[4], command[5], command[6], command[7],
                    command[8], command[9], command[10], command[11],
                    command[12], command[13], command[14], command[15]);
                return uuid;
            }
            else
            {
                command += ((const struct load_command *)command)->cmdsize;
            }
        }

        return NULL;
    }

    char* GetImageName()
    {
        const char* imageName = _dyld_get_image_name(GetImageIndex());
        size_t nameSize = strlen(imageName);
        char* imageNameCopy = (char*)IL2CPP_MALLOC(nameSize);
        strncpy(imageNameCopy, imageName, nameSize);
        return imageNameCopy;
    }

#endif
}
}
}

#endif
