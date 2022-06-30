#include "HuatuoConfig.h"

namespace huatuo
{
	HuatuoConfig HuatuoConfig::_ins;

	uint32_t HuatuoConfig::s_threadObjectStackSize = 1024 * 128;
	uint32_t HuatuoConfig::s_threadFrameStackSize = 1024 * 2;
}