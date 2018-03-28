using System.Net;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class HG_BallControllerCpAwakeSystem : AwakeSystem<HG_BallControllerCp, HG_GameWarComponent,
        HG_PlayerControllerCp, HG_CpuControllerCp>
    {
        public override void Awake(HG_BallControllerCp self, HG_GameWarComponent A, HG_PlayerControllerCp B,
            HG_CpuControllerCp C)
        {
            self.Awake(A, B, C);
        }
    }

    public class HG_BallControllerCp : Component
    {
        private HG_GameWarComponent gc; //Reference to GameController Object
        private HG_PlayerControllerCp player; //Reference to Player Object
        private HG_CpuControllerCp cpu; //Reference to Cpu Object

        private float ballMaxSpeed = 10.0f; //Ball should not move faster than this value

        public AudioClip ballHitGround; //Audio when ball hits floor
        public AudioClip ballHitMiddlePole; //Audio when ball hits the pole
        public AudioClip[] ballHitHead; //Audio when ball hits the heads

        public Vector3[] ballStartingPosition; //initial position of the ball


        public GameObject ballSpeedDebug; //debug object to show ball's speed at all times
        public GameObject hitEffect; //visual effect for contact points
        public GameObject ballShadow; //shadow object that follows the ball


        private GameObject gameObject;
        private Transform transform;
        private TimerComponent timerComponent;
        private ResourcesComponent resourcesComponent;

        public void Awake(HG_GameWarComponent A, HG_PlayerControllerCp B, HG_CpuControllerCp C)
        {
            ballStartingPosition = new[] {new Vector3(-3, 5, 0), new Vector3(3, 5, 0)};
            
            gameObject = this.GetParent<UI>().GameObject;
            transform = gameObject.transform;
            ReferenceCollector rc = gameObject.GetComponent<ReferenceCollector>();

            timerComponent = Game.Scene.ModelScene.GetComponent<TimerComponent>();

//            player			 = GameObject.FindGameObjectWithTag("PlayerHead");
//            cpu				 = GameObject.FindGameObjectWithTag("CpuHead");
//            gc				 = GameObject.FindGameObjectWithTag("GameController");
            gc = A;
            player = B;
            cpu = C;
            ballHitGround = rc.Get<AudioClip>("Ball-Hit-Ground");
            ballHitMiddlePole = rc.Get<AudioClip>("ballHitsMiddlePole");
            //===资源类
            resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
            ballSpeedDebug =  rc.Get<GameObject>("Ball-Speed");
            ballShadow =  rc.Get<GameObject>("BallShadow");
            hitEffect = (GameObject)resourcesComponent.GetAsset("hdgame_hiteffect.unity3d", "HitEffect");
            transform.position = ballStartingPosition[0];
            
            AudioClip hitsd1 = (AudioClip)resourcesComponent.GetAsset($"{UIType. HG_Sound}.unity3d", "BallHit-01");
            AudioClip hitsd2 = (AudioClip)resourcesComponent.GetAsset($"{UIType. HG_Sound}.unity3d", "BallHit-02");
            AudioClip hitsd3 = (AudioClip)resourcesComponent.GetAsset($"{UIType. HG_Sound}.unity3d", "BallHit-03");
            ballHitHead = new[] {hitsd1, hitsd2, hitsd3};

        }


        public void FixedUpdate()
        {
            //never allow the ball to get stuck at the two ends of the screen.
            escapeLimits();

            //move ball's shadow object
            manageBallShadow();

            //debug - show ball's speed
            if (ballSpeedDebug)
                ballSpeedDebug.GetComponent<TextMesh>().text =
                    "Speed: " + gameObject.GetComponent<Rigidbody>().velocity.magnitude.ToString();

            //limit ball's maximum spped
            if (gameObject.GetComponent<Rigidbody>().velocity.magnitude > ballMaxSpeed)
                gameObject.GetComponent<Rigidbody>().velocity =
                    gameObject.GetComponent<Rigidbody>().velocity.normalized * ballMaxSpeed;
        }


        /// <summary>
        /// Make shadow object follow ball's movements
        /// </summary>
        void manageBallShadow()
        {
            if (!ballShadow)
                return;

            ballShadow.transform.position = new Vector3(transform.position.x, -1, 0);
            ballShadow.transform.localScale = new Vector3(1.5f, 0.75f, 0.001f);
        }


        /// <summary>
        /// never allow the ball to get stuck at the two ends of the screen.
        /// we do this by applying a small force to move the ball in another direction.
        /// Screen boundaries are hardcoded and must be updated to match your own design.
        /// </summary>
        void escapeLimits()
        {
            if (transform.position.x <= -8.4f)
                gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(3, 0, 0), ForceMode.Impulse);
            if (transform.position.x >= 8.4f)
                gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(-3, 0, 0), ForceMode.Impulse);
        }


        /// <summary>
        /// Manages collition events
        /// </summary>
        void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.tag == "Field")
            {
                playSfx(ballHitGround);
                createHitGfx();
                checkGoal();
            }

            if (other.gameObject.tag == "MiddlePole")
            {
                playSfx(ballHitMiddlePole);
                //if the ball is going to get stuck on the pole, move it by applying a small force
                if (transform.position.x <= 0.12f || transform.position.x >= -0.12f)
                    gameObject.GetComponent<Rigidbody>()
                        .AddForce(new Vector3(Random.Range(-1.0f, 1.0f), 0, 0), ForceMode.Impulse);
            }

            if (other.gameObject.tag == "PlayerHead")
            {
                playSfx(ballHitHead[Random.Range(0, ballHitHead.Length)]);
                other.gameObject.GetComponent<PlayerController>().changeFaceStatus();
                createHitGfx();
            }

            if (other.gameObject.tag == "CpuHead")
            {
                playSfx(ballHitHead[Random.Range(0, ballHitHead.Length)]);
                other.gameObject.GetComponent<CpuController>().changeFaceStatus();
                createHitGfx();
            }
        }


        /// <summary>
        /// Creates a small visual object to show the contact point between ball and other objects
        /// </summary>
        void createHitGfx()
        {
            GameObject hitGfx = GameObject.Instantiate(hitEffect,
                transform.position + new Vector3(0, -0.4f, -1),
                Quaternion.Euler(0, 180, 0)) as GameObject;
            hitGfx.name = "hitGfx";
        }


        /// <summary>
        /// Check if a goal happened.
        /// </summary>
        void checkGoal()
        {
            //it the time is up, this goal is not accepted.
            if (HG_GameWarComponent.gameIsFinished)
                return;

            if (transform.position.x < 0)
            {
                //ball has landed in the left field. we received a goal :(
                resetBallPosition(0);
                gc.manageGoalEvent(0);
            }
            else if (transform.position.x > 0)
            {
                //ball has landed in the right field. we scored a goal :)
                resetBallPosition(1);
                gc.manageGoalEvent(1);
            }

            player.resetPosition(); //reset player position
            cpu.resetPosition(); //reset cpu position
        }


        /// <summary>
        /// Move the ball to the starting position after a goal happened
        /// </summary>
        public async void resetBallPosition(int _posIndex)
        {
            //give the ball to the player or the cpu if they have received a goal
            if (_posIndex == 1)
            {
                transform.position = ballStartingPosition[1];
            }
            else
            {
                transform.position = ballStartingPosition[0];
            }

            //freeze the ball for a while
            gameObject.GetComponent<Rigidbody>().Sleep();
            gameObject.GetComponent<Rigidbody>().isKinematic = true;

//		yield return new WaitForSeconds(1.0f);

            await timerComponent.WaitAsync(1000);
            //unfreeze the ball
            //GetComponent<Rigidbody>().isKinematic = false;
        }


        //*****************************************************************************
        // Play sound clips
        //*****************************************************************************
        void playSfx(AudioClip _clip)
        {
            gameObject.GetComponent<AudioSource>().clip = _clip;
            if (!gameObject.GetComponent<AudioSource>().isPlaying)
            {
                gameObject.GetComponent<AudioSource>().Play();
            }
        }
    }
}