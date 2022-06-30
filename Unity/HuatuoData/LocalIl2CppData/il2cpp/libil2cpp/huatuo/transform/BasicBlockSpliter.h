#pragma once

#include <list>
#include <unordered_set>
#include <set>

#include "../CommonDef.h"
#include "../metadata/MetadataDef.h"

namespace huatuo
{
namespace transform
{
	class BasicBlockSpliter
	{
	public:
		BasicBlockSpliter(const metadata::MethodBody& body) : _body(body) { }

		void SplitBasicBlocks();

		const std::set<uint32_t>& GetSplitOffsets() const { return _splitOffsets; }
	private:
		const metadata::MethodBody& _body;
		std::set<uint32_t> _splitOffsets;

		void SplitNormal(const byte* ilcodeStart, uint32_t codeSize, std::unordered_set<uint32_t>& ilOffsets);
		void SplitExceptionHandles(const byte* ilcodeStart, uint32_t codeSize, const std::vector<metadata::ExceptionClause>& exceptionClauses);
	};
}
}
