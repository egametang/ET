using System;
using System.Net;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class HG_UiMenuComponentAwakeSystem : AwakeSystem<HG_UIMenuComponent>
    {
        public override void Awake(HG_UIMenuComponent self)
        {
            self.Awake();
        }
    }
    //[ObjectSystem]
    //public class HG_UIMenuComponentFixUpdateSystem : LateUpdateSystem<HG_UIMenuComponent>
    //{
    //    public override void LateUpdate(HG_UIMenuComponent self)
    //    {
    //        self.FixedUpdate();
    //    }
    //}

    /// <summary>
    /// 菜单组件
    /// 
    /// </summary>
    public class HG_UIMenuComponent : Component
    {
        private GameObject Button_Start;
        private GameObject Button_Options;
        private GameObject Button_Next;
        private GameObject Button_Prev;

        private AudioSource _audioSource;
        private AudioClip clip;
        private bool canTap = true;
        private TimerComponent timerComponent;
        private RawImage PlayerAvatar;

     
            public void Awake()
        {
            canTap = true;
            ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
            clip = (AudioClip)resourcesComponent.GetAsset($"{UIType.HG_Sound}.unity3d", "MenuTap");
            timerComponent = Game.Scene.ModelScene.GetComponent<TimerComponent>();

            GameObject gameObject = this.GetParent<UI>().GameObject;
            ReferenceCollector rc = gameObject.GetComponent<ReferenceCollector>();
            _audioSource = gameObject.AddComponent<AudioSource>();

            PlayerAvatar =  rc.Get<GameObject>("PlayerAvatar").GetComponent<RawImage>();
            //添加自动义组件;
            UI ui = ComponentFactory.Create<UI, GameObject>(PlayerAvatar.gameObject);
            ui.AddComponent<HeartBeatAnimationEffectComponent>();
            
            Button_Start = rc.Get<GameObject>("Button_Start");
            Button_Start.GetComponent<Button>().onClick.Add(BtnClick_StartGame);
            
            Button_Options = rc.Get<GameObject>("Button_Options");
            Button_Options.GetComponent<Button>().onClick.Add(BtnClick_OpenOptions);
            
            Button_Next = rc.Get<GameObject>("Button_Next");
            Button_Next.GetComponent<Button>().onClick.Add(BtnClick_NextPlay);
            
            Button_Prev = rc.Get<GameObject>("Button_Prev");
            Button_Prev.GetComponent<Button>().onClick.Add(BtnClick_PrePlay);

//            while (true)
//            {
//                await timerComponent.WaitAsync(1000);
//            }
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

        public async void BtnClick_StartGame()
        {
            try
            {
                if (canTap)
                {
                    playSfx(clip);
                    canTap = false;
                    await timerComponent.WaitAsync(250);
                    Log.Info("click start");
                    Game.EventSystem.Run(EventIdType.InitSceneStart_HDWar);
                   
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        public async void BtnClick_OpenOptions()
        {
            try
            {
                if (canTap)
                {
                    playSfx(clip);
                    canTap = false;
                    await timerComponent.WaitAsync(250);
                    Log.Info("click open opart");
                    canTap = true;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        public async void BtnClick_NextPlay()
        {
            try
            {
                if (canTap)
                {
                    playSfx(clip);
                    canTap = false;
                    await timerComponent.WaitAsync(250);
                    Log.Info("click next");
                    canTap = true;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        public async void BtnClick_PrePlay()
        {
            try
            {
                if (canTap)
                {
                    playSfx(clip);
                    canTap = false;
                    await timerComponent.WaitAsync(250);
                    Log.Info("click pre");
                    canTap = true;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();
        }
    }
}