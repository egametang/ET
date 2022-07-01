#include "il2cpp-config.h"

#include "il2cpp-class-internals.h"
#include "os/Environment.h"
#include "os/File.h"
#include "os/Image.h"
#include "os/Initialize.h"
#include "os/LibraryLoader.h"
#include "os/Locale.h"
#include "os/Path.h"
#include "NativeSymbol.h"
#include "utils/Collections.h"
#include "utils/PathUtils.h"
#include "utils/MemoryMappedFile.h"
#include "utils/Runtime.h"
#include <string>
#include <cstdlib>

namespace il2cpp
{
namespace utils
{
#if IL2CPP_ENABLE_NATIVE_STACKTRACES

    static Il2CppMethodPointer MaskSpareBits(const Il2CppMethodPointer method)
    {
        return (Il2CppMethodPointer)((size_t)method & ~IL2CPP_POINTER_SPARE_BITS);
    }

    struct MethodInfoToMethodPointerConverter
    {
        Il2CppMethodPointer operator()(const MethodDefinitionKey& methodInfo) const
        {
            return MaskSpareBits(methodInfo.method);
        }
    };

    typedef il2cpp::utils::collections::ArrayValueMap<Il2CppMethodPointer, MethodDefinitionKey, MethodInfoToMethodPointerConverter> NativeMethodMap;
    static NativeMethodMap s_NativeMethods;

    struct NativeSymbolMutator
    {
        void operator()(MethodDefinitionKey* method)
        {
            // So, when a function is marked as noreturn, some compilers emit a call to that function and then
            // put the next function immediately after call instruction which means the return address on the stack
            // will appear to point to the wrong function. This messes up our stack walking as we now are confused
            // which method is actually on the stack. To work around this, we add "1" to each of the function addresses,
            // so each function appears to start 1 byte later which means the address on the stack will appear as if
            // it is pointing to function that called the no return function. This is okay because no function will
            // ever return to the first byte of another function.
            method->method = reinterpret_cast<Il2CppMethodPointer>(reinterpret_cast<intptr_t>(method->method) + 1);
        }
    };

    void NativeSymbol::RegisterMethods(const std::vector<MethodDefinitionKey>& managedMethods)
    {
        s_NativeMethods.assign(managedMethods);

#if IL2CPP_MUTATE_METHOD_POINTERS
        NativeSymbolMutator mutator;
        s_NativeMethods.mutate(mutator);
#endif
    }

#pragma pack(push, p1, 4)
    struct SymbolInfo
    {
        uint64_t address;
        uint32_t length;
    };
#pragma pack(pop, p1)

    static int32_t s_SymbolCount;
    static SymbolInfo* s_SymbolInfos;
    static void* s_ImageBase;

#if !RUNTIME_TINY
    static void* LoadSymbolInfoFileFrom(const std::string& path)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(path, kFileModeOpen, kFileAccessRead, kFileShareRead, kFileOptionsNone, &error);

        if (error != 0)
            return NULL;

        // Note that we won't unmap this file, we'll leave it open the entire lifetime of the process.
        void* mappedFile = utils::MemoryMappedFile::Map(handle);

        il2cpp::os::File::Close(handle, &error);
        IL2CPP_ASSERT(error == 0);

        return mappedFile;
    }

#endif //!RUNTIME_TINY

    static void* LoadSymbolInfoFile()
    {
#if RUNTIME_TINY
        return NULL;
#else
#if IL2CPP_TARGET_ANDROID
    #if defined(__i386__)
        std::string symbolMapFileName = "SymbolMap-x86";
    #elif defined(__arm__)
        std::string symbolMapFileName = "SymbolMap-ARMv7";
    #elif defined(__aarch64__)
        std::string symbolMapFileName = "SymbolMap-ARM64";
    #elif defined(__x86_64__)
        std::string symbolMapFileName = "SymbolMap-x86_64";
    #else
        #error Unknown symbol map file name
    #endif
#else
    #if !IL2CPP_CAN_USE_MULTIPLE_SYMBOL_MAPS
        std::string symbolMapFileName = "SymbolMap";
    #elif IL2CPP_SIZEOF_VOID_P == 4
        std::string symbolMapFileName = "SymbolMap-32";
    #elif IL2CPP_SIZEOF_VOID_P == 8
        std::string symbolMapFileName = "SymbolMap-64";
    #else
        #error Unknown symbol map file name
    #endif
#endif

        void* result = LoadSymbolInfoFileFrom(il2cpp::utils::PathUtils::Combine(il2cpp::utils::PathUtils::DirectoryName(il2cpp::os::Path::GetExecutablePath()), symbolMapFileName));
        if (result != NULL)
            return result;

        return LoadSymbolInfoFileFrom(il2cpp::utils::PathUtils::Combine(utils::Runtime::GetDataDir(), symbolMapFileName));
#endif
    }

    static void InitializeSymbolInfos()
    {
        s_ImageBase = il2cpp::os::Image::GetImageBase();

        if (!il2cpp::os::Image::ManagedSectionExists())
        {
            void* fileBuffer = LoadSymbolInfoFile();
            if (fileBuffer == NULL)
                return;

            s_SymbolCount = *((int32_t*)fileBuffer);
            s_SymbolInfos = (SymbolInfo*)((uint8_t*)fileBuffer + sizeof(s_SymbolCount));
        }
    }

    static bool CompareEndOfSymbols(const SymbolInfo &a, const SymbolInfo &b)
    {
        return a.address + a.length < b.address + b.length;
    }

    static SymbolInfo* FindSymbolInfoForNativeMethod(Il2CppMethodPointer nativeMethod)
    {
        SymbolInfo* end = s_SymbolInfos + s_SymbolCount;

        // our 'key' could be anywhere within a symbol. Our comparison function compares the end address
        // of the symbols. By doing this, upper bound returns the first symbol whose end address is greater
        // than our 'key'. This is our symbol since our end is the first end above an interior value.
        SymbolInfo interiorSymbol = { (size_t)((char*)nativeMethod - (char*)s_ImageBase), 0 };
        SymbolInfo* containingSymbol = std::upper_bound(s_SymbolInfos, end, interiorSymbol, &CompareEndOfSymbols);

        if (containingSymbol == end)
            return NULL;

        // We only include managed methods in the symbol data. A lookup for a native method might find the
        // previous or next managed method in the data. This will be incorrect, so check the start and the size,
        // to make sure the interior symbol is really within the method found in the containing symbol.
        if ((interiorSymbol.address != containingSymbol->address) &&
            ((interiorSymbol.address < containingSymbol->address) ||
             (interiorSymbol.address - containingSymbol->address > containingSymbol->length)))
            return NULL;

        return containingSymbol;
    }

    static bool s_TriedToInitializeSymbolInfo = false;

    static bool IsInstructionPointerProbablyInManagedMethod(Il2CppMethodPointer managedMethodStart, Il2CppMethodPointer instructionPointer)
    {
        const int probableMaximumManagedMethodSizeInBytes = 5000;
        if (std::abs((intptr_t)managedMethodStart - (intptr_t)instructionPointer) < probableMaximumManagedMethodSizeInBytes)
            return true;

        return false;
    }

    const VmMethod* NativeSymbol::GetMethodFromNativeSymbol(Il2CppMethodPointer nativeMethod)
    {
        if (!s_TriedToInitializeSymbolInfo)
        {
            // Only attempt to initialize the symbol information once. If it is not present the first time,
            // it likely won't be there later either. Repeated checking can cause performance problems.
            s_TriedToInitializeSymbolInfo = true;
            InitializeSymbolInfos();
        }

        // address has to be above our base address
        if ((void*)nativeMethod < (void*)s_ImageBase)
            return NULL;

        if (il2cpp::os::Image::ManagedSectionExists())
        {
            if (!il2cpp::os::Image::IsInManagedSection((void*)nativeMethod))
                return NULL;
        }

        if (s_SymbolCount > 0)
        {
            SymbolInfo* containingSymbol = FindSymbolInfoForNativeMethod(nativeMethod);
            if (containingSymbol == NULL)
                return NULL;

            nativeMethod = (Il2CppMethodPointer)((char*)s_ImageBase + containingSymbol->address);

            // We can't assume that the map file is aligned.
            // We must use the same masking/no masking logic used to insert into the data structure for the lookup.
            // If we don't, the find will try to look up unmasked in a table full of masked values.

            // do exact lookup based on the symbol start address, as that is our key
            NativeMethodMap::iterator iter = s_NativeMethods.find_first(MaskSpareBits(nativeMethod));
            if (iter != s_NativeMethods.end())
            {
                return IL2CPP_VM_METHOD_METADATA_FROM_METHOD_KEY(iter);
            }
        }
        else
        {
            // Get the first symbol greater than the one we want, because our instruction pointer
            // probably won't be at the start of the method.
            NativeMethodMap::iterator methodAfterNativeMethod = s_NativeMethods.upper_bound(nativeMethod);

            // If method are all of the managed methods are in the same custom section of the binary, then assume we
            // will find the proper method, so the end iterator means we found the last method in this list. If we
            // don't have custom sections, then we may have actually not found the method. In that case, let's not
            // return a method we are unsure of.
            if (!il2cpp::os::Image::ManagedSectionExists())
            {
                if (methodAfterNativeMethod == s_NativeMethods.end())
                    return NULL;

                if (!IsInstructionPointerProbablyInManagedMethod(methodAfterNativeMethod->method, nativeMethod))
                    return NULL;
            }

            // Go back one to get the method we actually want.
            if (methodAfterNativeMethod != s_NativeMethods.begin())
                methodAfterNativeMethod--;

            return IL2CPP_VM_METHOD_METADATA_FROM_METHOD_KEY(methodAfterNativeMethod);
        }

        return NULL;
    }

    bool NativeSymbol::GetMethodDebugInfo(const MethodInfo *method, Il2CppMethodDebugInfo* methodDebugInfo)
    {
        Il2CppMethodPointer nativeMethod = method->methodPointer;

        if (il2cpp::os::Image::ManagedSectionExists())
        {
            if (!il2cpp::os::Image::IsInManagedSection((void*)nativeMethod))
                return false;
        }

        int32_t codeSize = 0;
        if (s_SymbolCount > 0)
        {
            SymbolInfo* containingSymbol = FindSymbolInfoForNativeMethod(nativeMethod);
            if (containingSymbol == NULL)
                return false;

            codeSize = containingSymbol->length;
        }

        if (methodDebugInfo != NULL)
        {
            methodDebugInfo->methodPointer = method->methodPointer;
            methodDebugInfo->code_size = codeSize;
            methodDebugInfo->file = NULL;
        }

        return true;
    }

#endif
} /* namespace utils */
} /* namespace il2cpp */
