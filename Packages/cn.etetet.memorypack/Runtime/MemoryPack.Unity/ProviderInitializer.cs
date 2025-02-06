#nullable enable

using MemoryPack.Formatters;
using UnityEngine;

namespace MemoryPack
{
    public static partial class MemoryPackFormatterProvider
    {
        static void UnityRegister<T>()
            where T : unmanaged
        {
            Register(new UnmanagedFormatter<T>());
            Register(new UnmanagedArrayFormatter<T>());
            Register(new ListFormatter<T>());
            Register(new NullableFormatter<T>());
        }

        static partial void RegisterInitialFormatters()
        {
            // struct
            UnityRegister<UnityEngine.Vector2>();
            UnityRegister<UnityEngine.Vector3>();
            UnityRegister<UnityEngine.Vector4>();
            UnityRegister<UnityEngine.Quaternion>();
            UnityRegister<UnityEngine.Color>();
            UnityRegister<UnityEngine.Bounds>();
            UnityRegister<UnityEngine.Rect>();
            UnityRegister<UnityEngine.Keyframe>();
            Register(new UnmanagedFormatter<UnityEngine.WrapMode>());
            UnityRegister<UnityEngine.Matrix4x4>();
            UnityRegister<UnityEngine.GradientColorKey>();
            UnityRegister<UnityEngine.GradientAlphaKey>();
            Register(new UnmanagedFormatter<UnityEngine.GradientMode>());
            UnityRegister<UnityEngine.Color32>();
            UnityRegister<UnityEngine.LayerMask>();
            UnityRegister<UnityEngine.Vector2Int>();
            UnityRegister<UnityEngine.Vector3Int>();
            UnityRegister<UnityEngine.RangeInt>();
            UnityRegister<UnityEngine.RectInt>();
            UnityRegister<UnityEngine.BoundsInt>();

            // class
            if (!IsRegistered<AnimationCurve>())
            {
                Register(new AnimationCurveFormatter());
                Register(new ArrayFormatter<AnimationCurve>());
                Register(new ListFormatter<AnimationCurve>());
            }
            if (!IsRegistered<Gradient>())
            {
                Register(new GradientFormatter());
                Register(new ArrayFormatter<Gradient>());
                Register(new ListFormatter<Gradient>());
            }
            if (!IsRegistered<RectOffset>())
            {
                Register(new RectOffsetFormatter());
                Register(new ArrayFormatter<RectOffset>());
                Register(new ListFormatter<RectOffset>());
            }
        }
    }
}
