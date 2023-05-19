// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>
    /// A layer on which animations can play with their states managed independantly of other layers while blending the
    /// output with those layers.
    /// </summary>
    ///
    /// <remarks>
    /// This class can be used as a custom yield instruction to wait until all animations finish playing.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/layers">Layers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerLayer
    /// 
    public sealed class AnimancerLayer : AnimancerNode, IAnimationClipCollection
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        /// <summary>[Internal] Creates a new <see cref="AnimancerLayer"/>.</summary>
        internal AnimancerLayer(AnimancerPlayable root, int index)
        {
            Root = root;
            Index = index;
            CreatePlayable();

            if (ApplyParentAnimatorIK)
                _ApplyAnimatorIK = root.ApplyAnimatorIK;
            if (ApplyParentFootIK)
                _ApplyFootIK = root.ApplyFootIK;
        }

        /************************************************************************************************************************/

        /// <summary>Creates and assigns the <see cref="AnimationMixerPlayable"/> managed by this layer.</summary>
        protected override void CreatePlayable(out Playable playable) => playable = AnimationMixerPlayable.Create(Root._Graph);

        /************************************************************************************************************************/

        /// <summary>A layer is its own root.</summary>
        public override AnimancerLayer Layer => this;

        /// <summary>The <see cref="AnimancerNode.Root"/> receives the output of the <see cref="Playable"/>.</summary>
        public override IPlayableWrapper Parent => Root;

        /// <summary>Indicates whether child playables should stay connected to this layer at all times.</summary>
        public override bool KeepChildrenConnected => Root.KeepChildrenConnected;

        /************************************************************************************************************************/

        /// <summary>All of the animation states connected to this layer.</summary>
        private readonly List<AnimancerState> States = new List<AnimancerState>();

        /************************************************************************************************************************/

        private AnimancerState _CurrentState;

        /// <summary>
        /// The state of the animation currently being played.
        /// <para></para>
        /// Specifically, this is the state that was most recently started using any of the Play or CrossFade methods
        /// on this layer. States controlled individually via methods in the <see cref="AnimancerState"/> itself will
        /// not register in this property.
        /// <para></para>
        /// Each time this property changes, the <see cref="CommandCount"/> is incremented.
        /// </summary>
        public AnimancerState CurrentState
        {
            get => _CurrentState;
            private set
            {
                _CurrentState = value;
                CommandCount++;
            }
        }

        /// <summary>
        /// The number of times the <see cref="CurrentState"/> has changed. By storing this value and later comparing
        /// the stored value to the current value, you can determine whether the state has been changed since then,
        /// even it has changed back to the same state.
        /// </summary>
        public int CommandCount { get; private set; }

#if UNITY_EDITOR
        /// <summary>[Editor-Only] [Internal] Increases the <see cref="CommandCount"/> by 1.</summary>
        internal void IncrementCommandCount() => CommandCount++;
#endif

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Determines whether this layer is set to additive blending. Otherwise it will override any earlier layers.
        /// </summary>
        public bool IsAdditive
        {
            get => Root.Layers.IsAdditive(Index);
            set => Root.Layers.SetAdditive(Index, value);
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Sets an <see cref="AvatarMask"/> to determine which bones this layer will affect.
        /// </summary>
        public void SetMask(AvatarMask mask)
        {
            Root.Layers.SetMask(Index, mask);
        }

#if UNITY_ASSERTIONS
        /// <summary>[Assert-Only] The <see cref="AvatarMask"/> that determines which bones this layer will affect.</summary>
        internal AvatarMask _Mask;
#endif

        /************************************************************************************************************************/

        /// <summary>
        /// The average velocity of the root motion of all currently playing animations, taking their current
        /// <see cref="AnimancerNode.Weight"/> into account.
        /// </summary>
        public Vector3 AverageVelocity
        {
            get
            {
                var velocity = default(Vector3);

                for (int i = States.Count - 1; i >= 0; i--)
                {
                    var state = States[i];
                    velocity += state.AverageVelocity * state.Weight;
                }

                return velocity;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Child States
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override int ChildCount => States.Count;

        /// <summary>Returns the state connected to the specified `index` as a child of this layer.</summary>
        /// <remarks>This method is identical to <see cref="this[int]"/>.</remarks>
        public override AnimancerState GetChild(int index) => States[index];

        /// <summary>Returns the state connected to the specified `index` as a child of this layer.</summary>
        /// <remarks>This indexer is identical to <see cref="GetChild(int)"/>.</remarks>
        public AnimancerState this[int index] => States[index];

        /************************************************************************************************************************/

        /// <summary>
        /// Adds a new port and uses <see cref="AnimancerState.SetParent"/> to connect the `state` to it.
        /// </summary>
        public void AddChild(AnimancerState state)
        {
            if (state.Parent == this)
                return;

            state.SetRoot(Root);

            var index = States.Count;
            States.Add(null);// OnAddChild will assign the state.
            _Playable.SetInputCount(index + 1);
            state.SetParent(this, index);
        }

        /************************************************************************************************************************/

        /// <summary>Connects the `state` to this layer at its <see cref="AnimancerNode.Index"/>.</summary>
        protected internal override void OnAddChild(AnimancerState state) => OnAddChild(States, state);

        /************************************************************************************************************************/

        /// <summary>Disconnects the `state` from this layer at its <see cref="AnimancerNode.Index"/>.</summary>
        protected internal override void OnRemoveChild(AnimancerState state)
        {
            var index = state.Index;
            Validate.AssertCanRemoveChild(state, States);

            if (_Playable.GetInput(index).IsValid())
                Root._Graph.Disconnect(_Playable, index);

            // Swap the last state into the place of the one that was just removed.
            var last = States.Count - 1;
            if (index < last)
            {
                state = States[last];
                state.DisconnectFromGraph();

                States[index] = state;
                state.Index = index;

                if (state.Weight != 0 || Root.KeepChildrenConnected)
                    state.ConnectToGraph();
            }

            States.RemoveAt(last);
            _Playable.SetInputCount(last);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns an enumerator that will iterate through all states connected directly to this layer (not inside
        /// <see cref="MixerState"/>s).
        /// </summary>
        public override IEnumerator<AnimancerState> GetEnumerator() => States.GetEnumerator();

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Create State
        /************************************************************************************************************************/

        /// <summary>
        /// Creates and returns a new <see cref="ClipState"/> to play the `clip`.
        /// </summary>
        /// <remarks>
        /// <see cref="AnimancerPlayable.GetKey"/> is used to determine the <see cref="AnimancerState.Key"/>.
        /// </remarks>
        public ClipState CreateState(AnimationClip clip) => CreateState(Root.GetKey(clip), clip);

        /// <summary>
        /// Creates and returns a new <see cref="ClipState"/> to play the `clip` and registers it with the `key`.
        /// </summary>
        public ClipState CreateState(object key, AnimationClip clip)
        {
            var state = new ClipState(clip)
            {
                _Key = key,
            };
            AddChild(state);
            return state;
        }

        /// <summary>Creates and returns a new <typeparamref name="T"/> attached to this layer.</summary>
        public T CreateState<T>() where T : AnimancerState, new()
        {
            var state = new T();
            AddChild(state);
            return state;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="GetOrCreateState(AnimationClip, bool)"/> for each of the specified clips.
        /// <para></para>
        /// If you only want to create a single state, use <see cref="CreateState(AnimationClip)"/>.
        /// </summary>
        public void CreateIfNew(AnimationClip clip0, AnimationClip clip1)
        {
            GetOrCreateState(clip0);
            GetOrCreateState(clip1);
        }

        /// <summary>
        /// Calls <see cref="GetOrCreateState(AnimationClip, bool)"/> for each of the specified clips.
        /// <para></para>
        /// If you only want to create a single state, use <see cref="CreateState(AnimationClip)"/>.
        /// </summary>
        public void CreateIfNew(AnimationClip clip0, AnimationClip clip1, AnimationClip clip2)
        {
            GetOrCreateState(clip0);
            GetOrCreateState(clip1);
            GetOrCreateState(clip2);
        }

        /// <summary>
        /// Calls <see cref="GetOrCreateState(AnimationClip, bool)"/> for each of the specified clips.
        /// <para></para>
        /// If you only want to create a single state, use <see cref="CreateState(AnimationClip)"/>.
        /// </summary>
        public void CreateIfNew(AnimationClip clip0, AnimationClip clip1, AnimationClip clip2, AnimationClip clip3)
        {
            GetOrCreateState(clip0);
            GetOrCreateState(clip1);
            GetOrCreateState(clip2);
            GetOrCreateState(clip3);
        }

        /// <summary>
        /// Calls <see cref="GetOrCreateState(AnimationClip, bool)"/> for each of the specified clips.
        /// <para></para>
        /// If you only want to create a single state, use <see cref="CreateState(AnimationClip)"/>.
        /// </summary>
        public void CreateIfNew(params AnimationClip[] clips)
        {
            if (clips == null)
                return;

            var count = clips.Length;
            for (int i = 0; i < count; i++)
            {
                var clip = clips[i];
                if (clip != null)
                    GetOrCreateState(clip);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="AnimancerPlayable.GetKey"/> and returns the state which registered with that key or
        /// creates one if it doesn't exist.
        /// <para></para>
        /// If the state already exists but has the wrong <see cref="AnimancerState.Clip"/>, the `allowSetClip`
        /// parameter determines what will happen. False causes it to throw an <see cref="ArgumentException"/> while
        /// true allows it to change the <see cref="AnimancerState.Clip"/>. Note that the change is somewhat costly to
        /// performance to use with caution.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public AnimancerState GetOrCreateState(AnimationClip clip, bool allowSetClip = false)
        {
            return GetOrCreateState(Root.GetKey(clip), clip, allowSetClip);
        }

        /// <summary>
        /// Returns the state registered with the <see cref="IHasKey.Key"/> if there is one. Otherwise
        /// this method uses <see cref="ITransition.CreateState"/> to create a new one and registers it with
        /// that key before returning it.
        /// </summary>
        public AnimancerState GetOrCreateState(ITransition transition)
        {
            var state = Root.States.GetOrCreate(transition);
            state.LayerIndex = Index;
            return state;
        }

        /// <summary>
        /// Returns the state which registered with the `key` or creates one if it doesn't exist.
        /// <para></para>
        /// If the state already exists but has the wrong <see cref="AnimancerState.Clip"/>, the `allowSetClip`
        /// parameter determines what will happen. False causes it to throw an <see cref="ArgumentException"/> while
        /// true allows it to change the <see cref="AnimancerState.Clip"/>. Note that the change is somewhat costly to
        /// performance to use with caution.
        /// <seealso cref="AnimancerState"/>
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException">The `key` is null.</exception>
        /// <remarks>
        /// See also: <see cref="AnimancerPlayable.StateDictionary.GetOrCreate(object, AnimationClip, bool)"/>.
        /// </remarks>
        public AnimancerState GetOrCreateState(object key, AnimationClip clip, bool allowSetClip = false)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (Root.States.TryGet(key, out var state))
            {
                // If a state exists with the 'key' but has the wrong clip, either change it or complain.
                if (!ReferenceEquals(state.Clip, clip))
                {
                    if (allowSetClip)
                    {
                        state.Clip = clip;
                    }
                    else
                    {
                        throw new ArgumentException(AnimancerPlayable.StateDictionary.GetClipMismatchError(key, state.Clip, clip));
                    }
                }
                else// Otherwise make sure it is on the correct layer.
                {
                    AddChild(state);
                }
            }
            else
            {
                state = CreateState(key, clip);
            }

            return state;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Destroys all states connected to this layer. This operation cannot be undone.
        /// </summary>
        public void DestroyStates()
        {
            for (int i = States.Count - 1; i >= 0; i--)
            {
                States[i].Destroy();
            }

            States.Clear();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Play Management
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected internal override void OnStartFade()
        {
            for (int i = States.Count - 1; i >= 0; i--)
                States[i].OnStartFade();
        }

        /************************************************************************************************************************/
        // Play Immediately.
        /************************************************************************************************************************/

        /// <summary>Stops all other animations, plays the `clip`, and returns its state.</summary>
        /// <remarks>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// To restart it from the beginning you can use <c>...Play(clip).Time = 0;</c>.
        /// </remarks>
        public AnimancerState Play(AnimationClip clip)
            => Play(GetOrCreateState(clip));

        /// <summary>Stops all other animations, plays the `state`, and returns it.</summary>
        /// <remarks>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// To restart it from the beginning you can use <c>...Play(clip).Time = 0;</c>.
        /// </remarks>
        public AnimancerState Play(AnimancerState state)
        {
            if (Weight == 0 && TargetWeight == 0)
                Weight = 1;

            AddChild(state);

            CurrentState = state;

            state.Play();

            for (int i = States.Count - 1; i >= 0; i--)
            {
                var otherState = States[i];
                if (otherState != state)
                    otherState.Stop();
            }

            return state;
        }

        /************************************************************************************************************************/
        // Cross Fade.
        /************************************************************************************************************************/

        /// <summary>
        /// Starts fading in the `clip` over the course of the `fadeDuration` while fading out all others in the same
        /// layer. Returns its state.
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
            => Play(Root.States.GetOrCreate(clip), fadeDuration, mode);

        /// <summary>
        /// Starts fading in the `state` over the course of the `fadeDuration` while fading out all others in this
        /// layer. Returns the `state`.
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
            // Skip the fade if:
            if (fadeDuration <= 0 ||// There is no duration.
                (Root.SkipFirstFade && Index == 0 && Weight == 0))// Or this is Layer 0 and it has no weight.
            {
                if (mode == FadeMode.FromStart || mode == FadeMode.NormalizedFromStart)
                    state.Time = 0;

                Weight = 1;
                return Play(state);
            }

            EvaluateFadeMode(mode, ref state, ref fadeDuration);

            StartFade(1, fadeDuration);
            if (Weight == 0)
                return Play(state);

            AddChild(state);

            CurrentState = state;

            // If the state is already playing or will finish fading in faster than this new fade,
            // continue the existing fade but still pretend it was restarted.
            if (state.IsPlaying && state.TargetWeight == 1 &&
                (state.Weight == 1 || state.FadeSpeed * fadeDuration > Math.Abs(1 - state.Weight)))
            {
                OnStartFade();
            }
            else// Otherwise fade in the target state and fade out all others.
            {
                state.IsPlaying = true;
                state.StartFade(1, fadeDuration);

                for (int i = States.Count - 1; i >= 0; i--)
                {
                    var otherState = States[i];
                    if (otherState != state)
                        otherState.StartFade(0, fadeDuration);
                }
            }

            return state;
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
            var state = Root.States.GetOrCreate(transition);
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
        /// To restart it from the beginning you can simply set the returned state's time to 0.
        /// </remarks>
        public AnimancerState TryPlay(object key)
            => Root.States.TryGet(key, out var state) ? Play(state) : null;

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
        public AnimancerState TryPlay(object key, float fadeDuration, FadeMode mode = FadeMode.FixedSpeed)
            => Root.States.TryGet(key, out var state) ? Play(state, fadeDuration, mode) : null;

        /************************************************************************************************************************/

        /// <summary>
        /// Manipulates the other parameters according to the `mode`.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// The <see cref="AnimancerState.Clip"/> is null when using <see cref="FadeMode.FromStart"/> or
        /// <see cref="FadeMode.NormalizedFromStart"/>.
        /// </exception>
        private void EvaluateFadeMode(FadeMode mode, ref AnimancerState state, ref float fadeDuration)
        {
            switch (mode)
            {
                case FadeMode.FixedSpeed:
                    fadeDuration *= Math.Abs(1 - state.Weight);
                    break;

                case FadeMode.FixedDuration:
                    break;

                case FadeMode.FromStart:
                    {
#if UNITY_ASSERTIONS
                        if (state.Clip == null)
                            throw new ArgumentException(
                                $"{nameof(FadeMode)}.{nameof(FadeMode.FromStart)} can only be used on {nameof(ClipState)}s." +
                                $" State = {state}");
#endif

                        var previousState = state;
                        state = GetOrCreateWeightlessState(state);
                        if (previousState != state)
                        {
                            var previousLayer = previousState.Layer;
                            if (previousLayer != this && previousLayer.CurrentState == previousState)
                                previousLayer.StartFade(0, fadeDuration);
                        }

                        break;
                    }

                case FadeMode.NormalizedSpeed:
                    fadeDuration *= Math.Abs(1 - state.Weight) * state.Length;
                    break;

                case FadeMode.NormalizedDuration:
                    fadeDuration *= state.Length;
                    break;

                case FadeMode.NormalizedFromStart:
                    {
#if UNITY_ASSERTIONS
                        if (state.Clip == null)
                            throw new ArgumentException(
                                $"{nameof(FadeMode)}.{nameof(FadeMode.NormalizedFromStart)} can only be used on {nameof(ClipState)}s." +
                                $" State = {state}");
#endif

                        var previousState = state;
                        state = GetOrCreateWeightlessState(state);
                        fadeDuration *= state.Length;
                        if (previousState != state)
                        {
                            var previousLayer = previousState.Layer;
                            if (previousLayer != this && previousLayer.CurrentState == previousState)
                                previousLayer.StartFade(0, fadeDuration);
                        }
                        break;
                    }

                default:
                    throw new ArgumentException($"Invalid {nameof(FadeMode)}: " + mode, nameof(mode));
            }
        }

        /************************************************************************************************************************/

#if UNITY_ASSERTIONS
        /// <summary>[Assert-Only]
        /// The maximum number of duplicate states that can be created by <see cref="GetOrCreateWeightlessState"/> for
        /// a single clip before it will start giving usage warnings. Default = 5.
        /// </summary>
        public static int MaxStateDepth { get; private set; } = 5;
#endif

        /// <summary>[Assert-Conditional] Sets the <see cref="MaxStateDepth"/>.</summary>
        /// <remarks>This would not need to be a separate method if C# supported conditional property setters.</remarks>
        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        public static void SetMaxStateDepth(int depth)
        {
#if UNITY_ASSERTIONS
            MaxStateDepth = depth;
#endif
        }

        /// <summary>
        /// If the `state` is not currently at 0 <see cref="AnimancerNode.Weight"/>, this method finds a copy of it
        /// which is at 0 or creates a new one.
        /// </summary>
        /// <exception cref="InvalidOperationException">The <see cref="AnimancerState.Clip"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// More states have been created for this <see cref="AnimancerState.Clip"/> than the
        /// <see cref="MaxStateDepth"/> allows.
        /// </exception>
        public AnimancerState GetOrCreateWeightlessState(AnimancerState state)
        {
            if (state.Weight != 0)
            {
                var clip = state.Clip;
                if (clip == null)
                {
                    // We could probably support any state type by giving them a Clone method, but that would take a
                    // lot of work for something that might never get used.
                    throw new InvalidOperationException(
                        $"{nameof(GetOrCreateWeightlessState)} can only be used on {nameof(ClipState)}s. State = " + state);
                }

                // Get the default state registered with the clip.
                if (state.Key as Object != clip)
                    state = Root.States.GetOrCreate(clip, clip);

#if UNITY_ASSERTIONS
                int depth = 0;
#endif

                // If that state is not at 0 weight, get or create another state registered using the previous state as a key.
                // Keep going through states in this manner until you find one at 0 weight.
                while (state.Weight != 0)
                {
                    // Explicitly cast the state to an object to avoid the overload that warns about using a state as a key.
                    state = Root.States.GetOrCreate((object)state, clip);

#if UNITY_ASSERTIONS
                    if (++depth == MaxStateDepth)
                    {
                        throw new ArgumentOutOfRangeException(nameof(depth), nameof(GetOrCreateWeightlessState) + " has created " +
                            MaxStateDepth + " or more states for a single clip." +
                            " This is most likely a result of calling the method repeatedly on consecutive frames." +
                            $" You probably just want to use {nameof(FadeMode)}.{nameof(FadeMode.FixedSpeed)} instead," +
                            $" but you can call {nameof(AnimancerLayer)}.{nameof(SetMaxStateDepth)} if necessary.");
                    }
#endif
                }
            }

            // Make sure it is on this layer and at time 0.
            AddChild(state);
            state.Time = 0;

            return state;
        }

        /************************************************************************************************************************/
        // Stopping
        /************************************************************************************************************************/

        /// <summary>
        /// Sets <see cref="AnimancerNode.Weight"/> = 0 and calls <see cref="AnimancerState.Stop"/> on all animations
        /// to stop them from playing and rewind them to the start.
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            CurrentState = null;

            for (int i = States.Count - 1; i >= 0; i--)
                States[i].Stop();
        }

        /************************************************************************************************************************/
        // Checking
        /************************************************************************************************************************/

        /// <summary>
        /// Returns true if the `clip` is currently being played by at least one state.
        /// </summary>
        public bool IsPlayingClip(AnimationClip clip)
        {
            for (int i = States.Count - 1; i >= 0; i--)
            {
                var state = States[i];
                if (state.Clip == clip && state.IsPlaying)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if at least one animation is being played.
        /// </summary>
        public bool IsAnyStatePlaying()
        {
            for (int i = States.Count - 1; i >= 0; i--)
            {
                if (States[i].IsPlaying)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the <see cref="CurrentState"/> is playing and hasn't yet reached its end.
        /// <para></para>
        /// This method is called by <see cref="IEnumerator.MoveNext"/> so this object can be used as a custom yield
        /// instruction to wait until it finishes.
        /// </summary>
        protected internal override bool IsPlayingAndNotEnding() => _CurrentState != null && _CurrentState.IsPlayingAndNotEnding();

        /************************************************************************************************************************/

        /// <summary>
        /// Calculates the total <see cref="AnimancerNode.Weight"/> of all states in this layer.
        /// </summary>
        public float GetTotalWeight()
        {
            float weight = 0;

            for (int i = States.Count - 1; i >= 0; i--)
            {
                weight += States[i].Weight;
            }

            return weight;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Inverse Kinematics
        /************************************************************************************************************************/

        private bool _ApplyAnimatorIK;

        /// <inheritdoc/>
        public override bool ApplyAnimatorIK
        {
            get => _ApplyAnimatorIK;
            set => base.ApplyAnimatorIK = _ApplyAnimatorIK = value;
        }

        /************************************************************************************************************************/

        private bool _ApplyFootIK;

        /// <inheritdoc/>
        public override bool ApplyFootIK
        {
            get => _ApplyFootIK;
            set => base.ApplyFootIK = _ApplyFootIK = value;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Inspector
        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipCollection"/>]
        /// Gathers all the animations in this layer.
        /// </summary>
        public void GatherAnimationClips(ICollection<AnimationClip> clips) => clips.GatherFromSources(States);

        /************************************************************************************************************************/

        /// <summary>The Inspector display name of this layer.</summary>
        public override string ToString()
        {
#if UNITY_ASSERTIONS
            if (DebugName == null)
            {
                if (_Mask != null)
                    return _Mask.name;

                SetDebugName(Index == 0 ? "Base Layer" : "Layer " + Index);
            }

            return base.ToString();
#else
            return "Layer " + Index;
#endif
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void AppendDetails(StringBuilder text, string delimiter)
        {
            base.AppendDetails(text, delimiter);

            text.Append(delimiter).Append($"{nameof(CurrentState)}: ").Append(CurrentState);
            text.Append(delimiter).Append($"{nameof(CommandCount)}: ").Append(CommandCount);
            text.Append(delimiter).Append($"{nameof(IsAdditive)}: ").Append(IsAdditive);

#if UNITY_ASSERTIONS
            text.Append(delimiter).Append($"{nameof(AvatarMask)}: ").Append(AnimancerUtilities.ToStringOrNull(_Mask));
#endif
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

