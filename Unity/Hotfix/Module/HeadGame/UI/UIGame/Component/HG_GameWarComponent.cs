using System.Net;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class HG_GameWarComponentAwakeSystem : AwakeSystem<HG_GameWarComponent>
    {
        public override void Awake(HG_GameWarComponent self)
        {
            self.Awake();
        }
    }

    [ObjectSystem]
    public class HG_GameWarComponentUpdataSystem : UpdateSystem<HG_GameWarComponent>
    {
        public override void Update(HG_GameWarComponent self)
        {
            self.Update();
        }
    }
    //[ObjectSystem]
    //public class HG_GameWarComponentFixUpdateSystem : LateUpdateSystem<HG_GameWarComponent>
    //{
    //    public override void LateUpdate(HG_GameWarComponent self)
    //    {
    //        self.FixedUpdate();
    //    }
    //}
    /// <summary>
    /// 战斗中的 操作。 
    /// 自己的控制组件；
    /// 电脑的或者敌人的。
    /// 足球的;
    /// </summary>
    public class HG_GameWarComponent : Component
    {
        public static int gameTime = 10; //gameplay time for each round
        private int remainingTime; //time left to finish the game
        public static int playerGoals; //total goals by player
        public static int cpuGoals; //total goals by cpu
        public static long startDelay = 3000; //cooldown timer before starting the game

        public static bool gameIsFinished = false; //global flag to finish the game
        private bool gameFinishFlag = false; //private flag

        public AudioClip startWistle; //Audioclip
        public AudioClip endWistle; //Audioclip
        public AudioClip[] goalReceived; //Audioclip
        public AudioClip[] goalLanded; //Audioclip

        //Reference to important game objects used inside the game
        public GameObject counter;

        public GameObject ball;
        public GameObject goalPlane;


        //	    public GameObject hudPlayerGoals; // event 发出去
        //	    public GameObject hudCpuGoals;// event 发出去
        //	    public GameObject hudGameTime;// event 发出去

        //通过其他面板弄;
        public GameObject GameFinishPlane;

        public GameObject gameFinishStatusImage;

        public Texture2D[] statusImages; //available images for end game status

        private bool isFirstRun = true; //flag to initialize the game
        public static bool isGoalTransition = false; //flag to manage after goal events


        private GameObject gameObject;
        private Transform transform;
        private TimerComponent timerComponent;
        private EventCenterController eventCenter;
        private HG_PlayerControllerCp plCp;
        private int mapMask;
        Camera curCamera;
        HG_CpuControllerCp cpuCp;
        HG_BallControllerCp battleCp;

        public void Awake()
        {
            isFirstRun = true;
            gameObject = this.GetParent<UI>().GameObject;
            
            transform = gameObject.transform;
            ReferenceCollector rc = gameObject.GetComponent<ReferenceCollector>();
            gameObject.AddComponent<AudioSource>();
            timerComponent = Game.Scene.ModelScene.GetComponent<TimerComponent>();

            eventCenter = Game.Scene.GetComponent<EventCenterController>();


            GameObject PlayerObj = rc.Get<GameObject>("Player");
            //添加自动义组件;
            UI ui = ComponentFactory.Create<UI, GameObject>(PlayerObj);
            plCp = ui.AddComponent<HG_PlayerControllerCp>();

            GameObject CPUObj = rc.Get<GameObject>("CPU");


            ball = rc.Get<GameObject>("Ball");
            //添加自动义组件;
            UI CPUui = ComponentFactory.Create<UI, GameObject>(CPUObj);
            cpuCp = CPUui.AddComponent<HG_CpuControllerCp>();

            //添加自动义组件;
            UI Ballui = ComponentFactory.Create<UI, GameObject>(ball);
            battleCp = Ballui.AddComponent<HG_BallControllerCp, HG_GameWarComponent, HG_PlayerControllerCp, HG_CpuControllerCp>(this, plCp, cpuCp);
            cpuCp.SetBall(ball);

            startWistle = rc.Get<AudioClip>("startWistle");
            endWistle = rc.Get<AudioClip>("gameIsFinishedWistle");
            counter = rc.Get<GameObject>("Counter");
            goalPlane = rc.Get<GameObject>("GoalPlane");

            ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();


            AudioClip goalSd = (AudioClip)resourcesComponent.GetAsset($"{UIType.HG_Sound}.unity3d", "goal-received");
            goalReceived = new[] { goalSd };
            AudioClip goalLandSd1 =
                (AudioClip)resourcesComponent.GetAsset($"{UIType.HG_Sound}.unity3d", "goal-landed-01");
            AudioClip goalLandSd2 =
                (AudioClip)resourcesComponent.GetAsset($"{UIType.HG_Sound}.unity3d", "goal-landed-02");
            goalLanded = new[] { goalLandSd1, goalLandSd2 };

            gameIsFinished = false;
            gameFinishFlag = false;
            playerGoals = 0;
            cpuGoals = 0;
            eventCenter.AddMsg(HG_WarEvent.HG_OP_Left, Event_OP_Left);
            eventCenter.AddMsg(HG_WarEvent.HG_OP_Right, Event_OP_Right);
            eventCenter.AddMsg(HG_WarEvent.HG_OP_Jump, Event_OP_Jump);
            //eventCenter.AddMsg(HG_WarEvent.testEvet, GetMsg);
            //eventCenter.AddMsg(HG_WarEvent.testEvet, GetMsg1);
            //eventCenter.AddMsg(HG_WarEvent.testEvet, GetMsg2);
            //eventCenter.RemoveMsg(HG_WarEvent.testEvet, GetMsg2);
            mapMask = LayerMask.GetMask("UI");
            curCamera = Camera.current;
            remainingTime = gameTime;
            eventCenter.SendMsg(HG_WarEvent.HG_WarTimeChange, remainingTime);
            //Log.Warning($"发送了剩余时间 {remainingTime} ");
            passTime = 0;
            init();

          
        }
        void GetMsg()
        {
            Log.Info("get new evnet 没参数");
        }
        void GetMsg1<T>(T obj)
        {
            Log.Info($"get new evnet 参数1 {obj} ");
        }
        void GetMsg2(object obj, object obj1)
        {
            Log.Info($"get new evnet 参数2  {obj}  {obj1}");
        }

        int passTime = 0;
        GameObject Counter;
        //setup the game for the first run
        async void init()
        {
            //if this is the first run, init and stop the physics for a bit
            if (isFirstRun)
            {
                //freeze the ball
                ball.GetComponent<Rigidbody>().useGravity = false;
                ball.GetComponent<Rigidbody>().isKinematic = true;

                //show the countdown timer
                Counter =
                    GameObject.Instantiate(counter, new Vector3(0, 1.8f, -1),
                        Quaternion.Euler(0, 180, 0)) as GameObject;
                Counter.name = "Starting-Time-Counter";
                //			yield return new WaitForSeconds(startDelay);
                await timerComponent.WaitAsync(startDelay);
                //start the game
                isFirstRun = false;
                playSfx(startWistle);
                //			yield return new WaitForSeconds(startWistle.length);
                await timerComponent.WaitAsync((long)(startWistle.length * 1000));

                if(!this.IsDisposed)
                {
                    //unfreeze the ball
                    ball.GetComponent<Rigidbody>().useGravity = true;
                    ball.GetComponent<Rigidbody>().isKinematic = false;
                    while (true)
                    {
                        await timerComponent.WaitAsync(1000);
                        passTime++;
                        //show game timer
                        manageGameTime();
                    }
                }
           
            }
        }
        /// <summary>
        /// Move player with keyboard keys.
        /// </summary>
        void movePlayerWithKeyboard()
        {

            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                //Log.Info("left click");
                plCp.moveLeft();
            }

            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                //Log.Info("right click");
                plCp.moveRight();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                //Log.Info("jump click");
                plCp.doJump();
            }

        }


        public void Update()
        {
            if(this.IsDisposed)
            {
                Log.Warning("game war cp update   when disposed");
            }
            
      

            if (!gameIsFinished)
                manageGoals();

            if (Input.GetMouseButtonDown(1))
            {
                Log.Info("mouse dwon");
                //                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //                RaycastHit hit;
                //                if (Physics.Raycast(ray, out hit, 1000))
                //                {
                //                    
                //                }
            }

            if (!HG_GameWarComponent.isGoalTransition && !HG_GameWarComponent.gameIsFinished)
            {
                movePlayerWithKeyboard();
                movePlayerWithTouch();
            }

            //if time is up, setup game finish events
            if (gameIsFinished && !gameFinishFlag)
            {
                gameFinishFlag = true;

                //play end wistle
                playSfx(endWistle);
                bool? isWin = null;
                //declare the winner
                if (playerGoals > cpuGoals)
                {
                    isWin = true;
                    Log.Info("Player Wins");
                    //                    gameFinishStatusImage.GetComponent<Renderer>().material.mainTexture = statusImages[0];
                }
                else if (playerGoals < cpuGoals)
                {
                    isWin = false;
                    Log.Info("CPU Wins");
                    //                    gameFinishStatusImage.GetComponent<Renderer>().material.mainTexture = statusImages[1];
                }
                else if (playerGoals == cpuGoals)
                {
                    Log.Info("Draw!");
                    //                    gameFinishStatusImage.GetComponent<Renderer>().material.mainTexture = statusImages[2];
                }

                //show gamefinish plane
                //GameFinishPlane.SetActive(true);

                UI ui = Game.Scene.GetComponent<UIComponent>().Create<bool?>(UIType.HG_UIResult, isWin);
            }
        }
        private RaycastHit hitInfo;
        private RaycastHit hitInfo2;
        private Ray ray;
        private Ray ray2;
        public void FixedUpdate()
        {

        }
        void movePlayerWithTouch()
        {
            //Log.Info($"touch num is {	Input.touches.Length}");
            if (Input.touches.Length > 0 && curCamera)
            {
                //                Camera.current
                ray = curCamera.ScreenPointToRay(Input.touches[0].position);
                if (Input.touches.Length > 1)
                    ray2 = curCamera.ScreenPointToRay(Input.touches[1].position);
                else
                    ray2 = curCamera.ScreenPointToRay(Input.touches[0].position);
            }
            else if (Input.GetMouseButton(0) && curCamera)
                ray = curCamera.ScreenPointToRay(Input.mousePosition);
            else
                return;


            //Log.Info("check btn click");
            if (Physics.Raycast(ray, out hitInfo, 1000))
            {
                GameObject objectHit = hitInfo.transform.gameObject;
                //Log.Info($"btn click namge is {objectHit.name}");
                switch (objectHit.name)
                {
                    case "Button_Jump":

                        plCp.doJump();
                        break;
                    case "Button_Left":
                        plCp.moveLeft();
                        break;
                    case "Button_Right":
                        plCp.moveRight();
                        break;
                }
            }

            if (Physics.Raycast(ray2, out hitInfo2, 1000))
            {
                GameObject objectHit2 = hitInfo2.transform.gameObject;
                switch (objectHit2.name)
                {
                    case "Button_Jump":

                        plCp.doJump();
                        break;
                    case "Button_Left":
                        plCp.moveLeft();
                        break;
                    case "Button_Right":
                        plCp.moveRight();
                        break;
                }
            }
        }


        /// <summary>
        /// Update game data after a goal happens.
        /// Arg Parameters:
        /// 0 = received a goal
        /// 1 = scored a goal
        /// </summary>
        public async void manageGoalEvent(int _side)
        {
            //fake pause the game for a while
            isGoalTransition = true;

            //blow the wistle && add the score
            if (_side == 0)
            {
                //player received a goal
                playSfx(goalReceived[Random.Range(0, goalReceived.Length)]);
                cpuGoals++;
            }
            else
            {
                //cpu received a goal
                playSfx(goalLanded[Random.Range(0, goalLanded.Length)]);
                playerGoals++;
            }

            //activate the goal event plane
            GameObject gp = null;
            if (_side == 1)
            {
                gp = GameObject.Instantiate(goalPlane, new Vector3(15, 2, -1), Quaternion.Euler(0, 180, 0)) as
                    GameObject;
                float t = 0;
                float speed = 2.0f;
                while (t < 1)
                {
                    t += Time.deltaTime * speed;
                    gp.transform.position = new Vector3(Mathf.SmoothStep(15, 0, t), 2, -1);
                    //				yield return 0;
                    await timerComponent.WaitAsync(0);
                }

                //			yield return new WaitForSeconds(0.75f);

                await timerComponent.WaitAsync(750);
                float t2 = 0;
                while (t2 < 1)
                {
                    t2 += Time.deltaTime * speed;
                    gp.transform.position = new Vector3(Mathf.SmoothStep(0, -15, t2), 2, -1);
                    //				yield return 0;
                    await timerComponent.WaitAsync(0);
                }
            }

            //		yield return new WaitForSeconds(1.5f);
            await timerComponent.WaitAsync(1500);
            GameObject.Destroy(gp);

            isGoalTransition = false;

            //do not continue the game if the time is up
            if (gameIsFinished)
                return;

            //continue the game
            playSfx(startWistle);
            //		yield return new WaitForSeconds(startWistle.length);
            await timerComponent.WaitAsync((long)startWistle.length * 1000);
            ball.GetComponent<Rigidbody>().isKinematic = false;
            ball.GetComponent<Rigidbody>().AddForce(new Vector3(0, 1, 0), ForceMode.Impulse);
        }


        //show goals on the screen
        void manageGoals()
        {
            //		hudCpuGoals.GetComponent<TextMesh>().text = cpuGoals.ToString();
            //		hudPlayerGoals.GetComponent<TextMesh>().text = playerGoals.ToString();
            eventCenter.SendMsg(HG_WarEvent.HG_WarTScoreChange, playerGoals, cpuGoals);
        }


        //show game timer on the screen
        void manageGameTime()
        {
            remainingTime = (int)(gameTime - passTime);

            //finish the game if time is up
            if (remainingTime <= 0)
            {
                gameIsFinished = true;
                remainingTime = 0;
            }

            //		hudGameTime.GetComponent<TextMesh>().text = remainingTime.ToString();
            eventCenter.SendMsg(HG_WarEvent.HG_WarTimeChange, remainingTime);
        }


        //*****************************************************************************
        // Play sound clips
        //*****************************************************************************
        void playSfx(AudioClip _clip)
        {
            if (gameObject)
            {
                gameObject.GetComponent<AudioSource>().clip = _clip;
                if (!gameObject.GetComponent<AudioSource>().isPlaying)
                {
                    gameObject.GetComponent<AudioSource>().Play();
                }
            }
        }


        void Event_OP_Left()
        {
            if (!HG_GameWarComponent.isGoalTransition && !HG_GameWarComponent.gameIsFinished)
            {
                plCp.moveLeft();
            }
        }

        void Event_OP_Right()
        {
            if (!HG_GameWarComponent.isGoalTransition && !HG_GameWarComponent.gameIsFinished)
            {
                plCp.moveRight();
            }
        }

        void Event_OP_Jump()
        {
            if (!HG_GameWarComponent.isGoalTransition && !HG_GameWarComponent.gameIsFinished)
            {
                plCp.doJump();
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            eventCenter.RemoveMsg(HG_WarEvent.HG_OP_Left, Event_OP_Left);
            eventCenter.RemoveMsg(HG_WarEvent.HG_OP_Right, Event_OP_Right);
            eventCenter.RemoveMsg(HG_WarEvent.HG_OP_Jump, Event_OP_Jump);
            GameObject.Destroy(Counter);
            plCp.Dispose();
            cpuCp.Dispose();
            battleCp.Dispose();

            base.Dispose();
        }
    }
}