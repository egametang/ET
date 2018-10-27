using UnityEngine;
using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// GObjectPool is use for GObject pooling.
	/// </summary>
	public class GObjectPool
	{
		/// <summary>
		/// Callback function when a new object is creating.
		/// </summary>
		/// <param name="obj"></param>
		public delegate void InitCallbackDelegate(GObject obj);

		/// <summary>
		/// Callback function when a new object is creating.
		/// </summary>
		public InitCallbackDelegate initCallback;

		Dictionary<string, Queue<GObject>> _pool;
		Transform _manager;

		/// <summary>
		/// 需要设置一个manager，加入池里的对象都成为这个manager的孩子
		/// </summary>
		/// <param name="manager"></param>
		public GObjectPool(Transform manager)
		{
			_manager = manager;
			_pool = new Dictionary<string, Queue<GObject>>();
		}

		/// <summary>
		/// Dispose all objects in the pool.
		/// </summary>
		public void Clear()
		{
			foreach (KeyValuePair<string, Queue<GObject>> kv in _pool)
			{
				Queue<GObject> list = kv.Value;
				foreach (GObject obj in list)
					obj.Dispose();
			}
			_pool.Clear();
		}

		/// <summary>
		/// 
		/// </summary>
		public int count
		{
			get { return _pool.Count; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public GObject GetObject(string url)
		{
			url = UIPackage.NormalizeURL(url);
			if (url == null)
				return null;

			Queue<GObject> arr;
			if (_pool.TryGetValue(url, out arr)
				&& arr.Count > 0)
				return arr.Dequeue();

			GObject obj = UIPackage.CreateObjectFromURL(url);
			if (obj != null)
			{
				if (initCallback != null)
					initCallback(obj);
			}

			return obj;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		public void ReturnObject(GObject obj)
		{
			string url = obj.resourceURL;
			Queue<GObject> arr;
			if (!_pool.TryGetValue(url, out arr))
			{
				arr = new Queue<GObject>();
				_pool.Add(url, arr);
			}

			ToolSet.SetParent(obj.displayObject.cachedTransform, _manager);
			arr.Enqueue(obj);
		}
	}
}
