using System.Collections.Generic;
using FairyGUI.Utils;
using System;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// Controller class.
	/// 控制器类。控制器的创建和设计需通过编辑器完成，不建议使用代码创建。
	/// 最常用的方法是通过selectedIndex获得或改变控制器的活动页面。如果要获得控制器页面改变的通知，使用onChanged事件。
	/// </summary>
	public class Controller : EventDispatcher
	{
		/// <summary>
		/// Name of the controller
		/// 控制器名称。
		/// </summary>
		public string name;

		/// <summary>
		/// When controller page changed.
		/// 当控制器活动页面改变时，此事件被触发。
		/// </summary>
		public EventListener onChanged { get; private set; }

		internal GComponent parent;
		internal bool autoRadioGroupDepth;
		internal bool changing;

		int _selectedIndex;
		int _previousIndex;
		List<string> _pageIds;
		List<string> _pageNames;
		List<ControllerAction> _actions;

		static uint _nextPageId;

		public Controller()
		{
			_pageIds = new List<string>();
			_pageNames = new List<string>();
			_selectedIndex = -1;
			_previousIndex = -1;

			onChanged = new EventListener(this, "onChanged");
		}

		public void Dispose()
		{
			RemoveEventListeners();
		}

		/// <summary>
		/// Current page index.
		/// 获得或设置当前活动页面索引。
		/// </summary>
		public int selectedIndex
		{
			get
			{
				return _selectedIndex;
			}
			set
			{
				if (_selectedIndex != value)
				{
					if (value > _pageIds.Count - 1)
						throw new IndexOutOfRangeException("" + value);

					changing = true;

					_previousIndex = _selectedIndex;
					_selectedIndex = value;
					parent.ApplyController(this);

					onChanged.Call();

					changing = false;
				}
			}
		}

		/// <summary>
		/// Set current page index, no onChanged event.
		/// 通过索引设置当前活动页面，和selectedIndex的区别在于，这个方法不会触发onChanged事件。
		/// </summary>
		/// <param name="value">Page index</param>
		public void SetSelectedIndex(int value)
		{
			if (_selectedIndex != value)
			{
				if (value > _pageIds.Count - 1)
					throw new IndexOutOfRangeException("" + value);

				changing = true;
				_previousIndex = _selectedIndex;
				_selectedIndex = value;
				parent.ApplyController(this);
				changing = false;
			}
		}

		/// <summary>
		/// Set current page by name, no onChanged event.
		/// 通过页面名称设置当前活动页面，和selectedPage的区别在于，这个方法不会触发onChanged事件。
		/// </summary>
		/// <param name="value">Page name</param>
		public void SetSelectedPage(string value)
		{
			int i = _pageNames.IndexOf(value);
			if (i == -1)
				i = 0;
			this.SetSelectedIndex(i);
		}

		/// <summary>
		/// Current page name.
		/// 获得当前活动页面名称
		/// </summary>
		public string selectedPage
		{
			get
			{
				if (_selectedIndex == -1)
					return null;
				else
					return _pageNames[_selectedIndex];
			}
			set
			{
				int i = _pageNames.IndexOf(value);
				if (i == -1)
					i = 0;
				this.selectedIndex = i;
			}
		}

		/// <summary>
		/// Previouse page index.
		/// 获得上次活动页面索引
		/// </summary>
		public int previsousIndex
		{
			get { return _previousIndex; }
		}

		/// <summary>
		/// Previous page name.
		/// 获得上次活动页面名称。
		/// </summary>
		public string previousPage
		{
			get
			{
				if (_previousIndex == -1)
					return null;
				else
					return _pageNames[_previousIndex];
			}
		}

		/// <summary>
		/// Page count of this controller.
		/// 获得页面数量。
		/// </summary>
		public int pageCount
		{
			get { return _pageIds.Count; }
		}

		/// <summary>
		/// Get page name by an index.
		/// 通过页面索引获得页面名称。
		/// </summary>
		/// <param name="index">Page index</param>
		/// <returns>Page Name</returns>
		public string GetPageName(int index)
		{
			return _pageNames[index];
		}

		/// <summary>
		/// Get page id by name
		/// </summary>
		/// <param name="aName"></param>
		/// <returns></returns>
		public string GetPageIdByName(string aName)
		{
			int i = _pageNames.IndexOf(aName);
			if (i != -1)
				return _pageIds[i];
			else
				return null;
		}

		/// <summary>
		/// Add a new page to this controller.
		/// </summary>
		/// <param name="name">Page name</param>
		public void AddPage(string name)
		{
			if (name == null)
				name = string.Empty;

			AddPageAt(name, _pageIds.Count);
		}

		/// <summary>
		/// Add a new page to this controller at a certain index.
		/// </summary>
		/// <param name="name">Page name</param>
		/// <param name="index">Insert position</param>
		public void AddPageAt(string name, int index)
		{
			string nid = "_" + (_nextPageId++);
			if (index == _pageIds.Count)
			{
				_pageIds.Add(nid);
				_pageNames.Add(name);
			}
			else
			{
				_pageIds.Insert(index, nid);
				_pageNames.Insert(index, name);
			}
		}

		/// <summary>
		/// Remove a page.
		/// </summary>
		/// <param name="name">Page name</param>
		public void RemovePage(string name)
		{
			int i = _pageNames.IndexOf(name);
			if (i != -1)
			{
				_pageIds.RemoveAt(i);
				_pageNames.RemoveAt(i);
				if (_selectedIndex >= _pageIds.Count)
					this.selectedIndex = _selectedIndex - 1;
				else
					parent.ApplyController(this);
			}
		}

		/// <summary>
		/// Removes a page at a certain index.
		/// </summary>
		/// <param name="index"></param>
		public void RemovePageAt(int index)
		{
			_pageIds.RemoveAt(index);
			_pageNames.RemoveAt(index);
			if (_selectedIndex >= _pageIds.Count)
				this.selectedIndex = _selectedIndex - 1;
			else
				parent.ApplyController(this);
		}

		/// <summary>
		/// Remove all pages.
		/// </summary>
		public void ClearPages()
		{
			_pageIds.Clear();
			_pageNames.Clear();
			if (_selectedIndex != -1)
				this.selectedIndex = -1;
			else
				parent.ApplyController(this);
		}

		/// <summary>
		/// Check if the controller has a page.
		/// </summary>
		/// <param name="aName">Page name.</param>
		/// <returns></returns>
		public bool HasPage(string aName)
		{
			return _pageNames.IndexOf(aName) != -1;
		}

		internal int GetPageIndexById(string aId)
		{
			return _pageIds.IndexOf(aId);
		}

		internal string GetPageNameById(string aId)
		{
			int i = _pageIds.IndexOf(aId);
			if (i != -1)
				return _pageNames[i];
			else
				return null;
		}

		internal string GetPageId(int index)
		{
			return _pageIds[index];
		}

		internal string selectedPageId
		{
			get
			{
				if (_selectedIndex == -1)
					return null;
				else
					return _pageIds[_selectedIndex];
			}
			set
			{
				int i = _pageIds.IndexOf(value);
				if (i != -1)
					this.selectedIndex = i;
			}
		}

		internal string oppositePageId
		{
			set
			{
				int i = _pageIds.IndexOf(value);
				if (i > 0)
					this.selectedIndex = 0;
				else if (_pageIds.Count > 1)
					this.selectedIndex = 1;
			}
		}

		internal string previousPageId
		{
			get
			{
				if (_previousIndex == -1)
					return null;
				else
					return _pageIds[_previousIndex];
			}
		}

		public void RunActions()
		{
			if (_actions != null)
			{
				int cnt = _actions.Count;
				for (int i = 0; i < cnt; i++)
				{
					_actions[i].Run(this, previousPageId, selectedPageId);
				}
			}
		}

		public void Setup(ByteBuffer buffer)
		{
			int beginPos = buffer.position;
			buffer.Seek(beginPos, 0);

			name = buffer.ReadS();
			autoRadioGroupDepth = buffer.ReadBool();

			buffer.Seek(beginPos, 1);

			int cnt = buffer.ReadShort();
			_pageIds.Capacity = cnt;
			_pageNames.Capacity = cnt;
			for (int i = 0; i < cnt; i++)
			{
				_pageIds.Add(buffer.ReadS());
				_pageNames.Add(buffer.ReadS());
			}

			buffer.Seek(beginPos, 2);

			cnt = buffer.ReadShort();
			if (cnt > 0)
			{
				if (_actions == null)
					_actions = new List<ControllerAction>(cnt);

				for (int i = 0; i < cnt; i++)
				{
					int nextPos = buffer.ReadShort();
					nextPos += buffer.position;

					ControllerAction action = ControllerAction.CreateAction((ControllerAction.ActionType)buffer.ReadByte());
					action.Setup(buffer);
					_actions.Add(action);

					buffer.position = nextPos;
				}
			}

			if (parent != null && _pageIds.Count > 0)
				_selectedIndex = 0;
			else
				_selectedIndex = -1;
		}
	}
}
