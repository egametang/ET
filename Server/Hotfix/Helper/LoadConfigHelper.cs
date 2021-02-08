using System.Collections.Generic;
using System.IO;

namespace ET
{
    public static class LoadConfigHelper
    {
        public static void LoadAllConfigBytes(Dictionary<string, byte[]> output)
        {
            foreach (string file in Directory.GetFiles($"../Generate/Server/Proto", "*.bytes"))
            {
                string key = $"{Path.GetFileName(file)}.bytes";
                output[key] = File.ReadAllBytes(file);
            }
        }
    }
}