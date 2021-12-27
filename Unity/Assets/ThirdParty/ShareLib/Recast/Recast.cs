using System;
using System.Runtime.InteropServices;

namespace ET
{
    public static class Recast
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        const string RecastDLL = "__Internal";
#else
        const string RecastDLL = "RecastDll";
#endif
        public const int MAX_POLYS = 256;
        
        [DllImport(RecastDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern IntPtr RecastLoad(int id, byte[] buffer, int n);

        public static long RecastLoadLong(int id, byte[] buffer, int n)
        {
            return RecastLoad(id, buffer, n).ToInt64();
        }
        
        [DllImport(RecastDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern IntPtr RecastGet(int id);

        public static long RecastGetLong(int id)
        {
            return RecastGet(id).ToInt32();
        }
        
        [DllImport(RecastDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern void RecastClear();
        
        [DllImport(RecastDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern int RecastFind(IntPtr navPtr, float[] extents, float[] startPos, float[] endPos, float[] straightPath);
        
        public static int RecastFind(long navPtr, float[] extents, float[] startPos, float[] endPos, float[] straightPath)
        {
            return RecastFind(new IntPtr(navPtr), extents, startPos, endPos, straightPath);
        }
        
        [DllImport(RecastDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern int RecastFindNearestPoint(IntPtr navPtr, float[] extents, float[] pos, float[] nearestPos);

        public static int RecastFindNearestPoint(long navPtr, float[] extents, float[] pos, float[] nearestPos)
        {
            return RecastFindNearestPoint(new IntPtr(navPtr), extents, pos, nearestPos);
        }
        
        [DllImport(RecastDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern int RecastFindRandomPointAroundCircle(IntPtr navPtr, float[] extents, float[] centerPos, float radius, float[] randomPos);
        
        public static int RecastFindRandomPointAroundCircle(long navPtr, float[] extents, float[] centerPos, float radius, float[] randomPos)
        {
            return RecastFindRandomPointAroundCircle(new IntPtr(navPtr), extents, centerPos, radius, randomPos);
        }
        
        [DllImport(RecastDLL, CallingConvention=CallingConvention.Cdecl)]
        private static extern int RecastFindRandomPoint(IntPtr navPtr, float[] randomPos);

        public static int RecastFindRandomPoint(long navPtr, float[] randomPos)
        {
            return RecastFindRandomPoint(new IntPtr(navPtr), randomPos);
        }
    }
}