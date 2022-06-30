#pragma once

#include <stack>

#include "../CommonDef.h"

#include "gc/GarbageCollector.h"
#include "vm/Exception.h"
#include "vm/StackTrace.h"

#include "../metadata/MetadataDef.h"
#include "../HuatuoConfig.h"

#include "InterpreterDefs.h"


#if DEBUG
#define PUSH_STACK_FRAME(method) do { \
	Il2CppStackFrameInfo stackFrameInfo = { method, (uintptr_t)method->methodPointer }; \
	il2cpp::vm::StackTrace::PushFrame(stackFrameInfo); \
} while(0)

#define POP_STACK_FRAME() do { il2cpp::vm::StackTrace::PopFrame(); } while(0)

#else 
#define PUSH_STACK_FRAME(method)
#define POP_STACK_FRAME() 
#endif

namespace huatuo
{
namespace interpreter
{

	class MachineState
	{
	public:
		MachineState()
		{
			HuatuoConfig& hc = HuatuoConfig::GetIns();
			_stackSize = hc.GetInterpreterThreadObjectStackSize();
			_stackBase = (StackObject*)il2cpp::gc::GarbageCollector::AllocateFixed(hc.GetInterpreterThreadObjectStackSize() * sizeof(StackObject), nullptr);
			std::memset(_stackBase, 0, _stackSize * sizeof(StackObject));
			_stackTopIdx = 0;

			_frameBase = (InterpFrame*)IL2CPP_CALLOC(hc.GetInterpreterThreadFrameStackSize(), sizeof(InterpFrame));
			_frameCount = hc.GetInterpreterThreadFrameStackSize();
			_frameTopIdx = 0;
		}

		~MachineState()
		{
			il2cpp::gc::GarbageCollector::FreeFixed(_stackBase);
		}

		StackObject* AllocArgments(uint32_t argCount)
		{
			if (_stackTopIdx + argCount > _stackSize)
			{
				il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetStackOverflowException("AllocArguments"));
			}
			StackObject* args = _stackBase + _stackTopIdx;
			_stackTopIdx += argCount;
			return args;
		}

		StackObject* GetStackBasePtr() const
		{
			return _stackBase;
		}

		ptrdiff_t GetStackTop() const
		{
			return _stackTopIdx;
		}

		StackObject* AllocStackSlot(uint32_t slotNum)
		{
			if (_stackTopIdx + slotNum > _stackSize)
			{
				il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetStackOverflowException("AllocStackSlot"));
			}
			StackObject* dataPtr = _stackBase + _stackTopIdx;
			_stackTopIdx += slotNum;
			return dataPtr;
		}

		void SetStackTop(ptrdiff_t oldTop)
		{
			_stackTopIdx = oldTop;
		}

		uint32_t GetFrameTopIdx() const
		{
			return _frameTopIdx;
		}

		InterpFrame* PushFrame()
		{
			if (_frameTopIdx >= _frameCount)
			{
				il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetStackOverflowException("AllocFrame"));
			}
			return _frameBase + _frameTopIdx++;
		}

		void PopFrame()
		{
			IL2CPP_ASSERT(_frameTopIdx > 0);
			--_frameTopIdx;
		}

		void PopFrameN(uint32_t count)
		{
			IL2CPP_ASSERT(count > 0 && _frameTopIdx >= count);
			_frameTopIdx -= count;
		}

		InterpFrame* GetTopFrame() const
		{
			if (_frameTopIdx > 0)
			{
				return _frameBase + _frameTopIdx - 1;
			}
			else
			{
				return nullptr;
			}
		}

		//const InterpFrame* GetFrameBaseMinusOne() const
		//{
		//	return _frameBase - 1;
		//}

		void PushExecutingImage(const Il2CppImage* image)
		{
			_executingImageStack.push(image);
		}

		void PopExecutingImage()
		{
			_executingImageStack.pop();
		}

		const Il2CppImage* GetTopExecutingImage() const
		{
			if (_executingImageStack.empty())
			{
				return nullptr;
			}
			else
			{
				return _executingImageStack.top();
			}
		}

		void CollectFrames(il2cpp::vm::StackFrames* stackFrames)
		{
			for (uint32_t i = 0; i < _frameTopIdx; i++)
			{
				InterpFrame* frame = _frameBase + i;
				const MethodInfo* method = frame->method->method;
				Il2CppStackFrameInfo stackFrameInfo = { method, (uintptr_t)method->methodPointer };
				stackFrames->push_back(stackFrameInfo);
			}
		}

	private:

		StackObject* _stackBase;
		ptrdiff_t _stackSize;
		ptrdiff_t _stackTopIdx;

		InterpFrame* _frameBase;
		uint32_t _frameTopIdx;
		uint32_t _frameCount;

		std::stack<const Il2CppImage*> _executingImageStack;
	};

	class ExecutingInterpImageScope
	{
	public:
		ExecutingInterpImageScope(MachineState& state, const Il2CppImage* image) : _state(state)
		{
			_state.PushExecutingImage(image);
		}

		~ExecutingInterpImageScope()
		{
			_state.PopExecutingImage();
		}
		
	private:
		MachineState& _state;
	};

	//class NativeInterpFrameGroup
	//{
	//public:
	//	NativeInterpFrameGroup(MachineState& state, const MethodInfo* method) : _state(state), _interMethod({})
	//	{
	//		
	//		InterpFrame* frame = state.PushFrame();
	//		*frame = {};
	//		_interMethod.method = method;
	//		frame->method = &_interMethod;
	//	}

	//	~NativeInterpFrameGroup()
	//	{
	//		_state.PopFrame();
	//	}

	//private:
	//	MachineState _state;
	//	InterpMethodInfo _interMethod;
	//};

	class InterpFrameGroup
	{
	public:
		InterpFrameGroup(MachineState& ms) : _machineState(ms), _stackBaseIdx(ms.GetStackTop()), _frameBaseIdx(ms.GetFrameTopIdx())
		{

		}

		void CleanUpFrames()
		{
			IL2CPP_ASSERT(_machineState.GetFrameTopIdx() >= _frameBaseIdx);
			uint32_t n = _machineState.GetFrameTopIdx() - _frameBaseIdx;
			if (n > 0)
			{
				for (uint32_t i = 0; i < n; i++)
				{
					LeaveFrame();
				}
			}
		}

		InterpFrame* EnterFrame(const InterpMethodInfo* imi, StackObject* argBase, bool withArgStack);

		InterpFrame* LeaveFrame()
		{
			IL2CPP_ASSERT(_machineState.GetFrameTopIdx() > _frameBaseIdx);
			POP_STACK_FRAME();
			InterpFrame* frame = _machineState.GetTopFrame();
			if (frame->exHandleStack)
			{
				frame->exHandleStack->~vector();
				IL2CPP_FREE(frame->exHandleStack);
				frame->exHandleStack = nullptr;
			}
			_machineState.PopFrame();
			_machineState.SetStackTop(frame->oldStackTop);
			return _machineState.GetFrameTopIdx() > _frameBaseIdx ? _machineState.GetTopFrame() : nullptr;
		}

		void* AllocLoc(size_t size)
		{
			uint32_t soNum = (uint32_t)((size + sizeof(StackObject) - 1) / sizeof(StackObject));
			//void* data = _machineState.AllocStackSlot(soNum);
			//std::memset(data, 0, soNum * 8);
			void* data = IL2CPP_MALLOC_ZERO(size);
			return data;
 		}

		size_t GetFrameCount() const { return _machineState.GetFrameTopIdx() - _frameBaseIdx; }
	private:
		MachineState& _machineState;
		ptrdiff_t _stackBaseIdx;
		uint32_t _frameBaseIdx;
	};
}
}