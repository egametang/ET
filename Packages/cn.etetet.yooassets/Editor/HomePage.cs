using System;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    internal class HomePageWindow
    {
        [MenuItem("YooAsset/Home Page", false, 1)]
        public static void OpenWindow()
        {
            Application.OpenURL("https://www.yooasset.com/");
        }
    }
}