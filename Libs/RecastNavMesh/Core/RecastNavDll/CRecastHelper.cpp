#include "CRecastHelper.h"

#include <iostream>
#include <string>


#include "../../../../Detour/Include/DetourCommon.h"
#include "../../../../Detour/Include/DetourNavMesh.h"
#include "../../../../Detour/Include/DetourNavMeshQuery.h"
#include "../../../../Recast/Include/Recast.h"


using namespace std;

/////////////////////////////////////////////////
//	local functions
/////////////////////////////////////////////////

enum SamplePolyFlags
{
	SAMPLE_POLYFLAGS_WALK = 0x01,		// Ability to walk (ground, grass, road)
	SAMPLE_POLYFLAGS_SWIM = 0x02,		// Ability to swim (water).
	SAMPLE_POLYFLAGS_DOOR = 0x04,		// Ability to move through doors.
	SAMPLE_POLYFLAGS_JUMP = 0x08,		// Ability to jump.
	SAMPLE_POLYFLAGS_DISABLED = 0x10,		// Disabled polygon
	SAMPLE_POLYFLAGS_ALL = 0xffff	// All abilities.
};

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

dtNavMesh* loadMap(const char* path)
{
	FILE* fp = NULL;
	errno_t error = fopen_s(&fp, path, "rb");
	if (!fp) return 0;

	// Read header.
	NavMeshSetHeader header;
	size_t readLen = fread(&header, sizeof(NavMeshSetHeader), 1, fp);
	if (readLen != 1)
	{
		fclose(fp);
		return 0;
	}
	if (header.magic != NAVMESHSET_MAGIC)
	{
		fclose(fp);
		return 0;
	}
	if (header.version != NAVMESHSET_VERSION)
	{
		fclose(fp);
		return 0;
	}

	dtNavMesh* mesh = dtAllocNavMesh();
	if (!mesh)
	{
		fclose(fp);
		return 0;
	}
	dtStatus status = mesh->init(&header.params);
	if (dtStatusFailed(status))
	{
		fclose(fp);
		return 0;
	}

	// Read tiles.
	for (int i = 0; i < header.numTiles; ++i)
	{
		NavMeshTileHeader tileHeader;
		readLen = fread(&tileHeader, sizeof(tileHeader), 1, fp);
		if (readLen != 1)
		{
			fclose(fp);
			return 0;
		}

		if (!tileHeader.tileRef || !tileHeader.dataSize)
			break;

		unsigned char* data = (unsigned char*)dtAlloc(tileHeader.dataSize, DT_ALLOC_PERM);
		if (!data) break;
		memset(data, 0, tileHeader.dataSize);
		readLen = fread(data, tileHeader.dataSize, 1, fp);
		if (readLen != 1)
		{
			dtFree(data);
			fclose(fp);
			return 0;
		}

		mesh->addTile(data, tileHeader.dataSize, DT_TILE_FREE_DATA, tileHeader.tileRef, 0);
	}

	fclose(fp);

	return mesh;
}

inline bool inRange(const float* v1, const float* v2, const float r, const float h)
{
	const float dx = v2[0] - v1[0];
	const float dy = v2[1] - v1[1];
	const float dz = v2[2] - v1[2];
	return (dx * dx + dz * dz) < r * r && fabsf(dy) < h;
}

static int fixupCorridor(dtPolyRef* path, const int npath, const int maxPath,
	const dtPolyRef* visited, const int nvisited)
{
	int furthestPath = -1;
	int furthestVisited = -1;

	// Find furthest common polygon.
	for (int i = npath - 1; i >= 0; --i)
	{
		bool found = false;
		for (int j = nvisited - 1; j >= 0; --j)
		{
			if (path[i] == visited[j])
			{
				furthestPath = i;
				furthestVisited = j;
				found = true;
			}
		}
		if (found)
			break;
	}

	// If no intersection found just return current path. 
	if (furthestPath == -1 || furthestVisited == -1)
		return npath;

	// Concatenate paths.	

	// Adjust beginning of the buffer to include the visited.
	const int req = nvisited - furthestVisited;
	const int orig = rcMin(furthestPath + 1, npath);
	int size = rcMax(0, npath - orig);
	if (req + size > maxPath)
		size = maxPath - req;
	if (size)
		memmove(path + req, path + orig, size * sizeof(dtPolyRef));

	// Store visited
	for (int i = 0; i < req; ++i)
		path[i] = visited[(nvisited - 1) - i];

	return req + size;
}

// This function checks if the path has a small U-turn, that is,
// a polygon further in the path is adjacent to the first polygon
// in the path. If that happens, a shortcut is taken.
// This can happen if the target (T) location is at tile boundary,
// and we're (S) approaching it parallel to the tile edge.
// The choice at the vertex can be arbitrary, 
//  +---+---+
//  |:::|:::|
//  +-S-+-T-+
//  |:::|   | <-- the step can end up in here, resulting U-turn path.
//  +---+---+
static int fixupShortcuts(dtPolyRef* path, int npath, dtNavMeshQuery* navQuery)
{
	if (npath < 3)
		return npath;

	// Get connected polygons
	static const int maxNeis = 16;
	dtPolyRef neis[maxNeis];
	int nneis = 0;

	const dtMeshTile* tile = 0;
	const dtPoly* poly = 0;
	if (dtStatusFailed(navQuery->getAttachedNavMesh()->getTileAndPolyByRef(path[0], &tile, &poly)))
		return npath;

	for (unsigned int k = poly->firstLink; k != DT_NULL_LINK; k = tile->links[k].next)
	{
		const dtLink* link = &tile->links[k];
		if (link->ref != 0)
		{
			if (nneis < maxNeis)
				neis[nneis++] = link->ref;
		}
	}

	// If any of the neighbour polygons is within the next few polygons
	// in the path, short cut to that polygon directly.
	static const int maxLookAhead = 6;
	int cut = 0;
	for (int i = dtMin(maxLookAhead, npath) - 1; i > 1 && cut == 0; i--) {
		for (int j = 0; j < nneis; j++)
		{
			if (path[i] == neis[j]) {
				cut = i;
				break;
			}
		}
	}
	if (cut > 1)
	{
		int offset = cut - 1;
		npath -= offset;
		for (int i = 1; i < npath; i++)
			path[i] = path[i + offset];
	}

	return npath;
}

static bool getSteerTarget(dtNavMeshQuery* navQuery, const float* startPos, const float* endPos,
	const float minTargetDist,
	const dtPolyRef* path, const int pathSize,
	float* steerPos, unsigned char& steerPosFlag, dtPolyRef& steerPosRef,
	float* outPoints = 0, int* outPointCount = 0)
{
	// Find steer target.
	static const int MAX_STEER_POINTS = 3;
	float steerPath[MAX_STEER_POINTS * 3];
	unsigned char steerPathFlags[MAX_STEER_POINTS];
	dtPolyRef steerPathPolys[MAX_STEER_POINTS];
	int nsteerPath = 0;
	navQuery->findStraightPath(startPos, endPos, path, pathSize,
		steerPath, steerPathFlags, steerPathPolys, &nsteerPath, MAX_STEER_POINTS);
	if (!nsteerPath)
		return false;

	if (outPoints && outPointCount)
	{
		*outPointCount = nsteerPath;
		for (int i = 0; i < nsteerPath; ++i)
			dtVcopy(&outPoints[i * 3], &steerPath[i * 3]);
	}


	// Find vertex far enough to steer to.
	int ns = 0;
	while (ns < nsteerPath)
	{
		// Stop at Off-Mesh link or when point is further than slop away.
		if ((steerPathFlags[ns] & DT_STRAIGHTPATH_OFFMESH_CONNECTION) ||
			!inRange(&steerPath[ns * 3], startPos, minTargetDist, 1000.0f))
			break;
		ns++;
	}
	// Failed to find good point to steer to.
	if (ns >= nsteerPath)
		return false;

	dtVcopy(steerPos, &steerPath[ns * 3]);
	steerPos[1] = startPos[1];
	steerPosFlag = steerPathFlags[ns];
	steerPosRef = steerPathPolys[ns];

	return true;
}

/////////////////////////////////////////////////
//	class CRecast
/////////////////////////////////////////////////

CRecast::CRecast()
{
	m_navMesh = NULL;
	m_navQuery = NULL;
}

bool CRecast::LoadMap(const char* path)
{
	m_navMesh = loadMap(path);
	m_navQuery = dtAllocNavMeshQuery();
	if (!m_navMesh)
		return false;
	m_navQuery->init(m_navMesh, 2048);
	return true;
}

void CRecast::FreeMap()
{
	dtFreeNavMeshQuery(m_navQuery);
}

unsigned int DT_COORD_INVALID = 1 << 13; // 新增错误码：传入坐标错误

/// <summary>
/// 寻路
/// </summary>
/// <param name="spos">float[3] 起点三维坐标</param>
/// <param name="epos">float[3] 终点三维坐标</param>
/// <returns>寻路的结果</returns>
dtStatus CRecast::FindPath(const float* spos, const float* epos)
{
	dtVcopy(m_spos, spos);
	dtVcopy(m_epos, epos);

	m_filter.setIncludeFlags(SAMPLE_POLYFLAGS_ALL ^ SAMPLE_POLYFLAGS_DISABLED);
	m_filter.setExcludeFlags(0);

	m_navQuery->findNearestPoly(spos, m_polyPickExt, &m_filter, &m_startRef, 0);
	m_navQuery->findNearestPoly(epos, m_polyPickExt, &m_filter, &m_endRef, 0);

	if (!m_startRef || !m_endRef)
	{
		return DT_FAILURE| DT_COORD_INVALID;
	}

	// 开始寻路
	dtStatus status = m_navQuery->findPath(m_startRef, m_endRef, spos, epos, &m_filter, m_polys, &m_npolys, MAX_POLYS);
	return status;
}

dtStatus CRecast::Raycast(const float* spos, const float* epos)
{
	dtVcopy(m_spos, spos);
	dtVcopy(m_epos, epos);

	m_filter.setIncludeFlags(SAMPLE_POLYFLAGS_ALL ^ SAMPLE_POLYFLAGS_DISABLED);
	m_filter.setExcludeFlags(0);

	m_navQuery->findNearestPoly(spos, m_polyPickExt, &m_filter, &m_startRef, 0);
	m_navQuery->findNearestPoly(epos, m_polyPickExt, &m_filter, &m_endRef, 0);


	if (!m_startRef || !m_endRef)
	{
		return DT_FAILURE | DT_COORD_INVALID;
	}

	float hit_param = FLT_MAX;
	float hit_normal[3] = {0.0f};
	memset(&m_npolys, 0, sizeof(dtPolyRef) * MAX_POLYS);
	m_npolys = 0;

	//  float target[3] = {m_epos[0], m_epos[1], m_epos[2]};
	dtPolyRef target_ref;
	float nearest[3] = {0};
	dtStatus status = m_navQuery->findNearestPoly(
	m_epos, m_polyPickExt, &m_filter, &target_ref, nearest);
	if (dtStatusFailed(status)) return false;
	if (!m_navQuery->isValidPolyRef(target_ref, &m_filter)) return false;
	// fix 目标点高度
	m_epos[1] = nearest[1];

	// 开始射线检测
	status = m_navQuery->raycast(m_startRef, m_spos, m_epos, &m_filter, &hit_param, hit_normal, m_polys, &m_npolys, MAX_POLYS);
	if (dtStatusFailed(status)) return status;

	//计算碰撞点逻辑
	if (0.0f < hit_param && hit_param < 1.0f)
	{
		float delta[3];
		for (int i = 0; i < 3; ++i)
		{
			delta[i] = m_epos[i] - m_spos[i];
			m_hitPos[i] = m_spos[i] + delta[i] * hit_param;
		}
	}
	else if (0.000001f > hit_param)
	{
		memcpy(m_hitPos, m_spos, sizeof(m_hitPos));
	}
	else
	{
		memcpy(m_hitPos, m_epos, sizeof(m_hitPos));
	}

	return status;
}

void CRecast::Smooth(float STEP_SIZE, float SLOP)
{
	m_nsmoothPath = 0;
	if (m_npolys)
	{
		// Iterate over the path to find smooth path on the detail mesh surface.
		dtPolyRef polys[MAX_POLYS];
		memcpy(polys, m_polys, sizeof(dtPolyRef) * m_npolys);
		int npolys = m_npolys;

		float iterPos[3], targetPos[3];
		m_navQuery->closestPointOnPoly(m_startRef, m_spos, iterPos, 0);
		m_navQuery->closestPointOnPoly(polys[npolys - 1], m_epos, targetPos, 0);

		if(STEP_SIZE == 0) STEP_SIZE = 0.5f;
		if(SLOP == 0) SLOP = 0.01f;

		m_nsmoothPath = 0;

		dtVcopy(&m_smoothPath[m_nsmoothPath * 3], iterPos);
		m_nsmoothPath++;

		// Move towards target a small advancement at a time until target reached or
		// when ran out of memory to store the path.
		while (npolys && m_nsmoothPath < MAX_SMOOTH)
		{
			// Find location to steer towards.
			float steerPos[3];
			unsigned char steerPosFlag;
			dtPolyRef steerPosRef;

			if (!getSteerTarget(m_navQuery, iterPos, targetPos, SLOP,
				polys, npolys, steerPos, steerPosFlag, steerPosRef))
				break;

			bool endOfPath = (steerPosFlag & DT_STRAIGHTPATH_END) ? true : false;
			bool offMeshConnection = (steerPosFlag & DT_STRAIGHTPATH_OFFMESH_CONNECTION) ? true : false;

			// Find movement delta.
			float delta[3], len;
			dtVsub(delta, steerPos, iterPos);
			len = dtMathSqrtf(dtVdot(delta, delta));
			// If the steer target is end of path or off-mesh link, do not move past the location.
			if ((endOfPath || offMeshConnection) && len < STEP_SIZE)
				len = 1;
			else
				len = STEP_SIZE / len;
			float moveTgt[3];
			dtVmad(moveTgt, iterPos, delta, len);

			// Move
			float result[3];
			dtPolyRef visited[16];
			int nvisited = 0;
			m_navQuery->moveAlongSurface(polys[0], iterPos, moveTgt, &m_filter,
				result, visited, &nvisited, 16);

			npolys = fixupCorridor(polys, npolys, MAX_POLYS, visited, nvisited);
			npolys = fixupShortcuts(polys, npolys, m_navQuery);

			// BUG FIX:原来是h=0, 这里改为目的地的高度，否则如果getPolyHeight()没有得到数值（这是有可能的），则高度为0，就错了。Aug.2.2020. Liu Gang.
			// 原因：如果这里的getPolyHeight()获取失败，则h就为零了，而下一个循环，就会在上面的getSteerTarget()中结束，这里就是最后一个节点，高度为零
			float h = result[1];
			m_navQuery->getPolyHeight(polys[0], result, &h);
			result[1] = h;
			dtVcopy(iterPos, result);

			// Handle end of path and off-mesh links when close enough.
			if (endOfPath && inRange(iterPos, steerPos, SLOP, 1.0f))
			{
				// Reached end of path.
				dtVcopy(iterPos, targetPos);
				if (m_nsmoothPath < MAX_SMOOTH)
				{
					dtVcopy(&m_smoothPath[m_nsmoothPath * 3], iterPos);
					m_nsmoothPath++;
				}
				break;
			}
			else if (offMeshConnection && inRange(iterPos, steerPos, SLOP, 1.0f))
			{
				// Reached off-mesh connection.
				float startPos[3], endPos[3];

				// Advance the path up to and over the off-mesh connection.
				dtPolyRef prevRef = 0, polyRef = polys[0];
				int npos = 0;
				while (npos < npolys && polyRef != steerPosRef)
				{
					prevRef = polyRef;
					polyRef = polys[npos];
					npos++;
				}
				for (int i = npos; i < npolys; ++i)
					polys[i - npos] = polys[i];
				npolys -= npos;

				// Handle the connection.
				dtStatus status = m_navMesh->getOffMeshConnectionPolyEndPoints(prevRef, polyRef, startPos, endPos);
				if (dtStatusSucceed(status))
				{
					if (m_nsmoothPath < MAX_SMOOTH)
					{
						dtVcopy(&m_smoothPath[m_nsmoothPath * 3], startPos);
						m_nsmoothPath++;
						// Hack to make the dotted path not visible during off-mesh connection.
						if (m_nsmoothPath & 1)
						{
							dtVcopy(&m_smoothPath[m_nsmoothPath * 3], startPos);
							m_nsmoothPath++;
						}
					}
					// Move position at the other side of the off-mesh link.
					dtVcopy(iterPos, endPos);
					float eh = 0.0f;
					m_navQuery->getPolyHeight(polys[0], iterPos, &eh);
					iterPos[1] = eh;
				}
			}

			// Store results.
			if (m_nsmoothPath < MAX_SMOOTH)
			{
				dtVcopy(&m_smoothPath[m_nsmoothPath * 3], iterPos);
				m_nsmoothPath++;
			}
		}
	}
}

float* CRecast::fixPosition(const float* pos) {
	dtPolyRef posRef;
    m_filter.setIncludeFlags(SAMPLE_POLYFLAGS_ALL ^ SAMPLE_POLYFLAGS_DISABLED);
    m_filter.setExcludeFlags(0);  
	float nearest[3] = {0};

	dtStatus status = m_navQuery->findNearestPoly(pos, m_polyPickExt, &m_filter,
												&posRef, nearest);
	if (dtStatusFailed(status)) return nullptr;
	if (!posRef) return nullptr; // 有时候返回成功，但是也没有找到对应的多边形，实际上还是出错了
	memcpy(m_fixPos, nearest, sizeof(m_fixPos));
	return m_fixPos;
	//float h = 0;
	//status = m_navQuery->getPolyHeight(posRef, nearest, &h);
	//if (!dtStatusFailed(status))
	//{
	//	m_fixPos[1] = h;
	//}

	//return m_fixPos;
}

/////////////////////////////////////////////////
//	class CRecastHelper
/////////////////////////////////////////////////

CRecastHelper::~CRecastHelper()
{
	for (map<int, CRecast*>::iterator iter = m_mapRecast.begin(); iter != m_mapRecast.end(); iter++)
	{
		delete iter->second;
	}
	m_mapRecast.clear();
}

bool CRecastHelper::LoadMap(int id, const char* path)
{
	CRecast* recast = new CRecast();
	bool ret = recast->LoadMap(path);
	if (!ret)
		return false;
	m_mapRecast[id] = recast;
	return true;
}
void CRecastHelper::FreeMap(int id)
{
	m_mapRecast[id]->FreeMap();
	delete m_mapRecast[id];
	m_mapRecast.erase(id);
}

