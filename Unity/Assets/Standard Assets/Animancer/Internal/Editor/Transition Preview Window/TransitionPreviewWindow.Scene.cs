// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    internal partial class TransitionPreviewWindow
    {
        /// <summary>Temporary scene management for the <see cref="TransitionPreviewWindow"/>.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions#previews">Previews</see>
        /// </remarks>
        [Serializable]
        private sealed class Scene
        {
            /************************************************************************************************************************/

            /// <summary><see cref="HideFlags.HideAndDontSave"/> without <see cref="HideFlags.NotEditable"/>.</summary>
            private const HideFlags HideAndDontSave = HideFlags.HideInHierarchy | HideFlags.DontSave;

            [NonSerialized] private PreviewRenderUtility _PreviewRenderUtility;

            [SerializeField] private Transform _PreviewSceneRoot;
            [SerializeField] private Transform _InstanceRoot;

            /************************************************************************************************************************/

            [SerializeField]
            private Transform _OriginalRoot;

            public Transform OriginalRoot
            {
                get => _OriginalRoot;
                set
                {
                    _OriginalRoot = value;
                    InstantiateModel();

                    if (value == null)
                        return;

                    var gameObject = value.gameObject;

                    if (gameObject == DefaultHumanoid ||
                        !EditorUtility.IsPersistent(gameObject))
                        return;

                    var models = Settings.Models;
                    var index = models.LastIndexOf(gameObject);
                    if (index >= 0 && index < models.Count - 1)
                        models.RemoveAt(index);
                    models.Add(gameObject);
                    AnimancerSettings.SetDirty();
                }
            }

            /************************************************************************************************************************/

            private static GameObject _DefaultHumanoid;

            public static GameObject DefaultHumanoid
            {
                get
                {
                    if (_DefaultHumanoid == null)
                    {
                        // Try to load Animancer's DefaultHumanoid.
                        var path = AssetDatabase.GUIDToAssetPath("c9f3e1113795a054c939de9883b31fed");
                        if (!string.IsNullOrEmpty(path))
                        {
                            _DefaultHumanoid = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                            if (_DefaultHumanoid != null)
                                return _DefaultHumanoid;
                        }

                        // Otherwise try to load Unity's DefaultAvatar.
                        _DefaultHumanoid = EditorGUIUtility.Load("Avatar/DefaultAvatar.fbx") as GameObject;

                        if (_DefaultHumanoid == null)
                        {
                            // Otherwise just create an empty object.
                            _DefaultHumanoid = EditorUtility.CreateGameObjectWithHideFlags(
                                "DefaultAvatar", HideFlags.HideAndDontSave, typeof(Animator));
                            _DefaultHumanoid.transform.parent = _Instance._Scene._PreviewSceneRoot;
                        }
                    }

                    return _DefaultHumanoid;
                }
            }

            public static bool IsDefaultHumanoid(GameObject gameObject) => gameObject == _DefaultHumanoid;

            /************************************************************************************************************************/

            private static GameObject _DefaultSprite;

            public static GameObject DefaultSprite
            {
                get
                {
                    if (_DefaultSprite == null)
                    {
                        _DefaultSprite = EditorUtility.CreateGameObjectWithHideFlags(
                            "DefaultSprite", HideFlags.HideAndDontSave, typeof(Animator), typeof(SpriteRenderer));
                        _DefaultSprite.transform.parent = _Instance._Scene._PreviewSceneRoot;
                    }

                    return _DefaultSprite;
                }
            }

            public static bool IsDefaultSprite(GameObject gameObject) => gameObject == _DefaultSprite;

            /************************************************************************************************************************/

            [SerializeField]
            private Animator[] _InstanceAnimators;
            public Animator[] InstanceAnimators => _InstanceAnimators;

            [SerializeField] private int _SelectedInstanceAnimator;
            [NonSerialized] private AnimationType _SelectedInstanceType;

            public Animator SelectedInstanceAnimator
            {
                get
                {
                    if (_InstanceAnimators == null ||
                        _InstanceAnimators.Length == 0)
                        return null;

                    if (_SelectedInstanceAnimator > _InstanceAnimators.Length)
                        _SelectedInstanceAnimator = _InstanceAnimators.Length;

                    return _InstanceAnimators[_SelectedInstanceAnimator];
                }
            }

            /************************************************************************************************************************/

            [NonSerialized]
            private AnimancerPlayable _InstanceAnimancer;
            public AnimancerPlayable InstanceAnimancer
            {
                get
                {
                    if ((_InstanceAnimancer == null || !_InstanceAnimancer.IsValid) &&
                        _InstanceRoot != null)
                    {
                        var animator = SelectedInstanceAnimator;
                        if (animator != null)
                        {
                            AnimancerPlayable.SetNextGraphName(animator.name + " (Animancer Preview)");
                            _InstanceAnimancer = AnimancerPlayable.Create();
                            _InstanceAnimancer.SetOutput(
                                new AnimancerEditorUtilities.DummyAnimancerComponent(animator, _InstanceAnimancer));
                            NormalizedTime = _NormalizedTime;
                        }
                    }

                    return _InstanceAnimancer;
                }
            }

            /************************************************************************************************************************/

            public void OnEnable()
            {
                if (_PreviewRenderUtility == null)
                    _PreviewRenderUtility = new PreviewRenderUtility();

                if (_PreviewSceneRoot == null)
                {
                    _PreviewSceneRoot = EditorUtility.CreateGameObjectWithHideFlags(
                        $"{nameof(Animancer)}.{nameof(TransitionPreviewWindow)}", HideAndDontSave).transform;
                    _PreviewRenderUtility.AddSingleGO(_PreviewSceneRoot.gameObject);

                    const float Grey = 0.15f;
                    _PreviewRenderUtility.ambientColor = new Color(Grey, Grey, Grey, 0f);
                }

                UnityEditor.SceneManagement.EditorSceneManager.sceneOpening += OnSceneOpening;
                EditorApplication.playModeStateChanged += OnPlayModeChanged;

                InitialiseDefaultRoot();
            }

            /************************************************************************************************************************/

            public void OnDisable()
            {
                UnityEditor.SceneManagement.EditorSceneManager.sceneOpening -= OnSceneOpening;
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;

                DestroyAnimancerInstance();

                if (_PreviewRenderUtility != null)
                {
                    _PreviewRenderUtility.Cleanup();
                    _PreviewRenderUtility = null;
                }
            }

            /************************************************************************************************************************/

            public void OnDestroy()
            {
                if (_PreviewSceneRoot != null)
                {
                    DestroyImmediate(_PreviewSceneRoot.gameObject);
                    _PreviewSceneRoot = null;
                }
            }

            /************************************************************************************************************************/

            public void Update()
            {
                if (!_IsChangingPlayMode && _InstanceRoot == null)
                    InstantiateModel();

                if (_InstanceAnimancer != null && _InstanceAnimancer.IsGraphPlaying)
                    _Instance.Repaint();
            }

            /************************************************************************************************************************/

            [NonSerialized] private bool _IsChangingPlayMode;

            private void OnPlayModeChanged(PlayModeStateChange change)
            {
                switch (change)
                {
                    case PlayModeStateChange.ExitingEditMode:
                    case PlayModeStateChange.ExitingPlayMode:
                        DestroyModelInstance();
                        _IsChangingPlayMode = true;
                        break;

                    case PlayModeStateChange.EnteredEditMode:
                    case PlayModeStateChange.EnteredPlayMode:
                        _IsChangingPlayMode = false;
                        break;
                }
            }

            /************************************************************************************************************************/

            private void OnSceneOpening(string path, UnityEditor.SceneManagement.OpenSceneMode mode)
            {
                if (mode == UnityEditor.SceneManagement.OpenSceneMode.Single)
                    DestroyModelInstance();
            }

            /************************************************************************************************************************/

            private void InstantiateModel()
            {
                DestroyModelInstance();

                if (_OriginalRoot == null)
                    return;

                _PreviewSceneRoot.gameObject.SetActive(false);
                _InstanceRoot = Instantiate(_OriginalRoot, _PreviewSceneRoot);
                _InstanceRoot.localPosition = Vector3.zero;
                _InstanceRoot.name = _OriginalRoot.name;

                DisableUnnecessaryComponents(_InstanceRoot.gameObject);

                _InstanceAnimators = _InstanceRoot.GetComponentsInChildren<Animator>();
                for (int i = 0; i < _InstanceAnimators.Length; i++)
                {
                    var animator = _InstanceAnimators[i];
                    animator.enabled = false;
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    animator.fireEvents = false;
                    animator.updateMode = AnimatorUpdateMode.Normal;
                    animator.gameObject.AddComponent<RedirectRootMotion>()
                        .animator = animator;
                }

                _PreviewSceneRoot.gameObject.SetActive(true);

                SetSelectedAnimator(_SelectedInstanceAnimator);
                InitialiseCamera();
                _Instance._Inspector.GatherAnimations();
            }

            /************************************************************************************************************************/

            /// <summary>Disables all unnecessary components on the `root` or its children.</summary>
            private static void DisableUnnecessaryComponents(GameObject root)
            {
                var behaviours = root.GetComponentsInChildren<Behaviour>();
                for (int i = 0; i < behaviours.Length; i++)
                {
                    var behaviour = behaviours[i];

                    // Other undesirable components aren't Behaviours anyway: Transform, MeshFilter, Renderer
                    if (behaviour is Animator)
                        continue;

                    behaviour.enabled = false;
                    if (behaviour is MonoBehaviour mono)
                        mono.runInEditMode = false;
                }
            }

            /************************************************************************************************************************/

            public void SetSelectedAnimator(int index)
            {
                DestroyAnimancerInstance();

                var animator = SelectedInstanceAnimator;
                if (animator != null && animator.enabled)
                {
                    animator.Rebind();
                    animator.enabled = false;
                    return;
                }

                _SelectedInstanceAnimator = index;

                animator = SelectedInstanceAnimator;
                if (animator != null)
                {
                    animator.enabled = true;
                    _SelectedInstanceType = AnimationBindings.GetAnimationType(animator);

                    if (_SelectedInstanceType == AnimationType.Sprite)
                    {
                        CameraEulerAngles = default;
                    }
                    else
                    {
                        CameraEulerAngles = CameraEulerAngles;
                    }
                }
            }

            /************************************************************************************************************************/

            public void OnTargetPropertyChanged()
            {
                _SelectedInstanceAnimator = 0;
                if (_ExpandedHierarchy != null)
                    _ExpandedHierarchy.Clear();

                OriginalRoot = AnimancerEditorUtilities.FindRoot(_Instance._TransitionProperty.TargetObject);
                InitialiseDefaultRoot();
                _CameraPosition = Vector3NaN;
                _NormalizedTime = 0;
            }

            /************************************************************************************************************************/

            private void InitialiseDefaultRoot()
            {
                if (OriginalRoot != null)
                    return;

                var collection = _Instance.GetTransition() as IAnimationClipCollection;
                if (collection == null)
                    return;

                var models = Settings.Models;
                var animatableBindings = new HashSet<EditorCurveBinding>[models.Count];

                for (int i = 0; i < models.Count; i++)
                {
                    animatableBindings[i] = AnimationBindings.GetBindings(models[i]).ObjectBindings;
                }

                using (ObjectPool.Disposable.AcquireSet<AnimationClip>(out var clips))
                {
                    collection.GatherAnimationClips(clips);

                    var bestMatchIndex = -1;
                    var bestMatchCount = 0;
                    foreach (var clip in clips)
                    {
                        var clipBindings = AnimationBindings.GetBindings(clip);

                        for (int iModel = animatableBindings.Length - 1; iModel >= 0; iModel--)
                        {
                            var modelBindings = animatableBindings[iModel];
                            var matches = 0;

                            for (int iBinding = 0; iBinding < clipBindings.Length; iBinding++)
                            {
                                if (modelBindings.Contains(clipBindings[iBinding]))
                                    matches++;
                            }

                            if (bestMatchCount < matches)
                            {
                                bestMatchCount = matches;
                                bestMatchIndex = iModel;

                                if (bestMatchCount == clipBindings.Length)
                                    goto FoundBestMatch;
                            }
                        }
                    }

                    FoundBestMatch:
                    if (bestMatchIndex >= 0)
                    {
                        OriginalRoot = models[bestMatchIndex].transform;
                        return;
                    }

                    foreach (var clip in clips)
                    {
                        var type = AnimationBindings.GetAnimationType(clip);
                        switch (type)
                        {
                            case AnimationType.Humanoid:
                                OriginalRoot = DefaultHumanoid.transform;
                                return;

                            case AnimationType.Sprite:
                                OriginalRoot = DefaultSprite.transform;
                                return;
                        }
                    }
                }
            }

            /************************************************************************************************************************/

            [NonSerialized] private Texture _PreviewTexture;

            public void DoPreviewGUI()
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndVertical();

                var area = GUILayoutUtility.GetLastRect();

                var inspectorBorder = new Rect(area.xMax, area.y, 0, area.height);
                _Instance._Inspector.DoResizeGUI(inspectorBorder);

                if (area.width <= 0 || area.height <= 0)
                    return;

                if (Event.current.type == EventType.Repaint)
                {
                    DrawFloor();

                    // Start the model paused at the beginning of the animation.
                    // For some reason Unity doesn't like having this in OnEnable.
                    if (InstanceAnimancer != null && InstanceAnimancer.Layers.Count == 0)
                    {
                        ShowTransitionPaused();
                        InitialiseCamera();
                    }

                    var fog = RenderSettings.fog;
                    Unsupported.SetRenderSettingsUseFogNoDirty(false);

                    if (InstanceAnimancer != null ||
                        _PreviewTexture == null ||
                        _OriginalRoot == null)
                    {
                        _PreviewRenderUtility.BeginPreview(area, GUIStyle.none);
                        _PreviewRenderUtility.Render(true);
                        _PreviewTexture = _PreviewRenderUtility.EndPreview();
                    }

                    GUI.DrawTexture(area, _PreviewTexture, ScaleMode.StretchToFill, false);

                    Unsupported.SetRenderSettingsUseFogNoDirty(fog);
                }

                AnimancerGUI.HandleDragAndDrop<GameObject>(area,
                    (gameObject) => GetDragAndDropRoot() != null,
                    (gameObject) => OriginalRoot = GetDragAndDropRoot());

                HandleMouseInput(area);
            }

            /************************************************************************************************************************/

            public ITransitionDetailed ShowTransitionPaused()
            {
                var transition = _Instance.GetTransition();
                if (transition.IsValid())
                {
                    var animancer = InstanceAnimancer;
                    animancer.Play(transition, 0);
                    OnPlayAnimation();
                    animancer.Evaluate();
                    animancer.PauseGraph();
                }
                return transition;
            }

            /************************************************************************************************************************/

            private static Transform GetDragAndDropRoot()
            {
                var objects = DragAndDrop.objectReferences;
                if (objects.Length != 1)
                    return null;

                return AnimancerEditorUtilities.FindRoot(objects[0]);
            }

            /************************************************************************************************************************/

            [NonSerialized] private bool _IsDraggingCamera;

            private void HandleMouseInput(Rect area)
            {
                var currentEvent = Event.current;
                switch (currentEvent.type)
                {
                    case EventType.MouseDown:
                        _IsDraggingCamera = area.Contains(currentEvent.mousePosition);
                        if (_IsDraggingCamera)
                            currentEvent.Use();
                        break;

                    case EventType.MouseUp:
                        if (_IsDraggingCamera)
                        {
                            _IsDraggingCamera = false;
                            currentEvent.Use();
                        }
                        break;

                    case EventType.MouseDrag:
                        if (_IsDraggingCamera)
                        {
                            if (currentEvent.button == 1)// Right Click to Rotate.
                            {
                                var sensitivity = Screen.dpi * 0.01f * Settings.RotationSensitivity;

                                var euler = CameraEulerAngles;
                                euler.x += currentEvent.delta.y * sensitivity;
                                euler.y += currentEvent.delta.x * sensitivity;
                                CameraEulerAngles = euler;
                            }
                            else// Other to Move.
                            {
                                var previousRay = Camera.ScreenPointToRay(currentEvent.mousePosition - currentEvent.delta);
                                var currentRay = Camera.ScreenPointToRay(currentEvent.mousePosition);

                                var previousPosition = previousRay.origin + previousRay.direction * CameraZoom;
                                var currentPosition = currentRay.origin + currentRay.direction * CameraZoom;
                                var delta = currentPosition - previousPosition;
                                delta = Vector3.Reflect(delta, Camera.transform.right);

                                if (float.IsNaN(_CameraPosition.x))
                                    _CameraPosition = _DefaultCameraPosition;

                                CameraPosition += delta * Settings.MovementSensitivity;
                            }

                            currentEvent.Use();
                        }
                        break;

                    case EventType.ScrollWheel:
                        if (area.Contains(currentEvent.mousePosition))
                        {
                            CameraZoom *= 1 + 0.03f * currentEvent.delta.y;
                            currentEvent.Use();
                        }
                        break;

                    case EventType.KeyDown:
                        if (currentEvent.keyCode == KeyCode.F)
                        {
                            CameraPosition = _DefaultCameraPosition;
                            CameraZoom = _DefaultCameraZoom;
                            CameraEulerAngles = _DefaultCameraEulerAngles;
                            currentEvent.Use();
                        }
                        break;
                }
            }

            /************************************************************************************************************************/

            public void OnPlayAnimation()
            {
                var animancer = InstanceAnimancer;
                if (animancer == null ||
                    animancer.States.Current == null)
                    return;

                var state = animancer.States.Current;

                state.RecreatePlayableRecursive();

                if (state.HasEvents)
                {
                    var warnings = OptionalWarning.UnsupportedEvents.DisableTemporarily();
                    var normalizedEndTime = state.Events.NormalizedEndTime;
                    state.Events = null;
                    state.Events.NormalizedEndTime = normalizedEndTime;
                    warnings.Enable();
                }
            }

            /************************************************************************************************************************/

            [SerializeField] private float _NormalizedTime;

            public float NormalizedTime
            {
                get => _NormalizedTime;
                set
                {
                    _NormalizedTime = value;

                    var animancer = InstanceAnimancer;
                    if (animancer == null)
                        return;

                    var transition = ShowTransitionPaused();
                    if (!transition.IsValid())
                        return;

                    var state = animancer.States.Current;

                    var length = transition.MaximumDuration;
                    var time = value * length;
                    var fadeDuration = transition.FadeDuration;

                    var startTime = transition.NormalizedStartTime * length;
                    if (float.IsNaN(startTime))
                        startTime = 0;

                    var inspector = _Instance._Inspector;
                    if (time < startTime)// Previous animation.
                    {
                        if (inspector.PreviousAnimation != null)
                        {
                            var fromState = animancer.States.GetOrCreate(Inspector.PreviousAnimationKey, inspector.PreviousAnimation, true);
                            animancer.Play(fromState);
                            OnPlayAnimation();
                            fromState.NormalizedTime = value;
                            value = 0;
                        }
                    }
                    else if (time < startTime + fadeDuration)// Fade from previous animation to the target.
                    {
                        if (inspector.PreviousAnimation != null)
                        {
                            var fromState = animancer.States.GetOrCreate(Inspector.PreviousAnimationKey, inspector.PreviousAnimation, true);
                            animancer.Play(fromState);
                            OnPlayAnimation();
                            fromState.NormalizedTime = value;

                            state.IsPlaying = true;
                            state.Weight = (time - startTime) / fadeDuration;
                            fromState.Weight = 1 - state.Weight;
                        }
                    }
                    else if (inspector.NextAnimation != null)// Fade from the target transition to the next animation.
                    {
                        var normalizedEndTime = state.HasEvents ? state.Events.NormalizedEndTime : float.NaN;
                        if (float.IsNaN(normalizedEndTime))
                            normalizedEndTime = AnimancerEvent.Sequence.GetDefaultNormalizedEndTime(state.Speed);

                        if (value < normalizedEndTime)
                        {
                            // Just the main state.
                        }
                        else
                        {
                            var toState = animancer.States.GetOrCreate(Inspector.NextAnimationKey, inspector.NextAnimation, true);
                            animancer.Play(toState);
                            OnPlayAnimation();
                            toState.NormalizedTime = value - normalizedEndTime;

                            var endTime = normalizedEndTime * length;
                            var fadeOutEnd = TimeRuler.GetFadeOutEnd(toState.Speed, endTime, length);
                            if (time < fadeOutEnd)
                            {
                                state.IsPlaying = true;
                                toState.Weight = (time - endTime) / (fadeOutEnd - endTime);
                                state.Weight = 1 - toState.Weight;
                            }
                        }
                    }

                    state.NormalizedTime = state.Weight > 0 ? value : 0;
                    animancer.Evaluate();

                    _Instance.Repaint();
                }
            }

            /************************************************************************************************************************/

            public void DoHierarchyGUI()
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("Preview Scene Hierarchy");
                DoHierarchyGUI(_PreviewSceneRoot);
                GUILayout.EndVertical();
            }

            [SerializeField] private List<Transform> _ExpandedHierarchy;

            private void DoHierarchyGUI(Transform transform)
            {
                var area = AnimancerGUI.LayoutSingleLineRect();

                var style = ObjectPool.GetCachedResult(() => new GUIStyle(EditorStyles.miniButton)
                {
                    alignment = TextAnchor.MiddleLeft,
                });

                if (GUI.Button(EditorGUI.IndentedRect(area), transform.name, style))
                {
                    Selection.activeTransform = transform;
                    GUIUtility.ExitGUI();
                }

                var childCount = transform.childCount;
                if (childCount == 0)
                    return;

                var index = _ExpandedHierarchy != null ? _ExpandedHierarchy.IndexOf(transform) : -1;
                var isExpanded = EditorGUI.Foldout(area, index >= 0, GUIContent.none);
                if (isExpanded)
                {
                    if (index < 0)
                    {
                        AnimancerUtilities.NewIfNull(ref _ExpandedHierarchy);
                        _ExpandedHierarchy.Add(transform);
                    }

                    EditorGUI.indentLevel++;
                    for (int i = 0; i < childCount; i++)
                        DoHierarchyGUI(transform.GetChild(i));
                    EditorGUI.indentLevel--;
                }
                else if (index >= 0)
                {
                    _ExpandedHierarchy.RemoveAt(index);
                }
            }

            /************************************************************************************************************************/
            #region Camera
            /************************************************************************************************************************/

            private static Vector3 Vector3NaN => new Vector3(float.NaN, float.NaN, float.NaN);

            /************************************************************************************************************************/

            private Camera Camera => _PreviewRenderUtility.camera;

            /************************************************************************************************************************/

            private bool HasCamera => Camera != null && Camera.transform.parent != null;

            /************************************************************************************************************************/

            [SerializeField] private Vector3 _CameraPosition = Vector3NaN;
            [SerializeField] private Vector3 _DefaultCameraPosition = Vector3NaN;

            private Vector3 CameraPosition
            {
                get => _CameraPosition;
                set
                {
                    _CameraPosition = value;

                    if (HasCamera &&
                        !value.IsNaN())
                    {
                        Camera.transform.parent.localPosition = value;
                    }
                }
            }

            /************************************************************************************************************************/

            [SerializeField] private float _CameraZoom;
            [SerializeField] private float _DefaultCameraZoom;

            private float CameraZoom
            {
                get => _CameraZoom;
                set
                {
                    _CameraZoom = value;
                    if (HasCamera)
                        Camera.transform.localPosition = new Vector3(0, 0, -_CameraZoom);
                }
            }

            /************************************************************************************************************************/

            [SerializeField] private Vector3 _CameraEulerAngles = Vector3NaN;
            [SerializeField] private Vector3 _DefaultCameraEulerAngles = Vector3NaN;

            private Vector3 CameraEulerAngles
            {
                get
                {
                    if (HasCamera)
                        return Camera.transform.parent.localEulerAngles;
                    else
                        return _CameraEulerAngles;
                }
                set
                {
                    if (_SelectedInstanceType == AnimationType.Sprite)
                    {
                        if (HasCamera)
                            Camera.transform.parent.localRotation = Quaternion.identity;
                        return;
                    }

                    _CameraEulerAngles = value;
                    if (!value.IsNaN() && HasCamera)
                        Camera.transform.parent.localEulerAngles = value;
                }
            }

            /************************************************************************************************************************/

            private void InitialiseCamera()
            {
                var renderers = _InstanceRoot.GetComponentsInChildren<Renderer>();
                var bounds = renderers.Length > 0 ? renderers[0].bounds : default;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }

                const string CameraParentName = "Animancer Preview Camera Root";
                var cameraParent = _PreviewSceneRoot.Find(CameraParentName);
                if (cameraParent == null)
                {
                    cameraParent = EditorUtility.CreateGameObjectWithHideFlags(CameraParentName, HideAndDontSave).transform;
                    cameraParent.parent = _PreviewSceneRoot;

                    var lights = _PreviewRenderUtility.lights;
                    for (int i = 0; i < lights.Length; i++)
                    {
                        var light = lights[i];
                        light.transform.parent = cameraParent;
                        light.gameObject.hideFlags = HideAndDontSave;
                    }

                    Camera.transform.parent = cameraParent;
                    Camera.transform.localRotation = Quaternion.identity;
                }

                Camera.farClipPlane = 100;

                CameraPosition = _CameraPosition.IsNaN() ?
                    bounds.center :
                    _CameraPosition;

                var zoom = CameraZoom;
                if (zoom <= 0)
                {
                    zoom = bounds.extents.magnitude * 2 / Mathf.Tan(Camera.fieldOfView * Mathf.Deg2Rad);
                    if (zoom <= 0)
                        zoom = 10;
                }
                CameraZoom = zoom;

                CameraEulerAngles = _CameraEulerAngles.IsNaN() ?
                    new Vector3(45, 135, 0) :
                    _CameraEulerAngles;

                if (_DefaultCameraPosition.IsNaN())
                {
                    _DefaultCameraPosition = CameraPosition;
                    _DefaultCameraZoom = CameraZoom;
                    _DefaultCameraEulerAngles = CameraEulerAngles;
                }
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Floor
            /************************************************************************************************************************/

            private const float FloorScale = 5;

            [NonSerialized] private Vector3 _FloorPosition;

            private void DrawFloor()
            {
                if (!Settings.FloorEnabled)
                    return;

                var position = _FloorPosition;

                Quaternion rotation;
                if (_SelectedInstanceType == AnimationType.Sprite)
                {
                    position.z = CameraZoom * 0.01f;
                    rotation = Quaternion.Euler(-90f, 0f, 0f);
                }
                else
                {
                    position.y = 0;
                    rotation = Quaternion.identity;
                }

                var scale = Vector3.one * FloorScale;
                var matrix = Matrix4x4.TRS(position, rotation, scale);
                var layer = 0;
                var camera = _PreviewRenderUtility.camera;
                Graphics.DrawMesh(Floor.Plane, matrix, Floor.Material, layer, camera, 0);
            }

            /************************************************************************************************************************/

            // Initialisation based on UnityEditor.AvatarPreview.
            internal static class Floor
            {
                /************************************************************************************************************************/

                public static readonly Mesh Plane = Resources.GetBuiltinResource(typeof(Mesh), "New-Plane.fbx") as Mesh;

                /************************************************************************************************************************/

                private static Material _Material;
                private static Material _DefaultMaterial;
                private static Texture2D _Texture;

                /************************************************************************************************************************/

                public static Material Material
                {
                    get
                    {
                        // Initialisation based on UnityEditor.AvatarPreview.

                        if (_Material == null)
                        {
                            if (_Texture == null)
                                _Texture = (Texture2D)EditorGUIUtility.Load("Avatar/Textures/AvatarFloor.png");

                            _Material = Settings.FloorMaterial;
                            if (_Material != null)
                            {
                                _Material = Instantiate(_Material);
                                _Material.hideFlags = HideFlags.HideAndDontSave;

                                var property = Settings.FloorTexturePropertyName;
                                if (!string.IsNullOrEmpty(property))
                                    _Material.SetTexture(property, _Texture);

                                _Material.SetVector("_Alphas", new Vector4(0.5f, 0.3f, 0f, 0f));
                            }
                            else
                            {
                                if (_DefaultMaterial == null)
                                {
                                    var shader = EditorGUIUtility.Load("Previews/PreviewPlaneWithShadow.shader") as Shader;

                                    _DefaultMaterial = new Material(shader)
                                    {
                                        mainTexture = _Texture,
                                        mainTextureScale = Vector2.one * 20,
                                        hideFlags = HideFlags.HideAndDontSave,
                                    };

                                    _DefaultMaterial.SetVector("_Alphas", new Vector4(0.5f, 0.3f, 0f, 0f));
                                }

                                _Material = _DefaultMaterial;
                            }
                        }

                        return _Material;
                    }
                }

                /************************************************************************************************************************/

                public static void DiscardCustomMaterial()
                {
                    if (_Material != _DefaultMaterial)
                        DestroyImmediate(_Material);

                    _Material = null;
                }

                /************************************************************************************************************************/
            }

            /************************************************************************************************************************/

            [AddComponentMenu("")]
#if UNITY_2019_1_OR_NEWER
            [ExecuteAlways]
#else
            [ExecuteInEditMode]
#endif
            private sealed class RedirectRootMotion : MonoBehaviour
            {
                public Animator animator;

                private void OnAnimatorMove()
                {
                    if (animator == null ||
                        _Instance == null)
                        return;

                    var scene = _Instance._Scene;
                    if (animator == scene.SelectedInstanceAnimator)
                    {
                        scene._FloorPosition -= animator.deltaPosition;

                        scene._FloorPosition.x %= FloorScale;
                        scene._FloorPosition.y %= FloorScale;
                        scene._FloorPosition.z %= FloorScale;
                    }
                }
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Cleanup
            /************************************************************************************************************************/

            public void DestroyModelInstance()
            {
                DestroyAnimancerInstance();

                if (_InstanceRoot == null)
                    return;

                DestroyImmediate(_InstanceRoot.gameObject);
                _InstanceRoot = null;
                _InstanceAnimators = null;
            }

            /************************************************************************************************************************/

            private void DestroyAnimancerInstance()
            {
                if (_InstanceAnimancer == null)
                    return;

                _InstanceAnimancer.Destroy();
                _InstanceAnimancer = null;
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
        }
    }
}

#endif

