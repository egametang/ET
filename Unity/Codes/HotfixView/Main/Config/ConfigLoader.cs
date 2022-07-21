using Bright.Serialization;
using Cysharp.Threading.Tasks;

namespace ET
{
    public class ConfigLoader : IConfigLoader
    {
        async UniTask<ByteBuf> IConfigLoader.Loader(string path)
        {
            var bytes = await YooAssetManager.Instance.LoadRawFileBytesAsync(path);
            return new ByteBuf(bytes);
        }
    }
}
