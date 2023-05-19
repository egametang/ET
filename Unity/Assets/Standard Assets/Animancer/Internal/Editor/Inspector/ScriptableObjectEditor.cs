// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

// Uncomment this #define to apply this custom editor to all ScriptableObjects.
// If you have another plugin with a custom ScriptableObject editor, you will probably want that one instead.
//#define ANIMANCER_SCRIPTABLE_OBJECT_EDITOR

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// A custom Inspector for <see cref="ScriptableObject"/>s which adds a message explaining that changes in play
    /// mode will persist.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ScriptableObjectEditor
    /// 
#if ANIMANCER_SCRIPTABLE_OBJECT_EDITOR
    [CustomEditor(typeof(ScriptableObject), true, isFallback = true), CanEditMultipleObjects]
#endif
    public class ScriptableObjectEditor : UnityEditor.Editor
    {
        /************************************************************************************************************************/

        /// <summary>Draws the regular Inspector then adds a message explaining that changes in Play Mode will persist.</summary>
        /// <remarks>Called by the Unity editor to draw the custom Inspector GUI elements.</remarks>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (target != null &&
                EditorApplication.isPlayingOrWillChangePlaymode &&
                EditorUtility.IsPersistent(target))
            {
                EditorGUILayout.HelpBox("This is an asset, not a scene object," +
                    " which means that any changes you make to it are permanent" +
                    " and will NOT be undone when you exit Play Mode.", MessageType.Warning);
            }
        }

        /************************************************************************************************************************/
    }
}

#endif

