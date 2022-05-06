using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public enum DevelopMode
    {
        正式 = 0,
        开发 = 1,
        压测 = 2,
    }
    
    public class ServerCommandLineEditor: EditorWindow
    {
        [MenuItem("Tools/ServerTools")]
        public static void ShowWindow()
        {
            GetWindow(typeof (ServerCommandLineEditor));
        }
        
        private int selectStartConfigIndex;
        private string[] startConfigs;
        private string startConfig;
        private DevelopMode developMode;

        public void OnEnable()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo("../Config/StartConfig");
            this.startConfigs = directoryInfo.GetDirectories().Select(x => x.Name).ToArray();
        }

        public void OnGUI()
        {
            selectStartConfigIndex = EditorGUILayout.Popup(selectStartConfigIndex, this.startConfigs);
            this.startConfig = this.startConfigs[this.selectStartConfigIndex];
            this.developMode = (DevelopMode) EditorGUILayout.EnumPopup("起服模式：", this.developMode);
            int develop = (int) this.developMode;
            
            if (GUILayout.Button("Start Server(Single Srocess)"))
            {
                string arguments = $"App.dll --Process=1 --StartConfig=StartConfig/{this.startConfig} --Console=1";
                ProcessHelper.Run("dotnet.exe", arguments, "../Bin/");
            }
            
            if (GUILayout.Button("Start Watcher"))
            {
                string arguments = $"App.dll --AppType=Watcher --StartConfig=StartConfig/{this.startConfig} --Console=1";
                ProcessHelper.Run("dotnet.exe", arguments, "../Bin/");
            }

            if (GUILayout.Button("Start Mongo"))
            {
                ProcessHelper.Run("mongod", @"--dbpath=db", "../Database/bin/");
            }
        }
    }
}