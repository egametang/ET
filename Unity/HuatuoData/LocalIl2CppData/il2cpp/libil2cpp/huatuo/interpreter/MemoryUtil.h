#pragma once
#include "InterpreterDefs.h"

namespace huatuo
{
namespace interpreter
{

	inline void Copy1(void* dst, void* src)
	{
		*(uint8_t*)dst = *(uint8_t*)src;
	}

	inline void Copy2(void* dst, void* src)
	{
		*(uint16_t*)dst = *(uint16_t*)src;
	}

	inline void Copy4(void* dst, void* src)
	{
		*(uint32_t*)dst = *(uint32_t*)src;
	}

	inline void Copy8(void* dst, void* src)
	{
		*(uint64_t*)dst = *(uint64_t*)src;
	}

	inline void Copy12(void* dst, void* src)
	{
		if (dst <= src)
		{
			*(uint64_t*)dst = *(uint64_t*)src;
			*(uint32_t*)((byte*)dst + 8) = *(uint32_t*)((byte*)src + 8);
		}
		else
		{
			*(uint32_t*)((byte*)dst + 8) = *(uint32_t*)((byte*)src + 8);
			*(uint64_t*)dst = *(uint64_t*)src;
		}
	}

	inline void Copy16(void* dst, void* src)
	{
		if (dst <= src)
		{
			*(uint64_t*)dst = *(uint64_t*)src;
			*(uint64_t*)((byte*)dst + 8) = *(uint64_t*)((byte*)src + 8);
		}
		else
		{
			*(uint64_t*)((byte*)dst + 8) = *(uint64_t*)((byte*)src + 8);
			*(uint64_t*)dst = *(uint64_t*)src;
		}
	}

	inline void Copy20(void* dst, void* src)
	{
		if (dst <= src)
		{
			*(uint64_t*)dst = *(uint64_t*)src;
			*(uint64_t*)((byte*)dst + 8) = *(uint64_t*)((byte*)src + 8);
			*(uint32_t*)((byte*)dst + 16) = *(uint32_t*)((byte*)src + 16);
		}
		else
		{
			*(uint32_t*)((byte*)dst + 16) = *(uint32_t*)((byte*)src + 16);
			*(uint64_t*)((byte*)dst + 8) = *(uint64_t*)((byte*)src + 8);
			*(uint64_t*)dst = *(uint64_t*)src;
		}
	}

	inline void Copy24(void* dst, void* src)
	{
		if (dst <= src)
		{
			*(uint64_t*)dst = *(uint64_t*)src;
			*(uint64_t*)((byte*)dst + 8) = *(uint64_t*)((byte*)src + 8);
			*(uint64_t*)((byte*)dst + 16) = *(uint64_t*)((byte*)src + 16);
		}
		else
		{
			*(uint64_t*)((byte*)dst + 16) = *(uint64_t*)((byte*)src + 16);
			*(uint64_t*)((byte*)dst + 8) = *(uint64_t*)((byte*)src + 8);
			*(uint64_t*)dst = *(uint64_t*)src;
		}
	}

	inline void Copy28(void* dst, void* src)
	{
		if (dst <= src)
		{
			*(uint64_t*)dst = *(uint64_t*)src;
			*(uint64_t*)((byte*)dst + 8) = *(uint64_t*)((byte*)src + 8);
			*(uint64_t*)((byte*)dst + 16) = *(uint64_t*)((byte*)src + 16);
			*(uint32_t*)((byte*)dst + 24) = *(uint32_t*)((byte*)src + 24);
		}
		else
		{
			*(uint32_t*)((byte*)dst + 24) = *(uint32_t*)((byte*)src + 24);
			*(uint64_t*)((byte*)dst + 16) = *(uint64_t*)((byte*)src + 16);
			*(uint64_t*)((byte*)dst + 8) = *(uint64_t*)((byte*)src + 8);
			*(uint64_t*)dst = *(uint64_t*)src;
		}
	}

	inline void Copy32(void* dst, void* src)
	{
		if (dst <= src)
		{
			*(uint64_t*)dst = *(uint64_t*)src;
			*(uint64_t*)((byte*)dst + 8) = *(uint64_t*)((byte*)src + 8);
			*(uint64_t*)((byte*)dst + 16) = *(uint64_t*)((byte*)src + 16);
			*(uint64_t*)((byte*)dst + 24) = *(uint64_t*)((byte*)src + 24);
		}
		else
		{
			*(uint64_t*)((byte*)dst + 24) = *(uint64_t*)((byte*)src + 24);
			*(uint64_t*)((byte*)dst + 16) = *(uint64_t*)((byte*)src + 16);
			*(uint64_t*)((byte*)dst + 8) = *(uint64_t*)((byte*)src + 8);
			*(uint64_t*)dst = *(uint64_t*)src;
		}
	}

	inline void CopyBySize(void* dst, void* src, uint32_t size)
	{
		switch (size)
		{
		case 1: Copy1(dst, src); break;
		case 2: Copy2(dst, src); break;
		case 4: Copy4(dst, src); break;
		case 8: Copy8(dst, src); break;
		case 12: Copy12(dst, src); break;
		case 16: Copy16(dst, src); break;
		default: std::memmove(dst, src, size); break;
		}
	}

	inline void InitDefault1(void* dst)
	{
		*(uint8_t*)dst = 0;
	}

	inline void InitDefault2(void* dst)
	{
		*(uint16_t*)dst = 0;
	}

	inline void InitDefault4(void* dst)
	{
		*(uint32_t*)dst = 0;
	}

	inline void InitDefault8(void* dst)
	{
		*(uint64_t*)dst = 0;
	}

	inline void InitDefault12(void* dst)
	{
		*(uint64_t*)dst = 0;
		*(uint32_t*)((byte*)dst + 8) = 0;
	}

	inline void InitDefault16(void* dst)
	{
		*(uint64_t*)dst = 0;
		*(uint64_t*)((byte*)dst + 8) = 0;
	}

	inline void InitDefault20(void* dst)
	{
		*(uint64_t*)dst = 0;
		*(uint64_t*)((byte*)dst + 8) = 0;
		*(uint32_t*)((byte*)dst + 16) = 0;
	}

	inline void InitDefault24(void* dst)
	{
		*(uint64_t*)dst = 0;
		*(uint64_t*)((byte*)dst + 8) = 0;
		*(uint64_t*)((byte*)dst + 16) = 0;
	}

	inline void InitDefault28(void* dst)
	{
		*(uint64_t*)dst = 0;
		*(uint64_t*)((byte*)dst + 8) = 0;
		*(uint64_t*)((byte*)dst + 16) = 0;
		*(uint32_t*)((byte*)dst + 24) = 0;
	}

	inline void InitDefault32(void* dst)
	{
		*(uint64_t*)dst = 0;
		*(uint64_t*)((byte*)dst + 8) = 0;
		*(uint64_t*)((byte*)dst + 16) = 0;
		*(uint64_t*)((byte*)dst + 24) = 0;
	}

	inline void InitDefaultN(void* dst, size_t size)
	{
		std::memset(dst, 0, size);
	}

	inline void SetConst1(void* dst, int8_t value)
	{
		*(int8_t*)dst = value;
	}

	inline void SetConst2(void* dst, int16_t value)
	{
		*(int16_t*)dst = value;
	}

	inline void SetConst4(void* dst, int32_t value)
	{
		*(int32_t*)dst = value;
	}

	inline void SetConst8(void* dst, int64_t value)
	{
		*(int64_t*)dst = value;
	}
}
}
