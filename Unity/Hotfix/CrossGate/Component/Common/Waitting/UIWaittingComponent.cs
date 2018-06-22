using ETModel;
using UnityEngine;
//using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class UIWaittingComponentSystem : AwakeSystem<UIWaittingComponent, string>
    {
        public override void Awake(UIWaittingComponent self, string type)
        {
            self.MyType = type;
            self.Awake();
        }
    }

    public class UIWaittingComponent : UIBaseComponent
    {
        //区分倒计时自动销毁标记
        public string MyType;

        //private GameObject UIWaitting;
        //private Text Text;
        private UGUISpriteAnimationComponent Animation;

        public void Awake()
        {
            this.Type = this.MyType;
            //ReferenceCollector rc = this.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            //UIWaitting = rc.Get<GameObject>("UIWaitting");
            //Text = rc.Get<GameObject>("Text").GetComponent<Text>();
            Animation = GetParent<UI>().AddComponent<UGUISpriteAnimationComponent>();
        }

        public override void Show()
        {
            int index = Random.Range(0, 6);
            Sprite[] array = new Sprite[6];
            for (int j = 0; j < 6; j++)
            {
                Sprite sp = Resources.Load<Sprite>("CG/WaittingMouse/CG0281" + (2 + index) + "401_00" + (1 + j));
                array[j] = sp;
            }
            Animation.Init(array);
            Animation.PlayAnimation(0.07f);
            base.Show();
        }

        public override void Close()
        {
            Animation.StopAnimation();
            base.Close();
        }
    }
}
