using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class ShaderConfig
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public delegate Shader GetFunction(string name);

		/// <summary>
		/// 
		/// </summary>
		public static GetFunction Get = Shader.Find;

		/// <summary>
		/// 
		/// </summary>
		public static string imageShader = "FairyGUI/Image";

		/// <summary>
		/// 
		/// </summary>
		public static string textShader = "FairyGUI/Text";

		/// <summary>
		/// 
		/// </summary>
		public static string textBrighterShader = "FairyGUI/Text Brighter";

		/// <summary>
		/// 
		/// </summary>
		public static string bmFontShader = "FairyGUI/BMFont";

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Shader GetShader(string name)
		{
			Shader shader = Get(name);
			if (shader == null)
			{
				Debug.LogWarning("FairyGUI: shader not found: " + name);
				shader = Shader.Find("UI/Default");
			}
			shader.hideFlags = DisplayOptions.hideFlags;
			return shader;
		}
	}
}
