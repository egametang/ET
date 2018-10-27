using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("FairyGUI/UI Content Scaler")]
	public class UIContentScaler : MonoBehaviour
	{
		/// <summary>
		/// 
		/// </summary>
		public enum ScaleMode
		{
			ConstantPixelSize,
			ScaleWithScreenSize,
			ConstantPhysicalSize
		}

		/// <summary>
		/// 
		/// </summary>
		public enum ScreenMatchMode
		{
			MatchWidthOrHeight,
			MatchWidth,
			MatchHeight
		}

		/// <summary>
		/// 
		/// </summary>
		public ScaleMode scaleMode;

		/// <summary>
		/// 
		/// </summary>
		public ScreenMatchMode screenMatchMode;

		/// <summary>
		/// 
		/// </summary>
		public int designResolutionX;

		/// <summary>
		/// 
		/// </summary>
		public int designResolutionY;

		/// <summary>
		/// 
		/// </summary>
		public int fallbackScreenDPI = 96;

		/// <summary>
		/// 
		/// </summary>
		public int defaultSpriteDPI = 96;

		/// <summary>
		/// 
		/// </summary>
		public float constantScaleFactor = 1;

		/// <summary>
		/// 当false时，计算比例时会考虑designResolutionX/Y的设置是针对横屏还是竖屏。否则不考虑。
		/// </summary>
		public bool ignoreOrientation = false;

		[System.NonSerialized]
		public static float scaleFactor = 1;

		[System.NonSerialized]
		bool _changed;

		void OnEnable()
		{
			if (Application.isPlaying)
			{
				//播放模式下都是通过Stage自带的UIContentScaler实现调整的，所以这里只是把参数传过去
				UIContentScaler scaler = Stage.inst.gameObject.GetComponent<UIContentScaler>();
				if (scaler != this)
				{
					scaler.scaleMode = scaleMode;
					if (scaleMode == ScaleMode.ScaleWithScreenSize)
					{
						scaler.designResolutionX = designResolutionX;
						scaler.designResolutionY = designResolutionY;
						scaler.screenMatchMode = screenMatchMode;
						scaler.ignoreOrientation = ignoreOrientation;
					}
					else if (scaleMode == ScaleMode.ConstantPhysicalSize)
					{
						scaler.fallbackScreenDPI = fallbackScreenDPI;
						scaler.defaultSpriteDPI = defaultSpriteDPI;
					}
					else
					{
						scaler.constantScaleFactor = constantScaleFactor;
					}
					scaler.ApplyChange();
					GRoot.inst.ApplyContentScaleFactor();
				}
			}
			else //Screen width/height is not reliable in OnEnable in editmode
				_changed = true;
		}

		void Update()
		{
			if (_changed)
			{
				_changed = false;
				ApplyChange();
			}
		}

		void OnDestroy()
		{
			if (!Application.isPlaying)
				scaleFactor = 1;
		}

		//For UIContentScalerEditor Only, as the Screen.width/height is not correct in OnInspectorGUI
		/// <summary>
		/// 
		/// </summary>
		public void ApplyModifiedProperties()
		{
			_changed = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void ApplyChange()
		{
			if (scaleMode == ScaleMode.ScaleWithScreenSize)
			{
				if (designResolutionX == 0 || designResolutionY == 0)
					return;

				int dx = designResolutionX;
				int dy = designResolutionY;
				if (!ignoreOrientation && (Screen.width > Screen.height && dx < dy || Screen.width < Screen.height && dx > dy))
				{
					//scale should not change when orientation change
					int tmp = dx;
					dx = dy;
					dy = tmp;
				}

				if (screenMatchMode == ScreenMatchMode.MatchWidthOrHeight)
				{
					float s1 = (float)Screen.width / dx;
					float s2 = (float)Screen.height / dy;
					scaleFactor = Mathf.Min(s1, s2);
				}
				else if (screenMatchMode == ScreenMatchMode.MatchWidth)
					scaleFactor = (float)Screen.width / dx;
				else
					scaleFactor = (float)Screen.height / dy;
			}
			else if (scaleMode == ScaleMode.ConstantPhysicalSize)
			{
				float dpi = Screen.dpi;
				if (dpi == 0)
					dpi = fallbackScreenDPI;
				if (dpi == 0)
					dpi = 96;
				scaleFactor = dpi / (defaultSpriteDPI == 0 ? 96 : defaultSpriteDPI);
			}
			else
				scaleFactor = constantScaleFactor;

			if (scaleFactor > 10)
				scaleFactor = 10;

			StageCamera.screenSizeVer++;
		}
	}
}
