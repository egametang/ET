using UnityEngine;

namespace FairyGUI
{
#if UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR
	/// <summary>
	/// 当使用DLL形式的插件时，因为DLL默认是为移动平台编译的，所以不支持复制粘贴。
	/// 将这个脚本放到工程里，并在游戏启动时调用CopyPastePatch.Apply()，可以在PC平台激活复制粘贴功能
	/// </summary>
	public class CopyPastePatch
	{
		/// <summary>
		/// 
		/// </summary>
		public static void Apply()
		{
			InputTextField.onCopy = OnCopy;
			InputTextField.onPaste = OnPaste;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="textField"></param>
		/// <param name="value"></param>
		public static void OnCopy(InputTextField textField, string value)
		{
			TextEditor te = new TextEditor();
#if UNITY_5_3_OR_NEWER
			te.text = value;
#else
			te.content = new GUIContent(value);
#endif
			te.OnFocus();
			te.Copy();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="textField"></param>
		public static void OnPaste(InputTextField textField)
		{
			TextEditor te = new TextEditor();
			te.Paste();
#if UNITY_5_3_OR_NEWER
			string value = te.text;
#else
			string value = te.content.text;
#endif
			if (!string.IsNullOrEmpty(value))
				textField.ReplaceSelection(value);
		}
	}
#endif
}
