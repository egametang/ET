using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class TouchScreenKeyboard : IKeyboard
	{
		UnityEngine.TouchScreenKeyboard _keyboard;

		public bool done
		{
#if UNITY_2017_2_OR_NEWER
			get { return _keyboard == null || _keyboard.status == UnityEngine.TouchScreenKeyboard.Status.Done || _keyboard.status == UnityEngine.TouchScreenKeyboard.Status.Canceled; }
#else
			get { return _keyboard == null || _keyboard.done; }
#endif
		}

		public bool supportsCaret
		{
			get { return false; }
		}

		public string GetInput()
		{
			if (_keyboard != null)
			{
				string s = _keyboard.text;

#if UNITY_2017_2_OR_NEWER
				if (_keyboard.status == UnityEngine.TouchScreenKeyboard.Status.Done || _keyboard.status == UnityEngine.TouchScreenKeyboard.Status.Canceled)
					_keyboard = null;
#else
				if (_keyboard.done)
					_keyboard = null;
#endif

				return s;
			}
			else
				return null;
		}

		public void Open(string text, bool autocorrection, bool multiline, bool secure, bool alert, string textPlaceholder, int keyboardType, bool hideInput)
		{
			if (_keyboard != null)
				return;

			UnityEngine.TouchScreenKeyboard.hideInput = hideInput;
			_keyboard = UnityEngine.TouchScreenKeyboard.Open(text, (TouchScreenKeyboardType)keyboardType, autocorrection, multiline, secure, alert, textPlaceholder);
		}

		public void Close()
		{
			if (_keyboard != null)
			{
				_keyboard.active = false;
				_keyboard = null;
			}
		}
	}
}
