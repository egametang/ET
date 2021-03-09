using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 用Apoca\recastnavigation-master\RecastDemo\Projects\vs2019\recastnavigation.sln工程编译
    /// recastDll.dll，是Release版x64平台编译的结果
    /// Aug.28.2020. Liu gang
    /// </summary>
    public class RecastInterface
    {
        private const string RecastDLL = "RecastNavDll";

        [DllImport(RecastDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool recast_init();

        [DllImport(RecastDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void recast_fini();

        [DllImport(RecastDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool recast_loadmap(int id, char[] path);

        [DllImport(RecastDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool recast_freemap(int id);

        [DllImport(RecastDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int recast_findpath(int id, float[] spos, float[] epos);

        [DllImport(RecastDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool recast_smooth(int id, float step_size, float slop);

        [DllImport(RecastDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int recast_raycast(int id, float[] spos, float[] epos);

        [DllImport(RecastDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int recast_getcountpoly(int id);

        [DllImport(RecastDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int recast_getcountsmooth(int id);

        [DllImport(RecastDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr recast_getpathpoly(int id);

        [DllImport(RecastDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr recast_getpathsmooth(int id);

        [DllImport(RecastDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr recast_getfixposition(int id, float[] pos);

        [DllImport(RecastDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr recast_gethitposition(int id);

        /// <summary>
        /// 初始化Recast引擎
        /// </summary>
        /// <returns></returns>
        public static bool Init()
        {
            return recast_init();
        }

        /// <summary>
        /// 结束化Recast引擎
        /// </summary>
        public static void Fini()
        {
            recast_fini();
        }

        /// <summary>
        /// 加载地图数据，支持同时加载多张地图
        /// </summary>
        /// <param name="id">地图Id</param>
        /// <param name="path">地图文件名（包含路径）</param>
        /// <returns></returns>
        public static bool LoadMap(int id, char[] path)
        {
            if (path == null || path.Length == 0)
                return false;
            return recast_loadmap(id, path);
        }

        /// <summary>
        /// 释放地图数据
        /// </summary>
        /// <param name="id">地图Id</param>
        /// <returns></returns>
        public static bool FreeMap(int id)
        {
            return recast_freemap(id);
        }

        public static Vector3 SPos = new Vector3();
        public static Vector3 EPos = new Vector3();

        /// <summary>
        /// 寻路
        /// </summary>
        /// <param name="id">地图Id</param>
        /// <param name="spos">起点三维坐标</param>
        /// <param name="epos">终点三维坐标</param>
        /// <returns></returns>
        public static bool FindPath(int id, Vector3 spos, Vector3 epos)
        {
            {
                float[] fixPos = fixposition(id, spos);
                if (fixPos != null)
                {
                    spos.y = fixPos[1];
                }
                else
                {
                    Console.WriteLine(string.Concat("错误:", ($"Recast寻路 FindPath Error：- 起点非法 - spos:{spos} - MapId:{id}")));
                }

                SPos = spos;
            }
            {
                float[] fixPos = fixposition(id, epos);
                if (fixPos != null)
                {
                    epos.y = fixPos[1];
                }
                else
                {
                    Console.WriteLine(string.Concat("错误:",($"Recast寻路 FindPath Error：- 终点非法 - epos:{epos} - MapId:{id}")));
                }

                EPos = epos;
            }
            dtStatus status = (dtStatus) recast_findpath(id, new[] { -spos.x, spos.y, spos.z }, new[] { -epos.x, epos.y, epos.z });
            if (dtStatusFailed(status))
            {
                dtStatus statusDetail = status & dtStatus.DT_STATUS_DETAIL_MASK;
                string _strLastError = $"Recast寻路 FindPath Error：寻路失败！错误码<" + statusDetail + $"> - MapId:{id}";
                if (statusDetail == dtStatus.DT_COORD_INVALID)
                {
                    _strLastError += " - 坐标非法！From<" + spos + "> To<" + epos + $"> - MapId:{id}";
                }
                Console.WriteLine(string.Concat("错误:",_strLastError));
                return false;
            }
            else if (dtStatusInProgress(status))
            {
                string _strLastError = $"Recast寻路 Error：寻路尚未结束!- MapId:{id}";
                Console.WriteLine(string.Concat("错误:",_strLastError));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 寻路以后，需要调用本函数，得到真实路径，这条路径可以是很平滑的路径
        /// BUG FIX：如果Smooth函数的第一个参数设置为5，则可能找到的路线非常长（节点达到2048）
        /// </summary>
        /// <param name="id">地图Id</param>
        /// <param name="step_size">平滑系数，数值越小，越平滑；如果给0，则自动变为0.5</param>
        /// <param name="slop">用途不明（但肯定不影响平滑），如果给0，则自动变为0.01</param>
        /// <returns></returns>
        public static bool Smooth(int id, float step_size, float slop)
        {
            return recast_smooth(id, step_size, slop);
        }

        /// <summary>
        /// 射线检测
        /// </summary>
        /// <param name="id">地图Id</param>
        /// <param name="spos">起点三维坐标</param>
        /// <param name="epos">终点三维坐标</param>
        /// <returns></returns>
        public static bool Raycast(int id, Vector3 spos, Vector3 epos)
        {
            dtStatus status = (dtStatus) recast_raycast(id, new float[] { -spos.x, spos.y, spos.z }, new float[] { -epos.x, epos.y, epos.z });
            if (dtStatusFailed(status))
            {
                dtStatus statusDetail = status & dtStatus.DT_STATUS_DETAIL_MASK;
                string _strLastError = "Recast寻路 Raycast Error：寻路失败！错误码<" + statusDetail + $"> - MapId:{id}";
                if (statusDetail == dtStatus.DT_COORD_INVALID)
                {
                    _strLastError += " - 坐标非法！From<" + spos + "> To<" + epos + $"> - MapId:{id}";
                }
                Console.WriteLine(string.Concat("错误:",_strLastError));
                return false;
            }
            else if (dtStatusInProgress(status))
            {
                string _strLastError = $"Recast寻路 Error：寻路尚未结束! - MapId:{id}";
                Console.WriteLine(string.Concat("错误:",_strLastError));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取射线检测的碰撞点
        /// <param name="id">地图Id</param>
        /// <returns></returns>
        public static float[] getHitPosition(int id)
        {
            unsafe
            {
                try
                {
                    IntPtr hitPos = recast_gethitposition(id);
                    float[] arrHitPos = new float[3];
                    if (hitPos.ToPointer() != null)
                    {
                        Marshal.Copy(hitPos, arrHitPos, 0, 3);
                        arrHitPos[0] = -arrHitPos[0];
                        return arrHitPos;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(string.Concat("错误:",($"RecstInterface getHitPosition Exception! - {e}")));
                    return null;
                }
            }
        }

        /// <summary>
        /// 在mesh中修正坐标高度
        /// 如果你给的Y坐标太低了，则可能会落到附近其他更低的地方
        /// <param name="id">地图Id</param>
        /// <returns></returns>
        public static float[] fixposition(int id, Vector3 pos)
        {
            unsafe
            {
                try
                {
                    IntPtr fixPos = recast_getfixposition(id, new float[] { -pos.x, pos.y, pos.z }); // （pos.y+1）抬高一点计算
                    float[] arrFixPos = new float[3];
                    if (fixPos.ToPointer() != null)
                    {
                        Marshal.Copy(fixPos, arrFixPos, 0, 3);
                        arrFixPos[0] = -arrFixPos[0];
                        return arrFixPos;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(string.Concat("错误:",($"RecstInterface fixposition Exception! - {e}")));
                    return null;
                }
            }
        }

        public static int[] GetPathPoly(int id, out int polyCount)
        {
            unsafe
            {
                try
                {
                    // https://bbs.csdn.net/topics/392618929?list=676344
                    polyCount = recast_getcountpoly(id);
                    IntPtr polys = recast_getpathpoly(id);
                    if (polys.ToPointer() != null)
                    {
                        int[] arrPolys = new int[polyCount];
                        Marshal.Copy(polys, arrPolys, 0, polyCount);
                        return arrPolys;
                    }

                    return null;
                }
                catch (Exception e)
                {
                    polyCount = 0;
                    Console.WriteLine(string.Concat("错误:",($"RecstInterface fixposition Exception! - {e}")));
                    return null;
                }
            }
        }

        public static float[] GetPathSmooth(int id, out int smoothCount)
        {
            unsafe
            {
                try
                {
                    int polyCount = recast_getcountpoly(id);
                    smoothCount = recast_getcountsmooth(id);
                    IntPtr smooths = recast_getpathsmooth(id);
                    float[] arrSmooths = new float[smoothCount * 3];
                    Marshal.Copy(smooths, arrSmooths, 0, smoothCount * 3);
                    for (int i = 0; i < smoothCount; ++i)
                    {
                        arrSmooths[i * 3] = -arrSmooths[i * 3];
                    }

                    return arrSmooths;
                }
                catch (Exception e)
                {
                    smoothCount = 0;
                    Console.WriteLine(string.Concat("错误:",($"RecstInterface fixposition Exception! - {e}")));
                    return null;
                }
            }
        }

        [Flags]
        public enum dtStatus
        {
            DT_FAILURE = 1 << 31,
            DT_SUCCESS = 1 << 30,
            DT_IN_PROGRESS = 1 << 29,
            DT_STATUS_DETAIL_MASK = 0x0ffffff,
            DT_WRONG_MAGIC = 1 << 0,
            DT_WRONG_VERSION = 1 << 1,
            DT_OUT_OF_MEMORY = 1 << 2,
            DT_INVALID_PARAM = 1 << 3,
            DT_BUFFER_TOO_SMALL = 1 << 4,
            DT_OUT_OF_NODES = 1 << 5,
            DT_PARTIAL_RESULT = 1 << 6,
            DT_ALREADY_OCCUPIED = 1 << 7,
            DT_COORD_INVALID = 1 << 13,
        }

        // Returns true of status is success.
        public static bool dtStatusSucceed(dtStatus status)
        {
            return (status & dtStatus.DT_SUCCESS) != 0;
        }

        // Returns true of status is failure.
        public static bool dtStatusFailed(dtStatus status)
        {
            return (status & dtStatus.DT_FAILURE) != 0;
        }

        // Returns true of status is in progress.
        public static bool dtStatusInProgress(dtStatus status)
        {
            return (status & dtStatus.DT_IN_PROGRESS) != 0;
        }

        // Returns true if specific detail is set.
        public static bool dtStatusDetail(dtStatus status, uint detail)
        {
            return ((uint) status & detail) != 0;
        }
    }
}