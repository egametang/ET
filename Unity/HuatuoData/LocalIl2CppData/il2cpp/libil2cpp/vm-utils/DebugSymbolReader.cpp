#include "il2cpp-config.h"

#include "os/File.h"
#include "os/Image.h"
#include "os/Path.h"
#include "utils/Logging.h"
#include "utils/Memory.h"
#include "utils/MemoryMappedFile.h"
#include "utils/PathUtils.h"
#include "utils/StringView.h"
#include "vm-utils/DebugSymbolReader.h"
#include <string>

#if IL2CPP_TARGET_ARM64E
#include <ptrauth.h>
#endif

namespace il2cpp
{
namespace utils
{
#if !IL2CPP_TINY
    struct usymliteHeader
    {
        uint32_t magic;
        uint32_t version;
        uint32_t lineCount;
        uint32_t id; // executable's id, offset in string table
        uint32_t os;
        uint32_t arch;
    };

    struct usymliteLine
    {
        uint64_t address;
        uint32_t fileName; // Reference to the managed source file name in the string table
        uint32_t line; // Managed line number
    };

    struct Reader
    {
        void* debugSymbolData;
        const usymliteLine* lines;
        const char* strings;
        usymliteHeader header;
        std::string uuid;
        std::string os;
        std::string arch;
        uint64_t firstLineAddress;
        uint64_t lastLineAddress;
        uint32_t maxStringIndex;
    };

    static Reader s_usym = { 0 };

    const int headerSize = 24;
    const int lineSize = 16;
    const uint32_t magicUsymlite = 0x2D6D7973; // "sym-"
    const uint32_t noLine = 0xFFFFFFFF;

    // Do a binary search to find the line with the given address
    // This is looking for the line with the closest address without going over (price is right style)
    usymliteLine FindLine(uint64_t address)
    {
        uint32_t head = 0;
        uint32_t tail = s_usym.header.lineCount - 1;

        while (head < tail)
        {
            uint32_t mid = (head + tail + 1) / 2;
            uint64_t midAddr = s_usym.lines[mid].address;

            if (address < midAddr)
            {
                tail = mid - 1;
            }
            else
            {
                head = mid;
            }
        }

        return s_usym.lines[head];
    }

    const char* GetString(uint32_t index)
    {
        IL2CPP_ASSERT(index < s_usym.maxStringIndex);
        return s_usym.strings + index;
    }

    bool DebugSymbolReader::LoadDebugSymbols()
    {
        int error = 0;
        std::string symbolsPath;
        std::string executablePath = os::Path::GetExecutablePath();
        if (!executablePath.empty())
        {
            symbolsPath = PathUtils::Combine(PathUtils::DirectoryName(executablePath), StringView<char>("il2cpp.usym"));
        }
        else
        {
            return false;
        }

        os::FileHandle *handle =
            os::File::Open(symbolsPath.c_str(), kFileModeOpen, kFileAccessRead, kFileShareRead, kFileOptionsNone, &error);

        if (error != 0)
        {
            return false;
        }

        s_usym.debugSymbolData = utils::MemoryMappedFile::Map(handle);
        int64_t length = os::File::GetLength(handle, &error);

        os::File::Close(handle, &error);
        if (error != 0)
        {
            utils::MemoryMappedFile::Unmap(s_usym.debugSymbolData);
            s_usym.debugSymbolData = NULL;
            return false;
        }

        s_usym.header = *(usymliteHeader *)((char *)s_usym.debugSymbolData);

        if (s_usym.header.magic != magicUsymlite)
        {
            utils::MemoryMappedFile::Unmap(s_usym.debugSymbolData);
            s_usym.debugSymbolData = NULL;
            return false;
        }

        int64_t lineOffset = headerSize;
        int64_t stringOffset = lineOffset + (s_usym.header.lineCount * lineSize);

        s_usym.maxStringIndex = (uint32_t)(length - stringOffset);
        s_usym.lines = (const usymliteLine*)((const char *)s_usym.debugSymbolData + lineOffset);
        s_usym.strings = ((const char *)s_usym.debugSymbolData + stringOffset);

#if IL2CPP_ENABLE_NATIVE_INSTRUCTION_POINTER_EMISSION
        const char* our_uuid = os::Image::GetImageUUID();
        s_usym.uuid = std::string(GetString(s_usym.header.id));

        if (s_usym.uuid != std::string(our_uuid))
        {
            // UUID mismatch means this usymfile is not for this program
            il2cpp::utils::Logging::Write("Ignoring symbol file due to UUID mismatch. File contains %s but expected %s.", s_usym.uuid.c_str(), our_uuid);
            utils::MemoryMappedFile::Unmap(s_usym.debugSymbolData);
            s_usym.debugSymbolData = NULL;
            s_usym.lines = NULL;
            s_usym.strings = NULL;
            return false;
        }

        IL2CPP_FREE((void*)our_uuid);
#endif

        s_usym.os = std::string(GetString(s_usym.header.os));
        s_usym.arch = std::string(GetString(s_usym.header.arch));

        s_usym.firstLineAddress = s_usym.lines[0].address;
        s_usym.lastLineAddress = s_usym.lines[s_usym.header.lineCount - 1].address;

        return true;
    }

    bool DebugSymbolReader::GetSourceLocation(void *nativeInstructionPointer, SourceLocation &sourceLocation)
    {
        if (s_usym.debugSymbolData == NULL || nativeInstructionPointer == NULL)
        {
            return false;
        }

        // The instruction pointer points to the next address, so to get the address we came from, we subtract 1.
        // findLine matches the address to the closest address <= the one we give, so it finds the one we need
        uint64_t adjustedAddress = ((uint64_t)nativeInstructionPointer) - ((uint64_t)os::Image::GetImageBase()) - 1;

#if IL2CPP_TARGET_ANDROID
        // We don't seem to need to subtract by one for Android
        // https://github.com/Unity-Technologies/unity-services-crash/commit/50611fcf29a1d876689942ed1f1cdca23e32c522
        adjustedAddress += 1;
#endif

#if IL2CPP_TARGET_ARM64E
        adjustedAddress = (uint64_t)ptrauth_strip((void*)adjustedAddress, ptrauth_key_return_address);
#endif

        // Quick check to remove anything outside the range
        if (adjustedAddress < s_usym.firstLineAddress || s_usym.lastLineAddress < adjustedAddress)
        {
            return false;
        }

        usymliteLine line = FindLine(adjustedAddress);

        // End of symbol entries are placed to indicate that we're past the end of a C# function.
        // These EOS entries have their Line and FileName set to 0xFFFFFFFF
        if (line.line == noLine)
        {
            return false;
        }

        sourceLocation.lineNumber = line.line;
        sourceLocation.filePath = GetString(line.fileName);

        return true;
    }

#endif

    bool DebugSymbolReader::DebugSymbolsAvailable()
    {
        #if IL2CPP_TINY
        return false;
        #elif IL2CPP_MONO_DEBUGGER
        return true;
        #else
        return s_usym.debugSymbolData != NULL;
        #endif
    }
} /* namespace utils */
} /* namespace il2cpp */
