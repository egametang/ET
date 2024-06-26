
using System;
using Halodi.PackageRegistry.Core;
using UnityEditor;
using UnityEngine;

namespace Halodi.PackageRegistry.UI
{
    class CredentialEditorView : EditorWindow
    {
        private bool initialized = false;

        private CredentialManager credentialManager;

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

        public void CreateNew(CredentialManager credentialManager)
        {
            this.credentialManager = credentialManager;
            this.createNew = true;
            this.registry = new ScopedRegistry();
            this.initialized = true;
        }

        public void Edit(NPMCredential credential, CredentialManager credentialManager)
        {
            this.credentialManager = credentialManager;
            this.registry = new ScopedRegistry();
            this.registry.url = credential.url;
            this.registry.auth = credential.alwaysAuth;
            this.registry.token = credential.token;

            this.createNew = false;
            this.initialized = true;
        }

        void OnGUI()
        {
            if (initialized)
            {
                if (createNew)
                {
                    EditorGUILayout.LabelField("Add credential", EditorStyles.whiteLargeLabel);

                    registry.url = EditorGUILayout.TextField("Registry URL", registry.url);
                }
                else
                {
                    EditorGUILayout.LabelField("Edit credential", EditorStyles.whiteLargeLabel);
                    EditorGUILayout.LabelField("Registry URL: " + registry.url);
                }
                
                if(string.IsNullOrEmpty(registry.url))
                {
                    EditorGUILayout.HelpBox("Enter the registry URL you want to add authentication for.", MessageType.Warning);
                }

                registry.auth = EditorGUILayout.Toggle("Always auth", registry.auth);
                registry.token = EditorGUILayout.TextField("Token", registry.token);

                EditorGUILayout.Space();

                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(registry.url));
                tokenMethod = GetTokenView.CreateGUI(tokenMethod, registry);
                
                if (!string.IsNullOrEmpty(registry.url) && string.IsNullOrEmpty(registry.token))
                {
                    EditorGUILayout.HelpBox("Select an authentication method and click on \"Get token\"", MessageType.Warning);
                }
                
                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(registry.token));
                
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                EditorGUILayout.EndVertical();

                EditorGUILayout.HelpBox("Restart Unity to reload credentials after saving.", MessageType.Info);
                EditorGUILayout.BeginHorizontal();
                if (createNew)
                {
                    if (GUILayout.Button("Add"))
                    {
                        Save();
                    }
                }
                else
                {
                    if (GUILayout.Button("Save"))
                    {
                        Save();
                    }
                }
                
                EditorGUI.EndDisabledGroup();
                EditorGUI.EndDisabledGroup();

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
            if (registry.isValidCredential() && !string.IsNullOrEmpty(registry.token))
            {
                credentialManager.SetCredential(registry.url, registry.auth, registry.token);
                credentialManager.Write();
                
                // TODO figure out in which cases/Editor versions a restart is actually required,
                // and where a Client.Resolve() call or PackMan reload is sufficient
                if (EditorUtility.DisplayDialog("Unity Editor restart might be required", "The Unity editor might need to be restarted for this change to take effect.", "Restart Now", "Cancel"))
                {
                    EditorApplication.OpenProject(Environment.CurrentDirectory);
                }
                
                Close();
                GUIUtility.ExitGUI();
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid", "Invalid settings for credential.", "Ok");
            }
        }



    }
}
