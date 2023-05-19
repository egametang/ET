// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>Base class for <see cref="Playable"/> wrapper objects in an <see cref="AnimancerPlayable"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerNode
    /// 
    public abstract class AnimancerNode : Key, IEnumerable<AnimancerState>, IEnumerator, IPlayableWrapper
    {
        /************************************************************************************************************************/
        #region Playable
        /************************************************************************************************************************/

        /// <summary>
        /// The internal object this node manages in the <see cref="PlayableGraph"/>.
        /// <para></para>
        /// Must be set by <see cref="CreatePlayable()"/>. Failure to do so will throw the following exception throughout
        /// the system when using this node: "<see cref="ArgumentException"/>: The playable passed as an argument is
        /// invalid. To create a valid playable, please use the appropriate Create method".
        /// </summary>
        protected internal Playable _Playable;

        /// <summary>[Internal] The <see cref="Playable"/> managed by this node.</summary>
        Playable IPlayableWrapper.Playable => _Playable;

        /// <summary>
        /// Indicates whether the <see cref="_Playable"/> is usable (properly initialised and not destroyed).
        /// </summary>
        public bool IsValid => _Playable.IsValid();

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only, Internal] Indicates whether the Inspector details for this node are expanded.</summary>
        internal bool _IsInspectorExpanded;
#endif

        /************************************************************************************************************************/

        /// <summary>Creates and assigns the <see cref="Playable"/> managed by this node.</summary>
        /// <remarks>This method also applies the <see cref="Speed"/> if it was set beforehand.</remarks>
        public virtual void CreatePlayable()
        {
#if UNITY_ASSERTIONS
            if (Root == null)
                throw new InvalidOperationException($"{nameof(AnimancerNode)}.{nameof(Root)}" +
                    $" is null when attempting to create its {nameof(Playable)}: {this}" +
                    $"\nThe {nameof(Root)} is generally set when you first play a state," +
                    " so you probably just need to play it before trying to access it.");

            if (_Playable.IsValid())
                Debug.LogWarning($"{nameof(AnimancerNode)}.{nameof(CreatePlayable)}" +
                    $" was called before destroying the previous {nameof(Playable)}: {this}", Root?.Component as Object);
#endif

            CreatePlayable(out _Playable);

#if UNITY_ASSERTIONS
            if (!_Playable.IsValid())
                throw new InvalidOperationException(
                    $"{nameof(AnimancerNode)}.{nameof(CreatePlayable)} did not create a valid {nameof(Playable)}:" + this);
#endif

            if (_Speed != 1)
                _Playable.SetSpeed(_Speed);

            var parent = Parent;
            if (parent != null)
                ApplyConnectedState(parent);
        }

        /// <summary>Creates and assigns the <see cref="Playable"/> managed by this node.</summary>
        protected abstract void CreatePlayable(out Playable playable);

        /************************************************************************************************************************/

        /// <summary>Destroys the <see cref="Playable"/>.</summary>
        public void DestroyPlayable()
        {
            if (_Playable.IsValid())
                Root._Graph.DestroyPlayable(_Playable);
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="DestroyPlayable"/> and <see cref="CreatePlayable()"/>.</summary>
        public virtual void RecreatePlayable()
        {
            DestroyPlayable();
            CreatePlayable();
        }

        /// <summary>Calls <see cref="RecreatePlayable"/> on this node and all its children recursively.</summary>
        public void RecreatePlayableRecursive()
        {
            RecreatePlayable();

            for (int i = ChildCount - 1; i >= 0; i--)
                GetChild(i)?.RecreatePlayableRecursive();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Graph
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerPlayable"/> at the root of the graph.</summary>
        public AnimancerPlayable Root { get; internal set; }

        /************************************************************************************************************************/

        /// <summary>The root <see cref="AnimancerLayer"/> which this node is connected to.</summary>
        public abstract AnimancerLayer Layer { get; }

        /// <summary>The object which receives the output of this node.</summary>
        public abstract IPlayableWrapper Parent { get; }

        /************************************************************************************************************************/

        /// <summary>The index of the port this node is connected to on the parent's <see cref="Playable"/>.</summary>
        /// <remarks>
        /// A negative value indicates that it is not assigned to a port.
        /// <para></para>
        /// Indices are generally assigned starting from 0, ascending in the order they are connected to their layer.
        /// They will not usually change unless the <see cref="Parent"/> changes or another state on the same layer is
        /// destroyed so the last state is swapped into its place to avoid shuffling everything down to cover the gap.
        /// <para></para>
        /// The setter is internal so user defined states cannot set it incorrectly. Ideally,
        /// <see cref="AnimancerLayer"/> should be able to set the port in its constructor and
        /// <see cref="AnimancerState.SetParent"/> should also be able to set it, but classes that further inherit from
        /// there should not be able to change it without properly calling that method.
        /// </remarks>
        public int Index { get; internal set; } = int.MinValue;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="AnimancerNode"/>.</summary>
        protected AnimancerNode()
        {
#if UNITY_ASSERTIONS
            if (TraceConstructor)
                _ConstructorStackTrace = new System.Diagnostics.StackTrace(true);
#endif
        }

        /************************************************************************************************************************/

#if UNITY_ASSERTIONS
        /// <summary>[Assert-Only]
        /// Should a <see cref="System.Diagnostics.StackTrace"/> be captured in the constructor of all new nodes so
        /// <see cref="OptionalWarning.UnusedNode"/> can include it in the warning if that node ends up being unused?
        /// </summary>
        /// <remarks>This has a notable performance cost so it should only be used when trying to identify a problem.</remarks>
        public static bool TraceConstructor { get; set; }

        /// <summary>[Assert-Only]
        /// The stack trace of the constructor (or null if <see cref="TraceConstructor"/> was false).
        /// </summary>
        private System.Diagnostics.StackTrace _ConstructorStackTrace;

        /// <summary>[Assert-Only] Checks <see cref="OptionalWarning.UnusedNode"/>.</summary>
        ~AnimancerNode()
        {
            if (Root != null ||
                OptionalWarning.UnusedNode.IsDisabled())
                return;

#if UNITY_ASSERTIONS
            var name = DebugName;
            if (name == null)
#else
            string name = null;
#endif
            {
                // ToString will likely throw an exception since finalizers are not run on the main thread.
                try { name = ToString(); }
                catch { name = GetType().FullName; }
            }

            var message = $"The {nameof(Root)} {nameof(AnimancerPlayable)} of '{name}'" +
                $" is null during finalization (garbage collection)." +
                $" This probably means that it was never used for anything and should not have been created.";

            if (_ConstructorStackTrace != null)
                message += "\n\nThis node was created at:\n" + _ConstructorStackTrace;
            else
                message += $"\n\nEnable {nameof(AnimancerNode)}.{nameof(TraceConstructor)} on startup to allow" +
                    $" this warning to include the {nameof(System.Diagnostics.StackTrace)} of when the node was constructed.";

            OptionalWarning.UnusedNode.Log(message);
        }
#endif

        /************************************************************************************************************************/

        /// <summary>[Internal] Connects the <see cref="Playable"/> to the <see cref="Parent"/>.</summary>
        internal void ConnectToGraph()
        {
            var parent = Parent;
            if (parent == null)
                return;

#if UNITY_ASSERTIONS
            if (Index < 0)
                throw new InvalidOperationException(
                    $"Invalid {nameof(AnimancerNode)}.{nameof(Index)}" +
                    " when attempting to connect to its parent:" +
                    "\n    Node: " + this +
                    "\n    Parent: " + parent);

            Validate.AssertPlayable(this);
#endif

            var parentPlayable = parent.Playable;
            Root._Graph.Connect(_Playable, 0, parentPlayable, Index);
            parentPlayable.SetInputWeight(Index, _Weight);
            _IsWeightDirty = false;
        }

        /// <summary>[Internal] Disconnects the <see cref="Playable"/> from the <see cref="Parent"/>.</summary>
        internal void DisconnectFromGraph()
        {
            var parent = Parent;
            if (parent == null)
                return;

            var parentPlayable = parent.Playable;
            if (parentPlayable.GetInput(Index).IsValid())
                Root._Graph.Disconnect(parentPlayable, Index);
        }

        /************************************************************************************************************************/

        private void ApplyConnectedState(IPlayableWrapper parent)
        {
#if UNITY_ASSERTIONS
            if (Index < 0)
                throw new InvalidOperationException(
                    $"Invalid {nameof(AnimancerNode)}.{nameof(Index)}" +
                    " when attempting to connect to its parent:" +
                    "\n    Node: " + this +
                    "\n    Parent: " + parent);
#endif

            _IsWeightDirty = true;

            if (_Weight != 0 || parent.KeepChildrenConnected)
            {
                ConnectToGraph();
            }
            else
            {
                Root.RequireUpdate(this);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="AnimancerPlayable.RequireUpdate(AnimancerNode)"/> as long as the <see cref="Root"/> is not
        /// null.
        /// </summary>
        protected void RequireUpdate()
        {
            if (Root != null)
                Root.RequireUpdate(this);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Updates the <see cref="Weight"/> for fading, applies it to this state's port on the parent mixer, and plays
        /// or pauses the <see cref="Playable"/> if its state is dirty.
        /// <para></para>
        /// If the <see cref="Parent"/>'s <see cref="KeepChildrenConnected"/> is set to false, this method will
        /// also connect/disconnect this node from the <see cref="Parent"/> in the playable graph.
        /// </summary>
        protected internal virtual void Update(out bool needsMoreUpdates)
        {
            UpdateFade(out needsMoreUpdates);

            ApplyWeight();

        }

        /************************************************************************************************************************/
        // IEnumerator for yielding in a coroutine to wait until animations have stopped.
        /************************************************************************************************************************/

        /// <summary>
        /// Returns true if the animation is playing and hasn't yet reached its end.
        /// <para></para>
        /// This method is called by <see cref="IEnumerator.MoveNext"/> so this object can be used as a custom yield
        /// instruction to wait until it finishes.
        /// </summary>
        protected internal abstract bool IsPlayingAndNotEnding();

        /// <summary>Calls <see cref="IsPlayingAndNotEnding"/>.</summary>
        bool IEnumerator.MoveNext() => IsPlayingAndNotEnding();

        /// <summary>Returns null.</summary>
        object IEnumerator.Current => null;

        /// <summary>Does nothing.</summary>
        void IEnumerator.Reset() { }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Children
        /************************************************************************************************************************/

        /// <summary>[<see cref="IPlayableWrapper"/>]
        /// The number of states using this node as their <see cref="AnimancerState.Parent"/>.
        /// </summary>
        public virtual int ChildCount => 0;

        /// <summary>Returns the state connected to the specified `index` as a child of this node.</summary>
        /// <exception cref="NotSupportedException">This node can't have children.</exception>
        AnimancerNode IPlayableWrapper.GetChild(int index) => GetChild(index);

        /// <summary>[<see cref="IPlayableWrapper"/>]
        /// Returns the state connected to the specified `index` as a child of this node.
        /// </summary>
        /// <exception cref="NotSupportedException">This node can't have children.</exception>
        public virtual AnimancerState GetChild(int index)
            => throw new NotSupportedException(this + " can't have children.");

        /// <summary>Called when a child is connected with this node as its <see cref="AnimancerState.Parent"/>.</summary>
        /// <exception cref="NotSupportedException">This node can't have children.</exception>
        protected internal virtual void OnAddChild(AnimancerState state)
        {
            state.ClearParent();
            throw new NotSupportedException(this + " can't have children.");
        }

        /// <summary>Called when a child's <see cref="AnimancerState.Parent"/> is changed from this node.</summary>
        /// <exception cref="NotSupportedException">This node can't have children.</exception>
        protected internal virtual void OnRemoveChild(AnimancerState state)
        {
            state.ClearParent();
            throw new NotSupportedException(this + " can't have children.");
        }

        /************************************************************************************************************************/

        /// <summary>Connects the `state` to this node at its <see cref="Index"/>.</summary>
        /// <exception cref="InvalidOperationException">The <see cref="Index"/> was already occupied.</exception>
        protected void OnAddChild(IList<AnimancerState> states, AnimancerState state)
        {
            var index = state.Index;

            if (states[index] != null)
            {
                state.ClearParent();
                throw new InvalidOperationException(
                    $"Tried to add a state to an already occupied port on {this}:" +
                    $"\n    {nameof(Index)}: {index}" +
                    $"\n    Old State: {states[index]} " +
                    $"\n    New State: {state}");
            }

#if UNITY_ASSERTIONS
            if (state.Root != Root)
                Debug.LogError(
                    $"{nameof(AnimancerNode)}.{nameof(Root)} mismatch:" +
                    $"\n    {nameof(state)}: {state}" +
                    $"\n    {nameof(state)}.{nameof(state.Root)}: {state.Root}" +
                    $"\n    {nameof(Parent)}.{nameof(Root)}: {Root}", Root?.Component as Object);
#endif

            states[index] = state;

            if (Root != null)
                state.ApplyConnectedState(this);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Indicates whether child playables should stay connected to this mixer at all times (default false).
        /// </summary>
        public virtual bool KeepChildrenConnected => false;

        /// <summary>
        /// Ensures that all children of this node are connected to the <see cref="_Playable"/>.
        /// </summary>
        internal void ConnectAllChildrenToGraph()
        {
            if (!Parent.Playable.GetInput(Index).IsValid())
                ConnectToGraph();

            for (int i = ChildCount - 1; i >= 0; i--)
                GetChild(i)?.ConnectAllChildrenToGraph();
        }

        /// <summary>
        /// Ensures that all children of this node which have zero weight are disconnected from the
        /// <see cref="_Playable"/>.
        /// </summary>
        internal void DisconnectWeightlessChildrenFromGraph()
        {
            if (Weight == 0)
                DisconnectFromGraph();

            for (int i = ChildCount - 1; i >= 0; i--)
                GetChild(i)?.DisconnectWeightlessChildrenFromGraph();
        }

        /************************************************************************************************************************/
        // IEnumerable for 'foreach' statements.
        /************************************************************************************************************************/

        /// <summary>Gets an enumerator for all of this node's child states.</summary>
        public virtual IEnumerator<AnimancerState> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Weight
        /************************************************************************************************************************/

        /// <summary>The current blend weight of this node. Accessed via <see cref="Weight"/>.</summary>
        private float _Weight;

        /// <summary>Indicates whether the weight has changed and should be applied to the parent mixer.</summary>
        private bool _IsWeightDirty = true;

        /************************************************************************************************************************/

        /// <summary>
        /// The current blend weight of this node which determines how much it affects the final output. 0 has no
        /// effect while 1 applies the full effect of this node and values inbetween apply a proportional effect.
        /// <para></para>
        /// Setting this property cancels any fade currently in progress. If you don't wish to do that, you can use
        /// <see cref="SetWeight"/> instead.
        /// <para></para>
        /// <em>Animancer Lite only allows this value to be set to 0 or 1 in runtime builds.</em>
        /// </summary>
        ///
        /// <example>
        /// Calling <see cref="AnimancerPlayable.Play(AnimationClip)"/> immediately sets the weight of all states to 0
        /// and the new state to 1. Note that this is separate from other values like
        /// <see cref="AnimancerState.IsPlaying"/> so a state can be paused at any point and still show its pose on the
        /// character or it could be still playing at 0 weight if you want it to still trigger events (though states
        /// are normally stopped when they reach 0 weight so you would need to explicitly set it to playing again).
        /// <para></para>
        /// Calling <see cref="AnimancerPlayable.Play(AnimationClip, float, FadeMode)"/> does not immediately change
        /// the weights, but instead calls <see cref="StartFade"/> on every state to set their
        /// <see cref="TargetWeight"/> and <see cref="FadeSpeed"/>. Then every update each state's weight will move
        /// towards that target value at that speed.
        /// </example>
        public float Weight
        {
            get => _Weight;
            set
            {
                SetWeight(value);
                TargetWeight = value;
                FadeSpeed = 0;
            }
        }

        /// <summary>
        /// Sets the current blend weight of this node which determines how much it affects the final output.
        /// 0 has no effect while 1 applies the full effect of this node.
        /// <para></para>
        /// This method allows any fade currently in progress to continue. If you don't wish to do that, you can set
        /// the <see cref="Weight"/> property instead.
        /// <para></para>
        /// <em>Animancer Lite only allows this value to be set to 0 or 1 in runtime builds.</em>
        /// </summary>
        public void SetWeight(float value)
        {
            if (_Weight == value)
                return;

#if UNITY_ASSERTIONS
            if (!(value >= 0) || value == float.PositiveInfinity)// Reversed comparison includes NaN.
                throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(Weight)} must be a finite positive value");
#endif

            _Weight = value;
            SetWeightDirty();
        }

        /// <summary>
        /// Flags this node as having a dirty weight that needs to be applied next update.
        /// </summary>
        protected internal void SetWeightDirty()
        {
            _IsWeightDirty = true;
            RequireUpdate();
        }

        /************************************************************************************************************************/

        /// <summary>[Internal]
        /// Applies the <see cref="Weight"/> to the connection between this node and its <see cref="Parent"/>.
        /// </summary>
        internal void ApplyWeight()
        {

            if (!_IsWeightDirty)
                return;

            _IsWeightDirty = false;

            var parent = Parent;
            if (parent == null)
                return;

            Playable parentPlayable;

            if (!parent.KeepChildrenConnected)
            {
                if (_Weight == 0)
                {
                    DisconnectFromGraph();
                    return;
                }

                parentPlayable = parent.Playable;
                if (!parentPlayable.GetInput(Index).IsValid())
                    ConnectToGraph();
            }
            else parentPlayable = parent.Playable;

            parentPlayable.SetInputWeight(Index, _Weight);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Fading
        /************************************************************************************************************************/

        /// <summary>
        /// The desired <see cref="Weight"/> which this node is fading towards according to the
        /// <see cref="FadeSpeed"/>.
        /// </summary>
        public float TargetWeight { get; set; }

        /// <summary>
        /// The speed at which this node is fading towards the <see cref="TargetWeight"/>.
        /// </summary>
        public float FadeSpeed { get; set; }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="OnStartFade"/> and starts fading the <see cref="Weight"/> over the course
        /// of the `fadeDuration` (in seconds).
        /// <para></para>
        /// If the `targetWeight` is 0 then <see cref="Stop"/> will be called when the fade is complete.
        /// <para></para>
        /// If the <see cref="Weight"/> is already equal to the `targetWeight` then the fade will end
        /// immediately.
        /// <para></para>
        /// <em>Animancer Lite only allows a `targetWeight` of 0 or 1 and the default `fadeDuration` (0.25 seconds) in
        /// runtime builds.</em>
        /// </summary>
        public void StartFade(float targetWeight, float fadeDuration = AnimancerPlayable.DefaultFadeDuration)
        {

            TargetWeight = targetWeight;

            if (targetWeight == Weight)
            {
                if (targetWeight == 0)
                {
                    Stop();
                }
                else
                {
                    FadeSpeed = 0;
                    OnStartFade();
                }

                return;
            }

            // Duration 0 = Instant.
            if (fadeDuration <= 0)
            {
                FadeSpeed = float.PositiveInfinity;
            }
            else// Otherwise determine how fast we need to go to cover the distance in the specified time.
            {
                FadeSpeed = Math.Abs(Weight - targetWeight) / fadeDuration;
            }

            OnStartFade();
            RequireUpdate();
        }

        /************************************************************************************************************************/

        /// <summary>Called by <see cref="StartFade"/>.</summary>
        protected internal abstract void OnStartFade();

        /************************************************************************************************************************/

        /// <summary>
        /// Stops the animation and makes it inactive immediately so it no longer affects the output.
        /// Sets <see cref="Weight"/> = 0 by default.
        /// </summary>
        public virtual void Stop()
        {
            Weight = 0;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Moves the <see cref="Weight"/> towards the <see cref="TargetWeight"/> according to the
        /// <see cref="FadeSpeed"/>.
        /// </summary>
        private void UpdateFade(out bool needsMoreUpdates)
        {
            var fadeSpeed = FadeSpeed;
            if (fadeSpeed == 0)
            {
                needsMoreUpdates = false;
                return;
            }

            _IsWeightDirty = true;

            fadeSpeed *= ParentEffectiveSpeed * AnimancerPlayable.DeltaTime;
            if (fadeSpeed < 0)
                fadeSpeed = -fadeSpeed;

            var target = TargetWeight;
            var current = _Weight;

            var delta = target - current;
            if (delta > 0)
            {
                if (delta > fadeSpeed)
                {
                    _Weight = current + fadeSpeed;
                    needsMoreUpdates = true;
                    return;
                }
            }
            else
            {
                if (-delta > fadeSpeed)
                {
                    _Weight = current - fadeSpeed;
                    needsMoreUpdates = true;
                    return;
                }
            }

            _Weight = target;
            needsMoreUpdates = false;

            if (target == 0)
            {
                Stop();
            }
            else
            {
                FadeSpeed = 0;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Inverse Kinematics
        /************************************************************************************************************************/

        /// <summary>
        /// Should setting the <see cref="Parent"/> also set this node's <see cref="ApplyAnimatorIK"/> to match it?
        /// Default is true.
        /// </summary>
        public static bool ApplyParentAnimatorIK { get; set; } = true;

        /// <summary>
        /// Should setting the <see cref="Parent"/> also set this node's <see cref="ApplyFootIK"/> to match it?
        /// Default is true.
        /// </summary>
        public static bool ApplyParentFootIK { get; set; } = true;

        /************************************************************************************************************************/

        /// <summary>
        /// Copies the <see cref="ApplyAnimatorIK"/> and <see cref="ApplyFootIK"/> settings from the
        /// <see cref="Parent"/> if <see cref="ApplyParentAnimatorIK"/> and <see cref="ApplyParentFootIK"/> are true
        /// respectively.
        /// </summary>
        public virtual void CopyIKFlags(AnimancerNode node)
        {
            if (Root == null)
                return;

            if (ApplyParentAnimatorIK)
            {
                ApplyAnimatorIK = node.ApplyAnimatorIK;
                if (ApplyParentFootIK)
                    ApplyFootIK = node.ApplyFootIK;
            }
            else if (ApplyParentFootIK)
            {
                ApplyFootIK = node.ApplyFootIK;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual bool ApplyAnimatorIK
        {
            get
            {
                for (int i = ChildCount - 1; i >= 0; i--)
                {
                    var state = GetChild(i);
                    if (state == null)
                        continue;

                    if (state.ApplyAnimatorIK)
                        return true;
                }

                return false;
            }
            set
            {
                for (int i = ChildCount - 1; i >= 0; i--)
                {
                    var state = GetChild(i);
                    if (state == null)
                        continue;

                    state.ApplyAnimatorIK = value;
                }
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual bool ApplyFootIK
        {
            get
            {
                for (int i = ChildCount - 1; i >= 0; i--)
                {
                    var state = GetChild(i);
                    if (state == null)
                        continue;

                    if (state.ApplyFootIK)
                        return true;
                }

                return false;
            }
            set
            {
                for (int i = ChildCount - 1; i >= 0; i--)
                {
                    var state = GetChild(i);
                    if (state == null)
                        continue;

                    state.ApplyFootIK = value;
                }
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Speed
        /************************************************************************************************************************/

        private float _Speed = 1;

        /// <summary>[Pro-Only] How fast the <see cref="AnimancerState.Time"/> is advancing every frame (default 1).</summary>
        /// 
        /// <remarks>
        /// A negative value will play the animation backwards.
        /// <para></para>
        /// To pause an animation, consider setting <see cref="AnimancerState.IsPlaying"/> to false instead of setting
        /// this value to 0.
        /// <para></para>
        /// <em>Animancer Lite does not allow this value to be changed in runtime builds.</em>
        /// </remarks>
        ///
        /// <example><code>
        /// void PlayAnimation(AnimancerComponent animancer, AnimationClip clip)
        /// {
        ///     var state = animancer.Play(clip);
        ///
        ///     state.Speed = 1;// Normal speed.
        ///     state.Speed = 2;// Double speed.
        ///     state.Speed = 0.5f;// Half speed.
        ///     state.Speed = -1;// Normal speed playing backwards.
        /// }
        /// </code></example>
        public float Speed
        {
            get => _Speed;
            set
            {
#if UNITY_ASSERTIONS
                if (!value.IsFinite())
                    throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(Speed)} must be finite");
#endif
                _Speed = value;

                if (_Playable.IsValid())
                    _Playable.SetSpeed(value);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The multiplied <see cref="Speed"/> of each of this node's parents down the hierarchy, excluding the root
        /// <see cref="AnimancerPlayable.Speed"/>.
        /// </summary>
        private float ParentEffectiveSpeed
        {
            get
            {
                var parent = Parent;
                if (parent == null)
                    return 1;

                var speed = parent.Speed;

                while ((parent = parent.Parent) != null)
                {
                    speed *= parent.Speed;
                }

                return speed;
            }
        }

        /// <summary>
        /// The <see cref="Speed"/> of this node multiplied by the <see cref="Speed"/> of each of its parents to
        /// determine the actual speed it's playing at.
        /// </summary>
        public float EffectiveSpeed
        {
            get => Speed * ParentEffectiveSpeed;
            set => Speed = value / ParentEffectiveSpeed;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Descriptions
        /************************************************************************************************************************/

#if UNITY_ASSERTIONS
        /// <summary>[Assert-Only] The Inspector display name of this node.</summary>
        /// <remarks>Set using <see cref="SetDebugName"/>.</remarks>
        public string DebugName { get; private set; }
#endif

        /// <summary>The Inspector display name of this node.</summary>
        public override string ToString()
        {
#if UNITY_ASSERTIONS
            if (DebugName != null)
                return DebugName;
#endif

            return base.ToString();
        }

        /// <summary>[Assert-Conditional]
        /// Sets the Inspector display name of this node. <see cref="ToString"/> returns the name.
        /// </summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public void SetDebugName(string name)
        {
#if UNITY_ASSERTIONS
            DebugName = name;
#endif
        }

        /************************************************************************************************************************/

        /// <summary>Returns a detailed descrption of the current details of this node.</summary>
        public string GetDescription(int maxChildDepth = 10, string delimiter = "\n")
        {
            var text = ObjectPool.AcquireStringBuilder();
            AppendDescription(text, maxChildDepth, delimiter);
            return text.ReleaseToString();
        }

        /************************************************************************************************************************/

        /// <summary>Appends a detailed descrption of the current details of this node.</summary>
        public void AppendDescription(StringBuilder text, int maxChildDepth = 10, string delimiter = "\n")
        {
            text.Append(ToString());

            AppendDetails(text, delimiter);

            if (maxChildDepth-- > 0 && ChildCount > 0)
            {
                text.Append(delimiter).Append($"{nameof(ChildCount)}: ").Append(ChildCount);
                var indentedDelimiter = delimiter + Strings.Indent;

                var i = 0;
                foreach (var childState in this)
                {
                    text.Append(delimiter).Append('[').Append(i++).Append("] ");
                    childState.AppendDescription(text, maxChildDepth, indentedDelimiter);
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Called by <see cref="AppendDescription"/> to append the details of this node.</summary>
        protected virtual void AppendDetails(StringBuilder text, string delimiter)
        {
            text.Append(delimiter).Append("Playable: ");
            if (_Playable.IsValid())
                text.Append(_Playable.GetPlayableType());
            else
                text.Append("Invalid");

            text.Append(delimiter).Append($"{nameof(Index)}: ").Append(Index);

            var realSpeed = _Playable.IsValid() ? _Playable.GetSpeed() : _Speed;
            if (realSpeed == _Speed)
            {
                text.Append(delimiter).Append($"{nameof(Speed)}: ").Append(_Speed);
            }
            else
            {
                text.Append(delimiter).Append($"{nameof(Speed)} (Real): ").Append(_Speed)
                    .Append(" (").Append(realSpeed).Append(')');
            }

            text.Append(delimiter).Append($"{nameof(Weight)}: ").Append(Weight);

            if (Weight != TargetWeight)
            {
                text.Append(delimiter).Append($"{nameof(TargetWeight)}: ").Append(TargetWeight);
                text.Append(delimiter).Append($"{nameof(FadeSpeed)}: ").Append(FadeSpeed);
            }

            AppendIKDetails(text, delimiter, this);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Appends the details of <see cref="IPlayableWrapper.ApplyAnimatorIK"/> and
        /// <see cref="IPlayableWrapper.ApplyFootIK"/>.
        /// </summary>
        public static void AppendIKDetails(StringBuilder text, string delimiter, IPlayableWrapper node)
        {
            text.Append(delimiter).Append("InverseKinematics: ");
            if (node.ApplyAnimatorIK)
            {
                text.Append("OnAnimatorIK");
                if (node.ApplyFootIK)
                    text.Append(", FootIK");
            }
            else if (node.ApplyFootIK)
            {
                text.Append("FootIK");
            }
            else
            {
                text.Append("None");
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

