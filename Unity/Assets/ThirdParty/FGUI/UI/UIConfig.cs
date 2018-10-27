using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// Global configs. These options should be set before any UI construction.
	/// </summary>
	[AddComponentMenu("FairyGUI/UI Config")]
	public class UIConfig : MonoBehaviour
	{
		/// <summary>
		/// Dynamic Font Support. 
		/// 4.x: Put the xxx.ttf into /Resources or /Resources/Fonts, and set defaultFont="xxx".
		/// 5.x: set defaultFont to system font name(or names joint with comma). e.g. defaultFont="Microsoft YaHei, SimHei"
		/// </summary>
		public static string defaultFont = "";

		/// <summary>
		/// When using chinese fonts on desktop, I found that the display effect is not very clear. So I wrote shaders to light up their outline.
		/// If you dont use chinese fonts, or dont like the new effect, just set to false here.
		/// The switch is meaningless on mobile platforms.
		/// </summary>
		public static bool renderingTextBrighterOnDesktop = true;

		/// <summary>
		/// Resource using in Window.ShowModalWait for locking the window.
		/// </summary>
		public static string windowModalWaiting;

		/// <summary>
		/// Resource using in GRoot.ShowModalWait for locking the screen.
		/// </summary>
		public static string globalModalWaiting;

		/// <summary>
		/// When a modal window is in front, the background becomes dark.
		/// </summary>
		public static Color modalLayerColor = new Color(0f, 0f, 0f, 0.4f);

		/// <summary>
		/// Default button click sound.
		/// </summary>
		public static NAudioClip buttonSound;

		/// <summary>
		/// Default button click sound volume.
		/// </summary>
		public static float buttonSoundVolumeScale = 1f;

		/// <summary>
		/// Resource url of horizontal scrollbar
		/// </summary>
		public static string horizontalScrollBar;

		/// <summary>
		/// Resource url of vertical scrollbar
		/// </summary>
		public static string verticalScrollBar;

		/// <summary>
		/// Scrolling step in pixels
		/// 当调用ScrollPane.scrollUp/Down/Left/Right时，或者点击滚动条的上下箭头时，滑动的距离。
		/// 鼠标滚轮触发一次滚动的距离设定为defaultScrollStep*2
		/// </summary>
		public static float defaultScrollStep = 25;
		[Obsolete("UIConfig.defaultScrollSpeed is deprecated. Use defaultScrollStep instead.")]
		public static float defaultScrollSpeed = 25;

		/// <summary>
		/// Deceleration ratio of scrollpane when its in touch dragging.
		/// 当手指拖动并释放滚动区域后，内容会滑动一定距离后停下，这个速率就是减速的速率。
		/// 越接近1，减速越慢，意味着滑动的时间和距离更长。
		/// 这个是全局设置，也可以通过ScrollPane.decelerationRate进行个性设置。
		/// </summary>
		public static float defaultScrollDecelerationRate = 0.967f;
		[Obsolete("UIConfig.defaultTouchScrollSpeedRatio is deprecated. Use defaultScrollDecelerationRate instead.")]
		public static float defaultTouchScrollSpeedRatio = 1;

		/// <summary>
		/// Scrollbar display mode. Recommended 'Auto' for mobile and 'Visible' for web.
		/// </summary>
		public static ScrollBarDisplayType defaultScrollBarDisplay = ScrollBarDisplayType.Default;

		/// <summary>
		/// Allow dragging anywhere in container to scroll.
		/// </summary>
		public static bool defaultScrollTouchEffect = true;

		/// <summary>
		/// The "rebound" effect in the scolling container.
		/// </summary> 
		public static bool defaultScrollBounceEffect = true;

		/// <summary>
		/// Resources url of PopupMenu.
		/// </summary>
		public static string popupMenu;

		/// <summary>
		/// Resource url of menu seperator.
		/// </summary>
		public static string popupMenu_seperator;

		/// <summary>
		/// In case of failure of loading content for GLoader, use this sign to indicate an error.
		/// </summary>
		public static string loaderErrorSign;

		/// <summary>
		/// Resource url of tooltips.
		/// </summary>
		public static string tooltipsWin;

		/// <summary>
		/// The number of visible items in ComboBox.
		/// </summary>
		public static int defaultComboBoxVisibleItemCount = 10;

		/// <summary>
		/// Pixel offsets of finger to trigger scrolling
		/// </summary>
		public static int touchScrollSensitivity = 20;

		/// <summary>
		/// Pixel offsets of finger to trigger dragging
		/// </summary>
		public static int touchDragSensitivity = 10;

		/// <summary>
		/// Pixel offsets of mouse pointer to trigger dragging.
		/// </summary>
		public static int clickDragSensitivity = 2;

		/// <summary>
		/// Allow softness on top or left side for scrollpane.
		/// </summary>
		public static bool allowSoftnessOnTopOrLeftSide = true;

		/// <summary>
		/// When click the window, brings to front automatically.
		/// </summary>
		public static bool bringWindowToFrontOnClick = true;

		/// <summary>
		/// 
		/// </summary>
		public static int inputCaretSize = 1;

		/// <summary>
		/// 
		/// </summary>
		public static Color inputHighlightColor = new Color32(255, 223, 141, 128);

		/// <summary>
		/// 
		/// </summary>
		public static float frameTimeForAsyncUIConstruction = 0.002f;

		/// <summary>
		/// if RenderTexture using in paiting mode has depth support.
		/// </summary>
		public static bool depthSupportForPaintingMode = false;

		public enum ConfigKey
		{
			DefaultFont,
			ButtonSound,
			ButtonSoundVolumeScale,
			HorizontalScrollBar,
			VerticalScrollBar,
			DefaultScrollStep,
			DefaultScrollBarDisplay,
			DefaultScrollTouchEffect,
			DefaultScrollBounceEffect,
			TouchScrollSensitivity,
			WindowModalWaiting,
			GlobalModalWaiting,
			PopupMenu,
			PopupMenu_seperator,
			LoaderErrorSign,
			TooltipsWin,
			DefaultComboBoxVisibleItemCount,
			TouchDragSensitivity,
			ClickDragSensitivity,
			ModalLayerColor,
			RenderingTextBrighterOnDesktop,
			AllowSoftnessOnTopOrLeftSide,
			InputCaretSize,
			InputHighlightColor,
			RightToLeftText,

			PleaseSelect = 100
		}

		[Serializable]
		public class ConfigValue
		{
			public bool valid;
			public string s;
			public int i;
			public float f;
			public bool b;
			public Color c;

			public void Reset()
			{
				valid = false;
				s = null;
				i = 0;
				f = 0;
				b = false;
				c = Color.black;
			}
		}

		public List<ConfigValue> Items = new List<ConfigValue>();
		public List<string> PreloadPackages = new List<string>();

		void Awake()
		{
			if (Application.isPlaying)
			{
				foreach (string packagePath in PreloadPackages)
				{
					UIPackage.AddPackage(packagePath);
				}

				Load();
			}
		}

		public void Load()
		{
			int cnt = Items.Count;
			for (int i = 0; i < cnt; i++)
			{
				ConfigValue value = Items[i];
				if (!value.valid)
					continue;

				switch ((UIConfig.ConfigKey)i)
				{
					case ConfigKey.ButtonSound:
						if (Application.isPlaying)
							UIConfig.buttonSound = UIPackage.GetItemAssetByURL(value.s) as NAudioClip;
						break;

					case ConfigKey.ButtonSoundVolumeScale:
						UIConfig.buttonSoundVolumeScale = value.f;
						break;

					case ConfigKey.ClickDragSensitivity:
						UIConfig.clickDragSensitivity = value.i;
						break;

					case ConfigKey.DefaultComboBoxVisibleItemCount:
						UIConfig.defaultComboBoxVisibleItemCount = value.i;
						break;

					case ConfigKey.DefaultFont:
						UIConfig.defaultFont = value.s;
						break;

					case ConfigKey.DefaultScrollBarDisplay:
						UIConfig.defaultScrollBarDisplay = (ScrollBarDisplayType)value.i;
						break;

					case ConfigKey.DefaultScrollBounceEffect:
						UIConfig.defaultScrollBounceEffect = value.b;
						break;

					case ConfigKey.DefaultScrollStep:
						UIConfig.defaultScrollStep = value.i;
						break;

					case ConfigKey.DefaultScrollTouchEffect:
						UIConfig.defaultScrollTouchEffect = value.b;
						break;

					case ConfigKey.GlobalModalWaiting:
						UIConfig.globalModalWaiting = value.s;
						break;

					case ConfigKey.HorizontalScrollBar:
						UIConfig.horizontalScrollBar = value.s;
						break;

					case ConfigKey.LoaderErrorSign:
						UIConfig.loaderErrorSign = value.s;
						break;

					case ConfigKey.ModalLayerColor:
						UIConfig.modalLayerColor = value.c;
						break;

					case ConfigKey.PopupMenu:
						UIConfig.popupMenu = value.s;
						break;

					case ConfigKey.PopupMenu_seperator:
						UIConfig.popupMenu_seperator = value.s;
						break;

					case ConfigKey.RenderingTextBrighterOnDesktop:
						UIConfig.renderingTextBrighterOnDesktop = value.b;
						break;

					case ConfigKey.TooltipsWin:
						UIConfig.tooltipsWin = value.s;
						break;

					case ConfigKey.TouchDragSensitivity:
						UIConfig.touchDragSensitivity = value.i;
						break;

					case ConfigKey.TouchScrollSensitivity:
						UIConfig.touchScrollSensitivity = value.i;
						break;

					case ConfigKey.VerticalScrollBar:
						UIConfig.verticalScrollBar = value.s;
						break;

					case ConfigKey.WindowModalWaiting:
						UIConfig.windowModalWaiting = value.s;
						break;

					case ConfigKey.AllowSoftnessOnTopOrLeftSide:
						UIConfig.allowSoftnessOnTopOrLeftSide = value.b;
						break;

					case ConfigKey.InputCaretSize:
						UIConfig.inputCaretSize = value.i;
						break;

					case ConfigKey.InputHighlightColor:
						UIConfig.inputHighlightColor = value.c;
						break;
				}
			}
		}

		public static void ClearResourceRefs()
		{
			UIConfig.defaultFont = "";
			UIConfig.buttonSound = null;
			UIConfig.globalModalWaiting = null;
			UIConfig.horizontalScrollBar = null;
			UIConfig.loaderErrorSign = null;
			UIConfig.popupMenu = null;
			UIConfig.popupMenu_seperator = null;
			UIConfig.tooltipsWin = null;
			UIConfig.verticalScrollBar = null;
			UIConfig.windowModalWaiting = null;
		}

		public void ApplyModifiedProperties()
		{
			//nothing yet
		}

		public delegate NAudioClip SoundLoader(string url);

		/// <summary>
		/// 
		/// </summary>
		public static SoundLoader soundLoader = null;
	}
}
