#pragma once
#include "../CommonDef.h"
#include "../metadata/MetadataDef.h"

namespace huatuo
{
	namespace interpreter
	{

		// from obj or arg
		enum class LocationDataType
		{
			I1,
			U1,
			I2,
			U2,
			U8,
			// U16,
			S_12,
			S_16, // struct size == 16
			S_20,
			S_24, // struct size == 24
			S_28,
			S_32, // struct size == 32
			S_N,  // struct size = 3，5，6，7， > 8, size is described by stackObjectSize
		};

		union StackObject
		{
			void* ptr; // can't adjust position. will raise native_invoke init args bugs.
			bool b;
			int8_t i8;
			uint8_t u8;
			int16_t i16;
			uint16_t u16;
			int32_t i32;
			uint32_t u32;
			int64_t i64;
			uint64_t u64;
			float f4;
			double f8;
			Il2CppObject* obj;
			Il2CppString* str;
			Il2CppObject** ptrObj;
		};

		static_assert(sizeof(StackObject) == 8, "requrie 64bit");


		enum class ExceptionFlowType
		{
			None,
			Exception,
			Leave,
		};

		struct InterpMethodInfo;

		struct ExceptionFlowInfo
		{
			ExceptionFlowType exFlowType;
			int32_t throwOffset;
			Il2CppException* ex;
			int32_t nextExClauseIndex;
			int32_t leaveTarget;
		};

		struct InterpFrame
		{
			const InterpMethodInfo* method;
			StackObject* stackBasePtr;
			ptrdiff_t oldStackTop;
			void* ret;

			byte* ip;

			//Il2CppException* saveException;
			std::vector<ExceptionFlowInfo>* exHandleStack;
			ExceptionFlowInfo prevExFlowInfo;
			ExceptionFlowInfo curExFlowInfo;

			//std::vector<void*> *bigLocalAllocs;
		};

		struct InterpExceptionClause
		{
			metadata::CorILExceptionClauseType flags;
			int32_t tryBeginOffset;
			int32_t tryEndOffset;
			int32_t handlerBeginOffset;
			int32_t handlerEndOffset;
			int32_t filterBeginOffset;
			Il2CppClass* exKlass;
		};

		struct ArgDesc
		{
			LocationDataType type;
			uint32_t stackObjectSize; //
		};

		struct InterpMethodInfo
		{
			const MethodInfo* method;
			ArgDesc* args;
			uint32_t argCount;
			uint32_t argStackObjectSize;
			byte* codes;
			uint32_t codeLength;
			uint32_t maxStackSize; // args + locals + evalstack size
			uint32_t localVarBaseOffset;
			uint32_t evalStackBaseOffset;
			uint32_t localStackSize; // args + locals StackObject size
			std::vector<const void*> resolveDatas;
			std::vector<InterpExceptionClause*> exClauses;
			uint32_t isTrivialCopyArgs : 1;
		};
	}
}