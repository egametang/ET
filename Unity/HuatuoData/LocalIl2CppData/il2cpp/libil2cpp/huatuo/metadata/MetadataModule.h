#pragma once

#include "InterpreterImage.h"
#include "AOTHomologousImage.h"
#include "Assembly.h"

namespace huatuo
{

namespace metadata
{

	class MetadataModule
	{
	public:

		static void Initialize();

		static InterpreterImage* GetImage(uint32_t imageIndex)
		{
			return InterpreterImage::GetImage(imageIndex);
		}

		static InterpreterImage* GetImage(const Il2CppImage* image)
		{
			return GetImage(DecodeImageIndex(image->token));
		}

		static InterpreterImage* GetImage(const Il2CppClass* klass)
		{
			return GetImage(klass->image);
		}

		static InterpreterImage* GetImage(const Il2CppTypeDefinition* typeDef)
		{
			return GetImage(DecodeImageIndex(typeDef->byvalTypeIndex));
		}

		static InterpreterImage* GetImage(const Il2CppMethodDefinition* typeDef)
		{
			return GetImage(DecodeImageIndex(typeDef->nameIndex));
		}

		static InterpreterImage* GetImageByEncodedIndex(uint32_t encodedIndex)
		{
			return GetImage(DecodeImageIndex(encodedIndex));
		}

		static const char* GetStringFromEncodeIndex(StringIndex index)
		{
			uint32_t imageIndex = DecodeImageIndex(index);
			return GetImage(imageIndex)->GetStringFromRawIndex(DecodeMetadataIndex(index));
		}

		static uint32_t GetTypeEncodeIndex(const Il2CppTypeDefinition* typeDef)
		{
			InterpreterImage* image = GetImage(typeDef);
			return huatuo::metadata::EncodeImageAndMetadataIndex(image->GetIndex(), image->GetTypeRawIndex(typeDef));
		}

		static Il2CppMetadataTypeHandle GetAssemblyTypeHandleFromRawIndex(const Il2CppImage* image, AssemblyTypeIndex index)
		{
			return GetImage(image)->GetAssemblyTypeHandleFromRawIndex(index);
		}

		static Il2CppMetadataTypeHandle GetAssemblyTypeHandleFromEncodeIndex(AssemblyTypeIndex index)
		{
			uint32_t imageIndex = DecodeImageIndex(index);
			return GetImage(imageIndex)->GetAssemblyTypeHandleFromRawIndex(DecodeMetadataIndex(index));
		}

		static Il2CppMetadataTypeHandle GetAssemblyExportedTypeHandleFromEncodeIndex(AssemblyTypeIndex index)
		{
			uint32_t imageIndex = DecodeImageIndex(index);
			return GetImage(imageIndex)->GetAssemblyExportedTypeHandleFromRawIndex(DecodeMetadataIndex(index));
		}

		static const Il2CppTypeDefinitionSizes* GetTypeDefinitionSizesFromEncodeIndex(TypeDefinitionIndex index)
		{
			uint32_t imageIndex = DecodeImageIndex(index);
			return GetImage(imageIndex)->GetTypeDefinitionSizesFromRawIndex(DecodeMetadataIndex(index));
		}

		static const Il2CppType* GetIl2CppTypeFromEncodeIndex(uint32_t index)
		{
			uint32_t imageIndex = DecodeImageIndex(index);
			IL2CPP_ASSERT(imageIndex > 0);

			uint32_t rawIndex = DecodeMetadataIndex(index);
			return GetImage(imageIndex)->GetIl2CppTypeFromRawIndex(rawIndex);
		}

		static Il2CppClass* GetTypeInfoFromTypeDefinitionEncodeIndex(TypeDefinitionIndex index)
		{
			uint32_t imageIndex = DecodeImageIndex(index);
			IL2CPP_ASSERT(imageIndex > 0);

			uint32_t rawIndex = DecodeMetadataIndex(index);
			return GetImage(imageIndex)->GetTypeInfoFromTypeDefinitionRawIndex(rawIndex);
		}

		static const Il2CppFieldDefinition* GetFieldDefinitionFromEncodeIndex(uint32_t index)
		{
			uint32_t imageIndex = DecodeImageIndex(index);
			return GetImage(imageIndex)->GetFieldDefinitionFromRawIndex(DecodeMetadataIndex(index));
		}

		static const Il2CppMethodDefinition* GetMethodDefinitionFromIndex(MethodIndex index)
		{
			uint32_t imageIndex = DecodeImageIndex(index);
			return GetImage(imageIndex)->GetMethodDefinitionFromRawIndex(DecodeMetadataIndex(index));
		}

		static uint32_t GetFieldOffset(const Il2CppClass* klass, int32_t fieldIndexInType, FieldInfo* field)
		{
			return GetImage(klass)->GetFieldOffset(klass, fieldIndexInType, field);
		}

		static const MethodInfo* GetMethodInfoFromMethodDefinitionIndex(uint32_t index)
		{
			uint32_t imageIndex = DecodeImageIndex(index);
			return GetImage(imageIndex)->GetMethodInfoFromMethodDefinitionRawIndex(DecodeMetadataIndex(index));
		}

		static const MethodInfo* GetMethodInfoFromMethodDefinition(const Il2CppMethodDefinition* methodDef)
		{
			uint32_t imageIndex = DecodeImageIndex(methodDef->nameIndex);
			return GetImage(imageIndex)->GetMethodInfoFromMethodDefinition(methodDef);
		}

		static const MethodInfo* GetMethodInfoFromVTableSlot(const Il2CppClass* klass, int32_t vTableSlot)
		{
			return GetImage(klass)->GetMethodInfoFromVTableSlot(klass, vTableSlot);
		}

		static const Il2CppMethodDefinition* GetMethodDefinitionFromVTableSlot(const Il2CppTypeDefinition* typeDefine, int32_t vTableSlot)
		{
			return GetImage(typeDefine)->GetMethodDefinitionFromVTableSlot(typeDefine, vTableSlot);
		}

		static Il2CppMethodPointer GetAdjustorThunk(const Il2CppImage* image, uint32_t token)
		{
			uint32_t imageIndex = DecodeImageIndex(image->token);
			return GetImage(imageIndex)->GetAdjustorThunk(token);
		}

		static Il2CppMethodPointer GetMethodPointer(const Il2CppImage* image, uint32_t token)
		{
			uint32_t imageIndex = DecodeImageIndex(image->token);
			return GetImage(imageIndex)->GetMethodPointer(token);
		}

		static InvokerMethod GetMethodInvoker(const Il2CppImage* image, uint32_t token)
		{
			uint32_t imageIndex = DecodeImageIndex(image->token);
			return GetImage(imageIndex)->GetMethodInvoker(token);
		}

		static const Il2CppParameterDefinition* GetParameterDefinitionFromIndex(const Il2CppImage* image, ParameterIndex index)
		{
			uint32_t imageIndex = DecodeImageIndex(image->token);
			return GetImage(imageIndex)->GetParameterDefinitionFromIndex(index);
		}

		static const Il2CppType* GetInterfaceFromOffset(const Il2CppClass* klass, TypeInterfaceIndex offset)
		{
			return GetImage(klass)->GetInterfaceFromOffset(klass, offset);
		}

		static const Il2CppType* GetInterfaceFromOffset(const Il2CppTypeDefinition* typeDefine, TypeInterfaceIndex offset)
		{
			return GetImage(typeDefine)->GetInterfaceFromOffset(typeDefine, offset);
		}

		static Il2CppInterfaceOffsetInfo GetInterfaceOffsetInfo(const Il2CppTypeDefinition* typeDefine, TypeInterfaceOffsetIndex index)
		{
			return GetImage(typeDefine)->GetInterfaceOffsetInfo(typeDefine, index);
		}

		static Il2CppClass* GetNestedTypeFromOffset(const Il2CppClass* klass, TypeNestedTypeIndex offset)
		{
			return GetImage(klass)->GetNestedTypeFromOffset(klass, offset);
		}

		static Il2CppMetadataTypeHandle GetNestedTypes(Il2CppMetadataTypeHandle handle, void** iter)
		{
			Il2CppTypeDefinition* typeDef = (Il2CppTypeDefinition*)handle;
			return (Il2CppMetadataTypeHandle)(GetImage(typeDef)->GetNestedTypes(typeDef, iter));
		}

		static const Il2CppGenericContainer* GetGenericContainerFromEncodeIndex(uint32_t index)
		{
			return GetImage(DecodeImageIndex(index))->GetGenericContainerByRawIndex(DecodeMetadataIndex(index));

		}

		static const Il2CppFieldDefaultValue* GetFieldDefaultValueEntry(uint32_t index)
		{
			return GetImage(DecodeImageIndex(index))->GetFieldDefaultValueEntryByRawIndex(DecodeMetadataIndex(index));
		}

		static const uint8_t* GetFieldOrParameterDefalutValue(uint32_t index)
		{
			return GetImage(DecodeImageIndex(index))->GetFieldOrParameterDefalutValueByRawIndex(DecodeMetadataIndex(index));
		}

		static bool HasAttribute(const Il2CppImage* image, uint32_t token, Il2CppClass* attribute)
		{
			return GetImage(image)->HasAttribute(token, attribute);
		}

		static std::tuple<void*, void*> GetCustomAttributeDataRange(const Il2CppImage* image, uint32_t token)
		{
			return GetImage(image)->GetCustomAttributeDataRange(token);
		}

		static bool IsImplementedByInterpreter(MethodInfo* method)
		{
			Il2CppClass* klass = method->klass;
			Il2CppClass* parent = klass->parent;
			return parent != il2cpp_defaults.multicastdelegate_class && parent != il2cpp_defaults.delegate_class && AOTHomologousImage::FindImageByAssembly(klass->image->assembly);
		}
	private:
	};
}

}