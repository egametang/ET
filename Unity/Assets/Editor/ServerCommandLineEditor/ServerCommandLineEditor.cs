using UnityEditor;
using UnityEngine;

namespace ET
{
    public class ServerCommandLineEditor: EditorWindow
    {
        public void OnGUI()
        {
            if (GUILayout.Button("启动"))
            {
                string arguments = $"";
                ProcessHelper.Run("App.exe", arguments, "../Bin/");
            }

            if (GUILayout.Button("启动数据库"))
            {
                ProcessHelper.Run("mongod", @"--dbpath=db", "../Database/bin/");
            }

            GUILayout.EndHorizontal();
        }
    }
}