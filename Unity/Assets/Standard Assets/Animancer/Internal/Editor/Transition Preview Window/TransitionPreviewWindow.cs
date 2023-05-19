// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] [Pro-Only]
    /// An <see cref="EditorWindow"/> which allows the user to preview animation transitions separately from the rest
    /// of the scene in Edit Mode or Play Mode.
    /// </summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions#previews">Previews</see>
    /// </remarks>
    internal sealed partial class TransitionPreviewWindow : EditorWindow
    {
        /************************************************************************************************************************/
        #region Public API
        /************************************************************************************************************************/

        private static Texture _Icon;

        /// <summary>The icon image used by this window.</summary>
        public static Texture Icon
        {
            get
            {
#if UNITY_2019_3_OR_NEWER
                const string IconName = "ViewToolOrbit";
#else
                const string IconName = "UnityEditor.LookDevView";
#endif
                // Possible icons: "UnityEditor.LookDevView", "SoftlockInline", "ViewToolOrbit".

                if (_Icon == null)
                    _Icon = EditorGUIUtility.IconContent(IconName).image;
                return _Icon;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Focusses the <see cref="TransitionPreviewWindow"/> or creates one if none exists.</summary>
        public static void Open(SerializedProperty transitionProperty, bool open)
        {
            if (open)
            {
                GetWindow<TransitionPreviewWindow>(typeof(SceneView))
                    .SetTargetProperty(transitionProperty);
            }
            else if (IsPreviewingCurrentProperty())
            {
                EditorApplication.delayCall += _Instance.Close;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The <see cref="AnimancerState.NormalizedTime"/> of the current transition. Can only be set if the property
        /// being previewed matches the current <see cref="TransitionDrawer.Context"/>.
        /// </summary>
        public static float PreviewNormalizedTime
        {
            get => _Instance._Scene.NormalizedTime;
            set
            {
                if (float.IsNaN(value) ||
                    !IsPreviewingCurrentProperty())
                    return;

                _Instance._Scene.NormalizedTime = value;
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the <see cref="AnimancerState"/> of the current transition if the property being previewed matches
        /// the <see cref="TransitionDrawer.Context"/>. Otherwise returns null.
        /// </summary>
        public static AnimancerState GetCurrentState()
        {
            if (!IsPreviewingCurrentProperty() ||
                _Instance._Scene.InstanceAnimancer == null)
                return null;

            var transition = _Instance.GetTransition();
            _Instance._Scene.InstanceAnimancer.States.TryGet(transition, out var state);
            return state;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Indicates whether the current <see cref="TransitionDrawer.TransitionContext.Property"/> is being previewed
        /// at the moment.
        /// </summary>
        public static bool IsPreviewingCurrentProperty()
        {
            return
                _Instance != null &&
                TransitionDrawer.Context != null &&
                _Instance._TransitionProperty.IsValid() &&
                Serialization.AreSameProperty(TransitionDrawer.Context.Property, _Instance._TransitionProperty);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Messages
        /************************************************************************************************************************/

        private static TransitionPreviewWindow _Instance;

        [SerializeField] private Inspector _Inspector;
        [SerializeField] private Scene _Scene;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            _Instance = this;
            titleContent = new GUIContent("Transition Preview", Icon);
            autoRepaintOnSceneChange = true;

            AnimancerUtilities.NewIfNull(ref _Inspector);
            AnimancerUtilities.NewIfNull(ref _Scene);

            if (_TransitionProperty.IsValid() &&
                !CanBePreviewed(_TransitionProperty))
            {
                DestroyTransitionProperty();
            }

            _Scene.OnEnable();
        }

        /************************************************************************************************************************/

        private void OnDisable()
        {
            _Scene.OnDisable();
            _Instance = null;
        }

        /************************************************************************************************************************/

        private void OnDestroy()
        {
            _Scene.OnDestroy();
            DestroyTransitionProperty();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        /************************************************************************************************************************/

        private void OnGUI()
        {
            // Maximising then un-maximising the window causes it to lose the _Instance for some reason.
            _Instance = this;

            GUILayout.BeginHorizontal();
            {
                _Scene.DoPreviewGUI();
                _Inspector.DoInspectorGUI();
            }
            GUILayout.EndHorizontal();
        }

        /************************************************************************************************************************/

        private void Update()
        {
            _Instance = this;

            if (Settings.AutoClose && !_TransitionProperty.IsValid())
            {
                Close();
                return;
            }

            _Scene.Update();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Transition Property
        /************************************************************************************************************************/

        [SerializeField] private Serialization.PropertyReference _TransitionProperty;

        /************************************************************************************************************************/

        /// <summary>Indicates whether the `property` is able to be previewed by this system.</summary>
        public static bool CanBePreviewed(SerializedProperty property)
        {
            var type = property.GetAccessor().FieldType;
            return typeof(ITransitionDetailed).IsAssignableFrom(type);
        }

        /************************************************************************************************************************/

        private void SetTargetProperty(SerializedProperty property)
        {
            if (property.serializedObject.targetObjects.Length != 1)
            {
                Close();
                throw new ArgumentException($"{nameof(TransitionPreviewWindow)} does not support multi-object selection.");
            }

            if (!CanBePreviewed(property))
            {
                Close();
                throw new ArgumentException($"The specified property does not implement {nameof(ITransitionDetailed)}.");
            }

            DestroyTransitionProperty();

            _TransitionProperty = property;
            _Scene.OnTargetPropertyChanged();
        }

        /************************************************************************************************************************/

        private ITransitionDetailed GetTransition()
        {
            if (!_TransitionProperty.IsValid())
                return null;

            return _TransitionProperty.Property.GetValue<ITransitionDetailed>();
        }

        /************************************************************************************************************************/

        private void DestroyTransitionProperty()
        {
            if (_TransitionProperty == null)
                return;

            _Scene.DestroyModelInstance();

            _TransitionProperty.Dispose();
            _TransitionProperty = null;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

