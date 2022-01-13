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
RECAST_DLL NavMeshContex* RecastGet(int32_t id);
RECAST_DLL NavMeshContex* RecastLoad(int32_t id, const char* buffer, int32_t n);
RECAST_DLL void RecastClear();
RECAST_DLL int32_t RecastFind(NavMeshContex* navMeshContex, float* extents, float* startPos, float* endPos, float* straightPath);
RECAST_DLL int32_t RecastFindNearestPoint(NavMeshContex* navMeshContex, float* extents, float* pos, float* nearestPos);
RECAST_DLL int32_t RecastFindRandomPoint(NavMeshContex* navMeshContex, float* pos);
RECAST_DLL int32_t RecastFindRandomPointAroundCircle(NavMeshContex* navMeshContex, float* extents, const float* centerPos, const float maxRadius, float* pos);
	

#ifdef __cplusplus
}
#endif


int32_t InitNav(const char* buffer, int32_t n, dtNavMesh*& navMesh);


class NavMesh
{
public:
	static NavMesh* instance;

	static NavMesh* GetInstace();

	std::map<int32_t, NavMeshContex*> navMeshContexs;

	NavMeshContex* New(int32_t id, const char* buffer, int32_t n);

	NavMeshContex* Get(int32_t id);

	void Clear();

private:
	NavMesh();
};



#endif
