using System;
using System.Collections;
using System.Collections.Generic;
using Halodi.PackageRegistry.Core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Halodi.PackageRegistry.UI
{
    public class RegistryManagerView : EditorWindow
    {
        [MenuItem("ET/Init/Manage scoped registries", false, 21)]
        internal static void ManageRegistries()
        {
            SettingsService.OpenProjectSettings("Project/Package Manager/Credentials");
        }

        private ReorderableList drawer;

        void OnEnable()
        {
            drawer = GetRegistryList(new RegistryManager());
            minSize = new Vector2(640, 320);
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Scoped registries", EditorStyles.whiteLargeLabel);
            drawer.DoLayoutList();
        }

        internal static ReorderableList GetRegistryList(RegistryManager registryManager)
        {
            ReorderableList registryList = null;
            registryList = new ReorderableList(registryManager.registries, typeof(ScopedRegistry), false, true, true, true)
            {
                drawHeaderCallback = rect =>
                {
                    GUI.Label(rect, "Scoped Registries in this project");
                },
                elementHeight = 70,
                drawElementCallback = (rect, index, active, focused) =>
                {
                    var registry = registryList.list[index] as ScopedRegistry;
                    if (registry == null) return;
                    bool registryHasAuth = !string.IsNullOrEmpty(registry.token) && registry.isValidCredential();

                    var rect2 = rect;
                    rect.width -= 60;
                    rect.height = 20;
                    GUI.Label(rect, registry.name + " (" + registry.scopes.Count + " scopes)", EditorStyles.boldLabel);
                    rect.y += 20;
                    GUI.Label(rect, registry.url);
                    rect.y += 20;
                    EditorGUI.BeginDisabledGroup(true);
                    GUI.Toggle(rect, registryHasAuth, "Has Credentials");
                    EditorGUI.EndDisabledGroup();
                    
                    rect2.x = rect2.xMax - 60;
                    rect2.height = 20;
                    rect2.width = 60;
                    rect2.y += 5;
                    if (GUI.Button(rect2, "Edit"))
                    {
                        ScopedRegistryEditorView registryEditor = EditorWindow.GetWindow<ScopedRegistryEditorView>(true, "Edit registry", true);
                        registryEditor.Edit(registry, registryManager);
                    }
                },
                onAddCallback = list =>
                {
                    ScopedRegistryEditorView registryEditor = EditorWindow.GetWindow<ScopedRegistryEditorView>(true, "Add registry", true);
                    registryEditor.CreateNew(registryManager);
                },
                onRemoveCallback = list =>
                {
                    Debug.Log("index to remove: " + list.index);
                    var entry = list.list[list.index] as ScopedRegistry;
                    registryManager.Remove(entry);
                }
            };
            return registryList;
        }
    }



}