using Cysharp.Threading.Tasks;
using ET.Enums;
using FairyGUI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ET
{
    public class FGUI : Singleton<FGUI>
    {
        #region Fields
        private bool _initialized = false;
        private UIPackage _mainPackage;
        #endregion

        #region Apis
        public async UniTask<GComponent> CreateGComponentAsync(string packageName, string resName)
        {
            GComponent com = null;
            string errorMsg = null;
            var success = UniTask.WaitUntil(() => com != null);
            var fail = UniTask.WaitUntil(() => errorMsg.NotEmpty());
            UIPackage.CreateObjectAsync(packageName, resName, (go) =>
            {
                if (go == null)
                {
                    errorMsg = $"创建物体{packageName}.{resName}失败";
                }
                else
                {
                    com = go.asCom;
                }
            });
            await UniTask.WhenAny(success, fail);
            if (errorMsg.NotEmpty())
            {
                throw new System.Exception(errorMsg);
            }
            return com;
        }
        public async UniTask<UIPackage> AddUIPackageAsync(string packageName)
        {
            UIPackage package = UIPackage.GetByName(packageName);
            if (package == null)
            {
                await InitMainAsync();
                await UniTask.WaitUntil(() => _mainPackage != null);
                var bytes = await YooAssetManager.Instance.LoadRawFileBytesAsync(packageName + "_fui");
                package = UIPackage.AddPackage(bytes, packageName, OnLoadResourceFinished);
            }
            return package;
        }
        #endregion

        #region Logics
        private async UniTask InitMainAsync()
        {
            if (_initialized)
            {
                return;
            }
            _initialized = true;
            //加载Main
            var bytes = await YooAssetManager.Instance.LoadRawFileBytesAsync("Main_fui");
            _mainPackage = UIPackage.AddPackage(bytes, "Main", OnLoadResourceFinished);
        }
        #endregion

        #region Callbacks
        public static async void OnLoadResourceFinished(string name, string extension, System.Type type, PackageItem item)
        {
            Log.Debug($"{name}, {extension}, {type}, {item}");

            if (type == typeof(Texture))
            {
                Texture t = await YooAssetManager.Instance.LoadAsync<Texture>(name);
                item.owner.SetItemAsset(item, t, DestroyMethod.Custom);
            }
        }
        #endregion
    }
}
