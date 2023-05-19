// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] The general type of object an <see cref="AnimationClip"/> can animate.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimationType
    /// 
    public enum AnimationType
    {
        /// <summary>Unable to determine a type.</summary>
        None,

        /// <summary>A Humanoid rig.</summary>
        Humanoid,

        /// <summary>A Generic rig.</summary>
        Generic,

        /// <summary>A <see cref="Generic"/> rig which only animates a <see cref="SpriteRenderer.sprite"/>.</summary>
        Sprite,
    }

    /// <summary>[Editor-Only]
    /// Various utility functions relating to the properties animated by an <see cref="AnimationClip"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimationBindings
    /// 
    public sealed class AnimationBindings : AssetPostprocessor
    {
        /************************************************************************************************************************/
        #region Animation Types
        /************************************************************************************************************************/

        private static Dictionary<AnimationClip, bool> _ClipToIsSprite;

        /// <summary>Determines the <see cref="AnimationType"/> of the specified `clip`.</summary>
        public static AnimationType GetAnimationType(AnimationClip clip)
        {
            if (clip == null)
                return AnimationType.None;

            if (clip.isHumanMotion)
                return AnimationType.Humanoid;

            AnimancerEditorUtilities.InitialiseCleanDictionary(ref _ClipToIsSprite);

            if (!_ClipToIsSprite.TryGetValue(clip, out var isSprite))
            {
                var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                for (int i = 0; i < bindings.Length; i++)
                {
                    var binding = bindings[i];
                    if (binding.type == typeof(SpriteRenderer) &&
                        binding.propertyName == "m_Sprite")
                    {
                        isSprite = true;
                        break;
                    }
                }

                _ClipToIsSprite.Add(clip, isSprite);
            }

            return isSprite ? AnimationType.Sprite : AnimationType.Generic;
        }

        /************************************************************************************************************************/

        /// <summary>Determines the <see cref="AnimationType"/> of the specified `animator`.</summary>
        public static AnimationType GetAnimationType(Animator animator)
        {
            if (animator == null)
                return AnimationType.None;

            if (animator.isHuman)
                return AnimationType.Humanoid;

            // If all renderers are SpriteRenderers, it's a Sprite animation.
            // Otherwise it's Generic.
            var renderers = animator.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return AnimationType.Generic;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (!(renderers[i] is SpriteRenderer))
                    return AnimationType.Generic;
            }

            return AnimationType.Sprite;
        }

        /************************************************************************************************************************/

        /// <summary>Determines the <see cref="AnimationType"/> of the specified `gameObject`.</summary>
        public static AnimationType GetAnimationType(GameObject gameObject)
        {
            var type = AnimationType.None;
            var animators = gameObject.GetComponentsInChildren<Animator>();
            for (int i = 0; i < animators.Length; i++)
            {
                var animatorType = GetAnimationType(animators[i]);
                switch (animatorType)
                {
                    case AnimationType.Humanoid: return AnimationType.Humanoid;
                    case AnimationType.Generic: return AnimationType.Generic;

                    case AnimationType.Sprite:
                        if (type == AnimationType.None)
                            type = AnimationType.Sprite;
                        break;

                    case AnimationType.None:
                    default:
                        break;
                }
            }

            return type;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        private static bool _CanGatherBindings;

        /// <summary>No more than one set of bindings should be gathered per frame.</summary>
        private static bool CanGatherBindings()
        {
            if (!_CanGatherBindings)
                return false;

            _CanGatherBindings = false;
            EditorApplication.delayCall += () => _CanGatherBindings = true;
            return true;
        }

        /************************************************************************************************************************/

        private static Dictionary<GameObject, BindingData> _ObjectToBindings;

        /// <summary>Returns a cached <see cref="BindingData"/> representing the specified `gameObject`.</summary>
        /// <remarks>Note that the cache is cleared by <see cref="EditorApplication.hierarchyChanged"/>.</remarks>
        public static BindingData GetBindings(GameObject gameObject, bool forceGather = true)
        {
            if (AnimancerEditorUtilities.InitialiseCleanDictionary(ref _ObjectToBindings))
            {
                EditorApplication.hierarchyChanged += _ObjectToBindings.Clear;
            }

            if (!_ObjectToBindings.TryGetValue(gameObject, out var bindings))
            {
                if (!forceGather && !CanGatherBindings())
                    return null;

                bindings = new BindingData(gameObject);
                _ObjectToBindings.Add(gameObject, bindings);
            }

            return bindings;
        }

        /************************************************************************************************************************/

        private static Dictionary<AnimationClip, EditorCurveBinding[]> _ClipToBindings;

        /// <summary>Returns a cached array of all properties animated by the specified `clip`.</summary>
        public static EditorCurveBinding[] GetBindings(AnimationClip clip)
        {
            AnimancerEditorUtilities.InitialiseCleanDictionary(ref _ClipToBindings);

            if (!_ClipToBindings.TryGetValue(clip, out var bindings))
            {
                var curveBindings = AnimationUtility.GetCurveBindings(clip);
                var objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                bindings = new EditorCurveBinding[curveBindings.Length + objectBindings.Length];
                Array.Copy(curveBindings, bindings, curveBindings.Length);
                Array.Copy(objectBindings, 0, bindings, curveBindings.Length, objectBindings.Length);
                _ClipToBindings.Add(clip, bindings);
            }

            return bindings;
        }

        /************************************************************************************************************************/

        private void OnPostprocessAnimation(GameObject root, AnimationClip clip) => OnAnimationChanged(clip);

        /// <summary>Clears any cached values relating to the `clip` since they may no longer be correct.</summary>
        public static void OnAnimationChanged(AnimationClip clip)
        {
            if (_ObjectToBindings != null)
                foreach (var binding in _ObjectToBindings.Values)
                    binding.OnAnimationChanged(clip);

            if (_ClipToBindings != null)
                _ClipToBindings.Remove(clip);
        }

        /************************************************************************************************************************/

        /// <summary>Clears all cached values in this class.</summary>
        public static void ClearCache()
        {
            _ObjectToBindings.Clear();
            _ClipToBindings.Clear();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// A collection of data about the properties on a <see cref="UnityEngine.GameObject"/> and its children
        /// which can be animated and the relationships between those properties and the properties that individual
        /// <see cref="AnimationClip"/>s are trying to animate.
        /// </summary>
        public sealed class BindingData
        {
            /************************************************************************************************************************/

            /// <summary>The target object that this data represents.</summary>
            public readonly GameObject GameObject;

            /// <summary>Creates a new <see cref="BindingData"/> representing the specified `gameObject`.</summary>
            public BindingData(GameObject gameObject) => GameObject = gameObject;

            /************************************************************************************************************************/

            private AnimationType? _ObjectType;

            /// <summary>The cached <see cref="AnimationType"/> of the <see cref="GameObject"/>.</summary>
            public AnimationType ObjectType
            {
                get
                {
                    if (_ObjectType == null)
                        _ObjectType = GetAnimationType(GameObject);
                    return _ObjectType.Value;
                }
            }

            /************************************************************************************************************************/

            private HashSet<EditorCurveBinding> _ObjectBindings;

            /// <summary>The cached properties of the <see cref="GameObject"/> and its children which can be animated.</summary>
            public HashSet<EditorCurveBinding> ObjectBindings
            {
                get
                {
                    if (AnimancerUtilities.NewIfNull(ref _ObjectBindings))
                    {
                        var transforms = GameObject.GetComponentsInChildren<Transform>();
                        for (int i = 0; i < transforms.Length; i++)
                        {
                            var bindings = AnimationUtility.GetAnimatableBindings(transforms[i].gameObject, GameObject);
                            _ObjectBindings.UnionWith(bindings);
                        }
                    }

                    return _ObjectBindings;
                }
            }

            /************************************************************************************************************************/

            private HashSet<string> _ObjectTransformBindings;

            /// <summary>
            /// The <see cref="EditorCurveBinding.path"/> of all <see cref="Transform"/> bindings in
            /// <see cref="ObjectBindings"/>.
            /// </summary>
            public HashSet<string> ObjectTransformBindings
            {
                get
                {
                    if (AnimancerUtilities.NewIfNull(ref _ObjectTransformBindings))
                    {
                        foreach (var binding in ObjectBindings)
                        {
                            if (binding.type == typeof(Transform))
                                _ObjectTransformBindings.Add(binding.path);
                        }
                    }

                    return _ObjectTransformBindings;
                }
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Determines the <see cref="MatchType"/> representing the properties animated by the `state` in
            /// comparison to the properties that actually exist on the target <see cref="GameObject"/> and its
            /// children.
            /// <para></para>
            /// Also compiles a `message` explaining the differences if that paraneter is not null.
            /// </summary>
            public MatchType GetMatchType(Animator animator, AnimancerState state, StringBuilder message, bool forceGather = true)
            {
                using (ObjectPool.Disposable.AcquireSet<AnimationClip>(out var clips))
                {
                    state.GatherAnimationClips(clips);

                    var bindings = message != null ? new Dictionary<EditorCurveBinding, bool>() : null;
                    var existingBindings = 0;

                    var match = default(MatchType);

                    if (animator.avatar == null)
                    {
                        message?.AppendLine()
                            .Append($"{LinePrefix}The {nameof(Animator)} has no {nameof(Avatar)}.");

                        if (animator.isHuman)
                            match = MatchType.Error;
                    }

                    foreach (var clip in clips)
                    {
                        var clipMatch = GetMatchType(clip, message, bindings, ref existingBindings, forceGather);
                        if (match < clipMatch)
                            match = clipMatch;
                    }

                    AppendBindings(message, bindings, existingBindings);

                    return match;
                }
            }

            /************************************************************************************************************************/

            private const string LinePrefix = "- ";

            private Dictionary<AnimationClip, MatchType> _BindingMatches;

            /// <summary>
            /// Determines the <see cref="MatchType"/> representing the properties animated by the `clip` in
            /// comparison to the properties that actually exist on the target <see cref="GameObject"/> and its
            /// children.
            /// <para></para>
            /// Also compiles a `message` explaining the differences if that paraneter is not null.
            /// </summary>
            public MatchType GetMatchType(AnimationClip clip, StringBuilder message,
                Dictionary<EditorCurveBinding, bool> bindingsInMessage, ref int existingBindings, bool forceGather = true)
            {
                AnimancerEditorUtilities.InitialiseCleanDictionary(ref _BindingMatches);

                if (_BindingMatches.TryGetValue(clip, out var match))
                {
                    if (bindingsInMessage == null)
                        return match;
                }
                else if (!forceGather && !CanGatherBindings())
                {
                    return MatchType.Unknown;
                }

                var objectType = ObjectType;
                var clipType = GetAnimationType(clip);
                if (clipType != objectType)
                {
                    if (message != null)
                    {
                        message.AppendLine()
                            .Append($"{LinePrefix}This message does not necessarily mean anything is wrong," +
                            $" but if something is wrong then this might help you identify the problem.");

                        message.AppendLine()
                            .Append($"{LinePrefix}The {nameof(AnimationType)} of the '")
                            .Append(clip.name)
                            .Append("' animation is ")
                            .Append(clipType)
                            .Append(" while the '")
                            .Append(GameObject.name)
                            .Append("' Rig is ")
                            .Append(objectType)
                            .Append(". See the documentation for more information about Animation Types:" +
                                $" {Strings.DocsURLs.Inspector}#animation-types");
                    }

                    switch (clipType)
                    {
                        default:
                        case AnimationType.None:
                        case AnimationType.Humanoid:
                            match = MatchType.Error;
                            if (message == null)
                                goto SetMatch;
                            else
                                break;

                        case AnimationType.Generic:
                        case AnimationType.Sprite:
                            match = MatchType.Warning;
                            break;
                    }
                }

                var bindingMatch = GetMatchType(GetBindings(clip), bindingsInMessage, ref existingBindings);
                if (match < bindingMatch)
                    match = bindingMatch;

                SetMatch:
                _BindingMatches[clip] = match;

                return match;
            }

            /************************************************************************************************************************/

            private MatchType GetMatchType(EditorCurveBinding[] bindings,
                Dictionary<EditorCurveBinding, bool> bindingsInMessage, ref int existingBindings)
            {
                if (bindings.Length == 0)
                    return MatchType.Empty;

                var bindingCount = bindings.Length;

                var matchCount = 0;
                for (int i = 0; i < bindings.Length; i++)
                {
                    var binding = bindings[i];
                    if (ShouldIgnoreBinding(binding))
                    {
                        bindingCount--;
                        continue;
                    }

                    var matches = MatchesObjectBinding(binding);
                    if (matches)
                        matchCount++;

                    if (bindingsInMessage != null && !bindingsInMessage.ContainsKey(binding))
                    {
                        bindingsInMessage.Add(binding, matches);
                        if (matches)
                            existingBindings++;
                    }
                }

                if (matchCount == bindingCount)
                    return MatchType.Correct;
                else if (matchCount != 0)
                    return MatchType.Warning;
                else
                    return MatchType.Error;
            }

            /************************************************************************************************************************/

            private static bool ShouldIgnoreBinding(EditorCurveBinding binding)
            {
                if (binding.type == typeof(Animator) && string.IsNullOrEmpty(binding.path))
                {
                    switch (binding.propertyName)
                    {
                        case "RootQ.w":
                        case "RootQ.x":
                        case "RootQ.y":
                        case "RootQ.z":

                        case "RootT.x":
                        case "RootT.y":
                        case "RootT.z":

                            return true;
                    }
                }

                return false;
            }

            /************************************************************************************************************************/

            private bool MatchesObjectBinding(EditorCurveBinding binding)
            {
                if (binding.type == typeof(Transform))
                {
                    switch (binding.propertyName)
                    {
                        case "m_LocalEulerAngles.x":
                        case "m_LocalEulerAngles.y":
                        case "m_LocalEulerAngles.z":
                        case "localEulerAnglesRaw.x":
                        case "localEulerAnglesRaw.y":
                        case "localEulerAnglesRaw.z":
                            return ObjectTransformBindings.Contains(binding.path);
                    }
                }

                return ObjectBindings.Contains(binding);
            }

            /************************************************************************************************************************/

            private static string[] NoStrings = new string[0];

            private static void AppendBindings(StringBuilder message, Dictionary<EditorCurveBinding, bool> bindings, int existingBindings)
            {
                if (bindings == null ||
                    bindings.Count <= existingBindings)
                    return;

                message.AppendLine()
                    .Append(LinePrefix + "This message has been copied to the clipboard" +
                    " (in case it is too long for Unity to display in the Console).");

                message.AppendLine()
                    .Append(LinePrefix)
                    .Append(bindings.Count - existingBindings)
                    .Append(" of ")
                    .Append(bindings.Count)
                    .Append(" bindings do not exist in the Rig: [x] = Missing, [o] = Exists");

                using (ObjectPool.Disposable.AcquireList<EditorCurveBinding>(out var sortedBindings))
                {
                    sortedBindings.AddRange(bindings.Keys);
                    sortedBindings.Sort((a, b) =>
                    {
                        var result = a.path.CompareTo(b.path);
                        if (result != 0)
                            return result;

                        if (a.type != b.type)
                        {
                            if (a.type == typeof(Transform))
                                return -1;
                            else if (b.type == typeof(Transform))
                                return 1;

                            result = a.type.Name.CompareTo(b.type.Name);
                            if (result != 0)
                                return result;
                        }

                        return a.propertyName.CompareTo(b.propertyName);
                    });

                    var previousBinding = default(EditorCurveBinding);
                    var pathSplit = NoStrings;

                    for (int iBinding = 0; iBinding < sortedBindings.Count; iBinding++)
                    {
                        var binding = sortedBindings[iBinding];
                        if (binding.path != previousBinding.path)
                        {
                            var newPathSplit = binding.path.Split('/');

                            var iSegment = Math.Min(newPathSplit.Length - 1, pathSplit.Length - 1);

                            for (; iSegment >= 0; iSegment--)
                            {
                                if (pathSplit[iSegment] == newPathSplit[iSegment])
                                    break;
                            }
                            iSegment++;

                            if (!string.IsNullOrEmpty(binding.path))
                            {
                                for (; iSegment < newPathSplit.Length; iSegment++)
                                {
                                    message.AppendLine();

                                    for (int iIndent = 0; iIndent < iSegment; iIndent++)
                                        message.Append(Strings.Indent);

                                    message.Append("> ").Append(newPathSplit[iSegment]);
                                }
                            }

                            pathSplit = newPathSplit;
                        }

                        if (TransformBindings.Append(bindings, sortedBindings, ref iBinding, message))
                            continue;

                        message.AppendLine();

                        if (binding.path.Length > 0)
                            for (int iIndent = 0; iIndent < pathSplit.Length; iIndent++)
                                message.Append(Strings.Indent);

                        message
                            .Append(bindings[binding] ? "[o] " : "[x] ")
                            .Append(binding.type.Name)
                            .Append('.')
                            .Append(binding.propertyName);

                        previousBinding = binding;
                    }
                }
            }

            /************************************************************************************************************************/

            private static class TransformBindings
            {
                [Flags]
                private enum Flags
                {
                    None = 0,

                    PositionX = 1 << 0,
                    PositionY = 1 << 1,
                    PositionZ = 1 << 2,

                    RotationX = 1 << 3,
                    RotationY = 1 << 4,
                    RotationZ = 1 << 5,
                    RotationW = 1 << 6,

                    EulerX = 1 << 7,
                    EulerY = 1 << 8,
                    EulerZ = 1 << 9,

                    ScaleX = 1 << 10,
                    ScaleY = 1 << 11,
                    ScaleZ = 1 << 12,
                }

                private static bool HasAll(Flags flag, Flags has) => (flag & has) == has;

                private static bool HasAny(Flags flag, Flags has) => (flag & has) != Flags.None;

                /************************************************************************************************************************/

                private static readonly Flags[]
                    PositionFlags = { Flags.PositionX, Flags.PositionY, Flags.PositionZ },
                    RotationFlags = { Flags.RotationX, Flags.RotationY, Flags.RotationZ, Flags.RotationW },
                    EulerFlags = { Flags.EulerX, Flags.EulerY, Flags.EulerZ },
                    ScaleFlags = { Flags.ScaleX, Flags.ScaleY, Flags.ScaleZ };

                /************************************************************************************************************************/

                public static bool Append(Dictionary<EditorCurveBinding, bool> bindings,
                    List<EditorCurveBinding> sortedBindings, ref int index, StringBuilder message)
                {
                    var binding = sortedBindings[index];
                    if (binding.type != typeof(Transform))
                        return false;

                    if (string.IsNullOrEmpty(binding.path))
                        message.AppendLine().Append('>');
                    else
                        message.Append(':');

                    using (ObjectPool.Disposable.AcquireList<EditorCurveBinding>(out var otherBindings))
                    {
                        var flags = GetFlags(bindings, sortedBindings, ref index, otherBindings, out var anyExists);

                        message.Append(anyExists ? " [o]" : " [x]");

                        var first = true;

                        AppendProperty(message, ref first, flags, PositionFlags, "position", "xyz");
                        AppendProperty(message, ref first, flags, RotationFlags, "rotation", "wxyz");
                        AppendProperty(message, ref first, flags, EulerFlags, "euler", "xyz");
                        AppendProperty(message, ref first, flags, ScaleFlags, "scale", "xyz");

                        for (int i = 0; i < otherBindings.Count; i++)
                        {
                            if (anyExists)
                                message.Append(',');

                            binding = otherBindings[i];
                            message
                                .Append(" [")
                                .Append(bindings[binding] ? 'o' : 'x')
                                .Append("] ")
                                .Append(binding.propertyName);
                        }
                    }

                    return true;
                }

                /************************************************************************************************************************/

                private static Flags GetFlags(Dictionary<EditorCurveBinding, bool> bindings,
                    List<EditorCurveBinding> sortedBindings, ref int index, List<EditorCurveBinding> otherBindings, out bool anyExists)
                {
                    var flags = Flags.None;
                    anyExists = false;

                    var binding = sortedBindings[index];

                    CheckFlags:

                    switch (binding.propertyName)
                    {
                        case "m_LocalPosition.x": flags |= Flags.PositionX; break;
                        case "m_LocalPosition.y": flags |= Flags.PositionY; break;
                        case "m_LocalPosition.z": flags |= Flags.PositionZ; break;
                        case "m_LocalRotation.x": flags |= Flags.RotationX; break;
                        case "m_LocalRotation.y": flags |= Flags.RotationY; break;
                        case "m_LocalRotation.z": flags |= Flags.RotationZ; break;
                        case "m_LocalRotation.w": flags |= Flags.RotationW; break;
                        case "m_LocalEulerAngles.x": flags |= Flags.EulerX; break;
                        case "m_LocalEulerAngles.y": flags |= Flags.EulerY; break;
                        case "m_LocalEulerAngles.z": flags |= Flags.EulerZ; break;
                        case "localEulerAnglesRaw.x": flags |= Flags.EulerX; break;
                        case "localEulerAnglesRaw.y": flags |= Flags.EulerY; break;
                        case "localEulerAnglesRaw.z": flags |= Flags.EulerZ; break;
                        case "m_LocalScale.x": flags |= Flags.ScaleX; break;
                        case "m_LocalScale.y": flags |= Flags.ScaleY; break;
                        case "m_LocalScale.z": flags |= Flags.ScaleZ; break;
                        default: otherBindings.Add(binding); goto SkipFlagExistence;
                    }

                    if (bindings != null &&
                        bindings.TryGetValue(binding, out var exists))
                    {
                        bindings = null;
                        anyExists = exists;
                    }
                    SkipFlagExistence:

                    if (index + 1 < sortedBindings.Count)
                    {
                        var nextBinding = sortedBindings[index + 1];
                        if (nextBinding.type == typeof(Transform) &&
                            nextBinding.path == binding.path)
                        {
                            index++;
                            binding = nextBinding;
                            goto CheckFlags;
                        }
                    }

                    return flags;
                }

                /************************************************************************************************************************/

                private static void AppendProperty(StringBuilder message, ref bool first, Flags flags,
                    Flags[] propertyFlags, string propertyName, string flagNames)
                {
                    var all = Flags.None;
                    for (int i = 0; i < propertyFlags.Length; i++)
                        all |= propertyFlags[i];

                    if (!HasAny(flags, all))
                        return;

                    AppendSeparator(message, ref first, " ", ", ").Append(propertyName);

                    if (!HasAll(flags, all))
                    {
                        var firstSub = true;

                        for (int i = 0; i < propertyFlags.Length; i++)
                        {
                            if (HasAll(flags, propertyFlags[i]))
                            {
                                AppendSeparator(message, ref firstSub, "(", ", ").Append(flagNames[i]);
                            }
                        }

                        message.Append(')');
                    }
                }

                /************************************************************************************************************************/

                private static StringBuilder AppendSeparator(StringBuilder message, ref bool first, string prefix, string separator)
                {
                    if (first)
                    {
                        first = false;
                        return message.Append(prefix);
                    }
                    else return message.Append(separator);
                }

                /************************************************************************************************************************/
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Logs a description of the issues found when comparing the properties animated by the `state` to the
            /// properties that actually exist on the target <see cref="GameObject"/> and its children.
            /// </summary>
            public void LogIssues(AnimancerState state, MatchType match)
            {
                var animator = state.Root?.Component.Animator;
                var newMatch = match;
                var message = ObjectPool.AcquireStringBuilder();

                switch (match)
                {
                    default:
                    case MatchType.Unknown:
                        message.Append("The animation bindings are still being checked.");
                        Debug.Log(EditorGUIUtility.systemCopyBuffer = message.ReleaseToString(), animator);
                        break;

                    case MatchType.Correct:
                        message.Append("No issues were found when comparing the properties animated by '")
                            .Append(state)
                            .Append("' to the Rig of '")
                            .Append(animator.name)
                            .Append("'.");
                        Debug.Log(EditorGUIUtility.systemCopyBuffer = message.ReleaseToString(), animator);
                        break;

                    case MatchType.Empty:
                        message.Append("'")
                            .Append(state)
                            .Append("' does not animate any properties so it will not do anything.");
                        Debug.Log(EditorGUIUtility.systemCopyBuffer = message.ReleaseToString(), animator);
                        break;

                    case MatchType.Warning:
                        message.Append("Possible Bug Detected: some of the details of '")
                            .Append(state)
                            .Append("' do not match the Rig of '")
                            .Append(animator.name)
                            .Append("' so the animation might not work correctly.");
                        newMatch = GetMatchType(animator, state, message);
                        Debug.LogWarning(EditorGUIUtility.systemCopyBuffer = message.ReleaseToString(), animator);
                        break;

                    case MatchType.Error:
                        message.Append("Possible Bug Detected: the details of '")
                            .Append(state)
                            .Append("' do not match the Rig of '")
                            .Append(animator.name)
                            .Append("' so the animation might not work correctly.");
                        newMatch = GetMatchType(animator, state, message);
                        Debug.LogError(EditorGUIUtility.systemCopyBuffer = message.ReleaseToString(), animator);
                        break;
                }

                if (newMatch != match)
                    Debug.LogWarning($"{nameof(MatchType)} changed from {match} to {newMatch}" +
                        " between the initial check and the button press.");
            }

            /************************************************************************************************************************/

            /// <summary>[Internal] Removes any cached values relating to the `clip`.</summary>
            internal void OnAnimationChanged(AnimationClip clip)
            {
                if (_BindingMatches != null)
                    _BindingMatches.Remove(clip);
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/

        /// <summary>Indicates whether the Unity Editor is currently changing between Play Mode and Edit Mode.</summary>
        public static bool IsChangingPlayMode { get; private set; }

        [InitializeOnLoadMethod]
        private static void WatchForPlayModeChanges()
        {
            if (EditorApplication.isPlaying != EditorApplication.isPlayingOrWillChangePlaymode)
                IsChangingPlayMode = true;

            EditorApplication.playModeStateChanged += (change) =>
            {
                switch (change)
                {
                    case PlayModeStateChange.ExitingEditMode:
                    case PlayModeStateChange.ExitingPlayMode:
                        IsChangingPlayMode = true;
                        break;

                    case PlayModeStateChange.EnteredEditMode:
                    case PlayModeStateChange.EnteredPlayMode:
                    default:
                        IsChangingPlayMode = false;
                        break;
                }
            };
        }

        /************************************************************************************************************************/
        #region GUI
        /************************************************************************************************************************/

        /// <summary>
        /// A summary of the compatability between the properties animated by an <see cref="AnimationClip"/> and the
        /// properties that actually exist on a particular <see cref="GameObject"/> (and its children).
        /// </summary>
        public enum MatchType
        {
            /// <summary>All properties exist.</summary>
            Correct,

            /// <summary>Not yet checked.</summary>
            Unknown,

            /// <summary>The <see cref="AnimationClip"/> does not animate anything.</summary>
            Empty,

            /// <summary>Some of the animated properties do not exist on the object.</summary>
            Warning,

            /// <summary>None of the animated properties exist on the object.</summary>
            Error,
        }

        /************************************************************************************************************************/

        private static readonly GUIStyle ButtonStyle = new GUIStyle();// No margins or anything.

        /************************************************************************************************************************/

        private static readonly int ButtonHash = "Button".GetHashCode();

        /// <summary>
        /// Draws a <see cref="GUI.Button(Rect, GUIContent, GUIStyle)"/> indicating the <see cref="MatchType"/> of the
        /// `state` compared to the object it is being played on.
        /// <para></para>
        /// Clicking the button calls <see cref="BindingData.LogIssues"/>.
        /// </summary>
        public static void DoBindingMatchGUI(ref Rect area, AnimancerState state)
        {
            if (IsChangingPlayMode ||
                !AnimancerPlayableDrawer.VerifyAnimationBindings ||
                state.Root == null ||
                state.Root.Component == null ||
                state.Root.Component.Animator == null)
                goto Hide;

            var animator = state.Root.Component.Animator;
            var bindings = GetBindings(animator.gameObject, false);
            if (bindings == null)
                goto Hide;

            var match = bindings.GetMatchType(animator, state, null, false);
            var icon = GetIcon(match);
            if (icon == null)
                goto Hide;

            var buttonArea = AnimancerGUI.StealFromRight(ref area, area.height, AnimancerGUI.StandardSpacing);
            if (GUI.Button(buttonArea, icon, ButtonStyle))
                bindings.LogIssues(state, match);

            return;

            Hide:
            GUI.Button(default, GUIContent.none, ButtonStyle);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Icons
        /************************************************************************************************************************/

        /// <summary>Get an icon = corresponding to the specified <see cref="MatchType"/>.</summary>
        public static Texture GetIcon(MatchType match)
        {
            switch (match)
            {
                default:
                case MatchType.Correct: return null;
                case MatchType.Unknown: return Icons.GetUnknown();
                case MatchType.Empty: return Icons.Empty;
                case MatchType.Warning: return Icons.Warning;
                case MatchType.Error: return Icons.Error;
            }
        }

        /************************************************************************************************************************/

        /// <summary>A unit test to make sure that the icons are properly loaded.</summary>
        public static void AssertIcons()
        {
            var matchTypes = (MatchType[])Enum.GetValues(typeof(MatchType));

            for (int i = 0; i < matchTypes.Length; i++)
            {
                var match = matchTypes[i];
                var icon = GetIcon(match);
                switch (matchTypes[i])
                {
                    case MatchType.Correct:
                        Debug.Assert(icon == null, $"The icon for {nameof(MatchType)}.{match} should be null.");
                        break;

                    case MatchType.Unknown:
                        for (int iIcon = 0; iIcon < Icons.Unknown.Length; iIcon++)
                            Debug.Assert(Icons.Unknown[iIcon] != null,
                                $"The icon for {nameof(MatchType)}.{nameof(MatchType.Unknown)}[{iIcon}] was not loaded.");
                        break;

                    case MatchType.Empty:
                    case MatchType.Warning:
                    case MatchType.Error:
                    default:
                        Debug.Assert(icon != null, $"The icon for {nameof(MatchType)}.{match} was not loaded.");
                        break;
                }
            }
        }

        /************************************************************************************************************************/

        private static class Icons
        {
            /************************************************************************************************************************/

            public static readonly Texture Empty = EditorGUIUtility.IconContent("console.infoicon.sml").image;
            public static readonly Texture Warning = EditorGUIUtility.IconContent("console.warnicon.sml").image;
            public static readonly Texture Error = EditorGUIUtility.IconContent("console.erroricon.sml").image;

            /************************************************************************************************************************/

            public static readonly Texture[] Unknown =
            {
                EditorGUIUtility.IconContent("WaitSpin00").image,
                EditorGUIUtility.IconContent("WaitSpin01").image,
                EditorGUIUtility.IconContent("WaitSpin02").image,
                EditorGUIUtility.IconContent("WaitSpin03").image,
                EditorGUIUtility.IconContent("WaitSpin04").image,
                EditorGUIUtility.IconContent("WaitSpin05").image,
                EditorGUIUtility.IconContent("WaitSpin06").image,
                EditorGUIUtility.IconContent("WaitSpin07").image,
                EditorGUIUtility.IconContent("WaitSpin08").image,
                EditorGUIUtility.IconContent("WaitSpin09").image,
                EditorGUIUtility.IconContent("WaitSpin10").image,
                EditorGUIUtility.IconContent("WaitSpin11").image,
            };

            public static Texture GetUnknown()
            {
                var i = (int)(EditorApplication.timeSinceStartup * 10) % Unknown.Length;
                return Unknown[i];
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

