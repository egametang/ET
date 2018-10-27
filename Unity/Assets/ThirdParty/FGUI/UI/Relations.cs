using System;
using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class Relations
	{
		GObject _owner;
		List<RelationItem> _items;

		public GObject handling;

		public Relations(GObject owner)
		{
			_owner = owner;
			_items = new List<RelationItem>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		/// <param name="relationType"></param>
		public void Add(GObject target, RelationType relationType)
		{
			Add(target, relationType, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		/// <param name="relationType"></param>
		/// <param name="usePercent"></param>
		public void Add(GObject target, RelationType relationType, bool usePercent)
		{
			int cnt = _items.Count;
			for (int i = 0; i < cnt; i++)
			{
				RelationItem item = _items[i];
				if (item.target == target)
				{
					item.Add(relationType, usePercent);
					return;
				}
			}
			RelationItem newItem = new RelationItem(_owner);
			newItem.target = target;
			newItem.Add(relationType, usePercent);
			_items.Add(newItem);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		/// <param name="relationType"></param>
		public void Remove(GObject target, RelationType relationType)
		{
			int cnt = _items.Count;
			int i = 0;
			while (i < cnt)
			{
				RelationItem item = _items[i];
				if (item.target == target)
				{
					item.Remove(relationType);
					if (item.isEmpty)
					{
						item.Dispose();
						_items.RemoveAt(i);
						cnt--;
						continue;
					}
					else
						i++;
				}
				i++;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool Contains(GObject target)
		{
			int cnt = _items.Count;
			for (int i = 0; i < cnt; i++)
			{
				RelationItem item = _items[i];
				if (item.target == target)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		public void ClearFor(GObject target)
		{
			int cnt = _items.Count;
			int i = 0;
			while (i < cnt)
			{
				RelationItem item = _items[i];
				if (item.target == target)
				{
					item.Dispose();
					_items.RemoveAt(i);
					cnt--;
				}
				else
					i++;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void ClearAll()
		{
			int cnt = _items.Count;
			for (int i = 0; i < cnt; i++)
			{
				RelationItem item = _items[i];
				item.Dispose();
			}
			_items.Clear();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		public void CopyFrom(Relations source)
		{
			ClearAll();

			List<RelationItem> arr = source._items;
			foreach (RelationItem ri in arr)
			{
				RelationItem item = new RelationItem(_owner);
				item.CopyFrom(ri);
				_items.Add(item);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			ClearAll();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dWidth"></param>
		/// <param name="dHeight"></param>
		/// <param name="applyPivot"></param>
		public void OnOwnerSizeChanged(float dWidth, float dHeight, bool applyPivot)
		{
			int cnt = _items.Count;
			if (cnt == 0)
				return;

			for (int i = 0; i < cnt; i++)
				_items[i].ApplyOnSelfSizeChanged(dWidth, dHeight, applyPivot);
		}

		/// <summary>
		/// 
		/// </summary>
		public bool isEmpty
		{
			get
			{
				return _items.Count == 0;
			}
		}

		public void Setup(ByteBuffer buffer, bool parentToChild)
		{
			int cnt = buffer.ReadByte();
			GObject target;
			for (int i = 0; i < cnt; i++)
			{
				int targetIndex = buffer.ReadShort();
				if (targetIndex == -1)
					target = _owner.parent;
				else if (parentToChild)
					target = ((GComponent)_owner).GetChildAt(targetIndex);
				else
					target = _owner.parent.GetChildAt(targetIndex);

				RelationItem newItem = new RelationItem(_owner);
				newItem.target = target;
				_items.Add(newItem);

				int cnt2 = buffer.ReadByte();
				for (int j = 0; j < cnt2; j++)
				{
					RelationType rt = (RelationType)buffer.ReadByte();
					bool usePercent = buffer.ReadBool();
					newItem.InternalAdd(rt, usePercent);
				}
			}
		}
	}
}
