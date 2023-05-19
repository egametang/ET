// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>
    /// A <see cref="PlayableBehaviour"/> which can be used as a substitute for the
    /// <see cref="RuntimeAnimatorController"/> normally used to control an <see cref="Animator"/>.
    /// </summary>
    /// 
    /// <remarks>
    /// This class can be used as a custom yield instruction to wait until all animations finish playing.
    /// <para></para>
    /// The most common way to access this class is via <see cref="AnimancerComponent.Playable"/>.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing">Playing Animations</see>
    /// </remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerPlayable
    /// 
    public sealed partial class AnimancerPlayable : PlayableBehaviour,
        IEnumerable<AnimancerState>, IEnumerator, IPlayableWrapper, IAnimationClipCollection
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        /// <summary>The fade duration to use if the caller doesn't specify.</summary>
        public const float DefaultFadeDuration = 0.25f;

        /************************************************************************************************************************/

        /// <summary>[Internal] The <see cref="PlayableGraph"/> containing this <see cref="AnimancerPlayable"/>.</summary>
        internal PlayableGraph _Graph;

        /// <summary>[Pro-Only] The <see cref="PlayableGraph"/> containing this <see cref="AnimancerPlayable"/>.</summary>
        public PlayableGraph Graph => _Graph;

        /// <summary>[Internal] The <see cref="Playable"/> containing this <see cref="AnimancerPlayable"/>.</summary>
        internal Playable _RootPlayable;

        /// <summary>[Internal] The <see cref="Playable"/> which layers connect to.</summary>
        internal Playable _LayerMixer;

        /************************************************************************************************************************/

        /// <summary>[Internal] The <see cref="Playable"/> which layers connect to.</summary>
        Playable IPlayableWrapper.Playable => _LayerMixer;

        /// <summary>[Internal] An <see cref="AnimancerPlayable"/> is the root of the graph so it has no parent.</summary>
        IPlayableWrapper IPlayableWrapper.Parent => null;

        /// <summary>[Internal] The <see cref="LayerList.Count"/>.</summary>
        int IPlayableWrapper.ChildCount => Layers.Count;

        /// <summary>[Internal] Returns the layer at the specified `index`.</summary>
        AnimancerNode IPlayableWrapper.GetChild(int index) => Layers[index];

        /************************************************************************************************************************/
        // These collections can't be readonly because when Unity clones the Template it copies the memory without running the
        // field initialisers on the new clone so everything would be referencing the same collections.
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerLayer"/>s which each manage their own set of animations.</summary>
        /// <remarks>
        /// See the documentation for more information about
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/blending/layers">
        /// Layers</see>.
        /// </remarks>
        public LayerList Layers { get; private set; }

        /// <summary>The <see cref="AnimancerState"/>s managed by this playable.</summary>
        /// <remarks>
        /// See the documentation for more information about
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/playing/states">
        /// States</see>.
        /// </remarks>
        public StateDictionary States { get; private set; }

        /// <summary>All of the nodes that need to be updated.</summary>
        private Key.KeyedList<AnimancerNode> _DirtyNodes;

        /// <summary>All of the objects that need to be updated early.</summary>
        private Key.KeyedList<IUpdatable> _Updatables;

        /// <summary>The <see cref="PlayableBehaviour"/> that calls <see cref="IUpdatable.LateUpdate"/>.</summary>
        private LateUpdate _LateUpdate;

        /************************************************************************************************************************/

        /// <summary>The component that is playing this <see cref="AnimancerPlayable"/>.</summary>
        public IAnimancerComponent Component { get; private set; }

        /************************************************************************************************************************/

        /// <summary>
        /// The number of times the <see cref="AnimancerLayer.CurrentState"/> has changed on layer 0. By storing this
        /// value and later comparing the stored value to the current value, you can determine whether the state has
        /// been changed since then, even it has changed back to the same state.
        /// </summary>
        public int CommandCount => Layers[0].CommandCount;

        /************************************************************************************************************************/

        /// <summary>Determines what time source is used to update the <see cref="PlayableGraph"/>.</summary>
        public DirectorUpdateMode UpdateMode
        {
            get => _Graph.GetTimeUpdateMode();
            set => _Graph.SetTimeUpdateMode(value);
        }

        /************************************************************************************************************************/

        /// <summary>How fast the <see cref="AnimancerState.Time"/> of all animations is advancing every frame.</summary>
        /// 
        /// <remarks>
        /// 1 is the normal speed.
        /// <para></para>
        /// A negative value will play the animations backwards.
        /// <para></para>
        /// Setting this value to 0 would pause all animations, but calling <see cref="PauseGraph"/> is more efficient.
        /// <para></para>
        /// <em>Animancer Lite does not allow this value to be changed in runtime builds.</em>
        /// </remarks>
        ///
        /// <example><code>
        /// void SetSpeed(AnimancerComponent animancer)
        /// {
        ///     animancer.Playable.Speed = 1;// Normal speed.
        ///     animancer.Playable.Speed = 2;// Double speed.
        ///     animancer.Playable.Speed = 0.5f;// Half speed.
        ///     animancer.Playable.Speed = -1;// Normal speed playing backwards.
        /// }
        /// </code></example>
        public float Speed
        {
            get => (float)_LayerMixer.GetSpeed();
            set => _LayerMixer.SetSpeed(value);
        }

        /************************************************************************************************************************/

        private bool _KeepChildrenConnected;

        /// <summary>
        /// Indicates whether child playables should stay connected to the graph at all times.
        /// </summary>
        /// 
        /// <remarks>
        /// Humanoid Rigs default this value to <c>false</c> so that playables will be disconnected from the graph
        /// while they are at 0 weight which stops it from evaluating them every frame.
        /// <para></para>
        /// Generic Rigs default this value to <c>true</c> because they do not always animate the same standard set of
        /// values so every connection change has a higher performance cost than with Humanoid Rigs which is generally
        /// more significant than the gains for having fewer playables connected at a time.
        /// <para></para>
        /// The default is set by <see cref="SetOutput(Animator, IAnimancerComponent)"/>.
        /// </remarks>
        /// 
        /// <example><code>
        /// [SerializeField]
        /// private AnimancerComponent _Animancer;
        /// 
        /// public void Initialise()
        /// {
        ///     _Animancer.Playable.KeepChildrenConnected = true;
        /// }
        /// </code></example>
        public bool KeepChildrenConnected
        {
            get => _KeepChildrenConnected;
            set
            {
                if (_KeepChildrenConnected == value)
                    return;

                _KeepChildrenConnected = value;
                Layers.SetWeightlessChildrenConnected(value);
            }
        }

        /************************************************************************************************************************/

        private bool _SkipFirstFade;

        /// <summary>
        /// Normally the first animation on the Base Layer should not fade in because there is nothing fading out. But
        /// sometimes that is undesirable, such as if the <see cref="Animator.runtimeAnimatorController"/> is assigned
        /// since Animancer can blend with that.
        /// </summary>
        /// <remarks>
        /// Setting this value to false ensures that the <see cref="AnimationLayerMixerPlayable"/> has at least two
        /// inputs because it ignores the <see cref="AnimancerNode.Weight"/> of the layer when there is only one.
        /// </remarks>
        public bool SkipFirstFade
        {
            get => _SkipFirstFade;
            set
            {
                _SkipFirstFade = value;

                if (!value && Layers.Count < 2)
                {
                    Layers.Count = 1;
                    _LayerMixer.SetInputCount(2);
                }
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Initialisation
        /************************************************************************************************************************/

        /// <summary>
        /// Since <see cref="ScriptPlayable{T}.Create(PlayableGraph, int)"/> needs to clone an existing instance, we
        /// keep a static template to avoid allocating an extra garbage one every time. This is why the fields are
        /// assigned in <see cref="OnPlayableCreate"/> rather than being readonly with field initialisers.
        /// </summary>
        private static readonly AnimancerPlayable Template = new AnimancerPlayable();

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="PlayableGraph"/> containing an <see cref="AnimancerPlayable"/>.
        /// <para></para>
        /// The caller is responsible for calling <see cref="Destroy()"/> on the returned object, except in Edit Mode
        /// where it will be called automatically.
        /// <para></para>
        /// Consider calling <see cref="SetNextGraphName"/> before this method to give it a name.
        /// </summary>
        public static AnimancerPlayable Create()
        {
#if UNITY_EDITOR
            var name = _NextGraphName;
            _NextGraphName = null;

            var graph = name != null ?
                PlayableGraph.Create(name) :
                PlayableGraph.Create();
#else
            var graph = PlayableGraph.Create();
#endif

            return ScriptPlayable<AnimancerPlayable>.Create(graph, Template, 2)
                .GetBehaviour();
        }

        /************************************************************************************************************************/

        /// <summary>[Internal] Called by Unity as it creates this <see cref="AnimancerPlayable"/>.</summary>
        public override void OnPlayableCreate(Playable playable)
        {
            _RootPlayable = playable;
            _Graph = playable.GetGraph();

            _Updatables = new Key.KeyedList<IUpdatable>();
            _DirtyNodes = new Key.KeyedList<AnimancerNode>();
            _LateUpdate = LateUpdate.Create(this);
            Layers = new LayerList(this, out _LayerMixer);
            States = new StateDictionary(this);

            playable.SetInputWeight(0, 1);

#if UNITY_EDITOR
            RegisterInstance();
#endif
        }

        /************************************************************************************************************************/

#if UNITY_EDITOR
        private static string _NextGraphName;
#endif

        /// <summary>[Editor-Conditional]
        /// Sets the display name for the next <see cref="Create"/> call to give its <see cref="PlayableGraph"/>.
        /// </summary>
        /// <remarks>
        /// Having this method separate from <see cref="Create"/> allows the
        /// <see cref="System.Diagnostics.ConditionalAttribute"/> to compile it out of runtime builds which would
        /// otherwise require #ifs on the caller side.
        /// </remarks>
        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public static void SetNextGraphName(string name)
        {
#if UNITY_EDITOR
            _NextGraphName = name;
#endif
        }

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only] Returns "AnimancerPlayable (Graph Name)".</summary>
        public override string ToString()
            => $"{nameof(AnimancerPlayable)} ({(_Graph.IsValid() ? _Graph.GetEditorName() : "Graph Not Initialised")})";
#endif

        /************************************************************************************************************************/

        /// <summary>
        /// Plays this playable on the <see cref="IAnimancerComponent.Animator"/> and sets the
        /// <see cref="Component"/>.
        /// </summary>
        public void SetOutput(IAnimancerComponent animancer) => SetOutput(animancer.Animator, animancer);

        /// <summary>Plays this playable on the specified `animator` and sets the <see cref="Component"/>.</summary>
        public void SetOutput(Animator animator, IAnimancerComponent animancer)
        {
#if UNITY_EDITOR
            // Do nothing if the target is a prefab.
            if (UnityEditor.EditorUtility.IsPersistent(animator))
                return;
#endif

#if UNITY_ASSERTIONS
            if (animancer != null)
            {
                Debug.Assert(animancer.IsPlayableInitialised && animancer.Playable == this,
                    $"{nameof(SetOutput)} was called on an {nameof(AnimancerPlayable)} which does not match the" +
                    $" {nameof(IAnimancerComponent)}.{nameof(IAnimancerComponent.Playable)}.");
                Debug.Assert(animator == animancer.Animator,
                    $"{nameof(SetOutput)} was called with an {nameof(Animator)} which does not match the" +
                    $" {nameof(IAnimancerComponent)}.{nameof(IAnimancerComponent.Animator)}.");
            }
#endif

            Component = animancer;

            var output = _Graph.GetOutput(0);
            if (output.IsOutputValid())
                _Graph.DestroyOutput(output);

            if (animator != null)
            {
                var isHumanoid = animator.isHuman;

                // Generic Rigs get better performance by keeping children connected but Humanoids don't.
                KeepChildrenConnected = !isHumanoid;

                // Generic Rigs can blend with an underlying Animator Controller but Humanoids can't.
                SkipFirstFade = isHumanoid || animator.runtimeAnimatorController == null;

                AnimationPlayableUtilities.Play(animator, _RootPlayable, _Graph);
                _IsGraphPlaying = true;
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Inserts a `playable` after the root of the <see cref="Graph"/> so that it can modify the final output.
        /// <para></para>
        /// It can be removed using <see cref="AnimancerUtilities.RemovePlayable"/>.
        /// </summary>
        public void InsertOutputPlayable(Playable playable)
        {
            var output = _Graph.GetOutput(0);
            _Graph.Connect(output.GetSourcePlayable(), 0, playable, 0);
            playable.SetInputWeight(0, 1);
            output.SetSourcePlayable(playable);
        }

        /// <summary>[Pro-Only]
        /// Inserts an animation job after the root of the <see cref="Graph"/> so that it can modify the final output.
        /// <para></para>
        /// It can can be removed by passing the returned value into <see cref="AnimancerUtilities.RemovePlayable"/>.
        /// </summary>
        public AnimationScriptPlayable InsertOutputJob<T>(T data) where T : struct, IAnimationJob
        {
            var playable = AnimationScriptPlayable.Create(_Graph, data, 1);
            var output = _Graph.GetOutput(0);
            _Graph.Connect(output.GetSourcePlayable(), 0, playable, 0);
            playable.SetInputWeight(0, 1);
            output.SetSourcePlayable(playable);
            return playable;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Cleanup
        /************************************************************************************************************************/

        /// <summary>
        /// Returns true as long as the <see cref="PlayableGraph"/> hasn't been destroyed (such as by <see cref="Destroy()"/>).
        /// </summary>
        public bool IsValid => _Graph.IsValid();

        /// <summary>Destroys the <see cref="PlayableGraph"/>. This operation cannot be undone.</summary>
        public void Destroy()
        {
            if (_Graph.IsValid())
                _Graph.Destroy();
        }

        /// <summary>Cleans up the resources managed by this <see cref="AnimancerPlayable"/>.</summary>
        public override void OnPlayableDestroy(Playable playable)
        {
            // Destroy all active updatables.
            Debug.Assert(_CurrentUpdatable == -1, UpdatableLoopStartError);
            _CurrentUpdatable = _Updatables.Count;
            DestroyNext:
            try
            {
                while (--_CurrentUpdatable >= 0)
                {
                    _Updatables[_CurrentUpdatable].OnDestroy();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, Component as Object);
                goto DestroyNext;
            }
            _Updatables.Clear();

            DisposeAll();

            // No need to destroy every layer and state individually because destroying the graph will do so anyway.

            Layers = null;
            States = null;
        }

        /************************************************************************************************************************/

        private List<IDisposable> _Disposables;

        /// <summary>A list of objects that need to be disposed when this <see cref="AnimancerPlayable"/> is destroyed.</summary>
        /// <remarks>This list is primarily used to dispose native arrays used in Animation Jobs.</remarks>
        public List<IDisposable> Disposables => _Disposables ?? (_Disposables = new List<IDisposable>());

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="IDisposable.Dispose"/> on all the <see cref="Disposables"/>.</summary>
        ~AnimancerPlayable() => DisposeAll();

        /// <summary>Calls <see cref="IDisposable.Dispose"/> on all the <see cref="Disposables"/>.</summary>
        private void DisposeAll()
        {
            if (_Disposables == null)
                return;

            var i = _Disposables.Count;
            DisposeNext:
            try
            {
                while (--i >= 0)
                {
                    _Disposables[i].Dispose();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, Component as Object);
                goto DisposeNext;
            }

            _Disposables.Clear();
            _Disposables = null;
            GC.SuppressFinalize(this);
        }

        /************************************************************************************************************************/
        #region Inverse Kinematics
        /************************************************************************************************************************/

        private bool _ApplyAnimatorIK;

        /// <inheritdoc/>
        public bool ApplyAnimatorIK
        {
            get => _ApplyAnimatorIK;
            set
            {
                _ApplyAnimatorIK = value;

                for (int i = Layers.Count - 1; i >= 0; i--)
                    Layers._Layers[i].ApplyAnimatorIK = value;
            }
        }

        /************************************************************************************************************************/

        private bool _ApplyFootIK;

        /// <inheritdoc/>
        public bool ApplyFootIK
        {
            get => _ApplyFootIK;
            set
            {
                _ApplyFootIK = value;

                for (int i = Layers.Count - 1; i >= 0; i--)
                    Layers._Layers[i].ApplyFootIK = value;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Playing
        /************************************************************************************************************************/

        /// <summary>Calls <see cref="IAnimancerComponent.GetKey"/> on the <see cref="Component"/>.</summary>
        public object GetKey(AnimationClip clip) => Component.GetKey(clip);

        /************************************************************************************************************************/
        // Play Immediately.
        /************************************************************************************************************************/

        /// <summary>Stops all other animations on the same layer, plays the `clip`, and returns its state.</summary>
        /// <remarks>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// To restart it from the beginning you can use <c>...Play(clip, layerIndex).Time = 0;</c>.
        /// </remarks>
        public AnimancerState Play(AnimationClip clip)
            => Play(States.GetOrCreate(clip));

        /// <summary>Stops all other animations on the same laye, plays the `state`, and returns it.</summary>
        /// <remarks>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// If you wish to force it back to the start, you can simply set the `state`s time to 0.
        /// </remarks>
        public AnimancerState Play(AnimancerState state)
        {
            var layer = state.Layer ?? Layers[0];
            return layer.Play(state);
        }

        /************************************************************************************************************************/
        // Cross Fade.
        /************************************************************************************************************************/

        /// <summary>
        /// Starts fading in the `clip` while fading out all other states in the same layer over the course of the
        /// `fadeDuration`. Returns its state.
        /// </summary>
        /// <remarks>
        /// If the `state` was already playing and fading in with less time remaining than the `fadeDuration`, this
        /// method will allow it to complete the existing fade rather than starting a slower one.
        /// <para></para>
        /// If the layer currently has 0 <see cref="AnimancerNode.Weight"/>, this method will fade in the layer itself
        /// and simply <see cref="AnimancerState.Play"/> the `state`.
        /// <para></para>
        /// <em>Animancer Lite only allows the default `fadeDuration` (0.25 seconds) in runtime builds.</em>
        /// </remarks>
        public AnimancerState Play(AnimationClip clip, float fadeDuration, FadeMode mode = FadeMode.FixedSpeed)
            => Play(States.GetOrCreate(clip), fadeDuration, mode);

        /// <summary>
        /// Starts fading in the `state` while fading out all others in the same layer over the course of the
        /// `fadeDuration`. Returns the `state`.
        /// </summary>
        /// <remarks>
        /// If the `state` was already playing and fading in with less time remaining than the `fadeDuration`, this
        /// method will allow it to complete the existing fade rather than starting a slower one.
        /// <para></para>
        /// If the layer currently has 0 <see cref="AnimancerNode.Weight"/>, this method will fade in the layer itself
        /// and simply <see cref="AnimancerState.Play"/> the `state`.
        /// <para></para>
        /// <em>Animancer Lite only allows the default `fadeDuration` (0.25 seconds) in runtime builds.</em>
        /// </remarks>
        public AnimancerState Play(AnimancerState state, float fadeDuration, FadeMode mode = FadeMode.FixedSpeed)
        {
            var layer = state.Layer ?? Layers[0];
            return layer.Play(state, fadeDuration, mode);
        }

        /************************************************************************************************************************/
        // Transition.
        /************************************************************************************************************************/

        /// <summary>
        /// Creates a state for the `transition` if it didn't already exist, then calls
        /// <see cref="Play(AnimancerState)"/> or <see cref="Play(AnimancerState, float, FadeMode)"/>
        /// depending on the <see cref="ITransition.FadeDuration"/>.
        /// </summary>
        public AnimancerState Play(ITransition transition)
            => Play(transition, transition.FadeDuration, transition.FadeMode);

        /// <summary>
        /// Creates a state for the `transition` if it didn't already exist, then calls
        /// <see cref="Play(AnimancerState)"/> or <see cref="Play(AnimancerState, float, FadeMode)"/>
        /// depending on the <see cref="ITransition.FadeDuration"/>.
        /// </summary>
        public AnimancerState Play(ITransition transition, float fadeDuration, FadeMode mode = FadeMode.FixedSpeed)
        {
            var state = States.GetOrCreate(transition);
            state = Play(state, fadeDuration, mode);
            transition.Apply(state);
            return state;
        }

        /************************************************************************************************************************/
        // Try Play.
        /************************************************************************************************************************/

        /// <summary>
        /// Stops all other animations on the same layer, plays the animation registered with the `key`, and returns
        /// that state. Or if no state is registered with that `key`, this method does nothing and returns null.
        /// </summary>
        /// <remarks>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// If you wish to force it back to the start, you can simply set the returned state's time to 0.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The `key` is null.</exception>
        public AnimancerState TryPlay(object key)
            => States.TryGet(key, out var state) ? Play(state) : null;

        /// <summary>
        /// Starts fading in the animation registered with the `key` while fading out all others in the same layer
        /// over the course of the `fadeDuration`. Or if no state is registered with that `key`, this method does
        /// nothing and returns null.
        /// </summary>
        /// <remarks>
        /// If the `state` was already playing and fading in with less time remaining than the `fadeDuration`, this
        /// method will allow it to complete the existing fade rather than starting a slower one.
        /// <para></para>
        /// If the layer currently has 0 <see cref="AnimancerNode.Weight"/>, this method will fade in the layer itself
        /// and simply <see cref="AnimancerState.Play"/> the `state`.
        /// <para></para>
        /// <em>Animancer Lite only allows the default `fadeDuration` (0.25 seconds) in runtime builds.</em>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The `key` is null.</exception>
        public AnimancerState TryPlay(object key, float fadeDuration, FadeMode mode = FadeMode.FixedSpeed)
            => States.TryGet(key, out var state) ? Play(state, fadeDuration, mode) : null;

        /************************************************************************************************************************/

        /// <summary>
        /// Gets the state registered with the <see cref="IHasKey.Key"/>, stops and rewinds it to the start, then
        /// returns it.
        /// </summary>
        public AnimancerState Stop(IHasKey hasKey) => Stop(hasKey.Key);

        /// <summary>
        /// Calls <see cref="AnimancerState.Stop"/> on the state registered with the `key` to stop it from playing and
        /// rewind it to the start.
        /// </summary>
        public AnimancerState Stop(object key)
        {
            if (States.TryGet(key, out var state))
                state.Stop();

            return state;
        }

        /// <summary>
        /// Calls <see cref="AnimancerState.Stop"/> on all animations to stop them from playing and rewind them to the
        /// start.
        /// </summary>
        public void Stop()
        {
            if (Layers._Layers == null)
                return;

            for (int i = Layers.Count - 1; i >= 0; i--)
                Layers._Layers[i].Stop();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns true if a state is registered with the <see cref="IHasKey.Key"/> and it is currently playing.
        /// </summary>
        public bool IsPlaying(IHasKey hasKey) => IsPlaying(hasKey.Key);

        /// <summary>
        /// Returns true if a state is registered with the `key` and it is currently playing.
        /// </summary>
        public bool IsPlaying(object key) => States.TryGet(key, out var state) && state.IsPlaying;

        /// <summary>
        /// Returns true if at least one animation is being played.
        /// </summary>
        public bool IsPlaying()
        {
            if (!_IsGraphPlaying)
                return false;

            for (int i = Layers.Count - 1; i >= 0; i--)
            {
                if (Layers._Layers[i].IsAnyStatePlaying())
                    return true;
            }

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns true if the `clip` is currently being played by at least one state in the specified layer.
        /// <para></para>
        /// This method is inefficient because it searches through every state to find any that are playing the `clip`,
        /// unlike <see cref="IsPlaying(object)"/> which only checks the state registered using the specified key.
        /// </summary>
        public bool IsPlayingClip(AnimationClip clip)
        {
            if (!_IsGraphPlaying)
                return false;

            for (int i = Layers.Count - 1; i >= 0; i--)
                if (Layers._Layers[i].IsPlayingClip(clip))
                    return true;

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calculates the total <see cref="AnimancerNode.Weight"/> of all states in this playable.
        /// </summary>
        public float GetTotalWeight()
        {
            float weight = 0;

            for (int i = Layers.Count - 1; i >= 0; i--)
                weight += Layers._Layers[i].GetTotalWeight();

            return weight;
        }

        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipCollection"/>]
        /// Gathers all the animations in all layers.
        /// </summary>
        public void GatherAnimationClips(ICollection<AnimationClip> clips) => Layers.GatherAnimationClips(clips);

        /************************************************************************************************************************/
        // IEnumerable for 'foreach' statements.
        /************************************************************************************************************************/

        /// <summary>
        /// Returns an enumerator that will iterate through all states in each layer (but not sub-states).
        /// </summary>
        public IEnumerator<AnimancerState> GetEnumerator()
        {
            foreach (var state in Layers.GetAllStateEnumerable())
                yield return state;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /************************************************************************************************************************/
        // IEnumerator for yielding in a coroutine to wait until animations have stopped.
        /************************************************************************************************************************/

        /// <summary>
        /// Determines if any animations are still playing so this object can be used as a custom yield instruction.
        /// </summary>
        bool IEnumerator.MoveNext()
        {
            for (int i = Layers.Count - 1; i >= 0; i--)
                if (Layers._Layers[i].IsPlayingAndNotEnding())
                    return true;

            return false;
        }

        /// <summary>Returns null.</summary>
        object IEnumerator.Current => null;

        /// <summary>Does nothing.</summary>
        void IEnumerator.Reset() { }

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
        [System.Obsolete("You should not use an AnimancerState as a key. Just call AnimancerState.Stop().", true)]
        public AnimancerState Stop(AnimancerState key)
        {
            key.Stop();
            return key;
        }

        /// <summary>[Warning]
        /// You should not use an <see cref="AnimancerState"/> as a key.
        /// Just check <see cref="AnimancerState.IsPlaying"/>.
        /// </summary>
        [System.Obsolete("You should not use an AnimancerState as a key. Just check AnimancerState.IsPlaying.", true)]
        public bool IsPlaying(AnimancerState key) => key.IsPlaying;

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Evaluation
        /************************************************************************************************************************/

        private bool _IsGraphPlaying = true;

        /// <summary>Indicates whether the <see cref="PlayableGraph"/> is currently playing.</summary>
        public bool IsGraphPlaying
        {
            get => _IsGraphPlaying;
            set
            {
                if (value)
                    UnpauseGraph();
                else
                    PauseGraph();
            }
        }

        /// <summary>
        /// Resumes playing the <see cref="PlayableGraph"/> if <see cref="PauseGraph"/> was called previously.
        /// </summary>
        public void UnpauseGraph()
        {
            if (!_IsGraphPlaying)
            {
                _Graph.Play();
                _IsGraphPlaying = true;

#if UNITY_EDITOR
                // In Edit Mode, unpausing the graph does not work properly unless we force it to change.
                if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                    Evaluate(Time.maximumDeltaTime);
#endif
            }
        }

        /// <summary>
        /// Freezes the <see cref="PlayableGraph"/> at its current state.
        /// <para></para>
        /// If you call this method, you are responsible for calling <see cref="UnpauseGraph"/> to resume playing.
        /// </summary>
        public void PauseGraph()
        {
            if (_IsGraphPlaying)
            {
                _Graph.Stop();
                _IsGraphPlaying = false;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Evaluates all of the currently playing animations to apply their states to the animated objects.
        /// </summary>
        public void Evaluate() => _Graph.Evaluate();

        /// <summary>
        /// Advances all currently playing animations by the specified amount of time (in seconds) and evaluates the
        /// graph to apply their states to the animated objects.
        /// </summary>
        public void Evaluate(float deltaTime) => _Graph.Evaluate(deltaTime);

        /************************************************************************************************************************/

        /// <summary>Returns a detailed descrption of all currently playing states and other registered states.</summary>
        public string GetDescription(int maxChildDepth = 7)
        {
            var text = ObjectPool.AcquireStringBuilder();
            AppendDescription(text, maxChildDepth);
            return text.ReleaseToString();
        }

        /// <summary>Appends a detailed descrption of all currently playing states and other registered states.</summary>
        public void AppendDescription(StringBuilder text, int maxChildDepth = 7)
        {
            text.Append($"{nameof(AnimancerPlayable)} (")
                .Append(Component)
                .Append(") Layer Count: ")
                .Append(Layers.Count);

            const string Delimiter = "\n    ";
            AnimancerNode.AppendIKDetails(text, Delimiter, this);

            var count = Layers.Count;
            for (int i = 0; i < count; i++)
            {
                text.Append(Delimiter);
                Layers[i].AppendDescription(text, maxChildDepth, Delimiter);
            }

            text.AppendLine();
            AppendInternalDetails(text, Strings.Indent, Strings.Indent + Strings.Indent);

#if UNITY_EDITOR
            Editor.AnimancerEditorUtilities.AppendNonCriticalIssues(text);
#endif
        }

        /// <summary>
        /// Appends a list of all <see cref="IUpdatable"/>s and <see cref="AnimancerNode"/>s that are registered for updates.
        /// </summary>
        public void AppendInternalDetails(StringBuilder text, string sectionPrefix, string itemPrefix)
        {
            var count = _Updatables.Count;
            text.Append(sectionPrefix).Append("Updatables: ").Append(count);
            for (int i = 0; i < count; i++)
                text.AppendLine().Append(itemPrefix).Append(_Updatables[i]);

            text.AppendLine();

            count = _DirtyNodes.Count;
            text.Append(sectionPrefix).Append("Dirty Nodes: ").Append(count);
            for (int i = 0; i < count; i++)
                text.AppendLine().Append(itemPrefix).Append(_DirtyNodes[i]);

            text.AppendLine();

            count = _Disposables != null ? _Disposables.Count : 0;
            text.Append(sectionPrefix).Append("Disposables: ").Append(count);
            for (int i = 0; i < count; i++)
                text.AppendLine().Append(itemPrefix).Append(_Disposables[i]);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Update
        /************************************************************************************************************************/

        /// <summary>
        /// Adds the `updatable` to the list of objects that need to be updated if it was not there already.
        /// <para></para>
        /// This method is safe to call at any time, even during an update.
        /// <para></para>
        /// The execution order of updatables is non-deterministic. Specifically, the most recently added will be
        /// updated first and <see cref="CancelUpdate(IUpdatable)"/> will change the order by swapping the last one
        /// into the place of the removed element.
        /// </summary>
        public void RequireUpdate(IUpdatable updatable)
        {
            _Updatables.AddNew(updatable);
        }

        /// <summary>
        /// Removes the `updatable` from the list of objects that need to be updated.
        /// <para></para>
        /// This method is safe to call at any time, even during an update.
        /// <para></para>
        /// The last element is swapped into the place of the one being removed so that the rest of them do not need to
        /// be moved down one place to fill the gap. This is more efficient, but means that the update order can change.
        /// </summary>
        public void CancelUpdate(IUpdatable updatable)
        {
            var index = Key.IndexOf(updatable.Key);
            if (index < 0)
                return;

            _Updatables.RemoveAtSwap(index);

            if (_CurrentUpdatable < index && this == Current)
                _CurrentUpdatable--;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Adds the `node` to the list that need to be updated if it was not there already.
        /// <para></para>
        /// This method is safe to call at any time, even during an update.
        /// <para></para>
        /// The execution order of nodes is non-deterministic. Specifically, the most recently added will be
        /// updated first and <see cref="CancelUpdate(AnimancerNode)"/> will change the order by swapping the last one
        /// into the place of the removed element.
        /// </summary>
        public void RequireUpdate(AnimancerNode node)
        {
            Validate.AssertPlayable(node);
            Validate.AssertRoot(node, this);
            _DirtyNodes.AddNew(node);
        }

        /// <summary>
        /// Removes the `node` from the list of objects that need to be updated.
        /// <para></para>
        /// This method is safe to call at any time, even during an update.
        /// <para></para>
        /// The last element is swapped into the place of the one being removed so that the rest of them do not need to
        /// be moved down one place to fill the gap. This is more efficient, but means that the update order can change.
        /// </summary>
        internal void CancelUpdate(AnimancerNode node)
        {
            var index = Key.IndexOf(node);
            if (index < 0)
                return;

            _DirtyNodes.RemoveAtSwap(index);

            if (_CurrentNode < index && this == Current)
                _CurrentNode--;
        }

        /************************************************************************************************************************/

        /// <summary>The object currently executing <see cref="PrepareFrame"/>.</summary>
        public static AnimancerPlayable Current { get; private set; }

        /// <summary>
        /// The current (most recent) <see cref="FrameData.deltaTime"/>.
        /// <para></para>
        /// After <see cref="PrepareFrame"/>, this property will be left at its most recent value.
        /// </summary>
        public static float DeltaTime { get; private set; }

        /// <summary>
        /// The current (most recent) <see cref="FrameData.frameId"/>.
        /// <para></para>
        /// <see cref="AnimancerState.Time"/> uses this value to determine whether it has accessed the playable's time
        /// since it was last updated in order to cache its value.
        /// </summary>
        public ulong FrameID { get; private set; }

        /// <summary>The index of the <see cref="IUpdatable"/> currently being updated.</summary>
        private static int _CurrentUpdatable = -1;

        /// <summary>The index of the <see cref="AnimancerNode"/> currently being updated.</summary>
        private static int _CurrentNode = -1;

        /// <summary>An error message for potential multithreading issues.</summary>
        private const string UpdatableLoopStartError =
            nameof(AnimancerPlayable) + "." + nameof(_CurrentUpdatable) + " != -1." +
            " This may mean that multiple loops are iterating through the updatables simultaneously" +
            " (likely on different threads).";

        /************************************************************************************************************************/

        /// <summary>[Internal]
        /// Calls <see cref="IUpdatable.EarlyUpdate"/> and <see cref="AnimancerNode.Update"/> on everything
        /// that needs it.
        /// </summary>
        /// <remarks>
        /// Called by the <see cref="PlayableGraph"/> before the rest of the <see cref="Playable"/>s are evaluated.
        /// </remarks>
        public override void PrepareFrame(Playable playable, FrameData info)
        {
#if UNITY_ASSERTIONS
            if (OptionalWarning.AnimatorSpeed.IsEnabled() && Component != null)
            {
                var animator = Component.Animator;
                if (animator != null &&
                    animator.speed != 1 &&
                    animator.runtimeAnimatorController == null)
                {
                    animator.speed = 1;
                    OptionalWarning.AnimatorSpeed.Log(
                        $"{nameof(Animator)}.{nameof(Animator.speed)} does not affect {nameof(Animancer)}." +
                        $" Use {nameof(AnimancerPlayable)}.{nameof(Speed)} instead.", animator);
                }
            }
#endif

            Current = this;
            DeltaTime = info.deltaTime;

            // Custom Updatables.
            Debug.Assert(_CurrentUpdatable == -1, UpdatableLoopStartError);
            _CurrentUpdatable = _Updatables.Count;
            ContinueUpdatableLoop:
            try
            {
                while (--_CurrentUpdatable >= 0)
                {
                    _Updatables[_CurrentUpdatable].EarlyUpdate();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, Component as Object);
                goto ContinueUpdatableLoop;
            }

            // Dirty Nodes.
            Debug.Assert(_CurrentNode == -1, UpdatableLoopStartError);
            _CurrentNode = _DirtyNodes.Count;
            ContinueNodeLoop:
            try
            {
                while (--_CurrentNode >= 0)
                {
                    var node = _DirtyNodes[_CurrentNode];
                    if (node._Playable.IsValid())
                    {
                        node.Update(out var needsMoreUpdates);
                        if (needsMoreUpdates)
                            continue;
                    }

                    _DirtyNodes.RemoveAtSwap(_CurrentNode);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, Component as Object);
                goto ContinueNodeLoop;
            }

            _LateUpdate.IsConnected = _Updatables.Count != 0;

            // Any time before or during this method will still have all Playables at their time from last frame, so we
            // don't want them to think their time is dirty until we are done.
            FrameID = info.frameId;
            Current = null;
        }

        /************************************************************************************************************************/
        #region Late Update
        /************************************************************************************************************************/

        private static AnimancerPlayable _CurrentLateUpdate;

        /// <summary>Indicates whether the internal <see cref="LateUpdate"/> is currently executing.</summary>
        public static bool IsRunningLateUpdate(AnimancerPlayable animancer) => _CurrentLateUpdate == animancer;

        /************************************************************************************************************************/

        /// <summary>
        /// A <see cref="PlayableBehaviour"/> which connects to a later port than the main layer mixer so that its
        /// <see cref="PrepareFrame"/> method gets called after all other playables are updated in order to call
        /// <see cref="IUpdatable.LateUpdate"/> on the <see cref="_Updatables"/>.
        /// </summary>
        private sealed class LateUpdate : PlayableBehaviour
        {
            /************************************************************************************************************************/

            /// <summary>See <see cref="AnimancerPlayable.Template"/>.</summary>
            private static readonly LateUpdate Template = new LateUpdate();

            /// <summary>The <see cref="AnimancerPlayable"/> this behaviour is connected to.</summary>
            private AnimancerPlayable _Root;

            /// <summary>The underlying <see cref="Playable"/> of this behaviour.</summary>
            private Playable _Playable;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="LateUpdate"/> for the `root`.</summary>
            public static LateUpdate Create(AnimancerPlayable root)
            {
                var instance = ScriptPlayable<LateUpdate>.Create(root._Graph, Template, 0)
                    .GetBehaviour();
                instance._Root = root;
                return instance;
            }

            /************************************************************************************************************************/

            /// <summary>Called by Unity as it creates this <see cref="AnimancerPlayable"/>.</summary>
            public override void OnPlayableCreate(Playable playable) => _Playable = playable;

            /************************************************************************************************************************/

            private bool _IsConnected;

            /// <summary>
            /// Indicates whether this behaviour is connected to the <see cref="PlayableGraph"/> and thus, whether it
            /// will receive <see cref="PrepareFrame"/> calls.
            /// </summary>
            public bool IsConnected
            {
                get => _IsConnected;
                set
                {
                    if (value)
                    {
                        if (!_IsConnected)
                        {
                            _IsConnected = true;
                            _Root._Graph.Connect(_Playable, 0, _Root._RootPlayable, 1);
                        }
                    }
                    else
                    {
                        if (_IsConnected)
                        {
                            _IsConnected = false;
                            _Root._Graph.Disconnect(_Root._RootPlayable, 1);
                        }
                    }
                }
            }

            /************************************************************************************************************************/

            /// <summary>Calls <see cref="IUpdatable.LateUpdate"/> on everything that needs it.</summary>
            /// <remarks>
            /// Called by the <see cref="PlayableGraph"/> after the rest of the <see cref="Playable"/>s are evaluated.
            /// </remarks>
            public override void PrepareFrame(Playable playable, FrameData info)
            {
                _CurrentLateUpdate = Current = _Root;

                Debug.Assert(_CurrentUpdatable == -1, UpdatableLoopStartError);
                var updatables = _Root._Updatables;
                _CurrentUpdatable = updatables.Count;
                ContinueLoop:
                try
                {
                    while (--_CurrentUpdatable >= 0)
                    {
                        updatables[_CurrentUpdatable].LateUpdate();
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, _Root?.Component as Object);
                    goto ContinueLoop;
                }

                // Ideally we would be able to update the dirty nodes here instead of in the early update so that they
                // can respond immediately to the effects of the late update.

                // However, doing that with KeepChildrenConnected == false (the default for efficiency) causes problems
                // where states that aren't connected early (before they update) don't affect the output even though
                // weight changes do apply. So in the first frame when cross fading to a new animation it will lower
                // the weight of the previous state a bit without the corresponding increase to the new animation's
                // weight having any effect, giving a total weight less than 1 and thus an incorrect output.

                _CurrentLateUpdate = Current = null;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Editor
#if UNITY_EDITOR
        /************************************************************************************************************************/

        private static List<AnimancerPlayable> _AllInstances;

        /// <summary>[Editor-Only]
        /// Registers this object in the list of things that need to be cleaned up in Edit Mode.
        /// </summary>
        private void RegisterInstance()
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (AnimancerUtilities.NewIfNull(ref _AllInstances))
            {
                UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += () =>
                {
                    for (int i = _AllInstances.Count - 1; i >= 0; i--)
                    {
                        var playable = _AllInstances[i];
                        if (playable.IsValid)
                            playable.Destroy();
                    }

                    _AllInstances.Clear();
                };
            }
            else// Clear out any old instances.
            {
                for (int i = _AllInstances.Count - 1; i >= 0; i--)
                {
                    var playable = _AllInstances[i];
                    if (!playable.ShouldStayAlive())
                    {
                        if (playable != null &&
                            playable.IsValid)
                            playable.Destroy();

                        _AllInstances.RemoveAt(i);
                    }
                }
            }

            _AllInstances.Add(this);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Determines whether this playable should stay alive or be destroyed.
        /// </summary>
        private bool ShouldStayAlive()
        {
            if (!IsValid)
                return false;

            if (Component == null)
                return true;

            var obj = Component as Object;
            if (!ReferenceEquals(obj, null) && obj == null)
                return false;

            if (Component.Animator == null)
                return false;

            return true;
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Returns true if the `initial` mode was <see cref="AnimatorUpdateMode.AnimatePhysics"/> and the `current`
        /// has changed to another mode or if the `initial` mode was something else and the `current` has changed to
        /// <see cref="AnimatorUpdateMode.AnimatePhysics"/>.
        /// </summary>
        public static bool HasChangedToOrFromAnimatePhysics(AnimatorUpdateMode? initial, AnimatorUpdateMode current)
        {
            if (initial == null)
                return false;

            var wasAnimatePhysics = initial.Value == AnimatorUpdateMode.AnimatePhysics;
            var isAnimatePhysics = current == AnimatorUpdateMode.AnimatePhysics;
            return wasAnimatePhysics != isAnimatePhysics;
        }

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
    }
}

