using System;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class PopupMenu
	{
		protected GComponent _contentPane;
		protected GList _list;

		public PopupMenu()
		{
			Create(null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="resourceURL"></param>
		public PopupMenu(string resourceURL)
		{
			Create(resourceURL);
		}

		void Create(string resourceURL)
		{
			if (resourceURL == null)
			{
				resourceURL = UIConfig.popupMenu;
				if (resourceURL == null)
				{
					Debug.LogError("FairyGUI: UIConfig.popupMenu not defined");
					return;
				}
			}

			_contentPane = UIPackage.CreateObjectFromURL(resourceURL).asCom;
			_contentPane.onAddedToStage.Add(__addedToStage);

			_list = _contentPane.GetChild("list").asList;
			_list.RemoveChildrenToPool();

			_list.AddRelation(_contentPane, RelationType.Width);
			_list.RemoveRelation(_contentPane, RelationType.Height);
			_contentPane.AddRelation(_list, RelationType.Height);

			_list.onClickItem.Add(__clickItem);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="caption"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public GButton AddItem(string caption, EventCallback0 callback)
		{
			GButton item = _list.AddItemFromPool().asButton;
			item.title = caption;
			item.data = callback;
			item.grayed = false;
			Controller c = item.GetController("checked");
			if (c != null)
				c.selectedIndex = 0;

			return item;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="caption"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public GButton AddItem(string caption, EventCallback1 callback)
		{
			GButton item = _list.AddItemFromPool().asButton;
			item.title = caption;
			item.data = callback;
			item.grayed = false;
			Controller c = item.GetController("checked");
			if (c != null)
				c.selectedIndex = 0;

			return item;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="caption"></param>
		/// <param name="index"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public GButton AddItemAt(string caption, int index, EventCallback0 callback)
		{
			GObject obj = _list.GetFromPool(_list.defaultItem);
			_list.AddChildAt(obj, index);

			GButton item = (GButton)obj;
			item.title = caption;
			item.data = callback;
			item.grayed = false;
			Controller c = item.GetController("checked");
			if (c != null)
				c.selectedIndex = 0;

			return item;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="caption"></param>
		/// <param name="index"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public GButton AddItemAt(string caption, int index, EventCallback1 callback)
		{
			GObject obj = _list.GetFromPool(_list.defaultItem);
			_list.AddChildAt(obj, index);

			GButton item = (GButton)obj;
			item.title = caption;
			item.data = callback;
			item.grayed = false;
			Controller c = item.GetController("checked");
			if (c != null)
				c.selectedIndex = 0;

			return item;
		}

		/// <summary>
		/// 
		/// </summary>
		public void AddSeperator()
		{
			if (UIConfig.popupMenu_seperator == null)
			{
				Debug.LogError("FairyGUI: UIConfig.popupMenu_seperator not defined");
				return;
			}

			_list.AddItemFromPool(UIConfig.popupMenu_seperator);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public string GetItemName(int index)
		{
			GButton item = _list.GetChildAt(index).asButton;
			return item.name;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="caption"></param>
		public void SetItemText(string name, string caption)
		{
			GButton item = _list.GetChild(name).asButton;
			item.title = caption;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="visible"></param>
		public void SetItemVisible(string name, bool visible)
		{
			GButton item = _list.GetChild(name).asButton;
			if (item.visible != visible)
			{
				item.visible = visible;
				_list.SetBoundsChangedFlag();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="grayed"></param>
		public void SetItemGrayed(string name, bool grayed)
		{
			GButton item = _list.GetChild(name).asButton;
			item.grayed = grayed;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="checkable"></param>
		public void SetItemCheckable(string name, bool checkable)
		{
			GButton item = _list.GetChild(name).asButton;
			Controller c = item.GetController("checked");
			if (c != null)
			{
				if (checkable)
				{
					if (c.selectedIndex == 0)
						c.selectedIndex = 1;
				}
				else
					c.selectedIndex = 0;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="check"></param>
		public void SetItemChecked(string name, bool check)
		{
			GButton item = _list.GetChild(name).asButton;
			Controller c = item.GetController("checked");
			if (c != null)
				c.selectedIndex = check ? 2 : 1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool isItemChecked(string name)
		{
			GButton item = _list.GetChild(name).asButton;
			Controller c = item.GetController("checked");
			if (c != null)
				return c.selectedIndex == 2;
			else
				return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool RemoveItem(string name)
		{
			GComponent item = _list.GetChild(name).asCom;
			if (item != null)
			{
				int index = _list.GetChildIndex(item);
				_list.RemoveChildToPoolAt(index);
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// 
		/// </summary>
		public void ClearItems()
		{
			_list.RemoveChildrenToPool();
		}

		/// <summary>
		/// 
		/// </summary>
		public int itemCount
		{
			get { return _list.numChildren; }
		}

		/// <summary>
		/// 
		/// </summary>
		public GComponent contentPane
		{
			get { return _contentPane; }
		}

		/// <summary>
		/// 
		/// </summary>
		public GList list
		{
			get { return _list; }
		}

		public void Dispose()
		{
			_contentPane.Dispose();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Show()
		{
			Show(null, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		/// <param name="downward"></param>
		public void Show(GObject target, object downward)
		{
			GRoot r = target != null ? target.root : GRoot.inst;
			r.ShowPopup(this.contentPane, (target is GRoot) ? null : target, downward);
		}

		private void __clickItem(EventContext context)
		{
			GButton item = ((GObject)context.data).asButton;
			if (item == null)
				return;

			if (item.grayed)
			{
				_list.selectedIndex = -1;
				return;
			}

			Controller c = item.GetController("checked");
			if (c != null && c.selectedIndex != 0)
			{
				if (c.selectedIndex == 1)
					c.selectedIndex = 2;
				else
					c.selectedIndex = 1;
			}

			GRoot r = (GRoot)_contentPane.parent;
			r.HidePopup(this.contentPane);
			if (item.data is EventCallback0)
				((EventCallback0)item.data)();
			else if (item.data is EventCallback1)
				((EventCallback1)item.data)(context);
		}

		private void __addedToStage()
		{
			_list.selectedIndex = -1;
			_list.ResizeToFit(int.MaxValue, 10);
		}
	}
}
