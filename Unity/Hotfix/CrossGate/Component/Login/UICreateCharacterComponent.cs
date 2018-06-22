using System;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class UICreateCharacterComponentSystem : AwakeSystem<UICreateCharacterComponent, string>
    {
        public override void Awake(UICreateCharacterComponent self, string type)
        {
            self.Awake();
        }
    }

    public class UICreateCharacterComponent : UIBaseComponent
    {
        //角色选择界面
        private int ScecletID = 100002;
        private GameObject SecletBox;
        private GameObject MaleRoot;
        private GameObject FemaleRoot;
        private Button NexPageButton;
        private Button EnterGameButton;

        //男性
        private Button Male_Button_0;
        private Button Male_Button_1;
        private Button Male_Button_2;
        private Button Male_Button_3;
        private Button Male_Button_4;
        private Button Male_Button_5;
        private Button Male_Button_6;
        private Button Male_Button_7;
        private Button Male_Button_8;
        private Button Male_Button_9;
        private Button Male_Button_10;
        private Button Male_Button_11;
        private Button Male_Button_12;
        private Button Male_Button_13;

        //女性
        private Button Female_Button_0;
        private Button Female_Button_1;
        private Button Female_Button_2;
        private Button Female_Button_3;
        private Button Female_Button_4;
        private Button Female_Button_5;
        private Button Female_Button_6;
        private Button Female_Button_7;
        private Button Female_Button_8;
        private Button Female_Button_9;
        private Button Female_Button_10;
        private Button Female_Button_11;
        private Button Female_Button_12;
        private Button Female_Button_13;

        //点数分配界面
        private InputField NameInput;
        private Text TotalPointLeftText;
        private Text HealthPointLeftText;
        private Text StrPointLeftText;
        private Text DefendPointLeftText;
        private Text SpeedPointLeftText;
        private Text MagicPointLeftText;
        private Slider HealthSlider;
        private Slider StrSlider;
        private Slider DefendSlider;
        private Slider SpeedSlider;
        private Slider MagicSlider;

        //水晶
        private Text DimondTotalPointLeftText;
        private Text DiPointLeftText;
        private Text ShuiPointLeftText;
        private Text HuoPointLeftText;
        private Text FengPointLeftText;
        private Slider DiSlider;
        private Slider ShuiSlider;
        private Slider HuoSlider;
        private Slider FengSlider;

        public void Awake()
        {
            ReferenceCollector rc = this.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            //
            SecletBox = rc.Get<GameObject>("SecletBox");
            MaleRoot = rc.Get<GameObject>("Male");
            FemaleRoot = rc.Get<GameObject>("Female");
            NexPageButton = rc.Get<GameObject>("ChangeButton").GetComponent<Button>();
            EnterGameButton = rc.Get<GameObject>("EnterGameButton").GetComponent<Button>();
            NexPageButton.onClick.Add(OnNexPageButton);
            EnterGameButton.onClick.Add(OnEnterGameButton);
            //
            Male_Button_0 = rc.Get<GameObject>("male_Button0").GetComponent<Button>();
            Male_Button_0.onClick.AddListener(delegate { OnHeadSpriteClick(100002, Male_Button_0.gameObject.transform); });
            Male_Button_1 = rc.Get<GameObject>("male_Button1").GetComponent<Button>();
            Male_Button_1.onClick.AddListener(delegate { OnHeadSpriteClick(100027, Male_Button_1.gameObject.transform); });
            Male_Button_2 = rc.Get<GameObject>("male_Button2").GetComponent<Button>();
            Male_Button_2.onClick.AddListener(delegate { OnHeadSpriteClick(100052, Male_Button_2.gameObject.transform); });
            Male_Button_3 = rc.Get<GameObject>("male_Button3").GetComponent<Button>();
            Male_Button_3.onClick.AddListener(delegate { OnHeadSpriteClick(100077, Male_Button_3.gameObject.transform); });
            Male_Button_4 = rc.Get<GameObject>("male_Button4").GetComponent<Button>();
            Male_Button_4.onClick.AddListener(delegate { OnHeadSpriteClick(100102, Male_Button_4.gameObject.transform); });
            Male_Button_5 = rc.Get<GameObject>("male_Button5").GetComponent<Button>();
            Male_Button_5.onClick.AddListener(delegate { OnHeadSpriteClick(100127, Male_Button_5.gameObject.transform); });
            Male_Button_6 = rc.Get<GameObject>("male_Button6").GetComponent<Button>();
            Male_Button_6.onClick.AddListener(delegate { OnHeadSpriteClick(100152, Male_Button_6.gameObject.transform); });
            Male_Button_7 = rc.Get<GameObject>("male_Button7").GetComponent<Button>();
            Male_Button_7.onClick.AddListener(delegate { OnHeadSpriteClick(106002, Male_Button_7.gameObject.transform); });
            Male_Button_8 = rc.Get<GameObject>("male_Button8").GetComponent<Button>();
            Male_Button_8.onClick.AddListener(delegate { OnHeadSpriteClick(106027, Male_Button_8.gameObject.transform); });
            Male_Button_9 = rc.Get<GameObject>("male_Button9").GetComponent<Button>();
            Male_Button_9.onClick.AddListener(delegate { OnHeadSpriteClick(106052, Male_Button_9.gameObject.transform); });
            Male_Button_10 = rc.Get<GameObject>("male_Button10").GetComponent<Button>();
            Male_Button_10.onClick.AddListener(delegate { OnHeadSpriteClick(106077, Male_Button_10.gameObject.transform); });
            Male_Button_11 = rc.Get<GameObject>("male_Button11").GetComponent<Button>();
            Male_Button_11.onClick.AddListener(delegate { OnHeadSpriteClick(106102, Male_Button_11.gameObject.transform); });
            Male_Button_12 = rc.Get<GameObject>("male_Button12").GetComponent<Button>();
            Male_Button_12.onClick.AddListener(delegate { OnHeadSpriteClick(106127, Male_Button_12.gameObject.transform); });
            Male_Button_13 = rc.Get<GameObject>("male_Button13").GetComponent<Button>();
            Male_Button_13.onClick.AddListener(delegate { OnHeadSpriteClick(106152, Male_Button_13.gameObject.transform); });
            //
            Female_Button_0 = rc.Get<GameObject>("Female_Button0").GetComponent<Button>();
            Female_Button_0.onClick.AddListener(delegate { OnHeadSpriteClick(100252, Female_Button_0.gameObject.transform); });
            Female_Button_1 = rc.Get<GameObject>("Female_Button1").GetComponent<Button>();
            Female_Button_1.onClick.AddListener(delegate { OnHeadSpriteClick(100277, Female_Button_1.gameObject.transform); });
            Female_Button_2 = rc.Get<GameObject>("Female_Button2").GetComponent<Button>();
            Female_Button_2.onClick.AddListener(delegate { OnHeadSpriteClick(100302, Female_Button_2.gameObject.transform); });
            Female_Button_3 = rc.Get<GameObject>("Female_Button3").GetComponent<Button>();
            Female_Button_3.onClick.AddListener(delegate { OnHeadSpriteClick(100327, Female_Button_3.gameObject.transform); });
            Female_Button_4 = rc.Get<GameObject>("Female_Button4").GetComponent<Button>();
            Female_Button_4.onClick.AddListener(delegate { OnHeadSpriteClick(100352, Female_Button_4.gameObject.transform); });
            Female_Button_5 = rc.Get<GameObject>("Female_Button5").GetComponent<Button>();
            Female_Button_5.onClick.AddListener(delegate { OnHeadSpriteClick(100377, Female_Button_5.gameObject.transform); });
            Female_Button_6 = rc.Get<GameObject>("Female_Button6").GetComponent<Button>();
            Female_Button_6.onClick.AddListener(delegate { OnHeadSpriteClick(100402, Female_Button_6.gameObject.transform); });
            Female_Button_7 = rc.Get<GameObject>("Female_Button7").GetComponent<Button>();
            Female_Button_7.onClick.AddListener(delegate { OnHeadSpriteClick(106252, Female_Button_7.gameObject.transform); });
            Female_Button_8 = rc.Get<GameObject>("Female_Button8").GetComponent<Button>();
            Female_Button_8.onClick.AddListener(delegate { OnHeadSpriteClick(106277, Female_Button_8.gameObject.transform); });
            Female_Button_9 = rc.Get<GameObject>("Female_Button9").GetComponent<Button>();
            Female_Button_9.onClick.AddListener(delegate { OnHeadSpriteClick(106302, Female_Button_9.gameObject.transform); });
            Female_Button_10 = rc.Get<GameObject>("Female_Button10").GetComponent<Button>();
            Female_Button_10.onClick.AddListener(delegate { OnHeadSpriteClick(106327, Female_Button_10.gameObject.transform); });
            Female_Button_11 = rc.Get<GameObject>("Female_Button11").GetComponent<Button>();
            Female_Button_11.onClick.AddListener(delegate { OnHeadSpriteClick(106352, Female_Button_11.gameObject.transform); });
            Female_Button_12 = rc.Get<GameObject>("Female_Button12").GetComponent<Button>();
            Female_Button_12.onClick.AddListener(delegate { OnHeadSpriteClick(106377, Female_Button_12.gameObject.transform); });
            Female_Button_13 = rc.Get<GameObject>("Female_Button13").GetComponent<Button>();
            Female_Button_13.onClick.AddListener(delegate { OnHeadSpriteClick(106402, Female_Button_13.gameObject.transform); });
            //
            NameInput = rc.Get<GameObject>("NameInputField").GetComponent<InputField>();
            TotalPointLeftText = rc.Get<GameObject>("PointText").GetComponent<Text>();
            HealthPointLeftText = rc.Get<GameObject>("HealthText").GetComponent<Text>();
            StrPointLeftText = rc.Get<GameObject>("StrText").GetComponent<Text>();
            DefendPointLeftText = rc.Get<GameObject>("DefendText").GetComponent<Text>();
            SpeedPointLeftText = rc.Get<GameObject>("SpeedText").GetComponent<Text>();
            MagicPointLeftText = rc.Get<GameObject>("MagicText").GetComponent<Text>();
            //
            DimondTotalPointLeftText = rc.Get<GameObject>("ShuxingPointText").GetComponent<Text>();
            DiPointLeftText = rc.Get<GameObject>("DiText").GetComponent<Text>();
            ShuiPointLeftText = rc.Get<GameObject>("ShuiText").GetComponent<Text>();
            HuoPointLeftText = rc.Get<GameObject>("HuoText").GetComponent<Text>();
            FengPointLeftText = rc.Get<GameObject>("FengText").GetComponent<Text>();
            //
            HealthSlider = rc.Get<GameObject>("HealthSlider").GetComponent<Slider>();
            HealthSlider.onValueChanged.AddListener(OnHealthSlider);
            StrSlider = rc.Get<GameObject>("StrthSlider").GetComponent<Slider>();
            StrSlider.onValueChanged.AddListener(OnStrSlider);
            DefendSlider = rc.Get<GameObject>("DefendSlider").GetComponent<Slider>();
            DefendSlider.onValueChanged.AddListener(OnDefendSlider);
            SpeedSlider = rc.Get<GameObject>("SpeedSlider").GetComponent<Slider>();
            SpeedSlider.onValueChanged.AddListener(OnSpeedSlider);
            MagicSlider = rc.Get<GameObject>("MagicSlider").GetComponent<Slider>();
            MagicSlider.onValueChanged.AddListener(OnMagicSlider);
            //
            DiSlider = rc.Get<GameObject>("DiSlider").GetComponent<Slider>();
            DiSlider.onValueChanged.AddListener(OnDiSlider);
            ShuiSlider = rc.Get<GameObject>("ShuiSlider").GetComponent<Slider>();
            ShuiSlider.onValueChanged.AddListener(OnShuiSlider);
            HuoSlider = rc.Get<GameObject>("HuoSlider").GetComponent<Slider>();
            HuoSlider.onValueChanged.AddListener(OnHuoSlider);
            FengSlider = rc.Get<GameObject>("FengSlider").GetComponent<Slider>();
            FengSlider.onValueChanged.AddListener(OnFengSlider);
        }

        #region 头像回调事件

        private void OnHeadSpriteClick(int id, Transform transform)
        {
            ScecletID = id;
            SecletBox.transform.SetParent(transform);
            SecletBox.transform.localPosition = Vector3.zero;
            SecletBox.transform.localScale = Vector3.one;
        }

        #endregion

        #region Slider回调事件

        //水晶属性
        private void OnDiamondValueChange()
        {
            //播放音效
            int point = GetDimondTotalPoint();
            DimondTotalPointLeftText.text = "水晶属性 - 剩余点数: " + point;
            DiPointLeftText.text = "地: " + DiSlider.value;
            ShuiPointLeftText.text = "水: " + ShuiSlider.value;
            HuoPointLeftText.text = "火: " + HuoSlider.value;
            FengPointLeftText.text = "风: " + FengSlider.value;
        }

        private int GetDimondTotalPoint()
        {
            return 10 - (int)ShuiSlider.value - (int)DiSlider.value - (int)FengSlider.value - (int)HuoSlider.value;
        }

        private void OnFengSlider(float arg0)
        {
            ShuiSlider.interactable = !(arg0 > 0);
            if (ShuiSlider.value < 1f)
            {
                OnDiamondValueChange();
            }

            if (GetDimondTotalPoint() < 0 && arg0 > 0)
            {
                FengSlider.value -= 1;
                OnDiamondValueChange();
            }
        }

        private void OnHuoSlider(float arg0)
        {
            DiSlider.interactable = !(arg0 > 0);
            if (DiSlider.value < 1f)
            {
                OnDiamondValueChange();
            }

            if (GetDimondTotalPoint() < 0 && arg0 > 0)
            {
                HuoSlider.value -= 1;
                OnDiamondValueChange();
            }
        }

        private void OnShuiSlider(float arg0)
        {
            FengSlider.interactable = !(arg0 > 0);
            if (FengSlider.value < 1f)
            {
                OnDiamondValueChange();
            }

            if (GetDimondTotalPoint() < 0 && arg0 > 0)
            {
                ShuiSlider.value -= 1;
                OnDiamondValueChange();
            }
        }

        private void OnDiSlider(float arg0)
        {
            HuoSlider.interactable = !(arg0 > 0);
            if (HuoSlider.value < 1f)
            {
                OnDiamondValueChange();
            }

            if (GetDimondTotalPoint() < 0 && arg0 > 0)
            {
                DiSlider.value -= 1;
                OnDiamondValueChange();
            }
        }

        //人物点数
        private void OnStateSlideChange()
        {
            //播放音效
            int point = GetStateTotalPoint();
            TotalPointLeftText.text = "基本值 - 剩余点数: " + point;
            HealthPointLeftText.text = "体力: " + HealthSlider.value;
            StrPointLeftText.text = "力量: " + StrSlider.value;
            DefendPointLeftText.text = "防御: " + DefendSlider.value;
            SpeedPointLeftText.text = "速度: " + SpeedSlider.value;
            MagicPointLeftText.text = "魔法: " + MagicSlider.value;
        }

        private int GetStateTotalPoint()
        {
            return 30 - (int)HealthSlider.value - (int)StrSlider.value - (int)DefendSlider.value - (int)SpeedSlider.value - (int)MagicSlider.value;
        }

        private void OnMagicSlider(float arg0)
        {
            if (arg0 > 0 && GetStateTotalPoint() < 0)
            {
                MagicSlider.value -= 1;
            }

            OnStateSlideChange();
        }

        private void OnSpeedSlider(float arg0)
        {
            if (arg0 > 0 && GetStateTotalPoint() < 0)
            {
                SpeedSlider.value -= 1;
            }

            OnStateSlideChange();
        }

        private void OnDefendSlider(float arg0)
        {
            if (arg0 > 0 && GetStateTotalPoint() < 0)
            {
                DefendSlider.value -= 1;
            }

            OnStateSlideChange();
        }

        private void OnStrSlider(float arg0)
        {
            if (arg0 > 0 && GetStateTotalPoint() < 0)
            {
                StrSlider.value -= 1;
            }

            OnStateSlideChange();
        }

        private void OnHealthSlider(float arg0)
        {
            if (arg0 > 0 && GetStateTotalPoint() < 0)
            {
                HealthSlider.value -= 1;
            }

            OnStateSlideChange();
        }

        #endregion

        #region 按钮回调事件

        private async void OnEnterGameButton()
        {
            if (this.IsDisposed) return;

            //判断全部条件是否满足
            if (string.IsNullOrEmpty(NameInput.text))
            {
                GameTool.ShowPopMessage("玩家名字不能为空!", true);
                return;
            }
            if (!GameTool.CharacterDetection(NameInput.text))
            {
                GameTool.ShowPopMessage("玩家名字含有非法字符!", true);
                return;
            }
            if (GetStateTotalPoint() > 0)
            {
                GameTool.ShowPopMessage("请将人物基本点数分配完毕!", true);
                return;
            }
            if (GetDimondTotalPoint() > 0)
            {
                GameTool.ShowPopMessage("请将水晶属性点数分配完毕!", true);
                return;
            }

            //发起创建角色请求:

            //打开加载界面
            Game.Scene.GetComponent<UIComponent>().Create(UIType.UIWaitting);
            try
            {
                C2G_CrateCharacter_Request request = new C2G_CrateCharacter_Request
                {
                    PlayerName = NameInput.text,
                    CharacterID = ScecletID,
                    HealthBP = (int)HealthSlider.value,
                    StrBp = (int)StrSlider.value,
                    DefBP = (int)DefendSlider.value,
                    SpeedBP = (int)SpeedSlider.value,
                    MagicBP = (int)MagicSlider.value,
                    Di = (int)DiSlider.value,
                    Shui = (int)ShuiSlider.value,
                    Huo = (int)HuoSlider.value,
                    Feng = (int)FengSlider.value,
                };

                G2C_CrateCharacter_Response g2C_CrateCharacter_Response = await SessionComponent.Instance.Session.Call(request) as G2C_CrateCharacter_Response;
                Game.Scene.GetComponent<UIComponent>().Close(UIType.UIWaitting);

                if (g2C_CrateCharacter_Response.Error != ErrorCode.ERR_Success || g2C_CrateCharacter_Response.Info == null)
                {
                    ErrorHelper.ShowErrorMessage(g2C_CrateCharacter_Response.Error);
                    return;
                }

                //创建成功, 进入游戏
                GameTool.ShowPopMessage("新角色建立成功!");

                //将信息持有到本地
                ClientComponent.Instance.LocalRole = ComponentFactory.Create<Role>();
                ClientComponent.Instance.LocalRole.BasicInfo = g2C_CrateCharacter_Response.Info;

                //切换动画
                Game.Scene.GetComponent<UIComponent>().Create(UIType.UILoadingScene);
                Game.Scene.GetComponent<UIComponent>().Close(UIType.UICreateCharacter);
                Game.Scene.GetComponent<UIComponent>().Close(UIType.UIStartMenu);

                //通过事件调用
                Game.EventSystem.Run(EventIdType.LoginEnterMap);
            }
            catch (Exception e)
            {
                //关闭Loading界面
                Game.Scene.GetComponent<UIComponent>().Close(UIType.UIWaitting);
                GameTool.ShowPopMessage("创建失败, 未知异常!");
                Log.Error(e.ToStr());
            }
        }

        private void OnNexPageButton()
        {
            //切换音效
            MaleRoot.SetActive(!MaleRoot.active);
            FemaleRoot.SetActive(!FemaleRoot.active);
        }

        #endregion

    }
}
