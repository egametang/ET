#include "MetadataUtil.h"

#include "vm/GlobalMetadata.h"
#include "vm/GlobalMetadataFileInternals.h"
#include "vm/Type.h"
#include "vm/GlobalMetadata.h"
#include "vm/Class.h"
#include "metadata/Il2CppTypeCompare.h"
#include "metadata/GenericMetadata.h"

#include "Image.h"

namespace huatuo
{
namespace metadata
{
	uint32_t GetNotZeroBitCount(uint64_t x)
	{
		uint32_t count = 0;
		for (int i = 0; i < 64; i++)
		{
			if (x & ((uint64_t)1 << i))
			{
				++count;
			}
		}
		return count;
	}

	int32_t GetTypeValueSize(const Il2CppType* type)
	{
		if (type->byref)
		{
			return 8;
		}
		switch (type->type)
		{
		case IL2CPP_TYPE_BOOLEAN:
		case IL2CPP_TYPE_I1:
		case IL2CPP_TYPE_U1:
			return 1;
		case IL2CPP_TYPE_CHAR:
		case IL2CPP_TYPE_I2:
		case IL2CPP_TYPE_U2:
			return 2;
		case IL2CPP_TYPE_I4:
		case IL2CPP_TYPE_U4:
		case IL2CPP_TYPE_R4:
			return 4;

		case IL2CPP_TYPE_I8:
		case IL2CPP_TYPE_U8:
		case IL2CPP_TYPE_R8:
		case IL2CPP_TYPE_I:
		case IL2CPP_TYPE_U:
		case IL2CPP_TYPE_FNPTR:
		case IL2CPP_TYPE_PTR:
		case IL2CPP_TYPE_BYREF:
		case IL2CPP_TYPE_STRING:
		case IL2CPP_TYPE_ARRAY:
		case IL2CPP_TYPE_SZARRAY:
		case IL2CPP_TYPE_OBJECT:
			return 8;
		case IL2CPP_TYPE_TYPEDBYREF:
			return sizeof(Il2CppTypedRef);
		case IL2CPP_TYPE_CLASS:
		{
			IL2CPP_ASSERT(!IS_CLASS_VALUE_TYPE(il2cpp::vm::Class::FromIl2CppType(type)));
			return 8;
		}
		case IL2CPP_TYPE_VALUETYPE:
		{
			Il2CppClass* klass = il2cpp::vm::Class::FromIl2CppType(type);
			IL2CPP_ASSERT(IS_CLASS_VALUE_TYPE(klass));
			return il2cpp::vm::Class::GetValueSize(klass, nullptr);
		}
		case IL2CPP_TYPE_GENERICINST:
		{
			Il2CppGenericClass* genericClass = type->data.generic_class;
			if (genericClass->type->type == IL2CPP_TYPE_CLASS)
			{
				IL2CPP_ASSERT(!IS_CLASS_VALUE_TYPE(il2cpp::vm::Class::FromIl2CppType(type)));
				return 8;
			}
			else
			{
				Il2CppClass* klass = il2cpp::vm::Class::FromIl2CppType(type);
				IL2CPP_ASSERT(IS_CLASS_VALUE_TYPE(klass));
				return il2cpp::vm::Class::GetValueSize(klass, nullptr);
			}
		}
		default:
		{
			TEMP_FORMAT(errMsg, "GetTypeValueSize unknown type:%d", (int)type->type);
			RaiseHuatuoExecutionEngineException(errMsg);
			return -1;
		}
		}
	}

	bool IsValueType(const Il2CppType* type)
	{
		switch (type->type)
		{
		case IL2CPP_TYPE_BOOLEAN:
		case IL2CPP_TYPE_I1:
		case IL2CPP_TYPE_U1:
		case IL2CPP_TYPE_CHAR:
		case IL2CPP_TYPE_I2:
		case IL2CPP_TYPE_U2:
		case IL2CPP_TYPE_I4:
		case IL2CPP_TYPE_U4:
		case IL2CPP_TYPE_R4:
		case IL2CPP_TYPE_I8:
		case IL2CPP_TYPE_U8:
		case IL2CPP_TYPE_R8:
		case IL2CPP_TYPE_I:
		case IL2CPP_TYPE_U:
		case IL2CPP_TYPE_TYPEDBYREF:
		case IL2CPP_TYPE_VALUETYPE: return true;
		case IL2CPP_TYPE_GENERICINST: return type->data.generic_class->type->type == IL2CPP_TYPE_VALUETYPE;
		default: return false;
		}
	}

	bool IsTypeSameByTypeIndex(TypeIndex t1, TypeIndex t2)
	{
		const Il2CppType* srcParamType = il2cpp::vm::GlobalMetadata::GetIl2CppTypeFromIndex(t1);
		IL2CPP_ASSERT(srcParamType);
		const Il2CppType* dstParamType = il2cpp::vm::GlobalMetadata::GetIl2CppTypeFromIndex(t2);
		IL2CPP_ASSERT(dstParamType);
		return il2cpp::metadata::Il2CppTypeEqualityComparer::AreEqual(srcParamType, dstParamType);
	}

	bool IsTypeEqual(const Il2CppType* t1, const Il2CppType* t2)
	{
		return il2cpp::metadata::Il2CppTypeEqualityComparer::AreEqual(t1, t2);
	}

	bool IsTypeGenericCompatible(const Il2CppType* typeTo, const Il2CppType* typeFrom)
	{
		if (typeTo->type != typeFrom->type)
		{
			return false;
		}
		switch (typeTo->type)
		{
		case IL2CPP_TYPE_VALUETYPE:
		{
			Il2CppClass* klass1 = il2cpp::vm::Class::FromIl2CppType(typeTo);
			Il2CppClass* klass2 = il2cpp::vm::Class::FromIl2CppType(typeFrom);
			return klass1->instance_size == klass2->instance_size;
		}
		case IL2CPP_TYPE_CLASS:
		{
			Il2CppClass* klass1 = il2cpp::vm::Class::FromIl2CppType(typeTo);
			Il2CppClass* klass2 = il2cpp::vm::Class::FromIl2CppType(typeFrom);
			return il2cpp::vm::Class::IsAssignableFrom(klass1, klass2);
		}
		case IL2CPP_TYPE_GENERICINST:
		{
			Il2CppClass* klass1 = il2cpp::vm::Class::FromIl2CppType(typeTo);
			Il2CppClass* klass2 = il2cpp::vm::Class::FromIl2CppType(typeFrom);
			if (IS_CLASS_VALUE_TYPE(klass1) != IS_CLASS_VALUE_TYPE(klass2))
			{
				return false;
			}
			if (IS_CLASS_VALUE_TYPE(klass1))
			{
				return klass1->instance_size == klass2->instance_size;
			}
			else
			{
				return il2cpp::vm::Class::IsAssignableFrom(klass1, klass2);
			}
			break;
		}
		default: return true;
		}
		return true;
	}

	bool IsExactlyMatch(const Il2CppMethodDefinition* src, const Il2CppMethodDefinition* dst)
	{
		if (std::strcmp(il2cpp::vm::GlobalMetadata::GetStringFromIndex(src->nameIndex), il2cpp::vm::GlobalMetadata::GetStringFromIndex(dst->nameIndex)))
		{
			return false;
		}
		if (src->parameterCount != dst->parameterCount)
		{
			return false;
		}

		if (!IsTypeSameByTypeIndex(src->returnType, dst->returnType))
		{
			return false;
		}

		for (uint32_t i = 0; i < src->parameterCount; i++)
		{
			const Il2CppParameterDefinition* srcParam = (const Il2CppParameterDefinition*)il2cpp::vm::GlobalMetadata::GetParameterDefinitionFromIndex(src, src->parameterStart + i);
			IL2CPP_ASSERT(srcParam);
			const Il2CppParameterDefinition* dstParam = (const Il2CppParameterDefinition*)il2cpp::vm::GlobalMetadata::GetParameterDefinitionFromIndex(dst, dst->parameterStart + i);
			IL2CPP_ASSERT(dstParam);

			if (!IsTypeSameByTypeIndex(srcParam->typeIndex, dstParam->typeIndex))
			{
				return false;
			}
		}
		return true;
	}

	const Il2CppType* TryInflateIfNeed(const Il2CppType* selfType, const Il2CppGenericContext* genericContext, bool inflateMethodVars)
	{
		// FIXME mEMORY LEAK
		return genericContext ? il2cpp::metadata::GenericMetadata::InflateIfNeeded(selfType, genericContext, inflateMethodVars) : selfType;
	}

	const Il2CppType* TryInflateIfNeed(const Il2CppType* containerType, const Il2CppType* selfType)
	{
		if (IsGenericIns(containerType) /* && IsGenericIns(selfType)*/)
		{
			// TOTO memory leak
			return il2cpp::metadata::GenericMetadata::InflateIfNeeded(selfType, &containerType->data.generic_class->context, true);
		}
		else
		{
			return selfType;
		}
	}

	bool IsSameOverrideType(const Il2CppType* t1, const Il2CppType* t2)
	{
		if (t1->type != t2->type)
		{
			return false;
		}

		if (t1->byref != t2->byref)
		{
			return false;
		}

		switch (t1->type)
		{
		case IL2CPP_TYPE_VALUETYPE:
		case IL2CPP_TYPE_CLASS:
			return t1->data.typeHandle == t2->data.typeHandle;
		case IL2CPP_TYPE_PTR:
		case IL2CPP_TYPE_SZARRAY:
			return IsSameOverrideType(t1->data.type, t2->data.type);

		case IL2CPP_TYPE_ARRAY:
		{
			if (t1->data.array->rank < t2->data.array->rank)
			{
				return false;
			}
			return IsSameOverrideType(t1->data.array->etype, t2->data.array->etype);
		}
		case IL2CPP_TYPE_GENERICINST:
		{
			const Il2CppGenericInst* i1 = t1->data.generic_class->context.class_inst;
			const Il2CppGenericInst* i2 = t2->data.generic_class->context.class_inst;

			// this happens when maximum generic recursion is hit
			if (i1 == NULL || i2 == NULL)
			{
				return i1 == i2;
			}

			if (i1->type_argc != i2->type_argc)
				return false;

			if (!IsSameOverrideType(t1->data.generic_class->type, t2->data.generic_class->type))
				return false;

			/* FIXME: we should probably just compare the instance pointers directly.  */
			for (uint32_t i = 0; i < i1->type_argc; ++i)
			{
				if (!IsSameOverrideType(i1->type_argv[i], i2->type_argv[i]))
				{
					return false;
				}
			}

			return true;
		}
		case IL2CPP_TYPE_VAR:
		{
			return t1->data.genericParameterHandle == t2->data.genericParameterHandle;
		}
		case IL2CPP_TYPE_MVAR:
		{
			const Il2CppGenericParameter* gp1 = (const Il2CppGenericParameter*)t1->data.genericParameterHandle;
			const Il2CppGenericParameter* gp2 = (const Il2CppGenericParameter*)t2->data.genericParameterHandle;
			return gp1->num == gp2->num;
		}
		default:
			return true;
		}
		IL2CPP_ASSERT(false);
		return false;
	}

	bool IsOverrideMethodIgnoreName(const Il2CppType* type1, const Il2CppMethodDefinition* methodDef1, const Il2CppType* type2, const Il2CppMethodDefinition* methodDef2)
	{
		if (methodDef1->parameterCount != methodDef2->parameterCount)
		{
			return false;
		}

		const Il2CppType* returnType1 = TryInflateIfNeed(type1, il2cpp::vm::GlobalMetadata::GetIl2CppTypeFromIndex(methodDef1->returnType));
		const Il2CppType* returnType2 = TryInflateIfNeed(type2, il2cpp::vm::GlobalMetadata::GetIl2CppTypeFromIndex(methodDef2->returnType));


		if (!IsSameOverrideType(returnType1, returnType2))
		{
			return false;
		}

		for (uint32_t i = 0; i < methodDef1->parameterCount; i++)
		{
			const Il2CppParameterDefinition* srcParam = (const Il2CppParameterDefinition*)il2cpp::vm::GlobalMetadata::GetParameterDefinitionFromIndex(methodDef1, methodDef1->parameterStart + i);
			IL2CPP_ASSERT(srcParam);
			const Il2CppParameterDefinition* dstParam = (const Il2CppParameterDefinition*)il2cpp::vm::GlobalMetadata::GetParameterDefinitionFromIndex(methodDef2, methodDef2->parameterStart + i);
			IL2CPP_ASSERT(dstParam);

			const Il2CppType* paramType1 = TryInflateIfNeed(type1, il2cpp::vm::GlobalMetadata::GetIl2CppTypeFromIndex(srcParam->typeIndex));
			const Il2CppType* paramType2 = TryInflateIfNeed(type2, il2cpp::vm::GlobalMetadata::GetIl2CppTypeFromIndex(dstParam->typeIndex));

			if (!IsSameOverrideType(paramType1, paramType2))
			{
				return false;
			}
		}
		return true;
	}

	bool IsOverrideMethod(const Il2CppType* type1, const Il2CppMethodDefinition* methodDef1, const Il2CppType* type2, const Il2CppMethodDefinition* methodDef2)
	{
		const char* name1 = il2cpp::vm::GlobalMetadata::GetStringFromIndex(methodDef1->nameIndex);
		const char* name2 = il2cpp::vm::GlobalMetadata::GetStringFromIndex(methodDef2->nameIndex);
		if (std::strcmp(name1, name2))
		{
			return false;
		}
		return IsOverrideMethodIgnoreName(type1, methodDef1, type2, methodDef2);
	}

	bool IsMatchSigType(const Il2CppType* dstType, const Il2CppType* sigType, const Il2CppGenericContainer* klassGenericContainer, const Il2CppGenericContainer* methodGenericContainer)
	{
		if (dstType->type != sigType->type)
		{
			return false;
		}

		if (dstType->byref != sigType->byref)
		{
			return false;
		}

		switch (dstType->type)
		{
		case IL2CPP_TYPE_VALUETYPE:
		case IL2CPP_TYPE_CLASS:
			return dstType->data.typeHandle == sigType->data.typeHandle;
		case IL2CPP_TYPE_PTR:
		case IL2CPP_TYPE_SZARRAY:
			return IsMatchSigType(dstType->data.type, sigType->data.type, klassGenericContainer, methodGenericContainer);

		case IL2CPP_TYPE_ARRAY:
		{
			if (dstType->data.array->rank < sigType->data.array->rank)
			{
				return false;
			}
			return IsMatchSigType(dstType->data.array->etype, sigType->data.array->etype, klassGenericContainer, methodGenericContainer);
		}
		case IL2CPP_TYPE_GENERICINST:
		{
			const Il2CppGenericInst* i1 = dstType->data.generic_class->context.class_inst;
			const Il2CppGenericInst* i2 = sigType->data.generic_class->context.class_inst;

			// this happens when maximum generic recursion is hit
			if (i1 == NULL || i2 == NULL)
			{
				return i1 == i2;
			}

			if (i1->type_argc != i2->type_argc)
				return false;

			if (!IsMatchSigType(dstType->data.generic_class->type, sigType->data.generic_class->type, klassGenericContainer, methodGenericContainer))
				return false;

			/* FIXME: we should probably just compare the instance pointers directly.  */
			for (uint32_t i = 0; i < i1->type_argc; ++i)
			{
				if (!IsMatchSigType(i1->type_argv[i], i2->type_argv[i], klassGenericContainer, methodGenericContainer))
				{
					return false;
				}
			}

			return true;
		}
		case IL2CPP_TYPE_VAR:
		{
			Il2CppMetadataGenericParameterHandle sigGph = il2cpp::vm::GlobalMetadata::GetGenericParameterFromIndex(
				(Il2CppMetadataGenericContainerHandle)klassGenericContainer, sigType->data.__genericParameterIndex);
			return dstType->data.genericParameterHandle == sigGph;
		}
		case IL2CPP_TYPE_MVAR:
		{
			Il2CppMetadataGenericParameterHandle sigGph = il2cpp::vm::GlobalMetadata::GetGenericParameterFromIndex(
				(Il2CppMetadataGenericContainerHandle)methodGenericContainer, sigType->data.__genericParameterIndex);
			return dstType->data.genericParameterHandle == sigGph;
		}
		default:
			return true;
		}
		IL2CPP_ASSERT(false);
		return false;
	}

	bool IsMatchSigType(const Il2CppType* dstType, const Il2CppType* sigType, const Il2CppType** klassInstArgv, const Il2CppType** methodInstArgv)
	{
		if (dstType->byref != sigType->byref)
		{
			return false;
		}

		if (sigType->type == IL2CPP_TYPE_VAR)
		{
			sigType = klassInstArgv[sigType->data.__genericParameterIndex];
		}
		else if (sigType->type == IL2CPP_TYPE_MVAR)
		{
			sigType = methodInstArgv[sigType->data.__genericParameterIndex];
		}

		if (dstType->type != sigType->type)
		{
			return false;
		}
		switch (sigType->type)
		{
		case IL2CPP_TYPE_VALUETYPE:
		case IL2CPP_TYPE_CLASS:
			return dstType->data.typeHandle == sigType->data.typeHandle;
		case IL2CPP_TYPE_PTR:
		case IL2CPP_TYPE_SZARRAY:
			return IsMatchSigType(dstType->data.type, sigType->data.type, klassInstArgv, methodInstArgv);

		case IL2CPP_TYPE_ARRAY:
		{
			if (dstType->data.array->rank < sigType->data.array->rank)
			{
				return false;
			}
			return IsMatchSigType(dstType->data.array->etype, sigType->data.array->etype, klassInstArgv, methodInstArgv);
		}
		case IL2CPP_TYPE_GENERICINST:
		{
			const Il2CppGenericInst* i1 = dstType->data.generic_class->context.class_inst;
			const Il2CppGenericInst* i2 = sigType->data.generic_class->context.class_inst;

			// this happens when maximum generic recursion is hit
			if (i1 == NULL || i2 == NULL)
			{
				return i1 == i2;
			}

			if (i1->type_argc != i2->type_argc)
				return false;

			if (!IsMatchSigType(dstType->data.generic_class->type, sigType->data.generic_class->type, klassInstArgv, methodInstArgv))
				return false;

			/* FIXME: we should probably just compare the instance pointers directly.  */
			for (uint32_t i = 0; i < i1->type_argc; ++i)
			{
				if (!IsMatchSigType(i1->type_argv[i], i2->type_argv[i], klassInstArgv, methodInstArgv))
				{
					return false;
				}
			}

			return true;
		}
		case IL2CPP_TYPE_VAR:
		{
			/*Il2CppMetadataGenericParameterHandle sigGph = il2cpp::vm::GlobalMetadata::GetGenericParameterFromIndex(
				(Il2CppMetadataGenericContainerHandle)klassGenericContainer, sigType->data.__genericParameterIndex);
			return dstType->data.genericParameterHandle == sigType->data.__genericParameterIndex;*/
			RaiseHuatuoNotSupportedException("");
			break;
		}
		case IL2CPP_TYPE_MVAR:
		{
			/*Il2CppMetadataGenericParameterHandle sigGph = il2cpp::vm::GlobalMetadata::GetGenericParameterFromIndex(
				(Il2CppMetadataGenericContainerHandle)methodGenericContainer, sigType->data.__genericParameterIndex);
			return dstType->data.genericParameterHandle == sigGph;*/
			RaiseHuatuoNotSupportedException("");
			break;
		}
		default: return true;
		}
		IL2CPP_ASSERT(false);
		return false;
	}


	bool IsMatchMethodSig(const MethodInfo* methodDef, const MethodRefSig& resolveSig, const Il2CppGenericContainer* klassGenericContainer, uint32_t genericArgCount)
	{
		if (methodDef->parameters_count != (uint16_t)resolveSig.params.size())
		{
			return false;
		}
		Il2CppGenericContainer* methodGenericContainer = nullptr;
		const Il2CppType* returnType1 = &resolveSig.returnType;
		const Il2CppType* returnType2 = methodDef->return_type;
		if (!IsMatchSigType(returnType2, returnType1, klassGenericContainer, methodGenericContainer))
		{
			return false;
		}
		for (uint32_t i = 0; i < methodDef->parameters_count; i++)
		{
			const Il2CppType* paramType1 = &resolveSig.params[i];
			const Il2CppType* paramType2 = GET_METHOD_PARAMETER_TYPE(methodDef->parameters[i]);
			if (!IsMatchSigType(paramType2, paramType1, klassGenericContainer, methodGenericContainer))
			{
				return false;
			}
		}
		return true;
	}

	bool IsMatchMethodSig(const Il2CppMethodDefinition* methodDef, const MethodRefSig& resolveSig, const Il2CppGenericContainer* klassGenericContainer, uint32_t genericArgCount)
	{
		if (methodDef->parameterCount != (uint16_t)resolveSig.params.size())
		{
			return false;
		}
		Il2CppGenericContainer* methodGenericContainer = nullptr;
		// if generic param not match. return false
		if (methodDef->genericContainerIndex == kGenericContainerIndexInvalid)
		{
			if (genericArgCount != 0)
			{
				return false;
			}
		}
		else
		{
			if (genericArgCount == 0)
			{
				return false;
			}
			methodGenericContainer = (Il2CppGenericContainer*)il2cpp::vm::GlobalMetadata::GetGenericContainerFromIndex(methodDef->genericContainerIndex);
			if (genericArgCount != methodGenericContainer->type_argc)
			{
				return false;
			}
		}

		const Il2CppType* returnType1 = &resolveSig.returnType;
		const Il2CppType* returnType2 = il2cpp::vm::GlobalMetadata::GetIl2CppTypeFromIndex(methodDef->returnType);
		if (!IsMatchSigType(returnType2, returnType1, klassGenericContainer, methodGenericContainer))
		{
			return false;
		}
		for (uint32_t i = 0; i < methodDef->parameterCount; i++)
		{
			const Il2CppType* paramType1 = &resolveSig.params[i];
			const Il2CppParameterDefinition* dstParam = (const Il2CppParameterDefinition*)il2cpp::vm::GlobalMetadata::GetParameterDefinitionFromIndex(methodDef, methodDef->parameterStart + i);
			IL2CPP_ASSERT(dstParam);
			const Il2CppType* paramType2 = il2cpp::vm::GlobalMetadata::GetIl2CppTypeFromIndex(dstParam->typeIndex);
			if (!IsMatchSigType(paramType2, paramType1, klassGenericContainer, methodGenericContainer))
			{
				return false;
			}
		}
		return true;
	}

	bool IsMatchMethodSig(const MethodInfo* methodDef, const MethodRefSig& resolveSig, const Il2CppType** klassInstArgv, const Il2CppType** methodInstArgv)
	{
		if (methodDef->parameters_count != (uint16_t)resolveSig.params.size())
		{
			return false;
		}
		Il2CppGenericContainer* methodGenericContainer = nullptr;
		const Il2CppType* returnType1 = &resolveSig.returnType;
		const Il2CppType* returnType2 = methodDef->return_type;
		if (!IsMatchSigType(returnType2, returnType1, klassInstArgv, methodInstArgv))
		{
			return false;
		}
		for (uint32_t i = 0; i < methodDef->parameters_count; i++)
		{
			const Il2CppType* paramType1 = &resolveSig.params[i];
			const Il2CppType* paramType2 = GET_METHOD_PARAMETER_TYPE(methodDef->parameters[i]);
			if (!IsMatchSigType(paramType2, paramType1, klassInstArgv, methodInstArgv))
			{
				return false;
			}
		}
		return true;
	}


	const Il2CppMethodDefinition* ResolveMethodDefinition(const Il2CppType* type, const char* resolveMethodName, const MethodRefSig& resolveSig, const Il2CppGenericInst* genericInstantiation)
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
				return methodDef;
			}
		}
		RaiseMethodNotFindException(type, resolveMethodName);
		return nullptr;
	}

	const MethodInfo* GetMethodInfoFromMethodDef(const Il2CppType* type, const Il2CppMethodDefinition* methodDef)
	{
		Il2CppClass* klass = il2cpp::vm::Class::FromIl2CppType(type);
		il2cpp::vm::Class::SetupMethods(klass);
		void* iter = nullptr;
		for (const MethodInfo* cur = nullptr; (cur = il2cpp::vm::Class::GetMethods(klass, &iter)) != nullptr; )
		{
			if (!cur->is_inflated)
			{
				if ((const Il2CppMethodDefinition*)cur->methodMetadataHandle == methodDef)
				{
					return cur;
				}
			}
			else
			{
				if ((const Il2CppMethodDefinition*)cur->genericMethod->methodDefinition->methodMetadataHandle == methodDef)
				{
					return cur;
				}
			}
		}
		RaiseMethodNotFindException(type, il2cpp::vm::GlobalMetadata::GetStringFromIndex(methodDef->nameIndex));
		return nullptr;
	}

	bool ResolveField(const Il2CppType* type, const char* resolveFieldName, Il2CppType* resolveFieldType, const Il2CppFieldDefinition*& retFieldDef)
	{
		const Il2CppTypeDefinition* typeDef = GetUnderlyingTypeDefinition(type);
		const Il2CppGenericContainer* klassGenericContainer = GetGenericContainerFromIl2CppType(type);
		for (uint16_t i = 0; i < typeDef->field_count; i++)
		{
			const Il2CppFieldDefinition* fieldDef = il2cpp::vm::GlobalMetadata::GetFieldDefinitionFromTypeDefAndFieldIndex(typeDef, i);
			const char* fieldName = il2cpp::vm::GlobalMetadata::GetStringFromIndex(fieldDef->nameIndex);
			const Il2CppType* fieldType = il2cpp::vm::GlobalMetadata::GetIl2CppTypeFromIndex(fieldDef->typeIndex);
			if (std::strcmp(resolveFieldName, fieldName) == 0 && IsMatchSigType(fieldType, resolveFieldType, klassGenericContainer, nullptr))
			{
				retFieldDef = fieldDef;
				return true;
			}
		}
		retFieldDef = nullptr;
		return false;
	}

	const Il2CppGenericContainer* GetGenericContainerFromIl2CppType(const Il2CppType* type)
	{
		switch (type->type)
		{
		case IL2CPP_TYPE_GENERICINST:
		{
			return (Il2CppGenericContainer*)il2cpp::vm::GlobalMetadata::GetGenericContainerFromGenericClass(type->data.generic_class);
		}
		case IL2CPP_TYPE_VALUETYPE:
		case IL2CPP_TYPE_CLASS:
		{
			return (Il2CppGenericContainer*)il2cpp::vm::GlobalMetadata::GetGenericContainerFromIndex(((Il2CppTypeDefinition*)type->data.typeHandle)->genericContainerIndex);
		}
		default:
		{
			return nullptr;
		}
		}
	}

	Il2CppGenericInst* TryInflateGenericInst(Il2CppGenericInst* inst, const Il2CppGenericContext* genericContext)
	{
		for (uint32_t i = 0; i < inst->type_argc; i++)
		{
			inst->type_argv[i] = TryInflateIfNeed(inst->type_argv[i], genericContext, true);
		}
		return inst;
	}
}
}
