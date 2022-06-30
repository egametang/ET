#pragma once
#include "Image.h"

namespace huatuo
{
namespace metadata
{
	struct AOTFieldData
	{
		uint32_t typeDefIndex; // rowIndex - 1
		const Il2CppFieldDefinition* fieldDef;
	};

	class AOTHomologousImage : public Image
	{
	public:

		static int32_t LoadMetadataForAOTAssembly(void* dllBytes, uint32_t dllSize);

		static AOTHomologousImage* FindImageByAssembly(const Il2CppAssembly* ass);

		AOTHomologousImage() : _aotAssembly(nullptr)
		{

		}

		const Il2CppAssembly* GetAOTAssembly() const
		{
			return _aotAssembly;
		}

		LoadImageErrorCode Load(const byte* imageData, size_t length)
		{
			LoadImageErrorCode err = _rawImage.Load(imageData, length);
			if (err != LoadImageErrorCode::OK)
			{
				return err;
			}

			TbAssembly data = _rawImage.ReadAssembly(1);
			const char* assName = _rawImage.GetStringFromRawIndex(data.name);
			const Il2CppAssembly* aotAss = il2cpp::vm::Assembly::GetLoadedAssembly(assName);
			// FIXME. not free memory.
			if (!aotAss)
			{
				return LoadImageErrorCode::AOT_ASSEMBLY_NOT_FIND;
			}
			if(huatuo::metadata::IsInterpreterImage(aotAss->image))
			{
				return LoadImageErrorCode::HOMOLOGOUS_ONLY_SUPPORT_AOT_ASSEMBLY;
			}
			_aotAssembly = aotAss;

			return LoadImageErrorCode::OK;
		}

		void InitRuntimeMetadatas();

		void InitTypes();
		void InitMethods();
		void InitFields();

		MethodBody* GetMethodBody(const MethodInfo* method) override;

		const Il2CppType* GetIl2CppTypeFromRawTypeDefIndex(uint32_t index) override;
		Il2CppGenericContainer* GetGenericContainerByRawIndex(uint32_t index) override;
		const Il2CppMethodDefinition* GetMethodDefinitionFromRawIndex(uint32_t index) override;
		const Il2CppType* GetIl2CppTypeFromRawIndex(uint32_t index) const override;
		const Il2CppTypeDefinition* GetTypeFromRawIndex(uint32_t index) const override;
		Il2CppGenericContainer* GetGenericContainerByTypeDefIndex(int32_t typeDefIndex) override;
		void ReadFieldRefInfoFromToken(const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, TableType tableType, uint32_t rowIndex, FieldRefInfo& ret);
		const MethodInfo* ReadMethodInfoFromToken(const Il2CppGenericContainer* klassGenericContainer,
			const Il2CppGenericContainer* methodGenericContainer, const Il2CppGenericContext* genericContext, Il2CppGenericInst* genericInst, TableType tableType, uint32_t rowIndex);
		const MethodInfo* GetMethodInfo(const Il2CppType* containerType, const Il2CppMethodDefinition* methodDef, const Il2CppGenericInst* instantiation, const Il2CppGenericContext* genericContext);
		const MethodInfo* ResolveMethodInfo(const Il2CppType* type, const char* resolveMethodName, const MethodRefSig& resolveSig, const Il2CppGenericInst* genericInstantiation, const Il2CppGenericContext* genericContext);

		const MethodInfo* GetMethodInfoFromToken(uint32_t token, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, const Il2CppGenericContext* genericContext) override;
		const FieldInfo* GetFieldInfoFromToken(uint32_t token, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, const Il2CppGenericContext* genericContext) override;
		void GetStandAloneMethodSigFromToken(uint32_t token, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, const Il2CppGenericContext* genericContext, ResolveStandAloneMethodSig& methodSig) override;

	private:
		const Il2CppAssembly* _aotAssembly;

		std::vector<const Il2CppType*> _il2cppTypeForTypeDefs;
		std::vector<Il2CppTypeDefinition*> _typeDefs;

		std::unordered_map<uint32_t, MethodBody*> _token2MethodBodies;
		std::vector< const Il2CppMethodDefinition*> _methodDefs;

		std::vector<AOTFieldData> _fields;
	};
}
}