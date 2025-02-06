using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ET.Editor
{
    /// <summary> 用于显示当前项目中的Proto </summary>
    public class ProtoViewWindow : EditorWindow
    {
        [MenuItem("ET/Proto/ProtoView")]
        static void Init()
        {
            GetWindow<ProtoViewWindow>("Proto View");
        }
        
        //显示
        private bool togSort = false;
        private bool lastTogSort = false;
        private Vector2 scrollViewPos;
        private string vsCodePath = @"C:\Users\18070\AppData\Local\Programs\Microsoft VS Code\Code.exe";
        private string newVSCodePath = @"C:\Users\18070\AppData\Local\Programs\Microsoft VS Code\Code.exe";
        // private Texture2D bgTexture;
        private bool togOpenFolder = false;      //是否同时打开文件夹
    
        //数据
        private List<ItemData> protoItems = new List<ItemData>();
        private string delDataPath;
        private string protoConbinePath;

        // private Process process;

        private void OnEnable()
        {
#if UNITY_EDITOR_WIN
            delDataPath = Application.dataPath.Replace("Assets", "").Replace("/", "\\");
#elif UNITY_EDITOR_OSX
            delDataPath = Application.dataPath.Replace("Assets", "");
            #endif
            protoConbinePath = this.delDataPath + "Temp";
        
            RefreshDataList();

            // bgTexture = CreateTexture(1,1, Color.clear);
        }

        // private void OnDestroy()
        // {
        //     this.CloseVSCode();
        // }

        public void OnGUI()
        {
            GUIStyle rightBtnStyle = new GUIStyle(GUI.skin.FindStyle("button"));
            rightBtnStyle.alignment = TextAnchor.MiddleRight;
            rightBtnStyle.normal.textColor = Color.white; //默认状态下字体颜色
            // rightBtnStyle.normal.background = bgTexture;
            
            GUIStyle errorBtnStyle = new GUIStyle(GUI.skin.FindStyle("button"));
            errorBtnStyle.alignment = TextAnchor.MiddleRight;
            errorBtnStyle.normal.textColor = Color.red; //默认状态下字体颜色
            errorBtnStyle.focused.textColor = Color.red;
            errorBtnStyle.hover.textColor = Color.red;
            // errorBtnStyle.normal.background = bgTexture;
            
            GUIStyle existBtnStyle = new GUIStyle(GUI.skin.FindStyle("button"));
            existBtnStyle.alignment = TextAnchor.MiddleRight;
            existBtnStyle.normal.textColor = Color.yellow;
            existBtnStyle.focused.textColor = Color.yellow;
            existBtnStyle.hover.textColor = Color.yellow;
            // existBtnStyle.normal.background = bgTexture;
            
            GUIStyle rightTxtStyle = new GUIStyle(GUI.skin.FindStyle("label"));
            
            GUIStyle errorTxtStyle = new GUIStyle(GUI.skin.FindStyle("label"));
            errorTxtStyle.normal.textColor = Color.red;
            
            GUIStyle existTxtStyle = new GUIStyle(GUI.skin.FindStyle("label"));
            existTxtStyle.normal.textColor = Color.yellow;
            
            
            GUILayout.Space(5);
            
            //顶部工具栏
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("check", GUILayout.Width(100)))
            {
                RefreshDataList();
            }
            GUILayout.Space(10);
            
            bool newTogSort = GUILayout.Toggle(togSort, "排序");
            if (newTogSort != lastTogSort)
            {
                lastTogSort = newTogSort;
                togSort = newTogSort;
                RefreshDataList();
            }
            GUILayout.Space(10);

            float tmpWidth = 420;
            var rectNormal = GUILayoutUtility.GetRect(tmpWidth, 20);
            Event curEvent = Event.current;
            if (Event.current.type == EventType.MouseUp)
            {
                if (rectNormal.Contains(curEvent.mousePosition))
                {
                    newVSCodePath = EditorUtility.OpenFilePanel("select VSCode Path", newVSCodePath, "");
                }
            }

            if (string.IsNullOrEmpty(newVSCodePath))
            {
                vsCodePath = "点击这里, 设置VSCode路径";
            }
            else
            {
                vsCodePath = this.newVSCodePath;
            }
            EditorGUI.LabelField(rectNormal, this.vsCodePath);
            GUI.Box(new Rect(rectNormal.x-5, rectNormal.y, tmpWidth+10, rectNormal.height), GUIContent.none, EditorStyles.helpBox);
            
            GUILayout.FlexibleSpace ();
            
            if (GUILayout.Button("open in vscode", GUILayout.Width(100)))
            {
                try
                {
                    CombineProtoOpenByVSCode();
                }
                catch (Exception e)
                {
                    Debug.LogError($"打开VSCode报错, 当前VSCode路径:<{this.vsCodePath}>, 具体错误:{e}");
                }
            }
            togOpenFolder = GUILayout.Toggle(togOpenFolder, "");
            
            GUILayout.Space(10);
            if (GUILayout.Button("save", GUILayout.Width(60)))
            {
                SeprateProtoAndSave();
            }
            if (GUILayout.Button("Prot2CS", GUILayout.Width(60)))
            {
                EditorApplication.ExecuteMenuItem("ET/Proto/Proto2CS");
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);

            //内容
            scrollViewPos = GUILayout.BeginScrollView(scrollViewPos);
            for (int i = 0; i < protoItems.Count; i++)
            {
                GUILayout.BeginHorizontal();
                var temBtnStyle = rightBtnStyle;
                var temTxtStyle = rightTxtStyle;
                switch (protoItems[i].errorCode)
                {
                    case 1:
                        temBtnStyle = errorBtnStyle;
                        temTxtStyle = errorTxtStyle;
                        break;
                    case 2:
                        temBtnStyle = existBtnStyle;
                        temTxtStyle = existTxtStyle;
                        break;
                    case 3:
                        temBtnStyle = errorBtnStyle;
                        temTxtStyle = errorTxtStyle;
                        break;
                }
                if (GUILayout.Button(protoItems[i].name, temBtnStyle, GUILayout.Width(250)))
                {
                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(protoItems[i].fullPath.Replace(delDataPath, ""));
                    if (obj != null)
                    {
                        EditorGUIUtility.PingObject(obj);
                        Selection.activeObject = obj;
                    }
                    else
                    {
                        Debug.LogError("File not found at path: " + protoItems[i].fullPath.Replace(delDataPath, "") + ", \n" + delDataPath + ", \n" + Application.dataPath);
                    }
                }
                GUILayout.Label(protoItems[i].fullPath, temTxtStyle);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
 
        private List<string> FindProtoFilesInPackagesDirectory()
        {
            List<string> protoFiles = new List<string>();
            string packagesPath = Path.Combine(Directory.GetCurrentDirectory(), "Packages");
 
            if (Directory.Exists(packagesPath))
            {
                SearchDirectoryForProtoFiles(packagesPath, protoFiles);
            }
            else
            {
                Debug.LogWarning("Packages directory not found at: " + packagesPath);
            }
 
            return protoFiles;
        }
 
        private void SearchDirectoryForProtoFiles(string directoryPath, List<string> protoFiles)
        {
            try
            {
                string[] filePaths = Directory.GetFiles(directoryPath, "*.proto", SearchOption.AllDirectories);
                protoFiles.AddRange(filePaths);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error searching directory for .proto files: " + ex.Message);
            }
        }

        private void CombineProtoOpenByVSCode()
        {
            string folderPath = "";
            string subFolderPath = "";
            
#if UNITY_EDITOR_WIN
            folderPath = protoConbinePath + "\\TmpProto";
            subFolderPath = folderPath + "\\";
#elif UNITY_EDITOR_OSX
            folderPath = protoConbinePath + "/TmpProto";
            subFolderPath = folderPath + "/";
#endif
            
            //先移除所有之前的
            DirectoryInfo directoryInfo = new DirectoryInfo(subFolderPath);
            if (directoryInfo.Exists)
            {
                FileInfo[] files = directoryInfo.GetFiles();
                foreach (FileInfo file in files)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Failed to delete file: " + file.FullName + ". Error: " + ex.Message);
                    }
                }
            }
            
            //增加现有的
            foreach (var item in protoItems)
            {
                if (File.Exists(item.fullPath))
                {
                    if (!Directory.Exists(subFolderPath))
                    {
                        Directory.CreateDirectory(subFolderPath);
                    }
                    File.Copy(item.fullPath, subFolderPath + item.simpPath, true);
                }
            }

            if (togOpenFolder)
            {
                EditorUtility.RevealInFinder(folderPath);
            }

            OpenInVSCode(folderPath);
        }

        private void SeprateProtoAndSave()
        {
            // if (!CloseVSCode()) return;
            
            string folderPath = "";
            string subFolderPath = "";
            
#if UNITY_EDITOR_WIN
            folderPath = protoConbinePath + "\\TmpProto";
            subFolderPath = folderPath + "\\";
#elif UNITY_EDITOR_OSX
            folderPath = protoConbinePath + "/TmpProto";
            subFolderPath = folderPath + "/";
#endif
            
            foreach (var item in protoItems)
            {
                string tmpFilePath = subFolderPath + item.simpPath;
                if (!File.Exists(tmpFilePath))
                {
                    Debug.LogError($"路径不存在: {tmpFilePath}");
                    return;
                }
                File.Copy(tmpFilePath, item.fullPath, true);
            }
            
            AssetDatabase.Refresh();
            
            //移除临时文件中的所有proto
            if (Directory.Exists(subFolderPath))
            {
                foreach (var item in protoItems)
                {
                    string tmpFilePath = subFolderPath + item.simpPath;
                    if (File.Exists(tmpFilePath))
                    {
                        File.Delete(tmpFilePath);
                    }
                }
            }
        }

        private void RefreshDataList()
        {
            protoItems.Clear();
        
            List<string> tmpProtoFiles = FindProtoFilesInPackagesDirectory();
            // Debug.Log($"当前项目中所有的Proto -----------> {tmpProtoFiles.Count}");
            // foreach (string protoFile in tmpProtoFiles)
            // {
            //     Debug.Log(protoFile);
            // }
            foreach (string fullPath in tmpProtoFiles)
            {
                string[] tmpStrs = null;
                #if UNITY_EDITOR_WIN
                tmpStrs = fullPath.Split("\\");
                #elif UNITY_EDITOR_OSX
                tmpStrs = fullPath.Split("/");
                #endif
                string simpPath = tmpStrs[^1];
                string name = simpPath.Split(".")[0];
                string numStr = name.Split("_")[^1];
                int num = -1;
                short errorCode = 0;
                
                if (ProtoStyleIsError(name))
                {
                    Debug.LogError($"Proto文件名 格式有误: {simpPath}");
                    errorCode = 1;
                    protoItems.Add(new ItemData(name, num, simpPath, fullPath, errorCode));
                }
                else
                {
                    if (int.TryParse(numStr, out num))
                    {
                        foreach (var item in protoItems)
                        {
                            if (item.num == num)
                            {
                                errorCode = 2;
                                item.errorCode = errorCode;
                                Debug.LogError($"Proto文件名 数值相同: {num}, {simpPath}, {item.simpPath}");
                            }
                        }
                        protoItems.Add(new ItemData(name, num, simpPath, fullPath, errorCode));
                    }
                    else
                    {
                        Debug.LogError($"Proto文件名 格式错误: {simpPath}");
                        errorCode = 3;
                        protoItems.Add(new ItemData(name, num, simpPath, fullPath, errorCode));
                    }
                }
            }

            if (togSort)
            {
                protoItems.Sort((p1, p2) => p1.num.CompareTo(p2.num));
            }
        }
        
        private void OpenInVSCode(string dirPath)
        {
            Debug.Log($"tackor-----> {dirPath}");
            
            // 创建一个新的进程启动信息
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = vsCodePath,
                // 如果需要传递参数，可以在这里添加
                Arguments = dirPath,
                UseShellExecute        = true,   // 允许操作系统使用 shell 来启动进程
                RedirectStandardOutput = false,  // 通常不需要重定向输出
                RedirectStandardError  = false,  // 通常不需要重定向错误
                CreateNoWindow         = false    // 是否创建新窗口，取决于你的需求
            };

            // process = Process.Start(startInfo);
            using (Process process = Process.Start(startInfo))
            {
                // 如果需要等待进程结束，可以使用 process.WaitForExit()
                // 在这个例子中，我们立即返回
                // process.WaitForExit();
            }
        }

        // private bool CloseVSCode()
        // {
        //     bool haveClosedVSCode = false;
        //     try
        //     {
        //         Log.Debug($"tackor--------> {this.process != null}, {!this.process.HasExited}");
        //         if (process != null && !process.HasExited)
        //         {
        //             process.Kill();
        //             process.WaitForExit(); // 可选：等待进程完全退出
        //             // Debug.Log($"VSCode 关闭了 {this.process.Id}");
        //
        //             haveClosedVSCode = true;
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e);
        //     }
        //
        //     return false;
        // }
        
        private bool ProtoStyleIsError(string input)
        {
            Regex regexC = new Regex("_C_");
            Regex regexS = new Regex("_S_");
            bool isC = regexC.IsMatch(input);
            bool isS = regexS.IsMatch(input);

            bool isCorrect = (isC && !isS) || (!isC && isS);
            return !isCorrect;
        }
        
        private Texture2D CreateTexture(int width, int height, Color color)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = color;
            }
            Texture2D tex = new Texture2D(width, height);
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }

        class ItemData
        {
            public string name;
            public int num;
            public string simpPath;
            public string fullPath;
            public short errorCode;

            public ItemData(string name, int num, string simpPath, string fullPath, short errorCode)
            {
                this.name = name;
                this.num = num;
                this.simpPath = simpPath;
                this.fullPath = fullPath;
                this.errorCode = errorCode;
            }
        }
    }
}