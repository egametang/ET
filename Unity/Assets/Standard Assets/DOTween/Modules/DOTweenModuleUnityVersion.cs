// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2018/07/13

using System;
using UnityEngine;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
#if UNITY_2018_1_OR_NEWER && (NET_4_6 || NET_STANDARD_2_0)
using System.Threading.Tasks;
#endif

#pragma warning disable 1591
namespace DG.Tweening
{
    /// <summary>
    /// Shortcuts/functions that are not strictly related to specific Modules
    /// but are available only on some Unity versions
    /// </summary>
	public static class DOTweenModuleUnityVersion
    {
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5 || UNITY_2017_1_OR_NEWER
        #region Unity 4.3 or Newer

        #region Material

        /// <summary>Tweens a Material's color using the given gradient
        /// (NOTE 1: only uses the colors of the gradient, not the alphas - NOTE 2: creates a Sequence, not a Tweener).
        /// Also stores the image as the tween's target so it can be used for filtered operations</summary>
        /// <param name="gradient">The gradient to use</param><param name="duration">The duration of the tween</param>
        public static Sequence DOGradientColor(this Material target, Gradient gradient, float duration)
        {
            Sequence s = DOTween.Sequence();
            GradientColorKey[] colors = gradient.colorKeys;
            int len = colors.Length;
            for (int i = 0; i < len; ++i) {
                GradientColorKey c = colors[i];
                if (i == 0 && c.time <= 0) {
                    target.color = c.color;
                    continue;
                }
                float colorDuration = i == len - 1
                    ? duration - s.Duration(false) // Verifies that total duration is correct
                    : duration * (i == 0 ? c.time : c.time - colors[i - 1].time);
                s.Append(target.DOColor(c.color, colorDuration).SetEase(Ease.Linear));
            }
            s.SetTarget(target);
            return s;
        }
        /// <summary>Tweens a Material's named color property using the given gradient
        /// (NOTE 1: only uses the colors of the gradient, not the alphas - NOTE 2: creates a Sequence, not a Tweener).
        /// Also stores the image as the tween's target so it can be used for filtered operations</summary>
        /// <param name="gradient">The gradient to use</param>
        /// <param name="property">The name of the material property to tween (like _Tint or _SpecColor)</param>
        /// <param name="duration">The duration of the tween</param>
        public static Sequence DOGradientColor(this Material target, Gradient gradient, string property, float duration)
        {
            Sequence s = DOTween.Sequence();
            GradientColorKey[] colors = gradient.colorKeys;
            int len = colors.Length;
            for (int i = 0; i < len; ++i) {
                GradientColorKey c = colors[i];
                if (i == 0 && c.time <= 0) {
                    target.SetColor(property, c.color);
                    continue;
                }
                float colorDuration = i == len - 1
                    ? duration - s.Duration(false) // Verifies that total duration is correct
                    : duration * (i == 0 ? c.time : c.time - colors[i - 1].time);
                s.Append(target.DOColor(c.color, property, colorDuration).SetEase(Ease.Linear));
            }
            s.SetTarget(target);
            return s;
        }

        #endregion

        #endregion
#endif

#if UNITY_5_3_OR_NEWER || UNITY_2017_1_OR_NEWER
        #region Unity 5.3 or Newer

        #region CustomYieldInstructions

        /// <summary>
        /// Returns a <see cref="CustomYieldInstruction"/> that waits until the tween is killed or complete.
        /// It can be used inside a coroutine as a yield.
        /// <para>Example usage:</para><code>yield return myTween.WaitForCompletion(true);</code>
        /// </summary>
        public static CustomYieldInstruction WaitForCompletion(this Tween t, bool returnCustomYieldInstruction)
        {
            if (!t.active) {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return null;
            }
            return new DOTweenCYInstruction.WaitForCompletion(t);
        }

        /// <summary>
        /// Returns a <see cref="CustomYieldInstruction"/> that waits until the tween is killed or rewinded.
        /// It can be used inside a coroutine as a yield.
        /// <para>Example usage:</para><code>yield return myTween.WaitForRewind();</code>
        /// </summary>
        public static CustomYieldInstruction WaitForRewind(this Tween t, bool returnCustomYieldInstruction)
        {
            if (!t.active) {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return null;
            }
            return new DOTweenCYInstruction.WaitForRewind(t);
        }

        /// <summary>
        /// Returns a <see cref="CustomYieldInstruction"/> that waits until the tween is killed.
        /// It can be used inside a coroutine as a yield.
        /// <para>Example usage:</para><code>yield return myTween.WaitForKill();</code>
        /// </summary>
        public static CustomYieldInstruction WaitForKill(this Tween t, bool returnCustomYieldInstruction)
        {
            if (!t.active) {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return null;
            }
            return new DOTweenCYInstruction.WaitForKill(t);
        }

        /// <summary>
        /// Returns a <see cref="CustomYieldInstruction"/> that waits until the tween is killed or has gone through the given amount of loops.
        /// It can be used inside a coroutine as a yield.
        /// <para>Example usage:</para><code>yield return myTween.WaitForElapsedLoops(2);</code>
        /// </summary>
        /// <param name="elapsedLoops">Elapsed loops to wait for</param>
        public static CustomYieldInstruction WaitForElapsedLoops(this Tween t, int elapsedLoops, bool returnCustomYieldInstruction)
        {
            if (!t.active) {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return null;
            }
            return new DOTweenCYInstruction.WaitForElapsedLoops(t, elapsedLoops);
        }

        /// <summary>
        /// Returns a <see cref="CustomYieldInstruction"/> that waits until the tween is killed
        /// or has reached the given time position (loops included, delays excluded).
        /// It can be used inside a coroutine as a yield.
        /// <para>Example usage:</para><code>yield return myTween.WaitForPosition(2.5f);</code>
        /// </summary>
        /// <param name="position">Position (loops included, delays excluded) to wait for</param>
        public static CustomYieldInstruction WaitForPosition(this Tween t, float position, bool returnCustomYieldInstruction)
        {
            if (!t.active) {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return null;
            }
            return new DOTweenCYInstruction.WaitForPosition(t, position);
        }

        /// <summary>
        /// Returns a <see cref="CustomYieldInstruction"/> that waits until the tween is killed or started
        /// (meaning when the tween is set in a playing state the first time, after any eventual delay).
        /// It can be used inside a coroutine as a yield.
        /// <para>Example usage:</para><code>yield return myTween.WaitForStart();</code>
        /// </summary>
        public static CustomYieldInstruction WaitForStart(this Tween t, bool returnCustomYieldInstruction)
        {
            if (!t.active) {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return null;
            }
            return new DOTweenCYInstruction.WaitForStart(t);
        }

        #endregion

        #endregion
#endif

#if UNITY_2018_1_OR_NEWER
        #region Unity 2018.1 or Newer

        #region Material

        /// <summary>Tweens a Material's named texture offset property with the given ID to the given value.
        /// Also stores the material as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param>
        /// <param name="propertyID">The ID of the material property to tween (also called nameID in Unity's manual)</param>
        /// <param name="duration">The duration of the tween</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOOffset(this Material target, Vector2 endValue, int propertyID, float duration)
        {
            if (!target.HasProperty(propertyID)) {
                if (Debugger.logPriority > 0) Debugger.LogMissingMaterialProperty(propertyID);
                return null;
            }
            TweenerCore<Vector2, Vector2, VectorOptions> t = DOTween.To(() => target.GetTextureOffset(propertyID), x => target.SetTextureOffset(propertyID, x), endValue, duration);
            t.SetTarget(target);
            return t;
        }

        /// <summary>Tweens a Material's named texture scale property with the given ID to the given value.
        /// Also stores the material as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param>
        /// <param name="propertyID">The ID of the material property to tween (also called nameID in Unity's manual)</param>
        /// <param name="duration">The duration of the tween</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOTiling(this Material target, Vector2 endValue, int propertyID, float duration)
        {
            if (!target.HasProperty(propertyID)) {
                if (Debugger.logPriority > 0) Debugger.LogMissingMaterialProperty(propertyID);
                return null;
            }
            TweenerCore<Vector2, Vector2, VectorOptions> t = DOTween.To(() => target.GetTextureScale(propertyID), x => target.SetTextureScale(propertyID, x), endValue, duration);
            t.SetTarget(target);
            return t;
        }

        #endregion

        #region .NET 4.6 or Newer

#if (NET_4_6 || NET_STANDARD_2_0)

        #region Async Instructions

        /// <summary>
        /// Returns an async <see cref="Task"/> that waits until the tween is killed or complete.
        /// It can be used inside an async operation.
        /// <para>Example usage:</para><code>await myTween.WaitForCompletion();</code>
        /// </summary>
        public static async Task AsyncWaitForCompletion(this Tween t)
        {
            if (!t.active) {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return;
            }
            while (t.active && !t.IsComplete()) await Task.Yield();
        }

        /// <summary>
        /// Returns an async <see cref="Task"/> that waits until the tween is killed or rewinded.
        /// It can be used inside an async operation.
        /// <para>Example usage:</para><code>await myTween.AsyncWaitForRewind();</code>
        /// </summary>
        public static async Task AsyncWaitForRewind(this Tween t)
        {
            if (!t.active) {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return;
            }
            while (t.active && (!t.playedOnce || t.position * (t.CompletedLoops() + 1) > 0)) await Task.Yield();
        }

        /// <summary>
        /// Returns an async <see cref="Task"/> that waits until the tween is killed.
        /// It can be used inside an async operation.
        /// <para>Example usage:</para><code>await myTween.AsyncWaitForKill();</code>
        /// </summary>
        public static async Task AsyncWaitForKill(this Tween t)
        {
            if (!t.active) {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return;
            }
            while (t.active) await Task.Yield();
        }

        /// <summary>
        /// Returns an async <see cref="Task"/> that waits until the tween is killed or has gone through the given amount of loops.
        /// It can be used inside an async operation.
        /// <para>Example usage:</para><code>await myTween.AsyncWaitForElapsedLoops();</code>
        /// </summary>
        /// <param name="elapsedLoops">Elapsed loops to wait for</param>
        public static async Task AsyncWaitForElapsedLoops(this Tween t, int elapsedLoops)
        {
            if (!t.active) {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return;
            }
            while (t.active && t.CompletedLoops() < elapsedLoops) await Task.Yield();
        }

        /// <summary>
        /// Returns an async <see cref="Task"/> that waits until the tween is killed or started
        /// (meaning when the tween is set in a playing state the first time, after any eventual delay).
        /// It can be used inside an async operation.
        /// <para>Example usage:</para><code>await myTween.AsyncWaitForPosition();</code>
        /// </summary>
        /// <param name="position">Position (loops included, delays excluded) to wait for</param>
        public static async Task AsyncWaitForPosition(this Tween t, float position)
        {
            if (!t.active) {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return;
            }
            while (t.active && t.position * (t.CompletedLoops() + 1) < position) await Task.Yield();
        }

        /// <summary>
        /// Returns an async <see cref="Task"/> that waits until the tween is killed.
        /// It can be used inside an async operation.
        /// <para>Example usage:</para><code>await myTween.AsyncWaitForKill();</code>
        /// </summary>
        public static async Task AsyncWaitForStart(this Tween t)
        {
            if (!t.active) {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return;
            }
            while (t.active && !t.playedOnce) await Task.Yield();
        }

        #endregion
#endif

        #endregion

        #endregion
#endif
    }

    // █████████████████████████████████████████████████████████████████████████████████████████████████████████████████████
    // ███ CLASSES █████████████████████████████████████████████████████████████████████████████████████████████████████████
    // █████████████████████████████████████████████████████████████████████████████████████████████████████████████████████

#if UNITY_5_3_OR_NEWER || UNITY_2017_1_OR_NEWER
    public static class DOTweenCYInstruction
    {
        public class WaitForCompletion : CustomYieldInstruction
        {
            public override bool keepWaiting { get {
                return t.active && !t.IsComplete();
            }}
            readonly Tween t;
            public WaitForCompletion(Tween tween)
            {
                t = tween;
            }
        }

        public class WaitForRewind : CustomYieldInstruction
        {
            public override bool keepWaiting { get {
                return t.active && (!t.playedOnce || t.position * (t.CompletedLoops() + 1) > 0);
            }}
            readonly Tween t;
            public WaitForRewind(Tween tween)
            {
                t = tween;
            }
        }

        public class WaitForKill : CustomYieldInstruction
        {
            public override bool keepWaiting { get {
                return t.active;
            }}
            readonly Tween t;
            public WaitForKill(Tween tween)
            {
                t = tween;
            }
        }

        public class WaitForElapsedLoops : CustomYieldInstruction
        {
            public override bool keepWaiting { get {
                return t.active && t.CompletedLoops() < elapsedLoops;
            }}
            readonly Tween t;
            readonly int elapsedLoops;
            public WaitForElapsedLoops(Tween tween, int elapsedLoops)
            {
                t = tween;
                this.elapsedLoops = elapsedLoops;
            }
        }

        public class WaitForPosition : CustomYieldInstruction
        {
            public override bool keepWaiting { get {
                return t.active && t.position * (t.CompletedLoops() + 1) < position;
            }}
            readonly Tween t;
            readonly float position;
            public WaitForPosition(Tween tween, float position)
            {
                t = tween;
                this.position = position;
            }
        }

        public class WaitForStart : CustomYieldInstruction
        {
            public override bool keepWaiting { get {
                return t.active && !t.playedOnce;
            }}
            readonly Tween t;
            public WaitForStart(Tween tween)
            {
                t = tween;
            }
        }
    }
#endif
}
