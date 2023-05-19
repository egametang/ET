// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System.Collections.Generic;
using UnityEngine;

namespace Animancer
{
    /// <summary>A system for synchronising the <see cref="AnimancerState.NormalizedTime"/> of certain animations.</summary>
    /// <example>
    /// <list type="number">
    /// <item>Initialise a <see cref="TimeSynchronisationGroup"/> by adding any objects you want to synchronise.</item>
    /// <item>Call any of the <see cref="StoreTime(object)"/> methods before playing a new animation.</item>
    /// <item>Call any of the <see cref="SyncTime(object)"/> methods after playing that animation.</item>
    /// </list>
    /// Example: <see href="https://kybernetik.com.au/animancer/docs/examples/directional-sprites/character-controller#synchronisation">Character Controller -> Synchronisation</see>
    /// </example>
    /// https://kybernetik.com.au/animancer/api/Animancer/TimeSynchronisationGroup
    /// 
    public class TimeSynchronisationGroup : HashSet<object>
    {
        /************************************************************************************************************************/

        private AnimancerComponent _Animancer;

        /// <summary>The <see cref="AnimancerComponent"/> this group is synchronising.</summary>
        /// <remarks>
        /// This reference is not required if you always use the store and sync methods that take an
        /// <see cref="AnimancerState"/>.
        /// </remarks>
        public AnimancerComponent Animancer
        {
            get => _Animancer;
            set
            {
                _Animancer = value;
                NormalizedTime = null;
            }
        }

        /************************************************************************************************************************/

        /// <summary>The stored <see cref="AnimancerState.NormalizedTime"/> or <c>null</c> if no value was stored.</summary>
        public float? NormalizedTime { get; set; }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="TimeSynchronisationGroup"/> and sets its <see cref="Animancer"/>.</summary>
        public TimeSynchronisationGroup(AnimancerComponent animancer) => Animancer = animancer;

        /************************************************************************************************************************/

        /// <summary>
        /// Stores the <see cref="AnimancerState.NormalizedTime"/> of the <see cref="Animancer"/>'s current state if
        /// the `key` is in this group.
        /// </summary>
        public bool StoreTime(object key) => StoreTime(key, Animancer.States.Current);

        /// <summary>
        /// Stores the <see cref="AnimancerState.NormalizedTime"/> of the `state` if the `key` is in this group.
        /// </summary>
        public bool StoreTime(object key, AnimancerState state)
        {
            if (state != null && Contains(key))
            {
                NormalizedTime = state.NormalizedTime;
                return true;
            }
            else
            {
                NormalizedTime = null;
                return false;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Applies the <see cref="NormalizedTime"/> to the <see cref="Animancer"/>'s current state if the `key` is in
        /// this group.
        /// </summary>
        public bool SyncTime(object key) => SyncTime(key, Time.deltaTime);

        /// <summary>
        /// Applies the <see cref="NormalizedTime"/> to the <see cref="Animancer"/>'s current state if the `key` is in
        /// this group.
        /// </summary>
        public bool SyncTime(object key, float deltaTime) => SyncTime(key, Animancer.States.Current, deltaTime);

        /// <summary>Applies the <see cref="NormalizedTime"/> to the `state` if the `key` is in this group.</summary>
        public bool SyncTime(object key, AnimancerState state) => SyncTime(key, state, Time.deltaTime);

        /// <summary>Applies the <see cref="NormalizedTime"/> to the `state` if the `key` is in this group.</summary>
        public bool SyncTime(object key, AnimancerState state, float deltaTime)
        {
            if (NormalizedTime == null ||
                state == null ||
                !Contains(key))
                return false;

            // Setting the Time forces it to stay at that value after the next animation update.
            // But we actually want it to keep playing, so we need to add deltaTime manually.
            state.Time = NormalizedTime.Value * state.Length + deltaTime * state.EffectiveSpeed;
            return true;
        }

        /************************************************************************************************************************/
    }
}
