#include "InterpreterImage.h"

#include <cstring>
#include <cmath>
#include <iostream>
#include <algorithm>

#include "il2cpp-class-internals.h"
#include "vm/GlobalMetadata.h"
#include "vm/Type.h"
#include "vm/Field.h"
#include "vm/Object.h"
#include "vm/Runtime.h"
#include "vm/Array.h"
#include "vm/MetadataLock.h"
#include "vm/MetadataCache.h"
#include "vm/String.h"
#include "vm/Reflection.h"
#include "metadata/FieldLayout.h"
#include "metadata/Il2CppTypeCompare.h"
#include "metadata/GenericMetadata.h"
#include "os/Atomic.h"

#include "MetadataModule.h"
#include "MetadataUtil.h"

#include "../interpreter/Engine.h"
#include "../interpreter/InterpreterModule.h"

namespace huatuo
{
namespace metadata
{

	uint32_t InterpreterImage::s_cliImageCount = 0;
	InterpreterImage* InterpreterImage::s_images[kMaxLoadImageCount + 1] = {};

	void InterpreterImage::Initialize()
	{
		s_cliImageCount = 0;
		std::memset(s_images, 0, sizeof(Assembly*) * (kMaxLoadImageCount + 1));
	}

	uint32_t InterpreterImage::AllocImageIndex()
	{
		il2cpp::os::FastAutoLock lock(&il2cpp::vm::g_MetadataLock);
		return ++s_cliImageCount;
	}

	void InterpreterImage::RegisterImage(InterpreterImage* image)
	{
		il2cpp::os::Atomic::FullMemoryBarrier();
		il2cpp::os::FastAutoLock lock(&il2cpp::vm::g_MetadataLock);
		IL2CPP_ASSERT(image->GetIndex() > 0);
		s_images[image->GetIndex()] = image;
	}

	void InterpreterImage::InitBasic(Il2CppImage* image)
	{
		SetIl2CppImage(image);
		RegisterImage(this);
	}

	void InterpreterImage::BuildIl2CppAssembly(Il2CppAssembly* ass)
	{
		ass->token = EncodeWithIndex(GetIndex());
		ass->referencedAssemblyStart = EncodeWithIndex(1);
		ass->referencedAssemblyCount = _rawImage.GetTableRowNum(TableType::ASSEMBLYREF);

		TbAssembly data = _rawImage.ReadAssembly(1);
		auto& aname = ass->aname;
		aname.hash_alg = data.hashAlgId;
		aname.major = data.majorVersion;
		aname.minor = data.minorVersion;
		aname.build = data.buildNumber;
		aname.revision = data.revisionNumber;
		aname.flags = data.flags;
		aname.public_key = _rawImage.GetBlobFromRawIndex(data.publicKey);
		aname.name = _rawImage.GetStringFromRawIndex(data.name);
		aname.culture = _rawImage.GetStringFromRawIndex(data.culture);
	}

	void InterpreterImage::BuildIl2CppImage(Il2CppImage* image2)
	{
		image2->typeCount = _rawImage.GetTableRowNum(TableType::TYPEDEF);
		image2->exportedTypeCount = _rawImage.GetTableRowNum(TableType::EXPORTEDTYPE);
		image2->customAttributeCount = _rawImage.GetTableRowNum(TableType::CUSTOMATTRIBUTE);

		Il2CppImageGlobalMetadata* metadataImage = (Il2CppImageGlobalMetadata*)IL2CPP_MALLOC_ZERO(sizeof(Il2CppImageGlobalMetadata));
		metadataImage->typeStart = EncodeWithIndex(0);
		metadataImage->customAttributeStart = EncodeWithIndex(0);
		metadataImage->entryPointIndex = EncodeWithIndexExcept0(_rawImage.GetEntryPointToken());
		metadataImage->exportedTypeStart = EncodeWithIndex(0);
		metadataImage->image = image2;

		image2->metadataHandle = reinterpret_cast<Il2CppMetadataImageHandle>(metadataImage);

		image2->nameToClassHashTable = nullptr;
		image2->codeGenModule = nullptr;

		image2->token = EncodeWithIndex(0); // TODO
		image2->dynamic = 0;
	}

	void InterpreterImage::InitRuntimeMetadatas()
	{
		IL2CPP_ASSERT(_rawImage.GetTable(TableType::EXPORTEDTYPE).rowNum == 0);

		il2cpp::os::FastAutoLock metaLock(&il2cpp::vm::g_MetadataLock);

		InitGenericParamDefs0();
		InitTypeDefs_0();
		InitMethodDefs0();
		InitGenericParamDefs();
		InitNestedClass(); // must before typedefs1, because parent may be nested class
		InitTypeDefs_1();

		InitGenericParamConstraintDefs();

		InitParamDefs();
		InitMethodDefs();
		InitFieldDefs();
		InitFieldLayouts();
		InitFieldRVAs();
		InitBlittables();
		InitMethodImpls0();
		InitProperties();
		InitEvents();
		InitMethodSemantics();
		InitConsts();
		InitCustomAttributes();

		InitClassLayouts();

		InitTypeDefs_2();
		InitInterfaces();

		InitClass();

		InitVTables_1();
		InitVTables_2();
	}

	void InterpreterImage::InitTypeDefs_0()
	{
		const Table& typeDefTb = _rawImage.GetTable(TableType::TYPEDEF);
		_typesDefines.resize(typeDefTb.rowNum);
		_typeDetails.resize(typeDefTb.rowNum);
		for (uint32_t i = 0, n = typeDefTb.rowNum; i < n; i++)
		{
			Il2CppTypeDefinition& cur = _typesDefines[i];
			TypeDefinitionDetail& typeDetail = _typeDetails[i];
			typeDetail.index = i;
			typeDetail.typeDef = &cur;
			typeDetail.typeSizes = {};

			uint32_t rowIndex = i + 1;
			TbTypeDef data = _rawImage.ReadTypeDef(rowIndex);

			cur = {};

			cur.genericContainerIndex = kGenericContainerIndexInvalid;
			cur.declaringTypeIndex = kTypeDefinitionIndexInvalid;
			cur.elementTypeIndex = kTypeDefinitionIndexInvalid;

			cur.token = EncodeToken(TableType::TYPEDEF, rowIndex);

			bool isValueType = data.extends && IsValueTypeFromToken(DecodeTypeDefOrRefOrSpecCodedIndexTableType(data.extends), DecodeTypeDefOrRefOrSpecCodedIndexRowIndex(data.extends));
			Il2CppType cppType = {};
			cppType.type = isValueType ? IL2CPP_TYPE_VALUETYPE : IL2CPP_TYPE_CLASS;
			SET_IL2CPPTYPE_VALUE_TYPE(cppType, isValueType);
			cppType.data.typeHandle = (Il2CppMetadataTypeHandle)&cur;
			cur.byvalTypeIndex = AddIl2CppTypeCache(cppType);

			if (IsInterface(cur.flags))
			{
				cur.interfaceOffsetsStart = EncodeWithIndex(0);
				cur.interface_offsets_count = 0;
				cur.vtableStart = EncodeWithIndex(0);
				cur.vtable_count = 0;
			}
			else
			{
				cur.interfaceOffsetsStart = 0;
				cur.interface_offsets_count = 0;
				cur.vtableStart = 0;
				cur.vtable_count = 0;
			}
		}
	}

	void InterpreterImage::InitTypeDefs_1()
	{
		const Table& typeDefTb = _rawImage.GetTable(TableType::TYPEDEF);
		for (uint32_t i = 0, n = typeDefTb.rowNum; i < n; i++)
		{
			Il2CppTypeDefinition& last = _typesDefines[i > 0 ? i - 1 : 0];
			Il2CppTypeDefinition& cur = _typesDefines[i];
			uint32_t rowIndex = i + 1;
			TbTypeDef data = _rawImage.ReadTypeDef(rowIndex); // token from 1

			cur.flags = data.flags;
			cur.nameIndex = EncodeWithIndex(data.typeName);
			cur.namespaceIndex = EncodeWithIndex(data.typeNamespace);

			cur.fieldStart = EncodeWithIndex(data.fieldList - 1);
			cur.methodStart = EncodeWithIndex(data.methodList - 1);

			if (i > 0)
			{
				last.field_count = (uint16_t)(cur.fieldStart - last.fieldStart);
				last.method_count = (uint16_t)(cur.methodStart - last.methodStart);
			}
			if (i == n - 1)
			{
				cur.field_count = (uint16_t)(_rawImage.GetTableRowNum(TableType::FIELD) - DecodeMetadataIndex(cur.fieldStart));
				cur.method_count = (uint16_t)(_rawImage.GetTableRowNum(TableType::METHOD) - DecodeMetadataIndex(cur.methodStart));
			}

			if (data.extends != 0)
			{
				Il2CppType parentType = {};
				ReadTypeFromToken(GetGenericContainerByTypeDefinition(&cur), nullptr, DecodeTypeDefOrRefOrSpecCodedIndexTableType(data.extends), DecodeTypeDefOrRefOrSpecCodedIndexRowIndex(data.extends), parentType);

				if (parentType.type == IL2CPP_TYPE_CLASS || parentType.type == IL2CPP_TYPE_VALUETYPE)
				{
					Il2CppTypeDefinition* parentDef = (Il2CppTypeDefinition*)parentType.data.typeHandle;
					// FIXE ME . check mscorelib
					const char* parentNs = il2cpp::vm::GlobalMetadata::GetStringFromIndex(parentDef->namespaceIndex);
					if (std::strcmp(parentNs, "System") == 0)
					{
						const char* parentName = il2cpp::vm::GlobalMetadata::GetStringFromIndex(parentDef->nameIndex);
						if (std::strcmp(parentName, "Enum") == 0)
						{
							cur.bitfield |= (1 << (il2cpp::vm::kBitIsValueType - 1));
							cur.bitfield |= (1 << (il2cpp::vm::kBitIsEnum - 1));
						}
						else if (std::strcmp(parentName, "ValueType") == 0)
						{
							cur.bitfield |= (1 << (il2cpp::vm::kBitIsValueType - 1));
						}
					}
				}
				cur.parentIndex = AddIl2CppTypeCache(parentType);
			}
			else
			{
				cur.parentIndex = kInvalidIndex;
			}

			cur.elementTypeIndex = kInvalidIndex;
		}
	}

	void InterpreterImage::InitTypeDefs_2()
	{
		const Table& typeDefTb = _rawImage.GetTable(TableType::TYPEDEF);

		for (uint32_t i = 0, n = typeDefTb.rowNum; i < n; i++)
		{
			TbTypeDef data = _rawImage.ReadTypeDef(i + 1); // token from 1

			Il2CppTypeDefinition& last = _typesDefines[i > 0 ? i - 1 : 0];
			Il2CppTypeDefinition& cur = _typesDefines[i];
			uint32_t typeIndex = i; // type index start from 0, diff with field index ...

			// enum element_type == 
			if (cur.bitfield & (1 << (il2cpp::vm::kBitIsEnum - 1)))
			{
				cur.elementTypeIndex = _fieldDetails[DecodeMetadataIndex(cur.fieldStart)].fieldDef.typeIndex;
			}

			auto classLayoutRow = _classLayouts.find(typeIndex);
			uint16_t packingSize = 0;
			if (classLayoutRow != _classLayouts.end())
			{
				auto& layoutData = classLayoutRow->second;
				packingSize = layoutData.packingSize;
			}
			else
			{
				cur.bitfield |= (1 << (il2cpp::vm::kClassSizeIsDefault - 1));
			}
			if (packingSize != 0)
			{
				cur.bitfield |= ((uint32_t)il2cpp::vm::GlobalMetadata::ConvertPackingSizeToEnum((uint8_t)packingSize) << (il2cpp::vm::kPackingSize - 1));
			}
			else
			{
				cur.bitfield |= (1 << (il2cpp::vm::kPackingSizeIsDefault - 1));
			}
		}
	}

	void InterpreterImage::InitParamDefs()
	{
		const Table& tb = _rawImage.GetTable(TableType::PARAM);

		// extra 16 for not name params
		_params.reserve(tb.rowNum + 16);
		//for (uint32_t i = 0; i < tb.rowNum; i++)
		//{
		//	uint32_t rowIndex = i + 1;
		//	Il2CppParameterDefinition& pd = _params[i].paramDef;
		//	TbParam data = _rawImage.ReadParam(rowIndex);

		//	pd.nameIndex = EncodeWithIndex(data.name);
		//	pd.token = EncodeToken(TableType::PARAM, rowIndex);
		//	// pd.typeIndex 在InitMethodDefs中解析signature后填充。
		//}
	}


	void InterpreterImage::InitFieldDefs()
	{
		const Table& fieldTb = _rawImage.GetTable(TableType::FIELD);
		_fieldDetails.resize(fieldTb.rowNum);

		for (size_t i = 0; i < _typesDefines.size(); i++)
		{
			Il2CppTypeDefinition& typeDef = _typesDefines[i];
			uint32_t start = DecodeMetadataIndex(typeDef.fieldStart);
			for (uint32_t k = 0; k < typeDef.field_count; k++)
			{
				FieldDetail& fd = _fieldDetails[start + k];
				fd.typeDefIndex = (uint32_t)i;
			}
		}

		for (uint32_t i = 0, n = fieldTb.rowNum; i < n; i++)
		{
			FieldDetail& fd = _fieldDetails[i];
			Il2CppFieldDefinition& cur = fd.fieldDef;

			fd.offset = 0;
			fd.defaultValueIndex = kDefaultValueIndexNull;

			uint32_t rowIndex = i + 1;
			TbField data = _rawImage.ReadField(rowIndex);

			BlobReader br = _rawImage.GetBlobReaderByRawIndex(data.signature);
			FieldRefSig frs;
			ReadFieldRefSig(br, GetGenericContainerByTypeDefIndex(DecodeMetadataIndex(fd.typeDefIndex)), frs);
			frs.type.attrs = data.flags;

			//cur = {};
			cur.nameIndex = EncodeWithIndex(data.name);
			cur.token = EncodeToken(TableType::FIELD, rowIndex);
			cur.typeIndex = AddIl2CppTypeCache(frs.type);
		}
	}

	void InterpreterImage::InitFieldLayouts()
	{
		const Table& tb = _rawImage.GetTable(TableType::FIELDLAYOUT);
		for (uint32_t i = 0; i < tb.rowNum; i++)
		{
			TbFieldLayout data = _rawImage.ReadFieldLayout(i + 1);
			_fieldDetails[data.field - 1].offset = sizeof(Il2CppObject) + data.offset;
		}
	}

	void InterpreterImage::InitFieldRVAs()
	{
		const Table& tb = _rawImage.GetTable(TableType::FIELDRVA);
		for (uint32_t i = 0; i < tb.rowNum; i++)
		{
			TbFieldRVA data = _rawImage.ReadFieldRVA(i + 1);
			FieldDetail& fd = _fieldDetails[data.field - 1];
			fd.defaultValueIndex = (uint32_t)_fieldDefaultValues.size();

			Il2CppFieldDefaultValue fdv = {};
			fdv.fieldIndex = data.field - 1;
			fdv.typeIndex = fd.fieldDef.typeIndex;

			uint32_t dataImageOffset = (uint32_t)-1;
			bool ret = _rawImage.TranslateRVAToImageOffset(data.rva, dataImageOffset);
			IL2CPP_ASSERT(ret);
			fdv.dataIndex = (DefaultValueDataIndex)EncodeWithIndex(dataImageOffset);
			_fieldDefaultValues.push_back(fdv);
		}
	}

	void InterpreterImage::InitBlittables()
	{
		const Table& typeDefTb = _rawImage.GetTable(TableType::TYPEDEF);

		std::vector<bool> computFlags(typeDefTb.rowNum, false);

		for (uint32_t i = 0, n = typeDefTb.rowNum; i < n; i++)
		{
			ComputeBlittable(&_typesDefines[i], computFlags);
		}
	}

	void InterpreterImage::ComputeBlittable(Il2CppTypeDefinition* def, std::vector<bool>& computFlags)
	{
		if (DecodeImageIndex(def->byvalTypeIndex) != GetIndex())
		{
			return;
		}
		uint32_t typeIndex = GetTypeRawIndex(def);
		if (computFlags[typeIndex])
		{
			return;
		}
		computFlags[typeIndex] = true;

		const Il2CppType* type = GetIl2CppTypeFromRawIndex(DecodeMetadataIndex(def->byvalTypeIndex));

		const char* typeName = il2cpp::vm::GlobalMetadata::GetStringFromIndex(def->nameIndex);


		bool blittable = false;
		if (type->type == IL2CPP_TYPE_VALUETYPE)
		{
			blittable = true;
			for (int i = 0; i < def->field_count; i++)
			{
				const Il2CppFieldDefinition* field = GetFieldDefinitionFromRawIndex(DecodeMetadataIndex(def->fieldStart + i));
				const Il2CppType* fieldType = il2cpp::vm::GlobalMetadata::GetIl2CppTypeFromIndex(field->typeIndex);
				if (!huatuo::metadata::IsInstanceField(fieldType))
				{
					continue;
				}

				switch (fieldType->type)
				{
				case IL2CPP_TYPE_BOOLEAN:
				case IL2CPP_TYPE_CHAR:
				case IL2CPP_TYPE_I1:
				case IL2CPP_TYPE_U1:
				case IL2CPP_TYPE_I2:
				case IL2CPP_TYPE_U2:
				case IL2CPP_TYPE_I4:
				case IL2CPP_TYPE_U4:
				case IL2CPP_TYPE_I:
				case IL2CPP_TYPE_U:
				case IL2CPP_TYPE_I8:
				case IL2CPP_TYPE_U8:
				case IL2CPP_TYPE_R4:
				case IL2CPP_TYPE_R8:
				case IL2CPP_TYPE_PTR:
				case IL2CPP_TYPE_FNPTR:
				{
					break;
				}
				case IL2CPP_TYPE_VALUETYPE:
				{
					Il2CppTypeDefinition* fieldDef = (Il2CppTypeDefinition*)fieldType->data.typeHandle;
					ComputeBlittable(fieldDef, computFlags);
					blittable = fieldDef->bitfield & (1 << (il2cpp::vm::kBitIsBlittable - 1));
					break;
				}
				default:
				{
					blittable = false;
				}
				}
				if (!blittable)
				{
					break;
				}
			}
		}
		if (blittable)
		{
			def->bitfield |= (1 << (il2cpp::vm::kBitIsBlittable - 1));
		}
	}

	void InterpreterImage::InitConsts()
	{
		const Table& tb = _rawImage.GetTable(TableType::CONSTANT);
		for (uint32_t i = 0; i < tb.rowNum; i++)
		{
			TbConstant data = _rawImage.ReadConstant(i + 1);
			TableType parentType = DecodeHasConstantType(data.parent);
			uint32_t rowIndex = DecodeHashConstantIndex(data.parent);

			Il2CppType type = {};
			type.type = (Il2CppTypeEnum)data.type;
			TypeIndex dataTypeIndex = AddIl2CppTypeCache(type);

			switch (parentType)
			{
			case TableType::FIELD:
			{
				FieldDetail& fd = _fieldDetails[rowIndex - 1];
				fd.defaultValueIndex = (uint32_t)_fieldDefaultValues.size();

				Il2CppFieldDefaultValue fdv = {};
				fdv.fieldIndex = rowIndex - 1;
				fdv.typeIndex = dataTypeIndex;
				uint32_t dataImageOffset = _rawImage.GetImageOffsetOfBlob(data.value);
				fdv.dataIndex = (DefaultValueDataIndex)EncodeWithIndex(dataImageOffset);
				_fieldDefaultValues.push_back(fdv);
				break;
			}
			case TableType::PARAM:
			{
				ParamDetail& fd = _params[rowIndex - 1];
				fd.defaultValueIndex = (uint32_t)_paramDefaultValues.size();

				Il2CppParameterDefaultValue pdv = {};
				pdv.typeIndex = dataTypeIndex;
				pdv.parameterIndex = fd.parameterIndex;
				uint32_t dataImageOffset = _rawImage.GetImageOffsetOfBlob(data.value);
				pdv.dataIndex = (DefaultValueDataIndex)EncodeWithIndex(dataImageOffset);
				_paramDefaultValues.push_back(pdv);
				break;
			}
			case TableType::PROPERTY:
			{
				RaiseHuatuoNotSupportedException("not support property const");
				break;
			}
			default:
			{
				RaiseHuatuoExecutionEngineException("not support const TableType");
				break;
			}
			}
		}
	}

	void InterpreterImage::InitCustomAttributes()
	{
		const Table& tb = _rawImage.GetTable(TableType::CUSTOMATTRIBUTE);
		_tokenCustomAttributes.reserve(tb.rowNum);


		uint32_t threadStaticMethodToken = 0;
		Il2CppCustomAttributeTypeRange* curTypeRange = nullptr;
		for (uint32_t rowIndex = 1; rowIndex <= tb.rowNum; rowIndex++)
		{
			TbCustomAttribute data = _rawImage.ReadCustomAttribute(rowIndex);
			TableType parentType = DecodeHasCustomAttributeCodedIndexTableType(data.parent);
			uint32_t parentRowIndex = DecodeHasCustomAttributeCodedIndexRowIndex(data.parent);
			uint32_t token = EncodeToken(parentType, parentRowIndex);
			if (curTypeRange == nullptr || curTypeRange->token != token)
			{
				IL2CPP_ASSERT(_tokenCustomAttributes.find(token) == _tokenCustomAttributes.end());
				int32_t attributeStartIndex = EncodeWithIndex((int32_t)_customAttribues.size());
				int32_t handleIndex = (int32_t)_customAttributeHandles.size();
				_tokenCustomAttributes[token] = { handleIndex };
#ifdef HUATUO_UNITY_2021_OR_NEW
				_customAttributeHandles.push_back({ token, (uint32_t)attributeStartIndex });
#else
				_customAttributeHandles.push_back({ token, attributeStartIndex, 0 });
#endif
				curTypeRange = &_customAttributeHandles[handleIndex];
			}
#ifndef HUATUO_UNITY_2021_OR_NEW
			++curTypeRange->count;
#endif

			TableType ctorMethodTableType = DecodeCustomAttributeTypeCodedIndexTableType(data.type);
			uint32_t ctorMethodRowIndex = DecodeCustomAttributeTypeCodedIndexRowIndex(data.type);
			uint32_t ctorMethodToken = EncodeToken(ctorMethodTableType, ctorMethodRowIndex);
			//CustomAttribute ca = { ctorMethodToken, data.value };
			//ca.value = data.value;
			//ReadMethodRefInfoFromToken(nullptr, nullptr, , ca.attrCtorMethod);
			_customAttribues.push_back({ ctorMethodToken, data.value });

			if (parentType == TableType::FIELD)
			{
				// try set thread static flags
				if (threadStaticMethodToken == 0)
				{
					if (IsThreadStaticCtorToken(ctorMethodTableType, ctorMethodRowIndex))
					{
						threadStaticMethodToken = ctorMethodToken;
					}
				}
				if (ctorMethodToken == threadStaticMethodToken)
				{
					IL2CPP_ASSERT(threadStaticMethodToken != 0);
					_fieldDetails[parentRowIndex - 1].offset = THREAD_LOCAL_STATIC_MASK;
				}
			}

		}
		IL2CPP_ASSERT(_tokenCustomAttributes.size() == _customAttributeHandles.size());
#ifdef HUATUO_UNITY_2021_OR_NEW
		// add extra Il2CppCustomAttributeTypeRange for compute count
		_customAttributeHandles.push_back({ 0, EncodeWithIndex((int32_t)_customAttribues.size()) });
#endif
		_customAttribtesCaches.resize(_tokenCustomAttributes.size());
	}

	CustomAttributesCache* InterpreterImage::GenerateCustomAttributesCacheInternal(CustomAttributeIndex index)
	{
		IL2CPP_ASSERT(index != kCustomAttributeIndexInvalid);
		CustomAttributesCache* cache = _customAttribtesCaches[index];
		if (cache)
		{
			return cache;
		}
		IL2CPP_ASSERT(index < (CustomAttributeIndex)_customAttributeHandles.size());

		Il2CppCustomAttributeTypeRange& typeRange = _customAttributeHandles[index];

		il2cpp::os::FastAutoLock metaLock(&il2cpp::vm::g_MetadataLock);
		cache = _customAttribtesCaches[index];
		if (cache)
		{
			return cache;
		}

		huatuo::interpreter::ExecutingInterpImageScope scope(huatuo::interpreter::InterpreterModule::GetCurrentThreadMachineState(), this->_il2cppImage);

		cache = (CustomAttributesCache*)IL2CPP_CALLOC(1, sizeof(CustomAttributesCache));
		int32_t count;
#ifdef HUATUO_UNITY_2021_OR_NEW
		count = (int32_t)(_customAttributeHandles[index + 1].startOffset - typeRange.startOffset);
#else
		count = (int32_t)typeRange.count;
#endif
		cache->count = count;
		cache->attributes = (Il2CppObject**)il2cpp::gc::GarbageCollector::AllocateFixed(sizeof(Il2CppObject*) * count, 0);

		int32_t start = DecodeMetadataIndex(GET_CUSTOM_ATTRIBUTE_TYPE_RANGE_START(typeRange));
		for (int32_t i = 0; i < count; i++)
		{
			int32_t attrIndex = start + i;
			IL2CPP_ASSERT(attrIndex >= 0 && attrIndex < (int32_t)_customAttribues.size());
			CustomAttribute& ca = _customAttribues[attrIndex];
			MethodRefInfo mri = {};
			ReadMethodRefInfoFromToken(nullptr, nullptr, DecodeTokenTableType(ca.ctorMethodToken), DecodeTokenRowIndex(ca.ctorMethodToken), mri);
			const MethodInfo* ctorMethod = GetMethodInfoFromMethodDef(&mri.containerType, mri.methodDef);
			IL2CPP_ASSERT(ctorMethod);
			Il2CppClass* klass = ctorMethod->klass;
			Il2CppObject* attr = il2cpp::vm::Object::New(klass);
			Il2CppArray* paramArr = nullptr;
			if (ca.value != 0)
			{
				BlobReader reader = _rawImage.GetBlobReaderByRawIndex(ca.value);
				ConstructCustomAttribute(reader, attr, ctorMethod);
			}
			else
			{
				IL2CPP_ASSERT(ctorMethod->parameters_count == 0);
				il2cpp::vm::Runtime::Invoke(ctorMethod, attr, nullptr, nullptr);
			}

			cache->attributes[i] = attr;
			il2cpp::gc::GarbageCollector::SetWriteBarrier((void**)cache->attributes + i);
		}

		il2cpp::os::Atomic::FullMemoryBarrier();
		_customAttribtesCaches[index] = cache;
		return cache;
	}

	void InterpreterImage::InitMethodDefs0()
	{
		const Table& typeDefTb = _rawImage.GetTable(TableType::TYPEDEF);
		const Table& methodTb = _rawImage.GetTable(TableType::METHOD);

		_methodDefines.resize(methodTb.rowNum);
		for (Il2CppMethodDefinition& md : _methodDefines)
		{
			md.genericContainerIndex = kGenericContainerIndexInvalid;
		}
	}

	void InterpreterImage::InitMethodDefs()
	{
		const Table& typeDefTb = _rawImage.GetTable(TableType::TYPEDEF);
		const Table& methodTb = _rawImage.GetTable(TableType::METHOD);

		for (uint32_t i = 0, n = typeDefTb.rowNum; i < n; i++)
		{
			Il2CppTypeDefinition& typeDef = _typesDefines[i];
			uint32_t rawMethodStart = DecodeMetadataIndex(typeDef.methodStart);

			for (int m = 0; m < typeDef.method_count; m++)
			{
				Il2CppMethodDefinition& md = _methodDefines[rawMethodStart + m];
				md.declaringType = EncodeWithIndex(i);
			}
		}


		_methodDefine2InfoCaches.resize(methodTb.rowNum);
		_methodBodies.resize(methodTb.rowNum);

		int32_t paramTableRowNum = _rawImage.GetTable(TableType::PARAM).rowNum;
		for (uint32_t index = 0; index < methodTb.rowNum; index++)
		{
			Il2CppMethodDefinition& md = _methodDefines[index];
			uint32_t rowIndex = index + 1;
			TbMethod methodData = _rawImage.ReadMethod(rowIndex);

			md.nameIndex = EncodeWithIndex(methodData.name);
			md.parameterStart = methodData.paramList - 1;
			//md.genericContainerIndex = kGenericContainerIndexInvalid;
			md.token = EncodeToken(TableType::METHOD, rowIndex);
			md.flags = methodData.flags;
			md.iflags = methodData.implFlags;
			md.slot = kInvalidIl2CppMethodSlot;
			if (index > 0)
			{
				auto& last = _methodDefines[index - 1];
				last.parameterCount = md.parameterStart - last.parameterStart;
			}
			if (index == methodTb.rowNum - 1)
			{
				md.parameterCount = (int)paramTableRowNum - (int32_t)md.parameterStart;
			}

			MethodBody& body = _methodBodies[index];
			ReadMethodBody(md, methodData, body);
		}

		for (uint32_t i = 0, n = typeDefTb.rowNum; i < n; i++)
		{
			Il2CppTypeDefinition& typeDef = _typesDefines[i];
			uint32_t rawMethodStart = DecodeMetadataIndex(typeDef.methodStart);

			for (int m = 0; m < typeDef.method_count; m++)
			{
				Il2CppMethodDefinition& md = _methodDefines[rawMethodStart + m];
				const char* methodName = _rawImage.GetStringFromRawIndex(DecodeMetadataIndex(md.nameIndex));
				if (!std::strcmp(methodName, ".cctor"))
				{
					typeDef.bitfield |= (1 << (il2cpp::vm::kBitHasStaticConstructor - 1));
				}
				if (!std::strcmp(methodName, "Finalize"))
				{
					typeDef.bitfield |= (1 << (il2cpp::vm::kBitHasFinalizer - 1));
				}
				// TODO 可以考虑优化一下,将 signature在前一步存到暂时不用的 returnType里
				TbMethod methodData = _rawImage.ReadMethod(rawMethodStart + m + 1);

				BlobReader methodSigReader = _rawImage.GetBlobReaderByRawIndex(methodData.signature);
				uint32_t namedParamStart = md.parameterStart;
				uint32_t namedParamCount = md.parameterCount;

				uint32_t actualParamStart = (uint32_t)_params.size();
				ReadMethodDefSig(
					methodSigReader,
					GetGenericContainerByTypeDefinition(&typeDef),
					GetGenericContainerByRawIndex(DecodeMetadataIndex(md.genericContainerIndex)),
					md,
					_params);
				uint32_t actualParamCount = (uint32_t)_params.size() - actualParamStart;
				md.parameterStart = actualParamStart;
				md.parameterCount = actualParamCount;
				for (uint32_t paramRowIndex = namedParamStart + 1; paramRowIndex <= namedParamStart + namedParamCount; paramRowIndex++)
				{
					TbParam data = _rawImage.ReadParam(paramRowIndex);
					if (data.sequence > 0)
					{
						ParamDetail& paramDetail = _params[actualParamStart + data.sequence - 1];
						Il2CppParameterDefinition& pd = paramDetail.paramDef;
						IL2CPP_ASSERT(paramDetail.parameterIndex == data.sequence - 1);
						pd.nameIndex = EncodeWithIndex(data.name);
						pd.token = EncodeToken(TableType::PARAM, paramRowIndex);
					}
					else
					{
						// data.sequence == 0  is for returnType.
						// used for parent of CustomeAttributes of ReturnType
						// il2cpp not support ReturnType CustomAttributes. so we just ignore it.
					}
				}
			}
		}
	}

	void InterpreterImage::InitMethodImpls0()
	{
		const Table& miTb = _rawImage.GetTable(TableType::METHODIMPL);
		for (uint32_t i = 0; i < miTb.rowNum; i++)
		{
			TbMethodImpl data = _rawImage.ReadMethodImpl(i + 1);
			TypeDefinitionDetail& tdd = _typeDetails[data.classIdx - 1];
			Il2CppGenericContainer* gc = GetGenericContainerByTypeDefinition(tdd.typeDef);
			MethodImpl impl;
			ReadMethodRefInfoFromToken(gc, nullptr, DecodeMethodDefOrRefCodedIndexTableType(data.methodBody), DecodeMethodDefOrRefCodedIndexRowIndex(data.methodBody), impl.body);
			ReadMethodRefInfoFromToken(gc, nullptr, DecodeMethodDefOrRefCodedIndexTableType(data.methodDeclaration), DecodeMethodDefOrRefCodedIndexRowIndex(data.methodDeclaration), impl.declaration);
			tdd.methodImpls.push_back(impl);
		}
	}

	void InterpreterImage::InitProperties()
	{
		const Table& propertyMapTb = _rawImage.GetTable(TableType::PROPERTYMAP);
		const Table& propertyTb = _rawImage.GetTable(TableType::PROPERTY);
		_propeties.reserve(propertyTb.rowNum);

		Il2CppTypeDefinition* last = nullptr;
		for (uint32_t rowIndex = 1; rowIndex <= propertyMapTb.rowNum; rowIndex++)
		{
			TbPropertyMap data = _rawImage.ReadPropertyMap(rowIndex);
			TypeDefinitionDetail& cur = _typeDetails[data.parent - 1];
			cur.typeDef->propertyStart = EncodeWithIndex(data.propertyList); // start from 1
			if (last != nullptr)
			{
				last->property_count = data.propertyList - DecodeMetadataIndex(last->propertyStart);
			}
			last = cur.typeDef;
		}
		if (last)
		{
			last->property_count = propertyTb.rowNum - DecodeMetadataIndex(last->propertyStart) + 1;
		}

		for (uint32_t rowIndex = 1; rowIndex <= propertyTb.rowNum; rowIndex++)
		{
			TbProperty data = _rawImage.ReadProperty(rowIndex);
			_propeties.push_back({ _rawImage.GetStringFromRawIndex(data.name), data.flags, data.type, 0, 0 });
		}
	}

	void InterpreterImage::InitEvents()
	{
		const Table& eventMapTb = _rawImage.GetTable(TableType::EVENTMAP);
		const Table& eventTb = _rawImage.GetTable(TableType::EVENT);
		_events.reserve(eventTb.rowNum);

		Il2CppTypeDefinition* last = nullptr;
		for (uint32_t rowIndex = 1; rowIndex <= eventMapTb.rowNum; rowIndex++)
		{
			TbEventMap data = _rawImage.ReadEventMap(rowIndex);
			TypeDefinitionDetail& cur = _typeDetails[data.parent - 1];
			cur.typeDef->eventStart = EncodeWithIndex(data.eventList); // start from 1
			if (last != nullptr)
			{
				last->event_count = data.eventList - DecodeMetadataIndex(last->eventStart);
			}
			last = cur.typeDef;
		}
		if (last)
		{
			last->event_count = eventTb.rowNum - DecodeMetadataIndex(last->eventStart) + 1;
		}

		for (uint32_t rowIndex = 1; rowIndex <= eventTb.rowNum; rowIndex++)
		{
			TbEvent data = _rawImage.ReadEvent(rowIndex);
			_events.push_back({ _rawImage.GetStringFromRawIndex(data.name), data.eventFlags, data.eventType, 0, 0, 0 });
		}
	}


	void InterpreterImage::InitMethodSemantics()
	{
		const Table& msTb = _rawImage.GetTable(TableType::METHODSEMANTICS);
		for (uint32_t rowIndex = 1; rowIndex <= msTb.rowNum; rowIndex++)
		{
			TbMethodSemantics data = _rawImage.ReadMethodSemantics(rowIndex);
			uint32_t method = data.method;
			uint16_t semantics = data.semantics;
			TableType tableType = DecodeHasSemanticsCodedIndexTableType(data.association);
			uint32_t propertyOrEventIndex = DecodeHasSemanticsCodedIndexRowIndex(data.association) - 1;
			if (semantics & (uint16_t)MethodSemanticsAttributes::Getter)
			{
				IL2CPP_ASSERT(tableType == TableType::PROPERTY);
				_propeties[propertyOrEventIndex].getterMethodIndex = method;
			}
			if (semantics & (uint16_t)MethodSemanticsAttributes::Setter)
			{
				IL2CPP_ASSERT(tableType == TableType::PROPERTY);
				_propeties[propertyOrEventIndex].setterMethodIndex = method;
			}
			if (semantics & (uint16_t)MethodSemanticsAttributes::AddOn)
			{
				IL2CPP_ASSERT(tableType == TableType::EVENT);
				_events[propertyOrEventIndex].addMethodIndex = method;
			}
			if (semantics & (uint16_t)MethodSemanticsAttributes::RemoveOn)
			{
				IL2CPP_ASSERT(tableType == TableType::EVENT);
				_events[propertyOrEventIndex].removeMethodIndex = method;
			}
			if (semantics & (uint16_t)MethodSemanticsAttributes::Fire)
			{
				IL2CPP_ASSERT(tableType == TableType::EVENT);
				_events[propertyOrEventIndex].fireMethodIndex = method;
			}
		}
	}

	struct EnclosingClassInfo
	{
		uint32_t enclosingTypeIndex; // rowIndex - 1
		std::vector<uint32_t> nestedTypeIndexs;
	};

	void InterpreterImage::InitNestedClass()
	{
		const Table& nestedClassTb = _rawImage.GetTable(TableType::NESTEDCLASS);
		_nestedTypeDefineIndexs.reserve(nestedClassTb.rowNum);
		std::vector<EnclosingClassInfo> enclosingTypes;

		for (uint32_t i = 0; i < nestedClassTb.rowNum; i++)
		{
			TbNestedClass data = _rawImage.ReadNestedClass(i + 1);
			Il2CppTypeDefinition& nestedType = _typesDefines[data.nestedClass - 1];
			Il2CppTypeDefinition& enclosingType = _typesDefines[data.enclosingClass - 1];
			if (enclosingType.nested_type_count == 0)
			{
				// 此行代码不能删，用于标识 enclosingTypes的index
				enclosingType.nestedTypesStart = (uint32_t)enclosingTypes.size();
				enclosingTypes.push_back({ data.enclosingClass - 1 });
			}
			++enclosingType.nested_type_count;
			enclosingTypes[enclosingType.nestedTypesStart].nestedTypeIndexs.push_back(data.nestedClass - 1);
			//_nestedTypeDefineIndexs.push_back(data.nestedClass - 1);
			nestedType.declaringTypeIndex = enclosingType.byvalTypeIndex;
		}

		for (auto& enclosingType : enclosingTypes)
		{
			Il2CppTypeDefinition& enclosingTypeDef = _typesDefines[enclosingType.enclosingTypeIndex];
			IL2CPP_ASSERT(enclosingType.nestedTypeIndexs.size() == (size_t)enclosingTypeDef.nested_type_count);
			enclosingTypeDef.nestedTypesStart = (NestedTypeIndex)_nestedTypeDefineIndexs.size();
			enclosingTypeDef.nested_type_count = (uint16_t)enclosingType.nestedTypeIndexs.size();
			_nestedTypeDefineIndexs.insert(_nestedTypeDefineIndexs.end(), enclosingType.nestedTypeIndexs.begin(), enclosingType.nestedTypeIndexs.end());
		}
	}

	void InterpreterImage::InitClassLayouts()
	{
		const Table& classLayoutTb = _rawImage.GetTable(TableType::CLASSLAYOUT);
		for (uint32_t i = 0; i < classLayoutTb.rowNum; i++)
		{
			TbClassLayout data = _rawImage.ReadClassLayout(i + 1);
			_classLayouts[data.parent - 1] = data;
			if (data.classSize > 0)
			{
				Il2CppTypeDefinitionSizes& typeSizes = _typeDetails[data.parent - 1].typeSizes;
				typeSizes.instance_size = typeSizes.native_size = sizeof(Il2CppObject) + data.classSize;
			}
		}
	}

	uint32_t InterpreterImage::AddIl2CppTypeCache(Il2CppType& type)
	{
		//// TODO 优化
		uint32_t index = (uint32_t)_types.size();
		_types.push_back(type);
		return EncodeWithIndex(index);
	}

	uint32_t InterpreterImage::AddIl2CppGenericContainers(Il2CppGenericContainer& geneContainer)
	{
		uint32_t index = (uint32_t)_genericContainers.size();
		_genericContainers.push_back(geneContainer);
		return EncodeWithIndex(index);
	}

	void InterpreterImage::InitClass()
	{
		const Table& typeDefTb = _rawImage.GetTable(TableType::TYPEDEF);
		_classList.resize(typeDefTb.rowNum);
	}

	Il2CppClass* InterpreterImage::GetTypeInfoFromTypeDefinitionRawIndex(uint32_t index)
	{
		IL2CPP_ASSERT(index < _classList.size());
		Il2CppClass* klass = _classList[index];
		if (klass)
		{
			return klass;
		}
		klass = il2cpp::vm::GlobalMetadata::FromTypeDefinition(EncodeWithIndex(index));
		IL2CPP_ASSERT(klass->interfaces_count <= klass->interface_offsets_count || _typesDefines[index].interfaceOffsetsStart == 0);
		il2cpp::os::Atomic::FullMemoryBarrier();
		_classList[index] = klass;
		return klass;
	}

	const Il2CppType* InterpreterImage::GetInterfaceFromOffset(const Il2CppClass* klass, TypeInterfaceIndex offset)
	{
		const Il2CppTypeDefinition* typeDef = (const Il2CppTypeDefinition*)(klass->typeMetadataHandle);
		IL2CPP_ASSERT(typeDef);
		return GetInterfaceFromOffset(typeDef, offset);
	}

	const Il2CppType* InterpreterImage::GetInterfaceFromOffset(const Il2CppTypeDefinition* typeDef, TypeInterfaceIndex offset)
	{
		uint32_t globalOffset = typeDef->interfacesStart + offset;
		IL2CPP_ASSERT(globalOffset < (uint32_t)_interfaceDefines.size());
		return &_types[_interfaceDefines[globalOffset]];
	}

	Il2CppInterfaceOffsetInfo InterpreterImage::GetInterfaceOffsetInfo(const Il2CppTypeDefinition* typeDefine, TypeInterfaceOffsetIndex index)
	{
		uint32_t globalIndex = DecodeMetadataIndex((uint32_t)(typeDefine->interfaceOffsetsStart + index));
		IL2CPP_ASSERT(globalIndex < (uint32_t)_interfaceOffsets.size());

		InterfaceOffsetInfo& offsetPair = _interfaceOffsets[globalIndex];
		return { offsetPair.type, (int32_t)offsetPair.offset };
	}

	Il2CppClass* InterpreterImage::GetNestedTypeFromOffset(const Il2CppClass* klass, TypeNestedTypeIndex offset)
	{
		uint32_t globalIndex = ((Il2CppTypeDefinition*)klass->typeMetadataHandle)->nestedTypesStart + offset;
		IL2CPP_ASSERT(globalIndex < (uint32_t)_nestedTypeDefineIndexs.size());
		uint32_t typeDefIndex = _nestedTypeDefineIndexs[globalIndex];
		IL2CPP_ASSERT(typeDefIndex < (uint32_t)_typesDefines.size());
		return il2cpp::vm::GlobalMetadata::GetTypeInfoFromHandle((Il2CppMetadataTypeHandle)&_typesDefines[typeDefIndex]);
	}

	Il2CppTypeDefinition* InterpreterImage::GetNestedTypes(Il2CppTypeDefinition* typeDefinition, void** iter)
	{
		if (_nestedTypeDefineIndexs.empty())
		{
			return nullptr;
		}
		const TypeDefinitionIndex* nestedTypeIndices = (const TypeDefinitionIndex*)(&_nestedTypeDefineIndexs[typeDefinition->nestedTypesStart]);

		if (!*iter)
		{
			if (typeDefinition->nested_type_count == 0)
				return NULL;

			*iter = (void*)(nestedTypeIndices);
			return &_typesDefines[nestedTypeIndices[0]];
		}

		TypeDefinitionIndex* nestedTypeAddress = (TypeDefinitionIndex*)*iter;
		nestedTypeAddress++;
		ptrdiff_t index = nestedTypeAddress - nestedTypeIndices;

		if (index < typeDefinition->nested_type_count)
		{
			*iter = nestedTypeAddress;
			return &_typesDefines[*nestedTypeAddress];
		}

		return NULL;
	}

	const Il2CppAssembly* InterpreterImage::GetReferencedAssembly(int32_t referencedAssemblyTableIndex, const Il2CppAssembly assembliesTable[], int assembliesCount)
	{
		auto& table = _rawImage.GetTable(TableType::ASSEMBLYREF);
		IL2CPP_ASSERT((uint32_t)referencedAssemblyTableIndex < table.rowNum);

		TbAssemblyRef assRef = _rawImage.ReadAssemblyRef(referencedAssemblyTableIndex + 1);
		const char* refAssName = _rawImage.GetStringFromRawIndex(assRef.name);
		const Il2CppAssembly* il2cppAssRef = il2cpp::vm::Assembly::GetLoadedAssembly(refAssName);
		if (!il2cppAssRef)
		{
			il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetDllNotFoundException(refAssName));
		}
		return il2cppAssRef;
	}

	// FIXME 此处bug较多，仍需要仔细检查
	const MethodInfo* InterpreterImage::GetMethodInfo(const Il2CppType* containerType, const Il2CppMethodDefinition* methodDef, const Il2CppGenericInst* instantiation, const Il2CppGenericContext* genericContext)
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

	const MethodInfo* InterpreterImage::ResolveMethodInfo(const Il2CppType* type, const char* resolveMethodName, const MethodRefSig& resolveSig, const Il2CppGenericInst* genericInstantiation, const Il2CppGenericContext* genericContext)
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

	const MethodInfo* InterpreterImage::ReadMethodInfoFromToken(const Il2CppGenericContainer* klassGenericContainer,
		const Il2CppGenericContainer* methodGenericContainer, const Il2CppGenericContext* genericContext, Il2CppGenericInst* genericInst, TableType tableType, uint32_t rowIndex)
	{
		IL2CPP_ASSERT(rowIndex > 0);
		switch (tableType)
		{
		case TableType::METHOD:
		{
			const Il2CppMethodDefinition* methodDef = GetMethodDefinitionFromRawIndex(rowIndex - 1);
			const Il2CppType* type = GetIl2CppTypeFromRawIndex(DecodeMetadataIndex(GetTypeFromRawIndex(DecodeMetadataIndex(methodDef->declaringType))->byvalTypeIndex));
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


	const MethodInfo* InterpreterImage::GetMethodInfoFromToken(uint32_t token, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, const Il2CppGenericContext* genericContext)
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

		//MethodRefInfo mri = {};
		//ReadMethodRefInfoFromToken(klassGenericContainer, methodGenericContainer, , , mri);
		//const Il2CppType* finalContainerType = TryInflateIfNeed(&mri.containerType, genericContext, true);
		//const MethodInfo* method = GetMethodInfoFromMethodDef(&mri.containerType, mri.methodDef);
		//// final genericContext = finalContainerType.class_inst + mri.instantiation
		//if (mri.instantiation)

		//{
		//	const Il2CppGenericInst* finalClassIns = finalContainerType->type == IL2CPP_TYPE_GENERICINST ? finalContainerType->data.generic_class->context.class_inst : nullptr;
		//	const Il2CppGenericInst* finalMethodIns = mri.instantiation;
		//	Il2CppGenericContext finalGenericContext = { finalClassIns, finalMethodIns };
		//	method = method->is_inflated ? method->genericMethod->methodDefinition : method;
		//	method = il2cpp::metadata::GenericMetadata::Inflate(method, &finalGenericContext);
		//}

		IL2CPP_ASSERT(method);
		{
			il2cpp::os::FastAutoLock lock(&il2cpp::vm::g_MetadataLock);
			_token2ResolvedDataCache.insert({ key, (void*)method });
		}
		return method;
	}

	void InterpreterImage::GetStandAloneMethodSigFromToken(uint32_t token, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, const Il2CppGenericContext* genericContext, ResolveStandAloneMethodSig& methodSig)
	{
		ReadStandAloneSig(token, klassGenericContainer, methodGenericContainer, methodSig);
		if (genericContext)
		{
			// FIXME. memory leak
			methodSig.returnType = *TryInflateIfNeed(&methodSig.returnType, genericContext, true);
			for (uint32_t i = 0; i < methodSig.paramCount; i++)
			{
				methodSig.params[i] = *TryInflateIfNeed(methodSig.params + i, genericContext, true);
			}
		}
	}

	void InterpreterImage::ReadFieldRefInfoFromToken(const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, TableType tableType, uint32_t rowIndex, FieldRefInfo& ret)
	{
		IL2CPP_ASSERT(rowIndex > 0);
		if (tableType == TableType::FIELD)
		{
			const FieldDetail& fd = GetFieldDetailFromRawIndex(rowIndex - 1);
			ret.containerType = *GetIl2CppTypeFromRawTypeDefIndex(DecodeMetadataIndex(fd.typeDefIndex));
			ret.field = &fd.fieldDef;
			//ret.classType = *image.GetIl2CppTypeFromRawTypeDefIndex(DecodeMetadataIndex(ret.field->typeIndex));
		}
		else
		{
			IL2CPP_ASSERT(tableType == TableType::MEMBERREF);
			ReadFieldRefInfoFromMemberRef(klassGenericContainer, methodGenericContainer, rowIndex, ret);
		}
	}

	const FieldInfo* InterpreterImage::GetFieldInfoFromToken(uint32_t token, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, const Il2CppGenericContext* genericContext)
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

	void InterpreterImage::GetClassAndMethodGenericContainerFromGenericContainerIndex(GenericContainerIndex idx, const Il2CppGenericContainer*& klassGc, const Il2CppGenericContainer*& methodGc)
	{
		Il2CppGenericContainer* gc = GetGenericContainerByRawIndex(DecodeMetadataIndex(idx));
		IL2CPP_ASSERT(gc);
		if (gc->is_method)
		{
			const Il2CppMethodDefinition* methodDef = GetMethodDefinitionFromRawIndex(DecodeMetadataIndex(gc->ownerIndex));
			klassGc = GetGenericContainerByTypeDefIndex(DecodeMetadataIndex(methodDef->declaringType));
			methodGc = GetGenericContainerByRawIndex(DecodeMetadataIndex(methodDef->genericContainerIndex));
		}
		else
		{
			klassGc = gc;
			methodGc = nullptr;
		}
	}

	void InterpreterImage::InitGenericParamConstraintDefs()
	{
		const Table& tb = _rawImage.GetTable(TableType::GENERICPARAMCONSTRAINT);
		_genericConstraints.resize(tb.rowNum);
		for (uint32_t i = 0; i < tb.rowNum; i++)
		{
			uint32_t rowIndex = i + 1;
			TbGenericParamConstraint data = _rawImage.ReadGenericParamConstraint(rowIndex);
			Il2CppGenericParameter& genericParam = _genericParams[data.owner - 1];

			if (genericParam.constraintsCount == 0)
			{
				genericParam.constraintsStart = i;
			}
			++genericParam.constraintsCount;

			Il2CppType paramCons = {};

			const Il2CppGenericContainer* klassGc;
			const Il2CppGenericContainer* methodGc;
			GetClassAndMethodGenericContainerFromGenericContainerIndex(genericParam.ownerIndex, klassGc, methodGc);

			ReadTypeFromToken(klassGc, methodGc, DecodeTypeDefOrRefOrSpecCodedIndexTableType(data.constraint), DecodeTypeDefOrRefOrSpecCodedIndexRowIndex(data.constraint), paramCons);
			_genericConstraints[i] = DecodeMetadataIndex(AddIl2CppTypeCache(paramCons));
		}
	}

	void InterpreterImage::InitGenericParamDefs0()
	{
		const Table& tb = _rawImage.GetTable(TableType::GENERICPARAM);
		_genericParams.resize(tb.rowNum);
	}

	void InterpreterImage::InitGenericParamDefs()
	{
		const Table& tb = _rawImage.GetTable(TableType::GENERICPARAM);
		for (uint32_t i = 0; i < tb.rowNum; i++)
		{
			uint32_t rowIndex = i + 1;
			TbGenericParam data = _rawImage.ReadGenericParam(rowIndex);
			Il2CppGenericParameter& paramDef = _genericParams[i];
			paramDef.num = data.number;
			paramDef.flags = data.flags;
			paramDef.nameIndex = data.name;
			// constraintsStart 和 constrantsCount init at InitGenericParamConstrains() latter

			TableType ownerType = DecodeTypeOrMethodDefCodedIndexTableType(data.owner);
			uint32_t ownerIndex = DecodeTypeOrMethodDefCodedIndexRowIndex(data.owner);
			IL2CPP_ASSERT(ownerIndex > 0);
			Il2CppGenericContainer* geneContainer;
			int32_t interIndex = ownerIndex - 1;
			if (ownerType == TableType::TYPEDEF)
			{
				Il2CppTypeDefinition& typeDef = _typesDefines[interIndex];
				if (typeDef.genericContainerIndex == kGenericContainerIndexInvalid)
				{
					Il2CppGenericContainer c = {};
					c.ownerIndex = EncodeWithIndex(interIndex);
					c.is_method = false;
					typeDef.genericContainerIndex = AddIl2CppGenericContainers(c);
				}
				geneContainer = &_genericContainers[DecodeMetadataIndex(typeDef.genericContainerIndex)];
				paramDef.ownerIndex = typeDef.genericContainerIndex;
			}
			else
			{
				Il2CppMethodDefinition& methodDef = _methodDefines[interIndex];
				if (methodDef.genericContainerIndex == kGenericContainerIndexInvalid)
				{
					Il2CppGenericContainer c = {};
					c.ownerIndex = EncodeWithIndex(interIndex);
					c.is_method = true;
					methodDef.genericContainerIndex = AddIl2CppGenericContainers(c);
				}
				geneContainer = &_genericContainers[DecodeMetadataIndex(methodDef.genericContainerIndex)];
				paramDef.ownerIndex = methodDef.genericContainerIndex;
			}
			if (geneContainer->type_argc == 0)
			{
				geneContainer->genericParameterStart = i;
			}
			++geneContainer->type_argc;
		}
	}


	void InterpreterImage::InitInterfaces()
	{
		const Table& table = _rawImage.GetTable(TableType::INTERFACEIMPL);

		// interface中只包含直接继承的interface,不包括来自父类的
		// 此interface只在CastClass及Type.GetInterfaces()反射函数中
		// 发挥作用，不在callvir中发挥作用。
		// interfaceOffsets中包含了水平展开的所有interface(包括父类的)
		_interfaceDefines.resize(table.rowNum);
		uint32_t lastClassIdx = 0;
		for (uint32_t i = 0; i < table.rowNum; i++)
		{
			uint32_t rowIndex = i + 1;
			TbInterfaceImpl data = _rawImage.ReadInterfaceImpl(rowIndex);

			Il2CppTypeDefinition& typeDef = _typesDefines[data.classIdx - 1];
			Il2CppType intType = {};
			ReadTypeFromToken(GetGenericContainerByTypeDefinition(&typeDef), nullptr,
				DecodeTypeDefOrRefOrSpecCodedIndexTableType(data.interfaceIdx), DecodeTypeDefOrRefOrSpecCodedIndexRowIndex(data.interfaceIdx), intType);
			_interfaceDefines[i] = DecodeMetadataIndex(AddIl2CppTypeCache(intType));
			if (typeDef.interfaces_count == 0)
			{
				typeDef.interfacesStart = (InterfacesIndex)i;
			}
			else
			{
				// 必须连续
				IL2CPP_ASSERT(data.classIdx == lastClassIdx);
			}
			++typeDef.interfaces_count;
			lastClassIdx = data.classIdx;
		}
	}

	void InterpreterImage::ComputeVTable1(TypeDefinitionDetail* tdd)
	{
		Il2CppTypeDefinition& typeDef = *tdd->typeDef;
		if (IsInterface(typeDef.flags) || typeDef.vtableStart != 0)
		{
			return;
		}

		const Il2CppType* type = GetIl2CppTypeFromRawIndex(DecodeMetadataIndex(typeDef.byvalTypeIndex));

		int32_t vtableCount = 0;

		if (typeDef.parentIndex != kInvalidIndex)
		{
			const Il2CppType* parentType = il2cpp::vm::GlobalMetadata::GetIl2CppTypeFromIndex(typeDef.parentIndex);
			const Il2CppTypeDefinition* parentTypeDef = GetUnderlyingTypeDefinition(parentType);
			if (IsInterpreterType(parentTypeDef) && parentTypeDef->vtableStart == 0)
			{
				IL2CPP_ASSERT(DecodeImageIndex(parentTypeDef->byvalTypeIndex) == this->GetIndex());
				ComputeVTable1(&_typeDetails[DecodeMetadataIndex(parentTypeDef->byvalTypeIndex)]);
			}
			vtableCount += parentTypeDef->vtable_count;
		}

		for (uint32_t i = 0; i < typeDef.interfaces_count; i++)
		{
			const Il2CppType* intType = il2cpp::vm::GlobalMetadata::GetInterfaceFromOffset(&typeDef, i);
			const Il2CppTypeDefinition* intTypeDef = GetUnderlyingTypeDefinition(intType);
			vtableCount += intTypeDef->method_count;
		}

		for (uint32_t i = 0; i < typeDef.method_count; i++)
		{
			const Il2CppMethodDefinition* methodDef = il2cpp::vm::GlobalMetadata::GetMethodDefinitionFromIndex(typeDef.methodStart + i);
			if (huatuo::metadata::IsVirtualMethod(methodDef->flags))
			{
				++vtableCount;
			}
		}

		typeDef.vtableStart = EncodeWithIndex(0);
		// 计算出的vtableCount是一个保守上界,并非准确值.
		// 在ComputVTable2中会重新修正
		typeDef.vtable_count = vtableCount;
	}

	void InterpreterImage::InitVTables_1()
	{
		const Table& typeDefTb = _rawImage.GetTable(TableType::TYPEDEF);

		for (TypeDefinitionDetail& td : _typeDetails)
		{
			ComputeVTable1(&td);
		}
	}

	void InterpreterImage::ComputeVTable2(TypeDefinitionDetail* tdd)
	{
		Il2CppTypeDefinition& typeDef = *tdd->typeDef;
		if (IsInterface(typeDef.flags) || typeDef.interfaceOffsetsStart != 0)
		{
			return;
		}

		if (typeDef.parentIndex != kInvalidIndex)
		{
			const Il2CppType* parentType = il2cpp::vm::GlobalMetadata::GetIl2CppTypeFromIndex(typeDef.parentIndex);
			const Il2CppTypeDefinition* parentTypeDef = GetUnderlyingTypeDefinition(parentType);
			if (IsInterpreterType(parentTypeDef) && parentTypeDef->interfaceOffsetsStart == 0)
			{
				IL2CPP_ASSERT(DecodeImageIndex(parentTypeDef->byvalTypeIndex) == this->GetIndex());
				ComputeVTable2(&_typeDetails[DecodeMetadataIndex(parentTypeDef->byvalTypeIndex)]);
			}
		}

		const Il2CppType* type = GetIl2CppTypeFromRawIndex(DecodeMetadataIndex(typeDef.byvalTypeIndex));
		VTableSetUp* typeTree = VTableSetUp::BuildByType(_cacheTrees, type);

		uint32_t offsetsStart = (uint32_t)_interfaceOffsets.size();

		auto& vms = typeTree->GetVirtualMethodImpls();
		IL2CPP_ASSERT(tdd->vtable.empty());
		tdd->vtable = vms;

		auto& interfaceOffsetInfos = typeTree->GetInterfaceOffsetInfos();
		for (auto ioi : interfaceOffsetInfos)
		{
			_interfaceOffsets.push_back({ ioi.type, ioi.offset });
		}

		typeDef.vtable_count = (uint16_t)vms.size();
		typeDef.interfaceOffsetsStart = EncodeWithIndex(offsetsStart);
		typeDef.interface_offsets_count = (uint32_t)interfaceOffsetInfos.size();

		// klass may create by prev BuildTree
		Il2CppClass* klass = _classList[tdd->index];
		if (klass)
		{
			IL2CPP_ASSERT(klass->vtable_count >= typeDef.vtable_count);
			klass->vtable_count = typeDef.vtable_count;
			IL2CPP_ASSERT(klass->interface_offsets_count == 0);
			klass->interface_offsets_count = typeDef.interface_offsets_count;
		}
	}

	void InterpreterImage::InitVTables_2()
	{
		const Table& typeDefTb = _rawImage.GetTable(TableType::TYPEDEF);

		for (TypeDefinitionDetail& td : _typeDetails)
		{
			ComputeVTable2(&td);
		}

		for (auto& e : _cacheTrees)
		{
			e.second->~VTableSetUp();
			IL2CPP_FREE(e.second);
		}
		_cacheTrees.clear();
	}


	uint32_t InterpreterImage::GetFieldOffset(const Il2CppClass* klass, int32_t fieldIndexInType, FieldInfo* field)
	{
		Il2CppTypeDefinition* typeDef = (Il2CppTypeDefinition*)(klass->typeMetadataHandle);
		uint32_t fieldActualIndex = DecodeMetadataIndex(typeDef->fieldStart) + fieldIndexInType;
		IL2CPP_ASSERT(fieldActualIndex < (uint32_t)_fieldDetails.size());
		return _fieldDetails[fieldActualIndex].offset;
	}

	// index => MethodDefinition -> DeclaringClass -> index - klass->methodStart -> MethodInfo*
	const MethodInfo* InterpreterImage::GetMethodInfoFromMethodDefinitionRawIndex(uint32_t index)
	{
		IL2CPP_ASSERT((size_t)index <= _methodDefine2InfoCaches.size());
		if (_methodDefine2InfoCaches[index])
		{
			return _methodDefine2InfoCaches[index];
		}
		const Il2CppMethodDefinition* methodDefinition = GetMethodDefinitionFromRawIndex(index);
		const Il2CppType* type = il2cpp::vm::GlobalMetadata::GetIl2CppTypeFromIndex(methodDefinition->declaringType);
		//Il2CppClass* typeInfo = GetTypeInfoFromTypeDefinitionRawIndex(DecodeMetadataIndex());

		IL2CPP_ASSERT(type->type == IL2CPP_TYPE_VALUETYPE || type->type == IL2CPP_TYPE_CLASS);
		const Il2CppTypeDefinition* typeDefinition = reinterpret_cast<const Il2CppTypeDefinition*>(type->data.typeHandle);
		int32_t indexInClass = index - DecodeMetadataIndex(typeDefinition->methodStart);
		IL2CPP_ASSERT(indexInClass >= 0 && indexInClass < typeDefinition->method_count);
		Il2CppClass* klass = il2cpp::vm::Class::FromIl2CppType(type);
		il2cpp::vm::Class::SetupMethods(klass);
		// il2cpp::vm::Class::Init(klass);
		return _methodDefine2InfoCaches[index] = klass->methods[indexInClass];
	}

	const MethodInfo* InterpreterImage::GetMethodInfoFromMethodDefinition(const Il2CppMethodDefinition* methodDef)
	{
		uint32_t rawIndex = (uint32_t)(methodDef - &_methodDefines[0]);
		IL2CPP_ASSERT(rawIndex < (uint32_t)_methodDefines.size());
		return GetMethodInfoFromMethodDefinitionRawIndex(rawIndex);
	}

	// typeDef vTableSlot -> type virtual method index -> MethodDefinition*
	const Il2CppMethodDefinition* InterpreterImage::GetMethodDefinitionFromVTableSlot(const Il2CppTypeDefinition* typeDef, int32_t vTableSlot)
	{
		uint32_t typeDefIndex = GetTypeRawIndex(typeDef);
		IL2CPP_ASSERT(typeDefIndex < (uint32_t)_typeDetails.size());
		TypeDefinitionDetail& td = _typeDetails[typeDefIndex];

		IL2CPP_ASSERT(vTableSlot >= 0 && vTableSlot < (int32_t)td.vtable.size());
		VirtualMethodImpl& vmi = td.vtable[vTableSlot];
		return vmi.method;
	}

	const MethodInfo* InterpreterImage::GetMethodInfoFromVTableSlot(const Il2CppClass* klass, int32_t vTableSlot)
	{
		IL2CPP_ASSERT(!klass->generic_class);
		const Il2CppTypeDefinition* typeDef = (Il2CppTypeDefinition*)klass->typeMetadataHandle;
		//const Il2CppMethodDefinition* methodDef = GetMethodDefinitionFromVTableSlot((Il2CppTypeDefinition*)klass->typeMetadataHandle, vTableSlot);
		// FIX ME. why return null?
		//IL2CPP_ASSERT(methodDef);

		uint32_t typeDefIndex = GetTypeRawIndex(typeDef);
		IL2CPP_ASSERT(typeDefIndex < (uint32_t)_typeDetails.size());
		TypeDefinitionDetail& td = _typeDetails[typeDefIndex];

		IL2CPP_ASSERT(vTableSlot >= 0 && vTableSlot < (int32_t)td.vtable.size());
		VirtualMethodImpl& vmi = td.vtable[vTableSlot];
		if (vmi.method)
		{
			if (DecodeMetadataIndex(vmi.method->declaringType) == typeDefIndex)
			{
				return il2cpp::vm::GlobalMetadata::GetMethodInfoFromMethodHandle((Il2CppMetadataMethodDefinitionHandle)vmi.method);
			}
			else
			{
				Il2CppClass* implClass = il2cpp::vm::Class::FromIl2CppType(vmi.type);
				IL2CPP_ASSERT(implClass != klass);
				il2cpp::vm::Class::SetupMethods(implClass);
				for (uint32_t i = 0; i < implClass->method_count; i++)
				{
					const MethodInfo* implMethod = implClass->methods[i];
					if (implMethod->token == vmi.method->token)
					{
						return implMethod;
					}
				}
				RaiseHuatuoExecutionEngineException("not find vtable method");
			}
		}
		return nullptr;
	}

	Il2CppMethodPointer InterpreterImage::GetAdjustorThunk(uint32_t token)
	{
		uint32_t methodIndex = DecodeTokenRowIndex(token) - 1;
		IL2CPP_ASSERT(methodIndex < (uint32_t)_methodDefines.size());
		const Il2CppMethodDefinition* methodDef = &_methodDefines[methodIndex];
		return huatuo::interpreter::InterpreterModule::GetAdjustThunkMethodPointer(methodDef);
	}

	Il2CppMethodPointer InterpreterImage::GetMethodPointer(uint32_t token)
	{
		uint32_t methodIndex = DecodeTokenRowIndex(token) - 1;
		IL2CPP_ASSERT(methodIndex < (uint32_t)_methodDefines.size());
		const Il2CppMethodDefinition* methodDef = &_methodDefines[methodIndex];
		return huatuo::interpreter::InterpreterModule::GetMethodPointer(methodDef);
	}

	InvokerMethod InterpreterImage::GetMethodInvoker(uint32_t token)
	{
		uint32_t methodIndex = DecodeTokenRowIndex(token) - 1;
		IL2CPP_ASSERT(methodIndex < (uint32_t)_methodDefines.size());
		const Il2CppMethodDefinition* methodDef = &_methodDefines[methodIndex];
		return huatuo::interpreter::InterpreterModule::GetMethodInvoker(methodDef);
	}


	Il2CppString* InterpreterImage::ReadSerString(BlobReader& reader)
	{
		byte b = reader.PeekByte();
		if (b == 0xFF)
		{
			reader.SkipByte();
			return nullptr;
		}
		else if (b == 0)
		{
			reader.SkipByte();
			return il2cpp::vm::String::Empty();
		}
		else
		{
			uint32_t len = reader.ReadCompressedUint32();
			return il2cpp::vm::String::NewLen((char*)reader.GetAndSkipCurBytes(len), len);
		}
	}

	Il2CppReflectionType* InterpreterImage::ReadSystemType(BlobReader& reader)
	{
		Il2CppString* fullName = ReadSerString(reader);
		if (!fullName)
		{
			return nullptr;
		}
		Il2CppReflectionType* type = GetReflectionTypeFromName(fullName);
		if (!type)
		{
			std::string stdTypeName = il2cpp::utils::StringUtils::Utf16ToUtf8(fullName->chars);
			TEMP_FORMAT(errMsg, "CustomAttribute fixed arg type:System.Type fullName:'%s' not find", stdTypeName.c_str());
			il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetTypeLoadException(errMsg));
		}
		return type;
	}

	void InterpreterImage::ReadFixedArg(BlobReader& reader, const Il2CppType* argType, void* data)
	{
		switch (argType->type)
		{
		case IL2CPP_TYPE_BOOLEAN:
		{
			*(bool*)data = reader.Read<bool>();
			break;
		}
		case IL2CPP_TYPE_CHAR:
		{
			*(uint16_t*)data = reader.ReadUshort();
			break;
		}
		case IL2CPP_TYPE_I1:
		case IL2CPP_TYPE_U1:
		{
			*(byte*)data = reader.ReadByte();
			break;
		}
		case IL2CPP_TYPE_I2:
		case IL2CPP_TYPE_U2:
		{
			*(uint16_t*)data = reader.Read<uint16_t>();
			break;
		}
		case IL2CPP_TYPE_I4:
		case IL2CPP_TYPE_U4:
		{
			*(uint32_t*)data = reader.Read<uint32_t>();
			break;
		}
		case IL2CPP_TYPE_I8:
		case IL2CPP_TYPE_U8:
		{
			*(uint64_t*)data = reader.Read<uint64_t>();
			break;
		}
		case IL2CPP_TYPE_R4:
		{
			*(float*)data = reader.Read<float>();
			break;
		}
		case IL2CPP_TYPE_R8:
		{
			*(double*)data = reader.Read<double>();
			break;
		}
		case IL2CPP_TYPE_SZARRAY:
		{
			uint32_t numElem = reader.Read<uint32_t>();
			if (numElem != (uint32_t)-1)
			{
				Il2CppClass* arrKlass = il2cpp::vm::Class::FromIl2CppType(argType);
				Il2CppArray* arr = il2cpp::vm::Array::New(il2cpp::vm::Class::GetElementClass(arrKlass), numElem);
				for (uint16_t i = 0; i < numElem; i++)
				{
					ReadFixedArg(reader, argType->data.type, GET_ARRAY_ELEMENT_ADDRESS(arr, i, arr->klass->element_size));
				}
				*(void**)data = arr;
			}
			else
			{
				*(void**)data = nullptr;
			}
			break;
		}
		case IL2CPP_TYPE_STRING:
		{
			*(Il2CppString**)data = ReadSerString(reader);
			// FIXME memory barrier
			break;
		}
		case IL2CPP_TYPE_OBJECT:
		{
			uint64_t obj = 0;
			Il2CppType kind = {};
			kind.type = (Il2CppTypeEnum)reader.ReadByte();
			Il2CppClass* valueType = il2cpp::vm::Class::FromIl2CppType(&kind);
			ReadFixedArg(reader, &kind, &obj);
			*(Il2CppObject**)data = il2cpp::vm::Object::Box(valueType, &obj);
			// FIXME memory barrier
			break;
		}
		case IL2CPP_TYPE_CLASS:
		{
			Il2CppClass* klass = il2cpp::vm::Class::FromIl2CppType(argType);
			if (!klass)
			{
				RaiseHuatuoExecutionEngineException("type not find");
			}
			if (klass == il2cpp_defaults.object_class)
			{
				Il2CppType boxedValueType = {};
				boxedValueType.type = (Il2CppTypeEnum)reader.ReadByte();
				Il2CppClass* valKlass = il2cpp::vm::Class::FromIl2CppType(&boxedValueType);
				IL2CPP_ASSERT(valKlass);
				uint64_t val = 0;
				ReadFixedArg(reader, &boxedValueType, &val);
				*(Il2CppObject**)data = il2cpp::vm::Object::Box(valKlass, &val);
			}
			else if (klass == il2cpp_defaults.systemtype_class)
			{
				*(Il2CppReflectionType**)data = ReadSystemType(reader);
			}
			else
			{
				TEMP_FORMAT(errMsg, "fixed arg type:%s.%s not support", klass->namespaze, klass->name);
				RaiseHuatuoNotSupportedException(errMsg);
			}
			break;
		}
		case IL2CPP_TYPE_VALUETYPE:
		{
			Il2CppClass* valueType = il2cpp::vm::Class::FromIl2CppType(argType);
			IL2CPP_ASSERT(valueType->enumtype);
			ReadFixedArg(reader, &valueType->element_class->byval_arg, data);
			break;
		}
		case IL2CPP_TYPE_SYSTEM_TYPE:
		{
			*(Il2CppReflectionType**)data = ReadSystemType(reader);
			break;
		}
		case IL2CPP_TYPE_BOXED_OBJECT:
		{
			uint8_t fieldOrPropType = reader.ReadByte();
			IL2CPP_ASSERT(fieldOrPropType == 0x51);
			Il2CppType boxedValueType = {};
			boxedValueType.type = (Il2CppTypeEnum)reader.ReadByte();
			Il2CppClass* valKlass = il2cpp::vm::Class::FromIl2CppType(&boxedValueType);
			IL2CPP_ASSERT(valKlass);
			uint64_t val = 0;
			ReadFixedArg(reader, &boxedValueType, &val);
			*(Il2CppObject**)data = il2cpp::vm::Object::Box(valKlass, &val);
			break;
		}
		case IL2CPP_TYPE_ENUM:
		{
			Il2CppClass* valueType = il2cpp::vm::Class::FromIl2CppType(argType);
			IL2CPP_ASSERT(valueType->enumtype);
			ReadFixedArg(reader, &valueType->element_class->byval_arg, data);
			break;
		}
		default:
		{
			RaiseHuatuoExecutionEngineException("not support fixed argument type");
		}
		}
	}

	void InterpreterImage::ReadCustomAttributeFieldOrPropType(BlobReader& reader, Il2CppType& type)
	{
		type.type = (Il2CppTypeEnum)reader.ReadByte();

		switch (type.type)
		{
		case IL2CPP_TYPE_BOOLEAN:
		case IL2CPP_TYPE_CHAR:
		case IL2CPP_TYPE_I1:
		case IL2CPP_TYPE_U1:
		case IL2CPP_TYPE_I2:
		case IL2CPP_TYPE_U2:
		case IL2CPP_TYPE_I4:
		case IL2CPP_TYPE_U4:
		case IL2CPP_TYPE_I8:
		case IL2CPP_TYPE_U8:
		case IL2CPP_TYPE_R4:
		case IL2CPP_TYPE_R8:
		case IL2CPP_TYPE_STRING:
		{
			break;
		}
		case IL2CPP_TYPE_SZARRAY:
		{
			// FIXME MEMORY LEAK
			Il2CppType* eleType = (Il2CppType*)IL2CPP_MALLOC_ZERO(sizeof(Il2CppType));
			ReadCustomAttributeFieldOrPropType(reader, *eleType);
			type.data.type = eleType;
			break;
		}
		case IL2CPP_TYPE_ENUM:
		{
			Il2CppString* enumTypeName = ReadSerString(reader);

			Il2CppReflectionType* enumType = GetReflectionTypeFromName(enumTypeName);
			if (!enumType)
			{
				std::string stdStrName = il2cpp::utils::StringUtils::Utf16ToUtf8(enumTypeName->chars);
				TEMP_FORMAT(errMsg, "ReadCustomAttributeFieldOrPropType enum:'%s' not exists", stdStrName.c_str());
				RaiseHuatuoExecutionEngineException(errMsg);
			}
			type = *enumType->type;
			break;
		}
		case IL2CPP_TYPE_SYSTEM_TYPE:
		{
			type = il2cpp_defaults.systemtype_class->byval_arg;
			break;
		}
		case IL2CPP_TYPE_BOXED_OBJECT:
		{
			type = il2cpp_defaults.object_class->byval_arg;
			break;
		}
		default:
		{
			TEMP_FORMAT(errMsg, "ReadCustomAttributeFieldOrPropType. image:%s unknown type:%d", GetIl2CppImage()->name, (int)type.type);
			RaiseBadImageException(errMsg);
		}
		}
	}

	void InterpreterImage::ConstructCustomAttribute(BlobReader& reader, Il2CppObject* obj, const MethodInfo* ctorMethod)
	{
		uint16_t prolog = reader.ReadUshort();
		IL2CPP_ASSERT(prolog == 0x0001);
		if (ctorMethod->parameters_count == 0)
		{
			il2cpp::vm::Runtime::Invoke(ctorMethod, obj, nullptr, nullptr);
		}
		else
		{
			int32_t argSize = sizeof(uint64_t) * ctorMethod->parameters_count;
			uint64_t* argDatas = (uint64_t*)alloca(argSize);
			std::memset(argDatas, 0, argSize);
			void** argPtrs = (void**)alloca(sizeof(void*) * ctorMethod->parameters_count); // same with argDatas
			for (uint8_t i = 0; i < ctorMethod->parameters_count; i++)
			{
				argPtrs[i] = argDatas + i;
				const Il2CppType* paramType = GET_METHOD_PARAMETER_TYPE(ctorMethod->parameters[i]);
				ReadFixedArg(reader, paramType, argDatas + i);
				Il2CppClass* paramKlass = il2cpp::vm::Class::FromIl2CppType(paramType);
				if (!IS_CLASS_VALUE_TYPE(paramKlass))
				{
					argPtrs[i] = (void*)argDatas[i];
				}
			}
			il2cpp::vm::Runtime::Invoke(ctorMethod, obj, argPtrs, nullptr);
			// clear ref. may not need. gc memory barrier
			std::memset(argDatas, 0, argSize);
		}
		uint16_t numNamed = reader.ReadUshort();
		Il2CppClass* klass = obj->klass;
		for (uint16_t idx = 0; idx < numNamed; idx++)
		{
			byte fieldOrPropTypeTag = reader.ReadByte();
			IL2CPP_ASSERT(fieldOrPropTypeTag == 0x53 || fieldOrPropTypeTag == 0x54);
			Il2CppType fieldOrPropType = {};
			ReadCustomAttributeFieldOrPropType(reader, fieldOrPropType);
			Il2CppString* fieldOrPropName = ReadSerString(reader);
			std::string stdStrName = il2cpp::utils::StringUtils::Utf16ToUtf8(fieldOrPropName->chars);
			const char* cstrName = stdStrName.c_str();
			uint64_t value = 0;
			ReadFixedArg(reader, &fieldOrPropType, &value);
			if (fieldOrPropTypeTag == 0x53)
			{
				FieldInfo* field = il2cpp::vm::Class::GetFieldFromName(klass, cstrName);
				if (!field)
				{
					TEMP_FORMAT(errMsg, "CustomAttribute field missing. klass:%s.%s field:%s", klass->namespaze, klass->name, cstrName);
					il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetTypeInitializationException(errMsg, nullptr));
				}
				Il2CppReflectionField* refField = il2cpp::vm::Reflection::GetFieldObject(klass, field);
				IL2CPP_ASSERT(IsTypeEqual(&fieldOrPropType, field->type));
				uint32_t fieldSize = GetTypeValueSize(&fieldOrPropType);
				std::memcpy((byte*)obj + field->offset, &value, fieldSize);
				//fixme MEMORY BARRIER
				IL2CPP_ASSERT(refField);
			}
			else
			{
				const PropertyInfo* prop = il2cpp::vm::Class::GetPropertyFromName(klass, cstrName);
				if (!prop)
				{
					TEMP_FORMAT(errMsg, "CustomAttribute property missing. klass:%s property:%s", klass->name, cstrName);
					il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetTypeInitializationException(errMsg, nullptr));
				}
				IL2CPP_ASSERT(IsTypeEqual(&fieldOrPropType, GET_METHOD_PARAMETER_TYPE(prop->set->parameters[0])));
				Il2CppException* ex = nullptr;
				Il2CppClass* propKlass = il2cpp::vm::Class::FromIl2CppType(&fieldOrPropType);
				IL2CPP_ASSERT(propKlass);
				void* args[] = { (IS_CLASS_VALUE_TYPE(propKlass) ? &value : (void*)value) };
				il2cpp::vm::Runtime::Invoke(prop->set, obj, args, &ex);
				if (ex)
				{
					il2cpp::vm::Exception::Raise(ex);
				}
			}
		}
	}

	void InterpreterImage::ReadMethodDefSig(BlobReader& reader, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer, Il2CppMethodDefinition& methodDef, std::vector<ParamDetail>& paramArr)
	{
		uint8_t rawSigFlags = reader.ReadByte();

		if (rawSigFlags & (uint8_t)MethodSigFlags::GENERIC)
		{
			//IL2CPP_ASSERT(false);
			uint32_t genParamCount = reader.ReadCompressedUint32();
			Il2CppGenericContainer* gc = GetGenericContainerByRawIndex(DecodeMetadataIndex(methodDef.genericContainerIndex));
			IL2CPP_ASSERT(gc->type_argc == genParamCount);
		}
		uint32_t paramCount = reader.ReadCompressedUint32();
		//IL2CPP_ASSERT(paramCount >= methodDef.parameterCount);

		Il2CppType returnType = {};
		ReadType(reader, klassGenericContainer, methodGenericContainer, returnType);
		methodDef.returnType = AddIl2CppTypeCache(returnType);

		int readParamNum = 0;
		for (; reader.NonEmpty(); )
		{
			ParamDetail curParam = {};
			Il2CppType paramType = {};
			ReadType(reader, klassGenericContainer, methodGenericContainer, paramType);
			curParam.parameterIndex = readParamNum++;
			curParam.methodDef = &methodDef;
			curParam.paramDef.typeIndex = AddIl2CppTypeCache(paramType);
			paramArr.push_back(curParam);
		}
		IL2CPP_ASSERT(readParamNum == (int)paramCount);
	}
}
}