#include "AOTHomologousImage.h"

#include "vm/MetadataLock.h"
#include "vm/GlobalMetadata.h"
#include "vm/Class.h"
#include "vm/Image.h"
#include "vm/Exception.h"
#include "vm/MetadataCache.h"
#include "metadata/GenericMetadata.h"

namespace huatuo
{
namespace metadata
{
	std::vector<AOTHomologousImage*> s_images;


	int32_t AOTHomologousImage::LoadMetadataForAOTAssembly(void* dllBytes, uint32_t dllSize)
	{
		il2cpp::os::FastAutoLock lock(&il2cpp::vm::g_MetadataLock);

		AOTHomologousImage* image = new AOTHomologousImage();
		LoadImageErrorCode err = image->Load((byte*)CopyBytes(dllBytes, dllSize), dllSize);
		if (err != LoadImageErrorCode::OK)
		{
			return (int32_t)err;
		}
		if (FindImageByAssembly(image->GetAOTAssembly()))
		{
			return (int32_t)LoadImageErrorCode::HOMOLOGOUS_ASSEMBLY_HAS_LOADED;
		}
		image->InitRuntimeMetadatas();
		s_images.push_back(image);
		return 0;
	}

	AOTHomologousImage* AOTHomologousImage::FindImageByAssembly(const Il2CppAssembly* ass)
	{
		il2cpp::os::FastAutoLock lock(&il2cpp::vm::g_MetadataLock);
		for (AOTHomologousImage* image : s_images)
		{
			if (image->_aotAssembly == ass)
			{
				return image;
			}
		}
		return nullptr;
	}

	void AOTHomologousImage::InitRuntimeMetadatas()
	{
		InitTypes();
		InitMethods();
		InitFields();
	}

	void AOTHomologousImage::InitTypes()
	{
		const Table& typeDefTb = _rawImage.GetTable(TableType::TYPEDEF);
		uint32_t typeCount = typeDefTb.rowNum;
		_il2cppTypeForTypeDefs.resize(typeCount);
		_typeDefs.resize(typeCount);

		Il2CppImage* image = _aotAssembly->image;
		//if (image->typeCount != typeCount)
		//{
		//	RaiseHuatuoExecutionEngineException("image metadata not match");
		//}
		for (uint32_t index = 0; index < image->typeCount; index++)
		{
			Il2CppTypeDefinition* typeDef = (Il2CppTypeDefinition*)il2cpp::vm::MetadataCache::GetAssemblyTypeHandle(image, index);
			uint32_t rowIndex = DecodeTokenRowIndex(typeDef->token);
			IL2CPP_ASSERT(rowIndex > 0);
			if (rowIndex > typeCount)
			{
				continue;
			}
			TbTypeDef data = _rawImage.ReadTypeDef(rowIndex);
			uint32_t typeIndex = rowIndex - 1;
			 _typeDefs[typeIndex] = typeDef;
			 const Il2CppType* il2cppType = il2cpp::vm::GlobalMetadata::GetIl2CppTypeFromIndex(typeDef->byvalTypeIndex);
			 _il2cppTypeForTypeDefs[typeIndex] = il2cppType;

			 const char* name1 = _rawImage.GetStringFromRawIndex(data.typeName);
			 const char* name2 = il2cpp::vm::GlobalMetadata::GetStringFromIndex(typeDef->nameIndex);
			 if (std::strcmp(name1, name2))
			 {
				 RaiseHuatuoExecutionEngineException("metadata type not match");
			 }
			 const char* namespaze1 = _rawImage.GetStringFromRawIndex(data.typeNamespace);
			 const char* namespaze2 = il2cpp::vm::GlobalMetadata::GetStringFromIndex(typeDef->namespaceIndex);
			 if (std::strcmp(namespaze1, namespaze2))
			 {
				 RaiseHuatuoExecutionEngineException("metadata type not match");
			 }
		}
	}

	void AOTHomologousImage::InitMethods()
	{
		const Table& methodTb = _rawImage.GetTable(TableType::METHOD);
		_methodDefs.resize(methodTb.rowNum);

		for (Il2CppTypeDefinition* type : _typeDefs)
		{
			for (uint16_t i = 0; i < type->method_count; i++)
			{
				const Il2CppMethodDefinition* methodDef = il2cpp::vm::GlobalMetadata::GetMethodDefinitionFromIndex(type->methodStart + i);
				uint32_t rowIndex = DecodeTokenRowIndex(methodDef->token);
				IL2CPP_ASSERT(rowIndex > 0 && rowIndex <= methodTb.rowNum);
				uint32_t methodIndex = rowIndex - 1;
				IL2CPP_ASSERT(_methodDefs[methodIndex] == nullptr);
				_methodDefs[methodIndex] = methodDef;

				TbMethod methodData = _rawImage.ReadMethod(rowIndex);
				const char* name1 = _rawImage.GetStringFromRawIndex(methodData.name);
				const char* name2 = il2cpp::vm::GlobalMetadata::GetStringFromIndex(methodDef->nameIndex);
				if (std::strcmp(name1, name2))
				{
					RaiseHuatuoExecutionEngineException("metadata method not match");
				}
			}
		}
	}

	void AOTHomologousImage::InitFields()
	{
		const Table& fieldTb = _rawImage.GetTable(TableType::FIELD);
		_fields.resize(fieldTb.rowNum);

		for (size_t i = 0; i < _typeDefs.size() ; i++)
		{
			Il2CppTypeDefinition* type = _typeDefs[i];
			for (uint16_t j = 0; j < type->field_count; j++)
			{
				const Il2CppFieldDefinition* fieldDef = il2cpp::vm::GlobalMetadata::GetFieldDefinitionFromTypeDefAndFieldIndex(type, j);
				uint32_t rowIndex = DecodeTokenRowIndex(fieldDef->token);
				IL2CPP_ASSERT(rowIndex > 0);
				uint32_t fieldIndex = rowIndex - 1;
				IL2CPP_ASSERT(_fields[fieldIndex].fieldDef == nullptr);
				if (rowIndex >= fieldTb.rowNum)
				{
					continue;
				}
				_fields[fieldIndex] = {(uint32_t)i, fieldDef};

				TbField fieldData = _rawImage.ReadField(rowIndex);
				const char* name1 = _rawImage.GetStringFromRawIndex(fieldData.name);
				const char* name2 = il2cpp::vm::GlobalMetadata::GetStringFromIndex(fieldDef->nameIndex);
				if (std::strcmp(name1, name2))
				{
					RaiseHuatuoExecutionEngineException("metadata field not match");
				}
			}
		}
	}

	MethodBody* AOTHomologousImage::GetMethodBody(const MethodInfo* method)
	{
		uint32_t token = method->token;
		auto it = _token2MethodBodies.find(token);
		if (it != _token2MethodBodies.end())
		{
			return it->second;
		}
		TbMethod methodData = _rawImage.ReadMethod(DecodeTokenRowIndex(token));
		MethodBody* body = new (IL2CPP_MALLOC_ZERO(sizeof(MethodBody))) MethodBody();
		ReadMethodBody(*(Il2CppMethodDefinition*)GetUnderlyingMethodInfo(method)->methodMetadataHandle, methodData, *body);
		_token2MethodBodies.insert({ token, body });
		return body;
	}

	const Il2CppType* AOTHomologousImage::GetIl2CppTypeFromRawTypeDefIndex(uint32_t index)
	{
		IL2CPP_ASSERT((size_t)index < _il2cppTypeForTypeDefs.size());
		return _il2cppTypeForTypeDefs[index];
	}

	const Il2CppType* AOTHomologousImage::GetIl2CppTypeFromRawIndex(uint32_t index) const
	{
		return il2cpp::vm::GlobalMetadata::GetIl2CppTypeFromIndex(index);
	}

	Il2CppGenericContainer* AOTHomologousImage::GetGenericContainerByRawIndex(uint32_t index)
	{
		return (Il2CppGenericContainer*)il2cpp::vm::GlobalMetadata::GetGenericContainerFromIndex(index);
	}

	const Il2CppMethodDefinition* AOTHomologousImage::GetMethodDefinitionFromRawIndex(uint32_t index)
	{
		IL2CPP_ASSERT((size_t)index < _methodDefs.size());
		return _methodDefs[index];
	}

	const Il2CppTypeDefinition* AOTHomologousImage::GetTypeFromRawIndex(uint32_t index) const
	{
		IL2CPP_ASSERT((size_t)index < _typeDefs.size());
		return _typeDefs[index];
	}

	Il2CppGenericContainer* AOTHomologousImage::GetGenericContainerByTypeDefIndex(int32_t typeDefIndex)
	{
		Il2CppTypeDefinition* type = (Il2CppTypeDefinition*)il2cpp::vm::GlobalMetadata::GetTypeHandleFromIndex(typeDefIndex);
		return (Il2CppGenericContainer*)il2cpp::vm::GlobalMetadata::GetGenericContainerFromIndex(type->genericContainerIndex);
	}

	const MethodInfo* AOTHomologousImage::GetMethodInfo(const Il2CppType* containerType, const Il2CppMethodDefinition* methodDef, const Il2CppGenericInst* instantiation, const Il2CppGenericContext* genericContext)
	{
		const Il2CppType* finalContainerType = TryInflateIfNeed(containerType, genericContext, true);
		const MethodInfo* method = GetMethodInfoFromMethodDef(containerType, methodDef);
		IL2CPP_ASSERT(method);
		// final genericContext = finalContainerType.class_inst + mri.instantiation
		if (instantiation)
		{
			const Il2CppGenericInst* finalClassIns = finalContainerType->type == IL2CPP_TYPE_GENERICINST ? finalContainerType->data.generic_class->context.class_inst : nullptr;
			const Il2CppGenericInst* finalMethodIns = instantiation;
			Il2CppGenericContext finalGenericContext = { finalClassIns, finalMethodIns };
			method = method->is_inflated ? method->genericMethod->methodDefinition : method;
			method = il2cpp::metadata::GenericMetadata::Inflate(method, &finalGenericContext);
			IL2CPP_ASSERT(method);
		}
		return method;
	}

	const MethodInfo* AOTHomologousImage::ResolveMethodInfo(const Il2CppType* type, const char* resolveMethodName, const MethodRefSig& resolveSig, const Il2CppGenericInst* genericInstantiation, const Il2CppGenericContext* genericContext)
	{
		if (type->type != IL2CPP_TYPE_ARRAY)
		{
			const Il2CppTypeDefinition* typeDef = GetUnderlyingTypeDefinition(type);
			const Il2CppGenericContainer* klassGenericContainer = GetGenericContainerFromIl2CppType(type);
			const char* typeName = il2cpp::vm::GlobalMetadata::GetStringFromIndex(typeDef->nameIndex);
			for (uint32_t i = 0; i < typeDef->method_count; i++)
			{
				const Il2CppMethodDefinition* methodDef = il2cpp::vm::GlobalMetadata::GetMethodDefinitionFromIndex(typeDef->methodStart + i);
				const char* methodName = il2cpp::vm::GlobalMetadata::GetStringFromIndex(methodDef->nameIndex);
				if (std::strcmp(resolveMethodName, methodName) == 0 && IsMatchMethodSig(methodDef, resolveSig, klassGenericContainer, genericInstantiation ? genericInstantiation->type_argc : 0))
				{
					return GetMethodInfo(type, methodDef, genericInstantiation, genericContext);
				}
			}
		}
		else
		{
			IL2CPP_ASSERT(genericInstantiation == nullptr);
			Il2CppClass* arrayKlass = il2cpp::vm::Class::FromIl2CppType(type);
			il2cpp::vm::Class::SetupMethods(arrayKlass);
			//const Il2CppType* genericClassInstArgv[] = { &arrayKlass->element_class->byval_arg };
			const Il2CppType** genericClassInstArgv = genericContext && genericContext->class_inst ? genericContext->class_inst->type_argv : nullptr;
			const Il2CppType** genericMethodInstArgv = genericContext && genericContext->method_inst ? genericContext->method_inst->type_argv : nullptr;

			// FIXME MEMORY LEAK
			for (uint16_t i = 0; i < arrayKlass->method_count; i++)
			{
				const MethodInfo* method = arrayKlass->methods[i];
				if (std::strcmp(resolveMethodName, method->name) == 0 && IsMatchMethodSig(method, resolveSig, genericClassInstArgv, genericMethodInstArgv))
				{
					return method;
				}
			}
		}
		RaiseMethodNotFindException(type, resolveMethodName);
		return nullptr;
	}

	const MethodInfo* AOTHomologousImage::ReadMethodInfoFromToken(const Il2CppGenericContainer* klassGenericContainer,
		const Il2CppGenericContainer* methodGenericContainer, const Il2CppGenericContext* genericContext, Il2CppGenericInst* genericInst, TableType tableType, uint32_t rowIndex)
	{
		IL2CPP_ASSERT(rowIndex > 0);
		switch (tableType)
		{
		case TableType::METHOD:
		{
			const Il2CppMethodDefinition* methodDef = GetMethodDefinitionFromRawIndex(rowIndex - 1);
			const Il2CppTypeDefinition* declareType = (Il2CppTypeDefinition*)il2cpp::vm::GlobalMetadata::GetTypeHandleFromIndex(methodDef->declaringType);
			const Il2CppType* type = il2cpp::vm::GlobalMetadata::GetIl2CppTypeFromIndex(declareType->byvalTypeIndex);
			return GetMethodInfo(type, methodDef, genericInst, genericContext);
		}
		case TableType::MEMBERREF:
		{
			ResolveMemberRef rmr = {};
			ReadResolveMemberRefFromMemberRef(klassGenericContainer, methodGenericContainer, rowIndex, rmr);
			IL2CPP_ASSERT(rmr.parent.parentType == TableType::TYPEDEF || rmr.parent.parentType == TableType::TYPEREF || rmr.parent.parentType == TableType::TYPESPEC);
			IL2CPP_ASSERT(rmr.signature.memberType == TableType::METHOD_POINTER);
			if (genericContext)
			{
				rmr.parent.type = *TryInflateIfNeed(&rmr.parent.type, genericContext, true);
			}
			return ResolveMethodInfo(&rmr.parent.type, rmr.name, rmr.signature.method, genericInst, genericContext);
		}
		case TableType::METHODSPEC:
		{
			TbMethodSpec methodSpec = _rawImage.ReadMethodSpec(rowIndex);
			Il2CppGenericInst* genericInstantiation = nullptr;
			// FIXME! genericInstantiation memory leak
			ReadMethodSpecInstantiation(methodSpec.instantiation, klassGenericContainer, methodGenericContainer, genericInstantiation);
			// FIXME memory leak
			genericInstantiation = TryInflateGenericInst(genericInstantiation, genericContext);

			TableType methodTableType = DecodeMethodDefOrRefCodedIndexTableType(methodSpec.method);
			uint32_t methodRowIndex = DecodeMethodDefOrRefCodedIndexRowIndex(methodSpec.method);
			switch (methodTableType)
			{
			case TableType::METHOD:
			{
				return ReadMethodInfoFromToken(klassGenericContainer, methodGenericContainer, genericContext, genericInstantiation, methodTableType, methodRowIndex);
			}
			case TableType::MEMBERREF:
			{
				return ReadMethodInfoFromToken(klassGenericContainer, methodGenericContainer, genericContext, genericInstantiation, methodTableType, methodRowIndex);
			}
			default:
			{
				RaiseBadImageException("ReadMethodSpec invaild TableType");
				return nullptr;
			}
			}
			break;
		}
		default:
		{
			RaiseBadImageException("ReadMethodInfoFromToken invaild TableType");
			return nullptr;
		}
		}
	}

	const MethodInfo* AOTHomologousImage::GetMethodInfoFromToken(uint32_t token, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, const Il2CppGenericContext* genericContext)
	{
		auto key = std::tuple<uint32_t, const Il2CppGenericContext*>(token, genericContext);
		{
			il2cpp::os::FastAutoLock lock(&il2cpp::vm::g_MetadataLock);
			auto it = _token2ResolvedDataCache.find(key);
			if (it != _token2ResolvedDataCache.end())
			{
				return (const MethodInfo*)it->second;
			}
		}

		const MethodInfo* method = ReadMethodInfoFromToken(klassGenericContainer, methodGenericContainer, genericContext,
			nullptr, DecodeTokenTableType(token), DecodeTokenRowIndex(token));

		IL2CPP_ASSERT(method);
		{
			il2cpp::os::FastAutoLock lock(&il2cpp::vm::g_MetadataLock);
			_token2ResolvedDataCache.insert({ key, (void*)method });
		}
		return method;
	}

	void AOTHomologousImage::ReadFieldRefInfoFromToken(const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, TableType tableType, uint32_t rowIndex, FieldRefInfo& ret)
	{
		IL2CPP_ASSERT(rowIndex > 0);
		if (tableType == TableType::FIELD)
		{
			AOTFieldData& fd = _fields[rowIndex - 1];
			ret.containerType = *_il2cppTypeForTypeDefs[fd.typeDefIndex];
			ret.field = fd.fieldDef;
			//ret.classType = *image.GetIl2CppTypeFromRawTypeDefIndex(DecodeMetadataIndex(ret.field->typeIndex));
		}
		else
		{
			IL2CPP_ASSERT(tableType == TableType::MEMBERREF);
			ReadFieldRefInfoFromMemberRef(klassGenericContainer, methodGenericContainer, rowIndex, ret);
		}
	}

	const FieldInfo* AOTHomologousImage::GetFieldInfoFromToken(uint32_t token, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, const Il2CppGenericContext* genericContext)
	{
		auto key = std::tuple<uint32_t, const Il2CppGenericContext*>(token, genericContext);
		{
			il2cpp::os::FastAutoLock lock(&il2cpp::vm::g_MetadataLock);
			auto it = _token2ResolvedDataCache.find(key);
			if (it != _token2ResolvedDataCache.end())
			{
				return (const FieldInfo*)it->second;
			}
		}

		FieldRefInfo fri;
		ReadFieldRefInfoFromToken(klassGenericContainer, methodGenericContainer, DecodeTokenTableType(token), DecodeTokenRowIndex(token), fri);
		const Il2CppType* resultType = genericContext != nullptr ? il2cpp::metadata::GenericMetadata::InflateIfNeeded(&fri.containerType, genericContext, true) : &fri.containerType;
		const FieldInfo* fieldInfo = GetFieldInfoFromFieldRef(*resultType, fri.field);
		{
			il2cpp::os::FastAutoLock lock(&il2cpp::vm::g_MetadataLock);
			_token2ResolvedDataCache.insert({ key, (void*)fieldInfo });
		}
		return fieldInfo;
	}

	void AOTHomologousImage::GetStandAloneMethodSigFromToken(uint32_t token, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, const Il2CppGenericContext* genericContext, ResolveStandAloneMethodSig& methodSig)
	{
		RaiseHuatuoNotSupportedException("GetStandAloneMethodSigFromToken");
	}

}
}

