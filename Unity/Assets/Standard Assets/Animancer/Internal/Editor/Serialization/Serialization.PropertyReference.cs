// Serialization // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// Shared File Last Modified: 2020-05-17.
namespace Animancer.Editor
// namespace InspectorGadgets.Editor
{
    /// <summary>[Editor-Only] Various serialization utilities.</summary>
    public partial class Serialization
    {
        /// <summary>[Editor-Only]
        /// A serializable reference to a <see cref="SerializedProperty"/>.
        /// </summary>
        [Serializable]
        public sealed class PropertyReference
        {
            /************************************************************************************************************************/

            [SerializeField] private ObjectReference[] _TargetObjects;

            /// <summary>[<see cref="SerializeField"/>] The <see cref="SerializedObject.targetObject"/>.</summary>
            public ObjectReference TargetObject
            {
                get
                {
                    return _TargetObjects != null && _TargetObjects.Length > 0 ?
                        _TargetObjects[0] : null;
                }
            }

            /// <summary>[<see cref="SerializeField"/>] The <see cref="SerializedObject.targetObjects"/>.</summary>
            public ObjectReference[] TargetObjects => _TargetObjects;

            /************************************************************************************************************************/

            [SerializeField] private ObjectReference _Context;

            /// <summary>[<see cref="SerializeField"/>] The <see cref="SerializedObject.context"/>.</summary>
            public ObjectReference Context => _Context;

            /************************************************************************************************************************/

            [SerializeField] private string _PropertyPath;

            /// <summary>[<see cref="SerializeField"/>] The <see cref="SerializedProperty.propertyPath"/>.</summary>
            public string PropertyPath => _PropertyPath;

            /************************************************************************************************************************/

            [NonSerialized] private bool _IsInitialised;

            /// <summary>Indicates whether the <see cref="Property"/> has been accessed.</summary>
            public bool IsInitialised => _IsInitialised;

            /************************************************************************************************************************/

            [NonSerialized] private SerializedProperty _Property;

            /// <summary>[<see cref="SerializeField"/>] The referenced <see cref="SerializedProperty"/>.</summary>
            public SerializedProperty Property
            {
                get
                {
                    Initialise();
                    return _Property;
                }
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="PropertyReference"/> which wraps the specified `property`.
            /// </summary>
            public PropertyReference(SerializedProperty property)
            {
                _TargetObjects = ObjectReference.Convert(property.serializedObject.targetObjects);

                _Context = property.serializedObject.context;
                _PropertyPath = property.propertyPath;

                // Don't set the _Property. If it gets accessed we want to create out own instance.
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="PropertyReference"/> which wraps the specified `property`.
            /// </summary>
            public static implicit operator PropertyReference(SerializedProperty property) => new PropertyReference(property);

            /// <summary>
            /// Returns the target <see cref="Property"/>.
            /// </summary>
            public static implicit operator SerializedProperty(PropertyReference reference) => reference.Property;

            /************************************************************************************************************************/

            private void Initialise()
            {
                if (_IsInitialised)
                {
                    if (!TargetsExist)
                        Dispose();
                    return;
                }

                _IsInitialised = true;

                if (string.IsNullOrEmpty(_PropertyPath) ||
                    _TargetObjects.Length == 0 ||
                    !TargetsExist)
                    return;

                var targetObjects = ObjectReference.Convert(_TargetObjects);
                var serializedObject = new SerializedObject(targetObjects, _Context);
                _Property = serializedObject.FindProperty(_PropertyPath);
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Returns true if the specified property and objects match the targets of this reference.
            /// </summary>
            public bool IsTarget(SerializedProperty property, Object[] targetObjects)
            {
                if (_Property == null ||
                    _Property.propertyPath != property.propertyPath ||
                    _TargetObjects.Length != targetObjects.Length)
                    return false;

                for (int i = 0; i < _TargetObjects.Length; i++)
                {
                    if (_TargetObjects[i] != targetObjects[i])
                        return false;
                }

                return true;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Returns true if there is at least one target and none of them are null.
            /// </summary>
            private bool TargetsExist
            {
                get
                {
                    if (_TargetObjects.Length == 0)
                        return false;

                    for (int i = 0; i < _TargetObjects.Length; i++)
                    {
                        if (_TargetObjects[i].Object == null)
                            return false;
                    }

                    return true;
                }
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Calls <see cref="SerializedObject.Update"/> if the <see cref="Property"/> has been initialised.
            /// </summary>
            public void Update()
            {
                if (_Property == null)
                    return;

                if (!TargetsExist)
                {
                    Dispose();
                    return;
                }

                _Property.serializedObject.Update();
            }

            /// <summary>
            /// Calls <see cref="SerializedObject.ApplyModifiedProperties"/> if the <see cref="Property"/> has been initialised.
            /// </summary>
            public void ApplyModifiedProperties()
            {
                if (_Property == null)
                    return;

                if (!TargetsExist)
                {
                    Dispose();
                    return;
                }

                _Property.serializedObject.ApplyModifiedProperties();
            }

            /// <summary>
            /// Calls <see cref="SerializedObject.Dispose"/> if the <see cref="Property"/> has been initialised.
            /// </summary>
            public void Dispose()
            {
                if (_Property != null)
                {
                    _Property.serializedObject.Dispose();
                    _Property = null;
                }
            }

            /************************************************************************************************************************/

            /// <summary>Gets the height needed to draw the target property.</summary>
            public float GetPropertyHeight()
            {
                if (_Property == null)
                    return 0;

                return EditorGUI.GetPropertyHeight(_Property, _Property.isExpanded);
            }

            /************************************************************************************************************************/

            /// <summary>Draws the target object within the specified `area`.</summary>
            public void DoTargetGUI(Rect area)
            {
                area.height = EditorGUIUtility.singleLineHeight;

                Initialise();

                if (_Property == null)
                {
                    GUI.Label(area, "Missing " + this);
                    return;
                }

                var targets = _Property.serializedObject.targetObjects;

                var enabled = GUI.enabled;
                GUI.enabled = false;

                var showMixedValue = EditorGUI.showMixedValue;
                EditorGUI.showMixedValue = targets.Length > 1;

                var target = targets.Length > 0 ? targets[0] : null;
                EditorGUI.ObjectField(area, target, typeof(Object), true);

                EditorGUI.showMixedValue = showMixedValue;
                GUI.enabled = enabled;
            }

            /************************************************************************************************************************/

            /// <summary>Draws the target property within the specified `area`.</summary>
            public void DoPropertyGUI(Rect area)
            {
                Initialise();

                if (_Property == null)
                    return;

                _Property.serializedObject.Update();

                GUI.BeginGroup(area);
                area.x = area.y = 0;

                EditorGUI.PropertyField(area, _Property, _Property.isExpanded);

                GUI.EndGroup();

                _Property.serializedObject.ApplyModifiedProperties();
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/

        /// <summary>Returns true if the `reference` and <see cref="PropertyReference.Property"/> are not null.</summary>
        public static bool IsValid(this PropertyReference reference) => reference?.Property != null;

        /************************************************************************************************************************/
    }
}

#endif
