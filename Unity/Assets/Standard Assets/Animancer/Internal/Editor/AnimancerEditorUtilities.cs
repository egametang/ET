// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Various utilities used throughout Animancer.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerEditorUtilities
    /// 
    public static partial class AnimancerEditorUtilities
    {
        /************************************************************************************************************************/
        #region Misc
        /************************************************************************************************************************/

        /// <summary>Commonly used <see cref="BindingFlags"/> combinations.</summary>
        public const BindingFlags
            AnyAccessBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
            InstanceBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            StaticBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        /************************************************************************************************************************/

        /// <summary>Is the <see cref="Vector2.x"/> or <see cref="Vector2.y"/> NaN?</summary>
        public static bool IsNaN(this Vector2 vector) => float.IsNaN(vector.x) || float.IsNaN(vector.y);

        /// <summary>Is the <see cref="Vector3.x"/>, <see cref="Vector3.y"/>, or <see cref="Vector3.z"/> NaN?</summary>
        public static bool IsNaN(this Vector3 vector) => float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z);

        /************************************************************************************************************************/

        /// <summary>Adds default items or removes items to make the <see cref="List{T}.Count"/> equal to the `count`.</summary>
        public static void SetCount<T>(List<T> list, int count)
        {
            if (list.Count < count)
            {
                while (list.Count < count)
                    list.Add(default);
            }
            else
            {
                list.RemoveRange(count, list.Count - count);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Tries to find a <typeparamref name="T"/> component on the `gameObject` or its parents or children (in that
        /// order).
        /// </summary>
        public static T GetComponentInHierarchy<T>(GameObject gameObject) where T : class
        {
            var component = gameObject.GetComponentInParent<T>();
            if (component != null)
                return component;

            return gameObject.GetComponentInChildren<T>();
        }

        /************************************************************************************************************************/

        /// <summary>Assets cannot reference scene objects.</summary>
        public static bool ShouldAllowReference(Object obj, Object reference)
        {
            return obj == null || reference == null ||
                !EditorUtility.IsPersistent(obj) ||
                EditorUtility.IsPersistent(reference);
        }

        /************************************************************************************************************************/

        private static Dictionary<Type, Dictionary<string, MethodInfo>> _TypeToMethodNameToMethod;

        /// <summary>
        /// Tries to find a method with the specified name on the `target` object and invoke it.
        /// </summary>
        public static object Invoke(object target, string methodName) => Invoke(target.GetType(), target, methodName);

        /// <summary>
        /// Tries to find a method with the specified name on the `target` object and invoke it.
        /// </summary>
        public static object Invoke(Type type, object target, string methodName)
        {
            AnimancerUtilities.NewIfNull(ref _TypeToMethodNameToMethod);

            if (!_TypeToMethodNameToMethod.TryGetValue(type, out var nameToMethod))
            {
                nameToMethod = new Dictionary<string, MethodInfo>();
                _TypeToMethodNameToMethod.Add(type, nameToMethod);
            }

            if (!nameToMethod.TryGetValue(methodName, out var method))
            {
                method = type.GetMethod(methodName, AnyAccessBindings);
                nameToMethod.Add(methodName, method);

                if (method == null)
                    RegisterNonCriticalMissingMember(type.FullName, methodName);
            }

            if (method != null)
                return method.Invoke(target, null);

            return null;
        }

        /************************************************************************************************************************/

        private static List<Action<StringBuilder>> _NonCriticalIssues;

        /// <summary>
        /// Registers a delegate that can construct a description of an issue at a later time so that it doesn't waste
        /// the user's time on unimportant issues.
        /// </summary>
        public static void RegisterNonCriticalIssue(Action<StringBuilder> describeIssue)
        {
            AnimancerUtilities.NewIfNull(ref _NonCriticalIssues);

            _NonCriticalIssues.Add(describeIssue);
        }

        /// <summary>
        /// Calls <see cref="RegisterNonCriticalIssue"/> with an issue indicating that a particular type was not
        /// found by reflection.
        /// </summary>
        public static void RegisterNonCriticalMissingType(string type)
        {
            RegisterNonCriticalIssue((text) => text
                .Append("[Reflection] Unable to find type '")
                .Append(type)
                .Append("'"));
        }

        /// <summary>
        /// Calls <see cref="RegisterNonCriticalIssue"/> with an issue indicating that a particular member was not
        /// found by reflection.
        /// </summary>
        public static void RegisterNonCriticalMissingMember(string type, string name)
        {
            RegisterNonCriticalIssue((text) => text
                .Append("[Reflection] Unable to find member '")
                .Append(name)
                .Append("' in type '")
                .Append(type)
                .Append("'"));
        }

        /// <summary>
        /// Appends all issues given to <see cref="RegisterNonCriticalIssue"/> to the `text`.
        /// </summary>
        public static void AppendNonCriticalIssues(StringBuilder text)
        {
            if (_NonCriticalIssues == null)
                return;

            text.Append("\n\nThe following non-critical issues have also been found" +
                " (in Animancer generally, not specifically this object):\n\n");

            for (int i = 0; i < _NonCriticalIssues.Count; i++)
            {
                text.Append(" - ");
                _NonCriticalIssues[i](text);
                text.Append("\n\n");
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Waits one frame to call the `method` as long as Unity is currently in Edit Mode.
        /// </summary>
        public static void EditModeDelayCall(Action method)
        {
            // Would be better to check this before the delayCall, but it only works on the main thread.

            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    method();
            };
        }

        /************************************************************************************************************************/

        /// <summary>Finds an asset of the specified type anywhere in the project.</summary>
        public static T FindAssetOfType<T>() where T : Object
        {
            var filter = typeof(Component).IsAssignableFrom(typeof(T)) ? $"t:{nameof(GameObject)}" : $"t:{typeof(T).Name}";
            var guids = AssetDatabase.FindAssets(filter);
            if (guids.Length == 0)
                return null;

            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    return asset;
            }

            return null;
        }

        /************************************************************************************************************************/

        /// <summary>Removes any items from the `dictionary` that use destroyed objects as their key.</summary>
        public static void RemoveDestroyedObjects<TKey, TValue>(Dictionary<TKey, TValue> dictionary) where TKey : Object
        {
            using (ObjectPool.Disposable.AcquireList<TKey>(out var oldObjects))
            {
                foreach (var obj in dictionary.Keys)
                {
                    if (obj == null)
                        oldObjects.Add(obj);
                }

                for (int i = 0; i < oldObjects.Count; i++)
                {
                    dictionary.Remove(oldObjects[i]);
                }
            }
        }

        /// <summary>
        /// Creates a new dictionary and returns true if it was null or calls <see cref="RemoveDestroyedObjects"/> and
        /// returns false if it wasn't.
        /// </summary>
        public static bool InitialiseCleanDictionary<TKey, TValue>(ref Dictionary<TKey, TValue> dictionary) where TKey : Object
        {
            if (AnimancerUtilities.NewIfNull(ref dictionary))
            {
                return true;
            }
            else
            {
                RemoveDestroyedObjects(dictionary);
                return false;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Context Menus
        /************************************************************************************************************************/

        /// <summary>
        /// Adds a menu function which passes the result of <see cref="CalculateEditorFadeDuration"/> into `startFade`.
        /// </summary>
        public static void AddFadeFunction(GenericMenu menu, string label, bool isEnabled, AnimancerNode node, Action<float> startFade)
        {
            // Fade functions need to be delayed twice since the context menu itself causes the next frame delta
            // time to be unreasonably high (which would skip the start of the fade).
            menu.AddFunction(label, isEnabled,
                () => EditorApplication.delayCall +=
                () => EditorApplication.delayCall +=
                () =>
                {
                    startFade(node.CalculateEditorFadeDuration());
                });
        }

        /// <summary>
        /// Returns the duration of the `node`s current fade (if any), otherwise returns the `defaultDuration`.
        /// </summary>
        public static float CalculateEditorFadeDuration(this AnimancerNode node, float defaultDuration = 1)
            => node.FadeSpeed > 0 ? 1 / node.FadeSpeed : defaultDuration;

        /************************************************************************************************************************/

        /// <summary>
        /// Adds a menu function to open a web page. If the `linkSuffix` starts with a '/' then it will be relative to
        /// the <see cref="Strings.DocsURLs.Documentation"/>.
        /// </summary>
        public static void AddDocumentationLink(GenericMenu menu, string label, string linkSuffix)
        {
            if (linkSuffix[0] == '/')
                linkSuffix = Strings.DocsURLs.Documentation + linkSuffix;

            menu.AddItem(new GUIContent(label), false, () =>
            {
                EditorUtility.OpenWithDefaultApp(linkSuffix);
            });
        }

        /************************************************************************************************************************/

        /// <summary>Toggles the <see cref="Motion.isLooping"/> flag between true and false.</summary>
        [MenuItem("CONTEXT/" + nameof(AnimationClip) + "/Toggle Looping")]
        private static void ToggleLooping(MenuCommand command)
        {
            var clip = (AnimationClip)command.context;
            SetLooping(clip, !clip.isLooping);
        }

        /// <summary>Sets the <see cref="Motion.isLooping"/> flag.</summary>
        public static void SetLooping(AnimationClip clip, bool looping)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = looping;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            Debug.Log($"Set {clip.name} to be {(looping ? "Looping" : "Not Looping")}." +
                " Note that you may need to restart Unity for this change to take effect.", clip);

            // None of these let us avoid the need to restart Unity.
            //EditorUtility.SetDirty(clip);
            //AssetDatabase.SaveAssets();

            //var path = AssetDatabase.GetAssetPath(clip);
            //AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        /************************************************************************************************************************/

        /// <summary>Swaps the <see cref="AnimationClip.legacy"/> flag between true and false.</summary>
        [MenuItem("CONTEXT/" + nameof(AnimationClip) + "/Toggle Legacy")]
        private static void ToggleLegacy(MenuCommand command)
        {
            var clip = (AnimationClip)command.context;
            clip.legacy = !clip.legacy;
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="Animator.Rebind"/>.</summary>
        [MenuItem("CONTEXT/" + nameof(Animator) + "/Restore Bind Pose", priority = 110)]
        private static void RestoreBindPose(MenuCommand command)
        {
            var animator = (Animator)command.context;

            Undo.RegisterFullObjectHierarchyUndo(animator.gameObject, "Restore bind pose");

            var type = Type.GetType("UnityEditor.AvatarSetupTool, UnityEditor");
            if (type != null)
            {
                var method = type.GetMethod("SampleBindPose", BindingFlags.Public | BindingFlags.Static);
                if (method != null)
                    method.Invoke(null, new object[] { animator.gameObject });
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Dummy Animancer Component
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// An <see cref="IAnimancerComponent"/> that is not actually a <see cref="Component"/>.
        /// </summary>
        public sealed class DummyAnimancerComponent : IAnimancerComponent
        {
            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="DummyAnimancerComponent"/>.</summary>
            public DummyAnimancerComponent(Animator animator, AnimancerPlayable playable)
            {
                Animator = animator;
                Playable = playable;
                InitialUpdateMode = animator.updateMode;
            }

            /************************************************************************************************************************/

            /// <summary>[<see cref="IAnimancerComponent"/>] Returns true.</summary>
            public bool enabled => true;

            /// <summary>[<see cref="IAnimancerComponent"/>] Returns the <see cref="Animator"/>'s <see cref="GameObject"/>.</summary>
            public GameObject gameObject => Animator.gameObject;

            /// <summary>[<see cref="IAnimancerComponent"/>] The target <see cref="UnityEngine.Animator"/>.</summary>
            public Animator Animator { get; set; }

            /// <summary>[<see cref="IAnimancerComponent"/>] The target <see cref="AnimancerPlayable"/>.</summary>
            public AnimancerPlayable Playable { get; private set; }

            /// <summary>[<see cref="IAnimancerComponent"/>] Returns true.</summary>
            public bool IsPlayableInitialised => true;

            /// <summary>[<see cref="IAnimancerComponent"/>] Returns false.</summary>
            public bool ResetOnDisable => false;

            /// <summary>[<see cref="IAnimancerComponent"/>] Does nothing.</summary>
            public AnimatorUpdateMode UpdateMode { get; set; }

            /************************************************************************************************************************/

            /// <summary>[<see cref="IAnimancerComponent"/>] Returns the `clip`.</summary>
            public object GetKey(AnimationClip clip) => clip;

            /************************************************************************************************************************/

            /// <summary>[<see cref="IAnimancerComponent"/>] Returns null.</summary>
            public string AnimatorFieldName => null;

            /// <summary>[<see cref="IAnimancerComponent"/>] Returns null.</summary>
            public string ActionOnDisableFieldName => null;

            /// <summary>[<see cref="IAnimancerComponent"/>] Returns null.</summary>
            public AnimatorUpdateMode? InitialUpdateMode { get; private set; }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

