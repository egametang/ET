using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class UIDisconnectComponentSystem : AwakeSystem<UIDisconnectComponent, string>
    {
        public override void Awake(UIDisconnectComponent self, string type)
        {
            self.MyType = type;
            self.Awake();
        }
    }

    public class UIDisconnectComponent : UIBaseComponent
    {
        //区分倒计时自动销毁标记
        public string MyType;

        private Text Text;
        private Button EnterButton;

        public void Awake()
        {
            this.Type = this.MyType;
            ReferenceCollector rc = this.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            Text = rc.Get<GameObject>("Text").GetComponent<Text>();
            EnterButton = rc.Get<GameObject>("EnterButton").GetComponent<Button>();
            EnterButton.onClick.Add(OnButtonClick);
        }

        public void ShowDefineMessage(string msg)
        {
            Text.text = msg;
        }

        private void OnButtonClick()
        {
            //移除全部UI, 数据等信息
            Game.Scene.GetComponent<UIComponent>().RemoveAll();



            ///
            ///
            ///
            ///
            /// 

            //显示初始界面
            Game.Scene.GetComponent<UIComponent>().Create(UIType.UIStartMenu);
        }
    }
}
