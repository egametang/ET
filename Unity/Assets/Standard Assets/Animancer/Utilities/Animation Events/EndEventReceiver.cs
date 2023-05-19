// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using Animancer.Editor;
using UnityEngine;

namespace Animancer
{
    /// <summary>
    /// A component which uses Animation Events with the Function Name "End" to invoke the
    /// <see cref="AnimancerEvent.Sequence.endEvent"/> of the <see cref="AnimancerState"/> that triggered the event.
    /// </summary>
    /// <remarks>
    /// This component must always be attached to the same <see cref="GameObject"/> as the <see cref="Animator"/> in
    /// order to receive Animation Events from it.
    /// <para></para>
    /// Note that Unity will allocate some garbage every time it triggers an Animation Event with an
    /// <see cref="AnimationEvent"/> parameter.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/events/animation#end-animation-events">End Animation Events</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/EndEventReceiver
    /// 
    [AddComponentMenu(Strings.MenuPrefix + "End Event Receiver")]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(EndEventReceiver))]
    public class EndEventReceiver : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField]
        private AnimancerComponent _Animancer;

        /// <summary>[<see cref="SerializeField"/>]
        /// The <see cref="AnimancerComponent"/> on which the <see cref="AnimancerEvent.Sequence.endEvent"/> will be
        /// triggered.
        /// </summary>
        public ref AnimancerComponent Animancer => ref _Animancer;

        /************************************************************************************************************************/

        /// <summary>
        /// The <see cref="AnimationEvent"/> called 'End' which is currently being triggered.
        /// </summary>
        public static AnimationEvent CurrentEvent { get; private set; }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="End(AnimancerComponent, AnimationEvent)"/>.</summary>
        /// <remarks>Called by Animation Events with the Function Name "End".</remarks>
        private void End(AnimationEvent animationEvent)
        {
            End(_Animancer, animationEvent);
        }

        /// <summary>
        /// Tries to invoke the <see cref="AnimancerEvent.Sequence.endEvent"/> of the <see cref="AnimancerState"/> that
        /// triggered the `animationEvent`.
        /// </summary>
        /// <remarks>
        /// Note that Unity will allocate some garbage every time it triggers an Animation Event with an
        /// <see cref="AnimationEvent"/> parameter.
        /// </remarks>
        public static bool End(AnimancerComponent animancer, AnimationEvent animationEvent)
        {
            if (!animancer.IsPlayableInitialised)
            {
                // This could only happen if another Animator triggers the event on this object somehow.
                Debug.LogWarning($"{nameof(AnimationEvent)} '{nameof(End)}' was triggered by {animationEvent.animatorClipInfo.clip}" +
                    $", but the {nameof(AnimancerComponent)}.{nameof(AnimancerComponent.Playable)} hasn't been initialised.",
                    animancer);
                return false;
            }

            var layers = animancer.Layers;
            var count = layers.Count;

            // Try targeting the current state on each layer first.
            for (int i = 0; i < count; i++)
            {
                if (TryInvokeOnEndEventRecursive(layers[i].CurrentState, animationEvent))
                    return true;
            }

            // Otherwise try every state.
            for (int i = 0; i < count; i++)
            {
                if (TryInvokeOnEndEventRecursive(layers[i], animationEvent))
                    return true;
            }

            if (animationEvent.messageOptions == SendMessageOptions.RequireReceiver)
            {
                Debug.LogWarning($"{nameof(AnimationEvent)} '{nameof(End)}' was triggered by {animationEvent.animatorClipInfo.clip}" +
                    $", but no state was found with that {nameof(AnimancerState.Key)}.",
                    animancer);
            }

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Invokes the <see cref="AnimancerEvent.Sequence.OnEnd"/> callback of the state that is playing the animation
        /// which triggered the event. Returns true if such a state exists (even if it doesn't have a callback).
        /// </summary>
        private static bool OnEndEventReceived(AnimancerPlayable animancer, AnimationEvent animationEvent)
        {
            var layers = animancer.Layers;
            var count = layers.Count;

            // Try targeting the current state on each layer first.
            for (int i = 0; i < count; i++)
            {
                if (TryInvokeOnEndEventRecursive(layers[i].CurrentState, animationEvent))
                    return true;
            }

            // Otherwise try every state.
            for (int i = 0; i < count; i++)
            {
                if (TryInvokeOnEndEventRecursive(layers[i], animationEvent))
                    return true;
            }

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Invokes the <see cref="AnimancerEvent.Sequence.OnEnd"/> callback of the state that is playing the animation
        /// which triggered the event. Returns true if such a state exists (even if it doesn't have a callback).
        /// </summary>
        private static bool TryInvokeOnEndEventRecursive(AnimancerNode node, AnimationEvent animationEvent)
        {
            var childCount = node.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = node.GetChild(i);
                if (child != null &&
                    (TryInvokeOnEndEvent(child, animationEvent) ||
                    TryInvokeOnEndEventRecursive(child, animationEvent)))
                    return true;
            }

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="AnimancerState.Clip"/> and <see cref="AnimancerNode.Weight"/> match the
        /// <see cref="AnimationEvent"/>, this method invokes the <see cref="AnimancerEvent.Sequence.OnEnd"/> callback
        /// and returns true.
        /// </summary>
        private static bool TryInvokeOnEndEvent(AnimancerState state, AnimationEvent animationEvent)
        {
            if (state.Weight != animationEvent.animatorClipInfo.weight ||
                state.Clip != animationEvent.animatorClipInfo.clip ||
                !state.HasEvents)
                return false;

            var endEvent = state.Events.endEvent;
            if (endEvent.callback != null)
            {
                Debug.Assert(CurrentEvent == null, $"Recursive call to {nameof(TryInvokeOnEndEvent)} detected");

                try
                {
                    CurrentEvent = animationEvent;
                    endEvent.Invoke(state);
                }
                finally
                {
                    CurrentEvent = null;
                }
            }

            return true;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="CurrentEvent"/> has a float parameter above 0, this method returns that value.
        /// Otherwise this method calls <see cref="AnimancerEvent.GetFadeOutDuration"/> so if you aren't using an
        /// Animation Event with the function name "End" you can just call that method directly.
        /// </summary>
        public static float GetFadeOutDuration(float minDuration = AnimancerPlayable.DefaultFadeDuration)
        {
            if (CurrentEvent != null && CurrentEvent.floatParameter > 0)
                return CurrentEvent.floatParameter;

            return AnimancerEvent.GetFadeOutDuration(minDuration);
        }

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only]
        /// Called when this component is first added in Edit Mode.
        /// Finds the <see cref="Animancer"/> using <see cref="AnimancerEditorUtilities.GetComponentInHierarchy"/>.
        /// </summary>
        protected virtual void Reset()
        {
            _Animancer = AnimancerEditorUtilities.GetComponentInHierarchy<AnimancerComponent>(gameObject);
        }
#endif

        /************************************************************************************************************************/
    }
}
