using UnityEditor;
using System.Diagnostics;

namespace ET
{
    public static class ProtoEditor
    {
        [MenuItem("ET/Proto/Proto2CS")]
        public static void Run()
        {
            Process process = ProcessHelper.DotNet("./Packages/cn.etetet.proto/DotNet~/Exe/ET.Proto2CS.dll", "./", true);

            UnityEngine.Debug.Log(process.StandardOutput.ReadToEnd());
        }

        //快速创建 Proto 文件
        [MenuItem("Assets/Create/ET/Create Proto")]
        static void GenerateProto()
        {
            ProjectWindowUtil.CreateAssetWithContent(
                "Temp.proto",
                "syntax = \"proto3\";\n\npackage ET;\n\n// *************************\n// ******* XXXX *********\n// *************************\n\n\n"
            );
        }

        public static void Init()
        {
            Run();
        }
    }
}