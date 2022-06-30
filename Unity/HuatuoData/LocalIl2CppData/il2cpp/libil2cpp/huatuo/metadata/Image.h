#pragma once

#include <vector>
#include <unordered_map>
#include <tuple>

#include "vm/GlobalMetadataFileInternals.h"
#include "gc/GarbageCollector.h"
#include "gc/Allocator.h"
#include "gc/AppendOnlyGCHashMap.h"

#include "RawImage.h"
#include "VTableSetup.h"
#include "MetadataUtil.h"


namespace huatuo
{
namespace metadata
{

	typedef std::tuple<uint32_t, const Il2CppGenericContext*> TokenGenericContextType;

	struct TokenGenericContextTypeHash {
		size_t operator()(const TokenGenericContextType x) const noexcept {
			return std::get<0>(x) * 0x9e3779b9 + (size_t)std::get<1>(x);
		}
	};

	struct TokenGenericContextTypeEqual
	{
		bool operator()(const TokenGenericContextType a, const TokenGenericContextType b) const {
			return std::get<0>(a) == std::get<0>(b) && std::get<1>(a) == std::get<1>(b);
		}
	};

	class Image
	{
	public:

		const RawImage& GetRawImage() const
		{
			return _rawImage;
		}

		// misc
		bool IsValueTypeFromToken(TableType tableType, uint32_t rowIndex);
		bool IsThreadStaticCtorToken(TableType tableType, uint32_t rowIndex);
		void ReadMemberRefParentFromToken(const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, TableType tableType, uint32_t rowIndex, ResolveMemberRefParent& ret);
		void ReadTypeFromMemberRefParent(const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, TableType tableType, uint32_t rowIndex, Il2CppType& type);

		const Il2CppTypeDefinition* GetTypeDefinition(uint32_t assemblyRefIndex, uint32_t typeNamespace, uint32_t typeName);
		// type
		void ReadType(BlobReader& reader, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, Il2CppType& type);

		void ReadTypeFromTypeDef(uint32_t rowIndex, Il2CppType& type);
		void ReadTypeFromTypeRef(uint32_t rowIndex, Il2CppType& type);
		void ReadTypeFromTypeSpec(const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, uint32_t rowIndex, Il2CppType& type);
		void ReadTypeFromToken(const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, TableType tableType, uint32_t rowIndex, Il2CppType& type);

		void ReadTypeFromResolutionScope(uint32_t scope, uint32_t typeNamespace, uint32_t typeName, Il2CppType& type);

		void ReadArrayType(BlobReader& reader, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, Il2CppArrayType& type);
		void ReadGenericClass(BlobReader& reader, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, Il2CppGenericClass& type);

		// signature
		void ReadMemberRefSig(const Il2CppGenericContainer* klassGenericContainer, TbMemberRef& data, ResolveMemberRefSig& signature);
		void ReadFieldRefSig(BlobReader& reader, const Il2CppGenericContainer* klassGenericContainer, FieldRefSig& field);
		void ReadMethodRefSig(TbMemberRef& rowData, MethodRefSig& method);
		void ReadMethodSpecInstantiation(uint32_t signatureIdx, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, Il2CppGenericInst*& genericInstantiation);
		void ReadLocalVarSig(BlobReader& reader, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, Il2CppType*& vars, uint32_t& varCount);
		void ReadStandAloneSig(uint32_t signatureIdx, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, ResolveStandAloneMethodSig& sig);

		// resolve from token
		void ReadResolveMemberRefFromMemberRef(const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, uint32_t rowIndex, ResolveMemberRef& ret);
		void ReadMethodRefInfoFromToken(const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, TableType tableType, uint32_t rowIndex, MethodRefInfo& ret);
		void ReadMethodRefInfoFromMemberRef(const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, Il2CppGenericInst* genericInstantiation, uint32_t rowIndex, MethodRefInfo& ret);

		void ReadFieldRefInfoFromMemberRef(const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, uint32_t rowIndex, FieldRefInfo& ret);
		void ReadMethodBody(const Il2CppMethodDefinition& methodDef, const TbMethod& methodData, MethodBody& body);

		Il2CppString* GetIl2CppUserStringFromRawIndex(StringIndex index);
		Il2CppClass* GetClassFromToken(uint32_t token, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, const Il2CppGenericContext* genericContext);
		const FieldInfo* GetFieldInfoFromFieldRef(const Il2CppType& type, const Il2CppFieldDefinition* fieldDef);
		const void* GetRuntimeHandleFromToken(uint32_t token, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, const Il2CppGenericContext* genericContext);
		const MethodInfo* FindImplMethod(Il2CppClass* klass, const MethodInfo* matchMethod);

		virtual const Il2CppType* GetIl2CppTypeFromRawTypeDefIndex(uint32_t index) = 0;
		virtual Il2CppGenericContainer* GetGenericContainerByRawIndex(uint32_t index) = 0;
		virtual const Il2CppMethodDefinition* GetMethodDefinitionFromRawIndex(uint32_t index) = 0;
		virtual const Il2CppType* GetIl2CppTypeFromRawIndex(uint32_t index) const = 0;
		virtual const Il2CppTypeDefinition* GetTypeFromRawIndex(uint32_t index) const = 0;
		virtual Il2CppGenericContainer* GetGenericContainerByTypeDefIndex(int32_t typeDefIndex) = 0;

		virtual MethodBody* GetMethodBody(const MethodInfo* method) = 0;
		virtual const MethodInfo* GetMethodInfoFromToken(uint32_t token, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, const Il2CppGenericContext* genericContext) = 0;
		virtual const FieldInfo* GetFieldInfoFromToken(uint32_t token, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, const Il2CppGenericContext* genericContext) = 0;
		virtual void GetStandAloneMethodSigFromToken(uint32_t token, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, const Il2CppGenericContext* genericContext, ResolveStandAloneMethodSig& methodSig) = 0;

	protected:
		RawImage _rawImage;
		il2cpp::gc::AppendOnlyGCHashMap<uint32_t, Il2CppString*, std::hash<uint32_t>> _il2cppStringCache;

		std::unordered_map<std::tuple<uint32_t, const Il2CppGenericContext*>, void*, TokenGenericContextTypeHash, TokenGenericContextTypeEqual> _token2ResolvedDataCache;
	};
}
}