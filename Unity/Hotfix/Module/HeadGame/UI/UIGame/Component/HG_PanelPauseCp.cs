using System;
using System.Net;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class HG_PanelPauseCpAwakeSystem : AwakeSystem<HG_PanelPauseCp>
    {
        public override void Awake(HG_PanelPauseCp self)
        {
            self.Awake();
        }
    }
    /// <summary>
    /// 上下左右。名字。得分。还有操作;
    /// </summary>
    public class HG_PanelPauseCp : Component
    {
        //private EventCenterController eventCenter;
        private GameObject gameObject;
        private Transform transform;
        private TimerComponent timerComponent;

        private GameObject Btn_Restart;
        private GameObject Btn_Resume;
        private GameObject Btn_Menu;

        private AudioSource _audioSource;
        private AudioClip clip;
        private bool canTap = true;

        internal bool isPaused;
        private float savedTimeScale;
        public GameObject pausePlane;
        enum Status
        {
            PLAY, PAUSE
        }
        private Status currentStatus = Status.PLAY;

        public void Awake()
        {
            gameObject = this.GetParent<UI>().GameObject;
            transform = gameObject.transform;
            timerComponent = Game.Scene.ModelScene.GetComponent<TimerComponent>();
            //eventCenter = Game.Scene.GetComponent<EventCenterController>();
            _audioSource = gameObject.AddComponent<AudioSource>();

            ReferenceCollector rc = gameObject.GetComponent<ReferenceCollector>();
            Btn_Restart = rc.Get<GameObject>("Btn_Restart");
            Btn_Resume = rc.Get<GameObject>("Btn_Resume");
            Btn_Menu = rc.Get<GameObject>("Btn_Menu");

            ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
            clip = (AudioClip)resourcesComponent.GetAsset($"{UIType.HG_Sound}.unity3d", "MenuTap");

            PauseGame();
            Btn_Restart.GetComponent<Button>().onClick.Add(BtnClick_Restart);

            Btn_Resume.GetComponent<Button>().onClick.Add(BtnClick_Resume);

            Btn_Menu.GetComponent<Button>().onClick.Add(BtnClick_LeaveGame);



        }


        public async void BtnClick_Restart()
        {
            try
            {
                Log.Info("  BtnClick_Restart  ");
                if (canTap)
                {
                    playSfx(clip);
                    canTap = false;
                    await timerComponent.WaitAsync(250);
                    canTap = true;
                    //                    Log.Info("click BtnClick_PauseGame");
                    //                    Game.EventSystem.Run(EventIdType.InitSceneStart_HDWar);

                    Game.Scene.GetComponent<UIComponent>().Remove(UIType.HG_UIPause);
                    Game.EventSystem.Run(EventIdType.InitSceneStart_HDRestartWar);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        public async void BtnClick_Resume()
        {
            try
            {
                if (canTap)
                {
                    playSfx(clip);
                    canTap = false;
                    await timerComponent.WaitAsync(250);
                    canTap = true;
                    //eventCenter.SendMsg(HG_WarEvent.HG_OP_Right);
                    Game.Scene.GetComponent<UIComponent>().Remove(UIType.HG_UIPause);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        public async void BtnClick_LeaveGame()
        {
            try
            {
                if (canTap)
                {
                    playSfx(clip);
                    canTap = false;
                    await timerComponent.WaitAsync(250);
                    canTap = true;

                    //eventCenter.SendMsg(HG_WarEvent.HG_OP_Left);
                    Game.Scene.GetComponent<UIComponent>().Remove(UIType.HG_UIPause);
                    Game.EventSystem.Run(EventIdType.InitSceneStart_HDGame);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
     
        void PauseGame()
        {
        
            isPaused = true;
            savedTimeScale = Time.timeScale;
            Time.timeScale = 0;
            AudioListener.volume = 0;

            currentStatus = Status.PAUSE;
        }


        void UnPauseGame()
        {
      
            isPaused = false;
            Time.timeScale = savedTimeScale;
            AudioListener.volume = 1.0f;
      
            currentStatus = Status.PLAY;
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
            UnPauseGame();
            //eventCenter.RemoveMsg(HG_WarEvent.HG_WarTimeChange, Event_GetWarTimeChange);
            //eventCenter.RemoveMsg(HG_WarEvent.HG_WarTScoreChange, Event_GetScoreChange);
            base.Dispose();
        }
    }
}