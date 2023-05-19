// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace Animancer
{
    /// <summary>[Pro-Only] An <see cref="AnimancerState"/> which plays a <see cref="RuntimeAnimatorController"/>.</summary>
    /// <remarks>
    /// You can control this state very similarly to an <see cref="Animator"/> via its <see cref="Playable"/> property.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/animator-controllers">Animator Controllers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ControllerState
    /// 
    public class ControllerState : AnimancerState
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        private RuntimeAnimatorController _Controller;

        /// <summary>The <see cref="RuntimeAnimatorController"/> which this state plays.</summary>
        public RuntimeAnimatorController Controller
        {
            get => _Controller;
            set => ChangeMainObject(ref _Controller, value);
        }

        /// <summary>The <see cref="RuntimeAnimatorController"/> which this state plays.</summary>
        public override Object MainObject
        {
            get => Controller;
            set => Controller = (RuntimeAnimatorController)value;
        }

        /// <summary>The internal system which plays the <see cref="RuntimeAnimatorController"/>.</summary>
        public AnimatorControllerPlayable Playable
        {
            get
            {
                Validate.AssertPlayable(this);
                return _Playable;
            }
        }

        private new AnimatorControllerPlayable _Playable;

        /************************************************************************************************************************/

        private bool _KeepStateOnStop;

        /// <summary>
        /// If false, <see cref="Stop"/> will reset all layers to their default state. Default False.
        /// <para></para>
        /// The <see cref="DefaultStateHashes"/> will only be gathered the first time this property is set to false or
        /// <see cref="GatherDefaultStates"/> is called manually.
        /// </summary>
        public bool KeepStateOnStop
        {
            get => _KeepStateOnStop;
            set
            {
                _KeepStateOnStop = value;
                if (!value && DefaultStateHashes == null && _Playable.IsValid())
                    GatherDefaultStates();
            }
        }

        /// <summary>
        /// The <see cref="AnimatorStateInfo.shortNameHash"/> of the default state on each layer, used to reset to
        /// those states when <see cref="Stop"/> is called if <see cref="KeepStateOnStop"/> is true.
        /// </summary>
        public int[] DefaultStateHashes { get; set; }

        /************************************************************************************************************************/

#if UNITY_ASSERTIONS
        /// <summary>[Assert-Only] Animancer Events do not work properly on <see cref="ControllerState"/>s.</summary>
        protected override string UnsupportedEventsMessage =>
            "Animancer Events on " + nameof(ControllerState) + "s will probably not work as expected." +
            " The events will be associated with the entire Animator Controller and be triggered by any of the" +
            " states inside it. If you want to use events in an Animator Controller you will likely need to use" +
            " Unity's regular Animation Event system.";
#endif

        /************************************************************************************************************************/

        /// <summary>IK cannot be dynamically enabled on a <see cref="ControllerState"/>.</summary>
        public override void CopyIKFlags(AnimancerNode node) { }

        /************************************************************************************************************************/

        /// <summary>IK cannot be dynamically enabled on a <see cref="ControllerState"/>.</summary>
        public override bool ApplyAnimatorIK
        {
            get => false;
            set
            {
#if UNITY_ASSERTIONS
                if (value)
                    OptionalWarning.UnsupportedIK.Log($"IK cannot be dynamically enabled on a {nameof(ControllerState)}." +
                        " You must instead enable it on the desired layer inside the Animator Controller.", _Controller);
#endif
            }
        }

        /************************************************************************************************************************/

        /// <summary>IK cannot be dynamically enabled on a <see cref="ControllerState"/>.</summary>
        public override bool ApplyFootIK
        {
            get => false;
            set
            {
#if UNITY_ASSERTIONS
                if (value)
                    OptionalWarning.UnsupportedIK.Log($"IK cannot be dynamically enabled on a {nameof(ControllerState)}." +
                        " You must instead enable it on the desired state inside the Animator Controller.", _Controller);
#endif
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Public API
        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="ControllerState"/> to play the `controller`.</summary>
        public ControllerState(RuntimeAnimatorController controller, bool keepStateOnStop = false)
        {
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));

            _Controller = controller;
            _KeepStateOnStop = keepStateOnStop;
        }

        /************************************************************************************************************************/

        /// <summary>Creates and assigns the <see cref="AnimatorControllerPlayable"/> managed by this state.</summary>
        protected override void CreatePlayable(out Playable playable)
        {
            playable = _Playable = AnimatorControllerPlayable.Create(Root._Graph, _Controller);

            if (!_KeepStateOnStop)
                GatherDefaultStates();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Stores the values of all parameters, calls <see cref="AnimancerNode.DestroyPlayable"/>, then restores the
        /// parameter values.
        /// </summary>
        public override void RecreatePlayable()
        {
            if (!_Playable.IsValid())
            {
                CreatePlayable();
                return;
            }

            var parameterCount = _Playable.GetParameterCount();
            var values = new object[parameterCount];
            for (int i = 0; i < parameterCount; i++)
            {
                values[i] = AnimancerUtilities.GetParameterValue(_Playable, _Playable.GetParameter(i));
            }

            base.RecreatePlayable();

            for (int i = 0; i < parameterCount; i++)
            {
                AnimancerUtilities.SetParameterValue(_Playable, _Playable.GetParameter(i), values[i]);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The current state on layer 0, or the next state if it is currently in a transition.
        /// </summary>
        public AnimatorStateInfo StateInfo
        {
            get
            {
                Validate.AssertPlayable(this);
                return _Playable.IsInTransition(0) ?
                    _Playable.GetNextAnimatorStateInfo(0) :
                    _Playable.GetCurrentAnimatorStateInfo(0);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The <see cref="AnimatorStateInfo.normalizedTime"/> * <see cref="AnimatorStateInfo.length"/> of the
        /// <see cref="StateInfo"/>.
        /// </summary>
        protected override float RawTime
        {
            get
            {
                var info = StateInfo;
                return info.normalizedTime * info.length;
            }
            set
            {
                Validate.AssertPlayable(this);
                _Playable.PlayInFixedTime(0, 0, value);
            }
        }

        /************************************************************************************************************************/

        /// <summary>The current <see cref="AnimatorStateInfo.length"/> (on layer 0).</summary>
        public override float Length => StateInfo.length;

        /************************************************************************************************************************/

        /// <summary>Indicates whether the current state on layer 0 will loop back to the start when it reaches the end.</summary>
        public override bool IsLooping => StateInfo.loop;

        /************************************************************************************************************************/

        /// <summary>Gathers the <see cref="DefaultStateHashes"/> from the current states.</summary>
        public void GatherDefaultStates()
        {
            Validate.AssertPlayable(this);
            var layerCount = _Playable.GetLayerCount();
            if (DefaultStateHashes == null || DefaultStateHashes.Length != layerCount)
                DefaultStateHashes = new int[layerCount];

            while (--layerCount >= 0)
                DefaultStateHashes[layerCount] = _Playable.GetCurrentAnimatorStateInfo(layerCount).shortNameHash;
        }

        /// <summary>
        /// Calls the base <see cref="AnimancerState.Stop"/> and if <see cref="KeepStateOnStop"/> is false it also
        /// calls <see cref="ResetToDefaultStates"/>.
        /// </summary>
        public override void Stop()
        {
            if (_KeepStateOnStop)
            {
                base.Stop();
            }
            else
            {
                ResetToDefaultStates();

                // Don't call base.Stop(); because it sets Time = 0; which uses PlayInFixedTime and interferes with
                // resetting to the default states.
                Weight = 0;
                IsPlaying = false;
                Events = null;
            }
        }

        /// <summary>
        /// Resets all layers to their default state.
        /// </summary>
        /// <exception cref="NullReferenceException"><see cref="DefaultStateHashes"/> is null.</exception>
        /// <exception cref="IndexOutOfRangeException">
        /// The size of <see cref="DefaultStateHashes"/> is larger than the number of layers in the
        /// <see cref="Controller"/>.
        /// </exception>
        public void ResetToDefaultStates()
        {
            Validate.AssertPlayable(this);
            for (int i = DefaultStateHashes.Length - 1; i >= 0; i--)
                _Playable.Play(DefaultStateHashes[i], i, 0);

            // Allowing the RawTime to be applied prevents the default state from being played because Animator
            // Controllers do not properly respond to multiple Play calls in the same frame.
            CancelSetTime();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            if (_Controller != null)
                clips.Gather(_Controller.animationClips);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Destroy()
        {
            _Controller = null;
            base.Destroy();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Parameter IDs
        /************************************************************************************************************************/

        /// <summary>A wrapper for the name and hash of an <see cref="AnimatorControllerParameter"/>.</summary>
        public readonly struct ParameterID
        {
            /************************************************************************************************************************/

            /// <summary>The name of this parameter.</summary>
            public readonly string Name;

            /// <summary>The name hash of this parameter.</summary>
            public readonly int Hash;

            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="ParameterID"/> with the specified <see cref="Name"/> and uses
            /// <see cref="Animator.StringToHash"/> to calculate the <see cref="Hash"/>.
            /// </summary>
            public ParameterID(string name)
            {
                Name = name;
                Hash = Animator.StringToHash(name);
            }

            /// <summary>
            /// Creates a new <see cref="ParameterID"/> with the specified <see cref="Hash"/> and leaves the
            /// <see cref="Name"/> null.
            /// </summary>
            public ParameterID(int hash)
            {
                Name = null;
                Hash = hash;
            }

            /// <summary>Creates a new <see cref="ParameterID"/> with the specified <see cref="Name"/> and <see cref="Hash"/>.</summary>
            /// <remarks>This constructor does not verify that the `hash` actually corresponds to the `name`.</remarks>
            public ParameterID(string name, int hash)
            {
                Name = name;
                Hash = hash;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="ParameterID"/> with the specified <see cref="Name"/> and uses
            /// <see cref="Animator.StringToHash"/> to calculate the <see cref="Hash"/>.
            /// </summary>
            public static implicit operator ParameterID(string name) => new ParameterID(name);

            /// <summary>
            /// Creates a new <see cref="ParameterID"/> with the specified <see cref="Hash"/> and leaves the
            /// <see cref="Name"/> null.
            /// </summary>
            public static implicit operator ParameterID(int hash) => new ParameterID(hash);

            /************************************************************************************************************************/

            /// <summary>Returns the <see cref="Hash"/>.</summary>
            public static implicit operator int(ParameterID parameter) => parameter.Hash;

            /************************************************************************************************************************/

#if UNITY_EDITOR
            private static Dictionary<RuntimeAnimatorController, Dictionary<int, AnimatorControllerParameterType>>
                _ControllerToParameterHashAndType;
#endif

            /// <summary>[Editor-Conditional]
            /// Throws if the `controller` doesn't have a parameter with the specified <see cref="Hash"/>
            /// and `type`.
            /// </summary>
            /// <exception cref="ArgumentException"/>
            [System.Diagnostics.Conditional(Strings.UnityEditor)]
            public void ValidateHasParameter(RuntimeAnimatorController controller, AnimatorControllerParameterType type)
            {
#if UNITY_EDITOR
                Editor.AnimancerEditorUtilities.InitialiseCleanDictionary(ref _ControllerToParameterHashAndType);

                // Get the parameter details.
                if (!_ControllerToParameterHashAndType.TryGetValue(controller, out var parameterDetails))
                {
                    parameterDetails = new Dictionary<int, AnimatorControllerParameterType>();

                    var parameters = ((AnimatorController)controller).parameters;
                    var count = parameters.Length;
                    for (int i = 0; i < count; i++)
                    {
                        var parameter = parameters[i];
                        parameterDetails.Add(parameter.nameHash, parameter.type);
                    }

                    _ControllerToParameterHashAndType.Add(controller, parameterDetails);
                }

                // Check that there is a parameter with the correct hash and type.

                if (!parameterDetails.TryGetValue(Hash, out var parameterType))
                {
                    throw new ArgumentException($"{controller} has no {type} parameter matching {this}");
                }

                if (type != parameterType)
                {
                    throw new ArgumentException($"{controller} has a parameter matching {this}, but it is not a {type}");
                }
#endif
            }

            /************************************************************************************************************************/

            /// <summary>Returns a string containing the <see cref="Name"/> and <see cref="Hash"/>.</summary>
            public override string ToString()
            {
                return $"{nameof(ControllerState)}.{nameof(ParameterID)}" +
                    $"({nameof(Name)}: '{Name}'" +
                    $", {nameof(Hash)}: {Hash})";
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Inspector
        /************************************************************************************************************************/

        /// <summary>The number of parameters being wrapped by this state.</summary>
        public virtual int ParameterCount => 0;

        /// <summary>Returns the hash of a parameter being wrapped by this state.</summary>
        /// <exception cref="NotSupportedException">This state doesn't wrap any parameters.</exception>
        public virtual int GetParameterHash(int index) => throw new NotSupportedException();

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
        /// </remarks>
        public sealed class Drawer : Editor.ParametizedAnimancerStateDrawer<ControllerState>
        {
            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="Drawer"/> to manage the Inspector GUI for the `state`.</summary>
            public Drawer(ControllerState state) : base(state) { }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void DoDetailsGUI()
            {
                GatherParameters();
                base.DoDetailsGUI();
            }

            /************************************************************************************************************************/

            private readonly List<AnimatorControllerParameter>
                Parameters = new List<AnimatorControllerParameter>();

            /// <summary>Fills the <see cref="Parameters"/> list with the current parameter details.</summary>
            private void GatherParameters()
            {
                Parameters.Clear();

                var count = Target.ParameterCount;
                if (count == 0)
                    return;

                for (int i = 0; i < count; i++)
                {
                    var hash = Target.GetParameterHash(i);
                    Parameters.Add(GetParameter(hash));
                }
            }

            /************************************************************************************************************************/

            private AnimatorControllerParameter GetParameter(int hash)
            {
                Validate.AssertPlayable(Target);
                var parameterCount = Target._Playable.GetParameterCount();
                for (int i = 0; i < parameterCount; i++)
                {
                    var parameter = Target._Playable.GetParameter(i);
                    if (parameter.nameHash == hash)
                        return parameter;
                }

                return null;
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override int ParameterCount => Parameters.Count;

            /// <inheritdoc/>
            public override string GetParameterName(int index) => Parameters[index].name;

            /// <inheritdoc/>
            public override AnimatorControllerParameterType GetParameterType(int index) => Parameters[index].type;

            /// <inheritdoc/>
            public override object GetParameterValue(int index)
            {
                Validate.AssertPlayable(Target);
                return AnimancerUtilities.GetParameterValue(Target._Playable, Parameters[index]);
            }

            /// <inheritdoc/>
            public override void SetParameterValue(int index, object value)
            {
                Validate.AssertPlayable(Target);
                AnimancerUtilities.SetParameterValue(Target._Playable, Parameters[index], value);
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
        /// Base class for serializable <see cref="ITransition"/>s which can create a particular type of
        /// <see cref="ControllerState"/> when passed into <see cref="AnimancerPlayable.Play(ITransition)"/>.
        /// </summary>
        /// <remarks>
        /// Unfortunately the tool used to generate this documentation does not currently support nested types with
        /// identical names, so only one <c>Transition</c> class will actually have a documentation page.
        /// <para></para>
        /// Even though it has the <see cref="SerializableAttribute"/>, this class won't actually get serialized
        /// by Unity because it's generic and abstract. Each child class still needs to include the attribute.
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Transition_1
        /// 
        [Serializable]
        public abstract new class Transition<TState> : AnimancerState.Transition<TState>, IAnimationClipCollection
            where TState : ControllerState
        {
            /************************************************************************************************************************/

            [SerializeField]
            private RuntimeAnimatorController _Controller;

            /// <summary>[<see cref="SerializeField"/>]
            /// The <see cref="ControllerState.Controller"/> that will be used for the created state.
            /// </summary>
            public ref RuntimeAnimatorController Controller => ref _Controller;

            /// <inheritdoc/>
            public override Object MainObject => _Controller;

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

            /************************************************************************************************************************/

            [SerializeField, Tooltip("If false, stopping this state will reset all its layers to their default state. Default False.")]
            private bool _KeepStateOnStop;

            /// <summary>[<see cref="SerializeField"/>]
            /// If false, <see cref="Stop"/> will reset all layers to their default state.
            /// <para></para>
            /// If you set this value to false after the <see cref="Playable"/> is created, you must assign the
            /// <see cref="DefaultStateHashes"/> or call <see cref="GatherDefaultStates"/> yourself.
            /// </summary>
            public ref bool KeepStateOnStop => ref _KeepStateOnStop;

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override float MaximumDuration
            {
                get
                {
                    if (_Controller == null)
                        return 0;

                    var duration = 0f;

                    var clips = _Controller.animationClips;
                    for (int i = 0; i < clips.Length; i++)
                    {
                        var length = clips[i].length;
                        if (duration < length)
                            duration = length;
                    }

                    return duration;
                }
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override bool IsValid => _Controller != null;

            /************************************************************************************************************************/

            /// <summary>Returns the <see cref="Controller"/>.</summary>
            public static implicit operator RuntimeAnimatorController(Transition<TState> transition)
                => transition != null ? transition._Controller : null;

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void Apply(AnimancerState state)
            {
                base.Apply(state);

                var controllerState = State;
                if (controllerState != null)
                {
                    controllerState.KeepStateOnStop = _KeepStateOnStop;

                    if (!float.IsNaN(_NormalizedStartTime))
                    {
                        if (!_KeepStateOnStop)
                        {
                            controllerState._Playable.Play(controllerState.DefaultStateHashes[0], 0, _NormalizedStartTime);
                        }
                        else
                        {
                            state.NormalizedTime = _NormalizedStartTime;
                        }
                    }
                }
                else
                {
                    if (!float.IsNaN(_NormalizedStartTime))
                        state.NormalizedTime = _NormalizedStartTime;
                }
            }

            /************************************************************************************************************************/

            /// <summary>Adds all clips in the <see cref="Controller"/> to the collection.</summary>
            void IAnimationClipCollection.GatherAnimationClips(ICollection<AnimationClip> clips)
            {
                if (_Controller != null)
                    clips.Gather(_Controller.animationClips);
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/

        /// <summary>
        /// A serializable <see cref="ITransition"/> which can create a <see cref="ControllerState"/> when
        /// passed into <see cref="AnimancerPlayable.Play(ITransition)"/>.
        /// </summary>
        /// <remarks>
        /// This class can be implicitly cast to and from <see cref="RuntimeAnimatorController"/>.
        /// <para></para>
        /// Unfortunately the tool used to generate this documentation does not currently support nested types with
        /// identical names, so only one <c>Transition</c> class will actually have a documentation page.
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Transition
        /// 
        [Serializable]
        public class Transition : Transition<ControllerState>
        {
            /************************************************************************************************************************/

            /// <summary>Creates and returns a new <see cref="ControllerState"/>.</summary>
            /// <remarks>
            /// Note that using methods like <see cref="AnimancerPlayable.Play(ITransition)"/> will also call
            /// <see cref="ITransition.Apply"/>, so if you call this method manually you may want to call that method
            /// as well. Or you can just use <see cref="AnimancerUtilities.CreateStateAndApply"/>.
            /// <para></para>
            /// This method also assigns it as the <see cref="AnimancerState.Transition{TState}.State"/>.
            /// </remarks>
            public override ControllerState CreateState() => State = new ControllerState(Controller, KeepStateOnStop);

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="Transition"/>.</summary>
            public Transition() { }

            /// <summary>Creates a new <see cref="Transition"/> with the specified Animator Controller.</summary>
            public Transition(RuntimeAnimatorController controller) => Controller = controller;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="Transition"/> with the specified Animator Controller.</summary>
            public static implicit operator Transition(RuntimeAnimatorController controller) => new Transition(controller);

            /************************************************************************************************************************/
            #region Drawer
#if UNITY_EDITOR
            /************************************************************************************************************************/

            /// <summary>
            /// [Editor-Only] Draws the Inspector GUI for a <see cref="Transition{TState}"/> or
            /// <see cref="Transition"/>.
            /// </summary>
            /// 
            /// <remarks>
            /// Unfortunately the tool used to generate this documentation does not currently support nested types with
            /// identical names, so only one <c>Drawer</c> class will actually have a documentation page.
            /// <para></para>
            /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
            /// </remarks>
            [CustomPropertyDrawer(typeof(Transition<>), true)]
            [CustomPropertyDrawer(typeof(Transition), true)]
            public class Drawer : Editor.TransitionDrawer
            {
                /************************************************************************************************************************/

                private readonly string[] Parameters;
                private readonly string[] ParameterPrefixes;

                /************************************************************************************************************************/

                /// <summary>Creates a new <see cref="Drawer"/> without any parameters.</summary>
                public Drawer() : this(null) { }

                /// <summary>Creates a new <see cref="Drawer"/> and sets the <see cref="Parameters"/>.</summary>
                public Drawer(params string[] parameters) : base(nameof(_Controller))
                {
                    Parameters = parameters;
                    if (parameters == null)
                        return;

                    ParameterPrefixes = new string[parameters.Length];

                    for (int i = 0; i < ParameterPrefixes.Length; i++)
                    {
                        ParameterPrefixes[i] = "." + parameters[i];
                    }
                }

                /************************************************************************************************************************/

                /// <inheritdoc/>
                protected override void DoPropertyGUI(ref Rect area, SerializedProperty rootProperty,
                    SerializedProperty property, GUIContent label)
                {
                    if (ParameterPrefixes != null)
                    {
                        var controllerProperty = rootProperty.FindPropertyRelative(MainPropertyName);
                        var controller = controllerProperty.objectReferenceValue as AnimatorController;
                        if (controller != null)
                        {
                            var path = property.propertyPath;

                            for (int i = 0; i < ParameterPrefixes.Length; i++)
                            {
                                if (path.EndsWith(ParameterPrefixes[i]))
                                {
                                    area.height = Editor.AnimancerGUI.LineHeight;
                                    DoParameterGUI(area, controller, property);
                                    return;
                                }
                            }
                        }
                    }

                    EditorGUI.BeginChangeCheck();

                    base.DoPropertyGUI(ref area, rootProperty, property, label);

                    // When the controller changes, validate all parameters.
                    if (EditorGUI.EndChangeCheck() &&
                        Parameters != null &&
                        property.propertyPath.EndsWith(MainPropertyPathSuffix))
                    {
                        var controller = property.objectReferenceValue as AnimatorController;
                        if (controller != null)
                        {
                            for (int i = 0; i < Parameters.Length; i++)
                            {
                                property = rootProperty.FindPropertyRelative(Parameters[i]);
                                var parameterName = property.stringValue;
                                if (!HasFloatParameter(controller, parameterName))
                                {
                                    parameterName = GetFirstFloatParameterName(controller);
                                    if (!string.IsNullOrEmpty(parameterName))
                                        property.stringValue = parameterName;
                                }
                            }
                        }
                    }
                }

                /************************************************************************************************************************/

                /// <summary>
                /// Draws a dropdown menu to select the name of a parameter in the `controller`.
                /// </summary>
                protected void DoParameterGUI(Rect area, AnimatorController controller, SerializedProperty property)
                {
                    var parameterName = property.stringValue;
                    var parameters = controller.parameters;

                    var label = Editor.AnimancerGUI.TempContent(property);
                    label = EditorGUI.BeginProperty(area, label, property);

                    var xMax = area.xMax;
                    area.width = EditorGUIUtility.labelWidth;
                    EditorGUI.PrefixLabel(area, label);

                    area.x += area.width;
                    area.xMax = xMax;

                    var color = GUI.color;
                    if (!HasFloatParameter(controller, parameterName))
                        GUI.color = Editor.AnimancerGUI.ErrorFieldColor;

                    var content = Editor.AnimancerGUI.TempContent(parameterName);
                    if (EditorGUI.DropdownButton(area, content, FocusType.Passive))
                    {
                        property = property.Copy();

                        var menu = new GenericMenu();

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            var parameter = parameters[i];
                            Editor.Serialization.AddPropertyModifierFunction(menu, property, parameter.name,
                                parameter.type == AnimatorControllerParameterType.Float,
                                (targetProperty) =>
                                {
                                    targetProperty.stringValue = parameter.name;
                                });
                        }

                        if (menu.GetItemCount() == 0)
                            menu.AddDisabledItem(new GUIContent("No Parameters"));

                        menu.ShowAsContext();
                    }

                    GUI.color = color;

                    EditorGUI.EndProperty();
                }

                /************************************************************************************************************************/

                private static bool HasFloatParameter(AnimatorController controller, string name)
                {
                    if (string.IsNullOrEmpty(name))
                        return false;

                    var parameters = controller.parameters;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameter = parameters[i];
                        if (parameter.type == AnimatorControllerParameterType.Float && name == parameters[i].name)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                /************************************************************************************************************************/

                private static string GetFirstFloatParameterName(AnimatorController controller)
                {
                    var parameters = controller.parameters;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameter = parameters[i];
                        if (parameter.type == AnimatorControllerParameterType.Float)
                        {
                            return parameter.name;
                        }
                    }

                    return "";
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

