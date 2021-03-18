#pragma once

#define DllExport  __declspec(dllexport)

extern "C"
{
	/// <summary>
	/// 初始化Recast引擎
	/// </summary>
	/// <returns></returns>
	DllExport bool recast_init();

	/// <summary>
	/// 结束化Recast引擎
	/// </summary>
	/// <returns></returns>
	DllExport void recast_fini();

	/// <summary>
	/// 加载地图数据，支持同时打开多张地图，用id来查找
	/// </summary>
	/// <param name="id">地图Id</param>
	/// <param name="path">地图文件名（含路径）</param>
	/// <returns></returns>
	DllExport bool recast_loadmap(int id, const char* path);

	/// <summary>
	/// 释放地图数据
	/// </summary>
	/// <param name="id">地图Id</param>
	/// <returns></returns>
	DllExport bool recast_freemap(int id);

	/// <summary>
	/// 寻路，寻路的结果其实只是返回从起点到终点之间所有经过的凸多边形的序号
	/// </summary>
	/// <param name="id">地图Id</param>
	/// <param name="spos">起点坐标，float[3]</param>
	/// <param name="epos">终点坐标，float[3]</param>
	/// <returns>返回寻路结果：1u << 30 - 成功；1u << 31 - 失败；1u << 29 - 寻路未完成，详细内容见：DetourStatus.h</returns>
	DllExport int recast_findpath(int id, const float* spos, const float* epos);

	/// <summary>
	/// 计算平滑路径，其实是根据findpath得到的【从起点到终点所经过的凸多边形的序号】，得到真正的路径（三维坐标），所以这一步是不可缺少的
	/// </summary>
	/// <param name="id">地图Id</param>
	/// <param name="step_size">平滑参数，步长，越小越平滑，如果给0，则自动变为0.5</param>
	/// <param name="slop">用途不明（但肯定不影响平滑），如果给0，则自动变为0.01</param>
	/// <returns></returns>
	DllExport bool recast_smooth(int id, float step_size, float slop);

	/// <summary>
    /// 射线检测，从起始位置向终点位置发射一个射线，中间遇到阻挡停止，并返回阻挡Poly
    /// </summary>
	/// <param name="id">地图Id</param>
	/// <param name="spos">起点坐标，float[3]</param>
    /// <param name="epos">终点坐标，float[3]</param>
    /// <returns>返回寻路结果：1u << 30 - 成功；1u << 31 - 失败；1u << 29 -
    /// 寻路未完成，详细内容见：DetourStatus.h</returns>
    DllExport int recast_raycast(int id, const float* spos, const float* epos);

	/// <summary>
    /// 获取射线检测的碰撞点，配合recast_raycast()函数使用
    /// </summary>
	/// <param name="id">地图Id</param>
	/// <returns>返回碰撞点坐标</returns>
    DllExport float* recast_gethitposition(int id);

	/// <summary>
	/// 寻路后，得到路线经过的凸多边形id的个数
	/// </summary>
	/// <param name="id">地图Id</param>
	/// <returns></returns>
	DllExport int recast_getcountpoly(int id);

	/// <summary>
	/// 寻路后，得到具体的路径节点的个数
	/// </summary>
	/// <param name="id">地图Id</param>
	/// <returns></returns>
	DllExport int recast_getcountsmooth(int id);

	/// <summary>
	/// 得到pathfind以后，从起点到终点所经过的所有凸多边形id的序列
	/// </summary>
	/// <param name="id">地图Id</param>
	/// <returns>得到凸多边形id的序列</returns>
	DllExport unsigned int* recast_getpathpoly(int id);

	/// <summary>
	/// 得到smooth以后，路线的三维坐标序列
	/// </summary>
	/// <param name="id">地图Id</param>
	/// <returns>得到寻路坐标序列，每（x,y,z）三个数值为一个单元，所以实际返回的数量是smoothCount的3倍</returns>
	DllExport float* recast_getpathsmooth(int id);

	/// <summary>
    /// 修正坐标
    /// </summary>
	/// <param name="id">地图Id</param>
	/// <param name="spos">坐标，float[3]</param>
    /// <returns>得到修复后的坐标点</returns>
    DllExport float* recast_getfixposition(int id, const float* pos);
}