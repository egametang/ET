//-----------------------------------------------------------------------
// <copyright file="SyncVarAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY_EDITOR && !UNITY_2019_1_OR_NEWER
#pragma warning disable 0618

namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.Utilities;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Networking;

    /// <summary>
    /// SyncVar attribute drawer.
    /// </summary>
    public class SyncVarAttributeDrawer : OdinAttributeDrawer<SyncVarAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    this.CallNextDrawer(label);
                }
                GUILayout.EndVertical();

                GUILayout.Label("SyncVar", EditorStyles.miniLabel, GUILayoutOptions.Width(52f));
            }
            GUILayout.EndHorizontal();
        }
    }
}

#endif // UNITY_EDITOR && !UNITY_2019_1_OR_NEWER