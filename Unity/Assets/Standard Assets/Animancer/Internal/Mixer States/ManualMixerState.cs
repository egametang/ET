// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Animancer
{
    /// <summary>[Pro-Only]
    /// An <see cref="AnimancerState"/> which blends multiple child states. Unlike other mixers, this class does not
    /// perform any automatic weight calculations, it simple allows you to control the weight of all states manually.
    /// </summary>
    /// <remarks>
    /// This mixer type is similar to the Direct Blend Type in Mecanim Blend Trees.
    /// The official <see href="https://learn.unity.com/tutorial/5c5152bcedbc2a001fd5c696">Direct Blend Trees</see>
    /// tutorial explains their general concepts and purpose which apply to <see cref="ManualMixerState"/>s as well.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ManualMixerState
    /// 
    public class ManualMixerState : MixerState
    {
        /************************************************************************************************************************/
        #region Properties
        /************************************************************************************************************************/

        /// <summary>An empty array of states.</summary>
        public static readonly AnimancerState[] NoStates = new AnimancerState[0];

        /// <summary>The states managed by this mixer.</summary>
        private AnimancerState[] _States = NoStates;

        /// <summary>Returns the <see cref="_States"/>.</summary>
        public override IList<AnimancerState> ChildStates => _States;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override int ChildCount => _States.Length;

        /// <inheritdoc/>
        public override AnimancerState GetChild(int index) => _States[index];

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Initialisation
        /************************************************************************************************************************/

        /// <summary>
        /// Initialises this mixer with the specified number of children which can be set individually by
        /// <see cref="MixerState.CreateChild(int, AnimationClip)"/> and <see cref="MixerState.SetChild"/>.
        /// </summary>
        /// <remarks><see cref="AnimancerState.Destroy"/> will be called on any existing children.</remarks>
        public virtual void Initialise(int childCount)
        {
#if UNITY_ASSERTIONS
            if (childCount <= 1 && OptionalWarning.MixerMinChildren.IsEnabled())
                OptionalWarning.MixerMinChildren.Log(
                    $"{this} is being initialised with {nameof(childCount)} <= 1." +
                    $" The purpose of a mixer is to mix multiple child states.", Root?.Component);
#endif

            for (int i = _States.Length - 1; i >= 0; i--)
            {
                var state = _States[i];
                if (state == null)
                    continue;

                state.Destroy();
            }

            _States = new AnimancerState[childCount];

            if (_Playable.IsValid())
            {
                _Playable.SetInputCount(childCount);
            }
            else if (Root != null)
            {
                CreatePlayable();
            }
        }

        /************************************************************************************************************************/

        /// <summary>Initialises this mixer with one state per clip.</summary>
        public void Initialise(params AnimationClip[] clips)
        {
#if UNITY_ASSERTIONS
            if (clips == null)
                throw new ArgumentNullException(nameof(clips));
#endif

            var count = clips.Length;
            Initialise(count);

            for (int i = 0; i < count; i++)
            {
                var clip = clips[i];
                if (clip != null)
                    CreateChild(i, clip);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Initialises this mixer by calling <see cref="MixerState.CreateChild(int, Object)"/> for each of the
        /// `states`.
        /// </summary>
        public void Initialise(params Object[] states)
        {
#if UNITY_ASSERTIONS
            if (states == null)
                throw new ArgumentNullException(nameof(states));
#endif

            var count = states.Length;
            Initialise(count);

            for (int i = 0; i < count; i++)
            {
                var state = states[i];
                if (state != null)
                    CreateChild(i, state);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Transition
        /************************************************************************************************************************/

        /// <summary>
        /// Base class for serializable <see cref="ITransition"/>s which can create a particular type of
        /// <see cref="ManualMixerState"/> when passed into <see cref="AnimancerPlayable.Play(ITransition)"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// Even though it has the <see cref="SerializableAttribute"/>, this class won't actually get serialized
        /// by Unity because it's generic and abstract. Each child class still needs to include the attribute.
        /// <para></para>
        /// Unfortunately the tool used to generate this documentation does not currently support nested types with
        /// identical names, so only one <c>Transition</c> class will actually have a documentation page.
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Transition_1
        /// 
        [Serializable]
        public abstract new class Transition<TMixer> : AnimancerState.Transition<TMixer>, IAnimationClipCollection
            where TMixer : ManualMixerState
        {
            /************************************************************************************************************************/

            [SerializeField, Tooltip(Strings.ProOnlyTag +
                "How fast the mixer plays (1x = normal speed, 2x = double speed)")]
            private float _Speed = 1;

            /// <summary>[<see cref="SerializeField"/>]
            /// Determines how fast the mixer plays (1x = normal speed, 2x = double speed).
            /// </summary>
            public override float Speed
            {
                get => _Speed;
                set => _Speed = value;
            }

            /************************************************************************************************************************/

            [SerializeField, HideInInspector]
            [FormerlySerializedAs("_Clips")]
            private Object[] _States;

            /// <summary>[<see cref="SerializeField"/>] Objects that define how to create each state in the mixer.</summary>
            /// <remarks>See <see cref="Initialise(Object[])"/> for more information.</remarks>
            public ref Object[] States => ref _States;

            /// <summary>The name of the serialized backing field of <see cref="States"/>.</summary>
            public const string StatesField = nameof(_States);

            /************************************************************************************************************************/

            [SerializeField, HideInInspector]
            private float[] _Speeds;

            /// <summary>[<see cref="SerializeField"/>]
            /// The <see cref="AnimancerNode.Speed"/> to use for each state in the mixer.
            /// </summary>
            /// <remarks>If the size of this array doesn't match the <see cref="States"/>, it will be ignored.</remarks>
            public ref float[] Speeds => ref _Speeds;

            /// <summary>The name of the serialized backing field of <see cref="Speeds"/>.</summary>
            public const string SpeedsField = nameof(_Speeds);

            /************************************************************************************************************************/

            [SerializeField, HideInInspector]
            private bool[] _SynchroniseChildren;

            /// <summary>[<see cref="SerializeField"/>]
            /// The flags to be used in <see cref="MixerState.InitialiseSynchronisedChildren"/>.
            /// </summary>
            /// <remarks>The array can be null or empty. Any elements not in the array will be treated as true.</remarks>
            public ref bool[] SynchroniseChildren => ref _SynchroniseChildren;

            /// <summary>The name of the serialized backing field of <see cref="SynchroniseChildren"/>.</summary>
            public const string SynchroniseChildrenField = nameof(_SynchroniseChildren);

            /************************************************************************************************************************/

            /// <summary>[<see cref="ITransitionDetailed"/>] Are any of the <see cref="States"/> looping?</summary>
            public override bool IsLooping
            {
                get
                {
                    for (int i = _States.Length - 1; i >= 0; i--)
                    {
                        if (AnimancerUtilities.TryGetIsLooping(_States[i], out var isLooping) &&
                            isLooping)
                            return true;
                    }

                    return false;
                }
            }

            /// <inheritdoc/>
            public override float MaximumDuration
            {
                get
                {
                    if (_States == null)
                        return 0;

                    var duration = 0f;
                    var hasSpeeds = _Speeds != null && _Speeds.Length == _States.Length;

                    for (int i = _States.Length - 1; i >= 0; i--)
                    {
                        if (!AnimancerUtilities.TryGetLength(_States[i], out var length))
                            continue;

                        if (hasSpeeds)
                            length *= _Speeds[i];

                        if (duration < length)
                            duration = length;
                    }

                    return duration;
                }
            }

            /// <inheritdoc/>
            public override float AverageAngularSpeed
            {
                get
                {
                    if (_States == null)
                        return default;

                    var average = 0f;
                    var hasSpeeds = _Speeds != null && _Speeds.Length == _States.Length;

                    var count = 0;
                    for (int i = _States.Length - 1; i >= 0; i--)
                    {
                        if (AnimancerUtilities.TryGetAverageAngularSpeed(_States[i], out var speed))
                        {
                            if (hasSpeeds)
                                speed *= _Speeds[i];

                            average += speed;
                            count++;
                        }
                    }

                    return average / count;
                }
            }

            /// <inheritdoc/>
            public override Vector3 AverageVelocity
            {
                get
                {
                    if (_States == null)
                        return default;

                    var average = new Vector3();
                    var hasSpeeds = _Speeds != null && _Speeds.Length == _States.Length;

                    var count = 0;
                    for (int i = _States.Length - 1; i >= 0; i--)
                    {
                        if (AnimancerUtilities.TryGetAverageVelocity(_States[i], out var velocity))
                        {
                            if (hasSpeeds)
                                velocity *= _Speeds[i];

                            average += velocity;
                            count++;
                        }
                    }

                    return average / count;
                }
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Initialises the <see cref="AnimancerState.Transition{TState}.State"/> immediately after it is created.
            /// </summary>
            public virtual void InitialiseState()
            {
                var mixer = State;

                var auto = AutoSynchroniseChildren;
                try
                {
                    AutoSynchroniseChildren = false;
                    mixer.Initialise(_States);
                }
                finally
                {
                    AutoSynchroniseChildren = auto;
                }

                mixer.InitialiseSynchronisedChildren(_SynchroniseChildren);

                if (_Speeds != null)
                {
#if UNITY_ASSERTIONS
                    if (_Speeds.Length != 0 && _Speeds.Length != _States.Length)
                        Debug.LogError(
                            $"The number of serialized {nameof(Speeds)} ({_Speeds.Length})" +
                            $" does not match the number of {nameof(States)} ({_States.Length}).",
                            mixer.Root?.Component as Object);
#endif

                    var children = mixer._States;
                    for (int i = _Speeds.Length - 1; i >= 0; i--)
                        children[i].Speed = _Speeds[i];
                }
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void Apply(AnimancerState state)
            {
                base.Apply(state);

                if (!float.IsNaN(_Speed))
                    state.Speed = _Speed;
            }

            /************************************************************************************************************************/

            /// <summary>Adds the <see cref="States"/> to the collection.</summary>
            void IAnimationClipCollection.GatherAnimationClips(ICollection<AnimationClip> clips) => clips.GatherFromSource(_States);

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/

        /// <summary>
        /// A serializable <see cref="ITransition"/> which can create a <see cref="ManualMixerState"/> when
        /// passed into <see cref="AnimancerPlayable.Play(ITransition)"/>.
        /// </summary>
        /// <remarks>
        /// Unfortunately the tool used to generate this documentation does not currently support nested types with
        /// identical names, so only one <c>Transition</c> class will actually have a documentation page.
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Transition
        /// 
        [Serializable]
        public class Transition : Transition<ManualMixerState>
        {
            /************************************************************************************************************************/

            /// <summary>
            /// Creates and returns a new <see cref="ManualMixerState"/>.
            /// <para></para>
            /// Note that using methods like <see cref="AnimancerPlayable.Play(ITransition)"/> will also call
            /// <see cref="ITransition.Apply"/>, so if you call this method manually you may want to call that method
            /// as well. Or you can just use <see cref="AnimancerUtilities.CreateStateAndApply"/>.
            /// <para></para>
            /// This method also assigns it as the <see cref="AnimancerState.Transition{TState}.State"/>.
            /// </summary>
            public override ManualMixerState CreateState()
            {
                State = new ManualMixerState();
                InitialiseState();
                return State;
            }

            /************************************************************************************************************************/
            #region Drawer
#if UNITY_EDITOR
            /************************************************************************************************************************/

            /// <summary>[Editor-Only] Draws the Inspector GUI for a <see cref="Transition"/>.</summary>
            /// <remarks>
            /// Unfortunately the tool used to generate this documentation does not currently support nested types with
            /// identical names, so only one <c>Drawer</c> class will actually have a documentation page.
            /// <para></para>
            /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
            /// <para></para>
            /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
            /// </remarks>
            [CustomPropertyDrawer(typeof(Transition), true)]
            public class Drawer : Editor.TransitionDrawer
            {
                /************************************************************************************************************************/

                /// <summary>
                /// The property this drawer is currently drawing.
                /// <para></para>
                /// Normally each property has its own drawer, but arrays share a single drawer for all elements.
                /// </summary>
                public static SerializedProperty CurrentProperty { get; private set; }

                /// <summary>The <see cref="Transition{TState}.States"/> field.</summary>
                public static SerializedProperty CurrentStates { get; private set; }

                /// <summary>The <see cref="Transition{TState}.Speeds"/> field.</summary>
                public static SerializedProperty CurrentSpeeds { get; private set; }

                /// <summary>The <see cref="Transition{TState}.SynchroniseChildren"/> field.</summary>
                public static SerializedProperty CurrentSynchroniseChildren { get; private set; }

                private readonly Dictionary<string, ReorderableList>
                    PropertyPathToStates = new Dictionary<string, ReorderableList>();

                /************************************************************************************************************************/

                /// <summary>
                /// Gather the details of the `property`.
                /// <para></para>
                /// This method gets called by every <see cref="GetPropertyHeight"/> and <see cref="OnGUI"/> call since
                /// Unity uses the same <see cref="PropertyDrawer"/> instance for each element in a collection, so it
                /// needs to gather the details associated with the current property.
                /// </summary>
                protected virtual ReorderableList GatherDetails(SerializedProperty property)
                {
                    InitialiseMode(property);
                    GatherSubProperties(property);

                    var propertyPath = property.propertyPath;

                    if (!PropertyPathToStates.TryGetValue(propertyPath, out var states))
                    {
                        states = new ReorderableList(CurrentStates.serializedObject, CurrentStates)
                        {
                            drawHeaderCallback = DoStateListHeaderGUI,
                            elementHeightCallback = GetElementHeight,
                            drawElementCallback = DoElementGUI,
                            onAddCallback = OnAddElement,
                            onRemoveCallback = OnRemoveElement,
                            onReorderCallbackWithDetails = OnReorderList,
                        };

                        PropertyPathToStates.Add(propertyPath, states);
                    }

                    states.serializedProperty = CurrentStates;

                    return states;
                }

                /************************************************************************************************************************/

                /// <summary>
                /// Called every time a `property` is drawn to find the relevant child properties and store them to be
                /// used in <see cref="GetPropertyHeight"/> and <see cref="OnGUI"/>.
                /// </summary>
                protected virtual void GatherSubProperties(SerializedProperty property)
                {
                    CurrentProperty = property;
                    CurrentStates = property.FindPropertyRelative(StatesField);
                    CurrentSpeeds = property.FindPropertyRelative(SpeedsField);
                    CurrentSynchroniseChildren = property.FindPropertyRelative(SynchroniseChildrenField);

                    if (CurrentSpeeds.arraySize != 0)
                        CurrentSpeeds.arraySize = CurrentStates.arraySize;
                }

                /************************************************************************************************************************/

                /// <summary>
                /// Adds a menu item that will call <see cref="GatherSubProperties"/> then run the specified
                /// `function`.
                /// </summary>
                protected void AddPropertyModifierFunction(GenericMenu menu, string label,
                    Editor.MenuFunctionState state, Action<SerializedProperty> function)
                {
                    Editor.Serialization.AddPropertyModifierFunction(menu, CurrentProperty, label, state, (property) =>
                    {
                        GatherSubProperties(property);
                        function(property);
                    });
                }

                /// <summary>
                /// Adds a menu item that will call <see cref="GatherSubProperties"/> then run the specified
                /// `function`.
                /// </summary>
                protected void AddPropertyModifierFunction(GenericMenu menu, string label,
                    Action<SerializedProperty> function)
                {
                    Editor.Serialization.AddPropertyModifierFunction(menu, CurrentProperty, label, (property) =>
                    {
                        GatherSubProperties(property);
                        function(property);
                    });
                }

                /************************************************************************************************************************/

                /// <inheritdoc/>
                public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
                {
                    var height = EditorGUI.GetPropertyHeight(property, label);

                    if (property.isExpanded)
                    {
                        var states = GatherDetails(property);
                        height += Editor.AnimancerGUI.StandardSpacing + states.GetHeight();
                    }

                    return height;
                }

                /************************************************************************************************************************/

                /// <inheritdoc/>
                public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
                {
                    var originalProperty = property.Copy();

                    base.OnGUI(area, property, label);

                    if (!originalProperty.isExpanded)
                        return;

                    using (TransitionContext.Get(this, property))
                    {
                        if (Context.Transition == null)
                            return;

                        var states = GatherDetails(originalProperty);

                        var indentLevel = EditorGUI.indentLevel;

                        area.yMin = area.yMax - states.GetHeight();

                        EditorGUI.indentLevel++;
                        area = EditorGUI.IndentedRect(area);

                        EditorGUI.indentLevel = 0;
                        states.DoList(area);

                        EditorGUI.indentLevel = indentLevel;

                        TryCollapseArrays();
                    }
                }

                /************************************************************************************************************************/

                private static float _SpeedLabelWidth;
                private static float _SyncLabelWidth;

                /// <summary>Splits the specified `area` into separate sections.</summary>
                protected static void SplitListRect(Rect area, bool isHeader, out Rect animation, out Rect speed, out Rect sync)
                {
                    if (_SpeedLabelWidth == 0)
                        _SpeedLabelWidth = Editor.AnimancerGUI.CalculateWidth(EditorStyles.popup, "Speed");

                    if (_SyncLabelWidth == 0)
                        _SyncLabelWidth = Editor.AnimancerGUI.CalculateWidth(EditorStyles.popup, "Sync");

                    var spacing = Editor.AnimancerGUI.StandardSpacing;

                    var syncWidth = isHeader ?
                        _SyncLabelWidth :
                        Editor.AnimancerGUI.ToggleWidth - spacing;

                    var speedWidth = _SpeedLabelWidth + _SyncLabelWidth - syncWidth;

                    area.width += spacing;
                    sync = Editor.AnimancerGUI.StealFromRight(ref area, syncWidth, spacing);
                    speed = Editor.AnimancerGUI.StealFromRight(ref area, speedWidth, spacing);
                    animation = area;
                }

                /************************************************************************************************************************/
                #region Headers
                /************************************************************************************************************************/

                /// <summary>Draws the headdings of the state list.</summary>
                protected virtual void DoStateListHeaderGUI(Rect area)
                {
                    SplitListRect(area, true, out var animationArea, out var speedArea, out var syncArea);

                    DoAnimationHeaderGUI(animationArea);
                    DoSpeedHeaderGUI(speedArea);
                    DoSyncHeaderGUI(syncArea);
                }

                /************************************************************************************************************************/

                /// <summary>Draws an "Animation" header.</summary>
                protected static void DoAnimationHeaderGUI(Rect area)
                {
                    var content = Editor.AnimancerGUI.TempContent("Animation",
                        $"The {nameof(AnimationClip)}s or {nameof(ITransition)}s that will be used for each child state");
                    DoHeaderDropdownGUI(area, CurrentStates, content, null);
                }

                /************************************************************************************************************************/
                #region Speeds
                /************************************************************************************************************************/

                /// <summary>Draws a "Speed" header.</summary>
                protected void DoSpeedHeaderGUI(Rect area)
                {
                    var content = Editor.AnimancerGUI.TempContent("Speed",
                        "Determines how fast each child state plays (Default = 1)");
                    DoHeaderDropdownGUI(area, CurrentSpeeds, content, (menu) =>
                    {
                        AddPropertyModifierFunction(menu, "Reset All to 1",
                            CurrentSpeeds.arraySize == 0 ? Editor.MenuFunctionState.Selected : Editor.MenuFunctionState.Normal,
                            (_) => CurrentSpeeds.arraySize = 0);

                        AddPropertyModifierFunction(menu, "Normalize Durations", Editor.MenuFunctionState.Normal, NormalizeDurations);
                    });
                }

                /************************************************************************************************************************/

                /// <summary>
                /// Recalculates the <see cref="CurrentSpeeds"/> depending on the <see cref="AnimationClip.length"/> of
                /// their animations so that they all take the same amount of time to play fully.
                /// </summary>
                private static void NormalizeDurations(SerializedProperty property)
                {
                    var speedCount = CurrentSpeeds.arraySize;

                    var lengths = new float[CurrentStates.arraySize];
                    if (lengths.Length <= 1)
                        return;

                    int nonZeroLengths = 0;
                    float totalLength = 0;
                    float totalSpeed = 0;
                    for (int i = 0; i < lengths.Length; i++)
                    {
                        var state = CurrentStates.GetArrayElementAtIndex(i).objectReferenceValue;
                        if (AnimancerUtilities.TryGetLength(state, out var length) &&
                            length > 0)
                        {
                            nonZeroLengths++;
                            totalLength += length;
                            lengths[i] = length;

                            if (speedCount > 0)
                                totalSpeed += CurrentSpeeds.GetArrayElementAtIndex(i).floatValue;
                        }
                    }

                    if (nonZeroLengths == 0)
                        return;

                    var averageLength = totalLength / nonZeroLengths;
                    var averageSpeed = speedCount > 0 ? totalSpeed / nonZeroLengths : 1;

                    CurrentSpeeds.arraySize = lengths.Length;
                    InitialiseSpeeds(speedCount);

                    for (int i = 0; i < lengths.Length; i++)
                    {
                        if (lengths[i] == 0)
                            continue;

                        CurrentSpeeds.GetArrayElementAtIndex(i).floatValue = averageSpeed * lengths[i] / averageLength;
                    }

                    TryCollapseArrays();
                }

                /************************************************************************************************************************/

                /// <summary>
                /// Initialises every element in the <see cref="CurrentSpeeds"/> array from the `start` to the end of
                /// the array to contain a value of 1.
                /// </summary>
                public static void InitialiseSpeeds(int start)
                {
                    var count = CurrentSpeeds.arraySize;
                    while (start < count)
                        CurrentSpeeds.GetArrayElementAtIndex(start++).floatValue = 1;
                }

                /************************************************************************************************************************/
                #endregion
                /************************************************************************************************************************/
                #region Sync
                /************************************************************************************************************************/

                /// <summary>Draws a "Sync" header.</summary>
                protected void DoSyncHeaderGUI(Rect area)
                {
                    var content = Editor.AnimancerGUI.TempContent("Sync",
                        "Determines which child states have their normalized times constantly synchronised");
                    DoHeaderDropdownGUI(area, CurrentSpeeds, content, (menu) =>
                    {
                        var syncCount = CurrentSynchroniseChildren.arraySize;

                        var allState = syncCount == 0 ? Editor.MenuFunctionState.Selected : Editor.MenuFunctionState.Normal;
                        AddPropertyModifierFunction(menu, "All", allState,
                            (_) => CurrentSynchroniseChildren.arraySize = 0);

                        var syncNone = syncCount == CurrentStates.arraySize;
                        if (syncNone)
                        {
                            for (int i = 0; i < syncCount; i++)
                            {
                                if (CurrentSynchroniseChildren.GetArrayElementAtIndex(i).boolValue)
                                {
                                    syncNone = false;
                                    break;
                                }
                            }
                        }
                        var noneState = syncNone ? Editor.MenuFunctionState.Selected : Editor.MenuFunctionState.Normal;
                        AddPropertyModifierFunction(menu, "None", noneState, (_) =>
                        {
                            var count = CurrentSynchroniseChildren.arraySize = CurrentStates.arraySize;
                            for (int i = 0; i < count; i++)
                                CurrentSynchroniseChildren.GetArrayElementAtIndex(i).boolValue = false;
                        });

                        AddPropertyModifierFunction(menu, "Invert", Editor.MenuFunctionState.Normal, (_) =>
                        {
                            var count = CurrentSynchroniseChildren.arraySize;
                            for (int i = 0; i < count; i++)
                            {
                                var property = CurrentSynchroniseChildren.GetArrayElementAtIndex(i);
                                property.boolValue = !property.boolValue;
                            }

                            var newCount = CurrentSynchroniseChildren.arraySize = CurrentStates.arraySize;
                            for (int i = count; i < newCount; i++)
                                CurrentSynchroniseChildren.GetArrayElementAtIndex(i).boolValue = false;
                        });

                        AddPropertyModifierFunction(menu, "Non-Stationary", Editor.MenuFunctionState.Normal, (_) =>
                        {
                            var count = CurrentStates.arraySize;

                            for (int i = 0; i < count; i++)
                            {
                                var state = CurrentStates.GetArrayElementAtIndex(i).objectReferenceValue;
                                if (state == null)
                                    continue;

                                if (i >= syncCount)
                                {
                                    CurrentSynchroniseChildren.arraySize = i + 1;
                                    for (int j = syncCount; j < i; j++)
                                        CurrentSynchroniseChildren.GetArrayElementAtIndex(j).boolValue = true;
                                    syncCount = i + 1;
                                }

                                CurrentSynchroniseChildren.GetArrayElementAtIndex(i).boolValue =
                                    AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) &&
                                    velocity != Vector3.zero;
                            }

                            TryCollapseSync();
                        });
                    });
                }

                /************************************************************************************************************************/

                private static void SyncNone()
                {
                    var count = CurrentSynchroniseChildren.arraySize = CurrentStates.arraySize;
                    for (int i = 0; i < count; i++)
                        CurrentSynchroniseChildren.GetArrayElementAtIndex(i).boolValue = false;
                }

                /************************************************************************************************************************/
                #endregion
                /************************************************************************************************************************/

                /// <summary>Draws the GUI for a header dropdown button.</summary>
                public static void DoHeaderDropdownGUI(Rect area, SerializedProperty property, GUIContent content,
                    Action<GenericMenu> populateMenu)
                {
                    if (property != null)
                        EditorGUI.BeginProperty(area, GUIContent.none, property);

                    if (populateMenu != null)
                    {
                        if (EditorGUI.DropdownButton(area, content, FocusType.Passive))
                        {
                            var menu = new GenericMenu();
                            populateMenu(menu);
                            menu.ShowAsContext();
                        }
                    }
                    else
                    {
                        GUI.Label(area, content);
                    }

                    if (property != null)
                        EditorGUI.EndProperty();
                }

                /************************************************************************************************************************/
                #endregion
                /************************************************************************************************************************/

                /// <summary>Calculates the height of the state at the specified `index`.</summary>
                protected virtual float GetElementHeight(int index) => Editor.AnimancerGUI.LineHeight;

                /************************************************************************************************************************/

                /// <summary>Draws the GUI of the state at the specified `index`.</summary>
                private void DoElementGUI(Rect area, int index, bool isActive, bool isFocused)
                {
                    if (index < 0 || index > CurrentStates.arraySize)
                        return;

                    var state = CurrentStates.GetArrayElementAtIndex(index);
                    var speed = CurrentSpeeds.arraySize > 0 ? CurrentSpeeds.GetArrayElementAtIndex(index) : null;
                    DoElementGUI(area, index, state, speed);
                }

                /************************************************************************************************************************/

                /// <summary>Draws the GUI of the state at the specified `index`.</summary>
                protected virtual void DoElementGUI(Rect area, int index,
                    SerializedProperty state, SerializedProperty speed)
                {
                    SplitListRect(area, false, out var animationArea, out var speedArea, out var syncArea);

                    DoElementGUI(animationArea, speedArea, syncArea, index, state, speed);
                }

                /// <summary>Draws the GUI of the state at the specified `index`.</summary>
                protected void DoElementGUI(Rect animationArea, Rect speedArea, Rect syncArea, int index,
                    SerializedProperty state, SerializedProperty speed)
                {
                    DoClipOrTransitionField(animationArea, state, GUIContent.none);

                    if (speed != null)
                    {
                        EditorGUI.PropertyField(speedArea, speed, GUIContent.none);
                    }
                    else
                    {
                        EditorGUI.BeginProperty(speedArea, GUIContent.none, CurrentSpeeds);

                        var value = EditorGUI.FloatField(speedArea, 1);
                        if (value != 1)
                        {
                            CurrentSpeeds.InsertArrayElementAtIndex(0);
                            CurrentSpeeds.GetArrayElementAtIndex(0).floatValue = 1;
                            CurrentSpeeds.arraySize = CurrentStates.arraySize;
                            CurrentSpeeds.GetArrayElementAtIndex(index).floatValue = value;
                        }

                        EditorGUI.EndProperty();
                    }

                    DoSyncToggleGUI(syncArea, index);
                }

                /************************************************************************************************************************/

                /// <summary>
                /// Draws an <see cref="EditorGUI.ObjectField(Rect, GUIContent, Object, Type, bool)"/> that accepts
                /// <see cref="AnimationClip"/>s and <see cref="ITransition"/>s
                /// </summary>
                public static void DoClipOrTransitionField(Rect area, SerializedProperty property, GUIContent label)
                {
                    var targetObject = property.serializedObject.targetObject;
                    var oldReference = property.objectReferenceValue;

                    EditorGUI.BeginChangeCheck();
                    EditorGUI.ObjectField(area, property, label);
                    if (EditorGUI.EndChangeCheck())
                    {
                        var newReference = property.objectReferenceValue;
                        if (newReference == null || !IsClipOrTransition(newReference) || newReference == targetObject)
                            property.objectReferenceValue = oldReference;
                    }
                }

                /// <summary>Is the `clipOrTransition` an <see cref="AnimationClip"/> or <see cref="ITransition"/>?</summary>
                public static bool IsClipOrTransition(Object clipOrTransition)
                    => clipOrTransition is AnimationClip || clipOrTransition is ITransition;

                /************************************************************************************************************************/

                /// <summary>
                /// Draws a toggle to enable or disable <see cref="MixerState.SynchronisedChildren"/> for the child at
                /// the specified `index`.
                /// </summary>
                protected void DoSyncToggleGUI(Rect area, int index)
                {
                    var syncProperty = CurrentSynchroniseChildren;
                    var syncFlagCount = syncProperty.arraySize;

                    var enabled = true;

                    if (index < syncFlagCount)
                    {
                        syncProperty = syncProperty.GetArrayElementAtIndex(index);
                        enabled = syncProperty.boolValue;
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUI.BeginProperty(area, GUIContent.none, syncProperty);

                    enabled = GUI.Toggle(area, enabled, GUIContent.none);

                    EditorGUI.EndProperty();
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (index < syncFlagCount)
                        {
                            syncProperty.boolValue = enabled;
                        }
                        else
                        {
                            syncProperty.arraySize = index + 1;

                            for (int i = syncFlagCount; i < index; i++)
                            {
                                syncProperty.GetArrayElementAtIndex(i).boolValue = true;
                            }

                            syncProperty.GetArrayElementAtIndex(index).boolValue = enabled;
                        }
                    }
                }

                /************************************************************************************************************************/

                /// <summary>
                /// Called when adding a new state to the list to ensure that any other relevant arrays have new
                /// elements added as well.
                /// </summary>
                protected virtual void OnAddElement(ReorderableList list)
                {
                    var index = CurrentStates.arraySize;
                    CurrentStates.InsertArrayElementAtIndex(index);

                    if (CurrentSpeeds.arraySize > 0)
                        CurrentSpeeds.InsertArrayElementAtIndex(index);
                }

                /************************************************************************************************************************/

                /// <summary>
                /// Called when removing a state from the list to ensure that any other relevant arrays have elements
                /// removed as well.
                /// </summary>
                protected virtual void OnRemoveElement(ReorderableList list)
                {
                    var index = list.index;

                    Editor.Serialization.RemoveArrayElement(CurrentStates, index);

                    if (CurrentSpeeds.arraySize > 0)
                        Editor.Serialization.RemoveArrayElement(CurrentSpeeds, index);
                }

                /************************************************************************************************************************/

                /// <summary>
                /// Called when reordering states in the list to ensure that any other relevant arrays have their
                /// corresponding elements reordered as well.
                /// </summary>
                protected virtual void OnReorderList(ReorderableList list, int oldIndex, int newIndex)
                {
                    CurrentSpeeds.MoveArrayElement(oldIndex, newIndex);

                    var syncCount = CurrentSynchroniseChildren.arraySize;
                    if (Math.Max(oldIndex, newIndex) >= syncCount)
                    {
                        CurrentSynchroniseChildren.arraySize++;
                        CurrentSynchroniseChildren.GetArrayElementAtIndex(syncCount).boolValue = true;
                        CurrentSynchroniseChildren.arraySize = newIndex + 1;
                    }

                    CurrentSynchroniseChildren.MoveArrayElement(oldIndex, newIndex);
                }

                /************************************************************************************************************************/

                /// <summary>
                /// Calls <see cref="TryCollapseSpeeds"/> and <see cref="TryCollapseSync"/>.
                /// </summary>
                public static void TryCollapseArrays()
                {
                    TryCollapseSpeeds();
                    TryCollapseSync();
                }

                /************************************************************************************************************************/

                /// <summary>
                /// If every element in the <see cref="CurrentSpeeds"/> array is 1, this method sets the array size to 0.
                /// </summary>
                public static void TryCollapseSpeeds()
                {
                    var property = CurrentSpeeds;
                    var speedCount = property.arraySize;
                    if (speedCount <= 0)
                        return;

                    for (int i = 0; i < speedCount; i++)
                    {
                        if (property.GetArrayElementAtIndex(i).floatValue != 1)
                            return;
                    }

                    property.arraySize = 0;
                }

                /************************************************************************************************************************/

                /// <summary>
                /// Removes any true elements from the end of the <see cref="CurrentSynchroniseChildren"/> array.
                /// </summary>
                public static void TryCollapseSync()
                {
                    var property = CurrentSynchroniseChildren;
                    var count = property.arraySize;
                    var changed = false;

                    for (int i = count - 1; i >= 0; i--)
                    {
                        if (property.GetArrayElementAtIndex(i).boolValue)
                        {
                            count = i;
                            changed = true;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (changed)
                        property.arraySize = count;
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

