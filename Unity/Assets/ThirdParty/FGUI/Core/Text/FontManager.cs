using System.Collections.Generic;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class FontManager
	{
		static Dictionary<string, BaseFont> sFontFactory = new Dictionary<string, BaseFont>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="font"></param>
		/// <param name="alias"></param>
		static public void RegisterFont(BaseFont font, string alias)
		{
			if (!sFontFactory.ContainsKey(font.name))
				sFontFactory.Add(font.name, font);
			if (alias != null)
			{
				if (!sFontFactory.ContainsKey(alias))
					sFontFactory.Add(alias, font);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="font"></param>
		static public void UnregisterFont(BaseFont font)
		{
			sFontFactory.Remove(font.name);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		static public BaseFont GetFont(string name)
		{
			BaseFont ret;
			if (name.StartsWith(UIPackage.URL_PREFIX))
			{
				ret = UIPackage.GetItemAssetByURL(name) as BaseFont;
				if (ret != null)
					return ret;
			}

			if (!sFontFactory.TryGetValue(name, out ret))
			{
				ret = new DynamicFont(name);
				sFontFactory.Add(name, ret);
			}

			return ret;
		}

		/// <summary>
		/// 
		/// </summary>
		static public void Clear()
		{
			sFontFactory.Clear();
		}
	}
}
