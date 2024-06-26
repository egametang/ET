using System;
using System.Collections.Generic;
using Halodi.PackageRegistry.Core;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace Halodi.PackageRegistry.UI
{
    static class CredentialManagerSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            RegistryManager registryManager;
            ReorderableList credentialDrawer = null;
            ReorderableList registryDrawer = null;
            
            var provider = new SettingsProvider("Project/Package Manager/Credentials", SettingsScope.Project)
            {
                label = "Credentials",
                activateHandler = (str, v) =>
                {
                    registryManager = new RegistryManager();
                    registryDrawer = RegistryManagerView.GetRegistryList(registryManager);
                    credentialDrawer = CredentialManagerView.GetCredentialList(registryManager.credentialManager);
                },
                guiHandler = (searchContext) =>
                {
                    ThirdPartyInfo();

                    EditorGUILayout.Space();
                    registryDrawer.DoLayoutList();
                    
                    EditorGUILayout.Space();
                    credentialDrawer.DoLayoutList();
                },
                footerBarGuiHandler = () =>
                {
                    EditorGUILayout.BeginHorizontal();
#if UNITY_2020_1_OR_NEWER
                    if (GUILayout.Button("Reload Packages"))
                    {
                        // call internal PackageManagerWindow.ResetPackageDatabase();
                        var packageManagerWindow = typeof(Client).Assembly.GetType("UnityEditor.PackageManager.UI.PackageManagerWindow");
                        if (packageManagerWindow != null)
                            packageManagerWindow.GetMethod("ResetPackageDatabase")?.Invoke(packageManagerWindow, new object[] { });

                        Client.Resolve();
                    }
#endif
                    if (GUILayout.Button("Restart Editor"))
                    {
                        if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            EditorApplication.OpenProject(Environment.CurrentDirectory);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                },
                // Populate the search keywords to enable smart search filtering and label highlighting
                keywords = new HashSet<string>(new[] { "UPM", "NPM", "Credentials", "Packages", "Authentication", "Scoped Registry" })
            };

            return provider;
        }
        
        private static class Styles
        {
            public static readonly GUIStyle linkLabel;

            static Styles()
            {
#if UNITY_2019_2_OR_NEWER
                linkLabel = new GUIStyle(EditorStyles.linkLabel);
#else
                linkLabel = new GUIStyle(EditorStyles.miniLabel);
#endif
                linkLabel.fontSize = EditorStyles.miniLabel.fontSize;
                linkLabel.wordWrap = true;
            }
        }

        private static void ThirdPartyInfo()
        {
            EditorGUILayout.HelpBox("Packages on scoped registries are provided by third parties.\n", MessageType.Info);
            var lastRect = GUILayoutUtility.GetLastRect();
            lastRect.xMin = lastRect.xMax - 60;
            lastRect.yMin = lastRect.yMax - 20;
            if (GUI.Button(lastRect, "Read more", Styles.linkLabel))
                Application.OpenURL("https://docs.unity3d.com/Documentation/Manual/upm-scoped.html");
            EditorGUIUtility.AddCursorRect(lastRect, MouseCursor.Link);
        }
    }
}