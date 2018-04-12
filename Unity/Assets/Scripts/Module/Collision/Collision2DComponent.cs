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
    public class Collision2DComponent : MonoBehaviour
    {
        private event Action<GameObject> collisionEnter2DCallback;

        public event Action<GameObject> CollisionEnter2DCallback
        {
            add
            {
                this.collisionEnter2DCallback += value;
            }
            remove
            {
                this.collisionEnter2DCallback -= value;
            }
        }
        private event Action<GameObject> collisionExit2DCallback;

        public event Action<GameObject> CollisionExit2DCallback
        {
            add
            {
                this.collisionExit2DCallback += value;
            }
            remove
            {
                this.collisionExit2DCallback -= value;
            }
        }
        private event Action<GameObject> collisionStay2DCallback;

        public event Action<GameObject> CollisionStay2DCallback
        {
            add
            {
                this.collisionStay2DCallback += value;
            }
            remove
            {
                this.collisionStay2DCallback -= value;
            }
        }

        //triger 
        private event Action<GameObject> triggerEnter2DCallback;

        public event Action<GameObject>  TriggerEnter2DCallback
        {
            add
            {
                this.triggerEnter2DCallback += value;
            }
            remove
            {
                this.triggerEnter2DCallback -= value;
            }
        }
        private event Action<GameObject> triggerExit2DCallback;

        public event Action<GameObject> TriggerExit2DCallback
        {
            add
            {
                this.triggerExit2DCallback += value;
            }
            remove
            {
                this.triggerExit2DCallback -= value;
            }
        }
        private event Action<GameObject> triggerStay2DCallback;

        public event Action<GameObject> TriggerStay2DCallback
        {
            add
            {
                this.triggerStay2DCallback += value;
            }
            remove
            {
                this.triggerStay2DCallback -= value;
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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            this.collisionEnter2DCallback?.Invoke(collision.gameObject);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            this.collisionExit2DCallback?.Invoke(collision.gameObject);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            this.collisionStay2DCallback?.Invoke(collision.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            this.triggerEnter2DCallback?.Invoke(collision.gameObject);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            this.triggerExit2DCallback?.Invoke(collision.gameObject);
        }
        private void OnTriggerStay2D(Collider2D collision)
        {
            this.triggerStay2DCallback?.Invoke(collision.gameObject);
        }


    }
}
