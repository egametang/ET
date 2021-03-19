/////////////////////////////////////////////////
//
//	使用RecastNavigation库进行寻路的测试程序
//  Written by: Liu Gang. July.11.2020. 
//
/////////////////////////////////////////////////

#include <iostream>
#include <ostream>
#include <stdio.h>
#include <string.h>
#include <string>


#include "tools.h"
#include "../RecastNavDll/RecastDll.h"

std::string _strLastError;

int DoRecast(std::string mapPathName, std::string posFrom, std::string posTo);

int main(int argc, char* argv[])
{
	std::cout << "共有" << argc << "个参数" << std::endl;
	for (int i = 0; i < argc; i++)
	{
		std::cout << argv[i] << std::endl;
	}
	if (argc == 2 && _stricmp(argv[1], "/?") == 0)
	{
		printf_s("Recast工具 参数说明: ");
		printf_s("       Recast [寻路文件名, 含路径] [(起点坐标:) x,y] [(终点坐标:) x,y]");
		printf_s("       举例：");

		printf_s("       Recast solo_navmesh.bin 33.07,13.46 1103.89,478.21");
		return 0;
	}
	if (argc == 5) //第一个参数是可执行文件自身，第二个参数是Recast标志，第三个参数是文件名，第四个参数是起点，第五个参数是终点
	{
		int ret = DoRecast(argv[2], argv[3], argv[4]);
		if (ret == 0)
		{
			return 0;
		}
		else
		{
			std::string strRet = std::to_string(ret);
			std::string error = "Recast工具 错误(" + strRet + ") - " + _strLastError;
			printf_s(error.c_str());
			return ret;
		}
	}
	printf_s("Recast工具 参数错误(-1)！请使用 /? 查看参数说明。");
	return -1;
}

int Find2(int id, std::string mapPathName, const std::string posFrom, const std::string posTo);

int DoRecast(const std::string mapPathName, const std::string posFrom, const std::string posTo)
{
	// 1，初始化
	if (!recast_init())
	{
		_strLastError = "Recast 初始化失败！";
		return -11;
	}

	// 2，加载地图101
	int id1 = 101;
	if (!recast_loadmap(id1, mapPathName.c_str()))
	{
		_strLastError = "地图创建失败！- " + mapPathName;
		return -12;
	}

	// 2，加载地图102
	int id2 = 102;
	if (!recast_loadmap(id2, mapPathName.c_str()))
	{
		_strLastError = "地图创建失败！- " + mapPathName;
		return -12;
	}

	// 3，地图101开始寻路
	printf_s("寻路开始...\n");
	int ret = Find2(id1, mapPathName, posFrom, posTo);
	if (ret < 0)
	{
		printf_s(_strLastError.c_str());
	}
	printf_s("...寻路结束 - ret:%d\n\n", ret);

	// 3，地图102开始寻路
	printf_s("寻路开始...\n");
	ret = Find2(id2, mapPathName, posFrom, posTo);
	if (ret < 0)
	{
		printf_s(_strLastError.c_str());
	}
	printf_s("...寻路结束 - ret:%d\n\n", ret);

	// 4，释放地图
	recast_freemap(id1);
	recast_freemap(id2);

	// 5，释放Recast引擎
	recast_fini();

	printf_s("Recast 寻路成功！");
	return 0;
}

int Find2(int id, std::string mapPathName, const std::string posFrom, const std::string posTo)
{
	Tools::dtStatus status;
	
	std::vector<std::string> strsFrom = Tools::split(posFrom, ",");
	std::vector<std::string> strsTo = Tools::split(posTo, ",");

	//注意在Unity中因为坐标系原因，x需要为相反数
	float spos[3] = {stof(strsFrom[0]), stof(strsFrom[1]), stof(strsFrom[2])};
	float epos[3] = {stof(strsTo[0]), stof(strsTo[1]), stof(strsTo[2])};

	// 1，寻路
	status = recast_findpath(id, spos, epos);
	if (Tools::dtStatusFailed(status))
	{
		int statusDetail = status & Tools::DT_STATUS_DETAIL_MASK;
		std::string strDetail = std::to_string(statusDetail);
		_strLastError = "寻路失败！错误码<" + strDetail + ">";
		if (statusDetail == Tools::DT_COORD_INVALID)
		{
			char szFrom[256], szTo[256];
			sprintf_s(szFrom, sizeof(szFrom), "%f, %f, %f", spos[0], spos[1], spos[2]);
			sprintf_s(szTo, sizeof(szTo), "%f, %f, %f", epos[0], epos[1], epos[2]);
			std::string strFrom = szFrom, strTo = szTo;
			_strLastError += " - 坐标非法！From<" + strFrom + "> To<" + strTo + ">";
		}
		return -13;
	}
	else if (Tools::dtStatusInProgress(status))
	{
		return -14;
	}

	float* fixPos = new float[3];
	memcpy(fixPos, recast_getfixposition(id, spos), sizeof(float) * 3);
	float* fixPos2 = new float[3];
	memcpy(fixPos2, recast_getfixposition(id, epos), sizeof(float) * 3);

	// 2，得到实际（平滑）路径
	recast_smooth(id, 2, 0.5);

	// 寻路成功！
	// 3，得到凸多边形id序列
	std::string format = "  起点<%f,%f,%f> 终点<%f,%f,%f>\n";
	printf_s(format.c_str(), spos[0], spos[1], spos[2], epos[0], epos[1], epos[2]);

	int m_npolys = recast_getcountpoly(id);
	unsigned int* m_polys = recast_getpathpoly(id);
	std::string outputPoly = "  路线地块序号 (" + std::to_string(m_npolys) + "): \n    ";
	for (int i = 0; i < m_npolys; ++i)
	{
		std::string strPoly = std::to_string((m_polys[i]));
		outputPoly += strPoly + ", ";
	}
	outputPoly += "\n";
	printf_s(outputPoly.c_str());

	// 4，得到寻路路径的坐标序列
	int m_nsmoothPath = recast_getcountsmooth(id);
	float* m_smoothPath = recast_getpathsmooth(id);
	std::string outputSmooth = "  路线 (" + std::to_string(m_nsmoothPath) + "): \n";
	for (int i = 0; i < m_nsmoothPath; ++i)
	{
		std::string strSmooth = std::to_string(m_smoothPath[i * 3]) + "," + std::to_string(m_smoothPath[i * 3 + 1]) +
			"," + std::to_string(m_smoothPath[i * 3 + 2]);
		outputSmooth += "    <" + strSmooth + "> \n";
	}
	printf_s(outputSmooth.c_str());

	return 1;
}
