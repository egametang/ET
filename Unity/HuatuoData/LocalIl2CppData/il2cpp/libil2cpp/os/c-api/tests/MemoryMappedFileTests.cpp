#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"

#include "../MemoryMappedFile-c-api.h"
#include "../../../utils/MemoryMappedFile.h"
#include "../../File.h"
#include "../File-c-api.h"
#include "PathHelper.h"

#if IL2CPP_TARGET_POSIX
#include <fcntl.h>
#include <unistd.h>
#endif

static const char* TEST_FILE_NAME = CURRENT_DIRECTORY("MEM_MAP_TEST_FILE");
static const char* TEST_STRING = "THIS IS A TEST\r\nSTRING TO \r\nBE USED IN A \r\nMEMORY MAPPED FILE\r\n";

class MapTestsWithParamsFixture
{
public:
    MapTestsWithParamsFixture() : offset(0), length(7)
    {
        Initialize();
    }

    MapTestsWithParamsFixture(size_t offset_in, size_t length_in) : offset(offset_in), length(length_in)
    {
        Initialize();
    }

    ~MapTestsWithParamsFixture()
    {
        il2cpp::utils::MemoryMappedFile::Unmap(apiAddress);
        il2cpp::utils::MemoryMappedFile::Unmap(classAddress);
        CloseTestFile(handle);
        DeleteTestFile(handle);
        handle = NULL;
    }

    il2cpp::os::FileHandle* handle;
    void* apiAddress;
    void* classAddress;
    int64_t length;
    int64_t offset;

private:
    void Initialize()
    {
        handle = NULL;
        handle = CreateTestFile();
        WriteCharactersToTestFile(handle);
        apiAddress = NULL;
        classAddress = il2cpp::utils::MemoryMappedFile::Map(handle, length, offset);
    }

    il2cpp::os::FileHandle* CreateTestFile()
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, kFileShareReadWrite, 0, &error);

        return handle;
    }

    void WriteCharactersToTestFile(il2cpp::os::FileHandle* handle)
    {
        static const char* buffer = TEST_STRING;
        int error;

        il2cpp::os::File::Write(handle, buffer, (int)strlen(buffer), &error);
    }

    int CloseTestFile(il2cpp::os::FileHandle* handle)
    {
        int error;
        il2cpp::os::File::Close(handle, &error);
        return error;
    }

    int DeleteTestFile(il2cpp::os::FileHandle* handle)
    {
        int error;
        il2cpp::os::File::DeleteFile(TEST_FILE_NAME, &error);
        return error;
    }
};

class MapTestsFixture : public MapTestsWithParamsFixture
{
public:
    MapTestsFixture() : MapTestsWithParamsFixture(0, 0)
    {
    }
};

SUITE(MemoryMappedFile)
{
    TEST_FIXTURE(MapTestsFixture, MapReturnsAValidPointer)
    {
        apiAddress = UnityPalMemoryMappedFileMap(handle);
        CHECK_NOT_NULL(apiAddress);
    }

    TEST_FIXTURE(MapTestsFixture, MappedPointerHasMatchingCharactersAsFile)
    {
        apiAddress = UnityPalMemoryMappedFileMap(handle);
        CHECK_EQUAL(0, strncmp("THIS", (const char*)apiAddress, 4));
    }

    TEST_FIXTURE(MapTestsFixture, MappedPointerHasMatchingSizeAsFile)
    {
        apiAddress = UnityPalMemoryMappedFileMap(handle);
        CHECK_EQUAL(strlen(TEST_STRING), strlen((const char*)apiAddress));
    }

    TEST_FIXTURE(MapTestsFixture, ApiMapReturnsPointerThatDoesNotMatchClass)
    {
        apiAddress = UnityPalMemoryMappedFileMap(handle);
        CHECK_NOT_EQUAL(apiAddress, classAddress);
    }

    TEST_FIXTURE(MapTestsFixture, ApiMappedPointerCharactersMatchClassMappedPointer)
    {
        apiAddress = UnityPalMemoryMappedFileMap(handle);
        CHECK_EQUAL(strncmp("THIS", (const char*)classAddress, 4), strncmp("THIS", (const char*)apiAddress, 4));
    }

    TEST_FIXTURE(MapTestsFixture, ApiMappedLengthMatchesClassMatchedLength)
    {
        apiAddress = UnityPalMemoryMappedFileMap(handle);
        CHECK_EQUAL(strlen((const char*)classAddress), strlen((const char*)apiAddress));
    }

    TEST_FIXTURE(MapTestsWithParamsFixture, MapWithParamsReturnsAValidPointer)
    {
        apiAddress = UnityPalMemoryMappedFileMapWithParams(handle, length, offset);
        CHECK_NOT_NULL(apiAddress);
    }

    TEST_FIXTURE(MapTestsWithParamsFixture, MappedWithParamsPointerHasMatchingCharactersAsFile)
    {
        apiAddress = UnityPalMemoryMappedFileMapWithParams(handle, length, offset);
        CHECK_EQUAL(0, strncmp("THIS IS", (const char*)apiAddress, (size_t)length));
    }

    TEST_FIXTURE(MapTestsWithParamsFixture, MappedWithParamsPointerHasMatchingSizeAsFile)
    {
        apiAddress = UnityPalMemoryMappedFileMapWithParams(handle, length, offset);
        CHECK_EQUAL(strlen(TEST_STRING), strlen((const char*)apiAddress));
    }

    TEST_FIXTURE(MapTestsWithParamsFixture, ApiMapWithParamsReturnsPointerThatDoesNotMatchClass)
    {
        apiAddress = UnityPalMemoryMappedFileMapWithParams(handle, length, offset);
        CHECK_NOT_EQUAL(apiAddress, classAddress);
    }

    TEST_FIXTURE(MapTestsWithParamsFixture, ApiMappedWithParamsPointerCharactersMatchClassMappedPointer)
    {
        apiAddress = UnityPalMemoryMappedFileMapWithParams(handle, length, offset);
        CHECK_EQUAL(strncmp("THIS IS", (const char*)classAddress, (size_t)length), strncmp("THIS IS", (const char*)apiAddress, (size_t)length));
    }

    TEST_FIXTURE(MapTestsWithParamsFixture, ApiMappedWithParamsLengthMatchesClassMatchedLength)
    {
        apiAddress = UnityPalMemoryMappedFileMapWithParams(handle, length, offset);
        CHECK_EQUAL(strlen((const char*)classAddress), strlen((const char*)apiAddress));
    }
}

#endif // ENABLE_UNIT_TESTS
