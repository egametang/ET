#include "BasicBlockSpliter.h"

#include "../metadata/Opcodes.h"
#include "../metadata/MetadataUtil.h"

using namespace huatuo::metadata;

namespace huatuo
{
namespace transform
{

	void BasicBlockSpliter::SplitNormal(const byte* ilcodeStart, uint32_t codeSize, std::unordered_set<uint32_t>& ilOffsets)
	{
		const byte* codeEnd = ilcodeStart + codeSize;
		const byte* ip = ilcodeStart;

		while (ip < codeEnd)
		{
			ilOffsets.insert((uint32_t)(ip - ilcodeStart));
			const OpCodeInfo* oc = DecodeOpCodeInfo(ip, codeEnd);
			IL2CPP_ASSERT(oc);
			int32_t opCodeSize = GetOpCodeSize(ip, oc);
			const byte* nextIp = ip + opCodeSize;
			int32_t nextOffset = (int32_t)(nextIp - ilcodeStart);
			IL2CPP_ASSERT(nextOffset >= 0 && nextOffset <= (int32_t)codeSize);

			switch (oc->inlineType)
			{
			case ArgType::None:
			case ArgType::Data:
			{
				break;
			}
			case ArgType::StaticBranch:
			{
				_splitOffsets.insert(nextOffset);
				break;
			}
			case ArgType::BranchTarget:
			{
				int32_t offset;
				switch (oc->inlineParam)
				{
				case 1:
				{
					offset = GetI1(ip + 1);
					break;
				}
				case 4:
				{
					offset = GetI4LittleEndian(ip + 1);
					break;
				}
				default:
				{
					IL2CPP_ASSERT(false);
				}
				}
				// don't split 0 offset br
				if (offset != 0 || (oc->baseOpValue == OpcodeValue::LEAVE || oc->baseOpValue == OpcodeValue::LEAVE_S))
				{
					_splitOffsets.insert(nextOffset);
					_splitOffsets.insert(nextOffset + offset);
				}
				break;
			}
			case ArgType::Switch:
			{
				uint32_t caseNum = GetI4LittleEndian(ip + 1);
				bool splitAny = false;
				for (uint32_t caseIdx = 0; caseIdx < caseNum; caseIdx++)
				{
					int32_t caseOffset = GetI4LittleEndian(ip + 5 + caseIdx * 4);
					if (caseOffset != 0)
					{
						_splitOffsets.insert(nextOffset + caseOffset);
						splitAny = true;
					}
				}
				if (splitAny)
				{
					_splitOffsets.insert(nextOffset);
				}
				break;
			}
			default:
			{
				IL2CPP_ASSERT(false && "unknown inline type");
			}
			}
			ip = nextIp;
		}
		IL2CPP_ASSERT(ip == codeEnd);
	}

	void BasicBlockSpliter::SplitExceptionHandles(const byte* ilcodeStart, uint32_t codeSize, const std::vector<metadata::ExceptionClause>& exceptionClauses)
	{
		for (auto& eh : exceptionClauses)
		{
			_splitOffsets.insert(eh.tryOffset);
			_splitOffsets.insert(eh.tryOffset + eh.tryLength);
			_splitOffsets.insert(eh.handlerOffsets);
			_splitOffsets.insert(eh.handlerOffsets + eh.handlerLength);
			if (eh.flags == CorILExceptionClauseType::Filter)
			{
				_splitOffsets.insert(eh.classTokenOrFilterOffset);
			}
		}
	}

	void BasicBlockSpliter::SplitBasicBlocks()
	{
		const byte* ilcodeStart = _body.ilcodes;

		std::unordered_set<uint32_t> ilOffsets;
		ilOffsets.insert(_body.codeSize);

		SplitNormal(ilcodeStart, _body.codeSize, ilOffsets);
		SplitExceptionHandles(ilcodeStart, _body.codeSize, _body.exceptionClauses);

		/*if (_splitOffsets.find(0) != _splitOffsets.end())
		{
			_splitOffsets.erase(0);
		}*/
#if DEBUG
		for (uint32_t offset : _splitOffsets)
		{
			IL2CPP_ASSERT(ilOffsets.find(offset) != ilOffsets.end());
		}
		IL2CPP_ASSERT(_splitOffsets.find(_body.codeSize) != _splitOffsets.end());
#endif
	}
}
}