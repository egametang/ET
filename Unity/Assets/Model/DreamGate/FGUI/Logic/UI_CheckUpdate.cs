using UI_CheckUpdate;
using UnityEngine;

namespace ETModel
{
    public enum DownloadState
    {
        Begin,
        Retry,
        Done,
        Quit,
    }

    [ObjectSystem]
    public class UI_CheckUpdateAwakeSystem : AwakeSystem<UI_CheckUpdate>
    {
        public override void Awake(UI_CheckUpdate self)
        {
            self.Awake();
        }
    }

    public class UI_CheckUpdate : FGUIBase
    {
        private UI_MainBg view;
        private DownloadState downloadState;

        public override void Awake()
        {
            // 对初始化赋值
            this.pakName = "UI_CheckUpdate";
            this.CurrentUIType.NeedClearingStack = true;
            this.CurrentUIType.UIForms_ShowMode = UIFormsShowMode.HideOther;
            this.CurrentUIType.UIForms_Type = UIFormsType.Fixed;

            // 调用基类里的初始化方法 - 注意: base必须在最后调用, 否则将无法获得赋值后的属性
            base.Awake();

            // 获得View层索引: 

            // 绑定当前包内全部组件
            UI_CheckUpdateBinder.BindAll();

            // 创建组件主题 (子组件都在他里面可以获取到)
            this.view = UI_MainBg.CreateInstance();

            // 让父类持有这个物体, 用于框架对UI的管理
            this.GObject = this.view;

            // 注册按钮回调事件
            this.view.p_window.p_enterbutton.onClick.Add(OnButtonClick_Enter);

            SetState(DownloadState.Begin);
        }

        private async void OnButtonClick_Enter()
        {
            switch (this.downloadState)
            {
                case DownloadState.Begin:
                case DownloadState.Retry:
                    // 下载
                    SetState(DownloadState.Quit);
                    await BundleHelper.DownloadBundle();
                    break;
                case DownloadState.Done:
                    // 完成
                    Done();
                    break;
                case DownloadState.Quit:
                    // 退出游戏
                    Application.Quit();
                    break;
            }
        }

        private void Done()
        {
            // 关闭检查更新面板
            Game.EventSystem.Run(EventIdType.LoadingFinish);

            Game.Hotfix.LoadHotfixAssembly();

            // 加载配置
            Game.Scene.GetComponent<ResourcesComponent>().LoadBundle("config.unity3d");
            Game.Scene.AddComponent<ConfigComponent>();
            Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle("config.unity3d");
            Game.Scene.AddComponent<OpcodeTypeComponent>();
            Game.Scene.AddComponent<MessageDispatherComponent>();

            Game.Hotfix.GotoHotfix();
        }

        #region Update View

        public void UpdateState(int finishcount, int totalcount, int failcount, string downloadingfile)
        {
            this.view.p_window.p_progrress.text = "更新进度: " + (int) ((finishcount / (float) totalcount * 100)) + "%";
            this.view.p_window.p_finishcount.text = "已完成数量: " + finishcount;
            this.view.p_window.p_downloading.text = "下载中文件: " + downloadingfile;
            this.view.p_window.p_finishcount.text = "下载失败数量: " + failcount;
        }

        public void SetState(DownloadState state)
        {
            switch (state)
            {
                case DownloadState.Begin:
                    this.view.p_window.p_enterbutton.p_buttonlabel.text = "开 始";
                    break;
                case DownloadState.Retry:
                    this.view.p_window.p_enterbutton.p_buttonlabel.text = "重 试";
                    break;
                case DownloadState.Done:
                    this.view.p_window.p_enterbutton.p_buttonlabel.text = "完 成";
                    break;
                case DownloadState.Quit:
                    this.view.p_window.p_enterbutton.p_buttonlabel.text = "退 出";
                    break;
            }
            this.downloadState = state;
        }

        #endregion
    }
}
