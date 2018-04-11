using System;
using System.Net;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class HG_UiGameComponentAwakeSystem : AwakeSystem<HG_UIGameComponent>
    {
        public override void Awake(HG_UIGameComponent self)
        {
            self.Awake();
        }
    }
    /// <summary>
    /// 上下左右。名字。得分。还有操作;
    /// </summary>
    public class HG_UIGameComponent: Component
    {
        private EventCenterController eventCenter;
        private GameObject gameObject;
        private Transform transform;
        private TimerComponent timerComponent;
        
        private GameObject Button_Pause;
        private GameObject Button_Right;
        private GameObject Button_Left;
        private GameObject Button_Jump;
        private Text LabTime;
        private Text LabCpuName;
        private Text LabCpuScore;
        private Text LabPlayerName;
        private Text LabPlayerScore;
        
        private AudioSource _audioSource;
        private AudioClip clip;
        private bool canTap = true;

        public void Awake()
        {
            gameObject = this.GetParent<UI>().GameObject;
            transform = gameObject.transform;
            timerComponent = Game.Scene.ModelScene.GetComponent<TimerComponent>();
            eventCenter = Game.Scene.GetComponent<EventCenterController>();
            _audioSource = gameObject.AddComponent<AudioSource>();

            ReferenceCollector rc = gameObject.GetComponent<ReferenceCollector>();
            Button_Pause = rc.Get<GameObject>("Button_Pause");
            Button_Right = rc.Get<GameObject>("Button_Right");
            Button_Left = rc.Get<GameObject>("Button_Left");
            Button_Jump = rc.Get<GameObject>("Button_Jump");
            LabTime = rc.Get<GameObject>("Time").GetComponent<Text>();
            LabCpuName = rc.Get<GameObject>("CpuName").GetComponent<Text>();
            LabCpuScore = rc.Get<GameObject>("CpuGoals").GetComponent<Text>();
            LabPlayerName = rc.Get<GameObject>("PlayerName").GetComponent<Text>();
            LabPlayerScore = rc.Get<GameObject>("PlayerGoals").GetComponent<Text>();

            ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
            clip = (AudioClip)resourcesComponent.GetAsset($"{UIType.HG_Sound}.unity3d", "MenuTap");

            Log.Warning(" 监听   时间变化事件 ");
            eventCenter.AddMsg(HG_WarEvent.HG_WarTimeChange, Event_GetWarTimeChange);
            eventCenter.AddMsg(HG_WarEvent.HG_WarTScoreChange, Event_GetScoreChange);

            Button_Pause.GetComponent<Button>().onClick.Add(BtnClick_PauseGame);

            rc.Get<GameObject>("Button").GetComponent<Button>().onClick.Add(() => { eventCenter.SendMsg(HG_WarEvent.testEvet); });
            rc.Get<GameObject>("Button (1)").GetComponent<Button>().onClick.Add(() => { eventCenter.SendMsg(HG_WarEvent.testEvet,"asdsad"); });
            rc.Get<GameObject>("Button (2)").GetComponent<Button>().onClick.Add(() => { eventCenter.SendMsg(HG_WarEvent.testEvet,2, gameObject); });

            //            
            //            Button_Right.GetComponent<Button>().onClick.Add(BtnClick_MoveRight);
            //            
            //            Button_Left.GetComponent<Button>().onClick.Add(BtnClick_MoveLeft);
            //            
            //            Button_Jump.GetComponent<Button>().onClick.Add(BtnClick_Jump);


        }

        void Event_GetWarTimeChange(object obj)
        {

            Log.Warning($"收到了时间变化事件 {obj}");
            if (obj is int)
            {
                int time = (int)obj;
                LabTime.text = $"{time}";
            }
        }
        void Event_GetScoreChange(object obj,object obj1)
        {
            if (obj is int && obj1 is int)
            {
                int leftScore = (int)obj;
                int rightScore = (int)obj1;
                LabPlayerScore.text = $"{leftScore}";
                LabCpuScore.text = $"{rightScore}";
            }
        }
        
        
        public async void BtnClick_PauseGame()
        {
            try
            {
                Log.Info("  BtnClick_PauseGame  ");
                if (canTap)
                {
                    playSfx(clip);
                    canTap = false;
                    await timerComponent.WaitAsync(250);
                    canTap = true;
                    //                    Log.Info("click BtnClick_PauseGame");
                    //                    Game.EventSystem.Run(EventIdType.InitSceneStart_HDWar);
                    Game.Scene.GetComponent<UIComponent>().Create(UIType.HG_UIPause);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        public async void BtnClick_MoveRight()
        {
            try
            {
                if (canTap)
                {
                    playSfx(clip);
                    canTap = false;
                    await timerComponent.WaitAsync(250);
                    canTap = true;
                    eventCenter.SendMsg(HG_WarEvent.HG_OP_Right);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        public async void BtnClick_MoveLeft()
        {
            try
            {
                if (canTap)
                {
                    playSfx(clip);
                    canTap = false;
                    await timerComponent.WaitAsync(250);
                    canTap = true;
                    
                    eventCenter.SendMsg(HG_WarEvent.HG_OP_Left);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        public async void BtnClick_Jump()
        {
            try
            {
                if (canTap)
                {
                    playSfx(clip);
                    canTap = false;
                    await timerComponent.WaitAsync(250);
                    canTap = true;
                    eventCenter.SendMsg(HG_WarEvent.HG_OP_Jump);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        
        //*****************************************************************************
        // Play sound clips
        //*****************************************************************************
        void playSfx(AudioClip _clip)
        {
            _audioSource.clip = _clip;
            if (!_audioSource.isPlaying)
            {
                _audioSource.Play();
            }
        }
        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            eventCenter.RemoveMsg(HG_WarEvent.HG_WarTimeChange, Event_GetWarTimeChange);
            eventCenter.RemoveMsg(HG_WarEvent.HG_WarTScoreChange, Event_GetScoreChange);
            base.Dispose();
        }
    }
}