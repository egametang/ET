#pragma once

#include "il2cpp-config.h"

#include "Coff.h"
#include "Tables.h"

namespace huatuo
{
namespace metadata
{

	enum class CorILMethodFormat : uint8_t
	{
		Tiny = 0x2,
		Fat = 0x3,
		MoreSects = 0x8,
		InitLocals = 0x10,
	};

	enum class CorILSecion : uint8_t
	{
		EHTable = 0x1,
		OptILTable = 0x2,
		FatFormat = 0x40,
		MoreSects = 0x80,
	};

	enum class CorILExceptionClauseType
	{
		Exception = 0,
		Filter = 1,
		Finally = 2,
		Fault = 4,
	};

	struct CorILMethodFatHeader
	{
		uint16_t flags : 12;
		uint16_t size : 4;
		uint16_t maxStack;
		uint32_t codeSize;
		uint32_t localVarSigToken;
	};

	struct CorILEHSmall
	{
		uint16_t flags;
		uint16_t tryOffset;
		uint8_t tryLength;
		uint8_t handlerOffset0;
		uint8_t handlerOffset1;
		uint8_t handlerLength;
		uint32_t classTokenOrFilterOffset;
	};

	static_assert(sizeof(CorILEHSmall) == 12, "sizeof(CorILEHSmall) != 12");

	struct CorILEHFat
	{
		uint32_t flags;
		uint32_t tryOffset;
		uint32_t tryLength;
		uint32_t handlerOffset;
		uint32_t handlerLength;
		uint32_t classTokenOrFilterOffset;
	};

	static_assert(sizeof(CorILEHFat) == 24, "sizeof(CorILEHFat) != 24");

	struct CorILEHSectionHeaderSmall
	{
		uint8_t kind;
		uint8_t dataSize;
		uint16_t reserved;
		CorILEHSmall clauses[0];
	};

#pragma pack(push, 1)
	struct CorILEHSectionHeaderFat
	{
		uint8_t kind;
		uint8_t dataSize0;
		uint8_t dataSize1;
		uint8_t dataSize2;
		CorILEHFat clauses[0];
	};
#pragma pack(pop)

	const int MAX_TABLE_INDEX = 0x2C;
	const int TABLE_NUM = MAX_TABLE_INDEX + 1;

	class TagBits
	{
	public:
		static const uint32_t TypeDefOrRef = 2;
		static const uint32_t HasConstant = 2;
		static const uint32_t HasCustomAttribute = 5;
		static const uint32_t HasFieldMarshal = 1;
		static const uint32_t HasDeclSecurity = 2;
		static const uint32_t MemberRefParent = 3;
		static const uint32_t HasSemantics = 1;
		static const uint32_t MethodDefOrRef = 1;
		static const uint32_t MemberForwarded = 1;
		static const uint32_t Implementation = 2;
		static const uint32_t CustomAttributeType = 3;
		static const uint32_t ResoulutionScope = 2;
		static const uint32_t TypeOrMethodDef = 1;

	};


	const TableType HasCustomAttributeAssociateTables[] = {
		TableType::METHOD,
		TableType::FIELD,
		TableType::TYPEREF,
		TableType::TYPEDEF,
		TableType::PARAM,
		TableType::INTERFACEIMPL,
		TableType::MEMBERREF,
		TableType::MODULE,
		TableType::DECLSECURITY,
		TableType::PROPERTY,
		TableType::EVENT,
		TableType::STANDALONESIG,
		TableType::MODULEREF,
		TableType::TYPESPEC,
		TableType::ASSEMBLY,
		TableType::ASSEMBLYREF,
		TableType::FILE,
		TableType::EXPORTEDTYPE,
		TableType::MANIFESTRESOURCE,
		TableType::GENERICPARAM,
		TableType::GENERICPARAMCONSTRAINT,
		TableType::METHODSPEC,
	};

	inline TableType DecodeTokenTableType(uint32_t index)
	{
		return TableType(index >> 24);
	}

	inline uint32_t DecodeTokenRowIndex(uint32_t index)
	{
		return index & 0xFFFFFF;
	}

	struct ColumnOffsetSize
	{
		uint32_t size;
		uint16_t offset;
	};

	enum class SigType
	{
		DEFAULT = 0,
		C = 1,
		ST_STDCALL = 2,
		ST_THISCALL = 3,
		ST_FASTCALL = 4,
		VARARG = 5,
		FIELD = 6,
		LOCAL_VAR = 7,
		PROPERTY_NOT_THIS = 8,
		GENERIC = 0x10,
		SENTINEL = 0x41,
	};

	const uint32_t kSigMask = 0x0F;

	inline SigType DecodeSigType(uint8_t rawSigType)
	{
		return (SigType)(rawSigType & kSigMask);
	}

	inline uint8_t DecodeSigFlags(uint8_t rawSigType)
	{
		return (uint8_t)(rawSigType & 0xF0);
	}

	inline uint32_t EncodeToken(TableType type, uint32_t index)
	{
		return ((uint32_t)type << 24) | index;
	}

	inline void DecodeToken(uint32_t token, TableType& type, uint32_t& rowIndex)
	{
		type = (TableType)(token >> 24);
		rowIndex = token & 0xFFFFFF;
	}

	enum class MethodSigFlags
	{
		HAS_THIS = 0x20,
		EXPLICITTHIS = 0x40,
		DEFAULT = 0x0,
		VARARG = 0x5,
		GENERIC = 0x10,
	};

	inline Il2CppTypeEnum GetElementType(Il2CppTypeEnum encodeType)
	{
		return (Il2CppTypeEnum)((uint8_t)encodeType & 0x3f);
	}

	inline TableType DecodeTypeDefOrRefOrSpecCodedIndexTableType(uint32_t encodedToken)
	{
		switch (encodedToken & 0x3)
		{
		case 0: return TableType::TYPEDEF;
		case 1: return TableType::TYPEREF;
		case 2: return  TableType::TYPESPEC;
		default: IL2CPP_ASSERT(false); return (TableType)-1;
		}
	}

	inline uint32_t DecodeTypeDefOrRefOrSpecCodedIndexRowIndex(uint32_t encodedToken)
	{
		return encodedToken >> 2;
	}

	inline uint32_t EncodeTypeDefOrRefOrSpecCodedIndex(TableType type, uint32_t rowIndex)
	{
		uint32_t tableBits;
		switch (type)
		{
		case TableType::TYPEDEF: tableBits = 0; break;
		case TableType::TYPEREF: tableBits = 1; break;
		case TableType::TYPESPEC: tableBits = 2; break;
		default: IL2CPP_ASSERT(0); tableBits = 0; break;
		}
		return (rowIndex << 2) | tableBits;
	}

	inline uint32_t ConvertTypeDefOrRefOrSpecToken2CodedIndex(uint32_t token)
	{
		TableType type;
		uint32_t rowIndex;
		DecodeToken(token, type, rowIndex);

		uint32_t tableBits;
		switch (type)
		{
		case TableType::TYPEDEF: tableBits = 0; break;
		case TableType::TYPEREF: tableBits = 1; break;
		case TableType::TYPESPEC: tableBits = 2; break;
		default: IL2CPP_ASSERT(0); tableBits = 0; break;
		}
		return (rowIndex << 2) | tableBits;
	}

	inline void DecodeResolutionScopeCodedIndex(uint32_t encodedToken, TableType& tokenType, uint32_t& rawIndex)
	{
		switch (encodedToken & 0x3)
		{
		case 0: tokenType = TableType::MODULE; break;
		case 1: tokenType = TableType::MODULEREF; break;
		case 2: tokenType = TableType::ASSEMBLYREF; break;
		case 3: tokenType = TableType::TYPEREF; break;
		}
		rawIndex = encodedToken >> 2;
	}

	inline TableType DecodeTypeOrMethodDefCodedIndexTableType(uint32_t encodeIndex)
	{
		switch (encodeIndex & 0x1)
		{
		case 0: return TableType::TYPEDEF;
		case 1: return TableType::METHOD;
		}
		IL2CPP_ASSERT(false);
		return (TableType)-1;
	}

	inline uint32_t DecodeTypeOrMethodDefCodedIndexRowIndex(uint32_t encodeIndex)
	{
		return encodeIndex >> 1;
	}

	inline TableType DecodeMethodDefOrRefCodedIndexTableType(uint32_t token)
	{
		switch (token & 0x1)
		{
		case 0: return TableType::METHOD;
		case 1: return TableType::MEMBERREF;
		}
		return (TableType)-1;
	}

	inline uint32_t DecodeMethodDefOrRefCodedIndexRowIndex(uint32_t token)
	{
		return token >> 1;
	}

	inline uint32_t EncodeMethodDefOrRefCodedIndex(TableType type, uint32_t rowIndex)
	{
		IL2CPP_ASSERT(type == TableType::METHOD || type == TableType::MEMBERREF);
		return (rowIndex << 1) | (type != TableType::METHOD);
	}

	inline uint32_t ConvertMethodDefOrRefToken2CodedIndex(uint32_t token)
	{
		TableType type;
		uint32_t rowIndex;
		DecodeToken(token, type, rowIndex);
		return EncodeMethodDefOrRefCodedIndex(type, rowIndex);
	}

	inline TableType DecodeMemberRefParentType(uint32_t token)
	{
		switch (token & 0x7)
		{
		case 0: return TableType::TYPEDEF;
		case 1: return TableType::TYPEREF;
		case 2: return TableType::MODULEREF;
		case 3: return TableType::METHOD;
		case 4: return TableType::TYPESPEC;
		default: IL2CPP_ASSERT(false); return (TableType)-1;
		}
	}

	inline uint32_t DecodeMemberRefParentRowIndex(uint32_t token)
	{
		return token >> 3;
	}

	inline TableType DecodeFieldDefOrDefType(uint32_t encodeIndex)
	{
		switch (encodeIndex & 0x1)
		{
		case 0: return TableType::FIELD;
		case 1: return TableType::MEMBERREF;
		}
		return (TableType)-1;
	}

	inline uint32_t DecodeFieldDefOrDefTypeRowIndex(uint32_t encodeIndex)
	{
		return encodeIndex >> 1;
	}

	inline uint32_t EncodeFieldDefOrRefCodedIndex(TableType type, uint32_t rowIndex)
	{
		IL2CPP_ASSERT(type == TableType::FIELD || type == TableType::MEMBERREF);
		return (rowIndex << 1) | (type != TableType::FIELD);
	}

	inline uint32_t ConvertFieldDefOrRefToken2CodedIndex(uint32_t token)
	{
		TableType type;
		uint32_t rowIndex;
		DecodeToken(token, type, rowIndex);
		return EncodeFieldDefOrRefCodedIndex(type, rowIndex);
	}

	inline TableType DecodeMemberRefParentCodedIndexTableType(uint32_t encodeIndex)
	{
		switch ((encodeIndex & 0x7))
		{
		case 0: return TableType::TYPEDEF;
		case 1: return TableType::TYPEREF;
		case 2: return TableType::MODULEREF;
		case 3: return TableType::METHOD;
		case 4: return TableType::TYPESPEC;
		default: IL2CPP_ASSERT(false); return (TableType)-1;
		}
	}

	inline uint32_t DecodeMemberRefParentCodedIndexRowIndex(uint32_t encodeIndex)
	{
		return encodeIndex >> 3;
	}

	inline TableType DecodeHasCustomAttributeCodedIndexTableType(uint32_t codedIndex)
	{
		switch (codedIndex & 0x1f)
		{
		case 0: return TableType::METHOD;
		case 1: return TableType::FIELD;
		case 2: return TableType::TYPEREF;
		case 3: return TableType::TYPEDEF;
		case 4: return TableType::PARAM;
		case 5: return TableType::INTERFACEIMPL;
		case 6: return TableType::MEMBERREF;
		case 7: return TableType::MODULE;
		case 8: return TableType::DECLSECURITY;
		case 9: return TableType::PROPERTY;
		case 10: return TableType::EVENT;
		case 11: return TableType::STANDALONESIG;
		case 12: return TableType::MODULEREF;
		case 13: return TableType::TYPESPEC;
		case 14: return TableType::ASSEMBLY;
		case 15: return TableType::ASSEMBLYREF;
		case 16: return TableType::FILE;
		case 17: return TableType::EXPORTEDTYPE;
		case 18: return TableType::MANIFESTRESOURCE;
		case 19: return TableType::GENERICPARAM;
		case 20: return TableType::GENERICPARAMCONSTRAINT;
		case 21: return TableType::METHODSPEC;
		default: IL2CPP_ASSERT(false); return (TableType)-1;
		}
	}

	inline uint32_t DecodeHasCustomAttributeCodedIndexRowIndex(uint32_t codedIndex)
	{
		return codedIndex >> 5;
	}

	inline TableType DecodeCustomAttributeTypeCodedIndexTableType(uint32_t codeIndex)
	{
		switch (codeIndex & 0x7)
		{
		case 2: return TableType::METHOD;
		case 3: return TableType::MEMBERREF;
		default: IL2CPP_ASSERT(false); return (TableType)-1;
		}
	}

	inline uint32_t DecodeCustomAttributeTypeCodedIndexRowIndex(uint32_t codedIndex)
	{
		return codedIndex >> 3;
	}

	enum class UserStringEncoding
	{
		ASCII = 0,
		Unicode = 1,
	};

	struct ExceptionClause
	{
		CorILExceptionClauseType flags;
		uint32_t tryOffset;
		uint32_t tryLength;
		uint32_t handlerOffsets;
		uint32_t handlerLength;
		uint32_t classTokenOrFilterOffset;
	};

	struct MethodBody
	{
		uint32_t flags;
		uint32_t codeSize;
		const uint8_t* ilcodes;
		Il2CppType* localVars;
		uint32_t localVarCount;
		uint32_t maxStack;
		std::vector<ExceptionClause> exceptionClauses;
		// optional data sections
	};



	struct MethodRefInfo
	{
		Il2CppType containerType; // maybe generic
		const Il2CppMethodDefinition* methodDef;
		Il2CppGenericInst* instantiation;
	};

	struct MethodImpl
	{
		MethodRefInfo body;
		MethodRefInfo declaration;
	};

	struct MethodDefSig
	{
		Il2CppType classType;
		const char* name;
		uint32_t flags;
		uint32_t genericParamCount;
		Il2CppType returnType;
		std::vector<Il2CppType> params;
	};

	struct MethodRefSig
	{
		uint32_t flags;
		uint32_t genericParamCount;
		Il2CppType returnType;
		std::vector<Il2CppType> params;
	};

	struct FieldRefSig
	{
		Il2CppType type;
	};

	struct FieldRefInfo
	{
		Il2CppType containerType; // maybe generic
		const Il2CppFieldDefinition* field;
	};

	struct ResolveModuleRef
	{

	};

	struct ResolveMethodDef
	{
		const Il2CppMethodDefinition* methodDef;
	};

	struct ResolveMemberRefParent
	{
		TableType parentType;
		Il2CppType type; // TYPEREF, TYPEDEF,TYPESPEC
		ResolveModuleRef moduleRef;
		ResolveMethodDef methodDef;
	};

	struct ResolveMemberRefSig
	{
		TableType memberType; // FIELD_POINTER OR METHOD_POINTER
		MethodRefSig method;
		FieldRefSig field;
	};

	struct ResolveMemberRef
	{
		ResolveMemberRefParent parent;
		const char* name;
		ResolveMemberRefSig signature;
	};

	struct MemberRefInfo
	{
		TableType memberType;
		MethodRefInfo method;
		FieldRefInfo field;
	};

	struct ResolveStandAloneMethodSig
	{
		int32_t flags;
		uint32_t paramCount;
		Il2CppType returnType;
		Il2CppType* params;
	};

	inline TableType DecodeHasConstantType(uint32_t token)
	{
		switch (token & 0x3)
		{
		case 0: return TableType::FIELD;
		case 1: return TableType::PARAM;
		case 2: return TableType::PROPERTY;
		default: IL2CPP_ASSERT(false); return (TableType)-1;
			break;
		}
	}

	inline uint32_t DecodeHashConstantIndex(uint32_t token)
	{
		return token >> 2;
	}

	enum class MethodSemanticsAttributes
	{
		Setter = 0x1,
		Getter = 0x2,
		Other = 0x4,
		AddOn = 0x8,
		RemoveOn = 0x10,
		Fire = 0x20,
	};

	inline TableType DecodeHasSemanticsCodedIndexTableType(uint32_t codedIndex)
	{
		switch (codedIndex & 0x1)
		{
		case 0: return TableType::EVENT;
		case 1: return TableType::PROPERTY;
		}
		return (TableType)-1;
	}

	inline uint32_t DecodeHasSemanticsCodedIndexRowIndex(uint32_t codedIndex)
	{
		return codedIndex >> 1;
	}
}
}