using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
    /// <summary>
    /// 碰撞组件。添加到gameobject 上的。 hotfix 层通过向这里注入实现  碰撞检测;
    /// </summary>
    public class CollisionComponent : MonoBehaviour
    {
        private event Action<GameObject> collisionEnterCallback;

        public event Action<GameObject> CollisionEnterCallback
        {
            add
            {
                this.collisionEnterCallback += value;
            }
            remove
            {
                this.collisionEnterCallback -= value;
            }
        }
        private event Action<GameObject> collisionExitCallback;

        public event Action<GameObject> CollisionExitCallback
        {
            add
            {
                this.collisionExitCallback += value;
            }
            remove
            {
                this.collisionExitCallback -= value;
            }
        }
        private event Action<GameObject> collisionStayCallback;

        public event Action<GameObject> CollisionStayCallback
        {
            add
            {
                this.collisionStayCallback += value;
            }
            remove
            {
                this.collisionStayCallback -= value;
            }
        }

        //triger 
        private event Action<GameObject> triggerEnterCallback;

        public event Action<GameObject> TriggerEnterCallback
        {
            add
            {
                this.triggerEnterCallback += value;
            }
            remove
            {
                this.triggerEnterCallback -= value;
            }
        }
        private event Action<GameObject> triggerExitCallback;

        public event Action<GameObject> TriggerExitCallback
        {
            add
            {
                this.triggerExitCallback += value;
            }
            remove
            {
                this.triggerExitCallback -= value;
            }
        }
        private event Action<GameObject> triggerStayCallback;

        public event Action<GameObject> TriggerStayCallback
        {
            add
            {
                this.triggerStayCallback += value;
            }
            remove
            {
                this.triggerStayCallback -= value;
            }
        }


        private void Awake()
        {
        
        }

        //void OnCollisionEnter(Collision other)
        //{

        //    if (other.gameObject.tag == "Field")
        //    {

        //    }

        //}

        private void OnCollisionEnter(Collision collision)
        {
            this.collisionEnterCallback?.Invoke(collision.gameObject);
        }

        private void OnCollisionExit(Collision collision)
        {
            this.collisionExitCallback?.Invoke(collision.gameObject);
        }

        private void OnCollisionStay(Collision collision)
        {
            this.collisionStayCallback?.Invoke(collision.gameObject);
        }

        private void OnTriggerEnter(Collider collision)
        {
            this.triggerEnterCallback?.Invoke(collision.gameObject);
        }

        private void OnTriggerExit(Collider collision)
        {
            this.triggerExitCallback?.Invoke(collision.gameObject);
        }
        private void OnTriggerStay(Collider collision)
        {
            this.triggerStayCallback?.Invoke(collision.gameObject);
        }


    }
}
