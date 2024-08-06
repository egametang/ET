using System;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Halodi.PackageRegistry.UI
{
    public class BulkAddPackages : EditorWindow
    {
        private string PackageList = "";

        //[MenuItem("Packages/Add packages (bulk)", false, 22)]
        internal static void ManageRegistries()
        {
            EditorWindow.GetWindow<BulkAddPackages>(true, "Add packages", true);
        }

        void OnEnable()
        {
            PackageList = "";
            minSize = new Vector2(640, 320);
        }
        void OnGUI()
        {
            EditorGUILayout.LabelField("Add Packages", EditorStyles.whiteLargeLabel);
            EditorGUILayout.Separator();

            PackageList = EditorGUILayout.TextArea(PackageList, GUILayout.Height(200));

            EditorGUILayout.LabelField("Add multiple packages. Place each package on a newline.");
            EditorGUILayout.LabelField("Format:.");
            EditorGUILayout.LabelField("\tLatest version of package: com.halodi.halodi-unity-package-registry-manager");
            EditorGUILayout.LabelField("\tSpecific version: com.halodi.halodi-unity-package-registry-manager@0.1.0");



            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add packages"))
            {
                AddPackages();
                CloseWindow();
            }

            if (GUILayout.Button("Close"))
            {
                CloseWindow();
            }

            EditorGUILayout.EndHorizontal();

        }

        private void AddPackages()
        {
            string result = "";

            bool hasPackages = false;

            using (StringReader reader = new StringReader(PackageList))
            {
                string line = string.Empty;
                while ( (line = reader.ReadLine()) != null)
                {
                    if(!string.IsNullOrEmpty(line))
                    {
                        AddRequest request = UnityEditor.PackageManager.Client.Add(line);  
                        
                        while(!request.IsCompleted)
                        {
                            Thread.Sleep(100);
                        }

                        if(request.Status == StatusCode.Success)
                        {
                            result +=  "Imported: " + line + Environment.NewLine;
                        }
                        else
                        {
                            result += "Cannot import " + line + ": " + request.Error.message + Environment.NewLine;
                        }
                        hasPackages = true;
                    }
                } 
            }

            if(hasPackages)
            {
                EditorUtility.DisplayDialog("Added packages", "Packages added:" + Environment.NewLine + Environment.NewLine + result, "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("No packages entered", "No packages entered.", "OK");
            }
        }

        private void CloseWindow()
        {
            Close();
            GUIUtility.ExitGUI();
        }

    }

}