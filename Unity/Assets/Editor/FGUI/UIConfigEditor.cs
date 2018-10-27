using UnityEngine;
using UnityEditor;
using FairyGUI;

namespace FairyGUIEditor
{
	/// <summary>
	/// 
	/// </summary>
	[CustomEditor(typeof(UIConfig))]
	public class UIConfigEditor : Editor
	{
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
		string[] propertyToExclude;
#endif
		bool itemsFoldout;
		bool packagesFoldOut;
		int errorState;

		private const float kButtonWidth = 18f;

		void OnEnable()
		{
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
			propertyToExclude = new string[] { "m_Script", "Items", "PreloadPackages" };
#endif
			itemsFoldout = EditorPrefs.GetBool("itemsFoldOut");
			packagesFoldOut = EditorPrefs.GetBool("packagesFoldOut");
			errorState = 0;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

#if (UNITY_5 || UNITY_5_3_OR_NEWER)
			DrawPropertiesExcluding(serializedObject, propertyToExclude);
#endif

			UIConfig config = (UIConfig)target;

			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();
			itemsFoldout = EditorGUILayout.Foldout(itemsFoldout, "Config Items");
			if (EditorGUI.EndChangeCheck())
				EditorPrefs.SetBool("itemsFoldOut", itemsFoldout);
			EditorGUILayout.EndHorizontal();

			if (itemsFoldout)
			{
				Undo.RecordObject(config, "Items");

				int len = config.Items.Count;

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Add");
				UIConfig.ConfigKey selectedKey = (UIConfig.ConfigKey)EditorGUILayout.EnumPopup((System.Enum)UIConfig.ConfigKey.PleaseSelect);

				if (selectedKey != UIConfig.ConfigKey.PleaseSelect)
				{
					int index = (int)selectedKey;

					if (index > len - 1)
					{
						for (int i = len; i < index; i++)
							config.Items.Add(new UIConfig.ConfigValue());

						UIConfig.ConfigValue value = new UIConfig.ConfigValue();
						value.valid = true;
						InitDefaultValue(selectedKey, value);
						config.Items.Add(value);
					}
					else
					{
						UIConfig.ConfigValue value = config.Items[index];
						if (value == null)
						{
							value = new UIConfig.ConfigValue();
							value.valid = true;
							InitDefaultValue(selectedKey, value);
							config.Items[index] = value;
						}
						else if (!value.valid)
						{
							value.valid = true;
							InitDefaultValue(selectedKey, value);
						}
					}
				}
				EditorGUILayout.EndHorizontal();

				for (int i = 0; i < len; i++)
				{
					UIConfig.ConfigValue value = config.Items[i];
					if (value == null || !value.valid)
						continue;

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(((UIConfig.ConfigKey)i).ToString());
					switch ((UIConfig.ConfigKey)i)
					{
						case UIConfig.ConfigKey.ClickDragSensitivity:
						case UIConfig.ConfigKey.DefaultComboBoxVisibleItemCount:
						case UIConfig.ConfigKey.DefaultScrollStep:
						case UIConfig.ConfigKey.TouchDragSensitivity:
						case UIConfig.ConfigKey.TouchScrollSensitivity:
						case UIConfig.ConfigKey.InputCaretSize:
							value.i = EditorGUILayout.IntField(value.i);
							break;

						case UIConfig.ConfigKey.ButtonSound:
						case UIConfig.ConfigKey.GlobalModalWaiting:
						case UIConfig.ConfigKey.HorizontalScrollBar:
						case UIConfig.ConfigKey.LoaderErrorSign:
						case UIConfig.ConfigKey.PopupMenu:
						case UIConfig.ConfigKey.PopupMenu_seperator:
						case UIConfig.ConfigKey.TooltipsWin:
						case UIConfig.ConfigKey.VerticalScrollBar:
						case UIConfig.ConfigKey.WindowModalWaiting:
						case UIConfig.ConfigKey.DefaultFont:
							value.s = EditorGUILayout.TextField(value.s);
							break;

						case UIConfig.ConfigKey.DefaultScrollBounceEffect:
						case UIConfig.ConfigKey.DefaultScrollTouchEffect:
						case UIConfig.ConfigKey.RenderingTextBrighterOnDesktop:
						case UIConfig.ConfigKey.AllowSoftnessOnTopOrLeftSide:
						case UIConfig.ConfigKey.RightToLeftText:
							value.b = EditorGUILayout.Toggle(value.b);
							break;

						case UIConfig.ConfigKey.ButtonSoundVolumeScale:
							value.f = EditorGUILayout.Slider(value.f, 0, 1);
							break;

						case UIConfig.ConfigKey.ModalLayerColor:
						case UIConfig.ConfigKey.InputHighlightColor:
							value.c = EditorGUILayout.ColorField(value.c);
							break;
					}
					if (GUILayout.Button(new GUIContent("X", "Delete Item"), EditorStyles.miniButtonRight, GUILayout.Width(30)))
						config.Items[i].Reset();
					EditorGUILayout.EndHorizontal();
				}
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();
			packagesFoldOut = EditorGUILayout.Foldout(packagesFoldOut, "Preload Packages");
			if (EditorGUI.EndChangeCheck())
				EditorPrefs.SetBool("packagesFoldOut", packagesFoldOut);
			EditorGUILayout.EndHorizontal();

			if (packagesFoldOut)
			{
				Undo.RecordObject(config, "PreloadPackages");

				EditorToolSet.LoadPackages();

				if (EditorToolSet.packagesPopupContents != null)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Add");
					int selected = EditorGUILayout.Popup(EditorToolSet.packagesPopupContents.Length - 1, EditorToolSet.packagesPopupContents);
					EditorGUILayout.EndHorizontal();

					if (selected != EditorToolSet.packagesPopupContents.Length - 1)
					{
						UIPackage pkg = UIPackage.GetPackages()[selected];
						string tmp = pkg.assetPath.ToLower();
						int pos = tmp.LastIndexOf("resources/");
						if (pos != -1)
						{
							string packagePath = pkg.assetPath.Substring(pos + 10);
							if (config.PreloadPackages.IndexOf(packagePath) == -1)
								config.PreloadPackages.Add(packagePath);

							errorState = 0;
						}
						else
						{
							errorState = 10;
						}
					}
				}

				if (errorState > 0)
				{
					errorState--;
					EditorGUILayout.HelpBox("Package is not in resources folder.", MessageType.Warning);
				}

				int cnt = config.PreloadPackages.Count;
				int pi = 0;
				while (pi < cnt)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("" + pi + ".");
					config.PreloadPackages[pi] = EditorGUILayout.TextField(config.PreloadPackages[pi]);
					if (GUILayout.Button(new GUIContent("X", "Delete Item"), EditorStyles.miniButtonRight, GUILayout.Width(30)))
					{
						config.PreloadPackages.RemoveAt(pi);
						cnt--;
					}
					else
						pi++;
					EditorGUILayout.EndHorizontal();
				}
			}
			else
				errorState = 0;

			if (serializedObject.ApplyModifiedProperties())
				(target as UIConfig).ApplyModifiedProperties();
		}

		void InitDefaultValue(UIConfig.ConfigKey key, UIConfig.ConfigValue value)
		{
			switch ((UIConfig.ConfigKey)key)
			{
				case UIConfig.ConfigKey.ButtonSoundVolumeScale:
					value.f = 1;
					break;

				case UIConfig.ConfigKey.ClickDragSensitivity:
					value.i = 2;
					break;

				case UIConfig.ConfigKey.DefaultComboBoxVisibleItemCount:
					value.i = 10;
					break;

				case UIConfig.ConfigKey.DefaultScrollBarDisplay:
					value.i = (int)ScrollBarDisplayType.Default;
					break;

				case UIConfig.ConfigKey.DefaultScrollBounceEffect:
				case UIConfig.ConfigKey.DefaultScrollTouchEffect:
					value.b = true;
					break;

				case UIConfig.ConfigKey.DefaultScrollStep:
					value.i = 25;
					break;

				case UIConfig.ConfigKey.ModalLayerColor:
					value.c = new Color(0f, 0f, 0f, 0.4f);
					break;

				case UIConfig.ConfigKey.RenderingTextBrighterOnDesktop:
					value.b = true;
					break;

				case UIConfig.ConfigKey.TouchDragSensitivity:
					value.i = 10;
					break;

				case UIConfig.ConfigKey.TouchScrollSensitivity:
					value.i = 20;
					break;

				case UIConfig.ConfigKey.InputCaretSize:
					value.i = 1;
					break;

				case UIConfig.ConfigKey.InputHighlightColor:
					value.c = new Color32(255, 223, 141, 128);
					break;

				case UIConfig.ConfigKey.RightToLeftText:
					value.b = false;
					break;
			}
		}
	}
}
