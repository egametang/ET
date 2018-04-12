using System;
using System.Net;
using ETModel;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Threading.Tasks;

namespace ETHotfix
{
    [ObjectSystem]
    public class HG_PlayerControllerCpAwakeSystem : AwakeSystem<HG_PlayerControllerCp>
    {
        public override void Awake(HG_PlayerControllerCp self)
        {
            self.Awake();
        }
    }
    [ObjectSystem]
    public class HG_PlayerControllerCpUpdataSystem : UpdateSystem<HG_PlayerControllerCp>
    {
        public override void Update(HG_PlayerControllerCp self)
        {
            self.Update();
        }
    }



    public class HG_PlayerControllerCp : Component
    {
        //public Texture2D[] availableAvatars;			//different avatar images for player character
         StatusImage[] availableAvatars; //different avatar images for player character

        //starting position of player (slider)
        [Range(-7.0f, -3.0f)] public float startingX = -5.0f;

        private Vector3 startingPosition;

        //reference to child game objects
        public GameObject myShoe;

        public GameObject myHead;
        private float maxShoeTurn = 20.0f; //in degrees

        public bool canTurnHead = true; //flag
        public bool canTurnShoe = true; //flag

        private float moveSpeed = 5.0f; //player movement speed
        private float jumpSpeed = 300.0f; //player jump power (avoid setting it higher than 400)
        private bool canJump = true;

        private GameObject gameObject;
        private Transform transform;
        private TimerComponent timerComponent;

        private StatusImage curPerson;
        private ResourcesComponent resourcesComponent;
        
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
            
            //reset player's position
            startingPosition = new Vector3(startingX, 0.5f, 0);
            transform.position = startingPosition;

            curPerson = GetCurRoleInfo(PlayerPrefs.GetInt("selectedAvatar", 0));
            //change player avatar image based on the selection made from the Menu scene
            myHead.GetComponent<Renderer>().material.mainTexture =
                curPerson .normal;
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

        public void Update()
        {
            rotateShoe();
            CheckMovePl();
            //swing the head to the default rotation when player is not moving or jumping
            if (!Input.anyKey && canTurnHead)
            {
                JointSpring js = myHead.GetComponent<HingeJoint>().spring;
                js.targetPosition = 0;
                myHead.GetComponent<HingeJoint>().spring = js;
            }
        }
    

        /// <summary>
        /// Here we have 2 functions to move the player with keyboard keys, mouse clicks or touch events.
        /// </summary>
//        public void FixedUpdate()
//        {
//            if (!HG_GameWarComponent.isGoalTransition && !HG_GameWarComponent.gameIsFinished)
//            {
//                movePlayerWithKeyboard();
//                 movePlayerWithTouch();
//            }
//        }


        /// <summary>
        /// Swing the head of the player avatar
        /// </summary>
        void rotateHead(int _dir)
        {
            if (!canTurnHead)
                return;

            if (myHead)
            {
                //swing the head by setting a new rotation for the spring
                JointSpring js = myHead.GetComponent<HingeJoint>().spring;
                js.targetPosition = 15 * _dir;
                myHead.GetComponent<HingeJoint>().spring = js;
            }
        }


        /// <summary>
        /// Rotate player shows when we are moving him around.
        /// reset rotation when player is not moving.
        /// </summary>
        void rotateShoe()
        {
            if (!myShoe || !canTurnShoe)
                return;

            float rotateMinVelocity = 0.1f;

            if (gameObject.GetComponent<Rigidbody>().velocity.x >= rotateMinVelocity)
            {
                myShoe.transform.localEulerAngles = new Vector3(0, maxShoeTurn, 180);
            }
            else if (gameObject.GetComponent<Rigidbody>().velocity.x <= rotateMinVelocity * -1)
            {
                myShoe.transform.localEulerAngles = new Vector3(0, maxShoeTurn * -1, 180);
            }
            else
            {
                myShoe.transform.localEulerAngles = new Vector3(0, 0, 180);
            }
        }


//        /// <summary>
//        /// Move player with keyboard keys.
//        /// </summary>
//        void movePlayerWithKeyboard()
//        {
//            if (Input.GetKey(KeyCode.LeftArrow))
//            {
//                moveLeft();
//            }
//
//            if (Input.GetKey(KeyCode.RightArrow))
//            {
//                moveRight();
//            }
//
//            if (Input.GetKeyDown(KeyCode.Space) && canJump)
//            {
//                doJump();
//            }
//        }


        /// <summary>
        /// We use a simple multitouch (detects 2 touches at once) routine to monitor 
        /// user interaction with UI buttons.
        /// </summary>
        private RaycastHit hitInfo;

        private RaycastHit hitInfo2;
        private Ray ray;
        private Ray ray2;

//        void movePlayerWithTouch()
//        {
//
//            if (Input.touches.Length > 0)
//            {
//                ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
//                if (Input.touches.Length > 1)
//                    ray2 = Camera.main.ScreenPointToRay(Input.touches[1].position);
//                else
//                    ray2 = Camera.main.ScreenPointToRay(Input.touches[0].position);
//            }
//            else if (Input.GetMouseButton(0))
//                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
////            else
////                yield break;
//
//            if (Physics.Raycast(ray, out hitInfo))
//            {
//                GameObject objectHit = hitInfo.transform.gameObject;
//                switch (objectHit.name)
//                {
//                    case "Button-Jump":
//                        if (canJump)
//                            doJump();
//                        break;
//                    case "Button-Left":
//                        moveLeft();
//                        break;
//                    case "Button-Right":
//                        moveRight();
//                        break;
//                }
//            }
//
//            if (Physics.Raycast(ray2, out hitInfo2))
//            {
//                GameObject objectHit2 = hitInfo2.transform.gameObject;
//                switch (objectHit2.name)
//                {
//                    case "Button-Jump":
//                        if (canJump)
//                            doJump();
//                        break;
//                    case "Button-Left":
//                        moveLeft();
//                        break;
//                    case "Button-Right":
//                        moveRight();
//                        break;
//                }
//            }
//            
//        }


        /// <summary>
        /// Change status image for avatar face
        /// </summary>
        public async void  changeFaceStatus()
        {
            myHead.GetComponent<Renderer>().material.mainTexture =
                curPerson.hit;
//            yield return new WaitForSeconds(0.3f);
            await timerComponent.WaitAsync(300);
            myHead.GetComponent<Renderer>().material.mainTexture =
                curPerson.normal;
        }
        bool? isGoRight = null;
        void CheckMovePl()
        {
            if(isGoRight == false)
            {
                gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(moveSpeed * -1, 0, 0), ForceMode.Acceleration);
            }
            else if(isGoRight == true)
            {
                gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(moveSpeed, 0, 0), ForceMode.Acceleration);
            }
        }
        /// <summary>
        /// nul 就是停下来 
        /// </summary>
        /// <param name="isRight"></param>
        public  void moveLeft(bool isLeft)
        {
            if (isLeft)
            {
                isGoRight = false;
                rotateHead(-1);
            }
            else
            {
                isGoRight = null;
            }
            //isGoRight = (isLeft)? false : null;
            //print ("Left arrow pressed!");
            //gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(moveSpeed * -1, 0, 0), ForceMode.Acceleration);
   
        }


        public void moveRight(bool isRight)
        {
            if (isRight)
            {
                isGoRight = true;
                rotateHead(1);
            }
            else
            {
                isGoRight = null;
            }
            //print ("Right arrow pressed!");
            //gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(moveSpeed, 0, 0), ForceMode.Acceleration);
 
        }


        public void doJump()
        {
            if (canJump)
            {
                //print ("Space pressed!");
                canJump = false;
                Vector3 jumpPower = new Vector3(0, jumpSpeed, 0);
                gameObject.GetComponent<Rigidbody>().AddForce(jumpPower, ForceMode.Impulse);
            }

        }


        /// <summary>
        /// take the object to its initial position
        /// </summary>
        public void resetPosition()
        {
            transform.position = startingPosition;
            gameObject.GetComponent<Rigidbody>().sleepThreshold = 0.005f;
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }


        /// <summary>
        /// Shoot the ball upon collision
        /// </summary>
        void OnCollisionEnter(Collision other)
        {
            Vector3 bounceDir = other.gameObject.transform.position - gameObject.transform.position;
            Vector3 shootForce;
            bounceDir.Normalize();
            shootForce = Vector3.Scale(bounceDir, new Vector3(0.15f, 0.15f, 0.15f));
            other.gameObject.GetComponent<Rigidbody>().AddForce(shootForce, ForceMode.Impulse);

            if (other.gameObject.tag == "Field")
                canJump = true;

            //print (other.gameObject.name + " - Time: " + Time.time + " - Force: " + shootForce);
        }
    }
}