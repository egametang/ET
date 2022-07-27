using System;
using System.Collections.Generic;
using System.IO;

namespace ET.Server
{
    [Callback(CallbackType.GetAllConfigBytes)]
    public class GetAllConfigBytes: IAction<ConfigComponent, Dictionary<string, byte[]>>
    {
        public void Handle(ConfigComponent configComponent, Dictionary<string, byte[]> output)
        {
            List<string> startConfigs = new List<string>()
            {
                "StartMachineConfigCategory", 
                "StartProcessConfigCategory", 
                "StartSceneConfigCategory", 
                "StartZoneConfigCategory",
            };
            HashSet<Type> configTypes = Game.EventSystem.GetTypes(typeof (ConfigAttribute));
            foreach (Type configType in configTypes)
            {
                string configFilePath;
                if (startConfigs.Contains(configType.Name))
                {
                    configFilePath = $"../Config/{Game.Options.StartConfig}/{configType.Name}.bytes";    
                }
                else
                {
                    configFilePath = $"../Config/{configType.Name}.bytes";
                }
                output[configType.Name] = File.ReadAllBytes(configFilePath);
            }
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