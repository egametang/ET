// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>[Pro-Only] An <see cref="AnimancerState"/> which plays a <see cref="PlayableAsset"/>.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/platformer">Platformer</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer/PlayableAssetState
    /// 
    public sealed class PlayableAssetState : AnimancerState
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        /// <summary>The <see cref="PlayableAsset"/> which this state plays.</summary>
        private PlayableAsset _Asset;

        /// <summary>The <see cref="PlayableAsset"/> which this state plays.</summary>
        public PlayableAsset Asset
        {
            get => _Asset;
            set => ChangeMainObject(ref _Asset, value);
        }

        /// <summary>The <see cref="PlayableAsset"/> which this state plays.</summary>
        public override Object MainObject
        {
            get => _Asset;
            set => _Asset = (PlayableAsset)value;
        }

        /************************************************************************************************************************/

        private float _Length;

        /// <summary>The <see cref="PlayableAsset.duration"/>.</summary>
        public override float Length => _Length;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnSetIsPlaying()
        {
            var inputCount = _Playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                var playable = _Playable.GetInput(i);
                if (!playable.IsValid())
                    continue;

                if (IsPlaying)
                    playable.Play();
                else
                    playable.Pause();
            }
        }

        /************************************************************************************************************************/

        /// <summary>IK cannot be dynamically enabled on a <see cref="PlayableAssetState"/>.</summary>
        public override void CopyIKFlags(AnimancerNode node) { }

        /************************************************************************************************************************/

        /// <summary>IK cannot be dynamically enabled on a <see cref="PlayableAssetState"/>.</summary>
        public override bool ApplyAnimatorIK
        {
            get => false;
            set
            {
#if UNITY_ASSERTIONS
                if (value)
                    OptionalWarning.UnsupportedIK.Log(
                        $"IK cannot be dynamically enabled on a {nameof(PlayableAssetState)}.", Root?.Component);
#endif
            }
        }

        /************************************************************************************************************************/

        /// <summary>IK cannot be dynamically enabled on a <see cref="PlayableAssetState"/>.</summary>
        public override bool ApplyFootIK
        {
            get => false;
            set
            {
#if UNITY_ASSERTIONS
                if (value)
                    OptionalWarning.UnsupportedIK.Log(
                        $"IK cannot be dynamically enabled on a {nameof(PlayableAssetState)}.", Root?.Component);
#endif
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Methods
        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="PlayableAssetState"/> to play the `asset`.</summary>
        /// <exception cref="ArgumentNullException">The `asset` is null.</exception>
        public PlayableAssetState(PlayableAsset asset)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            _Asset = asset;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void CreatePlayable(out Playable playable)
        {
            playable = _Asset.CreatePlayable(Root._Graph, Root.Component.gameObject);
            _Length = (float)_Asset.duration;
            if (!_HasInitialisedBindings)
                InitialiseBindings();
        }

        /************************************************************************************************************************/

        private IList<Object> _Bindings;
        private bool _HasInitialisedBindings;

        /************************************************************************************************************************/

        /// <summary>The objects controlled by each track in the asset.</summary>
        public IList<Object> Bindings
        {
            get => _Bindings;
            set
            {
                _Bindings = value;
                InitialiseBindings();
            }
        }

        /************************************************************************************************************************/

        /// <summary>Sets the <see cref="Bindings"/>.</summary>
        public void SetBindings(params Object[] bindings)
        {
            Bindings = bindings;
        }

        /************************************************************************************************************************/

        private void InitialiseBindings()
        {
            if (_Bindings == null || Root == null)
                return;

            _HasInitialisedBindings = true;

            var bindingCount = _Bindings.Count;
            if (bindingCount == 0)
                return;

            var output = _Asset.outputs.GetEnumerator();
            var graph = Root._Graph;

            for (int i = 0; i < bindingCount; i++)
            {
                if (!output.MoveNext())
                    return;

                if (ShouldSkipBinding(output.Current, out var name, out var type))
                {
                    i--;
                    continue;
                }

                var binding = _Bindings[i];
                if (binding == null && type != null)
                    continue;

#if UNITY_ASSERTIONS
                if (type != null && !type.IsAssignableFrom(binding.GetType()))
                {
                    Debug.LogError(
                        $"Binding Type Mismatch: bindings[{i}] is '{binding}' but should be a {type.FullName} for {name}",
                        Root?.Component as Object);
                    continue;
                }

                Validate.AssertPlayable(this);
#endif

                var playable = _Playable.GetInput(i);

                if (type == typeof(Animator))
                {
                    var playableOutput = AnimationPlayableOutput.Create(graph, name, (Animator)binding);
                    playableOutput.SetSourcePlayable(playable);
                }
                else if (type == typeof(AudioSource))
                {
                    var playableOutput = AudioPlayableOutput.Create(graph, name, (AudioSource)binding);
                    playableOutput.SetSourcePlayable(playable);
                }
                else// ActivationTrack, SignalTrack, ControlTrack, PlayableTrack.
                {
                    var playableOutput = ScriptPlayableOutput.Create(graph, name);
                    playableOutput.SetUserData(binding);
                    playableOutput.SetSourcePlayable(playable);
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Should the `binding` be skipped when determining how to map the <see cref="Bindings"/>?</summary>
        private static bool ShouldSkipBinding(PlayableBinding binding, out string name, out Type type)
        {
            name = binding.streamName;
            type = binding.outputTargetType;

            if (type == typeof(GameObject) && name == "Markers")
                return true;

            return false;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Destroy()
        {
            _Asset = null;
            base.Destroy();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Transition
        /************************************************************************************************************************/

        /// <summary>
        /// A serializable <see cref="ITransition"/> which can create a <see cref="PlayableAssetState"/> when
        /// passed into <see cref="AnimancerPlayable.Play(ITransition)"/>.
        /// </summary>
        /// <remarks>
        /// Unfortunately the tool used to generate this documentation does not currently support nested types with
        /// identical names, so only one <c>Transition</c> class will actually have a documentation page.
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Transition
        /// 
        [Serializable]
        public class Transition : Transition<PlayableAssetState>, IAnimationClipCollection
        {
            /************************************************************************************************************************/

            [SerializeField, Tooltip("The asset to play")]
            private PlayableAsset _Asset;

            /// <summary>[<see cref="SerializeField"/>] The asset to play.</summary>
            public ref PlayableAsset Asset => ref _Asset;

            /// <inheritdoc/>
            public override Object MainObject => _Asset;

            /// <summary>
            /// The <see cref="Asset"/> will be used as the <see cref="AnimancerState.Key"/> for the created state to
            /// be registered with.
            /// </summary>
            public override object Key => _Asset;

            /************************************************************************************************************************/

            [SerializeField, Tooltip(Strings.ProOnlyTag +
                "How fast the animation plays (1x = normal speed, 2x = double speed)")]
            private float _Speed = 1;

            /// <summary>[<see cref="SerializeField"/>]
            /// Determines how fast the animation plays (1x = normal speed, 2x = double speed).
            /// </summary>
            public override float Speed
            {
                get => _Speed;
                set => _Speed = value;
            }

            /************************************************************************************************************************/

            [SerializeField, Tooltip(Strings.ProOnlyTag + "If enabled, the animation's time will start at this value when played")]
            private float _NormalizedStartTime = float.NaN;

            /// <summary>[<see cref="SerializeField"/>]
            /// Determines what <see cref="AnimancerState.NormalizedTime"/> to start the animation at.
            /// <para></para>
            /// The default value is <see cref="float.NaN"/> which indicates that this value is not used so the
            /// animation will continue from its current time.
            /// </summary>
            public override float NormalizedStartTime
            {
                get => _NormalizedStartTime;
                set => _NormalizedStartTime = value;
            }

            /// <summary>
            /// If this transition will set the <see cref="AnimancerState.NormalizedTime"/>, then it needs to use
            /// <see cref="FadeMode.FromStart"/>.
            /// </summary>
            public override FadeMode FadeMode => float.IsNaN(_NormalizedStartTime) ? FadeMode.FixedSpeed : FadeMode.FromStart;

            /************************************************************************************************************************/

            [SerializeField, Tooltip("The objects controlled by each of the tracks in the Timeline Asset")]
            private Object[] _Bindings;

            /// <summary>[<see cref="SerializeField"/>] The objects controlled by each of the tracks in the Timeline Asset.</summary>
            public ref Object[] Bindings => ref _Bindings;

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override float MaximumDuration => _Asset != null ? (float)_Asset.duration : 0;

            /// <inheritdoc/>
            public override bool IsValid => _Asset != null;

            /************************************************************************************************************************/

            /// <summary>
            /// Creates and returns a new <see cref="PlayableAssetState"/>.
            /// <para></para>
            /// Note that using methods like <see cref="AnimancerPlayable.Play(ITransition)"/> will also call
            /// <see cref="ITransition.Apply"/>, so if you call this method manually you may want to call that method
            /// as well. Or you can just use <see cref="AnimancerUtilities.CreateStateAndApply"/>.
            /// <para></para>
            /// This method also assigns it as the <see cref="AnimancerState.Transition{TState}.State"/>.
            /// </summary>
            public override PlayableAssetState CreateState()
            {
                State = new PlayableAssetState(_Asset);
                State.SetBindings(_Bindings);
                return State;
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void Apply(AnimancerState state)
            {
                base.Apply(state);

                if (!float.IsNaN(_Speed))
                    state.Speed = _Speed;

                if (!float.IsNaN(_NormalizedStartTime))
                    state.NormalizedTime = _NormalizedStartTime;
                else if (state.Weight == 0)
                    state.NormalizedTime = AnimancerEvent.Sequence.GetDefaultNormalizedStartTime(_Speed);
            }

            /************************************************************************************************************************/

            /// <summary>Gathers all the animations associated with this object.</summary>
            void IAnimationClipCollection.GatherAnimationClips(ICollection<AnimationClip> clips) => clips.GatherFromAsset(_Asset);

            /************************************************************************************************************************/
#if UNITY_EDITOR
            /************************************************************************************************************************/

            /// <summary>[Editor-Only] Draws the Inspector GUI for a <see cref="Transition"/>.</summary>
            /// <remarks>
            /// Unfortunately the tool used to generate this documentation does not currently support nested types with
            /// identical names, so only one <c>Drawer</c> class will actually have a documentation page.
            /// <para></para>
            /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
            /// </remarks>
            [UnityEditor.CustomPropertyDrawer(typeof(Transition), true)]
            public class Drawer : Editor.TransitionDrawer
            {
                /************************************************************************************************************************/

                /// <summary>Creates a new <see cref="Drawer"/>.</summary>
                public Drawer() : base(nameof(_Asset)) { }

                /************************************************************************************************************************/

                /// <inheritdoc/>
                public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
                {
                    var height = base.GetPropertyHeight(property, label);

                    if (property.isExpanded)
                    {
                        var bindings = property.FindPropertyRelative(nameof(_Bindings));
                        bindings.isExpanded = true;
                        height -= Editor.AnimancerGUI.StandardSpacing + Editor.AnimancerGUI.LineHeight;
                    }

                    return height;
                }

                /************************************************************************************************************************/

                private static PlayableAsset _CurrentAsset;

                /// <inheritdoc/>
                protected override void DoPropertyGUI(
                    ref Rect area, UnityEditor.SerializedProperty rootProperty, UnityEditor.SerializedProperty property, GUIContent label)
                {
                    if (property.propertyPath.EndsWith($".{nameof(_Asset)}"))
                    {
                        _CurrentAsset = property.objectReferenceValue as PlayableAsset;
                    }

                    if (property.propertyPath.EndsWith($".{nameof(_Bindings)}"))
                    {
                        IEnumerator<PlayableBinding> outputEnumerator;
                        var outputCount = 0;
                        var firstBindingIsAnimation = false;
                        if (_CurrentAsset != null)
                        {
                            var outputs = _CurrentAsset.outputs;
                            outputEnumerator = outputs.GetEnumerator();

                            while (outputEnumerator.MoveNext())
                            {
                                if (ShouldSkipBinding(outputEnumerator.Current, out _, out _))
                                    continue;

                                if (outputCount == 0 && outputEnumerator.Current.outputTargetType == typeof(Animator))
                                    firstBindingIsAnimation = true;

                                outputCount++;
                            }

                            outputEnumerator = outputs.GetEnumerator();
                        }
                        else outputEnumerator = null;

                        // Bindings.
                        property.Next(true);
                        // Array.
                        property.Next(true);
                        // Array Size.

                        var color = GUI.color;
                        var miniButton = Editor.AnimancerGUI.MiniButton;
                        var sizeArea = area;
                        var bindingCount = property.intValue;
                        if (bindingCount != outputCount && !(bindingCount == 0 && outputCount == 1 && firstBindingIsAnimation))
                        {
                            GUI.color = Editor.AnimancerGUI.WarningFieldColor;

                            var labelText = label.text;

                            var countLabel = outputCount.ToString();
                            var fixSizeWidth = Editor.AnimancerGUI.CalculateWidth(miniButton, countLabel);
                            var fixSizeArea = Editor.AnimancerGUI.StealFromRight(ref sizeArea, fixSizeWidth, Editor.AnimancerGUI.StandardSpacing);
                            if (GUI.Button(fixSizeArea, countLabel, miniButton))
                                property.intValue = outputCount;

                            label.text = labelText;
                        }
                        UnityEditor.EditorGUI.PropertyField(sizeArea, property, label, false);
                        GUI.color = color;

                        UnityEditor.EditorGUI.indentLevel++;

                        bindingCount = property.intValue;
                        for (int i = 0; i < bindingCount; i++)
                        {
                            Editor.AnimancerGUI.NextVerticalArea(ref area);
                            property.Next(false);

                            if (outputEnumerator != null && outputEnumerator.MoveNext())
                            {
                                CheckIfSkip:
                                if (ShouldSkipBinding(outputEnumerator.Current, out var name, out var type))
                                {
                                    outputEnumerator.MoveNext();
                                    goto CheckIfSkip;
                                }

                                label.text = name;

                                var targetObject = property.serializedObject.targetObject;
                                var allowSceneObjects = targetObject != null && !UnityEditor.EditorUtility.IsPersistent(targetObject);

                                label = UnityEditor.EditorGUI.BeginProperty(area, label, property);
                                var fieldArea = area;
                                var obj = property.objectReferenceValue;
                                var objExists = obj != null;

                                if (objExists)
                                {
                                    if (i == 0 && type == typeof(Animator))
                                    {
                                        DoRemoveButton(ref fieldArea, label, property, ref obj,
                                            "This Animation Track is the first Track" +
                                            " so it will automatically control the Animancer output and likely does not need a binding.");
                                    }
                                    else if (type == null)
                                    {
                                        DoRemoveButton(ref fieldArea, label, property, ref obj,
                                            "This Animation Track does not need a binding.");
                                        type = typeof(Object);
                                    }
                                    else if (!type.IsAssignableFrom(obj.GetType()))
                                    {
                                        DoRemoveButton(ref fieldArea, label, property, ref obj,
                                            "This binding has the wrong type for this Animation Track.");
                                    }
                                }

                                if (type != null || objExists)
                                {
                                    property.objectReferenceValue =
                                        UnityEditor.EditorGUI.ObjectField(fieldArea, label, obj, type, allowSceneObjects);
                                }
                                else
                                {
                                    UnityEditor.EditorGUI.LabelField(fieldArea, label);
                                }

                                UnityEditor.EditorGUI.EndProperty();
                            }
                            else
                            {
                                GUI.color = Editor.AnimancerGUI.WarningFieldColor;

                                UnityEditor.EditorGUI.PropertyField(area, property, false);
                            }

                            GUI.color = color;
                        }

                        UnityEditor.EditorGUI.indentLevel--;
                        return;
                    }

                    base.DoPropertyGUI(ref area, rootProperty, property, label);
                }

                /************************************************************************************************************************/

                private static void DoRemoveButton(ref Rect area, GUIContent label, UnityEditor.SerializedProperty property,
                    ref Object obj, string tooltip)
                {
                    label.tooltip = tooltip;
                    GUI.color = Editor.AnimancerGUI.WarningFieldColor;
                    var miniButton = Editor.AnimancerGUI.MiniButton;

                    var text = label.text;
                    label.text = "x";

                    var xWidth = Editor.AnimancerGUI.CalculateWidth(miniButton, label);
                    var xArea = Editor.AnimancerGUI.StealFromRight(
                        ref area, xWidth, Editor.AnimancerGUI.StandardSpacing);
                    if (GUI.Button(xArea, label, miniButton))
                        property.objectReferenceValue = obj = null;

                    label.text = text;
                }

                /************************************************************************************************************************/
            }

            /************************************************************************************************************************/
#endif
            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

