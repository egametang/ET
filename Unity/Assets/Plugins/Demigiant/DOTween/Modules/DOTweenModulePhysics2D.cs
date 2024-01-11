// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2018/07/13

#if true && (UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5 || UNITY_2017_1_OR_NEWER) // MODULE_MARKER
using System;
using DG.Tweening.Core;
using DG.Tweening.Plugins;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using UnityEngine;

#pragma warning disable 1591
namespace DG.Tweening
{
	public static class DOTweenModulePhysics2D
    {
        #region Shortcuts

        #region Rigidbody2D Shortcuts

        /// <summary>Tweens a Rigidbody2D's position to the given value.
        /// Also stores the Rigidbody2D as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOMove(this Rigidbody2D target, Vector2 endValue, float duration, bool snapping = false)
        {
            TweenerCore<Vector2, Vector2, VectorOptions> t = DOTween.To(() => target.position, target.MovePosition, endValue, duration);
            t.SetOptions(snapping).SetTarget(target);
            return t;
        }

        /// <summary>Tweens a Rigidbody2D's X position to the given value.
        /// Also stores the Rigidbody2D as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOMoveX(this Rigidbody2D target, float endValue, float duration, bool snapping = false)
        {
            TweenerCore<Vector2, Vector2, VectorOptions> t = DOTween.To(() => target.position, target.MovePosition, new Vector2(endValue, 0), duration);
            t.SetOptions(AxisConstraint.X, snapping).SetTarget(target);
            return t;
        }

        /// <summary>Tweens a Rigidbody2D's Y position to the given value.
        /// Also stores the Rigidbody2D as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOMoveY(this Rigidbody2D target, float endValue, float duration, bool snapping = false)
        {
            TweenerCore<Vector2, Vector2, VectorOptions> t = DOTween.To(() => target.position, target.MovePosition, new Vector2(0, endValue), duration);
            t.SetOptions(AxisConstraint.Y, snapping).SetTarget(target);
            return t;
        }

        /// <summary>Tweens a Rigidbody2D's rotation to the given value.
        /// Also stores the Rigidbody2D as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<float, float, FloatOptions> DORotate(this Rigidbody2D target, float endValue, float duration)
        {
            TweenerCore<float, float, FloatOptions> t = DOTween.To(() => target.rotation, target.MoveRotation, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        #region Special

        /// <summary>Tweens a Rigidbody2D's position to the given value, while also applying a jump effect along the Y axis.
        /// Returns a Sequence instead of a Tweener.
        /// Also stores the Rigidbody2D as the tween's target so it can be used for filtered operations.
        /// <para>IMPORTANT: a rigidbody2D can't be animated in a jump arc using MovePosition, so the tween will directly set the position</para></summary>
        /// <param name="endValue">The end value to reach</param>
        /// <param name="jumpPower">Power of the jump (the max height of the jump is represented by this plus the final Y offset)</param>
        /// <param name="numJumps">Total number of jumps</param>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static Sequence DOJump(this Rigidbody2D target, Vector2 endValue, float jumpPower, int numJumps, float duration, bool snapping = false)
        {
            if (numJumps < 1) numJumps = 1;
            float startPosY = 0;
            float offsetY = -1;
            bool offsetYSet = false;
            Sequence s = DOTween.Sequence();
            Tween yTween = DOTween.To(() => target.position, x => target.position = x, new Vector2(0, jumpPower), duration / (numJumps * 2))
                .SetOptions(AxisConstraint.Y, snapping).SetEase(Ease.OutQuad).SetRelative()
                .SetLoops(numJumps * 2, LoopType.Yoyo)
                .OnStart(() => startPosY = target.position.y);
            s.Append(DOTween.To(() => target.position, x => target.position = x, new Vector2(endValue.x, 0), duration)
                    .SetOptions(AxisConstraint.X, snapping).SetEase(Ease.Linear)
                ).Join(yTween)
                .SetTarget(target).SetEase(DOTween.defaultEaseType);
            yTween.OnUpdate(() => {
                if (!offsetYSet) {
                    offsetYSet = true;
                    offsetY = s.isRelative ? endValue.y : endValue.y - startPosY;
                }
                Vector3 pos = target.position;
                pos.y += DOVirtual.EasedValue(0, offsetY, yTween.ElapsedPercentage(), Ease.OutQuad);
                target.MovePosition(pos);
            });
            return s;
        }

        /// <summary>Tweens a Rigidbody2D's position through the given path waypoints, using the chosen path algorithm.
        /// Also stores the Rigidbody2D as the tween's target so it can be used for filtered operations.
        /// <para>NOTE: to tween a Rigidbody2D correctly it should be set to kinematic at least while being tweened.</para>
        /// <para>BEWARE: doesn't work on Windows Phone store (waiting for Unity to fix their own bug).
        /// If you plan to publish there you should use a regular transform.DOPath.</para></summary>
        /// <param name="path">The waypoints to go through</param>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="pathType">The type of path: Linear (straight path), CatmullRom (curved CatmullRom path) or CubicBezier (curved with control points)</param>
        /// <param name="pathMode">The path mode: 3D, side-scroller 2D, top-down 2D</param>
        /// <param name="resolution">The resolution of the path (useless in case of Linear paths): higher resolutions make for more detailed curved paths but are more expensive.
        /// Defaults to 10, but a value of 5 is usually enough if you don't have dramatic long curves between waypoints</param>
        /// <param name="gizmoColor">The color of the path (shown when gizmos are active in the Play panel and the tween is running)</param>
        public static TweenerCore<Vector3, Path, PathOptions> DOPath(
            this Rigidbody2D target, Vector2[] path, float duration, PathType pathType = PathType.Linear,
            PathMode pathMode = PathMode.Full3D, int resolution = 10, Color? gizmoColor = null
        )
        {
            if (resolution < 1) resolution = 1;
            int len = path.Length;
            Vector3[] path3D = new Vector3[len];
            for (int i = 0; i < len; ++i) path3D[i] = path[i];
            TweenerCore<Vector3, Path, PathOptions> t = DOTween.To(PathPlugin.Get(), () => target.position, x => target.MovePosition(x), new Path(pathType, path3D, resolution, gizmoColor), duration)
                .SetTarget(target).SetUpdate(UpdateType.Fixed);

            t.plugOptions.isRigidbody2D = true;
            t.plugOptions.mode = pathMode;
            return t;
        }
        /// <summary>Tweens a Rigidbody2D's localPosition through the given path waypoints, using the chosen path algorithm.
        /// Also stores the Rigidbody2D as the tween's target so it can be used for filtered operations
        /// <para>NOTE: to tween a Rigidbody2D correctly it should be set to kinematic at least while being tweened.</para>
        /// <para>BEWARE: doesn't work on Windows Phone store (waiting for Unity to fix their own bug).
        /// If you plan to publish there you should use a regular transform.DOLocalPath.</para></summary>
        /// <param name="path">The waypoint to go through</param>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="pathType">The type of path: Linear (straight path), CatmullRom (curved CatmullRom path) or CubicBezier (curved with control points)</param>
        /// <param name="pathMode">The path mode: 3D, side-scroller 2D, top-down 2D</param>
        /// <param name="resolution">The resolution of the path: higher resolutions make for more detailed curved paths but are more expensive.
        /// Defaults to 10, but a value of 5 is usually enough if you don't have dramatic long curves between waypoints</param>
        /// <param name="gizmoColor">The color of the path (shown when gizmos are active in the Play panel and the tween is running)</param>
        public static TweenerCore<Vector3, Path, PathOptions> DOLocalPath(
            this Rigidbody2D target, Vector2[] path, float duration, PathType pathType = PathType.Linear,
            PathMode pathMode = PathMode.Full3D, int resolution = 10, Color? gizmoColor = null
        )
        {
            if (resolution < 1) resolution = 1;
            int len = path.Length;
            Vector3[] path3D = new Vector3[len];
            for (int i = 0; i < len; ++i) path3D[i] = path[i];
            Transform trans = target.transform;
            TweenerCore<Vector3, Path, PathOptions> t = DOTween.To(PathPlugin.Get(), () => trans.localPosition, x => target.MovePosition(trans.parent == null ? x : trans.parent.TransformPoint(x)), new Path(pathType, path3D, resolution, gizmoColor), duration)
                .SetTarget(target).SetUpdate(UpdateType.Fixed);

            t.plugOptions.isRigidbody2D = true;
            t.plugOptions.mode = pathMode;
            t.plugOptions.useLocalPosition = true;
            return t;
        }
        // Used by path editor when creating the actual tween, so it can pass a pre-compiled path
        internal static TweenerCore<Vector3, Path, PathOptions> DOPath(
            this Rigidbody2D target, Path path, float duration, PathMode pathMode = PathMode.Full3D
        )
        {
            TweenerCore<Vector3, Path, PathOptions> t = DOTween.To(PathPlugin.Get(), () => target.position, x => target.MovePosition(x), path, duration)
                .SetTarget(target);

            t.plugOptions.isRigidbody2D = true;
            t.plugOptions.mode = pathMode;
            return t;
        }
        internal static TweenerCore<Vector3, Path, PathOptions> DOLocalPath(
            this Rigidbody2D target, Path path, float duration, PathMode pathMode = PathMode.Full3D
        )
        {
            Transform trans = target.transform;
            TweenerCore<Vector3, Path, PathOptions> t = DOTween.To(PathPlugin.Get(), () => trans.localPosition, x => target.MovePosition(trans.parent == null ? x : trans.parent.TransformPoint(x)), path, duration)
                .SetTarget(target);

            t.plugOptions.isRigidbody2D = true;
            t.plugOptions.mode = pathMode;
            t.plugOptions.useLocalPosition = true;
            return t;
        }

        #endregion

        #endregion

        #endregion
	}
}
#endif
