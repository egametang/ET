using System;
using System.Collections.Generic;
using Halodi.PackageRegistry.Core;
using UnityEditor;
using UnityEngine;
using static Halodi.PackageRegistry.Core.UpgradePackagesManager;

namespace Halodi.PackageRegistry.UI
{
    internal class UpgradePackagesView : EditorWindow
    {
        //[MenuItem("Packages/Upgrade Packages", false, 23)]
        internal static void ManageRegistries()
        {
            EditorWindow.GetWindow<UpgradePackagesView>(true, "Upgrade packages", true);
        }

        private UpgradePackagesManager manager;

        private bool upgradeAll;

        private bool showPreviewPackages = false;

        private bool useVerified = true;
        private Dictionary<PackageUpgradeState, bool> upgradeList = new Dictionary<PackageUpgradeState, bool>();

        void OnEnable()
        {
            manager = new UpgradePackagesManager();

            minSize = new Vector2(640, 320);
            upgradeAll = false;
        }

        void OnDisable()
        {
            manager = null;
        }

        private Vector2 scrollPos;

        private void Package(PackageUpgradeState info)
        {

            if(info.HasNewVersion(showPreviewPackages, useVerified))
            {

                GUIStyle boxStyle = new GUIStyle();
                boxStyle.padding = new RectOffset(10, 10, 0, 0);

                EditorGUILayout.BeginHorizontal(boxStyle);


                EditorGUI.BeginChangeCheck();

                bool upgrade = false;
                if (upgradeList.ContainsKey(info))
                {
                    upgrade = upgradeList[info];
                }

                upgrade = EditorGUILayout.BeginToggleGroup(info.GetCurrentVersion(), upgrade);
                if (EditorGUI.EndChangeCheck())
                {
                    if (!upgrade)
                    {
                        upgradeAll = false;
                    }
                }

                upgradeList[info] = upgrade;

                EditorGUILayout.EndToggleGroup();


                EditorGUILayout.LabelField(info.GetNewestVersion(showPreviewPackages, useVerified));

                EditorGUILayout.EndHorizontal();
            }
        }

        void OnGUI()
        {
            if (manager != null)
            {
                manager.Update();

                EditorGUILayout.LabelField("Upgrade packages", EditorStyles.whiteLargeLabel);

                showPreviewPackages = EditorGUILayout.ToggleLeft("Show Preview Packages", showPreviewPackages);

                useVerified = EditorGUILayout.ToggleLeft("Prefer verified packages", useVerified);

                if (manager.packagesLoaded)
                {

                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);




                    foreach (var info in manager.UpgradeablePackages)
                    {
                        Package(info);
                    }

                    EditorGUILayout.EndScrollView();


                    EditorGUI.BeginChangeCheck();
                    upgradeAll = EditorGUILayout.ToggleLeft("Upgrade all packages", upgradeAll);
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var info in manager.UpgradeablePackages)
                        {
                            upgradeList[info] = upgradeAll;
                        }
                    }

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Upgrade"))
                    {
                        Upgrade();
                        CloseWindow();
                    }

                    if (GUILayout.Button("Close"))
                    {
                        CloseWindow();
                    }

                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.LabelField("Loading packages...", EditorStyles.whiteLargeLabel);
                }



            }
        }

        private void Upgrade()
        {

            if (manager != null)
            {

                EditorUtility.DisplayProgressBar("Upgrading packages", "Starting", 0);

                string output = "";
                bool failures = false;
                try
                {
                    foreach (var info in manager.UpgradeablePackages)
                    {
                        if(upgradeList.ContainsKey(info))
                        {
                            if(upgradeList[info])
                            {
                                EditorUtility.DisplayProgressBar("Upgrading packages", "Upgrading " + info.info.displayName, 0.5f);

                                string error = "";
                                if (manager.UpgradePackage(info.GetNewestVersion(showPreviewPackages, useVerified), ref error))
                                {
                                    output += "[Success] Upgraded " + info.info.displayName + Environment.NewLine;
                                }
                                else
                                {
                                    output += "[Error] Failed upgrade of" + info.info.displayName + " with error: " + error + Environment.NewLine;
                                    failures = true;
                                }
                            }
                        }

                    }


                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }


                string message;
                if (failures)
                {
                    message = "Upgraded with errors." + Environment.NewLine + output;
                }
                else
                {
                    message = "Upgraded all packages. " + Environment.NewLine + output;
                }
                EditorUtility.DisplayDialog("Upgrade finished", message, "OK");

            }
        }


        private void CloseWindow()
        {
            Close();
            GUIUtility.ExitGUI();
        }

    }
}