// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>
    /// Bitwise flags used by <see cref="Validate.IsEnabled"/> and <see cref="Validate.Disable"/> to determine which
    /// warnings Animancer should give.
    /// <para></para>
    /// <strong>These warnings are all optional</strong>. Feel free to disable any of them if you understand the
    /// <em>potential</em> issues they are referring to.
    /// </summary>
    /// 
    /// <remarks>
    /// All warnings are enabled by default, but are compiled out of runtime builds (except development builds).
    /// </remarks>
    /// 
    /// <example>
    /// You can put the following method in any class to disable whatever warnings you don't want on startup:
    /// <code>
    /// #if UNITY_ASSERTIONS
    /// [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
    /// private static void DisableAnimancerWarnings()
    /// {
    ///     Animancer.OptionalWarning.EndEventInterrupt.Disable();
    ///     
    ///     // You could disable OptionalWarning.All, but that is not recommended for obvious reasons.
    /// }
    /// #endif
    /// </code></example>
    /// https://kybernetik.com.au/animancer/api/Animancer/OptionalWarning
    /// 
    [Flags]
    public enum OptionalWarning
    {
        /// <summary>
        /// A <see href="https://kybernetik.com.au/animancer/docs/introduction/features">Pro-Only Feature</see> has been
        /// used in <see href="https://kybernetik.com.au/animancer/lite">Animancer Lite</see>.
        /// </summary>
        /// 
        /// <remarks>
        /// Some <see href="https://kybernetik.com.au/animancer/docs/introduction/features">Features</see> are only
        /// available in <see href="https://kybernetik.com.au/animancer/pro">Animancer Pro</see>.
        /// <para></para>
        /// <see href="https://kybernetik.com.au/animancer/lite">Animancer Lite</see> allows you to try out those
        /// features in the Unity Editor and gives this warning the first time each one is used to inform you that they
        /// will not work in runtime builds.
        /// </remarks>
        ProOnly = 1 << 0,

        /// <summary>
        /// An <see cref="AnimancerComponent.Playable"/> is being initialised while its <see cref="GameObject"/> is
        /// inactive.
        /// </summary>
        /// 
        /// <remarks>
        /// Unity will not call <see cref="AnimancerComponent.OnDestroy"/> if the <see cref="GameObject"/> is never
        /// enabled. That would prevent it from destroying the internal <see cref="PlayableGraph"/>, leading to a
        /// memory leak.
        /// <para></para>
        /// Animations usually shouldn't be played on inactive objects so you most likely just need to call
        /// <see cref="GameObject.SetActive(bool)"/> first.
        /// <para></para>
        /// If you do intend to use it while inactive, you will need to disable this warning and call
        /// <see cref="AnimancerComponent.OnDestroy"/> manually when the object is destroyed (such as when its scene is
        /// unloaded).
        /// </remarks>
        CreateGraphWhileDisabled = 1 << 1,

        /// <summary>
        /// An <see cref="AnimancerComponent.Playable"/> is being initialised during a type of GUI event that shouldn't
        /// cause side effects.
        /// </summary>
        /// 
        /// <remarks>
        /// <see cref="EventType.Layout"/> and <see cref="EventType.Repaint"/> should display the current details of
        /// things, but they should not modify things.
        /// </remarks>
        CreateGraphDuringGuiEvent = 1 << 2,

        /// <summary>
        /// An <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">Animancer Event</see> is
        /// being added to an <see cref="AnimancerEvent.Sequence"/> which already contains an identical event.
        /// </summary>
        /// 
        /// <remarks>
        /// This warning often occurs due to a misunderstanding about the way events are
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer#auto-clear">Automatically
        /// Cleared</see>.
        /// <para></para>
        /// If you play an <see cref="AnimationClip"/>, its <see cref="AnimancerState.Events"/> will be empty so you
        /// can add whatever events you want.
        /// <para></para>
        /// But <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see> store their own
        /// events, so if you play one then modify its <see cref="AnimancerState.Events"/> you are actually modifying
        /// the transition's events. Then if you play the same transition again, you will modify the events again,
        /// often leading to the same event being added multiple times.
        /// <para></para>
        /// If that is not the case, you can simply disable this warning. There is nothing inherently wrong with having
        /// multiple identical events in the same sequence.
        /// </remarks>
        DuplicateEvent = 1 << 3,

        /// <summary>
        /// An <see href="https://kybernetik.com.au/animancer/docs/manual/events/end">End Event</see> did not actually
        /// end the animation.
        /// </summary>
        /// 
        /// <remarks>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/end">End Events</see> are triggered every
        /// frame after their time has passed, so in this case it might be necessary to explicitly clear the event or
        /// simply use a regular <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">Animancer Event</see>.
        /// <para></para>
        /// If you intend for the event to keep getting triggered, you can just disable this warning.
        /// </remarks>
        EndEventInterrupt = 1 << 4,

        /// <summary>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">Animancer Events</see> are
        /// being used on a state that does not properly support them so they might not work as intended.
        /// </summary>
        /// 
        /// <remarks>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">Animancer Events</see> on a
        /// <see cref="ControllerState"/> will be triggered based on its <see cref="AnimancerState.NormalizedTime"/>,
        /// which comes from the current state of its Animator Controller regardless of which state that may be.
        /// <para></para>
        /// If you intend for the event to be associated with a specific state inside the Animator Controller, you need
        /// to use <see href="https://kybernetik.com.au/animancer/docs/manual/events/animation">Animation Events</see>
        /// instead.
        /// <para></para>
        /// But if you intend the event to be triggered by any state inside the Animator Controller, then you can
        /// simply disable this warning.
        /// </remarks>
        UnsupportedEvents = 1 << 5,

        /// <summary>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/ik">Inverse Kinematics</see> cannot be
        /// dynamically enabled on some <see href="https://kybernetik.com.au/animancer/docs/manual/playing/states">States</see>
        /// Types.
        /// </summary>
        /// 
        /// <remarks>
        /// To use IK on a <see cref="ControllerState"/> you must instead enable it on the desired layer inside the
        /// Animator Controller.
        /// <para></para>
        /// IK is not supported by <see cref="PlayableAssetState"/>.
        /// <para></para>
        /// Setting <see cref="AnimancerNode.ApplyAnimatorIK"/> on such a state will simply do nothing, so feel free to
        /// disable this warning if you are enabling IK on states without checking their type.
        /// </remarks>
        UnsupportedIK = 1 << 6,

        /// <summary>
        /// A <see cref="MixerState"/> is being initialised with its <see cref="AnimancerNode.ChildCount"/> &lt;= 1.
        /// </summary>
        /// 
        /// <remarks>
        /// The purpose of a mixer is to mix multiple child states so you are probably initialising it with incorrect
        /// parameters.
        /// <para></para>
        /// A mixer with only one child will simply play that child, so feel free to disable this warning if that is
        /// what you intend to do.
        /// </remarks>
        MixerMinChildren = 1 << 7,

        /// <summary>
        /// A <see cref="MixerState"/> is synchronising a child with <see cref="AnimancerState.Length"/> = 0.
        /// </summary>
        /// 
        /// <remarks>
        /// Synchronisation is based on the <see cref="AnimancerState.NormalizedTime"/> which can't be calculated if
        /// the <see cref="AnimancerState.Length"/> is 0.
        /// <para></para>
        /// Some state types can change their <see cref="AnimancerState.Length"/>, in which case you can just disable
        /// this warning. But otherwise, the indicated state should not be added to the synchronisation list.
        /// </remarks>
        MixerSynchroniseZeroLength = 1 << 8,

        /// <summary>
        /// A <see href="https://kybernetik.com.au/animancer/docs/manual/blending/fading#custom-fade">Custom Fade</see>
        /// is being started but its weight calculation does not go from 0 to 1.
        /// </summary>
        /// 
        /// <remarks>
        /// The <see cref="CustomFade.CalculateWeight"/> method is expected to return 0 when the parameter is 0 and
        /// 1 when the parameter is 1. It can do anything you want with other values, but violating that guideline will
        /// trigger this warning because it would likely lead to undesirable results.
        /// <para></para>
        /// If your <see cref="CustomFade.CalculateWeight"/> method is expensive you could disable this warning to save
        /// some performance, but violating the above guidelines is not recommended.
        /// </remarks>
        CustomFadeBounds = 1 << 9,

        /// <summary>
        /// A weight calculation method was not specified when attempting to start a
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/blending/fading#custom-fade">Custom Fade</see>.
        /// </summary>
        /// 
        /// <remarks>
        /// Passing a <c>null</c> parameter into <see cref="CustomFade.Apply(AnimancerState, AnimationCurve)"/> and
        /// other similar methods will trigger this warning and return <c>null</c> because a <see cref="CustomFade"/>
        /// serves no purpose if it doesn't have a method for calculating the weight.
        /// </remarks>
        CustomFadeNotNull = 1 << 10,

        /// <summary>
        /// The <see cref="Animator.speed"/> property does not affect Animancer. 
        /// Use <see cref="AnimancerPlayable.Speed"/> instead.
        /// </summary>
        /// 
        /// <remarks>
        /// The <see cref="Animator.speed"/> property only works with Animator Controllers but does not affect the
        /// Playables API so Animancer has its own <see cref="AnimancerPlayable.Speed"/> property.
        /// </remarks>
        AnimatorSpeed = 1 << 11,

        /// <summary>An <see cref="AnimancerNode.Root"/> is null during finalization (garbage collection).</summary>
        /// <remarks>
        /// This probably means that node was never used for anything and should not have been created.
        /// <para></para>
        /// To minimise the performance cost of checking this warning, it does not capture the stack trace of the
        /// node's creation by default. However, you can enable <see cref="AnimancerNode.TraceConstructor"/> on startup
        /// so that it can include the stack trace in the warning message for any nodes that end up being unused.
        /// </remarks>
        UnusedNode = 1 << 12,

        /// <summary>
        /// <see cref="PlayableAssetState.InitialiseBindings"/> is trying to bind to the same <see cref="Animator"/>
        /// that is being used by Animancer.
        /// </summary>
        /// <remarks>
        /// Doing this will replace Animancer's output so its animations would not work anymore.
        /// </remarks>
        PlayableAssetAnimatorBinding = 1 << 13,

        /// <summary>All warning types.</summary>
        All = ~0,
    }

    /// https://kybernetik.com.au/animancer/api/Animancer/Validate
    /// 
    public static partial class Validate
    {
        /************************************************************************************************************************/

#if UNITY_ASSERTIONS
        /// <summary>[Assert-Only] The <see cref="OptionalWarning"/> flags that are currently disabled (default none).</summary>
        private static OptionalWarning _DisabledWarnings;
#endif

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Disables the specified warning type. Supports bitwise combinations.</summary>
        /// <example>
        /// You can put the following method in any class to disable whatever warnings you don't want on startup:
        /// <code>
        /// #if UNITY_ASSERTIONS
        /// [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        /// private static void DisableAnimancerWarnings()
        /// {
        ///     Animancer.OptionalWarning.EndEventInterrupt.Disable();
        ///     
        ///     // You could disable OptionalWarning.All, but that is not recommended for obvious reasons.
        /// }
        /// #endif
        /// </code></example>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void Disable(this OptionalWarning type)
        {
#if UNITY_ASSERTIONS
            _DisabledWarnings |= type;
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Re-enables the specified warning type. Supports bitwise combinations.</summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void Enable(this OptionalWarning type)
        {
#if UNITY_ASSERTIONS
            _DisabledWarnings &= ~type;
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Enables or disables the specified warning type. Supports bitwise combinations.</summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void SetEnabled(this OptionalWarning type, bool enable)
        {
#if UNITY_ASSERTIONS
            if (enable)
                type.Enable();
            else
                type.Disable();
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Logs the `message` as a warning if the `type` is enabled.</summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void Log(this OptionalWarning type, string message, object context = null)
        {
#if UNITY_ASSERTIONS
            if (message == null || type.IsDisabled())
                return;

            Debug.LogWarning($"Possible Bug Detected: {message}" +
                $"\n\nThis warning can be disabled by calling {nameof(Animancer)}.{nameof(OptionalWarning)}.{type}.{nameof(Disable)}()" +
                " and it will automatically be compiled out of Runtime Builds (except for Development Builds)." +
                $" More information can be found at {Strings.DocsURLs.OptionalWarning}\n",
                context as Object);
#endif
        }

        /************************************************************************************************************************/
#if UNITY_ASSERTIONS
        /************************************************************************************************************************/

        /// <summary>[Assert-Only] Returns true if none of the specified warning types have been disabled.</summary>
        public static bool IsEnabled(this OptionalWarning type) => (_DisabledWarnings & type) == 0;

        /************************************************************************************************************************/

        /// <summary>[Assert-Only] Returns true if all of the specified warning types are disabled.</summary>
        public static bool IsDisabled(this OptionalWarning type) => (_DisabledWarnings & type) != 0;

        /************************************************************************************************************************/

        /// <summary>[Assert-Only] Disables the specified warnings and returns those that were previously enabled.</summary>
        /// <example><code>
        /// var warnings = OptionalWarning.All.DisableTemporarily();
        /// // Do stuff.
        /// warnings.Enable();
        /// </code></example>
        public static OptionalWarning DisableTemporarily(this OptionalWarning type)
        {
            var previous = type;
            type.Disable();
            return previous & type;
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}

