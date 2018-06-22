using UnityEngine;
using UnityEngine.UI;

namespace ETModel
{
    [ObjectSystem]
    public class UiLoadingComponentAwakeSystem : AwakeSystem<UILoadingComponent>
    {
        public override void Awake(UILoadingComponent self)
        {
            self.LogoText = self.GetParent<UI>().GameObject.Get<GameObject>("LogoText").GetComponent<Text>();
            self.ProgressText = self.GetParent<UI>().GameObject.Get<GameObject>("ProgressText").GetComponent<Text>();
            self.LoadingObjectText = self.GetParent<UI>().GameObject.Get<GameObject>("LoadingObjectText").GetComponent<Text>();
            self.SizeText = self.GetParent<UI>().GameObject.Get<GameObject>("SizeText").GetComponent<Text>();
            self.ExitButton = self.GetParent<UI>().GameObject.Get<GameObject>("ExitButton").GetComponent<Button>();
            self.ExitButton.onClick.Add(self.OnExitButtonClick);
        }
    }

    [ObjectSystem]
    public class UiLoadingComponentStartSystem : StartSystem<UILoadingComponent>
    {
        public override async void Start(UILoadingComponent self)
        {
            TimerComponent timerComponent = Game.Scene.GetComponent<TimerComponent>();
            long instanceId = self.InstanceId;
            while (true)
            {
                //异步等待1秒
                await timerComponent.WaitAsync(1000);

                //如果是Unity3d编辑器则直接加载资源?
                if (self.InstanceId != instanceId)
                {
                    return;
                }

                BundleDownloaderComponent bundleDownloaderComponent = Game.Scene.GetComponent<BundleDownloaderComponent>();
                if (bundleDownloaderComponent == null) 
                {
                    continue;
                }
                self.ProgressText.text = "更新进度: " + $"{bundleDownloaderComponent.Progress}%";
                self.SizeText.text = "剩余Kb: " + (long)(bundleDownloaderComponent.RemaningSize / 1000f) + "/kb";
                self.LoadingObjectText.text = "下载中: " + bundleDownloaderComponent.downloadingBundle.Replace(".unity3d", ".so");
            }
        }
    }

    public class UILoadingComponent : Component
    {
        public Text LogoText;
        public Text ProgressText;
        public Text LoadingObjectText;
        public Text SizeText;
        public Button ExitButton;

        public void OnLoadFail()
        {
            ExitButton.gameObject.SetActive(true);
            LoadingObjectText.gameObject.SetActive(false);
            ProgressText.text = "更新失败! 请检查网络环境!";
        }

        public void OnExitButtonClick()
        {
            Application.Quit();
        }
    }
}
