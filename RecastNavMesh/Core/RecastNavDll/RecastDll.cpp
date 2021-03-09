#include "RecastDll.h"

#include "CRecastHelper.h"


extern "C"
{
	CRecastHelper* _helper = NULL;

	DllExport bool recast_init()
	{
		_helper = new CRecastHelper();
		return true;
	}

	void recast_fini()
	{
		if (_helper)
			delete _helper;
		_helper = NULL;
	}

	bool recast_loadmap(int id, const char* path)
	{
		if (!_helper) return false;
		if (!_helper->LoadMap(id, path))
			return false;
		return true;
	}

	bool recast_freemap(int id)
	{
		if (!_helper || !_helper->Get(id)) return false;
		_helper->FreeMap(id);
		return true;
	}

	int recast_findpath(int id, const float* spos, const float* epos)
	{
		if (!_helper || !_helper->Get(id)) return 0;
		dtStatus status = _helper->Get(id)->FindPath(spos, epos);
		return status;
	}
	bool recast_smooth(int id, float step_size, float slop)
	{
		if (!_helper || !_helper->Get(id)) return false;
		_helper->Get(id)->Smooth(step_size, slop);
		return true;
	}

	int recast_raycast(int id, const float* spos, const float* epos)
	{
		if (!_helper || !_helper->Get(id)) return 0;
		dtStatus status = _helper->Get(id)->Raycast(spos, epos);
        return status;
	}

    float* recast_gethitposition(int id)
	{
		if (!_helper || !_helper->Get(id)) return NULL;
		return _helper->Get(id)->GetHitPosition();
	}

	int recast_getcountpoly(int id)
	{
		if (!_helper || !_helper->Get(id)) return 0;
		return _helper->Get(id)->GetCountPoly();
	}

	int recast_getcountsmooth(int id)
	{
		if (!_helper || !_helper->Get(id)) return 0;
		return _helper->Get(id)->GetCountSmooth();
	}

	unsigned int* recast_getpathpoly(int id)
	{
		if (!_helper || !_helper->Get(id)) return NULL;

		int count = 0;
		unsigned int* polys = _helper->Get(id)->GetPathPoly(&count);
		if (count == 0)
			return NULL;
		return polys;
	}

	float* recast_getpathsmooth(int id)
	{
		if (!_helper || !_helper->Get(id)) return NULL;

		int count = 0;
		float* polys = _helper->Get(id)->GetPathSmooth(&count);
		if (count == 0)
			return NULL;
		return polys;
	}

	float* recast_getfixposition(int id, const float* pos)
	{
        if (!_helper || !_helper->Get(id)) return NULL;
        float* fixPos = _helper->Get(id)->fixPosition(pos);
        return fixPos;
	}
}

