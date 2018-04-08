using System;
using UnityEngine;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class EventCenterControllerSystem : AwakeSystem<EventCenterController>
    {
        public override void Awake(EventCenterController self)
        {
            self.Awake();
        }
    }
    /// <summary>
    /// Event center controller.
    /// 事件控制中心;
    /// </summary>
    public class EventCenterController : Component
    {
       
        public void Awake()
        {
            
        }
        public delegate void VoidDelegate();

        public delegate void VoidDataDelegate(object data);

        public delegate void VoidObjDelegate(object data, object data2);

        /// <summary>
        /// The message arr.
        /// 不带参数对回调委托;
        /// </summary>
        Dictionary<string, List<VoidDelegate>> msgArr = new Dictionary<string, List<VoidDelegate>>();

        /// <summary>
        /// The message data arr.
        /// 带参数对回调委托;
        /// </summary>
        Dictionary<string, List<VoidDataDelegate>> msgDataArr = new Dictionary<string, List<VoidDataDelegate>>();

        /// <summary>
        /// The message object arr.
        /// 不定参数回调委托;
        /// </summary>
        Dictionary<string, List<VoidObjDelegate>> msgObjArr = new Dictionary<string, List<VoidObjDelegate>>();

        #region 无参数回调

        public void AddMsg(string evtName, VoidDelegate fun)
        {
//		GameObject obj = new GameObject ();
            if (msgArr.ContainsKey(evtName))
            {
                if (!msgArr[evtName].Contains(fun))
                {
                    msgArr[evtName].Add(fun);
                }
            }
            else
            {
                msgArr[evtName] = new List<VoidDelegate>();
                msgArr[evtName].Add(fun);
            }
        }

        public void RemoveMsg(string evtName, VoidDelegate fun)
        {
            if (msgArr.ContainsKey(evtName))
            {
                if (msgArr[evtName].Contains(fun))
                {
                    msgArr[evtName].Remove(fun);
                }
            }
        }

        #endregion


        #region 不定数据回调

        public void AddMsg(string evtName, VoidObjDelegate fun)
        {
            if (msgObjArr.ContainsKey(evtName))
            {
                if (!msgObjArr[evtName].Contains(fun))
                {
                    msgObjArr[evtName].Add(fun);
                }
            }
            else
            {
                msgObjArr[evtName] = new List<VoidObjDelegate>();
                msgObjArr[evtName].Add(fun);
            }
        }

        public void RemoveMsg(string evtName, VoidObjDelegate fun)
        {
            if (msgObjArr.ContainsKey(evtName))
            {
                if (msgObjArr[evtName].Contains(fun))
                {
                    msgObjArr[evtName].Remove(fun);
                }
            }
        }

        #endregion

        #region 封装数据回调

        public void AddMsg(string evtName, VoidDataDelegate fun)
        {
            if (msgDataArr.ContainsKey(evtName))
            {
                if (!msgDataArr[evtName].Contains(fun))
                {
                    msgDataArr[evtName].Add(fun);
                }
            }
            else
            {
                msgDataArr[evtName] = new List<VoidDataDelegate>();
                msgDataArr[evtName].Add(fun);
            }
        }

        public void RemoveMsg(string evtName, VoidDataDelegate fun)
        {
            if (msgDataArr.ContainsKey(evtName))
            {
                if (msgDataArr[evtName].Contains(fun))
                {
                    msgDataArr[evtName].Remove(fun);
                }
            }
        }

        #endregion

        public void SendMsg(string evtName)
        {
            if (msgArr.ContainsKey(evtName))
            {
                List<VoidDelegate> allFuns = null;
                msgArr.TryGetValue(evtName, out allFuns);
                if (allFuns != null)
                {
//				try {
                    List<VoidDelegate> errorList = new List<VoidDelegate>();
                    List<VoidDelegate> exeList = new List<VoidDelegate>(allFuns);
                    foreach (VoidDelegate oneFun in exeList)
                    {
                        if (oneFun != null)
                        {
                            if (oneFun.Target != null && oneFun.Method != null)
                            {
                                if (oneFun.Target is MonoBehaviour)
                                {
                                    MonoBehaviour mon = oneFun.Target as MonoBehaviour;
                                    if (mon != null)
                                    {
                                        GameObject obj = mon.gameObject;
                                        if (obj != null)
                                        {
                                            obj.SendMessage(oneFun.Method.Name, null,
                                                SendMessageOptions.DontRequireReceiver);
                                        }
                                        else
                                        {
                                            errorList.Add(oneFun);
                                        }
                                    }
                                    else
                                    {
                                        errorList.Add(oneFun);
                                    }
                                }
                                else
                                {
                                    oneFun();
                                }
                            }
                            else
                            {
                                errorList.Add(oneFun);
                            }
                        }
                    }
                    foreach (VoidDelegate oneFun in errorList)
                    {
                        if (oneFun != null)
                        {
                            allFuns.Remove(oneFun);
                        }
                    }
//				} catch {
//					Debuger.LogError ("foreach (VoidDelegate)  error  " + evtName);
//				}
                }
            }
        }

        public void SendMsg(string evtName, object objData)
        {
            if (msgDataArr.ContainsKey(evtName))
            {
                List<VoidDataDelegate> allFuns = null;
                msgDataArr.TryGetValue(evtName, out allFuns);
                if (allFuns != null)
                {
//				try {
                    List<VoidDataDelegate> errorList = new List<VoidDataDelegate>();
                    List<VoidDataDelegate> exeList = new List<VoidDataDelegate>(allFuns);
                    foreach (VoidDataDelegate oneFun in exeList)
                    {
                        if (oneFun.Target != null && oneFun.Method != null)
                        {
                            if (oneFun.Target is MonoBehaviour)
                            {
                                MonoBehaviour mon = oneFun.Target as MonoBehaviour;
                                GameObject obj = mon.gameObject;
                                obj.SendMessage(oneFun.Method.Name, objData, SendMessageOptions.DontRequireReceiver);
                            }
                            else
                            {
                                oneFun(objData);
                            }
                        }
                        else
                        {
                            errorList.Add(oneFun);
                        }
                    }
                    foreach (VoidDataDelegate oneFun in errorList)
                    {
                        if (oneFun != null)
                        {
                            allFuns.Remove(oneFun);
                        }
                    }
//				} catch {
//					Debuger.LogError ("foreach (VoidDataDelegate)  error " + evtName);
//				}
                }
            }
        }

        public void SendMsg(string evtName, object objData, object objData2)
        {
            if (msgObjArr.ContainsKey(evtName))
            {
                List<VoidObjDelegate> allFuns = null;
                msgObjArr.TryGetValue(evtName, out allFuns);
                if (allFuns != null)
                {
//				try {
                    List<VoidObjDelegate> errorList = new List<VoidObjDelegate>();
                    List<VoidObjDelegate> exeList = new List<VoidObjDelegate>(allFuns);
                    foreach (VoidObjDelegate oneFun in exeList)
                    {
                        if (oneFun.Target != null && oneFun.Method != null)
                        {
//							if (oneFun.Target  is GameObject) {
//								GameObject obj = oneFun.Target as GameObject; 
//								obj.SendMessage (oneFun.Method.Name, objData, SendMessageOptions.DontRequireReceiver);
//							} else {
                            oneFun(objData, objData2);
//							}
                        }
                        else
                        {
                            errorList.Add(oneFun);
                        }
                    }
                    foreach (VoidObjDelegate oneFun in errorList)
                    {
                        if (oneFun != null)
                        {
                            allFuns.Remove(oneFun);
                        }
                    }
//				} catch {
//					Debuger.LogError ("foreach (VoidObjDelegate)  error  " + evtName);
//				}
                }
            }
        }
    }
}