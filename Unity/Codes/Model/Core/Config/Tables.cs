
using Bright.Serialization;
using Cysharp.Threading.Tasks;
using System;

namespace ET
{
    public interface IConfigLoader
    {
        UniTask<ByteBuf> Loader(string path);
    }

    public partial class Tables
    {
        private static Tables _instance;
        public static Tables Ins => _instance;

        public async UniTask Init(IConfigLoader loader)
        {
            if (_instance != null)
            {
                return;
            }
            _instance = new Tables();
            await _instance.LoadAsync(loader.Loader);
        }
    }
}
