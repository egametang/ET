using System;
using System.Collections.Generic;

namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public class XMLList
	{
		public List<XML> rawList;

		public XMLList()
		{
			rawList = new List<XML>();
		}

		public XMLList(List<XML> list)
		{
			rawList = list;
		}

		public void Add(XML xml)
		{
			rawList.Add(xml);
		}

		public void Clear()
		{
			rawList.Clear();
		}

		public int Count
		{
			get { return rawList.Count; }
		}

		public XML this[int index]
		{
			get { return rawList[index]; }
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(rawList, null);
		}

		public Enumerator GetEnumerator(string selector)
		{
			return new Enumerator(rawList, selector);
		}

		static List<XML> _tmpList = new List<XML>();
		public XMLList Filter(string selector)
		{
			bool allFit = true;
			_tmpList.Clear();
			int cnt = rawList.Count;
			for (int i = 0; i < cnt; i++)
			{
				XML xml = rawList[i];
				if (xml.name == selector)
					_tmpList.Add(xml);
				else
					allFit = false;
			}

			if (allFit)
				return this;
			else
			{
				XMLList ret = new XMLList(_tmpList);
				_tmpList = new List<XML>();
				return ret;
			}
		}

		public XML Find(string selector)
		{
			int cnt = rawList.Count;
			for (int i = 0; i < cnt; i++)
			{
				XML xml = rawList[i];
				if (xml.name == selector)
					return xml;
			}
			return null;
		}

		public struct Enumerator
		{
			List<XML> _source;
			string _selector;
			int _index;
			int _total;
			XML _current;

			public Enumerator(List<XML> source, string selector)
			{
				_source = source;
				_selector = selector;
				_index = -1;
				if (_source != null)
					_total = _source.Count;
				else
					_total = 0;
				_current = null;
			}

			public XML Current
			{
				get { return _current; }
			}

			public bool MoveNext()
			{
				while (++_index < _total)
				{
					_current = _source[_index];
					if (_selector == null || _current.name == _selector)
						return true;
				}

				return false;
			}

			public void Reset()
			{
				_index = -1;
			}
		}
	}
}
