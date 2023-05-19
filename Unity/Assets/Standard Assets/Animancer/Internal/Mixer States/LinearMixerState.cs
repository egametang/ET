// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>[Pro-Only]
    /// An <see cref="AnimancerState"/> which blends an array of other states together using linear interpolation
    /// between the specified thresholds.
    /// </summary>
    /// <remarks>
    /// This mixer type is similar to the 1D Blend Type in Mecanim Blend Trees.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/LinearMixerState
    /// 
    public class LinearMixerState : MixerState<float>
    {
        /************************************************************************************************************************/

        private bool _ExtrapolateSpeed = true;

        /// <summary>
        /// Should setting the <see cref="MixerState{TParameter}.Parameter"/> above the highest threshold increase the
        /// <see cref="AnimancerNode.Speed"/> of this mixer proportionally?
        /// </summary>
        public bool ExtrapolateSpeed
        {
            get => _ExtrapolateSpeed;
            set
            {
                _ExtrapolateSpeed = value;

                if (!_Playable.IsValid())
                    return;

                var speed = Speed;

                var childCount = ChildCount;
                if (value && childCount > 0)
                {
                    var threshold = GetThreshold(childCount - 1);
                    if (Parameter > threshold)
                        speed *= Parameter / threshold;
                }

                _Playable.SetSpeed(speed);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Initialises the <see cref="AnimationMixerPlayable"/> and <see cref="ManualMixerState._States"/> with one
        /// state per clip and assigns thresholds evenly spaced between the specified min and max (inclusive).
        /// </summary>
        public void Initialise(AnimationClip[] clips, float minThreshold = 0, float maxThreshold = 1)
        {
#if UNITY_ASSERTIONS
            if (minThreshold >= maxThreshold)
                throw new ArgumentException($"{nameof(minThreshold)} must be less than {nameof(maxThreshold)}");
#endif

            base.Initialise(clips);
            AssignLinearThresholds(minThreshold, maxThreshold);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Initialises the <see cref="AnimationMixerPlayable"/> with two ports and connects two states to them for
        /// the specified clips at the specified thresholds (default 0 and 1).
        /// </summary>
        public void Initialise(AnimationClip clip0, AnimationClip clip1,
            float threshold0 = 0, float threshold1 = 1)
        {
            Initialise(2);
            CreateChild(0, clip0);
            CreateChild(1, clip1);
            SetThresholds(threshold0, threshold1);
#if UNITY_ASSERTIONS
            AssertThresholdsSorted();
#endif
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Initialises the <see cref="AnimationMixerPlayable"/> with three ports and connects three states to them for
        /// the specified clips at the specified thresholds (default -1, 0, and 1).
        /// </summary>
        public void Initialise(AnimationClip clip0, AnimationClip clip1, AnimationClip clip2,
            float threshold0 = -1, float threshold1 = 0, float threshold2 = 1)
        {
            Initialise(3);
            CreateChild(0, clip0);
            CreateChild(1, clip1);
            CreateChild(2, clip2);
            SetThresholds(threshold0, threshold1, threshold2);
#if UNITY_ASSERTIONS
            AssertThresholdsSorted();
#endif
        }

        /************************************************************************************************************************/
#if UNITY_ASSERTIONS
        /************************************************************************************************************************/

        private bool _NeedToCheckThresholdSorting;

        /// <summary>
        /// Called whenever the thresholds are changed. Indicates that <see cref="AssertThresholdsSorted"/> needs to
        /// be called by the next <see cref="ForceRecalculateWeights"/> if UNITY_ASSERTIONS is defined, then calls
        /// <see cref="MixerState{TParameter}.OnThresholdsChanged"/>.
        /// </summary>
        public override void OnThresholdsChanged()
        {
            _NeedToCheckThresholdSorting = true;

            base.OnThresholdsChanged();
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the thresholds are not sorted from lowest to highest without
        /// any duplicates.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="InvalidOperationException">The thresholds have not been initialised.</exception>
        public void AssertThresholdsSorted()
        {
#if UNITY_ASSERTIONS
            _NeedToCheckThresholdSorting = false;
#endif

            if (!HasThresholds)
                throw new InvalidOperationException("Thresholds have not been initialised");

            var previous = float.NegativeInfinity;

            var childCount = ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                var state = GetChild(i);
                if (state == null)
                    continue;

                var next = GetThreshold(i);
                if (next > previous)
                    previous = next;
                else
                    throw new ArgumentException("Thresholds are out of order." +
                        " They must be sorted from lowest to highest with no equal values.");
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Recalculates the weights of all <see cref="ManualMixerState._States"/> based on the current value of the
        /// <see cref="MixerState{TParameter}.Parameter"/> and the thresholds.
        /// </summary>
        protected override void ForceRecalculateWeights()
        {
            WeightsAreDirty = false;

#if UNITY_ASSERTIONS
            if (_NeedToCheckThresholdSorting)
                AssertThresholdsSorted();
#endif

            // Go through all states, figure out how much weight to give those with thresholds adjacent to the
            // current parameter value using linear interpolation, and set all others to 0 weight.

            var index = 0;
            var previousState = GetNextState(ref index);
            if (previousState == null)
                goto ResetExtrapolatedSpeed;

            var parameter = Parameter;
            var previousThreshold = GetThreshold(index);

            if (parameter <= previousThreshold)
            {
                previousState.Weight = 1;
                DisableRemainingStates(index);
                goto ResetExtrapolatedSpeed;
            }

            var childCount = ChildCount;
            while (++index < childCount)
            {
                var nextState = GetNextState(ref index);
                if (nextState == null)
                    break;

                var nextThreshold = GetThreshold(index);

                if (parameter > previousThreshold && parameter <= nextThreshold)
                {
                    var t = (parameter - previousThreshold) / (nextThreshold - previousThreshold);
                    previousState.Weight = 1 - t;
                    nextState.Weight = t;
                    DisableRemainingStates(index);
                    goto ResetExtrapolatedSpeed;
                }
                else
                {
                    previousState.Weight = 0;
                }

                previousState = nextState;
                previousThreshold = nextThreshold;
            }

            previousState.Weight = 1;

            if (ExtrapolateSpeed)
                _Playable.SetSpeed(Speed * (parameter / previousThreshold));

            return;
            ResetExtrapolatedSpeed:
            if (ExtrapolateSpeed && _Playable.IsValid())
                _Playable.SetSpeed(Speed);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Assigns the thresholds to be evenly spaced between the specified min and max (inclusive).
        /// </summary>
        public void AssignLinearThresholds(float min = 0, float max = 1)
        {
            var childCount = ChildCount;

            var thresholds = new float[childCount];

            var increment = (max - min) / (childCount - 1);

            for (int i = 0; i < childCount; i++)
            {
                thresholds[i] =
                    i < childCount - 1 ?
                    min + i * increment :// Assign each threshold linearly spaced between the min and max.
                    max;// and ensure that the last one is exactly at the max (to avoid floating-point error).
            }

            SetThresholds(thresholds);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void AppendDetails(StringBuilder text, string delimiter)
        {
            text.Append(delimiter)
                .Append($"{nameof(ExtrapolateSpeed)}: ")
                .Append(ExtrapolateSpeed);

            base.AppendDetails(text, delimiter);
        }

        /************************************************************************************************************************/
        #region Inspector
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override int ParameterCount => 1;

        /// <inheritdoc/>
        protected override string GetParameterName(int index) => "Parameter";

        /// <inheritdoc/>
        protected override AnimatorControllerParameterType GetParameterType(int index) => AnimatorControllerParameterType.Float;

        /// <inheritdoc/>
        protected override object GetParameterValue(int index) => Parameter;

        /// <inheritdoc/>
        protected override void SetParameterValue(int index, object value) => Parameter = (float)value;

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Returns a <see cref="Drawer"/> for this state.</summary>
        protected internal override Editor.IAnimancerNodeDrawer CreateDrawer() => new Drawer(this);

        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Draws the Inspector GUI for an <see cref="ControllerState"/>.</summary>
        /// <remarks>
        /// Unfortunately the tool used to generate this documentation does not currently support nested types with
        /// identical names, so only one <c>Drawer</c> class will actually have a documentation page.
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
        /// </remarks>
        public class Drawer : Drawer<LinearMixerState>
        {
            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="Drawer"/> to manage the Inspector GUI for the `state`.
            /// </summary>
            public Drawer(LinearMixerState state) : base(state) { }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void AddContextMenuFunctions(UnityEditor.GenericMenu menu)
            {
                base.AddContextMenuFunctions(menu);

                menu.AddItem(new GUIContent("Extrapolate Speed"), Target.ExtrapolateSpeed, () =>
                {
                    Target.ExtrapolateSpeed = !Target.ExtrapolateSpeed;
                });

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
        /// A serializable <see cref="ITransition"/> which can create a <see cref="LinearMixerState"/> when
        /// passed into <see cref="AnimancerPlayable.Play(ITransition)"/>.
        /// </summary>
        /// <remarks>
        /// Unfortunately the tool used to generate this documentation does not currently support nested types with
        /// identical names, so only one <c>Transition</c> class will actually have a documentation page.
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Transition
        /// 
        [Serializable]
        public new class Transition : Transition<LinearMixerState, float>
        {
            /************************************************************************************************************************/

            [SerializeField]
            [Tooltip("Should setting the Parameter above the highest threshold increase the Speed of the mixer proportionally?")]
            private bool _ExtrapolateSpeed = true;

            /// <summary>[<see cref="SerializeField"/>]
            /// Should setting the <see cref="MixerState{TParameter}.Parameter"/> above the highest threshold increase the
            /// <see cref="AnimancerNode.Speed"/> of the mixer proportionally?
            /// </summary>
            public ref bool ExtrapolateSpeed => ref _ExtrapolateSpeed;

            /************************************************************************************************************************/

            /// <summary>
            /// Creates and returns a new <see cref="LinearMixerState"/>.
            /// <para></para>
            /// Note that using methods like <see cref="AnimancerPlayable.Play(ITransition)"/> will also call
            /// <see cref="ITransition.Apply"/>, so if you call this method manually you may want to call that method
            /// as well. Or you can just use <see cref="AnimancerUtilities.CreateStateAndApply"/>.
            /// <para></para>
            /// This method also assigns it as the <see cref="AnimancerState.Transition{TState}.State"/>.
            /// </summary>
            public override LinearMixerState CreateState()
            {
                State = new LinearMixerState();
                InitialiseState();
                return State;
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void Apply(AnimancerState state)
            {
                base.Apply(state);
                State.ExtrapolateSpeed = _ExtrapolateSpeed;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Sorts all states so that their thresholds go from lowest to highest.
            /// <para></para>
            /// This method uses Bubble Sort which is inefficient for large numbers of states.
            /// </summary>
            public void SortByThresholds()
            {
                var thresholdCount = Thresholds.Length;
                if (thresholdCount <= 1)
                    return;

                var speedCount = Speeds.Length;
                var syncCount = SynchroniseChildren.Length;

                var previousThreshold = Thresholds[0];

                for (int i = 1; i < thresholdCount; i++)
                {
                    var threshold = Thresholds[i];
                    if (threshold >= previousThreshold)
                    {
                        previousThreshold = threshold;
                        continue;
                    }

                    Thresholds.Swap(i, i - 1);
                    States.Swap(i, i - 1);

                    if (i < speedCount)
                        Speeds.Swap(i, i - 1);

                    if (i == syncCount && !SynchroniseChildren[i - 1])
                    {
                        var sync = SynchroniseChildren;
                        Array.Resize(ref sync, ++syncCount);
                        sync[i - 1] = true;
                        sync[i] = false;
                        SynchroniseChildren = sync;
                    }
                    else if (i < syncCount)
                    {
                        SynchroniseChildren.Swap(i, i - 1);
                    }

                    if (i == 1)
                    {
                        i = 0;
                        previousThreshold = float.NegativeInfinity;
                    }
                    else
                    {
                        i -= 2;
                        previousThreshold = Thresholds[i];
                    }
                }
            }

            /************************************************************************************************************************/
            #region Drawer
#if UNITY_EDITOR
            /************************************************************************************************************************/

            /// <summary>[Editor-Only] Draws the Inspector GUI for a <see cref="Transition"/>.</summary>
            /// <remarks>
            /// Unfortunately the tool used to generate this documentation does not currently support nested types with
            /// identical names, so only one <c>Drawer</c> class will actually have a documentation page.
            /// <para></para>
            /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
            /// <para></para>
            /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
            /// </remarks>
            [UnityEditor.CustomPropertyDrawer(typeof(Transition), true)]
            public class Drawer : TransitionDrawer
            {
                /************************************************************************************************************************/

                private static GUIContent _SortingErrorContent;

                /// <inheritdoc/>
                protected override void DoThresholdGUI(Rect area, int index)
                {
                    var color = GUI.color;

                    if (index > 0)
                    {
                        var previousThreshold = CurrentThresholds.GetArrayElementAtIndex(index - 1);
                        var currentThreshold = CurrentThresholds.GetArrayElementAtIndex(index);
                        if (previousThreshold.floatValue >= currentThreshold.floatValue)
                        {
                            if (_SortingErrorContent == null)
                            {
                                _SortingErrorContent = UnityEditor.EditorGUIUtility.IconContent("console.erroricon.sml");

                                _SortingErrorContent.tooltip =
                                    "Linear Mixer Thresholds must always be sorted in ascending order (click to sort)";
                            }

                            var style = ObjectPool.GetCachedResult(() => new GUIStyle(GUI.skin.label)
                            {
                                padding = new RectOffset(),
                            });

                            var iconArea = Editor.AnimancerGUI.StealFromRight(ref area, area.height, Editor.AnimancerGUI.StandardSpacing);
                            if (GUI.Button(iconArea, _SortingErrorContent, style))
                            {
                                Editor.Serialization.RecordUndo(Context.Property);
                                ((Transition)Context.Transition).SortByThresholds();
                            }

                            GUI.color = Editor.AnimancerGUI.ErrorFieldColor;
                        }
                    }

                    base.DoThresholdGUI(area, index);

                    GUI.color = color;
                }

                /************************************************************************************************************************/

                /// <inheritdoc/>
                protected override void AddThresholdFunctionsToMenu(UnityEditor.GenericMenu menu)
                {
                    AddPropertyModifierFunction(menu, "Evenly Spaced", (_) =>
                    {
                        var count = CurrentThresholds.arraySize;
                        if (count <= 1)
                            return;

                        var first = CurrentThresholds.GetArrayElementAtIndex(0).floatValue;
                        var last = CurrentThresholds.GetArrayElementAtIndex(count - 1).floatValue;
                        for (int i = 0; i < count; i++)
                        {
                            CurrentThresholds.GetArrayElementAtIndex(i).floatValue = Mathf.Lerp(first, last, i / (float)(count - 1));
                        }
                    });

                    AddCalculateThresholdsFunction(menu, "From Speed",
                        (state, threshold) => AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) ? velocity.magnitude : float.NaN);
                    AddCalculateThresholdsFunction(menu, "From Velocity X",
                        (state, threshold) => AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) ? velocity.x : float.NaN);
                    AddCalculateThresholdsFunction(menu, "From Velocity Y",
                        (state, threshold) => AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) ? velocity.y : float.NaN);
                    AddCalculateThresholdsFunction(menu, "From Velocity Z",
                        (state, threshold) => AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) ? velocity.z : float.NaN);
                    AddCalculateThresholdsFunction(menu, "From Angular Speed (Rad)",
                        (state, threshold) => AnimancerUtilities.TryGetAverageAngularSpeed(state, out var speed) ? speed : float.NaN);
                    AddCalculateThresholdsFunction(menu, "From Angular Speed (Deg)",
                        (state, threshold) => AnimancerUtilities.TryGetAverageAngularSpeed(state, out var speed) ? speed * Mathf.Rad2Deg : float.NaN);
                }

                /************************************************************************************************************************/

                private void AddCalculateThresholdsFunction(UnityEditor.GenericMenu menu, string label,
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
                            var value = calculateThreshold(state, threshold.floatValue);
                            if (!float.IsNaN(value))
                                threshold.floatValue = value;
                        }
                    });
                }

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
    }
}

