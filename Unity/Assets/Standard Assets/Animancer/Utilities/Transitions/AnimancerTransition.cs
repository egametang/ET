// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace Animancer
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> based <see cref="ITransition"/>s which can create a
    /// <see cref="ClipState"/> when passed into <see cref="AnimancerPlayable.Play(ITransition)"/>.
    /// </summary>
    /// <remarks>
    /// When adding a <see cref="CreateAssetMenuAttribute"/> to any derived classes, you can use
    /// <see cref="Strings.MenuPrefix"/> and <see cref="Strings.AssetMenuOrder"/>.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions/types#transition-assets">Transition Assets</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerTransition
    /// 
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(AnimancerTransition))]
    public abstract class AnimancerTransition : ScriptableObject, ITransition, IAnimationClipSource
    {
        /************************************************************************************************************************/

        /// <summary>Returns the <see cref="ITransition"/> wrapped by this <see cref="ScriptableObject"/>.</summary>
        public abstract ITransition GetTransition();

        /************************************************************************************************************************/

        /// <summary>Wraps <see cref="ITransition.FadeDuration"/>.</summary>
        public virtual float FadeDuration => GetTransition().FadeDuration;

        /// <summary>Wraps <see cref="IHasKey.Key"/>.</summary>
        public virtual object Key => GetTransition().Key;

        /// <summary>Wraps <see cref="ITransition.FadeMode"/>.</summary>
        public virtual FadeMode FadeMode => GetTransition().FadeMode;

        /// <summary>Wraps <see cref="ITransition.CreateState"/>.</summary>
        public virtual AnimancerState CreateState() => GetTransition().CreateState();

        /// <summary>Wraps <see cref="ITransition.Apply"/>.</summary>
        public virtual void Apply(AnimancerState state)
        {
            GetTransition().Apply(state);
            state.SetDebugName(name);
        }

        /************************************************************************************************************************/

        /// <summary>Wraps <see cref="AnimancerUtilities.GatherFromSource(ICollection{AnimationClip}, object)"/>.</summary>
        public virtual void GetAnimationClips(List<AnimationClip> clips) => clips.GatherFromSource(GetTransition());

        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/

    /// <summary>An <see cref="AnimancerTransition"/> which uses a generic field for its <see cref="ITransition"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions/types#transition-assets">Transition Assets</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerTransition_1
    /// 
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(AnimancerTransition) + "_1")]
    public class AnimancerTransition<T> : AnimancerTransition where T : ITransition
    {
        /************************************************************************************************************************/

        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("_Animation")]
        private T _Transition;

        /// <summary>[<see cref="SerializeField"/>]
        /// The <see cref="ITransition"/> wrapped by this <see cref="ScriptableObject"/>.
        /// </summary>
        /// <remarks>
        /// WARNING: the <see cref="AnimancerState.Transition{TState}.State"/> holds the most recently played state, so
        /// if you are sharing this transition between multiple objects it will only remember one of them.
        /// <para></para>
        /// You can use <see cref="AnimancerPlayable.StateDictionary.GetOrCreate(ITransition)"/> or
        /// <see cref="AnimancerLayer.GetOrCreateState(ITransition)"/> to get or create the state for a
        /// specific object.
        /// </remarks>
        public ref T Transition => ref _Transition;

        /// <inheritdoc/>
        public override ITransition GetTransition() => _Transition;

        /************************************************************************************************************************/
    }
}

/************************************************************************************************************************/

#if UNITY_EDITOR
namespace Animancer.Editor
{
    /// <summary>A custom editor for <see cref="AnimancerTransition"/>s.</summary>
    /// <remarks>
    /// This class contains several context menu functions for generating <see cref="AnimancerTransition"/>s based on
    /// Animator Controller States.
    /// </remarks>
    [CustomEditor(typeof(AnimancerTransition), true)]
    internal class AnimancerTransitionEditor : ScriptableObjectEditor
    {
        /************************************************************************************************************************/

        /// <summary>Creates an <see cref="AnimancerTransition"/> from the <see cref="MenuCommand.context"/>.</summary>
        [MenuItem("CONTEXT/" + nameof(AnimatorState) + "/Generate Transition")]
        [MenuItem("CONTEXT/" + nameof(BlendTree) + "/Generate Transition")]
        [MenuItem("CONTEXT/" + nameof(AnimatorStateTransition) + "/Generate Transition")]
        [MenuItem("CONTEXT/" + nameof(AnimatorStateMachine) + "/Generate Transitions")]
        private static void GenerateTransition(MenuCommand command)
        {
            if (command.context is AnimatorState state)
            {
                Selection.activeObject = GenerateTransition(state);
            }
            else if (command.context is BlendTree blendTree)
            {
                Selection.activeObject = GenerateTransition(null, blendTree);
            }
            else if (command.context is AnimatorStateTransition transition)
            {
                Selection.activeObject = GenerateTransition(transition);
            }
            else if (command.context is AnimatorStateMachine stateMachine)// Layer or Sub-State Machine.
            {
                Selection.activeObject = GenerateTransitions(stateMachine);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Creates an <see cref="AnimancerTransition"/> from the `state`.</summary>
        private static AnimancerTransition GenerateTransition(AnimatorState state)
            => GenerateTransition(state, state.motion);

        /************************************************************************************************************************/

        /// <summary>Creates an <see cref="AnimancerTransition"/> from the `motion`.</summary>
        private static AnimancerTransition GenerateTransition(Object originalAsset, Motion motion)
        {
            if (motion is BlendTree blendTree)
            {
                return GenerateTransition(originalAsset as AnimatorState, blendTree);
            }
            else if (motion is AnimationClip || motion == null)
            {
                var asset = CreateInstance<ClipTransition>();
                asset.Transition = new ClipState.Transition
                {
                    Clip = (AnimationClip)motion,
                };

                GetDetailsFromState(originalAsset as AnimatorState, asset.Transition);
                SaveTransition(originalAsset, asset);
                return asset;
            }
            else
            {
                Debug.LogError($"Unsupported {nameof(Motion)} Type: {motion.GetType()}");
                return null;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Initialises the `transition` based on the `state`.</summary>
        private static void GetDetailsFromState(AnimatorState state, ITransitionDetailed transition)
        {
            if (state == null)
                return;

            transition.Speed = state.speed;

            var isForwards = state.speed >= 0;
            var defaultEndTime = AnimancerEvent.Sequence.GetDefaultNormalizedEndTime(state.speed);
            var endTime = defaultEndTime;

            var exitTransitions = state.transitions;
            for (int i = 0; i < exitTransitions.Length; i++)
            {
                var exitTransition = exitTransitions[i];
                if (exitTransition.hasExitTime)
                {
                    if (isForwards)
                    {
                        if (endTime > exitTransition.exitTime)
                            endTime = exitTransition.exitTime;
                    }
                    else
                    {
                        if (endTime < exitTransition.exitTime)
                            endTime = exitTransition.exitTime;
                    }
                }
            }

            if (endTime != defaultEndTime)
            {
                transition.SerializedEvents = new AnimancerEvent.Sequence.Serializable();
                transition.SerializedEvents.SetNormalizedEndTime(endTime);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Creates an <see cref="AnimancerTransition"/> from the `blendTree`.</summary>
        private static AnimancerTransition GenerateTransition(AnimatorState state, BlendTree blendTree)
        {
            var asset = CreateTransition(blendTree);
            if (asset == null)
                return null;

            if (state != null)
                asset.name = state.name;

            GetDetailsFromState(state, (ITransitionDetailed)asset.GetTransition());
            SaveTransition(blendTree, asset);
            return asset;
        }

        /************************************************************************************************************************/

        /// <summary>Creates an <see cref="AnimancerTransition"/> from the `transition`.</summary>
        private static AnimancerTransition GenerateTransition(AnimatorStateTransition transition)
        {
            AnimancerTransition animancerTransition = null;

            if (transition.destinationStateMachine != null)
                animancerTransition = GenerateTransitions(transition.destinationStateMachine);

            if (transition.destinationState != null)
                animancerTransition = GenerateTransition(transition.destinationState);

            return animancerTransition;
        }

        /************************************************************************************************************************/

        /// <summary>Creates <see cref="AnimancerTransition"/>s from all states in the `stateMachine`.</summary>
        private static AnimancerTransition GenerateTransitions(AnimatorStateMachine stateMachine)
        {
            AnimancerTransition transition = null;

            foreach (var child in stateMachine.stateMachines)
                transition = GenerateTransitions(child.stateMachine);

            foreach (var child in stateMachine.states)
                transition = GenerateTransition(child.state);

            return transition;
        }

        /************************************************************************************************************************/

        /// <summary>Creates an <see cref="AnimancerTransition"/> from the `blendTree`.</summary>
        private static AnimancerTransition CreateTransition(BlendTree blendTree)
        {
            switch (blendTree.blendType)
            {
                case BlendTreeType.Simple1D:
                    var linearAsset = CreateInstance<LinearMixerTransition>();
                    InitialiseChildren(ref linearAsset.Transition, blendTree);
                    return linearAsset;

                case BlendTreeType.SimpleDirectional2D:
                case BlendTreeType.FreeformDirectional2D:
                    var directionalAsset = CreateInstance<MixerTransition2D>();
                    directionalAsset.Transition = new MixerState.Transition2D
                    {
                        Type = MixerState.Transition2D.MixerType.Directional
                    };
                    InitialiseChildren(ref directionalAsset.Transition, blendTree);
                    return directionalAsset;

                case BlendTreeType.FreeformCartesian2D:
                    var cartesianAsset = CreateInstance<MixerTransition2D>();
                    cartesianAsset.Transition = new MixerState.Transition2D
                    {
                        Type = MixerState.Transition2D.MixerType.Cartesian
                    };
                    InitialiseChildren(ref cartesianAsset.Transition, blendTree);
                    return cartesianAsset;

                case BlendTreeType.Direct:
                    var manualAsset = CreateInstance<ManualMixerTransition>();
                    InitialiseChildren<ManualMixerState.Transition, ManualMixerState>(ref manualAsset.Transition, blendTree);
                    return manualAsset;

                default:
                    Debug.LogError($"Unsupported {nameof(BlendTreeType)}: {blendTree.blendType}");
                    return null;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Initialises the `transition` based on the <see cref="BlendTree.children"/>.</summary>
        private static void InitialiseChildren(ref LinearMixerState.Transition transition, BlendTree blendTree)
        {
            var children = InitialiseChildren<LinearMixerState.Transition, LinearMixerState>(ref transition, blendTree);
            transition.Thresholds = new float[children.Length];
            for (int i = 0; i < children.Length; i++)
                transition.Thresholds[i] = children[i].threshold;
        }

        /// <summary>Initialises the `transition` based on the <see cref="BlendTree.children"/>.</summary>
        private static void InitialiseChildren(ref MixerState.Transition2D transition, BlendTree blendTree)
        {
            var children = InitialiseChildren<MixerState.Transition2D, MixerState<Vector2>>(ref transition, blendTree);
            transition.Thresholds = new Vector2[children.Length];
            for (int i = 0; i < children.Length; i++)
                transition.Thresholds[i] = children[i].position;
        }

        /// <summary>Initialises the `transition` based on the <see cref="BlendTree.children"/>.</summary>
        private static ChildMotion[] InitialiseChildren<TTransition, TState>(ref TTransition transition, BlendTree blendTree)
            where TTransition : ManualMixerState.Transition<TState>, new()
            where TState : ManualMixerState
        {
            transition = new TTransition();

            var children = blendTree.children;
            transition.States = new Object[children.Length];
            float[] speeds = new float[children.Length];
            var hasCustomSpeeds = false;

            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                transition.States[i] = child.motion is AnimationClip ?
                    child.motion :
                    (Object)GenerateTransition(blendTree, child.motion);

                if ((speeds[i] = child.timeScale) != 1)
                    hasCustomSpeeds = true;
            }

            if (hasCustomSpeeds)
                transition.Speeds = speeds;

            return children;
        }

        /************************************************************************************************************************/

        /// <summary>Saves the `transition` in the same folder as the `originalAsset`.</summary>
        private static void SaveTransition(Object originalAsset, AnimancerTransition transition)
        {
            if (string.IsNullOrEmpty(transition.name))
                transition.name = originalAsset.name;

            var path = AssetDatabase.GetAssetPath(originalAsset);
            path = Path.GetDirectoryName(path);
            path = Path.Combine(path, transition.name + ".asset");
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(transition, path);

            Debug.Log($"Saved {path}", transition);
        }

        /************************************************************************************************************************/
    }
}
#endif
