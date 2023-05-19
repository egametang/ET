// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR && !UNITY_2019_1_OR_NEWER

using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>
    /// For some reason Unity executes <see cref="PropertyDrawer"/>s differently in the default Inspector than it does
    /// in a custom Inspector (even an empty one like this) and that difference causes the <see cref="TimeRuler"/> to
    /// not display properly without a custom Inspector.
    /// <para></para>
    /// Since it is set as a fallback, any other custom Inspector should automatically override it, but if not you can
    /// simply delete this file (or comment it out).
    /// </summary>
    [CustomEditor(typeof(Object), true, isFallback = true), CanEditMultipleObjects]
    internal sealed class DummyObjectEditor : UnityEditor.Editor { }
}

#endif
