// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>
    /// Enforces various rules throughout the system, most of which are compiled out if UNITY_ASSERTIONS is not defined
    /// (by default, it is only defined in the Unity Editor and in Development Builds).
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/Validate
    /// 
    public static partial class Validate
    {
        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Throws if the `clip` is marked as <see cref="AnimationClip.legacy"/>.</summary>
        /// <exception cref="ArgumentException"/>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertNotLegacy(AnimationClip clip)
        {
#if UNITY_ASSERTIONS
            if (clip.legacy)
                throw new ArgumentException($"Legacy clip '{clip.name}' cannot be used by Animancer." +
                    " Set the legacy property to false before using this clip." +
                    " If it was imported as part of a model then the model's Rig type must be changed to Humanoid or Generic." +
                    " Otherwise you can use the 'Toggle Legacy' function in the clip's context menu" +
                    " (via the cog icon in the top right of its Inspector).");
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Throws if the <see cref="AnimancerNode.Root"/> is not the `root`.</summary>
        /// <exception cref="ArgumentException"/>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertRoot(AnimancerNode node, AnimancerPlayable root)
        {
#if UNITY_ASSERTIONS
            if (node.Root != root)
                throw new ArgumentException($"{nameof(AnimancerNode)}.{nameof(AnimancerNode.Root)} mismatch:" +
                    $" cannot use a node in an {nameof(AnimancerPlayable)} that is not its {nameof(AnimancerNode.Root)}: " +
                    node.GetDescription());
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Throws if the state's <see cref="Playable"/> is invalid.</summary>
        /// <exception cref="InvalidOperationException"/>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertPlayable(AnimancerNode node)
        {
#if UNITY_ASSERTIONS
            if (node._Playable.IsValid())
                return;

            if (node.Root == null)
                throw new InvalidOperationException($"{nameof(AnimancerNode)}.{nameof(AnimancerNode.Root)} hasn't been set so it's" +
                    $" {nameof(Playable)} hasn't been created. It can be set by playing the state" +
                    $" or calling {nameof(AnimancerState.SetRoot)} on it directly." +
                    $" {nameof(AnimancerState.SetParent)} would also work if the parent has a {nameof(AnimancerNode.Root)}." +
                    $"\nState: {node}");
            else
                throw new InvalidOperationException($"{nameof(AnimancerNode)}.{nameof(IPlayableWrapper.Playable)} has not been created." +
                    $" {nameof(AnimancerNode.CreatePlayable)} likely needs to be called on it before performing this operation." +
                    $"\nState: {node}");
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional]
        /// Throws if the `state` was not actually assigned to its specified <see cref="AnimancerNode.Index"/> in
        /// the `states`.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="IndexOutOfRangeException">
        /// The <see cref="AnimancerNode.Index"/> is larger than the number of `states`.
        /// </exception>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertCanRemoveChild(AnimancerState state, IList<AnimancerState> states)
        {
#if UNITY_ASSERTIONS
            var index = state.Index;

            if (index < 0)
                throw new InvalidOperationException(
                    "Cannot remove a child state that did not have an Index assigned");

            if (index > states.Count)
                throw new IndexOutOfRangeException(
                    "AnimancerState.Index (" + state.Index + ") is outside the collection of states (count " + states.Count + ")");

            if (states[state.Index] != state)
                throw new InvalidOperationException(
                    "Cannot remove a child state that was not actually connected to its port on " + state.Parent + ":" +
                    "\n    Port: " + state.Index +
                    "\n    Connected Child: " + AnimancerUtilities.ToStringOrNull(states[state.Index]) +
                    "\n    Disconnecting Child: " + AnimancerUtilities.ToStringOrNull(state));
#endif
        }

        /************************************************************************************************************************/
    }
}

