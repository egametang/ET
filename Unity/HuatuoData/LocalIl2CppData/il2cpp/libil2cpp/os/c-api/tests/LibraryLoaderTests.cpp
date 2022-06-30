#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"

#include "../LibraryLoader-c-api.h"
#include "../../LibraryLoader.h"
#include "utils/StringUtils.h"
#include "utils/BaselibHandleUtils.h"

#if IL2CPP_TARGET_POSIX && !IL2CPP_TARGET_PS4
#include "dlfcn.h"
#else
#define RTLD_LAZY 0
#endif

#if IL2CPP_TARGET_PS4
static const char* POSIX_DL_NAME = "/app0/test.prx";
static const char* POSIX_FUNC_NAME = "prx_func";
#elif IL2CPP_TARGET_LINUX
static const char* POSIX_DL_NAME = "libm.so.6";
static const char* POSIX_FUNC_NAME = "abs";
#else
static const char* POSIX_DL_NAME = "libm";
static const char* POSIX_FUNC_NAME = "abs";
#endif // IL2CPP_TARGET_PS4
static const char* WINDOWS_DL_NAME = "user32";
static const char* WINDOWS_FUNC_NAME = "CalcMenuBar";

SUITE(LibraryLoader)
{
#if IL2CPP_TARGET_POSIX

    struct PosixLoaderFixture
    {
        PosixLoaderFixture()
        {
            palDynLib = UnityPalLibraryLoaderLoadDynamicLibrary(POSIX_DL_NAME, RTLD_LAZY);
            Il2CppNativeString libName(il2cpp::utils::StringUtils::Utf8ToNativeString(POSIX_DL_NAME));
            std::string detailedError;
            classDynLib = il2cpp::utils::BaselibHandleUtils::HandleToVoidPtr(il2cpp::os::LibraryLoader::LoadDynamicLibrary(STRING_TO_STRINGVIEW(libName), detailedError));
        }

        ~PosixLoaderFixture()
        {
            il2cpp::os::LibraryLoader::CleanupLoadedLibraries();
        }

        void* palDynLib;
        void* classDynLib;
        bool palResult;
        bool classResult;
        Il2CppMethodPointer palMethodPointer;
        Il2CppMethodPointer classMethodPointer;
    };

    TEST_FIXTURE(PosixLoaderFixture, PosixLoadDynamicLibraryReturnsAPointer)
    {
        CHECK_NOT_NULL(palDynLib);
    }

    TEST_FIXTURE(PosixLoaderFixture, PosixCloseLoadedLibraryReturnsTrue)
    {
        CHECK_EQUAL(true, UnityPalLibraryLoaderCloseLoadedLibrary(&palDynLib));
    }

    TEST_FIXTURE(PosixLoaderFixture, PosixGetFunctionPointerisNotNull)
    {
        CHECK_NOT_NULL(UnityPalLibraryLoaderGetFunctionPointer(palDynLib, POSIX_FUNC_NAME));
    }

    TEST_FIXTURE(PosixLoaderFixture, PosixLoadDynamicLibraryMatchesClass)
    {
        CHECK_EQUAL(classDynLib, palDynLib);
    }

    TEST(PosixCloseLoadedLibraryMatchesClass)
    {
        // Note: not using fixture for this test, because order is important.
        void* palDynLib = UnityPalLibraryLoaderLoadDynamicLibrary(POSIX_DL_NAME, RTLD_LAZY);
        bool palResult = UnityPalLibraryLoaderCloseLoadedLibrary(&palDynLib);
        Il2CppNativeString libName(il2cpp::utils::StringUtils::Utf8ToNativeString(POSIX_DL_NAME));
        std::string detailedError;
        void* classDynLib = il2cpp::utils::BaselibHandleUtils::HandleToVoidPtr(il2cpp::os::LibraryLoader::LoadDynamicLibrary(STRING_TO_STRINGVIEW(libName), detailedError));
        bool classResult = il2cpp::os::LibraryLoader::CloseLoadedLibrary(il2cpp::utils::BaselibHandleUtils::VoidPtrToHandle<Baselib_DynamicLibrary_Handle>(classDynLib));
        CHECK_EQUAL(classResult, palResult);
    }

    TEST_FIXTURE(PosixLoaderFixture, PosixGetFunctionPointerMatchesClass)
    {
        palMethodPointer = UnityPalLibraryLoaderGetFunctionPointer(classDynLib, POSIX_FUNC_NAME);
        std::string detailedError;
        classMethodPointer = il2cpp::os::LibraryLoader::GetFunctionPointer(il2cpp::utils::BaselibHandleUtils::VoidPtrToHandle<Baselib_DynamicLibrary_Handle>(classDynLib), POSIX_FUNC_NAME, detailedError);
        CHECK_EQUAL(classMethodPointer, palMethodPointer);
    }

#endif // IL2CPP_TARGET_POSIX
#if defined(WINDOWS)

    struct WindowsLoaderFixture
    {
        WindowsLoaderFixture()
        {
            palDynLib = UnityPalLibraryLoaderLoadDynamicLibrary(WINDOWS_DL_NAME, RTLD_LAZY);
            Il2CppNativeString libName(il2cpp::utils::StringUtils::Utf8ToNativeString(WINDOWS_DL_NAME));
            std::string detailedError;
            classDynLib = il2cpp::utils::BaselibHandleUtils::HandleToVoidPtr(il2cpp::os::LibraryLoader::LoadDynamicLibrary(STRING_TO_STRINGVIEW(libName), detailedError));
        }

        ~WindowsLoaderFixture()
        {
            il2cpp::os::LibraryLoader::CleanupLoadedLibraries();
        }

        void* palDynLib;
        void* classDynLib;
        bool palResult;
        bool classResult;
        Il2CppMethodPointer palMethodPointer;
        Il2CppMethodPointer classMethodPointer;
    };

    TEST_FIXTURE(WindowsLoaderFixture, WindowsLoadDynamicLibraryReturnsAPointer)
    {
        CHECK_NOT_NULL(palDynLib);
    }

    TEST_FIXTURE(WindowsLoaderFixture, WindowsCloseLoadedLibraryReturnsTrue)
    {
        CHECK_EQUAL((int32_t)true, UnityPalLibraryLoaderCloseLoadedLibrary(&palDynLib));
    }

    TEST_FIXTURE(WindowsLoaderFixture, WindowsGetFunctionPointerisNotNull)
    {
        CHECK_NOT_NULL(UnityPalLibraryLoaderGetFunctionPointer(palDynLib, WINDOWS_FUNC_NAME));
    }

    TEST_FIXTURE(WindowsLoaderFixture, WindowsLoadDynamicLibraryMatchesClass)
    {
        CHECK_EQUAL(classDynLib, palDynLib);
    }

    TEST(WindowsCloseLoadedLibraryMatchesClass)
    {
        // Note: not using fixture for this test, because order is important.
        void* palDynLib = UnityPalLibraryLoaderLoadDynamicLibrary(WINDOWS_DL_NAME, RTLD_LAZY);
        bool palResult = UnityPalLibraryLoaderCloseLoadedLibrary(&palDynLib);
        Il2CppNativeString libName(il2cpp::utils::StringUtils::Utf8ToNativeString(WINDOWS_DL_NAME));
        std::string detailedError;
        void* classDynLib = il2cpp::utils::BaselibHandleUtils::HandleToVoidPtr(il2cpp::os::LibraryLoader::LoadDynamicLibrary(STRING_TO_STRINGVIEW(libName), detailedError));
        bool classResult = il2cpp::os::LibraryLoader::CloseLoadedLibrary(il2cpp::utils::BaselibHandleUtils::VoidPtrToHandle<Baselib_DynamicLibrary_Handle>(classDynLib));
        CHECK_EQUAL(classResult, palResult);
    }

    TEST_FIXTURE(WindowsLoaderFixture, WindowsGetFunctionPointerMatchesClass)
    {
        palMethodPointer = UnityPalLibraryLoaderGetFunctionPointer(classDynLib, WINDOWS_FUNC_NAME);
        std::string detailedError;
        classMethodPointer = il2cpp::os::LibraryLoader::GetFunctionPointer(il2cpp::utils::BaselibHandleUtils::VoidPtrToHandle<Baselib_DynamicLibrary_Handle>(classDynLib), WINDOWS_FUNC_NAME, detailedError);
        CHECK_EQUAL(classMethodPointer, palMethodPointer);
    }

#endif // WINDOWS
}

#endif // ENABLE_UNIT_TESTS
