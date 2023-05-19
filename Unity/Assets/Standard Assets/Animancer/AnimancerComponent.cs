// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>
    /// The main component through which other scripts can interact with <see cref="Animancer"/>. It allows you to play
    /// animations on an <see cref="UnityEngine.Animator"/> without using a <see cref="RuntimeAnimatorController"/>.
    /// </summary>
    /// <remarks>
    /// This class can be used as a custom yield instruction to wait until all animations finish playing.
    /// <para></para>
    /// This class is mostly just a wrapper that connects an <see cref="AnimancerPlayable"/> to an
    /// <see cref="UnityEngine.Animator"/>.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/component-types">Component Types</see>
    /// </remarks>
    [AddComponentMenu(Strings.MenuPrefix + "Animancer Component")]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(AnimancerComponent))]
    [DefaultExecutionOrder(-5000)]// Initialise before anything else tries to use this component.
    public class AnimancerComponent : MonoBehaviour,
        IAnimancerComponent, IEnumerable<AnimancerState>, IEnumerator, IAnimationClipSource, IAnimationClipCollection
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        [SerializeField, Tooltip("The Animator component which this script controls")]
        private Animator _Animator;

        /// <summary>[<see cref="SerializeField"/>]
        /// The <see cref="UnityEngine.Animator"/> component which this script controls.
        /// </summary>
        public Animator Animator
        {
            get => _Animator;
            set
            {
                // It doesn't seem to be possible to stop the old Animator from playing the graph.

                _Animator = value;
                if (IsPlayableInitialised)
                    _Playable.SetOutput(value, this);
            }
        }

#if UNITY_EDITOR
        /// <summary>[Editor-Only] The name of the serialized backing field for the <see cref="Animator"/> property.</summary>
        string IAnimancerComponent.AnimatorFieldName => nameof(_Animator);
#endif

        /************************************************************************************************************************/

        private AnimancerPlayable _Playable;

        /// <summary>
        /// The internal system which manages the playing animations.
        /// Accessing this property will automatically initialise it.
        /// </summary>
        public AnimancerPlayable Playable
        {
            get
            {
                InitialisePlayable();
                return _Playable;
            }
        }

        /// <summary>Indicates whether the <see cref="Playable"/> has been initialised.</summary>
        public bool IsPlayableInitialised => _Playable != null && _Playable.IsValid;

        /************************************************************************************************************************/

        /// <summary>The states managed by this component.</summary>
        public AnimancerPlayable.StateDictionary States => Playable.States;

        /// <summary>The layers which each manage their own set of animations.</summary>
        public AnimancerPlayable.LayerList Layers => Playable.Layers;

        /// <summary>Returns the <see cref="Playable"/>.</summary>
        public static implicit operator AnimancerPlayable(AnimancerComponent animancer) => animancer.Playable;

        /// <summary>Returns layer 0.</summary>
        public static implicit operator AnimancerLayer(AnimancerComponent animancer) => animancer.Playable.Layers[0];

        /************************************************************************************************************************/

        [SerializeField, Tooltip("Determines what happens when this component is disabled" +
            " or its " + nameof(GameObject) + " becomes inactive (i.e. in " + nameof(OnDisable) + "):" +
            "\n- " + nameof(DisableAction.Stop) + " all animations" +
            "\n- " + nameof(DisableAction.Pause) + " all animations" +
            "\n- " + nameof(DisableAction.Continue) + " playing" +
            "\n- " + nameof(DisableAction.Reset) + " to the original values" +
            "\n- " + nameof(DisableAction.Destroy) + " all layers and states")]
        private DisableAction _ActionOnDisable;

#if UNITY_EDITOR
        /// <summary>[Editor-Only] The name of the serialized backing field for the <see cref="ActionOnDisable"/> property.</summary>
        string IAnimancerComponent.ActionOnDisableFieldName => nameof(_ActionOnDisable);
#endif

        /// <summary>[<see cref="SerializeField"/>]
        /// Determines what happens when this component is disabled or its <see cref="GameObject"/> becomes inactive
        /// (i.e. in <see cref="OnDisable"/>).
        /// <para></para>
        /// The default value is <see cref="DisableAction.Stop"/>.
        /// </summary>
        public ref DisableAction ActionOnDisable => ref _ActionOnDisable;

        /// <summary>Determines whether the object will be reset to its original values when disabled.</summary>
        bool IAnimancerComponent.ResetOnDisable => _ActionOnDisable == DisableAction.Reset;

        /// <summary>
        /// An action to perform when disabling an <see cref="AnimancerComponent"/>. See <see cref="ActionOnDisable"/>.
        /// </summary>
        public enum DisableAction
        {
            /// <summary>
            /// Stop all animations and rewind them, but leave all animated values as they are (unlike
            /// <see cref="Reset"/>).
            /// <para></para>
            /// Calls <see cref="Stop()"/> and <see cref="AnimancerPlayable.PauseGraph"/>.
            /// </summary>
            Stop,

            /// <summary>
            /// Pause all animations in their current state so they can resume later.
            /// <para></para>
            /// Calls <see cref="AnimancerPlayable.PauseGraph"/>.
            /// </summary>
            Pause,

            /// <summary>Keep playing while inactive.</summary>
            Continue,

            /// <summary>
            /// Stop all animations, rewind them, and force the object back into its original state (often called the
            /// bind pose).
            /// <para></para>
            /// WARNING: this must occur before the <see cref="UnityEngine.Animator"/> receives its <c>OnDisable</c>
            /// call, meaning the <see cref="AnimancerComponent"/> must be above it in the Inspector or on a child
            /// object so that <see cref="OnDisable"/> gets called first.
            /// <para></para>
            /// Calls <see cref="Stop()"/>, <see cref="Animator.Rebind"/>, and <see cref="AnimancerPlayable.PauseGraph"/>.
            /// </summary>
            Reset,

            /// <summary>
            /// Destroy the <see cref="PlayableGraph"/> and all its layers and states. This means that any layers or
            /// states referenced by other scripts will no longer be valid so they will need to be recreated if you
            /// want to use this object again.
            /// <para></para>
            /// Calls <see cref="AnimancerPlayable.Destroy()"/>.
            /// </summary>
            Destroy,
        }

        /************************************************************************************************************************/
        #region Update Mode
        /************************************************************************************************************************/

        /// <summary>
        /// Determines when animations are updated and which time source is used. This property is mainly a wrapper
        /// around the <see cref="Animator.updateMode"/>.
        /// <para></para>
        /// Note that changing to or from <see cref="AnimatorUpdateMode.AnimatePhysics"/> at runtime has no effect.
        /// </summary>
        /// <exception cref="NullReferenceException">No <see cref="Animator"/> is assigned.</exception>
        public AnimatorUpdateMode UpdateMode
        {
            get => _Animator.updateMode;
            set
            {
                _Animator.updateMode = value;

                if (!IsPlayableInitialised)
                    return;

                // UnscaledTime on the Animator is actually identical to Normal when using the Playables API so we need
                // to set the graph's DirectorUpdateMode to determine how it gets its delta time.
                _Playable.UpdateMode = value == AnimatorUpdateMode.UnscaledTime ?
                    DirectorUpdateMode.UnscaledGameTime :
                    DirectorUpdateMode.GameTime;

#if UNITY_EDITOR
                if (InitialUpdateMode == null)
                {
                    InitialUpdateMode = value;
                }
                else if (UnityEditor.EditorApplication.isPlaying)
                {
                    if (AnimancerPlayable.HasChangedToOrFromAnimatePhysics(InitialUpdateMode, value))
                        Debug.LogWarning($"Changing the {nameof(Animator)}.{nameof(Animator.updateMode)}" +
                            $" to or from {nameof(AnimatorUpdateMode.AnimatePhysics)} at runtime will have no effect." +
                            " You must set it in the Unity Editor or on startup.", this);
                }
#endif
            }
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// The <see cref="UpdateMode"/> what was first used when this script initialised.
        /// This is used to give a warning when changing to or from <see cref="AnimatorUpdateMode.AnimatePhysics"/> at
        /// runtime since it won't work correctly.
        /// </summary>
        public AnimatorUpdateMode? InitialUpdateMode { get; private set; }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Initialisation
        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only]
        /// Destroys the <see cref="Playable"/> if it was initialised and searches for an <see cref="Animator"/> on
        /// this object, or it's children or parents.
        /// </summary>
        /// <remarks>
        /// Called by the Unity Editor when this component is first added (in Edit Mode) and whenever the Reset command
        /// is executed from its context menu.
        /// </remarks>
        protected virtual void Reset()
        {
            OnDestroy();
            _Animator = Editor.AnimancerEditorUtilities.GetComponentInHierarchy<Animator>(gameObject);
        }
#endif

        /************************************************************************************************************************/

        /// <summary>Ensures that the <see cref="PlayableGraph"/> is playing.</summary>
        /// <remarks>Called by Unity when this component becomes enabled and active.</remarks>
        protected virtual void OnEnable()
        {
            if (IsPlayableInitialised)
                _Playable.UnpauseGraph();
        }

        /// <summary>Acts according to the <see cref="ActionOnDisable"/>.</summary>
        /// <remarks>Called by Unity when this component becomes enabled and active.</remarks>
        protected virtual void OnDisable()
        {
            if (!IsPlayableInitialised)
                return;

            switch (_ActionOnDisable)
            {
                case DisableAction.Stop:
                    Stop();
                    _Playable.PauseGraph();
                    break;

                case DisableAction.Pause:
                    _Playable.PauseGraph();
                    break;

                case DisableAction.Continue:
                    break;

                case DisableAction.Reset:
                    Debug.Assert(_Animator.isActiveAndEnabled,
                        $"{nameof(DisableAction)}.{nameof(DisableAction.Reset)} failed because the {nameof(Animator)} is not enabled." +
                        $" This most likely means you are disabling the {nameof(GameObject)} and the {nameof(Animator)} is above the" +
                        $" {nameof(AnimancerComponent)} in the Inspector so it got disabled right before this method was called." +
                        $" See the Inspector of {this} to fix the issue" +
                        $" or use {nameof(DisableAction)}.{nameof(DisableAction.Stop)}" +
                        $" and call {nameof(Animator)}.{nameof(Animator.Rebind)} manually" +
                        $" before disabling the {nameof(GameObject)}.",
                        this);

                    Stop();
                    _Animator.Rebind();
                    _Playable.PauseGraph();
                    break;

                case DisableAction.Destroy:
                    _Playable.Destroy();
                    _Playable = null;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(ActionOnDisable));
            }
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="AnimancerPlayable"/> if it doesn't already exist.</summary>
        public void InitialisePlayable()
        {
            if (IsPlayableInitialised)
                return;

#if UNITY_ASSERTIONS
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
#endif
            {
                if (!gameObject.activeInHierarchy)
                    OptionalWarning.CreateGraphWhileDisabled.Log($"An {nameof(AnimancerPlayable)} is being created for '{this}'" +
                        $" which is attached to an inactive {nameof(GameObject)}." +
                        $" If that object is never activated then Unity will not call {nameof(OnDestroy)}" +
                        $" so {nameof(AnimancerPlayable)}.{nameof(AnimancerPlayable.Destroy)} will need to be called manually.", this);
            }

#if UNITY_EDITOR
            if (OptionalWarning.CreateGraphDuringGuiEvent.IsEnabled())
            {
                var currentEvent = Event.current;
                if (currentEvent != null && (currentEvent.type == EventType.Layout || currentEvent.type == EventType.Repaint))
                    OptionalWarning.CreateGraphDuringGuiEvent.Log(
                        $"Creating an {nameof(AnimancerPlayable)} during a {currentEvent.type} event is likely undesirable.", this);
            }
#endif
#endif

            if (_Animator == null)
                _Animator = GetComponent<Animator>();

            AnimancerPlayable.SetNextGraphName(name + " (Animancer)");
            _Playable = AnimancerPlayable.Create();
            _Playable.SetOutput(_Animator, this);

#if UNITY_EDITOR
            if (_Animator != null)
                InitialUpdateMode = UpdateMode;
#endif
        }

        /************************************************************************************************************************/

        /// <summary>Ensures that the <see cref="Playable"/> is properly cleaned up.</summary>
        /// <remarks>Called by Unity when this component is destroyed.</remarks>
        protected virtual void OnDestroy()
        {
            if (IsPlayableInitialised)
            {
                _Playable.Destroy();
                _Playable = null;
            }
        }

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only]
        /// Ensures that the <see cref="AnimancerPlayable"/> is destroyed in Edit Mode, but not in Play Mode since we want
        /// Unity to complain if that happens.
        /// </summary>
        ~AnimancerComponent()
        {
            if (_Playable != null)
                Editor.AnimancerEditorUtilities.EditModeDelayCall(OnDestroy);
        }
#endif

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Play Management
        /************************************************************************************************************************/

        /// <summary>
        /// Returns the `clip` itself. This method is used to determine the dictionary key to use for an animation
        /// when none is specified by the user, such as in <see cref="Play(AnimationClip)"/>. It can be overridden by
        /// child classes to use something else as the key.
        /// </summary>
        public virtual object GetKey(AnimationClip clip) => clip;

        /************************************************************************************************************************/
        // Play Immediately.
        /************************************************************************************************************************/

        /// <summary>
        /// Stops all other animations on the same layer, plays the `clip`, and returns its state.
        /// <para></para>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// To restart it from the beginning you can use <c>...Play(clip, layerIndex).Time = 0;</c>.
        /// </summary>
        public AnimancerState Play(AnimationClip clip)
            => Playable.Play(States.GetOrCreate(clip));

        /// <summary>
        /// Stops all other animations on the same layer, plays the `state`, and returns it.
        /// <para></para>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// To restart it from the beginning you can use <c>...Play(state).Time = 0;</c>.
        /// </summary>
        public AnimancerState Play(AnimancerState state)
            => Playable.Play(state);

        /************************************************************************************************************************/
        // Cross Fade.
        /************************************************************************************************************************/

        /// <summary>
        /// Starts fading in the `clip` while fading out all other states in the same layer over the course of the
        /// `fadeDuration`. Returns its state.
        /// <para></para>
        /// If the state was already playing and fading in with less time remaining than the `fadeDuration`, this
        /// method will allow it to complete the existing fade rather than starting a slower one.
        /// <para></para>
        /// If the layer currently has 0 <see cref="AnimancerNode.Weight"/>, this method will fade in the layer itself
        /// and simply <see cref="AnimancerState.Play(AnimationClip)"/> the `clip`.
        /// <para></para>
        /// <em>Animancer Lite only allows the default `fadeDuration` (0.25 seconds) in runtime builds.</em>
        /// </summary>
        public AnimancerState Play(AnimationClip clip, float fadeDuration, FadeMode mode = FadeMode.FixedSpeed)
            => Playable.Play(States.GetOrCreate(clip), fadeDuration, mode);

        /// <summary>
        /// Starts fading in the `state` while fading out all others in the same layer over the course of the
        /// `fadeDuration`. Returns the `state`.
        /// <para></para>
        /// If the `state` was already playing and fading in with less time remaining than the `fadeDuration`, this
        /// method will allow it to complete the existing fade rather than starting a slower one.
        /// <para></para>
        /// If the layer currently has 0 <see cref="AnimancerNode.Weight"/>, this method will fade in the layer itself
        /// and simply <see cref="AnimancerState.Play(AnimancerState)"/> the `state`.
        /// <para></para>
        /// <em>Animancer Lite only allows the default `fadeDuration` (0.25 seconds) in runtime builds.</em>
        /// </summary>
        public AnimancerState Play(AnimancerState state, float fadeDuration, FadeMode mode = FadeMode.FixedSpeed)
            => Playable.Play(state, fadeDuration, mode);

        /************************************************************************************************************************/
        // Transition.
        /************************************************************************************************************************/

        /// <summary>
        /// Creates a state for the `transition` if it didn't already exist, then calls
        /// <see cref="Play(AnimancerState)"/> or <see cref="Play(AnimancerState, float, FadeMode)"/>
        /// depending on <see cref="ITransition.CrossFadeFromStart"/>.
        /// </summary>
        public AnimancerState Play(ITransition transition)
            => Playable.Play(transition);

        /// <summary>
        /// Creates a state for the `transition` if it didn't already exist, then calls
        /// <see cref="Play(AnimancerState)"/> or <see cref="Play(AnimancerState, float, FadeMode)"/>
        /// depending on <see cref="ITransition.CrossFadeFromStart"/>.
        /// </summary>
        public AnimancerState Play(ITransition transition, float fadeDuration, FadeMode mode = FadeMode.FixedSpeed)
            => Playable.Play(transition, fadeDuration, mode);

        /************************************************************************************************************************/
        // Try Play.
        /************************************************************************************************************************/

        /// <summary>
        /// Stops all other animations on the same layer, plays the animation registered with the `key`, and returns
        /// that state.
        /// <para></para>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// To restart it from the beginning you can use <c>...Play(key).Time = 0;</c>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The `key` is null.</exception>
        /// <exception cref="KeyNotFoundException">No state is registered with the `key`.</exception>
        public AnimancerState TryPlay(object key)
            => Playable.TryPlay(key);

        /// <summary>
        /// Starts fading in the animation registered with the `key` while fading out all others in the same layer
        /// over the course of the `fadeDuration`.
        /// <para></para>
        /// If the state was already playing and fading in with less time remaining than the `fadeDuration`, this
        /// method will allow it to complete the existing fade rather than starting a slower one.
        /// <para></para>
        /// If the layer currently has 0 <see cref="AnimancerNode.Weight"/>, this method will fade in the layer itself
        /// and simply <see cref="AnimancerState.Play(AnimancerState)"/> the state.
        /// <para></para>
        /// <em>Animancer Lite only allows the default `fadeDuration` (0.25 seconds) in runtime builds.</em>
        /// </summary>
        /// <exception cref="ArgumentNullException">The `key` is null.</exception>
        /// <exception cref="KeyNotFoundException">No state is registered with the `key`.</exception>
        public AnimancerState TryPlay(object key, float fadeDuration, FadeMode mode = FadeMode.FixedSpeed)
            => Playable.TryPlay(key, fadeDuration, mode);

        /************************************************************************************************************************/

        /// <summary>
        /// Gets the state associated with the `clip`, stops and rewinds it to the start, then returns it.
        /// </summary>
        public AnimancerState Stop(AnimationClip clip) => Stop(GetKey(clip));

        /// <summary>
        /// Gets the state registered with the <see cref="IHasKey.Key"/>, stops and rewinds it to the start, then
        /// returns it.
        /// </summary>
        public AnimancerState Stop(IHasKey hasKey) => _Playable != null ? _Playable.Stop(hasKey) : null;

        /// <summary>
        /// Gets the state associated with the `key`, stops and rewinds it to the start, then returns it.
        /// </summary>
        public AnimancerState Stop(object key) => _Playable != null ? _Playable.Stop(key) : null;

        /// <summary>
        /// Stops all animations and rewinds them to the start.
        /// </summary>
        public void Stop()
        {
            if (_Playable != null)
                _Playable.Stop();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns true if a state is registered for the `clip` and it is currently playing.
        /// <para></para>
        /// The actual dictionary key is determined using <see cref="GetKey"/>.
        /// </summary>
        public bool IsPlaying(AnimationClip clip) => IsPlaying(GetKey(clip));

        /// <summary>
        /// Returns true if a state is registered with the <see cref="IHasKey.Key"/> and it is currently playing.
        /// </summary>
        public bool IsPlaying(IHasKey hasKey) => _Playable != null && _Playable.IsPlaying(hasKey);

        /// <summary>
        /// Returns true if a state is registered with the `key` and it is currently playing.
        /// </summary>
        public bool IsPlaying(object key) => _Playable != null && _Playable.IsPlaying(key);

        /// <summary>
        /// Returns true if at least one animation is being played.
        /// </summary>
        public bool IsPlaying() => _Playable != null && _Playable.IsPlaying();

        /************************************************************************************************************************/

        /// <summary>
        /// Returns true if the `clip` is currently being played by at least one state.
        /// <para></para>
        /// This method is inefficient because it searches through every state to find any that are playing the `clip`,
        /// unlike <see cref="IsPlaying(AnimationClip)"/> which only checks the state registered using the `clip`s key.
        /// </summary>
        public bool IsPlayingClip(AnimationClip clip) => _Playable != null && _Playable.IsPlayingClip(clip);

        /************************************************************************************************************************/

        /// <summary>
        /// Evaluates all of the currently playing animations to apply their states to the animated objects.
        /// </summary>
        public void Evaluate() => Playable.Evaluate();

        /// <summary>
        /// Advances all currently playing animations by the specified amount of time (in seconds) and evaluates the
        /// graph to apply their states to the animated objects.
        /// </summary>
        public void Evaluate(float deltaTime) => Playable.Evaluate(deltaTime);

        /************************************************************************************************************************/
        #region Key Error Methods
#if UNITY_EDITOR
        /************************************************************************************************************************/
        // These are overloads of other methods that take a System.Object key to ensure the user doesn't try to use an
        // AnimancerState as a key, since the whole point of a key is to identify a state in the first place.
        /************************************************************************************************************************/

        /// <summary>[Warning]
        /// You should not use an <see cref="AnimancerState"/> as a key.
        /// Just call <see cref="AnimancerState.Stop"/>.
        /// </summary>
        [Obsolete("You should not use an AnimancerState as a key. Just call AnimancerState.Stop().", true)]
        public AnimancerState Stop(AnimancerState key)
        {
            key.Stop();
            return key;
        }

        /// <summary>[Warning]
        /// You should not use an <see cref="AnimancerState"/> as a key.
        /// Just check <see cref="AnimancerState.IsPlaying"/>.
        /// </summary>
        [Obsolete("You should not use an AnimancerState as a key. Just check AnimancerState.IsPlaying.", true)]
        public bool IsPlaying(AnimancerState key) => key.IsPlaying;

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Enumeration
        /************************************************************************************************************************/
        // IEnumerable for 'foreach' statements.
        /************************************************************************************************************************/

        /// <summary>
        /// Returns an enumerator that will iterate through all states in each layer (but not sub-states).
        /// </summary>
        public IEnumerator<AnimancerState> GetEnumerator()
        {
            if (!IsPlayableInitialised)
                yield break;

            foreach (var state in _Playable)
                yield return state;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /************************************************************************************************************************/
        // IEnumerator for yielding in a coroutine to wait until all animations have stopped.
        /************************************************************************************************************************/

        /// <summary>
        /// Determines if any animations are still playing so this object can be used as a custom yield instruction.
        /// </summary>
        bool IEnumerator.MoveNext()
        {
            if (!IsPlayableInitialised)
                return false;

            return ((IEnumerator)_Playable).MoveNext();
        }

        /// <summary>Returns null.</summary>
        object IEnumerator.Current => null;

#pragma warning disable UNT0006 // Incorrect message signature.
        /// <summary>Does nothing.</summary>
        void IEnumerator.Reset() { }
#pragma warning restore UNT0006 // Incorrect message signature.

        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipSource"/>]
        /// Calls <see cref="GatherAnimationClips(ICollection{AnimationClip})"/>.
        /// </summary>
        public void GetAnimationClips(List<AnimationClip> clips)
        {
            var set = ObjectPool.AcquireSet<AnimationClip>();
            set.UnionWith(clips);

            GatherAnimationClips(set);

            clips.Clear();
            clips.AddRange(set);

            ObjectPool.Release(set);
        }

        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipCollection"/>]
        /// Gathers all the animations in the <see cref="Playable"/>.
        /// <para></para>
        /// In the Unity Editor this method also gathers animations from other components on parent and child objects.
        /// </summary>
        public virtual void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            if (IsPlayableInitialised)
                _Playable.GatherAnimationClips(clips);

#if UNITY_EDITOR
            Editor.AnimationGatherer.GatherFromGameObject(gameObject, clips);

            if (_Animator != null && _Animator.gameObject != gameObject)
                Editor.AnimationGatherer.GatherFromGameObject(_Animator.gameObject, clips);
#endif
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}
