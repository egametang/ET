using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace I2.Loc
{
	public class LocalizationReader 
	{
		#region Dictionary Assets

		public static Dictionary<string,string> ReadTextAsset( TextAsset asset )
		{
			string Text = Encoding.UTF8.GetString (asset.bytes, 0, asset.bytes.Length);
			Text = Text.Replace("\r\n", "\n");
			Text = Text.Replace("\r", "\n");
			StringReader reader = new StringReader(Text);
			
			string s;
            Dictionary<string, string> Dict = new Dictionary<string, string>(StringComparer.Ordinal);
			while ( (s=reader.ReadLine()) != null )
			{
				string Key, Value, Category, TermType, Comment;
				if (!TextAsset_ReadLine(s, out Key, out Value, out Category, out Comment, out TermType))
					continue;
				
				if (!string.IsNullOrEmpty(Key) && !string.IsNullOrEmpty(Value))
					Dict[Key]=Value;
			}
			return Dict;
		}

		public static bool TextAsset_ReadLine( string line, out string key, out string value, out string category, out string comment, out string termType )
		{
			key		= string.Empty;
			category= string.Empty;
			comment = string.Empty;
			termType= string.Empty;
			value = string.Empty;

			//--[ Comment ]-----------------------
			int iComment = line.LastIndexOf("//", StringComparison.Ordinal);
			if (iComment>=0)
			{
				comment = line.Substring(iComment+2).Trim();
				comment = DecodeString(comment);
				line = line.Substring(0, iComment);
			}

			//--[ Key ]-----------------------------
			int iKeyEnd = line.IndexOf("=", StringComparison.Ordinal);
			if (iKeyEnd<0)
			{
				return false;
			}

			key = line.Substring(0, iKeyEnd).Trim();
			value = line.Substring(iKeyEnd+1).Trim();
			value = value.Replace ("\r\n", "\n").Replace ("\n", "\\n");
			value = DecodeString(value);

			//--[ Type ]---------
			if (key.Length>2 && key[0]=='[')
			{
				int iTypeEnd = key.IndexOf(']');
				if (iTypeEnd>=0)
				{
					termType = key.Substring(1, iTypeEnd-1);
					key = key.Substring(iTypeEnd+1);
				}
			}
			
			ValidateFullTerm( ref key );

			return true;
		}

		#endregion

		#region CSV
		public static string ReadCSVfile( string Path, Encoding encoding )
		{
			string Text = string.Empty;
			#if (UNITY_WP8 || UNITY_METRO) && !UNITY_EDITOR
				byte[] buffer = UnityEngine.Windows.File.ReadAllBytes (Path);
				Text = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
			#else
				/*using (System.IO.StreamReader reader = System.IO.File.OpenText(Path))
				{
					Text = reader.ReadToEnd();
				}*/
				using (var reader = new StreamReader(Path, encoding ))
					Text = reader.ReadToEnd();
			#endif

			Text = Text.Replace("\r\n", "\n");
			Text = Text.Replace("\r", "\n");

			return Text;
		}

		public static List<string[]> ReadCSV( string Text, char Separator=',' )
		{
			int iStart = 0;
			List<string[]> CSV = new List<string[]>();

			while (iStart < Text.Length)
			{
				string[] list = ParseCSVline (Text, ref iStart, Separator);
				if (list==null) break;
				CSV.Add(list);
			}
			return CSV;
		}

		static string[] ParseCSVline( string Line, ref int iStart, char Separator )
		{
			List<string> list = new List<string>();
			
			//Line = "puig,\"placeres,\"\"cab\nr\nera\"\"algo\"\npuig";//\"Frank\npuig\nplaceres\",aaa,frank\nplaceres";

			int TextLength = Line.Length;
			int iWordStart = iStart;
			bool InsideQuote = false;

			while (iStart < TextLength)
			{
				char c = Line[iStart];

				if (InsideQuote)
				{
					if (c=='\"') //--[ Look for Quote End ]------------
					{
						if (iStart+1 >= TextLength || Line[iStart+1] != '\"')  //-- Single Quote:  Quotation Ends
						{
							InsideQuote = false;
						}
						else
						if (iStart+2 < TextLength && Line[iStart+2]=='\"')  //-- Tripple Quotes: Quotation ends
						{
							InsideQuote = false;
							iStart+=2;
						}
						else 
							iStart++;  // Skip Double Quotes
					}
				}

				else  //-----[ Separators ]----------------------

				if (c=='\n' || c==Separator)
				{
					AddCSVtoken(ref list, ref Line, iStart, ref iWordStart);
					if (c=='\n')  // Stop the row on line breaks
					{
						iStart++;
						break;
					}
				}

				else //--------[ Start Quote ]--------------------

				if (c=='\"')
					InsideQuote = true;

				iStart++;
			}
			if (iStart>iWordStart)
				AddCSVtoken(ref list, ref Line, iStart, ref iWordStart);

			return list.ToArray();
		}

		static void AddCSVtoken( ref List<string> list, ref string Line, int iEnd, ref int iWordStart)
		{
			string Text = Line.Substring(iWordStart, iEnd-iWordStart);
			iWordStart = iEnd+1;

			Text = Text.Replace("\"\"", "\"" );
			if (Text.Length>1 && Text[0]=='\"' && Text[Text.Length-1]=='\"')
				Text = Text.Substring(1, Text.Length-2 );

			list.Add( Text);
		}

		

		#endregion

		#region I2CSV

		public static List<string[]> ReadI2CSV( string Text )
		{
			string[] ColumnSeparator = {"[*]"};
			string[] RowSeparator = {"[ln]"};

			List<string[]> CSV = new List<string[]>();
			foreach (var line in Text.Split (RowSeparator, StringSplitOptions.None))
				CSV.Add (line.Split (ColumnSeparator, StringSplitOptions.None));

			return CSV;
		}

		#endregion

		#region Misc

		public static void ValidateFullTerm( ref string Term )
		{
			Term = Term.Replace('\\', '/');
			int First = Term.IndexOf('/');
			if (First<0)
				return;
			
			int second;
			while ( (second=Term.LastIndexOf('/')) != First )
				Term = Term.Remove( second,1);
		}

		
		// this function encodes \r\n and \n into \\n
		public static string EncodeString( string str )
		{
			if (string.IsNullOrEmpty(str))
				return string.Empty;

			return str.Replace("\r\n", "<\\n>")
				.Replace("\r", "<\\n>")
					.Replace("\n", "<\\n>");
		}
		
		public static string DecodeString( string str )
		{
			if (string.IsNullOrEmpty(str))
				return string.Empty;
			
			return str.Replace("<\\n>", "\r\n");
		}

		#endregion
	}
}