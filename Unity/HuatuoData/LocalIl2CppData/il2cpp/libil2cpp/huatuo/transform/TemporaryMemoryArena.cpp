#include "TemporaryMemoryArena.h"

namespace huatuo
{
namespace transform
{

	TemporaryMemoryArena::Block TemporaryMemoryArena::AllocBlock(size_t size)
	{
		void* data = IL2CPP_MALLOC(size);
		return { data, size };
	}

	void TemporaryMemoryArena::Begin()
	{
		IL2CPP_ASSERT(_buf == nullptr);
		IL2CPP_ASSERT(_size == 0);
		IL2CPP_ASSERT(_pos == 0);
		RequireSize(kMinBlockSize);
	}

	void TemporaryMemoryArena::End()
	{
		if (_buf)
		{
			IL2CPP_FREE(_buf);
			//_buf = nullptr;
			//_size = _pos = 0;
		}
		for (auto& block : _useOuts)
		{
			IL2CPP_FREE(block.data);
		}
	}
}
}