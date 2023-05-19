// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using Unity.Collections;

namespace Animancer
{
    /// <summary>[Pro-Only]
    /// A base wrapper which allows access to the value of properties that are controlled by animations.
    /// </summary>
    /// <example>
    /// Example: <see href="https://kybernetik.com.au/animancer/docs/examples/jobs">Animation Jobs</see>
    /// </example>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimatedProperty_2
    /// 
    public abstract class AnimatedProperty<TJob, TValue> : AnimancerJob<TJob>, IDisposable
        where TJob : struct, IAnimationJob
        where TValue : struct
    {
        /************************************************************************************************************************/

        /// <summary>The properties wrapped by this object.</summary>
        protected NativeArray<PropertyStreamHandle> _Properties;

        /// <summary>The value of each of the <see cref="_Properties"/> from the most recent update.</summary>
        protected NativeArray<TValue> _Values;

        /************************************************************************************************************************/
        #region Initialisation
        /************************************************************************************************************************/

        /// <summary>
        /// Allocates room for a specified number of properties to be filled by
        /// <see cref="InitialiseProperty(int, Transform, Type, string)"/>.
        /// </summary>
        public AnimatedProperty(IAnimancerComponent animancer, int propertyCount,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            _Properties = new NativeArray<PropertyStreamHandle>(propertyCount, Allocator.Persistent, options);
            _Values = new NativeArray<TValue>(propertyCount, Allocator.Persistent);
            CreateJob();

            var playable = animancer.Playable;
            CreatePlayable(playable);
            playable.Disposables.Add(this);
        }

        /// <summary>Initialises a single property.</summary>
        public AnimatedProperty(IAnimancerComponent animancer, string propertyName)
            : this(animancer, 1, NativeArrayOptions.UninitializedMemory)
        {
            var animator = animancer.Animator;
            _Properties[0] = animator.BindStreamProperty(animator.transform, typeof(Animator), propertyName);
        }

        /// <summary>Initialises a group of properties.</summary>
        public AnimatedProperty(IAnimancerComponent animancer, params string[] propertyNames)
            : this(animancer, propertyNames.Length, NativeArrayOptions.UninitializedMemory)
        {
            var count = propertyNames.Length;

            var animator = animancer.Animator;
            var transform = animator.transform;
            for (int i = 0; i < count; i++)
                InitialiseProperty(animator, i, transform, typeof(Animator), propertyNames[i]);
        }

        /************************************************************************************************************************/

        /// <summary>Initialises a property on the target <see cref="Animator"/>.</summary>
        public void InitialiseProperty(Animator animator, int index, string name)
            => InitialiseProperty(animator, index, animator.transform, typeof(Animator), name);

        /// <summary>Initialises the specified `index` to read a property with the specified `name`.</summary>
        public void InitialiseProperty(Animator animator, int index, Transform transform, Type type, string name)
            => _Properties[index] = animator.BindStreamProperty(transform, type, name);

        /************************************************************************************************************************/

        /// <summary>Creates and assigns the <see cref="AnimancerJob._Job"/>.</summary>
        protected abstract void CreateJob();

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Accessors
        /************************************************************************************************************************/

        /// <summary>Returns the value of the first property.</summary>
        public TValue Value => this[0];

        /// <summary>Returns the value of the first property.</summary>
        public static implicit operator TValue(AnimatedProperty<TJob, TValue> properties) => properties[0];

        /************************************************************************************************************************/

        /// <summary>Returns the value of the property at the specified `index`.</summary>
        /// <remarks>This method is identical to <see cref="this[int]"/>.</remarks>
        public TValue GetValue(int index) => _Values[index];

        /// <summary>Returns the value of the property at the specified `index`.</summary>
        /// <remarks>This indexer is identical to <see cref="GetValue(int)"/>.</remarks>
        public TValue this[int index] => _Values[index];

        /************************************************************************************************************************/

        /// <summary>Resizes the `values` if necessary and copies the value of each property into it.</summary>
        public void GetValues(ref TValue[] values)
        {
            var count = _Values.Length;
            if (values == null || values.Length != count)
                values = new TValue[count];

            _Values.CopyTo(values);
        }

        /// <summary>
        /// Returns a new array containing the values of all properties.
        /// <para></para>
        /// Use <see cref="GetValues(ref TValue[])"/> to avoid allocating a new array every call.
        /// </summary>
        public TValue[] GetValues()
        {
            var values = new TValue[_Values.Length];
            _Values.CopyTo(values);
            return values;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        void IDisposable.Dispose() => Dispose();

        /// <summary>Cleans up the <see cref="NativeArray{T}"/>s.</summary>
        /// <remarks>Called by <see cref="AnimancerPlayable.OnPlayableDestroy"/>.</remarks>
        protected virtual void Dispose()
        {
            if (_Properties.IsCreated)
            {
                _Properties.Dispose();
                _Values.Dispose();
            }
        }

        /// <summary>Destroys the <see cref="_Playable"/> and restores the graph connection it was intercepting.</summary>
        public override void Destroy()
        {
            Dispose();
            base.Destroy();
        }

        /************************************************************************************************************************/
    }
}
