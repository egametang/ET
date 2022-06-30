#include "CommonDef.h"

#include "metadata/AOTHomologousImage.h"
#include "HuatuoConfig.h"

extern "C"
{

	IL2CPP_EXPORT int32_t HuatuoApi_LoadMetadataForAOTAssembly(void* dllBytes, uint32_t dllSize)
	{
		return huatuo::metadata::AOTHomologousImage::LoadMetadataForAOTAssembly(dllBytes, dllSize);
	}

	IL2CPP_EXPORT uint32_t HuatuoApi_GetInterpreterThreadObjectStackSize()
	{
		return huatuo::HuatuoConfig::GetInterpreterThreadObjectStackSize();
	}

	IL2CPP_EXPORT void HuatuoApi_SetInterpreterThreadObjectStackSize(uint32_t size)
	{
		huatuo::HuatuoConfig::SetInterpreterThreadObjectStackSize(size);
	}

	IL2CPP_EXPORT uint32_t HuatuoApi_GetInterpreterThreadFrameStackSize()
	{
		return huatuo::HuatuoConfig::GetInterpreterThreadFrameStackSize();
	}

	IL2CPP_EXPORT void HuatuoApi_SetInterpreterThreadFrameStackSize(uint32_t size)
	{
		huatuo::HuatuoConfig::SetInterpreterThreadFrameStackSize(size);
	}
}
