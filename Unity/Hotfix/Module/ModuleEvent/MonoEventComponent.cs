using ETModel;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class MonoEventComponentAwakeSystem: AwakeSystem<MonoEventComponent>
    {
        public override void Awake(MonoEventComponent self)
        {
            self.Awake();
        }
    }

    public class MonoEventComponent: Component
    {
        public static MonoEventComponent Instance;

        public void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        public void AddButtonClick(Button obj, Action action)
        {
            AddButtonClick(obj, true, action);
        }

        /// <summary> /// 添加ButtonClick事件 /// </summary>
        /// <param name="obj">Button组件</param>
        /// <param name="IsRemoveAllListeners">是否清除以前注册的事件</param>
        /// <param name="action">回调函数</param>
        public void AddButtonClick(Button obj, bool IsRemoveAllListeners, Action action)
        {
            if (IsRemoveAllListeners) obj.onClick.RemoveAllListeners();
            obj.onClick.Add(action);
        }

        /// <summary> /// 添加触发事件 /// </summary>
        /// <param name="obj">GameObject</param>
        /// <param name="eventTriggerType">EventTriggerType</param>
        /// <param name="action">回调函数</param>
        /// <returns></returns>
        public EventTrigger AddEventTrigger(GameObject obj, EventTriggerType eventTriggerType, Action<BaseEventData> action)
        {
            return AddEventTrigger(obj, eventTriggerType, true, action);
        }

        /// <summary> /// 添加触发事件 /// </summary>
        /// <param name="obj">GameObject</param>
        /// <param name="eventTriggerType">EventTriggerType</param>
        /// <param name="IsRemoveAllListeners">添加事件是否清空以前添加的事件</param>
        /// <param name="action">回调函数</param>
        /// <returns></returns>
        public EventTrigger AddEventTrigger(
                GameObject obj, EventTriggerType eventTriggerType, bool IsRemoveAllListeners, Action<BaseEventData> action)
        {
            return AddEventTrigger(obj, eventTriggerType, IsRemoveAllListeners, false, action);
        }

        /// <summary> /// 添加触发事件 /// </summary>
        /// <param name="obj">GameObject</param>
        /// <param name="eventTriggerType">EventTriggerType</param>
        /// <param name="IsRemoveAllListeners">添加事件是否清空以前添加的事件</param>
        /// <param name="IsClearTriggers">是否清除GameObject上所有触发事件</param>
        /// <param name="action">回调函数</param>
        /// <returns></returns>
        public EventTrigger AddEventTrigger(
                GameObject obj, EventTriggerType eventTriggerType, bool IsRemoveAllListeners, bool IsClearTriggers, Action<BaseEventData> action)
        {
            if (obj == null) return null;
            EventTrigger eventTrigger = obj.GetComponent<EventTrigger>() ?? obj.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventTriggerType };
            UnityAction<BaseEventData> callback = new UnityAction<BaseEventData>(action);
            entry.callback.AddListener(callback);
            if (IsClearTriggers) eventTrigger.triggers.Clear();
            eventTrigger.triggers.Add(entry);
            return eventTrigger;
        }
    }
}