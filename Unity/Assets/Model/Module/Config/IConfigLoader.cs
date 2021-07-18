using System.Collections.Generic;

namespace ET
{
    public interface IConfigLoader
    {
        void GetAllConfigBytes(Dictionary<string, byte[]> output);
        byte[] GetOneConfigBytes(string configName);
    }
}