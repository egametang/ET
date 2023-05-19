// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerPlayable
    /// 
    partial class AnimancerPlayable
    {
        /// <summary>A list of <see cref="AnimancerLayer"/>s with methods to control their mixing and masking.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/layers">Layers</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/LayerList
        /// 
        public sealed class LayerList : IEnumerable<AnimancerLayer>, IAnimationClipCollection
        {
            /************************************************************************************************************************/
            #region Fields
            /************************************************************************************************************************/

            /// <summary>The <see cref="AnimancerPlayable"/> at the root of the graph.</summary>
            private readonly AnimancerPlayable Root;

            /// <summary>[Internal] The layers which each manage their own set of animations.</summary>
            internal AnimancerLayer[] _Layers;

            /// <summary>The <see cref="AnimationLayerMixerPlayable"/> which blends the layers.</summary>
            private readonly AnimationLayerMixerPlayable LayerMixer;

            /// <summary>The number of layers that have actually been created.</summary>
            private int _Count;

            /************************************************************************************************************************/

            /// <summary>[Internal] Creates a new <see cref="LayerList"/>.</summary>
            internal LayerList(AnimancerPlayable root, out Playable layerMixer)
            {
                Root = root;
                layerMixer = LayerMixer = AnimationLayerMixerPlayable.Create(root._Graph, 1);
                Root._Graph.Connect(layerMixer, 0, Root._RootPlayable, 0);
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region List Operations
            /************************************************************************************************************************/

            /// <summary>[Pro-Only] The number of layers in this list.</summary>
            /// <exception cref="ArgumentOutOfRangeException">
            /// The value is set higher than the <see cref="DefaultCapacity"/>. This is simply a safety measure,
            /// so if you do actually need more layers you can just increase the limit.
            /// </exception>
            /// <exception cref="IndexOutOfRangeException">The value is set to a negative number.</exception>
            public int Count
            {
                get => _Count;
                set
                {
                    var count = _Count;

                    if (value == count)
                        return;

                    CheckAgain:

                    if (value > count)// Increasing.
                    {
                        Add();
                        count++;
                        goto CheckAgain;
                    }
                    else// Decreasing.
                    {
                        if (_Layers != null)
                        {
                            while (value < count--)
                            {
                                var layer = _Layers[count];
                                if (layer._Playable.IsValid())
                                    Root._Graph.DestroySubgraph(layer._Playable);
                                layer.DestroyStates();
                            }

                            Array.Clear(_Layers, value, _Count - value);
                        }

                        _Count = value;

                        Root._LayerMixer.SetInputCount(value);
                    }
                }
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// If the <see cref="Count"/> is below the specified `min`, this method increases it to that value.
            /// </summary>
            public void SetMinCount(int min)
            {
                if (Count < min)
                    Count = min;
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// The maximum number of layers that can be created before an <see cref="ArgumentOutOfRangeException"/> will
            /// be thrown (default 4).
            /// <para></para>
            /// Lowering this value will not affect layers that have already been created.
            /// </summary>
            /// <example>
            /// To set this value automatically when the application starts, place the following method in any class:
            /// <code>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            /// private static void SetMaxLayerCount()
            /// {
            ///     Animancer.AnimancerPlayable.LayerList.defaultCapacity = 8;
            /// }</code>
            /// Otherwise you can set the <see cref="Capacity"/> of each individual list:
            /// <code>AnimancerComponent animancer;
            /// animancer.Layers.Capacity = 8;</code>
            /// </example>
            public static int DefaultCapacity { get; set; } = 4;

            /// <summary>[Pro-Only]
            /// If the <see cref="DefaultCapacity"/> is below the specified `min`, this method increases it to that value.
            /// </summary>
            public static void SetMinDefaultCapacity(int min)
            {
                if (DefaultCapacity < min)
                    DefaultCapacity = min;
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// The maximum number of layers that can be created before an <see cref="ArgumentOutOfRangeException"/> will
            /// be thrown. The initial capacity is determined by <see cref="DefaultCapacity"/>.
            /// <para></para>
            /// Lowering this value will destroy any layers beyond the specified value.
            /// <para></para>
            /// Any changes to this value after a layer has been created will cause the allocation of a new array and
            /// garbage collection of the old one, so you should generally set it during initialisation.
            /// </summary>
            /// <exception cref="ArgumentOutOfRangeException">The value is not greater than 0.</exception>
            public int Capacity
            {
                get => _Layers != null ? _Layers.Length : DefaultCapacity;
                set
                {
                    if (value <= 0)
                        throw new ArgumentOutOfRangeException(nameof(value), "must be greater than 0 (" + value + " <= 0)");

                    if (value < _Count)
                        Count = value;

                    Array.Resize(ref _Layers, value);
                }
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Creates and returns a new <see cref="AnimancerLayer"/>. New layers will override earlier layers by default.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// The value is set higher than the <see cref="Capacity"/>. This is simply a safety measure,
            /// so if you do actually need more layers you can just increase the limit.
            /// </exception>
            public AnimancerLayer Add()
            {
                if (_Layers == null)
                    _Layers = new AnimancerLayer[DefaultCapacity];

                var index = _Count;

                if (index >= _Layers.Length)
                    throw new InvalidOperationException(
                        "Attempted to increase the layer count above the current capacity (" +
                        (index + 1) + " > " + _Layers.Length + "). This is simply a safety measure," +
                        " so if you do actually need more layers you can just increase the " +
                        $"{nameof(Capacity)} or {nameof(DefaultCapacity)}.");

                _Count = index + 1;
                Root._LayerMixer.SetInputCount(_Count);

                var layer = new AnimancerLayer(Root, index);
                _Layers[index] = layer;
                return layer;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Returns the layer at the specified index. If it didn't already exist, this method creates it.
            /// </summary>
            public AnimancerLayer this[int index]
            {
                get
                {
                    SetMinCount(index + 1);
                    return _Layers[index];
                }
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Enumeration
            /************************************************************************************************************************/

            /// <summary>Returns an enumerator that will iterate through all layers.</summary>
            public IEnumerator<AnimancerLayer> GetEnumerator()
            {
                if (_Layers == null)
                    _Layers = new AnimancerLayer[DefaultCapacity];

                return ((IEnumerable<AnimancerLayer>)_Layers).GetEnumerator();
            }

            /// <summary>Returns an enumerator that will iterate through all layers.</summary>
            IEnumerator IEnumerable.GetEnumerator()
            {
                if (_Layers == null)
                    _Layers = new AnimancerLayer[DefaultCapacity];

                return _Layers.GetEnumerator();
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Returns an enumerator that will iterate through all states in each layer (not states inside mixers).
            /// </summary>
            public IEnumerable<AnimancerState> GetAllStateEnumerable()
            {
                var count = Count;
                for (int i = 0; i < count; i++)
                {
                    foreach (var state in _Layers[i])
                    {
                        yield return state;
                    }
                }
            }

            /************************************************************************************************************************/

            /// <summary>[<see cref="IAnimationClipCollection"/>]
            /// Gathers all the animations in all layers.
            /// </summary>
            public void GatherAnimationClips(ICollection<AnimationClip> clips) => clips.GatherFromSources(_Layers);

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Layer Details
            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Checks whether the layer at the specified index is set to additive blending. Otherwise it will override any
            /// earlier layers.
            /// </summary>
            public bool IsAdditive(int index)
            {
                return LayerMixer.IsLayerAdditive((uint)index);
            }

            /// <summary>[Pro-Only]
            /// Sets the layer at the specified index to blend additively with earlier layers (if true) or to override them
            /// (if false). Newly created layers will override by default.
            /// </summary>
            public void SetAdditive(int index, bool value)
            {
                SetMinCount(index + 1);
                LayerMixer.SetLayerAdditive((uint)index, value);
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Sets an <see cref="AvatarMask"/> to determine which bones the layer at the specified index will affect.
            /// </summary>
            public void SetMask(int index, AvatarMask mask)
            {
                SetMinCount(index + 1);

#if UNITY_ASSERTIONS
                _Layers[index]._Mask = mask;
#endif

                AnimancerUtilities.NewIfNull(ref mask);

                LayerMixer.SetLayerMaskFromAvatarMask((uint)index, mask);
            }

            /************************************************************************************************************************/

            /// <summary>[Editor-Conditional]
            /// Sets the Inspector display name of the layer at the specified index. Note that layer names are Editor-Only
            /// so any calls to this method will automatically be compiled out of runtime builds.
            /// </summary>
            [System.Diagnostics.Conditional(Strings.UnityEditor)]
            public void SetName(int index, string name) => this[index].SetDebugName(name);

            /************************************************************************************************************************/

            /// <summary>
            /// The average velocity of the root motion of all currently playing animations, taking their current
            /// <see cref="AnimancerNode.Weight"/> into account.
            /// </summary>
            public Vector3 AverageVelocity
            {
                get
                {
                    var velocity = default(Vector3);

                    for (int i = 0; i < _Count; i++)
                    {
                        var layer = _Layers[i];
                        velocity += layer.AverageVelocity * layer.Weight;
                    }

                    return velocity;
                }
            }

            /************************************************************************************************************************/

            /// <summary>[Internal]
            /// Connects or disconnects all children from their parent <see cref="Playable"/>.
            /// </summary>
            internal void SetWeightlessChildrenConnected(bool connected)
            {
                if (_Layers == null)
                    return;

                if (connected)
                {
                    for (int i = _Count - 1; i >= 0; i--)
                        _Layers[i].ConnectAllChildrenToGraph();
                }
                else
                {
                    for (int i = _Count - 1; i >= 0; i--)
                        _Layers[i].DisconnectWeightlessChildrenFromGraph();
                }
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
        }
    }
}

