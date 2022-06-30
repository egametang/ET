#if ENABLE_UNIT_TESTS

#include "il2cpp-config.h"

#include "UnitTest++.h"

#include "../../File.h"
#include "../../Process.h"
#include "../File-c-api.h"
#include "PathHelper.h"

static const char* TEST_FILE_NAME_WITHOUT_PATH = "TESTFILE2";
static const char* TEST_FILE_NAME = CURRENT_DIRECTORY("TESTFILE2");
static const char* DUPLICATE_TEST_FILE_NAME = CURRENT_DIRECTORY("DUP_TESTFILE2");
static const char* BACKUP_TEST_FILE_NAME = CURRENT_DIRECTORY("BACKUP_TESTFILE2");
static const int64_t TEST_FILE_LENGTH = 1234567891L;
static const char* TEST_STRING = "THIS IS A TEST";

static il2cpp::os::FileHandle* PrepareTestFile()
{
    int error;
    il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeCreateNew, 0, 0, 0, &error);

    return handle;
}

static void CleanupTestFile(il2cpp::os::FileHandle* handle)
{
    int error;
    il2cpp::os::File::Close(handle, &error);
    il2cpp::os::File::DeleteFile(TEST_FILE_NAME, &error);
}

static void WriteSomeCharactersToTestFile(il2cpp::os::FileHandle* handle)
{
    static const char* buffer = TEST_STRING;
    int error;

    il2cpp::os::File::Write(handle, buffer, (int)strlen(buffer), &error);
}

SUITE(File)
{
    struct FileFixture
    {
        FileFixture()
        {
            handle = PrepareTestFile();
        }

        ~FileFixture()
        {
            CleanupTestFile(handle);
        }

        il2cpp::os::FileHandle* handle;
    };

#if !IL2CPP_TARGET_WINRT && !IL2CPP_TARGET_XBOXONE
    TEST_FIXTURE(FileFixture, FileIsAttyWithValidButNoTTY_ReturnsFalse)
    {
        CHECK_MSG(!UnityPalIsatty(handle), "A normal is a TTY, which is not expected.");
    }

    TEST(FileIsAttyMatchesClass)
    {
        il2cpp::os::FileHandle* handle = il2cpp::os::File::GetStdInput();

        CHECK_EQUAL((int32_t)il2cpp::os::File::Isatty(handle).Get(), UnityPalIsatty(handle));
    }

    TEST_FIXTURE(FileFixture, FileIsAttyWithValidButNoTTYMatchesClass)
    {
        CHECK_EQUAL((int32_t)il2cpp::os::File::Isatty(handle).Get(), UnityPalIsatty(handle));
    }

#endif

    TEST(FileOpenNoError_ReturnsNonNullHandle)
    {
        const char* FILE_NAME = CURRENT_DIRECTORY("TESTFILE2");
        int error;
        UnityPalFileHandle* handle = NULL;

        handle = UnityPalOpen(FILE_NAME, kFileModeCreateNew, 0, 0, 0, &error);

        CHECK_NOT_NULL(handle);

        il2cpp::os::File::Close(handle, &error);
        il2cpp::os::File::DeleteFile(FILE_NAME, &error);
    }

    TEST(FileOpenWithError)
    {
        int error = 0;
        UnityPalFileHandle* handle = NULL;
        handle = UnityPalOpen(CURRENT_DIRECTORY("file_does_not_exist"), kFileModeOpen, 0, 0, 0, &error);

#if IL2CPP_TARGET_PS4
        CHECK_EQUAL(il2cpp::os::kErrorCodePathNotFound, error);
#else
        CHECK_EQUAL(il2cpp::os::kErrorCodeFileNotFound, error);
#endif
    }

    TEST(FileOpenWithEmptyPath_ReturnsNull)
    {
        int unused = 0;
        CHECK_NULL(UnityPalOpen("", kFileModeCreateNew, 0, 0, 0, &unused));
    }

    TEST(FileOpenWithFileThatDoesNotExist_ReturnsNull)
    {
        int unused = 0;
        CHECK_NULL(UnityPalOpen("file_that_does_not_exist", kFileModeOpen, 0, 0, 0, &unused));
    }

    TEST(FileOpenWithErrorMatchesClass)
    {
        int api_error = 0;
        UnityPalFileHandle* api_handle = NULL;
        int class_error = 0;
        UnityPalFileHandle* class_handle = NULL;

        api_handle = UnityPalOpen(CURRENT_DIRECTORY("file_does_not_exist"), kFileModeOpen, 0, 0, 0, &api_error);
        class_handle = il2cpp::os::File::Open(CURRENT_DIRECTORY("file_does_not_exist"), kFileModeOpen, 0, 0, 0, &class_error);

        CHECK_EQUAL(class_error, api_error);
    }

    TEST(GetStdInput_IsNotNull)
    {
        CHECK_NOT_NULL(UnityPalGetStdInput());
    }

    TEST(GetStdOutput_IsNotNull)
    {
        CHECK_NOT_NULL(UnityPalGetStdOutput());
    }

    TEST(GetStdError_IsNotNull)
    {
        CHECK_NOT_NULL(UnityPalGetStdError());
    }

    TEST_FIXTURE(FileFixture, GetFileTypeNormal_IsDisk)
    {
        CHECK_EQUAL(kFileTypeDisk, UnityPalGetFileType(handle));
    }

    TEST(GetFileTypeMatchesClass)
    {
        CHECK_EQUAL(il2cpp::os::File::GetFileType(UnityPalGetStdError()), UnityPalGetFileType(UnityPalGetStdError()));
    }

    TEST_FIXTURE(FileFixture, GetFileTypeErrorMatchesClass)
    {
        CHECK_EQUAL(il2cpp::os::File::GetFileType(handle), UnityPalGetFileType(handle));
    }

    TEST_FIXTURE(FileFixture, GetFileAttributesCleanError)
    {
        int error;

        UnityPalFileAttributes attributes = UnityPalGetFileAttributes(TEST_FILE_NAME, &error);

        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST_FIXTURE(FileFixture, GetFileAttributesCleanErrorMatchesClass)
    {
        int api_error;
        int class_error;

        UnityPalGetFileAttributes(TEST_FILE_NAME, &api_error);
        il2cpp::os::File::GetFileAttributes(TEST_FILE_NAME, &class_error);

        CHECK_EQUAL(class_error, api_error);
    }

    TEST_FIXTURE(FileFixture, GetFileAttributesMatchesClass)
    {
        int api_error;
        int class_error;

        UnityPalFileAttributes api_attributes = UnityPalGetFileAttributes(TEST_FILE_NAME, &api_error);
        UnityPalFileAttributes class_attributes = il2cpp::os::File::GetFileAttributes(TEST_FILE_NAME, &class_error);

        CHECK_EQUAL(class_attributes, api_attributes);
    }

    TEST(GetFileAttributesWithBadPath_ReturnsNegativeOne)
    {
        int error;

        CHECK_EQUAL(-1, UnityPalGetFileAttributes(CURRENT_DIRECTORY("sf&236732q#"), &error));
    }

    TEST(GetFileAttributesWithBadPathMatchesClass)
    {
        int api_error;
        int class_error;

        UnityPalFileAttributes api_attributes = UnityPalGetFileAttributes(CURRENT_DIRECTORY("#23sfs#"), &api_error);
        UnityPalFileAttributes class_attributes = il2cpp::os::File::GetFileAttributes(CURRENT_DIRECTORY("#23sfs#"), &class_error);

        CHECK_EQUAL(class_attributes, api_attributes);
    }

    TEST_FIXTURE(FileFixture, SetFileAttributesNormalResult_ReturnsTrue)
    {
        int error;
        CHECK(UnityPalSetFileAttributes(TEST_FILE_NAME, kFileAttributeTemporary, &error));
    }

    TEST(SetFileAttributesNormalError)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);

        UnityPalSetFileAttributes(TEST_FILE_NAME, kFileAttributeNormal,  &error);

        CleanupTestFile(handle);

        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST(SetFileAttributesBadResult_ReturnsFalse)
    {
        int error;

        CHECK(!UnityPalSetFileAttributes(CURRENT_DIRECTORY("234232345$$"), kFileAttributeTemporary, &error));
    }

    TEST(SetFileAttributesBadError)
    {
        int error;

        UnityPalSetFileAttributes(CURRENT_DIRECTORY("234232345$$"), kFileAttributeTemporary, &error);

        CHECK_NOT_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST_FIXTURE(FileFixture, SetFileAttributesNormalResultMatchesClass)
    {
        int error;

        CHECK_EQUAL((int32_t)il2cpp::os::File::SetFileAttributes(TEST_FILE_NAME, kFileAttributeTemporary, &error), UnityPalSetFileAttributes(TEST_FILE_NAME, kFileAttributeTemporary, &error));
    }

    TEST(SetFileAttributesNormalAttributeReadMatchesClass)
    {
        il2cpp::os::FileHandle* handle = PrepareTestFile();
        int error;

        UnityPalSetFileAttributes(TEST_FILE_NAME, kFileAttributeTemporary, &error);
        UnityPalFileAttributes api_attributes = UnityPalGetFileAttributes(TEST_FILE_NAME, &error);
        CleanupTestFile(handle);

        handle = PrepareTestFile();
        il2cpp::os::File::SetFileAttributes(TEST_FILE_NAME, kFileAttributeTemporary, &error);
        UnityPalFileAttributes class_attributes = il2cpp::os::File::GetFileAttributes(TEST_FILE_NAME, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(api_attributes, class_attributes);
    }

    TEST(SetFileAttributesBadResultMatchesClass)
    {
        int error;
        CHECK_EQUAL((int32_t)il2cpp::os::File::SetFileAttributes(CURRENT_DIRECTORY("234232345$$"), kFileAttributeTemporary, &error), UnityPalSetFileAttributes(CURRENT_DIRECTORY("234232345$$"), kFileAttributeTemporary, &error));
    }

    TEST(SetFileAttributesBadErrorMatchesClass)
    {
        int api_error;
        int class_error;

        UnityPalSetFileAttributes(CURRENT_DIRECTORY("234232345$$"), kFileAttributeTemporary, &api_error);
        il2cpp::os::File::SetFileAttributes(CURRENT_DIRECTORY("234232345$$"), kFileAttributeTemporary, &class_error);

        CHECK_EQUAL(class_error, api_error);
    }

    TEST_FIXTURE(FileFixture, GetFileStatNormalResult_ReturnsTrue)
    {
        int error;
        UnityPalFileStat fileStat;
        CHECK(UnityPalGetFileStat(TEST_FILE_NAME, &fileStat, &error));
    }

    TEST_FIXTURE(FileFixture, GetFileStatNormalError)
    {
        int error;
        UnityPalFileStat fileStat;

        UnityPalGetFileStat(TEST_FILE_NAME, &fileStat, &error);

        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST(GetFileStatBadResult_ReturnsFalse)
    {
        int error;
        UnityPalFileStat fileStat;

        CHECK(!UnityPalGetFileStat(CURRENT_DIRECTORY("#Q23423"), &fileStat, &error));
    }

    TEST(GetFileStatBadError)
    {
        int error;
        UnityPalFileStat fileStat;

        UnityPalGetFileStat(CURRENT_DIRECTORY("#Q23423"), &fileStat, &error);

        CHECK_NOT_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST_FIXTURE(FileFixture, GetFileStatNormalName)
    {
        int error;
        UnityPalFileStat fileStat;

        UnityPalGetFileStat(TEST_FILE_NAME, &fileStat, &error);

        CHECK_EQUAL(TEST_FILE_NAME_WITHOUT_PATH, fileStat.name);
    }

    TEST_FIXTURE(FileFixture, GetFileStatNormalLength)
    {
        int error;
        UnityPalFileStat fileStat;

        UnityPalGetFileStat(TEST_FILE_NAME, &fileStat, &error);

        CHECK_EQUAL(0, fileStat.length);
    }

    TEST_FIXTURE(FileFixture, GetFileStatNormalCreationTime)
    {
        int error;
        UnityPalFileStat fileStat;

        UnityPalGetFileStat(TEST_FILE_NAME, &fileStat, &error);

        CHECK(fileStat.creation_time > 100000000);
    }

    TEST_FIXTURE(FileFixture, GetFileStatNormalLastAccessTime)
    {
        int error;
        UnityPalFileStat fileStat;

        UnityPalGetFileStat(TEST_FILE_NAME, &fileStat, &error);

        CHECK(fileStat.last_access_time > 100000000);
    }

    TEST_FIXTURE(FileFixture, GetFileStatNormalLastWriteTime)
    {
        int error;
        UnityPalFileStat fileStat;

        UnityPalGetFileStat(TEST_FILE_NAME, &fileStat, &error);

        CHECK(fileStat.last_write_time > 100000000);
    }

    TEST_FIXTURE(FileFixture, GetFileStatNormalResultMatchesClass)
    {
        int error;
        UnityPalFileStat apiStat;
        il2cpp::os::FileStat classStat;

        CHECK_EQUAL((int32_t)il2cpp::os::File::GetFileStat(TEST_FILE_NAME, &classStat, &error), UnityPalGetFileStat(TEST_FILE_NAME, &apiStat, &error));
    }

    TEST_FIXTURE(FileFixture, GetFileStatNormalErrorMatchesClass)
    {
        int apiError;
        int classError;
        UnityPalFileStat apiStat;
        il2cpp::os::FileStat classStat;

        UnityPalGetFileStat(TEST_FILE_NAME, &apiStat, &apiError);
        il2cpp::os::File::GetFileStat(TEST_FILE_NAME, &classStat, &classError);

        CHECK_EQUAL(classError, apiError);
    }

    TEST(GetFileStatBadResultMatchesClass)
    {
        int error;
        UnityPalFileStat apiStat;
        il2cpp::os::FileStat classStat;

        CHECK_EQUAL((int32_t)il2cpp::os::File::GetFileStat(CURRENT_DIRECTORY("#Q23423"), &classStat, &error), UnityPalGetFileStat(CURRENT_DIRECTORY("#Q23423"), &apiStat, &error));
    }

    TEST(GetFileStatBadErrorMatchesClass)
    {
        int apiError;
        int classError;
        UnityPalFileStat apiStat;
        il2cpp::os::FileStat classStat;

        UnityPalGetFileStat(CURRENT_DIRECTORY("#Q23423"), &apiStat, &apiError);
        il2cpp::os::File::GetFileStat(CURRENT_DIRECTORY("#Q23423"), &classStat, &classError);

        CHECK_EQUAL(classError, apiError);
    }

    TEST_FIXTURE(FileFixture, GetFileStatNormalNameMatchesClass)
    {
        int error;
        UnityPalFileStat apiFileStat;
        il2cpp::os::FileStat classFileStat;

        UnityPalGetFileStat(TEST_FILE_NAME, &apiFileStat, &error);
        il2cpp::os::File::GetFileStat(TEST_FILE_NAME, &classFileStat, &error);

        CHECK_EQUAL(classFileStat.name, apiFileStat.name);
    }

    TEST_FIXTURE(FileFixture, GetFileStatNormalAttributesMatchesClass)
    {
        int error;
        UnityPalFileStat apiFileStat;
        il2cpp::os::FileStat classFileStat;

        UnityPalGetFileStat(TEST_FILE_NAME, &apiFileStat, &error);
        il2cpp::os::File::GetFileStat(TEST_FILE_NAME, &classFileStat, &error);

        CHECK_EQUAL(classFileStat.attributes, apiFileStat.attributes);
    }

    TEST_FIXTURE(FileFixture, GetFileStatNormalLengthMatchesClass)
    {
        int error;
        UnityPalFileStat apiFileStat;
        il2cpp::os::FileStat classFileStat;

        UnityPalGetFileStat(TEST_FILE_NAME, &apiFileStat, &error);
        il2cpp::os::File::GetFileStat(TEST_FILE_NAME, &classFileStat, &error);

        CHECK_EQUAL(classFileStat.length, apiFileStat.length);
    }

    TEST_FIXTURE(FileFixture, GetFileStatNormalCreationTimeMatchesClass)
    {
        int error;
        UnityPalFileStat apiFileStat;
        il2cpp::os::FileStat classFileStat;

        UnityPalGetFileStat(TEST_FILE_NAME, &apiFileStat, &error);
        il2cpp::os::File::GetFileStat(TEST_FILE_NAME, &classFileStat, &error);

        CHECK_EQUAL(classFileStat.creation_time, apiFileStat.creation_time);
    }

    TEST_FIXTURE(FileFixture, GetFileStatNormalLastAccessTimeMatchesClass)
    {
        int error;
        UnityPalFileStat apiFileStat;
        il2cpp::os::FileStat classFileStat;

        UnityPalGetFileStat(TEST_FILE_NAME, &apiFileStat, &error);
        il2cpp::os::File::GetFileStat(TEST_FILE_NAME, &classFileStat, &error);

        CHECK_EQUAL(classFileStat.last_access_time, apiFileStat.last_access_time);
    }

    TEST_FIXTURE(FileFixture, GetFileStatNormalLastWriteTimeMatchesClass)
    {
        int error;
        UnityPalFileStat apiFileStat;
        il2cpp::os::FileStat classFileStat;

        UnityPalGetFileStat(TEST_FILE_NAME, &apiFileStat, &error);
        il2cpp::os::File::GetFileStat(TEST_FILE_NAME, &classFileStat, &error);

        CHECK_EQUAL(classFileStat.last_write_time, apiFileStat.last_write_time);
    }

    TEST_FIXTURE(FileFixture, CopyFileNormalResult_ReturnsTrue)
    {
        int error;

        bool copyResult = UnityPalCopyFile(TEST_FILE_NAME, DUPLICATE_TEST_FILE_NAME, true, &error);
        UnityPalDeleteFile(DUPLICATE_TEST_FILE_NAME, &error);

        CHECK(copyResult);
    }

    TEST_FIXTURE(FileFixture, CopyFileNormalError)
    {
        int error;

        UnityPalCopyFile(TEST_FILE_NAME, DUPLICATE_TEST_FILE_NAME, true, &error);
        UnityPalDeleteFile(DUPLICATE_TEST_FILE_NAME, &error);

        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST(CopyFileBadResult_ReturnsFalse)
    {
        int error;
        CHECK(!UnityPalCopyFile(CURRENT_DIRECTORY("#453453"), CURRENT_DIRECTORY("#sdfsdw3"), true, &error));
    }

    TEST(CopyFileBadError)
    {
        int error;
        UnityPalCopyFile(CURRENT_DIRECTORY("#453453"), CURRENT_DIRECTORY("#sdfsdw3"), true, &error);

        CHECK_NOT_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST_FIXTURE(FileFixture, CopyFileNormalStat)
    {
        int error;
        UnityPalFileStat fileStat;

        UnityPalCopyFile(TEST_FILE_NAME, DUPLICATE_TEST_FILE_NAME, true, &error);
        UnityPalGetFileStat(DUPLICATE_TEST_FILE_NAME, &fileStat, &error);
        UnityPalDeleteFile(DUPLICATE_TEST_FILE_NAME, &error);

        CHECK(fileStat.last_write_time > 100000000);
    }

    TEST_FIXTURE(FileFixture, CopyFileNormalResultMatchesClass)
    {
        int error;

        bool api_result = UnityPalCopyFile(TEST_FILE_NAME, DUPLICATE_TEST_FILE_NAME, true, &error);
        UnityPalDeleteFile(DUPLICATE_TEST_FILE_NAME, &error);

        bool class_result = il2cpp::os::File::CopyFile(TEST_FILE_NAME, DUPLICATE_TEST_FILE_NAME, true, &error);
        il2cpp::os::File::DeleteFile(DUPLICATE_TEST_FILE_NAME, &error);

        CHECK_EQUAL(class_result, api_result);
    }

    TEST_FIXTURE(FileFixture, CopyFileNormalErrorMatchesClass)
    {
        int api_error;
        int class_error;

        UnityPalCopyFile(TEST_FILE_NAME, DUPLICATE_TEST_FILE_NAME, true, &api_error);
        UnityPalDeleteFile(DUPLICATE_TEST_FILE_NAME, &api_error);
        il2cpp::os::File::CopyFile(TEST_FILE_NAME, DUPLICATE_TEST_FILE_NAME, true, &class_error);
        il2cpp::os::File::DeleteFile(DUPLICATE_TEST_FILE_NAME, &class_error);

        CHECK_EQUAL(class_error, api_error);
    }

    TEST(CopyFileBadResultMatchesClass)
    {
        int error;
        CHECK_EQUAL((int32_t)il2cpp::os::File::CopyFile(CURRENT_DIRECTORY("#453453"), CURRENT_DIRECTORY("#sdfsdw3"), true, &error), UnityPalCopyFile(CURRENT_DIRECTORY("#453453"), CURRENT_DIRECTORY("#sdfsdw3"), true, &error));
    }

    TEST(CopyFileBadErrorMatchesClass)
    {
        int api_error;
        int class_error;
        UnityPalCopyFile(CURRENT_DIRECTORY("#453453"), CURRENT_DIRECTORY("#sdfsdw3"), true, &api_error);
        il2cpp::os::File::CopyFile(CURRENT_DIRECTORY("#453453"), CURRENT_DIRECTORY("#sdfsdw3"), true, &class_error);

        CHECK_EQUAL(class_error, api_error);
    }

    TEST(MoveFileBadResult_ReturnsFalse)
    {
        int error;
        CHECK(!UnityPalMoveFile(CURRENT_DIRECTORY("#453453"), CURRENT_DIRECTORY("#sdfsdw3"), &error));
    }

    TEST(MoveFileBadError)
    {
        int error;
        UnityPalMoveFile(CURRENT_DIRECTORY("#453453"), CURRENT_DIRECTORY("#sdfsdw3"), &error);

        CHECK_NOT_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST(MoveFileNormalResultMatchesClass)
    {
        il2cpp::os::FileHandle* handle = PrepareTestFile();
        int error;

        bool api_result = UnityPalMoveFile(TEST_FILE_NAME, DUPLICATE_TEST_FILE_NAME, &error);
        UnityPalDeleteFile(DUPLICATE_TEST_FILE_NAME, &error);
        CleanupTestFile(handle);
        handle = PrepareTestFile();
        bool class_result = il2cpp::os::File::MoveFile(TEST_FILE_NAME, DUPLICATE_TEST_FILE_NAME, &error);
        il2cpp::os::File::DeleteFile(DUPLICATE_TEST_FILE_NAME, &error);

        CleanupTestFile(handle);

        CHECK_EQUAL(class_result, api_result);
    }

    TEST(MoveFileNormalErrorMatchesClass)
    {
        il2cpp::os::FileHandle* handle = PrepareTestFile();
        int api_error;
        int class_error;

        UnityPalMoveFile(TEST_FILE_NAME, DUPLICATE_TEST_FILE_NAME, &api_error);
        UnityPalDeleteFile(DUPLICATE_TEST_FILE_NAME, &api_error);
        CleanupTestFile(handle);
        handle = PrepareTestFile();
        il2cpp::os::File::MoveFile(TEST_FILE_NAME, DUPLICATE_TEST_FILE_NAME, &class_error);
        il2cpp::os::File::DeleteFile(DUPLICATE_TEST_FILE_NAME, &class_error);
        CleanupTestFile(handle);

        CHECK_EQUAL(class_error, api_error);
    }

    TEST(MoveFileBadResultMatchesClass)
    {
        int error;
        CHECK_EQUAL((int32_t)il2cpp::os::File::MoveFile(CURRENT_DIRECTORY("#453453"), CURRENT_DIRECTORY("#sdfsdw3"), &error), UnityPalMoveFile(CURRENT_DIRECTORY("#453453"), CURRENT_DIRECTORY("#sdfsdw3"), &error));
    }

    TEST(MoveFileBadErrorMatchesClass)
    {
        int api_error;
        int class_error;
        UnityPalMoveFile(CURRENT_DIRECTORY("#453453"), CURRENT_DIRECTORY("#sdfsdw3"), &api_error);
        il2cpp::os::File::MoveFile(CURRENT_DIRECTORY("#453453"), CURRENT_DIRECTORY("#sdfsdw3"), &class_error);

        CHECK_EQUAL(class_error, api_error);
    }

    TEST_FIXTURE(FileFixture, DeleteFileNormalResult_ReturnsTrue)
    {
        int error;
        CHECK(UnityPalDeleteFile(TEST_FILE_NAME, &error));
    }

    TEST_FIXTURE(FileFixture, DeleteFileNormalError)
    {
        int error;

        UnityPalDeleteFile(TEST_FILE_NAME, &error);

        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST(DeleteFileBadResult_ReturnsFalse)
    {
        int error;
        CHECK(!UnityPalDeleteFile(CURRENT_DIRECTORY("#453453"), &error));
    }


    TEST(DeleteFileBadError)
    {
        int error;
        UnityPalDeleteFile(CURRENT_DIRECTORY("#453453"), &error);

        CHECK_NOT_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST_FIXTURE(FileFixture, DeleteFileNormalStat)
    {
        int error;
        UnityPalFileStat fileStat;

        UnityPalDeleteFile(TEST_FILE_NAME, &error);

        error = il2cpp::os::kErrorCodeSuccess;
        UnityPalGetFileStat(TEST_FILE_NAME, &fileStat, &error);

        CHECK_NOT_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST(DeleteFileNormalResultMatchesClass)
    {
        il2cpp::os::FileHandle* handle = PrepareTestFile();
        int error;

        bool api_result = UnityPalDeleteFile(TEST_FILE_NAME, &error);
        CleanupTestFile(handle);
        handle = PrepareTestFile();
        bool class_result = il2cpp::os::File::DeleteFile(TEST_FILE_NAME, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(class_result, api_result);
    }

    TEST(DeleteFileNormalErrorMatchesClass)
    {
        il2cpp::os::FileHandle* handle = PrepareTestFile();
        int api_error;
        int class_error;

        UnityPalDeleteFile(TEST_FILE_NAME, &api_error);
        CleanupTestFile(handle);
        handle = PrepareTestFile();
        il2cpp::os::File::DeleteFile(TEST_FILE_NAME, &class_error);
        CleanupTestFile(handle);

        CHECK_EQUAL(class_error, api_error);
    }

    TEST(DeleteFileBadResultMatchesClass)
    {
        int error;
        CHECK_EQUAL((int32_t)il2cpp::os::File::DeleteFile(CURRENT_DIRECTORY("#453453"), &error), UnityPalDeleteFile(CURRENT_DIRECTORY("#453453"), &error));
    }

    TEST(DeleteFileBadErrorMatchesClass)
    {
        int api_error;
        int class_error;
        UnityPalDeleteFile(CURRENT_DIRECTORY("#453453"), &api_error);
        il2cpp::os::File::DeleteFile(CURRENT_DIRECTORY("#453453"), &class_error);

        CHECK_EQUAL(class_error, api_error);
    }

    TEST(OpenFileNormalResult_ReturnsNonNullHandle)
    {
        int error;
        UnityPalFileHandle* handle = NULL;

        handle =  UnityPalOpen(TEST_FILE_NAME, kFileModeCreateNew, 0, 0, 0, &error);

        CHECK_NOT_NULL(handle);

        il2cpp::os::File::Close(handle, &error);
        il2cpp::os::File::DeleteFile(TEST_FILE_NAME, &error);
    }

    TEST(OpenFileNormalError)
    {
        int error;
        UnityPalFileHandle* handle = NULL;

        handle = UnityPalOpen(TEST_FILE_NAME, kFileModeCreateNew, 0, 0, 0, &error);

        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, error);

        il2cpp::os::File::Close(handle, &error);
        il2cpp::os::File::DeleteFile(TEST_FILE_NAME, &error);
    }

    TEST(OpenFileNormalStat)
    {
        int error;
        UnityPalFileStat fileStat;
        UnityPalFileHandle* handle = NULL;

        handle = UnityPalOpen(TEST_FILE_NAME, kFileModeCreateNew, 0, 0, 0, &error);
        UnityPalGetFileStat(TEST_FILE_NAME, &fileStat, &error);
        il2cpp::os::File::Close(handle, &error);
        il2cpp::os::File::DeleteFile(TEST_FILE_NAME, &error);

        CHECK(fileStat.last_write_time > 1);
    }

    TEST(OpenFileNormalErrorMatchesClass)
    {
        int error;
        int api_error;
        int class_error;
        UnityPalFileHandle* handle = NULL;

        handle = UnityPalOpen(TEST_FILE_NAME, kFileModeCreateNew, 0, 0, 0, &api_error);
        il2cpp::os::File::Close(handle, &error);
        il2cpp::os::File::DeleteFile(TEST_FILE_NAME, &error);

        handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeCreateNew, 0, 0, 0, &class_error);
        il2cpp::os::File::Close(handle, &error);
        il2cpp::os::File::DeleteFile(TEST_FILE_NAME, &error);

        CHECK_EQUAL(class_error, api_error);
    }

    TEST(OpenFileNormalStatMatchesClass)
    {
        int error;
        UnityPalFileStat api_fileStat;
        il2cpp::os::FileStat class_fileStat;
        UnityPalFileHandle* handle = NULL;

        handle = UnityPalOpen(TEST_FILE_NAME, kFileModeCreateNew, 0, 0, 0, &error);
        UnityPalGetFileStat(TEST_FILE_NAME, &api_fileStat, &error);
        il2cpp::os::File::Close(handle, &error);
        il2cpp::os::File::DeleteFile(TEST_FILE_NAME, &error);

        handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeCreateNew, 0, 0, 0, &error);
        il2cpp::os::File::GetFileStat(TEST_FILE_NAME, &class_fileStat, &error);
        il2cpp::os::File::Close(handle, &error);

        CHECK_EQUAL(class_fileStat.attributes, api_fileStat.attributes);
    }

#if !IL2CPP_USE_GENERIC_FILE
    struct TruncateFixture
    {
        TruncateFixture()
        {
            handle  = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
            WriteSomeCharactersToTestFile(handle);
        }

        ~TruncateFixture()
        {
            CleanupTestFile(handle);
        }

        il2cpp::os::FileHandle* handle;
        int error;
        int64_t length;
    };

    TEST_FIXTURE(TruncateFixture, TruncateReturnsTrue)
    {
        length = UnityPalGetLength(handle, &error);
        UnityPalSeek(handle, 5, 0, &error);
        CHECK(UnityPalTruncate(handle, &error));
    }

    TEST_FIXTURE(TruncateFixture, TruncateFileReturnsCleanErrorCode)
    {
        UnityPalSeek(handle, 5, 0, &error);
        UnityPalTruncate(handle, &error);
        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST_FIXTURE(TruncateFixture, TruncateFileSize14TruncatesToSize5Successfully)
    {
        UnityPalSeek(handle, 5, 0, &error);
        UnityPalTruncate(handle, &error);
        int64_t length = UnityPalGetLength(handle, &error);
        CHECK_EQUAL(5, length);
    }

    TEST_FIXTURE(TruncateFixture, TruncateFileSize14TruncatesToSize20Successfully)
    {
        UnityPalSeek(handle, 20, 0, &error);
        UnityPalTruncate(handle, &error);
        int64_t length = UnityPalGetLength(handle, &error);
        CHECK_EQUAL(20, length);
    }

    TEST(TruncateBadFileReturnsError)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        CleanupTestFile(handle);
        UnityPalTruncate(handle, &error);
        CHECK_NOT_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST(TruncateBadFileReturnsFalse)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        CleanupTestFile(handle);
        CHECK(!UnityPalTruncate(handle, &error));
    }
#endif // IL2CPP_USE_GENERIC_FILE

// The utime function returns -1 on PS4. I'm not sure why.
#if !IL2CPP_TARGET_PS4
    TEST(SetFileTimeNormal)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate,  kFileAccessReadWrite, 0, 0, &error);
        UnityPalFileStat fileStat;
        bool setFileTimeResult = UnityPalSetFileTime(handle, 131360602701336952, 131360602701336952, 131360602701336952, &error);
        UnityPalGetFileStat(TEST_FILE_NAME, &fileStat, &error);
        CleanupTestFile(handle);

        CHECK(setFileTimeResult);
    }

    TEST(SetFileTimeNormalError)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        bool result = UnityPalSetFileTime(handle, 131360602701336952, 131360602701336952, 131360602701336952, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }
#endif

    TEST(SetFileTimeNormalMatchesClass)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        bool api_result = UnityPalSetFileTime(handle, 131360602701336952, 131360602701336952, 131360602701336952, &error);
        bool class_result = il2cpp::os::File::SetFileTime(handle, 131360602701336952, 131360602701336952, 131360602701336952, &error);

        CleanupTestFile(handle);

        CHECK_EQUAL(class_result, api_result);
    }

    struct SetFileTimeNormalStatFixture
    {
        SetFileTimeNormalStatFixture()
        {
            int error;
            il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);

            UnityPalSetFileTime(handle, 131360602701336952, 131360602701336952, 131360602701336952, &error);
            UnityPalGetFileStat(TEST_FILE_NAME, &api_fileStat, &error);

            il2cpp::os::File::SetFileTime(handle, 131360602701336952, 131360602701336952, 131360602701336952, &error);
            il2cpp::os::File::GetFileStat(TEST_FILE_NAME, &class_fileStat, &error);

            CleanupTestFile(handle);
        }

        UnityPalFileStat api_fileStat;
        il2cpp::os::FileStat class_fileStat;
    };

    TEST_FIXTURE(SetFileTimeNormalStatFixture, CreationTimeMatchesClass)
    {
        CHECK_EQUAL(class_fileStat.creation_time, api_fileStat.creation_time);
    }

    TEST_FIXTURE(SetFileTimeNormalStatFixture, LastWriteTimeMatchesClass)
    {
        CHECK_EQUAL(class_fileStat.last_write_time, api_fileStat.last_write_time);
    }

    TEST_FIXTURE(SetFileTimeNormalStatFixture, LastAccessTimeMatchesClass)
    {
        CHECK_EQUAL(class_fileStat.last_access_time, api_fileStat.last_access_time);
    }

    TEST(SetFileTimeNormalErrorMatchesClass)
    {
        int error;
        int api_error;
        int class_error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);

        UnityPalSetFileTime(handle, 131360602701336952, 131360602701336952, 131360602701336952, &api_error);
        il2cpp::os::File::SetFileTime(handle, 131360602701336952, 131360602701336952, 131360602701336952, &class_error);

        CleanupTestFile(handle);

        CHECK_EQUAL(class_error, api_error);
    }

    TEST(GetLengthNormal)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);
        int64_t length = UnityPalGetLength(handle, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(14, length);
    }

    TEST_FIXTURE(FileFixture, GetLengthZero)
    {
        int error;
        CHECK_EQUAL(0, UnityPalGetLength(handle, &error));
    }

    TEST(GetLengthBadFileError)
    {
        int error;
        il2cpp::os::FileHandle* closedHandle = PrepareTestFile();
        CleanupTestFile(closedHandle);
        UnityPalGetLength(closedHandle, &error);

        CHECK_NOT_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST(GetLengthNormalMatchesClass)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);
        int64_t api_length = UnityPalGetLength(handle, &error);
        int64_t class_length = il2cpp::os::File::GetLength(handle, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(class_length, api_length);
    }

    TEST_FIXTURE(FileFixture, GetLengthZeroMatchesClass)
    {
        int error;
        int64_t api_length = UnityPalGetLength(handle, &error);
        int64_t class_length = il2cpp::os::File::GetLength(handle, &error);

        CHECK_EQUAL(class_length, api_length);
    }

    TEST(GetLengthBadFileErrorMatchesClass)
    {
        int api_error;
        int class_error;
        il2cpp::os::FileHandle* closedHandle = PrepareTestFile();
        CleanupTestFile(closedHandle);
        UnityPalGetLength(closedHandle, &api_error);
        il2cpp::os::File::GetLength(closedHandle, &class_error);

        CHECK_EQUAL(class_error, api_error);
    }

    TEST(SetLengthNormalResult)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);

        bool setLengthResult = UnityPalSetLength(handle, TEST_FILE_LENGTH, &error);

        CleanupTestFile(handle);

        CHECK(setLengthResult);
    }

    TEST(SetLengthNormalGetLength)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);

        bool result = UnityPalSetLength(handle, TEST_FILE_LENGTH, &error);
        int64_t length = il2cpp::os::File::GetLength(handle, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(TEST_FILE_LENGTH, length);
    }

    TEST(SetLengthBadResult_ReturnsFalse)
    {
        int error;
        il2cpp::os::FileHandle* closedHandle = PrepareTestFile();
        CleanupTestFile(closedHandle);

        CHECK(!UnityPalSetLength(closedHandle, TEST_FILE_LENGTH, &error));
    }

    TEST(SetLengthBadError)
    {
        int error;
        il2cpp::os::FileHandle* closedHandle = PrepareTestFile();
        CleanupTestFile(closedHandle);

        UnityPalSetLength(closedHandle, TEST_FILE_LENGTH, &error);

        CHECK_NOT_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST(SetLengthNormalResultMatchesClass)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);

        bool api_result = UnityPalSetLength(handle, TEST_FILE_LENGTH, &error);

        CleanupTestFile(handle);
        handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        bool class_result = il2cpp::os::File::SetLength(handle, TEST_FILE_LENGTH, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(class_result, api_result);
    }

    TEST(SetLengthNormalGetLengthMatchesClass)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);

        UnityPalSetLength(handle, TEST_FILE_LENGTH, &error);
        int64_t api_length = il2cpp::os::File::GetLength(handle, &error);
        CleanupTestFile(handle);

        handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        il2cpp::os::File::SetLength(handle, TEST_FILE_LENGTH, &error);
        int64_t class_length = il2cpp::os::File::GetLength(handle, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(class_length, api_length);
    }

    TEST(SetLengthBadResultMatchesClass)
    {
        int error;
        il2cpp::os::FileHandle* closedHandle = PrepareTestFile();
        CleanupTestFile(closedHandle);

        bool api_result = UnityPalSetLength(closedHandle, TEST_FILE_LENGTH, &error);
        bool class_result = il2cpp::os::File::SetLength(closedHandle, TEST_FILE_LENGTH, &error);

        CHECK_EQUAL(class_result, api_result);
    }

    TEST(SetLengthBadErrorMatchesClass)
    {
        int api_error;
        int class_error;
        il2cpp::os::FileHandle* closedHandle = PrepareTestFile();
        CleanupTestFile(closedHandle);

        UnityPalSetLength(closedHandle, TEST_FILE_LENGTH, &api_error);
        il2cpp::os::File::SetLength(closedHandle, TEST_FILE_LENGTH, &class_error);

        CHECK_EQUAL(class_error, api_error);
    }

    TEST(SeekNormalResult)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);

        int64_t result = UnityPalSeek(handle, 10, 0, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(10, result);
    }

    TEST(SeekNormalBuffer)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);

        int64_t result = UnityPalSeek(handle, 10, 0, &error);
        char buffer[15];
        il2cpp::os::File::Read(handle, buffer, 4, &error);

        CleanupTestFile(handle);

        CHECK_EQUAL(0, strncmp("TEST", buffer, 4));
    }

    TEST(SeekBadError)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        CleanupTestFile(handle);

        UnityPalSeek(handle, 10, 0, &error);

        CHECK_NOT_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST(SeekNormalResultMatchesClass)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);

        int64_t api_result = UnityPalSeek(handle, 10, 0, &error);
        CleanupTestFile(handle);

        handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);

        int64_t class_result = il2cpp::os::File::Seek(handle, 10, 0, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(class_result, api_result);
    }

    TEST(SeekNormalBufferMatchesClass)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);

        UnityPalSeek(handle, 10, 0, &error);
        char api_buffer[15];
        char class_buffer[15];
        il2cpp::os::File::Read(handle, api_buffer, 4, &error);

        CleanupTestFile(handle);


        handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);

        il2cpp::os::File::Seek(handle, 10, 0, &error);
        il2cpp::os::File::Read(handle, class_buffer, 4, &error);

        CleanupTestFile(handle);

        CHECK_EQUAL(0, strncmp(class_buffer, api_buffer, 4));
    }

    TEST(SeekBadErrorMatchesClass)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        CleanupTestFile(handle);

        UnityPalSeek(handle, 10, 0, &error);

        CHECK_NOT_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST(ReadNormalResult)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);

        char buffer[16];
        UnityPalSeek(handle, 0, 0, &error);
        int result = UnityPalRead(handle, buffer, 16, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(14, result);
    }

    TEST(ReadNormalBuffer)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);

        char buffer[16];
        UnityPalSeek(handle, 0, 0, &error);
        UnityPalRead(handle, buffer, 16, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(0, strncmp(TEST_STRING, buffer, 14));
    }

    TEST(ReadBadResult)
    {
        int usused;
        char buffer[16];
        int64_t result = UnityPalRead(NULL, buffer, 16, &usused);

        CHECK(!result);
    }

    TEST(ReadBadError)
    {
        int error;
        char buffer[16];
        UnityPalRead(NULL, buffer, 16, &error);

        CHECK_NOT_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST(ReadNormalResultMatchesClass)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);

        UnityPalSeek(handle, 0, 0, &error);
        char buffer[16];
        int api_result = UnityPalRead(handle, buffer, 14, &error);
        CleanupTestFile(handle);

        handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);

        il2cpp::os::File::Seek(handle, 0, 0, &error);
        int class_result = il2cpp::os::File::Read(handle, buffer, 14, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(class_result, api_result);
    }

    TEST(ReadNormalBufferMatchesClass)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);

        UnityPalSeek(handle, 0, 0, &error);
        char api_buffer[16];
        char class_buffer[16];
        UnityPalRead(handle, api_buffer, 14, &error);
        CleanupTestFile(handle);

        handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);

        il2cpp::os::File::Seek(handle, 0, 0, &error);
        il2cpp::os::File::Read(handle, class_buffer, 14, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(0, strncmp(class_buffer, api_buffer, 14));
    }


    TEST(ReadBadResultMatchesClass)
    {
        int unused;
        char buffer[16];

        int64_t api_result = UnityPalRead(NULL, buffer, 14, &unused);
        int64_t class_result = il2cpp::os::File::Read(NULL, buffer, 14, &unused);

        CHECK_EQUAL(class_result, api_result);
    }

    TEST(ReadBadErrorMatchesClass)
    {
        int api_error;
        int class_error;

        char buffer[16];

        UnityPalRead(NULL, buffer, 14, &api_error);
        il2cpp::os::File::Read(NULL, buffer, 14, &class_error);

        CHECK_EQUAL(class_error, api_error);
    }

    TEST(WriteNormalResult)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);

        static const char* buffer = TEST_STRING;

        int32_t result = UnityPalWrite(handle, buffer, 14, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(14, result);
    }

    TEST(WriteNormalError)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);

        static const char* buffer = TEST_STRING;

        UnityPalWrite(handle, buffer, 14, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST(WriteNormalBufferCheck)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);

        static const char* buffer = TEST_STRING;
        UnityPalWrite(handle, buffer, 14, &error);

        char read_buffer[16];
        il2cpp::os::File::Seek(handle, 0, 0, &error);
        il2cpp::os::File::Read(handle, read_buffer, 16, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(0, strncmp(TEST_STRING, buffer, 14));
    }

    TEST(WriteBadResult)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        CleanupTestFile(handle);
        static const char* buffer = TEST_STRING;

        int32_t result = UnityPalWrite(handle, buffer, 14, &error);


        CHECK_EQUAL(-1, result);
    }

    TEST(WriteNormalResultMatchesClass)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);

        static const char* buffer = TEST_STRING;

        int32_t api_result = UnityPalWrite(handle, buffer, 14, &error);
        int32_t class_result = il2cpp::os::File::Write(handle, buffer, 14, &error);

        CleanupTestFile(handle);

        CHECK_EQUAL(class_result, api_result);
    }

    TEST(WriteNormalBufferCheckMatchesClass)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);

        static const char* buffer = TEST_STRING;
        UnityPalWrite(handle, buffer, 14, &error);

        char api_read_buffer[16];
        il2cpp::os::File::Seek(handle, 0, 0, &error);
        il2cpp::os::File::Read(handle, api_read_buffer, 16, &error);
        CleanupTestFile(handle);

        handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        il2cpp::os::File::Write(handle, buffer, 14, &error);
        char class_read_buffer[16];
        il2cpp::os::File::Seek(handle, 0, 0, &error);
        il2cpp::os::File::Read(handle, class_read_buffer, 16, &error);
        CleanupTestFile(handle);


        CHECK_EQUAL(0, strncmp(class_read_buffer, api_read_buffer, 14));
    }

    TEST(WriteBadResultMatchesClass)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        CleanupTestFile(handle);
        static const char* buffer = TEST_STRING;

        int32_t api_result = UnityPalWrite(handle, buffer, 14, &error);
        int32_t class_result = il2cpp::os::File::Write(handle, buffer, 14, &error);

        CHECK_EQUAL(class_result, api_result);
    }

    TEST(WriteBadErrorMatchesClass)
    {
        int error;
        int api_error;
        int class_error;

        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, 1, 1, 0, 0, &error);
        CleanupTestFile(handle);
        static const char* buffer = TEST_STRING;

        UnityPalWrite(handle, buffer, 14, &api_error);
        il2cpp::os::File::Write(handle, buffer, 14, &class_error);

        CHECK_EQUAL(api_error, class_error);
    }

    TEST(FlushNormalResult_ReturnsTrue)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);

        bool flushResult = UnityPalFlush(handle, &error);
        CleanupTestFile(handle);

        CHECK(flushResult);
    }

    TEST(FlushNormalError)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);

        UnityPalFlush(handle, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }

    TEST(FlushBadResult_ReturnsFalse)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);
        CleanupTestFile(handle);

        CHECK(!UnityPalFlush(handle, &error));
    }

    TEST(FlushBadError)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);
        CleanupTestFile(handle);

        UnityPalFlush(handle, &error);

        CHECK_NOT_EQUAL(il2cpp::os::kErrorCodeSuccess, error);
    }


    TEST(FlushNormalResultMatchesClass)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);

        WriteSomeCharactersToTestFile(handle);
        bool api_result = UnityPalFlush(handle, &error);
        WriteSomeCharactersToTestFile(handle);
        bool class_result = il2cpp::os::File::Flush(handle, &error);
        CleanupTestFile(handle);

        CHECK_EQUAL(class_result, api_result);
    }

    TEST(FlushNormalErrorMatchesClass)
    {
        int error;
        int api_error;
        int class_error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);

        WriteSomeCharactersToTestFile(handle);
        UnityPalFlush(handle, &api_error);
        WriteSomeCharactersToTestFile(handle);
        il2cpp::os::File::Flush(handle, &class_error);
        CleanupTestFile(handle);

        CHECK_EQUAL(class_error, api_error);
    }

    TEST(FlushBadResultMatchesClass)
    {
        int error;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);
        CleanupTestFile(handle);

        bool api_result = UnityPalFlush(handle, &error);
        bool class_result = il2cpp::os::File::Flush(handle, &error);

        CHECK_EQUAL(class_result, api_result);
    }

    TEST(FlushBadErrorMatchesClass)
    {
        int error;
        int api_error;
        int class_error;

        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(TEST_FILE_NAME, kFileModeOpenOrCreate, kFileAccessReadWrite, 0, 0, &error);
        WriteSomeCharactersToTestFile(handle);
        CleanupTestFile(handle);

        UnityPalFlush(handle, &api_error);
        il2cpp::os::File::Flush(handle, &class_error);

        CHECK_EQUAL(class_error, api_error);
    }

#if !IL2CPP_TARGET_PS4 && !IL2CPP_TARGET_WINRT && !IL2CPP_TARGET_XBOXONE
    TEST(CreatePipeNormalResult_ReturnsTrue)
    {
        int error;
        il2cpp::os::FileHandle* read_handle;
        il2cpp::os::FileHandle* write_handle;

        bool createPipeResult = UnityPalCreatePipe(&read_handle, &write_handle);
        il2cpp::os::File::Close(read_handle, &error);
        il2cpp::os::File::Close(write_handle, &error);

        CHECK(createPipeResult);
    }

    TEST(CreatePipeNormalBuffer)
    {
        int error;
        il2cpp::os::FileHandle* read_handle;
        il2cpp::os::FileHandle* write_handle;

        UnityPalCreatePipe(&read_handle, &write_handle);

        WriteSomeCharactersToTestFile(write_handle);
        char buffer[16];
        il2cpp::os::File::Read(read_handle, buffer, 4, &error);

        il2cpp::os::File::Close(read_handle, &error);
        il2cpp::os::File::Close(write_handle, &error);

        CHECK_EQUAL(0, strncmp("THIS", buffer, 4));
    }

    TEST(CreatePipeNormalResultMatchesClass)
    {
        int error;
        il2cpp::os::FileHandle* read_handle;
        il2cpp::os::FileHandle* write_handle;

        bool api_result = UnityPalCreatePipe(&read_handle, &write_handle);
        il2cpp::os::File::Close(read_handle, &error);
        il2cpp::os::File::Close(write_handle, &error);

        bool class_result = il2cpp::os::File::CreatePipe(&read_handle, &write_handle).Get();
        il2cpp::os::File::Close(read_handle, &error);
        il2cpp::os::File::Close(write_handle, &error);

        CHECK_EQUAL(class_result, api_result);
    }

    TEST(CreatePipeNormalBufferMatchesClass)
    {
        int error;
        il2cpp::os::FileHandle* api_read_handle;
        il2cpp::os::FileHandle* api_write_handle;
        il2cpp::os::FileHandle* class_read_handle;
        il2cpp::os::FileHandle* class_write_handle;

        UnityPalCreatePipe(&api_read_handle, &api_write_handle);

        WriteSomeCharactersToTestFile(api_write_handle);
        char api_buffer[16];
        il2cpp::os::File::Read(api_read_handle, api_buffer, 4, &error);
        il2cpp::os::File::Close(api_read_handle, &error);
        il2cpp::os::File::Close(api_write_handle, &error);

        il2cpp::os::File::CreatePipe(&class_read_handle, &class_write_handle);
        WriteSomeCharactersToTestFile(class_write_handle);
        char class_buffer[16];
        il2cpp::os::File::Read(class_read_handle, class_buffer, 4, &error);
        il2cpp::os::File::Close(class_read_handle, &error);
        il2cpp::os::File::Close(class_write_handle, &error);

        CHECK_EQUAL(0, strncmp(class_buffer, api_buffer, 4));
    }
#endif

#if IL2CPP_CAN_CHECK_EXECUTABLE

#if !IL2CPP_TARGET_WINDOWS
#include <sys/stat.h>
#endif

    struct ExecutableFileFixture
    {
        ExecutableFileFixture() : executableFilename("foo.exe"), nonexecutableFilename("foo")
        {
            int unused;
            executableFile = il2cpp::os::File::Open(executableFilename, kFileModeCreate, 0, 0, 0, &unused);
            nonexecutableFile = il2cpp::os::File::Open(nonexecutableFilename, kFileModeCreate, 0, 0, 0, &unused);

#if !IL2CPP_TARGET_WINDOWS
            chmod(executableFilename, S_IRUSR | S_IWUSR | S_IXUSR);
#endif
        }

        ~ExecutableFileFixture()
        {
            int unused;
            il2cpp::os::File::Close(executableFile, &unused);
            il2cpp::os::File::DeleteFile(executableFilename, &unused);
            il2cpp::os::File::Close(nonexecutableFile, &unused);
            il2cpp::os::File::DeleteFile(nonexecutableFilename, &unused);
        }

        const char* executableFilename;
        const char* nonexecutableFilename;
        il2cpp::os::FileHandle* executableFile;
        il2cpp::os::FileHandle* nonexecutableFile;
    };

#if IL2CPP_TARGET_WINDOWS
    TEST_FIXTURE(ExecutableFileFixture, IsExecutable_ReturnsTrueForFileEndingInExe)
    {
        CHECK_MSG(UnityPalIsExecutable(executableFilename), "A file ending in the .exe extension is not reported as executable, which is not expected.");
    }

    TEST_FIXTURE(ExecutableFileFixture, IsExecutable_ReturnsFalseForFileNotEndingInExe)
    {
        CHECK_MSG(!UnityPalIsExecutable(nonexecutableFilename), "A file not ending in the .exe extension is reported as executable, which is not expected.");
    }

#else
    TEST_FIXTURE(ExecutableFileFixture, IsExecutable_ReturnsTrueForFileWithExecutablePermissions)
    {
        CHECK_MSG(UnityPalIsExecutable(executableFilename), "A file with executable permissions is not reported as executable, which is not expected.");
    }

    TEST_FIXTURE(ExecutableFileFixture, IsExecutable_ReturnsFalseForFileWithoutExecutablePermissions)
    {
        CHECK_MSG(!UnityPalIsExecutable(nonexecutableFilename), "A file without executable permissions is reported as executable, which is not expected.");
    }

#endif

#endif // IL2CPP_CAN_CHECK_EXECUTABLE
}

#endif // ENABLE_UNIT_TESTS
