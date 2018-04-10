
using System.Net;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class HG_CpuControllerCpAwakeSystem : AwakeSystem<HG_CpuControllerCp>
    {
        public override void Awake(HG_CpuControllerCp self)
        {
            self.Awake();
        }
    }
    [ObjectSystem]
    public class HG_CpuControllerCpFixUpdateSystem : LateUpdateSystem<HG_CpuControllerCp>
    {
        public override void LateUpdate(HG_CpuControllerCp self)
        {
            self.FixedUpdate();
        }
    }


    public class HG_CpuControllerCp : Component
    {
        //A dropdown option to select the AI level
        //the different AI levels uses different parameters for the speed and accuracy of the cpu.
        public enum cpuLevels
        {
            easy,
            normal,
            hard
        }

        public cpuLevels cpuLevel = cpuLevels.easy;

        public int selectedCpuAvatar = 0
            ; //cpu avatar can be picked from available textures [in this demo, it can be 0 to 3]

         StatusImage[] availableAvatars; //different avatar images 

        //reference to child game objects
         GameObject myHead;

         GameObject myShoe;


        private JointSpring js;
        public bool canTurnHead = true;
        public bool canTurnShoe = true;
        private float maxShoeTurn = 20.0f; //in degrees

        //starting position of cpu
        [Range(3.0f, 7.0f)] public float startingX = 5.0f;

        public Vector3 startingPosition = new Vector3(5,0.5f,0);
        private bool canMove = true;

        //private AI parameters
        private float moveSpeed;

        private float jumpSpeed;
        private float minJumpDistance;
        private bool canJump = true;
        private bool preventDoubleJump = true;
        private long jumpDelay;
        private float accuracy;
        private float adjustingPosition;

        private Vector2 cpuFieldLimits = new Vector2(0.1f, 8.5f);
        private GameObject ball;

        public void SetBall(GameObject _ball)
        {
            ball = _ball;
        }
        
        private GameObject gameObject;
        private Transform transform;
        private TimerComponent timerComponent;
        private ResourcesComponent resourcesComponent;

        private StatusImage curPerson;
        
        public void Awake()
        {
            gameObject = this.GetParent<UI>().GameObject;
            transform = gameObject.transform;
            ReferenceCollector rc = gameObject.GetComponent<ReferenceCollector>();
            myHead =  rc.Get<GameObject>("HeadImage");
            myShoe =  rc.Get<GameObject>("ShoeImage");
            
            timerComponent = Game.Scene.ModelScene.GetComponent<TimerComponent>();
            
            //===资源类
             resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
      
            
            startingPosition = new Vector3(startingX, 0.5f, 0);
            //find and cache ball object
            //ball = GameObject.FindGameObjectWithTag("Ball");
            resetPosition();
            setCpuLevel();

            curPerson = GetCurRoleInfo(selectedCpuAvatar);
            //set cpu avatar
            myHead.GetComponent<Renderer>().material.mainTexture = curPerson.normal;
	
        }

        StatusImage GetCurRoleInfo(int index)
        {
            index = index + 1;
            Texture2D  onePlNormal = (Texture2D)resourcesComponent.GetAsset($"{UIType.HG_Res}.unity3d", $"Player-Head-0{index}-n");
            Texture2D  onePlHit = (Texture2D)resourcesComponent.GetAsset($"{UIType.HG_Res}.unity3d", $"Player-Head-0{index}-c");
                
            StatusImage onePerson = new StatusImage();
            onePerson.hit = onePlHit;
            onePerson.normal = onePlNormal;
            return onePerson;
        }

        //change the AI configuration based on the enum selection
        void setCpuLevel()
        {
            switch (cpuLevel)
            {
                case cpuLevels.easy:
                    moveSpeed = 13.0f;
                    jumpSpeed = 250.0f;
                    minJumpDistance = 3.0f;
                    accuracy = 0.5f;
//                    jumpDelay = 0.5f;
                    jumpDelay = 500;
                    break;

                case cpuLevels.normal:
                    moveSpeed = 16.0f;
                    jumpSpeed = 275.0f;
                    minJumpDistance = 3.4f;
                    accuracy = 0.4f;
//                    jumpDelay = 0.2f;
                    jumpDelay = 200;
                    break;

                case cpuLevels.hard:
                    moveSpeed = 19.0f;
                    jumpSpeed = 300.0f;
                    minJumpDistance = 3.75f;
                    accuracy = 0.35f;
//                    jumpDelay = 0.1f;
                    jumpDelay = 100;
                    break;
            }
        }


        public void FixedUpdate()
        {
            if(this.IsDisposed)
            {
                Log.Warning(" cpu  update when dispised ");
            }
     
            if (!this.IsDisposed && canMove && !HG_GameWarComponent.isGoalTransition && !HG_GameWarComponent.gameIsFinished)
                playSoccer();
        }


        /// <summary>
        /// Main cpu play routines
        /// </summary>
        void playSoccer()
        {
            if (ball.transform.position.x < cpuFieldLimits.x)
            {
                //freePlay();

                //swing the head to the default rotation when not playing
                if (canTurnHead)
                {
                    js = myHead.GetComponent<HingeJoint>().spring;
                    js.targetPosition = 0;
                    myHead.GetComponent<HingeJoint>().spring = js;
                }

                //reset shoe rotation when not playing
                if (canTurnShoe)
                    myShoe.transform.localEulerAngles = new Vector3(0, 0, 180);
            }
            else if (ball.transform.position.x > cpuFieldLimits.x && ball.transform.position.x < cpuFieldLimits.y)
            {
                //move the cpu towards the ball
                transform.position = new Vector3(
                    Mathf.SmoothStep(transform.position.x, ball.transform.position.x + adjustingPosition,
                        Time.deltaTime * moveSpeed),
                    transform.position.y,
                    transform.position.z);

                if (transform.position.x <= ball.transform.position.x)
                {
                    //swing cpu's head
                    rotateHead(-1);

                    //rotate cpu's shoe
                    if (canTurnShoe)
                        myShoe.transform.localEulerAngles = new Vector3(0, maxShoeTurn, 180);
                }
                else
                {
                    rotateHead(1);
                    if (canTurnShoe)
                        myShoe.transform.localEulerAngles = new Vector3(0, maxShoeTurn * -1, 180);
                }


                //if cpu is close enough to the ball, make it jump
                if (Vector3.Distance(transform.position, ball.transform.position) < minJumpDistance && canJump)
                {
                    canJump = false;
                    Vector3 jumpPower = new Vector3(0, jumpSpeed - Random.Range(0, 50), 0);
                    gameObject. GetComponent<Rigidbody>().AddForce(jumpPower, ForceMode.Impulse);
                }
            }
        }


        /// <summary>
        /// Swing the head of the cpu avatar
        /// </summary>
        void rotateHead(int _dir)
        {
            if (!canTurnHead)
                return;

            if (myHead)
            {
                js = myHead.GetComponent<HingeJoint>().spring;
                js.targetPosition = 15 * _dir;
                myHead.GetComponent<HingeJoint>().spring = js;
            }
        }


        //take the object to its initial position
        public async void resetPosition()
        {
            canMove = false;
            transform.position = startingPosition;
            gameObject.GetComponent<Rigidbody>().sleepThreshold = 0.005f;
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            adjustingPosition = Random.Range(0.1f, accuracy);

//            yield return new WaitForSeconds(0.75f);
            await timerComponent.WaitAsync(750);
            canMove = true;
        }


        //Optional silly moves by cpu when ball is not in his field
        void freePlay()
        {
        }


        /// <summary>
        /// Change status image for avatar face
        /// </summary>
        public async void changeFaceStatus()
        {
            myHead.GetComponent<Renderer>().material.mainTexture = curPerson.hit;
//            yield return new WaitForSeconds(0.3f);
            await timerComponent.WaitAsync(300);
            myHead.GetComponent<Renderer>().material.mainTexture = curPerson.normal;
        }


        /// <summary>
        /// Shoot the ball upon collision
        /// </summary>
        void OnCollisionEnter(Collision other)
        {
            Vector3 bounceDir = other.gameObject.transform.position - gameObject.transform.position;
            Vector3 shootForce;
            bounceDir.Normalize();
            shootForce = bounceDir * 0.5f;
            other.gameObject.GetComponent<Rigidbody>().AddForce(shootForce, ForceMode.Impulse);

            if (other.gameObject.tag == "Field" && preventDoubleJump)
                jumpActivation();
//                StartCoroutine(jumpActivation());
        }


        /// <summary>
        /// enable jump ability again
        /// </summary>
        async void jumpActivation()
        {
            preventDoubleJump = false;
//            yield return new WaitForSeconds(jumpDelay);
            await timerComponent.WaitAsync(jumpDelay);
            //print ("Cpu jump activated at: " + Time.timeSinceLevelLoad);
            canJump = true;
            preventDoubleJump = true;
        }
    }
}