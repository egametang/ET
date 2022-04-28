using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ET
{
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

        public void OnEnable()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo("../Config/StartConfig");
            this.startConfigs = directoryInfo.GetDirectories().Select(x => x.Name).ToArray();
        }

        public void OnGUI()
        {
            selectStartConfigIndex = EditorGUILayout.Popup(selectStartConfigIndex, this.startConfigs);
            this.startConfig = this.startConfigs[this.selectStartConfigIndex];
            
            if (GUILayout.Button("Start Server(Single Srocess)"))
            {
                string arguments = $"Server.dll --Process=1 --StartConfig=StartConfig/{this.startConfig} --Console=1";
                ProcessHelper.Run("dotnet.exe", arguments, "../Bin/");
            }

            if (GUILayout.Button("Start Mongo"))
            {
                ProcessHelper.Run("mongod", @"--dbpath=db", "../Database/bin/");
            }
        }
    }
}