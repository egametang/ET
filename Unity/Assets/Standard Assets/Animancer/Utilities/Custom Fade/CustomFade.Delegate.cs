// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/CustomFade
    /// 
    public partial class CustomFade
    {
        /************************************************************************************************************************/

        /// <summary>Modify the current fade to use the specified `calculateWeight` delegate.</summary>
        /// <example>See <see cref="CustomFade"/>.</example>
        /// <remarks>The `calculateWeight` should follow the <see cref="OptionalWarning.CustomFadeBounds"/> guideline.</remarks>
        public static void Apply(AnimancerPlayable animancer, Func<float, float> calculateWeight)
            => Delegate.Acquire(calculateWeight).Apply(animancer);

        /// <summary>Modify the current fade to use the specified `calculateWeight` delegate.</summary>
        /// <example>See <see cref="CustomFade"/>.</example>
        /// <remarks>The `calculateWeight` should follow the <see cref="OptionalWarning.CustomFadeBounds"/> guideline.</remarks>
        public static void Apply(AnimancerState state, Func<float, float> calculateWeight)
            => Delegate.Acquire(calculateWeight).Apply(state);

        /************************************************************************************************************************/

        /// <summary>Modify the current fade to use the specified `function` to calculate the weight.</summary>
        /// <example>See <see cref="CustomFade"/>.</example>
        public static void Apply(AnimancerPlayable animancer, Easing.Function function)
            => Delegate.Acquire(function.GetDelegate()).Apply(animancer);

        /// <summary>Modify the current fade to use the specified `function` to calculate the weight.</summary>
        /// <example>See <see cref="CustomFade"/>.</example>
        public static void Apply(AnimancerState state, Easing.Function function)
            => Delegate.Acquire(function.GetDelegate()).Apply(state);

        /************************************************************************************************************************/

        /// <summary>A <see cref="CustomFade"/> which uses a delegate to calculate the weight.</summary>
        private sealed class Delegate : CustomFade
        {
            /************************************************************************************************************************/

            private Func<float, float> _CalculateWeight;

            /************************************************************************************************************************/

            public static Delegate Acquire(Func<float, float> calculateWeight)
            {
                if (calculateWeight == null)
                {
                    OptionalWarning.CustomFadeNotNull.Log($"{nameof(calculateWeight)} is null.");
                    return null;
                }

                var fade = ObjectPool<Delegate>.Acquire();
                fade._CalculateWeight = calculateWeight;
                return fade;
            }

            /************************************************************************************************************************/

            protected override float CalculateWeight(float progress) => _CalculateWeight(progress);

            /************************************************************************************************************************/

            protected override void Release() => ObjectPool<Delegate>.Release(this);

            /************************************************************************************************************************/
        }
    }
}
