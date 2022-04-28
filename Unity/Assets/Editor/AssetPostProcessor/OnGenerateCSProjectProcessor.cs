using UnityEditor;
using UnityEngine;

namespace ET
{
    public class OnGenerateCSProjectProcessor : AssetPostprocessor
    {
        public static string OnGeneratedCSProject(string path, string content)
        {
            if (path.EndsWith("Unity.Mono.csproj"))
            {
                return content.Replace("<OutputPath>Temp\\Bin\\Debug\\Unity.Mono\\</OutputPath>", "<OutputPath>Temp\\Bin\\Debug\\</OutputPath>");
            }
            return content;
        }
    }
}