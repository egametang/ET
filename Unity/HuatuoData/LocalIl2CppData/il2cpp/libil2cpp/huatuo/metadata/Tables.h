#pragma once
#include <stdint.h>

namespace huatuo
{
namespace metadata
{

    enum class TableType
    {
        MODULE,
        TYPEREF,
        TYPEDEF,
        FIELD_POINTER,
        FIELD,
        METHOD_POINTER,
        METHOD,
        PARAM_POINTER,
        PARAM,
        INTERFACEIMPL,
        MEMBERREF, /* 0xa */
        CONSTANT,
        CUSTOMATTRIBUTE,
        FIELDMARSHAL,
        DECLSECURITY,
        CLASSLAYOUT,
        FIELDLAYOUT, /* 0x10 */
        STANDALONESIG,
        EVENTMAP,
        EVENT_POINTER,
        EVENT,
        PROPERTYMAP,
        PROPERTY_POINTER,
        PROPERTY,
        METHODSEMANTICS,
        METHODIMPL,
        MODULEREF, /* 0x1a */
        TYPESPEC,
        IMPLMAP,
        FIELDRVA,
        UNUSED6,
        UNUSED7,
        ASSEMBLY, /* 0x20 */
        ASSEMBLYPROCESSOR,
        ASSEMBLYOS,
        ASSEMBLYREF,
        ASSEMBLYREFPROCESSOR,
        ASSEMBLYREFOS,
        FILE,
        EXPORTEDTYPE,
        MANIFESTRESOURCE,
        NESTEDCLASS,
        GENERICPARAM, /* 0x2a */
        METHODSPEC,
        GENERICPARAMCONSTRAINT,
        UNUSED8,
        UNUSED9,
        UNUSED10,
        /* Portable PDB tables */
        DOCUMENT, /* 0x30 */
        METHODBODY,
        LOCALSCOPE,
        LOCALVARIABLE,
        LOCALCONSTANT,
        IMPORTSCOPE,
        STATEMACHINEMETHOD,
        CUSTOMDEBUGINFORMATION
    };


    // 0
    struct TbModule
    {
        uint16_t generation;
        uint32_t name;
        uint32_t mvid;
        uint32_t encid;
        uint32_t encBaseId;
    };

    // 1
    struct TbTypeRef
    {
        uint32_t resolutionScope;
        uint32_t typeName;
        uint32_t typeNamespace;
    };

    // 2
    struct TbTypeDef
    {
        uint32_t flags;
        uint32_t typeName;
        uint32_t typeNamespace;
        uint32_t extends;
        uint32_t fieldList;
        uint32_t methodList;
    };

    // 3 FIELD_POINTER

    // 4
    struct TbField
    {
        uint32_t flags;
        uint32_t name;
        uint32_t signature;
    };

    // 5 METHOD_POINTER
        
    // 6
    struct TbMethod
    {
        uint32_t rva;
        uint16_t implFlags;
        uint16_t flags;
        uint32_t name;
        uint32_t signature;
        uint32_t paramList;
    };

    // 7 PARAM_POINTER

    // 8
    struct TbParam
    {
        uint16_t flags;
        uint16_t sequence;
        uint32_t name;
    };

    // 9
    struct TbInterfaceImpl
    {
        uint32_t classIdx;
        uint32_t interfaceIdx;
    };

    // 0xa
    struct TbMemberRef
    {
        uint32_t classIdx;
        uint32_t name;
        uint32_t signature;
    };

    struct TbConstant
    {
        uint8_t type; // 实际上占2字节
        uint32_t parent;
        uint32_t value;
    };

    struct TbCustomAttribute
    {
        uint32_t parent;
        uint32_t type;
        uint32_t value;
    };

    struct TbFieldMarshal
    {
        uint32_t parent;
        uint32_t nativeType;
    };

    struct TbDeclSecurity
    {
        uint16_t action;
        uint32_t parent;
        uint32_t permissionSet;
    };

    struct TbClassLayout
    {
        uint16_t packingSize;
        uint32_t classSize;
        uint32_t parent;
    };

    // 0x10
    struct TbFieldLayout
    {
        uint32_t offset;
        uint32_t field;
    };

    struct TbStandAloneSig
    {
        uint32_t signature; // 指向 blob heap的位置
    };

    struct TbEventMap
    {
        uint32_t parent;
        uint32_t eventList;
    };

    // 0x13 EVENT_POINTER

    // 0x14
    struct TbEvent
    {
        uint16_t eventFlags;
        uint32_t name;
        uint32_t eventType;
    };

    struct TbPropertyMap
    {
        uint32_t parent;
        uint32_t propertyList;
    };

    // PROPERTY_POINTER

    struct TbProperty
    {
        uint16_t flags;
        uint32_t name;
        uint32_t type;
    };

    struct TbMethodSemantics
    {
        uint16_t semantics;
        uint32_t method;
        uint32_t association;
    };

    struct TbMethodImpl
    {
        uint32_t classIdx;
        uint32_t methodBody;
        uint32_t methodDeclaration;
    };

    struct TbModuleRef
    {
        uint32_t name;
    };

    struct TbTypeSpec
    {
        uint32_t signature;
    };

    struct TbImplMap
    {
        uint16_t mappingFlags;
        uint32_t memberForwarded;
        uint32_t importName;
        uint32_t importScope;
    };

    struct TbFieldRVA
    {
        uint32_t rva;
        uint32_t field;
    };

    // UNUSED 6
    // UNUSED 7

    struct TbAssembly
    {
        uint32_t hashAlgId;
        uint16_t majorVersion;
        uint16_t minorVersion;
        uint16_t buildNumber;
        uint16_t revisionNumber;
        uint32_t flags;
        uint32_t publicKey;
        uint32_t name;
        uint32_t culture;
    };

    struct TbAssemblyProcessor
    {
        uint32_t processor;
    };

    struct TbAssemblyOS
    {
        uint32_t osPlatformID;
        uint32_t osMajorVersion;
        uint32_t osMinorVersion;
    };

    struct TbAssemblyRef
    {
        uint16_t majorVersion;
        uint16_t minorVersion;
        uint16_t buildNumber;
        uint16_t revisionNumber;
        uint32_t flags;
        uint32_t publicKeyOrToken;
        uint32_t name;
        uint32_t culture;
        uint32_t hashValue;
    };

    struct TbAssemblyRefProcessor
    {
        uint32_t processor;
        uint32_t assemblyRef;
    };

    struct TbAssemblyRefOS
    {
        uint32_t osPlatformID;
        uint32_t osMajorVersion;
        uint32_t osMinorVersion;
        uint32_t assemblyRef;
    };

    struct TbFile
    {
        uint32_t flags;
        uint32_t name;
        uint32_t hashValue;
    };

    struct TbExportedType
    {
        uint32_t flags;
        uint32_t typeDefId;
        uint32_t typeName;
        uint32_t typeNamespace;
        uint32_t implementation;
    };

    struct TbManifestResource
    {
        uint32_t offset;
        uint32_t flags;
        uint32_t name;
        uint32_t implementation;
    };

    struct TbNestedClass
    {
        uint32_t nestedClass;
        uint32_t enclosingClass;
    };

    struct TbGenericParam
    {
        uint16_t number;
        uint16_t flags;
        uint32_t owner;
        uint32_t name;
    };

    struct TbMethodSpec
    {
        uint32_t method;
        uint32_t instantiation;
    };

    struct TbGenericParamConstraint
    {
        uint32_t owner;
        uint32_t constraint;
    };

    // 以下这些都不是tables的类型
    // 但mono特殊处理一下，额外也加到这个表中

    // size 84
    struct TbSymbolDocument
    {

    };

    // size 52
    struct TbSymbolMethodBody
    {

    };

    // size 20
    struct TbSymbolLocalScope
    {

    };

    // size 56
    struct TbSymbolLocalVariable
    {

    };

    // size 24
    struct TbSymbolConstant
    {

    };

    // SymUsing. size 8
    struct TbSymbolImportScope
    {

    };


    struct TbSymbolMisc
    {

    };

    struct TbSymbolString
    {

    };

}

}