using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FairyGUI.Utils
{
	public enum XMLTagType
	{
		Start,
		End,
		Void,
		CDATA,
		Comment,
		Instruction
	}

	/// <summary>
	/// 
	/// </summary>
	public class XMLIterator
	{
		public static string tagName;
		public static XMLTagType tagType;
		public static string lastTagName;

		static string source;
		static int sourceLen;
		static int parsePos;
		static int tagPos;
		static int tagLength;
		static int lastTagEnd;
		static bool attrParsed;
		static bool lowerCaseName;
		static StringBuilder buffer = new StringBuilder();
		static Dictionary<string, string> attributes = new Dictionary<string, string>();

		const string CDATA_START = "<![CDATA[";
		const string CDATA_END = "]]>";
		const string COMMENT_START = "<!--";
		const string COMMENT_END = "-->";

		public static void Begin(string source, bool lowerCaseName = false)
		{
			XMLIterator.source = source;
			XMLIterator.lowerCaseName = lowerCaseName;
			sourceLen = source.Length;
			parsePos = 0;
			lastTagEnd = 0;
			tagPos = 0;
			tagLength = 0;
			tagName = null;
		}

		public static bool NextTag()
		{
			int pos;
			char c;
			tagType = XMLTagType.Start;
			buffer.Length = 0;
			lastTagEnd = parsePos;
			attrParsed = false;
			lastTagName = tagName;

			while ((pos = source.IndexOf('<', parsePos)) != -1)
			{
				parsePos = pos;
				pos++;

				if (pos == sourceLen)
					break;

				c = source[pos];
				if (c == '!')
				{
					if (sourceLen > pos + 7 && source.Substring(pos - 1, 9) == CDATA_START)
					{
						pos = source.IndexOf(CDATA_END, pos);
						tagType = XMLTagType.CDATA;
						tagName = string.Empty;
						tagPos = parsePos;
						if (pos == -1)
							tagLength = sourceLen - parsePos;
						else
							tagLength = pos + 3 - parsePos;
						parsePos += tagLength;
						return true;
					}
					else if (sourceLen > pos + 2 && source.Substring(pos - 1, 4) == COMMENT_START)
					{
						pos = source.IndexOf(COMMENT_END, pos);
						tagType = XMLTagType.Comment;
						tagName = string.Empty;
						tagPos = parsePos;
						if (pos == -1)
							tagLength = sourceLen - parsePos;
						else
							tagLength = pos + 3 - parsePos;
						parsePos += tagLength;
						return true;
					}
					else
					{
						pos++;
						tagType = XMLTagType.Instruction;
					}
				}
				else if (c == '/')
				{
					pos++;
					tagType = XMLTagType.End;
				}
				else if (c == '?')
				{
					pos++;
					tagType = XMLTagType.Instruction;
				}

				for (; pos < sourceLen; pos++)
				{
					c = source[pos];
					if (Char.IsWhiteSpace(c) || c == '>' || c == '/')
						break;
				}
				if (pos == sourceLen)
					break;

				buffer.Append(source, parsePos + 1, pos - parsePos - 1);
				if (buffer.Length > 0 && buffer[0] == '/')
					buffer.Remove(0, 1);

				bool singleQuoted = false, doubleQuoted = false;
				int possibleEnd = -1;
				for (; pos < sourceLen; pos++)
				{
					c = source[pos];
					if (c == '"')
					{
						if (!singleQuoted)
							doubleQuoted = !doubleQuoted;
					}
					else if (c == '\'')
					{
						if (!doubleQuoted)
							singleQuoted = !singleQuoted;
					}

					if (c == '>')
					{
						if (!(singleQuoted || doubleQuoted))
						{
							possibleEnd = -1;
							break;
						}

						possibleEnd = pos;
					}
					else if (c == '<')
						break;
				}
				if (possibleEnd != -1)
					pos = possibleEnd;

				if (pos == sourceLen)
					break;

				if (source[pos - 1] == '/')
					tagType = XMLTagType.Void;

				tagName = buffer.ToString();
				if (lowerCaseName)
					tagName = tagName.ToLower();
				tagPos = parsePos;
				tagLength = pos + 1 - parsePos;
				parsePos += tagLength;

				return true;
			}

			tagPos = sourceLen;
			tagLength = 0;
			tagName = null;
			return false;
		}

		public static string GetTagSource()
		{
			return source.Substring(tagPos, tagLength);
		}

		public static string GetRawText(bool trim = false)
		{
			if (lastTagEnd == tagPos)
				return string.Empty;
			else if (trim)
			{
				int i = lastTagEnd;
				for (; i < tagPos; i++)
				{
					char c = source[i];
					if (!char.IsWhiteSpace(c))
						break;
				}

				if (i == tagPos)
					return string.Empty;
				else
					return source.Substring(i, tagPos - i).TrimEnd();
			}
			else
				return source.Substring(lastTagEnd, tagPos - lastTagEnd);
		}

		public static string GetText(bool trim = false)
		{
			if (lastTagEnd == tagPos)
				return string.Empty;
			else if (trim)
			{
				int i = lastTagEnd;
				for (; i < tagPos; i++)
				{
					char c = source[i];
					if (!char.IsWhiteSpace(c))
						break;
				}

				if (i == tagPos)
					return string.Empty;
				else
					return XMLUtils.DecodeString(source.Substring(i, tagPos - i).TrimEnd());
			}
			else
				return XMLUtils.DecodeString(source.Substring(lastTagEnd, tagPos - lastTagEnd));
		}

		public static bool HasAttribute(string attrName)
		{
			if (!attrParsed)
			{
				attributes.Clear();
				ParseAttributes(attributes);
				attrParsed = true;
			}

			return attributes.ContainsKey(attrName);
		}

		public static string GetAttribute(string attrName)
		{
			if (!attrParsed)
			{
				attributes.Clear();
				ParseAttributes(attributes);
				attrParsed = true;
			}

			string value;
			if (attributes.TryGetValue(attrName, out value))
				return value;
			else
				return null;
		}

		public static string GetAttribute(string attrName, string defValue)
		{
			string ret = GetAttribute(attrName);
			if (ret != null)
				return ret;
			else
				return defValue;
		}

		public static int GetAttributeInt(string attrName)
		{
			return GetAttributeInt(attrName, 0);
		}

		public static int GetAttributeInt(string attrName, int defValue)
		{
			string value = GetAttribute(attrName);
			if (value == null || value.Length == 0)
				return defValue;

			int ret;
			if (int.TryParse(value, out ret))
				return ret;
			else
				return defValue;
		}

		public static float GetAttributeFloat(string attrName)
		{
			return GetAttributeFloat(attrName, 0);
		}

		public static float GetAttributeFloat(string attrName, float defValue)
		{
			string value = GetAttribute(attrName);
			if (value == null || value.Length == 0)
				return defValue;

			float ret;
			if (float.TryParse(value, out ret))
				return ret;
			else
				return defValue;
		}

		public static bool GetAttributeBool(string attrName)
		{
			return GetAttributeBool(attrName, false);
		}

		public static bool GetAttributeBool(string attrName, bool defValue)
		{
			string value = GetAttribute(attrName);
			if (value == null || value.Length == 0)
				return defValue;

			bool ret;
			if (bool.TryParse(value, out ret))
				return ret;
			else
				return defValue;
		}

		public static Dictionary<string, string> GetAttributes(Dictionary<string, string> result)
		{
			if (result == null)
				result = new Dictionary<string, string>();

			if (attrParsed)
			{
				foreach (KeyValuePair<string, string> kv in attributes)
					result[kv.Key] = kv.Value;
			}
			else //这里没有先ParseAttributes再赋值给result是为了节省复制的操作
				ParseAttributes(result);

			return result;
		}

		public static Hashtable GetAttributes(Hashtable result)
		{
			if (result == null)
				result = new Hashtable();

			if (attrParsed)
			{
				foreach (KeyValuePair<string, string> kv in attributes)
					result[kv.Key] = kv.Value;
			}
			else //这里没有先ParseAttributes再赋值给result是为了节省复制的操作
				ParseAttributes(result);

			return result;
		}

		static void ParseAttributes(IDictionary attrs)
		{
			string attrName;
			int valueStart;
			int valueEnd;
			bool waitValue = false;
			int quoted;
			buffer.Length = 0;
			int i = tagPos;
			int attrEnd = tagPos + tagLength;

			if (i < attrEnd && source[i] == '<')
			{
				for (; i < attrEnd; i++)
				{
					char c = source[i];
					if (Char.IsWhiteSpace(c) || c == '>' || c == '/')
						break;
				}
			}

			for (; i < attrEnd; i++)
			{
				char c = source[i];
				if (c == '=')
				{
					valueStart = -1;
					valueEnd = -1;
					quoted = 0;
					for (int j = i + 1; j < attrEnd; j++)
					{
						char c2 = source[j];
						if (Char.IsWhiteSpace(c2))
						{
							if (valueStart != -1 && quoted == 0)
							{
								valueEnd = j - 1;
								break;
							}
						}
						else if (c2 == '>')
						{
							if (quoted == 0)
							{
								valueEnd = j - 1;
								break;
							}
						}
						else if (c2 == '"')
						{
							if (valueStart != -1)
							{
								if (quoted != 1)
								{
									valueEnd = j - 1;
									break;
								}
							}
							else
							{
								quoted = 2;
								valueStart = j + 1;
							}
						}
						else if (c2 == '\'')
						{
							if (valueStart != -1)
							{
								if (quoted != 2)
								{
									valueEnd = j - 1;
									break;
								}
							}
							else
							{
								quoted = 1;
								valueStart = j + 1;
							}
						}
						else if (valueStart == -1)
						{
							valueStart = j;
						}
					}

					if (valueStart != -1 && valueEnd != -1)
					{
						attrName = buffer.ToString();
						if (lowerCaseName)
							attrName = attrName.ToLower();
						buffer.Length = 0;
						attrs[attrName] = XMLUtils.DecodeString(source.Substring(valueStart, valueEnd - valueStart + 1));
						i = valueEnd + 1;
					}
					else
						break;
				}
				else if (!Char.IsWhiteSpace(c))
				{
					if (waitValue || c == '/' || c == '>')
					{
						if (buffer.Length > 0)
						{
							attrName = buffer.ToString();
							if (lowerCaseName)
								attrName = attrName.ToLower();
							attrs[attrName] = string.Empty;
							buffer.Length = 0;
						}

						waitValue = false;
					}

					if (c != '/' && c != '>')
						buffer.Append(c);
				}
				else
				{
					if (buffer.Length > 0)
						waitValue = true;
				}
			}
		}
	}
}
