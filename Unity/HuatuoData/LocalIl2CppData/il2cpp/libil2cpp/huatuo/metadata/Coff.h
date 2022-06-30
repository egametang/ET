#pragma once
#include "../CommonDef.h"


namespace huatuo
{
namespace metadata
{
    struct PEHeader
    {
        uint16_t matchine;
        uint16_t sections;
        uint32_t timestamp;
        uint32_t ptrSymbolTable;
        uint32_t numSymbols;
        uint16_t optionalHeadersize;
        uint16_t characteristics;
    };

    struct PEDirEntry
    {
        uint32_t rva;
        uint32_t size;
    };

    struct CLIHeader
    {
        uint32_t cb;
        uint16_t majorRuntimeVersion;
        uint16_t minorRuntimeVersion;
        PEDirEntry metaData;
        uint32_t flags;
        uint32_t entryPointToken;
        PEDirEntry resources;
        uint64_t strongNameSignature;
        uint64_t codeManagerTable;
        uint64_t vTableFixups;
        uint64_t exportAddressTableJumps;
        uint64_t managedNativeHeader;
    };


    struct PESectionHeader
    {
        char name[8];
        uint32_t virtualSize;
        uint32_t virtualAddress;
        uint32_t sizeOfRawData;
        uint32_t ptrRawData;
        uint32_t ptrRelocations;
        uint32_t ptrLineNumbers;
        uint16_t numRelocation;
        uint16_t numLineNumber;
        uint32_t characteristics;
    };

    struct MetadataRootPartial
    {
        uint32_t signature;
        uint16_t majorVersion;
        uint16_t minorVersion;
        uint32_t reserved;
        uint32_t length;
        byte    versionFirstByte;
    };

    struct StreamHeader
    {
        uint32_t offset;
        uint32_t size;
        char name[1];
    };

    struct TableStreamHeader
    {
        uint32_t reserved;
        uint8_t majorVersion;
        uint8_t minorVersion;
        uint8_t heapSizes;
        uint8_t reserved2;
        uint64_t valid;
        uint64_t sorted;
        uint32_t rows[1];
        // tables;
    };

    struct CliStream
    {
        const char* name;
        const byte* data;
        uint32_t size;
    };

    struct UserString
    {
        const char* data;
        uint32_t size;
        uint8_t flags;
    };

    struct Blob
    {
        const byte* data;
        uint32_t size;
    };

    struct Table
    {
        const byte* data;
        uint32_t rowMetaDataSize;
        uint32_t rowNum;
        bool vaild;
        bool sorted;
    };
}
}
