#pragma once

#include "BasicBlockSpliter.h"

#include "../metadata/Image.h"
#include "../interpreter/Instruction.h"
#include "../interpreter/Engine.h"

namespace huatuo
{
namespace transform
{

	struct IRBasicBlock
	{
		bool visited;
		bool inPending;
		uint32_t ilOffset;
		uint32_t codeOffset;
		std::vector<interpreter::IRCommon*> insts;
	};

	struct ArgVarInfo
	{
		const Il2CppType* type;
		Il2CppClass* klass;
		int32_t argOffset; // StackObject index
		int32_t argLocOffset;
	};

	struct LocVarInfo
	{
		const Il2CppType* type;
		Il2CppClass* klass;
		int32_t locOffset;
	};

	enum class EvalStackReduceDataType
	{
		I4,
		I8,
		I,
		R4,
		R8,
		Ref,
		Obj,
		Other,
	};

	struct EvalStackVarInfo
	{
		EvalStackReduceDataType reduceType;
		int32_t byteSize;
		int32_t locOffset;
	};

	class HiTransform
	{
	public:
		static void Transform(metadata::Image* image, const MethodInfo* methodInfo, metadata::MethodBody& body, interpreter::InterpMethodInfo& result);
	};
}
}