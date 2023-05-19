// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerPlayable
    /// 
    partial class AnimancerPlayable
    {
        /// <summary>A dictionary of <see cref="AnimancerState"/>s mapped to their <see cref="AnimancerState.Key"/>.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/states">States</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/StateDictionary
        /// 
        public sealed class StateDictionary : IEnumerable<KeyValuePair<object, AnimancerState>>, IAnimationClipCollection
        {
            /************************************************************************************************************************/

            /// <summary>The <see cref="AnimancerPlayable"/> at the root of the graph.</summary>
            private readonly AnimancerPlayable Root;

            /************************************************************************************************************************/

            /// <summary>
            /// Determines the <see cref="IEqualityComparer{T}"/> used by every new <see cref="StateDictionary"/> when
            /// it is created. Changing this value will not affect existing instances.
            /// <list type="bullet">
            /// <item>The default is false, which will use a <see cref="FastComparer"/>.</item>
            /// <item>Setting it to true will use a <see cref="FastReferenceComparer"/>, which is faster but does not
            /// work for value types such as enums because it uses <see cref="object.ReferenceEquals"/>.</item>
            /// </list>
            /// </summary>
            public static bool ReferenceKeysOnly { get; set; }

            /// <summary><see cref="AnimancerState.Key"/> mapped to <see cref="AnimancerState"/>.</summary>
            private readonly Dictionary<object, AnimancerState>
                States = new Dictionary<object, AnimancerState>(
                    ReferenceKeysOnly ? (IEqualityComparer<object>)FastReferenceComparer.Instance : FastComparer.Instance);

            /************************************************************************************************************************/

            /// <summary>[Internal] Creates a new <see cref="StateDictionary"/>.</summary>
            internal StateDictionary(AnimancerPlayable root) => Root = root;

            /************************************************************************************************************************/

            /// <summary>The number of states that have been registered with a <see cref="AnimancerState.Key"/>.</summary>
            public int Count => States.Count;

            /************************************************************************************************************************/
            #region Create
            /************************************************************************************************************************/

            /// <summary>
            /// Creates and returns a new <see cref="ClipState"/> to play the `clip`.
            /// <para></para>
            /// To create a state on a different layer, call <c>animancer.Layers[x].CreateState(clip)</c> instead.
            /// </summary>
            /// <remarks>
            /// <see cref="GetKey"/> is used to determine the <see cref="AnimancerState.Key"/>.
            /// </remarks>
            public ClipState Create(AnimationClip clip) => Root.Layers[0].CreateState(clip);

            /// <summary>
            /// Creates and returns a new <see cref="ClipState"/> to play the `clip` and registers it with the `key`.
            /// <para></para>
            /// To create a state on a different layer, call <c>animancer.Layers[x].CreateState(key, clip)</c> instead.
            /// </summary>
            public ClipState Create(object key, AnimationClip clip) => Root.Layers[0].CreateState(key, clip);

            /// <summary>
            /// Creates and returns a new <typeparamref name="T"/>.
            /// <para></para>
            /// To create it on a different layer, call <c>animancer.Layers[x].CreateState&lt;T&gt;()</c> instead.
            /// </summary>
            public T Create<T>() where T : AnimancerState, new() => Root.Layers[0].CreateState<T>();

            /************************************************************************************************************************/

            /// <summary>
            /// Calls <see cref="GetOrCreate(AnimationClip, bool)"/> for each of the specified clips.
            /// <para></para>
            /// If you only want to create a single state, use <see cref="AnimancerLayer.CreateState(AnimationClip)"/>.
            /// </summary>
            public void CreateIfNew(AnimationClip clip0, AnimationClip clip1)
            {
                GetOrCreate(clip0);
                GetOrCreate(clip1);
            }

            /// <summary>
            /// Calls <see cref="GetOrCreate(AnimationClip, bool)"/> for each of the specified clips.
            /// <para></para>
            /// If you only want to create a single state, use <see cref="AnimancerLayer.CreateState(AnimationClip)"/>.
            /// </summary>
            public void CreateIfNew(AnimationClip clip0, AnimationClip clip1, AnimationClip clip2)
            {
                GetOrCreate(clip0);
                GetOrCreate(clip1);
                GetOrCreate(clip2);
            }

            /// <summary>
            /// Calls <see cref="GetOrCreate(AnimationClip, bool)"/> for each of the specified clips.
            /// <para></para>
            /// If you only want to create a single state, use <see cref="AnimancerLayer.CreateState(AnimationClip)"/>.
            /// </summary>
            public void CreateIfNew(AnimationClip clip0, AnimationClip clip1, AnimationClip clip2, AnimationClip clip3)
            {
                GetOrCreate(clip0);
                GetOrCreate(clip1);
                GetOrCreate(clip2);
                GetOrCreate(clip3);
            }

            /// <summary>
            /// Calls <see cref="GetOrCreate(AnimationClip, bool)"/> for each of the specified `clips`.
            /// <para></para>
            /// If you only want to create a single state, use <see cref="AnimancerLayer.CreateState(AnimationClip)"/>.
            /// </summary>
            public void CreateIfNew(params AnimationClip[] clips)
            {
                if (clips == null)
                    return;

                var count = clips.Length;
                for (int i = 0; i < count; i++)
                {
                    var clip = clips[i];
                    if (clip != null)
                        GetOrCreate(clip);
                }
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Access
            /************************************************************************************************************************/

            /// <summary>
            /// The <see cref="AnimancerLayer.CurrentState"/> on layer 0.
            /// <para></para>
            /// Specifically, this is the state that was most recently started using any of the Play methods on that layer.
            /// States controlled individually via methods in the <see cref="AnimancerState"/> itself will not register in
            /// this property.
            /// </summary>
            public AnimancerState Current => Root.Layers[0].CurrentState;

            /************************************************************************************************************************/

            /// <summary>Calls <see cref="GetKey"/> then returns the state registered with that key.</summary>
            /// <exception cref="ArgumentNullException">The key is null.</exception>
            /// <exception cref="KeyNotFoundException">No state is registered with the key.</exception>
            public AnimancerState this[AnimationClip clip] => States[Root.GetKey(clip)];

            /// <summary>Returns the state registered with the <see cref="IHasKey.Key"/>.</summary>
            /// <exception cref="ArgumentNullException">The `key` is null.</exception>
            /// <exception cref="KeyNotFoundException">No state is registered with the `key`.</exception>
            public AnimancerState this[IHasKey hasKey] => States[hasKey.Key];

            /// <summary>Returns the state registered with the `key`.</summary>
            /// <exception cref="ArgumentNullException">The `key` is null.</exception>
            /// <exception cref="KeyNotFoundException">No state is registered with the `key`.</exception>
            public AnimancerState this[object key] => States[key];

            /************************************************************************************************************************/

            /// <summary>
            /// Calls <see cref="GetKey"/> then passes the key to
            /// <see cref="TryGet(object, out AnimancerState)"/> and returns the result.
            /// </summary>
            public bool TryGet(AnimationClip clip, out AnimancerState state)
            {
                if (clip == null)
                {
                    state = null;
                    return false;
                }

                return States.TryGetValue(Root.GetKey(clip), out state);
            }

            /// <summary>
            /// Passes the <see cref="IHasKey.Key"/> into <see cref="TryGet(object, out AnimancerState)"/>
            /// and returns the result.
            /// </summary>
            public bool TryGet(IHasKey hasKey, out AnimancerState state)
            {
                if (hasKey == null)
                {
                    state = null;
                    return false;
                }

                return States.TryGetValue(hasKey.Key, out state);
            }

            /// <summary>
            /// If a state is registered with the `key`, this method outputs it as the `state` and returns true. Otherwise
            /// `state` is set to null and this method returns false.
            /// </summary>
            public bool TryGet(object key, out AnimancerState state) => States.TryGetValue(key, out state);

            /************************************************************************************************************************/

            /// <summary>
            /// Calls <see cref="GetKey"/> and returns the state which registered with that key or creates one if it
            /// doesn't exist.
            /// <para></para>
            /// If the state already exists but has the wrong <see cref="AnimancerState.Clip"/>, the `allowSetClip`
            /// parameter determines what will happen. False causes it to throw an <see cref="ArgumentException"/> while
            /// true allows it to change the <see cref="AnimancerState.Clip"/>. Note that the change is somewhat costly to
            /// performance to use with caution.
            /// </summary>
            /// <exception cref="ArgumentException"/>
            public AnimancerState GetOrCreate(AnimationClip clip, bool allowSetClip = false)
                => GetOrCreate(Root.GetKey(clip), clip, allowSetClip);

            /// <summary>
            /// Returns the state registered with the `transition`s <see cref="IHasKey.Key"/> if there is one. Otherwise
            /// this method uses <see cref="ITransition.CreateState"/> to create a new one and registers it with
            /// that key before returning it.
            /// </summary>
            public AnimancerState GetOrCreate(ITransition transition)
            {
                var key = transition.Key;

                if (!States.TryGetValue(key, out var state))
                {
                    state = transition.CreateState();
                    state.SetRoot(Root);
                    Register(key, state);
                }

                return state;
            }

            /// <summary>
            /// Returns the state which registered with the `key` or creates one if it doesn't exist.
            /// <para></para>
            /// If the state already exists but has the wrong <see cref="AnimancerState.Clip"/>, the `allowSetClip`
            /// parameter determines what will happen. False causes it to throw an <see cref="ArgumentException"/> while
            /// true allows it to change the <see cref="AnimancerState.Clip"/>. Note that the change is somewhat costly to
            /// performance to use with caution.
            /// </summary>
            /// <exception cref="ArgumentException"/>
            /// <exception cref="ArgumentNullException">The `key` is null.</exception>
            /// <remarks>See also: <see cref="AnimancerLayer.GetOrCreateState(object, AnimationClip, bool)"/></remarks>
            public AnimancerState GetOrCreate(object key, AnimationClip clip, bool allowSetClip = false)
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                if (States.TryGetValue(key, out var state))
                {
                    // If a state exists with the 'key' but has the wrong clip, either change it or complain.
                    if (!ReferenceEquals(state.Clip, clip))
                    {
                        if (allowSetClip)
                        {
                            state.Clip = clip;
                        }
                        else
                        {
                            throw new ArgumentException(GetClipMismatchError(key, state.Clip, clip));
                        }
                    }
                }
                else
                {
                    state = Root.Layers[0].CreateState(key, clip);
                }

                return state;
            }

            /************************************************************************************************************************/

            /// <summary>Returns an error message explaining that a state already exists with the specified `key`.</summary>
            public static string GetClipMismatchError(object key, AnimationClip oldClip, AnimationClip newClip)
                => $"A state already exists using the specified '{nameof(key)}', but has a different {nameof(AnimationClip)}:" +
                $"\n - Key: {key}" +
                $"\n - Old Clip: {oldClip}" +
                $"\n - New Clip: {newClip}";

            /************************************************************************************************************************/

            /// <summary>[Internal]
            /// Registers the `state` in this dictionary so the `key` can be used to get it later on using
            /// <see cref="this[object]"/>.
            /// </summary>
            internal void Register(object key, AnimancerState state)
            {
                if (key != null)
                {
#if UNITY_ASSERTIONS
                    if (state.Root != Root)
                        throw new ArgumentException(
                            $"{nameof(StateDictionary)} cannot register a state with a different {nameof(Root)}: " + state);
#endif

                    States.Add(key, state);
                }

                state._Key = key;
            }

            /// <summary>[Internal]
            /// Removes the `state` from this dictionary.
            /// </summary>
            internal void Unregister(AnimancerState state)
            {
                if (state._Key == null)
                    return;

                States.Remove(state._Key);
                state._Key = null;
            }

            /************************************************************************************************************************/
            #region Enumeration
            /************************************************************************************************************************/
            // IEnumerable for 'foreach' statements.
            /************************************************************************************************************************/

            /// <summary>
            /// Returns an enumerator that will iterate through all states in each layer (not states inside mixers).
            /// </summary>
            public IEnumerator<KeyValuePair<object, AnimancerState>> GetEnumerator() => States.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /************************************************************************************************************************/

            /// <summary>[<see cref="IAnimationClipCollection"/>]
            /// Gathers all the animations in all layers.
            /// </summary>
            public void GatherAnimationClips(ICollection<AnimationClip> clips)
            {
                foreach (var state in States.Values)
                    clips.GatherFromSource(state);
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Destroy
            /************************************************************************************************************************/

            /// <summary>
            /// Calls <see cref="AnimancerState.Destroy"/> on the state associated with the `clip` (if any).
            /// Returns true if the state existed.
            /// </summary>
            public bool Destroy(AnimationClip clip)
            {
                if (clip == null)
                    return false;

                return Destroy(Root.GetKey(clip));
            }

            /// <summary>
            /// Calls <see cref="AnimancerState.Destroy"/> on the state associated with the <see cref="IHasKey.Key"/>
            /// (if any). Returns true if the state existed.
            /// </summary>
            public bool Destroy(IHasKey hasKey)
            {
                if (hasKey == null)
                    return false;

                return Destroy(hasKey.Key);
            }

            /// <summary>
            /// Calls <see cref="AnimancerState.Destroy"/> on the state associated with the `key` (if any).
            /// Returns true if the state existed.
            /// </summary>
            public bool Destroy(object key)
            {
                if (key == null)
                    return false;

                if (States.TryGetValue(key, out var state))
                {
                    state.Destroy();
                    return true;
                }

                return false;
            }

            /************************************************************************************************************************/

            /// <summary>Calls <see cref="Destroy(AnimationClip)"/> on each of the `clips`.</summary>
            public void DestroyAll(IList<AnimationClip> clips)
            {
                if (clips == null)
                    return;

                for (int i = clips.Count - 1; i >= 0; i--)
                    Destroy(clips[i]);
            }

            /// <summary>Calls <see cref="Destroy(AnimationClip)"/> on each of the `clips`.</summary>
            public void DestroyAll(IEnumerable<AnimationClip> clips)
            {
                if (clips == null)
                    return;

                foreach (var clip in clips)
                    Destroy(clip);
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Calls <see cref="Destroy(AnimationClip)"/> on all states gathered by
            /// <see cref="IAnimationClipSource.GetAnimationClips"/>.
            /// </summary>
            public void DestroyAll(IAnimationClipSource source)
            {
                if (source == null)
                    return;

                var clips = ObjectPool.AcquireList<AnimationClip>();
                source.GetAnimationClips(clips);
                DestroyAll(clips);
                ObjectPool.Release(clips);
            }

            /// <summary>
            /// Calls <see cref="Destroy(AnimationClip)"/> on all states gathered by
            /// <see cref="IAnimationClipCollection.GatherAnimationClips"/>.
            /// </summary>
            public void DestroyAll(IAnimationClipCollection source)
            {
                if (source == null)
                    return;

                var clips = ObjectPool.AcquireSet<AnimationClip>();
                source.GatherAnimationClips(clips);
                DestroyAll(clips);
                ObjectPool.Release(clips);
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Destroys all states connected to all layers (regardless of whether they are actually registered in this
            /// dictionary).
            /// </summary>
            public void DestroyAll()
            {
                var count = Root.Layers.Count;
                while (--count >= 0)
                    Root.Layers._Layers[count].DestroyStates();

                States.Clear();
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Key Error Methods
#if UNITY_EDITOR
            /************************************************************************************************************************/
            // These are overloads of other methods that take a System.Object key to ensure the user doesn't try to use an
            // AnimancerState as a key, since the whole point of a key is to identify a state in the first place.
            /************************************************************************************************************************/

            /// <summary>[Warning]
            /// You should not use an <see cref="AnimancerState"/> as a key.
            /// The whole point of a key is to identify a state in the first place.
            /// </summary>
            [System.Obsolete("You should not use an AnimancerState as a key. The whole point of a key is to identify a state in the first place.", true)]
            public AnimancerState this[AnimancerState key] => key;

            /// <summary>[Warning]
            /// You should not use an <see cref="AnimancerState"/> as a key.
            /// The whole point of a key is to identify a state in the first place.
            /// </summary>
            [System.Obsolete("You should not use an AnimancerState as a key. The whole point of a key is to identify a state in the first place.", true)]
            public bool TryGet(AnimancerState key, out AnimancerState state)
            {
                state = key;
                return true;
            }

            /// <summary>[Warning]
            /// You should not use an <see cref="AnimancerState"/> as a key.
            /// The whole point of a key is to identify a state in the first place.
            /// </summary>
            [System.Obsolete("You should not use an AnimancerState as a key. The whole point of a key is to identify a state in the first place.", true)]
            public AnimancerState GetOrCreate(AnimancerState key, AnimationClip clip) => key;

            /// <summary>[Warning]
            /// You should not use an <see cref="AnimancerState"/> as a key.
            /// Just call <see cref="AnimancerState.Destroy"/>.
            /// </summary>
            [System.Obsolete("You should not use an AnimancerState as a key. Just call AnimancerState.Destroy.", true)]
            public bool Destroy(AnimancerState key)
            {
                key.Destroy();
                return true;
            }

            /************************************************************************************************************************/
#endif
            #endregion
            /************************************************************************************************************************/
        }
    }
}

