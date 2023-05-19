// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Animancer
{
    /// <summary>[Pro-Only] Base class for <see cref="AnimancerState"/>s which blend other states together.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/MixerState
    /// 
    public abstract partial class MixerState : AnimancerState
    {
        /************************************************************************************************************************/
        #region Properties
        /************************************************************************************************************************/

        /// <summary>Mixers should keep child playables connected to the graph at all times.</summary>
        public override bool KeepChildrenConnected => true;

        /// <summary>A <see cref="MixerState"/> has no <see cref="AnimationClip"/>.</summary>
        public override AnimationClip Clip => null;

        /************************************************************************************************************************/

        /// <summary>Returns the collection of states connected to this mixer. Note that some elements may be null.</summary>
        /// <remarks>
        /// Getting an enumerator that automatically skips over null states is slower and creates garbage, so
        /// internally we use this property and perform null checks manually even though it increases the code
        /// complexity a bit.
        /// </remarks>
        public abstract IList<AnimancerState> ChildStates { get; }

        /// <inheritdoc/>
        public override int ChildCount => ChildStates.Count;

        /// <inheritdoc/>
        public override AnimancerState GetChild(int index) => ChildStates[index];

        /// <inheritdoc/>
        public override IEnumerator<AnimancerState> GetEnumerator()
        {
            var childStates = ChildStates;
            var count = childStates.Count;
            for (int i = 0; i < count; i++)
            {
                var state = childStates[i];
                if (state == null)
                    continue;

                yield return state;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnSetIsPlaying()
        {
            var childStates = ChildStates;
            for (int i = childStates.Count - 1; i >= 0; i--)
            {
                var state = childStates[i];
                if (state == null)
                    continue;

                state.IsPlaying = IsPlaying;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Are any child states looping?</summary>
        public override bool IsLooping
        {
            get
            {
                var childStates = ChildStates;
                for (int i = childStates.Count - 1; i >= 0; i--)
                {
                    var state = childStates[i];
                    if (state == null)
                        continue;

                    if (state.IsLooping)
                        return true;
                }

                return false;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The weighted average <see cref="AnimancerState.Time"/> of each child state according to their
        /// <see cref="AnimancerNode.Weight"/>.
        /// </summary>
        /// <remarks>
        /// If there are any <see cref="SynchronisedChildren"/>, only those states will be included in the getter
        /// calculation.
        /// </remarks>
        protected override float RawTime
        {
            get
            {
                RecalculateWeights();

                if (!GetSynchronisedTimeDetails(out var totalWeight, out var normalizedTime, out var length))
                    GetTimeDetails(out totalWeight, out normalizedTime, out length);

                if (totalWeight == 0)
                    return base.RawTime;

                totalWeight *= totalWeight;
                return normalizedTime * length / totalWeight;
            }
            set
            {
                var states = ChildStates;
                var childCount = states.Count;

                if (value == 0)
                    goto ZeroTime;

                var length = Length;
                if (length == 0)
                    goto ZeroTime;

                value /= length;// Normalize.

                while (--childCount >= 0)
                {
                    var state = states[childCount];
                    if (state != null)
                        state.NormalizedTime = value;
                }

                return;

                // If the value is 0, we can set the child times slightly more efficiently.
                ZeroTime:
                while (--childCount >= 0)
                {
                    var state = states[childCount];
                    if (state != null)
                        state.Time = 0;
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Gets the time details based on the <see cref="SynchronisedChildren"/>.</summary>
        private bool GetSynchronisedTimeDetails(out float totalWeight, out float normalizedTime, out float length)
        {
            totalWeight = 0;
            normalizedTime = 0;
            length = 0;

            if (_SynchronisedChildren != null)
            {
                for (int i = _SynchronisedChildren.Count - 1; i >= 0; i--)
                {
                    var state = _SynchronisedChildren[i];
                    var weight = state.Weight;
                    if (weight == 0)
                        continue;

                    var stateLength = state.Length;
                    if (stateLength == 0)
                        continue;

                    totalWeight += weight;
                    normalizedTime += state.Time / stateLength * weight;
                    length += stateLength * weight;
                }
            }

            return totalWeight > MinimumSynchroniseChildrenWeight;
        }

        /// <summary>Gets the time details based on all child states.</summary>
        private void GetTimeDetails(out float totalWeight, out float normalizedTime, out float length)
        {
            totalWeight = 0;
            normalizedTime = 0;
            length = 0;

            var states = ChildStates;
            for (int i = states.Count - 1; i >= 0; i--)
            {
                var state = states[i];
                if (state == null)
                    continue;

                var weight = state.Weight;
                if (weight == 0)
                    continue;

                var stateLength = state.Length;
                if (stateLength == 0)
                    continue;

                totalWeight += weight;
                normalizedTime += state.Time / stateLength * weight;
                length += stateLength * weight;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The weighted average <see cref="AnimancerState.Length"/> of each child state according to their
        /// <see cref="AnimancerNode.Weight"/>.
        /// </summary>
        public override float Length
        {
            get
            {
                RecalculateWeights();

                var length = 0f;
                var totalChildWeight = 0f;

                if (_SynchronisedChildren != null)
                {
                    for (int i = _SynchronisedChildren.Count - 1; i >= 0; i--)
                    {
                        var state = _SynchronisedChildren[i];
                        var weight = state.Weight;
                        if (weight == 0)
                            continue;

                        var stateLength = state.Length;
                        if (stateLength == 0)
                            continue;

                        totalChildWeight += weight;
                        length += stateLength * weight;
                    }
                }

                if (totalChildWeight > 0)
                    return length / totalChildWeight;

                var states = ChildStates;
                totalChildWeight = CalculateTotalWeight(states);
                if (totalChildWeight <= 0)
                    return 0;

                for (int i = states.Count - 1; i >= 0; i--)
                {
                    var state = states[i];
                    if (state != null)
                        length += state.Length * state.Weight;
                }

                return length / totalChildWeight;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Initialisation
        /************************************************************************************************************************/

        /// <summary>Creates and assigns the <see cref="AnimationMixerPlayable"/> managed by this state.</summary>
        protected override void CreatePlayable(out Playable playable)
        {
            playable = AnimationMixerPlayable.Create(Root._Graph, ChildStates.Count, false);
            RecalculateWeights();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates and returns a new <see cref="ClipState"/> to play the `clip` with this mixer as its parent.
        /// </summary>
        public ClipState CreateChild(int index, AnimationClip clip)
        {
            var state = new ClipState(clip);
            state.SetParent(this, index);
            state.IsPlaying = IsPlaying;
            return state;
        }

        /// <summary>
        /// Calls <see cref="AnimancerUtilities.CreateStateAndApply"/> and sets this mixer as the state's parent.
        /// </summary>
        public AnimancerState CreateChild(int index, ITransition transition)
        {
            var state = transition.CreateStateAndApply(Root);
            state.SetParent(this, index);
            state.IsPlaying = IsPlaying;
            return state;
        }

        /// <summary>Calls <see cref="CreateChild(int, AnimationClip)"/> or <see cref="CreateChild(int, ITransition)"/>.</summary>
        public AnimancerState CreateChild(int index, Object state)
        {
            if (state is AnimationClip clip)
            {
                return CreateChild(index, clip);
            }
            else if (state is ITransition transition)
            {
                return CreateChild(index, transition);
            }
            else return null;
        }

        /************************************************************************************************************************/

        /// <summary>Assigns the `state` as a child of this mixer.</summary>
        public void SetChild(int index, AnimancerState state) => state.SetParent(this, index);

        /************************************************************************************************************************/

        /// <summary>Connects the `state` to this mixer at its <see cref="AnimancerNode.Index"/>.</summary>
        protected internal override void OnAddChild(AnimancerState state)
        {
            OnAddChild(ChildStates, state);

            if (AutoSynchroniseChildren)
                Synchronise(state);

#if UNITY_ASSERTIONS
            if (_IsGeneratedName)
            {
                _IsGeneratedName = false;
                SetDebugName(null);
            }
#endif
        }

        /************************************************************************************************************************/

        /// <summary>Disconnects the `state` from this mixer at its <see cref="AnimancerNode.Index"/>.</summary>
        protected internal override void OnRemoveChild(AnimancerState state)
        {
            if (_SynchronisedChildren != null)
                _SynchronisedChildren.Remove(state);

            var states = ChildStates;
            Validate.AssertCanRemoveChild(state, states);
            states[state.Index] = null;
            Root?._Graph.Disconnect(_Playable, state.Index);

#if UNITY_ASSERTIONS
            if (_IsGeneratedName)
            {
                _IsGeneratedName = false;
                SetDebugName(null);
            }
#endif
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Destroy()
        {
            DestroyChildren();
            base.Destroy();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Destroys all <see cref="ChildStates"/> connected to this mixer. This operation cannot be undone.
        /// </summary>
        public void DestroyChildren()
        {
            var states = ChildStates;
            for (int i = states.Count - 1; i >= 0; i--)
            {
                var state = states[i];
                if (state != null)
                    state.Destroy();
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Jobs
        /************************************************************************************************************************/

        /// <summary>
        /// Creates an <see cref="AnimationScriptPlayable"/> to run the specified Animation Job instead of the usual
        /// <see cref="AnimationMixerPlayable"/>.
        /// </summary>
        /// <example><code>
        /// var job = new MyJob();// A struct that implements IAnimationJob.
        /// var mixer = new WhateverMixerType();
        /// mixer.CreatePlayable(animancer, job);
        /// // Use mixer.Initialise and CreateState to make the children as normal.
        /// </code>
        /// See also: <seealso cref="CreatePlayable{T}(out Playable, T, bool)"/>
        /// </example>
        public AnimationScriptPlayable CreatePlayable<T>(AnimancerPlayable root, T job, bool processInputs = false)
            where T : struct, IAnimationJob
        {
            SetRoot(null);

            Root = root;
            root.States.Register(Key, this);

            var playable = AnimationScriptPlayable.Create(root._Graph, job, ChildCount);

            if (!processInputs)
                playable.SetProcessInputs(false);

            for (int i = ChildCount - 1; i >= 0; i--)
                GetChild(i)?.SetRoot(root);

            return playable;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates an <see cref="AnimationScriptPlayable"/> to run the specified Animation Job instead of the usual
        /// <see cref="AnimationMixerPlayable"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/source/creating-custom-states">Creating Custom States</see>
        /// </remarks>
        /// 
        /// <example><code>
        /// public class MyMixer : LinearMixerState
        /// {
        ///     protected override void CreatePlayable(out Playable playable)
        ///     {
        ///         CreatePlayable(out playable, new MyJob());
        ///     }
        /// 
        ///     private struct MyJob : IAnimationJob
        ///     {
        ///         public void ProcessAnimation(AnimationStream stream)
        ///         {
        ///         }
        /// 
        ///         public void ProcessRootMotion(AnimationStream stream)
        ///         {
        ///         }
        ///     }
        /// }
        /// </code>
        /// See also: <seealso cref="CreatePlayable{T}(AnimancerPlayable, T, bool)"/>
        /// </example>
        protected void CreatePlayable<T>(out Playable playable, T job, bool processInputs = false)
            where T : struct, IAnimationJob
        {
            var scriptPlayable = AnimationScriptPlayable.Create(Root._Graph, job, ChildCount);

            if (!processInputs)
                scriptPlayable.SetProcessInputs(false);

            playable = scriptPlayable;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Gets the Animation Job data from the <see cref="AnimationScriptPlayable"/>.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// This mixer was not initialised using <see cref="CreatePlayable{T}(AnimancerPlayable, T, bool)"/>
        /// or <see cref="CreatePlayable{T}(out Playable, T, bool)"/>.
        /// </exception>
        public T GetJobData<T>()
            where T : struct, IAnimationJob
            => ((AnimationScriptPlayable)_Playable).GetJobData<T>();

        /// <summary>
        /// Sets the Animation Job data in the <see cref="AnimationScriptPlayable"/>.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// This mixer was not initialised using <see cref="CreatePlayable{T}(AnimancerPlayable, T, bool)"/>
        /// or <see cref="CreatePlayable{T}(out Playable, T, bool)"/>.
        /// </exception>
        public void SetJobData<T>(T value)
            where T : struct, IAnimationJob
            => ((AnimationScriptPlayable)_Playable).SetJobData<T>(value);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Updates
        /************************************************************************************************************************/

        /// <summary>Updates the time of this mixer and all of its child states.</summary>
        protected internal override void Update(out bool needsMoreUpdates)
        {
            base.Update(out needsMoreUpdates);

            if (RecalculateWeights())
            {
                // Apply the child weights immediately to ensure they are all in sync. Otherwise some of them might
                // have already updated before the mixer and would not apply it until next frame.
                var childStates = ChildStates;
                for (int i = childStates.Count - 1; i >= 0; i--)
                {
                    var state = childStates[i];
                    if (state == null)
                        continue;

                    state.ApplyWeight();
                }
            }

            ApplySynchroniseChildren(ref needsMoreUpdates);
        }

        /************************************************************************************************************************/

        /// <summary>Indicates whether the weights of all child states should be recalculated.</summary>
        public bool WeightsAreDirty { get; set; }

        /************************************************************************************************************************/

        /// <summary>
        /// If <see cref="WeightsAreDirty"/> this method recalculates the weights of all child states and returns true.
        /// </summary>
        public bool RecalculateWeights()
        {
            if (WeightsAreDirty)
            {
                ForceRecalculateWeights();

                Debug.Assert(!WeightsAreDirty,
                    $"{nameof(MixerState)}.{nameof(WeightsAreDirty)} was not set to false by {nameof(ForceRecalculateWeights)}().");

                return true;
            }
            else return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Recalculates the weights of all child states based on the current value of the
        /// <see cref="MixerState{TParameter}.Parameter"/> and the thresholds.
        /// <para></para>
        /// Overrides of this method must set <see cref="WeightsAreDirty"/> = false.
        /// </summary>
        protected virtual void ForceRecalculateWeights() { }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Synchronise Children
        /************************************************************************************************************************/

        /// <summary>Should newly added children be automatically added to the synchronisation list? Default true.</summary>
        public static bool AutoSynchroniseChildren { get; set; } = true;

        /// <summary>The minimum total weight of all children for their times to be synchronised (default 0.01).</summary>
        public static float MinimumSynchroniseChildrenWeight { get; set; } = 0.01f;

        /************************************************************************************************************************/

        private List<AnimancerState> _SynchronisedChildren;

        /// <summary>A copy of the internal list of child states that will have their times synchronised.</summary>
        /// <remarks>
        /// If this mixer is a child of another mixer, its synchronised children will be managed by the parent.
        /// <para></para>
        /// The getter allocates a new array if <see cref="SynchronisedChildCount"/> is greater than zero. Otherwise it
        /// returns null.
        /// </remarks>
        public AnimancerState[] SynchronisedChildren
        {
            get => SynchronisedChildCount > 0 ? _SynchronisedChildren.ToArray() : null;
            set
            {
                if (!AnimancerUtilities.NewIfNull(ref _SynchronisedChildren))
                    _SynchronisedChildren.Clear();

                for (int i = 0; i < value.Length; i++)
                    Synchronise(value[i]);
            }
        }

        /// <summary>The number of <see cref="SynchronisedChildren"/>.</summary>
        public int SynchronisedChildCount => _SynchronisedChildren != null ? _SynchronisedChildren.Count : 0;

        /************************************************************************************************************************/

        /// <summary>Is the `state` in the <see cref="SynchronisedChildren"/>?</summary>
        public bool IsSynchronised(AnimancerState state)
        {
            var synchroniser = GetParentMixer();
            return
                synchroniser._SynchronisedChildren != null &&
                synchroniser._SynchronisedChildren.Contains(state);
        }

        /************************************************************************************************************************/

        /// <summary>Adds the `state` to the <see cref="SynchronisedChildren"/>.</summary>
        /// <remarks>
        /// The `state` must be a child of this mixer.
        /// <para></para>
        /// If this mixer is a child of another mixer, the `state` will be added to the parent's
        /// <see cref="SynchronisedChildren"/> instead.
        /// </remarks>
        public void Synchronise(AnimancerState state)
        {
            if (state == null)
                return;

            Debug.Assert(IsChildOf(state, this));

            var synchroniser = GetParentMixer();
            synchroniser.SynchroniseDirect(state);
        }

        /// <summary>The internal implementation of <see cref="Synchronise"/>.</summary>
        private void SynchroniseDirect(AnimancerState state)
        {
            if (state == null)
                return;

            if (state is MixerState mixer)
            {
                for (int i = 0; i < mixer._SynchronisedChildren.Count; i++)
                    Synchronise(mixer._SynchronisedChildren[i]);
                mixer._SynchronisedChildren.Clear();
                return;
            }

#if UNITY_ASSERTIONS
            if (OptionalWarning.MixerSynchroniseZeroLength.IsEnabled() && state.Length == 0)
                OptionalWarning.MixerSynchroniseZeroLength.Log(
                    $"Adding a state with zero {nameof(AnimancerState.Length)} to the synchronisation list: '{state}'." +
                    $"\n\nSynchronisation is based on the {nameof(NormalizedTime)}" +
                    $" which can't be calculated if the {nameof(Length)} is 0." +
                    $" Some state types can change their {nameof(Length)}, in which case you can just disable this warning." +
                    $" But otherwise, the indicated state probably shouldn't be added to the synchronisation list.", Root?.Component);
#endif

            AnimancerUtilities.NewIfNull(ref _SynchronisedChildren);

#if UNITY_ASSERTIONS
            if (_SynchronisedChildren.Contains(state))
                Debug.LogError($"{state} is already in the {nameof(SynchronisedChildren)} list.");
#endif

            _SynchronisedChildren.Add(state);
            RequireUpdate();
        }

        /************************************************************************************************************************/

        /// <summary>Removes the `state` from the <see cref="SynchronisedChildren"/>.</summary>
        public void DontSynchronise(AnimancerState state)
        {
            var synchroniser = GetParentMixer();
            if (synchroniser._SynchronisedChildren != null &&
                synchroniser._SynchronisedChildren.Remove(state))
                state._Playable.SetSpeed(state.Speed);
        }

        /************************************************************************************************************************/

        /// <summary>Removes all children of this mixer from the <see cref="SynchronisedChildren"/>.</summary>
        public void DontSynchroniseChildren()
        {
            var synchroniser = GetParentMixer();
            var synchronisedChildren = synchroniser._SynchronisedChildren;
            if (synchronisedChildren == null)
                return;

            if (synchroniser == this)
            {
                for (int i = synchronisedChildren.Count - 1; i >= 0; i--)
                {
                    var state = synchronisedChildren[i];
                    state._Playable.SetSpeed(state.Speed);
                }

                synchronisedChildren.Clear();
            }
            else
            {
                for (int i = synchronisedChildren.Count - 1; i >= 0; i--)
                {
                    var state = synchronisedChildren[i];
                    if (IsChildOf(state, this))
                    {
                        state._Playable.SetSpeed(state.Speed);
                        synchronisedChildren.RemoveAt(i);
                    }
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Initialises the internal <see cref="SynchronisedChildren"/> list.</summary>
        /// <remarks>
        /// The array can be null or empty. Any elements not in the array will be treated as <c>true</c>.
        /// <para></para>
        /// This method can only be called before any <see cref="SynchronisedChildren"/> are added and also before this
        /// mixer is made the child of another mixer.
        /// </remarks>
        public void InitialiseSynchronisedChildren(params bool[] synchroniseChildren)
        {
            Debug.Assert(GetParentMixer() == this,
                $"{nameof(InitialiseSynchronisedChildren)} cannot be used on a mixer that is a child of another mixer.");
            Debug.Assert(_SynchronisedChildren == null,
                $"{nameof(InitialiseSynchronisedChildren)} cannot be used on a mixer already has synchronised children.");

            int flagCount;
            if (synchroniseChildren != null)
            {
                flagCount = synchroniseChildren.Length;
                for (int i = 0; i < flagCount; i++)
                    if (synchroniseChildren[i])
                        SynchroniseDirect(GetChild(i));
            }
            else flagCount = 0;

            for (int i = flagCount; i < ChildCount; i++)
                SynchroniseDirect(GetChild(i));
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the highest <see cref="MixerState"/> in the hierarchy above this mixer or this mixer itself if
        /// there are none above it.
        /// </summary>
        public MixerState GetParentMixer()
        {
            var mixer = this;

            var parent = Parent;
            while (parent != null)
            {
                if (parent is MixerState parentMixer)
                    mixer = parentMixer;

                parent = parent.Parent;
            }

            return mixer;
        }

        /// <summary>Returns the highest <see cref="MixerState"/> in the hierarchy above the `state` (inclusive).</summary>
        public static MixerState GetParentMixer(IPlayableWrapper node)
        {
            MixerState mixer = null;

            while (node != null)
            {
                if (node is MixerState parentMixer)
                    mixer = parentMixer;

                node = node.Parent;
            }

            return mixer;
        }

        /************************************************************************************************************************/

        /// <summary>Is the `child` a child of the `parent`?</summary>
        public static bool IsChildOf(IPlayableWrapper child, IPlayableWrapper parent)
        {
            while (true)
            {
                child = child.Parent;
                if (child == parent)
                    return true;
                else if (child == null)
                    return false;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Synchronises the <see cref="AnimancerState.NormalizedTime"/>s of the <see cref="SynchronisedChildren"/> by
        /// modifying their internal playable speeds.
        /// </summary>
        protected void ApplySynchroniseChildren(ref bool needsMoreUpdates)
        {
            if (_SynchronisedChildren == null || _SynchronisedChildren.Count <= 1)
                return;

            needsMoreUpdates = true;

            var deltaTime = AnimancerPlayable.DeltaTime * CalculateRealEffectiveSpeed();
            if (deltaTime == 0)
                return;

            var count = _SynchronisedChildren.Count;

            // Calculate the weighted average normalized time and normalized speed of all children.

            var totalWeight = 0f;
            var weightedNormalizedTime = 0f;
            var weightedNormalizedSpeed = 0f;

            for (int i = 0; i < count; i++)
            {
                var state = _SynchronisedChildren[i];

                var weight = state.Weight;
                if (weight == 0)
                    continue;

                var length = state.Length;
                if (length == 0)
                    continue;

                totalWeight += weight;

                weight /= length;

                weightedNormalizedTime += state.Time * weight;
                weightedNormalizedSpeed += state.Speed * weight;
            }

#if UNITY_ASSERTIONS
            if (!(totalWeight >= 0) || totalWeight == float.PositiveInfinity)// Reversed comparison includes NaN.
                throw new ArgumentOutOfRangeException(nameof(totalWeight), totalWeight, "Total weight must be a finite positive value");
            if (!weightedNormalizedTime.IsFinite())
                throw new ArgumentOutOfRangeException(nameof(weightedNormalizedTime), weightedNormalizedTime, "Time must be finite");
            if (!weightedNormalizedSpeed.IsFinite())
                throw new ArgumentOutOfRangeException(nameof(weightedNormalizedSpeed), weightedNormalizedSpeed, "Speed must be finite");
#endif

            // If the total weight is too small, pretend they are all at Weight = 1.
            if (totalWeight < MinimumSynchroniseChildrenWeight)
            {
                weightedNormalizedTime = 0;
                weightedNormalizedSpeed = 0;

                var nonZeroCount = 0;
                for (int i = 0; i < count; i++)
                {
                    var state = _SynchronisedChildren[i];

                    var length = state.Length;
                    if (length == 0)
                        continue;

                    length = 1f / length;

                    weightedNormalizedTime += state.Time * length;
                    weightedNormalizedSpeed += state.Speed * length;

                    nonZeroCount++;
                }

                totalWeight = nonZeroCount;
            }

            // Increment that time value according to delta time.
            weightedNormalizedTime += deltaTime * weightedNormalizedSpeed;
            weightedNormalizedTime /= totalWeight;

            var inverseDeltaTime = 1f / deltaTime;

            // Modify the speed of all children to go from their current normalized time to the average in one frame.
            for (int i = 0; i < count; i++)
            {
                var state = _SynchronisedChildren[i];
                var length = state.Length;
                if (length == 0)
                    continue;

                var normalizedTime = state.Time / length;
                var speed = (weightedNormalizedTime - normalizedTime) * length * inverseDeltaTime;
                state._Playable.SetSpeed(speed);
            }

            // After this, all the playables will update and advance according to their new speeds this frame.
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The multiplied <see cref="PlayableExtensions.GetSpeed"/> of this mixer and its parents down the
        /// hierarchy to determine the actual speed its output is being played at.
        /// </summary>
        /// <remarks>
        /// This can be different from the <see cref="AnimancerNode.EffectiveSpeed"/> because the
        /// <see cref="SynchronisedChildren"/> have their playable speed modified without setting their
        /// <see cref="AnimancerNode.Speed"/>.
        /// </remarks>
        public float CalculateRealEffectiveSpeed()
        {
            var speed = _Playable.GetSpeed() * Root.Speed;

            var parent = Parent;
            while (parent != null)
            {
                speed *= parent.Playable.GetSpeed();
                parent = parent.Parent;
            }

            return (float)speed;
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
        #region Other Methods
        /************************************************************************************************************************/

        /// <summary>Calculates the sum of the <see cref="AnimancerNode.Weight"/> of all `states`.</summary>
        public float CalculateTotalWeight(IList<AnimancerState> states)
        {
            var total = 0f;

            for (int i = states.Count - 1; i >= 0; i--)
            {
                var state = states[i];
                if (state != null)
                    total += state.Weight;
            }

            return total;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Sets <see cref="AnimancerState.Time"/> for all <see cref="ChildStates"/>.
        /// </summary>
        public void SetChildrenTime(float value, bool normalized = false)
        {
            var states = ChildStates;
            for (int i = states.Count - 1; i >= 0; i--)
            {
                var state = states[i];
                if (state == null)
                    continue;

                if (normalized)
                    state.NormalizedTime = value;
                else
                    state.Time = value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Sets the weight of all states after the `previousIndex` to 0.
        /// </summary>
        protected void DisableRemainingStates(int previousIndex)
        {
            var states = ChildStates;
            var childCount = states.Count;
            while (++previousIndex < childCount)
            {
                var state = states[previousIndex];
                if (state == null)
                    continue;

                state.Weight = 0;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the state at the specified `index` if it is not null, otherwise increments the index and checks
        /// again. Returns null if no state is found by the end of the <see cref="ChildStates"/>.
        /// </summary>
        protected AnimancerState GetNextState(ref int index)
        {
            var states = ChildStates;
            var childCount = states.Count;
            while (index < childCount)
            {
                var state = states[index];
                if (state != null)
                    return state;

                index++;
            }

            return null;
        }

        /************************************************************************************************************************/

        /// <summary>Divides the weight of all child states by the `totalWeight`.</summary>
        /// <remarks>
        /// If the `totalWeight` is equal to the total <see cref="AnimancerNode.Weight"/> of all child states, then the
        /// new total will become 1.
        /// </remarks>
        public void NormalizeWeights(float totalWeight)
        {
            if (totalWeight == 1)
                return;

            totalWeight = 1f / totalWeight;

            var states = ChildStates;
            for (int i = states.Count - 1; i >= 0; i--)
            {
                var state = states[i];
                if (state == null)
                    continue;

                state.Weight *= totalWeight;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Gets a user-friendly key to identify the `state` in the Inspector.</summary>
        public virtual string GetDisplayKey(AnimancerState state) => $"[{state.Index}]";

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override Vector3 AverageVelocity
        {
            get
            {
                var velocity = default(Vector3);

                RecalculateWeights();

                var childStates = ChildStates;
                for (int i = childStates.Count - 1; i >= 0; i--)
                {
                    var state = childStates[i];
                    if (state == null)
                        continue;

                    velocity += state.AverageVelocity * state.Weight;
                }

                return velocity;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Recalculates the <see cref="AnimancerState.Duration"/> of all child states so that they add up to 1.
        /// </summary>
        /// <exception cref="NullReferenceException">There are any states with no <see cref="Clip"/>.</exception>
        public void NormalizeDurations()
        {
            var childStates = ChildStates;

            int divideBy = 0;
            float totalDuration = 0f;

            // Count the number of states that exist and their total duration.
            var count = childStates.Count;
            for (int i = 0; i < count; i++)
            {
                var state = childStates[i];
                if (state == null)
                    continue;

                divideBy++;
                totalDuration += state.Duration;
            }

            // Calculate the average duration.
            totalDuration /= divideBy;

            // Set all states to that duration.
            for (int i = 0; i < count; i++)
            {
                var state = childStates[i];
                if (state == null)
                    continue;

                state.Duration = totalDuration;
            }
        }

        /************************************************************************************************************************/

#if UNITY_ASSERTIONS
        /// <summary>Has the <see cref="AnimancerNode.DebugName"/> been generated from the child states?</summary>
        private bool _IsGeneratedName;
#endif

        /// <summary>
        /// Returns a string describing the type of this mixer and the name of <see cref="Clip"/>s connected to it.
        /// </summary>
        public override string ToString()
        {
#if UNITY_ASSERTIONS
            if (DebugName != null)
                return DebugName;
#endif

            // Gather child names.
            var childNames = ObjectPool.AcquireList<string>();
            var allSimple = true;
            var states = ChildStates;
            var count = states.Count;
            for (int i = 0; i < count; i++)
            {
                var state = states[i];
                if (state == null)
                    continue;

                if (state.MainObject != null)
                {
                    childNames.Add(state.MainObject.name);
                }
                else
                {
                    childNames.Add(state.ToString());
                    allSimple = false;
                }
            }

            // If they all have a main object, check if they all have the same prefix so it doesn't need to be repeated.
            int prefixLength = 0;
            count = childNames.Count;
            if (count <= 1 || !allSimple)
            {
                prefixLength = 0;
            }
            else
            {
                var prefix = childNames[0];
                var shortest = prefixLength = prefix.Length;

                for (int iName = 0; iName < count; iName++)
                {
                    var childName = childNames[iName];

                    if (shortest > childName.Length)
                    {
                        shortest = prefixLength = childName.Length;
                    }

                    for (int iCharacter = 0; iCharacter < prefixLength; iCharacter++)
                    {
                        if (childName[iCharacter] != prefix[iCharacter])
                        {
                            prefixLength = iCharacter;
                            break;
                        }
                    }
                }

                if (prefixLength < 3 ||// Less than 3 characters probably isn't an intentional prefix.
                    prefixLength >= shortest)
                    prefixLength = 0;
            }

            // Build the mixer name.
            var mixerName = ObjectPool.AcquireStringBuilder();

            if (count > 0)
            {
                if (prefixLength > 0)
                    mixerName.Append(childNames[0], 0, prefixLength).Append('[');

                for (int i = 0; i < count; i++)
                {
                    if (i > 0)
                        mixerName.Append(", ");

                    var childName = childNames[i];
                    mixerName.Append(childName, prefixLength, childName.Length - prefixLength);
                }

                mixerName.Append(prefixLength > 0 ? "] (" : " (");
            }

            ObjectPool.Release(childNames);

            var type = GetType().FullName;
            if (type.EndsWith("State"))
                mixerName.Append(type, 0, type.Length - 5);
            else
                mixerName.Append(type);

            if (count > 0)
                mixerName.Append(')');

            var result = mixerName.ReleaseToString();

#if UNITY_ASSERTIONS
            _IsGeneratedName = true;
            SetDebugName(result);
#endif

            return result;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void AppendDetails(StringBuilder text, string delimiter)
        {
            base.AppendDetails(text, delimiter);

            text.Append(delimiter).Append("SynchronisedChildren: ");
            if (SynchronisedChildCount == 0)
            {
                text.Append("0");
            }
            else
            {
                text.Append(_SynchronisedChildren.Count);
                delimiter += Strings.Indent;
                for (int i = 0; i < _SynchronisedChildren.Count; i++)
                {
                    text.Append(delimiter)
                        .Append(_SynchronisedChildren[i]);
                }
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void GatherAnimationClips(ICollection<AnimationClip> clips) => clips.GatherFromSources((IList)ChildStates);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Inspector
        /************************************************************************************************************************/

        /// <summary>The number of parameters being managed by this state.</summary>
        protected virtual int ParameterCount => 0;

        /// <summary>Returns the name of a parameter being managed by this state.</summary>
        /// <exception cref="NotSupportedException">This state doesn't manage any parameters.</exception>
        protected virtual string GetParameterName(int index) => throw new NotSupportedException();

        /// <summary>Returns the type of a parameter being managed by this state.</summary>
        /// <exception cref="NotSupportedException">This state doesn't manage any parameters.</exception>
        protected virtual UnityEngine.AnimatorControllerParameterType GetParameterType(int index) => throw new NotSupportedException();

        /// <summary>Returns the value of a parameter being managed by this state.</summary>
        /// <exception cref="NotSupportedException">This state doesn't manage any parameters.</exception>
        protected virtual object GetParameterValue(int index) => throw new NotSupportedException();

        /// <summary>Sets the value of a parameter being managed by this state.</summary>
        /// <exception cref="NotSupportedException">This state doesn't manage any parameters.</exception>
        protected virtual void SetParameterValue(int index, object value) => throw new NotSupportedException();

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Returns a <see cref="Drawer{T}"/> for this state.</summary>
        protected internal override Editor.IAnimancerNodeDrawer CreateDrawer() => new Drawer<MixerState>(this);

        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Draws the Inspector GUI for a <see cref="MixerState"/>.</summary>
        /// <remarks>
        /// Unfortunately the tool used to generate this documentation does not currently support nested types with
        /// identical names, so only one <c>Drawer</c> class will actually have a documentation page.
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
        /// </remarks>
        public class Drawer<T> : Editor.ParametizedAnimancerStateDrawer<T> where T : MixerState
        {
            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="Drawer{T}"/> to manage the Inspector GUI for the `state`.
            /// </summary>
            public Drawer(T state) : base(state) { }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override int ParameterCount => Target.ParameterCount;

            /// <inheritdoc/>
            public override string GetParameterName(int index) => Target.GetParameterName(index);

            /// <inheritdoc/>
            public override UnityEngine.AnimatorControllerParameterType GetParameterType(int index) => Target.GetParameterType(index);

            /// <inheritdoc/>
            public override object GetParameterValue(int index) => Target.GetParameterValue(index);

            /// <inheritdoc/>
            public override void SetParameterValue(int index, object value) => Target.SetParameterValue(index, value);

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void AddContextMenuFunctions(GenericMenu menu)
            {
                base.AddContextMenuFunctions(menu);

                //var flagCount = Target._SynchronisedChildren != null ? Target._SynchronisedChildren.Length : 0;

                //var childCount = Target.ChildCount;
                //for (int i = 0; i < childCount; i++)
                //{
                //    var index = i;
                //    var sync = i >= flagCount || Target._SynchronisedChildren[i];
                //    var state = Target.GetChild(i);
                //    var label = $"Synchronise Children/[{i}] {AnimancerUtilities.ToStringOrNull(state)}";

                //    menu.AddItem(new GUIContent(label), sync, () =>
                //    {
                //        if (index >= flagCount)
                //        {
                //            var newSynchroniseChildren = new bool[index + 1];
                //            if (flagCount > 0)
                //                Array.Copy(Target._SynchronisedChildren, newSynchroniseChildren, flagCount);

                //            for (int j = flagCount; j < newSynchroniseChildren.Length; j++)
                //                newSynchroniseChildren[j] = true;

                //            Target._SynchronisedChildren = newSynchroniseChildren;
                //        }

                //        Target._SynchronisedChildren[index] = !sync;
                //        Target.RequireUpdate();
                //    });
                //}
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Transition
        /************************************************************************************************************************/

        /// <summary>
        /// Base class for serializable <see cref="ITransition"/>s which can create a particular type of
        /// <see cref="MixerState{TParameter}"/> when passed into
        /// <see cref="AnimancerPlayable.Play(ITransition)"/>.
        /// </summary>
        /// <remarks>
        /// Even though it has the <see cref="SerializableAttribute"/>, this class won't actually get serialized
        /// by Unity because it's generic and abstract. Each child class still needs to include the attribute.
        /// <para></para>
        /// Unfortunately the tool used to generate this documentation does not currently support nested types with
        /// identical names, so only one <c>Transition</c> class will actually have a documentation page.
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Transition_2
        /// 
        [Serializable]
        public abstract class Transition<TMixer, TParameter> : ManualMixerState.Transition<TMixer>
            where TMixer : MixerState<TParameter>
        {
            /************************************************************************************************************************/

            [SerializeField, HideInInspector]
            private TParameter[] _Thresholds;

            /// <summary>[<see cref="SerializeField"/>]
            /// The parameter values at which each of the states are used and blended.
            /// </summary>
            public ref TParameter[] Thresholds => ref _Thresholds;

            /// <summary>The name of the serialized backing field of <see cref="Thresholds"/>.</summary>
            public const string ThresholdsField = nameof(_Thresholds);

            /************************************************************************************************************************/

            [SerializeField]
            private TParameter _DefaultParameter;

            /// <summary>[<see cref="SerializeField"/>]
            /// The initial parameter value to give the mixer when it is first created.
            /// </summary>
            public ref TParameter DefaultParameter => ref _DefaultParameter;

            /// <summary>The name of the serialized backing field of <see cref="DefaultParameter"/>.</summary>
            public const string DefaultParameterField = nameof(_DefaultParameter);

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void InitialiseState()
            {
                base.InitialiseState();

                State.SetThresholds(_Thresholds);
                State.Parameter = _DefaultParameter;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Transition 2D
        /************************************************************************************************************************/

        /// <summary>
        /// A serializable <see cref="ITransition"/> which can create a <see cref="CartesianMixerState"/> or
        /// <see cref="DirectionalMixerState"/> when passed into
        /// <see cref="AnimancerPlayable.Play(ITransition)"/>.
        /// </summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Transition2D
        /// 
        [Serializable]
        public class Transition2D : Transition<MixerState<Vector2>, Vector2>
        {
            /************************************************************************************************************************/

            /// <summary>
            /// A type of <see cref="MixerState"/> that can be created by a <see cref="Transition2D"/>.
            /// </summary>
            public enum MixerType
            {
                /// <summary><see cref="CartesianMixerState"/></summary>
                Cartesian,

                /// <summary><see cref="DirectionalMixerState"/></summary>
                Directional,
            }

            [SerializeField]
            private MixerType _Type;

            /// <summary>[<see cref="SerializeField"/>]
            /// The type of <see cref="MixerState"/> that this transition will create.
            /// </summary>
            public ref MixerType Type => ref _Type;

            /************************************************************************************************************************/

            /// <summary>
            /// Creates and returns a new <see cref="CartesianMixerState"/> or <see cref="DirectionalMixerState"/>
            /// depending on the <see cref="Type"/>.
            /// </summary>
            /// <remarks>
            /// Note that using methods like <see cref="AnimancerPlayable.Play(ITransition)"/> will also call
            /// <see cref="ITransition.Apply"/>, so if you call this method manually you may want to call that method
            /// as well. Or you can just use <see cref="AnimancerUtilities.CreateStateAndApply"/>.
            /// <para></para>
            /// This method also assigns it as the <see cref="AnimancerState.Transition{TState}.State"/>.
            /// </remarks>
            public override MixerState<Vector2> CreateState()
            {
                switch (_Type)
                {
                    case MixerType.Cartesian: State = new CartesianMixerState(); break;
                    case MixerType.Directional: State = new DirectionalMixerState(); break;
                    default: throw new ArgumentOutOfRangeException(nameof(_Type));
                }
                InitialiseState();
                return State;
            }

            /************************************************************************************************************************/
            #region Drawer
#if UNITY_EDITOR
            /************************************************************************************************************************/

            /// <summary>[Editor-Only]
            /// Draws the Inspector GUI for a <see cref="Vector2"/> <see cref="Transition{TMixer, TParameter}"/>.
            /// </summary>
            /// <remarks>
            /// Unfortunately the tool used to generate this documentation does not currently support nested types with
            /// identical names, so only one <c>Drawer</c> class will actually have a documentation page.
            /// <para></para>
            /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
            /// <para></para>
            /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
            /// </remarks>
            [CustomPropertyDrawer(typeof(Transition2D), true)]
            public class Drawer : TransitionDrawer
            {
                /************************************************************************************************************************/

                /// <summary>
                /// Creates a new <see cref="Drawer"/> using the a wider `thresholdWidth` than usual to accomodate
                /// both the X and Y values.
                /// </summary>
                public Drawer() : base(StandardThresholdWidth * 2 + 20) { }

                /************************************************************************************************************************/
                #region Threshold Calculation Functions
                /************************************************************************************************************************/

                /// <inheritdoc/>
                protected override void AddThresholdFunctionsToMenu(GenericMenu menu)
                {
                    AddCalculateThresholdsFunction(menu, "From Velocity/XY", (state, threshold) =>
                    {
                        if (AnimancerUtilities.TryGetAverageVelocity(state, out var velocity))
                            return new Vector2(velocity.x, velocity.y);
                        else
                            return new Vector2(float.NaN, float.NaN);
                    });

                    AddCalculateThresholdsFunction(menu, "From Velocity/XZ", (state, threshold) =>
                    {
                        if (AnimancerUtilities.TryGetAverageVelocity(state, out var velocity))
                            return new Vector2(velocity.x, velocity.z);
                        else
                            return new Vector2(float.NaN, float.NaN);
                    });

                    AddCalculateThresholdsFunctionPerAxis(menu, "From Speed",
                        (state, threshold) => AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) ? velocity.magnitude : float.NaN);
                    AddCalculateThresholdsFunctionPerAxis(menu, "From Velocity X",
                        (state, threshold) => AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) ? velocity.x : float.NaN);
                    AddCalculateThresholdsFunctionPerAxis(menu, "From Velocity Y",
                        (state, threshold) => AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) ? velocity.y : float.NaN);
                    AddCalculateThresholdsFunctionPerAxis(menu, "From Velocity Z",
                        (state, threshold) => AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) ? velocity.z : float.NaN);
                    AddCalculateThresholdsFunctionPerAxis(menu, "From Angular Speed (Rad)",
                        (state, threshold) => AnimancerUtilities.TryGetAverageAngularSpeed(state, out var speed) ? speed : float.NaN);
                    AddCalculateThresholdsFunctionPerAxis(menu, "From Angular Speed (Deg)",
                        (state, threshold) => AnimancerUtilities.TryGetAverageAngularSpeed(state, out var speed) ? speed * Mathf.Rad2Deg : float.NaN);

                    AddPropertyModifierFunction(menu, "Initialise 4 Directions", Initialise4Directions);
                    AddPropertyModifierFunction(menu, "Initialise 8 Directions", Initialise8Directions);
                }

                /************************************************************************************************************************/

                private void Initialise4Directions(SerializedProperty property)
                {
                    var oldSpeedCount = CurrentSpeeds.arraySize;

                    CurrentStates.arraySize = CurrentThresholds.arraySize = CurrentSpeeds.arraySize = 5;
                    CurrentThresholds.GetArrayElementAtIndex(0).vector2Value = Vector2.zero;
                    CurrentThresholds.GetArrayElementAtIndex(1).vector2Value = Vector2.up;
                    CurrentThresholds.GetArrayElementAtIndex(2).vector2Value = Vector2.right;
                    CurrentThresholds.GetArrayElementAtIndex(3).vector2Value = Vector2.down;
                    CurrentThresholds.GetArrayElementAtIndex(4).vector2Value = Vector2.left;

                    InitialiseSpeeds(oldSpeedCount);

                    var type = property.FindPropertyRelative(nameof(_Type));
                    type.enumValueIndex = (int)MixerType.Directional;
                }

                /************************************************************************************************************************/

                private void Initialise8Directions(SerializedProperty property)
                {
                    var oldSpeedCount = CurrentSpeeds.arraySize;

                    CurrentStates.arraySize = CurrentThresholds.arraySize = CurrentSpeeds.arraySize = 9;
                    CurrentThresholds.GetArrayElementAtIndex(0).vector2Value = Vector2.zero;
                    CurrentThresholds.GetArrayElementAtIndex(1).vector2Value = Vector2.up;
                    CurrentThresholds.GetArrayElementAtIndex(2).vector2Value = new Vector2(1, 1);
                    CurrentThresholds.GetArrayElementAtIndex(3).vector2Value = Vector2.right;
                    CurrentThresholds.GetArrayElementAtIndex(4).vector2Value = new Vector2(1, -1);
                    CurrentThresholds.GetArrayElementAtIndex(5).vector2Value = Vector2.down;
                    CurrentThresholds.GetArrayElementAtIndex(6).vector2Value = new Vector2(-1, -1);
                    CurrentThresholds.GetArrayElementAtIndex(7).vector2Value = Vector2.left;
                    CurrentThresholds.GetArrayElementAtIndex(8).vector2Value = new Vector2(-1, 1);

                    InitialiseSpeeds(oldSpeedCount);

                    var type = property.FindPropertyRelative(nameof(_Type));
                    type.enumValueIndex = (int)MixerType.Directional;
                }

                /************************************************************************************************************************/

                private void AddCalculateThresholdsFunction(GenericMenu menu, string label,
                    Func<Object, Vector2, Vector2> calculateThreshold)
                {
                    AddPropertyModifierFunction(menu, label, (property) =>
                    {
                        GatherSubProperties(property);
                        var count = CurrentStates.arraySize;
                        for (int i = 0; i < count; i++)
                        {
                            var state = CurrentStates.GetArrayElementAtIndex(i).objectReferenceValue;
                            if (state == null)
                                continue;

                            var threshold = CurrentThresholds.GetArrayElementAtIndex(i);
                            var value = calculateThreshold(state, threshold.vector2Value);
                            if (!Editor.AnimancerEditorUtilities.IsNaN(value))
                                threshold.vector2Value = value;
                        }
                    });
                }

                /************************************************************************************************************************/

                private void AddCalculateThresholdsFunctionPerAxis(GenericMenu menu, string label,
                    Func<Object, float, float> calculateThreshold)
                {
                    AddCalculateThresholdsFunction(menu, "X/" + label, 0, calculateThreshold);
                    AddCalculateThresholdsFunction(menu, "Y/" + label, 1, calculateThreshold);
                }

                private void AddCalculateThresholdsFunction(GenericMenu menu, string label, int axis,
                    Func<Object, float, float> calculateThreshold)
                {
                    AddPropertyModifierFunction(menu, label, (property) =>
                    {
                        var count = CurrentStates.arraySize;
                        for (int i = 0; i < count; i++)
                        {
                            var state = CurrentStates.GetArrayElementAtIndex(i).objectReferenceValue;
                            if (state == null)
                                continue;

                            var threshold = CurrentThresholds.GetArrayElementAtIndex(i);

                            var value = threshold.vector2Value;
                            var newValue = calculateThreshold(state, value[axis]);
                            if (!float.IsNaN(newValue))
                                value[axis] = newValue;
                            threshold.vector2Value = value;
                        }
                    });
                }

                /************************************************************************************************************************/
                #endregion
                /************************************************************************************************************************/
            }

            /************************************************************************************************************************/
#endif
            #endregion
            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Transition Drawer
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Draws the Inspector GUI for a <see cref="Transition{TMixer, TParameter}"/>.</summary>
        /// <remarks>
        /// This class would be nested inside <see cref="Transition{TMixer, TParameter}"/>, but the generic parameters
        /// cause problems in Unity 2019.3.
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/TransitionDrawer
        /// 
        public class TransitionDrawer : ManualMixerState.Transition.Drawer
        {
            /************************************************************************************************************************/

            /// <summary>The number of horizontal pixels the "Threshold" label occupies.</summary>
            private readonly float ThresholdWidth;

            /************************************************************************************************************************/

            private static float _StandardThresholdWidth;

            /// <summary>
            /// The number of horizontal pixels the word "Threshold" occupies when drawn with the
            /// <see cref="EditorStyles.popup"/> style.
            /// </summary>
            protected static float StandardThresholdWidth
            {
                get
                {
                    if (_StandardThresholdWidth == 0)
                        _StandardThresholdWidth = Editor.AnimancerGUI.CalculateWidth(EditorStyles.popup, "Threshold");
                    return _StandardThresholdWidth;
                }
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="TransitionDrawer"/> using the default <see cref="StandardThresholdWidth"/>.
            /// </summary>
            public TransitionDrawer() : this(StandardThresholdWidth) { }

            /// <summary>
            /// Creates a new <see cref="TransitionDrawer"/> using a custom width for its threshold labels.
            /// </summary>
            protected TransitionDrawer(float thresholdWidth) => ThresholdWidth = thresholdWidth;

            /************************************************************************************************************************/

            /// <summary>
            /// The serialized <see cref="Transition{TMixer, TParameter}.Thresholds"/> of the
            /// <see cref="ManualMixerState.Transition.Drawer.CurrentProperty"/>.
            /// </summary>
            protected static SerializedProperty CurrentThresholds { get; private set; }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void GatherSubProperties(SerializedProperty property)
            {
                base.GatherSubProperties(property);

                CurrentThresholds = property.FindPropertyRelative(Transition2D.ThresholdsField);

                var count = Math.Max(CurrentStates.arraySize, CurrentThresholds.arraySize);
                CurrentStates.arraySize = count;
                CurrentThresholds.arraySize = count;
                if (CurrentSpeeds.arraySize != 0)
                    CurrentSpeeds.arraySize = count;
            }

            /************************************************************************************************************************/

            /// <summary>Splits the specified `area` into separate sections.</summary>
            protected void SplitListRect(Rect area, bool isHeader, out Rect animation, out Rect threshold, out Rect speed, out Rect sync)
            {
                SplitListRect(area, isHeader, out animation, out speed, out sync);

                threshold = animation;

                var xMin = threshold.xMin = Math.Max(
                    EditorGUIUtility.labelWidth + Editor.AnimancerGUI.IndentSize,
                    threshold.xMax - ThresholdWidth);

                animation.xMax = xMin - Editor.AnimancerGUI.StandardSpacing;
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void DoStateListHeaderGUI(Rect area)
            {
                SplitListRect(area, true, out var animationArea, out var thresholdArea, out var speedArea, out var syncArea);

                DoAnimationHeaderGUI(animationArea);

                var content = Editor.AnimancerGUI.TempContent("Threshold",
                    "The parameter values at which each child state will be fully active");
                DoHeaderDropdownGUI(thresholdArea, CurrentThresholds, content, AddThresholdFunctionsToMenu);

                DoSpeedHeaderGUI(speedArea);

                DoSyncHeaderGUI(syncArea);
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void DoElementGUI(Rect area, int index,
                SerializedProperty clip, SerializedProperty speed)
            {
                SplitListRect(area, false, out var animationArea, out var thresholdArea, out var speedArea, out var syncArea);

                DoElementGUI(animationArea, speedArea, syncArea, index, clip, speed);

                DoThresholdGUI(thresholdArea, index);
            }

            /************************************************************************************************************************/

            /// <summary>Draws the GUI of the threshold at the specified `index`.</summary>
            protected virtual void DoThresholdGUI(Rect area, int index)
            {
                var threshold = CurrentThresholds.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(area, threshold, GUIContent.none);
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void OnAddElement(ReorderableList list)
            {
                var index = CurrentStates.arraySize;
                base.OnAddElement(list);
                CurrentThresholds.InsertArrayElementAtIndex(index);
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void OnRemoveElement(ReorderableList list)
            {
                base.OnRemoveElement(list);
                Editor.Serialization.RemoveArrayElement(CurrentThresholds, list.index);
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void OnReorderList(ReorderableList list, int oldIndex, int newIndex)
            {
                base.OnReorderList(list, oldIndex, newIndex);
                CurrentThresholds.MoveArrayElement(oldIndex, newIndex);
            }

            /************************************************************************************************************************/

            /// <summary>Adds functions to the `menu` relating to the thresholds.</summary>
            protected virtual void AddThresholdFunctionsToMenu(GenericMenu menu) { }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
    }
}

