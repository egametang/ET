// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws the Inspector GUI for a <see cref="AnimancerState.Transition{TState}"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/TransitionDrawer
    /// 
    [CustomPropertyDrawer(typeof(AnimancerState.Transition<>), true)]
    public class TransitionDrawer : PropertyDrawer
    {
        /************************************************************************************************************************/

        /// <summary>The visual state of a drawer.</summary>
        private enum Mode
        {
            Uninitialised,
            Normal,
            AlwaysExpanded,
        }

        /// <summary>The current state of this drawer.</summary>
        private Mode _Mode;

        /************************************************************************************************************************/

        /// <summary>
        /// If set, the field with this name will be drawn with the foldout arrow instead of in its default place.
        /// </summary>
        protected readonly string MainPropertyName;

        /// <summary>"." + <see cref="MainPropertyName"/> (to avoid creating garbage repeatedly).</summary>
        protected readonly string MainPropertyPathSuffix;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="TransitionDrawer"/>.</summary>
        public TransitionDrawer() { }

        /// <summary>Creates a new <see cref="TransitionDrawer"/> and sets the <see cref="MainPropertyName"/>.</summary>
        public TransitionDrawer(string mainPropertyName)
        {
            MainPropertyName = mainPropertyName;
            MainPropertyPathSuffix = "." + mainPropertyName;
        }

        /************************************************************************************************************************/

        /// <summary>Returns the property specified by the <see cref="MainPropertyName"/>.</summary>
        private SerializedProperty GetMainProperty(SerializedProperty rootProperty)
        {
            if (MainPropertyName == null)
                return null;
            else
                return rootProperty.FindPropertyRelative(MainPropertyName);
        }

        /************************************************************************************************************************/

        /// <summary>Returns the number of vertical pixels the `property` will occupy when it is drawn.</summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            InitialiseMode(property);

            var height = EditorGUI.GetPropertyHeight(property, label, true);

            if (property.isExpanded)
            {
                var mainProperty = GetMainProperty(property);
                if (mainProperty != null)
                    height -= EditorGUI.GetPropertyHeight(mainProperty) + AnimancerGUI.StandardSpacing;

                var endTime = property.FindPropertyRelative(NormalizedStartTimeFieldName);
                if (endTime != null)
                    height += AnimancerGUI.LineHeight + AnimancerGUI.StandardSpacing;
            }

            return height;
        }

        /************************************************************************************************************************/

        /// <summary>Draws the root `property` GUI and calls <see cref="DoPropertyGUI"/> for each of its children.</summary>
        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            InitialiseMode(property);

            using (TransitionContext.Get(this, property))
            {
                if (Context.Transition == null)
                    return;

                // Highlight the area if this transition is currently being previewed.
                var isPreviewing = TransitionPreviewWindow.IsPreviewingCurrentProperty();
                if (isPreviewing)
                {
                    var highlightArea = area;
                    highlightArea.xMin -= AnimancerGUI.IndentSize;
                    EditorGUI.DrawRect(highlightArea, new Color(0.35f, 0.5f, 1, 0.2f));
                }

                DoHeaderGUI(area, property, label, isPreviewing, out var headerHeight);
                DoChildPropertiesGUI(area, headerHeight, property);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="_Mode"/> is <see cref="Mode.Uninitialised"/>, this method determines how it should start
        /// based on the number of properties in the `serializedObject`. If the only serialized field is an
        /// <see cref="AnimancerState.Transition{TState}"/> then it should start expanded.
        /// </summary>
        protected void InitialiseMode(SerializedProperty property)
        {
            if (_Mode == Mode.Uninitialised)
            {
                _Mode = Mode.AlwaysExpanded;

                var iterator = property.serializedObject.GetIterator();
                iterator.Next(true);

                var count = 0;
                do
                {
                    switch (iterator.propertyPath)
                    {
                        case "m_ObjectHideFlags":
                        case "m_Script":
                            break;

                        default:
                            count++;
                            if (count > 1)
                            {
                                _Mode = Mode.Normal;
                                return;
                            }
                            break;
                    }
                }
                while (iterator.NextVisible(false));
            }

            if (_Mode == Mode.AlwaysExpanded)
                property.isExpanded = true;
        }

        /************************************************************************************************************************/

        private void DoHeaderGUI(Rect area, SerializedProperty property, GUIContent label, bool isPreviewing, out float height)
        {
            area.height = AnimancerGUI.LineHeight;

            DoPreviewButtonGUI(ref area, property, isPreviewing);

            label.text = AnimancerGUI.GetNarrowText(label.text);

            var mainProperty = GetMainProperty(property);
            if (mainProperty != null)
            {
                var mainPropertyReferenceIsMissing =
                    mainProperty.propertyType == SerializedPropertyType.ObjectReference &&
                    mainProperty.objectReferenceValue == null;

                DoPropertyGUI(ref area, property, mainProperty, label);

                // If the main Object reference was just assigned and all fields were at their type default,
                // reset the value to run its default constructor and field initialisers then reassign the reference.
                var reference = mainProperty.objectReferenceValue;
                if (mainPropertyReferenceIsMissing && reference != null)
                {
                    mainProperty.objectReferenceValue = null;
                    if (Serialization.IsDefaultValueByType(property))
                        Serialization.ResetValue(property);
                    mainProperty.objectReferenceValue = reference;
                }

                if (_Mode != Mode.AlwaysExpanded)
                {
                    var hierarchyMode = EditorGUIUtility.hierarchyMode;
                    EditorGUIUtility.hierarchyMode = true;

                    property.isExpanded = EditorGUI.Foldout(area, property.isExpanded, GUIContent.none, true);

                    EditorGUIUtility.hierarchyMode = hierarchyMode;
                }
            }
            else
            {
                area.height = EditorGUI.GetPropertyHeight(property, label, false);
                if (_Mode != Mode.AlwaysExpanded)
                {
                    EditorGUI.PropertyField(area, property, label, false);
                }
                else
                {
                    label = EditorGUI.BeginProperty(area, label, property);
                    EditorGUI.LabelField(area, label);
                    EditorGUI.EndProperty();
                }
            }

            height = area.height;
        }

        /************************************************************************************************************************/

        private void DoPreviewButtonGUI(ref Rect area, SerializedProperty property, bool wasPreviewing)
        {
            if (property.serializedObject.targetObjects.Length != 1 ||
                !TransitionPreviewWindow.CanBePreviewed(property))
                return;

            var buttonArea = AnimancerGUI.StealFromRight(ref area,
                area.height + AnimancerGUI.StandardSpacing * 2, AnimancerGUI.StandardSpacing);

            var content = AnimancerGUI.TempContent("", "Preview this transition");
            content.image = TransitionPreviewWindow.Icon;

            var style = ObjectPool.GetCachedResult(() => new GUIStyle(AnimancerGUI.MiniButton)
            {
#if UNITY_2019_3_OR_NEWER
                padding = new RectOffset(0, 0, 0, 1),
#else
                padding = new RectOffset(),
#endif
                fixedWidth = 0,
                fixedHeight = 0,
            });

            var isPrevewing = GUI.Toggle(buttonArea, wasPreviewing, content, style);
            if (wasPreviewing != isPrevewing)
                TransitionPreviewWindow.Open(property, isPrevewing);

            content.image = null;
        }

        /************************************************************************************************************************/

        private void DoChildPropertiesGUI(Rect area, float headerHeight, SerializedProperty property)
        {
            if (!property.isExpanded && _Mode != Mode.AlwaysExpanded)
                return;

            area.y += headerHeight + AnimancerGUI.StandardSpacing;

            EditorGUI.indentLevel++;

            var rootProperty = property;
            property = property.Copy();

            SerializedProperty eventsProperty = null;

            var depth = property.depth;
            property.NextVisible(true);
            while (property.depth > depth)
            {
                // Grab the Events property and draw it last.
                if (eventsProperty == null && property.propertyPath.EndsWith("._Events"))
                {
                    eventsProperty = property.Copy();
                }
                else if (MainPropertyPathSuffix == null || !property.propertyPath.EndsWith(MainPropertyPathSuffix))
                {
                    var label = AnimancerGUI.TempContent(property);
                    DoPropertyGUI(ref area, rootProperty, property, label);
                    AnimancerGUI.NextVerticalArea(ref area);
                }

                if (!property.NextVisible(false))
                    break;
            }

            if (eventsProperty != null)
            {
                var label = AnimancerGUI.TempContent(eventsProperty);
                DoPropertyGUI(ref area, rootProperty, eventsProperty, label);
            }

            EditorGUI.indentLevel--;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Draws the `property` GUI in relation to the `rootProperty` which was passed into <see cref="OnGUI"/>.
        /// </summary>
        protected virtual void DoPropertyGUI(ref Rect area, SerializedProperty rootProperty,
            SerializedProperty property, GUIContent label)
        {
            // If we keep using the GUIContent that was passed into OnGUI then GetPropertyHeight will change it to
            // match the 'property' which we don't want.
            label = AnimancerGUI.TempContent(label.text, label.tooltip, false);

            area.height = EditorGUI.GetPropertyHeight(property, label, true);

            if (property.propertyPath.EndsWith("._FadeDuration"))
            {
                var length = Context.MaximumDuration;
                AnimancerGUI.DoOptionalTimeField(ref area, label, property, false, length,
                    AnimancerPlayable.DefaultFadeDuration, false);

                if (property.floatValue < 0)
                    property.floatValue = 0;

                return;
            }

            if (property.propertyPath.EndsWith("._Speed"))
            {
                AnimancerGUI.DoOptionalTimeField(ref area, label, property, true, float.NaN, 1);
                return;
            }

            if (TryDoStartTimeField(ref area, rootProperty, property, label))
                return;

            EditorGUI.PropertyField(area, property, label, true);
        }

        /************************************************************************************************************************/

        /// <summary>The name of the backing field of <see cref="ClipState.Transition.NormalizedStartTime"/>.</summary>
        public const string NormalizedStartTimeFieldName = "_NormalizedStartTime";

        /// <summary>
        /// If the `property` is a "Start Time" field, this method draws it as well as the "End Time" below it and
        /// returns true.
        /// </summary>
        public static bool TryDoStartTimeField(ref Rect area, SerializedProperty rootProperty,
            SerializedProperty property, GUIContent label)
        {
            if (!property.propertyPath.EndsWith("." + NormalizedStartTimeFieldName))
                return false;

            // Start Time.
            label.text = AnimancerGUI.GetNarrowText("Start Time");
            var length = Context.MaximumDuration;
            var defaultStartTime = AnimancerEvent.Sequence.GetDefaultNormalizedStartTime(Context.Transition.Speed);
            AnimancerGUI.DoOptionalTimeField(ref area, label, property, true, length, defaultStartTime);

            AnimancerGUI.NextVerticalArea(ref area);

            // End Time.
            var events = rootProperty.FindPropertyRelative("_Events");
            using (var context = EventSequenceDrawer.Context.Get(events))
            {
                var areaCopy = area;
                var index = Mathf.Max(0, context.Times.Count - 1);
                EventSequenceDrawer.DoTimeGUI(ref areaCopy, context, index, true);
            }

            return true;
        }

        /************************************************************************************************************************/
        #region Context
        /************************************************************************************************************************/

        /// <summary>The current <see cref="TransitionContext"/>.</summary>
        public static TransitionContext Context { get; private set; }

        /************************************************************************************************************************/

        /// <summary>Details of an <see cref="AnimancerState.Transition{T}"/>.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer.Editor/TransitionContext
        /// 
        public sealed class TransitionContext : IDisposable
        {
            /************************************************************************************************************************/

            /// <summary>The main property representing the <see cref="AnimancerState.Transition{T}"/> field.</summary>
            public SerializedProperty Property { get; private set; }

            /// <summary>The actual transition object rerieved from the <see cref="Property"/>.</summary>
            public ITransitionDetailed Transition { get; private set; }

            /// <summary>The cached value of <see cref="ITransitionDetailed.MaximumDuration"/>.</summary>
            public float MaximumDuration { get; private set; }

            /************************************************************************************************************************/

            private static readonly TransitionContext Instance = new TransitionContext();

            private TransitionContext() { }

            /// <summary>
            /// Returns a disposable <see cref="TransitionContext"/> representing the specified parameters.
            /// <para></para>
            /// Note that the same instance is returned every time and it can be accessed via <see cref="Context"/>.
            /// </summary>
            public static IDisposable Get(TransitionDrawer drawer, SerializedProperty transitionProperty)
            {
                Debug.Assert(Context == null,
                    "Cannot get a new context before the previous one is disposed.");

                Instance.Property = transitionProperty;
                Instance.Transition = transitionProperty.GetValue<ITransitionDetailed>();
                Instance.MaximumDuration = Instance.Transition != null ? Instance.Transition.MaximumDuration : 0;

                EditorGUI.BeginChangeCheck();

                return Context = Instance;
            }

            /************************************************************************************************************************/

            /// <summary>Clears the <see cref="Context"/>.</summary>
            public void Dispose()
            {
                if (EditorGUI.EndChangeCheck())
                    Instance.Property.serializedObject.ApplyModifiedProperties();

                Context = null;
                Instance.Property = null;
                Instance.Transition = null;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

