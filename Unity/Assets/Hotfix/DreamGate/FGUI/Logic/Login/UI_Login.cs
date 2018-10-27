using ETModel;
using UI_Login;

namespace ETHotfix
{
    [ObjectSystem]
    public class UI_LoginAwakeSystem : AwakeSystem<UI_Login>
    {
        public override void Awake(UI_Login self)
        {
            self.Awake();
        }
    }

    public class UI_Login : FGUIBase
    {
        private Login_MainBg view;

        public override void Awake()
        {
            // 对初始化赋值
            this.folderName = "Login";
            this.pakName = "UI_Login";
            this.CurrentUIType.NeedClearingStack = true;
            this.CurrentUIType.UIForms_ShowMode = UIFormsShowMode.HideOther;
            this.CurrentUIType.UIForms_Type = UIFormsType.Normal;

            // 调用基类里的初始化方法 - 注意: base必须在最后调用, 否则将无法获得赋值后的属性
            base.Awake();

            // 获得View层索引: 

            // 绑定当前包内全部组件
            UI_LoginILBinder.BindAll();

            // 创建组件主题 (子组件都在他里面可以获取到)
            this.view = Login_MainBg.CreateInstance();

            // 让父类持有这个物体, 用于框架对UI的管理
            this.GObject = this.view;
            
            //注册按钮回调事件
            //this.view.BubbleEvent.onClick.Add(OnButtonClick_Enter);
        }

        private void OnButtonClick_Enter()
        {
        }
    }
}
