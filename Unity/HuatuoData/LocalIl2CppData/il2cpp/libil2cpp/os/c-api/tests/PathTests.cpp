#if ENABLE_UNIT_TESTS

#include "il2cpp-config.h"

#include "UnitTest++.h"

#include "../Path-c-api.h"
#include "../../Path.h"
#include "../../../utils/PathUtils.h"

SUITE(Path)
{
    TEST(GetTempPathNotNull)
    {
        CHECK_NOT_NULL(UnityPalGetTempPath());
    }

    TEST(GetTempPathSanity)
    {
        // Path will never be the same depeding on user, platform,machine etc.  Check if the string is
        // at least a few chars long
        CHECK(strlen(UnityPalGetTempPath()) > 3);
    }

    TEST(GetTempPathTestMatchesClass)
    {
        CHECK_EQUAL(il2cpp::os::Path::GetTempPath().c_str(), UnityPalGetTempPath());
    }

    TEST(GetExecutablePathNotNull)
    {
        CHECK_NOT_NULL(UnityPalGetExecutablePath());
    }

#if !IL2CPP_TARGET_PS4 // PS4 doesn't have an executable path concept - it returns an empty string.
    TEST(GetExecutablePathIsEmpty)
    {
        // Path will never be the same depeding on user, platform,machine etc.  Check if the string is
        // at least a few chars long
        CHECK(strlen(UnityPalGetExecutablePath()) > 5);
    }
#else
    TEST(GetExecutablePathIsEmpty)
    {
        CHECK_EQUAL(UnityPalGetExecutablePath(), "");
    }
#endif

    TEST(GetExecutablePathMatchesClass)
    {
        CHECK_EQUAL(il2cpp::os::Path::GetExecutablePath().c_str(), UnityPalGetExecutablePath());
    }

    static std::string FormatExpectedAbsolutePathMessage(const char* input)
    {
        return std::string("The path '") + std::string(input) + std::string("' is not reported as absolute, which is not expected.");
    }

    static std::string FormatExpectedNotAbsolutePathMessage(const char* input)
    {
        return std::string("The path '") + std::string(input) + std::string("' is reported as absolute, which is not expected.");
    }

    enum PathType
    {
        kAbsolute,
        kRelative
    };

    static void VerifyPathIs(PathType type, const char* input)
    {
        if (type == kAbsolute)
            CHECK_MSG(UnityPalIsAbsolutePath(input), FormatExpectedAbsolutePathMessage(input).c_str());
        else
            CHECK_MSG(!UnityPalIsAbsolutePath(input), FormatExpectedNotAbsolutePathMessage(input).c_str());
    }

    TEST(PathThatIsNull_IsNotAbsolute)
    {
        VerifyPathIs(kRelative, NULL);
    }

#if defined(WINDOWS)
    TEST(PathThatIsEmpty_IsNotAbsolute)
    {
        VerifyPathIs(kRelative, "");
    }

    TEST(PathThatIsWithoutDriveLetter_IsNotAbsolute)
    {
        VerifyPathIs(kRelative, "home\test");
    }

    TEST(PathThatIsDriveLetterWithSeparator_IsNotAbsolute)
    {
        VerifyPathIs(kRelative, "C:");
    }

    TEST(PathThatIsDriveLetterWithBackslash_IsAbsolute)
    {
        VerifyPathIs(kAbsolute, "C:\\");
    }

    TEST(PathThatIsDriveLetterWithBackslashFollowedByAnything_IsAbsolute)
    {
        VerifyPathIs(kAbsolute, "C:\\bar");
    }

    TEST(PathThatIsDriveLetterWithForwardslash_IsAbsolute)
    {
        VerifyPathIs(kAbsolute, "C:/");
    }

    TEST(PathThatIsDoubleBackslash_IsNotAbsolute)
    {
        VerifyPathIs(kRelative, "\\\\");
    }

    TEST(PathThatIsDoubleBackslashFollowedByAnything_IsAbsolute)
    {
        VerifyPathIs(kAbsolute, "\\\\bar");
    }

    TEST(IsAbsoluteMatchesForAbsolutePathClass)
    {
        std::string input = "C:\\";
        CHECK_EQUAL(static_cast<int32_t>(il2cpp::os::Path::IsAbsolute(input)), UnityPalIsAbsolutePath(input.c_str()));
    }

    TEST(IsAbsoluteMatchesForRelativePathClass)
    {
        std::string input = "test\\path";
        CHECK_EQUAL(static_cast<int32_t>(il2cpp::os::Path::IsAbsolute(input)), UnityPalIsAbsolutePath(input.c_str()));
    }
#else
    TEST(PathThatIsOnlyForwardSlash_IsAbsolute)
    {
        VerifyPathIs(kAbsolute, "/");
    }

    TEST(PathThatStartWithForwardSlash_IsAbsolute)
    {
        VerifyPathIs(kAbsolute, "/test/path/foo");
    }

    TEST(PathThatDoesNotStartWithAForwardSlash_INotAbsolute)
    {
        VerifyPathIs(kRelative, "test/path/foo");
    }

    TEST(IsAbsoluteMatchesForAbsolutePathClass)
    {
        std::string input = "/home/foo";
        CHECK_EQUAL(static_cast<int32_t>(il2cpp::os::Path::IsAbsolute(input)), UnityPalIsAbsolutePath(input.c_str()));
    }

    TEST(IsAbsoluteMatchesForRelativePathClass)
    {
        std::string input = "home/path";
        CHECK_EQUAL(static_cast<int32_t>(il2cpp::os::Path::IsAbsolute(input)), UnityPalIsAbsolutePath(input.c_str()));
    }
#endif

    TEST(BasenameForNullPath_ReturnsNull)
    {
        CHECK_NULL(UnityPalBasename(NULL));
    }

    TEST(BasenameForEmptyPath_ReturnsDot)
    {
        char* basename = UnityPalBasename("");
        CHECK_EQUAL(".", basename);
        free(basename);
    }

    TEST(BasenameForEmptyPath_MatchesClass)
    {
        std::string input = "";
        char* basename = UnityPalBasename(input.c_str());
        CHECK_EQUAL(il2cpp::utils::PathUtils::Basename(input), basename);
        free(basename);
    }
#if defined(WINDOWS)
    TEST(BasenameWithBackslashes_ReturnsLastPathElement)
    {
        char* basename = UnityPalBasename("test\\path\\foo");
        CHECK_EQUAL("foo", basename);
        free(basename);
    }

    TEST(BasenameWithBackslashes_MatchesClass)
    {
        std::string input = "test\\path\\foo";
        char* basename = UnityPalBasename(input.c_str());
        CHECK_EQUAL(il2cpp::utils::PathUtils::Basename(input), basename);
        free(basename);
    }
#else
    TEST(BasenameWithForwardslashes_ReturnsLastPathElement)
    {
        char* basename = UnityPalBasename("test/path/foo");
        CHECK_EQUAL("foo", basename);
        free(basename);
    }

    TEST(BasenameWithForwardslashes_MatchesClass)
    {
        std::string input = "test/path/foo";
        char* basename = UnityPalBasename(input.c_str());
        CHECK_EQUAL(il2cpp::utils::PathUtils::Basename(input), basename);
        free(basename);
    }
#endif

    TEST(BasenameWithoutSeparators_ReturnsInput)
    {
        char* basename = UnityPalBasename("bar");
        CHECK_EQUAL("bar", basename);
        free(basename);
    }

    TEST(BasenameWithoutSeparators_MatchesClass)
    {
        std::string input = "bar";
        char* basename = UnityPalBasename(input.c_str());
        CHECK_EQUAL(il2cpp::utils::PathUtils::Basename(input), basename);
        free(basename);
    }

    TEST(DirectoryNameForNullPath_ReturnsNull)
    {
        CHECK_NULL(UnityPalDirectoryName(NULL));
    }

    TEST(DirectoryNameForEmptyPath_ReturnsEmptyString)
    {
        char* DirectoryName = UnityPalDirectoryName("");
        CHECK_EQUAL("", DirectoryName);
        free(DirectoryName);
    }

    TEST(DirectoryNameForEmptyPath_MatchesClass)
    {
        std::string input = "";
        char* DirectoryName = UnityPalDirectoryName(input.c_str());
        CHECK_EQUAL(il2cpp::utils::PathUtils::DirectoryName(input), DirectoryName);
        free(DirectoryName);
    }
#if defined(WINDOWS)
    TEST(DirectoryNameWithBackslashes_ReturnsLastPathElement)
    {
        char* DirectoryName = UnityPalDirectoryName("test\\path\\foo");
        CHECK_EQUAL("test\\path", DirectoryName);
        free(DirectoryName);
    }

    TEST(DirectoryNameWithBackslashes_MatchesClass)
    {
        std::string input = "test\\path\\foo";
        char* DirectoryName = UnityPalDirectoryName(input.c_str());
        CHECK_EQUAL(il2cpp::utils::PathUtils::DirectoryName(input), DirectoryName);
        free(DirectoryName);
    }
#else
    TEST(DirectoryNameWithForwardslashes_ReturnsLastPathElement)
    {
        char* DirectoryName = UnityPalDirectoryName("test/path/foo");
        CHECK_EQUAL("test/path", DirectoryName);
        free(DirectoryName);
    }

    TEST(DirectoryNameWithForwardslashes_MatchesClass)
    {
        std::string input = "test/path/foo";
        char* DirectoryName = UnityPalDirectoryName(input.c_str());
        CHECK_EQUAL(il2cpp::utils::PathUtils::DirectoryName(input), DirectoryName);
        free(DirectoryName);
    }
#endif

    TEST(DirectoryNameWithoutSeparators_ReturnsDot)
    {
        char* DirectoryName = UnityPalDirectoryName("bar");
        CHECK_EQUAL(".", DirectoryName);
        free(DirectoryName);
    }

    TEST(DirectoryNameWithoutSeparators_MatchesClass)
    {
        std::string input = "bar";
        char* DirectoryName = UnityPalDirectoryName(input.c_str());
        CHECK_EQUAL(il2cpp::utils::PathUtils::DirectoryName(input), DirectoryName);
        free(DirectoryName);
    }
}

#endif // ENABLE_UNIT_TESTS
