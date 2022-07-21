using Bright.Serialization;
using Cysharp.Threading.Tasks;
using System.IO;

namespace ET
{
    public class ConfigLoader : IConfigLoader
    {
        public async UniTask<ByteBuf> Loader(string path)
        {
            var bytes = await File.ReadAllBytesAsync($"../Server/ConfigBin/{path}.bytes");
            return new ByteBuf(bytes);
        }
    }
}
