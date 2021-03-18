#pragma once

#include <map>

#include "../../../../Detour/Include/DetourNavMesh.h"
#include "../../../../Detour/Include/DetourNavMeshQuery.h"

static const int MAX_POLYS = 256;
static const int MAX_SMOOTH = 2048;

class CRecast
{
	dtNavMesh* m_navMesh;
	dtNavMeshQuery* m_navQuery;

	float m_spos[3];
	float m_epos[3];
	dtPolyRef m_startRef, m_endRef;
	float m_polyPickExt[3] = { 2,40,2 };
	dtQueryFilter m_filter;

	dtPolyRef m_polys[MAX_POLYS];
	int m_npolys = 0;

	float m_smoothPath[MAX_SMOOTH * 3];
	int m_nsmoothPath;
	float m_fixPos[3];
	float m_hitPos[3];

public:

	CRecast();

	bool LoadMap(const char* path);
	void FreeMap();

	dtStatus FindPath(const float* spos, const float* epos);

	dtStatus Raycast(const float* spos, const float* epos);

	// stepSize : 如果给0，会自动变成0.5f
	// slop : 如果给0，会自动变成0.01f
	void Smooth(float STEP_SIZE, float SLOP);

	int GetCountPoly() { return m_npolys; }
	int GetCountSmooth() { return m_nsmoothPath; }

	dtPolyRef* GetPathPoly(int* polyCount) { *polyCount = m_npolys; return m_polys; }
	float* GetPathSmooth(int* smoothCount) { *smoothCount = m_nsmoothPath; return m_smoothPath; }

	float* fixPosition(const float* pos);
	float* GetHitPosition() { return m_hitPos; }
};

class CRecastHelper
{
	std::map<int, CRecast*> m_mapRecast;

public:
	CRecastHelper() {}
	~CRecastHelper();

	CRecast* Get(int id) { return m_mapRecast[id]; }

	bool LoadMap(int id, const char* path);
	void FreeMap(int id);

};