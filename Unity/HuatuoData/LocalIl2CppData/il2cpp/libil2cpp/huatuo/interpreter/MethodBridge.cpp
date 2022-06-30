#include "MethodBridge.h"

#include <codegen/il2cpp-codegen-metadata.h>
#include "vm/ClassInlines.h"
#include "vm/Object.h"
#include "vm/Class.h"

#include "../metadata/MetadataModule.h"
#include "../metadata/MetadataUtil.h"

#include "Interpreter.h"
#include "MemoryUtil.h"

namespace huatuo
{
	namespace interpreter
	{
		ArgDesc GetValueTypeArgDescBySize(uint32_t size)
		{
			switch (size)
			{
			case 1:
			case 2:
			case 4:
			case 8: return { LocationDataType::U8, 1 };
			case 12: return { LocationDataType::S_12, 2 };
			case 16: return { LocationDataType::S_16, 2 };
			case 20: return { LocationDataType::S_20, 3 };
			case 24: return { LocationDataType::S_24, 3 };
			case 28: return { LocationDataType::S_28, 4 };
			case 32: return { LocationDataType::S_32, 4 };
			default: return { LocationDataType::S_N, (uint32_t)metadata::GetStackSizeByByteSize(size) };
			}
		}

		ArgDesc GetTypeArgDesc(const Il2CppType* type)
		{
			if (type->byref)
			{
				return { LocationDataType::U8, 1 };
			}
			switch (type->type)
			{
			case IL2CPP_TYPE_BOOLEAN:
			case IL2CPP_TYPE_U1:
				return{ LocationDataType::U1, 1 };
			case IL2CPP_TYPE_I1:
				return{ LocationDataType::I1, 1 };
			case IL2CPP_TYPE_I2:
				return{ LocationDataType::I2, 1 };
			case IL2CPP_TYPE_CHAR:
			case IL2CPP_TYPE_U2:
				return{ LocationDataType::U2, 1 };
			case IL2CPP_TYPE_I4:
			case IL2CPP_TYPE_U4:
			case IL2CPP_TYPE_R4:
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
			case IL2CPP_TYPE_CLASS:
				return{ LocationDataType::U8, 1 };
			case IL2CPP_TYPE_TYPEDBYREF:
				return GetValueTypeArgDescBySize(sizeof(Il2CppTypedRef));
			case IL2CPP_TYPE_VALUETYPE:
			{
				Il2CppClass* klass = il2cpp::vm::Class::FromIl2CppType(type);
				IL2CPP_ASSERT(IS_CLASS_VALUE_TYPE(klass));
				if (klass->enumtype)
				{
					return GetTypeArgDesc(&klass->castClass->byval_arg);
				}
				return GetValueTypeArgDescBySize(il2cpp::vm::Class::GetValueSize(klass, nullptr));
			}
			case IL2CPP_TYPE_GENERICINST:
			{
				Il2CppGenericClass* genericClass = type->data.generic_class;
				if (genericClass->type->type == IL2CPP_TYPE_CLASS)
				{
					IL2CPP_ASSERT(!IS_CLASS_VALUE_TYPE(il2cpp::vm::Class::FromIl2CppType(type)));
					return{ LocationDataType::U8, 1 };
				}
				else
				{
					Il2CppClass* klass = il2cpp::vm::Class::FromIl2CppType(type);
					IL2CPP_ASSERT(IS_CLASS_VALUE_TYPE(klass));
					return GetValueTypeArgDescBySize(il2cpp::vm::Class::GetValueSize(klass, nullptr));
				}
			}
			default:
			{
				RaiseHuatuoExecutionEngineException("not support arg type");
				return{ LocationDataType::U8, 1 };
			}
			}
		}

		void CopyArgs(StackObject* dstBase, StackObject* argBase, ArgDesc* args, uint32_t paramCount, uint32_t totalParamStackObjectSize)
		{
			uint32_t dstOffset = 0;
			for (uint32_t i = 0, n = paramCount; i < n; i++)
			{
				ArgDesc& arg = args[i];
				StackObject* dst = dstBase + dstOffset;
				StackObject* src = argBase + i;
				switch (arg.type)
				{
				case LocationDataType::I1:
				//{
				//	dst->i64 = *(int8_t*)src;
				//	++dstOffset;
				//	break;
				//}
				case LocationDataType::U1:
				//{
				//	dst->i64 = *(uint8_t*)src;
				//	++dstOffset;
				//	break;
				//}
				case LocationDataType::I2:
				//{
				//	dst->i64 = *(int16_t*)src;
				//	++dstOffset;
				//	break;
				//}
				case LocationDataType::U2:
				//{
				//	dst->i64 = *(uint16_t*)src;
				//	++dstOffset;
				//	break;
				//}
				case LocationDataType::U8:
				{
					dst->i64 = *(int64_t*)src;
					++dstOffset;
					break;
				}
				//case LocationDataType::U16:
				//{
				//	RaiseHuatuoExecutionEngineException("CopyArgs not support U16");
				//	Copy12(dst, src->ptr);
				//	dstOffset += 2;
				//	break;
				//}
				case LocationDataType::S_12:
				{
					// when size > 8, arg is ref to struct
					Copy12(dst, src->ptr);
					dstOffset += 2;
					break;
				}
				case LocationDataType::S_16:
				{
					// when size > 8, arg is ref to struct
					Copy16(dst, src->ptr);
					dstOffset += 2;
					break;
				}
				case LocationDataType::S_20:
				{
					Copy20(dst, src->ptr);
					dstOffset += 3;
					break;
				}
				case LocationDataType::S_24:
				{
					Copy24(dst, src->ptr);
					dstOffset += 3;
					break;
				}
				case LocationDataType::S_28:
				{
					Copy28(dst, src->ptr);
					dstOffset += 4;
					break;
				}
				case LocationDataType::S_32:
				{
					Copy32(dst, src->ptr);
					dstOffset += 4;
					break;
				}
				case LocationDataType::S_N:
				{
					std::memcpy(dst, src->ptr, arg.stackObjectSize * sizeof(StackObject));
					dstOffset += arg.stackObjectSize;
					break;
				}
				default:
				{
					RaiseHuatuoExecutionEngineException("CopyArgs not support data type");
				}
				}
			}
			IL2CPP_ASSERT(dstOffset == totalParamStackObjectSize);
		}

		bool IsPassArgAsValue(const Il2CppType* type, LocationDataType* locType)
		{
			ArgDesc argDesc = interpreter::GetTypeArgDesc(type);
			if (locType)
			{
				*locType = argDesc.type;
			}
			return argDesc.type <= LocationDataType::U8;
		}

		Il2CppObject* TranslateNativeValueToBoxValue(const Il2CppType* type, void* value)
		{
			if (type->byref)
			{
				IL2CPP_ASSERT(false);
				return nullptr;
			}
			Il2CppClass* klass = il2cpp::vm::Class::FromIl2CppType(type);
			return il2cpp::vm::Object::Box(klass, value);
		}

		const uint32_t kMaxByValueSize = 8;

		void ConvertInvokeArgs(StackObject* resultArgs, const MethodInfo* method, void** __args)
		{
			for (uint8_t i = 0; i < method->parameters_count; i++)
			{
				const Il2CppType* argType = GET_METHOD_PARAMETER_TYPE(method->parameters[i]);
				StackObject* dst = resultArgs + i;

				if (argType->byref)
				{
					dst->ptr = __args[i];
				}
				else if (huatuo::metadata::IsValueType(argType))
				{
					if (IsPassArgAsValue(argType))
					{
						dst->i64 = *(uint64_t*)__args[i];
					}
					else
					{
						dst->ptr = __args[i];
					}
				}
				//else if (argType->type == IL2CPP_TYPE_PTR)
				//{
				//    dst->ptr = __args[i];
				//}
				else
				{
					dst->ptr = __args[i];
				}
			}
		}

		struct HFATypeInfo
		{
			const Il2CppType* eleType;
			int32_t count;
		};


		static bool ComputeHFATypeInfo0(Il2CppClass* klass, HFATypeInfo& typeInfo)
		{
			il2cpp::vm::Class::SetupFields(klass);
			for (uint16_t i = 0; i < klass->field_count; i++)
			{
				FieldInfo* field = klass->fields + i;
				const Il2CppType* ftype = field->type;
				if (!metadata::IsInstanceField(ftype))
				{
					continue;
				}
				if (ftype->byref)
				{
					return false;
				}
				if ((ftype->type != IL2CPP_TYPE_R4 && ftype->type != IL2CPP_TYPE_R8))
				{
					if (ftype->type == IL2CPP_TYPE_VALUETYPE || (ftype->type == IL2CPP_TYPE_GENERICINST && ftype->data.generic_class->type->type == IL2CPP_TYPE_VALUETYPE))
					{
						Il2CppClass* fieldKlass = il2cpp::vm::Class::FromIl2CppType(ftype);
						if (!ComputeHFATypeInfo0(fieldKlass, typeInfo))
						{
							return false;
						}
					}
					else
					{
						return false;
					}
				}
				else if (typeInfo.eleType == nullptr || metadata::IsTypeEqual(ftype, typeInfo.eleType))
				{
					typeInfo.eleType = ftype;
					++typeInfo.count;
				}
				else
				{
					return false;
				}
			}
			return typeInfo.count <= 4;
		}

		static bool ComputeHFATypeInfo(Il2CppClass* klass, HFATypeInfo& typeInfo)
		{
			typeInfo = {};
			int32_t size = metadata::GetTypeValueSize(klass);
			switch (size)
			{
			case 8:
			case 12:
			case 16:
			case 24:
			case 32: break;
			default: return false;
			}
			
			bool isHFA = ComputeHFATypeInfo0(klass, typeInfo);
			if (isHFA && typeInfo.count >= 2 && typeInfo.count <= 4)
			{
				int32_t fieldSize = typeInfo.eleType->type == IL2CPP_TYPE_R4 ? 4 : 8;
				return size == fieldSize * typeInfo.count;
			}
			return false;
		}

		static void AppendString(char* sigBuf, size_t bufSize, size_t& pos, const char* str)
		{
			size_t len = std::strlen(str);
			if (pos + len < bufSize)
			{
				std::strcpy(sigBuf + pos, str);
				pos += len;
			}
			else
			{
				IL2CPP_ASSERT(false);
			}
		}

		static void AppendValueTypeSignature(Il2CppClass* klass, bool returnType, char* sigBuf, size_t bufferSize, size_t& pos)
		{
			int typeSize = il2cpp::vm::Class::GetValueSize(klass, nullptr);
#if HUATUO_TARGET_ARM
			HFATypeInfo typeInfo = {};
			if (ComputeHFATypeInfo(klass, typeInfo))
			{
				if (typeInfo.eleType->type == IL2CPP_TYPE_R4)
				{
					switch (typeInfo.count)
					{
					case 2:
					{
						AppendString(sigBuf, bufferSize, pos, "vf2");
						return;
					}
					case 3:
					{
						AppendString(sigBuf, bufferSize, pos, "vf3");
						return;
					}
					case 4:
					{
						AppendString(sigBuf, bufferSize, pos, "vf4");
						return;
					}
					}
				}
				else
				{
					IL2CPP_ASSERT(typeInfo.eleType->type == IL2CPP_TYPE_R8);
					switch (typeInfo.count)
					{
					case 2:
					{
						AppendString(sigBuf, bufferSize, pos, "vd2");
						return;
					}
					case 3:
					{
						AppendString(sigBuf, bufferSize, pos, "vd3");
						return;
					}
					case 4:
					{
						AppendString(sigBuf, bufferSize, pos, "vd4");
						return;
					}
					}
				}
			}
			// FIXME HSV

			if (typeSize <= 8)
			{
				AppendString(sigBuf, bufferSize, pos, "i8");
			}
			else if (typeSize <= 16)
			{
				AppendString(sigBuf, bufferSize, pos, "s2");
			}
			else
			{
				if (returnType)
				{
					pos += std::sprintf(sigBuf + pos, "S%d", typeSize);
				}
				else
				{
					AppendString(sigBuf, bufferSize, pos, "sr");
				}
			}
#else
			switch (typeSize)
			{
			case 1:
			case 2:
			case 4:
			case 8:
			{
				AppendString(sigBuf, bufferSize, pos, "i8");
				break;
			}
			default:
			{
				if (returnType)
				{
					pos += std::sprintf(sigBuf + pos, "S%d", typeSize);
				}
				else
				{
					AppendString(sigBuf, bufferSize, pos, "sr");
				}
			}
			}
#endif
		}

		static bool IsGenericFullInstantiate(const Il2CppType* type)
		{
			switch (type->type)
			{
			case IL2CPP_TYPE_VAR:
			case IL2CPP_TYPE_MVAR: return false;
			case IL2CPP_TYPE_GENERICINST:
			{
				Il2CppGenericContext& gctx = type->data.generic_class->context;
				const Il2CppGenericInst* gis[] = { gctx.class_inst, gctx.method_inst };

				for (const Il2CppGenericInst* gi : gis)
				{
					if (!gi)
					{
						continue;
					}
					for (uint8_t i = 0; i < gi->type_argc; i++)
					{
						const Il2CppType* gtype = gi->type_argv[i];
						if (!IsGenericFullInstantiate(gtype))
						{
							return false;
						}
					}
				}
				return true;
			}
			default: return true;

			}
		}

		static void AppendSignature(const Il2CppType* type, bool returnType, char* sigBuf, size_t bufferSize, size_t& pos)
		{
			if (type->byref)
			{
				AppendString(sigBuf, bufferSize, pos, "i8");
				return;
			}
			switch (type->type)
			{
			case IL2CPP_TYPE_VOID: AppendString(sigBuf, bufferSize, pos, "v"); break;
#if HUATUO_TARGET_ARM
			case IL2CPP_TYPE_BOOLEAN:
			case IL2CPP_TYPE_I1:
			case IL2CPP_TYPE_U1: AppendString(sigBuf, bufferSize, pos, "i1"); break;
			case IL2CPP_TYPE_I2:
			case IL2CPP_TYPE_U2:
			case IL2CPP_TYPE_CHAR: AppendString(sigBuf, bufferSize, pos, "i2"); break;
			case IL2CPP_TYPE_I4:
			case IL2CPP_TYPE_U4: AppendString(sigBuf, bufferSize, pos, "i4"); break;
			case IL2CPP_TYPE_R4: AppendString(sigBuf, bufferSize, pos, "r4"); break;
#else
			case IL2CPP_TYPE_R4:
#endif
			case IL2CPP_TYPE_R8: AppendString(sigBuf, bufferSize, pos, "r8"); break;
			case IL2CPP_TYPE_TYPEDBYREF:
			{
				IL2CPP_ASSERT(sizeof(Il2CppTypedRef) == sizeof(void*) * 3);
				AppendValueTypeSignature(il2cpp_defaults.typed_reference_class, returnType, sigBuf, bufferSize, pos);
				break;
			}
			case IL2CPP_TYPE_VALUETYPE:
			{
				Il2CppClass* klass = il2cpp::vm::Class::FromIl2CppType(type);
				IL2CPP_ASSERT(IS_CLASS_VALUE_TYPE(klass));
				AppendValueTypeSignature(klass, returnType, sigBuf, bufferSize, pos);
				break;
			}
			case IL2CPP_TYPE_GENERICINST:
			{
				const Il2CppType* underlyingGenericType = type->data.generic_class->type;
				if (underlyingGenericType->type == IL2CPP_TYPE_CLASS)
				{
					AppendString(sigBuf, bufferSize, pos, "i8");
				}
				else
				{
					IL2CPP_ASSERT(underlyingGenericType->type == IL2CPP_TYPE_VALUETYPE);
					if (!IsGenericFullInstantiate(type))
					{
						AppendString(sigBuf, bufferSize, pos, "#not_full_generic#");
						return;
					}
					Il2CppClass* klass = il2cpp::vm::Class::FromIl2CppType(type);
					IL2CPP_ASSERT(IS_CLASS_VALUE_TYPE(klass));
					AppendValueTypeSignature(klass, returnType, sigBuf, bufferSize, pos);
				}
				break;
			}
			default: AppendString(sigBuf, bufferSize, pos, "i8"); break;
			}
		}

		bool ComputeSignature(const Il2CppType* ret, const Il2CppType* params, uint32_t paramCount, bool instanceCall, char* sigBuf, size_t bufferSize)
		{
			size_t pos = 0;
			AppendSignature(ret, true, sigBuf, bufferSize, pos);

			if (instanceCall)
			{
				AppendString(sigBuf, bufferSize, pos, "i8");
			}

			for (uint8_t i = 0; i < paramCount; i++)
			{
				AppendSignature(params + i, false, sigBuf, bufferSize, pos);
			}
			sigBuf[pos] = 0;
			return true;
		}

		bool ComputeSignature(const Il2CppMethodDefinition* method, bool call, char* sigBuf, size_t bufferSize)
		{
			size_t pos = 0;

			const Il2CppImage* image = huatuo::metadata::MetadataModule::GetImage(method)->GetIl2CppImage();

			AppendSignature(huatuo::metadata::MetadataModule::GetIl2CppTypeFromEncodeIndex(method->returnType), true, sigBuf, bufferSize, pos);

			if (call && metadata::IsInstanceMethod(method))
			{
				AppendString(sigBuf, bufferSize, pos, "i8");
			}

			for (uint8_t i = 0; i < method->parameterCount; i++)
			{
				TypeIndex paramTypeIndex = huatuo::metadata::MetadataModule::GetParameterDefinitionFromIndex(image, method->parameterStart + i)->typeIndex;
				AppendSignature(huatuo::metadata::MetadataModule::GetIl2CppTypeFromEncodeIndex(paramTypeIndex), false, sigBuf, bufferSize, pos);
			}
			sigBuf[pos] = 0;
			return true;
		}

		bool ComputeSignature(const MethodInfo* method, bool call, char* sigBuf, size_t bufferSize)
		{
			size_t pos = 0;

			AppendSignature(method->return_type, true, sigBuf, bufferSize, pos);

			if (call && metadata::IsInstanceMethod(method))
			{
				AppendString(sigBuf, bufferSize, pos, "i8");
			}

			for (uint8_t i = 0; i < method->parameters_count; i++)
			{
				AppendSignature(GET_METHOD_PARAMETER_TYPE(method->parameters[i]), false, sigBuf, bufferSize, pos);
			}
			sigBuf[pos] = 0;
			return true;
		}

	}
}
