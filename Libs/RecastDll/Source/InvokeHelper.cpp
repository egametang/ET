#include "InvokeHelper.h"
#include "DetourNavMesh.h"
#include "DetourNavMeshQuery.h"
#include <cstring>
#include <unordered_map>
#include "DetourCommon.h"

static const int NAVMESHSET_MAGIC = 'M' << 24 | 'S' << 16 | 'E' << 8 | 'T'; //'MSET';
static const int NAVMESHSET_VERSION = 1;

struct NavMeshSetHeader
{
	int magic;
	int version;
	int numTiles;
	dtNavMeshParams params;
};

struct NavMeshTileHeader
{
	dtTileRef tileRef;
	int dataSize;
};

int32_t InitNav(const char* buffer, int32_t n, dtNavMesh*& navMesh)
{	
	int index = 0;
	// Read header.
	NavMeshSetHeader header;

	int count = sizeof(NavMeshSetHeader);
	if (index + count > n)
	{
		return -1;
	}
	memcpy(&header, buffer + index, count);
	index += count;

	if (header.magic != NAVMESHSET_MAGIC)
	{
		return -2;
	}
	if (header.version != NAVMESHSET_VERSION)
	{
		return -3;
	}

	dtNavMesh* mesh = dtAllocNavMesh();
	if (!mesh)
	{
		return -4;
	}
	dtStatus status = mesh->init(&header.params);
	if (dtStatusFailed(status))
	{
		return -5;
	}

	// Read tiles.
	for (int i = 0; i < header.numTiles; ++i)
	{
		NavMeshTileHeader tileHeader;

		count = sizeof(NavMeshTileHeader);
		if (index + count > n)
		{
			return -6;
		}
		memcpy(&tileHeader, buffer + index, count);
		index += count;

		if (!tileHeader.tileRef || !tileHeader.dataSize)
			break;

		unsigned char* data = (unsigned char*)dtAlloc(tileHeader.dataSize, DT_ALLOC_PERM);
		if (!data) break;
		memset(data, 0, tileHeader.dataSize);

		count = tileHeader.dataSize;
		if (index + count > n)
		{
			return -7;
		}
		memcpy(data, buffer + index, count);
		index += count;

		mesh->addTile(data, tileHeader.dataSize, DT_TILE_FREE_DATA, tileHeader.tileRef, 0);
	}
	navMesh = mesh;
	return 0;
}

static const int MAX_POLYS = 256;
static const int MAX_SMOOTH = 2048;

class NavMeshContex
{
public:
	dtNavMesh* navMesh;
	dtNavMeshQuery* navQuery;

	NavMeshContex()
	{
	}

	int32_t Init(const char* buffer, int32_t n)
	{
		int32_t ret = InitNav(buffer, n, navMesh);
		std::string s;
		
		if (ret != 0)
		{
			return -1;
		}
		
		navQuery = new dtNavMeshQuery();
		navQuery->init(navMesh, 2048);
		return 0;
	}
	
	~NavMeshContex()
	{
		if (navQuery != nullptr)
		{
			dtFreeNavMeshQuery(navQuery);
		}
		if (navMesh != nullptr)
		{
			dtFreeNavMesh(navMesh);
		}
	}
};

NavMesh* NavMesh::instance = nullptr;

NavMesh::NavMesh()
{
}

NavMesh* NavMesh::GetInstace()
{
	if (NavMesh::instance == nullptr)
	{
		NavMesh::instance = new NavMesh();
	}
	return NavMesh::instance;
}

NavMeshContex* NavMesh::New(int32_t id, const char* buffer, int32_t n)
{
	NavMeshContex* navMeshContex = new NavMeshContex();
	int32_t ret = navMeshContex->Init(buffer, n);
	
	if (ret != 0)
	{
		delete navMeshContex;
		return nullptr;
	}

	navMeshContexs[id] = navMeshContex;
	return navMeshContex;
}

NavMeshContex* NavMesh::Get(int32_t id)
{
	const auto it = navMeshContexs.find(id);
	if (it != navMeshContexs.end())
	{
		return it->second;
	}
	return nullptr;
}

void NavMesh::Clear()
{
	for (auto kv : navMeshContexs)
	{
		delete kv.second;
	}
	navMeshContexs.clear();
}

NavMeshContex* RecastLoad(int32_t id, const char* buffer, int32_t n)
{
	return NavMesh::GetInstace()->New(id, buffer, n);
}

NavMeshContex* RecastGet(int32_t id)
{
	return NavMesh::GetInstace()->Get(id);
}

void RecastClear()
{
	NavMesh::GetInstace()->Clear();
}

int32_t RecastFind(NavMeshContex* navMeshContex, float* extents, float* startPos, float* endPos, float* straightPath)
{
	//FILE* fp = fopen("./test.log", "wb");
	if (navMeshContex == nullptr)
	{
		return -1;
	}
	if (startPos == nullptr)
	{
		return -2;
	}
	if (endPos == nullptr)
	{
		return -3;
	}
	if (straightPath == nullptr)
	{
		return -4;
	}
	if (extents == nullptr)
	{
		return -5;
	}

	//char ss[200];
	//int nn = sprintf(ss, "startPos,%f,%f,%f\n", startPos[0], startPos[1], startPos[2]);
	//fwrite(ss, nn, 1, fp);
	//fflush(fp);

	dtPolyRef startRef = 0;
	dtPolyRef endRef = 0;
	float startNearestPt[3];
	float endNearestPt[3];

	dtQueryFilter filter;
	filter.setIncludeFlags(0xffff);
	filter.setExcludeFlags(0);
	
	navMeshContex->navQuery->findNearestPoly(startPos, extents, &filter, &startRef, startNearestPt);
	navMeshContex->navQuery->findNearestPoly(endPos, extents, &filter, &endRef, endNearestPt);
	
	dtPolyRef polys[MAX_POLYS];
	int npolys;
	unsigned char straightPathFlags[MAX_POLYS];
	dtPolyRef straightPathPolys[MAX_POLYS];
	int nstraightPath = 0;
	
	navMeshContex->navQuery->findPath(startRef, endRef, startNearestPt, endNearestPt, &filter, polys, &npolys, MAX_POLYS);

	if (npolys)
	{
		float epos1[3];
		dtVcopy(epos1, endNearestPt);

		if (polys[npolys - 1] != endRef)
		{
			navMeshContex->navQuery->closestPointOnPoly(polys[npolys - 1], endNearestPt, epos1, 0);
		}
		
		navMeshContex->navQuery->findStraightPath(startNearestPt, endNearestPt, polys, npolys, straightPath, straightPathFlags, straightPathPolys, &nstraightPath, MAX_POLYS, DT_STRAIGHTPATH_ALL_CROSSINGS);
	}
	
	return nstraightPath;
}

int32_t RecastFindNearestPoint(NavMeshContex* navMeshContex, float* extents, float* startPos, float* nearestPos)
{
	if (navMeshContex == nullptr)
	{
		return -1;
	}
	if (startPos == nullptr)
	{
		return -2;
	}
	if (nearestPos == nullptr)
	{
		return -3;
	}
	if (extents == nullptr)
	{
		return -5;
	}

	dtPolyRef startRef = 0;

	dtQueryFilter filter;
	filter.setIncludeFlags(0xffff);
	filter.setExcludeFlags(0);

	navMeshContex->navQuery->findNearestPoly(startPos, extents, &filter, &startRef, nearestPos);
	return startRef;
}

static float frand()
{
	//	return ((float)(rand() & 0xffff)/(float)0xffff);
	return (float)rand() / (float)RAND_MAX;
}

int32_t RecastFindRandomPoint(NavMeshContex* navMeshContex, float* pos)
{
	if (navMeshContex == nullptr)
	{
		return -1;
	}
	if (pos == nullptr)
	{
		return -2;
	}
	dtQueryFilter filter;
	filter.setIncludeFlags(0xffff);
	filter.setExcludeFlags(0);

	dtPolyRef startRef = 0;
	return navMeshContex->navQuery->findRandomPoint(&filter, frand, &startRef, pos);
}

int32_t RecastFindRandomPointAroundCircle(NavMeshContex* navMeshContex, float* extents, const float* centerPos, const float maxRadius, float* pos)
{
	if (navMeshContex == nullptr)
	{
		return -1;
	}
	if (pos == nullptr)
	{
		return -2;
	}
	dtQueryFilter filter;
	filter.setIncludeFlags(0xffff);
	filter.setExcludeFlags(0);

	dtPolyRef startRef = 0;
	dtPolyRef randomRef = 0;
	float startNearestPt[3];
	navMeshContex->navQuery->findNearestPoly(centerPos, extents, &filter, &startRef, startNearestPt);
	return navMeshContex->navQuery->findRandomPointAroundCircle(startRef, centerPos, maxRadius, &filter, frand, &randomRef, pos);
}
