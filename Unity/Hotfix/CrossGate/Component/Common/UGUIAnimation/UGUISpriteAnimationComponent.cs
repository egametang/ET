using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class UGUISpriteAnimationComponentAwakeSystem : AwakeSystem<UGUISpriteAnimationComponent>
    {
        public override void Awake(UGUISpriteAnimationComponent self)
        {
            self.Awake();
        }
    }

    [ObjectSystem]
    public class UGUISpriteAnimationComponentUpdateSystem : UpdateSystem<UGUISpriteAnimationComponent>
    {
        public override void Update(UGUISpriteAnimationComponent self)
        {
            self.Update();
        }
    }

    public class UGUISpriteAnimationComponent : Component
    {
        private Image ImageSource;
        private Sprite[] SpriteArray;
        private bool IsPlay;
        private int Index;
        private float Interval;
        private float Timmer;
        private int Countter;

        public void Awake()
        {
            ReferenceCollector rc = this.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            ImageSource = rc.Get<GameObject>("Image").GetComponent<Image>();
        }

        public void Update()
        {
            if (IsPlay)
            {
                Timmer += Time.deltaTime;
                if (Timmer >= Interval && SpriteArray != null)
                {
                    ImageSource.color = Color.white;
                    ImageSource.sprite = SpriteArray[Countter];
                    Timmer = 0;
                    Countter++;
                    if (Countter >= SpriteArray.Length) Countter = 0;
                }
            }
        }

        public void Init(Sprite[] array)
        {
            SpriteArray = array;
        }

        public void PlayAnimation(float interval)
        {
            Index = 0;
            Timmer = 0;
            Countter = 0;
            Interval = interval;
            ImageSource.color = new Color(0f, 0f, 0f, 0f);
            IsPlay = true;
        }

        public void StopAnimation()
        {
            IsPlay = false;
            ImageSource.sprite = null;
        }
    }
}
