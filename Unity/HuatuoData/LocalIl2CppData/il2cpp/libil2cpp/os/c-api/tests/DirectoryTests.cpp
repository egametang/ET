#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"

#include "../Directory-c-api.h"
#include "../../Directory.h"
#include "../../File.h"
#include "utils/StringUtils.h"
#include "PathHelper.h"
#include <set>

SUITE(Directory)
{
    static const char* CURRENT_PATH = CURRENT_DIRECTORY(".");
    static const char* GARBAGE_PATH = "&0)*$5534";
    static const char* NEW_DIR = CURRENT_DIRECTORY("TEST_NEWDIR");
    static const char* TEST_DIR_1 = CURRENT_DIRECTORY("TestDir1");
    static const char* TEST_DIR_2 = CURRENT_DIRECTORY("TestDir2");
    static const char* TEST_DIR_3 = CURRENT_DIRECTORY("TestDir3");
    static const char* TEST_PATTERN = CURRENT_DIRECTORY("./*");

    struct DirectoryFixture
    {
        DirectoryFixture()
        {
            apiRetValue = NULL;
            classRetValue = NULL;
            tmpRetValue = NULL;

            il2cpp::os::Directory::Create(NEW_DIR, &error);
            error = il2cpp::os::kErrorBadCommand;

            errorCompare = il2cpp::os::kErrorOutOfPaper;
        }

        ~DirectoryFixture()
        {
            il2cpp::os::Directory::Remove(NEW_DIR, &error);
        }

        const char* apiRetValue;
        const char* classRetValue;
        const char* tmpRetValue;
        int error;
        int errorCompare;
    };

    TEST_FIXTURE(DirectoryFixture, GetCurrentDirectoryReturnsValidPointer)
    {
        apiRetValue = UnityPalDirectoryGetCurrent(&error);

        CHECK_NOT_NULL(apiRetValue);
    }

// PS4 returns an empty directory, so the other UnityPalDirectoryGetCurrent tests are enough to confirm
// the proper behavior.
#if !IL2CPP_TARGET_PS4
    TEST_FIXTURE(DirectoryFixture, GetCurrentDirectoryReturnsAStringOfSomeLength)
    {
        apiRetValue = UnityPalDirectoryGetCurrent(&error);
        CHECK(strlen(apiRetValue) > 0);
    }

#endif

    TEST_FIXTURE(DirectoryFixture, GetCurrentDirectoryReturnsGoodError)
    {
        UnityPalDirectoryGetCurrent(&error);

        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST_FIXTURE(DirectoryFixture, ApiGetCurrentDirectoryReturnsSameResultAsClass)
    {
        CHECK_EQUAL(il2cpp::os::Directory::GetCurrent(&error).c_str(), UnityPalDirectoryGetCurrent(&error));
    }

    TEST_FIXTURE(DirectoryFixture, ApiGetCurrentDirectoryReturnsSameErrorAsClass)
    {
        il2cpp::os::Directory::GetCurrent(&errorCompare);
        UnityPalDirectoryGetCurrent(&error);

        CHECK_EQUAL(errorCompare, error);
    }

    TEST_FIXTURE(DirectoryFixture, SetCurrentDirectoryReturnsTrue)
    {
        CHECK(UnityPalDirectorySetCurrent(CURRENT_PATH, &error));
    }

    TEST_FIXTURE(DirectoryFixture, SetCurrentDirectoryReturnsGoodError)
    {
        UnityPalDirectorySetCurrent(CURRENT_PATH, &error);

        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST_FIXTURE(DirectoryFixture, ApiSetCurrentDirectoryReturnsSameValueAsClass)
    {
        CHECK_EQUAL((int32_t)il2cpp::os::Directory::SetCurrent(CURRENT_PATH, &error), UnityPalDirectorySetCurrent(CURRENT_PATH, &error));
    }

    TEST_FIXTURE(DirectoryFixture, ApiSetCurrentDirectoryReturnsSameErrorAsClass)
    {
        il2cpp::os::Directory::SetCurrent(CURRENT_PATH, &errorCompare),
        UnityPalDirectorySetCurrent(CURRENT_PATH, &error);

        CHECK_EQUAL(errorCompare, error);
    }

// On PS4, any string can be set as the current directory.
#if !IL2CPP_TARGET_PS4
    TEST_FIXTURE(DirectoryFixture, SetCurrentDirectoryWithGarbageReturnsFalse)
    {
        CHECK(!UnityPalDirectorySetCurrent(GARBAGE_PATH, &error));
    }

    TEST_FIXTURE(DirectoryFixture, SetCurrentDirectoryReturnsBadError)
    {
        error = il2cpp::os::kErrorCodeSuccess;
        UnityPalDirectorySetCurrent(GARBAGE_PATH, &error);

        CHECK_NOT_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

#else
    TEST_FIXTURE(DirectoryFixture, SetCurrentDirectoryWithGarbageReturnsTrue)
    {
        CHECK(UnityPalDirectorySetCurrent(GARBAGE_PATH, &error));
    }

    TEST_FIXTURE(DirectoryFixture, SetCurrentDirectoryReturnsSuccessErrorCode)
    {
        error = il2cpp::os::kErrorCodePathNotFound; // Could be any non-success error code for initialization
        UnityPalDirectorySetCurrent(GARBAGE_PATH, &error);

        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

#endif

    TEST_FIXTURE(DirectoryFixture, ApiSetCurrentDirectoryWithGarbageReturnsSameAsClass)
    {
        CHECK_EQUAL((int32_t)il2cpp::os::Directory::SetCurrent(GARBAGE_PATH, &error), UnityPalDirectorySetCurrent(GARBAGE_PATH, &error));
    }

    TEST_FIXTURE(DirectoryFixture, ApiSetCurrentDirectoryWithGarbageReturnsSameErrorAsClass)
    {
        error = il2cpp::os::kErrorCodeSuccess;
        errorCompare = il2cpp::os::kErrorCodeSuccess;
        il2cpp::os::Directory::SetCurrent(GARBAGE_PATH, &errorCompare),
        UnityPalDirectorySetCurrent(GARBAGE_PATH, &error);

        CHECK_EQUAL(errorCompare, error);
    }

    TEST_FIXTURE(DirectoryFixture, SetCurrentDirectoryWillMatchUpWithGetCurrentDirectory)
    {
        UnityPalDirectorySetCurrent(CURRENT_PATH, &error);
        tmpRetValue = UnityPalDirectoryGetCurrent(&error);
        UnityPalDirectorySetCurrent(NEW_DIR, &error);
        apiRetValue = UnityPalDirectoryGetCurrent(&error);

        CHECK_NOT_EQUAL(tmpRetValue, apiRetValue);
    }

    struct DirectoryCreateFixture
    {
        DirectoryCreateFixture()
        {
            error = il2cpp::os::kErrorBadCommand;
        }

        ~DirectoryCreateFixture()
        {
            il2cpp::os::Directory::Remove(NEW_DIR, &error);
        }

        int error;
    };

    TEST_FIXTURE(DirectoryCreateFixture, CreateDirectoryReturnsTrue)
    {
        CHECK(UnityPalDirectoryCreate(NEW_DIR, &error));
    }

    TEST_FIXTURE(DirectoryCreateFixture, CreateDirectoryErrorIsSuccess)
    {
        UnityPalDirectoryCreate(NEW_DIR, &error);
        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST(CreateDirectoryWithNullReturnsFalse)
    {
        int error;
        CHECK(!UnityPalDirectoryCreate("", &error));
    }

    TEST(CreateDirectoryWithNullReturnsBadError)
    {
        int error = il2cpp::os::kErrorCodeSuccess;
        UnityPalDirectoryCreate("", &error);
        CHECK_NOT_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    struct DirectoryCreateMatchesClassFixture
    {
        DirectoryCreateMatchesClassFixture()
        {
            error = il2cpp::os::kErrorBadCommand;
            errorCompare = il2cpp::os::kErrorOutOfPaper;
        }

        ~DirectoryCreateMatchesClassFixture()
        {
        }

        int error;
        int errorCompare;
        int32_t apiRetValue;
        bool classRetValue;
    };

    TEST_FIXTURE(DirectoryCreateMatchesClassFixture, ApiCreateDirectoryReturnMatchesClassReturn)
    {
        apiRetValue = UnityPalDirectoryCreate(NEW_DIR, &error);
        il2cpp::os::Directory::Remove(NEW_DIR, &error);
        classRetValue = il2cpp::os::Directory::Create(NEW_DIR, &error);
        il2cpp::os::Directory::Remove(NEW_DIR, &error);

        CHECK_EQUAL((int32_t)classRetValue, apiRetValue);
    }

    TEST_FIXTURE(DirectoryCreateMatchesClassFixture, ApiCreateDirectoryErrorMatchesClassError)
    {
        UnityPalDirectoryCreate(NEW_DIR, &error);
        il2cpp::os::Directory::Remove(NEW_DIR, &error);
        il2cpp::os::Directory::Create(NEW_DIR, &errorCompare);
        il2cpp::os::Directory::Remove(NEW_DIR, &error);

        CHECK_EQUAL(errorCompare, error);
    }

    TEST_FIXTURE(DirectoryCreateMatchesClassFixture, ApiCreateDirectoryWithNullMatchesClass)
    {
        CHECK_EQUAL((int32_t)il2cpp::os::Directory::Create("", &error) , UnityPalDirectoryCreate("", &error));
    }

    TEST_FIXTURE(DirectoryCreateMatchesClassFixture, ApiCreateDirectoryWithNullMatchesClassErrorWithNull)
    {
        UnityPalDirectoryCreate("", &error);
        il2cpp::os::Directory::Create("", &errorCompare);
        CHECK_EQUAL(errorCompare, error);
    }

    struct DirectoryRemoveFixture
    {
        DirectoryRemoveFixture()
        {
            il2cpp::os::Directory::Create(NEW_DIR, &error);
            error = il2cpp::os::kErrorBadCommand;
        }

        ~DirectoryRemoveFixture()
        {
        }

        int error;
    };

    TEST_FIXTURE(DirectoryRemoveFixture, RemoveDirectoryReturnsTrue)
    {
        CHECK(UnityPalDirectoryRemove(NEW_DIR, &error));
    }

    TEST_FIXTURE(DirectoryRemoveFixture, RemoveDirectoryErrorIsSuccess)
    {
        UnityPalDirectoryRemove(NEW_DIR, &error);
        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST(RemoveDirectoryWithNullReturnsFalse)
    {
        int error;
        CHECK(!UnityPalDirectoryRemove("", &error));
    }

    TEST(RemoveDirectoryWithNullReturnsBadError)
    {
        int error = il2cpp::os::kErrorCodeSuccess;
        UnityPalDirectoryRemove("", &error);
        CHECK_NOT_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }


    struct DirectoryRemoveMatchesClassFixture
    {
        DirectoryRemoveMatchesClassFixture()
        {
            error = il2cpp::os::kErrorBadCommand;
            errorCompare = il2cpp::os::kErrorOutOfPaper;
        }

        ~DirectoryRemoveMatchesClassFixture()
        {
        }

        int error;
        int errorCompare;
        int32_t apiRetValue;
        bool classRetValue;
    };

    TEST_FIXTURE(DirectoryRemoveMatchesClassFixture, ApiRemoveDirectoryReturnMatchesClassReturn)
    {
        il2cpp::os::Directory::Create(NEW_DIR, &error);
        apiRetValue = UnityPalDirectoryRemove(NEW_DIR, &error);
        il2cpp::os::Directory::Create(NEW_DIR, &error);
        classRetValue = il2cpp::os::Directory::Remove(NEW_DIR, &error);

        CHECK_EQUAL((int32_t)classRetValue, apiRetValue);
    }

    TEST_FIXTURE(DirectoryRemoveMatchesClassFixture, ApiRemoveDirectoryErrorMatchesClassError)
    {
        il2cpp::os::Directory::Create(NEW_DIR, &error);
        UnityPalDirectoryRemove(NEW_DIR, &error);
        il2cpp::os::Directory::Create(NEW_DIR, &errorCompare);
        il2cpp::os::Directory::Remove(NEW_DIR, &errorCompare);

        CHECK_EQUAL(errorCompare, error);
    }

    TEST_FIXTURE(DirectoryRemoveMatchesClassFixture, ApiRemoveDirectoryWithNullMatchesClass)
    {
        CHECK_EQUAL((int32_t)il2cpp::os::Directory::Remove("", &error) , UnityPalDirectoryRemove("", &error));
    }

    TEST_FIXTURE(DirectoryRemoveMatchesClassFixture, ApiRemoveDirectoryWithNullMatchesClassErrorWithNull)
    {
        UnityPalDirectoryRemove("", &error);
        il2cpp::os::Directory::Remove("", &errorCompare);

        CHECK_EQUAL(errorCompare, error);
    }

    struct FileSytemEntriesFixture
    {
        FileSytemEntriesFixture()
        {
            path = CURRENT_DIRECTORY(".");
            pathWithPattern = CURRENT_DIRECTORY("*");
            attrs = 0;
            mask = 0;
            error = 0;
            entries = NULL;
            numEntries = 0;
            classError = 0;
            testFind = NULL;
            il2cpp::os::Directory::Create(TEST_DIR_1, &error);
            il2cpp::os::Directory::Create(TEST_DIR_2, &error);
            il2cpp::os::Directory::Create(TEST_DIR_3, &error);
            resultFileName = NULL;
            nextResultFileName = NULL;
            thirdResultFileName = NULL;
            resultAttributes = 0;
            returnErrorCode = il2cpp::os::kErrorBadCommand;
            compareResultFileName = NULL;
            compareResultAttributes = 0;
        }

        ~FileSytemEntriesFixture()
        {
            il2cpp::os::Directory::Remove(TEST_DIR_1, &error);
            il2cpp::os::Directory::Remove(TEST_DIR_2, &error);
            il2cpp::os::Directory::Remove(TEST_DIR_3, &error);
        }

        const char* path;
        const char* pathWithPattern;
        int32_t attrs;
        int32_t mask;
        int error;
        char** entries;
        int32_t numEntries;
        int classError;
        std::set<std::string> classEntries;
        int32_t classNumEntries;
        UnityPalFindHandle* testFind;
        char* resultFileName;
        char* nextResultFileName;
        char* thirdResultFileName;
        int32_t resultAttributes;
        UnityPalErrorCode returnErrorCode;
        UnityPalErrorCode compareReturnErrorCode;
        char* compareResultFileName;
        int32_t compareResultAttributes;
        Il2CppNativeString classFileName;
    };

    TEST_FIXTURE(FileSytemEntriesFixture, GetFileSystemEntriesReturnsAValidPointer)
    {
        UnityPalDirectoryGetFileSystemEntries(path, pathWithPattern, attrs, mask, &error, &entries, &numEntries);
        CHECK_NOT_NULL(entries);
    }

    TEST_FIXTURE(FileSytemEntriesFixture, GetFileSystemEntriesReturnsThreeEntires)
    {
#if IL2CPP_TARGET_PS4
        const int expectedNumEntries = 9; // PS4 has the test exectuable, src, test.map, and test.prx files in the current directory as well.
#else
        const int expectedNumEntries = 3;
#endif
        UnityPalDirectoryGetFileSystemEntries(path, pathWithPattern, attrs, mask, &error, &entries, &numEntries);
        CHECK_EQUAL(expectedNumEntries, numEntries);
    }

    TEST_FIXTURE(FileSytemEntriesFixture, GetFileSystemEntriesReturnsCorrectEntryInSecondIndexOfArray)
    {
        UnityPalDirectoryGetFileSystemEntries(path, pathWithPattern, attrs, mask, &error, &entries, &numEntries);
        CHECK(strlen(entries[1]) > 5);
    }

    TEST_FIXTURE(FileSytemEntriesFixture, GetFileSystemEntriesReturnsSuccesfulErrorCode)
    {
        UnityPalDirectoryGetFileSystemEntries(path, pathWithPattern, attrs, mask, &error, &entries, &numEntries);
        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST_FIXTURE(FileSytemEntriesFixture, GetFileSystemEntriesWithGarbageReturnsNull)
    {
        UnityPalDirectoryGetFileSystemEntries(GARBAGE_PATH, GARBAGE_PATH, attrs, mask, &error, &entries, &numEntries);
        CHECK_NULL(entries);
    }

    TEST_FIXTURE(FileSytemEntriesFixture, GetFileSystemEntriesWithGarbageReturnsZeroEntires)
    {
        UnityPalDirectoryGetFileSystemEntries(GARBAGE_PATH, GARBAGE_PATH, attrs, mask, &error, &entries, &numEntries);
        CHECK_EQUAL(0, numEntries);
    }

    TEST_FIXTURE(FileSytemEntriesFixture, GetFileSystemEntriesReturnSameAsClass)
    {
        UnityPalDirectoryGetFileSystemEntries(path, pathWithPattern, attrs, mask, &error, &entries, &numEntries);
        classEntries = il2cpp::os::Directory::GetFileSystemEntries(path, pathWithPattern, attrs, mask, &classError);
        CHECK_EQUAL(classEntries.size(), numEntries);
    }

    TEST_FIXTURE(FileSytemEntriesFixture, GetFileSystemEntriesReturnsSameErrorCodeAsClass)
    {
        UnityPalDirectoryGetFileSystemEntries(path, pathWithPattern, attrs, mask, &error, &entries, &numEntries);
        il2cpp::os::Directory::GetFileSystemEntries(path, pathWithPattern, attrs, mask, &classError);
        CHECK_EQUAL(classError, error);
    }

    TEST_FIXTURE(FileSytemEntriesFixture, GetFileSystemEntriesWithGarbageNumEntriesMatchesClass)
    {
        UnityPalDirectoryGetFileSystemEntries(GARBAGE_PATH, GARBAGE_PATH, attrs, mask, &error, &entries, &numEntries);
        classEntries = il2cpp::os::Directory::GetFileSystemEntries(GARBAGE_PATH, GARBAGE_PATH, attrs, mask, &classError);
        CHECK_EQUAL(classEntries.size(), numEntries);
    }

    TEST_FIXTURE(FileSytemEntriesFixture, FindHandleNewReturnsAValidPointer)
    {
        CHECK_NOT_NULL(UnityPalDirectoryFindHandleNew(CURRENT_PATH));
    }

    TEST_FIXTURE(FileSytemEntriesFixture, CloseOsHandleSucceeds)
    {
        testFind = UnityPalDirectoryFindHandleNew(CURRENT_PATH);
        CHECK_EQUAL(0, UnityPalDirectoryCloseOSHandle(testFind));
    }

    TEST_FIXTURE(FileSytemEntriesFixture, FindFirstFileCorrectlyFillsInResultFileName)
    {
        const char* expectedFile = TEST_DIR_1;
        testFind = UnityPalDirectoryFindHandleNew(TEST_DIR_1);
        returnErrorCode = UnityPalDirectoryFindFirstFile(testFind, TEST_DIR_1, &resultFileName, &resultAttributes);
        UnityPalDirectoryCloseOSHandle(testFind);
        CHECK_EQUAL(expectedFile, resultFileName);
    }

    TEST_FIXTURE(FileSytemEntriesFixture, FindFirstFileResultAttributesAreValid)
    {
        testFind = UnityPalDirectoryFindHandleNew(TEST_PATTERN);
        returnErrorCode = UnityPalDirectoryFindFirstFile(testFind, TEST_PATTERN, &resultFileName, &resultAttributes);
        UnityPalDirectoryCloseOSHandle(testFind);
        CHECK(resultAttributes > 0);
    }

    TEST_FIXTURE(FileSytemEntriesFixture, FindFirstFileResultReturnErrorCodeIsSuccess)
    {
        testFind = UnityPalDirectoryFindHandleNew(TEST_PATTERN);
        returnErrorCode = UnityPalDirectoryFindFirstFile(testFind, TEST_PATTERN, &resultFileName, &resultAttributes);
        UnityPalDirectoryCloseOSHandle(testFind);
        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, returnErrorCode);
    }

    TEST_FIXTURE(FileSytemEntriesFixture, FindNextFileReturnsGoodStrings)
    {
        // UnityPalDirectoryFindNextFile will find "."" and ".." , we need to hit it
        // a few times to grab a test dir name

#if IL2CPP_TARGET_PS4
        const char* expectedFile = "test.map"; // PS4 has the test exectuable, src, and test.map files in the current directory as well.
#else
        const char* expectedFile = TEST_DIR_1;
#endif
        testFind = UnityPalDirectoryFindHandleNew(TEST_PATTERN);

        std::set<std::string> foundFiles;
        returnErrorCode = UnityPalDirectoryFindFirstFile(testFind, TEST_PATTERN, &resultFileName, &resultAttributes);
        while (returnErrorCode == il2cpp::os::kErrorCodeSuccess)
        {
            foundFiles.insert(resultFileName);
            returnErrorCode = UnityPalDirectoryFindNextFile(testFind, &resultFileName, &resultAttributes);
        }

        UnityPalDirectoryCloseOSHandle(testFind);
        CHECK(foundFiles.find(expectedFile) != foundFiles.end());
    }

    TEST_FIXTURE(FileSytemEntriesFixture, FindNextFileResultAttributesAreValid)
    {
        testFind = UnityPalDirectoryFindHandleNew(TEST_PATTERN);
        UnityPalDirectoryFindFirstFile(testFind, TEST_PATTERN, &resultFileName, &resultAttributes);
        returnErrorCode = UnityPalDirectoryFindNextFile(testFind, &nextResultFileName, &resultAttributes);
        UnityPalDirectoryCloseOSHandle(testFind);
        CHECK(resultAttributes > 0);
    }

    TEST_FIXTURE(FileSytemEntriesFixture, FindNextFileResultReturnErrorCodeIsSuccess)
    {
        testFind = UnityPalDirectoryFindHandleNew(TEST_PATTERN);
        UnityPalDirectoryFindFirstFile(testFind, TEST_PATTERN, &resultFileName, &resultAttributes);
        returnErrorCode = UnityPalDirectoryFindNextFile(testFind, &nextResultFileName, &resultAttributes);
        UnityPalDirectoryCloseOSHandle(testFind);
        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, returnErrorCode);
    }

    TEST_FIXTURE(FileSytemEntriesFixture, FindFirstFileMatchesClass)
    {
        testFind = UnityPalDirectoryFindHandleNew(TEST_PATTERN);
        returnErrorCode = UnityPalDirectoryFindFirstFile(testFind, TEST_PATTERN, &resultFileName, &resultAttributes);
        UnityPalDirectoryCloseOSHandle(testFind);
        Il2CppNativeString pattern(il2cpp::utils::StringUtils::Utf8ToNativeString(TEST_PATTERN));
        il2cpp::os::Directory::FindHandle classFindHandle(STRING_TO_STRINGVIEW(pattern));
        il2cpp::os::Directory::FindFirstFile(&classFindHandle, STRING_TO_STRINGVIEW(pattern), &classFileName, &resultAttributes);
        classFindHandle.CloseOSHandle();
        CHECK_EQUAL(il2cpp::utils::StringUtils::NativeStringToUtf8(classFileName).c_str(), resultFileName);
    }

    TEST_FIXTURE(FileSytemEntriesFixture, FindFirstFileResultAttributesMatchClass)
    {
        testFind = UnityPalDirectoryFindHandleNew(TEST_PATTERN);
        returnErrorCode = UnityPalDirectoryFindFirstFile(testFind, TEST_PATTERN, &resultFileName, &resultAttributes);
        UnityPalDirectoryCloseOSHandle(testFind);
        Il2CppNativeString pattern(il2cpp::utils::StringUtils::Utf8ToNativeString(TEST_PATTERN));
        il2cpp::os::Directory::FindHandle classFindHandle(STRING_TO_STRINGVIEW(pattern));
        il2cpp::os::Directory::FindFirstFile(&classFindHandle, STRING_TO_STRINGVIEW(pattern), &classFileName, &compareResultAttributes);
        classFindHandle.CloseOSHandle();
        CHECK_EQUAL(compareResultAttributes, resultAttributes);
    }

    TEST_FIXTURE(FileSytemEntriesFixture, FindFirstFileResultErrorMatchClass)
    {
        testFind = UnityPalDirectoryFindHandleNew(TEST_PATTERN);
        returnErrorCode = UnityPalDirectoryFindFirstFile(testFind, TEST_PATTERN, &resultFileName, &resultAttributes);
        UnityPalDirectoryCloseOSHandle(testFind);
        Il2CppNativeString pattern(il2cpp::utils::StringUtils::Utf8ToNativeString(TEST_PATTERN));
        il2cpp::os::Directory::FindHandle classFindHandle(STRING_TO_STRINGVIEW(pattern));
        compareReturnErrorCode = il2cpp::os::Directory::FindFirstFile(&classFindHandle, STRING_TO_STRINGVIEW(pattern), &classFileName, &compareResultAttributes);
        classFindHandle.CloseOSHandle();
        CHECK_EQUAL(compareReturnErrorCode, returnErrorCode);
    }

    TEST_FIXTURE(FileSytemEntriesFixture, FindNextFileMatchesClass)
    {
        testFind = UnityPalDirectoryFindHandleNew(TEST_PATTERN);
        UnityPalDirectoryFindFirstFile(testFind, TEST_PATTERN, &resultFileName, &resultAttributes);
        UnityPalDirectoryFindNextFile(testFind, &nextResultFileName, &resultAttributes);
        UnityPalDirectoryCloseOSHandle(testFind);
        Il2CppNativeString pattern(il2cpp::utils::StringUtils::Utf8ToNativeString(TEST_PATTERN));
        il2cpp::os::Directory::FindHandle classFindHandle(STRING_TO_STRINGVIEW(pattern));
        il2cpp::os::Directory::FindFirstFile(&classFindHandle, STRING_TO_STRINGVIEW(pattern), &classFileName, &compareResultAttributes);
        il2cpp::os::Directory::FindNextFile(&classFindHandle, &classFileName, &compareResultAttributes);
        classFindHandle.CloseOSHandle();
        CHECK_EQUAL(il2cpp::utils::StringUtils::NativeStringToUtf8(classFileName).c_str(), nextResultFileName);
    }
}

#endif // ENABLE_UNIT_TESTS
