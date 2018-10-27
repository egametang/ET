using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class EventListener
	{
		public EventDispatcher owner { get; private set; }

		EventBridge _bridge;
		string _type;

		public EventListener(EventDispatcher owner, string type)
		{
			this.owner = owner;
			this._type = type;
		}

		/// <summary>
		/// 
		/// </summary>
		public string type
		{
			get { return _type; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		public void AddCapture(EventCallback1 callback)
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			_bridge.AddCapture(callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		public void RemoveCapture(EventCallback1 callback)
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			_bridge.RemoveCapture(callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		public void Add(EventCallback1 callback)
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			_bridge.Add(callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		public void Remove(EventCallback1 callback)
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			_bridge.Remove(callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		public void Add(EventCallback0 callback)
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			_bridge.Add(callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		public void Remove(EventCallback0 callback)
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			_bridge.Remove(callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		public void Set(EventCallback1 callback)
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			_bridge.Clear();
			if (callback != null)
				_bridge.Add(callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		public void Set(EventCallback0 callback)
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			_bridge.Clear();
			if (callback != null)
				_bridge.Add(callback);
		}

		/// <summary>
		/// 
		/// </summary>
		public bool isEmpty
		{
			get
			{
				if (_bridge == null)
					_bridge = this.owner.TryGetEventBridge(_type);
				return _bridge == null || _bridge.isEmpty;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool isDispatching
		{
			get
			{
				if (_bridge == null)
					_bridge = this.owner.TryGetEventBridge(_type);
				return _bridge != null && _bridge._dispatching;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			_bridge.Clear();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool Call()
		{
			return owner.InternalDispatchEvent(this._type, _bridge, null, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool Call(object data)
		{
			return owner.InternalDispatchEvent(this._type, _bridge, data, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool BubbleCall(object data)
		{
			return owner.BubbleEvent(_type, data);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool BubbleCall()
		{
			return owner.BubbleEvent(_type, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool BroadcastCall(object data)
		{
			return owner.BroadcastEvent(_type, data);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool BroadcastCall()
		{
			return owner.BroadcastEvent(_type, null);
		}
	}
}
