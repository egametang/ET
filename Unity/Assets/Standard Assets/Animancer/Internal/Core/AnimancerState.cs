// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
using Animancer.Editor;
#endif

namespace Animancer
{
    /// <summary>
    /// Base class for all states in an <see cref="AnimancerPlayable"/> graph which manages one or more
    /// <see cref="Playable"/>s.
    /// </summary>
    /// 
    /// <remarks>
    /// This class can be used as a custom yield instruction to wait until the animation either stops playing or
    /// reaches its end.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/states">States</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerState
    /// 
    public abstract partial class AnimancerState : AnimancerNode, IAnimationClipCollection
    {
        /************************************************************************************************************************/
        #region Graph
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerPlayable"/> at the root of the graph.</summary>
        public void SetRoot(AnimancerPlayable root)
        {
            if (Root == root)
                return;

            if (Root != null)
            {
                Root.CancelUpdate(this);
                Root.States.Unregister(this);

                if (_Parent != null)
                {
                    _Parent.OnRemoveChild(this);
                    _Parent = null;
                }

                Index = -1;

                DestroyPlayable();
            }

            Root = root;

            if (root != null)
            {
                root.States.Register(_Key, this);
                CreatePlayable();
            }

            for (int i = ChildCount - 1; i >= 0; i--)
                GetChild(i)?.SetRoot(root);

            if (_Parent != null)
                CopyIKFlags(_Parent);
        }

        /************************************************************************************************************************/

        /// <summary>The object which receives the output of the <see cref="Playable"/>.</summary>
        public sealed override IPlayableWrapper Parent => _Parent;
        private AnimancerNode _Parent;

        /// <summary>Connects this state to the `parent` mixer at the specified `index`.</summary>
        /// <remarks>
        /// Use <see cref="AnimancerLayer.AddChild(AnimancerState)"/> instead of this method to connect a state to an
        /// available port on a layer.
        /// </remarks>
        public void SetParent(AnimancerNode parent, int index)
        {
            if (_Parent != null)
            {
                _Parent.OnRemoveChild(this);
                _Parent = null;
            }

            if (parent == null)
            {
                Index = -1;
                return;
            }

            SetRoot(parent.Root);
            Index = index;
            _Parent = parent;
            parent.OnAddChild(this);
            CopyIKFlags(parent);
        }

        /// <summary>[Internal]
        /// Called by <see cref="AnimancerNode.OnAddChild(IList{AnimancerState}, AnimancerState)"/> if the specified
        /// port is already occupied so it can be cleared without triggering any other calls.
        /// </summary>
        internal void ClearParent()
        {
            Index = -1;
            _Parent = null;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The <see cref="AnimancerNode.Weight"/> of this state multiplied by the <see cref="AnimancerNode.Weight"/> of each of
        /// its parents down the hierarchy to determine how much this state affects the final output.
        /// </summary>
        /// <exception cref="NullReferenceException">This state has no <see cref="AnimancerNode.Parent"/>.</exception>
        public float EffectiveWeight
        {
            get
            {
                var weight = Weight;

                var parent = _Parent;
                while (parent != null)
                {
                    weight *= parent.Weight;
                    parent = parent.Parent as AnimancerNode;
                }

                return weight;
            }
        }

        /************************************************************************************************************************/
        // Layer.
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override AnimancerLayer Layer => _Parent?.Layer;

        /// <summary>
        /// The index of the <see cref="AnimancerLayer"/> this state is connected to (determined by the
        /// <see cref="Parent"/>).
        /// </summary>
        public int LayerIndex
        {
            get
            {
                if (_Parent == null)
                    return -1;

                var layer = _Parent.Layer;
                if (layer == null)
                    return -1;

                return layer.Index;
            }
            set
            {
                Root.Layers[value].AddChild(this);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Key and Clip
        /************************************************************************************************************************/

        internal object _Key;

        /// <summary>
        /// The object used to identify this state in the root <see cref="AnimancerPlayable.States"/> dictionary.
        /// Can be null.
        /// </summary>
        public object Key
        {
            get => _Key;
            set
            {
                Root.States.Unregister(this);
                Root.States.Register(value, this);
            }
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimationClip"/> which this state plays (if any).</summary>
        /// <exception cref="NotSupportedException">This state type doesn't have a clip and you try to set it.</exception>
        public virtual AnimationClip Clip
        {
            get => null;
            set => throw new NotSupportedException($"{GetType()} does not support setting the {nameof(Clip)}.");
        }

        /// <summary>The main object to show in the Inspector for this state (if any).</summary>
        /// <exception cref="NotSupportedException">This state type doesn't have a main object and you try to set it.</exception>
        /// <exception cref="InvalidCastException">This state can't use the assigned value.</exception>
        public virtual Object MainObject
        {
            get => null;
            set => throw new NotSupportedException($"{GetType()} does not support setting the {nameof(MainObject)}.");
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Sets the `currentObject` and calls <see cref="AnimancerNode.RecreatePlayable"/>. If the `currentObject` was
        /// being used as the <see cref="Key"/> then it is changed as well.
        /// </summary>
        /// <exception cref="ArgumentNullException">The `newObject` is null.</exception>
        protected void ChangeMainObject<T>(ref T currentObject, T newObject) where T : Object
        {
            if (newObject == null)
                throw new ArgumentNullException(nameof(newObject));

            if (ReferenceEquals(currentObject, newObject))
                return;

            if (ReferenceEquals(_Key, currentObject))
                Key = newObject;

            currentObject = newObject;
            RecreatePlayable();
        }

        /************************************************************************************************************************/

        /// <summary>The average velocity of the root motion caused by this state.</summary>
        public virtual Vector3 AverageVelocity => default;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Playing
        /************************************************************************************************************************/

        /// <summary>Is the <see cref="Time"/> automatically advancing?</summary>
        private bool _IsPlaying;

        /// <summary>Has <see cref="_IsPlaying"/> changed since it was last applied to the <see cref="Playable"/>.</summary>
        /// <remarks>
        /// Playables start playing by default so we start dirty to pause it during the first update (unless
        /// <see cref="IsPlaying"/> is set to true before that).
        /// </remarks>
        private bool _IsPlayingDirty = true;

        /************************************************************************************************************************/

        /// <summary>Is the <see cref="Time"/> automatically advancing?</summary>
        ///
        /// <example><code>
        /// void IsPlayingExample(AnimancerComponent animancer, AnimationClip clip)
        /// {
        ///     var state = animancer.States.GetOrCreate(clip);
        ///
        ///     if (state.IsPlaying)
        ///         Debug.Log(clip + " is playing");
        ///     else
        ///         Debug.Log(clip + " is paused");
        ///
        ///     state.IsPlaying = false;// Pause the animation.
        ///
        ///     state.IsPlaying = true;// Unpause the animation.
        /// }
        /// </code></example>
        public bool IsPlaying
        {
            get => _IsPlaying;
            set
            {
                if (_IsPlaying == value)
                    return;

                _IsPlaying = value;

                // If it was already dirty then we just returned to the previous state so it is no longer dirty.
                if (_IsPlayingDirty)
                {
                    _IsPlayingDirty = false;
                    // We may still need to be updated for other reasons (such as Weight),
                    // but if not then we will be removed from the update list next update.
                }
                else// Otherwise we are now dirty so we need to be updated.
                {
                    _IsPlayingDirty = true;
                    RequireUpdate();
                }

                OnSetIsPlaying();
            }
        }

        /// <summary>Called when the value of <see cref="IsPlaying"/> is changed.</summary>
        protected virtual void OnSetIsPlaying() { }

        /// <summary>Creates and assigns the <see cref="Playable"/> managed by this state.</summary>
        /// <remarks>This method also applies the <see cref="AnimancerNode.Speed"/> and <see cref="IsPlaying"/>.</remarks>
        public sealed override void CreatePlayable()
        {
            base.CreatePlayable();

            if (!_IsPlaying)
                _Playable.Pause();
            _IsPlayingDirty = false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns true if this state is playing and is at or fading towards a non-zero
        /// <see cref="AnimancerNode.Weight"/>.
        /// </summary>
        public bool IsActive => _IsPlaying && TargetWeight > 0;

        /// <summary>
        /// Returns true if this state is not playing and is at 0 <see cref="AnimancerNode.Weight"/>.
        /// </summary>
        public bool IsStopped => !_IsPlaying && Weight == 0;

        /************************************************************************************************************************/

        /// <summary>Plays this state immediately, without any blending.</summary>
        /// <remarks>
        /// Sets <see cref="IsPlaying"/> = true, <see cref="AnimancerNode.Weight"/> = 1, and clears the
        /// <see cref="Events"/>.
        /// <para></para>
        /// This method does not change the <see cref="Time"/> so it will continue from its current value.
        /// </remarks>
        public void Play()
        {
            IsPlaying = true;
            Weight = 1;
            EventRunner.TryClear(_EventRunner);
        }

        /************************************************************************************************************************/

        /// <summary>Stops the animation and makes it inactive immediately so it no longer affects the output.</summary>
        /// <remarks>
        /// Sets <see cref="AnimancerNode.Weight"/> = 0, <see cref="IsPlaying"/> = false, <see cref="Time"/> = 0, and
        /// clears the <see cref="Events"/>.
        /// <para></para>
        /// To freeze the animation in place without ending it, you only need to set <see cref="IsPlaying"/> = false
        /// instead. Or to freeze all animations, you can call <see cref="AnimancerPlayable.PauseGraph"/>.
        /// </remarks>
        public override void Stop()
        {
            base.Stop();

            IsPlaying = false;
            Time = 0;
            EventRunner.TryClear(_EventRunner);
        }

        /************************************************************************************************************************/

        /// <summary>Called by <see cref="AnimancerNode.StartFade"/>. Clears the <see cref="Events"/>.</summary>
        protected internal override void OnStartFade()
        {
            EventRunner.TryClear(_EventRunner);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Timing
        /************************************************************************************************************************/
        // Time.
        /************************************************************************************************************************/

        /// <summary>
        /// The current time of the <see cref="Playable"/>, retrieved by <see cref="Time"/> whenever the
        /// <see cref="_TimeFrameID"/> is different from the <see cref="AnimancerPlayable.FrameID"/>.
        /// </summary>
        private float _Time;

        /// <summary>
        /// Indicates whether the <see cref="_Time"/> needs to be assigned to the <see cref="Playable"/> next update.
        /// </summary>
        /// <remarks>
        /// <see cref="EventRunner"/> executes after all other playables, at which point changes can still be made to
        /// their time but not their weight which means that if we set the time immediately then it can be out of sync
        /// with the weight. For example, if an animation ends and you play another, the first animation would be
        /// stopped and rewinded to the start but would still be at full weight so it would show its first frame before
        /// the new animation actually takes effect (even if the previous animation was not looping).
        /// <para></para>
        /// So instead, we simply delay setting the actual playable time until the next update so that time and weight
        /// are always in sync.
        /// </remarks>
        private bool _MustSetTime;

        /// <summary>
        /// The <see cref="AnimancerPlayable.FrameID"/> from when the <see cref="Time"/> was last retrieved from the
        /// <see cref="Playable"/>.
        /// </summary>
        private ulong _TimeFrameID;

        /************************************************************************************************************************/

        /// <summary>The number of seconds that have passed since the start of this animation.</summary>
        ///
        /// <remarks>
        /// This value will continue increasing after the animation passes the end of its <see cref="Length"/> while
        /// the animated object either freezes in place or starts again from the beginning according to whether it is
        /// looping or not.
        /// <para></para>
        /// This property internally uses <see cref="RawTime"/> whenever the value is out of date or gets changed.
        /// <para></para>
        /// <em>Animancer Lite does not allow this value to be changed in runtime builds (except resetting it to 0).</em>
        /// </remarks>
        ///
        /// <example><code>
        /// void PlayAnimation(AnimancerComponent animancer, AnimationClip clip)
        /// {
        ///     var state = animancer.Play(clip);
        ///
        ///     // Skip 0.5 seconds into the animation:
        ///     state.Time = 0.5f;
        ///
        ///     // Skip 50% of the way through the animation (0.5 in a range of 0 to 1):
        ///     state.NormalizedTime = 0.5f;
        ///
        ///     // Skip to the end of the animation and play backwards.
        ///     state.NormalizedTime = 1;
        ///     state.Speed = -1;
        /// }
        /// </code></example>
        public float Time
        {
            get
            {
                var root = Root;
                if (root == null || _MustSetTime)
                    return _Time;

                var frameID = root.FrameID;
                if (_TimeFrameID != frameID)
                {
                    _TimeFrameID = frameID;
                    _Time = RawTime;
                }

                return _Time;
            }
            set
            {
#if UNITY_ASSERTIONS
                if (!value.IsFinite())
                    throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(Time)} must be finite");
#endif

                var root = Root;
                if (root != null)
                    _TimeFrameID = root.FrameID;

                _Time = value;

                if (AnimancerPlayable.IsRunningLateUpdate(root))
                {
                    _MustSetTime = true;
                    RequireUpdate();
                }
                else
                {
                    RawTime = value;
                }

                _EventRunner?.OnTimeChanged();
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The internal implementation of <see cref="Time"/> which directly gets and sets the underlying value.
        /// </summary>
        /// <remarks>
        /// Setting this value actually calls <see cref="PlayableExtensions.SetTime"/> twice to ensure that animation
        /// events aren't triggered incorrectly. Calling it only once would trigger any animation events between the
        /// previous time and the new time. So if an animation plays to the end and you set the time back to 0 (such as
        /// by calling <see cref="Stop"/> or playing a different animation), the next time that animation played it
        /// would immediately trigger all of its events, then play through and trigger them normally as well.
        /// </remarks>
        protected virtual float RawTime
        {
            get
            {
                Validate.AssertPlayable(this);
                return (float)_Playable.GetTime();
            }
            set
            {
                Validate.AssertPlayable(this);
                var time = (double)value;
                _Playable.SetTime(time);
                _Playable.SetTime(time);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The <see cref="Time"/> of this state as a portion of the animation's <see cref="Length"/>, meaning the
        /// value goes from 0 to 1 as it plays from start to end, regardless of how long that actually takes.
        /// </summary>
        /// 
        /// <remarks>
        /// This value will continue increasing after the animation passes the end of its <see cref="Length"/> while
        /// the animated object either freezes in place or starts again from the beginning according to whether it is
        /// looping or not.
        /// <para></para>
        /// The fractional part of the value (<c>NormalizedTime % 1</c>) is the percentage (0-1) of progress in the
        /// current loop while the integer part (<c>(int)NormalizedTime</c>) is the number of times the animation has
        /// been looped.
        /// <para></para>
        /// <em>Animancer Lite does not allow this value to be changed to a value other than 0 in runtime builds.</em>
        /// </remarks>
        ///
        /// <example><code>
        /// void PlayAnimation(AnimancerComponent animancer, AnimationClip clip)
        /// {
        ///     var state = animancer.Play(clip);
        ///
        ///     // Skip 0.5 seconds into the animation:
        ///     state.Time = 0.5f;
        ///
        ///     // Skip 50% of the way through the animation (0.5 in a range of 0 to 1):
        ///     state.NormalizedTime = 0.5f;
        ///
        ///     // Skip to the end of the animation and play backwards.
        ///     state.NormalizedTime = 1;
        ///     state.Speed = -1;
        /// }
        /// </code></example>
        public float NormalizedTime
        {
            get
            {
                var length = Length;
                if (length != 0)
                    return Time / Length;
                else
                    return 0;
            }
            set => Time = value * Length;
        }

        /************************************************************************************************************************/

        /// <summary>Prevents the <see cref="RawTime"/> from being applied.</summary>
        protected void CancelSetTime() => _MustSetTime = false;

        /************************************************************************************************************************/
        // Duration.
        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// The <see cref="NormalizedTime"/> after which the <see cref="AnimancerEvent.Sequence.OnEnd"/> callback will
        /// be invoked every frame.
        /// </summary>
        /// <remarks>
        /// This is a wrapper around <see cref="AnimancerEvent.Sequence.NormalizedEndTime"/> so that if the value has
        /// not been set (<see cref="float.NaN"/>) it can be determined based on the
        /// <see cref="AnimancerNode.EffectiveSpeed"/>: positive speed ends at 1 and negative speed ends at 0.
        /// <para></para>
        /// <em>Animancer Lite does not allow this value to be changed in runtime builds.</em>
        /// </remarks>
        public float NormalizedEndTime
        {
            get
            {
                if (_EventRunner != null)
                {
                    var time = _EventRunner.Events.NormalizedEndTime;
                    if (!float.IsNaN(time))
                        return time;
                }

                return AnimancerEvent.Sequence.GetDefaultNormalizedEndTime(EffectiveSpeed);
            }
            set => Events.NormalizedEndTime = value;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The number of seconds the animation will take to play fully at its current
        /// <see cref="AnimancerNode.EffectiveSpeed"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// For the time remaining from now until it reaches the end, use <see cref="RemainingDuration"/> instead.
        /// <para></para>
        /// Setting this value modifies the <see cref="AnimancerNode.EffectiveSpeed"/>, not the <see cref="Length"/>.
        /// <para></para>
        /// <em>Animancer Lite does not allow this value to be changed in runtime builds.</em>
        /// </remarks>
        ///
        /// <example><code>
        /// void PlayAnimation(AnimancerComponent animancer, AnimationClip clip)
        /// {
        ///     var state = animancer.Play(clip);
        ///
        ///     state.Duration = 1;// Play fully in 1 second.
        ///     state.Duration = 2;// Play fully in 2 seconds.
        ///     state.Duration = 0.5f;// Play fully in half a second.
        ///     state.Duration = -1;// Play backwards fully in 1 second.
        ///     state.NormalizedTime = 1; state.Duration = -1;// Play backwards from the end in 1 second.
        /// }
        /// </code></example>
        public float Duration
        {
            get
            {
                var speed = EffectiveSpeed;
                if (_EventRunner != null)
                {
                    var endTime = _EventRunner.Events.NormalizedEndTime;
                    if (!float.IsNaN(endTime))
                    {
                        if (speed > 0)
                            return Length * endTime / speed;
                        else
                            return Length * (1 - endTime) / -speed;
                    }
                }

                return Length / Math.Abs(speed);
            }
            set
            {
                var length = Length;
                if (_EventRunner != null)
                {
                    var endTime = _EventRunner.Events.NormalizedEndTime;
                    if (!float.IsNaN(endTime))
                    {
                        if (EffectiveSpeed > 0)
                            length *= endTime;
                        else
                            length *= 1 - endTime;
                    }
                }

                EffectiveSpeed = length / value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The number of seconds this state will take to go from its current <see cref="NormalizedTime"/> to the
        /// <see cref="NormalizedEndTime"/> at its current <see cref="AnimancerNode.EffectiveSpeed"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// For the time it would take to play fully from the start, use the <see cref="Duration"/> instead.
        /// <para></para>
        /// Setting this value modifies the <see cref="AnimancerNode.EffectiveSpeed"/>, not the <see cref="Length"/>.
        /// <para></para>
        /// <em>Animancer Lite does not allow this value to be changed in runtime builds.</em>
        /// </remarks>
        ///
        /// <example><code>
        /// void PlayAnimation(AnimancerComponent animancer, AnimationClip clip)
        /// {
        ///     var state = animancer.Play(clip);
        ///
        ///     state.RemainingDuration = 1;// Play from the current time to the end in 1 second.
        ///     state.RemainingDuration = 2;// Play from the current time to the end in 2 seconds.
        ///     state.RemainingDuration = 0.5f;// Play from the current time to the end in half a second.
        ///     state.RemainingDuration = -1;// Play from the current time away from the end.
        /// }
        /// </code></example>
        public float RemainingDuration
        {
            get => (Length * NormalizedEndTime - Time) / EffectiveSpeed;
            set => EffectiveSpeed = (Length * NormalizedEndTime - Time) / value;
        }

        /************************************************************************************************************************/
        // Length.
        /************************************************************************************************************************/

        /// <summary>The total time this state would take to play in seconds when <see cref="AnimancerNode.Speed"/> = 1.</summary>
        public abstract float Length { get; }

        /// <summary>Will this state loop back to the start when it reaches the end?</summary>
        public virtual bool IsLooping => false;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Methods
        /************************************************************************************************************************/

        /// <summary>
        /// Updates the <see cref="AnimancerNode.Weight"/> for fading, applies it to this state's port on the parent
        /// mixer, and plays or pauses the <see cref="Playable"/> if its state is dirty.
        /// </summary>
        /// <remarks>
        /// If the <see cref="Parent"/>'s <see cref="AnimancerNode.KeepChildrenConnected"/> is set to false, this
        /// method will also connect/disconnect this node from the <see cref="Parent"/> in the playable graph.
        /// </remarks>
        protected internal override void Update(out bool needsMoreUpdates)
        {
            base.Update(out needsMoreUpdates);

            if (_IsPlayingDirty)
            {
                _IsPlayingDirty = false;

                if (_IsPlaying)
                    _Playable.Play();
                else
                    _Playable.Pause();
            }

            if (_MustSetTime)
            {
                _MustSetTime = false;
                RawTime = _Time;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Destroys the <see cref="Playable"/> and cleans up this state.</summary>
        /// <remarks>
        /// This method is NOT called automatically, so when implementing a custom state type you must use
        /// <see cref="AnimancerPlayable.Disposables"/> if you need to guarantee that things will get cleaned up.
        /// </remarks>
        public virtual void Destroy()
        {
            if (_Parent != null)
            {
                _Parent.OnRemoveChild(this);
                _Parent = null;
            }

            Index = -1;
            EventRunner.TryClear(_EventRunner);

            var root = Root;
            if (root != null)
            {
                root.States.Unregister(this);

                // For some reason this is slightly faster than _Playable.Destroy().
                if (_Playable.IsValid())
                    root._Graph.DestroyPlayable(_Playable);
            }
        }

        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipCollection"/>] Gathers all the animations in this state.</summary>
        public virtual void GatherAnimationClips(ICollection<AnimationClip> clips) => clips.Gather(Clip);

        /************************************************************************************************************************/

        /// <summary>
        /// Returns true if the animation is playing and has not yet passed the
        /// <see cref="AnimancerEvent.Sequence.endEvent"/>.
        /// </summary>
        /// <remarks>
        /// This method is called by <see cref="IEnumerator.MoveNext"/> so this object can be used as a custom yield
        /// instruction to wait until it finishes.
        /// </remarks>
        protected internal override bool IsPlayingAndNotEnding()
        {
            if (!IsPlaying)
                return false;

            var speed = EffectiveSpeed;
            if (speed > 0)
            {
                float endTime;
                if (_EventRunner != null)
                {
                    endTime = _EventRunner.Events.endEvent.normalizedTime;
                    if (float.IsNaN(endTime))
                        endTime = Length;
                    else
                        endTime *= Length;
                }
                else endTime = Length;

                return Time <= endTime;
            }
            else if (speed < 0)
            {
                float endTime;
                if (_EventRunner != null)
                {
                    endTime = _EventRunner.Events.endEvent.normalizedTime;
                    if (float.IsNaN(endTime))
                        endTime = 0;
                    else
                        endTime *= Length;
                }
                else endTime = 0;

                return Time >= endTime;
            }
            else return true;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the <see cref="AnimancerNode.DebugName"/> if one is set, otherwise a string describing the type of this
        /// state and the name of the <see cref="MainObject"/>.
        /// </summary>
        public override string ToString()
        {
#if UNITY_ASSERTIONS
            if (DebugName != null)
                return DebugName;
#endif

            var type = GetType().Name;
            var mainObject = MainObject;
            if (mainObject != null)
                return $"{mainObject.name} ({type})";
            else
                return type;
        }

        /************************************************************************************************************************/
        #region Descriptions
        /************************************************************************************************************************/

#if UNITY_EDITOR

        /// <summary>[Editor-Only] Returns a custom drawer for this state.</summary>
        protected internal virtual IAnimancerNodeDrawer CreateDrawer()
            => new AnimancerStateDrawer<AnimancerState>(this);
#endif

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void AppendDetails(StringBuilder text, string delimiter)
        {
            text.Append(delimiter).Append($"{nameof(Key)}: ").Append(AnimancerUtilities.ToStringOrNull(_Key));

            var mainObject = MainObject;
            if (mainObject != _Key as Object)
                text.Append(delimiter).Append($"{nameof(MainObject)}: ").Append(AnimancerUtilities.ToStringOrNull(mainObject));

#if UNITY_EDITOR
            if (mainObject != null)
                text.Append(delimiter).Append("AssetPath: ").Append(AssetDatabase.GetAssetPath(mainObject));
#endif

            base.AppendDetails(text, delimiter);

            text.Append(delimiter).Append($"{nameof(IsPlaying)}: ").Append(IsPlaying);

            try
            {
                var time = Time;
                var normalizedTime = NormalizedTime;
                var length = Length;
                var isLooping = IsLooping;
                text.Append(delimiter).Append($"{nameof(Time)} (Normalized): ").Append(time);
                text.Append(" (").Append(normalizedTime).Append(')');
                text.Append(delimiter).Append($"{nameof(Length)}: ").Append(length);
                text.Append(delimiter).Append($"{nameof(IsLooping)}: ").Append(isLooping);
            }
            catch { }// Ignore any exceptions.

            if (_EventRunner != null && _EventRunner.Events != null)
                _EventRunner.Events.endEvent.AppendDetails(text, "EndEvent", delimiter);
        }

        /************************************************************************************************************************/

        /// <summary>Returns the hierarchy path of this state through its <see cref="Parent"/>s.</summary>
        public string GetPath()
        {
            if (_Parent == null)
                return null;

            var path = ObjectPool.AcquireStringBuilder();

            AppendPath(path, _Parent);
            AppendPortAndType(path);

            return path.ReleaseToString();
        }

        /// <summary>Appends the hierarchy path of this state through its <see cref="Parent"/>s.</summary>
        private static void AppendPath(StringBuilder path, AnimancerNode parent)
        {
            var parentState = parent as AnimancerState;
            if (parentState != null && parentState._Parent != null)
            {
                AppendPath(path, parentState._Parent);
            }
            else
            {
                path.Append("Layers[")
                    .Append(parent.Layer.Index)
                    .Append("].States");
                return;
            }

            var state = parent as AnimancerState;
            if (state != null)
            {
                state.AppendPortAndType(path);
            }
            else
            {
                path.Append(" -> ")
                    .Append(parent.GetType());
            }
        }

        /// <summary>Appends "[Index] -> GetType().Name".</summary>
        private void AppendPortAndType(StringBuilder path)
        {
            path.Append('[')
                .Append(Index)
                .Append("] -> ")
                .Append(GetType().Name);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Transition
        /************************************************************************************************************************/

        /// <summary>
        /// Base class for serializable <see cref="ITransition"/>s which can create a particular type of
        /// <see cref="AnimancerState"/> when passed into <see cref="AnimancerPlayable.Play(ITransition)"/>.
        /// </summary>
        /// <remarks>
        /// Unfortunately the tool used to generate this documentation does not currently support nested types with
        /// identical names, so only one <c>Transition</c> class will actually have a documentation page.
        /// <para></para>
        /// Even though it has the <see cref="SerializableAttribute"/>, this class won't actually get serialized
        /// by Unity because it's generic and abstract. Each child class still needs to include the attribute.
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Transition_1
        /// 
        [Serializable]
        public abstract class Transition<TState> : ITransitionDetailed where TState : AnimancerState
        {
            /************************************************************************************************************************/

            [SerializeField, Tooltip(Strings.ProOnlyTag + "The amount of time the transition will take (in seconds)")]
            private float _FadeDuration = AnimancerPlayable.DefaultFadeDuration;

            /// <summary>[<see cref="SerializeField"/>] The amount of time the transition will take (in seconds).</summary>
            /// <exception cref="ArgumentOutOfRangeException">Thrown when setting the value to a negative number.</exception>
            public float FadeDuration
            {
                get => _FadeDuration;
                set
                {
                    if (value < 0)
                        throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(FadeDuration)} must not be negative");

                    _FadeDuration = value;
                }
            }

            /************************************************************************************************************************/

            /// <summary>[<see cref="ITransitionDetailed"/>]
            /// Indicates what the value of <see cref="AnimancerState.IsLooping"/> will be for the created state.
            /// Returns false unless overridden.
            /// </summary>
            public virtual bool IsLooping => false;

            /// <summary>[<see cref="ITransitionDetailed"/>]
            /// Determines what <see cref="NormalizedTime"/> to start the animation at.
            /// Returns <see cref="float.NaN"/> unless overridden.
            /// </summary>
            public virtual float NormalizedStartTime
            {
                get => float.NaN;
                set { }
            }

            /// <summary>[<see cref="ITransitionDetailed"/>]
            /// Determines how fast the animation plays (1x = normal speed).
            /// Returns 1 unless overridden.
            /// </summary>
            public virtual float Speed
            {
                get => 1;
                set { }
            }

            /// <summary>[<see cref="ITransitionDetailed"/>]
            /// The maximum amount of time the animation is expected to take (in seconds).
            /// </summary>
            /// <remarks>The actual duration can vary in states like <see cref="MixerState"/>.</remarks>
            public abstract float MaximumDuration { get; }

            /// <summary>[<see cref="ITransitionDetailed"/>]
            /// The <see cref="Motion.averageAngularSpeed"/> that the created state will have.
            /// </summary>
            /// <remarks>The actual average velocity can vary in states like <see cref="MixerState"/>.</remarks>
            public virtual float AverageAngularSpeed => 0;

            /// <summary>[<see cref="ITransitionDetailed"/>]
            /// The <see cref="Motion.averageSpeed"/> that the created state will have.
            /// </summary>
            /// <remarks>The actual average velocity can vary in states like <see cref="MixerState"/>.</remarks>
            public virtual Vector3 AverageVelocity => default;

            /************************************************************************************************************************/

            [SerializeField, Tooltip(Strings.ProOnlyTag + "Events which will be triggered as the animation plays")]
            private AnimancerEvent.Sequence.Serializable _Events;

            /// <summary>[<see cref="SerializeField"/>] [<see cref="ITransitionDetailed"/>]
            /// Events which will be triggered as the animation plays.
            /// </summary>
            /// <remarks>This property returns the <see cref="AnimancerEvent.Sequence.Serializable.Sequence"/>.</remarks>
            public AnimancerEvent.Sequence Events => _Events.Sequence;

            /// <summary>[<see cref="SerializeField"/>] [<see cref="ITransitionDetailed"/>]
            /// Events which will be triggered as the animation plays.
            /// </summary>
            public ref AnimancerEvent.Sequence.Serializable SerializedEvents => ref _Events;

            /************************************************************************************************************************/

            /// <summary>
            /// The state that was created by this object. Specifically, this is the state that was most recently
            /// passed into <see cref="Apply"/> (usually by <see cref="AnimancerPlayable.Play(ITransition)"/>).
            /// <para></para>
            /// You can use <see cref="AnimancerPlayable.StateDictionary.GetOrCreate(ITransition)"/> or
            /// <see cref="AnimancerLayer.GetOrCreateState(ITransition)"/> to get or create the state for a
            /// specific object.
            /// <para></para>
            /// <see cref="State"/> is simply a shorthand for casting this to <typeparamref name="TState"/>.
            /// </summary>
            public AnimancerState BaseState { get; private set; }

            /************************************************************************************************************************/

            private TState _State;

            /// <summary>
            /// The state that was created by this object. Specifically, this is the state that was most recently
            /// passed into <see cref="Apply"/> (usually by <see cref="AnimancerPlayable.Play(ITransition)"/>).
            /// </summary>
            /// 
            /// <remarks>
            /// You can use <see cref="AnimancerPlayable.StateDictionary.GetOrCreate(ITransition)"/> or
            /// <see cref="AnimancerLayer.GetOrCreateState(ITransition)"/> to get or create the state for a
            /// specific object.
            /// <para></para>
            /// This property is shorthand for casting the <see cref="BaseState"/> to <typeparamref name="TState"/>.
            /// </remarks>
            /// 
            /// <exception cref="InvalidCastException">
            /// The <see cref="BaseState"/> is not actually a <typeparamref name="TState"/>. This should only
            /// happen if a different type of state was created by something else and registered using the
            /// <see cref="Key"/>, causing this <see cref="AnimancerPlayable.Play(ITransition)"/> to pass that
            /// state into <see cref="Apply"/> instead of calling <see cref="CreateState"/> to make the correct type of
            /// state.
            /// </exception>
            public TState State
            {
                get
                {
                    if (_State == null)
                        _State = (TState)BaseState;

                    return _State;
                }
                protected set
                {
                    BaseState = _State = value;
                }
            }

            /************************************************************************************************************************/

            /// <summary>Indicates whether this transition can create a valid <see cref="AnimancerState"/>.</summary>
            public virtual bool IsValid => true;

            /************************************************************************************************************************/

            /// <summary>The <see cref="AnimancerState.Key"/> which the created state will be registered with.</summary>
            /// <remarks>Returns <c>this</c> unless overridden.</remarks>
            public virtual object Key => this;

            /// <summary>
            /// When a transition is passed into <see cref="AnimancerPlayable.Play(ITransition)"/>, this property
            /// determines which <see cref="Animancer.FadeMode"/> will be used.
            /// </summary>
            public virtual FadeMode FadeMode => FadeMode.FixedSpeed;

            /// <summary>Creates and returns a new <typeparamref name="TState"/>.</summary>
            /// <remarks>
            /// Note that using methods like <see cref="AnimancerPlayable.Play(ITransition)"/> will also call
            /// <see cref="Apply"/>, so if you call this method manually you may want to call that method as well. Or you
            /// can just use <see cref="AnimancerUtilities.CreateStateAndApply"/>.
            /// </remarks>
            public abstract TState CreateState();

            /// <summary>Creates and returns a new <typeparamref name="TState"/>.</summary>
            /// <remarks>
            /// Note that using methods like <see cref="AnimancerPlayable.Play(ITransition)"/> will also call
            /// <see cref="Apply"/>, so if you call this method manually you may want to call that method as well. Or you
            /// can just use <see cref="AnimancerUtilities.CreateStateAndApply"/>.
            /// </remarks>
            AnimancerState ITransition.CreateState() => CreateState();

            /************************************************************************************************************************/

            /// <summary>[<see cref="ITransition"/>]
            /// Sets the <see cref="BaseState"/> and applies any other modifications to the `state`.
            /// </summary>
            /// <remarks>
            /// Called by <see cref="AnimancerPlayable.Play(ITransition)"/>.
            /// <para></para>
            /// This method also clears the <see cref="State"/> if necessary, so it will re-cast the
            /// <see cref="BaseState"/> when it gets accessed again.
            /// </remarks>
            public virtual void Apply(AnimancerState state)
            {
                state.Events = _Events;

                BaseState = state;

                if (_State != state)
                    _State = null;
            }

            /************************************************************************************************************************/

            /// <summary>The <see cref="AnimancerState.MainObject"/> that the created state will have.</summary>
            public virtual Object MainObject { get; }

            /// <summary>The display name of this transition.</summary>
            public virtual string Name
            {
                get
                {
                    var mainObject = MainObject;
                    return mainObject != null ? mainObject.name : null;
                }
            }

            /// <summary>Returns the <see cref="Name"/> and type of this transition.</summary>
            public override string ToString()
            {
                var type = GetType().FullName;

                var name = Name;
                if (name != null)
                    return $"{name} ({type})";
                else
                    return type;
            }

            /************************************************************************************************************************/

#if UNITY_EDITOR
            /// <summary>[Editor-Only] Don't use Inspector Gadgets Nested Object Drawers.</summary>
            private const bool NestedObjectDrawers = false;
#endif

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

