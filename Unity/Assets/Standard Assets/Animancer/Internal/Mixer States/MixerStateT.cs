// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>[Pro-Only]
    /// Base class for mixers which blend an array of child states together based on a <see cref="Parameter"/>.
    /// </summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/MixerState_1
    /// 
    public abstract class MixerState<TParameter> : ManualMixerState
    {
        /************************************************************************************************************************/
        #region Properties
        /************************************************************************************************************************/

        /// <summary>An empty array of parameter values.</summary>
        public static readonly TParameter[] NoParameters = new TParameter[0];

        /// <summary>The parameter values at which each of the child states are used and blended.</summary>
        private TParameter[] _Thresholds = NoParameters;

        /************************************************************************************************************************/

        private TParameter _Parameter;

        /// <summary>The value used to calculate the weights of the child states.</summary>
        public TParameter Parameter
        {
            get => _Parameter;
            set
            {
                _Parameter = value;
                WeightsAreDirty = true;
                RequireUpdate();
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Thresholds
        /************************************************************************************************************************/

        /// <summary>
        /// Indicates whether the array of thresholds has been initialised with a size at least equal to the
        /// <see cref="AnimancerNode.ChildCount"/>.
        /// </summary>
        public bool HasThresholds => _Thresholds.Length >= ChildCount;

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the value of the threshold associated with the specified index.
        /// </summary>
        public TParameter GetThreshold(int index) => _Thresholds[index];

        /************************************************************************************************************************/

        /// <summary>
        /// Sets the value of the threshold associated with the specified index.
        /// </summary>
        public void SetThreshold(int index, TParameter threshold)
        {
            _Thresholds[index] = threshold;
            OnThresholdsChanged();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Assigns the specified array as the thresholds to use for blending.
        /// <para></para>
        /// WARNING: if you keep a reference to the `thresholds` array you must call <see cref="OnThresholdsChanged"/>
        /// whenever any changes are made to it, otherwise this mixer may not blend correctly.
        /// </summary>
        public void SetThresholds(params TParameter[] thresholds)
        {
#if UNITY_ASSERTIONS
            if (thresholds == null)
                throw new ArgumentNullException(nameof(thresholds));
#endif

            if (thresholds.Length != ChildCount)
                throw new ArgumentOutOfRangeException(nameof(thresholds), "Incorrect threshold count. There are " + ChildCount +
                    " states, but the specified thresholds array contains " + thresholds.Length + " elements.");

            _Thresholds = thresholds;
            OnThresholdsChanged();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="Array.Length"/> of the <see cref="_Thresholds"/> is not equal to the
        /// <see cref="AnimancerNode.ChildCount"/>, this method assigns a new array of that size and returns true.
        /// </summary>
        public bool ValidateThresholdCount()
        {
            var count = ChildCount;
            if (_Thresholds.Length != count)
            {
                _Thresholds = new TParameter[count];
                return true;
            }
            else return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Called whenever the thresholds are changed. By default this method simply indicates that the blend weights
        /// need recalculating but it can be overridden by child classes to perform validation checks or optimisations.
        /// </summary>
        public virtual void OnThresholdsChanged()
        {
            WeightsAreDirty = true;
            RequireUpdate();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls `calculate` for each of the <see cref="ManualMixerState._States"/> and stores the returned value as
        /// the threshold for that state.
        /// </summary>
        public void CalculateThresholds(Func<AnimancerState, TParameter> calculate)
        {
            ValidateThresholdCount();

            for (int i = ChildCount - 1; i >= 0; i--)
            {
                var state = GetChild(i);
                if (state == null)
                    continue;

                _Thresholds[i] = calculate(state);
            }

            OnThresholdsChanged();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Stores the values of all parameters, calls <see cref="AnimancerNode.DestroyPlayable"/>, then restores the
        /// parameter values.
        /// </summary>
        public override void RecreatePlayable()
        {
            base.RecreatePlayable();
            WeightsAreDirty = true;
            RequireUpdate();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Initialisation
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Initialise(int portCount)
        {
            base.Initialise(portCount);
            _Thresholds = new TParameter[portCount];
            OnThresholdsChanged();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Initialises the <see cref="AnimationMixerPlayable"/> and <see cref="ManualMixerState._States"/> with one
        /// state per clip and assigns the `thresholds`.
        /// <para></para>
        /// WARNING: if you keep a reference to the `thresholds` array, you must call
        /// <see cref="OnThresholdsChanged"/> whenever any changes are made to it, otherwise this mixer may not blend
        /// correctly.
        /// </summary>
        public void Initialise(AnimationClip[] clips, TParameter[] thresholds)
        {
            Initialise(clips);
            _Thresholds = thresholds;
            OnThresholdsChanged();
        }

        /// <summary>
        /// Initialises the <see cref="AnimationMixerPlayable"/> and <see cref="ManualMixerState._States"/> with one
        /// state per clip and assigns the thresholds by calling `calculateThreshold` for each state.
        /// </summary>
        public void Initialise(AnimationClip[] clips, Func<AnimancerState, TParameter> calculateThreshold)
        {
            Initialise(clips);
            CalculateThresholds(calculateThreshold);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates and returns a new <see cref="ClipState"/> to play the `clip` with this
        /// <see cref="MixerState"/> as its parent, connects it to the specified `index`, and assigns the
        /// `threshold` for it.
        /// </summary>
        public ClipState CreateChild(int index, AnimationClip clip, TParameter threshold)
        {
            SetThreshold(index, threshold);
            return CreateChild(index, clip);
        }

        /// <summary>
        /// Calls <see cref="AnimancerUtilities.CreateStateAndApply"/>, sets this mixer as the state's parent, and
        /// assigns the `threshold` for it.
        /// </summary>
        public AnimancerState CreateChild(int index, ITransition transition, TParameter threshold)
        {
            SetThreshold(index, threshold);
            return CreateChild(index, transition);
        }

        /************************************************************************************************************************/

        /// <summary>Assigns the `state` as a child of this mixer and assigns the `threshold` for it.</summary>
        public void SetChild(int index, AnimancerState state, TParameter threshold)
        {
            SetChild(index, state);
            SetThreshold(index, threshold);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Descriptions
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string GetDisplayKey(AnimancerState state) => $"[{state.Index}] {_Thresholds[state.Index]}";

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void AppendDetails(StringBuilder text, string delimiter)
        {
            text.Append(delimiter);
            text.Append($"{nameof(Parameter)}: ");
            AppendParameter(text, Parameter);

            text.Append(delimiter).Append("Thresholds: ");
            for (int i = 0; i < _Thresholds.Length; i++)
            {
                if (i > 0)
                    text.Append(", ");

                AppendParameter(text, _Thresholds[i]);
            }

            base.AppendDetails(text, delimiter);
        }

        /************************************************************************************************************************/

        /// <summary>Appends the `parameter` in a viewer-friendly format.</summary>
        public virtual void AppendParameter(StringBuilder description, TParameter parameter)
        {
            description.Append(parameter);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

