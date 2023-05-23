using System;
using UnityEditor;

namespace ET
{
    public static class ToolsEditor
    {
public static void ExcelExporter(CodeMode codeMode, ConfigFolder configFolder)
        {
            string genCode = string.Empty;

#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            switch (codeMode)
            {
                case CodeMode.Client:
                    genCode = $"sh gen_code_client.sh {configFolder}";
                    ShellHelper.Run($"{genCode}", "../Tools/Luban/");
                    break;
                case CodeMode.Server:
                    genCode = $"sh gen_code_server.sh {configFolder}";
                    ShellHelper.Run($"{genCode}", "../Tools/Luban/");
                    break;
                case CodeMode.ClientServer:
                    genCode = $"sh gen_code_client.sh {configFolder}";
                    ShellHelper.Run($"{genCode}", "../Tools/Luban/");
                    
                    genCode = $"sh gen_code_server.sh {configFolder}";
                    ShellHelper.Run($"{genCode}", "../Tools/Luban/");
                    
                    genCode = $"sh gen_code_client_server.sh {configFolder}";
                    ShellHelper.Run($"{genCode}", "../Tools/Luban/");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof (codeMode), codeMode, null);
            }
#else
            switch (codeMode)
            {
                case CodeMode.Client:
                    genCode = $"gen_code_client.bat {configFolder}";
                    ShellHelper.Run($"{genCode}", "../Tools/Luban/");
                    break;
                case CodeMode.Server:
                    genCode = $"gen_code_server.bat {configFolder}";
                    ShellHelper.Run($"{genCode}", "../Tools/Luban/");
                    break;
                case CodeMode.ClientServer:
                    genCode = $"gen_code_client.bat {configFolder}";
                    ShellHelper.Run($"{genCode}", "../Tools/Luban/");

                    genCode = $"gen_code_server.bat {configFolder}";
                    ShellHelper.Run($"{genCode}", "../Tools/Luban/");

                    genCode = $"gen_code_client_server.bat {configFolder}";
                    ShellHelper.Run($"{genCode}", "../Tools/Luban/");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof (codeMode), codeMode, null);
            }
#endif
        }
        
        public static void Proto2CS()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            const string tools = "./Tool";
#else
            const string tools = ".\\Tool.exe";
#endif
            ShellHelper.Run($"{tools} --AppType=Proto2CS --Console=1", "../Bin/");
        }
    }
}