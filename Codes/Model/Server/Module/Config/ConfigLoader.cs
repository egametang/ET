using System.Collections.Generic;
using System.IO;

namespace ET.Server
{
    [Callback(CallbackType.GetAllConfigBytes)]
    public class GetAllConfigBytes: IAction<Dictionary<string, byte[]>>
    {
        public void Handle(Dictionary<string, byte[]> output)
        {
            foreach (string file in Directory.GetFiles($"../Config", "*.bytes"))
            {
                string key = Path.GetFileNameWithoutExtension(file);
                output[key] = File.ReadAllBytes(file);
            }
            output["StartMachineConfigCategory"] = File.ReadAllBytes($"../Config/{Game.Options.StartConfig}/StartMachineConfigCategory.bytes");
            output["StartProcessConfigCategory"] = File.ReadAllBytes($"../Config/{Game.Options.StartConfig}/StartProcessConfigCategory.bytes");
            output["StartSceneConfigCategory"] = File.ReadAllBytes($"../Config/{Game.Options.StartConfig}/StartSceneConfigCategory.bytes");
            output["StartZoneConfigCategory"] = File.ReadAllBytes($"../Config/{Game.Options.StartConfig}/StartZoneConfigCategory.bytes");
        }
    }
    
    [Callback(CallbackType.GetOneConfigBytes)]
    public class GetOneConfigBytes: IFunc<string, byte[]>
    {
        public byte[] Handle(string configName)
        {
            byte[] configBytes = File.ReadAllBytes($"../Config/{configName}.bytes");
            return configBytes;
        }
    }
}