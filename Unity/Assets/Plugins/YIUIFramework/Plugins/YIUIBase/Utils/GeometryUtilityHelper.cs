using System;
using System.Reflection;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 支持光线投射与网格。
    /// </summary>
    public static class GeometryUtilityHelper
    {
        private static readonly Action<Plane[], Matrix4x4>
            ExtractPlanesDelegate;

        static GeometryUtilityHelper()
        {
            var methodInfo = typeof(GeometryUtility).GetMethod(
                "Internal_ExtractPlanes",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (methodInfo != null)
            {
                ExtractPlanesDelegate = (Action<Plane[], Matrix4x4>)
                    Delegate.CreateDelegate(
                        typeof(Action<Plane[], Matrix4x4>),
                        methodInfo,
                        false);
            }
        }

        /// <summary>
        /// Extract the planes.
        /// </summary>
        public static void ExtractPlanes(Plane[] planes, Camera camera)
        {
            var mat = camera.projectionMatrix * camera.worldToCameraMatrix;
            ExtractPlanes(planes, mat);
        }

        /// <summary>
        /// Extract the planes.
        /// </summary>
        public static void ExtractPlanes(
            Plane[] planes, Matrix4x4 worldToProjectionMatrix)
        {
            if (ExtractPlanesDelegate != null)
            {
                ExtractPlanesDelegate(planes, worldToProjectionMatrix);
            }
            else
            {
                GeometryUtility.CalculateFrustumPlanes(
                    worldToProjectionMatrix, planes);
            }
        }
    }
}