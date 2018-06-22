using ETModel;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace ETHotfix
{
    [ObjectSystem]
    public class UIWarnningMessageComponentAwakeSystem : AwakeSystem<UIWarnningMessageComponent, string>
    {
        public override void Awake(UIWarnningMessageComponent self, string type)
        {
            self.Awake();
        }
    }

    public class UIWarnningMessageComponent : UIBaseComponent
    {
        //private GameObject UIWarnningMessage;
        private Text Text;
        private Image Bar;
        private Color EmtyColor = new Color(1f, 1f, 1f, 0f);
        private Color OriColor = new Color(1f, 1f, 0f, 1f);
        private Tweener Tween1, Tween2, Tween3;

        public void Awake()
        {
            ReferenceCollector rc = this.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            //UIWarnningMessage = rc.Get<GameObject>("UIWarnningMessage");
            Text = rc.Get<GameObject>("Text").GetComponent<Text>();
            Bar = rc.Get<GameObject>("Bar").GetComponent<Image>();
            Bar.gameObject.SetActive(false);
        }

        public void ShowMessage(string msg)
        {
            Tween1?.Kill();
            Tween2?.Kill();
            Tween3?.Kill();
            Bar.gameObject.transform.localPosition = Vector3.zero;
            Bar.color = OriColor;
            Text.color = OriColor;
            Text.text = msg;
            Bar.gameObject.SetActive(true);
            Tween1 = Text.DOColor(EmtyColor, 2f).SetEase(Ease.Linear);
            Tween2 = Bar.DOColor(EmtyColor, 2f).SetEase(Ease.Linear);
            Tween3 = Bar.transform.DOLocalMoveY(200f, 2f).SetEase(Ease.Linear);
            Tween3.onComplete += () =>
            {
                Bar.gameObject.SetActive(false);
                Bar.color = OriColor;
                Text.color = OriColor;
            };
        }
    }
}
