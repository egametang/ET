using System.Collections.Generic;
using System.Text;

namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public class UBBParser
	{
		public static UBBParser inst = new UBBParser();

		string _text;
		int _readPos;

		protected Dictionary<string, TagHandler> handlers;

		public int defaultImgWidth = 0;
		public int defaultImgHeight = 0;

		protected delegate string TagHandler(string tagName, bool end, string attr);

		public UBBParser()
		{
			handlers = new Dictionary<string, TagHandler>();
			handlers["url"] = onTag_URL;
			handlers["img"] = onTag_IMG;
			handlers["b"] = onTag_Simple;
			handlers["i"] = onTag_Simple;
			handlers["u"] = onTag_Simple;
			handlers["sup"] = onTag_Simple;
			handlers["sub"] = onTag_Simple;
			handlers["color"] = onTag_COLOR;
			handlers["font"] = onTag_FONT;
			handlers["size"] = onTag_SIZE;
			handlers["align"] = onTag_ALIGN;
		}

		protected string onTag_URL(string tagName, bool end, string attr)
		{
			if (!end)
			{
				if (attr != null)
					return "<a href=\"" + attr + "\" target=\"_blank\">";
				else
				{
					string href = GetTagText(false);
					return "<a href=\"" + href + "\" target=\"_blank\">";
				}
			}
			else
				return "</a>";
		}

		protected string onTag_IMG(string tagName, bool end, string attr)
		{
			if (!end)
			{
				string src = GetTagText(true);
				if (src == null || src.Length == 0)
					return null;

				if (defaultImgWidth != 0)
					return "<img src=\"" + src + "\" width=\"" + defaultImgWidth + "\" height=\"" + defaultImgHeight + "\"/>";
				else
					return "<img src=\"" + src + "\"/>";
			}
			else
				return null;
		}

		protected string onTag_Simple(string tagName, bool end, string attr)
		{
			return end ? ("</" + tagName + ">") : ("<" + tagName + ">");
		}

		protected string onTag_COLOR(string tagName, bool end, string attr)
		{
			if (!end)
				return "<font color=\"" + attr + "\">";
			else
				return "</font>";
		}

		protected string onTag_FONT(string tagName, bool end, string attr)
		{
			if (!end)
				return "<font face=\"" + attr + "\">";
			else
				return "</font>";
		}

		protected string onTag_SIZE(string tagName, bool end, string attr)
		{
			if (!end)
				return "<font size=\"" + attr + "\">";
			else
				return "</font>";
		}

		protected string onTag_ALIGN(string tagName, bool end, string attr)
		{
			if (!end)
				return "<p align=\"" + attr + "\">";
			else
				return "</p>";
		}

		protected string GetTagText(bool remove)
		{
			int pos1 = _readPos;
			int pos2;
			StringBuilder buffer = null;
			while ((pos2 = _text.IndexOf('[', pos1)) != -1)
			{
				if (buffer == null)
					buffer = new StringBuilder();

				if (_text[pos2 - 1] == '\\')
				{
					buffer.Append(_text, pos1, pos2 - pos1 - 1);
					buffer.Append('[');
					pos1 = pos2 + 1;
				}
				else
				{
					buffer.Append(_text, pos1, pos2 - pos1);
					break;
				}
			}
			if (pos2 == -1)
				return null;

			if (remove)
				_readPos = pos2;

			return buffer.ToString();
		}

		public string Parse(string text)
		{
			_text = text;
			int pos1 = 0, pos2, pos3;
			bool end;
			string tag, attr;
			string repl;
			StringBuilder buffer = null;
			TagHandler func;
			while ((pos2 = _text.IndexOf('[', pos1)) != -1)
			{
				if (buffer == null)
					buffer = new StringBuilder();

				if (pos2 > 0 && _text[pos2 - 1] == '\\')
				{
					buffer.Append(_text, pos1, pos2 - pos1 - 1);
					buffer.Append('[');
					pos1 = pos2 + 1;
					continue;
				}

				buffer.Append(_text, pos1, pos2 - pos1);
				pos1 = pos2;
				pos2 = _text.IndexOf(']', pos1);
				if (pos2 == -1)
					break;

				if (pos2 == pos1 + 1)
				{
					buffer.Append(_text, pos1, 2);
					pos1 = pos2 + 1;
					continue;
				}

				end = _text[pos1 + 1] == '/';
				pos3 = end ? pos1 + 2 : pos1 + 1;
				tag = _text.Substring(pos3, pos2 - pos3);
				_readPos = pos2 + 1;
				attr = null;
				repl = null;
				pos3 = tag.IndexOf('=');
				if (pos3 != -1)
				{
					attr = tag.Substring(pos3 + 1);
					tag = tag.Substring(0, pos3);
				}
				tag = tag.ToLower();
				if (handlers.TryGetValue(tag, out func))
				{
					repl = func(tag, end, attr);
					if (repl != null)
						buffer.Append(repl);
				}
				else
				{
					buffer.Append(_text, pos1, pos2 - pos1 + 1);
				}
				pos1 = _readPos;
			}

			if (buffer == null)
			{
				_text = null;
				return text;
			}
			else
			{
				if (pos1 < _text.Length)
					buffer.Append(_text, pos1, _text.Length - pos1);

				_text = null;
				return buffer.ToString();
			}
		}
	}
}
