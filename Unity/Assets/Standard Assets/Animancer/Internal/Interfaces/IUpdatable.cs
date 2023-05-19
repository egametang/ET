// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using UnityEngine;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>An object that can be updated during Animancer's <see cref="PlayableBehaviour.PrepareFrame"/>.</summary>
    ///
    /// <example>
    /// Register to receive updates using <see cref="AnimancerPlayable.RequireUpdate(IUpdatable)"/> and stop
    /// receiving updates using <see cref="AnimancerPlayable.CancelUpdate(IUpdatable)"/>.
    /// <para></para><code>
    /// public sealed class MyUpdatable : Key, IUpdatable
    /// {
    ///     private AnimancerComponent _Animancer;
    ///
    ///     public void StartUpdating(AnimancerComponent animancer)
    ///     {
    ///         _Animancer = animancer;
    ///         _Animancer.Playable.RequireUpdate(this);
    ///     }
    ///
    ///     public void StopUpdating()
    ///     {
    ///         _Animancer.Playable.CancelUpdate(this);
    ///     }
    ///
    ///     void IUpdatable.EarlyUpdate()
    ///     {
    ///         // Called at the start of every animation update before the playables get updated.
    ///     }
    ///
    ///     void IUpdatable.LateUpdate()
    ///     {
    ///         // Called at the end of every animation update after the playables get updated.
    ///     }
    ///
    ///     void IUpdatable.OnDestroy()
    ///     {
    ///         // Called by AnimancerPlayable.Destroy if this object is currently being updated.
    ///     }
    /// }
    /// </code></example>
    /// https://kybernetik.com.au/animancer/api/Animancer/IUpdatable
    /// 
    public interface IUpdatable : IKeyedListItem
    {
        /************************************************************************************************************************/

        /// <summary>Called at the start of every <see cref="Animator"/> update before the playables get updated.</summary>
        /// <remarks>The <see cref="Animator.updateMode"/> determines when it updates.</remarks>
        void EarlyUpdate();

        /// <summary>Called at the end of every <see cref="Animator"/> update after the playables get updated.</summary>
        /// <remarks>
        /// The <see cref="Animator.updateMode"/> determines when it updates.
        /// This method has nothing to do with <see cref="MonoBehaviour"/>.LateUpdate().
        /// </remarks>
        void LateUpdate();

        /// <summary>Called by <see cref="AnimancerPlayable.Destroy"/> if this object is currently being updated.</summary>
        void OnDestroy();

        /************************************************************************************************************************/
    }
}

