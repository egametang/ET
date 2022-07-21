
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
        public static Tables Ins
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Tables();
                }
                return _instance;
            }
        }
        private bool _initialized = false;
        public async UniTask Init(IConfigLoader loader)
        {
            if (_initialized)
            {
                return;
            }
            _initialized = true;
            await _instance.LoadAsync(loader.Loader);
        }
    }
}
