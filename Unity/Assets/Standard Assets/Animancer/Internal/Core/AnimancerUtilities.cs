// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>Various extension methods and utilities.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerUtilities
    /// 
    public static partial class AnimancerUtilities
    {
        /************************************************************************************************************************/
        #region Misc
        /************************************************************************************************************************/

        /// <summary>Loops the `value` so that <c>0 &lt;= value &lt; 1</c>.</summary>
        /// <remarks>This is more efficient than using <see cref="Wrap"/> with a <c>length</c> of 1.</remarks>
        public static float Wrap01(float value)
        {
            var valueAsDouble = (double)value;
            value = (float)(valueAsDouble - Math.Floor(valueAsDouble));
            return value < 1 ? value : 0;
        }

        /// <summary>Loops the `value` so that <c>0 &lt;= value &lt; length</c>.</summary>
        /// <remarks>Unike <see cref="Mathf.Repeat"/>, this method will never return the `length`.</remarks>
        public static float Wrap(float value, float length)
        {
            var valueAsDouble = (double)value;
            var lengthAsDouble = (double)length;
            value = (float)(valueAsDouble - Math.Floor(valueAsDouble / lengthAsDouble) * lengthAsDouble);
            return value < length ? value : 0;
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] Returns true as long as the `value` is not NaN or Infinity.</summary>
        /// <remarks>Newer versions of the .NET framework apparently have a <c>float.IsFinite</c> method.</remarks>
        public static bool IsFinite(this float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        /************************************************************************************************************************/

        /// <summary>Assigns a new <typeparamref name="T"/> and returns true if `t` is null.</summary>
        public static bool NewIfNull<T>(ref T t) where T : class, new()
        {
            if (t == null)
            {
                t = new T();
                return true;
            }
            else return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If `obj` exists, this method returns <see cref="object.ToString"/>.
        /// Or if it is null, this method returns "null".
        /// Or if it is an <see cref="Object"/> that has been destroyed, this method returns "Null (ObjectType)".
        /// </summary>
        public static string ToStringOrNull(object obj)
        {
            if (obj == null)
                return "null";

            var unityObject = obj as Object;
            if (!ReferenceEquals(unityObject, null) && unityObject == null)
                return "Null (" + obj.GetType() + ")";

            return obj.ToString();
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] Swaps <c>array[a]</c> with <c>array[b]</c>.</summary>
        public static void Swap<T>(this T[] array, int a, int b)
        {
            var temp = array[a];
            array[a] = array[b];
            array[b] = temp;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Adds the specified type of <see cref="IAnimancerComponent"/>, links it to the `animator`, and returns it.
        /// </summary>
        public static T AddAnimancerComponent<T>(this Animator animator) where T : Component, IAnimancerComponent
        {
            var animancer = animator.gameObject.AddComponent<T>();
            animancer.Animator = animator;
            return animancer;
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Returns the <see cref="IAnimancerComponent"/> on the same <see cref="GameObject"/> as the `animator` if
        /// there is one. Otherwise this method adds a new one and returns it.
        /// </summary>
        public static T GetOrAddAnimancerComponent<T>(this Animator animator) where T : Component, IAnimancerComponent
        {
            var animancer = animator.GetComponent<T>();
            if (animancer != null)
                return animancer;
            else
                return animator.AddAnimancerComponent<T>();
        }

        /************************************************************************************************************************/

        /// <summary>Returns true if the `node` is not null and <see cref="AnimancerNode.IsValid"/>.</summary>
        public static bool IsValid(this AnimancerNode node) => node != null && node.IsValid;

        /// <summary>Returns true if the `transition` is not null and <see cref="ITransitionDetailed.IsValid"/>.</summary>
        public static bool IsValid(this ITransitionDetailed transition) => transition != null && transition.IsValid;

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="ITransition.CreateState"/> and <see cref="ITransition.Apply"/>.</summary>
        public static AnimancerState CreateStateAndApply(this ITransition transition, AnimancerPlayable root = null)
        {
            var state = transition.CreateState();
            state.SetRoot(root);
            transition.Apply(state);
            return state;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a <see cref="NativeArray{T}"/> containing a single element so that it can be used like a reference
        /// in Unity's C# Job system which does not allow regular reference types.
        /// <para></para>
        /// Note that you must call <see cref="NativeArray{T}.Dispose()"/> when you are done with the array.
        /// </summary>
        public static NativeArray<T> CreateNativeReference<T>() where T : struct
        {
            return new NativeArray<T>(1, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only] Reconnects the input of the specified `playable` to its output.</summary>
        public static void RemovePlayable(Playable playable, bool destroy = true)
        {
            if (!playable.IsValid())
                return;

            Debug.Assert(playable.GetInputCount() == 1,
                $"{nameof(RemovePlayable)} can only be used on playables with 1 input.");
            Debug.Assert(playable.GetOutputCount() == 1,
                $"{nameof(RemovePlayable)} can only be used on playables with 1 output.");

            var input = playable.GetInput(0);
            if (!input.IsValid())
            {
                if (destroy)
                    playable.Destroy();
                return;
            }

            var graph = playable.GetGraph();
            var output = playable.GetOutput(0);

            if (output.IsValid())// Connected to another Playable.
            {
                if (destroy)
                {
                    playable.Destroy();
                }
                else
                {
                    Debug.Assert(output.GetInputCount() == 1,
                        $"{nameof(RemovePlayable)} can only be used on playables connected to a playable with 1 input.");
                    graph.Disconnect(output, 0);
                    graph.Disconnect(playable, 0);
                }

                graph.Connect(input, 0, output, 0);
            }
            else// Connected to the graph output.
            {
                Debug.Assert(graph.GetOutput(0).GetSourcePlayable().Equals(playable),
                    $"{nameof(RemovePlayable)} can only be used on playables connected to another playable or to the graph output.");

                if (destroy)
                    playable.Destroy();
                else
                    graph.Disconnect(playable, 0);

                graph.GetOutput(0).SetSourcePlayable(input);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Checks if any <see cref="AnimationClip"/> in the `source` has an animation event with the specified
        /// `functionName`.
        /// </summary>
        public static bool HasEvent(IAnimationClipCollection source, string functionName)
        {
            var clips = ObjectPool.AcquireSet<AnimationClip>();
            source.GatherAnimationClips(clips);

            foreach (var clip in clips)
            {
                if (HasEvent(clip, functionName))
                {
                    ObjectPool.Release(clips);
                    return true;
                }
            }

            ObjectPool.Release(clips);
            return false;
        }

        /// <summary>Checks if the `clip` has an animation event with the specified `functionName`.</summary>
        public static bool HasEvent(AnimationClip clip, string functionName)
        {
            var events = clip.events;
            for (int i = events.Length - 1; i >= 0; i--)
            {
                if (events[i].functionName == functionName)
                    return true;
            }

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Calculates all thresholds in the `mixer` using the <see cref="AnimancerState.AverageVelocity"/> of each
        /// state on the X and Z axes.
        /// <para></para>
        /// Note that this method requires the <c>Root Transform Position (XZ) -> Bake Into Pose</c> toggle to be
        /// disabled in the Import Settings of each <see cref="AnimationClip"/> in the mixer.
        /// </summary>
        public static void CalculateThresholdsFromAverageVelocityXZ(this MixerState<Vector2> mixer)
        {
            mixer.ValidateThresholdCount();

            for (int i = mixer.ChildCount - 1; i >= 0; i--)
            {
                var state = mixer.GetChild(i);
                if (state == null)
                    continue;

                var averageVelocity = state.AverageVelocity;
                mixer.SetThreshold(i, new Vector2(averageVelocity.x, averageVelocity.z));
            }
        }

        /************************************************************************************************************************/

        /// <summary>Gets the value of the `parameter` in the `animator`.</summary>
        public static object GetParameterValue(Animator animator, AnimatorControllerParameter parameter)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    return animator.GetFloat(parameter.nameHash);

                case AnimatorControllerParameterType.Int:
                    return animator.GetInteger(parameter.nameHash);

                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    return animator.GetBool(parameter.nameHash);

                default:
                    throw new ArgumentException($"Unsupported {nameof(AnimatorControllerParameterType)}: " + parameter.type);
            }
        }

        /// <summary>Gets the value of the `parameter` in the `playable`.</summary>
        public static object GetParameterValue(AnimatorControllerPlayable playable, AnimatorControllerParameter parameter)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    return playable.GetFloat(parameter.nameHash);

                case AnimatorControllerParameterType.Int:
                    return playable.GetInteger(parameter.nameHash);

                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    return playable.GetBool(parameter.nameHash);

                default:
                    throw new ArgumentException($"Unsupported {nameof(AnimatorControllerParameterType)}: " + parameter.type);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Sets the `value` of the `parameter` in the `animator`.</summary>
        public static void SetParameterValue(Animator animator, AnimatorControllerParameter parameter, object value)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(parameter.nameHash, (float)value);
                    break;

                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(parameter.nameHash, (int)value);
                    break;

                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(parameter.nameHash, (bool)value);
                    break;

                case AnimatorControllerParameterType.Trigger:
                    if ((bool)value)
                        animator.SetTrigger(parameter.nameHash);
                    else
                        animator.ResetTrigger(parameter.nameHash);
                    break;

                default:
                    throw new ArgumentException($"Unsupported {nameof(AnimatorControllerParameterType)}: " + parameter.type);
            }
        }

        /// <summary>Sets the `value` of the `parameter` in the `playable`.</summary>
        public static void SetParameterValue(AnimatorControllerPlayable playable, AnimatorControllerParameter parameter, object value)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    playable.SetFloat(parameter.nameHash, (float)value);
                    break;

                case AnimatorControllerParameterType.Int:
                    playable.SetInteger(parameter.nameHash, (int)value);
                    break;

                case AnimatorControllerParameterType.Bool:
                    playable.SetBool(parameter.nameHash, (bool)value);
                    break;

                case AnimatorControllerParameterType.Trigger:
                    if ((bool)value)
                        playable.SetTrigger(parameter.nameHash);
                    else
                        playable.ResetTrigger(parameter.nameHash);
                    break;

                default:
                    throw new ArgumentException($"Unsupported {nameof(AnimatorControllerParameterType)}: " + parameter.type);
            }
        }

        /************************************************************************************************************************/
        #region Editor
        /************************************************************************************************************************/

        /// <summary>[Editor-Conditional] Indicates that the `target` needs to be re-serialized.</summary>
        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public static void SetDirty(Object target)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(target);
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Conditional]
        /// Plays the specified `clip` if called in Edit Mode and optionally pauses it immediately.
        /// </summary>
        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public static void EditModePlay(IAnimancerComponent animancer, AnimationClip clip, bool pauseImmediately = true)
        {
#if UNITY_EDITOR
            if (!ShouldPlay())
                return;

            // Delay for a frame in case this was called at a bad time (such as during OnValidate).
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (!ShouldPlay())
                    return;

                animancer.Playable.Play(clip);

                if (pauseImmediately)
                {
                    animancer.Playable.Evaluate();
                    animancer.Playable.PauseGraph();
                }
            };

            bool ShouldPlay()
            {
                if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode ||
                    animancer == null ||
                    clip == null)
                    return false;

                var obj = animancer as Object;
                if (!(obj is null) && obj == null)
                    return false;

                return true;
            }
#endif
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

