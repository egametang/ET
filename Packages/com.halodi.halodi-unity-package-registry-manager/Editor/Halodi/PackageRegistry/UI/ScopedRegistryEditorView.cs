
using System;
using Halodi.PackageRegistry.Core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Halodi.PackageRegistry.UI
{
    class ScopedRegistryEditorView : EditorWindow
    {
        private bool initialized = false;

        private RegistryManager controller;

        private bool createNew;

        private ScopedRegistry registry;

        private int tokenMethod;
        void OnEnable()
        {
            tokenMethod = 0;

            minSize = new Vector2(480, 320);
        }

        void OnDisable()
        {
            initialized = false;
        }

        public void CreateNew(RegistryManager controller)
        {
            this.controller = controller;
            this.createNew = true;
            this.registry = new ScopedRegistry();
            this.initialized = true;
        }

        public void Edit(ScopedRegistry registry, RegistryManager controller)
        {
            this.controller = controller;
            this.registry = registry;
            this.createNew = false;
            this.initialized = true;
        }


        private ReorderableList scopeList = null;
        void OnGUI()
        {
            if (initialized)
            {
                EditorGUILayout.Space();
                if (createNew)
                {
                    EditorGUILayout.LabelField("Add scoped registry ", EditorStyles.whiteLargeLabel);
                    registry.name = EditorGUILayout.TextField("Name", registry.name);

                    EditorGUI.BeginChangeCheck();
                    registry.url = EditorGUILayout.TextField("URL", registry.url);
                    if (EditorGUI.EndChangeCheck())
                    {
                        UpdateCredential();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Edit scoped registry", EditorStyles.whiteLargeLabel);
                    EditorGUILayout.LabelField("Name", registry.name);
                    EditorGUILayout.LabelField("URL", registry.url);
                }

                if (scopeList == null)
                {
                    scopeList = new ReorderableList(registry.scopes, typeof(string), true, false, true, true)
                    {
                        drawElementCallback = (rect, index, active, focused) =>
                        {
                            registry.scopes[index] = EditorGUI.TextField(rect, registry.scopes[index]);
                        },
                        onAddCallback = list =>
                        {
                            registry.scopes.Add("");
                        }
                    };
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Package Scopes");
                EditorGUILayout.BeginVertical();
                scopeList.DoLayoutList();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Authentication / Credentials", EditorStyles.whiteLargeLabel);
                
                registry.auth = EditorGUILayout.Toggle("Always auth", registry.auth);
                registry.token = EditorGUILayout.TextField("Token", registry.token);

                EditorGUILayout.Space();

                tokenMethod = GetTokenView.CreateGUI(tokenMethod, registry);
                
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.HelpBox("Restart Unity to reload credentials after saving.", MessageType.Info);
                EditorGUILayout.BeginHorizontal();
                if (createNew)
                {
                    if (GUILayout.Button("Add"))
                    {
                        Add();
                    }
                }
                else
                {
                    if (GUILayout.Button("Save"))
                    {
                        Save();
                    }
                }
                
                if (GUILayout.Button("Cancel"))
                {
                    Close();
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void Save()
        {
            if (registry.isValid())
            {
                controller.Save(registry);
                Close();
                GUIUtility.ExitGUI();
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid", "Invalid settings for registry.", "Ok");
            }
        }

        private void Add()
        {
            if (registry.isValid())
            {
                controller.Save(registry);
                Close();
                GUIUtility.ExitGUI();
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid", "Invalid settings for registry.", "Ok");
            }

        }

        private void UpdateCredential()
        {
            if (controller.credentialManager.HasRegistry(registry.url))
            {
                NPMCredential cred = controller.credentialManager.GetCredential(registry.url);
                registry.auth = cred.alwaysAuth;
                registry.token = cred.token;
            }
        }



    }
}
