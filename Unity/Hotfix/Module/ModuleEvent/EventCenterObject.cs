using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHotfix
{
    /// <summary>
    /// 一种事件消息的接收对象;
    /// </summary>
   public class EventCenterObject
    {
        //如果Action后面 可以做Action<T>的话就好了。 
        public event Action MsgCallback;
        public event Action<object> MsgP1Callback;
        public event Action<object, object> MsgP2Callback;

        public void SendMsg()
        {
            MsgCallback?.Invoke();
        }
        public void SendMsg<T>(T obj)
        {
            MsgP1Callback?.Invoke(obj);
        }
        public void SendMsg<T,K>(T obj,K obj1)
        {
            MsgP2Callback?.Invoke(obj, obj1);
        }
    }
}
