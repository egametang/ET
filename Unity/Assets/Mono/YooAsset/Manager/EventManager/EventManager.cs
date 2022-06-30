using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 事件管理器
/// </summary>
public static class EventManager
{
	private class PostWrapper
	{
		public int PostFrame;
		public int EventID;
		public IEventMessage Message;

		public void OnRelease()
		{
			PostFrame = 0;
			EventID = 0;
			Message = null;
		}
	}

	private static readonly Dictionary<int, List<Action<IEventMessage>>> _listeners = new Dictionary<int, List<Action<IEventMessage>>>(1000);
	private static readonly List<PostWrapper> _postWrappers = new List<PostWrapper>(1000);


	public static void Update()
	{
		for (int i = _postWrappers.Count - 1; i >= 0; i--)
		{
			var wrapper = _postWrappers[i];
			if (UnityEngine.Time.frameCount > wrapper.PostFrame)
			{
				SendMessage(wrapper.EventID, wrapper.Message);
				_postWrappers.RemoveAt(i);
			}
		}
	}


	/// <summary>
	/// 添加监听
	/// </summary>
	public static void AddListener<TEvent>(System.Action<IEventMessage> listener) where TEvent : IEventMessage
	{
		AddListener(typeof(TEvent), listener);
	}

	/// <summary>
	/// 添加监听
	/// </summary>
	public static void AddListener(System.Type eventType, System.Action<IEventMessage> listener)
	{
		int eventId = eventType.GetHashCode();
		AddListener(eventId, listener);
	}

	/// <summary>
	/// 添加监听
	/// </summary>
	public static void AddListener(int eventId, System.Action<IEventMessage> listener)
	{
		if (_listeners.ContainsKey(eventId) == false)
			_listeners.Add(eventId, new List<Action<IEventMessage>>());
		if (_listeners[eventId].Contains(listener) == false)
			_listeners[eventId].Add(listener);
	}


	/// <summary>
	/// 移除监听
	/// </summary>
	public static void RemoveListener<TEvent>(System.Action<IEventMessage> listener) where TEvent : IEventMessage
	{
		RemoveListener(typeof(TEvent), listener);
	}

	/// <summary>
	/// 移除监听
	/// </summary>
	public static void RemoveListener(System.Type eventType, System.Action<IEventMessage> listener)
	{
		int eventId = eventType.GetHashCode();
		RemoveListener(eventId, listener);
	}

	/// <summary>
	/// 移除监听
	/// </summary>
	public static void RemoveListener(int eventId, System.Action<IEventMessage> listener)
	{
		if (_listeners.ContainsKey(eventId))
		{
			if (_listeners[eventId].Contains(listener))
				_listeners[eventId].Remove(listener);
		}
	}


	/// <summary>
	/// 实时广播事件
	/// </summary>
	public static void SendMessage(IEventMessage message)
	{
		int eventId = message.GetType().GetHashCode();
		SendMessage(eventId, message);
	}

	/// <summary>
	/// 实时广播事件
	/// </summary>
	public static void SendMessage(int eventId, IEventMessage message)
	{
		if (_listeners.ContainsKey(eventId) == false)
			return;

		List<Action<IEventMessage>> listeners = _listeners[eventId];
		for (int i = listeners.Count - 1; i >= 0; i--)
		{
			listeners[i].Invoke(message);
		}
	}

	/// <summary>
	/// 延迟广播事件
	/// </summary>
	public static void PostMessage(IEventMessage message)
	{
		int eventId = message.GetType().GetHashCode();
		PostMessage(eventId, message);
	}

	/// <summary>
	/// 延迟广播事件
	/// </summary>
	public static void PostMessage(int eventId, IEventMessage message)
	{
		var wrapper = new PostWrapper();
		wrapper.PostFrame = UnityEngine.Time.frameCount;
		wrapper.EventID = eventId;
		wrapper.Message = message;
		_postWrappers.Add(wrapper);
	}


	/// <summary>
	/// 清空所有监听
	/// </summary>
	public static void ClearListeners()
	{
		foreach (int eventId in _listeners.Keys)
		{
			_listeners[eventId].Clear();
		}
		_listeners.Clear();
	}

	/// <summary>
	/// 获取监听者总数
	/// </summary>
	private static int GetAllListenerCount()
	{
		int count = 0;
		foreach (var list in _listeners)
		{
			count += list.Value.Count;
		}
		return count;
	}
}