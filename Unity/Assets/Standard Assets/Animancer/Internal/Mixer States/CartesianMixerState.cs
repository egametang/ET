// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Text;
using UnityEngine;

namespace Animancer
{
    /// <summary>[Pro-Only]
    /// An <see cref="AnimancerState"/> which blends an array of other states together based on a two dimensional
    /// parameter and thresholds using Gradient Band Interpolation.
    /// </summary>
    /// <remarks>
    /// This mixer type is similar to the 2D Freeform Cartesian Blend Type in Mecanim Blend Trees.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/CartesianMixerState
    /// 
    public class CartesianMixerState : MixerState<Vector2>
    {
        /************************************************************************************************************************/

        /// <summary>Precalculated values to speed up the recalculation of weights.</summary>
        private Vector2[][] _BlendFactors;

        /// <summary>Indicates whether the <see cref="_BlendFactors"/> need to be recalculated.</summary>
        private bool _BlendFactorsDirty = true;

        /************************************************************************************************************************/

        /// <summary>Gets or sets Parameter.x.</summary>
        public float ParameterX
        {
            get => Parameter.x;
            set => Parameter = new Vector2(value, Parameter.y);
        }

        /// <summary>Gets or sets Parameter.y.</summary>
        public float ParameterY
        {
            get => Parameter.y;
            set => Parameter = new Vector2(Parameter.x, value);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Called whenever the thresholds are changed. Indicates that the internal blend factors need to be
        /// recalculated and calls <see cref="ForceRecalculateWeights"/>.
        /// </summary>
        public override void OnThresholdsChanged()
        {
            _BlendFactorsDirty = true;
            base.OnThresholdsChanged();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Recalculates the weights of all <see cref="ManualMixerState._States"/> based on the current value of the
        /// <see cref="MixerState{TParameter}.Parameter"/> and the <see cref="MixerState{TParameter}._Thresholds"/>.
        /// </summary>
        protected override void ForceRecalculateWeights()
        {
            WeightsAreDirty = false;

            CalculateBlendFactors();

            var childCount = ChildCount;

            float totalWeight = 0;

            for (int i = 0; i < childCount; i++)
            {
                var state = GetChild(i);
                if (state == null)
                    continue;

                var blendFactors = _BlendFactors[i];

                var threshold = GetThreshold(i);
                var thresholdToParameter = Parameter - threshold;

                float weight = 1;

                for (int j = 0; j < childCount; j++)
                {
                    if (j == i || GetChild(j) == null)
                        continue;

                    var newWeight = 1 - Vector2.Dot(thresholdToParameter, blendFactors[j]);

                    if (weight > newWeight)
                        weight = newWeight;
                }

                if (weight < 0.01f)
                    weight = 0;

                state.Weight = weight;
                totalWeight += weight;
            }

            NormalizeWeights(totalWeight);
        }

        /************************************************************************************************************************/

        private void CalculateBlendFactors()
        {
            if (!_BlendFactorsDirty)
                return;

            _BlendFactorsDirty = false;

            var childCount = ChildCount;
            if (childCount <= 1)
                return;

            // Resize the precalculated values.
            if (_BlendFactors == null || _BlendFactors.Length != childCount)
            {
                _BlendFactors = new Vector2[childCount][];
                for (int i = 0; i < childCount; i++)
                    _BlendFactors[i] = new Vector2[childCount];
            }

            // Calculate the blend factors between each combination of thresholds.
            for (int i = 0; i < childCount; i++)
            {
                var blendFactors = _BlendFactors[i];

                var thresholdI = GetThreshold(i);

                var j = i + 1;
                for (; j < childCount; j++)
                {
                    var thresholdIToJ = GetThreshold(j) - thresholdI;

                    thresholdIToJ *= 1f / thresholdIToJ.sqrMagnitude;

                    // Each factor is used in [i][j] with it's opposite in [j][i].
                    blendFactors[j] = thresholdIToJ;
                    _BlendFactors[j][i] = -thresholdIToJ;
                }
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void AppendParameter(StringBuilder text, Vector2 parameter)
        {
            text.Append('(')
                .Append(parameter.x)
                .Append(", ")
                .Append(parameter.y)
                .Append(')');
        }

        /************************************************************************************************************************/
        #region Inspector
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override int ParameterCount => 2;

        /// <inheritdoc/>
        protected override string GetParameterName(int index)
        {
            switch (index)
            {
                case 0: return "Parameter X";
                case 1: return "Parameter Y";
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <inheritdoc/>
        protected override AnimatorControllerParameterType GetParameterType(int index) => AnimatorControllerParameterType.Float;

        /// <inheritdoc/>
        protected override object GetParameterValue(int index)
        {
            switch (index)
            {
                case 0: return ParameterX;
                case 1: return ParameterY;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <inheritdoc/>
        protected override void SetParameterValue(int index, object value)
        {
            switch (index)
            {
                case 0: ParameterX = (float)value; break;
                case 1: ParameterY = (float)value; break;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

