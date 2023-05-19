// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2018/07/13

using System;
using System.Reflection;
using UnityEngine;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;

#pragma warning disable 1591
namespace DG.Tweening
{
    /// <summary>
    /// Utility functions that deal with available Modules.
    /// Modules defines:
    /// - DOTAUDIO
    /// - DOTPHYSICS
    /// - DOTPHYSICS2D
    /// - DOTSPRITE
    /// - DOTUI
    /// Extra defines set and used for implementation of external assets:
    /// - DOTWEEN_TMP ► TextMesh Pro
    /// - DOTWEEN_TK2D ► 2D Toolkit
    /// </summary>
	public static class DOTweenModuleUtils
    {
        static bool _initialized;

        #region Reflection

        /// <summary>
        /// Called via Reflection by DOTweenComponent on Awake
        /// </summary>
#if UNITY_2018_1_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        public static void Init()
        {
            if (_initialized) return;

            _initialized = true;
            DOTweenExternalCommand.SetOrientationOnPath += Physics.SetOrientationOnPath;

#if UNITY_EDITOR
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5 || UNITY_2017_1
            UnityEditor.EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
#else
            UnityEditor.EditorApplication.playModeStateChanged += PlaymodeStateChanged;
#endif
#endif
        }

#if UNITY_2018_1_OR_NEWER
#pragma warning disable
        [UnityEngine.Scripting.Preserve]
        // Just used to preserve methods when building, never called
        static void Preserver()
        {
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            MethodInfo mi = typeof(MonoBehaviour).GetMethod("Stub");
        }
#pragma warning restore
#endif

        #endregion

#if UNITY_EDITOR
        // Fires OnApplicationPause in DOTweenComponent even when Editor is paused (otherwise it's only fired at runtime)
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5 || UNITY_2017_1
        static void PlaymodeStateChanged()
        #else
        static void PlaymodeStateChanged(UnityEditor.PlayModeStateChange state)
#endif
        {
            if (DOTween.instance == null) return;
            DOTween.instance.OnApplicationPause(UnityEditor.EditorApplication.isPaused);
        }
#endif

        // █████████████████████████████████████████████████████████████████████████████████████████████████████████████████████
        // ███ INTERNAL CLASSES ████████████████████████████████████████████████████████████████████████████████████████████████
        // █████████████████████████████████████████████████████████████████████████████████████████████████████████████████████

        public static class Physics
        {
            // Called via DOTweenExternalCommand callback
            public static void SetOrientationOnPath(PathOptions options, Tween t, Quaternion newRot, Transform trans)
            {
#if true // PHYSICS_MARKER
                if (options.isRigidbody) ((Rigidbody)t.target).rotation = newRot;
                else trans.rotation = newRot;
#else
                trans.rotation = newRot;
#endif
            }

            // Returns FALSE if the DOTween's Physics2D Module is disabled, or if there's no Rigidbody2D attached
            public static bool HasRigidbody2D(Component target)
            {
#if true // PHYSICS2D_MARKER
                return target.GetComponent<Rigidbody2D>() != null;
#else
                return false;
#endif
            }

            #region Called via Reflection


            // Called via Reflection by DOTweenPathInspector
            // Returns FALSE if the DOTween's Physics Module is disabled, or if there's no rigidbody attached
#if UNITY_2018_1_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            public static bool HasRigidbody(Component target)
            {
#if true // PHYSICS_MARKER
                return target.GetComponent<Rigidbody>() != null;
#else
                return false;
#endif
            }

            // Called via Reflection by DOTweenPath
#if UNITY_2018_1_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            public static TweenerCore<Vector3, Path, PathOptions> CreateDOTweenPathTween(
                MonoBehaviour target, bool tweenRigidbody, bool isLocal, Path path, float duration, PathMode pathMode
            ){
                TweenerCore<Vector3, Path, PathOptions> t;
#if true // PHYSICS_MARKER
                Rigidbody rBody = tweenRigidbody ? target.GetComponent<Rigidbody>() : null;
                if (tweenRigidbody && rBody != null) {
                    t = isLocal
                        ? rBody.DOLocalPath(path, duration, pathMode)
                        : rBody.DOPath(path, duration, pathMode);
                } else {
                    t = isLocal
                        ? target.transform.DOLocalPath(path, duration, pathMode)
                        : target.transform.DOPath(path, duration, pathMode);
                }
#else
                t = isLocal
                    ? target.transform.DOLocalPath(path, duration, pathMode)
                    : target.transform.DOPath(path, duration, pathMode);
#endif
                return t;
            }

            #endregion
        }
    }
}
