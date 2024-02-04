#ifndef INVOKE_HELPER_H
#define INVOKE_HELPER_H

#if !RECASTNAVIGATION_STATIC && WIN32
#define RECAST_DLL _declspec(dllexport)
#else
#define RECAST_DLL
#endif
#include "DetourNavMesh.h"
#include <cstdint>
#include <string>
#include <map>

#ifdef __cplusplus
extern "C" {
#endif
	
class NavMeshContex;
RECAST_DLL NavMeshContex* RecastLoad(const char* buffer, int32_t n);
RECAST_DLL void RecastClear(NavMeshContex* navMeshContex);
RECAST_DLL int32_t RecastFind(NavMeshContex* navMeshContex, float* extents, float* startPos, float* endPos, float* straightPath);
RECAST_DLL int32_t RecastFindNearestPoint(NavMeshContex* navMeshContex, float* extents, float* pos, float* nearestPos);
RECAST_DLL int32_t RecastFindRandomPoint(NavMeshContex* navMeshContex, float* pos);
RECAST_DLL int32_t RecastFindRandomPointAroundCircle(NavMeshContex* navMeshContex, float* extents, const float* centerPos, const float maxRadius, float* pos);
	

#ifdef __cplusplus
}
#endif


int32_t InitNav(const char* buffer, int32_t n, dtNavMesh*& navMesh);

#endif
