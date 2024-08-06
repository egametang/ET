using System;
using System.Collections.Generic;
using Halodi.PackageRegistry.Core;
using UnityEditor;
using UnityEngine;

namespace Halodi.PackageRegistry.UI
{
    internal class InstallPackageCreatorView : EditorWindow
    {
        //[MenuItem("Packages/Install Halodi Package Creator", false, 41)]
        internal static void ManageRegistries()
        {
            UnityEditor.PackageManager.Client.Add("com.halodi.halodi-unity-package-creator");  
        }
        

    }
}