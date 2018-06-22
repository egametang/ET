using System.Collections.Generic;
using System.Diagnostics;
using DG.Tweening;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class UIStartMenuComponentSystem : AwakeSystem<UIStartMenuComponent>
    {
        public override void Awake(UIStartMenuComponent self)
        {
            self.Awake();
        }
    }

    public class UIStartMenuComponent : UIBaseComponent
    {
        private Text versionText;
        private Text hotFixText;
        private GameObject cloud_0;
        private GameObject cloud_1;
        private GameObject cloud_2;
        private Button EnterButton;
        private Button TestButton;
        private Tweener Tween;

        public void Awake()
        {
            ReferenceCollector rc = this.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

            hotFixText = rc.Get<GameObject>("HotFixText").GetComponent<Text>();
            versionText = rc.Get<GameObject>("VersionText").GetComponent<Text>();
            cloud_0 = rc.Get<GameObject>("Cloud_0");
            cloud_1 = rc.Get<GameObject>("Cloud_1");
            cloud_2 = rc.Get<GameObject>("Cloud_2");
            EnterButton = rc.Get<GameObject>("EnterButton").GetComponent<Button>();
            TestButton = rc.Get<GameObject>("TestButton").GetComponent<Button>();

            //设置云朵动画
            cloud_0.transform.DOLocalMoveX(-1260f, 20f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
            cloud_1.transform.DOLocalMoveX(1320f, 15f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
            cloud_2.transform.DOLocalMoveX(1300f, 10f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);

            //文字动画
            Tween = EnterButton.gameObject.transform.DOScale(new Vector3(0.95f, 0.95f, 1f), 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);

            //显示当前热更模式
            #if ILRuntime
            hotFixText.text = "(ILRuntime模式)";
            #else
            hotFixText.text = "(Mono模式)";
            #endif

            EnterButton.onClick.Add(OnEnterClick);
            TestButton.onClick.Add(OnTestClick);

            versionText.text = "当前版本: " + ConfigHelper.GetText("GameVersion");
        }

        private void OnEnterClick()
        {
            //点击音效
            Tween.Kill();
            EnterButton.gameObject.SetActive(false);
            //创造登录界面
            Game.Scene.GetComponent<UIComponent>().Create(UIType.UILogin);
        }

        //临时测试专用
        private void OnTestClick()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            for (int i = 0; i < 10; i++)
            {
                ResourceComponent.Instance.GetMapItemSprite("CG00000" + (10 + i));
            }

            watch.Stop();

            //GenerateDbDataHelper.MakeImageInfo("2.0");
            //GenerateDbDataHelper.MakeImageInfo("3.0");

            /*
            Dictionary<int, ImageConfig> temp = new Dictionary<int, ImageConfig>();

        

            var sql = SqlConnectHelper.GetSQLiteConnection("imageinfo2.0.db");

            ImageConfig info = sql.Table<ImageConfig>().Where(d => d.Id == 98502).FirstOrDefault();

            //Log.Debug(info.MapId + "|" + info.PngId);

            ImageConfig info1 = sql.Table<ImageConfig>().Where(d => d.Id == 12638).FirstOrDefault();

            //Log.Debug(info1.MapId + "|" + info1.PngId);

            watch.Stop();
            sql.Dispose();
            
            Log.Debug("Done! db查询耗时: " + watch.ElapsedMilliseconds + "ms");

            temp.Add(info.Id, info);
            temp.Add(info1.Id, info1);

            watch.Restart();

            var a = temp[98502];
            var aa = temp[12638];

            watch.Stop();
            Log.Debug("Done! 字典查询耗时: " + watch.ElapsedMilliseconds + "ms");
            */

            Log.Debug("Done! 耗时--------------------------" + watch.ElapsedMilliseconds + "ms");
        }
    }
}
