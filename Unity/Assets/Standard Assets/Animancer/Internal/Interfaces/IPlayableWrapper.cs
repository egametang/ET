// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using UnityEngine;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>Interface for objects that manage a <see cref="UnityEngine.Playables.Playable"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/IPlayableWrapper
    /// 
    public interface IPlayableWrapper
    {
        /************************************************************************************************************************/

        /// <summary>The object which receives the output of the <see cref="Playable"/>.</summary>
        IPlayableWrapper Parent { get; }

        /// <summary>The <see cref="UnityEngine.Playables.Playable"/> managed by this object.</summary>
        Playable Playable { get; }

        /// <summary>The number of nodes using this object as their <see cref="Parent"/>.</summary>
        int ChildCount { get; }

        /// <summary>Returns the state connected to the specified `index` as a child of this object.</summary>
        AnimancerNode GetChild(int index);

        /// <summary>
        /// Indicates whether child playables should stay connected to the graph at all times.
        /// <para></para>
        /// If false, playables will be disconnected from the graph while they are at 0 weight to stop it from
        /// evaluating them every frame.
        /// </summary>
        /// <seealso cref="AnimancerPlayable.KeepChildrenConnected"/>
        bool KeepChildrenConnected { get; }

        /// <summary>How fast the <see cref="AnimancerState.Time"/> is advancing every frame.</summary>
        /// 
        /// <remarks>
        /// 1 is the normal speed.
        /// <para></para>
        /// A negative value will play the animation backwards.
        /// <para></para>
        /// <em>Animancer Lite does not allow this value to be changed in runtime builds.</em>
        /// </remarks>
        ///
        /// <example>
        /// <code>
        /// void PlayAnimation(AnimancerComponent animancer, AnimationClip clip)
        /// {
        ///     var state = animancer.Play(clip);
        ///
        ///     state.Speed = 1;// Normal speed.
        ///     state.Speed = 2;// Double speed.
        ///     state.Speed = 0.5f;// Half speed.
        ///     state.Speed = -1;// Normal speed playing backwards.
        /// }
        /// </code>
        /// </example>
        float Speed { get; set; }

        /************************************************************************************************************************/

        /// <summary>
        /// Should Unity call <c>OnAnimatorIK</c> on the animated object while this object and its children have any
        /// <see cref="AnimancerNode.Weight"/>?
        /// </summary>
        /// <remarks>
        /// This is equivalent to the "IK Pass" toggle in Animator Controller layers, except that due to limitations in
        /// the Playables API the <c>layerIndex</c> will always be zero.
        /// <para></para>
        /// This value starts false by default, but can be automatically changed by
        /// <see cref="AnimancerNode.CopyIKFlags"/> when the <see cref="Parent"/> is set.
        /// <para></para>
        /// IK only takes effect while at least one <see cref="ClipState"/> has a <see cref="AnimancerNode.Weight"/>
        /// above zero. Other node types either store the value to apply to their children or don't support IK.
        /// </remarks>
        bool ApplyAnimatorIK { get; set; }

        /************************************************************************************************************************/

        /// <summary>Should this object and its children apply IK to the character's feet?</summary>
        /// <remarks>
        /// This is equivalent to the "Foot IK" toggle in Animator Controller states.
        /// <para></para>
        /// This value starts true by default for <see cref="ClipState"/>s (false for others), but can be automatically
        /// changed by <see cref="AnimancerNode.CopyIKFlags"/> when the <see cref="Parent"/> is set.
        /// <para></para>
        /// IK only takes effect while at least one <see cref="ClipState"/> has a <see cref="AnimancerNode.Weight"/>
        /// above zero. Other node types either store the value to apply to their children or don't support IK.
        /// </remarks>
        bool ApplyFootIK { get; set; }

        /************************************************************************************************************************/
    }
}

/************************************************************************************************************************/
#if UNITY_EDITOR
/************************************************************************************************************************/

namespace Animancer.Editor
{
    partial class AnimancerEditorUtilities
    {
        /************************************************************************************************************************/

        /// <summary>
        /// Adds functions to show and set <see cref="IPlayableWrapper.ApplyAnimatorIK"/> and
        /// <see cref="IPlayableWrapper.ApplyFootIK"/>.
        /// </summary>
        public static void AddContextMenuIK(UnityEditor.GenericMenu menu, IPlayableWrapper ik)
        {
            menu.AddItem(new GUIContent("Inverse Kinematics/Apply Animator IK ?"),
                ik.ApplyAnimatorIK,
                () => ik.ApplyAnimatorIK = !ik.ApplyAnimatorIK);
            menu.AddItem(new GUIContent("Inverse Kinematics/Apply Foot IK ?"),
                ik.ApplyFootIK,
                () => ik.ApplyFootIK = !ik.ApplyFootIK);
        }

        /************************************************************************************************************************/
    }
}

/************************************************************************************************************************/
#endif
/************************************************************************************************************************/

